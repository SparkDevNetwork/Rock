
using System;
using System.Security.Cryptography;
using System.Text;
using Rock.Model;
using Newtonsoft.Json;
using System.IO;
using Rock;

namespace church.ccv.Authentication
{
    public static class ChurchOnlinePlatform
    {
        /// <summary>
        /// Generates Login Url for Church Online Platform
        /// </summary>
        /// <param name="person">User person object</param>
        /// <param name="ssoKey">Church Online Platform SSO Key</param>
        /// <param name="chOPUrl">Church Online Platform Url</param>
        public static string CreateSSOUrlChOP( Person person, string ssoKey, string chOPUrl )
        {
            // Create json of Multipass
            ChOPCredentials credentials = new ChOPCredentials( person );
            string json = JsonConvert.SerializeObject( credentials );

            // Encrypt json
            string multipass;
            string signature;
            byte[] encrypted = null;

            // Perform encrypt process
            using ( AesManaged myAes = new AesManaged() )
            {
                // Encrypt using json and a SHA256 byte hash of ssoKey
                encrypted = Encrypt( json, GetSHA256ReturnByte( ssoKey ), myAes.IV );
                byte[] combined = new byte[myAes.IV.Length + encrypted.Length];
                Array.Copy( myAes.IV, 0, combined, 0, myAes.IV.Length );
                Array.Copy( encrypted, 0, combined, myAes.IV.Length, encrypted.Length );

                // Base64 the encrypted json
                multipass = Convert.ToBase64String( combined );

                // Create SHA1 signature of the multipass using the string ssoKey
                byte[] encrypted_signature = Signature( multipass, ssoKey );

                // Base64 the signature
                signature = Convert.ToBase64String( encrypted_signature );
            }

            //  Build and Return SSO URL in following format
            //    http://YOUR_CHURCH_ONLINE_DOMAIN/sso?sso=1V3VU27wFIERYU0vPFCbU&signature=CsqMu%2F9yc8%3D
            return chOPUrl + "/sso?sso=" + multipass.UrlEncode() + "&signature=" + signature.UrlEncode();
        }

        //  Encrypt a string (this was copied from Microsoft documentation)
        private static byte[] Encrypt( string plainText, byte[] key, byte[] IV )
        {
            byte[] encrypted;

            using ( AesManaged aesAlg = new AesManaged() )
            {
                aesAlg.BlockSize = 128;
                aesAlg.KeySize = 256;
                aesAlg.Key = key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor( aesAlg.Key, aesAlg.IV );

                using ( MemoryStream msEncrypt = new MemoryStream() )
                {
                    using ( CryptoStream csEncrypt = new CryptoStream( msEncrypt, encryptor, CryptoStreamMode.Write ) )
                    {
                        using ( StreamWriter swEncrypt = new StreamWriter( csEncrypt ) )
                        {
                            swEncrypt.Write( plainText );
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        /// <summary>
        /// Creates SHA1 hash of sso Key
        /// </summary>
        /// <param name="multipass">Multipass</param>
        /// <param name="ssoKey">Church Online Platform SSO Key</param>
        private static byte[] Signature( string multipass, string ssoKey )
        {
            byte[] signature;

            using ( HMACSHA1 hmac = new HMACSHA1( Encoding.UTF8.GetBytes( ssoKey ) ) )
            {
                using ( MemoryStream msHmac = new MemoryStream( Encoding.UTF8.GetBytes( multipass ) ) )
                {
                    signature = hmac.ComputeHash( msHmac );
                }
            }
            return signature;
        }

        /// <summary>
        /// Create SHA256 hash of the SSOKey and return as byte[]
        /// </summary>
        /// <param name="ssoKey">Church Online Platform SSO Key</param>
        private static byte[] GetSHA256ReturnByte( string ssoKey )
        {
            var byteKey = Encoding.UTF8.GetBytes( ssoKey );
            SHA256Managed hashString = new SHA256Managed();

            return hashString.ComputeHash( byteKey );
        }

        //   MultiPass Object
        internal class ChOPCredentials
        {
            public ChOPCredentials( Person person )
            {
                Email = person.Email.ToLower();
                Expires = DateTime.UtcNow.AddMinutes( 5 ).ToString( "o" );
                FirstName = person.FirstName;
                LastName = person.LastName;
                Nickname = !string.IsNullOrWhiteSpace( person.NickName ) ? person.NickName : person.FirstName;
            }

            [JsonProperty( PropertyName = "email" )]
            public string Email { get; set; }
            [JsonProperty( PropertyName = "expires" )]
            public string Expires { get; set; }
            [JsonProperty( PropertyName = "first_name" )]
            public string FirstName { get; set; }
            [JsonProperty( PropertyName = "last_name" )]
            public string LastName { get; set; }
            [JsonProperty( PropertyName = "nickname" )]
            public string Nickname { get; set; }
        }
    }
}
