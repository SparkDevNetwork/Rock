using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class StringExtensionsTest
    {
        #region AsDateTime

        [TestMethod]
        [DataRow( null, null )]
        [DataRow( "", null )]
        [DataRow( "2025-07-08T14:23:45", "2025-07-08 14:23:45" )]
        [DataRow( "2025-07-08", "2025-07-08 00:00:00" )]
        [DataRow( "2020-08-23T00:00:00.0000000", "2020-08-23 00:00:00.000" )]
        [DataRow( "1,2,3", "2003-01-02 00:00:00.000" )]
        [DataRow( "💥🔥", null )]
        [DataRow( "123456", null )]
        [DataRow( "07/04", "YYYY-07-04" )] // MM/dd fallback (custom logic)
        [DataRow( "20:58", "YYYY-MM-DD 20:58:00" )] // HH:mm fallback (custom logic)
        [DataRow( "2,4", "YYYY-02-04 00:00:00.000" )]
        public void AsDateTime_ShouldParseOrReturnNull( string input, string expected )
        {
            var result = input.AsDateTime();

            if ( expected == null )
            {
                Assert.That.IsNull( result );
            }
            else
            {
                var now = RockDateTime.Now;
                expected = expected
                    .Replace( "YYYY", now.Year.ToString() )
                    .Replace( "MM", now.Month.ToString( "D2" ) )
                    .Replace( "DD", now.Day.ToString( "D2" ) );

                Assert.That.AreEqual( DateTime.Parse( expected ), result );
            }
        }

        #endregion

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

        #region SubstringSafe

        [TestMethod]
        public void SubstringSafe_NullString()
        {
            string test = null;
            var output = test.SubstringSafe( 1, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_NegativeIndex()
        {
            var output = "Test".SubstringSafe( -1, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_IndexTooLarge()
        {
            var output = "Test".SubstringSafe( 10, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_NegativeLength()
        {
            var output = "Test".SubstringSafe( 1, -3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_LengthTooLarge()
        {
            var output = "Test".SubstringSafe( 1, 30 );
            Assert.That.AreEqual( "est", output );
        }

        [TestMethod]
        public void SubstringSafe_EmptyString()
        {
            var output = "".SubstringSafe( 0, 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_EmptyString()
        {
            var output = "".SubstringSafe( 3 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_NullString()
        {
            string test = null;
            var output = test.SubstringSafe( 1 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_NegativeIndex()
        {
            var output = "Test".SubstringSafe( -1 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_IndexTooLarge()
        {
            var output = "Test".SubstringSafe( 10 );
            Assert.That.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_ProperUse()
        {
            var output = "Test".SubstringSafe( 1 );
            Assert.That.AreEqual( "est", output );
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

        #region RedirectUrlContainsXss

        [DataRow( "page/1" )]
        [DataRow( "test&nbsp;test" )]
        [DataRow( "Occurrence=2023-09-28T09:00:00" )]                   // Valid date input.
        [DataRow( "Occurrence%253d2023-09-28T09%25253a00%25253a00" )]   // Valid date input, partially double and triple encoded.
        [DataTestMethod]
        public void RedirectUrlContainsXss_ValidInput( string input )
        {
            var output = input.RedirectUrlContainsXss();
            Assert.That.AreEqual( output, false );

        }

        [DataRow( "<style>" )]                          // Angle brackets.
        [DataRow( "%3Cstyle>" )]                        // URL-encoded Angle brackets.
        [DataRow( "&lt;style>" )]                       // HTML-encoded Angle brackets.
        [DataRow( "javas\tcript:alert(0)" )]            // Tab character.
        [DataRow( "1/+/[*/[]/+alert(1)//" )]            // Asterisk character.
        [DataRow( "javascript%253Aalert(%27xss%27)" )]  // javascript: (with double URL-encoded colon).
        [DataRow( "java%0d%0ascript%0d%0a:alert(0)" )]  // javascript: (with URL-encoded CR/LF characters).
        [DataRow( "javas cript:alert(0)" )]             // javascript: (with space character).
        // javascript: (HTML-encoded hex character reference).
        [DataRow( "&#x6A;&#x61;&#x76;&#x61;&#x73;&#x63;&#x72;&#x69;&#x70;&#x74;&#x3A;" )]
        // javascript: (HTML-encoded decimal character reference, no separators).
        [DataRow( "&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041" )]
        [DataTestMethod]
        public void RedirectUrlContainsXss_RiskyInput( string input )
        {
            var output = input.RedirectUrlContainsXss();
            Assert.That.AreEqual( output, true );
        }

        #endregion RedirectUrlContainsXss

        #region Truncate

        [TestMethod]
        [DataRow( 3, "thi" )]
        [DataRow( 2, "th" )]
        [DataRow( 1, "t" )]
        public void Truncate_WithMaxLengthLessThanFour_DoesNotAddEllipsis( int maxLength, string expectedString )
        {
            var testString = "this is a test";

            var actualString = testString.Truncate( maxLength, true );

            Assert.That.AreEqual( expectedString, actualString );
        }

        [TestMethod]
        [DataRow( 4, "t..." )]
        [DataRow( 5, "th..." )]
        [DataRow( 6, "thi..." )]
        public void Truncate_WithMaxLengthGreaterThanThree_DoesAddEllipsis( int maxLength, string expectedString )
        {
            var testString = "this is a test";

            var actualString = testString.Truncate( maxLength, true );

            Assert.That.AreEqual( expectedString, actualString );
        }

        #endregion

        #region AsDecimalPercentage

        [TestMethod]
        public void AsDecimalPercentage_Invalid()
        {
            var output = @"25p".AsDecimalPercentage();
            Assert.That.AreEqual( 0m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidInteger()
        {
            var output = @"25".AsDecimalPercentage();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerWithPercentageSymbol()
        {
            var output = @"25%".AsDecimalPercentage();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerWithSpaceAndPercentageSymbol()
        {
            var output = @"25 %".AsDecimalPercentage();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerGreaterThan100()
        {
            var output = @"567".AsDecimalPercentage();
            Assert.That.AreEqual( 5.67m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerLessThanZero()
        {
            var output = @"-33".AsDecimalPercentage();
            Assert.That.AreEqual( -0.33m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerOverridePrecision()
        {
            var output = @"35".AsDecimalPercentage( precision: 1 );
            Assert.That.AreEqual( 0.4m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerLessThanMinimumPercentage()
        {
            var output = @"4".AsDecimalPercentage( minPercentage: 5 );
            Assert.That.AreEqual( 0.05m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerMoreThanMaximumPercentage()
        {
            var output = @"80".AsDecimalPercentage( maxPercentage: 75 );
            Assert.That.AreEqual( 0.75m, output );
        }

        #endregion

        #region AsDecimalPercentageOrNull

        [TestMethod]
        public void AsDecimalPercentageOrNull_Invalid()
        {
            var output = @"25p".AsDecimalPercentageOrNull();
            Assert.That.IsNull( output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidInteger()
        {
            var output = @"25".AsDecimalPercentageOrNull();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerWithPercentageSymbol()
        {
            var output = @"25%".AsDecimalPercentageOrNull();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerWithSpaceAndPercentageSymbol()
        {
            var output = @"25 %".AsDecimalPercentageOrNull();
            Assert.That.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerGreaterThan100()
        {
            var output = @"567".AsDecimalPercentageOrNull();
            Assert.That.AreEqual( 5.67m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerLessThanZero()
        {
            var output = @"-33".AsDecimalPercentageOrNull();
            Assert.That.AreEqual( -0.33m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerOverridePrecision()
        {
            var output = @"35".AsDecimalPercentageOrNull( precision: 1 );
            Assert.That.AreEqual( 0.4m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerLessThanMinimumPercentage()
        {
            var output = @"4".AsDecimalPercentageOrNull( minPercentage: 5 );
            Assert.That.AreEqual( 0.05m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerMoreThanMaximumPercentage()
        {
            var output = @"80".AsDecimalPercentageOrNull( maxPercentage: 75 );
            Assert.That.AreEqual( 0.75m, output );
        }

        #endregion
    }
}
