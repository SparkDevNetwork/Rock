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
using Rock.Jobs;
using Rock.Logging;
using Rock.Model;
using Rock.Tests.Integration;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Performance.Modules.Crm.Groups
{
    /// <summary>
    /// Tests that verify the operation of Group Requirements.
    /// </summary>
    [TestClass]
    public class GroupRequirementsPerformanceTests : DatabaseTestsBase
    {
        /// <summary>
        /// Verifies the performance of the CalculateGroupRequirements job.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_DataViewCachingEnabled()
        {
            CalculateGroupRequirementsJob_MeasurePerformance( dataViewCacheIsDisabled: false );
        }

        /// <summary>
        /// Verifies the performance of the CalculateGroupRequirements job.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_DataViewCachingDisabled()
        {
            CalculateGroupRequirementsJob_MeasurePerformance( dataViewCacheIsDisabled: true );
        }

        private void CalculateGroupRequirementsJob_MeasurePerformance( bool dataViewCacheIsDisabled )
        {
            // Remove all existing Group Requirement results.
            DbService.ExecuteCommand( "DELETE FROM [GroupMemberRequirement]" );

            var rockContext = new RockContext();
            var groupRequirementTypeService = new GroupRequirementTypeService( rockContext );

            var requirementAdultAttenderId = groupRequirementTypeService.GetByIdentifierOrThrow( _GroupRequirementTypeAdultAttenderGuid ).Id;
            var requirementAdultMaleId = groupRequirementTypeService.GetByIdentifierOrThrow( _GroupRequirementTypeAdultMaleGuid ).Id;
            var requirementAdultFemaleId = groupRequirementTypeService.GetByIdentifierOrThrow( _GroupRequirementTypeAdultFemaleGuid ).Id;

            string output = null;
            TestHelper.ExecuteWithTimer( "Calculate Group Requirements", () =>
            {
                var logger = new RockLoggerMemoryBuffer();
                logger.EventLogged += ( s, e ) =>
                {
                    TestHelper.Log( $"<{e.Event.Domain}> {e.Event.Message}" );
                };

                var job = new CalculateGroupRequirements();
                job.Logger = logger;

                var args = new CalculateGroupRequirements.CalculateGroupRequirementsJobArgs
                {
                    GroupRequirementTypeIdList = new List<int>
                    {
                        requirementAdultAttenderId, requirementAdultMaleId, requirementAdultFemaleId
                    },
                    DisableDataViewCache = dataViewCacheIsDisabled

                };
                job.Execute( args );

                output = job.Result;
                TestHelper.Log( output );
            } );
        }

        #region Test Data

        private string _RequirementsGroup1Guid = "23370B05-7A6A-4F37-959A-71120BF4E4A2";
        private string _RequirementsGroup2Guid = "179DC394-C546-4521-A7BD-F607F8B11870";
        private string _RequirementsGroup3Guid = "0D9F711F-467F-4649-812B-3428BF073B5C";
        private string _RequirementsGroup4Guid = "025CE685-0EC0-4E69-8CA5-E9FCBACD5417";

        private string _GroupRequirementTypeAdultAttenderGuid = "54744E0D-A9CE-4108-83FF-18DD459994F2";
        private string _GroupRequirementTypeAdultMaleGuid = "097CBDE9-CCE2-4CB8-810C-B4583C75C2EB";
        private string _GroupRequirementTypeAdultFemaleGuid = "BFDC283F-9E9C-4F10-882B-DB1346AB8C7D";

        private string _GroupTypeAttendersOnlyGuid = "964BC51B-E8AA-4F87-86DD-7FB92A44C6B7";


        /// <summary>
        /// Creates test data for Group Requirements.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_CreateTestData()
        {
            // Add the sample data.
            TestHelper.Log( $"Creating sample data for CalculateGroupRequirements Job..." );

            var rockContext = new RockContext();

            // Add Requirements
            AddGroupRequirementTypes();

            // Get a list of all people in the database.
            var personService = new PersonService( rockContext );
            var allPeopleIdList = personService.Queryable().Select( p => p.Id.ToString() ).ToList();

            // Add Test Groups
            CreateTestGroup( $"Requirements Test 1 - Adult Males 1", _RequirementsGroup1Guid, _GroupTypeAttendersOnlyGuid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Test 2 - Adult Females 1", _RequirementsGroup2Guid, _GroupTypeAttendersOnlyGuid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Test 3 - Adult Males 2", _RequirementsGroup3Guid, _GroupTypeAttendersOnlyGuid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Test 4 - Adult Females 2", _RequirementsGroup4Guid, _GroupTypeAttendersOnlyGuid, allPeopleIdList.GetRandomizedList( 1000 ) );

            // Add requirements to Test Groups.
            // Each Group also inherits the requirement "Attenders only" from the Group Type.
            CreateTestGroupRequirement( _RequirementsGroup1Guid, TestGuids.DataViews.AdultMembersAndAttendees, _GroupRequirementTypeAdultMaleGuid );
            CreateTestGroupRequirement( _RequirementsGroup2Guid, TestGuids.DataViews.AdultMembersAndAttendees, _GroupRequirementTypeAdultFemaleGuid );
            CreateTestGroupRequirement( _RequirementsGroup3Guid, TestGuids.DataViews.AdultMembersAndAttendees, _GroupRequirementTypeAdultMaleGuid );
            CreateTestGroupRequirement( _RequirementsGroup4Guid, TestGuids.DataViews.AdultMembersAndAttendees, _GroupRequirementTypeAdultFemaleGuid );

            TestHelper.Log( "Sample data created." );
        }

        private void AddGroupRequirementTypes()
        {
            // Create Group Requirement Types.
            TestDataHelper.Crm.AddGroupRequirementTypeArgs args;
            TestDataHelper.Crm.AddGroupRequirementTypeActionResult result;

            args = new TestDataHelper.Crm.AddGroupRequirementTypeArgs
            {
                GroupRequirementTypeGuid = _GroupRequirementTypeAdultAttenderGuid,
                Name = "Is Adult Attender",
                CheckType = RequirementCheckType.Dataview,
                PassFilterValue = TestGuids.DataViews.AdultMembersAndAttendees
            };

            result = TestDataHelper.Crm.AddGroupRequirementType( args );

            args = new TestDataHelper.Crm.AddGroupRequirementTypeArgs
            {
                GroupRequirementTypeGuid = _GroupRequirementTypeAdultMaleGuid,
                Name = "Is Adult Male Attender",
                CheckType = RequirementCheckType.Dataview,
                PassFilterValue = TestGuids.DataViews.AdultMembersAndAttendeesMales,
                WarningFilterValue = TestGuids.DataViews.AdultMembersAndAttendeesFemales,
            };

            result = TestDataHelper.Crm.AddGroupRequirementType( args );

            args = new TestDataHelper.Crm.AddGroupRequirementTypeArgs
            {
                GroupRequirementTypeGuid = _GroupRequirementTypeAdultFemaleGuid,
                Name = "Is Adult Female Attender",
                CheckType = RequirementCheckType.Dataview,
                PassFilterValue = TestGuids.DataViews.AdultMembersAndAttendeesFemales,
                WarningFilterValue = TestGuids.DataViews.AdultMembersAndAttendeesMales,
            };

            result = TestDataHelper.Crm.AddGroupRequirementType( args );

            // Create a Group Type with the "Is Adult Attender" requirement.
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );
            var groupType = groupTypeService.Get( _GroupTypeAttendersOnlyGuid.AsGuid() );

            if ( groupType == null )
            {
                groupType = new GroupType();
                groupTypeService.Add( groupType );
            }

            groupType.Guid = _GroupTypeAttendersOnlyGuid.AsGuid();
            groupType.Name = "Attenders Only";
            groupType.AllowAnyChildGroupType = true;
            groupType.AllowGroupSync = true;
            groupType.EnableSpecificGroupRequirements = true;

            if ( !groupType.Roles.Any( r => r.Name == "Member" ) )
            {
                groupType.Roles.Add( new GroupTypeRole { Name = "Member" } );
            }

            rockContext.SaveChanges();

            var addGroupRequirementArgs = new TestDataHelper.Crm.AddGroupRequirementToGroupTypeArgs
            {
                GroupRequirementGuid = "DD85B849-645D-4E64-9C58-4B217B34B62C".AsGuid(),
                ReplaceIfExists = true,
                GroupTypeIdentifier = _GroupTypeAttendersOnlyGuid,
                ForeignKey = "Test Data",
                AppliesToDataViewIdentifier = TestGuids.DataViews.AdultMembersAndAttendees,
                GroupRequirementTypeIdentifier = _GroupRequirementTypeAdultAttenderGuid
            };

            TestDataHelper.Crm.AddGroupTypeRequirement( rockContext, addGroupRequirementArgs );

            rockContext.SaveChanges();
        }

        private void CreateTestGroup( string name, string guid, string groupTypeIdentifier, List<string> personIdList )
        {
            var rockContext = new RockContext();

            // Create a new Group.
            var addGroupArgs = new TestDataHelper.Crm.AddGroupArgs
            {
                ReplaceIfExists = true, // false,
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

        private void CreateTestGroupRequirement( string groupIdentifier, string appliesToDataViewIdentifier, string groupRequirementTypeIdentifier )
        {
            var rockContext = new RockContext();

            // Add a Group Requirement.
            var addGroupRequirementArgs = new TestDataHelper.Crm.AddGroupRequirementToGroupArgs
            {
                ReplaceIfExists = true,
                GroupIdentifier = groupIdentifier,
                ForeignKey = "Test Data",
                AppliesToDataViewIdentifier = appliesToDataViewIdentifier,
                GroupRequirementTypeIdentifier = groupRequirementTypeIdentifier
            };

            var requirement = TestDataHelper.Crm.AddGroupRequirement( rockContext, addGroupRequirementArgs );
            rockContext.SaveChanges( disablePrePostProcessing: true );

            TestHelper.Log( $"Added Requirement #{requirement.Id} to Group \"{groupIdentifier}\"." );
        }

        #endregion
    }
}
