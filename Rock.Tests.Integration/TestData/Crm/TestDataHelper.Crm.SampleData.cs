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
using Rock.Model;
using Rock.Tests.Integration.Modules.Reporting.DataFilter;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static partial class Crm
        {
            #region Group Requirements Test Data

            public static class Guids
            {
                public static string GroupTypeChildOrMemberOrAttenderOnlyGuid = "964BC51B-E8AA-4F87-86DD-7FB92A44C6B7";
                public static string GroupYouthWithRequirementsGuid = "23370B05-7A6A-4F37-959A-71120BF4E4A2";

                public static string GroupRequirementTypeMemberOrAttenderGuid = "54744E0D-A9CE-4108-83FF-18DD459994F2";
                public static string GroupRequirementTypeIsTeenGuid = "097CBDE9-CCE2-4CB8-810C-B4583C75C2EB";
                public static string GroupRequirementTypeHasInvalidSql = "BFDC283F-9E9C-4F10-882B-DB1346AB8C7D";
                public static string GroupRequirementTypeIsTeenagerGuid = "6BA17A04-4C9B-470C-8A72-2B8050718274";
                public static string GroupRequirementTypeBackgroundCheckGuid = "1c21c346-a861-4a9a-bd6d-baa7d92419d5";

                public static string DataViewIsAdultGuid = "F38133D1-7EA2-4DB5-821F-D39470D6073E";

                public static string CategoryGroupRequirementsGuid = "e62709e3-0060-4778-aa34-4b0fd9f6df2e";
            }

            /// <summary>
            /// Creates test data for Group Requirements.
            /// </summary>
            [TestMethod]
            public static void AddSampleDataForGroupRequirements()
            {
                // Add the sample data.
                TestHelper.Log( $"Creating sample data for Group Requirements feature..." );

                var rockContext = new RockContext();

                AddSampleDataViews();

                // Add Requirements
                AddSampleDataGroupRequirementTypes();

                // Get a list of all people in the database.
                var personService = new PersonService( rockContext );
                var allPeopleIdList = personService.Queryable().Select( p => p.Id.ToString() ).ToList();

                var testPeopleIdList = new List<int>();

                //
                // Add Test Groups
                // 

                // Youth Group
                // Group Type Requirement: Is Child/Member/Attender.
                CreateTestGroup( "Requirements Test - Youth Group",
                    Guids.GroupYouthWithRequirementsGuid,
                    Guids.GroupTypeChildOrMemberOrAttenderOnlyGuid,
                    allPeopleIdList.GetRandomizedList( 1000 ) );

                // Add Members
                var addMemberArgs = new AddGroupMemberArgs();
                addMemberArgs.GroupIdentifier = Guids.GroupYouthWithRequirementsGuid;
                addMemberArgs.ReplaceIfExists = true;
                addMemberArgs.ForeignKey = "RequirementsTest";

                // Add Leaders:
                // Bill Marble (Expiring Background Check)
                // Ted Decker (Current Background Check)
                // Cindy Decker (No Background Check)
                // Noah Decker (No Background Check, Child)
                addMemberArgs.GroupRoleIdentifier = "Leader";
                addMemberArgs.PersonIdentifiers = $"{TestGuids.TestPeople.BillMarble},{TestGuids.TestPeople.TedDecker},{TestGuids.TestPeople.CindyDecker},{TestGuids.TestPeople.NoahDecker}";
                AddGroupMembers( rockContext, addMemberArgs );

                // Add Members:
                // Alex Decker (Valid Group Member, Child),
                // Brian Jones (Valid Group Member, Child)
                // Craig Lowe (Invalid Group Member, Adult Visitor).
                addMemberArgs.GroupRoleIdentifier = "Member";
                addMemberArgs.PersonIdentifiers = $"{TestGuids.TestPeople.AlexDecker},{TestGuids.TestPeople.BrianJones},{TestGuids.TestPeople.CraigLowe}";
                AddGroupMembers( rockContext, addMemberArgs );

                // Group Requirement 1: Adult Leaders of the Group must satisfy a Background Check.
                // Possible Status: Meets, MeetsWithWarning, NotMet, NotApplicable
                CreateTestGroupRequirement( Guids.GroupYouthWithRequirementsGuid,
                    null,
                    TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                    "Leader",
                    ageClassification: AppliesToAgeClassification.Adults );

                // Group Requirement 2: Group Members older than 19 should be Members or Attenders.
                // Possible Status: Meets, DoesNotMeet
                CreateTestGroupRequirement( Guids.GroupYouthWithRequirementsGuid,
                    Guids.DataViewIsAdultGuid,
                    Guids.GroupRequirementTypeMemberOrAttenderGuid );

                // Requirement 3: Invalid SQL.
                // Possible Status: Error
                CreateTestGroupRequirement( Guids.GroupYouthWithRequirementsGuid,
                     null,
                     Guids.GroupRequirementTypeHasInvalidSql );

                // Set Background Check for Ted Decker: Valid
                TestDataHelper.Crm.SetPersonBackgroundCheck( TestGuids.TestPeople.TedDecker,
                    RockDateTime.Now.AddMonths( -6 ),
                    true );

                // Set Background Check for Bill Marble: Expiring - Validation Date is within 60 days of the 3 year expiry period.
                TestDataHelper.Crm.SetPersonBackgroundCheck( TestGuids.TestPeople.BillMarble,
                    RockDateTime.Now.AddDays( -1050 ),
                    true );

                TestDataHelper.Crm.SetPersonConnectionStatus( TestGuids.TestPeople.MaddieLowe,
                    "Visitor" );

                TestHelper.Log( "Sample data created." );
            }

            private static void AddSampleDataViews()
            {
                // Create a Data View that returns people with Age >= 20.
                var dataViewArgs = new TestDataHelper.Reporting.CreateDataViewArgs
                {
                    Name = "Adults",
                    Description = "People aged 20 or older.",
                    AppliesToEntityTypeIdentifier = typeof( Rock.Model.Person ).FullName,
                    Guid = Guids.DataViewIsAdultGuid.AsGuid(),
                    ExistingItemStrategy = Integration.TestData.CreateExistingItemStrategySpecifier.Replace,
                    CategoryIdentifier = Guids.CategoryGroupRequirementsGuid
                };

                TestDataHelper.Reporting.CreateDataView( dataViewArgs );

                var ageFilterSettings = new NumericPropertyFilterSettings
                {
                    Comparison = ComparisonType.GreaterThanOrEqualTo,
                    LowerValue = 20
                };

                var ageFilterFactory = new AgeDataFilterFactory();

                var filterArgs = new TestDataHelper.Reporting.AddDataViewComponentFilterArgs
                {
                    DataViewIdentifier = Guids.DataViewIsAdultGuid,
                    NodeType = FilterExpressionType.Filter,
                    FilterTypeIdentifier = typeof( Rock.Reporting.DataFilter.Person.AgeFilter ).FullName,
                    FilterSettings = ageFilterFactory.GetSettingsString( ageFilterSettings )
                };

                var rockContext = new RockContext();

                TestDataHelper.Reporting.AddDataViewComponentFilter( filterArgs, rockContext );
                rockContext.SaveChanges();
            }

            private static void AddSampleDataGroupRequirementTypes()
            {
                // Create Group Requirement Types.
                AddGroupRequirementTypeArgs args;
                AddGroupRequirementTypeActionResult result;

                // Requirement: Is Member or Attender
                args = new AddGroupRequirementTypeArgs
                {
                    GroupRequirementTypeGuid = Guids.GroupRequirementTypeMemberOrAttenderGuid,
                    Name = "Is Member or Attender",
                    CheckType = RequirementCheckType.Dataview,
                    PassFilterValue = TestGuids.DataViews.AdultMembersAndAttendees
                };

                result = AddGroupRequirementType( args );

                // Requirement 5: Invalid SQL
                args = new AddGroupRequirementTypeArgs
                {
                    GroupRequirementTypeGuid = Guids.GroupRequirementTypeHasInvalidSql,
                    Name = "Invalid SQL",
                    CheckType = RequirementCheckType.Sql,
                    PassFilterValue = "SELECT 1 FROM [Invalid_Table_Name]",
                    WarningFilterValue = "SELECT 2 FROM [Invalid_Table_Name]",
                };

                result = AddGroupRequirementType( args );

                // Create a Group Type with a "Child/Member/Attender" requirement.
                // Members in groups of this type must be a Member or Attender if they are classifed as an Adult.
                var rockContext = new RockContext();
                var groupTypeService = new GroupTypeService( rockContext );
                var groupType = groupTypeService.Get( Guids.GroupTypeChildOrMemberOrAttenderOnlyGuid.AsGuid() );

                if ( groupType == null )
                {
                    groupType = new GroupType();
                    groupTypeService.Add( groupType );
                }

                groupType.Guid = Guids.GroupTypeChildOrMemberOrAttenderOnlyGuid.AsGuid();
                groupType.Name = "Children/Members/Attenders Only";
                groupType.AllowAnyChildGroupType = true;
                groupType.AllowGroupSync = true;
                groupType.EnableSpecificGroupRequirements = true;

                var roleNameList = new List<string> { "Leader", "Member", "Guest" };
                foreach ( var roleName in roleNameList )
                {
                    if ( !groupType.Roles.Any( r => r.Name == roleName ) )
                    {
                        groupType.Roles.Add( new GroupTypeRole { Name = roleName } );
                    }
                }

                rockContext.SaveChanges();

                var addGroupRequirementArgs = new AddGroupRequirementToGroupTypeArgs
                {
                    GroupRequirementGuid = "DD85B849-645D-4E64-9C58-4B217B34B62C".AsGuid(),
                    ReplaceIfExists = true,
                    GroupTypeIdentifier = Guids.GroupTypeChildOrMemberOrAttenderOnlyGuid,
                    ForeignKey = "Test Data",
                    AppliesToAgeClassification = AppliesToAgeClassification.Adults,
                    GroupRequirementTypeIdentifier = Guids.GroupRequirementTypeMemberOrAttenderGuid
                };

                AddGroupTypeRequirement( rockContext, addGroupRequirementArgs );

                rockContext.SaveChanges();
            }

            private static void CreateTestGroup( string name, string guid, string groupTypeIdentifier, List<string> personIdList )
            {
                var rockContext = new RockContext();

                // Create a new Group.
                var addGroupArgs = new TestDataHelper.Crm.AddGroupArgs
                {
                    ReplaceIfExists = true,
                    GroupGuid = guid,
                    GroupName = name,
                    ForeignKey = "Test Data",
                    GroupTypeIdentifier = groupTypeIdentifier
                };

                var result = TestDataHelper.Crm.AddGroup( addGroupArgs );
                if ( result.AffectedItemCount == 0 )
                {
                    return;
                }

                // Add Group Members
                var addPeopleArgs = new TestDataHelper.Crm.AddGroupMemberArgs
                {
                    GroupIdentifier = guid,
                    GroupRoleIdentifier = "Member",
                    ForeignKey = "IntegrationTest",
                    PersonIdentifiers = personIdList.AsDelimited( "," )
                };

                var groupMembers = TestDataHelper.Crm.AddGroupMembers( rockContext, addPeopleArgs );
                rockContext.SaveChanges( disablePrePostProcessing: true );

                TestHelper.Log( $"Added Group \"{name}\" ({groupMembers.Count} members)." );
            }

            private static void CreateTestGroupRequirement( string groupIdentifier, string appliesToDataViewIdentifier, string groupRequirementTypeIdentifier, string groupRoleIdentifier = null, AppliesToAgeClassification ageClassification = AppliesToAgeClassification.All )
            {
                var addGroupRequirementArgs = new TestDataHelper.Crm.AddGroupRequirementToGroupArgs
                {
                    ReplaceIfExists = true,
                    GroupIdentifier = groupIdentifier,
                    ForeignKey = "Test Data",
                    GroupRequirementTypeIdentifier = groupRequirementTypeIdentifier,
                    GroupRoleIdentifier = groupRoleIdentifier,
                    AppliesToDataViewIdentifier = appliesToDataViewIdentifier,
                    AppliesToAgeClassification = ageClassification
                };

                var rockContext = new RockContext();
                var requirement = TestDataHelper.Crm.AddGroupRequirement( rockContext, addGroupRequirementArgs );
                rockContext.SaveChanges( disablePrePostProcessing: true );

                if ( string.IsNullOrWhiteSpace( groupRoleIdentifier ) )
                {
                    groupRoleIdentifier = "(any)";
                }

                TestHelper.Log( $"Added Group Requirement. [RequirementId={requirement.Id}, Group={groupIdentifier}, Role={ groupRoleIdentifier}]" );
            }

            #endregion
        }
    }
}
