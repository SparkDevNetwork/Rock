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
    /// Calculates and updates the LastCheckIn property on check-in objects
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Preselects options if using Family check-in type based on the 'days back' value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Preselect Recent Attendance" )]
    public class PreSelectRecentAttendance : CheckInActionComponent
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
            if ( checkInState != null && checkInState.CheckInType.TypeOfCheckin == Rock.CheckIn.TypeOfCheckin.Family )
            {
                DateTime preSelectCutoff = RockDateTime.Today.AddDays( 0 - checkInState.CheckInType.AutoSelectDaysBack );

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People )
                    {
                        foreach ( var groupType in person.GroupTypes )
                        {
                            groupType.PreSelected = groupType.LastCheckIn.HasValue && groupType.LastCheckIn.Value.CompareTo( preSelectCutoff ) >= 0;

                            foreach ( var group in groupType.Groups )
                            {
                                group.PreSelected = group.LastCheckIn.HasValue && group.LastCheckIn.Value.CompareTo( preSelectCutoff ) >= 0;

                                foreach ( var location in group.Locations )
                                {
                                    location.PreSelected = location.LastCheckIn.HasValue && location.LastCheckIn.Value.CompareTo( preSelectCutoff ) >= 0;

                                    foreach ( var schedule in location.Schedules )
                                    {
                                        schedule.PreSelected = schedule.LastCheckIn.HasValue && schedule.LastCheckIn.Value.CompareTo( preSelectCutoff ) >= 0;
                                    }
                                }
                            }
                        }

                        person.PreSelected = person.GroupTypes.Any( t => t.PreSelected );
                    }
                }
            }

            return true;
        }
    }
}