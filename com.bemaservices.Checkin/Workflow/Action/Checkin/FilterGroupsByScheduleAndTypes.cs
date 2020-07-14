// <copyright>
// Copyright by BEMA Information Technologies
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
//
// </copyright>

///
/// Created By: BEMA Services, Inc.
/// Author: Sam Crisp
/// Description:
///     Use this checkin workflow action directly after the Filter Locations By Schedule action in the Schedule Select activity.
///     This filter will remove any criteria-based groups in a selected schedule if there already exists an already-belongs groups for that schedule.
///     
///     For example, a child who is group member in a 2nd grade check-in group (already belongs) and qualifies for a 1st-4th grade kids worship (checkin by grade) at the same hour...
///     This block will remove the kids worship group/location/schedule for that schedule only.
///     If a second hour attendance was also selected before this screen, the kids worship hour may be available to check in during that schedule,
///     since no already-belongs group is available for this child at the second hour.
///     Warning: to filter out critera-based groups, they must be connected to the same schedule as their already-belongs group
///
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;


namespace com_bemaservices.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes unselected schedules.
    /// </summary>
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "Removes criteria based groups based on available already-belongs group (in family mode)" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Filter Groups By Schedule and Types" )]
    [BooleanField( "Remove", "Select 'Yes' if schedules should be be removed.  Select 'No' if they should just be marked as excluded.", false )]
    public class FilterGroupsByScheduleAndTypes : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                foreach ( var family in checkInState.CheckIn.Families.ToList() )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                    foreach ( var person in family.People.ToList() )
                    {
                        //list of schedule Ids where already belongs groups are available to check-in
                        var scheduleIdsWithAlreadyBelongGroups = person.GroupTypes
                            .Where( gt => !gt.ExcludedByFilter && gt.GroupType.AttendanceRule == Rock.Model.AttendanceRule.AlreadyBelongs )
                            .SelectMany( gt => gt.Groups ).Where( g => !g.ExcludedByFilter )
                            .SelectMany( g => g.Locations ).Where( l => !l.ExcludedByFilter )
                            .SelectMany( l => l.Schedules ).Where( s => !s.ExcludedByFilter )
                            .Select( s => s.Schedule.Id ).ToList();

                        foreach ( var groupType in person.GroupTypes.Where( gt => gt.GroupType.AttendanceRule != Rock.Model.AttendanceRule.AlreadyBelongs ).ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                foreach ( var location in group.Locations.ToList() )
                                {
                                    foreach ( var schedule in location.Schedules.ToList() )
                                    {
                                        //If Criteria-based group's schedule exists in same schedule with an already-belongs group, exclude or remove it.
                                        if ( scheduleIdsWithAlreadyBelongGroups.Any( sId => sId == schedule.Schedule.Id ) )
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