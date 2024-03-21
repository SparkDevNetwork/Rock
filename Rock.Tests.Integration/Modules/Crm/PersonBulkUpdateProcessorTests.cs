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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Integration.Modules.Core;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    [Ignore( "This seems to be testing duplicate of the original code, which means we are no longer testing the right code. Many tests are failing now." )]
    public class PersonBulkUpdateProcessorTests : DatabaseTestsBase
    {
        private const string AlishaMarblePersonGuid = "69DC0FDC-B451-4303-BD91-EF17C0015D23";
        private const string TedDeckerPersonGuid = "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4";
        private const string BillMarblePersonGuid = "1EA811BB-3118-42D1-B020-32A82BC8081A";

        #region Tests

        #region Person Properties

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void BulkUpdateProcessor_UpdateCampus_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var campusMainId = CampusCache.GetId( TestGuids.Crm.CampusMain.AsGuid() );
            var campusStepId = CampusCache.GetId( TestGuids.Crm.CampusSouth.AsGuid() );

            var qryCampus = personService.Queryable()
                .Where( x => x.PrimaryCampusId == campusMainId )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryCampus,
                PersonBulkUpdateProcessor.FieldNames.Campus,
                campusMainId,
                campusStepId );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateCommunicationPreference_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryPreference = personService.Queryable()
                .Where( x => x.CommunicationPreference == CommunicationType.Email )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryPreference,
                PersonBulkUpdateProcessor.FieldNames.CommunicationPreference,
                CommunicationType.Email,
                CommunicationType.PushNotification );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateEmailPreference_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryPreference = personService.Queryable()
                .Where( x => x.EmailPreference == EmailPreference.EmailAllowed )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryPreference,
                PersonBulkUpdateProcessor.FieldNames.EmailPreference,
                EmailPreference.EmailAllowed,
                EmailPreference.DoNotEmail );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateEmailActive_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryInactiveEmails = personService.Queryable()
                .Where( x => x.IsEmailActive == true )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryInactiveEmails,
                PersonBulkUpdateProcessor.FieldNames.EmailIsActive,
                true,
                false );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateEmailNote_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryNotes = personService.Queryable()
                .Where( x => x.EmailNote == null )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryNotes,
                PersonBulkUpdateProcessor.FieldNames.EmailNote,
                null,
                "Test Note" );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateGender_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryMales = personService.Queryable()
                .Where( x => x.Gender == Gender.Female )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryMales,
                PersonBulkUpdateProcessor.FieldNames.Gender,
                Gender.Female,
                Gender.Male );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateGraduationYear_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            // Get a set of candidates that includes Ben Jones (graduating in 2030).
            var qryGraduationYear = personService.Queryable()
                .Where( x => x.GraduationYear == 2030 )
                .OrderBy( x => x.Id )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryGraduationYear,
                PersonBulkUpdateProcessor.FieldNames.GraduationYear,
                2030,
                2021 );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateRecordStatus_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var statusActiveId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;
            var statusPendingId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING ).Id;

            var qryStatus = personService.Queryable()
                .Where( x => x.RecordStatusValueId == statusActiveId )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryStatus,
                PersonBulkUpdateProcessor.FieldNames.RecordStatus,
                statusActiveId,
                statusPendingId );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateReviewReasonNote_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryNotes = personService.Queryable()
                .Where( x => x.ReviewReasonNote == null )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryNotes,
                PersonBulkUpdateProcessor.FieldNames.ReviewReasonNote,
                null,
                "Test Review Note" );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateMaritalStatus_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var statusSingleId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE ).Id;
            var statusMarriedId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id;

            var qryStatus = personService.Queryable()
                .Where( x => x.MaritalStatusValueId == statusSingleId )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryStatus,
                PersonBulkUpdateProcessor.FieldNames.MaritalStatus,
                statusSingleId,
                statusMarriedId );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateReviewReason_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var statusSelfInactivated = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED ).Id;

            var qryStatus = personService.Queryable()
                .Where( x => x.ReviewReasonValueId == null )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryStatus,
                PersonBulkUpdateProcessor.FieldNames.ReviewReason,
                null,
                statusSelfInactivated );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateSuffix_UpdatesSelectedRecords()
        {
            var personSuffixJrGuid = "0D0081EB-2017-43A7-81D6-AFB3A2A1EB35";

            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var suffixJrId = DefinedValueCache.Get( personSuffixJrGuid ).Id;

            var qrySuffix = personService.Queryable()
                .Where( x => x.SuffixValueId == null )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qrySuffix,
                PersonBulkUpdateProcessor.FieldNames.Suffix,
                null,
                suffixJrId );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateSystemNote_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var qryNotes = personService.Queryable()
                .Where( x => x.SystemNote == null )
                .Take( 100 );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryNotes,
                PersonBulkUpdateProcessor.FieldNames.SystemNote,
                null,
                "Test System Note" );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdateTitle_UpdatesSelectedRecords()
        {
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            // The sample data does not include Title assignments, so assign some.
            var titleType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_TITLE );
            var titleMrId = titleType.DefinedValues
                .Where( v => v.Value == "Mr." )
                .Select( v => v.Id )
                .FirstOrDefault();
            var titleMrsId = titleType.DefinedValues
                .Where( v => v.Value == "Mrs." )
                .Select( v => v.Id )
                .FirstOrDefault();

            var personGuidList = new List<Guid>
            {
                TestGuids.TestPeople.BenJones.AsGuid(),
                TestGuids.TestPeople.BillMarble.AsGuid(),
                TestGuids.TestPeople.SamHanks.AsGuid(),
                TestGuids.TestPeople.TedDecker.AsGuid()
            };

            var candidateList = personService.Queryable()
                .Where( p => personGuidList.Contains( p.Guid ) )
                .ToList();

            candidateList.ForEach( x => { x.TitleValueId = titleMrId; } );

            dataContext.SaveChanges();

            var qryTitle = personService.Queryable()
                .Where( x => x.TitleValueId == titleMrId );

            BulkUpdateProcessor_UpdateAndRevertPersonProperty(
                qryTitle,
                PersonBulkUpdateProcessor.FieldNames.Title,
                titleMrId,
                titleMrsId );
        }

        [TestMethod]
        public void BulkUpdateProcessor_UpdatePersonAttributeAllergy_UpdatesSelectedRecords()
        {
            var attributeAllergy = AttributeCache.Get( SystemGuid.Attribute.PERSON_ALLERGY );
            var categoryChildhoodInformationGuid = "752DC692-836E-4A3E-B670-4325CD7724BF";

            var allergyText = "Broccoli and peas!";

            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            // Get the set of people that currently have the Attribute Set to this value.
            var qryCandidates = personService.Queryable()
                .OrderBy( x => x.Id )
                .Take( 100 );

            var processor = GetDefaultProcessor();

            processor.PersonAttributeCategories.Add( categoryChildhoodInformationGuid.AsGuid() );

            BulkUpdateProcessor_UpdateAndRevertPersonAttribute(
                qryCandidates,
                dataContext,
                attributeAllergy.Key,
                null,
                allergyText );
        }

        /// <summary>
        /// Bulk updates the specified person property for the set of people in the supplied query to a new value, and then reverses the process to restore the original value.
        /// </summary>
        /// <param name="personQuery"></param>
        /// <param name="fieldCode"></param>
        /// <param name="originalValue"></param>
        /// <param name="updatedValue"></param>
        private void BulkUpdateProcessor_UpdateAndRevertPersonProperty( IQueryable<Person> personQuery, string fieldCode, object originalValue, object updatedValue )
        {
            // Make sure that the query implements...
            // * NoTracking() so the results are refreshed for each execution.
            // * OrderBy so that the result sets are predictable.
            personQuery = personQuery.AsNoTracking();

            if ( !typeof( IOrderedQueryable<Person> ).IsAssignableFrom( personQuery.Expression.Type ) )
            {
                throw new Exception( "An ordered query is required." );
            }

            var processor = GetDefaultProcessor();

            // Get the list of target people.
            var beforeUpdatePersonIdList = personQuery.Select( x => x.Id ).ToList();

            Assert.IsTrue( beforeUpdatePersonIdList.Any(), "No target people found" );

            Trace.WriteLine( $"{beforeUpdatePersonIdList.Count} target records found." );

            processor.PersonIdList = beforeUpdatePersonIdList;

            // Set property value to updated value and verify.
            processor.SetNewFieldValue( fieldCode, updatedValue );

            processor.Process();

            // Verify that none of the processed records have the old value.
            var afterUpdatePersonIdList = personQuery.Where( x => beforeUpdatePersonIdList.Contains( x.Id ) ).Select( x => x.Id ).ToList();

            Assert.IsFalse( afterUpdatePersonIdList.Any(), "One or more target records were not updated." );

            // Set the property to the original value and verify.
            processor.SetNewFieldValue( fieldCode, originalValue );

            processor.Process();

            afterUpdatePersonIdList = personQuery.Select( x => x.Id ).ToList();

            // Verify that we have the same candidate records as we started with, else the bulk update has not correctly reverted to the original value.
            CollectionAssert.AreEquivalent( beforeUpdatePersonIdList, afterUpdatePersonIdList );
        }

        /// <summary>
        /// Bulk updates the specified person property for the set of people in the supplied query to a new value, and then reverses the process to restore the original value.
        /// </summary>
        /// <param name="personQuery"></param>
        /// <param name="fieldCode"></param>
        /// <param name="originalValue"></param>
        /// <param name="updatedValue"></param>
        private void BulkUpdateProcessor_UpdateAndRevertPersonAttribute( IQueryable<Person> personQuery, RockContext dataContext, string attributeKey, string originalValue, string updatedValue )
        {
            // Make sure that the query implements...
            // * NoTracking() so the results are refreshed for each execution.
            // * OrderBy so that the result sets are predictable.
            personQuery = personQuery.AsNoTracking();

            if ( !typeof( IOrderedQueryable<Person> ).IsAssignableFrom( personQuery.Expression.Type ) )
            {
                throw new Exception( "An ordered query is required." );
            }

            var processor = GetDefaultProcessor();

            var person = new Person();

            person.LoadAttributes();

            var attributeCategory = person.Attributes[attributeKey].Categories.FirstOrDefault();

            processor.PersonAttributeCategories.Add( attributeCategory.Guid );

            // Get the list of target people.
            var targetPersonIdList = personQuery.Select( x => x.Id ).ToList();

            Assert.IsTrue( targetPersonIdList.Any(), "No target people found" );

            Trace.WriteLine( $"{targetPersonIdList.Count} target records found." );

            processor.PersonIdList = targetPersonIdList;

            // Set property value to updated value and verify.
            processor.UpdatePersonAttributeValues.AddOrReplace( attributeKey, updatedValue.ToStringSafe() );

            processor.Process();

            // Verify that none of the processed records have the old value.
            // Note that this does not work if the value is empty because the WhereAttributeValue predicate (incorrectly) returns no records.

            List<int> afterUpdatePersonIdList;

            if ( originalValue != null )
            {
                var qryCandidatesWithOriginalValue = personQuery
                        .Where( x => targetPersonIdList.Contains( x.Id ) )
                        .WhereAttributeValue( dataContext, attributeKey, originalValue )
                        .OrderBy( x => x.Id );

                afterUpdatePersonIdList = qryCandidatesWithOriginalValue.Select( x => x.Id ).ToList();

                Assert.IsFalse( qryCandidatesWithOriginalValue.Any(), "One or more target records were not updated." );
            }

            // Verify that all of the processed records have the new value.
            var qryCandidatesWithUpdatedValue = personQuery
                    .Where( x => targetPersonIdList.Contains( x.Id ) )
                    .WhereAttributeValue( dataContext, attributeKey, updatedValue )
                    .OrderBy( x => x.Id );

            afterUpdatePersonIdList = qryCandidatesWithUpdatedValue.Select( x => x.Id ).ToList();

            CollectionAssert.AreEquivalent( targetPersonIdList, afterUpdatePersonIdList );

            // Set the property back to the original value and verify.
            processor.UpdatePersonAttributeValues.AddOrReplace( attributeKey, originalValue.ToStringSafe() );

            processor.Process();

            // Verify that none of the target records now have the updated value.
            afterUpdatePersonIdList = qryCandidatesWithUpdatedValue.Select( x => x.Id ).ToList();

            Assert.IsFalse( afterUpdatePersonIdList.Any(), "Some target records were not reverted to original values." );
        }

        #endregion

        #region Following

        [TestMethod]
        public void BulkUpdateProcessor_AddRemoveFollowing_CanRoundtrip()
        {
            // Developer settings.
            const bool addData = true;
            const bool removeData = true;

            var rockContext = new RockContext();

            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );

            int personAliasEntityTypeId = personAliasEntityType.Id;

            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var followingService = new FollowingService( rockContext );

            var followedPerson = personService.Get( AlishaMarblePersonGuid.AsGuid() );

            var paQry = personAliasService.Queryable();

            var currentFollowerPersonQuery = followingService.Queryable()
                .AsNoTracking()
                .Where( f => f.EntityTypeId == personAliasEntityTypeId
                             && f.PersonAlias.Id == followedPerson.PrimaryAliasId )
                .Join( paQry, f => f.EntityId, p => p.Id, ( f, p ) => new { PersonAlias = p } )
                .Select( p => p.PersonAlias.PersonId )
                .Distinct();

            var initialFollowerPersonIdList = currentFollowerPersonQuery.ToList();

            var candidatePersonQuery = personService.Queryable()
                .AsNoTracking()
                .Where( x => x.Gender == Gender.Female )
                .Where( x => !initialFollowerPersonIdList.Contains( x.Id ) )
                .OrderBy( x => x.Id )
                .Take( 100 );

            var candidatePersonIdList = candidatePersonQuery.Select( x => x.Id ).ToList();

            // Initialize the processor
            List<int> afterUpdatePersonIdList;

            var processor = GetDefaultProcessor();

            processor.UpdateFollowingPersonId = followedPerson.Id;
            processor.PersonIdList = candidatePersonIdList;

            // Add new followers and verify.
            if ( addData )
            {
                processor.UpdateFollowingAction = PersonBulkUpdateProcessor.FollowingChangeActionSpecifier.Add;

                processor.Process();

                // Verify that all of the candidate people are now followers.
                afterUpdatePersonIdList = currentFollowerPersonQuery.ToList();

                CollectionAssert.IsSubsetOf( candidatePersonIdList, afterUpdatePersonIdList, "One or more candidates are not followers of the target." );
            }

            // Remove new followers and verify.
            if ( removeData )
            {
                processor.UpdateFollowingAction = PersonBulkUpdateProcessor.FollowingChangeActionSpecifier.Remove;

                processor.Process();

                // Verify that the followers have reverted to the initial set.
                afterUpdatePersonIdList = currentFollowerPersonQuery.ToList();

                CollectionAssert.AreEquivalent( initialFollowerPersonIdList, afterUpdatePersonIdList, "Some candidates were not removed as followers." );
            }
        }

        [TestMethod]
        public void BulkUpdateProcessor_AddRemoveTags_CanRoundtrip()
        {
            // Developer settings.
            const bool addData = true;
            const bool removeData = true;
            const int candidateCount = 100;

            const string TagStaff = "0E70E933-F07A-4DA2-B696-368378E8F6FE";

            var rockContext = new RockContext();

            var personEntityType = EntityTypeCache.Get( typeof( Rock.Model.Person ) );

            var tagService = new TagService( rockContext );

            var tagStaff = tagService.Get( TagStaff.AsGuid() );

            var taggedItemService = new TaggedItemService( rockContext );

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable();

            var currentTaggedPersonQuery = taggedItemService.Queryable()
                .AsNoTracking()
                .Where( x => x.TagId == tagStaff.Id )
                .Join( personQuery, f => f.EntityGuid, p => p.Guid, ( f, p ) => new { Person = p } )
                .Select( p => p.Person.Id )
                .Distinct();

            var initialTaggedPersonIdList = currentTaggedPersonQuery.ToList();

            var candidatePersonQuery = personService.Queryable()
                .AsNoTracking()
                .Where( x => x.Gender == Gender.Female )
                .Where( x => !initialTaggedPersonIdList.Contains( x.Id ) )
                .OrderBy( x => x.Id )
                .Take( candidateCount );

            var candidatePersonIdList = candidatePersonQuery.Select( x => x.Id ).ToList();

            // Initialize the processor
            List<int> afterUpdatePersonIdList;

            var processor = GetDefaultProcessor();

            processor.UpdateTagId = tagStaff.Id;
            processor.PersonIdList = candidatePersonIdList;

            // Add new tag and verify.
            if ( addData )
            {
                processor.UpdateTagAction = PersonBulkUpdateProcessor.TagChangeActionSpecifier.Add;
                processor.InstanceId = "Add_Tags";

                processor.Process();

                // Verify that all of the candidate people are now tagged.
                afterUpdatePersonIdList = currentTaggedPersonQuery.ToList();

                CollectionAssert.IsSubsetOf( candidatePersonIdList, afterUpdatePersonIdList, "One or more candidates are not tagged." );
            }

            // Remove new tag and verify.
            if ( removeData )
            {
                processor.UpdateTagAction = PersonBulkUpdateProcessor.TagChangeActionSpecifier.Remove;
                processor.InstanceId = "Remove_Tags";

                processor.Process();

                // Verify that the set of people tagged as "Staff" have reverted to the initial set.
                afterUpdatePersonIdList = currentTaggedPersonQuery.ToList();

                CollectionAssert.AreEquivalent( initialTaggedPersonIdList, afterUpdatePersonIdList, "Some candidates were not untagged." );
            }
        }

        [TestMethod]
        public void BulkUpdateProcessor_AddNote_Succeeds()
        {
            // Developer settings.
            const int candidateCount = 100;

            const string NoteTypePersonal = "66A1B9D7-7EFA-40F3-9415-E54437977D60";

            var TestNoteText = string.Format( "Test Note ({0})", Guid.NewGuid() );

            var rockContext = new RockContext();

            var personEntityType = EntityTypeCache.Get( typeof( Rock.Model.Person ) );

            var noteTypeService = new NoteTypeService( rockContext );

            var noteTypePersonal = noteTypeService.Get( NoteTypePersonal.AsGuid() );

            var noteService = new NoteService( rockContext );

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable();

            var currentNotedPersonQuery = noteService.Queryable()
                .AsNoTracking()
                .Where( x => x.NoteTypeId == noteTypePersonal.Id )
                .Where( x => x.Text == TestNoteText )
                .Select( p => p.EntityId ?? 0 );

            var initialNotedPersonIdList = currentNotedPersonQuery.ToList();

            var candidatePersonQuery = personService.Queryable()
                .AsNoTracking()
                .Where( x => !initialNotedPersonIdList.Contains( x.Id ) )
                .OrderBy( x => x.Id )
                .Take( candidateCount );

            var candidatePersonIdList = candidatePersonQuery.Select( x => x.Id ).ToList();

            // Initialize the processor
            List<int> afterUpdatePersonIdList;

            var processor = GetDefaultProcessor();

            processor.InstanceId = "Add_Notes";
            processor.UpdateNoteText = TestNoteText;
            processor.UpdateNoteIsAlert = true;
            processor.UpdateNoteIsPrivate = true;
            processor.UpdateNoteTypeId = noteTypePersonal.Id;

            processor.PersonIdList = candidatePersonIdList;

            // Add new tag and verify.
            processor.UpdateNoteAction = PersonBulkUpdateProcessor.NoteChangeActionSpecifier.Add;

            processor.Process();

            // Verify that all of the candidate people are now tagged.
            afterUpdatePersonIdList = currentNotedPersonQuery.ToList();

            CollectionAssert.IsSubsetOf( candidatePersonIdList, afterUpdatePersonIdList, "One or more candidates are not tagged." );
        }

        #endregion

        #region Groups

        private static string TestGuidGroup = "FBB97E7D-C880-4067-8AFF-8D160EE9169D";

        [TestMethod]
        public void BulkUpdateProcessor_AddModifyRemoveGroupMembership_Succeeds()
        {
            const int candidateCount = 100;

            // Make sure that the query implements...
            // * NoTracking() so the results are refreshed for each execution.
            // * OrderBy so that the result sets are predictable.
            var dataContext = new RockContext();
            var groupService = new GroupService( dataContext );
            var groupTypeService = new GroupTypeService( dataContext );

            var groupTest = groupService.Get( TestGuidGroup.AsGuid() );
            if ( groupTest != null )
            {
                groupService.Delete( groupTest );
                dataContext.SaveChanges();
            }

            groupTest = new Group();
            groupTest.Guid = TestGuidGroup.AsGuid();
            groupTest.Name = $"Test Group {TestGuidGroup}";

            var groupTypeGeneral = groupTypeService.Get( SystemGuid.GroupType.GROUPTYPE_GENERAL.AsGuid() );
            groupTest.GroupType = groupTypeGeneral;

            groupService.Add( groupTest );

            dataContext.SaveChanges();

            var groupMemberService = new GroupMemberService( dataContext );

            // Get the list of current group members.
            var currentMembersQuery = groupMemberService.Queryable()
                .AsNoTracking()
                .Where( x => x.GroupId == groupTest.Id )
                .Select( x => x.PersonId );

            var roleMemberId = groupTest.GroupType.Roles
                .Where( x => x.Name == "Member" )
                .Select( r => r.Id )
                .FirstOrDefault();

            var personService = new PersonService( dataContext );

            var existingMembersIdList = currentMembersQuery.ToList();

            // Get the list of candidate group members.
            var candidatePersonQuery = personService.Queryable()
                    .AsNoTracking()
                    .Where( x => x.Gender == Gender.Female )
                    .Where( x => !existingMembersIdList.Contains( x.Id ) )
                    .Take( candidateCount );

            var candidatePersonIdList = candidatePersonQuery.Select( x => x.Id ).ToList();

            Assert.IsTrue( candidatePersonIdList.Any(), "No target people found" );

            // Update 
            var processor = GetDefaultProcessor();

            processor.UpdateGroupId = groupTest.Id;
            processor.PersonIdList = candidatePersonIdList;

            // Set property value to updated value and verify.
            processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Add;
            processor.UpdateGroupRoleId = roleMemberId;
            processor.UpdateGroupStatus = GroupMemberStatus.Active;

            processor.Process();

            // Verify that all of the new members have been added.
            var afterUpdatePersonIdList = currentMembersQuery.Where( x => candidatePersonIdList.Contains( x ) ).ToList();

            CollectionAssert.AreEquivalent( candidatePersonIdList, afterUpdatePersonIdList, "One or more target records were not added." );

            // Update the new member records
            processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Modify;
            processor.UpdateGroupStatus = GroupMemberStatus.Inactive;
            processor.SelectedFields.Add( PersonBulkUpdateProcessor.FieldNames.GroupMemberStatus );

            processor.Process();

            // Verify that all the candidate members have been updated.
            var notUpdatedCandidateMembersQuery = groupMemberService.Queryable()
                    .AsNoTracking()
                    .Where( x => x.GroupId == groupTest.Id && x.GroupMemberStatus == GroupMemberStatus.Active )
                    .Where( x => candidatePersonIdList.Contains( x.PersonId ) );

            Assert.IsFalse( notUpdatedCandidateMembersQuery.Any(), "One or more target records were not updated." );

            // Remove the members
            processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Remove;

            processor.Process();

            // Verify that we have the same candidate records as we started with, else the bulk update has not correctly reverted to the original value.
            afterUpdatePersonIdList = currentMembersQuery.ToList();

            CollectionAssert.AreEquivalent( existingMembersIdList, afterUpdatePersonIdList );
        }

        #endregion

        #region Multi-threading

        [TestMethod]
        public void BulkUpdateProcessor_MultithreadingEnabled_ProcessesSuccessfully()
        {
            const int candidateCount = 101;

            // Make sure that the query implements...
            // * NoTracking() so the results are refreshed for each execution.
            // * OrderBy so that the result sets are predictable.
            var dataContext = new RockContext();

            var groupService = new GroupService( dataContext );
            var groupTest = groupService.Get( TestGuidGroup.AsGuid() );

            var groupMemberService = new GroupMemberService( dataContext );

            // Get the list of current group members.
            var currentMembersQuery = groupMemberService.Queryable()
                .AsNoTracking()
                .Where( x => x.GroupId == groupTest.Id )
                .OrderBy( x => x.Id )
                .Select( x => x.PersonId );

            var roleMember = groupTest.GroupType.Roles.First( x => x.Name == "Member" );

            var personService = new PersonService( dataContext );

            var existingMembersIdList = currentMembersQuery.ToList();

            // Get the list of candidate group members.
            var candidatePersonQuery = personService.Queryable()
                    .AsNoTracking()
                    .Where( x => x.Gender == Gender.Female )
                    .Where( x => !existingMembersIdList.Contains( x.Id ) )
                    .OrderBy( x => x.Id )
                    .Take( candidateCount );

            var candidatePersonIdList = candidatePersonQuery.Select( x => x.Id ).ToList();

            Assert.IsTrue( candidatePersonIdList.Any(), "No target people found" );

            // Add new members and verify.
            var processor = GetDefaultProcessor();

            processor.TaskCount = 4;

            processor.UpdateGroupId = groupTest.Id;
            processor.PersonIdList = candidatePersonIdList;

            processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Add;
            processor.UpdateGroupRoleId = roleMember.Id;
            processor.UpdateGroupStatus = GroupMemberStatus.Active;

            processor.Process();

            var afterUpdatePersonIdList = currentMembersQuery.Where( x => candidatePersonIdList.Contains( x ) ).ToList();

            CollectionAssert.AreEquivalent( candidatePersonIdList, afterUpdatePersonIdList, "One or more target records were not added." );

            // Remove the members
            processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Remove;

            processor.Process();

            // Verify that we have the same candidate records as we started with, else the bulk update has not correctly reverted to the original value.
            afterUpdatePersonIdList = currentMembersQuery.ToList();

            CollectionAssert.AreEquivalent( existingMembersIdList, afterUpdatePersonIdList );
        }

        #endregion

        #region Support functions

        private List<Guid> GetWellKnownPeopleGuidList()
        {
            var personGuidList = new List<Guid>();

            personGuidList.Add( TedDeckerPersonGuid.AsGuid() );
            personGuidList.Add( BillMarblePersonGuid.AsGuid() );

            return personGuidList;
        }

        private PersonBulkUpdateProcessor GetDefaultProcessor()
        {
            var processor = new PersonBulkUpdateProcessor();

            processor.TaskCount = 4;

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            processor.CurrentPerson = new CoreModuleTestHelper( "" ).GetAdminPersonOrThrow( dataContext );

            // Add some well-known people.
            var personGuidList = GetWellKnownPeopleGuidList();

            var personIdQuery = personService.Queryable().Where( x => personGuidList.Contains( x.Guid ) ).Select( x => x.Id );

            processor.PersonIdList.AddRange( personIdQuery );

            // Add some random people for bulk
            personIdQuery = personService.Queryable().Take( 1000 ).Select( x => x.Id );

            processor.PersonIdList.AddRange( personIdQuery );

            processor.StatusUpdated += ( s, e ) =>
            {
                if ( e.UpdateType == PersonBulkUpdateProcessor.ProcessorStatusUpdateTypeSpecifier.Progress )
                {
                    Trace.WriteLine( $"[{e.StatusDateTime}] {e.ProcessedCount} of {e.TotalCount} completed: {e.StatusMessage} [{e.StatusDetail}]" );
                }
                else
                {
                    Trace.WriteLine( $"[{e.StatusDateTime}] {e.StatusMessage} [{e.StatusDetail}]" );
                }
            };

            return processor;
        }

        #endregion

        #endregion

        #region Test Class

        /* [2020-04-29] DL
         * This is the class that is the target of these integration tests.
         * It is deployed as a private support class in the BulkUpdate block (v10.3) to allow file-copy replacement of the existing block.
         * As a class in a web project, it can't be instantiated or referenced in this test project directly, hence the need to create this copy for testing purposes.
         * To run these integration tests correctly, ensure that you have copied/pasted the most recent version of the PersonBulkUpdateProcessor class from the BulkUpdate.ascx file.
         * Any changes made to this class during unit testing should be copied/pasted back into the BulkUpdate block.
         */

        #region Bulk Update Processor

        /// <summary>
        /// Processes bulk update actions for a set of Person records.
        /// </summary>
        /// <remarks>
        /// Last Updated: [2020-05-01] DL
        /// </remarks>
        private class PersonBulkUpdateProcessor
        {
            #region Enumerations and Constants

            public static class FieldNames
            {
                public const string Title = "dvpTitle";
                public const string Suffix = "dvpSuffix";
                public const string ConnectionStatus = "dvpConnectionStatus";
                public const string RecordStatus = "dvpRecordStatus";
                public const string Gender = "ddlGender";
                public const string GroupRole = "ddlGroupRole";
                public const string MaritalStatus = "dvpMaritalStatus";
                public const string GraduationYear = "ddlGradePicker";
                public const string EmailIsActive = "ddlIsEmailActive";
                public const string CommunicationPreference = "ddlCommunicationPreference";
                public const string EmailPreference = "ddlEmailPreference";
                public const string GroupMemberStatus = "ddlGroupMemberStatus";
                public const string EmailNote = "tbEmailNote";
                public const string SystemNote = "tbSystemNote";
                public const string ReviewReason = "dvpReviewReason";
                public const string ReviewReasonNote = "tbReviewReasonNote";
                public const string Campus = "cpCampus";
            }

            public enum ProcessorStatusUpdateTypeSpecifier
            {
                Progress = 0,
                Info = 1,
                Warning = 2,
                Error = 3,
                Summary = 4
            }

            public enum NoteChangeActionSpecifier
            {
                None = 0,
                Add = 1
            }

            public enum TagChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2
            }

            public enum FollowingChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2
            }

            public enum GroupChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2,
                Modify = 3
            }

            #endregion

            #region Events

            public class ProcessorStatusUpdateEventArgs : EventArgs
            {
                public ProcessorStatusUpdateEventArgs()
                {
                    StatusDateTime = RockDateTime.Now;
                }

                public DateTime StatusDateTime;
                public long ProcessedCount;
                public long TotalCount;
                public long ErrorCount;

                public string StatusMessage;
                public string StatusDetail;

                public ProcessorStatusUpdateTypeSpecifier UpdateType = ProcessorStatusUpdateTypeSpecifier.Progress;
            }

            public event EventHandler<ProcessorStatusUpdateEventArgs> StatusUpdated;

            #endregion

            #region Constructors

            public PersonBulkUpdateProcessor()
            {
                this.SelectedFields = new List<string>();
                this.PersonIdList = new List<int>();
                this.PersonAttributeCategories = new List<Guid>();

                UpdatePersonAttributeValues = new Dictionary<string, string>();
                UpdateGroupAttributeValues = new Dictionary<string, string>();

                UpdateGroupAction = GroupChangeActionSpecifier.None;
                UpdateFollowingAction = FollowingChangeActionSpecifier.None;
                UpdateTagAction = TagChangeActionSpecifier.None;
                UpdateNoteAction = NoteChangeActionSpecifier.None;
            }

            #endregion

            #region Fields and Properties

            private readonly static TraceSource _tracer = new TraceSource( "Rock.Crm.BulkUpdate" );

            private int _currentPersonAliasId;
            private Person _currentPerson = null;

            private long _errorCount;
            private int _totalCount = 0;
            private int _processedCount = 0;
            private DateTime? _lastNotified = null;
            private decimal? _lastCompletionPercentage;

            // The number of CPU threads that can be used to process the updates.
            public int TaskCount { get; set; }

            /// <summary>
            /// The maximum size of a processing batch size formaximum number work items assigned to each task.
            /// </summary>
            public int BatchSize { get; set; }


            /// <summary>
            /// The list of unique identifiers for the people who will be targeted by the bulk update operation.
            /// </summary>
            public List<int> PersonIdList { get; set; }

            public List<string> SelectedFields { get; set; }
            public List<Guid> PersonAttributeCategories { get; set; }

            public Person CurrentPerson
            {
                get
                {
                    return _currentPerson;
                }

                set
                {
                    _currentPerson = value;

                    _currentPersonAliasId = ( _currentPerson == null ? 0 : _currentPerson.PrimaryAliasId ?? 0 );
                }
            }

            /// <summary>
            /// Gets a unique identifier for this instance of the processer that can be used for trace and diagnostic purposes.
            /// </summary>
            public string InstanceId { get; set; }

            /// <summary>
            /// The minimum time (measured in ms) that must elapse between subsequent progress notifications.
            /// </summary>
            public int NotificationPeriod { get; set; }

            #region Bulk Update Values

            public int? UpdateTitleValueId { get; set; }
            public int? UpdateSuffixValueId { get; set; }
            public int? UpdateConnectionStatusValueId { get; set; }
            public int? UpdateRecordStatusValueId { get; set; }
            public int? UpdateInactiveReasonId { get; set; }
            public string UpdateInactiveReasonNote { get; set; }
            public Gender UpdateGender { get; set; }
            public int? UpdateMaritalStatusValueId { get; set; }
            public int? UpdateGraduationYear { get; set; }
            public int? UpdateCampusId { get; set; }
            public bool UpdateEmailActive { get; set; }
            public CommunicationType? UpdateCommunicationPreference { get; set; }
            public EmailPreference? UpdateEmailPreference { get; set; }
            public string UpdateEmailNote { get; set; }
            public int? UpdateReviewReasonValueId { get; set; }
            public string UpdateSystemNote { get; set; }
            public string UpdateReviewReasonNote { get; set; }
            public Dictionary<string, string> UpdatePersonAttributeValues { get; set; }
            public NoteChangeActionSpecifier UpdateNoteAction { get; set; }
            public string UpdateNoteText { get; set; }
            public bool UpdateNoteIsAlert { get; set; }
            public bool UpdateNoteIsPrivate { get; set; }
            public int? UpdateNoteTypeId { get; set; }
            public GroupChangeActionSpecifier UpdateGroupAction { get; set; }
            public int? UpdateGroupId { get; set; }
            public int? UpdateGroupRoleId { get; set; }
            public GroupMemberStatus UpdateGroupStatus { get; set; }
            public Dictionary<string, string> UpdateGroupAttributeValues { get; set; }

            public FollowingChangeActionSpecifier UpdateFollowingAction { get; set; }

            /// <summary>
            /// The identifier of the Person who will be added as the Following target.
            /// </summary>
            public int? UpdateFollowingPersonId { get; set; }

            public TagChangeActionSpecifier UpdateTagAction { get; set; }
            public int UpdateTagId { get; set; }

            #endregion

            /// <summary>
            /// A list of identifiers of workflows that should be executed for each person after the update.
            /// </summary>
            public List<string> PostUpdateWorkflowIdList { get; set; }

            /// <summary>
            /// Set an update value for the specified field.
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="newValue"></param>
            public void SetNewFieldValue( string fieldName, object newValue )
            {
                switch ( fieldName )
                {
                    case FieldNames.Campus:
                        this.UpdateCampusId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.CommunicationPreference:
                        this.UpdateCommunicationPreference = newValue.ToStringSafe().ConvertToEnum<CommunicationType>( null );
                        break;
                    case FieldNames.ConnectionStatus:
                        this.UpdateConnectionStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.EmailPreference:
                        this.UpdateEmailPreference = newValue.ToStringSafe().ConvertToEnum<EmailPreference>( null );
                        break;
                    case FieldNames.EmailIsActive:
                        this.UpdateEmailActive = newValue.ToStringSafe().AsBoolean();
                        break;
                    case FieldNames.EmailNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateEmailNote = newValue as string;
                        break;
                    case FieldNames.Gender:
                        this.UpdateGender = newValue.ToStringSafe().ConvertToEnum<Gender>( null );
                        break;
                    case FieldNames.GraduationYear:
                        this.UpdateGraduationYear = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.MaritalStatus:
                        this.UpdateMaritalStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.RecordStatus:
                        this.UpdateRecordStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.ReviewReason:
                        this.UpdateReviewReasonValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.ReviewReasonNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateReviewReasonNote = newValue as string;
                        break;
                    case FieldNames.Suffix:
                        this.UpdateSuffixValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.SystemNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateSystemNote = newValue as string;
                        break;

                    case FieldNames.Title:
                        this.UpdateTitleValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;

                    case FieldNames.GroupRole:
                    case FieldNames.GroupMemberStatus:

                    default:
                        throw new Exception( string.Format( "Invalid field name. The field \"{0}\" cannot be resolved.", fieldName ) );
                }

                if ( !this.SelectedFields.Contains( fieldName ) )
                {
                    this.SelectedFields.Add( fieldName );
                }
            }

            #endregion

            #region Trace Output

            private void TraceInformation( string message, params object[] args )
            {
                TraceEvent( TraceEventType.Information, message, args );
            }

            private void TraceVerbose( string message, params object[] args )
            {
                TraceEvent( TraceEventType.Verbose, message, args );
            }

            private void TraceEvent( TraceEventType eventType, string message, params object[] args )
            {
                string prefix;

                if ( this.TaskCount == 1 )
                {
                    prefix = string.Format( "InstanceId={0}", this.InstanceId );
                }
                else
                {
                    prefix = string.Format( "InstanceId={0}, ThreadId={1}", this.InstanceId, Thread.CurrentThread.ManagedThreadId );
                }

                message = string.Format( "{0} || {1}", prefix, string.Format( message, args ) );

                _tracer.TraceEvent( eventType, 0, message );
            }

            #endregion

            #region Status Reporting

            private void SetTaskProgress( int completedCount, int totalCount, string statusMessage = null, string statusDetail = null )
            {
                lock ( _processingQueueLocker )
                {
                    _processedCount = completedCount;
                    _totalCount = totalCount;
                }

                var currentCompletionPercentage = decimal.Divide( _processedCount, _totalCount ) * 100;

                // Only send a progress notification if this is the first update, or work has been completed
                // and the elapsed time is sufficient to warrant an update.
                if ( _lastNotified.HasValue )
                {
                    var timeDiff = RockDateTime.Now - _lastNotified.Value;

                    if ( NotificationPeriod > 0
                         && timeDiff.TotalMilliseconds < NotificationPeriod )
                    {
                        return;
                    }

                    if ( currentCompletionPercentage == _lastCompletionPercentage )
                    {
                        return;
                    }
                }

                TraceVerbose( "Status Update Notification. [Processed={0} of {1}, Errors={2}, Message={3}]", _processedCount, _totalCount, _errorCount, statusMessage );

                _lastNotified = RockDateTime.Now;
                _lastCompletionPercentage = currentCompletionPercentage;

                if ( StatusUpdated != null )
                {
                    var args = new ProcessorStatusUpdateEventArgs
                    {
                        ProcessedCount = _processedCount,
                        TotalCount = _totalCount,
                        ErrorCount = _errorCount,
                        StatusMessage = statusMessage,
                        StatusDetail = statusDetail,
                        UpdateType = ProcessorStatusUpdateTypeSpecifier.Progress
                    };

                    StatusUpdated.Invoke( this, args );
                }
            }

            private void UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier updateType, string statusMessage = null, string statusDetail = null )
            {
                if ( StatusUpdated != null )
                {
                    var args = new ProcessorStatusUpdateEventArgs
                    {
                        ProcessedCount = _processedCount,
                        TotalCount = _totalCount,
                        ErrorCount = _errorCount,
                        StatusMessage = statusMessage,
                        StatusDetail = statusDetail,
                        UpdateType = updateType
                    };

                    StatusUpdated.Invoke( this, args );
                }
            }

            #endregion

            #region Bulk Update Processing

            private static object _processingQueueLocker = new object();

            /// <summary>
            /// Returns a data context that is associated with the current user for audit purposes.
            /// As a side effect, ensures that the HttpContext contains required information about the current user.
            /// </summary>
            /// <returns></returns>
            private RockContext GetDataContextForCurrentUser()
            {
                /* [2020-05-01] DL
                 * The RockContext determines the current user by retrieving the value of HttpContext.Current.Items["CurrentPerson"].
                 * In this component, the RockContext operates in a background thread that does not have access to the HttpContext of the original request.
                 *
                 * The workaround implemented here ensures that a copy of the original HttpRequest is available to the current thread.
                 * A more robust solution would be to add a RockContext.CurrentPerson property that can be set as an override for instances where data access occurs in absence of a HttpRequest.
                 */

                // Set a fake HttpContext for the current thread if it does not have one, and inject the CurrentPerson.
                if ( HttpContext.Current == null )
                {
                    var request = new HttpRequest( "", "http://localhost", "" );
                    var response = new HttpResponse( new StringWriter() );
                    var testHttpContext = new HttpContext( request, response );

                    testHttpContext.Items["CurrentPerson"] = this.CurrentPerson;

                    HttpContext.Current = testHttpContext;
                }

                return new RockContext();
            }

            /// <summary>
            /// Processes the bulk update.
            /// </summary>
            /// <param name="httpContext"></param>
            /// <param name="httpContextItems"></param>
            /// <param name="instanceId"></param>
            public void Process()
            {
                if ( string.IsNullOrWhiteSpace( this.InstanceId ) )
                {
                    this.InstanceId = Guid.NewGuid().ToString();
                }

                TraceInformation( "Process started." );

                var actionSummary = this.GetPendingActionSummary();

                foreach ( var action in this.GetPendingActionSummary() )
                {
                    TraceInformation( "Pending Action: {0}", action );
                }

                var startTime = RockDateTime.Now;

                ValidateCanProcess();

                var individuals = PersonIdList.ToList();

                _totalCount = individuals.Count;
                _processedCount = 0;
                _errorCount = 0;

                _lastNotified = null;
                _lastCompletionPercentage = null;

                // Determine the number of tasks to use.
                int taskCount = this.TaskCount;

                if ( taskCount > 64 )
                {
                    // Prevent the user from doing too much damage.
                    taskCount = 64;
                }
                else if ( taskCount < 1 )
                {
                    taskCount = Environment.ProcessorCount;
                }

                TraceInformation( "Processing initialized. [ItemCount={0}, MaximumThreadCount={1}, BatchSize={2}]", _totalCount, taskCount, this.BatchSize );

                try
                {
                    string finalStatus = null;
                    bool hasErrors = false;

                    SetTaskProgress( 0, _totalCount );

                    if ( individuals.Any() )
                    {
                        var options = new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = taskCount
                        };

                        // Partition the collection into batches.
                        var batchSize = this.BatchSize;

                        OrderablePartitioner<Tuple<int, int>> partitioner;

                        if ( batchSize == 0 )
                        {
                            // Let the scheduler determine the batch size.
                            partitioner = Partitioner.Create( 0, individuals.Count );
                        }
                        else
                        {
                            partitioner = Partitioner.Create( 0, individuals.Count, batchSize );
                        }

                        var updateExceptions = new ConcurrentQueue<Exception>();

                        Action<Tuple<int, int>, ParallelLoopState> processItemBatchDelegate = ( Tuple<int, int> range, ParallelLoopState loopState ) =>
                        {
                            try
                            {
                                TraceVerbose( "Worker task started." );

                                var firstIndex = range.Item1;
                                var lastIndex = range.Item2 - 1;
                                var itemCount = lastIndex - firstIndex + 1;

                                TraceVerbose( "Processing work items... [From={0}, To={1}, Count={2}]", firstIndex + 1, lastIndex + 1, itemCount );

                                var personIdList = individuals.Skip( firstIndex ).Take( itemCount ).ToList();

                                ProcessIndividuals( personIdList );

                                SetTaskProgress( _processedCount + itemCount, _totalCount );

                                TraceVerbose( "Worker task completed." );
                            }
                            catch ( Exception ex )
                            {
                                updateExceptions.Enqueue( ex );
                            }
                        };

                        var result = Parallel.ForEach( partitioner, options, processItemBatchDelegate );

                        if ( !result.IsCompleted )
                        {
                            finalStatus = string.Join( "<br>", updateExceptions.Select( w => w.Message.EncodeHtml() ) );

                            hasErrors = true;
                        }
                    }

                    SetTaskProgress( _totalCount, _totalCount );

                    var elapsedTime = RockDateTime.Now.Subtract( startTime );

                    if ( hasErrors )
                    {
                        UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Error, finalStatus, "alert-danger" );
                    }
                    else
                    {
                        if ( _errorCount == 0 )
                        {
                            finalStatus = string.Format( "{0} {1} successfully updated. ({2:0.0}s)",
                                PersonIdList.Count().ToString( "N0" ), ( PersonIdList.Count() > 1 ? "people were" : "person was" ),
                                elapsedTime.TotalSeconds );

                            UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Summary, finalStatus.EncodeHtml(), "alert-success" );
                        }
                        else
                        {
                            finalStatus = string.Format( "{0} {1} updated with {2} error(s). Please look in the exception log for more details. ({3:0.0}s)",
                                PersonIdList.Count().ToString( "N0" ), ( PersonIdList.Count() > 1 ? "people were" : "person was" ),
                                _errorCount,
                                elapsedTime.TotalSeconds );

                            UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Warning, finalStatus.EncodeHtml(), "alert-warning" );
                        }
                    }

                    TraceInformation( "Result: {0}", finalStatus );
                }
                catch ( Exception ex )
                {
                    string status = ex.Message;

                    UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Error, status.EncodeHtml(), "error" );
                }

                TraceInformation( "Process completed." );

            }

            private void ValidateCanProcess()
            {
                if ( PersonIdList == null || !PersonIdList.Any() )
                {
                    throw new Exception( "PersonIdList must contain one or more items." );
                }

                bool hasUpdateActions = false;

                hasUpdateActions = hasUpdateActions || ( this.SelectedFields != null && this.SelectedFields.Any() );
                hasUpdateActions = hasUpdateActions || ( this.UpdateGroupAction != GroupChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateFollowingAction != FollowingChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateTagAction != TagChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateNoteAction != NoteChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() );
                hasUpdateActions = hasUpdateActions || ( this.UpdateGroupAttributeValues != null && this.UpdateGroupAttributeValues.Any() );

                if ( !hasUpdateActions )
                {
                    throw new Exception( "SelectedFields must contain one or more items." );
                }

                if ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() )
                {
                    if ( this.PersonAttributeCategories == null || !this.PersonAttributeCategories.Any() )
                    {
                        // This requirement is arbitrary and should be removed.
                        throw new Exception( "PersonAttributeValues filter requires PersonAttributeCategories to be populated with corresponding categories." );
                    }
                }
            }

            /// <summary>
            /// Process the given individuals. This is used to be able to run smaller batches. This provides
            /// a huge boost to performance when dealing with large numbers of people.
            /// </summary>
            /// <param name="personIdList">The list of individuals to process in this batch.</param>
            private void ProcessIndividuals( List<int> personIdList )
            {
                var rockContext = this.GetDataContextForCurrentUser();

                var personService = new PersonService( rockContext );

                var ids = personIdList.ToList();

                #region Individual Details Updates

                int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var people = personService.Queryable( true ).Where( p => ids.Contains( p.Id ) ).ToList();

                foreach ( var person in people )
                {
                    if ( SelectedFields.Contains( FieldNames.Title ) )
                    {
                        person.TitleValueId = UpdateTitleValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.Suffix ) )
                    {
                        person.SuffixValueId = UpdateSuffixValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.ConnectionStatus ) )
                    {
                        person.ConnectionStatusValueId = UpdateConnectionStatusValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.RecordStatus ) )
                    {
                        person.RecordStatusValueId = UpdateRecordStatusValueId;

                        if ( UpdateRecordStatusValueId.HasValue && UpdateRecordStatusValueId.Value == inactiveStatusId )
                        {
                            person.RecordStatusReasonValueId = UpdateInactiveReasonId;

                            if ( !string.IsNullOrWhiteSpace( UpdateInactiveReasonNote ) )
                            {
                                person.InactiveReasonNote = UpdateInactiveReasonNote;
                            }
                        }
                    }

                    if ( SelectedFields.Contains( FieldNames.Gender ) )
                    {
                        person.Gender = UpdateGender;
                    }

                    if ( SelectedFields.Contains( FieldNames.MaritalStatus ) )
                    {
                        person.MaritalStatusValueId = UpdateMaritalStatusValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.GraduationYear ) )
                    {
                        person.GraduationYear = UpdateGraduationYear;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailIsActive ) )
                    {
                        person.IsEmailActive = UpdateEmailActive;
                    }

                    if ( SelectedFields.Contains( FieldNames.CommunicationPreference ) )
                    {
                        person.CommunicationPreference = UpdateCommunicationPreference.Value;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailPreference ) )
                    {
                        person.EmailPreference = UpdateEmailPreference.Value;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailNote ) )
                    {
                        person.EmailNote = UpdateEmailNote;
                    }

                    if ( SelectedFields.Contains( FieldNames.SystemNote ) )
                    {
                        person.SystemNote = UpdateSystemNote;
                    }

                    if ( SelectedFields.Contains( FieldNames.ReviewReason ) )
                    {
                        person.ReviewReasonValueId = UpdateReviewReasonValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.ReviewReasonNote ) )
                    {
                        person.ReviewReasonNote = UpdateReviewReasonNote;
                    }
                }

                if ( SelectedFields.Contains( FieldNames.Campus ) && UpdateCampusId.HasValue )
                {
                    int campusId = UpdateCampusId.Value;

                    Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

                    var familyMembers = new GroupMemberService( rockContext ).Queryable()
                        .Where( m => ids.Contains( m.PersonId ) && m.Group.GroupType.Guid == familyGuid )
                        .Select( m => new { m.PersonId, m.GroupId } )
                        .Distinct()
                        .ToList();

                    var families = new GroupMemberService( rockContext ).Queryable()
                        .Where( m => ids.Contains( m.PersonId ) && m.Group.GroupType.Guid == familyGuid )
                        .Select( m => m.Group )
                        .Distinct()
                        .ToList();

                    foreach ( int personId in ids )
                    {
                        var familyIds = familyMembers.Where( m => m.PersonId == personId ).Select( m => m.GroupId ).ToList();
                        if ( familyIds.Count == 1 )
                        {
                            int familyId = familyIds.FirstOrDefault();
                            var family = families.Where( g => g.Id == familyId ).FirstOrDefault();
                            {
                                if ( family != null )
                                {
                                    family.CampusId = campusId;
                                }
                                familyMembers.RemoveAll( m => m.GroupId == familyId );
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                // Update following
                if ( this.UpdateFollowingAction != FollowingChangeActionSpecifier.None )
                {
                    var personAliasEntityType = EntityTypeCache.Get( "Rock.Model.PersonAlias" );
                    if ( personAliasEntityType != null )
                    {
                        int personAliasEntityTypeId = personAliasEntityType.Id;

                        var personAliasService = new PersonAliasService( rockContext );
                        var followingService = new FollowingService( rockContext );

                        var followedPerson = personService.Get( this.UpdateFollowingPersonId.GetValueOrDefault() );

                        if ( followedPerson == null )
                        {
                            throw new Exception( "A Following Person must be specified." );
                        }

                        var followedPersonAliasId = followedPerson.PrimaryAliasId.GetValueOrDefault();

                        if ( UpdateFollowingAction == FollowingChangeActionSpecifier.Add )
                        {
                            var paQry = personAliasService.Queryable();

                            var alreadyFollowingIds = followingService.Queryable()
                                .Where( f =>
                                    f.EntityTypeId == personAliasEntityTypeId &&
                                    f.PersonAlias.Id == followedPersonAliasId )
                                .Join( paQry, f => f.EntityId, p => p.Id, ( f, p ) => new { PersonAlias = p } )
                                .Select( p => p.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            foreach ( int id in ids.Where( id => !alreadyFollowingIds.Contains( id ) ) )
                            {
                                var person = people.FirstOrDefault( p => p.Id == id );
                                if ( person != null && person.PrimaryAliasId.HasValue )
                                {
                                    var following = new Rock.Model.Following
                                    {
                                        EntityTypeId = personAliasEntityTypeId,
                                        EntityId = person.PrimaryAliasId.Value,
                                        PersonAliasId = followedPersonAliasId
                                    };
                                    followingService.Add( following );
                                }
                            }
                        }
                        else
                        {
                            var paQry = personAliasService.Queryable()
                                .Where( p => ids.Contains( p.PersonId ) )
                                .Select( p => p.Id );

                            foreach ( var following in followingService.Queryable()
                                .Where( f =>
                                    f.EntityTypeId == personAliasEntityTypeId &&
                                    paQry.Contains( f.EntityId ) &&
                                    f.PersonAlias.Id == _currentPersonAliasId ) )
                            {
                                followingService.Delete( following );
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                #endregion

                #region Person Attributes

                if ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() )
                {
                    var selectedCategories = new List<CategoryCache>();

                    foreach ( Guid categoryGuid in PersonAttributeCategories )
                    {
                        var category = CategoryCache.Get( categoryGuid, rockContext );
                        if ( category != null )
                        {
                            selectedCategories.Add( category );
                        }
                    }

                    var attributes = new List<AttributeCache>();
                    var attributeValues = new Dictionary<int, string>();

                    int categoryIndex = 0;
                    foreach ( var category in selectedCategories.OrderBy( c => c.Name ) )
                    {
                        categoryIndex++;

                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );

                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, _currentPerson ) )
                            {
                                var attributeCache = AttributeCache.Get( attribute.Id );

                                if ( this.UpdatePersonAttributeValues.ContainsKey( attribute.Key ) )
                                {
                                    attributes.Add( attributeCache );
                                    attributeValues.Add( attributeCache.Id, this.UpdatePersonAttributeValues[attribute.Key] );
                                }
                            }
                        }

                        if ( attributes.Any() )
                        {
                            foreach ( var person in people )
                            {
                                person.LoadAttributes();
                                foreach ( var attribute in attributes )
                                {
                                    string originalValue = person.GetAttributeValue( attribute.Key );
                                    string newValue = attributeValues[attribute.Id];
                                    if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                                    {
                                        Rock.Attribute.Helper.SaveAttributeValue( person, attribute, newValue, rockContext );
                                    }
                                }
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                #endregion

                #region Add Note

                if ( this.UpdateNoteAction != NoteChangeActionSpecifier.None
                     && !string.IsNullOrWhiteSpace( UpdateNoteText )
                     && _currentPerson != null )
                {
                    bool isPrivate = this.UpdateNoteIsPrivate;

                    var noteType = NoteTypeCache.Get( UpdateNoteTypeId.GetValueOrDefault( 0 ) );
                    if ( noteType != null )
                    {
                        var notes = new List<Note>();
                        var noteService = new NoteService( rockContext );

                        foreach ( int id in ids )
                        {
                            var note = new Note();
                            note.IsSystem = false;
                            note.EntityId = id;
                            note.Caption = isPrivate ? "You - Personal Note" : string.Empty;
                            note.Text = UpdateNoteText;
                            note.IsAlert = UpdateNoteIsAlert;
                            note.IsPrivateNote = isPrivate;
                            note.NoteTypeId = noteType.Id;
                            notes.Add( note );
                            noteService.Add( note );
                        }

                        rockContext.SaveChanges();
                    }
                }

                #endregion

                #region Group

                if ( this.UpdateGroupAction != GroupChangeActionSpecifier.None )
                {
                    var group = new GroupService( rockContext ).Get( UpdateGroupId.Value );
                    if ( group != null )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );

                        var existingMembersQuery = groupMemberService.Queryable( true ).Include( a => a.Group )
                                                                     .Where( m => m.GroupId == group.Id
                                                                                  && ids.Contains( m.PersonId ) );

                        if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Remove )
                        {
                            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                            var existingIds = existingMembersQuery.Select( gm => gm.Id ).Distinct().ToList();

                            Action<RockContext, List<int>> deleteAction = ( context, items ) =>
                            {
                                // Load the batch of GroupMember items into the context and delete them.
                                groupMemberService = new GroupMemberService( context );

                                var batchGroupMembers = groupMemberService.Queryable( true ).Where( x => items.Contains( x.Id ) ).ToList();

                                GroupMemberHistoricalService groupMemberHistoricalService = new GroupMemberHistoricalService( context );

                                foreach ( GroupMember groupMember in batchGroupMembers )
                                {
                                    try
                                    {
                                        bool archive = false;
                                        if ( groupTypeCache.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == groupMember.Id ) )
                                        {
                                            // if the group has GroupHistory enabled, and this group member has group member history snapshots, they were prompted to Archive
                                            archive = true;
                                        }
                                        else
                                        {
                                            string errorMessage;
                                            if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                                            {
                                                ExceptionLogService.LogException( new Exception( string.Format( "Error removing person {0} from group {1}: ", groupMember.Person.FullName, group.Name ) + errorMessage ), null );
                                                Interlocked.Increment( ref _errorCount );
                                                continue;
                                            }
                                        }

                                        if ( archive )
                                        {
                                            // NOTE: Delete will AutoArchive, but since we know that we need to archive, we can call .Archive directly
                                            groupMemberService.Archive( groupMember, this._currentPersonAliasId, true );
                                        }
                                        else
                                        {
                                            groupMemberService.Delete( groupMember, true );
                                        }

                                        context.SaveChanges();
                                    }
                                    catch ( Exception ex )
                                    {
                                        ExceptionLogService.LogException( new Exception( string.Format( "Error removing person {0} from group {1}", groupMember.Person.FullName, group.Name ), ex ), null );
                                        Interlocked.Increment( ref _errorCount );
                                    }
                                }
                            };

                            ProcessBatchUpdate( existingIds, this.BatchSize, deleteAction );
                        }
                        else
                        {
                            // Get the attribute values updated
                            var gm = new GroupMember();
                            gm.Group = group;
                            gm.GroupId = group.Id;
                            gm.LoadAttributes( rockContext );
                            var selectedGroupAttributes = new List<AttributeCache>();

                            if ( UpdateGroupAttributeValues != null
                                 && UpdateGroupAttributeValues.Count > 0 )
                            {
                                foreach ( var attributeCache in gm.Attributes.Select( a => a.Value ) )
                                {
                                    if ( UpdateGroupAttributeValues.ContainsKey( attributeCache.Key ) )
                                    {
                                        selectedGroupAttributes.Add( attributeCache );
                                    }
                                }
                            }

                            if ( UpdateGroupAction == GroupChangeActionSpecifier.Add )
                            {
                                if ( UpdateGroupRoleId.HasValue )
                                {
                                    var newGroupMembers = new List<GroupMember>();

                                    var existingIds = existingMembersQuery.Select( m => m.PersonId ).Distinct().ToList();

                                    var personKeys = ids.Where( id => !existingIds.Contains( id ) ).ToList();

                                    Action<RockContext, List<int>> addAction = ( context, items ) =>
                                    {
                                        groupMemberService = new GroupMemberService( context );

                                        foreach ( int id in items )
                                        {
                                            var groupMember = new GroupMember();
                                            groupMember.GroupId = group.Id;
                                            groupMember.GroupRoleId = UpdateGroupRoleId.Value;
                                            groupMember.GroupMemberStatus = UpdateGroupStatus;
                                            groupMember.PersonId = id;

                                            if ( groupMember.IsValidGroupMember( context ) )
                                            {

                                                groupMemberService.Add( groupMember );

                                                newGroupMembers.Add( groupMember );
                                            }
                                            else
                                            {
                                                // Validation errors will get added to the ValidationResults collection. Add those results to the log and then move on to the next person.
                                                var validationMessage = string.Join( ",", groupMember.ValidationResults.Select( r => r.ErrorMessage ).ToArray() );
                                                var person = new PersonService( rockContext ).GetNoTracking( groupMember.PersonId );
                                                var ex = new GroupMemberValidationException( string.Format( "Unable to add {0} to group: {1}", person, validationMessage ) );
                                                Interlocked.Increment( ref _errorCount );
                                                ExceptionLogService.LogException( ex );
                                            }
                                        }

                                        context.SaveChanges();
                                    };

                                    ProcessBatchUpdate( personKeys, this.BatchSize, addAction );

                                    if ( selectedGroupAttributes.Any() )
                                    {
                                        foreach ( var groupMember in newGroupMembers )
                                        {
                                            foreach ( var attribute in selectedGroupAttributes )
                                            {
                                                Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, UpdateGroupAttributeValues[attribute.Key], rockContext );
                                            }
                                        }
                                    }
                                }
                            }
                            else // Update
                            {
                                if ( SelectedFields.Contains( FieldNames.GroupRole ) && UpdateGroupRoleId.HasValue )
                                {
                                    foreach ( var member in existingMembersQuery.Where( m => m.GroupRoleId != UpdateGroupRoleId.Value ) )
                                    {
                                        if ( !existingMembersQuery.Any( m => m.PersonId == member.PersonId && m.GroupRoleId == UpdateGroupRoleId.Value ) )
                                        {
                                            member.GroupRoleId = UpdateGroupRoleId.Value;
                                        }
                                    }
                                }

                                if ( SelectedFields.Contains( FieldNames.GroupMemberStatus ) )
                                {
                                    foreach ( var member in existingMembersQuery )
                                    {
                                        member.GroupMemberStatus = UpdateGroupStatus;
                                    }
                                }

                                rockContext.SaveChanges();

                                if ( selectedGroupAttributes.Any() )
                                {
                                    Action<RockContext, List<GroupMember>> updateAction = ( context, items ) =>
                                    {
                                        foreach ( var groupMember in items )
                                        {
                                            foreach ( var attribute in selectedGroupAttributes )
                                            {
                                                Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, UpdateGroupAttributeValues[attribute.Key], context );
                                            }
                                        }

                                        context.SaveChanges();
                                    };

                                    // Process the Attribute updates in batches.
                                    var existingMembers = existingMembersQuery.ToList();

                                    ProcessBatchUpdate( existingMembers, this.BatchSize, updateAction );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tag
                var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

                if ( this.UpdateTagAction != TagChangeActionSpecifier.None )
                {
                    var tag = new TagService( rockContext ).Get( UpdateTagId );
                    if ( tag != null && tag.IsAuthorized( Rock.Security.Authorization.TAG, _currentPerson ) )
                    {
                        var taggedItemService = new TaggedItemService( rockContext );

                        // get guids of selected individuals
                        var personGuids = new PersonService( rockContext ).Queryable( true )
                                            .Where( p =>
                                                ids.Contains( p.Id ) )
                                            .Select( p => p.Guid )
                                            .ToList();

                        if ( this.UpdateTagAction == TagChangeActionSpecifier.Add )
                        {
                            foreach ( var personGuid in personGuids )
                            {
                                if ( !taggedItemService.Queryable().Where( t => t.TagId == UpdateTagId && t.EntityGuid == personGuid ).Any() )
                                {
                                    TaggedItem taggedItem = new TaggedItem();
                                    taggedItem.TagId = UpdateTagId;
                                    taggedItem.EntityTypeId = personEntityTypeId;
                                    taggedItem.EntityGuid = personGuid;

                                    taggedItemService.Add( taggedItem );
                                    rockContext.SaveChanges();
                                }
                            }
                        }
                        else // remove
                        {
                            foreach ( var personGuid in personGuids )
                            {
                                var taggedPerson = taggedItemService.Queryable().Where( t => t.TagId == UpdateTagId && t.EntityGuid == personGuid ).FirstOrDefault();
                                if ( taggedPerson != null )
                                {
                                    taggedItemService.Delete( taggedPerson );
                                }
                            }
                            rockContext.SaveChanges();
                        }
                    }
                }
                #endregion

                #region workflow

                if ( PostUpdateWorkflowIdList != null )
                {
                    foreach ( string value in PostUpdateWorkflowIdList )
                    {
                        int? intValue = value.AsIntegerOrNull();
                        if ( intValue.HasValue )
                        {

                            var workflowDetails = people.Select( p => new LaunchWorkflowDetails( p ) ).ToList();
                            var launchWorkflowsTxn = new Rock.Transactions.LaunchWorkflowsTransaction( intValue.Value, workflowDetails );
                            launchWorkflowsTxn.InitiatorPersonAliasId = _currentPersonAliasId;
                            Rock.Transactions.RockQueue.Enqueue( launchWorkflowsTxn );
                        }
                    }
                }

                #endregion
            }

            /// <summary>
            /// Process database updates for the supplied list of items in batches to improve performance for large datasets.
            /// </summary>
            /// <param name="itemsToProcess"></param>
            /// <param name="batchSize"></param>
            /// <param name="processingAction"></param>
            private void ProcessBatchUpdate<TListItem>( List<TListItem> itemsToProcess, int batchSize, Action<RockContext, List<TListItem>> processingAction )
            {
                int remainingCount = itemsToProcess.Count();

                int batchesProcessed = 0;

                if ( batchSize <= 0 )
                {
                    batchSize = 50;
                }

                while ( remainingCount > 0 )
                {
                    var batchItems = itemsToProcess.Skip( batchesProcessed * batchSize ).Take( batchSize ).ToList();

                    using ( var batchContext = this.GetDataContextForCurrentUser() )
                    {
                        processingAction.Invoke( batchContext, batchItems );
                    }

                    batchesProcessed++;

                    remainingCount -= batchItems.Count();
                }
            }

            #endregion

            #region Action Summary

            /// <summary>
            /// Gets a summary of the changes that will be made when the processor is executed according to the current settings.
            /// </summary>
            public List<string> GetPendingActionSummary()
            {
                #region Individual Details Updates

                int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var changes = new List<string>();

                if ( SelectedFields.Contains( FieldNames.Title ) )
                {
                    EvaluateChange( changes, "Title", DefinedValueCache.GetName( this.UpdateTitleValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.Suffix ) )
                {
                    EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( this.UpdateSuffixValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.ConnectionStatus ) )
                {
                    EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( this.UpdateConnectionStatusValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.RecordStatus ) )
                {
                    EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( this.UpdateRecordStatusValueId ) );

                    if ( this.UpdateRecordStatusValueId.HasValue && this.UpdateRecordStatusValueId.Value == inactiveStatusId )
                    {
                        EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( this.UpdateInactiveReasonId ) );

                        if ( !string.IsNullOrWhiteSpace( this.UpdateInactiveReasonNote ) )
                        {
                            EvaluateChange( changes, "Inactive Reason Note", this.UpdateInactiveReasonNote );
                        }
                    }
                }

                if ( SelectedFields.Contains( FieldNames.Gender ) )
                {
                    EvaluateChange( changes, "Gender", this.UpdateGender );
                }

                if ( SelectedFields.Contains( FieldNames.MaritalStatus ) )
                {
                    EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( this.UpdateMaritalStatusValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.GraduationYear ) )
                {
                    EvaluateChange( changes, "Graduation Year", this.UpdateGraduationYear );
                }

                if ( SelectedFields.Contains( FieldNames.EmailIsActive ) )
                {
                    EvaluateChange( changes, "Email Is Active", this.UpdateEmailActive );
                }

                if ( SelectedFields.Contains( FieldNames.CommunicationPreference ) )
                {
                    EvaluateChange( changes, "Communication Preference", this.UpdateCommunicationPreference );
                }

                if ( SelectedFields.Contains( FieldNames.EmailPreference ) )
                {
                    EvaluateChange( changes, "Email Preference", this.UpdateEmailPreference );
                }

                if ( SelectedFields.Contains( FieldNames.EmailNote ) )
                {
                    EvaluateChange( changes, "Email Note", this.UpdateEmailNote );
                }

                if ( SelectedFields.Contains( FieldNames.SystemNote ) )
                {
                    EvaluateChange( changes, "System Note", this.UpdateSystemNote );
                }

                if ( SelectedFields.Contains( FieldNames.ReviewReason ) )
                {
                    EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( this.UpdateReviewReasonValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.ReviewReasonNote ) )
                {
                    EvaluateChange( changes, "Review Reason Note", this.UpdateReviewReasonNote );
                }

                if ( SelectedFields.Contains( FieldNames.Campus ) )
                {
                    if ( this.UpdateCampusId.HasValue )
                    {
                        var campus = CampusCache.Get( this.UpdateCampusId.Value );
                        if ( campus != null )
                        {
                            EvaluateChange( changes, "Campus (for all family members)", campus.Name );
                        }
                    }
                }

                // following
                if ( this.UpdateFollowingAction == FollowingChangeActionSpecifier.Add )
                {
                    changes.Add( "Add to your Following list." );
                }
                else if ( this.UpdateFollowingAction == FollowingChangeActionSpecifier.Remove )
                {
                    changes.Add( "Remove from your Following list." );
                }

                #endregion

                #region Attributes

                var rockContext = this.GetDataContextForCurrentUser();

                if ( this.UpdatePersonAttributeValues != null
                     && this.UpdatePersonAttributeValues.Any() )
                {
                    var selectedCategories = new List<CategoryCache>();
                    foreach ( Guid categoryGuid in this.PersonAttributeCategories )
                    {
                        var category = CategoryCache.Get( categoryGuid, rockContext );
                        if ( category != null )
                        {
                            selectedCategories.Add( category );
                        }
                    }

                    var attributes = new List<AttributeCache>();
                    var attributeValues = new Dictionary<int, string>();

                    int categoryIndex = 0;

                    foreach ( var category in selectedCategories.OrderBy( c => c.Name ) )
                    {
                        categoryIndex++;

                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );

                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                var attributeCache = AttributeCache.Get( attribute.Id );

                                if ( attributeCache != null
                                     && this.UpdatePersonAttributeValues.ContainsKey( attributeCache.Key ) )
                                {
                                    var newValue = this.UpdatePersonAttributeValues[attributeCache.Key];

                                    EvaluateChange( changes, attributeCache.Name, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Note

                if ( this.UpdateNoteAction == NoteChangeActionSpecifier.Add )
                {
                    if ( !string.IsNullOrWhiteSpace( this.UpdateNoteText ) && this.UpdateNoteTypeId.HasValue && CurrentPerson != null )
                    {
                        var noteType = NoteTypeCache.Get( this.UpdateNoteTypeId.Value );

                        if ( noteType != null )
                        {
                            changes.Add( string.Format( "Add a <span class='field-name'>{0}{1}{2}</span> of <p><span class='field-value'>{3}</span></p>.",
                                ( this.UpdateNoteIsPrivate ? "Private " : "" ), noteType.Name, ( this.UpdateNoteIsAlert ? " (Alert)" : "" ), this.UpdateNoteText.ConvertCrLfToHtmlBr() ) );
                        }
                    }
                }

                #endregion

                #region Group

                if ( this.UpdateGroupAction != GroupChangeActionSpecifier.None )
                {
                    var group = new GroupService( rockContext ).Get( this.UpdateGroupId.Value );
                    if ( group != null )
                    {
                        if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Remove )
                        {
                            changes.Add( string.Format( "Remove from <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Add )
                        {
                            changes.Add( string.Format( "Add to <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else // Update
                        {
                            if ( SelectedFields.Contains( FieldNames.GroupRole ) )
                            {
                                if ( this.UpdateGroupRoleId.HasValue )
                                {
                                    var roleId = this.UpdateGroupRoleId.Value;
                                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                                    var role = groupType.Roles.Where( r => r.Id == roleId ).FirstOrDefault();
                                    if ( role != null )
                                    {
                                        string field = string.Format( "{0} Role", group.Name );
                                        EvaluateChange( changes, field, role.Name );
                                    }
                                }
                            }

                            if ( SelectedFields.Contains( FieldNames.GroupMemberStatus ) )
                            {
                                string field = string.Format( "{0} Member Status", group.Name );
                                EvaluateChange( changes, field, this.UpdateGroupStatus.ToString() );
                            }

                            var groupMember = new GroupMember();
                            groupMember.Group = group;
                            groupMember.GroupId = group.Id;
                            groupMember.LoadAttributes( rockContext );

                            foreach ( var attributeCache in groupMember.Attributes.Select( a => a.Value ) )
                            {
                                if ( this.UpdateGroupAttributeValues.ContainsKey( attributeCache.Key ) )
                                {
                                    var newValue = this.UpdateGroupAttributeValues[attributeCache.Key];

                                    string field = string.Format( "{0}: {1}", group.Name, attributeCache.Name );
                                    EvaluateChange( changes, field, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tag

                if ( this.UpdateTagAction != TagChangeActionSpecifier.None )
                {
                    var tagService = new TagService( rockContext );

                    var tag = tagService.Queryable().FirstOrDefault( x => x.Id == this.UpdateTagId );

                    if ( tag != null )
                    {
                        changes.Add( string.Format( "{0} {1} <span class='field-name'>{2}</span> tag.",
                                this.UpdateTagAction.ToString(),
                                this.UpdateTagAction == TagChangeActionSpecifier.Add ? "to" : "from",
                                tag.Name ) );
                    }
                }

                #endregion

                #region workflow

                if ( this.PostUpdateWorkflowIdList != null
                     && this.PostUpdateWorkflowIdList.Any() )
                {
                    var workflowTypes = new List<string>();

                    foreach ( var workflowId in this.PostUpdateWorkflowIdList )
                    {
                        var workflowType = WorkflowTypeCache.Get( workflowId );

                        if ( workflowType != null )
                        {
                            workflowTypes.Add( workflowType.Name );
                        }
                    }

                    if ( workflowTypes.Any() )
                    {
                        changes.Add( string.Format( "Activate the <span class='field-name'>{0}</span> {1}.",
                             workflowTypes.AsDelimited( ", ", " and " ),
                             "workflow".PluralizeIf( workflowTypes.Count > 1 ) ) );
                    }
                }

                #endregion

                return changes;
            }

            /// <summary>
            /// Evaluates the change, and adds a summary string of what if anything changed
            /// </summary>
            /// <param name="historyMessages">The history messages.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="oldValue">The old value.</param>
            /// <param name="newValue">The new value.</param>
            private void EvaluateChange( List<string> historyMessages, string propertyName, string newValue )
            {
                if ( !string.IsNullOrWhiteSpace( newValue ) )
                {
                    historyMessages.Add( string.Format( "Update <span class='field-name'>{0}</span> to value of <span class='field-value'>{1}</span>.", propertyName, newValue ) );
                }
                else
                {
                    historyMessages.Add( string.Format( "Clear <span class='field-name'>{0}</span> value.", propertyName ) );
                }
            }

            /// <summary>
            /// Evaluates the change, and adds a summary string of what if anything changed
            /// </summary>
            /// <param name="historyMessages">The history messages.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="oldValue">The old value.</param>
            /// <param name="newValue">The new value.</param>
            private void EvaluateChange( List<string> historyMessages, string propertyName, int? newValue )
            {
                EvaluateChange( historyMessages, propertyName,
                    newValue.HasValue ? newValue.Value.ToString() : string.Empty );
            }

            /// <summary>
            /// Evaluates the change.
            /// </summary>
            /// <param name="historyMessages">The history messages.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="oldValue">The old value.</param>
            /// <param name="newValue">The new value.</param>
            /// <param name="includeTime">if set to <c>true</c> [include time].</param>
            private void EvaluateChange( List<string> historyMessages, string propertyName, DateTime? newValue, bool includeTime = false )
            {
                string newStringValue = string.Empty;
                if ( newValue.HasValue )
                {
                    newStringValue = includeTime ? newValue.Value.ToString() : newValue.Value.ToShortDateString();
                }

                EvaluateChange( historyMessages, propertyName, newStringValue );
            }

            /// <summary>
            /// Evaluates the change.
            /// </summary>
            /// <param name="historyMessages">The history messages.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="oldValue">if set to <c>true</c> [old value].</param>
            /// <param name="newValue">if set to <c>true</c> [new value].</param>
            private void EvaluateChange( List<string> historyMessages, string propertyName, bool? newValue )
            {
                EvaluateChange( historyMessages, propertyName,
                    newValue.HasValue ? newValue.Value.ToString() : string.Empty );
            }

            /// <summary>
            /// Evaluates the change.
            /// </summary>
            /// <param name="historyMessages">The history messages.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="oldValue">The old value.</param>
            /// <param name="newValue">The new value.</param>
            private void EvaluateChange( List<string> historyMessages, string propertyName, Enum newValue )
            {
                string newStringValue = newValue != null ? newValue.ConvertToString() : string.Empty;
                EvaluateChange( historyMessages, propertyName, newStringValue );
            }

            #endregion
        }

        #endregion

        #endregion

    }
}
