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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    /// <summary>
    /// Removes groups that are proscribed by Lakepointe policy.
    /// </summary>
    [ActionCategory( "LPC > Check-In" )]
    [Description( "Removes groups that are proscribed by Lakepointe policy." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Remove Proscribed Groups" )]
    [Rock.SystemGuid.EntityTypeGuid( "F5843FD8-3B49-467C-81C1-23828D6A00E3" )]
    [GroupTypeField( "SmallGroupType", "Group Type for Children's Small Groups", true, "33aa4245-64a2-4413-ab6b-893c3f097aba" )]
    [GroupTypeField( "TempGroupType", "Group Type for Children's Temporary Groups", true, "9da37d01-e0c4-42f7-90f0-a35710d830c3" )]
    [GroupTypeField( "KidZoneGroupType", "Group Type for Kid Zone", true, "0f4d9344-c0be-4045-8426-f5e8f464e046" )]
    [GroupTypeField( "SoarGroupType", "Group Type for Soar Groups", true, "41e06b62-9f24-4c34-a4e5-4b76b2fec618" )]
    [GroupTypeField( "StudentConnectGroupType", "Group Type for Student Connect Groups", true, "eb80b420-988e-4898-8ee5-67c96c9f0b18" )]
    [GroupTypeField( "StudentUnitedGroupType", "Group Type for Student United Groups", true, "19051a16-0437-49b8-a0b2-1533f529222e" )]
    [GroupTypeField( "StudentUnassignedGroupType", "Group Type for Unassigned Student Groups", true, "b9c96802-c955-4892-b935-2e46f66adb55" )]
    [GroupTypeField( "PreschoolSmallGroupsGroupType", "Group Type for Preschool Small Groups", true, "467cea4d-1620-4d84-ac55-a85ad668fd43" )]
    [BooleanField( "KidZoneSupport", "Is Kid Zone Programming Supported", true )]

    public class RemoveProscribedOpportunities : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                foreach ( var family in checkInState.CheckIn.Families.ToList() )
                {
                    foreach ( var person in family.People.ToList() )
                    {
                        Debug.WriteLine( $"Processing {person.Person.FullName}" );

                        RemoveGroupsForSchedulesNotSelected( rockContext, action, person );
                        ApplySmallGroupKidsTownPolicy( rockContext, action, person );
                        ApplyStudentUnassignedPolicy( action, person );
                        ApplyStudentChildrenLeaderPolicy( rockContext, action, person );

                        // Before adding other policies here, consider whether it would be better to put each policy in a separate CheckInActionComponent (workflow action)
                    }
                }

                return true;
            }

            return false;
        }

        // Rock assumes families all check in to schedules together. Families at Lakepointe don't do that. One person in a
        // family might be checked into first hour where another is checked into second hour. So we've added
        // CheckinPerson.PersonSelectedSchedules to disambiguate. Here, we'll remove options that shouldn't be considered
        // because the person indicated they weren't checking in during that hour.
        private void RemoveGroupsForSchedulesNotSelected( RockContext rockContext, WorkflowAction action, CheckInPerson person )
        {
            var scheduleIds = person.PersonSelectedSchedules?.Select( s => s.Schedule.Id ).ToList();
            if ( scheduleIds == null )
            {
                return;
            }

            foreach ( var group in person.GroupTypes.SelectMany( gt => gt.Groups ) )
            {
                var isNotAnOption = !group.Locations
                    .SelectMany( l => l.Schedules
                        .Where( s => scheduleIds.Contains( s.Schedule.Id ) ) )
                    .Any();

                group.ExcludedByFilter |= isNotAnOption;
            }
        }

        // gradeschool students can't check into KidsTown unless they've first attended a small group--unless there isn't a small group option available in the earliest hour.
        private void ApplySmallGroupKidsTownPolicy( RockContext rockContext, WorkflowAction action, CheckInPerson person )
        {
            // if person has no grade, is less than kindergarten (GradeOffset == 12), or greater than 5th grade (GradeOffset == 7), this policy doesn't apply
            if ( person.Person.GradeOffset == null || person.Person.GradeOffset > 12 || person.Person.GradeOffset < 7 )
            {
                Debug.WriteLine( $"Policy does not apply" );
                return;
            }

            var childrenSmallGroupTypeGuid = GetAttributeValue( action, "SmallGroupType" ).AsGuid();
            var tempSmallGroupTypeGuid = GetAttributeValue( action, "TempGroupType" ).AsGuid();
            var kidZoneGroupTypeGuid = GetAttributeValue( action, "KidZoneGroupType" ).AsGuid();
            var soarGroupTypeGuid = GetAttributeValue( action, "SoarGroupType" ).AsGuid();
            var kidZoneSupport = GetAttributeValue( action, "KidZoneSupport" ).AsBoolean();

            bool smallGroupAssigned = false;
            int kidZoneLocationId = 0;

            // Then we're going to need to organize the opportunities available to them chronologically by schedule.
            // Which hours they want to check the kid into has already been established so we don't need to worry about whether they're going to defer on a particular hour.
            // for each hour (in order)

            if ( person.PersonSelectedSchedules == null )
            {
                return;
            }

            foreach ( var schedule in person.PersonSelectedSchedules.OrderBy( s => s.StartTime ) )
            {
                Debug.WriteLine( $"Processing schedule {schedule.Schedule.Name}" );

                var groupsOnThisScheduleForThisKid = person.GroupTypes
                    .Where(  gt => gt.GroupType.Guid == childrenSmallGroupTypeGuid
                                || gt.GroupType.Guid == tempSmallGroupTypeGuid
                                || gt.GroupType.Guid == kidZoneGroupTypeGuid ) // only remove children's groups, not Soar groups or Serving groups
                    .SelectMany( gt => gt.Groups )
                    .Where( g => g.Locations.FirstOrDefault()?.Schedules
                        .Where( s => s.Schedule.Id == schedule.Schedule.Id )
                    .Any() ?? false );

                // check for small groups first
                if ( !smallGroupAssigned
                    || person.Person.GradeOffset < 9 ) // no large group option for 4/5, so offer a temporary group even if they're already in one at an earlier hour
                {
                    var groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                        .Where( gt => gt.GroupType.Guid == childrenSmallGroupTypeGuid )     // that are small groups
                        .SelectMany( gt => gt.Groups )
                        .Where( g => g.Locations
                            .FirstOrDefault()
                            ?.Schedules
                                .Where( s => s.Schedule.Id == schedule.Schedule.Id )   // whose CheckinSchedule (accessed via Location) is this CheckinSchedule
                            .Any() ?? false );

                    if ( groups.Any() )
                    {
                        Debug.WriteLine( $"Found {groups.Count()} small group candidates." );
                        var thisGroup = groups.First();
                        thisGroup.Selected = true;
                        kidZoneLocationId = GetKidZoneLocationId( rockContext, thisGroup );
                        smallGroupAssigned = kidZoneSupport; // True for check-in 2.0, False for check-in 2.1 where we want to continue looking for small groups
                        Debug.WriteLine( $"Selected {thisGroup.Group.Name}" );

                        // remove any other groups on this schedule
                        foreach ( var g in groupsOnThisScheduleForThisKid.Where( g => g.Group.Id != thisGroup.Group.Id ) )
                        {
                            Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                            g.ExcludedByFilter = true;
                        }

                        // and skip to the next schedule
                        continue;
                    }
                }

                // Look for a temporary group
                if ( !smallGroupAssigned
                    || person.Person.GradeOffset < 9 ) // no large group option for 4/5, so offer a temporary group even if they're already in one at an earlier hour
                {
                    var groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                        .Where( gt => gt.GroupType.Guid == tempSmallGroupTypeGuid )     // that are temporary groups
                        .SelectMany( gt => gt.Groups )
                        .Where( g => g.Locations    
                            .FirstOrDefault()
                            ?.Schedules
                                .Where( s => s.Schedule.Id == schedule.Schedule.Id )   // whose CheckinSchedule (accessed via Location) is this CheckinSchedule
                            .Any() ?? false );

                    if ( groups.Any() )
                    {
                        Debug.WriteLine( $"Found {groups.Count()} temporary group candidates." );
                        var thisGroup = groups.First();
                        thisGroup.Selected = true;
                        kidZoneLocationId = GetKidZoneLocationId( rockContext, thisGroup );
                        smallGroupAssigned = kidZoneSupport; // True for check-in 2.0, False for check-in 2.1 where we want to continue looking for small groups
                        Debug.WriteLine( $"Selected {thisGroup.Group.Name}" );

                        // remove any other groups on this schedule
                        foreach ( var g in groupsOnThisScheduleForThisKid.Where( g => g.Group.Id != thisGroup.Group.Id ) )
                        {
                            Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                            g.ExcludedByFilter = true;
                        }

                        // and skip to the next schedule
                        continue;
                    }
                }

                // look for a large group option
                // We don't worry about this being a K-3 kid here because no KZ groups would have been added to the kid's list before we got here
                var kidZoneGroups = person.GroupTypes   // groups (via GroupTypes) potentially associated with this person
                    .Where( gt => gt.GroupType.Guid == kidZoneGroupTypeGuid )  // that are kid zone groups
                    .SelectMany( gt => gt.Groups )
                    .Where( g => g.Locations
                        .FirstOrDefault()
                        ?.Schedules
                            .Where( s => s.Schedule.Id == schedule.Schedule.Id )  // whose CheckinSchedule (accessed via Location) is this CheckinSchedule
                        .Any() ?? false );

                Debug.WriteLine( $"Found {kidZoneGroups.Count()} KidZone groups." );
                if ( kidZoneGroups.Any() )
                {
                    if ( kidZoneLocationId != 0 )
                    {
                        // We're looking for the location ID of the kidzone group specified on the small group instead of the kidzone group
                        // itself to deal with third-hour scenarios where a kid might need to stay in the same kidzone location both the second
                        // and third hours.
                        var target = kidZoneGroups.Where( g => g.Locations.FirstOrDefault().Location.Id == kidZoneLocationId );
                        kidZoneGroups = target.Any() ? target : kidZoneGroups;
                    }
                    else
                    {
                        // If we get here, we're trying to assign a KZ group for a kid who hasn't been to small group yet. This isn't supposed to happen
                        // but it does for RW LPE where KZ is first and SG is second. So we need to look for a small group the kid is in that might
                        // be on a later schedule so we can figure out which KZ group to put them in.

                        var groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                           .Where( gt => gt.GroupType.Guid == childrenSmallGroupTypeGuid || gt.GroupType.Guid == tempSmallGroupTypeGuid )     // that are small or temporary groups
                           .SelectMany( gt => gt.Groups );
                        foreach (var sg in groups)
                        {
                            kidZoneLocationId = GetKidZoneLocationId( rockContext, sg );
                            if ( kidZoneLocationId != 0 )
                            {
                                var target = kidZoneGroups.Where( g => g.Locations.FirstOrDefault().Location.Id == kidZoneLocationId );
                                kidZoneGroups = target.Any() ? target : kidZoneGroups;
                                break;
                            }
                        }
                    }

                    var thisGroup = kidZoneGroups.First();
                    thisGroup.Selected = true;
                    Debug.WriteLine( $"Selected {thisGroup.Group.Name}" );

                    // remove any other groups on this schedule
                    foreach ( var g in groupsOnThisScheduleForThisKid.Where( g => g.Group.Id != thisGroup.Group.Id ) )
                    {
                        Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                        g.ExcludedByFilter = true;
                    }
                }
            }
        }

        private int GetKidZoneLocationId( RockContext rockContext, CheckInGroup thisGroup )
        {
            // We're looking for the location ID of the kidzone group specified on the small group instead of the kidzone group
            // itself to deal with third-hour scenarios where a kid might need to stay in the same kidzone location both the second
            // and third hours.

            var attr = thisGroup.Group.GetAttributeValue( "KidZoneGroup" );
            if (attr == null )
            {
                return 0;
            }

            var parts = attr.Split( '|' );
            if (parts.Length < 2)
            {
                return 0;
            }

            var kzGuid = parts[1].AsGuidOrNull();
            if (kzGuid == null)
            {
                return 0;
            }

            var kzGroup = new GroupService( rockContext ).Get( kzGuid.Value );
            if ( kzGroup == null )
            {
                return 0;
            }

            return kzGroup.GroupLocations.FirstOrDefault().Location.Id;
        }

        // Removes unassigned groups as an option for students who already belong to a small group
        private void ApplyStudentUnassignedPolicy( WorkflowAction action, CheckInPerson person )
        {
            // if person has no grade, is less than sixth (GradeOffset == 6), or greater than 12th grade (GradeOffset == 0), this policy doesn't apply
            if (person.Person.GradeOffset == null || person.Person.GradeOffset > 6 || person.Person.GradeOffset < 0)
            {
                Debug.WriteLine( $"Policy does not apply" );
                return;
            }

            if (person.PersonSelectedSchedules == null)
            {
                return;
            }

            var studentConnectGroupType = GetAttributeValue( action, "StudentConnectGroupType" ).AsGuid();
            var studentUnitedGroupType = GetAttributeValue( action, "StudentUnitedGroupType" ).AsGuid();
            var studentUnassignedGroupType = GetAttributeValue( action, "StudentUnassignedGroupType" ).AsGuid();
            
            // Then we're going to need to organize the opportunities available to them chronologically by schedule.
            // They _should_ have only checked into one hour. If they checked more than one, we're going to put them
            // in the first and then dump out.

            var groupsOnAnyScheduleForThisKid = person.GroupTypes.SelectMany( gt => gt.Groups );

            foreach (var schedule in person.PersonSelectedSchedules.OrderBy( s => s.StartTime ))
            {
                Debug.WriteLine( $"Processing schedule {schedule.Schedule.Name}" );

                // check for small groups first
                var groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                    .Where( gt => gt.GroupType.Guid == studentConnectGroupType      // that are student connect groups
                               || gt.GroupType.Guid == studentUnitedGroupType )     // or student united groups
                    .SelectMany( gt => gt.Groups )
                    .Where( g => g.Locations
                        .FirstOrDefault()
                        ?.Schedules
                            .Where( s => s.Schedule.Id == schedule.Schedule.Id )   // whose CheckinSchedule (accessed via Location) is this CheckinSchedule
                        .Any() ?? false );

                if (groups.Any())
                {
                    Debug.WriteLine( $"Found {groups.Count()} student connect/united group candidates." );
                    var thisGroup = groups.First();
                    thisGroup.Selected = true;
                    Debug.WriteLine( $"Selected {thisGroup.Group.Name}" );

                    // remove any other groups on this schedule
                    foreach (var g in groupsOnAnyScheduleForThisKid.Where( g => g.Group.Id != thisGroup.Group.Id ))
                    {
                        Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                        g.ExcludedByFilter = true;
                    }

                    return;  // and bail out
                }

                // Look for a temporary group
                groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                    .Where( gt => gt.GroupType.Guid == studentUnassignedGroupType )     // that are unassigned groups
                    .SelectMany( gt => gt.Groups )
                    .Where( g => g.Locations
                        .FirstOrDefault()
                        ?.Schedules
                            .Where( s => s.Schedule.Id == schedule.Schedule.Id )   // whose CheckinSchedule (accessed via Location) is this CheckinSchedule
                        .Any() ?? false );

                if (groups.Any())
                {
                    Debug.WriteLine( $"Found {groups.Count()} unassigned group candidates." );
                    var thisGroup = groups.First();
                    thisGroup.Selected = true;
                    Debug.WriteLine( $"Selected {thisGroup.Group.Name}" );

                    // remove any other groups on this schedule
                    foreach (var g in groupsOnAnyScheduleForThisKid.Where( g => g.Group.Id != thisGroup.Group.Id ))
                    {
                        Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                        g.ExcludedByFilter = true;
                    }

                    return;  // and bail out
                }
            }
        }

        // Removes student/child groups as an option for adult leaders
        private void ApplyStudentChildrenLeaderPolicy( RockContext rockContext, WorkflowAction action, CheckInPerson person )
        {
            if ( person.PersonSelectedSchedules == null )
            {
                return;
            }

            var studentConnectGroupType = GetAttributeValue( action, "StudentConnectGroupType" ).AsGuid();
            var studentUnitedGroupType = GetAttributeValue( action, "StudentUnitedGroupType" ).AsGuid();
            var childrenSmallGroupTypeGuid = GetAttributeValue( action, "SmallGroupType" ).AsGuid();
            var soarGroupTypeGuid = GetAttributeValue( action, "SoarGroupType" ).AsGuid();
            var preschoolGroupTypeGuid = GetAttributeValue( action, "PreschoolSmallGroupsGroupType" ).AsGuid();

            Debug.WriteLine( $"Processing kid groups where person is a leader." );

            // check for kid groups where the person is in a leader role and remove those groups
            // (they should be checking into a serve team instead)
            var groups = person.GroupTypes  // groups (via GroupTypes) potentially associated with this person
                .Where( gt => gt.GroupType.Guid == studentConnectGroupType       // that are student connect groups
                           || gt.GroupType.Guid == studentUnitedGroupType       // or student united groups
                           || gt.GroupType.Guid == childrenSmallGroupTypeGuid   // or children's small groups
                           || gt.GroupType.Guid == soarGroupTypeGuid            // or soar small groups
                           || gt.GroupType.Guid == preschoolGroupTypeGuid )     // or preschool small groups
                .SelectMany( gt => gt.Groups );
                //.Where( g => g.Group.Members.Any( gm => gm.GroupRole.IsLeader && gm.PersonId == person.Person.Id )); // where this person is in a leader role
                // Elegant and tempting, but Members isn't populated in the CheckinGroup data structure.
                // Do this below instead.

            if ( groups.Any() )
            {
                Debug.WriteLine( $"Found {groups.Count()} kid groups where this person is a member." );
                var gms = new GroupMemberService( rockContext );

                foreach ( var g in groups )
                {
                    var isLeader = gms.GetByGroupIdAndPersonId( g.Group.Id, person.Person.Id )
                        .Any( m => m.GroupRole.IsLeader );

                    if ( isLeader )
                    {
                        Debug.WriteLine( $"Filtering group {g.Group.Name}" );
                        g.ExcludedByFilter = true;
                    }
                }
            }
        }
    }
}