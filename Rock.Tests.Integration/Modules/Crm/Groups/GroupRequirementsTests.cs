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
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Crm.Groups
{
    /// <summary>
    /// Tests that verify the operation of Group Requirements.
    /// </summary>
    [TestClass]
    public class GroupRequirementsTests : DatabaseTestsBase
    {
        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            InitializeData();
        }

        private static void InitializeData()
        {
            TestDataHelper.Crm.AddSampleDataForGroupRequirements();
        }

        #region Tests

        /// <summary>
        /// If a Group Member does not satisy a requirement specified at the Group Type level, 
        /// the requirement is flagged as "Not Applicable".
        /// </summary>
        [TestMethod]
        public void GroupRequirement_GroupTypeRequirementFails_SetsFailStatus()
        {
            // Craig Lowe is a Visitor.
            // He should fail the Group Type requirement "Children/Members/Attenders".
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.CraigLowe,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                null,
                TestDataHelper.Crm.Guids.GroupRequirementTypeMemberOrAttenderGuid,
                MeetsGroupRequirement.NotMet );
        }

        /// <summary>
        /// If a Group Member does not fall within the "Applies To" Data View for a Group to which this Requirement is attached,
        /// the requirement is flagged as "Not Applicable".
        /// </summary>
        [TestMethod]
        public void GroupRequirement_AppliesToDataViewDoesNotMatch_SetsNotApplicableStatus()
        {
            // Maddie Lowe is a Visitor. (Connection Status modified in setup for Group Requirement tests).
            // She should show "Not applicable" for the group requirement "Member or Attender",
            // because she is a child and does not meet the "Applies To" filter for "Is Adult".
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.MaddieLowe,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                groupRoleIdentifier: null,
                TestDataHelper.Crm.Guids.GroupRequirementTypeMemberOrAttenderGuid,
                MeetsGroupRequirement.NotApplicable );
        }

        /// <summary>
        /// If a Group Member does not fall within the "Applies To" Data View for a Group to which this Requirement is attached,
        /// the requirement is flagged as "Not Applicable".
        /// </summary>
        [TestMethod]
        public void GroupRequirement_AppliesToDataViewMatchesButRequirementFails_SetsFailStatus()
        {
            // Craig Lowe is a Visitor, aged 41.
            // He should show "Not Met" for the group requirement "Member or Attender",
            // because he passes the "Applies To" filter for "Is Adult" and therefore must satisfy the requirement.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.CraigLowe,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                groupRoleIdentifier: null,
                TestDataHelper.Crm.Guids.GroupRequirementTypeMemberOrAttenderGuid,
                MeetsGroupRequirement.NotMet );
        }

        /// <summary>
        /// If a Group Member meets a Group Requirement, the requirement is flagged as passed.
        /// </summary>
        [TestMethod]
        public void GroupRequirement_RoleDoesNotMatch_SetsNotApplicableStatus()
        {
            // Alex Decker is a Member of the Group, therefore the Background Check requirement
            // applied to the Leader role is not applicable.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.AlexDecker,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                "Member",
                TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                MeetsGroupRequirement.NotApplicable );
        }

        /// <summary>
        /// If a Group Member meets a Group Requirement, the requirement is flagged as passed.
        /// </summary>
        [TestMethod]
        public void GroupRequirement_QualifyingDataViewPasses_SetsPassStatus()
        {
            // Ted Decker is a Leader, therefore the Background Check requirement
            // applied to the Leader role must be met.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.TedDecker,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                "Leader",
                TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                MeetsGroupRequirement.Meets );
        }

        /// <summary>
        /// If a Group Member meets a Group Requirement, the requirement is flagged as passed.
        /// </summary>
        [TestMethod]
        public void GroupRequirement_QualifyingDataViewFails_SetsFailStatus()
        {
            // Cindy Decker is a Leader, but has not passed the mandatory Background Check.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.CindyDecker,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                "Leader",
                TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                MeetsGroupRequirement.NotMet );
        }

        /// <summary>
        /// If a Group Member meets a Group Requirement but is also included in the Warning Data View,
        /// the requirement is flagged as "Meets With Warning".
        /// </summary>
        [TestMethod]
        [Ignore( "Data seems correct, but dataview SQL for warning dataview seems wrong. Does not match results on pre-alpha, 2024-01-29 DSH." )]
        public void GroupRequirement_WarningDataViewMatchesPersonPassingRequirement_SetsWarnStatus()
        {
            // Bill Marble is a Leader with a completed Background Check, so he satisfies the Backgrouind Check requirement.
            // However, the check has expired and therefore this requirement should be flagged as met with a warning.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.BillMarble,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                "Leader",
                TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                MeetsGroupRequirement.MeetsWithWarning );
        }

        /// <summary>
        /// If a Group Member does not fall within the "Applies To" Data View for a Group to which this Requirement is attached,
        /// the requirement is flagged as "Not Applicable".
        /// </summary>
        [TestMethod]
        public void GroupRequirement_AgeClassificationDoesNotMatch_SetsNotApplicableStatus()
        {
            // Noah Decker is a junior Leader of the Group, but the Background Check requirement for leaders is not applicable
            // because he is not an adult.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.NoahDecker,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                null,
                TestDataHelper.Crm.Guids.GroupRequirementTypeBackgroundCheckGuid,
                MeetsGroupRequirement.NotApplicable );
        }

        /// <summary>
        /// If a Group Requirement has an invalid SQL condition, the requirement is flagged as an error.
        /// </summary>
        [TestMethod]
        public void GroupRequirement_SqlIsInvalid_SetsErrorStatus()
        {
            // This Group Requirement has an intentionally invalid SQL statement.
            GroupRequirement_AssertPersonGroupRequirementStatus( TestGuids.TestPeople.TedDecker,
                TestDataHelper.Crm.Guids.GroupYouthWithRequirementsGuid,
                null,
                TestDataHelper.Crm.Guids.GroupRequirementTypeHasInvalidSql,
                MeetsGroupRequirement.Error );
        }

        /// <summary>
        /// Asserts that a person has a known status for a requirement of a group.
        /// </summary>
        /// <param name="personIdentifier"></param>
        /// <param name="groupIdentifier"></param>
        /// <param name="groupRoleIdentifier"></param>
        /// <param name="groupRequirementTypeIdentifier"></param>
        /// <param name="expectedRequirementStatus"></param>
        private void GroupRequirement_AssertPersonGroupRequirementStatus( string personIdentifier, string groupIdentifier, string groupRoleIdentifier, string groupRequirementTypeIdentifier, MeetsGroupRequirement expectedRequirementStatus )
        {
            var rockContext = new RockContext();
            var group = TestDataHelper.Crm.GetEntityByIdentifierOrThrow<Group>( groupIdentifier, rockContext );

            int? roleId = null;
            if ( !string.IsNullOrWhiteSpace( groupRoleIdentifier ) )
            {
                roleId = TestDataHelper.Crm.GetEntityService<GroupTypeRole>( rockContext )
                    .Queryable()
                    .Where( r => r.GroupTypeId == group.GroupTypeId )
                    .GetByIdentifierOrThrow( groupRoleIdentifier )
                    .Id;
            }

            var requirementType = TestDataHelper.Crm.GetEntityByIdentifierOrThrow<GroupRequirementType>( groupRequirementTypeIdentifier, rockContext );

            List<PersonGroupRequirementStatus> requirementStatusList;
            PersonGroupRequirementStatus requirementStatus;

            // Verify requirements for Ted Decker:
            var person = TestDataHelper.Crm.GetEntityByIdentifierOrThrow<Person>( personIdentifier, rockContext );
            requirementStatusList = group.PersonMeetsGroupRequirements( rockContext, person.Id, roleId ).ToList();

            requirementStatus = requirementStatusList.FirstOrDefault( r => r.GroupRequirement.GroupRequirementTypeId == requirementType.Id );

            Assert.IsNotNull( requirementStatus, $"Requirement Status not found for Group Member. [Group={ group.Name }, Person={ person.FullName }, RequirementType={requirementType.Name}." );
            Assert.AreEqual( expectedRequirementStatus, requirementStatus.MeetsGroupRequirement );
        }

        #endregion

        /// <summary>
        /// Verifies that attempting to process an Interaction session having an empty Guid will ignore the invalid session and continue processing.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_WithDataViewCacheEnabled_CompletesSuccessfully()
        {
            CalculateGroupRequirementsJob_WithDataViewCache( true );
        }

        /// <summary>
        /// Verifies that attempting to process an Interaction session having an empty Guid will ignore the invalid session and continue processing.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_WithDataViewCacheDisabled_CompletesSuccessfully()
        {
            CalculateGroupRequirementsJob_WithDataViewCache( false );
        }

        private void CalculateGroupRequirementsJob_WithDataViewCache( bool enableDataViewCache )
        {
            var args = new CalculateGroupRequirements.CalculateGroupRequirementsJobArgs();
            args.DisableDataViewCache = !enableDataViewCache;

            TestHelper.StartTimer( $"Calculate Group Requirements [Caching={ enableDataViewCache }]" );
            var job = new CalculateGroupRequirements();
            job.Execute( args );
            var output = job.Result;
            TestHelper.EndTimer( "Calculate Group Requirements" );

            TestHelper.Log( output );
            Assert.That.Contains( output, "group requirements re-calculated" );
        }

    }
}
