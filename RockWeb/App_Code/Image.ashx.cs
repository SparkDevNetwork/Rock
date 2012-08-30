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
using System.Web;

using ImageResizer;

using Rock.Cms;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage, with all the bells and whistles.
    /// </summary>
    public class Image : IHttpHandler
    {
        public void ProcessRequest( HttpContext context )
        {
			if ( context.Request.QueryString == null || context.Request.QueryString.Count == 0)
			{
				return;
			}

            context.Response.Clear();

			FileService fileService = new FileService();
			string anID = context.Request.QueryString[0];
			int id;

			try
			{
				// Fetch the file...
                Rock.Cms.File file = ( int.TryParse( anID, out id ) ) ? fileService.Get( id ) : fileService.GetByEncryptedKey( anID );

				// is it cached?
				string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
				string physFilePath = context.Request.MapPath( string.Format( "~/Cache/{0}", cacheName ) );
				bool cached = FetchFromCache( file, physFilePath );

				// Image resizing requested?
				if ( !cached && WantsImageResizing( context ) )
				{
					ResizeAndCache( context, file, physFilePath );
				}

				// Post process
				SendFile( context, file );
			}
			catch ( Exception ex )
			{
				// TODO: log this error
				context.Response.StatusDescription = ex.Message;
				context.Response.StatusCode = 500;
				context.Response.End();
			}
        }

		private static void ResizeAndCache( HttpContext context, Rock.Cms.File file, string physFilePath )
		{
			ResizeSettings settings = new ResizeSettings( context.Request.QueryString );
			MemoryStream resizedStream = new MemoryStream();
			ImageBuilder.Current.Build( new MemoryStream( file.Data ), resizedStream, settings );
			file.Data = resizedStream.GetBuffer();

			// now cache the file for later use
			try
			{
				using ( BinaryWriter binWriter = new BinaryWriter( System.IO.File.Open( physFilePath, FileMode.Create ) ) )
				{
					binWriter.Write( file.Data );
				}
			}
			catch { /* do nothing, not critical if this fails, although TODO: log */ }
		}

		private static bool FetchFromCache( Rock.Cms.File file, string physFilePath )
		{
			bool cached = false;
			if ( System.IO.File.Exists( physFilePath ) && file.CreatedDateTime < System.IO.File.GetCreationTime( physFilePath ) )
			{
				try
				{
					using ( BinaryReader binReader = new BinaryReader( System.IO.File.Open( physFilePath, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read ) ) )
					{
						file.Data = System.IO.File.ReadAllBytes( physFilePath );
					}
					cached = true;
				}
				catch { /* ok, so we'll just skip using the cache, but TODO: log this */}

			}
			return cached;
		}

		/// <summary>
		/// A small utility method to determine if we need to use the ImageResizer.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>True if the request desires image resizing/manipulation; false otherwise.</returns>
		private static bool WantsImageResizing( HttpContext context )
		{
			return context.Request.QueryString.Count > 1;
		}

		private static void SendFile( HttpContext context, Rock.Cms.File file )
		{
			context.Response.ContentType = file.MimeType;
			context.Response.AddHeader( "content-disposition", "inline;filename=" + file.FileName );
			context.Response.BinaryWrite( file.Data );
			context.Response.Flush();
		}

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
		private void renderErrorImage( HttpContext context, string errorMessage )
		{
			context.Response.Clear();
			context.Response.ContentType = "image/jpeg";
			Bitmap bitmap = new Bitmap( 7 * errorMessage.Length, 30 );	// width based on error message
			Graphics g = Graphics.FromImage( bitmap );
			g.FillRectangle( new SolidBrush( Color.LightSalmon ), 0, 0, bitmap.Width, bitmap.Height ); // background
			g.DrawString( errorMessage, new Font( "Tahoma", 10, FontStyle.Bold ), new SolidBrush( Color.DarkRed ), new PointF( 5, 5 ) );
			bitmap.Save( context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg );
		}
    }
}