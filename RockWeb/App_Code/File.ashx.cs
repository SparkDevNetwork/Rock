using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Text;

using Rock.Services.Cms;
using Rock.Models.Cms;


namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class File : IHttpHandler
    {
        protected string guid = string.Empty;

        public void ProcessRequest( HttpContext context )
        {
			string fileGuid = context.Request.QueryString[0];
            context.Response.Clear();

			string errorMessage = string.Empty;
			string contentType = string.Empty;

			FileService fileService = new FileService();
			BlogService blogService = new BlogService();
			//string feedXml = blogService.ReturnFeed( key, count, format, out errorMessage, out contentType );
			string data = string.Empty;

			if ( errorMessage == string.Empty )
			{
				context.Response.ContentType = contentType;
				context.Response.Write( data );
			}
			else
			{
				context.Response.ContentType = "text/html";
				context.Response.Write( errorMessage );
			}

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
		/// Renders an Error image
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