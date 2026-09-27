// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Lava.Shared
{
    /// <summary>
    /// Assertions for Lava render output that plain MSTest cannot express.
    /// </summary>
    /// <remarks>
    /// Most Lava tests should assert with <c>Assert.AreEqual</c>,
    /// <c>Assert.Contains</c> or <c>Assert.DoesNotContain</c> against the string
    /// returned by <see cref="LavaRenderTestHelper.Render"/>. Only reach for this
    /// class when the expected output contains a value the test cannot know in
    /// advance, such as a generated identifier.
    /// </remarks>
    public static class LavaAssert
    {
        #region Fields

        /// <summary>
        /// Stands in for a wildcard while the rest of the expected text is escaped.
        /// It must be a sequence that Regex.Escape leaves alone and that no
        /// realistic template would emit.
        /// </summary>
        private const string WildcardPlaceholder = "zzWILDCARDzz";

        #endregion Fields

        #region Methods

        /// <summary>
        /// Asserts that the output matches the expected text, treating each
        /// wildcard token in the expected text as "one or more characters".
        /// </summary>
        /// <remarks>
        /// Everything outside a wildcard is matched literally, so spacing and
        /// punctuation are still significant. Both values have their line
        /// terminators normalized first, because a multi-line expected literal
        /// carries the line terminator of the source file it was written in.
        /// </remarks>
        /// <param name="expected">The expected text, containing one or more wildcard tokens.</param>
        /// <param name="actual">The rendered output.</param>
        /// <param name="wildcards">The wildcard tokens appearing in <paramref name="expected"/>, for example "&lt;guid&gt;".</param>
        public static void Matches( string expected, string actual, params string[] wildcards )
        {
            Assert.IsNotNull( expected, "The expected value cannot be null." );
            Assert.IsNotNull( actual, "The rendered output cannot be null." );

            if ( wildcards == null || !wildcards.Any() )
            {
                Assert.Fail( $"{nameof( Matches )} requires at least one wildcard token. Use Assert.AreEqual for an exact comparison." );
            }

            var pattern = BuildPattern( expected.NormalizeLineEndings(), wildcards );
            var comparand = actual.NormalizeLineEndings();

            // The default options leave "." unable to match a newline, which keeps a
            // wildcard confined to the line it appears on rather than letting it
            // swallow the rest of the output.
            if ( !Regex.IsMatch( comparand, pattern ) )
            {
                Assert.Fail( $"Rendered output did not match the expected text.\nExpected (wildcards: {string.Join( ", ", wildcards )}):\n{expected}\n\nActual:\n{actual}" );
            }
        }

        /// <summary>
        /// Asserts that the output contains each expected fragment, comparing
        /// with every whitespace character removed from both sides.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is for output whose layout is not what the test is about, such
        /// as the script a shortcode emits, where the fragment being looked for
        /// spans several lines and the indentation around it is incidental. An
        /// assertion that cares about spacing should use <c>Assert.Contains</c>
        /// against the rendered text instead.
        /// </para>
        /// <para>
        /// Wildcards are optional here, unlike <see cref="Matches"/>, because a
        /// fragment may well be fixed text.
        /// </para>
        /// </remarks>
        /// <param name="expectedFragments">The fragments the output must contain.</param>
        /// <param name="actual">The rendered output.</param>
        /// <param name="wildcards">Wildcard tokens appearing in the fragments, for example "&lt;guid&gt;".</param>
        public static void ContainsAllIgnoringWhitespace( IEnumerable<string> expectedFragments, string actual, params string[] wildcards )
        {
            Assert.IsNotNull( expectedFragments, "The expected fragments cannot be null." );
            Assert.IsNotNull( actual, "The rendered output cannot be null." );

            var fragments = expectedFragments.ToList();

            Assert.IsNotEmpty( fragments, "At least one expected fragment is required." );

            var comparand = RemoveWhitespace( actual );
            var tokens = wildcards ?? new string[0];

            foreach ( var fragment in fragments )
            {
                var pattern = BuildFragmentPattern( RemoveWhitespace( fragment ), tokens );

                if ( !Regex.IsMatch( comparand, pattern ) )
                {
                    Assert.Fail( $"Rendered output did not contain the expected fragment.\nFragment:\n{fragment}\n\nActual:\n{actual}" );
                }
            }
        }

        /// <summary>
        /// Removes every whitespace character, so that two values can be
        /// compared on their content alone.
        /// </summary>
        /// <param name="value">The value to reduce.</param>
        /// <returns>The value with no whitespace in it.</returns>
        private static string RemoveWhitespace( string value )
        {
            return Regex.Replace( value ?? string.Empty, @"\s", string.Empty );
        }

        /// <summary>
        /// Asserts that the rendered output parses as a date and represents the
        /// expected moment in time.
        /// </summary>
        /// <remarks>
        /// A rendered date cannot be compared as text: the same moment renders
        /// differently depending on the format the template asked for and the
        /// organization time zone in force. The output is parsed back to an offset
        /// and compared as an instant instead.
        /// </remarks>
        /// <param name="expected">The expected moment, or <c>null</c> to require the output not parse.</param>
        /// <param name="actual">The rendered output.</param>
        /// <param name="maximumDelta">How far apart the two may be and still match. Exact when not supplied.</param>
        public static void DateEqual( DateTimeOffset? expected, string actual, TimeSpan? maximumDelta = null )
        {
            var actualDate = LavaDateTime.ParseToOffset( actual, null );

            if ( expected == null )
            {
                // A null expectation is the request that the output not be a date
                // at all, so the comparison below has nothing to compare.
                Assert.IsNull( actualDate, $"Template output represents a DateTime, but none was expected. [Output=\"{actual}\"]" );

                return;
            }

            Assert.IsNotNull( actualDate, $"Template output does not represent a valid DateTime. [Output=\"{actual}\"]" );

            try
            {
                if ( maximumDelta != null )
                {
                    DateTimeAssert.AreEqual( expected, actualDate, maximumDelta.Value );
                }
                else
                {
                    DateTimeAssert.AreEqual( expected, actualDate );
                }
            }
            catch ( Exception ex )
            {
                // A date comparison that fails is almost always a time zone
                // problem, so report the zones in force rather than just the values.
                var info = $@"
Test Environment:
LocalDateTime = {DateTimeOffset.Now},
LocalTimeZoneName = {TimeZoneInfo.Local.DisplayName},
LocalTimeZoneOffset = {TimeZoneInfo.Local.BaseUtcOffset}
RockDateTime = {LavaDateTime.NowOffset},
RockTimeZoneName = {RockDateTime.OrgTimeZoneInfo.DisplayName},
RockTimeZoneOffset = {RockDateTime.OrgTimeZoneInfo.BaseUtcOffset}
";
                throw new Exception( $"Lava Date/Time test failed.\n{info}", ex );
            }
        }

        /// <summary>
        /// Asserts that the rendered output represents the expected moment,
        /// interpreting the expected value as a Rock time.
        /// </summary>
        /// <param name="expected">The expected moment, expressed in Rock time, or <c>null</c> to require the output not parse.</param>
        /// <param name="actual">The rendered output.</param>
        /// <param name="maximumDelta">How far apart the two may be and still match. Exact when not supplied.</param>
        public static void DateEqual( DateTime? expected, string actual, TimeSpan? maximumDelta = null )
        {
            // Null is passed through rather than substituted, so that it carries
            // the same "should not parse" meaning it has on the overload below.
            // Substituting DateTime.MinValue would silently assert a real moment.
            DateTimeOffset? expectedOffset = expected == null
                ? ( DateTimeOffset? ) null
                : LavaDateTime.ConvertToRockOffset( expected.Value );

            DateEqual( expectedOffset, actual, maximumDelta );
        }

        /// <summary>
        /// Asserts that the rendered output represents the same moment as the
        /// expected date string, which is parsed as a Rock time.
        /// </summary>
        /// <param name="expected">The expected date, in any format Rock can parse.</param>
        /// <param name="actual">The rendered output.</param>
        /// <param name="maximumDelta">How far apart the two may be and still match. Exact when not supplied.</param>
        public static void DateEqual( string expected, string actual, TimeSpan? maximumDelta = null )
        {
            var expectedDate = LavaDateTime.ParseToOffset( expected );

            // An expected value that does not parse is a mistake in the test, not
            // a request that the output not be a date. Say so, rather than passing
            // a null through to the overload below, where it would assert the
            // opposite of what the test meant.
            Assert.IsNotNull( expectedDate, $"The expected value is not a valid DateTime. [Expected=\"{expected}\"]" );

            DateEqual( expectedDate, actual, maximumDelta );
        }

        /// <summary>
        /// Builds an anchored regular expression in which every wildcard token
        /// matches one or more characters and all other text matches literally.
        /// </summary>
        /// <param name="expected">The expected text, with line terminators already normalized.</param>
        /// <param name="wildcards">The wildcard tokens to substitute.</param>
        /// <returns>The regular expression pattern.</returns>
        private static string BuildPattern( string expected, IEnumerable<string> wildcards )
        {
            return "^" + BuildFragmentPattern( expected, wildcards ) + "$";
        }

        /// <summary>
        /// Builds a pattern that matches the expected text anywhere in a larger
        /// string, with each wildcard standing for one or more characters.
        /// </summary>
        /// <param name="expected">The expected text.</param>
        /// <param name="wildcards">The wildcard tokens appearing in the expected text.</param>
        /// <returns>The pattern.</returns>
        private static string BuildFragmentPattern( string expected, IEnumerable<string> wildcards )
        {
            // Swap each wildcard for a placeholder that survives Regex.Escape, escape
            // everything else so it matches literally, then expand the placeholders.
            var pattern = expected;

            foreach ( var wildcard in wildcards )
            {
                pattern = pattern.Replace( wildcard, WildcardPlaceholder );
            }

            return Regex.Escape( pattern ).Replace( WildcardPlaceholder, "(.+)" );
        }

        #endregion Methods
    }
}
