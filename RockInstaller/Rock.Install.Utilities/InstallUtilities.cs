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

    }
}
