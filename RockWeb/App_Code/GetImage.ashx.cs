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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using ImageResizer;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

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

                context.RewritePath( context.Server.HtmlDecode( context.Request.UrlProxySafe().PathAndQuery ) );

                // Check to see if this is a BinaryFileType/BinaryFile or just a plain content file (if isBinaryFile not specified, assume it is a BinaryFile)
                bool isBinaryFile = ( context.Request.QueryString["isBinaryFile"] ?? "T" ).AsBoolean();

                if ( isBinaryFile )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        ProcessBinaryFileRequest( context, rockContext );
                    }
                }
                else
                {
                    ProcessContentFileRequest( context );
                }
            }
            catch ( Exception )
            {
                if ( !context.Response.IsClientConnected )
                {
                    // if client disconnected, ignore
                }
                else
                {
                    throw;
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

                        // If the requested file is not an image respond with not found.
                        if ( !mimeType.StartsWith( "image/" ) )
                        {
                            SendNotFound( context );
                            return;
                        }

                        context.Response.AddHeader( "content-disposition", string.Format( "inline;filename={0}", Path.GetFileName( trustedPhysicalFilePath ) ) );
                        context.Response.ContentType = mimeType;

                        // If extra query string params are passed in and it isn't an SVG file, assume resize is needed
                        if ( ( context.Request.QueryString.Count > GetQueryCount( context ) ) && ( mimeType != "image/svg+xml" ) )
                        {
                            using ( var resizedStream = GetResized( context.Request.QueryString, fileContents ) )
                            {
                                if ( resizedStream.CanSeek )
                                {
                                    resizedStream.Seek( 0, SeekOrigin.Begin );
                                }

                                resizedStream.CopyTo( context.Response.OutputStream );
                            }
                        }
                        else
                        {
                            if ( fileContents.CanSeek )
                            {
                                fileContents.Seek( 0, SeekOrigin.Begin );
                            }

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
        private void ProcessBinaryFileRequest( HttpContext context, RockContext rockContext )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            var disablePredictableIds = securitySettings.DisablePredictableIds;

            int? fileId = null;
            Guid? fileGuid = null;

            if ( disablePredictableIds )
            {
                var fileIdKey = context.Request.QueryString["fileIdKey"];
                var fileGuidString = context.Request.QueryString["guid"];

                if ( !string.IsNullOrEmpty( fileIdKey ) )
                {
                    fileId = IdHasher.Instance.GetId( fileIdKey );
                }

                if ( !string.IsNullOrEmpty( fileGuidString ) )
                {
                    fileGuid = new Guid( fileGuidString );
                }
            }
            else
            {
                fileId = context.Request.QueryString["id"].AsIntegerOrNull(); 
                fileGuid = context.Request.QueryString["guid"].AsGuidOrNull();
            }

            if ( !fileId.HasValue && !fileGuid.HasValue )
            {
                SendBadRequest( context, "File ID or GUID must be provided." );
                return;
            }

            if ((fileId.HasValue && fileId.Value == 0) || (fileGuid.HasValue && fileGuid.Value == Guid.Empty))
            {
                SendBadRequest( context, "Invalid File ID or GUID provided." );
                return;
            }

            var binaryFileQuery = new BinaryFileService( rockContext ).Queryable();
            if ( fileGuid.HasValue )
            {
                binaryFileQuery = binaryFileQuery.Where( a => a.Guid == fileGuid.Value );
            }
            else
            {
                binaryFileQuery = binaryFileQuery.Where( a => a.Id == fileId.Value );
            }

            //// get just the binaryFileMetaData (not the file content) just in case we can get the filecontent faster from the cache
            //// a null ModifiedDateTime shouldn't happen, but just in case, set it to DateTime.MaxValue so we error on the side of not getting it from the cache
            var binaryFileMetaData = binaryFileQuery.Select( a => new
            {
                a.Id,
                BinaryFileType_CacheToServerFileSystem = a.BinaryFileType.CacheToServerFileSystem,
                BinaryFileType_RequiresViewSecurity = a.BinaryFileType.RequiresViewSecurity,
                a.BinaryFileTypeId,
                ModifiedDateTime = a.ModifiedDateTime ?? DateTime.MaxValue,
                a.MimeType,
                a.FileName
            } ).FirstOrDefault();

            if ( binaryFileMetaData == null )
            {
                SendNotFound( context );
                return;
            }

            // Add cache validation headers
            context.Response.AddHeader( "Last-Modified", binaryFileMetaData.ModifiedDateTime.ToUniversalTime().ToString( "R" ) );
            context.Response.AddHeader( "ETag", binaryFileMetaData.ModifiedDateTime.ToString().XxHash() );

            var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;
            BinaryFile binaryFileAuth = new BinaryFileService( rockContext ).Queryable( "BinaryFileType" ).AsNoTracking().First( a => a.Id == binaryFileMetaData.Id );

            var parentEntityAllowsView = binaryFileAuth.ParentEntityAllowsView( currentPerson );

            // If no parent entity is specified then check if there is scecurity on the BinaryFileType
            // Use BinaryFileType.RequiresViewSecurity because checking security for every file is slow (~40ms+ per request)
            if ( parentEntityAllowsView == null && binaryFileMetaData.BinaryFileType_RequiresViewSecurity )
            {
                var securityGrant = SecurityGrant.FromToken( context.Request.QueryString["securityGrant"] );

                if ( !binaryFileAuth.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) && securityGrant?.IsAccessGranted( binaryFileAuth, Rock.Security.Authorization.VIEW ) != true )
                {
                    SendNotAuthorized( context );
                    return;
                }
            }

            // Since this has a value use it
            if ( parentEntityAllowsView == false )
            {
                SendNotAuthorized( context );
                return;
            }

            // Security checks pass so send the file
            Stream fileContent = null;
            try
            {
                // Is it cached
                string cacheName = UrlQueryToCachedFileName( context.Request.QueryString, binaryFileMetaData.MimeType );
                string physCachedFilePath = context.Request.MapPath( string.Format( "~/App_Data/Cache/{0}", cacheName ) );
                if ( binaryFileMetaData.BinaryFileType_CacheToServerFileSystem && File.Exists( physCachedFilePath ) )
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
                    BinaryFile binaryFile = GetFromBinaryFileService( context, binaryFileMetaData.Id, rockContext );

                    if ( binaryFile != null )
                    {
                        try
                        {
                            // put this in a try catch because the binaryFile content might be from a 3rd party dll that throws exceptions for http errors such as 4XX responses.
                            fileContent = binaryFile.ContentStream;
                        }
                        catch (System.Net.WebException wex )
                        {
                            // If this is a 4XX error than pass that along to the client
                            var response = ( HttpWebResponse ) wex.Response;
                            if ( response != null )
                            {
                                context.Response.StatusCode = ( int ) response.StatusCode;
                                context.Response.StatusDescription = response.StatusDescription;
                                context.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                // Otherwise log the exception and use the not found message for the client.
                                ExceptionLogService.LogException( wex );
                                SendNotFound( context );
                            }

                            return;
                        }
                        catch ( System.Web.HttpException hex )
                        {
                            // If this is a 4XX error than pass that along to the client
                            context.Response.StatusCode = hex.GetHttpCode();
                            context.Response.StatusDescription = hex.Message;
                            context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                            SendNotFound( context );
                            return;
                        }
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

                        if ( binaryFileMetaData.BinaryFileType_CacheToServerFileSystem )
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
                if ( binaryFileMetaData.BinaryFileTypeId != null )
                {
                    var binaryFileCache = BinaryFileTypeCache.Get( binaryFileMetaData.BinaryFileTypeId.Value );
                    binaryFileCache.CacheControlHeader.SetupHttpCachePolicy( context.Response.Cache );
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
            cleanedQueryString.Remove( "securityGrant" );

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
        private BinaryFile GetFromBinaryFileService( HttpContext context, int fileId, RockContext rockContext )
        {
            BinaryFile binaryFile = null;
            System.Threading.ManualResetEvent completedEvent = new ManualResetEvent( false );

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
            // Check if the request has disabled optimizations. If so return.
            if ( queryString["disableoptimizations"] != null )
            {
                return fileContent;
            }

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