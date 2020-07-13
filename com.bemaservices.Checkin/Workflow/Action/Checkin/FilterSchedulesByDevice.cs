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
///     Use this checkin workflow action at the end of the Family Select activity.
///     This action allows one kiosk device to only service the "current" service while other
///     devices use the multi-service settings.  As each single setvice goes inactive, the next
///     service will become active on the single service device.
///     You must add a check-in device boolean attribute with key "SingleServiceOnly" to set
///     which devices are single and which are multiple.  The attribute should have a defulat valule of 'No.'
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
    [Description( "Removes schedules if only a single schedule is allowed for the device." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Filter Schedules By Device" )]
    [BooleanField( "Remove", "Select 'Yes' if schedules should be be removed.  Select 'No' if they should just be marked as excluded.", true )]

    public class FilterSchedulesByDevice : CheckInActionComponent
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
                var singleService = checkInState.Kiosk.Device.GetAttributeValue ( "SingleServiceOnly" ).AsBoolean ();

                if ( singleService )
                {
                    // Find First service for device
                    var firstSchedule = checkInState.Kiosk.ActiveGroupTypes ( checkInState.ConfiguredGroupTypes )
                        .SelectMany( t => t.KioskGroups.Where ( g => g.IsCheckInActive == true) )
                        .SelectMany( g => g.KioskLocations.Where ( l => l.IsCheckInActive == true ) )
                        .SelectMany ( l => l.KioskSchedules.Where ( s => s.IsCheckInActive == true ) )      
                        .ToList ().OrderBy ( s => s.StartTime).FirstOrDefault ();

                    foreach ( var groupType in checkInState.Kiosk.ActiveGroupTypes ( checkInState.ConfiguredGroupTypes ).ToList() )
                    {
                        foreach ( var group in groupType.KioskGroups.ToList() )
                        {
                            foreach ( var location in group.KioskLocations.ToList() )
                            {
                                foreach( var schedule in location.KioskSchedules.ToList() )
                                {
                                    if ( schedule.Schedule.Id != firstSchedule.Schedule.Id )
                                    {
                                             location.KioskSchedules.Remove( schedule );
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