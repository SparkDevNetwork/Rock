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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Doors;

namespace Rock.Tests.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// What survives of a profile card template's output before another person's browser draws it.
    /// </summary>
    /// <remarks>
    /// Lava prints a person's own fields unescaped, so each case here is something a person could
    /// put in their nickname, or a template could print, to run script in everyone who opens their
    /// card. Each bypass is one a blocklist sanitiser has missed somewhere.
    /// </remarks>
    [TestClass]
    public class ChatProfileHtmlTests
    {
        #region What is kept

        [TestMethod]
        public void KeepsTextStructureListsAndClasses()
        {
            var html = "<div class=\"lh\">Serving in</div><ul><li><b>Worship</b> <span title=\"t\">9 AM</span></li></ul><p><strong>a</strong><em>b</em><i>c</i><small>d</small><br></p><h3>x</h3><ol><li>1</li></ol>";

            Assert.AreEqual( html, ChatProfileHtml.Sanitize( html ) );
        }

        [TestMethod]
        public void KeepsLinksOfTheAllowedSchemesAndForcesNoOpener()
        {
            Assert.AreEqual( "<a href=\"https://example.org/x?a=1&amp;b=2\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"https://example.org/x?a=1&b=2\">x</a>" ) );
            Assert.AreEqual( "<a href=\"http://example.org\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"http://example.org\">x</a>" ) );
            Assert.AreEqual( "<a href=\"mailto:a@b.org\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"mailto:a@b.org\">x</a>" ) );
            Assert.AreEqual( "<a href=\"tel:+16025550142\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"tel:+16025550142\">x</a>" ) );
            Assert.AreEqual( "<a href=\"/page/12?GroupId=4\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"/page/12?GroupId=4\">x</a>" ) );
        }

        [TestMethod]
        public void ReplacesTheTemplatesOwnRelAndTarget()
        {
            Assert.AreEqual( "<a href=\"https://e.org\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href=\"https://e.org\" rel=\"opener\" target=\"_blank\">x</a>" ) );
        }

        [TestMethod]
        public void KeepsAnHttpsImageWithItsAltText()
        {
            Assert.AreEqual( "<img src=\"https://e.org/a.png\" alt=\"A\">", ChatProfileHtml.Sanitize( "<img src=\"https://e.org/a.png\" alt=\"A\" width=\"900\">" ) );
        }

        [TestMethod]
        public void BlankInputIsEmpty()
        {
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( null ) );
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "  \n" ) );
        }

        [TestMethod]
        public void TextIsEscapedAsWritten()
        {
            Assert.AreEqual( "<p>Tom &amp; Jerry &lt;3</p>", ChatProfileHtml.Sanitize( "<p>Tom &amp; Jerry &lt;3</p>" ) );
        }

        #endregion What is kept

        #region Script

        [TestMethod]
        public void DropsScriptWithItsContents()
        {
            Assert.AreEqual( "<b>Ann</b>", ChatProfileHtml.Sanitize( "<b>Ann</b><script>alert(1)</script>" ) );
        }

        [TestMethod]
        public void DropsEveryEventHandlerAttribute()
        {
            Assert.AreEqual( "<span>Ann</span>", ChatProfileHtml.Sanitize( "<span onmouseover=\"alert(1)\" ONCLICK=\"alert(2)\">Ann</span>" ) );
        }

        [TestMethod]
        public void AnImageWithAHandlerAndNoSafeSourceIsDropped()
        {
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=x onerror=\"alert(1)\">" ) );
        }

        [TestMethod]
        public void DropsAScriptLinkHoweverItIsSpelled()
        {
            var spellings = new[]
            {
                "javascript:alert(1)",
                "JavaScript:alert(1)",
                "  javascript:alert(1)",
                "\u0001javascript:alert(1)",
                "java\tscript:alert(1)",
                "java\nscript:alert(1)",
                "java&#9;script:alert(1)",
                "java&#x0A;script:alert(1)",
                "&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;alert(1)",
                "&#x6A;avascript&colon;alert(1)",
                "javascript&#58;alert(1)",
                "vbscript:msgbox(1)",
                "data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==",
                "DATA:text/html,<script>alert(1)</script>",
                "\\\\evil.example/x"
            };

            foreach ( var href in spellings )
            {
                var html = "<a href=\"" + href.Replace( "\"", "&quot;" ) + "\">Ann</a>";
                Assert.AreEqual( "<a rel=\"noopener noreferrer\">Ann</a>", ChatProfileHtml.Sanitize( html ), href );
            }
        }

        [TestMethod]
        public void DropsAnImageThatIsNotHttps()
        {
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=\"http://e.org/a.png\">" ) );
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=\"data:image/svg+xml,&lt;svg onload=alert(1)&gt;\">" ) );
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=\"/GetImage.ashx?id=1\">" ) );
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=\"javascript:alert(1)\">" ) );
        }

        [TestMethod]
        public void DropsSvgAndMathWithEverythingInThem()
        {
            Assert.AreEqual( "<b>x</b>", ChatProfileHtml.Sanitize( "<svg><animate onbegin=\"alert(1)\" attributeName=\"x\" dur=\"1s\"/><a xlink:href=\"javascript:alert(1)\"><text>t</text></a></svg><b>x</b>" ) );
            Assert.AreEqual( "<b>x</b>", ChatProfileHtml.Sanitize( "<svg><set attributeName=\"href\" to=\"javascript:alert(1)\"/></svg><b>x</b>" ) );
            Assert.AreEqual( "<b>x</b>", ChatProfileHtml.Sanitize( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)></style></mglyph></table></mtext></math><b>x</b>" ) );
        }

        [TestMethod]
        public void DropsEmbeddedDocumentsAndForms()
        {
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<iframe src=\"javascript:alert(1)\"></iframe><object data=\"x\"></object><embed src=\"x\">" ) );
            Assert.AreEqual( "Go", ChatProfileHtml.Sanitize( "<form action=\"javascript:alert(1)\"><button formaction=\"javascript:alert(1)\">Go</button></form>" ) );
        }

        [TestMethod]
        public void DropsCommentsAndConditionalComments()
        {
            Assert.AreEqual( "<b>x</b>", ChatProfileHtml.Sanitize( "<!--<img src=x onerror=alert(1)>--><b>x</b><!--[if IE]><script>alert(1)</script><![endif]-->" ) );
        }

        [TestMethod]
        public void AnUnclosedOrMisnestedTagCannotCarryMarkupOut()
        {
            // The parser closes what is left open, and the output is written from its tree.
            Assert.AreEqual( "<b>Ann<i>x</i></b>", ChatProfileHtml.Sanitize( "<b>Ann<i>x" ) );
            Assert.AreEqual( "<p>&lt;img src=x onerror=alert(1)&gt;</p>", ChatProfileHtml.Sanitize( "<p>&lt;img src=x onerror=alert(1)&gt;</p>" ) );
            Assert.AreEqual( "<span title=\"&quot;&gt;&lt;img src=x onerror=alert(1)&gt;\">a</span>", ChatProfileHtml.Sanitize( "<span title='\"><img src=x onerror=alert(1)>'>a</span>" ) );
        }

        [TestMethod]
        public void AttributesAreNeverWrittenRaw()
        {
            Assert.AreEqual( "<a href=\"https://e.org/&quot;onmouseover=&quot;alert(1)\" rel=\"noopener noreferrer\">x</a>", ChatProfileHtml.Sanitize( "<a href='https://e.org/\"onmouseover=\"alert(1)'>x</a>" ) );
        }

        #endregion Script

        #region The page around the card

        [TestMethod]
        public void DropsStyleInEveryForm()
        {
            Assert.AreEqual( "<div>x</div>", ChatProfileHtml.Sanitize( "<style>body{display:none}</style><div style=\"position:fixed;inset:0\">x</div>" ) );
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<link rel=\"stylesheet\" href=\"https://e.org/a.css\">" ) );
        }

        [TestMethod]
        public void DropsBaseAndMeta()
        {
            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<base href=\"https://evil.example/\"><meta http-equiv=\"refresh\" content=\"0;url=https://evil.example\">" ) );
        }

        [TestMethod]
        public void DropsIdsSoATemplateCannotShadowThePagesElements()
        {
            Assert.AreEqual( "<div>x</div>", ChatProfileHtml.Sanitize( "<div id=\"chat\" name=\"x\" data-x=\"1\">x</div>" ) );
        }

        [TestMethod]
        public void KeepsTheWordsOfAnElementNotAllowed()
        {
            Assert.AreEqual( "Serving <b>Kids</b>", ChatProfileHtml.Sanitize( "<section><table><tr><td>Serving <b>Kids</b></td></tr></table></section>" ) );
        }

        [TestMethod]
        public void DeepNestingIsCutToItsText()
        {
            var html = new string( 'x', 0 );
            for ( var i = 0; i < 200; i++ )
            {
                html = "<span>" + html + "</span>";
            }

            html = html.Replace( "<span></span>", "<span>&lt;script&gt;deep</span>" );

            var result = ChatProfileHtml.Sanitize( html );

            Assert.IsFalse( result.Contains( "<script" ), result );
            Assert.IsTrue( result.Contains( "&lt;script&gt;deep" ), result );
        }

        #endregion The page around the card
    }
}
