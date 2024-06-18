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
using System.IO;
using System.Text;

namespace Rock.CheckIn.v2.Labels.Renderers
{
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

            _writer.Flush();
        }

        /// <inheritdoc/>
        public void WriteField( LabelField field )
        {
            WriteField( _writer, field );

            _writer.Flush();
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
                    _writer?.Dispose();
                    _writer = null;
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
}
