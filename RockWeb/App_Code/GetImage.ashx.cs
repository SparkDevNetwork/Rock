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

using ImageResizer;
using Rock.Storage;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage, with all the bells and whistles.
    /// </summary>
    public class GetImage : IHttpHandler
    {
        // TODO: Does security need to be taken into consideration in order to view an image?

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            context.Response.Clear();
            var queryString = context.Request.QueryString;

            if ( !( queryString["id"] == null || queryString["guid"] == null ) )
            {
                throw new Exception( "file id must be provided" );
            }

            var id = !string.IsNullOrEmpty( queryString["id"] ) ? queryString["id"] : queryString["guid"];
            int fileId;
            Guid fileGuid = Guid.Empty;

            if ( !( int.TryParse( id, out fileId ) || Guid.TryParse( id, out fileGuid ) ) )
            {
                SendNotFound( context );
                return;
            }

            try
            {
                var fileService = new BinaryFileService();
                var file = fileId > 0 ? fileService.Get( fileId ) : fileService.Get( fileGuid );
                string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
                string physFilePath = context.Request.MapPath( string.Format( "~/App_Data/Cache/{0}", cacheName ) );

                if ( file == null )
                {
                    SendNotFound( context );
                    return;
                }

                var fileType = file.BinaryFileType;

                // Is it cached
                if ( fileType.AllowCaching && File.Exists( physFilePath ) )
                {
                    // Is cached version newer?
                    if ( !file.LastModifiedDateTime.HasValue ||
                         file.LastModifiedDateTime.Value.CompareTo( File.GetCreationTime( physFilePath ) ) <= 0 )
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
                    Resize( queryString, file );

                if ( fileType.AllowCaching )
                    Cache( file, physFilePath );

                // Post process
                SendFile( context, file );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context ); 
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
            using ( BinaryWriter binWriter = new BinaryWriter( File.Open( physFilePath, FileMode.Create ) ) )
            {
                binWriter.Write( file.Data.Content );
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
            var entityType = file.StorageEntityType ?? file.BinaryFileType.StorageEntityType;
            var container = ProviderContainer.GetComponent( entityType.Name );

            if ( container is Rock.Storage.Provider.Database )
            {
                return file.Data;
            }
            
            var url = container.GetUrl( file );
            Stream stream;

            if ( url.StartsWith( "~/" ) )
            {
                var path = HttpContext.Current.Server.MapPath( url );
                var fileInfo = new FileInfo( path );
                stream = fileInfo.Open( FileMode.Open, FileAccess.Read );
            }
            else
            {
                var request = WebRequest.Create( url );
                var response = request.GetResponse();
                stream = response.GetResponseStream();
            }

            if ( stream != null )
            {
                using ( var memoryStream = new MemoryStream() )
                {
                    stream.CopyTo( memoryStream );
                    stream.Close();
                    return new BinaryFileData
                        {
                            Content = memoryStream.ToArray()
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
    }
}