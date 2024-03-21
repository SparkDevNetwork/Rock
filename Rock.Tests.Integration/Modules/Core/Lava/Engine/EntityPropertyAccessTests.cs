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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    [TestClass]
    public class EntityPropertyAccessTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Attribute

        /// <summary>
        /// Accessing the AttributeValues collection of an entity returns the Attribute values.
        /// </summary>
        [TestMethod]
        public void EntityPropertyAccess_ForPersonAttributeValues_ReturnsCorrectValues()
        {
            var rockContext = new RockContext();

            var testPerson = new PersonService( rockContext ).Queryable().First( x => x.NickName == "Ted" && x.LastName == "Decker" );

            testPerson.LoadAttributes();

            var values = new LavaDataDictionary { { "Person", testPerson } };

            var input = @"
{% for av in Person.AttributeValues %}
    {% if av.ValueFormatted != null and av.ValueFormatted != '' %}
        {{ av.AttributeName }}: {{ av.ValueFormatted }}<br>
    {% endif %}
{% endfor %}
";
            var expectedOutput = @"
 Employer: Rock Solid Church<br>
";

            var options = new LavaTestRenderOptions() { MergeFields = values, OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion
    }
}
