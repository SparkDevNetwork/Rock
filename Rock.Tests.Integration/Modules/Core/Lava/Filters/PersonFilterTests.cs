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
using Rock.Security;
using Rock.Tests.Integration.Organization;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
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

        #region Campus

        [TestMethod]
        public void PersonCampus_WithDefaultOptions_ReturnsPrimaryCampus()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var person = values["CurrentPerson"] as Person;
            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"{% assign campus = CurrentPerson | Campus %}{{ campus.Name }}";
            var outputExpected = person.GetCampus()?.Name.SplitCase();

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonCampus_WithIntegerInput_ResolvesPersonFromId()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var person = values["CurrentPerson"] as Person;

            var template = @"{% assign campus = " + person.Id.ToString() + " | Campus %}{{ campus.Name }}";
            var outputExpected = person.GetCampus()?.Name.SplitCase();

            TestHelper.AssertTemplateOutput( outputExpected,
                template );
        }

        #region Filter: NearestCampus

        [TestMethod]
        public void PersonNearestCampus_WithDefaultOptions_ReturnsSingleCampus()
        {
            var campusManager = CampusDataManager.Instance;
            campusManager.AddCampusTestDataSet();

            //var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var options = new LavaTestRenderOptions { EnabledCommands = "RockEntity" };

            var template = @"
{% person where:'[NickName] == ""Ted"" && [LastName] == ""Decker""' limit:'1' %}
{% assign campus = person | NearestCampus %}
Ted's Nearest Campus: {{ campus.Name }}
{% endperson %}";
            var outputExpected = "Ted's Nearest Campus: Main Campus";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        /// <summary>
        /// Documentation Example: Nearest Campus.
        /// </summary>
        [TestMethod]
        public void PersonNearestCampus_ReturningMultipleResults_ReturnsClosestCampusFirst()
        {
            var campusManager = CampusDataManager.Instance;
            campusManager.AddCampusTestDataSet();

            var options = new LavaTestRenderOptions { EnabledCommands = "RockEntity" };

            var template = @"
{% person where:'[NickName] == ""Ted"" && [LastName] == ""Decker""' limit:'1' %}
{% assign campus = person | NearestCampus %}
The nearest campus to {{ person.NickName }} is: {{ campus.Name }}.
<hr>
{% assign campusList = person | NearestCampus:2 %}
The two nearest campuses to {{ person.NickName }} are: {{ campusList | Select:'Name' | Join:',' }}.
{% endperson %}
";
            var outputExpected = @"
The nearest campus to Ted is: Main Campus.
<hr>
The two nearest campuses to Ted are: Main Campus, North Campus.
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonNearestCampus_ReturningMultipleResults_ExcludesCampusWithUnknownGeoCode()
        {
            var campusManager = CampusDataManager.Instance;
            campusManager.AddCampusTestDataSet();

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var options = new LavaTestRenderOptions { MergeFields = values, EnabledCommands = "RockEntity" };

            // Request all campuses, and verify that the Online campus is excluded from the result
            // because it has no GeoCode information.
            var template = @"
{% person where:'[NickName] == ""Ted"" && [LastName] == ""Decker""' limit:'1' %}
{% assign campusList = person | NearestCampus:99 %}
Ted's Nearest Campuses: {{ campusList | Select:'Name' | Join:',' }}
{% endperson %}";

            var outputExpected = "Ted's Nearest Campuses: Main Campus, North Campus, South Campus";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonNearestCampus_ForPersonWithNoMappedLocation_ReturnsEmptyResultSet()
        {
            var campusManager = CampusDataManager.Instance;
            campusManager.AddCampusTestDataSet();

            // Add a new Person with no Location.
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var testPersonGuidString = "0875D029-FE06-409F-A7D0-CEDBE18D454F";

            var person = personService.Get( testPersonGuidString );
            if ( person != null )
            {
                TestDataHelper.DeletePersonByGuid( new List<Guid> { testPersonGuidString.AsGuid() } );
            }

            person = new Person()
            {
                Guid = testPersonGuidString.AsGuid(),
                RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "John",
                LastName = "Smith"
            };

            PersonService.SaveNewPerson( person, rockContext );

            var options = new LavaTestRenderOptions { EnabledCommands = "RockEntity" };

            var template = @"
{% person where:'[Guid] == ""<testPersonGuid>""' %}
{% assign campus = person | NearestCampus %}
{{ person.NickName }}'s Nearest Campus: {% if campus == null %}Unknown{% endif %}
{% endperson %}";

            template = template.Replace( "<testPersonGuid>", testPersonGuidString );

            var outputExpected = "John's Nearest Campus: Unknown";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        #endregion

        [TestMethod]
        public void PersonChildren_ForParent_ReturnsAllChildren()
        {
            // Ted Decker has two children, Alex and Noah.
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var template = @"{% assign children = CurrentPerson | Children | Sort:'NickName' %}{% for child in children %}{{ child.NickName }}|{% endfor %}";
            var outputExpected = @"Alex|Noah|";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                new LavaTestRenderOptions { MergeFields = values } );
        }

        [TestMethod]
        public void PersonChildren_ForChild_ReturnsEmptySet()
        {
            // Alex Decker is a child in the Decker family, and has a brother Noah.
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.AlexDecker.AsGuid() );
            var template = @"{% assign children = CurrentPerson | Children | Sort:'NickName' %}{% for child in children %}{{ child.FullName }}|{% endfor %}";
            var outputExpected = @"";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                new LavaTestRenderOptions { MergeFields = values } );
        }

        [TestMethod]
        public void PersonParent_ForChild_ReturnsAllParents()
        {
            // Alex Decker has two parents, Ted and Cindy.
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.AlexDecker.AsGuid() );
            var template = @"{% assign parents = CurrentPerson | Parents | Sort:'NickName' %}{% for parent in parents %}{{ parent.NickName }}|{% endfor %}";
            var outputExpected = @"Cindy|Ted|";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                new LavaTestRenderOptions { MergeFields = values } );
        }

        [TestMethod]
        public void PersonParent_ForSpouse_ReturnsEmptySet()
        {
            // Cindy Decker is a parent of two children with Ted Decker.
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.CindyDecker.AsGuid() );
            var template = @"{% assign parents = CurrentPerson | Parents | Sort:'NickName' %}{% for parent in parents %}{{ parent.NickName }}|{% endfor %}";
            var outputExpected = @"";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                new LavaTestRenderOptions { MergeFields = values } );
        }

        /// <summary>
        /// Verify Fix for Issue #4988.
        /// </summary>
        /// <remarks>(refer https://github.com/SparkDevNetwork/Rock/issues/4988)</remarks>
        [TestMethod]
        public void PersonCampus_WithNullInput_ReturnsEmptyString()
        {
            var template = @"{% assign campus = UndefinedPerson | Campus %}{{ campus.Name }}";
            var outputExpected = "";

            TestHelper.AssertTemplateOutput( outputExpected,
                template );
        }

        #endregion

        #region FamilySalutation

        [TestMethod]
        [Ignore("Fix needed. This test relates to a fix that if not yet merged.")]
        public void FamilySalutation_ForDeckerFamilyWithDefaultParameters_ReturnsParentNames()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "Family Salutation: {{ CurrentPerson | FamilySalutation }}";
            var outputExpected = @"Family Salutation: Ted & Cindy Decker";

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
            // Ensure that Tokens are disabled for the Extreme account protection profile.
            // This is the default setting for the Rock sample data set.
            var rockSecuritySettingsService = new SecuritySettingsService();

            var settings = rockSecuritySettingsService.SecuritySettings;

            if ( !settings.DisableTokensForAccountProtectionProfiles.Contains( AccountProtectionProfile.Extreme ) )
            {
                settings.DisableTokensForAccountProtectionProfiles.Add( AccountProtectionProfile.Extreme );
                rockSecuritySettingsService.Save();
            }

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

        #region IsInSecurityRole

        [TestMethod]
        public void IsInSecurityRole_WithGroupTypeSecurityRole_ReturnsTrue()
        {
            Guid financeAdministrationGroupGuid = Guid.Parse( "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559" );
            var group = new GroupService( new RockContext() ).Queryable().FirstOrDefault( m => m.Guid == financeAdministrationGroupGuid );

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.AlishaMarble.AsGuid() );
            values.AddOrReplace( "GroupId", group.Id );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = @"
{% assign isInRole = CurrentPerson | IsInSecurityRole: GroupId %}
User is in Role = {{ isInRole }}
";
            const string outputExpected = "User is in Role = true";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        private static string _TestSecurityGroupGuid = "A92BC2E7-912F-4538-B5FA-EECFC1D7C68A";

        [TestMethod]
        public void IsInSecurityRole_WithGroupDesignatedAsSecurityRole_ReturnsTrue()
        {
            // Create a new Group with [Security Role] = true.
            var rockContext = new RockContext();
            var addGroupArgs = new TestDataHelper.Crm.AddGroupArgs
            {
                ReplaceIfExists = true,
                GroupGuid = _TestSecurityGroupGuid,
                GroupName = "Test Security Group",
                ForeignKey = "Test Data",
                GroupTypeIdentifier = SystemGuid.GroupType.GROUPTYPE_GENERAL,
            };

            TestDataHelper.Crm.AddGroup( addGroupArgs );

            var groupService = new GroupService( rockContext );
            var group = groupService.Queryable().GetByIdentifierOrThrow( _TestSecurityGroupGuid );

            group.IsSecurityRole = true;
            rockContext.SaveChanges();

            // Add Alisha Marble to the Group.
            var addGroupMemberArgs = new TestDataHelper.Crm.AddGroupMemberArgs
            {
                GroupIdentifier = _TestSecurityGroupGuid,
                PersonIdentifiers = TestGuids.TestPeople.AlishaMarble,
                GroupRoleIdentifier = "Member"
            };
            TestDataHelper.Crm.AddGroupMembers( rockContext, addGroupMemberArgs );
            rockContext.SaveChanges();

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.AlishaMarble.AsGuid() );
            values.AddOrReplace( "GroupId", group.Id );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = @"
{% assign isInRole = CurrentPerson | IsInSecurityRole: GroupId %}
User is in Role = {{ isInRole }}
";
            const string outputExpected = "User is in Role = true";

            TestHelper.AssertTemplateOutput( outputExpected, template, options );
        }

        [TestMethod]
        public void IsInSecurityRole_WithNonSecurityGroupId_ReturnsFalse()
        {
            Guid relationShipsGroupGuid = Guid.Parse( "0F16BD3F-4775-4CD1-8F2F-DF576AEAD290" );
            var group = new GroupService( new RockContext() ).Queryable().FirstOrDefault( m => m.Guid == relationShipsGroupGuid );

            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            values.AddOrReplace( "GroupId", group.Id );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = @"
{% assign isInRole = CurrentPerson | IsInSecurityRole: GroupId %}
User is in Role = {{ isInRole }}
";
            const string outputExpected = "User is in Role = false";

            TestHelper.AssertTemplateOutput( outputExpected, template, options );
        }

        [TestMethod]
        public void IsInSecurityRole_WithInvalidGroup_ReturnsFalse()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            values.AddOrReplace( "GroupId", "-1" );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = @"
{% assign isInRole = CurrentPerson | IsInSecurityRole: GroupId %}
User is in Role = {{ isInRole }}
";
            const string outputExpected = "User is in Role = false";

            TestHelper.AssertTemplateOutput( outputExpected, template, options );
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
