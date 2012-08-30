<%@ WebHandler Language="C#" Class="ImageUploader" %>

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.ServiceModel.Web;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;

using Rock;
using Rock.Cms;

using Goheer.EXIF;

/// <summary>
/// Upload an image to cmsFile storage.
/// Usage parameters:
///		id - the ID or GUID of the file to replace (optional)
///		enableResize - will cause image to be resized to roughly 1024 x 768
/// </summary>
public class ImageUploader : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest( HttpContext context )
	{
		// *********************************************
		// TODO: verify user is authorized to save file!
		// *********************************************

		if ( !context.User.Identity.IsAuthenticated )
			throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );
	
		try
		{
            HttpPostedFile uploadedFile = null;
            HttpFileCollection hfc = context.Request.Files;
            foreach ( string fk in hfc.AllKeys )
            {
                uploadedFile = hfc[fk];
                break;
            }
                
			// No file or no data?  No good.
			if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
			{
				context.Response.Write( "0" );
				return;
			}
			
			FileService fileService = new FileService();

            Rock.Cms.File cmsFile;
                        
			// was an ID given? if so, fetch that file and replace it with the new one
            if ( context.Request.QueryString.Count > 0)
			{
				string anID = context.Request.QueryString[0];
				int id;
                cmsFile = ( int.TryParse( anID, out id ) ) ? fileService.Get( id ) : fileService.GetByEncryptedKey( anID );
			}
			else
			{
				// ...otherwise create a new Cms File
				cmsFile = new Rock.Cms.File();
                cmsFile.IsTemporary = true;
                fileService.Add( cmsFile, null );
			}

			cmsFile.MimeType = uploadedFile.ContentType;
			cmsFile.FileName = uploadedFile.FileName;
			
			// Check to see if we should flip the image.
			try
			{
				Bitmap bmp = new Bitmap( uploadedFile.InputStream );
				var exif = new EXIFextractor( ref bmp, "\n" );
				if ( exif["Orientation"] != null )
				{
					RotateFlipType flip = OrientationToFlipType( exif["Orientation"].ToString() );
					if ( flip != RotateFlipType.RotateNoneFlipNone ) // don't flip if orientation is correct
					{
						bmp.RotateFlip( flip );
						exif.setTag( 0x112, "1" ); // reset orientation tag
					}
				}

				if ( context.Request.QueryString["enableResize"] != null )
				{
					Bitmap resizedBmp = RoughResize( bmp, 1024, 768 );
					bmp = resizedBmp;
				}

				using ( MemoryStream stream = new MemoryStream() )
				{
					bmp.Save( stream, ContentTypeToImageFormat( cmsFile.MimeType ) );
					cmsFile.Data = stream.ToArray();
					stream.Close();
				}
			}
			catch
			{
				// TODO: Log unable to rotate and/or resize.
			}
			
			fileService.Save( cmsFile, null );
            
			context.Response.Write( cmsFile.Id.ToJSON() );

		}
		catch ( Exception ex )
		{
			context.Response.Write( "err:" + ex.Message + "<br>" + ex.StackTrace );
		}
	}

	public bool IsReusable
	{
		get
		{
			return false;
		}
	}

	/// <summary>
	/// Returns the ImageFormat for the given ContentType string.
	/// Throws NotSupportedException if given an unknown/unsupported content type.
	/// </summary>
	/// <param name="contentType">the content type</param>
	/// <returns>ImageFormat</returns>
	private static ImageFormat ContentTypeToImageFormat( string contentType )
	{
		switch ( contentType )
		{
			case "image/jpg":
            case "image/jpeg":
				return ImageFormat.Jpeg;
			case "image/png":
				return ImageFormat.Png;
			case "image/gif":
				return ImageFormat.Gif;
			case "image/bmp":
				return ImageFormat.Bmp;
			case "image/tiff":
				return ImageFormat.Tiff;
			default:
				throw new NotSupportedException( string.Format( "unknown ImageFormat for {0}", contentType ) );
		}
	}
	private static RotateFlipType OrientationToFlipType( string orientation )
	{
		switch ( int.Parse( orientation ) )
		{
			case 1:
				return RotateFlipType.RotateNoneFlipNone;
			case 2:
				return RotateFlipType.RotateNoneFlipX;
			case 3:
				return RotateFlipType.Rotate180FlipNone;
			case 4:
				return RotateFlipType.Rotate180FlipX;
			case 5:
				return RotateFlipType.Rotate90FlipX;
			case 6:
				return RotateFlipType.Rotate90FlipNone;
			case 7:
				return RotateFlipType.Rotate270FlipX;
			case 8:
				return RotateFlipType.Rotate270FlipNone;
			default:
				return RotateFlipType.RotateNoneFlipNone;
		}
	}

	private static Image ResizeImage( Image imgToResize, Size size )
	{
		int sourceWidth = imgToResize.Width;
		int sourceHeight = imgToResize.Height;
		
		float nPercentW = ( (float)size.Width / (float)sourceWidth );
		float nPercentH = ( (float)size.Height / (float)sourceHeight );

		float nPercent = ( nPercentH < nPercentW ) ? nPercentH : nPercentW;

		int destWidth = (int)( sourceWidth * nPercent );
		int destHeight = (int)( sourceHeight * nPercent );

		Bitmap b = new Bitmap( destWidth, destHeight );
		Graphics g = Graphics.FromImage( (Image)b );
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;

		g.DrawImage( imgToResize, 0, 0, destWidth, destHeight );
		g.Dispose();

		return (Image)b;
	}

	private static Bitmap RoughResize( Bitmap input, int maxWidth, int maxHeight )
	{
		// ensure resize is even needed
		if ( input.Width > maxWidth || input.Height > maxHeight )
		{
			// determine which is dimension difference is larger
			if ( ( input.Width - maxWidth ) > ( input.Height - maxHeight ) )
			{
				// width difference is larger
				double resizeRatio = maxWidth / (double)input.Width;
				int newHeight = Convert.ToInt32( input.Height * resizeRatio );
				input = (Bitmap)ResizeImage( (Image)input, new Size( maxWidth, newHeight ) );
			}
			else
			{
				double resizeRatio = maxHeight / (double)input.Height;
				int newWidth = Convert.ToInt32( input.Width * resizeRatio );
				input = (Bitmap)ResizeImage( (Image)input, new Size( newWidth, maxHeight ) );
			}
		}
		return input;
	}
}