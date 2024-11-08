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

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Organization;
using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Core.Crm
{
    /// <summary>
    /// A collection of tests to identify potential issues that arise when configuring a Rock database
    /// to include Person search results for names with accented characters.
    /// </summary>
    [TestClass]
    [TestCategory( "Core.Crm.Search.Person" )]
    public partial class AccentedNameSearchTests : DatabaseTestsBase
    {
        [ClassInitialize]
        public static void TestInitialize( TestContext context )
        {
            ModifyDatabaseCollation();

            AddTestCampusRecords();
            AddTestPersonRecords();
        }

        [TestMethod]
        public void FamilySalutation_ForTwoParentsTwoChildren_ReturnsCorrectFormat()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var deckerFamilyGroup = groupService.GetByParentGroupIdAndName( null, "Decker Family" ).FirstOrDefault();

            var salutation = GroupService.CalculateFamilySalutation( deckerFamilyGroup, new Person.CalculateFamilySalutationArgs( true ) );

            Assert.That.AreEqual( "Ted, Cindy, Noah & Alex Decker", salutation );
        }

        [TestMethod]
        public void FamilySalutation_ForPeopleWithAccentedNames_ReturnsCorrectFormat()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var familyGroup = groupService.GetByParentGroupIdAndName( null, "Côté Family" ).FirstOrDefault();

            var salutation = GroupService.CalculateFamilySalutation( familyGroup, new Person.CalculateFamilySalutationArgs( true ) );

            Assert.That.AreEqual( "René & Joséphine Côté", salutation );
        }

        [TestMethod]
        public void CalculatePersonDuplicates_WithNamesDifferingOnlyByAccents_AreDetectedAsDuplicates()
        {
            var rows = DbService.ExecuteCommand( "EXEC [dbo].[spCrm_PersonDuplicateFinder]",
                System.Data.CommandType.Text,
                null );

            var rockContext = new RockContext();

            var duplicateService = new PersonDuplicateService( rockContext );

            var personGuid = _PersonGuidAccentedName.AsGuid();
            var personDuplicates = duplicateService.Queryable()
                    .Where( a => a.PersonAlias.Person.Guid == personGuid )
                    .ToList();

            Assert.That.AreEqual( 1, personDuplicates.Count, "Duplicate entry expected but not found." );
        }

        [TestMethod]
        public void ProcessBiAnalyticsJob_WithAccentedNames_CompletesWithNoErrors()
        {
            var jobAnalytics = new Rock.Jobs.ProcessBIAnalytics();

            var analyticsSettings = new Dictionary<string, string>();
            analyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessPersonBIAnalytics, "true" );

            jobAnalytics.ExecuteInternal( analyticsSettings );

            Assert.That.Contains( jobAnalytics.Result, "Person BI Results:\r\n-- Inserted" );
        }

        [TestMethod]
        public void PersonSearch_ForNameContainingAccentedVariants_ReturnsAllVariants()
        {
            var personSearchOptions = new PersonService.PersonSearchOptions
            {
                Name = "Gagne, Jean"
            };

            var results = Search( personSearchOptions );

            Assert.That.AreEqual( 2, results.Count, "Search results expected but not found." );
        }

        private List<Person> Search( PersonService.PersonSearchOptions options )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var personSearchQry = personService.Search( options );
            var results = personSearchQry.ToList();

            return results;
        }

        [TestMethod]
        public void SmartSearchPerson_ForNameContainingAccentedVariants_ReturnsAllVariants()
        {
            var results = SmartSearch( "Gagne" );

            Assert.That.AreEqual( 2, results.Count, "Search results expected but not found." );
        }

        private List<string> SmartSearch( string name )
        {
            // Try the look-ahead search called from the SmartSearch control.
            var nameSearchComponent = new Rock.Search.Person.Name();

            var matchNames = nameSearchComponent.Search( name )
                .ToList();

            return matchNames;
        }

        private static void ModifyDatabaseCollation()
        {
            var sql = @"
-- Person Table
DROP INDEX IF EXISTS [IX_IsDeceased_FirstName_LastName] ON [Person]
DROP INDEX IF EXISTS [IX_IsDeceased_LastName_FirstName] ON [Person]
ALTER TABLE [Person]
ALTER COLUMN [FirstName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [Person]
ALTER COLUMN [NickName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [Person]
ALTER COLUMN [LastName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [Person]
ALTER COLUMN [MiddleName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
CREATE NONCLUSTERED INDEX [IX_IsDeceased_FirstName_LastName] ON [dbo].[Person]
(
[IsDeceased] ASC,
[FirstName] ASC,
[LastName] ASC
) ON [PRIMARY]
-- PersonPreviousName Table
DROP INDEX IF EXISTS [IX_LastName] ON [PersonPreviousName]
ALTER TABLE [PersonPreviousName]
ALTER COLUMN [LastName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
CREATE NONCLUSTERED INDEX [IX_LastName] ON [dbo].[PersonPreviousName]
(
[LastName] ASC
) ON [PRIMARY]
-- AnalyticsSourcePersonHistorical Table
ALTER TABLE [AnalyticsSourcePersonHistorical]
ALTER COLUMN [FirstName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [AnalyticsSourcePersonHistorical]
ALTER COLUMN [NickName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [AnalyticsSourcePersonHistorical]
ALTER COLUMN [LastName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
ALTER TABLE [AnalyticsSourcePersonHistorical]
ALTER COLUMN [MiddleName] NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AI
";

            var result = DbService.ExecuteCommand( sql );

            Assert.That.AreEqual( -1, result, "Modify name column indexes failed." );
        }

        private const string _PersonGuidUnaccentedName = "FEB54A3B-9E21-48B1-8BE9-BB8E1F96306C";
        private const string _PersonGuidAccentedName = "9CDA4F50-A821-40F6-8B9C-923EE51F68CA";
        private const string _PersonGuidJeanPhilippeCote = "91B783F0-9DE5-4D30-95C0-DBC74F3F1AD9";
        private const string _PersonGuidJosephineCote = "CD8B1A16-59D2-41AD-BE49-2AE2CF6D129A";

        private static void AddTestCampusRecords()
        {
            // Define multiple campuses, because this causes the Person search function to behave differently.
            var manager = CampusDataManager.Instance;
            manager.AddCampusTestDataSet();
        }

        private static void AddTestPersonRecords()
        {
            // Add new Person records with are identical, differing only by names with accented characters.
            var manager = PersonDataManager.Instance;
            var argsAddPerson = new PersonDataManager.AddNewPersonActionArgs
            {
                ReplaceIfExists = true,
                PersonInfo = new PersonDataManager.PersonInfo
                {
                    Email = "jean.gagne@email.com",
                    DateOfBirth = "2000-06-01",
                    GenderIdentifier = "Male",
                    RecordStatusIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
                    RecordTypeIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON
                }
            };

            // Add Person 1, including a previous name that is identical but has accented characters.
            var personInfo = argsAddPerson.PersonInfo;

            personInfo.PersonGuid = _PersonGuidUnaccentedName;
            personInfo.FirstName = "Jean Philippe";
            personInfo.LastName = "Gagne";
            manager.AddNewPersonToNewFamily( argsAddPerson );

            var argsAddPreviousName = new PersonDataManager.UpdatePersonAddPreviousNameActionArgs
            {
                UpdateTargetIdentifier = _PersonGuidUnaccentedName,
                Properties = new PersonDataManager.PersonPreviousNameInfo { PreviousLastName = "Gagné" }
            };
            manager.UpdatePersonAddPreviousName( argsAddPreviousName );

            // Add Person 2, a duplicate of person but having the previous accented Last Name.
            personInfo.PersonGuid = _PersonGuidAccentedName;
            personInfo.FirstName = "Jéan";
            personInfo.LastName = "Gagné";
            manager.AddNewPersonToNewFamily( argsAddPerson );

            // Add new Person records for a family where some first names have accented characters.
            var addArgs2 = new PersonDataManager.AddNewPersonActionArgs
            {
                ReplaceIfExists = true,
                PersonInfo = new PersonDataManager.PersonInfo
                {
                    RecordStatusIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
                    RecordTypeIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON
                }
            };

            personInfo = addArgs2.PersonInfo;

            personInfo.PersonGuid = _PersonGuidJeanPhilippeCote;
            personInfo.FirstName = "René";
            personInfo.LastName = "Côté";

            var result = manager.AddNewPersonToNewFamily( addArgs2 );
            var familyId = result.FamilyGroupId;

            var addMember2 = new PersonDataManager.AddNewPersonToFamilyActionArgs
            {
                PersonInfo = new PersonDataManager.PersonInfo
                {
                    PersonGuid = _PersonGuidJosephineCote,
                    FirstName = "Joséphine",
                    LastName = "Côté",
                    PrimaryFamilyGroupIdentifier = familyId.ToString(),
                    RecordStatusIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
                    RecordTypeIdentifier = SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON
                },
                FamilyIdentifier = familyId.ToString(),

            };

            manager.AddNewPersonToExistingFamily( addMember2 );
        }
    }
}
