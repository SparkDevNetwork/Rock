using System.IO;
using System.Text;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Provides extra methods to the BinaryWriter class.
    /// </summary>
    static public class BinaryWriterExtensions
    {
        /// <summary>
        /// Write a string with Ebcdic encoding to the writer.
        /// </summary>
        /// <param name="reader">The reader that will be read from.</param>
        /// <param name="length">The number of characters to peek ahead.</param>
        /// <returns>A string that contains the ASCII encoded string represented by the data in the reader.</returns>
        public static void WriteEbcdicString( this BinaryWriter writer, string str )
        {
            var bytes = Encoding.Convert( Encoding.ASCII, Encoding.GetEncoding( "IBM037" ), Encoding.ASCII.GetBytes( str ) );

            writer.Write( bytes );
        }
    }
}
