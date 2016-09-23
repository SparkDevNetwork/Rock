﻿// <copyright>
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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) any locations that are not active
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Sets the avialable schedules for each grouptype, group, and location" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Available Schedules" )]

    public class SetAvailableSchedules : CheckInActionComponent
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
                                foreach ( var location in group.Locations )
                                {
                                    location.AvailableForSchedule =
                                        location.Schedules
                                            .Where( s => !s.ExcludedByFilter )
                                            .Select( s => s.Schedule.Id )
                                            .ToList();
                                }

                                group.AvailableForSchedule =
                                    group.Locations
                                        .Where( l => !l.ExcludedByFilter )
                                        .SelectMany( l => l.AvailableForSchedule )
                                        .Distinct()
                                        .ToList();
                            }

                            groupType.AvailableForSchedule =
                                groupType.Groups
                                    .Where( l => !l.ExcludedByFilter )
                                    .SelectMany( l => l.AvailableForSchedule )
                                    .Distinct()
                                    .ToList();
                        }
                    }
                }

                return true;
            }

            return false;
        }

    }
}