// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class FileUploader : IHttpHandler, IRequiresSessionState
    {
        #region Constants

        private const string DEFAULT_ROOT_PATH = "~/Content";

        /// <summary>
        /// Parameter Key Constants.
        /// </summary>
        protected class ParameterKey
        {
            public const string ApiKey = "apikey";
            public const string IsBinaryFile = "isBinaryFile";
            public const string IsAssetStorageProviderAsset = "IsAssetStorageProviderAsset";
            public const string StorageId = "StorageId";
            public const string Key = "Key";
            public const string FolderPath = "folderPath";
            public const string RootFolder = "rootFolder";
            public const string FileTypeGuid = "fileTypeGuid";
            public const string IsTemporary = "IsTemporary";
            public const string ParentEntityTypeId = "ParentEntityTypeId";
            public const string ParentEntityId = "ParentEntityId";
        }

        #endregion Constants

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
                string authToken = context.Request.Headers[Rock.Rest.HeaderTokens.AuthorizationToken];
                if ( string.IsNullOrWhiteSpace( authToken ) )
                {
                    authToken = context.Request.Params[ParameterKey.ApiKey];
                }

                if ( !string.IsNullOrWhiteSpace( authToken ) )
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

            try
            {
                HttpFileCollection hfc = context.Request.Files;
                HttpPostedFile uploadedFile = hfc.AllKeys.Select( fk => hfc[fk] ).FirstOrDefault();

                // No file or no data?  No good.
                if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
                {
                    throw new Rock.Web.FileUploadException( "No file specified.", System.Net.HttpStatusCode.BadRequest );
                }

                /*
                    7/27/2022 - SMC
                    Validate the file here before proceeding with any upload processing.

                    Reason: https://app.asana.com/0/1121505495628584/1202592611130311/f
                */

                // validate file type (child FileUploader classes, like ImageUploader, can do additional validation);
                this.ValidateFileType( context, uploadedFile );

                // Check to see if this is a BinaryFileType/BinaryFile or just a plain content file
                bool isBinaryFile = context.Request.QueryString[ParameterKey.IsBinaryFile].AsBoolean();

                if ( isBinaryFile )
                {
                    ProcessBinaryFile( context, uploadedFile, currentPerson );
                }
                else
                {
                    if ( !context.User.Identity.IsAuthenticated )
                    {
                        throw new Rock.Web.FileUploadException( "Must be logged in.", System.Net.HttpStatusCode.Forbidden );
                    }
                    else
                    {
                        if ( context.Request.Form[ParameterKey.IsAssetStorageProviderAsset].AsBoolean() )
                        {
                            ProcessAssetStorageProviderAsset( context, uploadedFile );
                        }
                        else
                        {
                            ProcessContentFile( context, uploadedFile );
                        }
                    }
                }
            }
            catch ( Rock.Web.FileUploadException fex )
            {
                ExceptionLogService.LogException( fex, context );
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = ( int ) fex.StatusCode;
                context.Response.Write( fex.Detail );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = ( int ) System.Net.HttpStatusCode.InternalServerError;
                context.Response.Write( "error: " + ex.Message );
            }
        }

        /// <summary>
        /// Processes the asset storage provider asset.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <exception cref="Rock.Web.FileUploadException">
        /// Insufficient info to upload a file of this type.
        /// or
        /// Unable to upload file
        /// </exception>
        private void ProcessAssetStorageProviderAsset( HttpContext context, HttpPostedFile uploadedFile )
        {
            int? assetStorageId = context.Request.Form[ParameterKey.StorageId].AsIntegerOrNull();
            string assetKey = context.Request.Form[ParameterKey.Key] + uploadedFile.FileName;

            if ( assetStorageId == null || assetKey.IsNullOrWhiteSpace() )
            {
                throw new Rock.Web.FileUploadException( "Insufficient info to upload a file of this type.", System.Net.HttpStatusCode.Forbidden );
            }

            var assetStorageProviderCache = AssetStorageProviderCache.Get( assetStorageId.Value );
            var component = assetStorageProviderCache.AssetStorageComponent;

            var asset = new Rock.Storage.AssetStorage.Asset();
            asset.Key = assetKey;
            asset.Type = Rock.Storage.AssetStorage.AssetType.File;
            asset.AssetStream = uploadedFile.InputStream;

            if ( component.UploadObject( assetStorageProviderCache.ToEntity(), asset ) )
            {
                context.Response.Write( new { Id = string.Empty, FileName = assetKey }.ToJson() );
            }
            else
            {
                throw new Rock.Web.FileUploadException( "Unable to upload file.", System.Net.HttpStatusCode.BadRequest );
            }
        }

        /// <summary>
        /// Processes the content file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessContentFile( HttpContext context, HttpPostedFile uploadedFile )
        {
            // NEVER TRUST THE CLIENT!!!!
            string untrustedFileName = uploadedFile.FileName;
            string untrustedFolderPath = context.Request.Form[ParameterKey.FolderPath] ?? string.Empty;
            string encryptedRootFolder = context.Request.QueryString[ParameterKey.RootFolder];

            string scrubbedFileName = ScrubFileName( untrustedFileName );
            string scrubbedFolderPath = ScrubFilePath( untrustedFolderPath );

            string trustedRootFolder = string.Empty;

            // If a rootFolder was specified in the URL, try decrypting it (It is encrypted to help prevent direct access to file system).
            if ( !string.IsNullOrWhiteSpace( encryptedRootFolder ) )
            {
                trustedRootFolder = Encryption.DecryptString( encryptedRootFolder );
            }

            // If we don't have a rootFolder, default to the ~/Content folder.
            if ( string.IsNullOrWhiteSpace( trustedRootFolder ) )
            {
                trustedRootFolder = DEFAULT_ROOT_PATH;
            }

            // Get the absolute path for our trusted root.
            string trustedPhysicalRootFolder = Path.GetFullPath( context.Request.MapPath( trustedRootFolder ) );

            // Treat rooted folder paths as relative.
            string untrustedRelativeFolderPath = "";
            if ( !string.IsNullOrWhiteSpace( scrubbedFolderPath ) )
            {
                untrustedRelativeFolderPath = scrubbedFolderPath.TrimStart( Path.GetPathRoot( scrubbedFolderPath ).ToCharArray() );
            }

            // Combine the root and folder paths to get the real physical location.
            string untrustedPhysicalFolderPath = Path.GetFullPath( Path.Combine( trustedPhysicalRootFolder, untrustedRelativeFolderPath ) );

            // Make sure the physical location is valid.
            if ( !untrustedPhysicalFolderPath.StartsWith( trustedPhysicalRootFolder ) )
            {
                // If the untrusted folder is outside our trusted root folder, something's fishy.
                throw new Rock.Web.FileUploadException( "Invalid folder path.", System.Net.HttpStatusCode.BadRequest );
            }

            // Yay! We now have a trusted physical path and a safe filename, let's put those together and upload our file.
            string trustedPhysicalFolderPath = untrustedPhysicalFolderPath;
            string physicalFilePath = Path.Combine( trustedPhysicalFolderPath, scrubbedFileName );

            // If the file already exists, bail.
            if ( File.Exists( physicalFilePath ) )
            {
                throw new Rock.Web.FileUploadException( "File already exists.", System.Net.HttpStatusCode.BadRequest );
            }

            // Make sure the physical path exists.
            if ( !Directory.Exists( trustedPhysicalFolderPath ) )
            {
                Directory.CreateDirectory( trustedPhysicalFolderPath );
            }

            // Write the file contents to a new file.
            using ( var fileContent = GetFileContentStream( context, uploadedFile ) )
            using ( var writeStream = File.OpenWrite( physicalFilePath ) )
            {
                if ( fileContent.CanSeek )
                {
                    fileContent.Seek( 0, SeekOrigin.Begin );
                }

                fileContent.CopyTo( writeStream );
            }

            // Generate the response data and write it to the HTTP response.
            var response = new
            {
                Id = string.Empty,
                FileName = Path.Combine( untrustedRelativeFolderPath, scrubbedFileName )
            };

            context.Response.Write( response.ToJson() );
        }

        /// <summary>
        /// Dictionary of deprecated or incorrect mime types and what they should be mapped to instead
        /// </summary>
        private readonly Dictionary<string, string> _mimeTypeRemap = new Dictionary<string, string>
        {
            { "text/directory", "text/vcard" },
            { "text/directory; profile=vCard", "text/vcard" },
            { "text/x-vcard", "text/vcard" }
        };

        /// <summary>
        /// Processes the binary file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        private void ProcessBinaryFile( HttpContext context, HttpPostedFile uploadedFile, Person currentPerson )
        {
            // get BinaryFileType info
            Guid fileTypeGuid = context.Request.QueryString[ParameterKey.FileTypeGuid].AsGuid();

            RockContext rockContext = new RockContext();
            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid );

            if ( binaryFileType == null )
            {
                throw new Rock.Web.FileUploadException( "Binary file type must be specified.", System.Net.HttpStatusCode.Forbidden );
            }
            
            if ( !binaryFileType.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                throw new Rock.Web.FileUploadException( "Not authorized to upload this type of file.", System.Net.HttpStatusCode.Forbidden );
            }

            if ( binaryFileType.MaxFileSizeBytes != null && uploadedFile.ContentLength > binaryFileType.MaxFileSizeBytes )
            {
                throw new Rock.Web.FileUploadException(
                    $"The maximum file size for file type \"{binaryFileType.Name}\" is {Rock.Utility.FileUtilities.FileSizeSuffixFormatter( binaryFileType.MaxFileSizeBytes.Value )}",
                    System.Net.HttpStatusCode.Forbidden );
            }

            char[] illegalCharacters = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

            if ( uploadedFile.FileName.IndexOfAny( illegalCharacters ) >= 0 || uploadedFile.FileName.EndsWith( "." ) )
            {
                throw new Rock.Web.FileUploadException( "Invalid Filename.  Please remove any special characters (" + string.Join( " ", illegalCharacters ) + ").", System.Net.HttpStatusCode.UnsupportedMediaType );
            }

            // always create a new BinaryFile record of IsTemporary when a file is uploaded
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = new BinaryFile();
            binaryFileService.Add( binaryFile );

            // assume file is temporary unless specified otherwise so that files that don't end up getting used will get cleaned up
            binaryFile.IsTemporary = context.Request.QueryString[ParameterKey.IsTemporary].AsBooleanOrNull() ?? true;
            binaryFile.ParentEntityTypeId = context.Request.QueryString[ParameterKey.ParentEntityTypeId].AsIntegerOrNull();
            binaryFile.ParentEntityId = context.Request.QueryString[ParameterKey.ParentEntityId].AsIntegerOrNull();
            binaryFile.BinaryFileTypeId = binaryFileType.Id;
            binaryFile.MimeType = uploadedFile.ContentType;
            binaryFile.FileSize = uploadedFile.ContentLength;

            /*
	            2020-02-11 BJW
	            The ReplaceSpecialCharacters extension call was added to remove characters that are outside the legal character range.
	            For example, if a file is moved from a Linux system, it might have a colon that manifests as a char with int value
	            in the thousands range (far outside typical character range. This causes unpredictable behavior with the various file
	            storage providers (GCP might handle it differently than the database storage provider). In order to add consistency
	            we simply replace any of these characters with an underscore. This includes spaces, which are normal, but are security
	            risks if they fall in certain parts of the filename.
             */
            binaryFile.FileName = ScrubFileName( uploadedFile.FileName ).ReplaceSpecialCharacters( "_" );

            if ( _mimeTypeRemap.ContainsKey( binaryFile.MimeType ) )
            {
                binaryFile.MimeType = _mimeTypeRemap[binaryFile.MimeType];
            }

            binaryFile.ContentStream = GetFileContentStream( context, uploadedFile );
            rockContext.SaveChanges();

            var response = new
            {
                Id = binaryFile.Id,
                Guid = binaryFile.Guid,
                FileName = binaryFile.FileName.UrlEncode()
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
        /// <exception cref="WebFaultException{System.String}">File type not allowed</exception>
        public virtual void ValidateFileType( HttpContext context, HttpPostedFile uploadedFile )
        {
            // validate file type (applies to all uploaded files)

            string scrubbedFileName = ScrubFileName( uploadedFile.FileName );

            if ( scrubbedFileName.IsNullOrWhiteSpace() || scrubbedFileName.EndsWith( "." ) )
            {
                throw new Rock.Web.FileUploadException( "Invalid or illegal file name.", System.Net.HttpStatusCode.UnsupportedMediaType );
            }

            // Get file extension and then trim any trailing spaces (to catch any nefarious stuff).
            string fileExtension = Path.GetExtension( scrubbedFileName ).ToLower().TrimStart( new char[] { '.' } ).Trim();

            var globalAttributesCache = GlobalAttributesCache.Get();

            // Check file extension against blacklist.
            IEnumerable<string> contentFileTypeBlackList = ( globalAttributesCache.GetValue( "ContentFiletypeBlacklist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
            contentFileTypeBlackList = contentFileTypeBlackList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );
            if ( contentFileTypeBlackList.Contains( fileExtension ) || scrubbedFileName.EndsWith( "." ) )
            {
                throw new Rock.Web.FileUploadException( "File not allowed.", System.Net.HttpStatusCode.NotAcceptable );
            }

            // If a whitelist exists, ensure that the file extension is in it.
            IEnumerable<string> contentFileTypeWhiteList = ( globalAttributesCache.GetValue( "ContentFiletypeWhitelist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
            contentFileTypeWhiteList = contentFileTypeWhiteList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );
            if ( contentFileTypeWhiteList.Any() && !contentFileTypeWhiteList.Contains( fileExtension ) )
            {
                throw new Rock.Web.FileUploadException( "File not allowed.", System.Net.HttpStatusCode.NotAcceptable );
            }

            // Check file name for any illegal characters.
            char[] illegalCharacters = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            if ( scrubbedFileName.IndexOfAny( illegalCharacters ) >= 0 )
            {
                throw new Rock.Web.FileUploadException( "Invalid file name.  Please remove any special characters (" + string.Join( " ", illegalCharacters ) + ").", System.Net.HttpStatusCode.UnsupportedMediaType );
            }
        }

        /// <summary>
        /// Scrubs a filename to make sure it doesn't have any directories, invalid characters, or spaces.
        /// </summary>
        /// <param name="untrustedFileName">The filename.</param>
        /// <returns>A scrubbed filename.</returns>
        public string ScrubFileName( string untrustedFileName )
        {
            // Scrub invalid path characters.
            string scrubbedFileName = ScrubFilePath( untrustedFileName );

            // Get the file name (without path).
            scrubbedFileName = Path.GetFileName( scrubbedFileName );

            /*
	            2020-03-25 JME
	            While C# has a listing of invalid file characters (used below), we added a few more of our own to help
	            with dealing with linking easily to files that have been uploaded.

	            Specific Use Case: Theme Editor was having issues when using uploaded files from the Image Upload control
             */

            // Scrub characters identified by .NET as invalid for file names.
            scrubbedFileName = Regex.Replace( scrubbedFileName, "[" + Regex.Escape( string.Concat( Path.GetInvalidFileNameChars() ) ) + "]", string.Empty, RegexOptions.CultureInvariant );

            /*
	            3/17/2020 - JME 
	            And remove spaces, I did not add the removal of spaces to the scrub as the scrub logic
                has existed for a while and is used in other places that may not want that. We can move
                this to the scrub should we desire in the future.

                Reason: The theme editor needs files with no spaces to be used in CSS

                7/27/2022 - SMC
                Moved to ScrubFileName() for consistent naming of all uploaded files.
            */
            scrubbedFileName = scrubbedFileName.Replace(" ", "_");

             // Remove Illegal Filename Characters
            char[] illegalChars = { '#', '(', ')', '&', '%' };
            scrubbedFileName = string.Concat( scrubbedFileName.Split( illegalChars ) );

            return scrubbedFileName;
        }

        /// <summary>
        /// Scrubs a file path to make sure it doesn't have any invalid characters
        /// </summary>
        /// <param name="untrustedFilePath">The file path.</param>
        /// <returns>A scrubbed file path.</returns>
        public string ScrubFilePath( string untrustedFilePath )
        {
            // Scrub characters identified by .NET as invalid.
            return Regex.Replace( untrustedFilePath.Trim(), "[" + Regex.Escape( string.Concat( Path.GetInvalidPathChars() ) ) + "]", string.Empty, RegexOptions.CultureInvariant );
        }
    }
}