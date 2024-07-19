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

using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Documentation
{
    /// <summary>
    /// Tests to verify examples provided in the Lava documentation.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class IfTagExamples : LavaIntegrationTestBase
    {
        [TestMethod]
        public void IfTag_DocumentationExample_TestingIfPropertyExists()
        {
            var personNoCallSign = new
            {
                FullName = "Ted Decker"
            };

            var input = @"
{% if Person.CallSign %}
    {{ Person.FullName }} you have a call sign... you must be cool!
{% else %}
    Oh... hi {{ Person.FullName }}
{% endif %}
";

            // Test the documentation example.
            var options = LavaTestRenderOptions.AllEngines
                .WithContextVariable( "Person", personNoCallSign )
                .WithIgnoreWhiteSpace();

            TestHelper.AssertTemplateOutput( "Oh... hi Ted Decker", input, options );

            // Test the inverse case of the example.
            var personWithCallSign = new
            {
                FullName = "Cindy Decker",
                CallSign = "C.D."
            };

            options.WithContextVariable( "Person", personWithCallSign );

            TestHelper.AssertTemplateOutput( "Cindy Decker you have a call sign... you must be cool!", input, options );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_TestingForEmptyProperty()
        {
            var person = new
            {
                FullName = "Ted Decker",
                MiddleName = ""
            };

            var input = @"
{% if Person.MiddleName == '' %}
    {{ Person.FullName }}, what no middle name?!
{% endif %}
";

            var options = LavaTestRenderOptions.AllEngines
                .WithContextVariable( "Person", person )
                .WithIgnoreWhiteSpace();

            TestHelper.AssertTemplateOutput( "Ted Decker, what no middle name?!", input, options );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_TestingForEmptyArray()
        {
            var personWithoutNumbers = new
            {
                FullName = "Ted Decker",
                PhoneNumbers = new string[] { }
            };
            var personWithNumbers = new
            {
                FullName = "Cindy Decker",
                PhoneNumbers = new string[] { "1234567890" }
            };

            var input = @"
{% if Person.PhoneNumbers != empty %}
    You have phone numbers
{% endif %}
";

            var options = LavaTestRenderOptions.AllEngines
                 .WithContextVariable( "Person", personWithNumbers )
                 .WithIgnoreWhiteSpace();

            TestHelper.AssertTemplateOutput( "You have phone numbers", input, options );

            options.WithContextVariable( "Person", personWithoutNumbers );

            TestHelper.AssertTemplateOutput( string.Empty, input, options );
        }

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

            var output = @"
AgeInYears: ""3""<br>
3 is greater than 10???
";

            var options = LavaTestRenderOptions.AllEngines
                .WithIgnoreWhiteSpace()
                .WithCurrentPerson( TestGuids.TestPeople.TedDecker );

            TestHelper.AssertTemplateOutput( output, input, options );
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

            var output = @"
isTrained is: """"<br>
Evaluates to null -- meaning there is no value stored
";

            var options = LavaTestRenderOptions.AllEngines
                .WithIgnoreWhiteSpace()
                .WithCurrentPerson( TestGuids.TestPeople.TedDecker );

            TestHelper.AssertTemplateOutput( output, input, options );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_OrderOfLogicalOperations()
        {
            var input = @"
{% if true or false and false %}
  This evaluates to true, since the 'and' condition is checked first.
{% endif %}";

            TestHelper.AssertTemplateOutput( "This evaluates to true, since the 'and' condition is checked first.",
                input,
                LavaTestRenderOptions.AllEngines.WithIgnoreWhiteSpace() );
        }
    }
}
