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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.CloudPrint.Shared
{
    /// <summary>
    /// A helper class for reading and writing objects to a pipe stream.
    /// </summary>
    internal class PipeObjectStream
    {
        /// <summary>
        /// The underlying stream object.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// The encoding to use when reading and writing text.
        /// </summary>
        private readonly Encoding _encoding;

        /// <summary>
        /// Initializes a new instance of the PipeStream class with the specified input/output Stream.
        /// </summary>
        /// <param name="ioStream">The input/output Stream to use with the PipeStream.</param>
        public PipeObjectStream( Stream ioStream )
        {
            _stream = ioStream;
            _encoding = new UTF8Encoding();
        }

        /// <summary>
        /// Reads data of type T.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the read operation.</param>
        /// <returns>A task representing the asynchronous operation that returns the read data of type T.</returns>
        public async Task<T> ReadAsync<T>( CancellationToken cancellationToken = default )
        {
            var lenOffset = 0;
            var lenBuffer = new byte[sizeof( int )];

            while ( lenOffset < lenBuffer.Length )
            {
                var n = await _stream.ReadAsync( lenBuffer, lenOffset, lenBuffer.Length - lenOffset, cancellationToken );

                if ( n == 0 )
                {
                    throw new IOException();
                }

                lenOffset += n;
            }

            var len = BitConverter.ToInt32( lenBuffer, 0 );

            var offset = 0;
            var buffer = new byte[len];

            while ( offset < buffer.Length )
            {
                var n = await _stream.ReadAsync( buffer, offset, buffer.Length - offset, cancellationToken );

                if ( n == 0 )
                {
                    throw new IOException();
                }

                offset += n;
            }

            var json = _encoding.GetString( buffer );
            var result = JsonSerializer.Deserialize<T>( json );

            return result == null
                ? throw new Exception( "Invalid object received." )
                : result;
        }

        /// <summary>
        /// Asynchronously writes the specified data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public async Task WriteAsync( object data, CancellationToken cancellationToken = default )
        {
            var json = JsonSerializer.Serialize( data );
            var len = _encoding.GetByteCount( json );
            var buffer = new byte[len + sizeof( int )];
            var lenBytes = BitConverter.GetBytes( len );

            Array.Copy( lenBytes, buffer, lenBytes.Length );
            _encoding.GetBytes( json, 0, json.Length, buffer, sizeof( int ) );

            await _stream.WriteAsync( buffer, 0, buffer.Length, cancellationToken );
            await _stream.FlushAsync( cancellationToken );
        }
    }
}
