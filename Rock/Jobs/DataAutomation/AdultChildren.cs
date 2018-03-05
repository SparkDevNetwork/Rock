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
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs.DataAutomation
{
    /// <summary>
    /// Job that will remove any "adult" children from a family and move them to their own new family. This job will only process people that 
    /// have a 'Child' role in one or more families, but also have an age indicating that they are an "adult". The job will process one person
    /// (not a group member) at a time. For each person, it will look at all the families they belong to and their role in each of those families.
    /// If they are already an adult in any family, they will not be added to any additional family, but will be removed from all families where 
    /// they area child. If they are currently not an adult in any family, the job will look to see if they are the only child in any of their families.
    /// If so, they will just be updated in that family to be an adult, and then removed from any other family where they are a child. If they
    /// are not an adult in any family, and are not the sole member of any family, a new family will be added for them, and they will be added
    /// to that family as an adult, and then removed from all other families where they are a child.
    /// </summary>
    [CustomDropdownListField( "Parent Relationship", "An optional relationship to add for the other adults in the original family.", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
ORDER BY [Text]", true, false, "", "", 0 )]
    [CustomDropdownListField( "Sibling Relationship", "An optional relationship to add for the other children in the original family.", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
ORDER BY [Text]", true, false, "", "", 1 )]
    [BooleanField( "Use Same Home Address", "Should the new adult's new family have the same Home address as the original family?", true, "", 2 )]
    [BooleanField( "Use Same Home Phone", "If new adult does not have a home phone, should they have the same Home phone as adults int he original family?", true, "", 3 )]
    [WorkflowTypeField( "Workflows", "The workflow type(s) to launch for each person that was processed. The person will be passed to the workflow as the entity. " +
        "If the workflow has an 'OldFamily' Group attribute the block will set this to the person's primary family before processing the person. " + 
        "If the workflow has a 'NewFamily' Group attribute the block will set that to the family that the person was updated or added as an adult to.", true, false, "", "", 4 )]
    [IntegerField( "Maximum Records", "The maximum number of people to process on each run of this job.", true, 200, "", 5 )]
    [IntegerField( "Adult Age", "The age to start considering children as an adult", true, 18, "", 6 )]

    [DisallowConcurrentExecution]
    public class AdultChildren : IJob
    {

        /// <summary>
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public AdultChildren()
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
            try
            {

                // Get the job settings.
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                int? parentRelationshipId = dataMap.GetString( "ParentRelationship" ).AsIntegerOrNull();
                int? siblingRelationshipId = dataMap.GetString( "SiblingRelationship" ).AsIntegerOrNull();
                bool copyAddress = dataMap.GetString( "UseSameHomeAddress" ).AsBooleanOrNull() ?? true;
                bool copyPhone = dataMap.GetString( "UseSameHomePhone" ).AsBooleanOrNull() ?? true;
                int maxRecords = dataMap.GetString( "MaximumRecords" ).AsIntegerOrNull() ?? 200;
                int adultAge = dataMap.GetString( "AdultAge" ).AsIntegerOrNull() ?? 18;
                List<Guid> workflows = dataMap.GetString( "Workflows" ).SplitDelimitedValues().AsGuidList();

                // Get some system guids
                var activeRecordStatusGuid = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
                var homeAddressGuid = SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
                var homePhoneGuid = SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                var personChangesGuid = SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid();
                var familyChangesGuid = SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid();

                // Get the family group type and roles
                var familyGroupType = GroupTypeCache.Read( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                if ( familyGroupType == null )
                {
                    throw new Exception( "Could not determine the 'Family' group type." );
                }
                var childRole = familyGroupType.Roles.FirstOrDefault( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
                var adultRole = familyGroupType.Roles.FirstOrDefault( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
                if ( childRole == null || adultRole == null )
                {
                    throw new Exception( "Could not determine the 'Adult' and 'Child' roles." );
                }

                // Calculate the date to use for determining if someone is an adult based on their age (birthdate)
                var adultBirthdate = RockDateTime.Today.AddYears( 0 - adultAge );

                // Get a list of people marked as a child in any family, but who are now an "adult" based on their age
                var adultChildIds = new List<int>();
                using ( var rockContext = new RockContext() )
                {
                    adultChildIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.GroupRoleId == childRole.Id &&
                            m.Person.BirthDate.HasValue &&
                            m.Person.BirthDate <= adultBirthdate &&
                            m.Person.RecordStatusValue != null &&
                            m.Person.RecordStatusValue.Guid == activeRecordStatusGuid )
                        .OrderBy( m => m.PersonId )
                        .Select( m => m.PersonId )
                        .Distinct()
                        .Take( maxRecords )
                        .ToList();
                }

                // Counter for displaying results
                int peopleProcessed = 0;

                // Loop through each person
                foreach ( int personId in adultChildIds )
                {
                    // Using a new rockcontext for each one (to improve performance)
                    using ( var rockContext = new RockContext() )
                    {
                        // Get all the 'family' group member records for this person.
                        var groupMemberService = new GroupMemberService( rockContext );
                        var groupMembers = groupMemberService.Queryable()
                            .Where( m =>
                                m.PersonId == personId &&
                                m.Group.GroupTypeId == familyGroupType.Id )
                            .ToList();

                        // If there are no group members (shouldn't happen), just ignore and keep going
                        if ( !groupMembers.Any() )
                        {
                            break;
                        }

                        // Get a reference to the person
                        var person = groupMembers.First().Person;

                        // Get the person's primary family, and if we can't get that (something else that shouldn't happen), just ignore this person.
                        var primaryFamily = person.PrimaryFamily;
                        if ( primaryFamily == null )
                        {
                            break;
                        }

                        // Setup a variable for tracking person changes
                        var personChanges = new List<string>();

                        // Get all the parent and sibling ids (for adding relationships later)
                        var parentIds = groupMembers
                            .SelectMany( m => m.Group.Members )
                            .Where( m =>
                                m.PersonId != personId &&
                                m.GroupRoleId == adultRole.Id )
                            .Select( m => m.PersonId )
                            .Distinct()
                            .ToList();

                        var siblingIds = groupMembers
                            .SelectMany( m => m.Group.Members )
                            .Where( m =>
                                m.PersonId != personId &&
                                m.GroupRoleId == childRole.Id )
                            .Select( m => m.PersonId )
                            .Distinct()
                            .ToList();

                        // If person is already an adult in any family, lets find the first one, and use that as the new family
                        var newFamily = groupMembers
                            .Where( m => m.GroupRoleId == adultRole.Id )
                            .OrderBy( m => m.GroupOrder )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        // If person was not already an adult in any family, let's look for a family where they are the only person, or create a new one
                        if ( newFamily == null )
                        { 
                            // Try to find a family where they are the only one in the family.
                            newFamily = groupMembers
                                .Select( m => m.Group )
                                .Where( g => !g.Members.Any( m => m.PersonId != personId ) )
                                .FirstOrDefault();

                            // If we found one, make them an adult in that family
                            if ( newFamily != null )
                            {
                                // The current person should be the only one in this family, but lets loop through each member anyway
                                foreach ( var groupMember in groupMembers.Where( m => m.GroupId == newFamily.Id ) )
                                {
                                    groupMember.GroupRoleId = adultRole.Id;
                                }

                                // Save role change to history
                                var memberChanges = new List<string>();
                                History.EvaluateChange( memberChanges, "Role", string.Empty, adultRole.Name );
                                HistoryService.SaveChanges( rockContext, typeof( Person ), familyChangesGuid, personId, memberChanges, newFamily.Name, typeof( Group ), newFamily.Id, false );
                            }
                            else
                            {
                                // If they are not already an adult in a family, and they're not in any family by themeselves, we need to create a new family for them.
                                // The SaveNewFamily adds history records for this

                                // Create a new group member and family
                                var groupMember = new GroupMember
                                {
                                    Person = person,
                                    GroupRoleId = adultRole.Id,
                                    GroupMemberStatus = GroupMemberStatus.Active
                                };
                                newFamily = GroupService.SaveNewFamily( rockContext, new List<GroupMember> { groupMember }, primaryFamily.CampusId, false );
                            }
                        }

                        // If user configured the job to copy home address and this person's family does not have any home addresses, copy them from the primary family
                        if ( copyAddress && !newFamily.GroupLocations.Any( l => l.GroupLocationTypeValue != null && l.GroupLocationTypeValue.Guid == homeAddressGuid ) )
                        {
                            var familyChanges = new List<string>();

                            foreach ( var groupLocation in primaryFamily.GroupLocations.Where( l => l.GroupLocationTypeValue != null && l.GroupLocationTypeValue.Guid == homeAddressGuid ) )
                            {
                                newFamily.GroupLocations.Add( new GroupLocation
                                {
                                    LocationId = groupLocation.LocationId,
                                    GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId,
                                    IsMailingLocation = groupLocation.IsMailingLocation,
                                    IsMappedLocation = groupLocation.IsMappedLocation
                                } );

                                History.EvaluateChange( familyChanges, groupLocation.GroupLocationTypeValue.Value + " Location", string.Empty, groupLocation.Location.ToString() );
                            }

                            HistoryService.SaveChanges( rockContext, typeof( Person ), familyChangesGuid, personId, familyChanges, false );
                        }

                        // If user configured the job to copy home phone and this person does not have a home phone, copy the first home phone number from another adult in original family(s)
                        if ( copyPhone && !person.PhoneNumbers.Any( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid ) )
                        {

                            // First look for adults in primary family
                            var homePhone = primaryFamily.Members
                                .Where( m =>
                                    m.PersonId != person.Id &&
                                    m.GroupRoleId == adultRole.Id )
                                .SelectMany( m => m.Person.PhoneNumbers )
                                .FirstOrDefault( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid );

                            // If one was not found in primary family, look in any of the person's other families
                            if ( homePhone == null )
                            {
                                homePhone = groupMembers
                                    .Where( m => m.GroupId != primaryFamily.Id )
                                    .SelectMany( m => m.Group.Members )
                                    .Where( m =>
                                        m.PersonId != person.Id &&
                                        m.GroupRoleId == adultRole.Id )
                                    .SelectMany( m => m.Person.PhoneNumbers )
                                    .FirstOrDefault( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid );
                            }

                            // If we found one, add it to the person
                            if ( homePhone != null )
                            {
                                person.PhoneNumbers.Add( new PhoneNumber
                                {
                                    CountryCode = homePhone.CountryCode,
                                    Number = homePhone.Number,
                                    NumberFormatted = homePhone.NumberFormatted,
                                    NumberReversed = homePhone.NumberReversed,
                                    Extension = homePhone.Extension,
                                    NumberTypeValueId = homePhone.NumberTypeValueId,
                                    IsMessagingEnabled = homePhone.IsMessagingEnabled,
                                    IsUnlisted = homePhone.IsUnlisted,
                                    Description = homePhone.Description
                                } );

                                History.EvaluateChange( personChanges, string.Format( "{0} Phone", homePhone.NumberTypeValue.Value ), string.Empty, homePhone.NumberFormattedWithCountryCode );
                            }
                        }

                        // At this point, the person was either already an adult in one or more families, 
                        //   or we updated one of their records to be an adult, 
                        //   or we created a new family with them as an adult. 
                        // So now we should delete any of the remaining family member records where they are still a child.
                        foreach ( var groupMember in groupMembers.Where( m => m.GroupRoleId == childRole.Id ) )
                        {
                            var memberDelete = new List<string>();
                            History.EvaluateChange( memberDelete, "Family", groupMember.Group.Name, string.Empty );
                            HistoryService.SaveChanges( rockContext, typeof( Person ), familyChangesGuid, personId, memberDelete, groupMember.Group.Name, typeof( Group ), groupMember.Group.Id, false );

                            groupMemberService.Delete( groupMember );
                        }

                        // Add a history record indicating that we processed this person
                        personChanges.Add( "Person was automatically updated to an adult by Data Automation job." );
                        HistoryService.SaveChanges( rockContext, typeof( Person ), personChangesGuid, personId, personChanges, false );

                        // Save all the changes
                        rockContext.SaveChanges();

                        // Since we just succesfully saved the change, increment the process counter
                        peopleProcessed++;

                        // If configured to do so, add any parent relationships (these methods take care of logging changes)
                        if ( parentRelationshipId.HasValue )
                        {
                            foreach ( int parentId in parentIds )
                            {
                                groupMemberService.CreateKnownRelationship( personId, parentId, parentRelationshipId.Value );
                            }
                        }

                        // If configured to do so, add any sibling relationships
                        if ( siblingRelationshipId.HasValue )
                        {
                            foreach ( int siblingId in siblingIds )
                            {
                                groupMemberService.CreateKnownRelationship( personId, siblingId, siblingRelationshipId.Value );
                            }
                        }

                        // Look for any workflows
                        if ( workflows.Any() )
                        {
                            // Create parameters for the old/new family
                            var workflowParameters = new Dictionary<string, string>
                            {
                                { "OldFamily", primaryFamily.Guid.ToString() },
                                {  "NewFamily", newFamily.Guid.ToString() }
                            };

                            // Launch all the workflows
                            foreach ( var wfGuid in workflows )
                            {
                                person.LaunchWorkflow( wfGuid, person.FullName, workflowParameters );
                            }
                        }
                    }
                }

                // Format the result message
                context.Result = peopleProcessed == 1 ? "1 person was processed" : $"{peopleProcessed:N0} people were processed";

            }
            catch ( System.Exception ex )
            {
                // If an exception happens, log the exception
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw ex;
            }
        }
    }
}
