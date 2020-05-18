using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class StringExtensionsTest
    {
        #region AsDoubleOrNull

        /// <summary>
        /// Should not cast the true boolean to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidBoolean()
        {
            var output = @"True".AsDoubleOrNull();
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// Should cast the integer to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidInteger()
        {
            var output = @"3".AsDoubleOrNull();
            Assert.That.AreEqual( 3.0d, output );
        }

        /// <summary>
        /// Should cast the double to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidDouble()
        {
            var output = @"3.141592".AsDoubleOrNull();
            Assert.That.AreEqual( ( double ) 3.141592d, output );
        }

        /// <summary>
        /// Should cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidString()
        {
            var output = @"$3.14".AsDoubleOrNull();
            Assert.That.AreEqual( ( double ) 3.14d, output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidString()
        {
            var output = @"a".AsDoubleOrNull();
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_EmptyString()
        {
            var output = @"".AsDoubleOrNull();
            Assert.That.IsNull( output );
        }

        /// <summary>
        /// Should not cast the decimal string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidDecimalString()
        {
            var output = @"T3.V3".AsDoubleOrNull();
            Assert.That.IsNull( output );
        }

        #endregion

        #region IsValidUrl
        [TestMethod]
        public void IsValidUrl_EmptyString()
        {
            string url = string.Empty;
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsFalse( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_ValidUnsec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_ValidSec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_MissingTop()
        {
            string url = @"http://www.rocksolidchurch";
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_NoProtocol()
        {
            string url = @"www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsFalse( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_DotChurch()
        {
            string url = @"https://www.rocksolidchurch.church";
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_Test()
        {
            string url = @"http://localhost:6229/page/1";
            //bool isValidUrl = Uri.IsWellFormedUriString( url, UriKind.Absolute );
            bool isValidUrl = url.IsValidUrl();
            Assert.That.IsTrue( isValidUrl );
        }

        #endregion

        #region SafeSubstring

        [TestMethod]
        public void SafeSubstring_NullString()
        {
            string test = null;
            var output = test.SafeSubstring( 1, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SafeSubstring_NegativeIndex()
        {
            var output = "Test".SafeSubstring( -1, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SafeSubstring_IndexTooLarge()
        {
            var output = "Test".SafeSubstring( 10, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SafeSubstring_NegativeLength()
        {
            var output = "Test".SafeSubstring( 1, -3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SafeSubstring_LengthTooLarge()
        {
            var output = "Test".SafeSubstring( 1, 30 );
            Assert.That.AreEqual( "est", output );
        }

        [TestMethod]
        public void SafeSubstring_EmptyString()
        {
            var output = "".SafeSubstring( 0, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        #endregion

        #region AsNumeric

        [TestMethod]
        public void AsNumeric_NumbersOnly()
        {
            var output = "0abcd123-45-6&78$9".AsNumeric();
            Assert.That.AreEqual( "0123456789", output );
        }

        #endregion
    }
}
