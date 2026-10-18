using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class StringHtmlExtensionsTests
    {
        [TestMethod]
        public void TestConvertHtmlStylesToInlineAttributesHandlesEmptyTagsCorrectly()
        {
            string originalHtml = @"
                <html>
                    <head>
                        <title></title>
                    </head>
                    <body>
                        <style>
                            .component-text td {
                                color: #0a0a0a;
                                font-family: Helvetica, Arial, sans-serif;
                                font-size: 16px;
                                font-weight: normal;
                                line-height: 1.3;
                            }
                        </style>
                        <span></span>
                        <div class=""structure-dropzone"">
                            <div class=""dropzone"">
                                <table class=""component component-text selected"">
                                    <tbody>
                                        <tr>
                                            <td>
                                                <h1>Title</h1><p> Can't wait to see what you have to say!</p>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </body>
                </html>";

            string expectedHtml = @"
                <html>
                    <head>
                        <title></title>
                    </head>
                    <body>
                        <style>
                            .component-text td {
                                color: #0a0a0a;
                                font-family: Helvetica, Arial, sans-serif;
                                font-size: 16px;
                                font-weight: normal;
                                line-height: 1.3;
                            }
                        </style>
                        <span></span>
                        <div class=""structure-dropzone"">
                            <div class=""dropzone"">
                                <table class=""component component-text selected"">
                                    <tbody>
                                        <tr>
                                            <td style=""color: #0a0a0a;font-family: Helvetica, Arial, sans-serif;font-size: 16px;font-weight: normal;line-height: 1.3"">
                                                <h1>Title</h1><p> Can't wait to see what you have to say!</p>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </body>
                </html>";

            string actualHtml = originalHtml.ConvertHtmlStylesToInlineAttributes();
            string actualHtmlNoSpaces = Regex.Replace( actualHtml, @"\s+", "" );
            string expectedHtmlNoSpaces = Regex.Replace( expectedHtml, @"\s+", "" );

            Assert.AreEqual( expectedHtmlNoSpaces, actualHtmlNoSpaces );
        }

        [TestMethod]
        public void TestConvertHtmlStylesToInlineAttributesFormatAllowedVoidElements()
        {
            string originalHtml = @"
                <html>
                    <head></head>
                    <body>
                        <area />
                        <base />
                        <br />
                        <table>
                        <colgroup>
                            <col span=""2"" style=""background-color:red"" />
                            <col style=""background-color:yellow"" />
                        </colgroup>
                        </table>
                        <embed />
                        <hr />
                        <img />
                        <input />
                        <link />
                        <meta />
                        <param />
                        <source />
                        <track />
                        <wbr />
                    </body>
                </html>";

            string expectedHtml = @"
                <html>
                    <head></head>
                    <body>
                        <area>
                        <base>
                        <br>
                        <table>
                            <colgroup>
                                <col span=""2"" style=""background-color:red"">
                                <col style=""background-color:yellow"">
                            </colgroup>
                        </table>
                        <embed>
                        <hr>
                        <img>
                        <input>
                        <link>
                        <meta>
                        <param>
                        <source>
                        <track>
                        <wbr>
                    </body>
                </html>";

            string actualHtml = originalHtml.ConvertHtmlStylesToInlineAttributes();
            string actualHtmlNoSpaces = Regex.Replace( actualHtml, @"\s+", "" );
            string expectedHtmlNoSpaces = Regex.Replace( expectedHtml, @"\s+", "" );

            Assert.AreEqual( expectedHtmlNoSpaces, actualHtmlNoSpaces );
        }

        [TestMethod]
        public void TestConvertHtmlStylesToInlineAttributesUnallowedVoidElements()
        {
            string originalHtml = @"
                <html>
                    <head>
                        <script></script>
                        <title></title>
                    </head>
                    <body>
                        <span></span>
                        <div></div>
                    </body>
                </html>";

            string expectedHtml = @"
                <html>
                    <head>
                        <script></script>
                        <title></title>
                    </head>
                    <body>
                        <span></span>
                        <div></div>
                    </body>
                </html>";

            string actualHtml = originalHtml.ConvertHtmlStylesToInlineAttributes();
            string actualHtmlNoSpaces = Regex.Replace( actualHtml, @"\s+", "" );
            string expectedHtmlNoSpaces = Regex.Replace( expectedHtml, @"\s+", "" );

            Assert.AreEqual( expectedHtmlNoSpaces, actualHtmlNoSpaces );
        }

        [TestMethod]
        public void TestTruncateHTMLWithoutMarkup()
        {
            var originalContent = "Welcome to Rock RMS.  We are glad you are here.";

            var expectedContent = "Welcome to Rock RMS.&hellip;";

            var result = originalContent.TruncateHtml( 20, "&hellip;" );
            Assert.AreEqual( expectedContent, result );
        }

        [TestMethod]
        public void TestTruncateHTMLWithBasicMarkup()
        {
            var originalContent = "<a href='https://www.twitter.com'>Twitter Link 1</a><br /><a href='https://www.twitter.com'>Twitter Link 2</a>";
            var expectedContent = "<a href='https://www.twitter.com'>Twitter Link 1</a><br /><a href='https://www.twitter.com'>Twitter Link 2</a>";

            var result = originalContent.TruncateHtml( 100, "&hellip;" );
            Assert.AreEqual( expectedContent, result );

            originalContent = "<a href='https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%20(Documentation).pdf'>Test Link 1</a>";
            expectedContent = "<a href='https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%20(Documentation).pdf'>Test Link 1</a>";

            result = originalContent.TruncateHtml( 100, "&hellip;" );
            Assert.AreEqual( expectedContent, result );

            originalContent = "<a href='https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%20(Documentation).pdf'>https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%20(Documentation).pdf</a>";
            expectedContent = "<a href='https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%20(Documentation).pdf'>https://rockadmin.bemaservices.com/Content/ExternalSite/PluginDocumentation/BEMA%20Support%20Plugin%&hellip;</a>";

            result = originalContent.TruncateHtml( 100, "&hellip;" );
            Assert.AreEqual( expectedContent, result );
        }

        [TestMethod]
        public void TestTruncateHTMLWithNestedTags()
        {
            var originalContent =
                @"<ul>
                <li><a href='mailto:test@yourorganization.com'>Email Address 1</a></li>
                <li><a href='http://yourorganization.com'>Website URL 1</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 2</a></li>
                <li><a href='http://yourorganization.com'>Website URL 2</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 3</a></li>
                <li><a href='http://yourorganization.com'>Website URL 3</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 4</a></li>
                <li><a href='http://yourorganization.com'>Website URL 4</a></li>
                </ul>";

            // The last Website URL is truncated, but all the closing tags are there.
            var expectedContent =
                @"<ul>
                <li><a href='mailto:test@yourorganization.com'>Email Address 1</a></li>
                <li><a href='http://yourorganization.com'>Website URL 1</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 2</a></li>
                <li><a href='http://yourorganization.com'>Website URL 2</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 3</a></li>
                <li><a href='http://yourorganization.com'>Website URL 3</a></li>
                <li><a href='mailto:test@yourorganization.com'>Email Address 4&hellip;</a></li></ul>";

            var result = originalContent.TruncateHtml( 100, "&hellip;" );
            Assert.AreEqual( expectedContent, result );
        }

        [TestMethod]
        public void TestHtmlNodeDescendants()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml( "<p><a href=\"#\">one</a></p>\r\n<p>two</p><p>three</p>" );
            var descendants = doc.DocumentNode.TextDescendants().ToArray();
            foreach ( var d in descendants )
            {
                System.Console.WriteLine( d.OuterHtml );
            }

            Assert.AreEqual( "one", descendants[0].InnerText );
            Assert.AreEqual( "two", descendants[1].InnerText );
            Assert.AreEqual( "three", descendants[2].InnerText );
        }

        #region SanitizeHtml

        [TestMethod]
        public void SanitizeHtml_WhereInputIncludesHtmlComment_CommentIsRemoved()
        {
            var html = @"
<!-- Comment 1 -->
<p>Para 1</p>
<!-- Comment 2 -->
<p>Para 2</p>
<!-- Comment 3 -->
";

            var output = html.SanitizeHtml( strict: true );

            Assert.DoesNotContain( "Comment", output );
        }

        [TestMethod]
        public void SanitizeHtml_WhereInputIsNull_ReturnsEmptyString()
        {
            string html = null;
            var output = html.SanitizeHtml( strict: true );

            Assert.AreEqual( string.Empty, output );
        }

        [TestMethod]
        public void SanitizeHtml_NotStrict_WithUnclosedFinalTag_EncodesTagAsText()
        {
            var output = "<p>a</p><img src=x onerror=y".SanitizeHtml( strict: false );

            Assert.AreEqual( "<p>a</p>&lt;img src=x onerror=y", output );
        }

        [TestMethod]
        public void SanitizeHtml_NotStrict_WithLessThanSignInText_KeepsText()
        {
            var output = "<p>Value must be < 10 and > 2</p>".SanitizeHtml( strict: false );

            Assert.AreEqual( "<p>Value must be &lt; 10 and &gt; 2</p>", output );
        }

        [TestMethod]
        public void SanitizeHtml_NotStrict_WithFormFeedSeparator_RemovesEventHandler()
        {
            var output = "<img\fsrc=x\fonerror=y>".SanitizeHtml( strict: false );

            Assert.IsFalse( output.Contains( "onerror" ) );
        }

        [DataTestMethod]
        [DataRow( "<a href=\"java\tscript:y\">a</a>" )]
        [DataRow( "<a href=\"java\nscript:y\">a</a>" )]
        [DataRow( "<a href=\"jav&#x09;ascript:y\">a</a>" )]
        [DataRow( "<a href=\"javascript&colon;y\">a</a>" )]
        public void SanitizeHtml_NotStrict_WithObfuscatedScriptScheme_RemovesHref( string html )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.AreEqual( "<a>a</a>", output );
        }

        [DataTestMethod]
        [DataRow( "<base href=\"/x\">" )]
        [DataRow( "<style>a{}</style>" )]
        [DataRow( "<title>a</title>" )]
        [DataRow( "<xmp>a</xmp>" )]
        [DataRow( "<noscript>a</noscript>" )]
        [DataRow( "<plaintext>a" )]
        [DataRow( "<frameset><frame src=\"/x\"></frameset>" )]
        public void SanitizeHtml_NotStrict_WithBlockedElement_RemovesElement( string html )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.AreEqual( string.Empty, output );
        }

        [DataTestMethod]
        [DataRow( "<!>" )]
        [DataRow( "<!-a->" )]
        [DataRow( "<!--a--->" )]
        [DataRow( "<p>a</p><!-- b -->" )]
        public void SanitizeHtml_NotStrict_WithComment_RemovesCommentWithoutThrowing( string html )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.IsFalse( output.Contains( "<!" ) );
        }

        [DataTestMethod]
        [DataRow( "<xmp><!--</xmp><img src=x onerror=y>--></xmp>" )]
        [DataRow( "<noscript><!--</noscript><img src=x onerror=y>--></noscript>" )]
        [DataRow( "<noembed><!--</noembed><img src=x onerror=y>--></noembed>" )]
        [DataRow( "<noframes><!--</noframes><img src=x onerror=y>--></noframes>" )]
        [DataRow( "<textarea></textarea><!--</textarea><img src=x onerror=y>-->" )]
        public void SanitizeHtml_NotStrict_WithCommentInRawTextElement_RemovesMarkup( string html )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.IsFalse( output.Contains( "onerror" ) );
            Assert.IsFalse( output.Contains( "<!" ) );
        }

        [DataTestMethod]
        [DataRow( "<button formaction=\"/x\">a</button>", "<button>a</button>" )]
        [DataRow( "<input form=\"f\" type=\"submit\" />", "<input type=\"submit\" />" )]
        public void SanitizeHtml_NotStrict_WithFormSubmitAttribute_RemovesAttribute( string html, string expected )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.AreEqual( expected, output );
        }

        [DataTestMethod]
        [DataRow( "<a href=\"https://www.rockrms.com/\" target=\"_blank\">Link</a>" )]
        [DataRow( "<a href=\"mailto:a@b.c\">Mail</a>" )]
        [DataRow( "<img src=\"/GetImage.ashx?Id=1\" alt=\"Image\" />" )]
        [DataRow( "<p><strong>b</strong> <em>i</em> <u>u</u></p>\r\n<ul><li>a</li></ul>" )]
        [DataRow( "<table class=\"table\"><thead><tr><th>H</th></tr></thead><tbody><tr><td>C</td></tr></tbody></table>" )]
        [DataRow( "<p>a&nbsp;b &quot;c&quot; &#39;d</p>" )]
        [DataRow( "<pre>a\r\n\tb</pre>" )]
        public void SanitizeHtml_NotStrict_WithLegitimateMarkup_ReturnsMarkupUnchanged( string html )
        {
            var output = html.SanitizeHtml( strict: false );

            Assert.AreEqual( html, output );
        }

        [TestMethod]
        public void SanitizeHtml_NotStrict_WithDeeplyNestedMarkup_ReturnsEncodedInputWithoutStackOverflow()
        {
            var html = string.Concat( Enumerable.Repeat( "<div>", 5000 ) ) + "a";
            string output = null;

            // A small stack, like a web request thread, so an overflow would end the test run.
            var thread = new System.Threading.Thread( () => output = html.SanitizeHtml( strict: false ), 256 * 1024 );
            thread.Start();
            thread.Join();

            Assert.AreEqual( html.EncodeHtml(), output );
        }

        [TestMethod]
        public void SanitizeHtml_NotStrict_WithNestingAtLimit_ReturnsMarkupUnchanged()
        {
            var html = string.Concat( Enumerable.Repeat( "<div>", 255 ) ) + "a" + string.Concat( Enumerable.Repeat( "</div>", 255 ) );
            string output = null;

            var thread = new System.Threading.Thread( () => output = html.SanitizeHtml( strict: false ), 256 * 1024 );
            thread.Start();
            thread.Join();

            Assert.AreEqual( html, output );
        }

        #endregion

        #region SanitizeLava

        private static readonly Regex _lavaOpenerRegex = new Regex( @"\{[{%\[]" );

        private static void AssertNoLavaOpener( string input )
        {
            var output = input.SanitizeLava();

            Assert.IsFalse( _lavaOpenerRegex.IsMatch( output ), $"Input '{input}' produced '{output}', which still contains a Lava opener." );
        }

        [TestMethod]
        public void SanitizeLava_WhereTagIsSplitByInnerTag_RemovesAllOpeners()
        {
            AssertNoLavaOpener( "{{% x %}{ 7 | Plus:7 }}" );
            Assert.AreEqual( string.Empty, "{{% x %}{ 7 | Plus:7 }}".SanitizeLava() );
        }

        [TestMethod]
        public void SanitizeLava_WhereOutputBodyContainsClosingBrace_RemovesOpener()
        {
            AssertNoLavaOpener( "{{ 7 | Plus:7 | Append:'}' }}" );
        }

        [TestMethod]
        public void SanitizeLava_WhereTagIsSplitByNestedTags_RemovesAllOpeners()
        {
            AssertNoLavaOpener( "{{{% a %}{% b %}% x %}{ 7 | Plus:7 }}" );
        }

        [TestMethod]
        public void SanitizeLava_WhereTagBodyContainsClosingBrace_RemovesOpener()
        {
            AssertNoLavaOpener( "{% assign x = '}' %}{{ x }}" );
        }

        [TestMethod]
        public void SanitizeLava_WhereShortcodeIsSplitByInnerTag_RemovesOpener()
        {
            AssertNoLavaOpener( "{[{% x %} sc ]}" );
            AssertNoLavaOpener( "{{[ a ]}[ sc ]}" );
            AssertNoLavaOpener( "{{[ sc ]}{ 7 | Plus:7 }}" );
        }

        [TestMethod]
        public void SanitizeLava_WhereInputHasOnlySingleBraces_ReturnsInputUnchanged()
        {
            Assert.AreEqual( "Room {A1} and {B}", "Room {A1} and {B}".SanitizeLava() );
            Assert.AreEqual( "{ { 1 } }", "{ { 1 } }".SanitizeLava() );
        }

        [TestMethod]
        public void SanitizeLava_WhereInputIsNullOrWhiteSpace_ReturnsEmptyString()
        {
            string lava = null;

            Assert.AreEqual( string.Empty, lava.SanitizeLava() );
            Assert.AreEqual( string.Empty, string.Empty.SanitizeLava() );
            Assert.AreEqual( string.Empty, "   ".SanitizeLava() );
        }

        [TestMethod]
        public void SanitizeLava_WhereInputIsNormalLava_RemovesLava()
        {
            Assert.AreEqual( string.Empty, "{{ Person.NickName }}".SanitizeLava() );
            Assert.AreEqual( "x", "{%- if 1 -%}x{%- endif -%}".SanitizeLava() );
            Assert.AreEqual( "Hi !", "Hi {[ sc arg:'1' ]}!".SanitizeLava() );
        }

        [TestMethod]
        public void SanitizeLava_WhereInputIsGenerated_NeverReturnsLavaOpener()
        {
            // Build every string up to length 7 from the characters that matter to
            // the Lava parser, plus a filler character, and make sure none of them
            // can produce an opener after sanitizing.
            var alphabet = new[] { '{', '}', '%', '[', ']', 'x' };
            var buffer = new char[7];

            for ( var length = 1; length <= buffer.Length; length++ )
            {
                var total = ( int ) System.Math.Pow( alphabet.Length, length );

                for ( var n = 0; n < total; n++ )
                {
                    var value = n;
                    for ( var i = 0; i < length; i++ )
                    {
                        buffer[i] = alphabet[value % alphabet.Length];
                        value /= alphabet.Length;
                    }

                    AssertNoLavaOpener( new string( buffer, 0, length ) );
                }
            }
        }

        [TestMethod]
        public void SanitizeLava_WhereInputIsDeeplyNested_ReturnsQuicklyWithoutLavaOpener()
        {
            // Each nesting level needs its own pass, so this would take minutes
            // if the number of passes were not capped.
            const int depth = 14000;
            var input = new string( '{', depth )
                + "{% x %}"
                + string.Concat( Enumerable.Repeat( "% x %}", depth ) )
                + "{ 7 }}";

            Assert.IsTrue( input.Length > 95000 );

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var output = input.SanitizeLava();
            stopwatch.Stop();

            Assert.IsFalse( _lavaOpenerRegex.IsMatch( output ), "The output still contains a Lava opener." );
            Assert.IsTrue( stopwatch.Elapsed.TotalSeconds < 2, $"Sanitizing took {stopwatch.Elapsed.TotalSeconds} seconds." );
        }

        [TestMethod]
        public void SanitizeLava_WhereInputMakesPatternSlow_ReturnsQuicklyWithoutLavaOpener()
        {
            // Every "{{" here scans ahead to the same closing brace, so one pass
            // of the output pattern takes far longer than its time limit.
            var input = string.Concat( Enumerable.Repeat( "{{a", 33000 ) ) + "}x";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var output = input.SanitizeLava();
            stopwatch.Stop();

            Assert.IsFalse( _lavaOpenerRegex.IsMatch( output ), "The output still contains a Lava opener." );
            Assert.IsTrue( stopwatch.Elapsed.TotalSeconds < 5, $"Sanitizing took {stopwatch.Elapsed.TotalSeconds} seconds." );
        }

        [TestMethod]
        public void SanitizeLava_WhereCommentJoinsBraces_RemovesBraceBeforeComment()
        {
            // The legacy RockLiquid engine strips "/- ... -/" comments before
            // parsing, which would join the two braces into an output.
            var input = "{/-" + "-/{ 7 | Plus:7 }}";
            var output = input.SanitizeLava();
            var withoutComments = Regex.Replace( output, @"/-.*?-/", string.Empty, RegexOptions.Singleline );

            AssertNoLavaOpener( input );
            Assert.AreEqual( "/-" + "-/{ 7 | Plus:7 }}", output );
            Assert.IsFalse( _lavaOpenerRegex.IsMatch( withoutComments ), $"Output '{output}' contains a Lava opener once comments are removed." );
        }

        #endregion
    }
}
