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
    /// Filters the available groups if one is from the previous attendance
    /// </summary>
    [Description( "Selects the previously attended group/location/schedule for each person without requiring all groups/locations/schedules to be loaded." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Select By Last Attended" )]
    public class SelectByLastAttended : CheckInActionComponent
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
                                //var lastGroupType = person.GroupTypes.Where( gt => gt.GroupType.Id == groupTypeCheckIns.Select( a => a
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