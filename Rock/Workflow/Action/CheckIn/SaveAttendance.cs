//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Saves the selected check-in data as attendance
    /// </summary>
    [Description("Saves the selected check-in data as attendance")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Save Attendance" )]
    [IntegerField( "Security Code Length", "The number of characters to use for the security code.", true, 3 )]
    public class SaveAttendance : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                DateTime startDateTime = DateTime.Now;

                int securityCodeLength = 3;
                if ( !int.TryParse( GetAttributeValue( action, "SecurityCodeLength" ), out securityCodeLength ) )
                {
                    securityCodeLength = 3;
                }

                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    var attendanceCodeService = new AttendanceCodeService();
                    var attendanceService = new AttendanceService();
                    var groupMemberService = new GroupMemberService();

                    foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                    {
                        foreach ( var person in family.People.Where( p => p.Selected ) )
                        {
                            var attendanceCode = attendanceCodeService.GetNew( securityCodeLength );
                            person.SecurityCode = attendanceCode.Code;

                            foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                            {
                                foreach ( var group in groupType.Groups.Where( g => g.Selected ) )
                                {
                                    foreach ( var location in group.Locations.Where( l => l.Selected ) )
                                    {
                                        if ( groupType.GroupType.AttendanceRule == AttendanceRule.AddOnCheckIn &&
                                            groupType.GroupType.DefaultGroupRoleId.HasValue &&
                                            !groupMemberService.GetByGroupIdAndPersonId( group.Group.Id, person.Person.Id, true ).Any() )
                                        {
                                            var groupMember = new GroupMember();
                                            groupMember.GroupId = group.Group.Id;
                                            groupMember.PersonId = person.Person.Id;
                                            groupMember.GroupRoleId = groupType.GroupType.DefaultGroupRoleId.Value;
                                            groupMemberService.Add( groupMember, null );
                                            groupMemberService.Save( groupMember, null );
                                        }

                                        foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                                        {
                                            // Only create one attendance record per day for each person/schedule/group/location
                                            var attendance = attendanceService.Get( startDateTime, location.Location.Id, schedule.Schedule.Id, group.Group.Id, person.Person.Id );
                                            if ( attendance == null )
                                            {
                                                attendance = uow.DbContext.Attendances.Create();
                                                attendance.LocationId = location.Location.Id;
                                                attendance.ScheduleId = schedule.Schedule.Id;
                                                attendance.GroupId = group.Group.Id;
                                                attendance.PersonId = person.Person.Id;
                                                attendance.DeviceId = checkInState.Kiosk.Device.Id;
                                                attendance.SearchTypeValueId = checkInState.CheckIn.SearchType.Id;
                                                attendanceService.Add( attendance, null );
                                            }

                                            attendance.AttendanceCodeId = attendanceCode.Id;
                                            attendance.StartDateTime = startDateTime;
                                            attendance.EndDateTime = null;
                                            attendance.DidAttend = true;
                                            attendanceService.Save( attendance, null );

                                            KioskLocationAttendance.AddAttendance( attendance );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return true;

            }

            return false;
        }
    }
}