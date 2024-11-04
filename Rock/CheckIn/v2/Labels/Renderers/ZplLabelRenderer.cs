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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using QRCoder;

using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// Renders labels in the ZPL format for Zebra label printers.
    /// </summary>
    internal class ZplLabelRenderer : TextLabelRenderer
    {
        /// <summary>
        /// The DPI of the printer. If not known we default to 203.
        /// </summary>
        private int _dpi = 203;

        /// <inheritdoc/>
        protected override void BeginLabel( StreamWriter writer )
        {
            if ( PrintRequest.Capabilities.Dpi.HasValue )
            {
                _dpi = PrintRequest.Capabilities.Dpi.Value;
            }

            var labelWidth = ( int ) ( _dpi * PrintRequest.Label.Width );
            var labelHeight = ( int ) ( _dpi * PrintRequest.Label.Height );

            // ^CI28 = use UTF-8 character set.
            writer.WriteLine( $"^XA^CI28^PW{labelWidth}^LL{labelHeight}" );
        }

        /// <inheritdoc/>
        protected override void WriteField( StreamWriter writer, LabelField field )
        {
            if ( field.Field.FieldType == LabelFieldType.Text )
            {
                WriteTextField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Line )
            {
                WriteLineField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Rectangle )
            {
                WriteRectangleField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Ellipse )
            {
                WriteEllipseField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Icon )
            {
                WriteIconField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Image )
            {
                WriteImageField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.AttendeePhoto )
            {
                WriteAttendeePhotoField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Barcode )
            {
                WriteBarcodeField( writer, field );
            }
        }

        /// <inheritdoc/>
        protected override void EndLabel( StreamWriter writer )
        {
            writer.WriteLine( "^XZ" );
        }

        /// <summary>
        /// Writes a text field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteTextField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<TextFieldConfiguration>();
            var values = field.GetFormattedValues( PrintRequest );

            if ( config.CollectionFormat == TextCollectionFormat.TwoColumn )
            {
                var col1Value = string.Join( "\\&", values.Where( ( _, idx ) => idx % 2 == 0 ) );
                var col2Value = string.Join( "\\&", values.Where( ( _, idx ) => idx % 2 == 1 ) );

                // Calculate column width with 1/4 inch gutter between.
                var gutter = 0.25;
                var colWidth = ( field.Field.Width - gutter ) / 2;

                WriteTextFieldColumn( writer,
                    config,
                    field.Field.Left,
                    field.Field.Top,
                    colWidth,
                    field.Field.Height,
                    col1Value );

                WriteTextFieldColumn( writer,
                    config,
                    field.Field.Left + colWidth + gutter,
                    field.Field.Top,
                    colWidth,
                    field.Field.Height,
                    col2Value );
            }
            else
            {
                string textValue;

                if ( config.CollectionFormat == TextCollectionFormat.CommaDelimited )
                {
                    textValue = string.Join( ", ", values );
                }
                else if ( config.CollectionFormat == TextCollectionFormat.OnePerLine )
                {
                    textValue = string.Join( "\\&", values );
                }
                else
                {
                    textValue = values[0];
                }

                WriteTextFieldColumn( writer,
                    config,
                    field.Field.Left,
                    field.Field.Top,
                    field.Field.Width,
                    field.Field.Height,
                    textValue );
            }
        }

        /// <summary>
        /// Writes a single column of text to the writer.
        /// </summary>
        /// <param name="writer">The writer that we will send ZPL code to.</param>
        /// <param name="config">The field configuration.</param>
        /// <param name="left">The left postion in inches.</param>
        /// <param name="top">The top position in inches.</param>
        /// <param name="width">The width in inches.</param>
        /// <param name="height">The height in inches.</param>
        /// <param name="textValue">The text content to display.</param>
        private void WriteTextFieldColumn( StreamWriter writer, TextFieldConfiguration config, double left, double top, double width, double height, string textValue )
        {
            WriteFieldOrigin( writer, left, top );

            if ( config.IsColorInverted )
            {
                writer.Write( "^FR" );
            }

            if ( config.MaxLength > 0 )
            {
                textValue = textValue.Truncate( config.MaxLength );
            }

            var fontSizeInPoints = GetFontSize( textValue.Length, config.FontSize, config.AdaptiveFontSize );
            var fontSize = GetFontDotSize( fontSizeInPoints );
            var horizontalFontSize = fontSize;
            var lineCount = Math.Max( 1, ( int ) Math.Round( ToDots( height ) / ( double ) fontSize ) );
            var alignment = "L";

            if ( config.HorizontalAlignment == HorizontalTextAlignment.Center )
            {
                alignment = "C";
                // If the text does not end in a ZPL newline then the alignment
                // doesn't work exactly as expected.
                textValue += "\\&";
            }
            else if ( config.HorizontalAlignment == HorizontalTextAlignment.Right )
            {
                alignment = "R";
            }

            if ( config.IsBold )
            {
                horizontalFontSize = ( int ) Math.Floor( fontSize * 1.15 );
            }
            else if ( config.IsCondensed )
            {
                horizontalFontSize = ( int ) Math.Floor( fontSize * 0.8 );
            }

            writer.WriteLine( $"^FB{ToDots( width )},{lineCount},0,{alignment}^A0,{horizontalFontSize},{fontSize}^FD{textValue}^FS" );
        }

        /// <summary>
        /// Writes a line field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteLineField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<LineFieldConfiguration>();

            string color = config.IsBlack ? "B" : "W";

            if ( field.Field.Width == 0 || field.Field.Height == 0 )
            {
                WriteFieldOrigin( writer, field );

                if ( field.Field.Width == 0 )
                {
                    writer.Write( $"^GB{config.Thickness},{ToDots( field.Field.Height )}" );
                }
                else
                {
                    writer.Write( $"^GB{ToDots( field.Field.Width )},{config.Thickness}" );
                }

                writer.Write( $",{config.Thickness},{color}" );

                writer.WriteLine( "^FS" );

                return;
            }

            double left;
            double top;
            string direction;

            if ( field.Field.Width >= 0 && field.Field.Height >= 0 )
            {
                left = field.Field.Left;
                top = field.Field.Top;
                direction = "L";
            }
            else if ( field.Field.Width >= 0 && field.Field.Height < 0 )
            {
                left = field.Field.Left;
                top = field.Field.Top + field.Field.Height;
                direction = "R";
            }
            else if ( field.Field.Height >= 0 ) // Width is < 0
            {
                left = field.Field.Left + field.Field.Width;
                top = field.Field.Top;
                direction = "R";
            }
            else // Width < 0 && Height < 0
            {
                left = field.Field.Left + field.Field.Width;
                top = field.Field.Top + field.Field.Height;
                direction = "L";
            }

            var width = Math.Abs( field.Field.Width );
            var height = Math.Abs( field.Field.Height );

            WriteFieldOrigin( writer, left, top );

            writer.Write( $"^GD{ToDots( width )},{ToDots( height )},{config.Thickness},{color},{direction}" );

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes a rectangle field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteRectangleField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<RectangleFieldConfiguration>();

            WriteFieldOrigin( writer, field );

            writer.Write( $"^GB{ToDots( field.Field.Width )},{ToDots( field.Field.Height )}" );

            if ( config.IsFilled )
            {
                writer.Write( $",{ToDots( field.Field.Height )}" );
            }
            else
            {
                writer.Write( $",{config.BorderThickness}" );
            }

            writer.Write( config.IsBlack ? ",B" : ",W" );

            if ( config.CornerRadius >= 0 )
            {
                writer.Write( $",{config.CornerRadius}" );
            }

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes a ellipse field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteEllipseField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<EllipseFieldConfiguration>();

            WriteFieldOrigin( writer, field );

            writer.Write( $"^GE{ToDots( field.Field.Width )},{ToDots( field.Field.Height )}" );

            if ( config.IsFilled )
            {
                writer.Write( $",{ToDots( field.Field.Height )}" );
            }
            else
            {
                writer.Write( $",{config.BorderThickness}" );
            }

            writer.Write( config.IsBlack ? ",B" : ",W" );

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes an icon field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteIconField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<IconFieldConfiguration>();

            if ( config.Icon == null )
            {
                return;
            }

            var width = ToDots( field.Field.Width );
            var height = ToDots( field.Field.Height );

            ZplImageCache image;

            try
            {
                image = GetIcon( width, height, config.Icon );
            }
            catch
            {
                return;
            }

            WriteFieldOrigin( writer, field );

            if ( config.IsColorInverted )
            {
                writer.Write( "^FR" );
            }

            writer.Write( $"^GFA,{image.ImageData.Length},{image.ImageData.Length},{( width + 7 ) / 8}," );

            writer.Write( image.ZplContent );

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes an image field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteImageField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<ImageFieldConfiguration>();

            var width = ToDots( field.Field.Width );

            var options = new ZplImageOptions
            {
                Brightness = config.Brightness,
                Dithering = DitherMode.None,
                Height = ToDots( field.Field.Height ),
                Width = width
            };

            ZplImageCache image;

            try
            {
                image = GetImage( config.ImageData, config.ImageId?.ToString(), options );
            }
            catch
            {
                return;
            }

            WriteFieldOrigin( writer, field );

            if ( config.IsColorInverted )
            {
                writer.Write( "^FR" );
            }

            writer.Write( $"^GFA,{image.ImageData.Length},{image.ImageData.Length},{( width + 7 ) / 8}," );

            writer.Write( image.ZplContent );

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes an attendee photo field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteAttendeePhotoField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<AttendeePhotoFieldConfiguration>();
            var width = ToDots( field.Field.Width );

            var options = new ZplImageOptions
            {
                Dithering = config.IsHighQuality ? DitherMode.Quality : DitherMode.Fast,
                Height = ToDots( field.Field.Height ),
                Width = width
            };

            ZplImageCache image;

            try
            {
                image = GetPersonPhoto( options );
            }
            catch
            {
                return;
            }

            if ( image == null )
            {
                return;
            }

            WriteFieldOrigin( writer, field );

            if ( config.IsColorInverted )
            {
                writer.Write( "^FR" );
            }

            writer.Write( $"^GFA,{image.ImageData.Length},{image.ImageData.Length},{( width + 7 ) / 8}," );

            writer.Write( image.ZplContent );

            writer.WriteLine( "^FS" );
        }

        /// <summary>
        /// Writes a barcode field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected void WriteBarcodeField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<BarcodeFieldConfiguration>();
            string content = null;

            if ( config.IsDynamic && config.DynamicTextTemplate.IsNotNullOrWhiteSpace() )
            {
                if ( config.DynamicTextTemplate.IsLavaTemplate() )
                {
                    var mergeFields = PrintRequest.GetMergeFields();
                    content = config.DynamicTextTemplate.ResolveMergeFields( mergeFields );
                }
                else
                {
                    content = config.DynamicTextTemplate;
                }
            }
            else if ( PrintRequest.LabelData is ILabelDataHasPerson personData )
            {
                var searchTypeValueId = GetSearchTypeAlternateIdValueId();

                content = personData.Person.GetPersonSearchKeys( PrintRequest.RockContext )
                    .Where( psk => psk.SearchTypeValueId == searchTypeValueId )
                    .FirstOrDefault()
                    ?.SearchValue;
            }

            if ( content.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( config.Format == BarcodeFormat.Code128 )
            {
                WriteFieldOrigin( writer, field );

                writer.WriteLine( $"^BY3^BCN,{ToDots( field.Field.Height )},N^FD{content}^FS" );
            }
            else
            {
                WriteFieldOrigin( writer, field );

                using ( var generator = new QRCodeGenerator() )
                {
                    var qr = generator.CreateQrCode( content, QRCodeGenerator.ECCLevel.Q );

                    // Calculate the size of the QR code. This needs to fit
                    // inside the smaller of the width or height (since it will
                    // be a square).
                    var dotsMax = ToDots( Math.Min( field.Field.Width, field.Field.Height ) );
                    var dotsPerModule = dotsMax / qr.ModuleMatrix.Count;
                    var size = qr.ModuleMatrix.Count * dotsPerModule;

                    var grf = GetQrCodeAsGrf( qr, dotsPerModule, size );

                    // Add 7 and divide by 8 to make sure we are rounded up to
                    // a dot width in multiples of 8 to match the GRF format.
                    writer.Write( $"^GFA,{grf.Length},{grf.Length},{( size + 7 ) / 8}," );

                    writer.Write( ZplImageHelper.ByteArrayToHexViaLookup32( grf ) );
                }

                writer.WriteLine( "^FS" );
            }
        }

        /// <summary>
        /// Writes the field origin (<c>^FO</c>) marker and the proper values for
        /// the field.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="field">The field to use for the origin data.</param>
        private void WriteFieldOrigin( StreamWriter writer, LabelField field )
        {
            writer.Write( $"^FO{ToDots( field.Field.Left )},{ToDots( field.Field.Top )}" );
        }

        /// <summary>
        /// Writes the field origin (<c>^FO</c>) marker and the proper values for
        /// the field.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="left">The left position to use for the origin data.</param>
        /// <param name="top">The top position to use for the origin data.</param>
        private void WriteFieldOrigin( StreamWriter writer, double left, double top )
        {
            writer.Write( $"^FO{ToDots( left )},{ToDots( top )}" );
        }

        /// <summary>
        /// Converts the number of inches into a number of printer dots.
        /// </summary>
        /// <param name="valueInInches">The number expressed in inches.</param>
        /// <returns>The number of dots that equals <paramref name="valueInInches"/>.</returns>
        private int ToDots( double valueInInches )
        {
            return ( int ) Math.Floor( valueInInches * _dpi );
        }

        /// <summary>
        /// Gets the font size to use for the given text value length.
        /// </summary>
        /// <param name="textLength">The length of the text string to be rendered.</param>
        /// <param name="baseFontSize">The base font size that will be used if no adaptive size is found.</param>
        /// <param name="adaptiveFontSizes">The table of adaptive font sizes.</param>
        /// <returns>The font size to use.</returns>
        private double GetFontSize( int textLength, double baseFontSize, Dictionary<int, double> adaptiveFontSizes )
        {
            if ( adaptiveFontSizes != null )
            {
                var size = adaptiveFontSizes
                    .Where( a => textLength >= a.Key )
                    .OrderByDescending( a => a.Key )
                    .FirstOrDefault();

                if ( size.Value > 0 )
                {
                    return size.Value;
                }
            }

            return baseFontSize;
        }

        /// <summary>
        /// Gets the font size in dot units that the printer understands.
        /// </summary>
        /// <param name="fontSizeInPoints">The font size from the field.</param>
        /// <returns>The number of dots that equals <paramref name="fontSizeInPoints"/>.</returns>
        private int GetFontDotSize( double fontSizeInPoints )
        {
            if ( fontSizeInPoints < 1 )
            {
                fontSizeInPoints = 12;
            }

            // A font size of 72 should equal one inch.
            return ( int ) Math.Floor( fontSizeInPoints / 72.0 * _dpi );
        }

        /// <summary>
        /// Gets the image from the source by applying all the conversion values
        /// specified in the options.
        /// </summary>
        /// <param name="imageData">The source image data.</param>
        /// <param name="dataKey">The key to uniquely identify the image data amongst other label images.</param>
        /// <param name="options">The options to apply to the conversion process.</param>
        /// <returns>An instance of <see cref="ZplImageCache"/>.</returns>
        [ExcludeFromCodeCoverage]
        protected internal virtual ZplImageCache GetImage( byte[] imageData, string dataKey, ZplImageOptions options )
        {
            var dataHash = dataKey.IsNotNullOrWhiteSpace() ? dataKey : xxHashSharp.xxHash.CalculateHash( imageData ).ToString();
            var cacheKey = $"{dataHash}_{options.Width}_{options.Height}_{options.Brightness}_{options.Dithering}";

            return ZplImageCache.GetOrAddExisting( cacheKey, () =>
            {
                using ( var stream = new MemoryStream( imageData ) )
                {
                    return ZplImageHelper.CreateImage( stream, options );
                }
            } );
        }

        /// <summary>
        /// Gets the image for the icon.
        /// </summary>
        /// <param name="width">The width of the icon image.</param>
        /// <param name="height">The height of the icon image.</param>
        /// <param name="icon">The icon to use when creating the image.</param>
        /// <returns>An instance of <see cref="ZplImageCache"/>.</returns>
        [ExcludeFromCodeCoverage]
        protected internal virtual ZplImageCache GetIcon( int width, int height, LabelIcon icon )
        {
            var cacheKey = $"{icon.Value}_{width}_{height}";

            return ZplImageCache.GetOrAddExisting( cacheKey, () =>
            {
                return ZplImageHelper.CreateIcon( width, height, icon );
            } );
        }

        /// <summary>
        /// Gets the ZPL image for the photo of the person represented by
        /// this print request.
        /// </summary>
        /// <param name="options">The image formatting options.</param>
        /// <returns>The image object or <c>null</c> if no image was available.</returns>
        [ExcludeFromCodeCoverage]
        protected internal virtual ZplImageCache GetPersonPhoto( ZplImageOptions options )
        {
            if ( !( PrintRequest.LabelData is ILabelDataHasPerson personData ) )
            {
                return null;
            }

            var stream = personData.Person.Photo?.ContentStream;

            if ( stream == null )
            {
                return null;
            }

            return ZplImageHelper.CreateImage( stream, options );
        }

        /// <summary>
        /// Fills a single row of the QR Code into GRF formatted data that can
        /// be sent to the printer.
        /// </summary>
        /// <param name="row">The row data in the GRF array.</param>
        /// <param name="bitArray">The QR code bit array for the row.</param>
        /// <param name="dotsPerModule">The number of dots per module (bit) in the QR code.</param>
        private static void FillQrCodeRow( Span<byte> row, BitArray bitArray, int dotsPerModule )
        {
            var rowIndex = 0;
            var shift = 7;
            byte bits = 0;

            for ( int i = 0; i < bitArray.Length; i++ )
            {
                for ( var x = 0; x < dotsPerModule; x++ )
                {
                    if ( bitArray[i] )
                    {
                        bits = ( byte ) ( bits | ( 1 << shift ) );
                    }

                    if ( shift > 0 )
                    {
                        shift--;
                    }
                    else
                    {
                        row[rowIndex++] = bits;
                        bits = 0;
                        shift = 7;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a byte array that represents the QR code in GRF format ready
        /// to be sent to the printer.
        /// </summary>
        /// <param name="data">The QR Code data.</param>
        /// <param name="dotsPerModule">The number of dots per module (cell) in the QR code.</param>
        /// <param name="size">The size in dots of the rendered QR Code data, this represents both the width and height.</param>
        /// <returns>An array of bytes that contain the GRF data.</returns>
        private static byte[] GetQrCodeAsGrf( QRCodeData data, int dotsPerModule, int size )
        {
            var rowWidth = ( size + 7 ) / 8;
            var grf = new byte[rowWidth * size];
            var grfIndex = 0;

            for ( int r = 0; r < data.ModuleMatrix.Count; r++ )
            {
                FillQrCodeRow( new Span<byte>( grf, grfIndex, rowWidth ), data.ModuleMatrix[r], dotsPerModule );

                for ( int rowCopy = 1; rowCopy < dotsPerModule; rowCopy++ )
                {
                    Array.Copy( grf, grfIndex, grf, grfIndex + ( rowCopy * rowWidth ), rowWidth );
                }

                grfIndex += dotsPerModule * rowWidth;
            }

            return grf;
        }

        /// <summary>
        /// Gets the Alternate Id search key defined value identifier so we can
        /// perform lookups on the identifier to use for barcodes.
        /// </summary>
        /// <returns>The defined value identifier.</returns>
        [ExcludeFromCodeCoverage]
        protected internal virtual int GetSearchTypeAlternateIdValueId()
        {
            return DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid(), PrintRequest.RockContext ).Id;
        }
    }
}
