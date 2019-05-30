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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    [ActionCategory( "Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that doesn't meet the 'Attendance Record Required For Check-in' requirement for the group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Scheduled" )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByScheduled : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
                return false;

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family == null )
                return true;

            var remove = GetAttributeValue( action, "Remove" ).AsBoolean();


            // see if there are any groups for the people in this family that have a AttendanceRecordRequiredForCheckIn requirement that needs to be checked
            bool anyScheduledRequirements = family.People
                .SelectMany( a => a.GroupTypes )
                .SelectMany( a => a.Groups.Where( g => g.Group.AttendanceRecordRequiredForCheckIn != AttendanceRecordRequiredForCheckIn.AllShow ) ).Any();

            if ( !anyScheduledRequirements )
            {
                return true;
            }

            var personIds = family.People.Select( p => p.Person.Id ).ToList();

            var today = RockDateTime.Today;

            var attendanceScheduledQuery = new AttendanceService( rockContext ).Queryable()
                .Where( a =>
                     a.ScheduledToAttend == true &&
                     a.RSVP == RSVP.Yes &&
                     personIds.Contains( a.PersonAlias.PersonId ) &&
                     a.Occurrence.OccurrenceDate == today.Date &&
                     a.Occurrence.GroupId.HasValue &&
                     a.Occurrence.LocationId.HasValue &&
                     a.Occurrence.ScheduleId.HasValue );
            
            var attendanceScheduledLookup = attendanceScheduledQuery.Select( a => new
            {
                a.PersonAlias.PersonId,
                GroupId = a.Occurrence.GroupId.Value,
                LocationId = a.Occurrence.LocationId.Value,
                ScheduleId = a.Occurrence.ScheduleId.Value
            } ).ToList();

            foreach ( var person in family.People.Where( p => p.Person.LastName.Length > 0 ) )
            {
                foreach ( var groupType in person.GroupTypes.ToList() )
                {
                    foreach ( var group in groupType.Groups.ToList() )
                    {
                        var selectedLocations = group.GetLocations( true );

                        if ( group.Group.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.RequireAttendanceRecord || group.Group.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.UseAttendanceRecordAsPreference )
                        {
                            foreach ( var location in group.GetLocations( true ) )
                            {
                                foreach ( var schedule in location.GetSchedules( true ) )
                                {
                                    bool isScheduled = attendanceScheduledLookup.Any( a =>
                                      a.PersonId == person.Person.Id &&
                                      a.GroupId == group.Group.Id &&
                                      a.LocationId == location.Location.Id &&
                                      a.ScheduleId == schedule.Schedule.Id
                                     );

                                    if ( isScheduled == false )
                                    {
                                        // if they aren't scheduled, and the group requires that the person be already scheduled, then remove the group for that person
                                        if ( group.Group.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.RequireAttendanceRecord )
                                        {
                                            if ( remove )
                                            {
                                                groupType.Groups.Remove( group );
                                            }
                                            else
                                            {
                                                group.ExcludedByFilter = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // if the person is scheduled, default that group as selected
                                        group.Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}