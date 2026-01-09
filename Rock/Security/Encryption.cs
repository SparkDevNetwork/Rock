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

        /*
            12/30/2025 - N.A.

            These three constants define the structure of the V2 encryption format. They are critical
            to ensuring compatibility between encryption and decryption processes. Changing them after
            release would make previously encrypted data unreadable.

                * _hmacTagSize (32): Size of the authentication tag for HMAC-SHA256. Changing this alters
                  the payload layout and invalidates all existing tags. Only change if introducing a
                  **new version** with a different MAC algorithm/size.

                * _v2Footer ("V2"): Two-byte footer appended after the tag for O(1) format detection.
                  Altering/removing this prevents the decryptor from recognizing v2 payloads.

                * _hkdfInfo ("Rock.Security.Encryption.v2"): Domain-separation label for HKDF key
                  derivation. Changing it derives different ENC/MAC keys from the same root key, making
                  old ciphertexts undecryptable even if the DataEncryptionKey is unchanged.

            Reason: These constants form a versioned contract for encrypted payloads and must remain
            immutable in V2. Future changes require introducing a new version (e.g., V3) with separate
            identifiers to maintain backward compatibility.
        */
        private const int _hmacTagSize = 32;
        private static readonly byte[] _v2Footer = Encoding.ASCII.GetBytes( "V2" );
        private static readonly byte[] _hkdfInfo = Encoding.UTF8.GetBytes( "Rock.Security.Encryption.v2" );

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

        // A collection of old keys used to encrypt/decrypt a string. These should only be used for reads if the current key failed.
        private static Dictionary<string,byte[]> _oldDataEncryptionKeyBytes
        {
            get
            {
                if ( _oldDataEncryptionKeyBytesBacker == null )
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

        ///// <summary>
        ///// Encrypts using v2: AES-CBC + HMAC-SHA256 (encrypt-then-MAC) and appends "V2" footer.
        ///// Layout (before Base64): [ivLen:int][IV][CIPHERTEXT][TAG:32]["V2"].
        ///// </summary>
        ///// <param name="plainText">The UTF-8 text to encrypt. If null or empty, returns an empty string.</param>
        ///// <param name="keyBytes">The data encryption key bytes. Used to derive separate ENC and MAC keys via HKDF-SHA256.</param>
        ///// <returns>Base64 string of the v2 payload; empty string if <paramref name="plainText"/> is null/empty.</returns>
        ///// <remarks>
        ///// Authenticate-then-encrypt: tag is computed over [ivLen||IV||CIPHERTEXT]. Consumers must verify the tag before decryption.
        ///// </remarks>
        private static string EncryptString( string plainText, byte[] keyBytes )
        {
            if ( string.IsNullOrEmpty( plainText ) )
            {
                return string.Empty;
            }

            if ( keyBytes == null || keyBytes.Length == 0 )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            string outStr = null;

            // derive distinct ENC/MAC keys from the single DataEncryptionKey
            byte[] encKey;
            byte[] macKey;
            DeriveKeys( keyBytes, out encKey, out macKey );

            using ( Aes aes = Aes.Create() )
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encKey;

                // Create the streams used for encryption.
                using ( MemoryStream msEncrypt = new MemoryStream() )
                {
                    // Create a decryptor to perform the stream transform.
                    using ( ICryptoTransform encryptor = aes.CreateEncryptor( aes.Key, aes.IV ) )
                    {
                        // prepend the IV
                        msEncrypt.Write( BitConverter.GetBytes( aes.IV.Length ), 0, sizeof( int ) );
                        msEncrypt.Write( aes.IV, 0, aes.IV.Length );

                        using ( CryptoStream csEncrypt = new CryptoStream( msEncrypt, encryptor, CryptoStreamMode.Write, true ) )
                        using ( StreamWriter swEncrypt = new StreamWriter( csEncrypt, Encoding.UTF8 ) )
                        {
                            // Write all data to the stream.
                            swEncrypt.Write( plainText );
                        }
                    }

                    byte[] data = msEncrypt.ToArray();                 // [ivLen][IV][CIPHERTEXT]
                    byte[] tag = ComputeHmacSha256( macKey, data );

                    msEncrypt.Write( tag, 0, tag.Length );             // [..][TAG]
                    msEncrypt.Write( _v2Footer, 0, _v2Footer.Length ); // [..][TAG]["V2"]

                    outStr = Convert.ToBase64String( msEncrypt.ToArray() );
                }
            }
            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// This is only here for reference when testing v1 encryption.
        /// I left it in here because it was/is useful for troubleshooting anything
        /// that arises during the switch to the new v2 Encryption.  The new
        /// DecryptString can handle both, but the new EncryptString ONLY generates v2 encrypted
        /// strings.
        ///
        /// Once the v18.2 has been rolled out to production release, it would be fine to delete
        /// this old method since nothing uses it except us developers.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="keyBytes"></param>
        /// <returns></returns>
        [Obsolete( "Do not use this method. It is only for testing v1 encryption." )]
        [RockObsolete( "18.0" )]
        private static string EncryptStringV1ForTesting( string plainText, byte[] keyBytes )
        {
            if ( string.IsNullOrEmpty( plainText ) )
            {
                return string.Empty;
            }

            if ( keyBytes == null || keyBytes.Length == 0 )
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
        /// <returns>decrypted string; otherwise <c>null</c>.</returns>
        /// <param name="cipherText">The text to decrypt.</param>
        public static string DecryptString( string cipherText )
        {
            return DecryptString( cipherText, true );
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptString(), using an identical sharedSecret.
        /// </summary>
        /// <returns>decrypted string; otherwise <c>null</c>.</returns>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="isLegacyAllowed">When true, legacy decryption can be used to check the encrypted string.
        /// In general, you only want to use false here when you are CERTAIN the data was encrypted with the
        /// non-legacy value. If the value was encrypted and stored in the database as an attribute value
        /// (or similar) then you would NOT know it was encrypted with non-legacy encryption.</param>
        public static string DecryptString( string cipherText, bool isLegacyAllowed )
        {
            string plainText = null;

            try
            {
                plainText = DecryptString( cipherText, _dataEncryptionKeyBytes, isLegacyAllowed );
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
            if ( _oldDataEncryptionKeyBytes != null )
            {
                foreach ( var oldDataEncryptionKeyBytes in _oldDataEncryptionKeyBytes )
                {
                    try
                    {
                        plainText = DecryptString( cipherText, oldDataEncryptionKeyBytes.Value, isLegacyAllowed );
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
        /// Decrypts the string. Dual-mode Decrypt (footer → v2; else v2-no-footer; else v1)
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="keyBytes">The key bytes.</param>
        /// <param name="isLegacyAllowed">When true, legacy decryption can be used to check the encrypted string.</param>
        /// <returns>decrypted string; otherwise <c>null</c>.</returns>
        /// <exception cref="System.ArgumentNullException">DataEncryptionKey must be specified in configuration file</exception>
        private static string DecryptString( string cipherText, byte[] keyBytes, bool isLegacyAllowed )
        {
            if ( string.IsNullOrEmpty( cipherText ) )
            {
                return string.Empty;
            }

            if ( keyBytes == null || keyBytes.Length == 0 )
            {
                throw new ArgumentNullException( "DataEncryptionKey must be specified in configuration file" );
            }

            byte[] allBytes;
            try
            {
                allBytes = Convert.FromBase64String( cipherText );
            }
            catch
            {
                return null;
            }

            // Look for the V2 footer... and skip V1 attempt when the V2
            // footer is present.
            if ( allBytes.Length >= _hmacTagSize + _v2Footer.Length )
            {
                bool hasFooter =
                    allBytes[allBytes.Length - 2] == _v2Footer[0] &&
                    allBytes[allBytes.Length - 1] == _v2Footer[1];

                if ( hasFooter )
                {
                    string v2Plaintext = TryDecryptV2_WithFooter( allBytes, keyBytes );
                    return v2Plaintext; // may be null on auth failure
                }
            }

            if ( !isLegacyAllowed )
            {
                return null;
            }

            // Legacy v1 (no MAC). The remaining code is the original V1 decryption logic
            // which is kept only for backward compatibility.
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
        /// Decrypts a v2 payload that ends with the "V2" footer.
        /// </summary>
        /// <param name="allBytes">The full Base64-decoded payload bytes.</param>
        /// <param name="keyBytes">The DataEncryptionKey (root secret).</param>
        /// <returns>Decrypted plaintext on success; otherwise <c>null</c> if footer is missing, authentication fails, or decryption fails.
        /// </returns>
        /// <remarks>
        /// Flow:
        ///     (1) confirm "V2" footer,
        ///     (2) verify HMAC-SHA256 over [ivLen||IV||CIPHERTEXT],
        ///     (3) only if the tag matches, decrypt with AES-CBC/PKCS7. No decrypt is attempted on tag failure.
        /// </remarks>
        private static string TryDecryptV2_WithFooter( byte[] allBytes, byte[] keyBytes )
        {
            string plaintext = null;

            try
            {
                int footerLen = _v2Footer.Length;
                int tagOffset = allBytes.Length - footerLen - _hmacTagSize;
                if ( tagOffset <= 0 )
                {
                    return null;
                }

                if ( allBytes[allBytes.Length - 2] != _v2Footer[0] ||
                     allBytes[allBytes.Length - 1] != _v2Footer[1] )
                {
                    return null;
                }

                byte[] data = new byte[tagOffset];
                Buffer.BlockCopy( allBytes, 0, data, 0, data.Length );

                byte[] tag = new byte[_hmacTagSize];
                Buffer.BlockCopy( allBytes, tagOffset, tag, 0, _hmacTagSize );

                byte[] encKey;
                byte[] macKey;
                DeriveKeys( keyBytes, out encKey, out macKey );

                byte[] expected = ComputeHmacSha256( macKey, data );
                if ( !ConstantTimeEquals( tag, expected ) )
                {
                    return null;
                }

                int offset = 0;
                if ( data.Length < sizeof( int ) )
                {
                    return null;
                }

                int ivLen = BitConverter.ToInt32( data, offset );
                if ( ivLen <= 0 || ivLen > 64 )
                {
                    return null;
                }

                offset += sizeof( int );
                if ( data.Length < offset + ivLen )
                {
                    return null;
                }

                byte[] iv = new byte[ivLen];
                Buffer.BlockCopy( data, offset, iv, 0, ivLen );
                offset += ivLen;

                byte[] cipherBytes = new byte[data.Length - offset];
                Buffer.BlockCopy( data, offset, cipherBytes, 0, cipherBytes.Length );

                // Decrypt with AES (CBC/PKCS7)
                using ( Aes aes = Aes.Create() )
                {
                    //Explicit to avoid mode/padding surprises at runtime.
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.Key = encKey;
                    aes.IV = iv;

                    using ( MemoryStream ms = new MemoryStream( cipherBytes ) )
                    using ( ICryptoTransform decryptor = aes.CreateDecryptor( aes.Key, aes.IV ) )
                    using ( CryptoStream cs = new CryptoStream( ms, decryptor, CryptoStreamMode.Read ) )
                    using ( StreamReader sr = new StreamReader( cs, Encoding.UTF8 ) )
                    {
                        plaintext = sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                plaintext = null;
            }

            return plaintext;
        }

        /// <summary>
        /// Reads the byte array.
        /// </summary>
        /// <param name="s">The stream</param>
        /// <returns>The bytes from the stream</returns>
        /// <exception cref="System.SystemException">If the Stream did not contain properly formatted byte array or did not read byte array properly.</exception>
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
        public static string GetSHA1Hash( string plainText )
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
        /// Gets the ephemeral (temporary) hashing key to be used with HMAC and
        /// other related hashing algorithms. This key should not be used for
        /// anything that needs to persist for long periods of time as the value
        /// might change between versions of Rock or even between Rock restarts.
        /// </summary>
        /// <returns>A string that can be used as a temporary hashing key.</returns>
        internal static string GetEphemeralHashingKey()
        {
            return GetDataEncryptionKey().Sha256Hash();
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

        #region Common (private) Helper Methods

        private static void DeriveKeys( byte[] ikm, out byte[] encKey, out byte[] macKey )
        {
            var prk = HkdfExtract( _salt, ikm );
            var okm = HkdfExpand( prk, _hkdfInfo, 64 );
            encKey = new byte[32];
            macKey = new byte[32];
            Buffer.BlockCopy( okm, 0, encKey, 0, 32 );
            Buffer.BlockCopy( okm, 32, macKey, 0, 32 );
        }

        private static byte[] HkdfExtract( byte[] salt, byte[] ikm )
        {
            if ( salt == null )
            {
                salt = new byte[32];
            }

            using ( HMACSHA256 hmac = new HMACSHA256( salt ) )
            {
                return hmac.ComputeHash( ikm );
            }
        }

        private static byte[] HkdfExpand( byte[] prk, byte[] info, int length )
        {
            using ( HMACSHA256 hmac = new HMACSHA256( prk ) )
            using ( MemoryStream ms = new MemoryStream() )
            {
                byte[] t = Array.Empty<byte>();
                byte counter = 1;

                while ( ms.Length < length )
                {
                    hmac.Initialize();
                    if ( t.Length > 0 )
                    {
                        hmac.TransformBlock( t, 0, t.Length, null, 0 );
                    }

                    if ( info != null && info.Length > 0 )
                    {
                        hmac.TransformBlock( info, 0, info.Length, null, 0 );
                    }

                    hmac.TransformFinalBlock( new[] { counter }, 0, 1 );
                    t = hmac.Hash;
                    ms.Write( t, 0, t.Length );
                    counter++;
                }

                byte[] okm = new byte[length];
                Buffer.BlockCopy( ms.ToArray(), 0, okm, 0, length );
                return okm;
            }
        }

        private static byte[] ComputeHmacSha256( byte[] key, byte[] data )
        {
            using ( HMACSHA256 hmac = new HMACSHA256( key ) )
            {
                return hmac.ComputeHash( data );
            }
        }
        private static bool ConstantTimeEquals( byte[] a, byte[] b )
        {
            if ( a == null || b == null || a.Length != b.Length )
            {
                return false;
            }

            int diff = 0;
            for ( int i = 0; i < a.Length; i++ )
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }

        #endregion

    }
}
