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

using Rock.Field.Types;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="AudioUrlFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class AudioUrlFieldTypeTests
    {
        [TestMethod]
        public void GetHtmlValue_WithSafeUrl_ReturnsPlayer()
        {
            var fieldType = new AudioUrlFieldType();

            var html = fieldType.GetHtmlValue( "https://x.example/a.mp3", new Dictionary<string, string>() );

            Assert.IsTrue( html.Contains( "<audio" ), html );
            Assert.IsTrue( html.Contains( "src='https://x.example/a.mp3'" ), html );
        }

        [TestMethod]
        public void GetHtmlValue_WithQuoteBreakout_DoesNotEndAttribute()
        {
            var fieldType = new AudioUrlFieldType();

            var html = fieldType.GetHtmlValue( "https://x.example/a.mp3'><b>TH120-marker</b>", new Dictionary<string, string>() );

            Assert.IsFalse( html.Contains( "<b>" ), html );
            Assert.IsFalse( html.Contains( "a.mp3'" ), html );
        }

        [TestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "data:audio/mpeg;base64,AAAA" )]
        public void GetHtmlValue_WithUnsafeScheme_ReturnsEncodedTextWithoutPlayer( string value )
        {
            var fieldType = new AudioUrlFieldType();

            var html = fieldType.GetHtmlValue( value, new Dictionary<string, string>() );

            Assert.AreEqual( value.EncodeHtml(), html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_WithSafeUrl_ReturnsLink()
        {
            var fieldType = new AudioUrlFieldType();

            var html = fieldType.GetCondensedHtmlValue( "https://x.example/a.mp3", new Dictionary<string, string>() );

            Assert.AreEqual( "<a href=\"https://x.example/a.mp3\">https://x.example/a.mp3</a>", html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_WithUnsafeScheme_ReturnsEncodedTextWithoutLink()
        {
            var fieldType = new AudioUrlFieldType();

            var html = fieldType.GetCondensedHtmlValue( "javascript:alert('x')", new Dictionary<string, string>() );

            Assert.AreEqual( "javascript:alert(&#39;x&#39;)", html );
        }
    }
}
