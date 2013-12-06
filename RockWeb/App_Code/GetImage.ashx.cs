//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Uses the ImageResizer image resizing library found here:
// http://imageresizing.net/docs/reference

using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using ImageResizer;
using Rock;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage, with all the bells and whistles.
    /// </summary>
    public class GetImage : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            context.Response.Clear();
            var queryString = context.Request.QueryString;

            int fileId = queryString["id"].AsInteger() ?? 0;
            Guid fileGuid = queryString["guid"].AsGuid();

            if ( fileId == 0 && fileGuid.Equals( Guid.Empty ) )
            {
                SendNotFound( context );
            }

            try
            {
                var binaryFileQuery = new BinaryFileService().Queryable();
                if ( fileGuid != Guid.Empty )
                {
                    binaryFileQuery = binaryFileQuery.Where( a => a.Guid == fileGuid );
                }
                else
                {
                    binaryFileQuery = binaryFileQuery.Where( a => a.Id == fileId );
                }

                //// get just the binaryFileMetaData (not the file content) just in case we can get the filecontent faster from the cache
                //// a null LastModifiedDateTime shouldn't happen, but just in case, set it to DateTime.MaxValue so we error on the side of not getting it from the cache
                var binaryFileMetaData = binaryFileQuery.Select( a => new
                    {
                        BinaryFileType_AllowCaching = a.BinaryFileType.AllowCaching,
                        LastModifiedDateTime = a.LastModifiedDateTime ?? DateTime.MaxValue,
                        a.MimeType,
                        a.FileName
                    } ).FirstOrDefault();

                if ( binaryFileMetaData == null )
                {
                    SendNotFound( context );
                    return;
                }

                byte[] fileContent = null;

                // Is it cached
                string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
                string physCachedFilePath = context.Request.MapPath( string.Format( "~/App_Data/Cache/{0}", cacheName ) );
                if ( binaryFileMetaData.BinaryFileType_AllowCaching && File.Exists( physCachedFilePath ) )
                {
                    // Has the file been modified since the last cached datetime?
                    DateTime cachedFileDateTime = File.GetCreationTime( physCachedFilePath );
                    if ( binaryFileMetaData.LastModifiedDateTime < cachedFileDateTime )
                    {
                        // NOTE: the cached file has already been resized (the size is part of the cached file's filename), so we don't need to resize it again
                        fileContent = FetchFromCache( physCachedFilePath );
                    }
                }

                if ( fileContent == null )
                {
                    // If we didn't get it from the cache, get it from the binaryFileService
                    BinaryFile binaryFile = GetFromBinaryFileService( context, fileId, fileGuid );

                    if ( binaryFile != null && binaryFile.Data != null )
                    {
                        fileContent = binaryFile.Data.Content;
                    }

                    if ( fileContent != null )
                    {
                        // If we got the image from the binaryFileService, it might need to be resized and cached
                        if ( queryString.Count > 1 )
                        {
                            // If more than 1 query string param is passed in, assume resize is needed
                            fileContent = GetResized( queryString, fileContent );
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
                context.Response.ContentType = binaryFileMetaData.MimeType;
                context.Response.AddHeader( "content-disposition", "inline;filename=" + binaryFileMetaData.FileName );
                context.Response.BinaryWrite( fileContent );
                context.Response.Flush();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.Response.Flush();
                context.ApplicationInstance.CompleteRequest();
            }
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

            // use the binaryFileService.BeginGet/EndGet which is a little faster than the regular get
            AsyncCallback cb = ( IAsyncResult asyncResult ) =>
            {
                // restore the context from the asyncResult.AsyncState 
                HttpContext asyncContext = (HttpContext)asyncResult.AsyncState;
                binaryFile = new BinaryFileService().EndGet( asyncResult, context );
                completedEvent.Set();
            };

            IAsyncResult beginGetResult;

            if ( fileGuid != Guid.Empty )
            {
                beginGetResult = new BinaryFileService().BeginGet( cb, context, fileGuid );
            }
            else
            {
                beginGetResult = new BinaryFileService().BeginGet( cb, context, fileId );
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
        private byte[] GetResized( NameValueCollection queryString, byte[] fileContent )
        {
            ResizeSettings settings = new ResizeSettings( queryString );
            MemoryStream resizedStream = new MemoryStream();
            ImageBuilder.Current.Build( new MemoryStream( fileContent ), resizedStream, settings );
            return resizedStream.GetBuffer();
        }

        /// <summary>
        /// Caches the specified file content.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="physFilePath">The physical file path.</param>
        private void Cache( byte[] fileContent, string physFilePath )
        {
            using ( BinaryWriter binWriter = new BinaryWriter( File.Open( physFilePath, FileMode.Create ) ) )
            {
                binWriter.Write( fileContent );
            }
        }

        /// <summary>
        /// Fetches from cache.
        /// </summary>
        /// <param name="physFilePath">The phys file path.</param>
        /// <returns></returns>
        private byte[] FetchFromCache( string physFilePath )
        {
            try
            {
                byte[] data;
                using ( new BinaryReader( File.Open( physFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) ) )
                {
                    data = File.ReadAllBytes( physFilePath );
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends 404 status.
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotFound( HttpContext context )
        {
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "The requested image could not be found.";
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