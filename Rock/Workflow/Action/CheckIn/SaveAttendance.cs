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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Saves the selected check-in data as attendance
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Saves the selected check-in data as attendance" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Save Attendance" )]

    [BooleanField( "Enforce Strict Location Threshold", "If enabled, the room capacity will be re-checked prior to saving attendance to reduce the possibility surpassing the room’s capacity threshold.", false, "", 0 )]
    [CodeEditorField( "Not Checked-In Message Format", " <span class='tip tip-lava'></span>", Web.UI.Controls.CodeEditorMode.Lava, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, @"
{{ Person.FullName }} was not able to be checked into {{ Group.Name }} in {{ Location.Name }} at {{ Schedule.Name }} due to a capacity limit. Please re-check {{ Person.NickName }} in to a new room.", "", 1 )]
    public class SaveAttendance : CheckInActionComponent
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
                AttendanceCode attendanceCode = null;

                bool reuseCodeForFamily = checkInState.CheckInType != null && checkInState.CheckInType.ReuseSameCode;
                int securityCodeAlphaNumericLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeAlphaNumericLength : 3;
                int securityCodeAlphaLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeAlphaLength : 0;
                int securityCodeNumericLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeNumericLength : 0;
                bool securityCodeNumericRandom = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeNumericRandom : true;


                var attendanceCodeService = new AttendanceCodeService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var attendanceRecords = new List<Attendance>();

                checkInState.Messages.Clear();

                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var currentOccurences = new List<OccurenceRecord>();
                    foreach ( var person in family.GetPeople( true ) )
                    {
                        if ( reuseCodeForFamily && attendanceCode != null )
                        {
                            person.SecurityCode = attendanceCode.Code;
                        }
                        else
                        {
                            attendanceCode = AttendanceCodeService.GetNew( securityCodeAlphaNumericLength, securityCodeAlphaLength, securityCodeNumericLength, securityCodeNumericRandom );
                            person.SecurityCode = attendanceCode.Code;
                        }

                        foreach ( var groupType in person.GetGroupTypes( true ) )
                        {
                            foreach ( var group in groupType.GetGroups( true ) )
                            {
                                if ( groupType.GroupType.AttendanceRule == AttendanceRule.AddOnCheckIn &&
                                    groupType.GroupType.DefaultGroupRoleId.HasValue &&
                                    !groupMemberService.GetByGroupIdAndPersonId( group.Group.Id, person.Person.Id, true ).Any() )
                                {
                                    var groupMember = new GroupMember();
                                    groupMember.GroupId = group.Group.Id;
                                    groupMember.PersonId = person.Person.Id;
                                    groupMember.GroupRoleId = groupType.GroupType.DefaultGroupRoleId.Value;
                                    groupMemberService.Add( groupMember );
                                }

                                foreach ( var location in group.GetLocations( true ) )
                                {
                                    bool isCheckedIntoLocation = false;
                                    foreach ( var schedule in location.GetSchedules( true ) )
                                    {
                                        var startDateTime = schedule.CampusCurrentDateTime;

                                        // If we're enforcing strict location thresholds, then before we create an attendance record
                                        // we need to check the location-schedule's current count.
                                        if ( GetAttributeValue( action, "EnforceStrictLocationThreshold" ).AsBoolean() && location.Location.SoftRoomThreshold.HasValue )
                                        {
                                            var thresHold = location.Location.SoftRoomThreshold.Value;
                                            if ( checkInState.ManagerLoggedIn && location.Location.FirmRoomThreshold.HasValue && location.Location.FirmRoomThreshold.Value > location.Location.SoftRoomThreshold.Value )
                                            {
                                                thresHold = location.Location.FirmRoomThreshold.Value;
                                            }

                                            var currentOccurence = GetCurrentOccurence( currentOccurences, location, schedule, startDateTime.Date );

                                            // The totalAttended is the number of people still checked in (not people who have been checked-out)
                                            // not counting the current person who may already be checked in,
                                            // + the number of people we have checked in so far (but haven't been saved yet).
                                            var attendanceQry = attendanceService.GetByDateOnLocationAndSchedule( startDateTime.Date, location.Location.Id, schedule.Schedule.Id )
                                                .AsNoTracking()
                                                .Where( a => a.EndDateTime == null );

                                            // Only process if the current person is NOT already checked-in to this location and schedule
                                            if ( !attendanceQry.Where( a => a.PersonAlias.PersonId == person.Person.Id ).Any() )
                                            {
                                                var totalAttended = attendanceQry.Count() + ( currentOccurence == null ? 0 : currentOccurence.Count );

                                                // If over capacity, remove the schedule and add a warning message.
                                                if ( totalAttended >= thresHold )
                                                {
                                                    // Remove the schedule since the location was full for this schedule.  
                                                    location.Schedules.Remove( schedule );

                                                    var message = new CheckInMessage()
                                                    {
                                                        MessageType = MessageType.Warning
                                                    };

                                                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                                                    mergeFields.Add( "Person", person.Person );
                                                    mergeFields.Add( "Group", group.Group );
                                                    mergeFields.Add( "Location", location.Location );
                                                    mergeFields.Add( "Schedule", schedule.Schedule );
                                                    message.MessageText = GetAttributeValue( action, "NotChecked-InMessageFormat" ).ResolveMergeFields( mergeFields );

                                                    // Now add it to the check-in state message list for others to see.
                                                    checkInState.Messages.Add( message );
                                                    continue;
                                                }
                                                else
                                                {
                                                    // Keep track of anyone who was checked in so far.
                                                    if ( currentOccurence == null )
                                                    {
                                                        currentOccurence = new OccurenceRecord()
                                                        {
                                                            Date = startDateTime.Date,
                                                            LocationId = location.Location.Id,
                                                            ScheduleId = schedule.Schedule.Id
                                                        };
                                                        currentOccurences.Add( currentOccurence );
                                                    }

                                                    currentOccurence.Count += 1;
                                                }
                                            }
                                        }

                                        // Only create one attendance record per day for each person/schedule/group/location
                                        var attendance = attendanceService.Get( startDateTime, location.Location.Id, schedule.Schedule.Id, group.Group.Id, person.Person.Id );
                                        if ( attendance == null )
                                        {
                                            var primaryAlias = personAliasService.GetPrimaryAlias( person.Person.Id );
                                            if ( primaryAlias != null )
                                            {
                                                attendance = attendanceService.AddOrUpdate( primaryAlias.Id, startDateTime.Date, group.Group.Id,
                                                    location.Location.Id, schedule.Schedule.Id, location.CampusId,
                                                    checkInState.Kiosk.Device.Id, checkInState.CheckIn.SearchType.Id,
                                                    checkInState.CheckIn.SearchValue, family.Group.Id, attendanceCode.Id );

                                                attendance.PersonAlias = primaryAlias;
                                            }
                                        }

                                        attendance.DeviceId = checkInState.Kiosk.Device.Id;
                                        attendance.SearchTypeValueId = checkInState.CheckIn.SearchType.Id;
                                        attendance.SearchValue = checkInState.CheckIn.SearchValue;
                                        attendance.CheckedInByPersonAliasId = checkInState.CheckIn.CheckedInByPersonAliasId;
                                        attendance.SearchResultGroupId = family.Group.Id;
                                        attendance.AttendanceCodeId = attendanceCode.Id;
                                        attendance.StartDateTime = startDateTime;
                                        attendance.EndDateTime = null;
                                        attendance.DidAttend = true;
                                        attendance.Note = group.Notes;

                                        KioskLocationAttendance.AddAttendance( attendance );
                                        isCheckedIntoLocation = true;

                                        // Keep track of attendance (Ids) for use by other actions later in the workflow pipeline
                                        attendanceRecords.Add( attendance );
                                    }

                                    // If the person was NOT checked into the location for any schedule then remove the location
                                    if ( ! isCheckedIntoLocation )
                                    {
                                        group.Locations.Remove( location );
                                    }
                                }
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                // Now that the records are persisted, take the Ids and save them to the temp CheckInFamliy object
                family.AttendanceIds = attendanceRecords.Select( a => a.Id ).ToList();
                attendanceRecords = null;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current occurrence from the given list for the matching location, schedule and startDateTime.
        /// </summary>
        /// <param name="currentOccurences">The current occurrences.</param>
        /// <param name="location">The location.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns></returns>
        private OccurenceRecord GetCurrentOccurence( List<OccurenceRecord> currentOccurences, CheckInLocation location, CheckInSchedule schedule, DateTime startDateTime )
        {
            return currentOccurences
                    .Where( a =>
                        a.Date == startDateTime.Date
                        && a.LocationId == location.Location.Id
                        && a.ScheduleId == schedule.Schedule.Id )
                    .FirstOrDefault();
        }
    }

    class OccurenceRecord
    {
        public int ScheduleId { get; set; }
        public int LocationId { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }
}