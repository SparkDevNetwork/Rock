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

using Rock.Lava;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Documentation
{
    /// <summary>
    /// Tests to verify examples provided in the Lava documentation for the if tag.
    /// </summary>
    /// <remarks>
    /// These examples seed a person attribute and read it back, so they need a
    /// database. The examples that only need a merge field live in Rock.Tests as
    /// unit tests.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class IfTagExamples : LavaIntegrationTestBase
    {
        [TestMethod]
        [Ignore( "This example is incorrect for the Fluid engine. If either operand is numeric, a numeric comparison is made." )]
        public void IfTag_DocumentationExample_NumberStringComparisons()
        {
            var addAttributeArgs = new PersonDataManager.AddPersonAttributeActionArgs
            {
                Key = "AgeInYears",
                FieldTypeIdentifier = SystemGuid.FieldType.TEXT
            };
            PersonDataManager.Instance.AddPersonAttribute( addAttributeArgs );

            var setAttributeArgs = new PersonDataManager.UpdateEntityAttributeValueActionArgs
            {
                UpdateTargetIdentifier = TestGuids.TestPeople.TedDecker,
                Key = "AgeInYears",
                Value = "3"
            };
            PersonDataManager.Instance.SetPersonAttribute( setAttributeArgs );

            var input = @"
{% assign AgeInYears = CurrentPerson | Attribute:'AgeInYears' %}
AgeInYears: ""{{ AgeInYears }}""<br>
{% if AgeInYears > 10 %}
    {{ AgeInYears }} is greater than 10???
{% endif %}
";

            var expectedOutput = @"
AgeInYears: ""3""<br>
3 is greater than 10???
".NormalizeLineEndings();

            var options = new LavaRenderOptions
            {
                MergeFields = new LavaDataDictionary
                {
                    { "CurrentPerson", TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ) }
                }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_BooleanAttributesTrueFalseOrNull()
        {
            // Create a new Attribute, but do not set any values.
            var addAttributeArgs = new PersonDataManager.AddPersonAttributeActionArgs
            {
                Key = "IsTrained",
                FieldTypeIdentifier = SystemGuid.FieldType.BOOLEAN
            };
            PersonDataManager.Instance.AddPersonAttribute( addAttributeArgs );

            var input = @"
{% assign isTrained = CurrentPerson | Attribute:'IsTrained' | AsBoolean %}
isTrained is: ""{{ isTrained }}""<br>

{% if isTrained == true %}
    Evaluates to true
{% elseif isTrained == false %}
    Evaluates to false
{% elseif isTrained == null %}
    Evaluates to null -- meaning there is no value stored
{% else %}
    Evaluates to something else?
{% endif %}";

            // The assign tag and the if branches each leave behind the newline
            // that followed them, and the matched branch keeps its indentation.
            var expectedOutput = "\n\n"
                + "isTrained is: \"\"<br>\n"
                + "\n\n"
                + "    Evaluates to null -- meaning there is no value stored\n";

            var options = new LavaRenderOptions
            {
                MergeFields = new LavaDataDictionary
                {
                    { "CurrentPerson", TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ) }
                }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }
    }
}
