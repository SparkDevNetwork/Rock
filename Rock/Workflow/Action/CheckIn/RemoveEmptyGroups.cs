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

using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes any group that does not have any locations. If group contains locations, but they are all excluded by filter, will also mark the group as excluded by filter.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes any group that does not have any locations. If group contains locations, but they are all excluded by filter, will also mark the group as excluded by filter." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Remove Empty Groups" )]
    [Rock.SystemGuid.EntityTypeGuid( "698115D4-7B5E-48F3-BBB0-C53A20193169")]
    public class RemoveEmptyGroups : CheckInActionComponent
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
                        var anyGroupsExcluded = false;

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                if ( group.Locations.Count == 0 )
                                {
                                    groupType.Groups.Remove( group );
                                    anyGroupsExcluded = true;
                                }
                                else if ( !group.Locations.Any( l => !l.ExcludedByFilter ) )
                                {
                                    group.ExcludedByFilter = true;
                                    anyGroupsExcluded = true;
                                }
                            }
                        }

                        // If this person has no groups available as a result of this action, set their "No Option Reason".
                        if ( anyGroupsExcluded && !person.GroupTypes.Any( gt => gt.Groups.Any( g => !g.ExcludedByFilter ) ) )
                        {
                            // Using "Locations" rather than "Groups" within this message makes more sense
                            // here, since the groups were removed due to having no locations.
                            person.NoOptionReason = "No Locations Available";
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}