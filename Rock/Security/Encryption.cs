//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Rock.Security
{
    /// <summary>
    /// Encryption Helper class
    /// </summary>
    public static class Encryption
    {
        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="Message">The string to encrypt.</param>
        /// <returns></returns>
        public static string EncryptString( string Message )
        {
            string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
            if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
            {
                encryptionPhrase = "Rock Rocks!";
            }

            byte[] Results;
            UTF8Encoding UTF8 = new UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash( UTF8.GetBytes( encryptionPhrase ) );

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes( Message );

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock( DataToEncrypt, 0, DataToEncrypt.Length );
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String( Results );
        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="Message">The string to decrypt.</param>
        /// <returns></returns>
        public static string DecryptString( string Message )
        {
            string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
            if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
            {
                encryptionPhrase = "Rock Rocks!";
            }

            byte[] Results;
            UTF8Encoding UTF8 = new UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash( UTF8.GetBytes( encryptionPhrase ) );

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String( Message );

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock( DataToDecrypt, 0, DataToDecrypt.Length );
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString( Results );
        }
    }
}