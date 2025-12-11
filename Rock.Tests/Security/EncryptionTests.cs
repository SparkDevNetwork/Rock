using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Security;

namespace Rock.Tests.Security
{
    [TestClass]
    public class EncryptionTests
    {
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

        [TestMethod]
        public void DecriptShortStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText1 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.AreEqual( _plainText1, decryptedPlainText );
        }

        [TestMethod]
        public void DecriptSpecialCharStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText2 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.AreEqual( _plainText2, decryptedPlainText );
        }

        [TestMethod]
        public void DecriptLongStringWithCorrectKey()
        {
            var encryptedPlainText = Encryption.EncryptString( _plainText3 );
            string decryptedPlainText = Encryption.DecryptString( encryptedPlainText );

            Assert.AreEqual( _plainText3, decryptedPlainText );
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
