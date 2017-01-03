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
    /// Removes (or excludes) any group types, groups, locations that are for a service time that person has already checked into
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes any group types, groups, locations that are for a service time that person has already checked into" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter by Previous Checkin" )]

    public class FilterByPreviousCheckin : CheckInActionComponent
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
            if ( checkInState != null && checkInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family )
            {
                bool configPrevents = checkInState.CheckInType.PreventDuplicateCheckin;

                var family = checkInState.CheckIn.CurrentFamily;

                if ( family != null )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                    var personIds = family.People.Select( p => p.Person.Id ).ToList();
                    var today = RockDateTime.Today;
                    var tomorrow = RockDateTime.Today.AddDays( 1 );

                    var existingAttendance = new AttendanceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.StartDateTime.CompareTo( today ) >= 0 &&
                            a.StartDateTime.CompareTo( tomorrow ) < 0 &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            a.PersonAlias != null &&
                            personIds.Contains( a.PersonAlias.PersonId ) &&
                            a.ScheduleId.HasValue )
                        .Select( a => new
                        {
                            PersonId = a.PersonAlias.PersonId,
                            ScheduleId = a.ScheduleId.Value
                        } )
                        .ToList();

                    if ( existingAttendance.Any() )
                    {
                        foreach ( var person in family.People.ToList() )
                        {
                            var attendedScheduleIds = existingAttendance.Where( a => a.PersonId == person.Person.Id ).Select( a => a.ScheduleId ).ToList();
                            if ( attendedScheduleIds.Any() )
                            {
                                foreach ( var groupType in person.GroupTypes.ToList() )
                                {
                                    if ( configPrevents || groupType.GroupType.GetAttributeValue( "PreventDuplicateCheckin" ).AsBoolean() )
                                    {
                                        attendedScheduleIds.ForEach( s => groupType.AvailableForSchedule.Remove( s ) );

                                        if ( !groupType.AvailableForSchedule.Any() )
                                        {
                                            person.GroupTypes.Remove( groupType );
                                        }
                                        else
                                        {
                                            foreach ( var group in groupType.Groups.ToList() )
                                            {
                                                attendedScheduleIds.ForEach( s => group.AvailableForSchedule.Remove( s ) );
                                                if ( !group.AvailableForSchedule.Any() )
                                                {
                                                    groupType.Groups.Remove( group );
                                                }
                                                else
                                                {
                                                    foreach ( var location in group.Locations.ToList() )
                                                    {
                                                        attendedScheduleIds.ForEach( s => location.AvailableForSchedule.Remove( s ) );
                                                        if ( !location.AvailableForSchedule.Any() )
                                                        {
                                                            group.Locations.Remove( location );
                                                        }
                                                    }
                                                    if ( group.Locations.Count == 0 )
                                                    {
                                                        groupType.Groups.Remove( group );
                                                    }
                                                }
                                            }
                                            if ( groupType.Groups.Count == 0 )
                                            {
                                                person.GroupTypes.Remove( groupType );
                                            }
                                        }
                                    }
                                }

                                if ( person.GroupTypes.Count == 0 )
                                {
                                    family.People.Remove( person );
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