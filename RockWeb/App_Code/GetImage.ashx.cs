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
using System.Net;
using System.Web;
using System.Linq;

using ImageResizer;
using Rock.Model;
using Rock;
using System.Threading;
using System.Diagnostics;

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
            Stopwatch stopwatch = Stopwatch.StartNew();
            
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
                string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
                string physCachedFilePath = context.Request.MapPath( string.Format( "~/App_Data/Cache/{0}", cacheName ) );

                var binaryFileService = new BinaryFileService();

                var binaryFileQuery = binaryFileService.Queryable();
                if ( fileGuid != Guid.Empty )
                {
                    binaryFileQuery = binaryFileQuery.Where( a => a.Guid == fileGuid );
                }
                else
                {
                    binaryFileQuery = binaryFileQuery.Where( a => a.Id == fileId );
                }

                // get just the binaryFileMetaData (not the file content) just in case we can get the filecontent faster from the cache
                var binaryFileMetaData = binaryFileQuery.Select( a => new
                    {
                        a.BinaryFileType,
                        a.LastModifiedDateTime,
                        a.MimeType,
                        a.FileName
                    } ).FirstOrDefault();

                Debug.WriteLine( string.Format( "GetImage {1} binaryFileMetaData @ {0} ms", stopwatch.Elapsed.TotalMilliseconds, binaryFileMetaData.FileName ) );
                stopwatch.Restart();

                if ( binaryFileMetaData == null )
                {
                    SendNotFound( context );
                    return;
                }

                byte[] fileContent = null;

                // Is it cached
                if ( binaryFileMetaData.BinaryFileType.AllowCaching && File.Exists( physCachedFilePath ) )
                {
                    // Is cached version newer?
                    if ( !binaryFileMetaData.LastModifiedDateTime.HasValue ||
                         binaryFileMetaData.LastModifiedDateTime.Value.CompareTo( File.GetCreationTime( physCachedFilePath ) ) <= 0 )
                    {
                        fileContent = FetchFromCache( physCachedFilePath );
                    }
                }
                else
                {
                    // use the binaryFileService.BeginGet/EndGet which is a little faster than the regular get
                    BinaryFile binaryFile = null;
                    System.Threading.ManualResetEvent completedEvent = new ManualResetEvent( false );

                    AsyncCallback cb = ( IAsyncResult asyncResult ) =>
                    {
                        // restore the context from the asyncResult.AsyncState 
                        HttpContext asyncContext = (HttpContext)asyncResult.AsyncState;
                        binaryFile = new BinaryFileService().EndGet( asyncResult, context );
                        completedEvent.Set();

                        Debug.WriteLine( string.Format( "GetImage {1} EndGet @ {0} ms", stopwatch.Elapsed.TotalMilliseconds, binaryFileMetaData.FileName ) );
                        stopwatch.Restart();
                    };

                    IAsyncResult beginGetResult;

                    if ( fileGuid != Guid.Empty )
                    {
                        beginGetResult = binaryFileService.BeginGet( cb, context, fileGuid );
                    }
                    else
                    {
                        beginGetResult = binaryFileService.BeginGet( cb, context, fileId );
                    }

                    // wait up to 5 minutes for the response
                    completedEvent.WaitOne( 300000 );

                    if ( binaryFile != null )
                    {
                        if ( binaryFile.Data != null )
                        {
                            fileContent = binaryFile.Data.Content;
                        }
                    }
                }

                if ( fileContent == null )
                {
                    SendNotFound( context );
                    return;
                }

                // If more than 1 query string param is passed in, assume resize is needed
                if ( queryString.Count > 1 )
                {
                    fileContent = GetResized( queryString, fileContent );
                    Debug.WriteLine( string.Format( "GetImage {1} GetResized @ {0} ms", stopwatch.Elapsed.TotalMilliseconds, binaryFileMetaData.FileName ) );
                    stopwatch.Restart();
                }

                if ( binaryFileMetaData.BinaryFileType.AllowCaching )
                {
                    Cache( fileContent, physCachedFilePath );
                    Debug.WriteLine( string.Format( "GetImage {1} Cache @ {0} ms", stopwatch.Elapsed.TotalMilliseconds, binaryFileMetaData.FileName ) );
                    stopwatch.Restart();
                }

                // respond with File
                context.Response.ContentType = binaryFileMetaData.MimeType;
                context.Response.AddHeader( "content-disposition", "inline;filename=" + binaryFileMetaData.FileName );
                context.Response.BinaryWrite( fileContent );
                context.Response.Flush();

                stopwatch.Stop();
                Debug.WriteLine( string.Format( "GetImage {1} Done @ {0} ms", stopwatch.Elapsed.TotalMilliseconds, binaryFileMetaData.FileName ) );
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
        /// Resizes and returns the resized content
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        private static byte[] GetResized( NameValueCollection queryString, byte[] fileContent )
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
        private static void Cache( byte[] fileContent, string physFilePath )
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
        private static byte[] FetchFromCache( string physFilePath )
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
        private static void SendNotFound( HttpContext context )
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