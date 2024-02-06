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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
{
    [TestClass]
    public class AttributeFilterTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Attribute

        /// <summary>
        /// Applying the Attribute filter to a known entity returns the Attribute value.
        /// </summary>
        [TestMethod]
        public void AttributeFilter_ForEntityDefaultAttribute_ReturnsCorrectValue()
        {
            var personDecker = TestHelper.GetTestPersonTedDecker();

            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = new RockContext();

            var tedDeckerPerson = new PersonService( rockContext ).Queryable().First( x => x.Guid == tedDeckerGuid );

            var values = new LavaDataDictionary { { "Person", tedDeckerPerson } };

            var options = new LavaTestRenderOptions { MergeFields = values };

            TestHelper.AssertTemplateOutput("2001-09-13",
                "{{ Person | Attribute:'BaptismDate' | Date:'yyyy-MM-dd' }}",
                options );
        }

        [TestMethod]
        public void AttributeFilter_ForBinaryFileWithObjectParameter_ReturnsBinaryFileObject()
        {
            var inputTemplate = @"
{% contentchannelitem expression:'ContentChannel.Name == ""External Website Ads"" && Title == ""SAMPLE: Easter""' %}
    {% assign image = contentchannelitem | Attribute:'Image','Object' %}
    Base64Format: {{ image | Base64Encode }}<br/>
{% endcontentchannelitem %}
";

            var expectedOutput = @"Base64Format: /9j/4AAQSkZJRgABAQEAAAAAAAD/*";

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "*" }, EnabledCommands = "RockEntity" };

            TestHelper.AssertTemplateOutput( expectedOutput, inputTemplate, options );
        }

        /// <summary>
        /// Using the Attribute filter output in a conditional operator returns the expected result.
        /// </summary>
        [TestMethod]
        public void AttributeFilter_RawValueBooleanComparison_ConvertsRawValueToBoolean()
        {
            // Set Attribute [BaptizedHere] = True for Ted Decker. 
            var personDecker = TestHelper.GetTestPersonTedDecker();

            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );

            var tedDeckerPerson = personService.Queryable().First( x => x.Guid == tedDeckerGuid );

            tedDeckerPerson.LoadAttributes();

            tedDeckerPerson.SetAttributeValue( "BaptizedHere", "True" );

            rockContext.SaveChanges();

            var values = new LavaDataDictionary { { "Person", tedDeckerPerson } };

            var options = new LavaTestRenderOptions { MergeFields = values };

            // Test a boolean comparison for the Raw Value of the [BaptizedHere] Attribute.
            var inputTemplate = @"
{%- assign isBaptizedHere = Person | Attribute:'BaptizedHere','RawValue' | AsBoolean -%}
{%- if isBaptizedHere != '' and isBaptizedHere == true -%}
True
{%- endif -%}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, inputTemplate, options );
        }

        #endregion
    }
}
