//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

using Goheer.EXIF;

using Rock.Model;
using Rock.Storage;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class ImageUploader : FileUploader
    {
        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile"></param>
        /// <param name="file">The file.</param>
        /// <param name="fileType"></param>
        public override void SaveData( HttpContext context, HttpPostedFile uploadedFile, BinaryFile file, BinaryFileType fileType)
        {
            // Check to see if we should flip the image.
            try
            {
                file.FileName = Path.GetFileName( uploadedFile.FileName );
                file.MimeType = uploadedFile.ContentType;

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

                using ( var stream = new MemoryStream() )
                {
                    bmp.Save( stream, ContentTypeToImageFormat( file.MimeType ) );

                    if ( file.Data == null )
                    {
                        file.Data = new BinaryFileData();
                    }

                    file.Data.Content = stream.ToArray();
                }

                // Use provider to persist file
                var provider = fileType != null
                    ? ProviderContainer.GetComponent( fileType.StorageEntityType.Name )
                    : ProviderContainer.DefaultComponent;
                provider.SaveFile( file, null );

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
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
}