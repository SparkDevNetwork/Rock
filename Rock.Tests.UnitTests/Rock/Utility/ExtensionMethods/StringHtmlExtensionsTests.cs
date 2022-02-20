using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.UnitTests.Rock.Utility.ExtensionMethods
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
    }
}
