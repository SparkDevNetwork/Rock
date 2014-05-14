using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Install.Utilities
{
    static public class InstallUtilities
    {

        public static string GenerateRandomDataEncryptionKey()
        {
            var rng = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            byte[] randomBytes = new byte[128];
            rng.GetNonZeroBytes(randomBytes);
            string dataEncryptionKey = Convert.ToBase64String(randomBytes);

            return dataEncryptionKey;
        }

        public static string CleanBaseAddress(string address)
        {
            if (!address.EndsWith("/"))
            {
                address = address + "/";
            }
            return address;
        }

        // takes a string address like http://www.rocksolidchurchdemo.com/rock and returns www.rocksolidchurchdemo.com
        public static string GetDomainFromString(string address)
        {
          
            if (!address.Contains(Uri.SchemeDelimiter))
            {
                address = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, address);
            }
            Uri uri = new Uri(address);
            return uri.Host; 
        }

        // gets the domain from an email address
        public static string GetDomainFromEmail( string emailAddress )
        {
            string[] emailParts = emailAddress.Split('@');
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
