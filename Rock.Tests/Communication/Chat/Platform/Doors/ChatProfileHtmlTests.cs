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
using System.Linq;
using System.Text;
using System.Threading;

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

            Assert.AreEqual( html, Clean( html ) );
        }

        [TestMethod]
        public void KeepsLinksOfTheAllowedSchemesAndForcesNoOpener()
        {
            Assert.AreEqual( "<a href=\"https://church.example.org/x?a=1&amp;b=2\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"https://church.example.org/x?a=1&b=2\">x</a>" ) );
            Assert.AreEqual( "<a href=\"mailto:a@b.org\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"mailto:a@b.org\">x</a>" ) );
            Assert.AreEqual( "<a href=\"tel:+16025550142\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"tel:+16025550142\">x</a>" ) );
            Assert.AreEqual( "<a href=\"/page/12?GroupId=4\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"/page/12?GroupId=4\">x</a>" ) );
        }

        [TestMethod]
        public void ALinkOffTheChurchsSiteOpensBesideTheChat()
        {
            // Followed in place it would replace the chat page, and with it any message being typed.
            Assert.AreEqual( "<a href=\"https://example.org/x?a=1&amp;b=2\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"https://example.org/x?a=1&b=2\">x</a>" ) );
            Assert.AreEqual( "<a href=\"http://example.org\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"http://example.org\">x</a>" ) );
            Assert.AreEqual( "<a href=\"https://church.example.org.evil.example/\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"https://church.example.org.evil.example/\">x</a>" ) );
        }

        [TestMethod]
        public void ReplacesTheTemplatesOwnRelAndTarget()
        {
            Assert.AreEqual( "<a href=\"https://e.org\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"https://e.org\" rel=\"opener\" target=\"_self\">x</a>" ) );
            Assert.AreEqual( "<a href=\"/page/12\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href=\"/page/12\" target=\"_top\">x</a>" ) );
        }

        [TestMethod]
        public void KeepsAnImageFromTheChurchsOwnSiteWithItsAltText()
        {
            Assert.AreEqual( "<img src=\"https://church.example.org/a.png\" alt=\"A\">", Clean( "<img src=\"https://church.example.org/a.png\" alt=\"A\" width=\"900\">" ) );
            Assert.AreEqual( "<img src=\"https://ROCK.church.example.org/a.png\">", Clean( "<img src=\"https://ROCK.church.example.org/a.png\">" ) );
            Assert.AreEqual( "<img src=\"/GetImage.ashx?id=1&amp;width=80\">", Clean( "<img src=\"/GetImage.ashx?id=1&width=80\">" ) );
            Assert.AreEqual( "<img src=\"GetImage.ashx?guid=0f1e\">", Clean( "<img src=\"GetImage.ashx?guid=0f1e\">" ) );
        }

        [TestMethod]
        public void SiteHostsAreReadFromTheChurchsApplicationRoots()
        {
            var hosts = ChatProfileHtml.SiteHostsOf( "https://Church.Example.org/", "https://rock.church.example.org:8443/x", " ", null, "not a url" );

            CollectionAssert.AreEquivalent( new[] { "church.example.org", "rock.church.example.org" }, hosts.ToList() );
        }

        [TestMethod]
        public void BlankInputIsEmpty()
        {
            Assert.AreEqual( string.Empty, Clean( null ) );
            Assert.AreEqual( string.Empty, Clean( "  \n" ) );
        }

        [TestMethod]
        public void TextIsEscapedAsWritten()
        {
            Assert.AreEqual( "<p>Tom &amp; Jerry &lt;3</p>", Clean( "<p>Tom &amp; Jerry &lt;3</p>" ) );
        }

        #endregion What is kept

        #region Script

        [TestMethod]
        public void DropsScriptWithItsContents()
        {
            Assert.AreEqual( "<b>Ann</b>", Clean( "<b>Ann</b><script>alert(1)</script>" ) );
        }

        [TestMethod]
        public void DropsEveryEventHandlerAttribute()
        {
            Assert.AreEqual( "<span>Ann</span>", Clean( "<span onmouseover=\"alert(1)\" ONCLICK=\"alert(2)\">Ann</span>" ) );
        }

        [TestMethod]
        public void AnImageWithAHandlerKeepsOnlyItsSafeSource()
        {
            Assert.AreEqual( "<img src=\"x\">", Clean( "<img src=x onerror=\"alert(1)\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"javascript:x\" onerror=\"alert(1)\">" ) );
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
                Assert.AreEqual( "<a rel=\"noopener noreferrer\">Ann</a>", Clean( html ), href );
            }
        }

        [TestMethod]
        public void ALinkToAnotherHostWrittenAsAPathIsDropped()
        {
            // A browser reads "//host" and "/\host" as another site, not a path on this one.
            foreach ( var href in new[] { "//evil.example/x", " //evil.example", "/\\evil.example", "/\t/evil.example" } )
            {
                Assert.AreEqual( "<a rel=\"noopener noreferrer\">Ann</a>", Clean( "<a href=\"" + href + "\">Ann</a>" ), href );
            }
        }

        [TestMethod]
        public void DropsAnImageFromAnywhereButTheChurchsOwnSite()
        {
            // An image loads as the card opens, with no click, so one from elsewhere would tell its
            // host each time someone looked at this person's card.
            Assert.AreEqual( string.Empty, Clean( "<img src=\"https://a.co/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"https://church.example.org.evil.example/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"https://evil.example/church.example.org\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"https://church.example.org@evil.example/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"//evil.example/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"/\\evil.example/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"\\\\evil.example/x\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"http://church.example.org/a.png\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"data:image/svg+xml,&lt;svg onload=alert(1)&gt;\">" ) );
            Assert.AreEqual( string.Empty, Clean( "<img src=\"javascript:alert(1)\">" ) );
        }

        [TestMethod]
        public void AChurchWithNoApplicationRootLoadsImagesOnlyFromPaths()
        {
            var none = ChatProfileHtml.SiteHostsOf();

            Assert.AreEqual( string.Empty, ChatProfileHtml.Sanitize( "<img src=\"https://church.example.org/a.png\">", none ) );
            Assert.AreEqual( "<img src=\"/a.png\">", ChatProfileHtml.Sanitize( "<img src=\"/a.png\">", none ) );
        }

        [TestMethod]
        public void DropsSvgAndMathWithEverythingInThem()
        {
            Assert.AreEqual( "<b>x</b>", Clean( "<svg><animate onbegin=\"alert(1)\" attributeName=\"x\" dur=\"1s\"/><a xlink:href=\"javascript:alert(1)\"><text>t</text></a></svg><b>x</b>" ) );
            Assert.AreEqual( "<b>x</b>", Clean( "<svg><set attributeName=\"href\" to=\"javascript:alert(1)\"/></svg><b>x</b>" ) );
            Assert.AreEqual( "<b>x</b>", Clean( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)></style></mglyph></table></mtext></math><b>x</b>" ) );
        }

        [TestMethod]
        public void DropsEmbeddedDocumentsAndForms()
        {
            Assert.AreEqual( string.Empty, Clean( "<iframe src=\"javascript:alert(1)\"></iframe><object data=\"x\"></object><embed src=\"x\">" ) );
            Assert.AreEqual( "Go", Clean( "<form action=\"javascript:alert(1)\"><button formaction=\"javascript:alert(1)\">Go</button></form>" ) );
        }

        [TestMethod]
        public void DropsCommentsAndConditionalComments()
        {
            Assert.AreEqual( "<b>x</b>", Clean( "<!--<img src=x onerror=alert(1)>--><b>x</b><!--[if IE]><script>alert(1)</script><![endif]-->" ) );
        }

        [TestMethod]
        public void AnUnclosedOrMisnestedTagCannotCarryMarkupOut()
        {
            // The parser closes what is left open, and the output is written from its tree.
            Assert.AreEqual( "<b>Ann<i>x</i></b>", Clean( "<b>Ann<i>x" ) );
            Assert.AreEqual( "<p>&lt;img src=x onerror=alert(1)&gt;</p>", Clean( "<p>&lt;img src=x onerror=alert(1)&gt;</p>" ) );
            Assert.AreEqual( "<span title=\"&quot;&gt;&lt;img src=x onerror=alert(1)&gt;\">a</span>", Clean( "<span title='\"><img src=x onerror=alert(1)>'>a</span>" ) );
        }

        [TestMethod]
        public void AttributesAreNeverWrittenRaw()
        {
            Assert.AreEqual( "<a href=\"https://church.example.org/&quot;onmouseover=&quot;alert(1)\" rel=\"noopener noreferrer\">x</a>", Clean( "<a href='https://church.example.org/\"onmouseover=\"alert(1)'>x</a>" ) );
        }

        #endregion Script

        #region The page around the card

        [TestMethod]
        public void DropsStyleInEveryForm()
        {
            Assert.AreEqual( "<div>x</div>", Clean( "<style>body{display:none}</style><div style=\"position:fixed;inset:0\">x</div>" ) );
            Assert.AreEqual( string.Empty, Clean( "<link rel=\"stylesheet\" href=\"https://e.org/a.css\">" ) );
        }

        [TestMethod]
        public void DropsBaseAndMeta()
        {
            Assert.AreEqual( string.Empty, Clean( "<base href=\"https://evil.example/\"><meta http-equiv=\"refresh\" content=\"0;url=https://evil.example\">" ) );
        }

        [TestMethod]
        public void DropsIdsSoATemplateCannotShadowThePagesElements()
        {
            Assert.AreEqual( "<div>x</div>", Clean( "<div id=\"chat\" name=\"x\" data-x=\"1\">x</div>" ) );
        }

        [TestMethod]
        public void KeepsTheWordsOfAnElementNotAllowed()
        {
            Assert.AreEqual( "Serving <b>Kids</b>", Clean( "<section><table><tr><td>Serving <b>Kids</b></td></tr></table></section>" ) );
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

            var result = Clean( html );

            Assert.IsFalse( result.Contains( "<script" ), result );
            Assert.IsTrue( result.Contains( "&lt;script&gt;deep" ), result );
        }

        [TestMethod]
        public void DeepNestingOfElementsNotAllowedIsCutToItsTextToo()
        {
            // A nickname of thousands of unknown tags, printed by the template on every card open.
            var html = new StringBuilder();
            for ( var i = 0; i < 5000; i++ )
            {
                html.Append( "<x-a>" );
            }

            html.Append( "deep<b>er</b>" );

            // Run on a thread with IIS's own stack size, so a walk that recursed once per element
            // fails here as it would take down the web server.
            string result = null;
            var thread = new Thread( () => result = Clean( html.ToString() ), 256 * 1024 );
            thread.Start();
            thread.Join();

            Assert.AreEqual( "deeper", result );
        }

        #endregion The page around the card

        #region Helpers

        // The church's public and internal application roots' hosts.
        private static readonly ISet<string> Site = ChatProfileHtml.SiteHostsOf( "https://church.example.org/", "https://rock.church.example.org/" );

        private static string Clean( string html )
        {
            return ChatProfileHtml.Sanitize( html, Site );
        }

        #endregion Helpers
    }
}
