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
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Crm.BulkUpdate;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Core;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.ViewModels.Blocks.Crm.BulkUpdate;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Crm
{
    /// <summary>
    /// Integration tests for <see cref="BulkUpdateProcessor"/>, the engine behind the Obsidian
    /// Bulk Update block.
    /// </summary>
    [TestClass]
    public class BulkUpdateProcessorTests : DatabaseTestsBase
    {
        #region Field Keys

        /// <summary>
        /// Client field keys for <see cref="BulkUpdateBag.UpdatedFields"/>
        /// </summary>
        private static class FieldKey
        {
            public const string Title = "title";
            public const string Suffix = "suffix";
            public const string Gender = "gender";
            public const string MaritalStatus = "maritalStatus";
            public const string GraduationYear = "graduationYear";
            public const string Campus = "campus";
            public const string ConnectionStatus = "connectionStatus";
            public const string RecordStatus = "recordStatus";
            public const string RecordSource = "recordSource";
            public const string CommunicationPreference = "communicationPreference";
            public const string IsEmailActive = "isEmailActive";
            public const string EmailPreference = "emailPreference";
            public const string EmailNote = "emailNote";
            public const string Following = "following";
            public const string ReviewReason = "reviewReason";
            public const string ReviewReasonNote = "reviewReasonNote";
            public const string SystemNote = "systemNote";
        }

        /// <summary>
        /// Toggle keys for <see cref="BulkUpdateGroupBag.UpdatedFields"/> (Group Update branch).
        /// </summary>
        private static class GroupFieldKey
        {
            public const string Role = "role";
            public const string MemberStatus = "memberStatus";
        }

        /// <summary>
        /// Toggle keys for <see cref="BulkUpdateStepBag.UpdatedFields"/> (Step Update branch).
        /// </summary>
        private static class StepFieldKey
        {
            public const string Status = "status";
            public const string StartDate = "startDate";
            public const string EndDate = "endDate";
            public const string Campus = "campus";
            public const string Note = "note";
        }

        #endregion Field Keys

        #region Person Fields (shared database, self-restoring)

        [TestMethod]
        public void PersonFields_Gender_IsUpdated()
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalGender = person.Gender;
            var targetGender = originalGender == Gender.Male ? Gender.Female : Gender.Male;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = targetGender
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetGender, ReloadPerson( person.Id ).Gender );

            RestorePerson( person.Id, p => p.Gender = originalGender );
        }

        [TestMethod]
        public void PersonFields_Gender_BlankSelectionClearsToUnknown()
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.BillMarble.AsGuid() );
            var originalGender = person.Gender;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = null
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( Gender.Unknown, ReloadPerson( person.Id ).Gender );

            RestorePerson( person.Id, p => p.Gender = originalGender );
        }

        [TestMethod]
        public void PersonFields_FieldNotToggled_IsLeftAlone()
        {
            const string targetEmailNote = "Toggle-isolation test note.";

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalGender = person.Gender;
            var originalEmailNote = person.EmailNote;
            var otherGender = originalGender == Gender.Male ? Gender.Female : Gender.Male;

            // emailNote is toggled ON (so the per-field loop actually runs); gender is present
            // in the bag but toggled OFF. The processor must apply emailNote and leave gender
            // untouched. Toggling only an off field would short-circuit ApplyPersonFields on
            // its !HasAny() guard, which would pass even if the per-field gate were broken.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = new Dictionary<string, bool> { [FieldKey.EmailNote] = true, [FieldKey.Gender] = false },
                EmailNote = targetEmailNote,
                Gender = otherGender
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            var reloaded = ReloadPerson( person.Id );
            Assert.AreEqual( targetEmailNote, reloaded.EmailNote, "The toggled-on field should be applied." );
            Assert.AreEqual( originalGender, reloaded.Gender, "A field whose toggle is false must not be changed." );

            RestorePerson( person.Id, p =>
            {
                p.EmailNote = originalEmailNote;
                p.Gender = originalGender;
            } );
        }

        [TestMethod]
        public void PersonFields_Title_IsUpdated()
        {
            var titleValue = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).DefinedValues.First();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalTitleValueId = person.TitleValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.Title ),
                TitleValueGuid = titleValue.Guid
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( titleValue.Id, ReloadPerson( person.Id ).TitleValueId );

            RestorePerson( person.Id, p => p.TitleValueId = originalTitleValueId );
        }

        [TestMethod]
        public void PersonFields_Suffix_IsUpdated()
        {
            var suffixValue = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).DefinedValues.First();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalSuffixValueId = person.SuffixValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.Suffix ),
                SuffixValueGuid = suffixValue.Guid
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( suffixValue.Id, ReloadPerson( person.Id ).SuffixValueId );

            RestorePerson( person.Id, p => p.SuffixValueId = originalSuffixValueId );
        }

        [TestMethod]
        public void PersonFields_MaritalStatus_IsUpdated()
        {
            var marriedId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.BenJones.AsGuid() );
            var originalMaritalStatusId = person.MaritalStatusValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.MaritalStatus ),
                MaritalStatusValueGuid = SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( marriedId, ReloadPerson( person.Id ).MaritalStatusValueId );

            RestorePerson( person.Id, p => p.MaritalStatusValueId = originalMaritalStatusId );
        }

        [TestMethod]
        public void PersonFields_GraduationYear_IsUpdated()
        {
            const int targetGraduationYear = 2099;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.BenJones.AsGuid() );
            var originalGraduationYear = person.GraduationYear;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.GraduationYear ),
                GraduationYear = targetGraduationYear
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetGraduationYear, ReloadPerson( person.Id ).GraduationYear );

            RestorePerson( person.Id, p => p.GraduationYear = originalGraduationYear );
        }

        [TestMethod]
        public void PersonFields_CommunicationPreference_IsUpdated()
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalPreference = person.CommunicationPreference;
            var targetPreference = originalPreference == CommunicationType.SMS ? CommunicationType.Email : CommunicationType.SMS;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.CommunicationPreference ),
                // The bag uses Rock.Enums.Communication.CommunicationType; same integer values
                // as the Person's Rock.Model.CommunicationType.
                CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) ( int ) targetPreference
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetPreference, ReloadPerson( person.Id ).CommunicationPreference );

            RestorePerson( person.Id, p => p.CommunicationPreference = originalPreference );
        }

        [TestMethod]
        public void PersonFields_EmailPreference_IsUpdated()
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalPreference = person.EmailPreference;
            var targetPreference = originalPreference == EmailPreference.DoNotEmail ? EmailPreference.EmailAllowed : EmailPreference.DoNotEmail;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.EmailPreference ),
                EmailPreference = targetPreference
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetPreference, ReloadPerson( person.Id ).EmailPreference );

            RestorePerson( person.Id, p => p.EmailPreference = originalPreference );
        }

        [TestMethod]
        public void PersonFields_IsEmailActive_IsUpdated()
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalIsEmailActive = person.IsEmailActive;
            var targetIsEmailActive = !originalIsEmailActive;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.IsEmailActive ),
                IsEmailActive = targetIsEmailActive
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetIsEmailActive, ReloadPerson( person.Id ).IsEmailActive );

            RestorePerson( person.Id, p => p.IsEmailActive = originalIsEmailActive );
        }

        [TestMethod]
        public void PersonFields_EmailNote_IsUpdated()
        {
            const string targetEmailNote = "Bulk update test email note.";

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalEmailNote = person.EmailNote;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.EmailNote ),
                EmailNote = targetEmailNote
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetEmailNote, ReloadPerson( person.Id ).EmailNote );

            RestorePerson( person.Id, p => p.EmailNote = originalEmailNote );
        }

        [TestMethod]
        public void PersonFields_SystemNote_IsUpdated()
        {
            const string targetSystemNote = "Bulk update test system note.";

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalSystemNote = person.SystemNote;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.SystemNote ),
                SystemNote = targetSystemNote
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( targetSystemNote, ReloadPerson( person.Id ).SystemNote );

            RestorePerson( person.Id, p => p.SystemNote = originalSystemNote );
        }

        [TestMethod]
        public void PersonFields_ReviewReasonAndNote_AreUpdated()
        {
            const string targetReviewReasonNote = "Bulk update test review note.";
            var reviewReasonId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED.AsGuid() ).Id;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalReviewReasonId = person.ReviewReasonValueId;
            var originalReviewReasonNote = person.ReviewReasonNote;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.ReviewReason, FieldKey.ReviewReasonNote ),
                ReviewReasonValueGuid = SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED.AsGuid(),
                ReviewReasonNote = targetReviewReasonNote
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            var reloaded = ReloadPerson( person.Id );
            Assert.AreEqual( reviewReasonId, reloaded.ReviewReasonValueId );
            Assert.AreEqual( targetReviewReasonNote, reloaded.ReviewReasonNote );

            RestorePerson( person.Id, p =>
            {
                p.ReviewReasonValueId = originalReviewReasonId;
                p.ReviewReasonNote = originalReviewReasonNote;
            } );
        }

        [TestMethod]
        public void PersonFields_ConnectionStatus_IsUpdated_WhenAuthorized()
        {
            var connectionStatusGuid = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid();
            var connectionStatusId = DefinedValueCache.Get( connectionStatusGuid ).Id;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalConnectionStatusId = person.ConnectionStatusValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.ConnectionStatus ),
                ConnectionStatusValueGuid = connectionStatusGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditConnectionStatus = true;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( connectionStatusId, ReloadPerson( person.Id ).ConnectionStatusValueId );

            RestorePerson( person.Id, p => p.ConnectionStatusValueId = originalConnectionStatusId );
        }

        [TestMethod]
        public void PersonFields_ConnectionStatus_IsSkipped_WhenNotAuthorized()
        {
            var connectionStatusGuid = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalConnectionStatusId = person.ConnectionStatusValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.ConnectionStatus ),
                ConnectionStatusValueGuid = connectionStatusGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditConnectionStatus = false;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( originalConnectionStatusId, ReloadPerson( person.Id ).ConnectionStatusValueId, "Connection Status must not change when the caller is not authorized." );
        }

        [TestMethod]
        public void PersonFields_RecordSource_IsUpdated_WhenAuthorized()
        {
            var recordSourceGuid = SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING.AsGuid();
            var recordSourceId = DefinedValueCache.Get( recordSourceGuid ).Id;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalRecordSourceId = person.RecordSourceValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.RecordSource ),
                RecordSourceValueGuid = recordSourceGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditRecordSource = true;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( recordSourceId, ReloadPerson( person.Id ).RecordSourceValueId );

            RestorePerson( person.Id, p => p.RecordSourceValueId = originalRecordSourceId );
        }

        [TestMethod]
        public void PersonFields_RecordSource_IsSkipped_WhenNotAuthorized()
        {
            var recordSourceGuid = SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING.AsGuid();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalRecordSourceId = person.RecordSourceValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.RecordSource ),
                RecordSourceValueGuid = recordSourceGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditRecordSource = false;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( originalRecordSourceId, ReloadPerson( person.Id ).RecordSourceValueId, "Record Source must not change when the caller is not authorized." );
        }

        [TestMethod]
        public void PersonFields_RecordStatus_IsSkipped_WhenNotAuthorized()
        {
            var pendingGuid = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalRecordStatusId = person.RecordStatusValueId;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.RecordStatus ),
                RecordStatusValueGuid = pendingGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditRecordStatus = false;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( originalRecordStatusId, ReloadPerson( person.Id ).RecordStatusValueId, "Record Status must not change when the caller is not authorized." );
        }

        [TestMethod]
        public void PersonFields_RecordStatus_IsUpdated_WhenAuthorized()
        {
            var pendingGuid = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid();
            var pendingId = DefinedValueCache.Get( pendingGuid ).Id;

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var originalRecordStatusId = person.RecordStatusValueId;

            // Pending is a non-Inactive status, so this stays on the shared database and
            // restores: it does not trip the inactivation cascade in Person.SaveHook.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.RecordStatus ),
                RecordStatusValueGuid = pendingGuid
            };

            var settings = NewSettings( bag );
            settings.CanEditRecordStatus = true;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( pendingId, ReloadPerson( person.Id ).RecordStatusValueId );

            RestorePerson( person.Id, p => p.RecordStatusValueId = originalRecordStatusId );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void PersonFields_RecordStatus_Inactive_WritesReasonAndNote()
        {
            const string inactiveNote = "Bulk update test inactive reason note.";
            var inactiveGuid = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid();
            var inactiveId = DefinedValueCache.Get( inactiveGuid ).Id;
            var reasonValue = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues.First();

            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );

            // Setting Inactive also writes the reason value and (non-blank) note, and triggers
            // the SaveHook inactivation cascade, so this runs against a pristine database.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.RecordStatus ),
                RecordStatusValueGuid = inactiveGuid,
                InactiveReasonValueGuid = reasonValue.Guid,
                InactiveReasonNote = inactiveNote
            };

            var settings = NewSettings( bag );
            settings.CanEditRecordStatus = true;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            var reloaded = ReloadPerson( person.Id );
            Assert.AreEqual( inactiveId, reloaded.RecordStatusValueId );
            Assert.AreEqual( reasonValue.Id, reloaded.RecordStatusReasonValueId, "The inactive reason value should be written." );
            Assert.AreEqual( inactiveNote, reloaded.InactiveReasonNote, "The inactive reason note should be written." );
        }

        #endregion Person Fields

        #region Family Campus

        [TestMethod]
        [IsolatedTestDatabase]
        public void Campus_SetsFamilyCampus()
        {
            var southCampusId = CampusCache.Get( TestGuids.Crm.CampusSouth.AsGuid() ).Id;

            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = Toggle( FieldKey.Campus ),
                CampusGuid = TestGuids.Crm.CampusSouth.AsGuid()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            var family = GetPrimaryFamily( new RockContext(), personId );
            Assert.AreEqual( southCampusId, family.CampusId );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Campus_BlankSelectionClearsFamilyCampus()
        {
            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = Toggle( FieldKey.Campus ),
                CampusGuid = null
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            var family = GetPrimaryFamily( new RockContext(), personId );
            Assert.IsNull( family.CampusId, "A blank campus selection should clear the family campus." );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Campus_PersonInMultipleFamilies_RecordsIssue()
        {
            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.BenJones );

            Assert.AreEqual( 1, CountFamilies( rockContext, personId ), "Precondition: Ben should belong to exactly one family before the test adds a second." );

            // Put Ben into a second family so the campus pipeline cannot pick a single family.
            var secondFamily = CreateFamilyGroup( rockContext, "Bulk Update Test Second Family" );
            AddPersonToGroup( rockContext, secondFamily, personId );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = Toggle( FieldKey.Campus ),
                CampusGuid = TestGuids.Crm.CampusSouth.AsGuid()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 0, result.SuccessCount );
            Assert.AreEqual( 1, result.IssuesCount, "A person in multiple families should be reported as an issue." );
            Assert.AreEqual( 0, result.FailedCount );

            var personResult = result.PersonResults.Single();
            Assert.AreEqual( personId, personResult.PersonId );
            StringAssert.Contains( string.Join( " ", personResult.Issues ), "multiple families" );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Campus_TwoPersonsSharingFamily_BothSucceed()
        {
            var southCampusId = CampusCache.Get( TestGuids.Crm.CampusSouth.AsGuid() ).Id;

            var rockContext = new RockContext();
            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var cindyId = GetPersonId( rockContext, TestGuids.TestPeople.CindyDecker );

            // This test exercises the shared-family dedup path, so both must belong to exactly
            // one family and it must be the same one; otherwise a second family would surface as
            // a per-person issue and the success count would not be a clear failure.
            Assert.AreEqual( 1, CountFamilies( rockContext, tedId ), "Precondition: Ted should belong to exactly one family." );
            Assert.AreEqual( 1, CountFamilies( rockContext, cindyId ), "Precondition: Cindy should belong to exactly one family." );
            Assert.AreEqual( GetPrimaryFamily( rockContext, tedId ).Id, GetPrimaryFamily( rockContext, cindyId ).Id, "Precondition: Ted and Cindy should share the same family." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId, cindyId } ),
                UpdatedFields = Toggle( FieldKey.Campus ),
                CampusGuid = TestGuids.Crm.CampusSouth.AsGuid()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 2 );
            Assert.AreEqual( southCampusId, GetPrimaryFamily( new RockContext(), tedId ).CampusId );
            Assert.AreEqual( southCampusId, GetPrimaryFamily( new RockContext(), cindyId ).CampusId );
        }

        #endregion Family Campus

        #region Following

        [TestMethod]
        [IsolatedTestDatabase]
        public void Following_Add_CreatesFollow()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var targetId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var targetPrimaryAliasId = GetPrimaryAliasId( rockContext, targetId );

            // Start from a known-empty state so the assertion is unambiguous.
            DeleteFollows( rockContext, adminAliasId, targetPrimaryAliasId );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { targetId } ),
                UpdatedFields = Toggle( FieldKey.Following ),
                Following = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add
            };

            var result = new BulkUpdateProcessor( NewSettings( bag, adminAliasId ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( 1, CountFollows( new RockContext(), adminAliasId, targetPrimaryAliasId ) );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Following_Add_IsIdempotent()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var targetId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var targetPrimaryAliasId = GetPrimaryAliasId( rockContext, targetId );

            DeleteFollows( rockContext, adminAliasId, targetPrimaryAliasId );
            AddFollowDirect( rockContext, adminAliasId, targetPrimaryAliasId );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { targetId } ),
                UpdatedFields = Toggle( FieldKey.Following ),
                Following = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add
            };

            var result = new BulkUpdateProcessor( NewSettings( bag, adminAliasId ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( 1, CountFollows( new RockContext(), adminAliasId, targetPrimaryAliasId ), "Adding a follow that already exists must not create a duplicate." );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Following_Remove_DeletesFollow()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var targetId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var targetPrimaryAliasId = GetPrimaryAliasId( rockContext, targetId );

            DeleteFollows( rockContext, adminAliasId, targetPrimaryAliasId );
            AddFollowDirect( rockContext, adminAliasId, targetPrimaryAliasId );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { targetId } ),
                UpdatedFields = Toggle( FieldKey.Following ),
                Following = Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove
            };

            var result = new BulkUpdateProcessor( NewSettings( bag, adminAliasId ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( 0, CountFollows( new RockContext(), adminAliasId, targetPrimaryAliasId ) );
        }

        #endregion Following

        #region Person Attributes

        [TestMethod]
        [IsolatedTestDatabase]
        public void PersonAttribute_AuthorizedKey_IsUpdated()
        {
            const string allergyValue = "Bulk update test allergy.";
            var allergyAttribute = AttributeCache.Get( SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() );

            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                PersonAttributes = new Dictionary<string, string> { [allergyAttribute.Key] = allergyValue }
            };

            var settings = NewSettings( bag );
            settings.AuthorizedPersonAttributes = new Dictionary<string, AttributeCache> { [allergyAttribute.Key] = allergyAttribute };

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( allergyValue, GetPersonAttributeValue( personId, allergyAttribute.Key ) );
        }

        [TestMethod]
        public void PersonAttribute_UnauthorizedKey_IsDropped()
        {
            var allergyAttribute = AttributeCache.Get( SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() );

            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var originalValue = GetPersonAttributeValue( personId, allergyAttribute.Key );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                PersonAttributes = new Dictionary<string, string> { [allergyAttribute.Key] = "Should not be written." }
            };

            // The authorization fence is empty, so the submitted key is outside it and dropped.
            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( originalValue ?? string.Empty, GetPersonAttributeValue( personId, allergyAttribute.Key ) ?? string.Empty, "An attribute key outside the authorization fence must not be written." );
        }

        #endregion Person Attributes

        #region Note

        [TestMethod]
        [IsolatedTestDatabase]
        public void Note_Add_CreatesPrivateAlertNote()
        {
            var noteText = $"Bulk update test note {Guid.NewGuid()}";
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            var noteType = NoteTypeCache.GetByEntity( personEntityTypeId, string.Empty, string.Empty, true ).First();

            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                NoteUpdate = new BulkUpdateNoteBag
                {
                    NoteType = new ListItemBag { Value = noteType.Guid.ToString() },
                    NoteText = noteText,
                    IsAlert = true,
                    IsPrivate = true
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedNoteTypeId = noteType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var note = new NoteService( verifyContext ).Queryable()
                    .FirstOrDefault( n => n.NoteTypeId == noteType.Id && n.EntityId == personId && n.Text == noteText );

                Assert.IsNotNull( note, "The note was not created." );
                Assert.IsTrue( note.IsAlert );
                Assert.IsTrue( note.IsPrivateNote );
                Assert.AreEqual( "You - Personal Note", note.Caption );
            }
        }

        [TestMethod]
        public void Note_NoAuthorizedNoteType_IsSkipped()
        {
            var noteText = $"Bulk update test note {Guid.NewGuid()}";

            var rockContext = new RockContext();
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                NoteUpdate = new BulkUpdateNoteBag { NoteText = noteText }
            };

            // AuthorizedNoteTypeId is left null, so the note pipeline is skipped.
            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var exists = new NoteService( verifyContext ).Queryable().Any( n => n.Text == noteText );
                Assert.IsFalse( exists, "No note should be created when no note type is authorized." );
            }
        }

        #endregion Note

        #region Group

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Add_AddsMembers()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateGeneralGroup( rockContext, "Bulk Update Test Group", out var memberRoleGuid );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var billId = GetPersonId( rockContext, TestGuids.TestPeople.BillMarble );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId, billId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    GroupRole = new ListItemBag { Value = memberRoleGuid.ToString() },
                    MemberStatus = GroupMemberStatus.Active
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 2 );

            using ( var verifyContext = new RockContext() )
            {
                var members = new GroupMemberService( verifyContext ).Queryable()
                    .Where( m => m.GroupId == group.Id )
                    .ToList();
                Assert.HasCount( 2, members );
                CollectionAssert.AreEquivalent( new[] { tedId, billId }, members.Select( m => m.PersonId ).ToList() );
                Assert.IsTrue( members.All( m => m.GroupMemberStatus == GroupMemberStatus.Active ) );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Add_AlreadyInRole_IsSkipped()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateGeneralGroup( rockContext, "Bulk Update Test Group", out var memberRoleGuid );
            var memberRoleId = GroupTypeCache.Get( group.GroupTypeId ).Roles.First( r => r.Guid == memberRoleGuid ).Id;

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var billId = GetPersonId( rockContext, TestGuids.TestPeople.BillMarble );

            // Ted is already a member in the target role; only Bill should be added.
            AddGroupMemberDirect( rockContext, group.Id, tedId, memberRoleId, GroupMemberStatus.Active );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId, billId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    GroupRole = new ListItemBag { Value = memberRoleGuid.ToString() },
                    MemberStatus = GroupMemberStatus.Active
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 2 );

            using ( var verifyContext = new RockContext() )
            {
                var members = new GroupMemberService( verifyContext ).Queryable()
                    .Where( m => m.GroupId == group.Id )
                    .ToList();
                Assert.HasCount( 2, members, "A person already in the target role must not be added a second time." );
                Assert.AreEqual( 1, members.Count( m => m.PersonId == tedId ) );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Remove_RemovesMembers()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateGeneralGroup( rockContext, "Bulk Update Test Group", out var memberRoleGuid );
            var memberRoleId = GroupTypeCache.Get( group.GroupTypeId ).Roles.First( r => r.Guid == memberRoleGuid ).Id;

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            AddGroupMemberDirect( rockContext, group.Id, tedId, memberRoleId, GroupMemberStatus.Active );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove,
                    Group = new ListItemBag { Value = group.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var stillMember = new GroupMemberService( verifyContext ).Queryable()
                    .Any( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.IsFalse( stillMember );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Update_ChangesMemberStatus()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateGeneralGroup( rockContext, "Bulk Update Test Group", out var memberRoleGuid );
            var memberRoleId = GroupTypeCache.Get( group.GroupTypeId ).Roles.First( r => r.Guid == memberRoleGuid ).Id;

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            AddGroupMemberDirect( rockContext, group.Id, tedId, memberRoleId, GroupMemberStatus.Active );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    MemberStatus = GroupMemberStatus.Inactive,
                    UpdatedFields = Toggle( GroupFieldKey.MemberStatus )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var member = new GroupMemberService( verifyContext ).Queryable()
                    .First( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.AreEqual( GroupMemberStatus.Inactive, member.GroupMemberStatus );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Update_ChangesRole()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var groupType = CreateTwoRoleGroupType( rockContext, "Bulk Update Test Type", out var roleAGuid, out var roleBGuid );
            var group = CreateGroupOfType( rockContext, "Bulk Update Two-Role Group", groupType.Id );

            var roleA = GroupTypeCache.Get( groupType.Id ).Roles.First( r => r.Guid == roleAGuid );
            var roleBId = GroupTypeCache.Get( groupType.Id ).Roles.First( r => r.Guid == roleBGuid ).Id;

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            AddGroupMemberDirect( rockContext, group.Id, tedId, roleA.Id, GroupMemberStatus.Active );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    GroupRole = new ListItemBag { Value = roleBGuid.ToString() },
                    UpdatedFields = Toggle( GroupFieldKey.Role )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var member = new GroupMemberService( verifyContext ).Queryable()
                    .First( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.AreEqual( roleBId, member.GroupRoleId );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Add_WritesAuthorizedMemberAttribute()
        {
            const string memberAttributeValue = "Bulk update test member attribute value.";

            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateGeneralGroup( rockContext, "Bulk Update Member-Attr Group", out var memberRoleGuid );

            // A member attribute qualified to this group type. Exercises the distinct
            // AuthorizedGroupMemberAttributes fence and the two-phase save (member first, then its
            // attribute value) that the Person Attributes tests do not reach.
            var attribute = CreateEntityAttribute(
                rockContext,
                EntityTypeCache.Get( typeof( GroupMember ) ).Id,
                "GroupTypeId",
                group.GroupTypeId.ToString(),
                "BulkUpdateTestMemberAttribute" );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    GroupRole = new ListItemBag { Value = memberRoleGuid.ToString() },
                    MemberStatus = GroupMemberStatus.Active,
                    MemberAttributes = new Dictionary<string, string> { [attribute.Key] = memberAttributeValue }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;
            settings.AuthorizedGroupMemberAttributes = new Dictionary<string, AttributeCache> { [attribute.Key] = AttributeCache.Get( attribute.Id ) };

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var member = new GroupMemberService( verifyContext ).Queryable()
                    .First( m => m.GroupId == group.Id && m.PersonId == tedId );
                member.LoadAttributes( verifyContext );
                Assert.AreEqual( memberAttributeValue, member.GetAttributeValue( attribute.Key ) );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Remove_WithHistory_ArchivesInsteadOfDeleting()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var group = CreateHistoryEnabledGroup( rockContext, "Bulk Update History", out var memberRoleGuid );
            var memberRoleId = GroupTypeCache.Get( group.GroupTypeId ).Roles.First( r => r.Guid == memberRoleGuid ).Id;

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            AddGroupMemberDirect( rockContext, group.Id, tedId, memberRoleId, GroupMemberStatus.Active );

            // Give the member a history snapshot so Remove must archive (preserve it) rather than
            // hard-delete.
            var member = new GroupMemberService( rockContext ).Queryable()
                .First( m => m.GroupId == group.Id && m.PersonId == tedId );
            new GroupMemberHistoricalService( rockContext ).Add(
                GroupMemberHistorical.CreateCurrentRowFromGroupMember( member, RockDateTime.Now ) );
            rockContext.SaveChanges();

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove,
                    Group = new ListItemBag { Value = group.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( verifyContext );

                var archivedMember = groupMemberService.Queryable( true, true )
                    .FirstOrDefault( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.IsNotNull( archivedMember, "The member row should still exist (archived, not hard-deleted)." );
                Assert.IsTrue( archivedMember.IsArchived, "The member should have been archived." );

                var stillActive = groupMemberService.Queryable()
                    .Any( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.IsFalse( stillActive, "An archived member should not appear in the default (non-archived) query." );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Group_Update_InvalidMember_DetachesAndRecordsIssue()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );

            // Role B (Leader) is capped at one active member.
            var groupType = CreateTwoRoleGroupType( rockContext, "Bulk Update Capacity Type", out var roleAGuid, out var roleBGuid, roleBMaxCount: 1 );
            var group = CreateGroupOfType( rockContext, "Bulk Update Capacity Group", groupType.Id );

            var roleA = GroupTypeCache.Get( groupType.Id ).Roles.First( r => r.Guid == roleAGuid );
            var roleB = GroupTypeCache.Get( groupType.Id ).Roles.First( r => r.Guid == roleBGuid );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var billId = GetPersonId( rockContext, TestGuids.TestPeople.BillMarble );

            // Bill already fills the single Leader slot; Ted starts as a Member.
            AddGroupMemberDirect( rockContext, group.Id, billId, roleB.Id, GroupMemberStatus.Active );
            AddGroupMemberDirect( rockContext, group.Id, tedId, roleA.Id, GroupMemberStatus.Active );

            // Move only Ted into the (full) Leader role. The processor must detach him and report a
            // per-person issue rather than letting the bad row fail the whole batch.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                GroupUpdate = new BulkUpdateGroupBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    Group = new ListItemBag { Value = group.Guid.ToString() },
                    GroupRole = new ListItemBag { Value = roleBGuid.ToString() },
                    UpdatedFields = Toggle( GroupFieldKey.Role )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedGroupId = group.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 0, result.SuccessCount );
            Assert.AreEqual( 1, result.IssuesCount );
            Assert.AreEqual( 0, result.FailedCount );
            StringAssert.Contains( string.Join( " ", result.PersonResults.Single().Issues ), "maximum allowed limit" );

            using ( var verifyContext = new RockContext() )
            {
                var member = new GroupMemberService( verifyContext ).Queryable()
                    .First( m => m.GroupId == group.Id && m.PersonId == tedId );
                Assert.AreEqual( roleA.Id, member.GroupRoleId, "The invalid role change must not have been committed." );
            }
        }

        #endregion Group

        #region Step

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Add_AddsStep()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Add Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Add Step Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.SarahSimmons );

            // The step type is brand new, so the person cannot already hold a step of it.
            Assert.IsFalse( PersonHasStepOfType( personId, stepType.Id ), "Precondition: the freshly created step type should have no steps yet." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StepStatus = new ListItemBag { Value = successStatus.Guid.ToString() },
                    StartDate = RockDateTime.Now.Date
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.IsTrue( PersonHasStepOfType( personId, stepType.Id ), "The step was not added." );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Add_AllowMultipleViolation_RecordsIssue()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Single Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Single Step Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var personPrimaryAliasId = GetPrimaryAliasId( rockContext, personId );

            // The step type does not allow multiple and the person already holds one step of it,
            // so a bulk Add must be rejected with a per-person issue rather than added.
            AddStepDirect( rockContext, stepType.Id, personPrimaryAliasId, RockDateTime.Now.Date, null, successStatus.Id );

            Assert.IsFalse( stepType.AllowMultiple, "Precondition: this scenario requires the step type to NOT allow multiple steps." );
            Assert.IsTrue( PersonHasStepOfType( personId, stepType.Id ), "Precondition: the person should already hold a step of this type." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StepStatus = new ListItemBag { Value = successStatus.Guid.ToString() },
                    StartDate = RockDateTime.Now.Date
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 0, result.SuccessCount );
            Assert.AreEqual( 1, result.IssuesCount );
            Assert.AreEqual( 0, result.FailedCount );
            StringAssert.Contains( string.Join( " ", result.PersonResults.Single().Issues ), "Allow Multiple" );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Remove_RemovesSteps()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Remove Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Remove Step Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var personPrimaryAliasId = GetPrimaryAliasId( rockContext, personId );

            // Seed a step so Remove has something to delete.
            AddStepDirect( rockContext, stepType.Id, personPrimaryAliasId, RockDateTime.Now.Date, null, successStatus.Id );

            Assert.IsTrue( PersonHasStepOfType( personId, stepType.Id ), "Precondition: the person should hold a step to remove." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.IsFalse( PersonHasStepOfType( personId, stepType.Id ), "The step was not removed." );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Update_ChangesStatusAndRecomputesCompletion()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Status Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Status Step Type", allowMultiple: false );
            var completeStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var pendingStatus = CreateStepStatus( rockContext, program.Id, "Pending", isCompleteStatus: false );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var personPrimaryAliasId = GetPrimaryAliasId( rockContext, personId );

            // Seed a completed step that carries a completion date, so the recompute has a
            // non-null value to clear (otherwise the "cleared" assertion could pass trivially).
            var seededDate = RockDateTime.Now.Date;
            var seededStep = AddStepDirect( rockContext, stepType.Id, personPrimaryAliasId, seededDate, null, completeStatus.Id );
            seededStep.CompletedDateTime = seededDate;
            rockContext.SaveChanges();

            Assert.IsNotNull( seededStep.CompletedDateTime, "Precondition: the seeded step should start with a completion date." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StepStatus = new ListItemBag { Value = pendingStatus.Guid.ToString() },
                    UpdatedFields = Toggle( StepFieldKey.Status )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var step = new StepService( verifyContext ).Queryable()
                    .First( s => s.StepTypeId == stepType.Id && s.PersonAlias.PersonId == personId );
                Assert.AreEqual( pendingStatus.Id, step.StepStatusId );
                Assert.IsNull( step.CompletedDateTime, "Moving a step to a non-complete status should clear the completion date." );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Update_ChangesNote()
        {
            const string targetNote = "Bulk update test step note.";

            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Note Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Note Step Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var personPrimaryAliasId = GetPrimaryAliasId( rockContext, personId );

            // Seed a step so Update has an existing row to modify.
            AddStepDirect( rockContext, stepType.Id, personPrimaryAliasId, RockDateTime.Now.Date, null, successStatus.Id );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    Note = targetNote,
                    UpdatedFields = Toggle( StepFieldKey.Note )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var step = new StepService( verifyContext ).Queryable()
                    .First( s => s.StepTypeId == stepType.Id && s.PersonAlias.PersonId == personId );
                Assert.AreEqual( targetNote, step.Note );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Add_WritesAuthorizedStepAttribute()
        {
            const string stepAttributeValue = "Bulk update test step attribute value.";

            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Step-Attr Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Step-Attr Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.SarahSimmons );

            // The step type is brand new, so the person cannot already hold a step of it.
            Assert.IsFalse( PersonHasStepOfType( personId, stepType.Id ), "Precondition: the freshly created step type should have no steps yet." );

            // A step attribute qualified to this step type. Exercises the distinct
            // AuthorizedStepAttributes fence and the two-phase save (step first, then its attribute
            // value) that the Person Attributes tests do not reach.
            var attribute = CreateEntityAttribute(
                rockContext,
                EntityTypeCache.Get( typeof( Step ) ).Id,
                "StepTypeId",
                stepType.Id.ToString(),
                "BulkUpdateTestStepAttribute" );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StepStatus = new ListItemBag { Value = successStatus.Guid.ToString() },
                    StartDate = RockDateTime.Now.Date,
                    StepAttributes = new Dictionary<string, string> { [attribute.Key] = stepAttributeValue }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;
            settings.AuthorizedStepAttributes = new Dictionary<string, AttributeCache> { [attribute.Key] = AttributeCache.Get( attribute.Id ) };

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );

            using ( var verifyContext = new RockContext() )
            {
                var step = new StepService( verifyContext ).Queryable()
                    .First( s => s.StepTypeId == stepType.Id && s.PersonAlias.PersonId == personId );
                step.LoadAttributes( verifyContext );
                Assert.AreEqual( stepAttributeValue, step.GetAttributeValue( attribute.Key ) );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Step_Update_InvalidDateOrder_DetachesAndRecordsIssue()
        {
            var seededStart = new DateTime( 2020, 1, 1 );
            var seededEnd = new DateTime( 2020, 6, 1 );
            var invalidStart = new DateTime( 2021, 1, 1 ); // after the existing end date

            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Date Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Date Step Type", allowMultiple: false, hasEndDate: true );
            var personId = GetPersonId( rockContext, TestGuids.TestPeople.SarahSimmons );
            var personPrimaryAliasId = GetPrimaryAliasId( rockContext, personId );

            // Seed a valid step (start before end), then move its start past the end so Step.IsValid
            // fails. The processor must detach it and report a per-person issue, not commit it.
            AddStepDirect( rockContext, stepType.Id, personPrimaryAliasId, seededStart, seededEnd, null );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { personId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Update,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StartDate = invalidStart,
                    UpdatedFields = Toggle( StepFieldKey.StartDate )
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 0, result.SuccessCount );
            Assert.AreEqual( 1, result.IssuesCount );
            Assert.AreEqual( 0, result.FailedCount );
            StringAssert.Contains( string.Join( " ", result.PersonResults.Single().Issues ), "StartDateTime" );

            using ( var verifyContext = new RockContext() )
            {
                var step = new StepService( verifyContext ).Queryable()
                    .First( s => s.StepTypeId == stepType.Id && s.PersonAlias.PersonId == personId );
                Assert.AreEqual( seededStart, step.StartDateTime, "The invalid start date must not have been committed." );
            }
        }

        #endregion Step

        #region Tag

        [TestMethod]
        [IsolatedTestDatabase]
        public void Tag_Add_TagsPersons()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var tag = CreatePersonTag( rockContext, "Bulk Update Test Tag" );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var billId = GetPersonId( rockContext, TestGuids.TestPeople.BillMarble );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId, billId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                TagUpdate = new BulkUpdateTagBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    Tag = new ListItemBag { Value = tag.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedTagId = tag.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 2 );
            Assert.AreEqual( 2, CountTaggedItems( new RockContext(), tag.Id ) );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Tag_Add_IsIdempotent()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var tag = CreatePersonTag( rockContext, "Bulk Update Test Tag" );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            var billId = GetPersonId( rockContext, TestGuids.TestPeople.BillMarble );

            // Ted is already tagged; only Bill should be newly tagged.
            AddTaggedItemDirect( rockContext, tag.Id, GetPersonGuid( rockContext, tedId ) );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId, billId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                TagUpdate = new BulkUpdateTagBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    Tag = new ListItemBag { Value = tag.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedTagId = tag.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 2 );
            Assert.AreEqual( 2, CountTaggedItems( new RockContext(), tag.Id ), "Tagging a person already tagged must not create a duplicate." );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Tag_Remove_UntagsPersons()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var tag = CreatePersonTag( rockContext, "Bulk Update Test Tag" );

            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );
            AddTaggedItemDirect( rockContext, tag.Id, GetPersonGuid( rockContext, tedId ) );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                TagUpdate = new BulkUpdateTagBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove,
                    Tag = new ListItemBag { Value = tag.Guid.ToString() }
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedTagId = tag.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            AssertAllSucceeded( result, 1 );
            Assert.AreEqual( 0, CountTaggedItems( new RockContext(), tag.Id ) );
        }

        #endregion Tag

        #region Workflow

        [TestMethod]
        [IsolatedTestDatabase]
        public void Workflow_Launch_DoesNotAffectCoreOutcome()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );

            var workflowType = new WorkflowTypeService( rockContext ).Queryable()
                .FirstOrDefault( wt => wt.IsActive == true );
            if ( workflowType == null )
            {
                Assert.Inconclusive( "No active workflow type is available in the sample data set." );
                return;
            }

            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var targetGender = person.Gender == Gender.Male ? Gender.Female : Gender.Male;

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { person.Id } ),
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = targetGender,
                PostUpdateWorkflowTypeGuids = new List<Guid> { workflowType.Guid }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedWorkflowTypeIds = new List<int> { workflowType.Id };

            var result = new BulkUpdateProcessor( settings ).Process();

            // The workflow launch is fire-and-forget and runs after the core commit; it must
            // not change the per-person outcome of the committed field write.
            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 1, result.SuccessCount );
            Assert.AreEqual( targetGender, ReloadPerson( person.Id ).Gender );
        }

        #endregion Workflow

        #region Result Accounting

        [TestMethod]
        public void Process_NoPersons_ReturnsEmptyResult()
        {
            var bag = new BulkUpdateBag
            {
                UpdatePersons = new List<BulkUpdatePersonBag>(),
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = Gender.Male
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            Assert.AreEqual( 0, result.TotalCount );
            Assert.AreEqual( 0, result.SuccessCount );
            Assert.AreEqual( 0, result.IssuesCount );
            Assert.AreEqual( 0, result.FailedCount );
        }

        [TestMethod]
        public void Process_NullUpdatePersons_ReturnsEmptyResult()
        {
            var bag = new BulkUpdateBag
            {
                UpdatePersons = null,
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = Gender.Male
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            Assert.AreEqual( 0, result.TotalCount );
        }

        [TestMethod]
        public void Process_UnresolvableAlias_IsDroppedFromTotal()
        {
            var rockContext = new RockContext();
            var personBags = PersonBagsForGuids( rockContext, TestGuids.TestPeople.TedDecker );

            // Add an alias guid that does not resolve to any person; it should be silently
            // dropped (it reduces TotalCount, it is not counted as a failure).
            personBags.Add( new BulkUpdatePersonBag { PersonAliasGuid = Guid.NewGuid() } );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = personBags,
                UpdatedFields = new Dictionary<string, bool>()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            Assert.AreEqual( 1, result.TotalCount, "Unresolvable aliases should not be counted in the total." );
            Assert.AreEqual( 1, result.SuccessCount );
            Assert.AreEqual( 0, result.FailedCount );
        }

        [TestMethod]
        public void Process_DuplicatePerson_IsDeduplicated()
        {
            var rockContext = new RockContext();
            var personBags = PersonBagsForGuids( rockContext, TestGuids.TestPeople.TedDecker );

            // Submit the same person twice; ResolvePersonIds must collapse the duplicate.
            personBags.Add( new BulkUpdatePersonBag { PersonAliasGuid = personBags[0].PersonAliasGuid } );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = personBags,
                UpdatedFields = new Dictionary<string, bool>()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag ) ).Process();

            Assert.AreEqual( 1, result.TotalCount, "A person listed twice should be processed once." );
            Assert.AreEqual( 1, result.SuccessCount );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void Process_MixedOutcomes_BucketsSumToTotal()
        {
            var rockContext = new RockContext();
            var adminAliasId = GetAdminPrimaryAliasId( rockContext );
            var program = CreateStepProgram( rockContext, "Bulk Update Mixed Step Program" );
            var stepType = CreateStepType( rockContext, program.Id, "Bulk Update Mixed Step Type", allowMultiple: false );
            var successStatus = CreateStepStatus( rockContext, program.Id, "Success", isCompleteStatus: true );

            var sarahId = GetPersonId( rockContext, TestGuids.TestPeople.SarahSimmons );
            var tedId = GetPersonId( rockContext, TestGuids.TestPeople.TedDecker );

            // The step type does not allow multiple: Ted already holds one (Allow Multiple issue),
            // Sarah does not (success). One of each gives a mixed result whose buckets must sum.
            AddStepDirect( rockContext, stepType.Id, GetPrimaryAliasId( rockContext, tedId ), RockDateTime.Now.Date, null, successStatus.Id );

            Assert.IsFalse( PersonHasStepOfType( sarahId, stepType.Id ), "Precondition: Sarah should have no step of this type." );
            Assert.IsTrue( PersonHasStepOfType( tedId, stepType.Id ), "Precondition: Ted should already hold a step of this type." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, new[] { sarahId, tedId } ),
                UpdatedFields = new Dictionary<string, bool>(),
                StepUpdate = new BulkUpdateStepBag
                {
                    Action = Rock.Enums.Crm.BulkUpdateActionSpecifier.Add,
                    StepType = new ListItemBag { Value = stepType.Guid.ToString() },
                    StepStatus = new ListItemBag { Value = successStatus.Guid.ToString() },
                    StartDate = RockDateTime.Now.Date
                }
            };

            var settings = NewSettings( bag, adminAliasId );
            settings.AuthorizedStepTypeId = stepType.Id;

            var result = new BulkUpdateProcessor( settings ).Process();

            Assert.AreEqual( 2, result.TotalCount );
            Assert.AreEqual( 1, result.SuccessCount );
            Assert.AreEqual( 1, result.IssuesCount );
            Assert.AreEqual( 0, result.FailedCount );
            Assert.AreEqual( result.TotalCount, result.SuccessCount + result.IssuesCount + result.FailedCount, "The three outcome buckets must sum to the total." );
        }

        [TestMethod]
        public void Process_TaskCountZero_FallsBackToProcessorCount()
        {
            var rockContext = new RockContext();

            // A non-positive TaskCount must fall back to Environment.ProcessorCount rather than
            // producing a MaxDegreeOfParallelism of 0 (which would throw). Empty UpdatedFields
            // keeps this a database no-op.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForGuids( rockContext, TestGuids.TestPeople.TedDecker ),
                UpdatedFields = new Dictionary<string, bool>()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag, taskCount: 0 ) ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 1, result.SuccessCount );
        }

        [TestMethod]
        public void Process_TaskCountAboveCap_DoesNotThrow()
        {
            var rockContext = new RockContext();

            // A TaskCount above the hard cap (64) must be clamped, not rejected. Empty
            // UpdatedFields keeps this a database no-op.
            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForGuids( rockContext, TestGuids.TestPeople.TedDecker ),
                UpdatedFields = new Dictionary<string, bool>()
            };

            var result = new BulkUpdateProcessor( NewSettings( bag, taskCount: 1000 ) ).Process();

            Assert.AreEqual( 1, result.TotalCount );
            Assert.AreEqual( 1, result.SuccessCount );
        }

        #endregion Result Accounting

        #region Concurrency

        [TestMethod]
        [IsolatedTestDatabase]
        public void Process_MultipleTasksAndBatches_AppliesToEveryPerson()
        {
            const int personCount = 50;

            var rockContext = new RockContext();
            var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            var personIds = new PersonService( rockContext ).Queryable()
                .Where( p => p.RecordTypeValueId == personRecordTypeId && !p.IsDeceased )
                .OrderBy( p => p.Id )
                .Select( p => p.Id )
                .Take( personCount )
                .ToList();

            Assert.IsTrue( personIds.Count > 1, "Test precondition: the sample data set must contain multiple people." );

            var bag = new BulkUpdateBag
            {
                UpdatePersons = PersonBagsForIds( rockContext, personIds ),
                UpdatedFields = Toggle( FieldKey.Gender ),
                Gender = Gender.Female
            };

            // Force several concurrent workers and small batches so the parallel path and the
            // per-batch RockContext partitioning are genuinely exercised.
            var settings = NewSettings( bag, taskCount: 4, batchSize: 7 );

            var result = new BulkUpdateProcessor( settings ).Process();

            Assert.AreEqual( personIds.Count, result.TotalCount );
            Assert.AreEqual( personIds.Count, result.SuccessCount );
            Assert.AreEqual( 0, result.FailedCount );

            using ( var verifyContext = new RockContext() )
            {
                var notFemaleCount = new PersonService( verifyContext ).Queryable()
                    .Count( p => personIds.Contains( p.Id ) && p.Gender != Gender.Female );
                Assert.AreEqual( 0, notFemaleCount, "Every selected person should have been updated." );
            }
        }

        #endregion Concurrency

        #region Helpers

        /// <summary>
        /// Builds settings with permissive defaults: all gates open, empty (non-null) fences, and
        /// a single worker for determinism. Tests tighten what they need.
        /// </summary>
        private static BulkUpdateSettings NewSettings( BulkUpdateBag bag, int? currentPersonAliasId = null, int taskCount = 1, int batchSize = 0 )
        {
            return new BulkUpdateSettings
            {
                Bag = bag,
                CurrentPersonAliasId = currentPersonAliasId,
                CanEditConnectionStatus = true,
                CanEditRecordStatus = true,
                CanEditRecordSource = true,
                AuthorizedPersonAttributes = new Dictionary<string, AttributeCache>(),
                AuthorizedGroupMemberAttributes = new Dictionary<string, AttributeCache>(),
                AuthorizedStepAttributes = new Dictionary<string, AttributeCache>(),
                AuthorizedWorkflowTypeIds = new List<int>(),
                TaskCount = taskCount,
                BatchSize = batchSize
            };
        }

        /// <summary>
        /// Builds the person payload the way the PersonPicker does: one entry per person,
        /// carrying that person's primary-alias <see cref="Guid"/>.
        /// </summary>
        private static List<BulkUpdatePersonBag> PersonBagsForIds( RockContext rockContext, IEnumerable<int> personIds )
        {
            var ids = personIds.Distinct().ToList();

            // The primary alias is the one whose AliasPersonId equals the PersonId (the alias
            // created with the person). This matches Person.PrimaryAlias. Materialize before
            // grouping so the projection is not pushed into SQL.
            var primaryAliasGuidByPersonId = new PersonAliasService( rockContext ).Queryable()
                .Where( a => ids.Contains( a.PersonId ) && a.AliasPersonId == a.PersonId )
                .Select( a => new { a.PersonId, a.Guid } )
                .ToList()
                .GroupBy( a => a.PersonId )
                .ToDictionary( g => g.Key, g => g.First().Guid );

            var nameByPersonId = new PersonService( rockContext ).Queryable()
                .Where( p => ids.Contains( p.Id ) )
                .Select( p => new { p.Id, p.NickName, p.LastName } )
                .ToList()
                .ToDictionary( p => p.Id, p => $"{p.NickName} {p.LastName}" );

            var bags = new List<BulkUpdatePersonBag>();
            foreach ( var id in ids )
            {
                if ( !primaryAliasGuidByPersonId.TryGetValue( id, out var aliasGuid ) || aliasGuid == Guid.Empty )
                {
                    continue;
                }

                bags.Add( new BulkUpdatePersonBag
                {
                    PersonAliasGuid = aliasGuid,
                    FullName = nameByPersonId.TryGetValue( id, out var name ) ? name : null
                } );
            }

            return bags;
        }

        /// <summary>
        /// Builds the person payload from a set of person GUIDs.
        /// </summary>
        private static List<BulkUpdatePersonBag> PersonBagsForGuids( RockContext rockContext, params string[] personGuids )
        {
            var guids = personGuids.Select( g => g.AsGuid() ).ToList();
            var ids = new PersonService( rockContext ).Queryable()
                .Where( p => guids.Contains( p.Guid ) )
                .Select( p => p.Id )
                .ToList();

            return PersonBagsForIds( rockContext, ids );
        }

        /// <summary>
        /// Gets the integer person identifier for a sample-data person GUID.
        /// </summary>
        private static int GetPersonId( RockContext rockContext, string personGuid )
        {
            var guid = personGuid.AsGuid();
            return new PersonService( rockContext ).Queryable()
                .Where( p => p.Guid == guid )
                .Select( p => p.Id )
                .First();
        }

        /// <summary>
        /// Gets the primary alias identifier of the sample-data administrator (Alisha Marble),
        /// used as the acting user for Following and audit-sensitive operations.
        /// </summary>
        private static int GetAdminPrimaryAliasId( RockContext rockContext )
        {
            var admin = CoreDataManager.Current.GetAdminPersonOrThrow( rockContext );
            return admin.PrimaryAliasId ?? throw new Exception( "The administrator test person has no primary alias." );
        }

        /// <summary>
        /// Re-reads a person on a fresh context so assertions see committed state, not the
        /// arrange context's tracked copy.
        /// </summary>
        private static Person ReloadPerson( int personId )
        {
            return new PersonService( new RockContext() ).Get( personId );
        }

        /// <summary>
        /// Restores a person's fields with a direct write (see the class note for why restore is
        /// not a second processor run).
        /// </summary>
        private static void RestorePerson( int personId, Action<Person> restore )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );
                restore( person );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Builds an <see cref="BulkUpdateBag.UpdatedFields"/> dictionary with the given keys
        /// all toggled on.
        /// </summary>
        private static Dictionary<string, bool> Toggle( params string[] keys )
        {
            return keys.ToDictionary( key => key, key => true );
        }

        /// <summary>
        /// Asserts that every selected person succeeded with no issues or failures.
        /// </summary>
        private static void AssertAllSucceeded( BulkUpdateResultBag result, int expectedCount )
        {
            Assert.AreEqual( expectedCount, result.TotalCount, "Unexpected TotalCount." );
            Assert.AreEqual( expectedCount, result.SuccessCount, "Unexpected SuccessCount." );
            Assert.AreEqual( 0, result.IssuesCount, "Did not expect any per-person issues." );
            Assert.AreEqual( 0, result.FailedCount, "Did not expect any failures." );
        }

        /// <summary>
        /// Gets the primary alias identifier for a person (the alias whose AliasPersonId equals
        /// the PersonId).
        /// </summary>
        private static int GetPrimaryAliasId( RockContext rockContext, int personId )
        {
            return new PersonAliasService( rockContext ).Queryable()
                .Where( a => a.PersonId == personId && a.AliasPersonId == personId )
                .Select( a => a.Id )
                .First();
        }

        /// <summary>
        /// Gets a person's <see cref="Guid"/> by identifier.
        /// </summary>
        private static Guid GetPersonGuid( RockContext rockContext, int personId )
        {
            return new PersonService( rockContext ).Queryable()
                .Where( p => p.Id == personId )
                .Select( p => p.Guid )
                .First();
        }

        /// <summary>
        /// Counts the distinct families a person belongs to.
        /// </summary>
        private static int CountFamilies( RockContext rockContext, int personId )
        {
            var familyGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            return new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
                .Select( m => m.GroupId )
                .Distinct()
                .Count();
        }

        /// <summary>
        /// Gets a person's family group (the first family the person belongs to).
        /// </summary>
        private static Group GetPrimaryFamily( RockContext rockContext, int personId )
        {
            var familyGuid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            return new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
                .Select( m => m.Group )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads a single person attribute value on a fresh context.
        /// </summary>
        private static string GetPersonAttributeValue( int personId, string attributeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );
                person.LoadAttributes( rockContext );
                return person.GetAttributeValue( attributeKey );
            }
        }

        /// <summary>
        /// Determines whether the person holds at least one step of the given step type.
        /// </summary>
        private static bool PersonHasStepOfType( int personId, int stepTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new StepService( rockContext ).Queryable()
                    .Any( s => s.StepTypeId == stepTypeId && s.PersonAlias.PersonId == personId );
            }
        }

        /// <summary>
        /// Counts the follows the given follower holds on the given followed alias (ignoring
        /// purposed follows, matching the bulk update pipeline).
        /// </summary>
        private static int CountFollows( RockContext rockContext, int followerAliasId, int followedPrimaryAliasId )
        {
            var personAliasEntityTypeId = EntityTypeCache.Get( typeof( PersonAlias ) ).Id;
            return new FollowingService( rockContext ).Queryable()
                .Count( f => f.EntityTypeId == personAliasEntityTypeId
                    && f.PersonAliasId == followerAliasId
                    && f.EntityId == followedPrimaryAliasId
                    && ( f.PurposeKey == null || f.PurposeKey == "" ) );
        }

        /// <summary>
        /// Deletes every follow the given follower holds on the given followed alias, so a
        /// Following test starts from a known-empty state.
        /// </summary>
        private static void DeleteFollows( RockContext rockContext, int followerAliasId, int followedPrimaryAliasId )
        {
            var personAliasEntityTypeId = EntityTypeCache.Get( typeof( PersonAlias ) ).Id;
            var followingService = new FollowingService( rockContext );
            var existing = followingService.Queryable()
                .Where( f => f.EntityTypeId == personAliasEntityTypeId
                    && f.PersonAliasId == followerAliasId
                    && f.EntityId == followedPrimaryAliasId )
                .ToList();

            if ( existing.Any() )
            {
                followingService.DeleteRange( existing );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts a follow directly (not via the processor), so a Remove/dedup test has a row
        /// to act on.
        /// </summary>
        private static void AddFollowDirect( RockContext rockContext, int followerAliasId, int followedPrimaryAliasId )
        {
            var personAliasEntityTypeId = EntityTypeCache.Get( typeof( PersonAlias ) ).Id;
            new FollowingService( rockContext ).Add( new Rock.Model.Following
            {
                EntityTypeId = personAliasEntityTypeId,
                EntityId = followedPrimaryAliasId,
                PersonAliasId = followerAliasId
            } );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates a new group of the given group type.
        /// </summary>
        private static Group CreateGroupOfType( RockContext rockContext, string name, int groupTypeId )
        {
            var group = new Group
            {
                Name = name,
                GroupTypeId = groupTypeId,
                IsActive = true,
                IsSystem = false
            };
            new GroupService( rockContext ).Add( group );
            rockContext.SaveChanges();
            return group;
        }

        /// <summary>
        /// Creates a new group of the General group type and returns the Member role's GUID.
        /// </summary>
        private static Group CreateGeneralGroup( RockContext rockContext, string name, out Guid memberRoleGuid )
        {
            var generalType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_GENERAL.AsGuid() );
            var memberRole = generalType.Roles.FirstOrDefault( r => r.Name == "Member" ) ?? generalType.Roles.First();
            memberRoleGuid = memberRole.Guid;
            return CreateGroupOfType( rockContext, name, generalType.Id );
        }

        /// <summary>
        /// Creates a new family group.
        /// </summary>
        private static Group CreateFamilyGroup( RockContext rockContext, string name )
        {
            var familyType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            return CreateGroupOfType( rockContext, name, familyType.Id );
        }

        /// <summary>
        /// Creates a group type with two roles (a non-leader "Member" and a leader "Leader"),
        /// returning their GUIDs so a role-change test has a second role to move into.
        /// </summary>
        private static GroupType CreateTwoRoleGroupType( RockContext rockContext, string name, out Guid roleAGuid, out Guid roleBGuid, int? roleBMaxCount = null )
        {
            var groupType = new GroupType
            {
                Name = name,
                GroupTerm = "Group",
                GroupMemberTerm = "Member",
                ShowInGroupList = false,
                ShowInNavigation = false
            };
            new GroupTypeService( rockContext ).Add( groupType );
            rockContext.SaveChanges();

            var roleA = new GroupTypeRole { GroupTypeId = groupType.Id, Name = "Member", IsLeader = false, Order = 0 };
            var roleB = new GroupTypeRole { GroupTypeId = groupType.Id, Name = "Leader", IsLeader = true, Order = 1, MaxCount = roleBMaxCount };
            var roleService = new GroupTypeRoleService( rockContext );
            roleService.Add( roleA );
            roleService.Add( roleB );
            rockContext.SaveChanges();

            roleAGuid = roleA.Guid;
            roleBGuid = roleB.Guid;
            return groupType;
        }

        /// <summary>
        /// Creates a new group whose group type has <c>EnableGroupHistory</c> on, returning the
        /// Member role's GUID. Used by the archive-vs-delete test.
        /// </summary>
        private static Group CreateHistoryEnabledGroup( RockContext rockContext, string name, out Guid memberRoleGuid )
        {
            var groupType = new GroupType
            {
                Name = name,
                GroupTerm = "Group",
                GroupMemberTerm = "Member",
                ShowInGroupList = false,
                ShowInNavigation = false,
                EnableGroupHistory = true
            };
            new GroupTypeService( rockContext ).Add( groupType );
            rockContext.SaveChanges();

            var role = new GroupTypeRole { GroupTypeId = groupType.Id, Name = "Member", IsLeader = false, Order = 0 };
            new GroupTypeRoleService( rockContext ).Add( role );
            rockContext.SaveChanges();

            memberRoleGuid = role.Guid;
            return CreateGroupOfType( rockContext, name + " Group", groupType.Id );
        }

        /// <summary>
        /// Creates a Text attribute for the given entity type and qualifier, then clears the
        /// attribute cache so a freshly-loaded entity sees it (the Attribute SaveHook refreshes
        /// the cache asynchronously, which a test cannot await).
        /// </summary>
        private static Rock.Model.Attribute CreateEntityAttribute( RockContext rockContext, int entityTypeId, string qualifierColumn, string qualifierValue, string key )
        {
            var attribute = new Rock.Model.Attribute
            {
                EntityTypeId = entityTypeId,
                EntityTypeQualifierColumn = qualifierColumn,
                EntityTypeQualifierValue = qualifierValue,
                Name = key,
                Key = key,
                Guid = Guid.NewGuid(),
                FieldTypeId = FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid() ).Id,
                ShowOnBulk = true
            };
            new AttributeService( rockContext ).Add( attribute );
            rockContext.SaveChanges();

            AttributeCache.Clear();
            return attribute;
        }

        /// <summary>
        /// Creates a Step Program directly, so a Step test can build its own hermetic step
        /// fixture instead of depending on the engagement "Sacraments" sample data, which the
        /// standard test database does not seed.
        /// </summary>
        private static StepProgram CreateStepProgram( RockContext rockContext, string name )
        {
            var stepProgram = new StepProgram
            {
                Name = name,
                IsActive = true
            };
            new StepProgramService( rockContext ).Add( stepProgram );
            rockContext.SaveChanges();
            return stepProgram;
        }

        /// <summary>
        /// Creates a Step Type under the given program. <paramref name="allowMultiple"/> is
        /// explicit so a test can force the exact "Allow Multiple" rule it intends to exercise.
        /// </summary>
        private static StepType CreateStepType( RockContext rockContext, int stepProgramId, string name, bool allowMultiple, bool hasEndDate = false )
        {
            var stepType = new StepType
            {
                StepProgramId = stepProgramId,
                Name = name,
                IsActive = true,
                AllowMultiple = allowMultiple,
                HasEndDate = hasEndDate
            };
            new StepTypeService( rockContext ).Add( stepType );
            rockContext.SaveChanges();
            return stepType;
        }

        /// <summary>
        /// Creates a Step Status under the given program.
        /// </summary>
        private static StepStatus CreateStepStatus( RockContext rockContext, int stepProgramId, string name, bool isCompleteStatus )
        {
            var stepStatus = new StepStatus
            {
                StepProgramId = stepProgramId,
                Name = name,
                IsActive = true,
                IsCompleteStatus = isCompleteStatus
            };
            new StepStatusService( rockContext ).Add( stepStatus );
            rockContext.SaveChanges();
            return stepStatus;
        }

        /// <summary>
        /// Inserts a Step directly (not via the processor), so a Step modify test has an existing
        /// step to act on.
        /// </summary>
        private static Step AddStepDirect( RockContext rockContext, int stepTypeId, int personPrimaryAliasId, DateTime? startDateTime, DateTime? endDateTime, int? stepStatusId )
        {
            var step = new Step
            {
                StepTypeId = stepTypeId,
                PersonAliasId = personPrimaryAliasId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                StepStatusId = stepStatusId
            };
            new StepService( rockContext ).Add( step );
            rockContext.SaveChanges();
            return step;
        }

        /// <summary>
        /// Adds a person to a group directly (not via the processor), so a Remove/Update test
        /// has a membership to act on. Uses the group type's first role when no role is given.
        /// </summary>
        private static void AddPersonToGroup( RockContext rockContext, Group group, int personId )
        {
            var role = GroupTypeCache.Get( group.GroupTypeId ).Roles.First();
            AddGroupMemberDirect( rockContext, group.Id, personId, role.Id, GroupMemberStatus.Active );
        }

        /// <summary>
        /// Adds a group member directly with the given role and status.
        /// </summary>
        private static void AddGroupMemberDirect( RockContext rockContext, int groupId, int personId, int groupRoleId, GroupMemberStatus status )
        {
            var member = new GroupMember
            {
                GroupId = groupId,
                PersonId = personId,
                GroupRoleId = groupRoleId,
                GroupMemberStatus = status
            };
            new GroupMemberService( rockContext ).Add( member );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates a new organizational (un-owned) tag that targets Person.
        /// </summary>
        private static Tag CreatePersonTag( RockContext rockContext, string name )
        {
            var tag = new Tag
            {
                Name = name,
                EntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id
            };
            new TagService( rockContext ).Add( tag );
            rockContext.SaveChanges();
            return tag;
        }

        /// <summary>
        /// Counts the tagged items for a tag.
        /// </summary>
        private static int CountTaggedItems( RockContext rockContext, int tagId )
        {
            return new TaggedItemService( rockContext ).Queryable().Count( t => t.TagId == tagId );
        }

        /// <summary>
        /// Tags a person directly (not via the processor), so a Remove/dedup test has a row to
        /// act on.
        /// </summary>
        private static void AddTaggedItemDirect( RockContext rockContext, int tagId, Guid personGuid )
        {
            new TaggedItemService( rockContext ).Add( new TaggedItem
            {
                TagId = tagId,
                EntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id,
                EntityGuid = personGuid
            } );
            rockContext.SaveChanges();
        }

        #endregion Helpers
    }
}
