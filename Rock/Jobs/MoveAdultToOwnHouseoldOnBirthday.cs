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
using System.Data.Entity.SqlServer;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run that puts 18 year olds in their own Household on their bday.
    /// </summary>
    [BooleanField( "Setup Parent/Child Relationships", "Setup Parent/Child relationships back to Adults in old family.", false, order: 0 )]
    [BooleanField( "Setup Sibling relationships", "Setup Sibling relationships back to other Children in old family.", false, order: 1 )]
    [BooleanField( "Copy Home Address", "Copy Home Address to new family record.", false, order: 2 )]
    [BooleanField( "Copy Home Phone", "Copy Home Phone from old family Adults to new Adult if new Adult does not already have a Home Phone.", false, order: 3 )]
    [WorkflowTypeField( "Workflow", "The workflow type to launch when a person move to new family.", key: "EraEntryWorkflow", order: 4 )]
    [IntegerField( "Moves Per Run", "The maximum number of moves to run per run.", true, 2500, order: 5 )]

    [DisallowConcurrentExecution]
    public class MoveAdultToOwnHouseoldOnBirthday : IJob
    {

        /// <summary>
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public MoveAdultToOwnHouseoldOnBirthday()
        {
        }

        /// <summary>
        /// Job that executes routine Rock cleanup tasks
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? launchWorkflowType = dataMap.GetString( "LaunchWorkflow" ).AsGuidOrNull();
            int maxRecords = Int32.Parse( dataMap.GetString( "MovesPerRun" ) );

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            var groupType = GroupTypeCache.GetFamilyGroupType();

            var activeStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
            var currentDate = RockDateTime.Today;

            // only include alive people that have record status of Active

            var familyMemberQry = groupMemberService.Queryable( "Person", true )
                        .Where( a => a.Group.GroupTypeId==groupType.Id 
                        && a.Person.RecordStatusValue.Guid == activeStatusGuid && a.Person.IsDeceased == false );

            // only include people whose birthday is today (which can be determined from the computed DaysUntilBirthday column)
            familyMemberQry = familyMemberQry.Where( a => a.Person.DaysUntilBirthday.HasValue && a.Person.DaysUntilBirthday == 0 );

            familyMemberQry = familyMemberQry.Where( a => a.GroupRole.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) );

            var qryWithAge = familyMemberQry.Select(
                      p => new
                      {
                          Person = p.Person,
                          Age = ( p.Person.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.Person.BirthDate, currentDate ), currentDate )
                            ? SqlFunctions.DateDiff( "year", p.Person.BirthDate, currentDate ) - 1
                            : SqlFunctions.DateDiff( "year", p.Person.BirthDate, currentDate ) )
                      } );

            qryWithAge = qryWithAge.Where( a => !a.Age.HasValue || a.Age >= 18 );
            var personList = qryWithAge.Take( maxRecords ).Select( a => a.Person ).ToList();

            foreach ( var person in personList )
            {
                Group family = person.GetFamily( rockContext );
                var groupMember = groupMemberService.Queryable( "Person", false )
                .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == groupType.Id &&
                        m.GroupId == family.Id ).FirstOrDefault();
                var familyMembers = person.GetFamilyMembers( false, rockContext ).ToList();
                rockContext.WrapTransaction( () =>
                {
                    // Create new family only if there any other member in family
                    if ( familyMembers.Count > 0 )
                    {

                        var groupChanges = new List<string>();
                        var newFamilyChanges = new List<string>();

                        // Family member was removed and should be created in their own new family
                        var newFamily = new Group();
                        newFamily.Name = person.LastName + " " + family.GroupType.Name;
                        History.EvaluateChange( groupChanges, "Family", family.Name, string.Empty );
                        History.EvaluateChange( newFamilyChanges, "Family", string.Empty, newFamily.Name );

                        newFamily.GroupTypeId = groupType.Id;

                        if ( family.CampusId.HasValue )
                        {
                            History.EvaluateChange( newFamilyChanges, "Campus", string.Empty, CampusCache.Read( family.CampusId.Value ).Name );
                        }

                        newFamily.CampusId = family.CampusId;

                        groupService.Add( newFamily );

                        rockContext.SaveChanges();

                        person.GivingGroupId = newFamily.Id;

                        groupMember.Group = newFamily;

                        if ( dataMap.GetString( "CopyHomeAddress" ).AsBoolean() )
                        {
                            CopyLocation( groupLocationService, family, newFamily );
                        }

                        rockContext.SaveChanges();

                        HistoryService.SaveChanges(
                                   rockContext,
                                   typeof( Person ),
                                   Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                   person.Id,
                                   newFamilyChanges,
                                   newFamily.Name,
                                   typeof( Group ),
                                   newFamily.Id );

                        HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Person ),
                                    Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                    person.Id,
                                    groupChanges,
                                    family.Name,
                                    typeof( Group ),
                                    family.Id );
                    }


                    var memberChanges = new List<string>();
                    // If this person is 18 or older, create them as an Adult in their new group
                    if ( ( groupMember.Person.Age ?? 0 ) >= 18 )
                    {
                        History.EvaluateChange( memberChanges, "Role", string.Empty, groupMember.GroupRole.Name );

                        var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                        groupMember.GroupRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                    }

                    var homePhoneDefinedValue = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    if ( dataMap.GetString( "CopyHomePhone" ).AsBoolean() && person.PhoneNumbers != null && !person.PhoneNumbers.Any( a => a.NumberTypeValueId == homePhoneDefinedValue.Id ) )
                    {
                        CopyHomePhone( rockContext, person, homePhoneDefinedValue, familyMembers );
                    }
                    rockContext.SaveChanges();

                    var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
                    if ( dataMap.GetString( "SetupSiblingRelationships" ).AsBoolean() )
                    {
                        var relationshipRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_SIBLING ) );
                        var childs = familyMembers.Where( a => a.GroupRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
                        foreach ( var child in childs )
                        {

                            groupMemberService.CreateKnownRelationship( person.Id, child.PersonId, relationshipRole.Id );
                        }
                    }

                    if ( dataMap.GetString( "SetupParent/ChildRelationships" ).AsBoolean() )
                    {
                        var relationshipRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_PARENT ) );
                        var test = familyMembers.ToList();
                        var parents = familyMembers.Where( a => a.GroupRole.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) );
                        foreach ( var parent in parents )
                        {
                            groupMemberService.CreateKnownRelationship( person.Id, parent.PersonId, relationshipRole.Id );
                        }
                    }
                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), groupMember.Id, memberChanges, groupMember.Group.Name, typeof( Group ), groupMember.Group.Id );

                } );

                if ( launchWorkflowType.HasValue )
                {
                    LaunchWorkflow( launchWorkflowType.Value, family, person.FullName );
                }
            }
        }

        /// <summary>
        /// Copy Location from old family to new family
        /// </summary>
        /// <param name="groupLocationService">The Group location service.</param>
        /// <param name="oldFamily">The old family.</param>
        /// <param name="newFamily">The new family.</param>
        private static void CopyLocation( GroupLocationService groupLocationService, Group oldFamily, Group newFamily )
        {
            var groupLocations = oldFamily.GroupLocations
                                .Where( l => l.GroupLocationTypeValue != null )
                                .OrderBy( l => l.GroupLocationTypeValue.Order );
            foreach ( var groupLocation in groupLocations )
            {
                var newGroupLocation = new GroupLocation();
                newGroupLocation.GroupId = newFamily.Id;
                newGroupLocation.LocationId = groupLocation.LocationId;
                newGroupLocation.GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId;
                newGroupLocation.IsMailingLocation = groupLocation.IsMailingLocation;
                newGroupLocation.IsMappedLocation = groupLocation.IsMappedLocation;
                groupLocationService.Add( newGroupLocation );
            }
        }

        /// <summary>
        /// Copy Home Phone from old family Adults to new Adult if new Adult does not already have a Home Phone
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="homePhoneDefinedValue">The home phone defined value.</param>
        private static void CopyHomePhone( RockContext rockContext, Person person, DefinedValueCache homePhoneDefinedValue, List<GroupMember> familyMembers )
        {
            var phoneNumberService = new PhoneNumberService( rockContext );
            var membersWithHomePhone = familyMembers
                            .Where( a => a.GroupRole.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) )
                            .Select( a => a.PersonId ).ToList();

            var homePhone = phoneNumberService.Queryable()
                .Where( a => a.NumberTypeValueId == homePhoneDefinedValue.Id && membersWithHomePhone.Contains( a.PersonId ) )
                .FirstOrDefault();

            if ( homePhone != null )
            {
                var phoneNumber = new PhoneNumber();
                phoneNumber.NumberTypeValueId = homePhoneDefinedValue.Id;
                phoneNumber.Number = homePhone.Number;
                person.PhoneNumbers.Add( phoneNumber );
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="originalFamily">The original family.</param>
        /// <param name="personFullName">The full name of Person.</param>
        private void LaunchWorkflow( Guid workflowTypeGuid, Group originalFamily, string personFullName )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowType = WorkflowTypeCache.Read( workflowTypeGuid );
                if ( workflowType != null )
                {
                    var workflowService = new WorkflowService( rockContext );
                    var workflow = Rock.Model.Workflow.Activate( workflowType, personFullName, rockContext );
                    workflow.SetAttributeValue( "OriginalFamilyId ", originalFamily.Id );
                    workflow.SaveAttributeValues();
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );

                }
            }
        }
    }
}
