//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Uses the ImageResizer image resizing library found here:
// http://imageresizing.net/docs/reference

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using ImageResizer;
using Rock.Storage;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage, with all the bells and whistles.
    /// </summary>
    public class Image : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            context.Response.Clear();
            var queryString = context.Request.QueryString;

            if ( queryString["id"] == null || queryString["guid"] == null )
            {
                throw new Exception( "file id must be provided" );
            }

            var id = string.IsNullOrEmpty( queryString["id"] ) ? queryString["id"] : queryString["guid"];
            int fileId;
            Guid fileGuid;

            if ( !int.TryParse( id, out fileId ) || !Guid.TryParse( id, out fileGuid ) )
            {
                context.Response.StatusCode = 404;
                context.Response.End();
                return;
            }

            try
            {
                var fileService = new BinaryFileService();
                var file = fileId > 0 ? fileService.Get( fileId ) : fileService.Get( fileGuid );
                string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
                string physFilePath = context.Request.MapPath( string.Format( "~/Cache/{0}", cacheName ) );

                if ( file == null )
                {
                    SendNotFound( context );
                    return;
                }

                // Is it cached
                if ( System.IO.File.Exists( physFilePath ) )
                {
                    // Is cached version newer?
                    if ( !file.LastModifiedDateTime.HasValue ||
                         file.LastModifiedDateTime.Value.CompareTo( System.IO.File.GetCreationTime( physFilePath ) ) <= 0 )
                    {
                        file.Data = new BinaryFileData { Content = FetchFromCache( physFilePath ) };
                    }
                }
                else
                {
                    file.Data = GetFileContent( file );
                }

                if ( file.Data == null )
                {
                    SendNotFound( context );
                    return;
                }

                // If more than 1 query string param is passed in, assume resize is needed
                if ( queryString.Count > 1 )
                {
                    Resize( queryString, file );
                }

                Cache( file, physFilePath );

                // Post process
                SendFile( context, file );
            }
            catch ( Exception ex )
            {
                // TODO: log this error
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.Response.Flush();
                context.Response.End();
            }
        }

        /// <summary>
        /// Resizes the specified context.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="file">The file.</param>
        private static void Resize( NameValueCollection queryString, BinaryFile file )
        {
            ResizeSettings settings = new ResizeSettings( queryString );
            MemoryStream resizedStream = new MemoryStream();
            ImageBuilder.Current.Build( new MemoryStream( file.Data.Content ), resizedStream, settings );
            file.Data.Content = resizedStream.GetBuffer();
        }

        /// <summary>
        /// Caches the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="physFilePath">The phys file path.</param>
        private static void Cache( BinaryFile file, string physFilePath )
        {
            try
            {
                using ( BinaryWriter binWriter = new BinaryWriter( System.IO.File.Open( physFilePath, FileMode.Create ) ) )
                {
                    binWriter.Write( file.Data.Content );
                }
            }
            catch { /* do nothing, not critical if this fails, although TODO: log */ }
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
                using ( new BinaryReader( System.IO.File.Open( physFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) ) )
                {
                    data = System.IO.File.ReadAllBytes( physFilePath );
                }

                return data;
            }
            catch
            {
                var log = new ExceptionLog();
                return null;
            }

            
        }

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="file">The file.</param>
        private static void SendFile( HttpContext context, BinaryFile file )
        {
            context.Response.ContentType = file.MimeType;
            context.Response.AddHeader( "content-disposition", "inline;filename=" + file.FileName );
            context.Response.BinaryWrite( file.Data.Content );
            context.Response.Flush();
        }

        /// <summary>
        /// Sends 404 status.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void SendNotFound( HttpContext context )
        {
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "The requested image could not be found.";
            context.Response.End();
        }

        /// <summary>
        /// Gets the content of the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private static BinaryFileData GetFileContent( BinaryFile file )
        {
            var entityType = file.StorageEntityType;
            var container = ProviderContainer.GetComponent( entityType.Name );

            if ( container is Rock.Storage.Provider.Database )
            {
                return file.Data;
            }

            var url = container.GetUrl( file );
            var request = WebRequest.Create( url );
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var encoding = Encoding.GetEncoding( "utf-8" );

            if ( stream != null )
            {
                using ( var sr = new StreamReader( stream, encoding ) )
                {
                    return new BinaryFileData
                        {
                            Content = encoding.GetBytes( sr.ReadToEnd() )
                        };
                }
            }

            return null;
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

        /// <summary>
        /// Not Currently Used.
        /// 
        /// Utility method to renders an error image to the output stream.
        /// Not sure if I like this idea, but it would generate the errorMessages
        /// as an image in red text.
        /// </summary>
        /// <param name="context">HttpContext of current request</param>
        /// <param name="errorMessage">error message text to render</param>
        private void RenderErrorImage( HttpContext context, string errorMessage )
        {
            context.Response.Clear();
            context.Response.ContentType = "image/jpeg";
            Bitmap bitmap = new Bitmap( 7 * errorMessage.Length, 30 );    // width based on error message
            Graphics g = Graphics.FromImage( bitmap );
            g.FillRectangle( new SolidBrush( Color.LightSalmon ), 0, 0, bitmap.Width, bitmap.Height ); // background
            g.DrawString( errorMessage, new Font( "Tahoma", 10, FontStyle.Bold ), new SolidBrush( Color.DarkRed ), new PointF( 5, 5 ) );
            bitmap.Save( context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg );
        }
    }
}