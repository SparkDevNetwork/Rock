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
            Assert.Equal( output, 3.0d );
        }

        /// <summary>
        /// Should cast the double to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDouble()
        {
            var output = @"3.141592".AsDoubleOrNull();
            Assert.Equal( output, (double)3.141592d );
        }

        /// <summary>
        /// Should cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidString()
        {
            var output = @"$3.14".AsDoubleOrNull();
            Assert.Equal( output, (double)3.14d );
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
    }
}
