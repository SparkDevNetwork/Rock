using System;
using Xunit;

namespace Rock.Tests.Utility.ExtensionMethods
{
    public class StringExtensionsTest
    {
        #region AsDoubleOrNull

        /// <summary>
        /// Should not cast the true boolean to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidBoolean()
        {
            var output = @"True".AsDoubleOrNull();
            Assert.Null( output );
        }

        /// <summary>
        /// Should cast the integer to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidInteger()
        {
            var output = @"3".AsDoubleOrNull();
            Assert.Equal( 3.0d, output );
        }

        /// <summary>
        /// Should cast the double to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDouble()
        {
            var output = @"3.141592".AsDoubleOrNull();
            Assert.Equal( (double)3.141592d, output );
        }

        /// <summary>
        /// Should cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidString()
        {
            var output = @"$3.14".AsDoubleOrNull();
            Assert.Equal( (double)3.14d, output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidString()
        {
            var output = @"a".AsDoubleOrNull();
            Assert.Null( output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_EmptyString()
        {
            var output = @"".AsDoubleOrNull();
            Assert.Null( output );
        }

        /// <summary>
        /// Should not cast the decimal string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidDecimalString()
        {
            var output = @"T3.V3".AsDoubleOrNull();
            Assert.Null( output );
        }

        #endregion

        #region IsValidUrl
        [Fact]
        public void IsValidUrl_EmptyString()
        {
            string url = string.Empty;
            bool isValidUrl = url.IsValidUrl();
            Assert.False( isValidUrl );
        }

        [Fact]
        public void IsValidUrl_ValidUnsec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.True( isValidUrl );
        }

        [Fact]
        public void IsValidUrl_ValidSec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.True( isValidUrl );
        }

        [Fact]
        public void IsValidUrl_MissingTop()
        {
            string url = @"http://www.rocksolidchurch";
            bool isValidUrl = url.IsValidUrl();
            Assert.True( isValidUrl );
        }

        [Fact]
        public void IsValidUrl_NoProtocol()
        {
            string url = @"www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.False( isValidUrl );
        }
        
        [Fact]
        public void IsValidUrl_DotChurch()
        {
            string url = @"https://www.rocksolidchurch.church";
            bool isValidUrl = url.IsValidUrl();
            Assert.True( isValidUrl );
        }

        [Fact]
        public void IsValidUrl_Test()
        {
            string url = @"http://localhost:6229/page/1";
            //bool isValidUrl = Uri.IsWellFormedUriString( url, UriKind.Absolute );
            bool isValidUrl = url.IsValidUrl();
            Assert.True( isValidUrl );
        }

        #endregion

        #region SafeSubstring

        [Fact]
        public void SafeSubstring_NullString()
        {
            string test = null;
            var output = test.SafeSubstring( 1, 3 );
            Assert.Equal( string.Empty, output );
        }

        [Fact]
        public void SafeSubstring_NegativeIndex()
        {
            var output = "Test".SafeSubstring( -1, 3 );
            Assert.Equal( string.Empty, output );
        }

        [Fact]
        public void SafeSubstring_IndexTooLarge()
        {
            var output = "Test".SafeSubstring( 10, 3 );
            Assert.Equal( string.Empty, output );
        }

        [Fact]
        public void SafeSubstring_NegativeLength()
        {
            var output = "Test".SafeSubstring( 1, -3 );
            Assert.Equal( string.Empty, output );
        }

        [Fact]
        public void SafeSubstring_LengthTooLarge()
        {
            var output = "Test".SafeSubstring( 1, 30 );
            Assert.Equal( "est", output );
        }

        [Fact]
        public void SafeSubstring_EmptyString()
        {
            var output = "".SafeSubstring( 0, 3 );
            Assert.Equal( string.Empty, output );
        }

        #endregion

        #region AsNumeric

        [Fact]
        public void AsNumeric_NumbersOnly()
        {
            var output = "0abcd123-45-6&78$9".AsNumeric();
            Assert.Equal( "0123456789", output );
        }

        #endregion
    }
}
