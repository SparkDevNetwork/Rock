// <copyright>
// Copyright by the Spark Development Network
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
using System.Linq;

using Rock.Attribute;
using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes unselected schedules.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes schedules from all locations if that shedule was not selected (in family mode)" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Filter Locations By Schedule" )]
    [BooleanField( "Remove", "Select 'Yes' if schedules should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterLocationsBySchedule : CheckInActionComponent
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
                foreach ( var family in checkInState.CheckIn.Families.ToList() )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                    foreach ( var person in family.People.ToList() )
                    {
                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                foreach ( var location in group.Locations.ToList() )
                                {
                                    foreach( var schedule in location.Schedules.ToList() )
                                    {
                                        if ( !person.PossibleSchedules.Any( s => s.Schedule.Id == schedule.Schedule.Id && s.Selected ) )
                                        {
                                            if ( remove )
                                            {
                                                location.Schedules.Remove( schedule );
                                            }
                                            else
                                            {
                                                schedule.ExcludedByFilter = true;
                                            }
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