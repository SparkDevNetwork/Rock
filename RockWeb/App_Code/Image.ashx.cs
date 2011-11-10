//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Uses the ImageResizer image resizing library found here:
// http://imageresizing.net/docs/reference

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using System.Text;

using Rock.Services.Cms;
using Rock.Models.Cms;

using ImageResizer;
using System.Collections.Specialized;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage, with all the bells and whistles.
    /// </summary>
    public class Image : IHttpHandler
    {
        protected string guid = string.Empty;

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

			// Fetch the file...
			Rock.Models.Cms.File file = ( int.TryParse( anID, out id ) ) ? fileService.GetFile( id ) : fileService.GetByGuid( anID );

			// is it cached?
			string cacheName = Uri.EscapeDataString( context.Request.Url.Query );
			string physFilePath = context.Request.MapPath( string.Format( "~/cache/{0}", cacheName ) );
			bool cached = FetchFromCache( file, physFilePath );

			// Image resizing requested?
			if ( ! cached && WantsImageResizing( context ) )
			{
				ResizeAndCache( context, file, physFilePath );
			}

			// Post process
			SendFile( context, file );
        }

		private static void ResizeAndCache( HttpContext context, Rock.Models.Cms.File file, string physFilePath )
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

		private static bool FetchFromCache( Rock.Models.Cms.File file, string physFilePath )
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

		private bool WantsImageResizing( HttpContext context )
		{
			NameValueCollection nvc = context.Request.QueryString;
			return ( nvc["maxwidth"] != null || nvc["maxheight"] != null || nvc["width"] != null ||
				nvc["height"] != null || nvc["rotate"] != null || nvc["scale"] != null );
		}

		private static void SendFile( HttpContext context, Rock.Models.Cms.File file )
		{
			context.Response.ContentType = file.MimeType;
			context.Response.AddHeader( "content-disposition", "inline;filename=" + file.FileName );
			context.Response.BinaryWrite( file.Data );
			context.Response.Flush();
			context.Response.End();
		}

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

		/// <summary>
		/// Utility method to renders an error image.  Not sure if I like this idea.
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