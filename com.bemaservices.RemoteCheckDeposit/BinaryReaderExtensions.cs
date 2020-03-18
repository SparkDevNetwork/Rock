using System.IO;
using System.Text;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Provides extra methods to the BinaryReader class.
    /// </summary>
    static public class BinaryReaderExtensions
    {
        /// <summary>
        /// Peek ahead at an Ebcdic string without moving the position in the reader.
        /// </summary>
        /// <param name="reader">The reader that will be read from.</param>
        /// <param name="length">The number of characters to peek ahead.</param>
        /// <returns>A string that contains the ASCII encoded string represented by the data in the reader.</returns>
        public static string PeekEbcdicString( this BinaryReader reader, int length )
        {
            var str = reader.ReadEbcdicString( length );

            reader.BaseStream.Position -= length;

            return str;
        }

        /// <summary>
        /// Read an Ebcdic encoded string from the reader.
        /// </summary>
        /// <param name="reader">The reader that will be read from.</param>
        /// <param name="length">The number of characters to peek ahead.</param>
        /// <returns>A string that contains the ASCII encoded string represented by the data in the reader.</returns>
        public static string ReadEbcdicString( this BinaryReader reader, int length )
        {
            var bytes = reader.ReadBytes( length );

            return Encoding.ASCII.GetString( Encoding.Convert( Encoding.GetEncoding( "IBM037" ), Encoding.ASCII, bytes, 0, length ) );
        }
    }
}
