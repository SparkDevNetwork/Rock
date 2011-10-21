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
			if ( context.Request.QueryString == null )
				return;

            context.Response.Clear();

			FileService fileService = new FileService();

			int id;
			string anID = context.Request.QueryString[0];

			Rock.Models.Cms.File file = ( int.TryParse( anID, out id ) ) ? fileService.GetFile( id ) : fileService.GetByGuid( anID );

			context.Response.ContentType = file.MimeType;
			context.Response.AddHeader("content-disposition", "inline;filename=" + file.FileName);
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