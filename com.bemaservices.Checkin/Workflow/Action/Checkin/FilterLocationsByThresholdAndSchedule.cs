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
/// Author: Bob Rufenacht
/// Description:
///     Use this checkin workflow action directly after the Set Available Schedules action in the Schedule Select activity.  The Rock defualt
///     Filter Locations By Threshold should be disabled when using this action.
///     This workflow action allows location thresholds to be enforced separately for each schedule in a multi-service check-in environment.
///     It asseses the number of people at each service and removes the schedule option for groups that are full at a particular schedule.
///

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using com_bemaservices.CheckIn.ExtensionMethods;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;
using Rock;

namespace com_bemaservices.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) any locations that are not active
    /// </summary>
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "Removes (or excludes) any scheduled from the locations that are above the soft threshold" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Locations by Threshold And Schedule" )]

    [BooleanField( "Remove", "Select 'Yes' if schedules should be be removed from the location.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterLocationsByThresholdAndSchedule : CheckInActionComponent
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
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                    foreach ( var person in family.People )
                    {
                        foreach ( var groupType in person.GroupTypes )
                        {
                            foreach ( var group in groupType.Groups )
                            {
                                foreach ( var location in group.Locations.ToList() )
                                {
                                    if ( location.Location.SoftRoomThreshold.HasValue )
                                    {
                                        foreach ( var schedule in location.Schedules.ToList() )
                                        {
                                            // If location and schedule is above the threshold, exclude or remove the schedule.
                                            var locAttendance = KioskLocationAttendance.Get( location.Location.Id );
                                            if ( locAttendance != null &&
                                                !locAttendance.DistinctPersonIds ( schedule.Schedule.Id ).Contains( person.Person.Id ) &&
                                                location.Location.SoftRoomThreshold.Value <= locAttendance.CurrentCount ( schedule.Schedule.Id) )
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
                }

                return true;
            }

            return false;
        }
    }
}