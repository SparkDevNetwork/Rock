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
                if ( !context.Response.IsClientConnected )
                {
                    // if client disconnected, ignore
                }
                else
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Processes the content file request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ProcessContentFileRequest( HttpContext context )
        {

            // Don't trust query strings
            string untrustedFilePath = context.Request.QueryString["fileName"];
            string encryptedRootFolder = context.Request.QueryString["rootFolder"];

            // Make sure a filename was provided
            if ( string.IsNullOrWhiteSpace( untrustedFilePath ) )
            {
                SendBadRequest( context, "fileName must be specified" );
                return;
            }

            string trustedRootFolder = string.Empty;

            // If a rootFolder was specified in the URL
            if ( !string.IsNullOrWhiteSpace( encryptedRootFolder ) )
            {
                // Decrypt it (It is encrypted to help prevent direct access to filesystem).
                trustedRootFolder = Encryption.DecryptString( encryptedRootFolder );
            }

            // If we don't have a rootFolder, default to the ~/Content folder.
            if ( string.IsNullOrWhiteSpace( trustedRootFolder ) )
            {
                trustedRootFolder = "~/Content";
            }

            // Get the absolute path for our trusted root.
            string trustedPhysicalRootFolder = Path.GetFullPath( context.Request.MapPath( trustedRootFolder ) );

            // Treat rooted file paths as relative
            string untrustedRelativeFilePath = untrustedFilePath.TrimStart( Path.GetPathRoot( untrustedFilePath ).ToCharArray() );

            // Get the absolute path for our untrusted file.
            string untrustedPhysicalFilePath = Path.GetFullPath( Path.Combine( trustedPhysicalRootFolder, untrustedRelativeFilePath ) );

            // Make sure the untrusted file is inside our trusted root folder.
            string trustedPhysicalFilePath = string.Empty;
            if ( untrustedPhysicalFilePath.StartsWith( trustedPhysicalRootFolder ) )
            {
                // If so, then we can trust it.
                trustedPhysicalFilePath = untrustedPhysicalFilePath;
            }
            else
            {
                // Otherwise, say the file doesn't exist.
                SendNotFound( context );
                return;
            }

            // Try to open a file stream
            try
            {
                using ( Stream fileContents = File.OpenRead( trustedPhysicalFilePath ) )
                {
                    if ( fileContents != null )
                    {
                        string mimeType = MimeMapping.GetMimeMapping( trustedPhysicalFilePath );
                        context.Response.AddHeader( "content-disposition", string.Format( "inline;filename={0}", Path.GetFileName( trustedPhysicalFilePath ) ) );
                        context.Response.ContentType = mimeType;

                        // If extra query string params are passed in and it isn't an SVG file, assume resize is needed
                        if ( ( context.Request.QueryString.Count > GetQueryCount( context ) ) && ( mimeType != "image/svg+xml" ) )
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
                    else
                    {
                        SendNotFound( context );
                    }
                }
            }
            catch ( Exception ex )
            {
                if ( ex is ArgumentException || ex is ArgumentNullException || ex is NotSupportedException )
                {
                    SendBadRequest( context, "fileName is invalid." );
                }
                else if ( ex is DirectoryNotFoundException || ex is FileNotFoundException )
                {
                    SendNotFound( context );
                }
                else if ( ex is UnauthorizedAccessException )
                {
                    SendNotAuthorized( context );
                }
                else
                {
                    throw ex;
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

            if ( fileId == 0 && fileGuid == Guid.Empty )
            {
                SendBadRequest( context, "File id key must be a guid or an int." );
                return;
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
                a.Id,
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
                BinaryFile binaryFileAuth = new BinaryFileService( rockContext ).Queryable( "BinaryFileType" ).First( a => a.Id == binaryFileMetaData.Id );
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
                    //// Compare the File's LastWrite DateTime (which comes from the OS's clock), adjust it for the Rock OrgTimeZone, then compare to BinaryFile's ModifiedDateTime (which is already in OrgTimeZone).
                    //// If the BinaryFile record in the database is less recent than the last time this was cached, it is safe to use the Cached version.
                    //// NOTE: A BinaryFile record is typically just added and never modified (a modify is just creating a new BinaryFile record and deleting the old one), so the cached version will probably always be the correct choice.
                    DateTime cachedFileDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( File.GetLastWriteTime( physCachedFilePath ) );
                    if ( binaryFileMetaData.ModifiedDateTime < cachedFileDateTime )
                    {
                        // NOTE: the cached file has already been resized (the size is part of the cached file's filename), so we don't need to resize it again
                        fileContent = FetchFromCache( physCachedFilePath );
                    }
                }

                if ( fileContent == null )
                {
                    // If we didn't get it from the cache, get it from the binaryFileService
                    BinaryFile binaryFile = GetFromBinaryFileService( context, binaryFileMetaData.Id );

                    if ( binaryFile != null )
                    {
                        fileContent = binaryFile.ContentStream;
                    }

                    // If we got the image from the binaryFileService, it might need to be resized and cached
                    if ( fileContent != null )
                    {
                        // If more than 1 query string param is passed in, or the mime type is TIFF, assume resize is needed
                        // Note: we force "image/tiff" to get resized so that it gets converted into a jpg (browsers don't like tiffs)
                        if ( context.Request.QueryString.Count > GetQueryCount( context ) || binaryFile.MimeType == "image/tiff" )
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

                            // Reset stream
                            if ( fileContent.CanSeek )
                            {
                                fileContent.Seek( 0, SeekOrigin.Begin );
                            }
                            else
                            {
                                fileContent = FetchFromCache( physCachedFilePath );
                            }
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

                // set the mime-type to that of the binary file
                context.Response.ContentType = binaryFileMetaData.MimeType != "image/tiff" ? binaryFileMetaData.MimeType : "image/jpg";

                // check that the format of the image wasn't changed by a format query parm if so adjust the mime-type to reflect the conversion
                if ( context.Request["format"].IsNotNullOrWhiteSpace() )
                {
                    switch ( context.Request["format"] )
                    {
                        case "png":
                            {
                                context.Response.ContentType = "image/png";
                                break;
                            }
                        case "gif":
                            {
                                context.Response.ContentType = "image/gif";
                                break;
                            }
                        case "jpg":
                            {
                                context.Response.ContentType = "image/jpeg";
                                break;
                            }
                    }
                }

                using ( var responseStream = fileContent )
                {
                    context.Response.AddHeader( "content-disposition", "inline;filename=" + binaryFileMetaData.FileName.MakeValidFileName().UrlEncode() );
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
                if ( fileContent != null )
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
                    fileName += "." + mimeParts[1].Replace( "jpeg", "jpg" ).Replace( "svg+xml", "svg" ).Replace( "tiff", "jpg" );
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
        private BinaryFile GetFromBinaryFileService( HttpContext context, int fileId )
        {
            BinaryFile binaryFile = null;
            System.Threading.ManualResetEvent completedEvent = new ManualResetEvent( false );

            var rockContext = new RockContext();

            // use the binaryFileService.BeginGet/EndGet which is a little faster than the regular get
            AsyncCallback cb = ( IAsyncResult asyncResult ) =>
            {
                // restore the context from the asyncResult.AsyncState 
                HttpContext asyncContext = ( HttpContext ) asyncResult.AsyncState;
                binaryFile = new BinaryFileService( rockContext ).EndGet( asyncResult, context );
                completedEvent.Set();
            };

            IAsyncResult beginGetResult = new BinaryFileService( rockContext ).BeginGet( cb, context, fileId );

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
            try
            {
                ResizeSettings settings = new ResizeSettings( queryString );

                if ( settings["mode"] == null || settings["mode"] == "clip" )
                {
                    settings.Add( "mode", "max" );

                    if ( !string.IsNullOrEmpty( settings["width"] ) && !string.IsNullOrEmpty( settings["height"] ) )
                    {
                        if ( settings["width"].AsInteger() > settings["height"].AsInteger() )
                        {
                            settings.Remove( "height" );
                        }
                        else
                        {
                            settings.Remove( "width" );
                        }
                    }
                }

                settings.Add( "autorotate.default", "true" );

                MemoryStream resizedStream = new MemoryStream();

                ImageBuilder.Current.Build( fileContent, resizedStream, settings, false );
                if ( resizedStream.Length > 0 )
                {
                    return resizedStream;
                }
                return fileContent;
            }
            catch
            {
                // if resize failed, just return original content
                return fileContent;
            }
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
        /// Sends a 400 (bad request)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendBadRequest( HttpContext context, string message )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest.ConvertToInt();
            context.Response.StatusDescription = message;
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Gets the number of parameters from the query string which do not require a resize
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns></returns>
        private int GetQueryCount( HttpContext context )
        {
            int count = 0;
            List<string> nonResizeQueryStrings = new List<string>()
            {
                "id",
                "guid",
                "isbinaryfile",
                "rootfolder",
                "fileName"
            };
            foreach ( string key in context.Request.QueryString )
            {
                if ( nonResizeQueryStrings.Contains( key.ToLower() ) )
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}