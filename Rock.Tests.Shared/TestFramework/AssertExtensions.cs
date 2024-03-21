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

        public static void AreEqual<T>( this Assert assert, T expected, T actual )
        {
            Assert.AreEqual( expected, actual );
        }
        public static void AreEqual( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase )
        {
            Assert.AreEqual( expected, actual, ignoreCase );
        }

        public static void Contains( this Assert assert, System.String value, System.String substring )
        {
            StringAssert.Contains( value, substring );
        }

        public static void DoesNotContain( this Assert assert, System.String value, System.String substring )
        {
            if ( value == null && substring == null )
            {
                throw new NullReferenceException( "Null value is unexpected." );
            }
            else if ( value == null )
            {
                return;
            }
            else if ( substring == null )
            {
                return;
            }

            Assert.IsFalse( value.Contains( substring ), $"The result \"{ value }\" contains the unexpected value \"{ substring }\"." );
        }

        public static void AreEqual( this Assert assert, System.Single expected, System.Single actual, System.Single delta )
        {
            Assert.AreEqual( expected, actual, delta );
        }
        public static void AreEqual( this Assert assert, System.Double expected, System.Double actual, System.Double delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, delta, message, parameters );
        }
        public static void AreEqual( this Assert assert, System.Object expected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, message, parameters );
        }
        public static void AreEqual( this Assert assert, System.Single expected, System.Single actual, System.Single delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, delta, message, parameters );
        }
        public static void AreEqual<T>( this Assert assert, T expected, T actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, message, parameters );
        }
        public static void AreEqual( this Assert assert, System.Double expected, System.Double actual, System.Double delta )
        {
            Assert.AreEqual( expected, actual, delta );
        }
        public static void AreEqual( this Assert assert, System.Object expected, System.Object actual )
        {
            Assert.AreEqual( expected, actual );
        }
        public static void AreEqual( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture )
        {
            Assert.AreEqual( expected, actual, ignoreCase, culture );
        }
        public static void AreEqual( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, ignoreCase, culture, message, parameters );
        }
        public static void AreEqual( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, ignoreCase, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, culture );
        }
        public static void AreNotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, culture, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.Single notExpected, System.Single actual, System.Single delta )
        {
            Assert.AreNotEqual( notExpected, actual, delta );
        }
        public static void AreNotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase );
        }
        public static void AreNotEqual( this Assert assert, System.Double notExpected, System.Double actual, System.Double delta )
        {
            Assert.AreNotEqual( notExpected, actual, delta );
        }
        public static void AreNotEqual( this Assert assert, System.Double notExpected, System.Double actual, System.Double delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, delta, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.Single notExpected, System.Single actual, System.Single delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, delta, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, message, parameters );
        }
        public static void AreNotEqual<T>( this Assert assert, T notExpected, T actual )
        {
            Assert.AreNotEqual( notExpected, actual );
        }
        public static void AreNotEqual<T>( this Assert assert, T notExpected, T actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.Object notExpected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, message, parameters );
        }
        public static void AreNotEqual( this Assert assert, System.Object notExpected, System.Object actual )
        {
            Assert.AreNotEqual( notExpected, actual );
        }
        public static void AreNotSame( this Assert assert, System.Object notExpected, System.Object actual )
        {
            Assert.AreNotSame( notExpected, actual );
        }
        public static void AreNotSame( this Assert assert, System.Object notExpected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotSame( notExpected, actual, message, parameters );
        }
        public static void AreSame( this Assert assert, System.Object expected, System.Object actual )
        {
            Assert.AreSame( expected, actual );
        }
        public static void AreSame( this Assert assert, System.Object expected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreSame( expected, actual, message, parameters );
        }
        public static void Fail( this Assert assert )
        {
            Assert.Fail();
        }
        public static void Fail( this Assert assert, System.String message, params System.Object[] parameters )
        {
            Assert.Fail( message, parameters );
        }
        public static void Inconclusive( this Assert assert, System.String message, params System.Object[] parameters )
        {
            Assert.Inconclusive( message, parameters );
        }
        public static void Inconclusive( this Assert assert )
        {
            Assert.Inconclusive();
        }
        public static void IsFalse( this Assert assert, System.Boolean condition )
        {
            Assert.IsFalse( condition );
        }
        public static void IsFalse( this Assert assert, System.Boolean condition, System.String message, params System.Object[] parameters )
        {
            Assert.IsFalse( condition, message, parameters );
        }
        public static void IsInstanceOfType( this Assert assert, System.Object value, System.Type expectedType )
        {
            Assert.IsInstanceOfType( value, expectedType );
        }
        public static void IsInstanceOfType( this Assert assert, System.Object value, System.Type expectedType, System.String message, params System.Object[] parameters )
        {
            Assert.IsInstanceOfType( value, expectedType, message, parameters );
        }
        public static void IsNotInstanceOfType( this Assert assert, System.Object value, System.Type wrongType, System.String message, params System.Object[] parameters )
        {
            Assert.IsNotInstanceOfType( value, wrongType, message, parameters );
        }
        public static void IsNotInstanceOfType( this Assert assert, System.Object value, System.Type wrongType )
        {
            Assert.IsNotInstanceOfType( value, wrongType );
        }
        public static void IsNotNull( this Assert assert, System.Object value, System.String message, params System.Object[] parameters )
        {
            Assert.IsNotNull( value, message, parameters );
        }
        public static void IsNotNull( this Assert assert, System.Object value )
        {
            Assert.IsNotNull( value );
        }
        public static void IsNull( this Assert assert, System.Object value, System.String message, params System.Object[] parameters )
        {
            Assert.IsNull( value, message, parameters );
        }
        public static void IsNull( this Assert assert, System.Object value )
        {
            Assert.IsNull( value );
        }
        public static void IsTrue( this Assert assert, System.Boolean condition, System.String message, params System.Object[] parameters )
        {
            Assert.IsTrue( condition, message, parameters );
        }
        public static void IsTrue( this Assert assert, System.Boolean condition )
        {
            Assert.IsTrue( condition );
        }
        public static void ReplaceNullChars( this Assert assert, System.String input )
        {
            Assert.ReplaceNullChars( input );
        }
        public static T ThrowsException<T>( this Assert assert, Action action )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action );
        }
        public static T ThrowsException<T>( this Assert assert, Action action, string message )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action, message );
        }
        public static T ThrowsException<T>( this Assert assert, Func<object> action )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action );
        }
        public static T ThrowsException<T>( this Assert assert, Func<object> action, string message )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action, message );
        }

        public static T ThrowsException<T>( this Assert assert, Func<object> action, string message, params object[] parameters )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action, message, parameters );
        }

        public static T ThrowsException<T>( this Assert assert, Action action, string message, params object[] parameters )
            where T : Exception
        {
            return Assert.ThrowsException<T>( action, message, parameters );
        }

        public static T ThrowsExceptionWithMessage<T>( this Assert assert, Action action, string expectedMessage )
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

            Assert.Fail( $"A ${typeof( T )} exception was expected but was not thrown." );

            // Doesn't actually do anything, but makes the compiler happy.
            return ( T ) null;
        }

        #region Empty Assertions

        public static void IsEmpty( this Assert assert, string input )
        {
            if ( string.IsNullOrEmpty( input ) )
            {
                return;
            }

            throw new AssertFailedException( "Expected: (empty), Actual: (value)" );
        }

        public static void IsNotEmpty( this Assert assert, string input )
        {
            if ( !string.IsNullOrEmpty( input ) )
            {
                return;
            }

            throw new AssertFailedException( "Expected: (value), Actual: (empty)" );
        }

        public static void IsNotEmpty<T>( this Assert assert, ICollection<T> input )
        {
            if ( input != null && input.Any() )
            {
                return;
            }

            throw new AssertFailedException( "Collection is empty, but a value was expected." );
        }

        #endregion

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
