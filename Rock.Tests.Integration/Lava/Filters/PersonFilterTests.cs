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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests for Lava Filters categorized as "Person".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class PersonFilterTests : LavaIntegrationTestBase
    {
        #region Address

        [TestMethod]
        public void PersonAddress_WithAddressTypeParameterOnly_ReturnsFullAddress()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "Home Address: {{ CurrentPerson | Address:'Home' }}";
            var outputExpected = @"Home Address: 11624 N 31st Dr Phoenix, AZ 85029-3202";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonAddress_WithFormatTemplateFieldCityState_ReturnsExpectedOutput()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "{{ CurrentPerson | Address:'Home','[[City]], [[State]]' }}";
            var outputExpected = @"Phoenix, AZ";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonAddress_WithFormatTemplateFieldGuid_ReturnsLocationGuid()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var person = values["CurrentPerson"] as Person;

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"{{ CurrentPerson | Address:'Home','[[Guid]]' }}";

            var outputExpected = person.GetHomeLocation()?.Guid.ToString( "D" );

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        #endregion

        #region Notes

        [TestMethod]
        public void PersonNotes_WithCurrentPersonHavingNotes_ReturnsNotes()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var person = values["CurrentPerson"] as Person;

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"
{% assign notes = CurrentPerson | Notes:'4,5','asc',2 %}
{% for note in notes %}
    <p>{{ note.Text }}</p>
{% endfor %}
";

            var outputExpected = @"
<p>Called Ted and heard that his mother is in the hospital and could use prayer.</p>
<p>Talked to Ted today about starting a new Young Adults ministry</p>
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        #endregion

        #region Person Tokens

        /// <summary>
        /// Verify that a PersonToken cannot be created where the Account Protection Profile of the person is equal to the threshold
        /// in Security Settings.
        /// </summary>
        [TestMethod]
        public void PersonTokenCreate_WithPersonHavingProtectedProfile_ReturnsTokenProhibited()
        {
            SetPersonAccountProtectionProfile( TestGuids.TestPeople.TedDecker, AccountProtectionProfile.Extreme );

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var options = new LavaTestRenderOptions { MergeFields = values, Wildcards = new List<string> { "<token>" } };

            var template = @"
Your token is: {{ CurrentPerson | PersonTokenCreate }}
";
            var outputExpected = @"
Your token is: TokenProhibited
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        /// <summary>
        /// Verify that a PersonToken can be created where the Account Protection Profile of the person is less than the threshold
        /// in Security Settings.
        /// </summary>
        [TestMethod]
        public void PersonTokenCreate_WithPersonHavingUnprotectedProfile_ReturnsTokenString()
        {
            SetPersonAccountProtectionProfile( TestGuids.TestPeople.SamHanks, AccountProtectionProfile.Medium );

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.SamHanks.AsGuid(), null, "Person" );
            var options = new LavaTestRenderOptions { MergeFields = values, Wildcards = new List<string> { "<token>" } };

            var template = @"
Your token is: {{ Person | PersonTokenCreate }}
";
            var outputExpected = @"
Your token is: <token>
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        /// <summary>
        /// Verify that a PersonToken can be created where the Account Protection Profile of the person is not set.
        /// </summary>
        [TestMethod]
        public void PersonTokenCreate_WithPersonHavingDefaultProtectionProfile_ReturnsTokenString()
        {
            SetPersonAccountProtectionProfile( TestGuids.TestPeople.BillMarble, AccountProtectionProfile.Low );

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.BillMarble.AsGuid() );
            var options = new LavaTestRenderOptions { MergeFields = values, Wildcards = new List<string> { "<token>" } };

            var template = @"
Your token is: {{ CurrentPerson | PersonTokenCreate }}
";
            var outputExpected = @"
Your token is: <token>
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }
        private void SetPersonAccountProtectionProfile( String guid, AccountProtectionProfile profile )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( guid.AsGuid() );
            person.AccountProtectionProfile = profile;

            rockContext.SaveChanges();
        }

        #endregion

        #region Steps

        [TestMethod]
        [Ignore( "Requires additional sample test data." )]
        public void PersonSteps_WithDefaultParameters_ReturnsAllStepsForCurrentPerson()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"
{% assign steps = CurrentPerson | Steps %}
{% for step in steps %}
    <p>{{ step.StepType.Name }} - {{ step.StepStatus.Name }}</p>
{% endfor %}
";

            var outputExpected = @"<p>Baptism-Success</p><p>Confirmation-Success</p><p>Marriage-Incomplete</p><p>Marriage-Success</p><p>Attender-Completed</p><p>Volunteer-Started</p>";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        [Ignore( "Requires additional sample test data." )]
        public void PersonSteps_WithStepProgramParameter_ReturnsStepsForProgramOnly()
        {
            // Test with Step Program Guid
            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                Tests.Shared.TestGuids.Steps.ProgramAlphaGuid.ToString(),
                null,
                null,
                "Ted Decker:<p>Attender - Completed</p><p>Volunteer - Started</p>" );

            // Test with Step Program Id.
            var dataContext = new RockContext();

            var stepProgramId = new StepProgramService( dataContext ).GetId( Tests.Shared.TestGuids.Steps.ProgramAlphaGuid );

            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                stepProgramId.ToString(),
                null,
                null,
                "Ted Decker: <p>Attender - Completed</p><p>Volunteer - Started</p>" );
        }

        [TestMethod]
        [Ignore( "Requires additional sample test data." )]
        public void PersonSteps_WithStepTypeParameter_ReturnsStepsForStepTypeOnly()
        {
            // Test with Step Type Guid
            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                null,
                Tests.Shared.TestGuids.Steps.StepTypeBaptismGuid.ToString(),
                null,
                "Ted Decker: <p>Baptism - Success</p>" );

            // Test with Step Type Id.
            var dataContext = new RockContext();

            var stepTypeId = new StepTypeService( dataContext ).GetId( Tests.Shared.TestGuids.Steps.StepTypeBaptismGuid );

            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                null,
                stepTypeId.ToString(),
                null,
                "Ted Decker: <p>Baptism - Success</p>" );
        }

        [TestMethod]
        [Ignore( "Requires additional sample test data." )]
        public void PersonSteps_WithStatusParameter_ReturnsStepsHavingMatchingStatusOnly()
        {
            // Test with Status Name
            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                null,
                null,
                "Incomplete",
                "Ted Decker: <p>Marriage - Incomplete</p>" );

            // Test with Status Guid
            PersonStepsTestWithTemplate( TestGuids.TestPeople.TedDecker,
                null,
                null,
                TestGuids.Steps.StatusSacramentsSuccessGuid.ToString(),
                "Ted Decker: <p>Baptism - Success</p><p>Confirmation - Success</p><p>Marriage - Success</p>" );
        }

        private void PersonStepsTestWithTemplate( string testPersonGuid, string stepProgram, string stepType, string stepStatus, string expectedOutput )
        {
            var values = AddTestPersonToMergeDictionary( testPersonGuid.AsGuid() );

            values.Add( "stepProgram", stepProgram );
            values.Add( "stepType", stepType );
            values.Add( "stepStatus", stepStatus );

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"
{% assign steps = CurrentPerson | Steps:stepProgram,stepStatus,stepType %}
{{ CurrentPerson.FullName }}:
{% for step in steps %}
    <p>{{ step.StepType.Name }} - {{ step.StepStatus.Name }}</p>
{% endfor %}
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                options );
        }

        #endregion

        private LavaDataDictionary AddTestPersonToMergeDictionary( Guid personGuid, LavaDataDictionary dictionary = null, string mergeKey = "CurrentPerson" )
        {
            var rockContext = new RockContext();

            var tedDeckerPerson = new PersonService( rockContext ).Queryable().First( x => x.Guid == personGuid );

            if ( dictionary == null )
            {
                dictionary = new LavaDataDictionary();
            }

            dictionary.AddOrReplace( mergeKey, tedDeckerPerson );

            return dictionary;
        }

    }
}
