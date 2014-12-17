﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Goheer.EXIF;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class ImageUploader : FileUploader
    {
        /// <summary>
        /// Gets the file bytes.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public override Stream GetFileContentStream( HttpContext context, HttpPostedFile uploadedFile )
        {
            if ( uploadedFile.ContentType == "image/svg+xml" )
            {
                return base.GetFileContentStream( context, uploadedFile );
            }
            else
            {
                Bitmap bmp = new Bitmap( uploadedFile.InputStream );

                // Check to see if we should flip the image.
                var exif = new EXIFextractor( ref bmp, "\n" );
                if ( exif["Orientation"] != null )
                {
                    RotateFlipType flip = OrientationToFlipType( exif["Orientation"].ToString() );

                    // don't flip if orientation is correct
                    if ( flip != RotateFlipType.RotateNoneFlipNone )
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

                var stream = new MemoryStream();
                bmp.Save( stream, ContentTypeToImageFormat( uploadedFile.ContentType ) );
                return stream;
            }
        }

        /// <summary>
        /// Validates the type of the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <exception cref="WebFaultException{System.String}">Filetype now allowed</exception>
        public override void ValidateFileType( HttpContext context, HttpPostedFile uploadedFile )
        {
            base.ValidateFileType( context, uploadedFile );

            var globalAttributesCache = GlobalAttributesCache.Read();
            IEnumerable<string> contentImageFileTypeWhiteList = ( globalAttributesCache.GetValue( "ContentImageFiletypeWhitelist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );

            // clean up list
            contentImageFileTypeWhiteList = contentImageFileTypeWhiteList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            string fileExtension = Path.GetExtension( uploadedFile.FileName ).ToLower().TrimStart( new char[] { '.' } );
            if ( !contentImageFileTypeWhiteList.Contains( fileExtension ) )
            {
                throw new Rock.Web.FileUploadException( "Image filetype not allowed", System.Net.HttpStatusCode.NotAcceptable );
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

        /// <summary>
        /// Orientations the type of to flip.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Resizes the image.
        /// </summary>
        /// <param name="imgToResize">The img to resize.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private static Image ResizeImage( Image imgToResize, Size size )
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercentW = (float)size.Width / (float)sourceWidth;
            float nPercentH = (float)size.Height / (float)sourceHeight;

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

        /// <summary>
        /// Roughes the resize.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
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