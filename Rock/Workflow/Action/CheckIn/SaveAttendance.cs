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

using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Saves the selected check-in data as attendance
    /// </summary>
    [Description("Saves the selected check-in data as attendance")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Save Attendance" )]
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
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if ( checkInState != null )
            {
                DateTime startDateTime = DateTime.Now;

                var attendanceService = new AttendanceService();

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        // TODO: Add code to generate security code
                        string securityCode = "xxx";

                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            foreach ( var location in groupType.Locations.Where( l => l.Selected ) )
                            {
                                foreach ( var group in location.Groups.Where( g => g.Selected ) )
                                {
                                    foreach ( var schedule in group.Schedules.Where( s => s.Selected ) )
                                    {
                                        var attendance = new Attendance();
                                        attendance.ScheduleId = schedule.Schedule.Id;
                                        attendance.GroupId = group.Group.Id;
                                        attendance.LocationId = location.Location.Id;
                                        attendance.PersonId = person.Person.Id;
                                        attendance.SecurityCode = securityCode;
                                        attendance.StartDateTime = startDateTime;
                                        attendanceService.Add( attendance, null );
                                        attendanceService.Save( attendance, null );
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