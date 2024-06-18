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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Rock.Enums.CheckIn.Labels;
using Rock.ViewModels.CheckIn.Labels;

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
                throw new NotImplementedException();
            }
            else if ( field.Field.FieldType == LabelFieldType.Image )
            {
                WriteImageField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.AttendeePhoto )
            {
                throw new NotImplementedException();
            }
            else if ( field.Field.FieldType == LabelFieldType.Barcode )
            {
                throw new NotImplementedException();
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
        protected internal void WriteTextField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<TextFieldConfiguration>();
            var values = field.GetFormattedValues( PrintRequest );

            string textValue;

            if ( config.CollectionFormat == TextCollectionFormat.FirstItemOnly )
            {
                textValue = values[0];
            }
            else if ( config.CollectionFormat == TextCollectionFormat.CommaDelimited )
            {
                textValue = string.Join( ", ", values );
            }
            else if ( config.CollectionFormat == TextCollectionFormat.OnePerLine )
            {
                textValue = string.Join( "\\&", values );
            }
            else
            {
                return;
            }

            var fontSize = GetFontDotSize( config.FontSize );

            WriteFieldOrigin( writer, field );

            if ( config.IsColorInverted )
            {
                writer.Write( "^FR" );
            }

            var lineCount = Math.Max( 1, ( int ) Math.Round( ToDots( field.Field.Height ) / ( double ) fontSize ) );

            writer.WriteLine( $"^FB{ToDots( field.Field.Width )},{lineCount}^A0,{fontSize},{fontSize}^FD{textValue}^FS" );
        }

        /// <summary>
        /// Writes a line field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected internal void WriteLineField( StreamWriter writer, LabelField field )
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
        protected internal void WriteRectangleField( StreamWriter writer, LabelField field )
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
        protected internal void WriteEllipseField( StreamWriter writer, LabelField field )
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
        /// Writes an image field to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the field to.</param>
        /// <param name="field">The field to write.</param>
        protected internal void WriteImageField( StreamWriter writer, LabelField field )
        {
            var config = field.GetConfiguration<ImageFieldConfiguration>();

            var width = ToDots( field.Field.Width );

            var options = new ZplImageOptions
            {
                Brightness = config.Brightness,
                Contrast = config.Contrast,
                Dithering = DitherMode.None,
                Height = ToDots( field.Field.Height ),
                Width = width
            };

            ZplImageCache image;

            try
            {
                image = GetImage( config.ImageData, options, false );
            }
            catch
            {
                return;
            }

            WriteFieldOrigin( writer, field );

            if ( config.IsInverted )
            {
                writer.Write( "^FR" );
            }

            writer.Write( $"^GFA,{image.ImageData.Length},{image.ImageData.Length},{( width + 7 ) / 8}," );

            foreach ( var b in image.ImageData )
            {
                writer.Write( $"{b:X2}" );
            }

            writer.WriteLine( "^FS" );
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
        /// <param name="options">The options to apply to the conversion process.</param>
        /// <param name="ignoreCache"><c>true</c> if the cache should be ignored.</param>
        /// <returns>An instance of <see cref="ZplImageCache"/>.</returns>
        [ExcludeFromCodeCoverage]
        protected internal virtual ZplImageCache GetImage( byte[] imageData, ZplImageOptions options, bool ignoreCache )
        {
            using ( var stream = new MemoryStream( imageData ) )
            {
                return ZplImageHelper.CreateImage( stream, options );
            }
        }
    }
}
