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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class EntityPropertyAccessTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Attribute

        /// <summary>
        /// Accessing the AttributeValues collection of an entity returns the Attribute values.
        /// </summary>
        [TestMethod]
        public void EntityPropertyAccess_ForGroupAttributeValues_ReturnsCorrectValue()
        {
            var rockContext = new RockContext();

            var testGroup = new GroupService( rockContext ).Queryable().First( x => x.Name == "Decker Group" );

            testGroup.LoadAttributes();

            var values = new LavaDataDictionary { { "Group", testGroup } };

            var input = @"
 {% for attribute in Group.AttributeValues %}
    {% if attribute.ValueFormatted > '' %}
        {{ attribute.AttributeName }}: {{ attribute.ValueFormatted }}<br>
    {% endif %}
{% endfor %}
";
            var expectedOutput = @"
Topic: Book of Genesis<br>
";

            var options = new LavaTestRenderOptions() { MergeFields = values };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion
    }
}
