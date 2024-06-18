// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Rock.Enums.CheckIn.Labels;
using Rock.Model;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// Provides a set of methods to help work with images when rendering to a
    /// ZPL based printer.
    /// </summary>
    internal static class ZplImageHelper
    {
        /// <summary>
        /// Gets a stream that contains the data to use for the person's photo.
        /// </summary>
        /// <param name="person">The person whose photo is to be retrieved.</param>
        /// <returns>A <see cref="Stream"/> that contains the data or <c>null</c> if it could not be determined.</returns>
        public static Stream GetPersonPhotoStream( Person person )
        {
            return person.Photo?.ContentStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageStream"></param>
        /// <param name="options">The options that describe the conversion operation.</param>
        /// <returns></returns>
        public static ZplImageCache CreateImage( Stream imageStream, ZplImageOptions options )
        {
            var newImage = Image.Load<RgbaVector>( imageStream );

            // Convert any transparent pixels to white. This is required because
            // the Grayscale() filter below will balk on transparency and cause
            // the component colors to become NaN.
            ConvertTransparencyToWhite( newImage );

            newImage.Mutate( img =>
            {
                if ( options.Width != newImage.Width || options.Height != newImage.Height )
                {
                    ResizeAndCrop( img, options.Width, options.Height );
                }

                if ( options.Brightness != 1 )
                {
                    img.Brightness( options.Brightness );
                }

                if ( options.Contrast != 1 )
                {
                    img.Contrast( options.Contrast );
                }

                img.Grayscale( GrayscaleMode.Bt601 );

                if ( options.Dithering == DitherMode.Fast )
                {
                    img.Dither( KnownDitherings.Bayer8x8, new Color[] { Color.Black, Color.White } );
                }
                else if ( options.Dithering == DitherMode.Quality )
                {
                    img.Dither( KnownDitherings.Atkinson, new Color[] { Color.Black, Color.White } );
                }
            } );

            using ( var outputStream = SaveToGrf( newImage ) )
            {
                return new ZplImageCache( outputStream.ToArray(), options.Width, options.Height );
            }
        }

        /// <summary>
        /// Converts any transparent pixels to be purely white. The GRF format
        /// has two states per pixel: "Ignore" and "Fill". So it has no concept
        /// of white, black or transparent. Just fill or not.
        /// </summary>
        /// <param name="image">The image to be updated.</param>
        private static void ConvertTransparencyToWhite( Image<RgbaVector> image )
        {
            for ( int rowIndex = 0; rowIndex < image.Height; rowIndex++ )
            {
                var row = image.DangerousGetPixelRowMemory( rowIndex );
                var span = row.Span;

                for ( int colIndex = 0; colIndex < image.Width; colIndex += 1 )
                {
                    if ( span[colIndex].A == 0 || float.IsNaN( span[colIndex].A ) )
                    {
                        span[colIndex].R = 1;
                        span[colIndex].G = 1;
                        span[colIndex].B = 1;
                        span[colIndex].A = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the image data to a GRF format that can be used in a ZPL label.
        /// </summary>
        /// <param name="newImage">The image to be saved to GRF.</param>
        /// <returns>A stream that contains the GRF data.</returns>
        private static MemoryStream SaveToGrf( Image<RgbaVector> newImage )
        {
            var outputStream = new MemoryStream();
            var rowWidth = newImage.Width;

            for ( int rowIndex = 0; rowIndex < newImage.Height; rowIndex++ )
            {
                var row = newImage.DangerousGetPixelRowMemory( rowIndex );
                var span = row.Span;
                var shift = 7;
                byte grfValue = 0;

                for ( int colIndex = 0; colIndex < rowWidth; colIndex += 1 )
                {
                    // We are already grayscale, so RGB are the same. This means
                    // the R component is basically the same as the lightness in
                    // HSL, so we just use 50% as the cutoff.
                    if ( span[colIndex].R < 0.5 && span[colIndex].A >= 0.5 )
                    {
                        grfValue |= ( byte ) ( 1 << shift );
                    }

                    if ( shift > 0 )
                    {
                        shift--;
                    }
                    else
                    {
                        outputStream.WriteByte( grfValue );
                        grfValue = 0;
                        shift = 7;
                    }
                }

                if ( shift != 7 )
                {
                    outputStream.WriteByte( grfValue );
                }
            }

            outputStream.Position = 0;

            return outputStream;
        }

        /// <summary>
        /// Resize the image and crop it to fit. This is essentially a "cover"
        /// crop operation.
        /// </summary>
        /// <param name="image">The image to be resized.</param>
        /// <param name="width">The new image width.</param>
        /// <param name="height">The new image height.</param>
        public static void ResizeAndCrop( IImageProcessingContext image, int width, int height )
        {
            var size = image.GetCurrentSize();
            var widthRatio = width / ( double ) size.Width;
            var heightRatio = height / ( double ) size.Height;

            if ( widthRatio >= heightRatio )
            {
                var newHeight = ( int ) Math.Round( size.Height * widthRatio );

                image.Resize( width, newHeight );
                image.Crop( new Rectangle( 0, ( int ) Math.Round( ( newHeight - height ) / 2.0 ), width, height ) );
            }
            else
            {
                var newWidth = ( int ) Math.Round( size.Width * heightRatio );

                image.Resize( newWidth, height );
                image.Crop( new Rectangle( ( int ) Math.Round( ( newWidth - width ) / 2.0 ), 0, width, height ) );
            }
        }

        //public static Stream ZebraTest( Stream initialPhotoStream, int pixelSize, float brightness, float contrast )
        //{
        //    Bitmap initialBitmap = new Bitmap( initialPhotoStream );

        //    // Adjust the image if any of the parameters not default
        //    if ( brightness != 1.0 || contrast != 1.0 )
        //    {
        //        initialBitmap = ImageAdjust( initialBitmap, ( float ) brightness, ( float ) contrast );
        //    }

        //    // Calculate rectangle to crop image into
        //    int height = initialBitmap.Height;
        //    int width = initialBitmap.Width;
        //    var cropSection = new System.Drawing.Rectangle( 0, 0, height, width );
        //    if ( height < width )
        //    {
        //        cropSection = new System.Drawing.Rectangle( ( width - height ) / 2, 0, ( width + height ) / 2, height ); // (width + height)/2 is a simplified version of the (width - height)/2 + height function
        //    }
        //    else if ( height > width )
        //    {
        //        cropSection = new System.Drawing.Rectangle( 0, ( height - width ) / 2, width, ( height + width ) / 2 );
        //    }

        //    // Crop and resize image
        //    Bitmap resizedBitmap = new Bitmap( pixelSize, pixelSize );
        //    using ( Graphics g = Graphics.FromImage( resizedBitmap ) )
        //    {
        //        g.DrawImage( initialBitmap, new System.Drawing.Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), cropSection, GraphicsUnit.Pixel );
        //    }

        //    // Grayscale Image
        //    var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        //    var outputBitmap = new Bitmap( resizedBitmap.Width, resizedBitmap.Height, PixelFormat.Format1bppIndexed );
        //    var data = new sbyte[resizedBitmap.Width, resizedBitmap.Height];
        //    var inputData = resizedBitmap.LockBits( new System.Drawing.Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb );
        //    try
        //    {
        //        var scanLine = inputData.Scan0;
        //        var line = new byte[inputData.Stride];
        //        for ( var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride )
        //        {
        //            Marshal.Copy( scanLine, line, 0, line.Length );
        //            for ( var x = 0; x < resizedBitmap.Width; x++ )
        //            {
        //                // Change to greyscale
        //                data[x, y] = ( sbyte ) ( 64 * ( ( ( line[x * 3 + 2] * 0.299 + line[x * 3 + 1] * 0.587 + line[x * 3 + 0] * 0.114 ) / 255 ) - 0.4 ) );
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        resizedBitmap.UnlockBits( inputData );
        //    }

        //    //Dither Image
        //    var outputData = outputBitmap.LockBits( new System.Drawing.Rectangle( 0, 0, outputBitmap.Width, outputBitmap.Height ), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed );
        //    try
        //    {
        //        var scanLine = outputData.Scan0;
        //        for ( var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride )
        //        {
        //            var line = new byte[outputData.Stride];
        //            for ( var x = 0; x < resizedBitmap.Width; x++ )
        //            {
        //                var j = data[x, y] > 0;
        //                if ( j )
        //                    line[x / 8] |= masks[x % 8];
        //                var error = ( sbyte ) ( data[x, y] - ( j ? 32 : -32 ) );
        //                if ( x < resizedBitmap.Width - 1 )
        //                    data[x + 1, y] += ( sbyte ) ( 7 * error / 16 );
        //                if ( y < resizedBitmap.Height - 1 )
        //                {
        //                    if ( x > 0 )
        //                        data[x - 1, y + 1] += ( sbyte ) ( 3 * error / 16 );
        //                    data[x, y + 1] += ( sbyte ) ( 5 * error / 16 );
        //                    if ( x < resizedBitmap.Width - 1 )
        //                        data[x + 1, y + 1] += ( sbyte ) ( 1 * error / 16 );
        //                }
        //            }

        //            Marshal.Copy( line, 0, scanLine, outputData.Stride );
        //        }
        //    }
        //    finally
        //    {
        //        outputBitmap.UnlockBits( outputData );
        //    }

        //    // Convert from x to .png
        //    MemoryStream convertedStream = new MemoryStream();
        //    outputBitmap.Save( convertedStream, System.Drawing.Imaging.ImageFormat.Png );
        //    convertedStream.Seek( 0, SeekOrigin.Begin );

        //    return convertedStream;
        //}

        ///// <summary>
        ///// Adjust the brightness, contrast or gamma of the given image.
        ///// </summary>
        ///// <param name="originalImage">The original image.</param>
        ///// <param name="brightness">The brightness multiplier (-1.99 to 1.99 fully white).</param>
        ///// <param name="contrast">The contrast multiplier (2.0 would be twice the contrast).</param>
        ///// <param name="gamma">The gamma multiplier (1.0 would no change in gamma).</param>
        ///// <returns>A new adjusted image.</returns>
        //private static Bitmap ImageAdjust( Bitmap originalImage, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f )
        //{
        //    Bitmap adjustedImage = originalImage;

        //    float adjustedBrightness = brightness - 1.0f;
        //    // Matrix used to effect the image
        //    float[][] ptsArray = {
        //        new float[] { contrast, 0, 0, 0, 0 }, // scale red
        //        new float[] { 0, contrast, 0, 0, 0 }, // scale green
        //        new float[] { 0, 0, contrast, 0, 0 }, // scale blue
        //        new float[] { 0, 0, 0, 1.0f, 0 },     // no change to alpha
        //        new float[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
        //    };

        //    var imageAttributes = new ImageAttributes();
        //    imageAttributes.ClearColorMatrix();
        //    imageAttributes.SetColorMatrix( new System.Drawing.Imaging.ColorMatrix( ptsArray ), ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
        //    imageAttributes.SetGamma( gamma, ColorAdjustType.Bitmap );
        //    Graphics g = Graphics.FromImage( adjustedImage );
        //    g.DrawImage( originalImage, new System.Drawing.Rectangle( 0, 0, adjustedImage.Width, adjustedImage.Height ),
        //        0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes );

        //    return adjustedImage;
        //}
    }
}
