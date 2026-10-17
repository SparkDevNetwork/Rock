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
    /// Unit tests for the <see cref="VideoUrlFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class VideoUrlFieldTypeTests
    {
        [TestMethod]
        public void GetHtmlValue_WithSafeUrl_ReturnsPlayer()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetHtmlValue( "https://x.example/v.mp4", new Dictionary<string, string>() );

            Assert.IsTrue( html.Contains( "<video" ), html );
            Assert.IsTrue( html.Contains( "src='https://x.example/v.mp4'" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_WithYouTubeUrl_ReturnsYouTubePlayer()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetHtmlValue( "https://www.youtube.com/watch?v=abc", new Dictionary<string, string>() );

            Assert.IsTrue( html.Contains( "src='https://www.youtube.com/watch?v=abc'" ), html );
            Assert.IsTrue( html.Contains( "type='video/youtube'" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_WithQuoteBreakout_DoesNotEndAttribute()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetHtmlValue( "https://x.example/v.mp4'><b>TH120-marker</b>", new Dictionary<string, string>() );

            Assert.IsFalse( html.Contains( "<b>" ), html );
            Assert.IsFalse( html.Contains( "v.mp4'" ), html );
        }

        [TestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "data:video/mp4;base64,AAAA" )]
        public void GetHtmlValue_WithUnsafeScheme_ReturnsEncodedTextWithoutPlayer( string value )
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetHtmlValue( value, new Dictionary<string, string>() );

            Assert.AreEqual( value.EncodeHtml(), html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_WithSafeUrl_ReturnsLink()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetCondensedHtmlValue( "https://x.example/v.mp4", new Dictionary<string, string>() );

            Assert.AreEqual( "<a href=\"https://x.example/v.mp4\">https://x.example/v.mp4</a>", html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_WithUnsafeScheme_ReturnsEncodedTextWithoutLink()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.GetCondensedHtmlValue( "javascript:alert('x')", new Dictionary<string, string>() );

            Assert.AreEqual( "javascript:alert(&#39;x&#39;)", html );
        }

        [TestMethod]
        [DataRow( "https://x.example/v.mp4?a=1&b=2" )]
        [DataRow( "<b>TH120-marker</b>" )]
        public void FormatValue_Condensed_ReturnsRawValue( string value )
        {
            var fieldType = new VideoUrlFieldType();

            var text = fieldType.FormatValue( null, value, new Dictionary<string, ConfigurationValue>(), true );

            Assert.AreEqual( value, text );
        }

        [TestMethod]
        [DataRow( "https://x.example/v.mp4?a=1&b=2", "https://x.example/v.mp4?a=1&amp;b=2" )]
        [DataRow( "<b>TH120-marker</b>", "&lt;b&gt;TH120-marker&lt;/b&gt;" )]
        public void FormatValueAsHtml_Condensed_ReturnsEncodedValue( string value, string expected )
        {
            var fieldType = new VideoUrlFieldType();

            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, value, new Dictionary<string, ConfigurationValue>(), true ) );
            Assert.AreEqual( expected, fieldType.FormatValueAsHtml( null, null, null, value, new Dictionary<string, ConfigurationValue>(), true ) );
        }

        [TestMethod]
        public void FormatValueAsHtml_NotCondensed_EncodesPlayerSource()
        {
            var fieldType = new VideoUrlFieldType();

            var html = fieldType.FormatValueAsHtml( null, "https://x.example/v.mp4?a=1&b=2", new Dictionary<string, ConfigurationValue>(), false );

            Assert.IsTrue( html.Contains( "src='https://x.example/v.mp4?a=1&amp;b=2'" ), html );
        }
    }
}
