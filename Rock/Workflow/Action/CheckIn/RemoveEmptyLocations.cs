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
using System.Linq;

using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes locations (from groups) if the location has no schedule.  If location contains schedules, but they are all excluded by filter, will also mark the location as excluded by filter.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes locations (from groups) if the location has no schedule.  If location contains schedules, but they are all excluded by filter, will also mark the location as excluded by filter." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Remove Empty Locations" )]
    public class RemoveEmptyLocations : CheckInActionComponent
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
                    foreach ( var person in family.People.ToList() )
                    {
                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                foreach ( var location in group.Locations.ToList() )
                                {
                                    if ( location.Schedules.Count == 0 )
                                    {
                                        group.Locations.Remove( location );
                                    }
                                    else if ( !location.Schedules.Any( s => !s.ExcludedByFilter ) )
                                    {
                                        location.ExcludedByFilter = true;
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