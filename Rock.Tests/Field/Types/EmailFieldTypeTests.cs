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
    /// Unit tests for the <see cref="EmailFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class EmailFieldTypeTests
    {
        [TestMethod]
        public void GetHtmlValue_WithValidAddress_ReturnsMailtoLink()
        {
            var fieldType = new EmailFieldType();

            var html = fieldType.GetHtmlValue( "person@example.com", new Dictionary<string, string>() );

            Assert.AreEqual( "<a href='mailto:person@example.com'>person@example.com</a>", html );
        }

        [TestMethod]
        public void GetHtmlValue_WithQuoteInValidAddress_ReturnsEncodedMailtoLink()
        {
            var fieldType = new EmailFieldType();

            var html = fieldType.GetHtmlValue( "o'brien@example.com", new Dictionary<string, string>() );

            Assert.AreEqual( "<a href='mailto:o&#39;brien@example.com'>o&#39;brien@example.com</a>", html );
        }

        [TestMethod]
        [DataRow( "a@example.com\"><b>TH120-marker</b>" )]
        [DataRow( "a@example.com'><b>TH120-marker</b>" )]
        [DataRow( "<b>TH120-marker</b>" )]
        public void GetHtmlValue_WithMarkup_ReturnsEncodedTextWithoutLink( string value )
        {
            var fieldType = new EmailFieldType();

            var html = fieldType.GetHtmlValue( value, new Dictionary<string, string>() );

            Assert.AreEqual( value.EncodeHtml(), html );
        }

        [TestMethod]
        public void GetCondensedHtmlValue_WithMarkup_ReturnsEncodedTextWithoutLink()
        {
            var fieldType = new EmailFieldType();
            var value = "a@example.com\"><b>TH120-marker</b>";

            var html = fieldType.GetCondensedHtmlValue( value, new Dictionary<string, string>() );

            Assert.IsFalse( html.Contains( "<b>" ), html );
            Assert.IsFalse( html.Contains( "<a" ), html );
        }
    }
}
