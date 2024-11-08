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
using System.Linq;

using Rock.Configuration;
using Rock.Enums.CheckIn.Labels;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
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
        #region Fields

        /// <summary>
        /// The standard icon font collection, only loaded when needed.
        /// </summary>
        private static Lazy<FontCollection> _iconFontCollection = new Lazy<FontCollection>( CreateIconFontCollection );

        /// <summary>
        /// The regular font for Font Awesome.
        /// </summary>
        private static FontFamily? _fontAwesomeRegular = null;

        /// <summary>
        /// The bold font for Font Awesome.
        /// </summary>
        private static FontFamily? _fontAwesomeBold = null;

        /// <summary>
        /// Holds a lookup table to quickly convert byte arrays to hex strings
        /// for writing to the ZPL content.
        /// </summary>
        private static readonly uint[] _hexLookupTable = CreateLookup32();

        #endregion

        /// <summary>
        /// Creates a new image from the original image. The original image
        /// will be scaled to fit fully inside the specified with and height
        /// and then centered.
        /// </summary>
        /// <param name="imageStream">The original image.</param>
        /// <param name="options">The options that describe the conversion operation.</param>
        /// <returns>The data for the new image.</returns>
        public static ZplImageCache CreateImage( Stream imageStream, ZplImageOptions options )
        {
            var image = Image.Load<RgbaVector>( imageStream );
            var newImage = new Image<RgbaVector>( options.Width, options.Height, new RgbaVector( 1, 1, 1, 1 ) );

            image.Mutate( img =>
            {
                if ( options.Width != image.Width || options.Height != image.Height )
                {
                    Resize( img, options.Width, options.Height );
                }

                if ( options.Brightness != 1 )
                {
                    img.Brightness( options.Brightness );
                }

                img.Grayscale( GrayscaleMode.Bt601 );

                Dither( img, options.Dithering );
            } );

            newImage.Mutate( img =>
            {
                var location = new Point( ( options.Width - image.Width ) / 2, ( options.Height - image.Height ) / 2 );

                img.DrawImage( image, location, 1 );
            } );

            using ( var outputStream = SaveToGrf( newImage ) )
            {
                return new ZplImageCache( outputStream.ToArray(), options.Width, options.Height );
            }
        }

        /// <summary>
        /// Apply dithering to the image being processed.
        /// </summary>
        /// <param name="image">The image to dither.</param>
        /// <param name="dithering">The dithering mode.</param>
        [ExcludeFromCodeCoverage]
        private static void Dither( IImageProcessingContext image, DitherMode dithering )
        {
            if ( dithering == DitherMode.Fast )
            {
                image.Dither( KnownDitherings.Bayer8x8, new Color[] { Color.Black, Color.White } );
            }
            else if ( dithering == DitherMode.Quality )
            {
                image.Dither( KnownDitherings.Atkinson, new Color[] { Color.Black, Color.White } );
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
        /// Resize the image to fit within the specified maximum width and height.
        /// </summary>
        /// <param name="image">The image to be resized.</param>
        /// <param name="maxWidth">The new image width.</param>
        /// <param name="maxHeight">The new image height.</param>
        private static void Resize( IImageProcessingContext image, int maxWidth, int maxHeight )
        {
            var size = image.GetCurrentSize();
            var widthRatio = maxWidth / ( double ) size.Width;
            var heightRatio = maxHeight / ( double ) size.Height;

            if ( widthRatio <= heightRatio )
            {
                var newHeight = ( int ) Math.Round( size.Height * widthRatio );

                image.Resize( maxWidth, newHeight );
            }
            else
            {
                var newWidth = ( int ) Math.Round( size.Width * heightRatio );

                image.Resize( newWidth, maxHeight );
            }
        }

        /// <summary>
        /// Creates a new icon font collection. This is called once automatically
        /// when the first icon is generated. Otherwise it is used by unit tests
        /// to ensure correct operation.
        /// </summary>
        /// <returns>The font collection.</returns>
        internal static FontCollection CreateIconFontCollection()
        {
            var fontCollection = new FontCollection();

            var webRoot = RockApp.Current.HostingSettings.WebRootPath;
            var fontPath = Path.Combine( webRoot, "Assets", "Fonts", "FontAwesome" );

            _fontAwesomeRegular = TryAddToFontCollection( fontCollection, Path.Combine( fontPath, "fa-regular-400.ttf" ) );
            _fontAwesomeBold = TryAddToFontCollection( fontCollection, Path.Combine( fontPath, "fa-solid-900.ttf" ) );

            return fontCollection;
        }

        /// <summary>
        /// Tries to add the font at the path to the collection. If it fails
        /// then null is returned rather than an exception being thrown.
        /// </summary>
        /// <param name="fontCollection">The font collection to add the font to.</param>
        /// <param name="path">The absolute path to the font file.</param>
        /// <returns>The <see cref="FontFamily"/> or <c>null</c> if it could not be loaded.</returns>
        private static FontFamily? TryAddToFontCollection( FontCollection fontCollection, string path )
        {
            try
            {
                return fontCollection.Add( path );
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                return null;
            }
        }

        /// <summary>
        /// Used by unit testing to clear the font cache between tests.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal static void ClearIconFontCache()
        {
            _iconFontCollection = new Lazy<FontCollection>( CreateIconFontCollection );
        }

        /// <summary>
        /// Creates a new image at the specified width and height and fill it
        /// with the icon content.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="icon">The icon to draw.</param>
        /// <returns>A new image cache object that contains the image data.</returns>
        public static ZplImageCache CreateIcon( int width, int height, LabelIcon icon )
        {
            var image = new Image<RgbaVector>( width, height, new RgbaVector( 1, 1, 1, 1 ) );

            // Ensure the fonts are loaded.
            _ = _iconFontCollection.Value;

            var fontFamily = icon.IsBold ? _fontAwesomeBold : _fontAwesomeRegular;

            if ( fontFamily != null )
            {
                // Some icons are wider than they are tall. So adjust the width
                // down a bit to compensate.
                var fontSize = Math.Min( width * 0.85, height );
                var font = fontFamily.Value.CreateFont( ( float ) fontSize, icon.IsBold ? FontStyle.Bold : FontStyle.Regular );

                // Measure the icon size so we can center it.
                var textOptions = new TextOptions( font );
                var textSize = TextMeasurer.Measure( icon.Code, textOptions );

                // Draw the icon centerd in the image.
                image.Mutate( img =>
                {
                    textOptions = new TextOptions( font )
                    {
                        Origin = new System.Numerics.Vector2( ( width - textSize.Width ) / 2, ( height - textSize.Height ) / 2 )
                    };

                    img.DrawText( textOptions, icon.Code, Color.Black );
                } );
            }

            return new ZplImageCache( SaveToGrf( image ).ToArray(), width, height );
        }

        /// <summary>
        /// Creates a lookup table for fast byte to Hex conversion.
        /// </summary>
        /// <returns>An array of lookup values.</returns>
        private static uint[] CreateLookup32()
        {
            var result = new uint[256];

            for ( int i = 0; i < 256; i++ )
            {
                string s = i.ToString( "X2" );
                result[i] = s[0] + ( ( uint ) s[1] << 16 );
            }

            return result;
        }

        /// <summary>
        /// Converts an array of bytes into a hex string extremely fast. This
        /// uses a lookup table and pre-allocates the character array to maximize
        /// speed and reduce allocations.
        /// </summary>
        /// <param name="bytes">The array of bytes to convert to hex.</param>
        /// <returns>A string that represents <paramref name="bytes"/> as a hexadecimal string.</returns>
        internal static string ByteArrayToHexViaLookup32( byte[] bytes )
        {
            var lookup32 = _hexLookupTable;
            var result = new char[bytes.Length * 2];

            for ( int i = 0; i < bytes.Length; i++ )
            {
                var val = lookup32[bytes[i]];

                result[2 * i] = ( char ) val;
                result[2 * i + 1] = ( char ) ( val >> 16 );
            }

            return new string( result );
        }
    }
}
