using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Extension methods for the MSTest Assert class.
    /// This class also re-implements the standard Assert methods so that they can be used with the "Assert.This" syntax.
    /// </summary>
    public static partial class AssertExtensions
    {
        /// <summary>
        /// Asserts that the two strings can be considered equivalent regardless of newlines.
        /// </summary>
        /// <param name="expected">The expected string.</param>
        /// <param name="actual">The actual sting.</param>
        public static void AreEqualIgnoreNewline( this Assert assert, string expected, string actual )
        {
            if ( expected == null && actual == null )
            {
                return;
            }
            else if ( expected == null )
            {
                throw new NullReferenceException( "The expected string was null" );
            }
            else if ( actual == null )
            {
                throw new NullReferenceException( "The actual string was null" );
            }

            Assert.AreEqual( expected.Trim().ToStripNewlines(), actual.Trim().ToStripNewlines() );
        }

        /// <summary>
        /// Asserts that the two strings can be considered equivalent regardless of whitespace.
        /// </summary>
        /// <param name="expected">The expected string.</param>
        /// <param name="actual">The actual sting.</param>
        public static void AreEqualIgnoreWhitespace( this Assert assert, string expected, string actual )
        {
            if ( expected == null && actual == null )
            {
                return;
            }
            else if ( expected == null )
            {
                throw new NullReferenceException( "The expected string was null" );
            }
            else if ( actual == null )
            {
                throw new NullReferenceException( "The actual string was null" );
            }

            expected = Regex.Replace( expected, @"\s*", string.Empty );
            actual = Regex.Replace( actual, @"\s*", string.Empty );

            Assert.AreEqual( expected, actual );
        }

        public static T ThrowsWithMessage<T>( this Assert assert, Action action, string expectedMessage )
            where T : Exception
        {
            try
            {
                action();
            }
            catch ( T ex )
            {
                if ( !ex.Message.Equals( expectedMessage, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    Assert.Fail( $"Excepted error message to be {expectedMessage}, but it was {ex.Message}" );
                }

                return ex;
            }

            Assert.Fail( $"A {typeof( T )} exception was expected but was not thrown." );

            // Doesn't actually do anything, but makes the compiler happy.
            return ( T ) null;
        }

        #region helpers

        private static string ToStripNewlines( this string s )
        {
            if ( s == null )
            {
                return string.Empty;
            }

            return s.Replace( "\r", "" ).Replace( "\n", "" ).ToString( CultureInfo.InvariantCulture );
        }
        #endregion

        #region Pattern Matches

        /// <summary>
        /// Asserts that the two strings can be considered equivalent if a pattern containing one or more wildcards is matched.
        /// </summary>
        /// <param name="expected">The expected string, containing one or more wildcards.</param>
        /// <param name="actual">The actual string.</param>
        public static void MatchesWildcard( this Assert assert, string expected, string actual, bool ignoreCase = false, bool ignoreWhiteSpace = false, string wildcard = "*" )
        {
            var expectedOutput = expected;

            // If ignoring whitespace, strip it from the comparison strings.
            if ( ignoreWhiteSpace )
            {
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
                actual = Regex.Replace( actual, @"\s*", string.Empty );
            }

            // Replace wildcards with a non-Regex symbol.
            expectedOutput = expectedOutput.Replace( wildcard, "<<<wildCard>>>" );

            expectedOutput = Regex.Escape( expectedOutput );

            // Require a match of 1 or more characters for a wildcard.
            expectedOutput = expectedOutput.Replace( "<<<wildCard>>>", "(.+)" );

            // Add anchors for the start and end of the string, to ensure that the entire string is matched.
            // If the caller wants to match a substring, they can place a wildcard at the start and end of the string.
            expectedOutput = "^" + expectedOutput + "$";

            // Allow the wildcard regex to match newlines.
            var options = RegexOptions.Singleline;

            if ( ignoreCase )
            {
                options = options | RegexOptions.IgnoreCase;
            }

            var regex = new Regex( expectedOutput, options );

            StringAssert.Matches( actual, regex );
        }

        #endregion
    }
}
