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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field;
using Rock.Field.Types;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="UrlLinkFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class UrlLinkFieldTypeTests
    {
        [TestMethod]
        [DataRow( "https://www.rockrms.com", "<a href='https://www.rockrms.com/'>https://www.rockrms.com</a>" )]
        [DataRow( "http://www.rockrms.com/page/12", "<a href='http://www.rockrms.com/page/12'>http://www.rockrms.com/page/12</a>" )]
        [DataRow( "/page/12", "<a href='/page/12'>/page/12</a>" )]
        [DataRow( "www.rockrms.com", "<a href='http://www.rockrms.com/'>www.rockrms.com</a>" )]
        public void GetHtmlValue_WithSafeUrl_ReturnsLink( string value, string expected )
        {
            var fieldType = new UrlLinkFieldType();

            Assert.AreEqual( expected, fieldType.GetHtmlValue( value, new Dictionary<string, string>() ) );
        }

        [TestMethod]
        [DataRow( "https://x.example/'><b>TH120-marker</b>" )]
        [DataRow( "/page/12'><b>TH120-marker</b>" )]
        [DataRow( "<b>TH120-marker</b>" )]
        public void GetHtmlValue_WithMarkup_ReturnsEncodedValue( string value )
        {
            var fieldType = new UrlLinkFieldType();

            var html = fieldType.GetHtmlValue( value, new Dictionary<string, string>() );

            Assert.IsFalse( html.Contains( "<b>" ), html );
            Assert.IsFalse( html.Contains( "/'" ) || html.Contains( "12'" ), html );
            Assert.IsTrue( html.Contains( "&lt;b&gt;TH120-marker&lt;/b&gt;" ), html );
        }

        [TestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "JaVaScRiPt:alert(1)" )]
        [DataRow( "mailto:ted@rocksolidchurchdemo.com" )]
        [DataRow( "data:text/html,<script>alert(1)</script>" )]
        public void GetHtmlValue_WithUnsafeScheme_ReturnsEncodedTextWithoutLink( string value )
        {
            var fieldType = new UrlLinkFieldType();

            var html = fieldType.GetHtmlValue( value, new Dictionary<string, string>() );

            Assert.AreEqual( value.EncodeHtml(), html );
        }

        [TestMethod]
        public void GetHtmlValue_WithAlwaysShowCondensed_ReturnsEncodedValue()
        {
            var fieldType = new UrlLinkFieldType();
            var configurationValues = new Dictionary<string, string>
            {
                ["ShouldAlwaysShowCondensed"] = "True"
            };

            var html = fieldType.GetHtmlValue( "<b>TH120-marker</b>", configurationValues );

            Assert.AreEqual( "&lt;b&gt;TH120-marker&lt;/b&gt;", html );
        }

        [TestMethod]
        public void GetHtmlValue_WithQueryString_EncodesAmpersand()
        {
            var fieldType = new UrlLinkFieldType();

            var html = fieldType.GetHtmlValue( "https://x.example/?a=1&b=2", new Dictionary<string, string>() );

            Assert.AreEqual( "<a href='https://x.example/?a=1&amp;b=2'>https://x.example/?a=1&amp;b=2</a>", html );
        }

        [TestMethod]
        [DataRow( "https://x.example/?a=1&b=2" )]
        [DataRow( "<b>TH120-marker</b>" )]
        public void FormatValue_Condensed_ReturnsRawValue( string value )
        {
            var fieldType = new UrlLinkFieldType();

            var text = fieldType.FormatValue( null, value, new Dictionary<string, ConfigurationValue>(), true );

            Assert.AreEqual( value, text );
        }

        [TestMethod]
        [DataRow( "https://x.example/?a=1&b=2" )]
        [DataRow( "<b>TH120-marker</b>" )]
        public void FormatValue_WithAlwaysShowCondensed_ReturnsRawValue( string value )
        {
            var fieldType = new UrlLinkFieldType();

            var text = fieldType.FormatValue( null, value, GetAlwaysShowCondensedConfiguration(), false );

            Assert.AreEqual( value, text );
        }

        [TestMethod]
        [DataRow( "https://x.example/?a=1&b=2", "https://x.example/?a=1&amp;b=2" )]
        [DataRow( "<b>TH120-marker</b>", "&lt;b&gt;TH120-marker&lt;/b&gt;" )]
        public void FormatValueAsHtml_Condensed_ReturnsEncodedValue( string value, string expected )
        {
            var fieldType = new UrlLinkFieldType();

            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, value, new Dictionary<string, ConfigurationValue>(), true ) );
            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, null, null, value, new Dictionary<string, ConfigurationValue>(), true ) );
        }

        [TestMethod]
        [DataRow( "https://x.example/?a=1&b=2", "https://x.example/?a=1&amp;b=2" )]
        [DataRow( "<b>TH120-marker</b>", "&lt;b&gt;TH120-marker&lt;/b&gt;" )]
        public void FormatValueAsHtml_WithAlwaysShowCondensed_ReturnsEncodedValue( string value, string expected )
        {
            var fieldType = new UrlLinkFieldType();

            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, value, GetAlwaysShowCondensedConfiguration(), false ) );
            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, null, null, value, GetAlwaysShowCondensedConfiguration(), false ) );
        }

        [TestMethod]
        public void FormatValueAsHtml_NotCondensed_ReturnsLink()
        {
            var fieldType = new UrlLinkFieldType();

            var html = fieldType.FormatValueAsHtml( null, "https://x.example/?a=1&b=2", new Dictionary<string, ConfigurationValue>(), false );

            Assert.AreEqual( "<a href='https://x.example/?a=1&amp;b=2'>https://x.example/?a=1&amp;b=2</a>", html );
        }

        private static Dictionary<string, ConfigurationValue> GetAlwaysShowCondensedConfiguration()
        {
            return new Dictionary<string, ConfigurationValue>
            {
                [UrlLinkFieldType.ConfigurationKey.ShouldAlwaysShowCondensed] = new ConfigurationValue( "True" )
            };
        }
    }
}
