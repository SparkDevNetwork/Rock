// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel.Web;
using System.Web;
using System.Web.SessionState;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class FileUploader : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="WebFaultException">Must be logged in</exception>
        public virtual void ProcessRequest( HttpContext context )
        {
            if ( !context.User.Identity.IsAuthenticated )
            {
                // If not, see if there's a valid token
                string authToken = context.Request.Headers["Authorization-Token"];
                if ( string.IsNullOrWhiteSpace( authToken ) )
                {
                    authToken = context.Request.Params["apikey"];
                }

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                    var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                    if ( userLogin != null )
                    {
                        var identity = new GenericIdentity( userLogin.UserName );
                        var principal = new GenericPrincipal( identity, null );
                        context.User = principal;
                    }
                }
            }

            var currentUser = UserLoginService.GetCurrentUser();
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            if ( !context.User.Identity.IsAuthenticated )
            {
                throw new Rock.Web.FileUploadException( "Must be logged in", System.Net.HttpStatusCode.Forbidden );
            }

            try
            {
                HttpFileCollection hfc = context.Request.Files;
                HttpPostedFile uploadedFile = hfc.AllKeys.Select( fk => hfc[fk] ).FirstOrDefault();

                // No file or no data?  No good.
                if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
                {
                    throw new Rock.Web.FileUploadException( "No File Specified", System.Net.HttpStatusCode.BadRequest );
                }

                // Check to see if this is a BinaryFileType/BinaryFile or just a plain content file
                bool isBinaryFile = context.Request.QueryString["isBinaryFile"].AsBoolean();

                if ( isBinaryFile )
                {
                    ProcessBinaryFile( context, uploadedFile );
                }
                else
                {
                    ProcessContentFile( context, uploadedFile );
                }
            }
            catch ( Rock.Web.FileUploadException fex )
            {
                ExceptionLogService.LogException( fex, context );
                context.Response.StatusCode = (int)fex.StatusCode;
                context.Response.Write( "error: " + fex.Detail );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                context.Response.Write( "error: " + ex.Message );
            }
        }

        /// <summary>
        /// Processes the content file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessContentFile( HttpContext context, HttpPostedFile uploadedFile )
        {
            // validate file type (child FileUploader classes, like ImageUploader, can do additional validation);
            this.ValidateFileType( context, uploadedFile );

            // get folderPath and construct filePath
            string relativeFolderPath = context.Request.Form["folderPath"] ?? string.Empty;
            string relativeFilePath = Path.Combine( relativeFolderPath, Path.GetFileName( uploadedFile.FileName ) );
            string rootFolderParam = context.Request.QueryString["rootFolder"];

            string rootFolder = string.Empty;

            if ( !string.IsNullOrWhiteSpace( rootFolderParam ) )
            {
                // if a rootFolder was specified in the URL, decrypt it (it is encrypted to help prevent direct access to filesystem)
                rootFolder = Rock.Security.Encryption.DecryptString( rootFolderParam );
            }

            if ( string.IsNullOrWhiteSpace( rootFolder ) )
            {
                // set to default rootFolder if not specified in the params
                rootFolder = "~/Content";
            }

            string physicalRootFolder = context.Request.MapPath( rootFolder );
            string physicalContentFolderName = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( new char[] { '/', '\\' } ) );
            string physicalFilePath = Path.Combine( physicalContentFolderName, uploadedFile.FileName );
            var fileContent = GetFileContentStream( context, uploadedFile );

            // store the content file in the specified physical content folder
            if ( !Directory.Exists( physicalContentFolderName ) )
            {
                Directory.CreateDirectory( physicalContentFolderName );
            }

            if ( File.Exists( physicalFilePath ) )
            {
                File.Delete( physicalFilePath );
            }

            using ( var writeStream = File.OpenWrite( physicalFilePath ) )
            {
                if ( fileContent.CanSeek )
                {
                    fileContent.Seek( 0, SeekOrigin.Begin );
                }
                fileContent.CopyTo( writeStream );
            }

            var response = new
            {
                Id = string.Empty,
                FileName = relativeFilePath
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Processes the binary file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessBinaryFile( HttpContext context, HttpPostedFile uploadedFile )
        {
            // get BinaryFileType info
            Guid fileTypeGuid = context.Request.QueryString["fileTypeGuid"].AsGuid();

            RockContext rockContext = new RockContext();
            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid );

            if ( binaryFileType == null )
            {
                throw new Rock.Web.FileUploadException( "Binary file type must be specified", System.Net.HttpStatusCode.Forbidden );
            }
            else
            {
                var currentUser = UserLoginService.GetCurrentUser();
                Person currentPerson = currentUser != null ? currentUser.Person : null;

                if ( !binaryFileType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    throw new Rock.Web.FileUploadException( "Not authorized to upload this type of file", System.Net.HttpStatusCode.Forbidden );
                }
            }

            // always create a new BinaryFile record of IsTemporary when a file is uploaded
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = new BinaryFile();
            binaryFileService.Add( binaryFile );

            // assume file is temporary unless specified otherwise so that files that don't end up getting used will get cleaned up
            binaryFile.IsTemporary = context.Request.QueryString["IsTemporary"].AsBooleanOrNull() ?? true;
            binaryFile.BinaryFileTypeId = binaryFileType.Id;
            binaryFile.MimeType = uploadedFile.ContentType;
            binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
            binaryFile.ContentStream = GetFileContentStream( context, uploadedFile );
            rockContext.SaveChanges();

            var response = new
            {
                Id = binaryFile.Id,
                FileName = binaryFile.FileName
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Gets the file bytes
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public virtual Stream GetFileContentStream( HttpContext context, HttpPostedFile uploadedFile )
        {
            // NOTE: GetFileBytes can get overridden by a child class (ImageUploader.ashx.cs for example)
            return uploadedFile.InputStream;
        }

        /// <summary>
        /// Validates the type of the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <exception cref="WebFaultException{System.String}">Filetype not allowed</exception>
        public virtual void ValidateFileType( HttpContext context, HttpPostedFile uploadedFile )
        {
            // validate file type (applies to all uploaded files)
            var globalAttributesCache = GlobalAttributesCache.Read();
            IEnumerable<string> contentFileTypeBlackList = ( globalAttributesCache.GetValue( "ContentFiletypeBlacklist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );

            // clean up list
            contentFileTypeBlackList = contentFileTypeBlackList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            string fileExtension = Path.GetExtension( uploadedFile.FileName ).ToLower().TrimStart( new char[] { '.' } );
            if ( contentFileTypeBlackList.Contains( fileExtension ) )
            {
                throw new Rock.Web.FileUploadException( "Filetype not allowed", System.Net.HttpStatusCode.NotAcceptable );
            }
        }
    }
}