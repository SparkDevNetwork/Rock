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

using System;
using System.IO;
using System.Text;

namespace Rock.Utility.ExtensionMethods
{
    internal static class StreamExtensions
    {
        private const char CR = '\r';
        private const char LF = '\n';
        private const char NULL = ( char ) 0;

        /// <summary>
        /// Returns the number of lines in the given <paramref name="stream"/>.
        /// Most of this code was borrowed from here:
        /// https://github.com/NimaAra/Easy.Common/blob/master/Easy.Common/Extensions/StreamExtensions.cs#L13
        /// </summary>
        public static long CountLines( this Stream stream, Encoding encoding = default )
        {
            if ( stream == null )
            {
                throw new ArgumentNullException( nameof( stream ) );
            }

            var lineCount = 0L;
            var byteBuffer = new byte[1024 * 1024];
            var detectedEOL = NULL;
            var currentChar = NULL;
            int bytesRead;

            if ( encoding is null || Equals( encoding, Encoding.ASCII ) || Equals( encoding, Encoding.UTF8 ) )
            {
                while ( ( bytesRead = stream.Read( byteBuffer, 0, byteBuffer.Length ) ) > 0 )
                {
                    for ( var i = 0; i < bytesRead; i++ )
                    {
                        currentChar = ( char ) byteBuffer[i];

                        if ( detectedEOL != NULL )
                        {
                            if ( currentChar == detectedEOL )
                            {
                                lineCount++;
                            }
                        }
                        else if ( currentChar == LF || currentChar == CR )
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }
            else
            {
                var charBuffer = new char[byteBuffer.Length];

                while ( ( bytesRead = stream.Read( byteBuffer, 0, byteBuffer.Length ) ) > 0 )
                {
                    var charCount = encoding.GetChars( byteBuffer, 0, bytesRead, charBuffer, 0 );

                    for ( var i = 0; i < charCount; i++ )
                    {
                        currentChar = charBuffer[i];

                        if ( detectedEOL != NULL )
                        {
                            if ( currentChar == detectedEOL )
                            {
                                lineCount++;
                            }
                        }
                        else if ( currentChar == LF || currentChar == CR )
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }

            if ( currentChar != LF && currentChar != CR && currentChar != NULL )
            {
                lineCount++;
            }

            return lineCount;
        }
    }
}
