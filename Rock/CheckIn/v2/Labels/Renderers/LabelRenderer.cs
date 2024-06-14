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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Enums.CheckIn.Labels;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// <para>
    /// Renders a <see cref="CheckInLabel"/> of type <see cref="LabelFormat.Designed"/>
    /// to the output that can be sent to a printer.
    /// </para>
    /// <para>
    /// A renderer may be used multiple times. Implementations must cleanup any
    /// instance data in the <see cref="EndLabel"/> method.
    /// </para>
    /// </summary>
    internal interface ILabelRenderer : IDisposable
    {
        /// <summary>
        /// Starts writing a label to the stream. This is called at the start
        /// of each label to be printed.
        /// </summary>
        /// <param name="outputStream">The stream to write the contents of the label into.</param>
        /// <param name="printRequest">The object that describes the request to print the label.</param>
        void BeginLabel( Stream outputStream, PrintLabelRequest printRequest );

        /// <summary>
        /// Writes a single field out to the stream that was passed to
        /// <see cref="BeginLabel(Stream, PrintLabelRequest)"/>.
        /// </summary>
        /// <param name="field">The field to be written.</param>
        void WriteField( LabelField field );

        /// <summary>
        /// Called after all fields have been written. The renderer should
        /// perform any final output to the stream and then cleanup any
        /// instance data.
        /// </summary>
        void EndLabel();
    }

    /// <summary>
    /// Helper renderer implementation when the output is going to be formatted
    /// as UTF-8 text data.
    /// </summary>
    internal abstract class TextLabelRenderer : ILabelRenderer
    {
        #region Fields

        /// <summary>
        /// A UTF-8 encoding with the Byte-Order-Marker disabled.
        /// </summary>
        private static readonly UTF8Encoding UTF8EncodingWithoutBom = new UTF8Encoding( false );

        /// <summary>
        /// The text stream writer currently in use for the label.
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// <c>true</c> if we have already been disposed.
        /// </summary>
        private bool _isDisposed;

        #endregion

        #region Properties

        /// <summary>
        /// The current print request for the label being printed.
        /// </summary>
        protected PrintLabelRequest PrintRequest { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void BeginLabel( Stream outputStream, PrintLabelRequest printRequest )
        {
            if ( _writer != null )
            {
                throw new InvalidOperationException( "Previous label was never closed." );
            }

            _writer = new StreamWriter( outputStream, UTF8EncodingWithoutBom, 4096, true );
            PrintRequest = printRequest;

            BeginLabel( _writer );
        }

        /// <inheritdoc/>
        public void WriteField( LabelField field )
        {
            WriteField( _writer, field );
        }

        /// <inheritdoc/>
        public void EndLabel()
        {
            EndLabel( _writer );

            _writer.Flush();
            _writer.Dispose();
            _writer = null;
        }

        /// <summary>
        /// Starts writing a label to the stream. This is called at the start
        /// of each label to be printed.
        /// </summary>
        /// <param name="writer">The stream to write the contents of the label into.</param>
        protected abstract void BeginLabel( StreamWriter writer );

        /// <summary>
        /// Writes a single field out to the stream.
        /// </summary>
        /// <param name="writer">The stream to write the contents of the label into.</param>
        /// <param name="field">The field to be written.</param>
        protected abstract void WriteField( StreamWriter writer, LabelField field );

        /// <summary>
        /// Called after all fields have been written. The renderer should
        /// perform any final output to the stream and then cleanup any
        /// instance data.
        /// </summary>
        /// <param name="writer">The stream to write the contents of the label into.</param>
        protected abstract void EndLabel( StreamWriter writer );

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">If <c>true</c> then any managed objects should be disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !_isDisposed )
            {
                if ( disposing )
                {
                    _writer.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }

        #endregion
    }

    /// <summary>
    /// Renders labels in the ZPL format for Zebra label printers.
    /// </summary>
    internal class ZplRenderer : TextLabelRenderer
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
                throw new NotImplementedException();
            }
            else if ( field.Field.FieldType == LabelFieldType.Rectangle )
            {
                WriteRectangleField( writer, field );
            }
            else if ( field.Field.FieldType == LabelFieldType.Ellipse )
            {
                throw new NotImplementedException();
            }
            else if ( field.Field.FieldType == LabelFieldType.Icon )
            {
                throw new NotImplementedException();
            }
            else if ( field.Field.FieldType == LabelFieldType.Image )
            {
                throw new NotImplementedException();
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
    }
}
