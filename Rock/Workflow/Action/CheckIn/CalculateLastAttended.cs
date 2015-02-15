// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Calculates and updates the LastCheckIn property on check-in objects
    /// </summary>
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
                DateTime sixMonthsAgo = RockDateTime.Today.AddMonths( -6 );
                var attendanceService = new AttendanceService( rockContext );

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People )
                    {
                        person.FirstTime = !attendanceService
                            .Queryable().AsNoTracking()
                            .Where( a => a.PersonAlias.PersonId == person.Person.Id )
                            .Any();

                        foreach ( var groupType in person.GroupTypes )
                        {
                            var groupTypeCheckIns = attendanceService
                                .Queryable().AsNoTracking()
                                .Where( a =>
                                    a.PersonAlias.PersonId == person.Person.Id &&
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
                                            location.LastCheckIn = locationCheckIns.Select( a => a.StartDateTime ).Max();
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

                return true;

            }

            return false;
        }
    }
}