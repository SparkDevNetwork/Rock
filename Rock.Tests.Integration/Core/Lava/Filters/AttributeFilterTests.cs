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
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Filters
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
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = RockApp.Current.CreateRockContext();

            var tedDeckerPerson = new PersonService( rockContext ).Queryable().First( x => x.Guid == tedDeckerGuid );

            var values = new LavaDataDictionary { { "Person", tedDeckerPerson } };

            var options = new LavaRenderOptions { MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ Person | Attribute:'BaptismDate' | Date:'yyyy-MM-dd' }}", options );

                Assert.AreEqual( "2001-09-13", output );
            } );
        }

        [TestMethod]
        public void AttributeFilter_ForBinaryFileWithObjectParameter_ReturnsBinaryFileObject()
        {
            var inputTemplate = @"
{%- contentchannelitem expression:'ContentChannel.Name == ""External Website Ads"" && Title == ""SAMPLE: Easter""' -%}
    {%- assign image = contentchannelitem | Attribute:'Image','Object' -%}
    Base64Format: {{ image | Base64Encode }}<br/>
{%- endcontentchannelitem -%}
";

            var expectedOutput = "Base64Format: /9j/4AAQSkZJRgABAQEAAAAAAAD/{base64Data}<br/>\n";

            var options = new LavaRenderOptions { EnabledCommands = "RockEntity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate, options );

                LavaAssert.Matches( expectedOutput, output, "{base64Data}" );
            } );
        }

        /// <summary>
        /// Using the Attribute filter output in a conditional operator returns the expected result.
        /// </summary>
        [TestMethod]
        public void AttributeFilter_RawValueBooleanComparison_ConvertsRawValueToBoolean()
        {
            // Set Attribute [BaptizedHere] = True for Ted Decker.
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = RockApp.Current.CreateRockContext();

            var personService = new PersonService( rockContext );

            var tedDeckerPerson = personService.Queryable().First( x => x.Guid == tedDeckerGuid );

            tedDeckerPerson.LoadAttributes();

            tedDeckerPerson.SetAttributeValue( "BaptizedHere", "True" );

            rockContext.SaveChanges();

            var values = new LavaDataDictionary { { "Person", tedDeckerPerson } };

            var options = new LavaRenderOptions { MergeFields = values };

            // Test a boolean comparison for the Raw Value of the [BaptizedHere] Attribute.
            var inputTemplate = @"
{%- assign isBaptizedHere = Person | Attribute:'BaptizedHere','RawValue' | AsBoolean -%}
{%- if isBaptizedHere != '' and isBaptizedHere == true -%}
True
{%- endif -%}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate, options );

                Assert.AreEqual( "True", output );
            } );
        }

        #endregion
    }
}
