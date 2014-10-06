using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace RockInstallTools
{
    public class RockInstallUtilities
    {
        public static string EncodePassword( string password, string userAccountGuid, string passwordKey )
        {
            byte[] encryptionKey = HexToByte( passwordKey );

            HMACSHA1 hash = new HMACSHA1();
            hash.Key = encryptionKey;

            HMACSHA1 uniqueHash = new HMACSHA1();
            uniqueHash.Key = HexToByte( userAccountGuid.Replace( "-", "" ) );

            return Convert.ToBase64String( uniqueHash.ComputeHash( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) ) );
        }
        
        public static string GeneratePasswordKey( int length )
        {
            var rng = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            byte[] randomBytes = new byte[length];
            rng.GetNonZeroBytes( randomBytes );
            string dataEncryptionKey = Convert.ToBase64String( randomBytes );

            return dataEncryptionKey;
        }

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

        private static byte[] HexToByte( string hexString )
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for ( int i = 0; i < returnBytes.Length; i++ )
                returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
            return returnBytes;
        }

        public static string GeneratePasswordKey()
        {
            int passwordLength = 32;
            int alphaNumericalCharsAllowed = 2;
            string randomPassword = Membership.GeneratePassword( passwordLength, alphaNumericalCharsAllowed );

            // turn string into byte array
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes( randomPassword );
            System.Text.StringBuilder hex = new System.Text.StringBuilder( bytes.Length * 2 );
            foreach ( byte b in bytes )
            {
                hex.AppendFormat( "{0:x2}", b );
            }

            return hex.ToString().ToUpper();
        }

        public static string CleanBaseAddress( string address )
        {
            if ( !address.EndsWith( "/" ) )
            {
                address = address + "/";
            }
            return address;
        }

        // takes a string address like http://www.rocksolidchurchdemo.com/rock and returns www.rocksolidchurchdemo.com
        public static string GetDomainFromString( string address )
        {

            if ( !address.Contains( Uri.SchemeDelimiter ) )
            {
                address = string.Concat( Uri.UriSchemeHttp, Uri.SchemeDelimiter, address );
            }
            Uri uri = new Uri( address );
            return uri.Host;
        }

        // gets the domain from an email address
        public static string GetDomainFromEmail( string emailAddress )
        {
            string[] emailParts = emailAddress.Split( '@' );
            if ( emailParts.Count() == 2 )
            {
                return emailParts[1];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
