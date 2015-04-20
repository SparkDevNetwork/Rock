// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
                    cypherText = EncryptString( plainText, encryptionKey );
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
        /// The _key bytes created new for each thread/session
        /// </summary>
        [ThreadStatic]
        private static byte[] _keyBytes = null;

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptString().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        public static string EncryptString( string plainText )
        {
            string dataEncryptionKey = Encryption.GetDataEncryptionKey();
            return EncryptString( plainText, dataEncryptionKey );
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="dataEncryptionKey">The data encryption key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">DataEncryptionKey must be specified in configuration file</exception>
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
                if ( _keyBytes == null )
                {
                    // generate a new key for every thread (vs. every call which is slow) 
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );
                    _keyBytes = key.GetBytes( aesAlg.KeySize / 8 );
                }

                aesAlg.Key = _keyBytes;

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
        /// <param name="cipherText">The text to decrypt.</param>
        public static string DecryptString( string cipherText )
        {
            string dataEncryptionKey = ConfigurationManager.AppSettings["DataEncryptionKey"];
            
            string plainText = null;

            try
            {
                plainText = DecryptString( cipherText, dataEncryptionKey );
            }
            catch { }

            if ( plainText != null )
            {
                return plainText;
            }

            // Check for any old decryption strings
            int i = 0;
            dataEncryptionKey = ConfigurationManager.AppSettings["OldDataEncryptionKey" + ( i > 0 ? i.ToString() : "" )];
            while ( !string.IsNullOrWhiteSpace( dataEncryptionKey ) )
            {
                try
                {
                    plainText = DecryptString( cipherText, dataEncryptionKey );
                }
                catch { }

                if ( plainText != null )
                {
                    return plainText;
                }

                i++;
                dataEncryptionKey = ConfigurationManager.AppSettings["OldDataEncryptionKey" + ( i > 0 ? i.ToString() : "" )];
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
                if ( _keyBytes == null )
                {
                    // generate a new key for every thread (vs. every call which is slow) 
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt );
                    _keyBytes = key.GetBytes( aesAlg.KeySize / 8 );
                }

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String( cipherText );
                using ( MemoryStream msDecrypt = new MemoryStream( bytes ) )
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = _keyBytes;
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

    }
}
