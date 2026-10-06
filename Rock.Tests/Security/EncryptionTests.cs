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
        So, our OldKeys feature isn't going to work 100% of the time. It'll occasionally return garbage data vs an exception if an incorrect key is used.

        Love,

        Mike
         
         */

        private string _plainText1 = "Cute and fuzzy bunnies.";
        private string _plainText2 = "$3c3r3tP@$$w0rd";
        private string _plainText3 = "He piled upon the whale’s white hump the sum of all the general rage and hate felt by his whole race from Adam down; and then, as if his chest had been a mortar, he burst his hot heart’s shell upon it.";
        private string _plainText1LegacyV1EncryptedExampleString = "EAAAABt2Hf15JwSgEX8UoQy37JKIsJeHgKWylyCMblZbFE4EReu3DT/rgxGoHFPCocSdRA==";

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

        [TestMethod]
        public void DecryptLegacyV1StringShouldWorkWhenAllowed()
        {
            var decryptedLegacyV1String = Encryption.DecryptString( _plainText1LegacyV1EncryptedExampleString );
            Assert.AreEqual( decryptedLegacyV1String, _plainText1 );
        }

        [TestMethod]
        public void DecryptLegacyV1StringShouldNotWorkWhenLegacyIsDisabled()
        {
            var decryptedLegacyV1String = Encryption.DecryptString( _plainText1LegacyV1EncryptedExampleString, isLegacyAllowed: false );
            Assert.AreNotEqual( decryptedLegacyV1String, _plainText1 );
        }

        [TestMethod]
        public void DecryptStringWithBitFlippedCorruptedDataShouldNotContainPartialMatch()
        {
            var anEncryptedString = Encryption.EncryptString( _plainText1 );
            var lastEightCharacters = _plainText1.Length <= 8
                ? _plainText1
                : _plainText1.Substring( _plainText1.Length - 8, 8 );

            // Flip a bit in the encrypted string to simulate data corruption
            char[] charArray = anEncryptedString.ToCharArray();
            charArray[10] = charArray[10] != 'A' ? 'A' : 'B'; // Change character at position 10
            var corruptedEncryptedString = new string( charArray );
            var decryptedStringWithMethod = Encryption.DecryptString( corruptedEncryptedString );

            // The decrypted string should not contain part of the original plain text
            Assert.DoesNotContain( lastEightCharacters, decryptedStringWithMethod.ToStringSafe() );
        }

        #region Purpose Encryption

        private const string _purpose1 = "Rock.Tests.Purpose1";
        private const string _purpose2 = "Rock.Tests.Purpose2";

        [TestMethod]
        public void DecryptStringForPurposeWithSamePurposeReturnsPlainText()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText3, _purpose1 );

            Assert.AreEqual( _plainText3, Encryption.DecryptStringForPurpose( encrypted, _purpose1 ) );
        }

        [TestMethod]
        public void DecryptStringForPurposeWithDifferentPurposeReturnsNull()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText1, _purpose1 );

            Assert.IsNull( Encryption.DecryptStringForPurpose( encrypted, _purpose2 ) );
        }

        [TestMethod]
        public void DecryptStringForPurposeRejectsTextEncryptedWithoutPurpose()
        {
            var encrypted = Encryption.EncryptString( _plainText1 );

            Assert.IsNull( Encryption.DecryptStringForPurpose( encrypted, _purpose1 ) );

            // Also reject it when something that looks like an authentication
            // code has been added to it.
            var fakeCode = System.Convert.ToBase64String( new byte[32] );

            Assert.IsNull( Encryption.DecryptStringForPurpose( $"{encrypted}.{fakeCode}", _purpose1 ) );
        }

        [TestMethod]
        public void DecryptStringDoesNotReturnTextEncryptedForPurpose()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText1, _purpose1 );

            // Remove the authentication code so only the encrypted text is
            // given to DecryptString, which uses the general key.
            var encryptedText = encrypted.Substring( 0, encrypted.LastIndexOf( '.' ) );

            Assert.AreNotEqual( _plainText1, Encryption.DecryptString( encryptedText ) );
        }

        [TestMethod]
        public void EncryptStringForPurposeProducesDifferentTextEachTime()
        {
            var encrypted1 = Encryption.EncryptStringForPurpose( _plainText1, _purpose1 );
            var encrypted2 = Encryption.EncryptStringForPurpose( _plainText1, _purpose1 );

            Assert.AreNotEqual( encrypted1, encrypted2 );
        }

        [TestMethod]
        public void DecryptStringForPurposeRejectsModifiedEncryptedText()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText3, _purpose1 );

            Assert.IsNull( Encryption.DecryptStringForPurpose( ChangeCharacter( encrypted, 5 ), _purpose1 ) );
        }

        [TestMethod]
        public void DecryptStringForPurposeRejectsModifiedAuthenticationCode()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText3, _purpose1 );

            Assert.IsNull( Encryption.DecryptStringForPurpose( ChangeCharacter( encrypted, encrypted.Length - 3 ), _purpose1 ) );
        }

        [TestMethod]
        public void DecryptStringForPurposeRejectsMissingAuthenticationCode()
        {
            var encrypted = Encryption.EncryptStringForPurpose( _plainText3, _purpose1 );
            var withoutCode = encrypted.Substring( 0, encrypted.LastIndexOf( '.' ) );

            Assert.IsNull( Encryption.DecryptStringForPurpose( withoutCode, _purpose1 ) );
            Assert.IsNull( Encryption.DecryptStringForPurpose( withoutCode + ".", _purpose1 ) );
        }

        [TestMethod]
        public void DecryptStringForPurposeReturnsNullForEmptyText()
        {
            Assert.IsNull( Encryption.DecryptStringForPurpose( null, _purpose1 ) );
            Assert.IsNull( Encryption.DecryptStringForPurpose( string.Empty, _purpose1 ) );
            Assert.IsNull( Encryption.DecryptStringForPurpose( ".", _purpose1 ) );
        }

        /// <summary>
        /// Changes the character at the specified index to a different valid
        /// Base64 character.
        /// </summary>
        private static string ChangeCharacter( string text, int index )
        {
            var replacement = text[index] == 'A' ? 'B' : 'A';

            return text.Substring( 0, index ) + replacement + text.Substring( index + 1 );
        }

        #endregion

        #region Root Folder Encryption

        [TestMethod]
        [DataRow( "~/Content" )]
        [DataRow( "~/App_Data/TemporaryFiles" )]
        [DataRow( "~/Content/Themes/Rock/" )]
        public void DecryptRootFolderReturnsRootFolderEncryptedForRootFolders( string rootFolder )
        {
            var encrypted = Encryption.EncryptRootFolder( rootFolder );

            Assert.AreEqual( rootFolder, Encryption.DecryptRootFolder( encrypted ) );
        }

        [TestMethod]
        public void EncryptRootFolderReturnsEmptyStringForEmptyRootFolder()
        {
            Assert.AreEqual( string.Empty, Encryption.EncryptRootFolder( null ) );
            Assert.AreEqual( string.Empty, Encryption.EncryptRootFolder( string.Empty ) );
        }

        [TestMethod]
        public void DecryptRootFolderReturnsNullForEmptyText()
        {
            Assert.IsNull( Encryption.DecryptRootFolder( null ) );
            Assert.IsNull( Encryption.DecryptRootFolder( string.Empty ) );
            Assert.IsNull( Encryption.DecryptRootFolder( " " ) );
        }

        [TestMethod]
        [DataRow( "~/Content" )]
        [DataRow( "~/Content/" )]
        [DataRow( "~/content/Images" )]
        [DataRow( "~/Content/Themes/Rock/Assets" )]
        public void DecryptRootFolderAllowsContentFolderEncryptedWithoutPurpose( string rootFolder )
        {
            var encrypted = Encryption.EncryptString( rootFolder );

            Assert.AreEqual( rootFolder, Encryption.DecryptRootFolder( encrypted ) );
        }

        [TestMethod]
        [DataRow( "~/" )]
        [DataRow( "~" )]
        [DataRow( "~/App_Data" )]
        [DataRow( "~/Bin" )]
        [DataRow( "~/App_Code/" )]
        [DataRow( "~/Content/.." )]
        [DataRow( "~/Content/../Bin" )]
        [DataRow( "~/Content/./../App_Data" )]
        [DataRow( "~\\Content\\..\\App_Data" )]
        [DataRow( "~/Content/ /.." )]
        [DataRow( "~/Content/.../x" )]
        [DataRow( "~/ContentX" )]
        [DataRow( "~/CONTEN~1" )]
        [DataRow( "~/Content/x:stream" )]
        [DataRow( "Content" )]
        [DataRow( "/Content" )]
        [DataRow( "C:\\inetpub\\wwwroot\\Content" )]
        public void DecryptRootFolderRejectsOtherFoldersEncryptedWithoutPurpose( string rootFolder )
        {
            var encrypted = Encryption.EncryptString( rootFolder );

            Assert.IsNull( Encryption.DecryptRootFolder( encrypted ) );
        }

        [TestMethod]
        public void DecryptRootFolderRejectsTextEncryptedForOtherPurpose()
        {
            var encrypted = Encryption.EncryptStringForPurpose( "~/App_Data", _purpose1 );

            Assert.IsNull( Encryption.DecryptRootFolder( encrypted ) );
        }

        [TestMethod]
        public void DecryptRootFolderRejectsModifiedEncryptedText()
        {
            var encrypted = Encryption.EncryptRootFolder( "~/App_Data/TemporaryFiles" );

            Assert.IsNull( Encryption.DecryptRootFolder( ChangeCharacter( encrypted, 5 ) ) );
            Assert.IsNull( Encryption.DecryptRootFolder( ChangeCharacter( encrypted, encrypted.Length - 3 ) ) );
        }

        [TestMethod]
        public void DecryptStringDoesNotReturnRootFolder()
        {
            var encrypted = Encryption.EncryptRootFolder( "~/App_Data/TemporaryFiles" );

            Assert.AreNotEqual( "~/App_Data/TemporaryFiles", Encryption.DecryptString( encrypted ) );
        }

        [TestMethod]
        public void EncryptRootFolderProducesDifferentTextEachTime()
        {
            var encrypted1 = Encryption.EncryptRootFolder( "~/Content" );
            var encrypted2 = Encryption.EncryptRootFolder( "~/Content" );

            Assert.AreNotEqual( encrypted1, encrypted2 );
        }

        #endregion
    }
}
