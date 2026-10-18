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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Verifies that <see cref="History.SummaryHtml"/> does not render stored
    /// values as markup.
    /// </summary>
    [TestClass]
    public class HistorySummaryHtmlTests
    {
        private const string Marker = "<img src=x onerror=alert(1)>";
        private const string EncodedMarker = "&lt;img src=x onerror=alert(1)&gt;";
        private const string BoldMarker = "<b>TH144</b>";
        private const string EncodedBoldMarker = "&lt;b&gt;TH144&lt;/b&gt;";

        private const int PersonEntityTypeId = 900001;
        private const int AttributeEntityTypeId = 900002;
        private const int OtherEntityTypeId = 900003;

        /// <summary>
        /// Loads the Person and Attribute entity types into the cache so that
        /// SummaryHtml can look up their identifiers without a database.
        /// </summary>
        /// <param name="context">The test context.</param>
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            EntityTypeCache.Get( new EntityType
            {
                Id = PersonEntityTypeId,
                Name = typeof( Person ).FullName,
                Guid = new Guid( "72657ED8-D16E-492E-AC12-144C5E7567E7" )
            } );

            EntityTypeCache.Get( new EntityType
            {
                Id = AttributeEntityTypeId,
                Name = typeof( Rock.Model.Attribute ).FullName,
                Guid = new Guid( "5997C8D3-8840-4591-99A5-552919F90CBD" )
            } );
        }

        private static void AssertNoMarkup( string html )
        {
            Assert.IsNotNull( html );
            Assert.IsFalse( html.Contains( "<img" ), html );
            Assert.IsFalse( html.Contains( "<b>" ), html );
        }

        #region Property history (encoded)

        [TestMethod]
        public void Add_WithMarkupInValueNameAndNewValue_EncodesValues()
        {
            var history = new History { Verb = "ADD", ChangeType = "Property", ValueName = BoldMarker, NewValue = Marker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( $"<span class='field-name'>{EncodedBoldMarker}</span>" ), html );
            Assert.IsTrue( html.Contains( $"<span class='field-value'>{EncodedMarker}</span>" ), html );
        }

        [TestMethod]
        public void Add_Sensitive_EncodesValueName()
        {
            var history = new History { Verb = "ADD", ChangeType = "Property", ValueName = BoldMarker, IsSensitive = true };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedBoldMarker ), html );
        }

        [TestMethod]
        public void Add_WithNullNewValue_DoesNotAppendValue()
        {
            var history = new History { Verb = "ADD", ChangeType = "Property", ValueName = "First Name" };

            Assert.AreEqual( "Added <span class='field-name'>First Name</span>.", history.SummaryHtml );
        }

        [TestMethod]
        public void Modify_WithMarkupInOldAndNewValue_EncodesValues()
        {
            var history = new History { Verb = "MODIFY", ChangeType = "Property", ValueName = BoldMarker, OldValue = Marker, NewValue = BoldMarker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( $"value from <span class='field-value'>{EncodedMarker}</span> to <span class='field-value'>{EncodedBoldMarker}</span>" ), html );
        }

        [TestMethod]
        public void Modify_FromNull_EncodesNewValue()
        {
            var history = new History { Verb = "MODIFY", ChangeType = "Property", ValueName = "Last Name", NewValue = Marker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.StartsWith( "Added" ), html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        [TestMethod]
        public void Modify_ToNull_EncodesOldValue()
        {
            var history = new History { Verb = "MODIFY", ChangeType = "Property", ValueName = "Last Name", OldValue = Marker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.StartsWith( "Deleted" ), html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        [TestMethod]
        public void Modify_WithEqualValues_EncodesChangeType()
        {
            var history = new History { Verb = "MODIFY", ChangeType = BoldMarker, ValueName = "Same", OldValue = "x", NewValue = "x" };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.EndsWith( EncodedBoldMarker ), html );
        }

        [TestMethod]
        public void Modify_Sensitive_EncodesValueName()
        {
            var history = new History { Verb = "MODIFY", ChangeType = "Property", ValueName = Marker, IsSensitive = true };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        [TestMethod]
        public void Modify_WithAmpersand_EncodesOnce()
        {
            var history = new History { Verb = "MODIFY", ChangeType = "Property", ValueName = "Company", OldValue = "Old Co", NewValue = "Smith & Jones" };

            var html = history.SummaryHtml;

            Assert.IsTrue( html.Contains( "<span class='field-value'>Smith &amp; Jones</span>" ), html );
            Assert.IsFalse( html.Contains( "&amp;amp;" ), html );
        }

        [TestMethod]
        public void Delete_WithMarkupInOldValue_EncodesValues()
        {
            var history = new History { Verb = "DELETE", ChangeType = "Property", ValueName = BoldMarker, OldValue = Marker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedBoldMarker ), html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        [TestMethod]
        public void Delete_Sensitive_EncodesValueName()
        {
            var history = new History { Verb = "DELETE", ChangeType = "Property", ValueName = Marker, IsSensitive = true };

            AssertNoMarkup( history.SummaryHtml );
        }

        #endregion

        #region Other verbs (encoded)

        [DataTestMethod]
        [DataRow( "PROCESS" )]
        [DataRow( "MATCHED" )]
        [DataRow( "UNMATCHED" )]
        [DataRow( "REGISTERED" )]
        [DataRow( "MERGE" )]
        [DataRow( "ADDEDTOGROUP" )]
        [DataRow( "REMOVEDFROMGROUP" )]
        [DataRow( "STEPADDED" )]
        public void Verb_WithMarkupInValueName_EncodesValueName( string verb )
        {
            var history = new History { Verb = verb, ChangeType = "Record", ValueName = Marker, CreatedDateTime = new DateTime( 2026, 10, 16 ) };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        [DataTestMethod]
        [DataRow( "LOGIN" )]
        [DataRow( "SENT" )]
        [DataRow( "STEPADDED" )]
        public void Verb_WithMarkupInRelatedData_EncodesRelatedData( string verb )
        {
            var history = new History { Verb = verb, ChangeType = "Record", ValueName = BoldMarker, RelatedData = Marker, CreatedDateTime = new DateTime( 2026, 10, 16 ) };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
            Assert.IsTrue( html.Contains( EncodedBoldMarker ), html );
        }

        [DataTestMethod]
        [DataRow( "CONNECTIONREQUESTADDED" )]
        [DataRow( "CONNECTIONREQUESTCONNECTED" )]
        [DataRow( "CONNECTIONREQUESTSTATUSMODIFY" )]
        [DataRow( "CONNECTIONREQUESTSTATEMODIFY" )]
        [DataRow( "CONNECTIONREQUESTDELETE" )]
        public void ConnectionRequestVerb_WithMarkupInOpportunityAndCreatedByName_EncodesValues( string verb )
        {
            var history = new History
            {
                Verb = verb,
                ChangeType = "Record",
                ValueName = Marker,
                EntityTypeId = OtherEntityTypeId,
                CreatedByPersonAlias = new PersonAlias
                {
                    Person = new Person { NickName = BoldMarker, LastName = "Smith & Jones" }
                }
            };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );

            if ( verb != "CONNECTIONREQUESTADDED" && verb != "CONNECTIONREQUESTCONNECTED" )
            {
                Assert.IsTrue( html.Contains( $" by {EncodedBoldMarker} Smith &amp; Jones." ), html );
            }
        }

        [TestMethod]
        public void CustomVerb_WithMarkupEverywhere_EncodesValues()
        {
            var history = new History { Verb = BoldMarker, ChangeType = "Property", ValueName = Marker, OldValue = Marker, NewValue = BoldMarker };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( $"<span class='field-name'>{EncodedMarker}</span>" ), html );
            Assert.IsTrue( html.Contains( $"New value of <span class='field-name'>{EncodedBoldMarker}</span>" ), html );
        }

        #endregion

        #region Attribute history (rebuilt from an allowlist)

        private static string GetAttributeSummary( string newValue )
        {
            var history = new History
            {
                Verb = "ADD",
                ChangeType = "Property",
                ValueName = "Notes",
                RelatedEntityTypeId = AttributeEntityTypeId,
                NewValue = newValue
            };

            return history.SummaryHtml;
        }

        private static string GetAttributeFieldValue( string newValue )
        {
            var html = GetAttributeSummary( newValue );
            var prefix = "Added <span class='field-name'>Notes</span> value of <span class='field-value'>";
            var suffix = "</span>.";

            Assert.IsTrue( html.StartsWith( prefix ), html );
            Assert.IsTrue( html.EndsWith( suffix ), html );

            return html.Substring( prefix.Length, html.Length - prefix.Length - suffix.Length );
        }

        [TestMethod]
        public void AttributeValueHistory_WithLinkAndScriptedImage_KeepsLinkAndStripsHandler()
        {
            var history = new History
            {
                Verb = "MODIFY",
                ChangeType = "Property",
                ValueName = BoldMarker,
                RelatedEntityTypeId = AttributeEntityTypeId,
                OldValue = "<a href='https://www.rockrms.com/'>Rock</a>",
                NewValue = "<img src=x onerror=alert(1)>"
            };

            var html = history.SummaryHtml;

            Assert.IsTrue( html.Contains( "<span class='field-value'><a href=\"https://www.rockrms.com/\">Rock</a></span>" ), html );
            Assert.IsTrue( html.Contains( "<span class='field-value'><img src=\"x\"></span>" ), html );
            Assert.IsFalse( html.Contains( "onerror" ), html );
            Assert.IsTrue( html.Contains( $"<span class='field-name'>{EncodedBoldMarker}</span>" ), html );
        }

        [TestMethod]
        public void AttributeChangeTypeHistory_WithScriptedImage_StripsHandler()
        {
            var history = new History
            {
                Verb = "ADD",
                ChangeType = "Attribute",
                ValueName = "First Visit",
                NewValue = "<img src=x onerror=alert(1)>"
            };

            var html = history.SummaryHtml;

            Assert.IsFalse( html.Contains( "onerror" ), html );
            Assert.IsTrue( html.Contains( "<img src=\"x\">" ), html );
        }

        [TestMethod]
        public void AttributeValueHistory_WithSocialMediaLink_IsPreserved()
        {
            var value = GetAttributeFieldValue( "<a href='https://facebook.com/x'><i class='fa fa-facebook'></i></a>" );

            Assert.AreEqual( "<a href=\"https://facebook.com/x\"><i class=\"fa fa-facebook\"></i></a>", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithAllowedFormatting_IsPreserved()
        {
            var value = GetAttributeFieldValue( "<strong>S</strong><em>E</em><b>B</b><br><span class='label'>L</span><img src='/GetImage.ashx?id=1&amp;width=50' alt='A &amp; B'>" );

            Assert.AreEqual( "<strong>S</strong><em>E</em><b>B</b><br><span class=\"label\">L</span><img src=\"/GetImage.ashx?id=1&amp;width=50\" alt=\"A &amp; B\">", value );
        }

        [DataTestMethod]
        [DataRow( "https://www.rockrms.com/a?b=1" )]
        [DataRow( "http://www.rockrms.com/" )]
        [DataRow( "mailto:ted@rocksolidchurchdemo.com" )]
        [DataRow( "tel:6235550100" )]
        [DataRow( "/page/12" )]
        [DataRow( "page/12" )]
        [DataRow( "//www.rockrms.com/" )]
        public void AttributeValueHistory_WithAllowedLink_KeepsHref( string href )
        {
            var value = GetAttributeFieldValue( $"<a href='{href}'>x</a>" );

            Assert.AreEqual( $"<a href=\"{href}\">x</a>", value );
        }

        [DataTestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "JaVaScRiPt:alert(1)" )]
        [DataRow( " javascript:alert(1)" )]
        [DataRow( "java&#x09;script:alert(1)" )]
        [DataRow( "java&#9;script:alert(1)" )]
        [DataRow( "jav&#x61;script:alert(1)" )]
        [DataRow( "java\tscript:alert(1)" )]
        [DataRow( "java\nscript:alert(1)" )]
        [DataRow( "java\fscript:alert(1)" )]
        [DataRow( "\u0001javascript:alert(1)" )]
        [DataRow( "javascript&colon;alert(1)" )]
        [DataRow( "vbscript:msgbox(1)" )]
        [DataRow( "data:text/html,<script>alert(1)</script>" )]
        public void AttributeValueHistory_WithUnsafeLink_DropsHref( string href )
        {
            var value = GetAttributeFieldValue( $"<a href=\"{href}\">x</a>" );

            // Depending on the HtmlAgilityPack version, the href is dropped, the
            // whole value falls back to encoded text, or an unknown entity is kept
            // as an inert relative URL (javascript&amp;colon;...). Check what a
            // browser would run rather than one exact output.
            AssertNoScriptableMarkup( value );
        }

        /// <summary>
        /// Asserts that the HTML has no script element, no event handler attribute,
        /// and no href or src that a browser would read as a javascript, vbscript
        /// or data URL.
        /// </summary>
        /// <param name="html">The HTML to check.</param>
        private static void AssertNoScriptableMarkup( string html )
        {
            Assert.IsFalse( html.IndexOf( "<script", StringComparison.OrdinalIgnoreCase ) >= 0, html );
            Assert.IsFalse( Regex.IsMatch( html, @"<[^>]*\son[a-z]+\s*=", RegexOptions.IgnoreCase ), html );

            var urlAttributes = Regex.Matches( html, @"\s(?:href|src)\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+))", RegexOptions.IgnoreCase );

            foreach ( Match match in urlAttributes )
            {
                var rawValue = match.Groups[1].Success ? match.Groups[1].Value
                    : match.Groups[2].Success ? match.Groups[2].Value
                    : match.Groups[3].Value;

                // A browser decodes the attribute once, then ignores characters
                // below U+0021 when it reads the scheme.
                var decodedValue = WebUtility.HtmlDecode( rawValue );
                var url = new StringBuilder();

                foreach ( var c in decodedValue )
                {
                    if ( c > ' ' )
                    {
                        url.Append( c );
                    }
                }

                var normalizedUrl = url.ToString();

                foreach ( var scheme in new[] { "javascript:", "vbscript:", "data:" } )
                {
                    Assert.IsFalse( normalizedUrl.StartsWith( scheme, StringComparison.OrdinalIgnoreCase ), html );
                }
            }
        }

        [DataTestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "data:image/svg+xml,<svg onload=alert(1)>" )]
        [DataRow( "mailto:x@example.com" )]
        public void AttributeValueHistory_WithUnsafeImageSource_DropsImage( string src )
        {
            var value = GetAttributeFieldValue( $"<img src=\"{src}\" alt=\"a\">" );

            Assert.AreEqual( string.Empty, value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithUnclosedScriptedImage_DoesNotPassMarkupThrough()
        {
            var value = GetAttributeFieldValue( "<img src=x onerror=alert(1)" );

            // The parser cannot read the unclosed tag, so the value is shown as text.
            Assert.AreEqual( "&lt;img src=x onerror=alert(1)", value );
        }

        [DataTestMethod]
        [DataRow( "hello <img src=x onerror=alert(3)" )]
        [DataRow( "<details open ontoggle=alert(8)" )]
        [DataRow( "<a href=javascript:alert(5)" )]
        [DataRow( "<script src=//evil.example/x.js" )]
        public void AttributeValueHistory_WithUnclosedLastTag_DoesNotPassMarkupThrough( string input )
        {
            var value = GetAttributeFieldValue( input );

            Assert.AreEqual( input.EncodeHtml(), value );
        }

        [DataTestMethod]
        [DataRow( "<img\fsrc=x\fonerror=alert(1)>" )]
        [DataRow( "<a\fhref=javascript:alert(3)>x</a>" )]
        public void AttributeValueHistory_WithFormFeedSeparators_DoesNotPassMarkupThrough( string input )
        {
            var value = GetAttributeFieldValue( input );

            Assert.AreEqual( input.EncodeHtml(), value );
        }

        [DataTestMethod]
        [DataRow( "<!a>" )]
        [DataRow( "<!>" )]
        [DataRow( "<!-->" )]
        [DataRow( "<!--->" )]
        [DataRow( "x <!--->" )]
        [DataRow( "x <!- y" )]
        [DataRow( "<p>x<!--a--->" )]
        [DataRow( "<!---><img src=x onerror=alert(1)>-->" )]
        [DataRow( "<!DOCTYPE html><?xml version='1.0'?>x" )]
        public void AttributeValueHistory_WithOddComments_DoesNotThrowOrPassMarkup( string input )
        {
            var value = GetAttributeFieldValue( input );

            Assert.IsFalse( value.Contains( "<!" ), value );
            Assert.IsFalse( value.Contains( "<?" ), value );
            Assert.IsFalse( value.Contains( "onerror" ), value );
        }

        [DataTestMethod]
        [DataRow( "<plaintext />" )]
        [DataRow( "<plaintext>x" )]
        [DataRow( "<textarea></textarea>" )]
        [DataRow( "<xmp></xmp>" )]
        [DataRow( "<title></title>" )]
        [DataRow( "<noembed></noembed>" )]
        [DataRow( "<template><img src=x></template>" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></noscript>" )]
        [DataRow( "<script>alert(1)</script>" )]
        [DataRow( "<style>body{display:none}</style>" )]
        [DataRow( "<iframe src=//evil.example/></iframe><object data=x></object><embed src=x>" )]
        [DataRow( "<base href='//evil.example/'><meta http-equiv=refresh content='0;url=//x'><link rel=stylesheet href=//x>" )]
        public void AttributeValueHistory_WithRawTextOrDangerousElements_EmitsNoTags( string input )
        {
            var value = GetAttributeFieldValue( input );

            Assert.IsFalse( value.Contains( "<" ), value );
            Assert.IsFalse( value.Contains( "onerror" ), value );
            Assert.IsFalse( value.Contains( "<plaintext" ), value );
            Assert.IsFalse( value.Contains( "<textarea" ), value );
            Assert.IsFalse( value.Contains( "<script" ), value );
            Assert.IsFalse( value.Contains( "<style" ), value );
            Assert.IsFalse( value.Contains( "<base" ), value );
            Assert.IsFalse( value.Contains( "<iframe" ), value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithPrefixedElement_DropsTagAndKeepsText()
        {
            var value = GetAttributeFieldValue( "<p>Hi<o:p></o:p></p>" );

            Assert.AreEqual( "Hi", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithSelfClosedLink_EmitsClosedLink()
        {
            var value = GetAttributeFieldValue( "<a href=\"https://evil.example/\" />" );

            Assert.AreEqual( "<a href=\"https://evil.example/\"></a>", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithFormActionButton_DropsButton()
        {
            var value = GetAttributeFieldValue( "<button formaction=\"https://evil.example/\">Click</button><input type=submit formaction=//x name=__EVENTTARGET>" );

            Assert.AreEqual( "Click", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithStyleOverlay_DropsStyle()
        {
            var value = GetAttributeFieldValue( "<div style=\"position:fixed;top:0;left:0;width:100%;height:100%\">x</div><span class=\"a\" style=\"background:url(https://evil.example/)\" onclick=\"alert(1)\">y</span>" );

            Assert.AreEqual( "x<span>y</span>", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithDisallowedAttributes_DropsThem()
        {
            var value = GetAttributeFieldValue( "<a href=\"https://x.example/\" ping=\"https://evil.example/\" target=\"_blank\" onmouseover=\"alert(1)\">x</a><i class=\"fa\" id=\"z\" style=\"color:red\"></i>" );

            Assert.AreEqual( "<a href=\"https://x.example/\" target=\"_blank\" rel=\"noopener noreferrer\">x</a><i class=\"fa\"></i>", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithClassBreakout_DropsClass()
        {
            var value = GetAttributeFieldValue( "<i class='a\"onmouseover=\"alert(1)'></i><i class='fa x\"y'></i>" );

            Assert.AreEqual( "<i></i><i class=\"fa\"></i>", value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithPageLevelClasses_KeepsOnlyAllowedClasses()
        {
            var value = GetAttributeFieldValue( "<a href=\"https://evil.example/login\"><span class=\"modal-backdrop fade\"></span></a><i class=\"fa fa-facebook\"></i><span class=\"label label-success Text-Big text-muted badge\">L</span><i class=\"FA fab fas far fa-2x\"></i>" );

            Assert.AreEqual( "<a href=\"https://evil.example/login\"><span></span></a><i class=\"fa fa-facebook\"></i><span class=\"label label-success text-muted badge\">L</span><i class=\"fab fas far fa-2x\"></i>", value );
        }

        [DataTestMethod]
        [DataRow( "<a href=\"https://x.example/\" target=\"_blank\">x</a>", "<a href=\"https://x.example/\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>" )]
        [DataRow( "<a href=\"https://x.example/\" target=\" _BLANK \">x</a>", "<a href=\"https://x.example/\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>" )]
        [DataRow( "<a href=\"https://x.example/\" target=\"_top\">x</a>", "<a href=\"https://x.example/\">x</a>" )]
        [DataRow( "<a href=\"javascript:alert(1)\" target=\"_blank\">x</a>", "<a>x</a>" )]
        public void AttributeValueHistory_WithTarget_OnlyKeepsBlankOnSafeLinks( string input, string expected )
        {
            Assert.AreEqual( expected, GetAttributeFieldValue( input ) );
        }

        [DataTestMethod]
        [DataRow( "<p>a</p><p>b</p>", "a<br>b" )]
        [DataRow( "<p>a</p>\r\n<p>b</p>", "a\r\n<br>b" )]
        [DataRow( "<ul><li>a</li><li>b</li><li>c</li></ul>", "a<br>b<br>c" )]
        [DataRow( "<div>a</div><div>b</div>", "a<br>b" )]
        [DataRow( "<div>a</div>x<div>b</div>", "axb" )]
        public void AttributeValueHistory_WithBlockElements_SeparatesThemWithLineBreaks( string input, string expected )
        {
            Assert.AreEqual( expected, GetAttributeFieldValue( input ) );
        }

        [TestMethod]
        public void AttributeValueHistory_WithLongNestedValue_EncodesWithoutParsing()
        {
            var input = string.Concat( System.Linq.Enumerable.Repeat( "<b>", 100000 ) );
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var value = GetAttributeFieldValue( input );

            stopwatch.Stop();
            Assert.IsTrue( stopwatch.ElapsedMilliseconds < 2000, $"{stopwatch.ElapsedMilliseconds} ms" );
            Assert.AreEqual( input.EncodeHtml(), value );
        }

        [TestMethod]
        public void AttributeValueHistory_AtLengthLimit_IsStillRebuilt()
        {
            var input = "<b>x</b>" + new string( 'y', 4000 - 8 );

            Assert.AreEqual( "<b>x</b>" + new string( 'y', 4000 - 8 ), GetAttributeFieldValue( input ) );
            Assert.AreEqual( ( input + "y" ).EncodeHtml(), GetAttributeFieldValue( input + "y" ) );
        }

        [TestMethod]
        public void AttributeValueHistory_WithLessThanInText_EncodesIt()
        {
            Assert.AreEqual( "a &lt; b", GetAttributeFieldValue( "a < b" ) );
            Assert.AreEqual( "x &lt; 3 and y &gt; 2", GetAttributeFieldValue( "x < 3 and y > 2" ) );
        }

        [DataTestMethod]
        [DataRow( "Tom &amp; Jerry", "Tom &amp; Jerry" )]
        [DataRow( "Tom & Jerry", "Tom &amp; Jerry" )]
        [DataRow( "O&#39;Brien &lt;b&gt;", "O&#39;Brien &lt;b&gt;" )]
        public void AttributeValueHistory_WithEntities_EncodesOnce( string input, string expected )
        {
            var value = GetAttributeFieldValue( input );

            Assert.AreEqual( expected, value );
        }

        [TestMethod]
        public void AttributeValueHistory_WithDeepNesting_FallsBackToEncoding()
        {
            var input = string.Concat( System.Linq.Enumerable.Repeat( "<b>", 5000 ) ) + "x";

            var value = GetAttributeFieldValue( input );

            Assert.IsTrue( value.StartsWith( "&lt;b&gt;" ), value.Substring( 0, 50 ) );
        }

        [TestMethod]
        public void NonAttributeRelatedEntity_WithScriptedImage_EncodesValue()
        {
            var history = new History
            {
                Verb = "ADD",
                ChangeType = "Property",
                ValueName = "Name",
                RelatedEntityTypeId = OtherEntityTypeId,
                NewValue = Marker
            };

            var html = history.SummaryHtml;

            AssertNoMarkup( html );
            Assert.IsTrue( html.Contains( EncodedMarker ), html );
        }

        #endregion
    }
}
