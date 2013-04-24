using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Security
{
    /// <summary>
    /// From http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
    /// </summary>
    public class Encryption
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes( "rsduYVC2leenXKTLYLkO9qsWU95HGCvWlbXcBTjtrj5dBJ7RPeGYiw7U3lZE+LWkT+jGrLP9deRMc8sUHJtc/wu2l4vANBx5f+p1zpRwQ2bB/E6Ta8k7haPiTRc4wYhrmWMrg8VfQ4MhAsSlijIfT9u+DszEkB2ba2k0FIPMSWk=" );

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        public static string EncryptString( string plainText )
        {
            if ( string.IsNullOrEmpty( plainText ) )
            {
                throw new ArgumentNullException( "plainText" );
            }

            string dataEncryptionKey = ConfigurationManager.AppSettings["DataEncryptionKey"];

            if ( string.IsNullOrEmpty( dataEncryptionKey ) )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            string outStr = null;
            RijndaelManaged aesAlg = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes( aesAlg.KeySize / 8 );

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor( aesAlg.Key, aesAlg.IV );

                // Create the streams used for encryption.
                using ( MemoryStream msEncrypt = new MemoryStream() )
                {
                    // prepend the IV
                    msEncrypt.Write( BitConverter.GetBytes( aesAlg.IV.Length ), 0, sizeof( int ) );
                    msEncrypt.Write( aesAlg.IV, 0, aesAlg.IV.Length );
                    using ( CryptoStream csEncrypt = new CryptoStream( msEncrypt, encryptor, CryptoStreamMode.Write ) )
                    {
                        using ( StreamWriter swEncrypt = new StreamWriter( csEncrypt ) )
                        {
                            //Write all data to the stream.
                            swEncrypt.Write( plainText );
                        }
                    }
                    outStr = Convert.ToBase64String( msEncrypt.ToArray() );
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if ( aesAlg != null )
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        public static string DecryptString( string cipherText )
        {
            if ( string.IsNullOrEmpty( cipherText ) )
            {
                throw new ArgumentNullException( "cipherText" );
            }

            string dataEncryptionKey = ConfigurationManager.AppSettings["DataEncryptionKey"];

            if ( string.IsNullOrEmpty( dataEncryptionKey ) )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String( cipherText );
                using ( MemoryStream msDecrypt = new MemoryStream( bytes ) )
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes( aesAlg.KeySize / 8 );
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray( msDecrypt );
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor( aesAlg.Key, aesAlg.IV );
                    using ( CryptoStream csDecrypt = new CryptoStream( msDecrypt, decryptor, CryptoStreamMode.Read ) )
                    {
                        using ( StreamReader srDecrypt = new StreamReader( csDecrypt ) )

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if ( aesAlg != null )
                    aesAlg.Clear();
            }

            return plaintext;
        }

        /// <summary>
        /// Reads the byte array.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <exception cref="System.SystemException">
        /// Stream did not contain properly formatted byte array
        /// or
        /// Did not read byte array properly
        /// </exception>
        private static byte[] ReadByteArray( Stream s )
        {
            byte[] rawLength = new byte[sizeof( int )];
            if ( s.Read( rawLength, 0, rawLength.Length ) != rawLength.Length )
            {
                throw new SystemException( "Stream did not contain properly formatted byte array" );
            }

            byte[] buffer = new byte[BitConverter.ToInt32( rawLength, 0 )];
            if ( s.Read( buffer, 0, buffer.Length ) != buffer.Length )
            {
                throw new SystemException( "Did not read byte array properly" );
            }

            return buffer;
        }
    }
}
