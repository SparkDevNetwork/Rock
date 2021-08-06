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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.IdentityModel.Tokens;

namespace Rock.Security
{
    /// <summary>
    /// From http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
    /// </summary>
    public class Encryption
    {
        private static byte[] _salt = Encoding.ASCII.GetBytes( "rsduYVC2leenXKTLYLkO9qsWU95HGCvWlbXcBTjtrj5dBJ7RPeGYiw7U3lZE+LWkT+jGrLP9deRMc8sUHJtc/wu2l4vANBx5f+p1zpRwQ2bB/E6Ta8k7haPiTRc4wYhrmWMrg8VfQ4MhAsSlijIfT9u+DszEkB2ba2k0FIPMSWk=" );

        // The byte array used for the AES key to encrypt/decrypt a string
        private static byte[] _dataEncryptionKeyBytes
        {
            get
            {
                if ( _dataEncryptionKeyBytesBacker == null )
                {
                    var encryptionKey = GetDataEncryptionKey();
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( encryptionKey, _salt );

                    // The 32 is the default value for RijndaelManaged.KeySize (256) divided by 8
                    _dataEncryptionKeyBytesBacker = key.GetBytes( 32 );
                }

                return _dataEncryptionKeyBytesBacker;
            }
        }

        private static byte[] _dataEncryptionKeyBytesBacker;

        // A collection of old keys used to encrypt/descrypt a string. These should only be used for reads if the current key failed.
        private static Dictionary<string,byte[]> _oldDataEncryptionKeyBytes
        {
            get
            {
                if ( _oldDataEncryptionKeyBytesBacker.IsNull() )
                {
                    /* 2021-06-07 ETD
                     * Thred safety fix:
                     * There is a possible edge case where a second call to this prop could occur while the first is still populating the dictionary, which means the second caller would only get an empty or partial list.
                     * To prevent this the backer has to have all the data once it is no longer null. This prop uses a temp dictionary and populates it once complete the backer is set to the temp dictionary.
                     * After being set the backer is no longer null and has a complete collection.
                     * 
                     * For our edge case scenario the second caller would simply end up duplicating the effort but it would get back the correct data.
                     */
                    var tempDictionary = new Dictionary<string, byte[]>();

                    int i = 0;
                    var appSettingKey = "OldDataEncryptionKey";
                    var dataEncryptionKey = ConfigurationManager.AppSettings[appSettingKey];
                    while ( !string.IsNullOrWhiteSpace( dataEncryptionKey ) )
                    {
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );
                        tempDictionary.Add( appSettingKey, key.GetBytes( 32 ) );

                        i++;
                        appSettingKey = $"OldDataEncryptionKey{i}";
                        dataEncryptionKey = ConfigurationManager.AppSettings[appSettingKey];
                    }

                    _oldDataEncryptionKeyBytesBacker = tempDictionary;
                }

                return _oldDataEncryptionKeyBytesBacker;
            }
        }

        private static Dictionary<string, byte[]> _oldDataEncryptionKeyBytesBacker;

        /// <summary>
        /// Tries to encrypt the string. Use this in situations where you might just want to skip encryption if it doesn't work.  
        /// You should use EncryptString in most cases.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="cypherText">The cypher text.</param>
        /// <returns></returns>
        public static bool TryEncryptString(string plainText, out string cypherText)
        {
            cypherText = null;
            
            string encryptionKey = Encryption.GetDataEncryptionKey();

            // non-web apps might not have the DataEncryptionKey, so check that first since it could happen quite a bit
            if ( string.IsNullOrWhiteSpace( encryptionKey ) )
            {
                return false;
            }
            else
            {
                try
                {
                    cypherText = EncryptString( plainText );
                    return true;
                }
                catch
                { 
                    // intentionally ignore exception since we are a try method
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the data encryption key.
        /// </summary>
        /// <returns></returns>
        private static string GetDataEncryptionKey()
        {
            return ConfigurationManager.AppSettings["DataEncryptionKey"];
        }

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptString().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        public static string EncryptString( string plainText )
        {
            return EncryptString( plainText, _dataEncryptionKeyBytes );
        }

        private static string EncryptString( string plainText, byte[] keyBytes )
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            if ( keyBytes.IsNull() || keyBytes.Length == 0 )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            string outStr = null;
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = keyBytes;

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
        /// Encrypts the string.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="dataEncryptionKey">The data encryption key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">DataEncryptionKey must be specified in configuration file</exception>
        [RockObsolete( "1.13" )]
        [Obsolete("Do not use this method. Use the override without the dataEncryption key. That method will get the key from the web.config and store the computed key which will make subsequent encrypts faster by 10-15ms. This method will compute the key each time and will be much slower.")]
        public static string EncryptString( string plainText, string dataEncryptionKey )
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            if ( string.IsNullOrEmpty( dataEncryptionKey ) )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            string outStr = null;
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );
                var keyBytes = key.GetBytes( aesAlg.KeySize / 8 );
                aesAlg.Key = keyBytes;

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
        /// EncryptString(), using an identical sharedSecret.
        /// </summary>
        /// <returns>
        ///  decrypted string ; otherwise, null.
        /// </returns>
        /// <param name="cipherText">The text to decrypt.</param>
        public static string DecryptString( string cipherText )
        {
            string plainText = null;

            try
            {
                plainText = DecryptString( cipherText, _dataEncryptionKeyBytes );
            }
            catch
            {
                // Intentionally left blank
            }

            if ( plainText != null )
            {
                return plainText;
            }

            // Try old decryption keys
            if ( _oldDataEncryptionKeyBytes.IsNotNull() )
            {
                foreach ( var oldDataEncryptionKeyBytes in _oldDataEncryptionKeyBytes )
                {
                    try
                    {
                        plainText = DecryptString( cipherText, oldDataEncryptionKeyBytes.Value );
                        if ( plainText.IsNotNullOrWhiteSpace() )
                        {
                            return plainText;
                        }
                    }
                    catch
                    {
                        // Intentionally left blank
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="dataEncryptionKey">The data encryption key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">DataEncryptionKey must be specified in configuration file</exception>
        [Obsolete( "Use the overload without the dataEncryptionKey param. It will cycle through all of the DataEncryptionKeys in the web.config until it works or there are none left and store the computed key which will subsequent decrypts faster by 10-15ms. This method will compute the key each time and will be much slower." )]
        [RockObsolete( "1.13" )]
        public static string DecryptString( string cipherText, string dataEncryptionKey )
        {
            if ( string.IsNullOrEmpty( cipherText ) )
            {
                return string.Empty;
            }

            if ( string.IsNullOrEmpty( dataEncryptionKey ) )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();

                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );
                var keyBytes = key.GetBytes( aesAlg.KeySize / 8 );

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String( cipherText );
                using ( MemoryStream msDecrypt = new MemoryStream( bytes ) )
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = keyBytes;
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
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="keyBytes">The key bytes.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">DataEncryptionKey must be specified in configuration file</exception>
        private static string DecryptString( string cipherText, byte[] keyBytes )
        {
            if ( string.IsNullOrEmpty( cipherText ) )
            {
                return string.Empty;
            }

            if ( keyBytes == null || keyBytes.Length == 0 )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            string plaintext = null;
            RijndaelManaged aesAlg = null;

            try
            {
                // Create the streams used for decryption.
                using ( MemoryStream msDecrypt = new MemoryStream( Convert.FromBase64String( cipherText ) ) )
                {
                    // Create a RijndaelManaged object with the specified key and the IV from the encrypted stream.
                    aesAlg = new RijndaelManaged
                    {
                        Key = keyBytes,
                        IV = ReadByteArray( msDecrypt )
                    };

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor( aesAlg.Key, aesAlg.IV );

                    // Read the decrypted bytes from the decrypting stream and place them in a string.
                    using ( CryptoStream csDecrypt = new CryptoStream( msDecrypt, decryptor, CryptoStreamMode.Read ) )
                    using ( StreamReader srDecrypt = new StreamReader( csDecrypt ) )
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                // Intentionally ignore so the caller can try another key when it gets a null value back.
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlg?.Clear();
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

        /// <summary>
        /// Gets the SHA1 hash.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Account encoding requires a 'PasswordKey' app setting</exception>
        public static string GetSHA1Hash(string plainText)
        {
            string passwordKey = ConfigurationManager.AppSettings["PasswordKey"];
            if ( String.IsNullOrWhiteSpace( passwordKey ) )
            {
                throw new ConfigurationErrorsException( "Account encoding requires a 'PasswordKey' app setting" );
            }

            byte[] encryptionKey = HexToByte( passwordKey );

            HMACSHA1 hash = new HMACSHA1();
            hash.Key = encryptionKey;

            return Convert.ToBase64String( hash.ComputeHash( Encoding.Unicode.GetBytes( plainText ) ) );
        }

        /// <summary>
        /// converts a hexadecimal string to byte.
        /// </summary>
        /// <param name="hexString">The hexadecimal string.</param>
        /// <returns></returns>
        public static byte[] HexToByte( string hexString )
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for ( int i = 0; i < returnBytes.Length; i++ )
            {
                returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
            }

            return returnBytes;
        }

        /// <summary>
        /// Generates the machine key.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GenerateMachineKey( int length )
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] buff = new byte[length];
            rngCsp.GetBytes( buff );
            StringBuilder sb = new StringBuilder( buff.Length * 2 );
            for ( int i = 0; i < buff.Length; i++ )
                sb.Append( string.Format( "{0:X2}", buff[i] ) );
            return sb.ToString();
        }

        /// <summary>
        /// Generates the encryption key.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GenerateEncryptionKey( int length )
        {
            var rng = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            byte[] randomBytes = new byte[length];
            rng.GetNonZeroBytes( randomBytes );
            string dataEncryptionKey = Convert.ToBase64String( randomBytes );

            return dataEncryptionKey;
        }

        /// <summary>
        /// Generates the unique token which can be used to create the token for a PersonToken
        /// NOTE: Use PersonToken.CreateNew to get this as a usable rckipid
        /// from https://stackoverflow.com/a/14644367/1755417
        /// </summary>
        /// <returns></returns>
        public static string GenerateUniqueToken()
        {
            // Include the creation datetime
            byte[] time = BitConverter.GetBytes( DateTime.UtcNow.ToBinary() );

            // Make it unique with a guid
            byte[] key = Guid.NewGuid().ToByteArray();

            string token = Convert.ToBase64String( time.Concat( key ).ToArray() );

            return token;
        }

    }
}
