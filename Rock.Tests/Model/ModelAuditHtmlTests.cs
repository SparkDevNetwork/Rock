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

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class ModelAuditHtmlTests
    {
        [TestMethod]
        public void GetCreatedAuditHtml_WithHtmlInPersonName_EncodesName()
        {
            var group = new Group
            {
                CreatedByPersonAlias = new PersonAlias
                {
                    PersonId = 5,
                    Person = new Person { NickName = "<b>x</b>", LastName = "O'Brien & Sons" }
                }
            };

            var html = group.GetCreatedAuditHtml( "/" );

            Assert.IsTrue( html.Contains( "&lt;b&gt;x&lt;/b&gt;" ), html );
            Assert.IsTrue( html.Contains( "O&#39;Brien &amp; Sons" ), html );
            Assert.IsFalse( html.Contains( "<b>" ), html );
        }

        [TestMethod]
        public void GetModifiedAuditHtml_WithHtmlInPersonName_EncodesName()
        {
            var group = new Group
            {
                ModifiedByPersonAlias = new PersonAlias
                {
                    PersonId = 5,
                    Person = new Person { NickName = "<img src=x onerror=alert(1)>", LastName = "Smith" }
                }
            };

            var html = group.GetModifiedAuditHtml( "/" );

            Assert.IsFalse( html.Contains( "<img" ), html );
            Assert.IsTrue( html.Contains( "<a href=/Person/5>&lt;img" ), html );
        }
    }
}
