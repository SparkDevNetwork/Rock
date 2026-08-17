using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Lava;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class StringExtensionsTest
    {

        [ClassInitialize()]
        public static void Initialize( TestContext context )
        {
            // Reset the timezone to avoid problems with other tests.
            LavaTestHelper.SetRockDateTimeToLocalTimezone();
        }

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
                Assert.IsNull( result );
            }
            else
            {
                var now = RockDateTime.Now;
                expected = expected
                    .Replace( "YYYY", now.Year.ToString() )
                    .Replace( "MM", now.Month.ToString( "D2" ) )
                    .Replace( "DD", now.Day.ToString( "D2" ) );

                Assert.AreEqual( DateTime.Parse( expected ), result );
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
            Assert.IsNull( output );
        }

        /// <summary>
        /// Should cast the integer to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidInteger()
        {
            var output = @"3".AsDoubleOrNull();
            Assert.AreEqual( 3.0d, output );
        }

        /// <summary>
        /// Should cast the double to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidDouble()
        {
            var output = @"3.141592".AsDoubleOrNull();
            Assert.AreEqual( ( double ) 3.141592d, output );
        }

        /// <summary>
        /// Should cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_ValidString()
        {
            var output = @"$3.14".AsDoubleOrNull();
            Assert.AreEqual( ( double ) 3.14d, output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidString()
        {
            var output = @"a".AsDoubleOrNull();
            Assert.IsNull( output );
        }

        /// <summary>
        /// Should not cast the string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_EmptyString()
        {
            var output = @"".AsDoubleOrNull();
            Assert.IsNull( output );
        }

        /// <summary>
        /// Should not cast the decimal string to a double.
        /// </summary>
        [TestMethod]
        public void AsDouble_InvalidDecimalString()
        {
            var output = @"T3.V3".AsDoubleOrNull();
            Assert.IsNull( output );
        }

        #endregion

        #region IsValidUrl
        [TestMethod]
        public void IsValidUrl_EmptyString()
        {
            string url = string.Empty;
            bool isValidUrl = url.IsValidUrl();
            Assert.IsFalse( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_ValidUnsec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_ValidSec()
        {
            string url = @"http://www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_MissingTop()
        {
            string url = @"http://www.rocksolidchurch";
            bool isValidUrl = url.IsValidUrl();
            Assert.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_NoProtocol()
        {
            string url = @"www.rocksolidchurch.org";
            bool isValidUrl = url.IsValidUrl();
            Assert.IsFalse( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_DotChurch()
        {
            string url = @"https://www.rocksolidchurch.church";
            bool isValidUrl = url.IsValidUrl();
            Assert.IsTrue( isValidUrl );
        }

        [TestMethod]
        public void IsValidUrl_Test()
        {
            string url = @"http://localhost:6229/page/1";
            //bool isValidUrl = Uri.IsWellFormedUriString( url, UriKind.Absolute );
            bool isValidUrl = url.IsValidUrl();
            Assert.IsTrue( isValidUrl );
        }

        #endregion

        #region SubstringSafe

        [TestMethod]
        public void SubstringSafe_NullString()
        {
            string test = null;
            var output = test.SubstringSafe( 1, 3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_NegativeIndex()
        {
            var output = "Test".SubstringSafe( -1, 3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_IndexTooLarge()
        {
            var output = "Test".SubstringSafe( 10, 3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_NegativeLength()
        {
            var output = "Test".SubstringSafe( 1, -3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_LengthTooLarge()
        {
            var output = "Test".SubstringSafe( 1, 30 );
            Assert.AreEqual( "est", output );
        }

        [TestMethod]
        public void SubstringSafe_EmptyString()
        {
            var output = "".SubstringSafe( 0, 3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_EmptyString()
        {
            var output = "".SubstringSafe( 3 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_NullString()
        {
            string test = null;
            var output = test.SubstringSafe( 1 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_NegativeIndex()
        {
            var output = "Test".SubstringSafe( -1 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_IndexTooLarge()
        {
            var output = "Test".SubstringSafe( 10 );
            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SubstringSafe_StartIndexOnly_ProperUse()
        {
            var output = "Test".SubstringSafe( 1 );
            Assert.AreEqual( "est", output );
        }

        #endregion

        #region AsNumeric

        [TestMethod]
        public void AsNumeric_NumbersOnly()
        {
            var output = "0abcd123-45-6&78$9".AsNumeric();
            Assert.AreEqual( "0123456789", output );
        }

        #endregion

        #region RedirectUrlContainsXss

        [DataRow( "page/1" )]
        [DataRow( "test&nbsp;test" )]
        [DataRow( "Occurrence=2023-09-28T09:00:00" )]                   // Valid date input.
        [DataRow( "Occurrence%253d2023-09-28T09%25253a00%25253a00" )]   // Valid date input, partially double and triple encoded.
        [TestMethod]
        public void RedirectUrlContainsXss_ValidInput( string input )
        {
            var output = input.RedirectUrlContainsXss();
            Assert.IsFalse( output );

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
        [TestMethod]
        public void RedirectUrlContainsXss_RiskyInput( string input )
        {
            var output = input.RedirectUrlContainsXss();
            Assert.IsTrue( output );
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

            Assert.AreEqual( expectedString, actualString );
        }

        [TestMethod]
        [DataRow( 4, "t..." )]
        [DataRow( 5, "th..." )]
        [DataRow( 6, "thi..." )]
        public void Truncate_WithMaxLengthGreaterThanThree_DoesAddEllipsis( int maxLength, string expectedString )
        {
            var testString = "this is a test";

            var actualString = testString.Truncate( maxLength, true );

            Assert.AreEqual( expectedString, actualString );
        }

        #endregion

        #region AsDecimalPercentage

        [TestMethod]
        public void AsDecimalPercentage_Invalid()
        {
            var output = @"25p".AsDecimalPercentage();
            Assert.AreEqual( 0m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidInteger()
        {
            var output = @"25".AsDecimalPercentage();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerWithPercentageSymbol()
        {
            var output = @"25%".AsDecimalPercentage();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerWithSpaceAndPercentageSymbol()
        {
            var output = @"25 %".AsDecimalPercentage();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerGreaterThan100()
        {
            var output = @"567".AsDecimalPercentage();
            Assert.AreEqual( 5.67m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerLessThanZero()
        {
            var output = @"-33".AsDecimalPercentage();
            Assert.AreEqual( -0.33m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerOverridePrecision()
        {
            var output = @"35".AsDecimalPercentage( precision: 1 );
            Assert.AreEqual( 0.4m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerLessThanMinimumPercentage()
        {
            var output = @"4".AsDecimalPercentage( minPercentage: 5 );
            Assert.AreEqual( 0.05m, output );
        }

        [TestMethod]
        public void AsDecimalPercentage_ValidIntegerMoreThanMaximumPercentage()
        {
            var output = @"80".AsDecimalPercentage( maxPercentage: 75 );
            Assert.AreEqual( 0.75m, output );
        }

        #endregion

        #region AsDecimalPercentageOrNull

        [TestMethod]
        public void AsDecimalPercentageOrNull_Invalid()
        {
            var output = @"25p".AsDecimalPercentageOrNull();
            Assert.IsNull( output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidInteger()
        {
            var output = @"25".AsDecimalPercentageOrNull();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerWithPercentageSymbol()
        {
            var output = @"25%".AsDecimalPercentageOrNull();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerWithSpaceAndPercentageSymbol()
        {
            var output = @"25 %".AsDecimalPercentageOrNull();
            Assert.AreEqual( 0.25m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerGreaterThan100()
        {
            var output = @"567".AsDecimalPercentageOrNull();
            Assert.AreEqual( 5.67m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerLessThanZero()
        {
            var output = @"-33".AsDecimalPercentageOrNull();
            Assert.AreEqual( -0.33m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerOverridePrecision()
        {
            var output = @"35".AsDecimalPercentageOrNull( precision: 1 );
            Assert.AreEqual( 0.4m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerLessThanMinimumPercentage()
        {
            var output = @"4".AsDecimalPercentageOrNull( minPercentage: 5 );
            Assert.AreEqual( 0.05m, output );
        }

        [TestMethod]
        public void AsDecimalPercentageOrNull_ValidIntegerMoreThanMaximumPercentage()
        {
            var output = @"80".AsDecimalPercentageOrNull( maxPercentage: 75 );
            Assert.AreEqual( 0.75m, output );
        }

        #endregion

        #region ToGuidV5

        /// <summary>
        /// The standard RFC 4122 namespace for fully-qualified domain names, used below because it has
        /// widely published version 5 test vectors.
        /// </summary>
        private static readonly Guid _dnsNamespace = new Guid( "6ba7b810-9dad-11d1-80b4-00c04fd430c8" );

        /// <summary>
        /// Verifies the implementation against published RFC 4122 version 5 test vectors. If this fails,
        /// the hashing or byte-order handling is wrong, not merely different from what we expected. These
        /// names are already lower case, so our lower-casing normalization does not affect them.
        /// </summary>
        [TestMethod]
        [DataRow( "python.org", "886313e1-3b8a-5372-9b90-0c9aee199e5d" )]
        [DataRow( "www.example.com", "2ed6657d-e927-568b-95e1-2665a8aea6a2" )]
        public void ToGuidV5_WithKnownLowerCaseTestVector_ReturnsExpectedGuid( string name, string expectedGuid )
        {
            var result = name.ToGuidV5( _dnsNamespace );

            Assert.AreEqual( new Guid( expectedGuid ), result );
        }

        /// <summary>
        /// Pins the deliberate deviation from canonical RFC 4122: because we lower case the name first, a
        /// mixed-case name does NOT match what a standards-compliant implementation would produce for that
        /// same name. This test exists so the trade-off is visible rather than discovered during interop.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_WithMixedCaseName_DeviatesFromCanonicalRfcResult()
        {
            // The canonical RFC 4122 v5 value for the mixed-case name "Python.org" under the DNS namespace,
            // as produced by a standards-compliant implementation that does not normalize casing.
            var canonicalMixedCaseResult = new Guid( "cb620f2d-413b-52b6-a026-e87bac9b6f47" );

            var result = "Python.org".ToGuidV5( _dnsNamespace );

            Assert.AreNotEqual( canonicalMixedCaseResult, result );

            // Instead it matches the all-lower-case name, which is the behavior we want.
            Assert.AreEqual( "python.org".ToGuidV5( _dnsNamespace ), result );
        }

        /// <summary>
        /// The whole point of a version 5 Guid is that it is derived, not random, so the same inputs must
        /// always produce the same output.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_CalledRepeatedly_ReturnsSameGuid()
        {
            var input = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53";

            var first = input.ToGuidV5( _dnsNamespace );
            var second = input.ToGuidV5( _dnsNamespace );

            Assert.AreEqual( first, second );
        }

        /// <summary>
        /// The generated Guid must carry version 5 in the third group and the RFC 4122 variant bits
        /// ("10") in the fourth group.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_ForAnyInput_SetsVersionAndVariantBits()
        {
            var result = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53".ToGuidV5( _dnsNamespace ).ToString( "D" );

            Assert.AreEqual( '5', result[14], $"Expected version 5 in '{result}'." );
            StringAssert.Contains( "89ab", result[19].ToString(), $"Expected RFC 4122 variant bits in '{result}'." );
        }

        /// <summary>
        /// The namespace scopes the result, so the same name under a different namespace must not collide.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_WithDifferentNamespace_ReturnsDifferentGuid()
        {
            var input = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53";
            var otherNamespace = new Guid( "6ba7b811-9dad-11d1-80b4-00c04fd430c8" );

            Assert.AreNotEqual( input.ToGuidV5( _dnsNamespace ), input.ToGuidV5( otherNamespace ) );
        }

        /// <summary>
        /// Different names under the same namespace must not collide.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_WithDifferentName_ReturnsDifferentGuid()
        {
            Assert.AreNotEqual( "alpha".ToGuidV5( _dnsNamespace ), "beta".ToGuidV5( _dnsNamespace ) );
        }

        /// <summary>
        /// The primary use is hashing Guid strings, which arrive in mixed casing depending on their source,
        /// so casing must not change the result.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_WithDifferingCase_ReturnsSameGuid()
        {
            var upper = "7E6286F7-0297-41FF-BDF6-BD5656E1BC53".ToGuidV5( _dnsNamespace );
            var lower = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53".ToGuidV5( _dnsNamespace );
            var mixed = "7e6286F7-0297-41ff-BDF6-bd5656e1BC53".ToGuidV5( _dnsNamespace );

            Assert.AreEqual( lower, upper );
            Assert.AreEqual( lower, mixed );
        }

        /// <summary>
        /// Only casing is normalized. Formatting differences still change the name and therefore the result,
        /// so callers must supply a consistently formatted value.
        /// </summary>
        [TestMethod]
        [DataRow( "{7e6286f7-0297-41ff-bdf6-bd5656e1bc53}", DisplayName = "Braces" )]
        [DataRow( "7e6286f7029741ffbdf6bd5656e1bc53", DisplayName = "No Hyphens" )]
        public void ToGuidV5_WithDifferentFormatting_ReturnsDifferentGuid( string differentlyFormatted )
        {
            var standardFormat = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53".ToGuidV5( _dnsNamespace );

            Assert.AreNotEqual( standardFormat, differentlyFormatted.ToGuidV5( _dnsNamespace ) );
        }

        /// <summary>
        /// An empty name is still hashable and must produce a stable, non-empty Guid.
        /// </summary>
        [TestMethod]
        public void ToGuidV5_WithEmptyName_ReturnsStableNonEmptyGuid()
        {
            var result = string.Empty.ToGuidV5( _dnsNamespace );

            Assert.AreNotEqual( Guid.Empty, result );
            Assert.AreEqual( result, string.Empty.ToGuidV5( _dnsNamespace ) );
        }

        #endregion
    }
}
