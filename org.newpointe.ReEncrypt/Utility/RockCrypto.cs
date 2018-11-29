using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace org.newpointe.ReEncrypt.Utility
{
    public static class RockCrypto
    {

        /// <summary>
        /// Rock's hardcoded KDF salt.
        /// </summary>
        private static byte[] _rock_static_kdf_salt = Encoding.ASCII.GetBytes( "rsduYVC2leenXKTLYLkO9qsWU95HGCvWlbXcBTjtrj5dBJ7RPeGYiw7U3lZE+LWkT+jGrLP9deRMc8sUHJtc/wu2l4vANBx5f+p1zpRwQ2bB/E6Ta8k7haPiTRc4wYhrmWMrg8VfQ4MhAsSlijIfT9u+DszEkB2ba2k0FIPMSWk=" );
        
        /// <summary>
        /// Gets the "DataEncryptionKey" app setting.
        /// </summary>
        public static string GetConfiguredEncryptionPassword()
        {
            return ConfigurationManager.AppSettings["DataEncryptionKey"];
        }

        /// <summary>
        /// Sets the "DataEncryptionKey" app setting. Will cause an application restart.
        /// </summary>
        public static void SetConfiguredEncryptionPassword( string newEncryptionPassword )
        {
            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            config.AppSettings.Settings["DataEncryptionKey"].Value = newEncryptionPassword;
            config.Save();
        }

        /// <summary>
        /// Generates a 256 bit encryption key from a password string. Uses PBKDF2 with a static salt.
        /// </summary>
        /// <param name="encryptionKeyString">The key string.</param>
        /// <param name="keyLength">The desired key length in bytes.</param>
        /// <returns></returns>
        public static byte[] GetDerivedKey( string encryptionPassword )
        {
            return GetDerivedKey( encryptionPassword, 256 / 8 );
        }

        /// <summary>
        /// Generates an encryption key from a password string. Uses PBKDF2 with a static salt.
        /// </summary>
        /// <param name="encryptionKeyString">The key string.</param>
        /// <param name="keyLength">The desired key length in bytes.</param>
        /// <returns></returns>
        public static byte[] GetDerivedKey( string encryptionPassword, int keyLength )
        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( encryptionPassword, _rock_static_kdf_salt );
            return key.GetBytes( keyLength );
        }


        /// <summary>
        /// Encrypts a string. Uses the Rijndael algorithm in CBC mode with a 256 bit key size, 128 bit block size, and a random IV. The IV is prepended to the ciphertext as IV Length + IV + CipherText. This is the same as Rock's internal encryption method.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="password">The encryption password.</param>
        /// <returns></returns>
        public static string EncryptString( string plaintext, string password )
        {
            return EncryptString( plaintext, GetDerivedKey( password ) );
        }

        /// <summary>
        /// Encrypts a string. Uses the Rijndael algorithm in CBC mode with a 256 bit key size, 128 bit block size, and a random IV. The IV is prepended to the ciphertext as IV Length + IV + CipherText. This is the same as Rock's internal encryption method.
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns></returns>
        public static string EncryptString( string plaintext, byte[] encryptionKey )
        {

            // Set up a new Rijndael
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {

                // Set the key
                rijndael.Key = encryptionKey;

                // Create a memory stream to hold the results.
                using (MemoryStream resultStream = new MemoryStream())
                {

                    // Prepend the IV Length
                    resultStream.Write( BitConverter.GetBytes( rijndael.IV.Length ), 0, sizeof( int ) );

                    // Prepend the IV
                    resultStream.Write( rijndael.IV, 0, rijndael.IV.Length );

                    // Create an encryptor
                    ICryptoTransform encryptor = rijndael.CreateEncryptor( rijndael.Key, rijndael.IV );

                    // Create the crypto stream
                    using (CryptoStream encryptionStream = new CryptoStream( resultStream, encryptor, CryptoStreamMode.Write ))
                    {

                        // Create a writer for the crypto stream
                        using (StreamWriter encryptionStreamWriter = new StreamWriter( encryptionStream ))
                        {

                            // Write all data to the stream.
                            encryptionStreamWriter.Write( plaintext );

                        }

                    }

                    // Return the results as a base64 string.
                    return Convert.ToBase64String( resultStream.ToArray() );

                }

            }

        }

        /// <summary>
        /// Decrypts a string. Uses the Rijndael algorithm in CBC mode with a 256 bit key size, 128 bit block size, and a random IV. The IV is prepended to the ciphertext as IV Length + IV + CipherText. This is the same as Rock's internal decryption method.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="password">The encryption password.</param>
        /// <returns></returns>
        public static string DecryptString( string ciphertext, string password )
        {
            return DecryptString( ciphertext, GetDerivedKey( password ) );
        }

        /// <summary>
        /// Decrypts a string. Uses the Rijndael algorithm in CBC mode with a 256 bit key size, 128 bit block size, and a random IV. The IV is prepended to the ciphertext as IV Length + IV + CipherText. This is the same as Rock's internal decryption method.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="password">The encryption password.</param>
        /// <returns></returns>
        public static string DecryptString( string ciphertext, byte[] encryptionKey )
        {

            // Set up a new Rijndael
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {

                // Set the key
                rijndael.Key = encryptionKey;

                // Get the ciphertext bytes
                byte[] bytes = Convert.FromBase64String( ciphertext );

                // Create a memory stream to hold the ciphertext.
                using (MemoryStream cipherStream = new MemoryStream( bytes ))
                {

                    // Read the IV Length
                    byte[] rawIVLength = ReadBytes( cipherStream, sizeof( int ) );

                    // Read the IV
                    rijndael.IV = ReadBytes( cipherStream, BitConverter.ToInt32( rawIVLength, 0) );

                    // Create a decryptor
                    ICryptoTransform decryptor = rijndael.CreateDecryptor( rijndael.Key, rijndael.IV );

                    // Create the crypto stream
                    using (CryptoStream decryptionStream = new CryptoStream( cipherStream, decryptor, CryptoStreamMode.Read ))
                    {

                        // Create a reader for the crypto stream
                        using (StreamReader decryptionStreamReader = new StreamReader( decryptionStream ))
                        {

                            // Read all data from the stream.
                            return decryptionStreamReader.ReadToEnd();

                        }

                    }

                }

            }

        }

        public static byte[] ReadBytes( Stream stream, int length )
        {
            byte[] results = new byte[length];

            if (stream.Read( results, 0, length ) != length)
            {
                throw new SystemException( "Failed to read bytes from stream." );
            }

            return results;

        }

        /// <summary>
        /// Generates the encryption key.
        /// </summary>
        /// <param name="length">The number of bytes to generate.</param>
        /// <returns></returns>
        public static byte[] GenerateRandomBytes( int length )
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[length];

            rng.GetBytes( randomBytes );

            return randomBytes;
        }

        /// <summary>
        /// Generates the encryption key.
        /// </summary>
        /// <param name="length">The number of bytes to generate (not the length of the resulting string).</param>
        /// <returns></returns>
        public static string GenerateRandomBase64String( int length )
        {
            return Convert.ToBase64String( GenerateRandomBytes( length ) );
            
        }

        /// <summary>
        /// Generates the encryption key.
        /// </summary>
        /// <param name="length">The number of bytes to generate (not the length of the resulting string).</param>
        /// <returns></returns>
        public static string GenerateRandomHexString( int length )
        {
            byte[] buffer = GenerateRandomBytes( length );

            StringBuilder builder = new StringBuilder( buffer.Length * 2 );

            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append( string.Format( "{0:X2}", buffer[i] ) );
            }

            return builder.ToString();
        }


    }

}
