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
using System.IO.Compression;
using System.Text;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// Provides helper methods to deal with Z64 formatted data. This is
    /// essentially just Base64 encoded, then LZ77 compressed, and then
    /// CRC-32 checksum. But everything is in a slightly custom format so
    /// we can't use standard libraries.
    /// </summary>
    /// <remarks>
    /// Most of this was cobbled together from various open source MIT
    /// licensed projects. Some in C#, some in other languages.
    /// </remarks>
    internal static class Zebra64
    {
        /// <summary>
        /// CRC-16 transition table for an initial value of 0.
        /// </summary>
        private static readonly uint[] CrcTable =
        {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
            0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
            0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
            0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
            0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
            0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
            0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
            0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
            0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
            0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
            0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
            0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
            0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
            0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
            0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
            0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
            0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
            0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
            0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
            0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
            0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
            0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
            0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
            0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
            0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
            0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
            0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
        };

        private const uint ADLER32_BASE = 65521;
        private const int ADLER32_NMAX = 5552;

        /// <summary>
        /// Decompresses a Z64 data string into a byte array of pixel data.
        /// </summary>
        /// <param name="compressedString">The Z64 data string, including the ':Z64:' prefix and the ':####' CRC suffix.</param>
        /// <returns>The decoded GRF pixel data or <c>null</c> if it could not be decoded.</returns>
        public static byte[] Decompress( string compressedString )
        {
            var chunks = compressedString.Split( ':' );
            var b64 = Convert.FromBase64String( chunks[2] );

            try
            {
                using ( var decompressedStream = new MemoryStream() )
                {
                    using ( var memoryStream = new MemoryStream( b64 ) )
                    {
                        // Skip unsupported ZLIB header bytes.
                        memoryStream.ReadByte();
                        memoryStream.ReadByte();

                        using ( var deflateStream = new DeflateStream( memoryStream, CompressionMode.Decompress ) )
                        {
                            deflateStream.CopyTo( decompressedStream );
                        }

                        return decompressedStream.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Compresses a byte array of pixel data into a Z64 data string.
        /// </summary>
        /// <param name="data">The GRF pixel data to be compressed.</param>
        /// <returns>A Base64 encoded representation of the compressed data with the ':Z64:' prefix or ':####' CRC suffix.</returns>
        public static string Compress( byte[] data )
        {
            using ( var compressedStream = new MemoryStream() )
            {
                // This first byte is the ZLib header for CM (Compression
                // Method) and CINFO (Compression Info).
                // 0x08 = CM = Deflate
                // 0x70 = CINFO = 32K window size
                compressedStream.WriteByte( 0x78 );

                // This second byte is also the ZLib header. It is a check
                // bits value as well as some other flags. Basically, we
                // just hard code it to a most likely correct value.
                compressedStream.WriteByte( 0x9C );
                using ( var deflateStream = new DeflateStream( compressedStream, CompressionMode.Compress, true ) )
                {
                    deflateStream.Write( data, 0, data.Length );
                }

                // Compute the Adler-32 checksum of the uncompressed data then
                // write it to the stream in network byte order.
                var adler = Adler32( data );
                adler = ( uint ) System.Net.IPAddress.HostToNetworkOrder( ( int ) adler );
                compressedStream.Write( BitConverter.GetBytes( adler ), 0, 4 );

                var b64 = Convert.ToBase64String( compressedStream.ToArray() );
                var crc = Crc16( Encoding.ASCII.GetBytes( b64 ) );

                return $":Z64:{b64}:{crc:X4}";
            }
        }

        /// <summary>
        /// Calculate a CRC-16 CITT checksum on the data with an initial value of 0.
        /// </summary>
        /// <param name="bytes">The data to processed.</param>
        /// <returns>A 16-bit integer with the CRC-16 checksum.</returns>
        public static ushort Crc16( byte[] bytes )
        {
            ushort crc = 0;

            for ( int i = 0; i < bytes.Length; ++i )
            {
                crc = ( ushort ) ( ( crc << 8 ) ^ CrcTable[( ( crc >> 8 ) ^ ( 0xff & bytes[i] ) )] );
            }

            return crc;
        }

        /// <summary>
        /// Calculate the Adler-32 checksum of the data.
        /// </summary>
        /// <param name="bytes">The data to be calculated.</param>
        /// <returns>The 32-bit unsigned integer representing the checksum value.</returns>
        public static uint Adler32( byte[] bytes )
        {
            uint sum2;
            uint n;
            var len = bytes.Length;
            var pos = 0;
            var value = 1U;

            // Split Adler-32 into component sums.
            sum2 = ( value >> 16 ) & 0xFFFF;
            value &= 0xFFFF;

            // Process the data in chunks of ADLER32_NMAX bytes.
            while ( len >= ADLER32_NMAX )
            {
                len -= ADLER32_NMAX;

                for ( n = ADLER32_NMAX / 16; n > 0; --n )
                {
                    for ( int i = 0; i < 16; i++ )
                    {
                        value += bytes[pos++];
                        sum2 += value;
                    }
                }

                value %= ADLER32_BASE;
                sum2 %= ADLER32_BASE;
            }

            // Process the remaining bytes that were smaller than ADLER32_NMAX.
            if ( len > 0 )
            {
                while ( len >= 16 )
                {
                    len -= 16;

                    for ( int i = 0; i < 16; i++ )
                    {
                        value += bytes[pos++];
                        sum2 += value;
                    }
                }

                while ( len-- > 0 )
                {
                    value += bytes[pos++];
                    sum2 += value;
                }

                value %= ADLER32_BASE;
                sum2 %= ADLER32_BASE;
            }

            // Put the two sums together into a single 32-bit number.
            value |= ( sum2 << 16 );

            return value;
        }
    }
}
