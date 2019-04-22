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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Calculates and updates the LastCheckIn property on check-in objects
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Calculates and updates the LastCheckIn property on check-in objects" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Calculate Last Attended" )]
    public class CalculateLastAttended : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                DateTime preSelectCutoff = RockDateTime.Today.AddDays( 0 - checkInState.CheckInType.AutoSelectDaysBack );

                var attendanceService = new AttendanceService( rockContext );

                // Find all the schedules that are used for checkin
                var checkinSchedules = new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                    .ToList();

                DateTime sixMonthsAgo = RockDateTime.Today.AddMonths( -6 );

                foreach ( var family in checkInState.CheckIn.GetFamilies( true ) )
                {
                    foreach ( var person in family.People )
                    {
                        // Find all of the attendance records for this person for any of the check-in types that are currently available for this kiosk and person
                        var groupTypeIds = person.GroupTypes.Select( t => t.GroupType.Id ).ToList();
                        var personAttendance = attendanceService
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                a.PersonAlias != null &&
                                a.Occurrence.Group != null &&
                                a.Occurrence.Schedule != null &&
                                a.PersonAlias.PersonId == person.Person.Id &&
                                groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) &&
                                a.StartDateTime >= sixMonthsAgo &&
                                a.DidAttend.HasValue &&
                                a.DidAttend.Value == true )
                            .Select( a => new
                            {
                                a.Id,
                                a.StartDateTime,
                                a.EndDateTime,
                                PersonId = a.PersonAlias.PersonId,
                                GroupTypeId = a.Occurrence.Group.GroupTypeId,
                                GroupId = a.Occurrence.GroupId,
                                LocationId = a.Occurrence.LocationId,
                                ScheduleId = a.Occurrence.ScheduleId,
                                Schedule = a.Occurrence.Schedule
                            } )
                            .ToList();

                        // If there are any previous attendance records start evaluating them for last checkin and if options should be preselected.
                        if ( personAttendance.Any() )
                        {
                            person.FirstTime = false;

                            // Get the datetime that person last checked in
                            person.LastCheckIn = personAttendance.Max( a => a.StartDateTime );

                            // If the date they last checked in is greater than the PreSelect cutoff date, then get all the group/location/schedules 
                            // that the person checked into on that date (if they somehow checked into multiple group/locations during same schedule, 
                            // only consider the the most recent group/location per schedule
                            var previousCheckins = new List<CheckinInfo>();
                            if ( person.LastCheckIn.HasValue &&
                                 preSelectCutoff.CompareTo( RockDateTime.Today ) < 0 &&
                                 person.LastCheckIn.Value.CompareTo( preSelectCutoff  ) >= 0 )
                            {
                                foreach ( var item in personAttendance
                                    .Where( a => a.StartDateTime.Date == person.LastCheckIn.Value.Date )
                                    .OrderBy( a => a.Schedule.StartTimeOfDay )
                                    .ThenByDescending( a => a.StartDateTime ) )
                                {
                                    if ( item.ScheduleId.HasValue && !previousCheckins.Any( i => i.ScheduleId == item.ScheduleId ) )
                                    {
                                        previousCheckins.Add( new CheckinInfo
                                        {
                                            ScheduleId = item.ScheduleId.Value,
                                            GroupId = item.GroupId.Value,
                                            LocationId = item.LocationId
                                        } );
                                    }
                                }
                            }
                            var selectedSchedules = new List<int>();    // Used to keep track of which schedules have already had options preselected.

                            foreach ( var groupType in person.GroupTypes )
                            {
                                // Find the previous attendance for this group type and save the most recent attendance date
                                var groupTypeAttendance = personAttendance.Where( a => a.GroupTypeId == groupType.GroupType.Id ).ToList();
                                if ( groupTypeAttendance.Any() )
                                {
                                    groupType.LastCheckIn = groupTypeAttendance.Max( a => a.StartDateTime );
                                }

                                foreach ( var group in groupType.Groups )
                                {
                                    // Find the previous attendance for this group and save the most recent attendance date
                                    var groupAttendance = groupTypeAttendance.Where( a => a.GroupId == group.Group.Id ).ToList();
                                    if ( groupAttendance.Any() )
                                    {
                                        group.LastCheckIn = groupAttendance.Max( a => a.StartDateTime );
                                    }

                                    // If person checked into this group on last visit, preselect it for now
                                    var previousGroupCheckins = previousCheckins.Where( c => c.GroupId == group.Group.Id ).ToList();
                                    if ( previousGroupCheckins.Any() )
                                    {
                                        group.PreSelected = true;
                                    }

                                    foreach ( var location in group.Locations )
                                    {
                                        // Find the previous attendance for this location and save the most recent attendance date
                                        var locationAttendance = groupAttendance.Where( a => a.LocationId == location.Location.Id ).ToList();
                                        if ( locationAttendance.Any() )
                                        {
                                            location.LastCheckIn = locationAttendance.Max( a => a.StartDateTime );
                                        }

                                        if ( group.PreSelected )
                                        {
                                            var previousLocationCheckins = previousGroupCheckins.Where( c => c.LocationId == location.Location.Id );
                                            if ( previousLocationCheckins.Any() )
                                            {
                                                // If person checked into this group and location on last visit, preselect the location for now
                                                location.PreSelected = true;

                                                // Try to find a schedule
                                                var availableSchedules = location.Schedules
                                                    .Where( s => !selectedSchedules.Contains( s.Schedule.Id ) )
                                                    .OrderBy( s => s.StartTime );
                                                foreach( var schedule in availableSchedules )
                                                {
                                                    // If person checked into this group/location/schedule on last visit, preselect it for now
                                                    if ( previousLocationCheckins.Any( c => c.ScheduleId == schedule.Schedule.Id ) )
                                                    {
                                                        schedule.PreSelected = true;
                                                        selectedSchedules.Add( schedule.Schedule.Id );
                                                    }
                                                }
                                                if ( !location.Schedules.Any( s => s.PreSelected ) && availableSchedules.Any() )
                                                {
                                                    var schedule = availableSchedules.First();
                                                    schedule.PreSelected = true;;
                                                    selectedSchedules.Add( schedule.Schedule.Id );
                                                }
                                            }
                                        }

                                        // Find the active schedules for this location (campus)
                                        var locationDateTime = RockDateTime.Now;
                                        if ( location.CampusId.HasValue )
                                        {
                                            locationDateTime = CampusCache.Get( location.CampusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
                                        }
                                        var activeScheduleIds = new List<int>();
                                        foreach( var schedule in checkinSchedules )
                                        {
                                            if ( schedule.WasScheduleOrCheckInActive( locationDateTime ) )
                                            {
                                                activeScheduleIds.Add( schedule.Id );
                                            }
                                        }

                                        // Check to see if the person is still checked into this grouptype/group/location combination
                                        var activeAttendanceIds = locationAttendance
                                            .Where( a =>
                                                a.StartDateTime > DateTime.Today &&
                                                a.ScheduleId.HasValue &&
                                                activeScheduleIds.Contains( a.ScheduleId.Value ) &&
                                                !a.EndDateTime.HasValue ) 
                                            .Select( a => a.Id )
                                            .ToList();

                                        // If so, allow person to check-out.
                                        if ( activeAttendanceIds.Any() )
                                        {
                                            var checkOutPerson = family.CheckOutPeople.FirstOrDefault( p => p.Person.Id == person.Person.Id );
                                            if ( checkOutPerson == null )
                                            {
                                                checkOutPerson = new Rock.CheckIn.CheckOutPerson();
                                                checkOutPerson.Person = person.Person;
                                                family.CheckOutPeople.Add( checkOutPerson );
                                            }
                                            checkOutPerson.AttendanceIds.AddRange( activeAttendanceIds );
                                        }
                                    }

                                    // If the group was preselected, but could not preselect the location, try to preselect the first location and schedule
                                    if ( group.PreSelected && !group.Locations.Any( l => l.PreSelected ) && group.Locations.Any() )
                                    {
                                        var location = group.Locations.First();
                                        var schedule = location.Schedules
                                                    .Where( s => !selectedSchedules.Contains( s.Schedule.Id ) )
                                                    .OrderBy( s => s.StartTime )
                                                    .FirstOrDefault();
                                        if ( schedule != null )
                                        {
                                            location.PreSelected = true;
                                            schedule.PreSelected = true;
                                            selectedSchedules.Add( schedule.Schedule.Id );
                                        }
                                    }

                                    // If there were still not any location/schedules able to be preselected for this group, unselect the group.
                                    if ( group.PreSelected && !group.Locations.Any( l => l.PreSelected ) )
                                    {
                                        group.PreSelected = false;
                                    }
                                }

                                groupType.PreSelected = groupType.Groups.Any( g => g.PreSelected );
                            }

                            person.PreSelected = person.GroupTypes.Any( t => t.PreSelected );
                        }
                        else
                        {
                            // Person hasn't had any attendance to selected group types in last 6 months, so check to see if this is the 
                            // first time that this person has ever checked into anything
                            person.FirstTime = !attendanceService
                                .Queryable().AsNoTracking()
                                .Where( a => a.PersonAlias.PersonId == person.Person.Id )
                                .Any();
                        }
                    }
                }

                return true;

            }

            return false;
        }

        /// <summary>
        /// Helper Class for storing the combination of schedule/grouptype/group/location that person last checked into
        /// </summary>
        public class CheckinInfo
        {
            /// <summary>
            /// Gets or sets the schedule identifier.
            /// </summary>
            /// <value>
            /// The schedule identifier.
            /// </value>
            public int ScheduleId { get; set; }

            /// <summary>
            /// Gets or sets the group type identifier.
            /// </summary>
            /// <value>
            /// The group type identifier.
            /// </value>
            public int GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the location identifier.
            /// </summary>
            /// <value>
            /// The location identifier.
            /// </value>
            public int? LocationId { get; set; }
        }
    }
}