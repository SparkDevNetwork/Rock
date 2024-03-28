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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Loads the schedules available for each group
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Loads the schedules available for each group" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Load Schedules" )]

    [BooleanField(
        "Load All",
        Key = AttributeKey.LoadAll,
        Description = "By default schedules are only loaded for the selected person, group type, location, and group. Select this option to load schedules for all the loaded people, group types, locations, and groups.",
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Remove",
        Key = AttributeKey.Remove,
        Description = "Select 'Yes' if a person a should be removed from a group's scheduled location if they don't meet the 'Attendance Record Required For Check-in' requirement for the group. Select 'No' if they should just be marked as excluded.",
        DefaultBooleanValue = true,
        Order = 2 )]
    [Rock.SystemGuid.EntityTypeGuid( "24A7E196-B50B-4BD6-A347-07CFC5ABEF9E")]
    public class LoadSchedules : CheckInActionComponent
    {
        /// <summary>
        /// 
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The load all
            /// </summary>
            public const string LoadAll = "LoadAll";

            /// <summary>
            /// The remove
            /// </summary>
            public const string Remove = "Remove";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );

            if ( checkInState == null )
            {
                return false;
            }

            bool loadSelectedOnly = GetAttributeValue( action, AttributeKey.LoadAll ).AsBoolean() == false;
            var today = RockDateTime.Today;

            var kioskGroupTypeLookup = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes ).ToDictionary( k => k.GroupType.Id, v => v );

            var remove = GetAttributeValue( action, AttributeKey.Remove ).AsBoolean();

            foreach ( var family in checkInState.CheckIn.GetFamilies( true ) )
            {
                // see if there are any groups for the people in this family that have a AttendanceRecordRequiredForCheckIn requirement that needs to be checked
                int[] groupIdsWithScheduledRequirements = family.People
                    .SelectMany( a => a.GroupTypes )
                    .SelectMany( a => a.Groups.Where( g => g.Group.AttendanceRecordRequiredForCheckIn != AttendanceRecordRequiredForCheckIn.ScheduleNotRequired ) )
                    .Select( a => a.Group.Id ).ToArray();

                List<PersonAttendanceScheduled> attendanceScheduledLookup = null;

                if ( groupIdsWithScheduledRequirements.Any() )
                {
                    var personIds = family.People.Select( p => p.Person.Id ).ToList();

                    // get attendance records where the person was scheduled (doesn't matter if they confirmed or declined)
                    var attendanceScheduledQuery = new AttendanceService( rockContext ).Queryable()
                        .Where( a =>
                             ( a.ScheduledToAttend == true || a.RequestedToAttend == true ) &&
                             personIds.Contains( a.PersonAlias.PersonId ) &&
                             a.Occurrence.OccurrenceDate == today.Date &&
                             a.Occurrence.GroupId.HasValue &&
                             a.Occurrence.LocationId.HasValue &&
                             a.Occurrence.ScheduleId.HasValue &&
                             groupIdsWithScheduledRequirements.Contains( a.Occurrence.GroupId.Value ) );

                    attendanceScheduledLookup = attendanceScheduledQuery.Select( a => new PersonAttendanceScheduled
                    {
                        PersonId = a.PersonAlias.PersonId,
                        GroupId = a.Occurrence.GroupId.Value,
                        LocationId = a.Occurrence.LocationId.Value,
                        ScheduleId = a.Occurrence.ScheduleId.Value
                    } ).ToList();
                }

                foreach ( var person in family.GetPeople( loadSelectedOnly ) )
                {
                    LoadPersonSchedules( person, attendanceScheduledLookup, kioskGroupTypeLookup, loadSelectedOnly, remove, checkInState );
                }
            }

            return true;
        }

        /// <summary>
        /// Loads the person schedules.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="attendanceScheduledLookup">The attendance scheduled lookup.</param>
        /// <param name="kioskGroupTypeLookup">The kiosk group type lookup.</param>
        /// <param name="loadSelectedOnly">if set to <c>true</c> [load selected only].</param>
        /// <param name="remove">if set to <c>true</c> [remove], schedule will be removed vs marked excluded when doesn't meet requirements</param>
        /// <param name="checkInState">State of the check in.</param>
        private void LoadPersonSchedules( CheckInPerson person, List<PersonAttendanceScheduled> attendanceScheduledLookup, Dictionary<int, KioskGroupType> kioskGroupTypeLookup, bool loadSelectedOnly, bool remove, CheckInState checkInState )
        {
            var personGroups = person.GetGroupTypes( loadSelectedOnly ).SelectMany( gt => gt.GetGroups( loadSelectedOnly ) ).ToList();

            var anyAvailableGroups = false;
            var anyAvailableSchedules = false;

            foreach ( var group in personGroups )
            {
                var kioskGroup = kioskGroupTypeLookup.GetValueOrNull( group.Group.GroupTypeId )?.KioskGroups?.Where( g => g.Group.Id == group.Group.Id && g.IsCheckInActive )
                                .FirstOrDefault();

                if ( kioskGroup == null )
                {
                    continue;
                }

                anyAvailableGroups = true;

                var groupType = GroupTypeCache.Get( group.Group.GroupTypeId );

                foreach ( var location in group.GetLocations( loadSelectedOnly ) )
                {
                    var kioskLocation = kioskGroup.KioskLocations
                        .Where( l => l.Location.Id == location.Location.Id && l.IsCheckInActive )
                        .FirstOrDefault();

                    if ( kioskLocation == null )
                    {
                        continue;
                    }

                    foreach ( var kioskSchedule in kioskLocation.KioskSchedules.Where( s => s.IsCheckInActive ) )
                    {
                        bool? personIsScheduled = null;
                        var attendanceRecordRequiredForCheckIn = group.Group.AttendanceRecordRequiredForCheckIn;

                        // if the groupType currently doesn't have scheduling enabled, ignore the group's AttendanceRecordRequiredForCheckIn setting, and just have it be ScheduleNotRequired
                        if ( groupType.IsSchedulingEnabled == false )
                        {
                            attendanceRecordRequiredForCheckIn = AttendanceRecordRequiredForCheckIn.ScheduleNotRequired;
                        }

                        // if ScheduleRequired or PreSelect, we'll need to see if the person is scheduled
                        if ( group.Group.AttendanceRecordRequiredForCheckIn != AttendanceRecordRequiredForCheckIn.ScheduleNotRequired )
                        {
                            personIsScheduled = attendanceScheduledLookup?.Any( a =>
                                                      a.PersonId == person.Person.Id &&
                                                      a.GroupId == group.Group.Id &&
                                                      a.LocationId == location.Location.Id &&
                                                      a.ScheduleId == kioskSchedule.Schedule.Id );
                        }

                        bool excludeSchedule = false;

                        if ( group.Group.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.ScheduleRequired )
                        {
                            if ( personIsScheduled != true )
                            {
                                // don't add to the person's available schedules
                                excludeSchedule = true;
                            }
                        }

                        bool preSelectForOccurrence = false;

                        if ( group.Group.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.PreSelect )
                        {
                            preSelectForOccurrence = personIsScheduled == true;
                        }

                        location.PreSelected = preSelectForOccurrence;

                        if ( excludeSchedule && remove )
                        {
                            // the schedule doesn't meet requirements, and the option for Excluding vs Removing is remove, so don't add it
                            continue;
                        }

                        if ( !location.Schedules.Any( s => s.Schedule.Id == kioskSchedule.Schedule.Id ) )
                        {
                            var checkInSchedule = new CheckInSchedule();
                            checkInSchedule.Schedule = kioskSchedule.Schedule.Clone( false );
                            checkInSchedule.CampusId = kioskSchedule.CampusId;
                            checkInSchedule.StartTime = kioskSchedule.StartTime;
                            checkInSchedule.PreSelected = preSelectForOccurrence;
                            checkInSchedule.ExcludedByFilter = excludeSchedule;
                            location.Schedules.Add( checkInSchedule );
                        }

                        if ( checkInState.CheckInType?.TypeOfCheckin == TypeOfCheckin.Family &&
                            !person.PossibleSchedules.Any( s => s.Schedule.Id == kioskSchedule.Schedule.Id ) )
                        {
                            var checkInSchedule = new CheckInSchedule();
                            checkInSchedule.Schedule = kioskSchedule.Schedule.Clone( false );
                            checkInSchedule.CampusId = kioskSchedule.CampusId;
                            checkInSchedule.StartTime = kioskSchedule.StartTime;
                            checkInSchedule.PreSelected = preSelectForOccurrence;
                            checkInSchedule.ExcludedByFilter = excludeSchedule;

                            person.PossibleSchedules.Add( checkInSchedule );
                        }

                        if ( !excludeSchedule )
                        {
                            anyAvailableSchedules = true;
                        }
                    }
                }
            }

            // If this person has no groups or schedules available, set their "No Option Reason".
            if ( !anyAvailableGroups )
            {
                person.NoOptionReason = "No Matching Groups Found";
            }
            else if ( !anyAvailableSchedules )
            {
                // Using "Locations" rather than "Schedules" within this message makes more sense
                // here, since we're showing this message to individuals attempting to check in.
                person.NoOptionReason = "No Locations Available";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PersonAttendanceScheduled
        {
            public int PersonId { get; set; }

            public int GroupId { get; set; }

            public int LocationId { get; set; }

            public int ScheduleId { get; set; }
        }
    }
}