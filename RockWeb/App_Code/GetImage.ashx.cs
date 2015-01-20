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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using ImageResizer;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage
    /// </summary>
    public class GetImage : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            try
            {
                context.Response.Clear();

                // Check to see if this is a BinaryFileType/BinaryFile or just a plain content file (if isBinaryFile not specified, assume it is a BinaryFile)
                bool isBinaryFile = ( context.Request.QueryString["isBinaryFile"] ?? "T" ).AsBoolean();

                if ( isBinaryFile )
                {
                    ProcessBinaryFileRequest( context );
                }
                else
                {
                    ProcessContentFileRequest( context );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
            }
        }

        /// <summary>
        /// Processes the content file request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.Exception">fileName must be specified</exception>
        private void ProcessContentFileRequest( HttpContext context )
        {
            string relativeFilePath = context.Request.QueryString["fileName"];
            string rootFolderParam = context.Request.QueryString["rootFolder"];

            if ( string.IsNullOrWhiteSpace( relativeFilePath ) )
            {
                throw new Exception( "fileName must be specified" );
            }

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
            string physicalContentFileName = Path.Combine( physicalRootFolder, relativeFilePath.TrimStart( new char[] { '/', '\\' } ) );
            using ( Stream fileContents = File.OpenRead( physicalContentFileName ) )
            {
                if ( fileContents != null )
                {
                    string mimeType = System.Web.MimeMapping.GetMimeMapping( physicalContentFileName );
                    context.Response.AddHeader( "content-disposition", string.Format( "inline;filename={0}", Path.GetFileName( physicalContentFileName ) ) );
                    context.Response.ContentType = mimeType;
                    
                    // If more than 1 query string param is passed in and it isn't an SVG file, do a Resize, assume resize is needed
                    if ( (context.Request.QueryString.Count > 1) && ( mimeType != "image/svg+xml" ))
                    {
                        using ( var resizedStream = GetResized( context.Request.QueryString, fileContents ) )
                        {
                            resizedStream.CopyTo( context.Response.OutputStream );
                        }
                    }
                    else
                    {
                        fileContents.CopyTo( context.Response.OutputStream );
                    }
                    
                    context.Response.Flush();
                    context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        /// <summary>
        /// Processes the binary file request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ProcessBinaryFileRequest( HttpContext context )
        {
            int fileId = context.Request.QueryString["id"].AsInteger();
            Guid fileGuid = context.Request.QueryString["guid"].AsGuid();

            if ( fileId == 0 && fileGuid.Equals( Guid.Empty ) )
            {
                SendNotFound( context );
            }

            var rockContext = new RockContext();

            var binaryFileQuery = new BinaryFileService( rockContext ).Queryable();
            if ( fileGuid != Guid.Empty )
            {
                binaryFileQuery = binaryFileQuery.Where( a => a.Guid == fileGuid );
            }
            else
            {
                binaryFileQuery = binaryFileQuery.Where( a => a.Id == fileId );
            }

            //// get just the binaryFileMetaData (not the file content) just in case we can get the filecontent faster from the cache
            //// a null ModifiedDateTime shouldn't happen, but just in case, set it to DateTime.MaxValue so we error on the side of not getting it from the cache
            var binaryFileMetaData = binaryFileQuery.Select( a => new
            {
                BinaryFileType_AllowCaching = a.BinaryFileType.AllowCaching,
                BinaryFileType_RequiresViewSecurity = a.BinaryFileType.RequiresViewSecurity,
                ModifiedDateTime = a.ModifiedDateTime ?? DateTime.MaxValue,
                a.MimeType,
                a.FileName
            } ).FirstOrDefault();

            if ( binaryFileMetaData == null )
            {
                SendNotFound( context );
                return;
            }

            //// if the binaryFile's BinaryFileType requires view security, check security
            //// note: we put a RequiresViewSecurity flag on BinaryFileType because checking security for every image would be slow (~40ms+ per image request)
            if ( binaryFileMetaData.BinaryFileType_RequiresViewSecurity )
            {
                var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
                Person currentPerson = currentUser != null ? currentUser.Person : null;
                BinaryFile binaryFileAuth = new BinaryFileService( rockContext ).Queryable( "BinaryFileType" ).First( a => a.Guid == fileGuid || a.Id == fileId );
                if ( !binaryFileAuth.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    SendNotAuthorized( context );
                    return;
                }
            }

            
            Stream fileContent = null;
            try
            {
                // Is it cached
                string cacheName = UrlQueryToCachedFileName( context.Request.QueryString, binaryFileMetaData.MimeType );
                string physCachedFilePath = context.Request.MapPath( string.Format( "~/App_Data/Cache/{0}", cacheName ) );
                if ( binaryFileMetaData.BinaryFileType_AllowCaching && File.Exists( physCachedFilePath ) )
                {
                    //// Compare the File's Creation DateTime (which comes from the OS's clock), adjust it for the Rock OrgTimeZone, then compare to BinaryFile's ModifiedDateTime (which is already in OrgTimeZone).
                    //// If the BinaryFile record in the database is less recent than the last time this was cached, it is safe to use the Cached version.
                    //// NOTE: A BinaryFile record is typically just added and never modified (a modify is just creating a new BinaryFile record and deleting the old one), so the cached version will probably always be the correct choice.
                    DateTime cachedFileDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( File.GetCreationTime( physCachedFilePath ) );
                    if ( binaryFileMetaData.ModifiedDateTime < cachedFileDateTime )
                    {
                        // NOTE: the cached file has already been resized (the size is part of the cached file's filename), so we don't need to resize it again
                        fileContent = FetchFromCache( physCachedFilePath );
                    }
                }

                if ( fileContent == null )
                {
                    // If we didn't get it from the cache, get it from the binaryFileService
                    BinaryFile binaryFile = GetFromBinaryFileService( context, fileId, fileGuid );

                    if ( binaryFile != null )
                    {
                        fileContent = binaryFile.ContentStream;
                    }

                    // If we got the image from the binaryFileService, it might need to be resized and cached
                    if ( fileContent != null )
                    {
                        // If more than 1 query string param is passed in, assume resize is needed
                        if ( context.Request.QueryString.Count > 1 )
                        {
                            // if it isn't an SVG file, do a Resize
                            if ( binaryFile.MimeType != "image/svg+xml" )
                            {
                                fileContent = GetResized( context.Request.QueryString, fileContent );
                            }
                        }

                        if ( binaryFileMetaData.BinaryFileType_AllowCaching )
                        {
                            Cache( fileContent, physCachedFilePath );
                        }
                    }
                }

                if ( fileContent == null )
                {
                    // if we couldn't get the file from the binaryFileServie or the cache, respond with NotFound
                    SendNotFound( context );
                    return;
                }

                // respond with File
                if ( binaryFileMetaData.BinaryFileType_AllowCaching )
                {
                    // if binaryFileType is set to allowcaching, also tell the browser to cache it for 365 days
                    context.Response.Cache.SetLastModified( binaryFileMetaData.ModifiedDateTime );
                    context.Response.Cache.SetMaxAge( new TimeSpan( 365, 0, 0, 0 ) );
                }

                context.Response.ContentType = binaryFileMetaData.MimeType;

                using ( var responseStream = fileContent )
                {
                    context.Response.AddHeader( "content-disposition", "inline;filename=" + binaryFileMetaData.FileName );
                    if ( responseStream.CanSeek )
                    {
                        responseStream.Seek( 0, SeekOrigin.Begin );
                    }
                    responseStream.CopyTo( context.Response.OutputStream );
                    context.Response.Flush();
                }
            }
            finally
            {
                if (fileContent != null)
                {
                    fileContent.Dispose();
                }
            }
        }

        /// <summary>
        /// URLs the name of the query to cached file.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <returns></returns>
        private string UrlQueryToCachedFileName( NameValueCollection queryString, string mimeType )
        {
            NameValueCollection cleanedQueryString = new NameValueCollection( queryString );

            // remove params that don't have impact on uniqueness
            cleanedQueryString.Remove( "isBinaryFile" );
            cleanedQueryString.Remove( "rootFolder" );
            cleanedQueryString.Remove( "fileName" );

            string fileName = string.Empty;
            foreach ( var key in cleanedQueryString.Keys )
            {
                fileName += string.Format( "{0}_{1}", key, cleanedQueryString[key as string] ).RemoveSpecialCharacters() + "-";
            }

            fileName = fileName.TrimEnd( new char[] { '-' } );

            if ( !string.IsNullOrWhiteSpace( mimeType ) )
            {
                string[] mimeParts = mimeType.Split( new char[] { '/' } );
                if ( mimeParts.Length >= 2 )
                {
                    fileName += "." + mimeParts[1].Replace( "jpeg", "jpg" ).Replace( "svg+xml", "svg" );
                }
            }

            return fileName;
        }

        /// <summary>
        /// Gets from binary file service.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="fileGuid">The file unique identifier.</param>
        /// <returns></returns>
        private BinaryFile GetFromBinaryFileService( HttpContext context, int fileId, Guid fileGuid )
        {
            BinaryFile binaryFile = null;
            System.Threading.ManualResetEvent completedEvent = new ManualResetEvent( false );

            var rockContext = new RockContext();

            // use the binaryFileService.BeginGet/EndGet which is a little faster than the regular get
            AsyncCallback cb = ( IAsyncResult asyncResult ) =>
            {
                // restore the context from the asyncResult.AsyncState 
                HttpContext asyncContext = (HttpContext)asyncResult.AsyncState;
                binaryFile = new BinaryFileService( rockContext ).EndGet( asyncResult, context );
                completedEvent.Set();
            };

            IAsyncResult beginGetResult;

            if ( fileGuid != Guid.Empty )
            {
                beginGetResult = new BinaryFileService( rockContext ).BeginGet( cb, context, fileGuid );
            }
            else
            {
                beginGetResult = new BinaryFileService( rockContext ).BeginGet( cb, context, fileId );
            }

            // wait up to 5 minutes for the response
            completedEvent.WaitOne( 300000 );
            return binaryFile;
        }

        /// <summary>
        /// Resizes and returns the resized content
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        private Stream GetResized( NameValueCollection queryString, Stream fileContent )
        {
            ResizeSettings settings = new ResizeSettings( queryString );
            MemoryStream resizedStream = new MemoryStream();
            ImageBuilder.Current.Build( fileContent, resizedStream, settings );
            return resizedStream;
        }

        /// <summary>
        /// Caches the specified file content.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="physFilePath">The physical file path.</param>
        private void Cache( Stream fileContent, string physFilePath )
        {
            try
            {
                // ensure that the Cache folder exists
                string cacheFolderPath = Path.GetDirectoryName( physFilePath );
                if ( !Directory.Exists( cacheFolderPath ) )
                {
                    Directory.CreateDirectory( cacheFolderPath );
                }

                using ( var writeStream = File.OpenWrite( physFilePath ) )
                {
                    if ( fileContent.CanSeek )
                    {
                        fileContent.Seek( 0, SeekOrigin.Begin );
                    }
                    fileContent.CopyTo( writeStream );
                }
            }
            catch
            {
                // if it fails, do nothing. They might have a hosting provider that doesn't allow writing to disk
            }
        }

        /// <summary>
        /// Fetches from cache.
        /// </summary>
        /// <param name="physFilePath">The phys file path.</param>
        /// <returns></returns>
        private Stream FetchFromCache( string physFilePath )
        {
            try
            {
                return File.Open( physFilePath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
            catch
            {
                // if it fails, return null, which will result in fetching it from the database instead
                return null;
            }
        }

        /// <summary>
        /// Sends 404 status.
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotFound( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.NotFound.ConvertToInt();
            context.Response.StatusDescription = "The requested image could not be found.";
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Sends a 403 (forbidden)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.Forbidden.ConvertToInt();
            context.Response.StatusDescription = "Not authorized to view image";
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}