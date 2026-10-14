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

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Integration.Core.Lava.Engine
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
            var rockContext = RockApp.Current.CreateRockContext();

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
            var expectedOutput = "\n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        First Visit: 12/15/2012<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Employer: Rock Solid Church<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Position: Outreach Pastor<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Background Check Date: 10/4/2010<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Do Not Send Giving Statement: No<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Baptism Date: 9/13/2001<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Adaptive D: 62<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Baptized Here: No<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Adaptive I: 79<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Adaptive S: 42<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Adaptive C: 12<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        DISC: D Value: <span class='label scale-label' style='background-color:#709ac7'>High</span><br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        DISC: I Value: <span class='label scale-label' style='background-color:#f4cf68'>High</span><br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        DISC: S Value: <span class='label scale-label' style='background-color:#c1debf'>Low</span><br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        DISC: C Value: <span class='label scale-label' style='background-color:#fac4c2'>Low</span><br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        DISC Last Save Date: 2/2/2013<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "        Personality Type: ID<br>\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n"
                + "    \n"
                + "\n";

            var options = new LavaRenderOptions() { MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.Contains( expectedOutput, output );
            } );
        }

        #endregion
    }
}
