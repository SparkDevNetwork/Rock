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
    /// Calculates and updates the LastCheckIn property on checkin objects
    /// </summary>
    [Description("Calculates and updates the LastCheckIn property on checkin objects")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Calculate Last Attended" )]
    public class CalculateLastAttended : CheckInActionComponent
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
                DateTime sixMonthsAgo = DateTime.Today.AddMonths( -6 );
                var attendanceService = new AttendanceService();

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People )
                    {
                        foreach ( var groupType in person.GroupTypes )
                        {
                            var groupTypeCheckIns = attendanceService.Queryable()
                                .Where( a => 
                                    a.PersonId == person.Person.Id &&
                                    a.Group.GroupTypeId == groupType.GroupType.Id && 
                                    a.StartDateTime >= sixMonthsAgo )
                                .ToList();

                            if ( groupTypeCheckIns.Any() )
                            {
                                groupType.LastCheckIn = groupTypeCheckIns.Select( a => a.StartDateTime ).Max();

                                foreach ( var group in groupType.Groups )
                                {
                                    var groupCheckIns = groupTypeCheckIns.Where( a => a.GroupId == group.Group.Id ).ToList();
                                    if ( groupCheckIns.Any() )
                                    {
                                        group.LastCheckIn = groupCheckIns.Select( a => a.StartDateTime ).Max();
                                    }

                                    foreach ( var location in group.Locations )
                                    {
                                        var locationCheckIns = groupCheckIns.Where( a => a.LocationId == location.Location.Id ).ToList();
                                        if ( locationCheckIns.Any() )
                                        {
                                            group.LastCheckIn = locationCheckIns.Select( a => a.StartDateTime ).Max();
                                        }

                                        foreach ( var schedule in location.Schedules )
                                        {
                                            var scheduleCheckIns = locationCheckIns.Where( a => a.ScheduleId == schedule.Schedule.Id ).ToList();
                                            if ( scheduleCheckIns.Any() )
                                            {
                                                schedule.LastCheckIn = scheduleCheckIns.Select( a => a.StartDateTime ).Max();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ( person.GroupTypes.Any() )
                        {
                            person.LastCheckIn = person.GroupTypes.Select( g => g.LastCheckIn ).Max();
                        }
                    }
                }

                SetCheckInState( action, checkInState );
                return true;

            }

            return false;
        }
    }
}