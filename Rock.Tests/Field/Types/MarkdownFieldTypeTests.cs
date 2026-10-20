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
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field;
using Rock.Field.Types;
using Rock.Tests.Shared;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="MarkdownFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class MarkdownFieldTypeTests
    {
        private static string GetHtmlValue( string markdown )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                return new MarkdownFieldType().GetHtmlValue( markdown, new Dictionary<string, string>() );
            }
        }

        private static string GetCondensedHtmlValue( string markdown )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                return new MarkdownFieldType().GetCondensedHtmlValue( markdown, new Dictionary<string, string>() );
            }
        }

        #region Markdown Formatting

        [TestMethod]
        [DataRow( "**b**", "<strong>b</strong>" )]
        [DataRow( "*i*", "<em>i</em>" )]
        [DataRow( "# One", "<h1>One</h1>" )]
        [DataRow( "###### Six", "<h6>Six</h6>" )]
        [DataRow( "1. a\n2. b", "<ol>\n<li>a</li>\n<li>b</li>\n</ol>" )]
        [DataRow( "3. a", "<ol start=\"3\">" )]
        [DataRow( "- a\n- b", "<ul>\n<li>a</li>\n<li>b</li>\n</ul>" )]
        [DataRow( "> q", "<blockquote>\n<p>q</p>\n</blockquote>" )]
        [DataRow( "```csharp\nvar x = 1;\n```", "<pre><code class=\"language-csharp\">var x = 1;\n</code></pre>" )]
        [DataRow( "[l](http://example.org)", "<a href=\"http://example.org\">l</a>" )]
        [DataRow( "[l](https://example.org)", "<a href=\"https://example.org\">l</a>" )]
        [DataRow( "[l](mailto:a@example.org)", "<a href=\"mailto:a@example.org\">l</a>" )]
        [DataRow( "[l](tel:5551234)", "<a href=\"tel:5551234\">l</a>" )]
        [DataRow( "[l](/page/12)", "<a href=\"/page/12\">l</a>" )]
        [DataRow( "![alt text](https://example.org/a.png)", "<img src=\"https://example.org/a.png\" alt=\"alt text\">" )]
        public void GetHtmlValue_KeepsMarkdownFormatting( string markdown, string expected )
        {
            var html = GetHtmlValue( markdown );

            Assert.IsTrue( html.Contains( expected ), html );
        }

        [TestMethod]
        public void GetHtmlValue_KeepsTables()
        {
            var html = GetHtmlValue( "<table><thead><tr><th colspan=\"2\">h</th></tr></thead><tbody><tr><td rowspan=\"2\">c</td></tr></tbody><tfoot><tr><td>f</td></tr></tfoot><caption>t</caption></table>" );

            Assert.IsTrue( html.Contains( "<th colspan=\"2\">h</th>" ), html );
            Assert.IsTrue( html.Contains( "<td rowspan=\"2\">c</td>" ), html );
            Assert.IsTrue( html.Contains( "<tfoot>" ), html );
            Assert.IsTrue( html.Contains( "<caption>t</caption>" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_KeepsTextAsTyped()
        {
            var html = GetHtmlValue( "5 < 6 & 7 \"q\" 'a'" );

            Assert.AreEqual( "5 < 6 & 7 \"q\" 'a'", System.Net.WebUtility.HtmlDecode( Regex.Replace( html, "<[^>]+>", string.Empty ) ).Trim() );
            Assert.IsFalse( html.Contains( "&amp;amp;" ), html );
            Assert.IsFalse( html.Contains( "&amp;lt;" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_EmptyValue_ReturnsEmptyString()
        {
            Assert.AreEqual( string.Empty, GetHtmlValue( string.Empty ) );
            Assert.AreEqual( string.Empty, GetHtmlValue( null ) );
            Assert.AreEqual( string.Empty, GetCondensedHtmlValue( string.Empty ) );
        }

        #endregion

        #region Unsafe Markup

        [TestMethod]
        public void GetHtmlValue_RawImageWithEventHandler_KeepsSourceAndAlt()
        {
            var html = GetHtmlValue( "<img src=\"https://example.org/a.png\" alt=\"a\" onerror=\"alert(1)\">" );

            Assert.IsTrue( html.Contains( "src=\"https://example.org/a.png\"" ), html );
            Assert.IsTrue( html.Contains( "alt=\"a\"" ), html );
            AssertNoLiveEventHandler( html );
        }

        [TestMethod]
        [DataRow( "[x](javascript:alert(1))" )]
        [DataRow( "[x](JaVaScRiPt:alert(1))" )]
        [DataRow( "<a href=\"javascript:alert(1)\">x</a>" )]
        [DataRow( "<a href=\"java\tscript:alert(1)\">x</a>" )]
        [DataRow( "<a href=\"&#106avascript:alert(1)\">x</a>" )]
        [DataRow( "<a href=\" javascript:alert(1)\">x</a>" )]
        public void GetHtmlValue_ScriptLink_LosesHref( string markdown )
        {
            var html = GetHtmlValue( markdown );

            Assert.IsFalse( html.Contains( "href" ), html );
            Assert.IsTrue( html.Contains( "x" ), html );
        }

        [TestMethod]
        [DataRow( "<iframe src=\"https://example.org\"></iframe>", "iframe" )]
        [DataRow( "<script>alert(1)</script>", "script" )]
        [DataRow( "<form action=\"https://example.org\"><input name=\"a\"></form>", "form" )]
        [DataRow( "<button>b</button>", "button" )]
        [DataRow( "<input type=\"text\">", "input" )]
        [DataRow( "<svg><circle r=\"5\"></circle></svg>", "svg" )]
        public void GetHtmlValue_RemovesUnsafeElements( string markdown, string tag )
        {
            var html = GetHtmlValue( markdown );

            Assert.IsFalse( Regex.IsMatch( html, $"<{tag}\\b", RegexOptions.IgnoreCase ), html );
            Assert.IsFalse( html.Contains( "alert(1)" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_StyleAttribute_KeepsOnlyAllowedCss()
        {
            var html = GetHtmlValue( "<div style=\"color: red; position: fixed; z-index: 9999\">d</div>" );

            Assert.IsTrue( html.Contains( "color" ), html );
            Assert.IsFalse( html.Contains( "position" ), html );
            Assert.IsFalse( html.Contains( "z-index" ), html );
            Assert.IsTrue( html.Contains( ">d</div>" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_RemovesIdAndDataAttributes()
        {
            var html = GetHtmlValue( "<div id=\"x\" data-toggle=\"modal\">d</div>" );

            Assert.IsFalse( html.Contains( "id=" ), html );
            Assert.IsFalse( html.Contains( "data-" ), html );
            Assert.IsTrue( html.Contains( ">d</div>" ), html );
        }

        [TestMethod]
        public void GetPublicConfigurationValues_HtmlValueIsSanitized()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new MarkdownFieldType().GetPublicConfigurationValues( new Dictionary<string, string>(), ConfigurationValueUsage.View, "<img src=\"https://example.org/a.png\" onerror=\"alert(1)\">" );

                AssertNoLiveEventHandler( config["htmlvalue"] );
            }
        }

        #endregion

        #region Condensed Value

        [TestMethod]
        public void GetCondensedHtmlValue_RemovesUnsafeMarkup()
        {
            var html = GetCondensedHtmlValue( "<img src=\"https://example.org/a.png\" onerror=\"alert(1)\"> [x](javascript:alert(1))" );

            AssertNoLiveEventHandler( html );
            Assert.IsFalse( html.Contains( "href" ), html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_LongValue_IsTruncatedAndWellFormed()
        {
            var html = GetCondensedHtmlValue( "**" + new string( 'x', 40 ) + " " + new string( 'y', 200 ) + "**" );

            Assert.IsTrue( html.Length < 200, html );
            Assert.AreEqual( Regex.Matches( html, "<strong>" ).Count, Regex.Matches( html, "</strong>" ).Count, html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_TagCutByTruncation_IsNotLive()
        {
            // The last space before the truncation length falls inside the tag,
            // so truncating leaves "<img ... onerror=alert(1)..." unterminated.
            var html = GetCondensedHtmlValue( new string( 'x', 40 ) + " <img src=x onerror=alert(1) title=" + new string( 'y', 60 ) + ">" );

            AssertNoLiveEventHandler( html );
        }

        #endregion

        /// <summary>
        /// Asserts that the HTML has no tag carrying an "on..." event handler attribute.
        /// </summary>
        /// <param name="html">The HTML to check.</param>
        private static void AssertNoLiveEventHandler( string html )
        {
            Assert.IsFalse( Regex.IsMatch( html, @"<[a-z][^<>]*\son[a-z]+\s*=", RegexOptions.IgnoreCase ), html );
        }
    }
}
