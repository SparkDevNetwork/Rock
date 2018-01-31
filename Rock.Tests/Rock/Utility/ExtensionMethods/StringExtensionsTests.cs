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
    }
}
