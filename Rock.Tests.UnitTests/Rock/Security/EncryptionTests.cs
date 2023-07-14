using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Security;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Security
{
    [TestClass]
    public class EncryptionTests
    {
        private static byte[] _salt1 = Encoding.ASCII.GetBytes( "rsduYVC2leenXKTLYLkO9qsWU95HGCvWlbXcBTjtrj5dBJ7RPeGYiw7U3lZE+LWkT+jGrLP9deRMc8sUHJtc/wu2l4vANBx5f+p1zpRwQ2bB/E6Ta8k7haPiTRc4wYhrmWMrg8VfQ4MhAsSlijIfT9u+DszEkB2ba2k0FIPMSWk=" );

        private string _dataEncryptionKey1 = "uEr6E60giN7XWSQq7iysuRo98s01Ko51z+vxkB/j40u+zb4nxqgts+/i7Q7LlMgF+Ho8lbDWSrxZs1ZL4Uj7WUBR0tdxqBQenAkbtxg5D6ae+F9t62bmcbfbssXG4J4rUSTcJS8XzbBlIWnH6TWHsme5norJg7IkQq6HxLGaqy8=";

        /* 09/04/2021 MDP
          
        We used to test our OldKeys feature ( old keys specified in Web.config).
        However, encrypted data can be decrypted with an incorrect key and return without throwing an exception (it just returns garbage data instead).
        So, our OldKeys feature isn't going to work 100% of the time. It'll occassionally return garbage data vs an exception if an incorrect key is used.

        Love,

        Mike
         
         */

        private string _plainText1 = "Cute and fuzzy bunnies.";
        private string _plainText2 = "$3c3r3tP@$$w0rd";
        private string _plainText3 = "He piled upon the whale’s white hump the sum of all the general rage and hate felt by his whole race from Adam down; and then, as if his chest had been a mortar, he burst his hot heart’s shell upon it.";

        private byte[] CalculateKeyBytes( string dataEncryptionKey )
        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes( dataEncryptionKey, _salt1 );
            var aesAlg = new RijndaelManaged();
            byte[] keyBytes = key.GetBytes( aesAlg.KeySize / 8 );
            return keyBytes;
        }

        [TestMethod]
        public void DecriptShortStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText1 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.IsTrue( decryptedPlainText == _plainText1 );
        }

        [TestMethod]
        public void DecriptSpecialCharStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText2 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.IsTrue( decryptedPlainText == _plainText2 );
        }

        [TestMethod]
        public void DecriptLongStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText3 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.IsTrue( decryptedPlainText == _plainText3 );
        }

        [TestMethod]
        public void CalculateKeyBytesTime()
        {
            // Run the calculation once before we time it to make sure any
            // dynamically loaded libraries are in memory first. Otherwise we
            // are testing the CLR instead of the method call.
            _ = CalculateKeyBytes( _dataEncryptionKey1 );

            // Now sleep for 1 second and run it one more time to make sure we
            // are settled down. This is to attempt to fix random CI errors
            // when the unit tests are run.
            System.Threading.Thread.Sleep( 1000 );
            _ = CalculateKeyBytes( _dataEncryptionKey1 );

            // Run the actual test.
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            _ = CalculateKeyBytes( _dataEncryptionKey1 );
            stopWatch.Stop();
            var time = stopWatch.ElapsedMilliseconds;

            Assert.IsTrue( time < 30, $"Test took {time}ms." );
        }

        [TestMethod]
        public void EncryptStringWithLegacyMethodAndDecryptWithNewMethod()
        {
            #pragma warning disable CS0618
            var oldMethodEncryptedString = Encryption.EncryptString( _plainText2, _dataEncryptionKey1 );
            #pragma warning restore CS0618
            var decryptedOldMethodStringWithNewMethod = Encryption.DecryptString( oldMethodEncryptedString );

            Assert.AreEqual( decryptedOldMethodStringWithNewMethod, _plainText2 );
        }
    }
}
