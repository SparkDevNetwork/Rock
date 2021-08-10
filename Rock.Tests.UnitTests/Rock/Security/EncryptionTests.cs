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

        // These keys are also in app.config except for _dataEncryptionKey5
        private string _dataEncryptionKey1 = "uEr6E60giN7XWSQq7iysuRo98s01Ko51z+vxkB/j40u+zb4nxqgts+/i7Q7LlMgF+Ho8lbDWSrxZs1ZL4Uj7WUBR0tdxqBQenAkbtxg5D6ae+F9t62bmcbfbssXG4J4rUSTcJS8XzbBlIWnH6TWHsme5norJg7IkQq6HxLGaqy8=";
        private string _dataEncryptionKey2 = "Xo+EIZM2l3JnB/2gd+FejEYo9tEXhUOIXsMchegVQXSvUvwVlB5HEEg1Qcf+vwmwRFRlgrWXoQpKNnoyith2o1m0DiWy5g0JuCqMJhmVGPLtgMSeKNZtPw9o8kl8qY/4A8AdjD4t1+5fqhEZ3KXbTlYoYxi74N/FPabr+EbHFv8=";
        private string _dataEncryptionKey3 = "Px0/BGrK48nvggLY0l2p64C2228eLxZVjTxoZGiU8Qdi2ePf7PTa/VBr8BMuOBdO9A54wajOyCnVEMtb7zNW1SeBDAi1H9hvPHIhE4t9d5ixFIzjDE0sNpS3eEZHUrL6PCE1H2/fpkX1GTeOYRMgvmdIQqN6L752JnXx5qkr2gg=";
        private string _dataEncryptionKey4 = "Ib7+dCqtPnh6QpQ+RaFhoZKNiYYwn2GRGxTv9wmdYdIywivWlipuUxKN6kOx2Mh48SqKue51dG+r65LHmW2Xctqy9G/1GaHMVG+8KwEtM+XpYFXQRLeFRdRpjOTtPMI80nrBmLK353DDtUt/RfncVRjrzePGHVe1eEDon7D6CvY=";

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

        private string EncryptStringForTesting( string plainText, string dataEncryptionKey )
        {
            string encryptedString = null;

            RijndaelManaged aesAlg = new RijndaelManaged();
            aesAlg.Key = CalculateKeyBytes( dataEncryptionKey );
            ICryptoTransform encryptor = aesAlg.CreateEncryptor( aesAlg.Key, aesAlg.IV );

            using ( MemoryStream msEncrypt = new MemoryStream() )
            {
                msEncrypt.Write( BitConverter.GetBytes( aesAlg.IV.Length ), 0, sizeof( int ) );
                msEncrypt.Write( aesAlg.IV, 0, aesAlg.IV.Length );
                using ( CryptoStream csEncrypt = new CryptoStream( msEncrypt, encryptor, CryptoStreamMode.Write ) )
                {
                    using ( StreamWriter swEncrypt = new StreamWriter( csEncrypt ) )
                    {
                        swEncrypt.Write( plainText );
                    }
                }

                encryptedString = Convert.ToBase64String( msEncrypt.ToArray() );
            }

            aesAlg.Clear();
            return encryptedString;
        }

        private string GeneratePasswordKey()
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

            var key = hex.ToString().ToUpper();
            return key;
        }

        private string GenerateDataEncryptionKey()
        {
            var rng = RNGCryptoServiceProvider.Create();
            byte[] randomBytes = new byte[128];
            rng.GetNonZeroBytes( randomBytes );
            string dataEncryptionKey = Convert.ToBase64String( randomBytes );
            return dataEncryptionKey;
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
        public void DecriptStringEncryptedWithOldKey3()
        {
            var encryptedTextWithOldKey = EncryptStringForTesting( _plainText1, _dataEncryptionKey2 );
            string decryptedText = Encryption.DecryptString( encryptedTextWithOldKey );

            Assert.IsTrue( decryptedText == _plainText1 );
        }

        [TestMethod]
        public void DecriptStringEncryptedWithOldKey4()
        {
            var encryptedTextWithOldKey = EncryptStringForTesting( _plainText1, _dataEncryptionKey3 );
            string decryptedText = Encryption.DecryptString( encryptedTextWithOldKey );

            Assert.IsTrue( decryptedText == _plainText1 );
        }

        [TestMethod]
        public void DecryptStringEncryptedWithUnavailableKey()
        {
            var encryptedTextWithOldKey = EncryptStringForTesting( _plainText2, _dataEncryptionKey4 );
            string decryptedText = Encryption.DecryptString( encryptedTextWithOldKey );

            Assert.IsNull( decryptedText );
        }

        [TestMethod]
        public void CalculateKeyBytesTime()
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            _ = CalculateKeyBytes( _dataEncryptionKey1 );
            stopWatch.Stop();
            var time = stopWatch.ElapsedMilliseconds;

            Assert.IsTrue( time < 30 );
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
