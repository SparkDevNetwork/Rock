using System.Text.RegularExpressions;

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

    }
}
