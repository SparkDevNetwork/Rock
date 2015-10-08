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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Loads the schedules available for each group
    /// </summary>
    [Description( "Loads the schedules available for each group" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Load Schedules" )]
    [BooleanField( "Load All", "By default schedules are only loaded for the selected person, group type, location, and group.  Select this option to load schedules for all the loaded people, group types, locations, and groups." )]
    public class LoadSchedules : CheckInActionComponent
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
                bool loadAll = GetAttributeValue( action, "LoadAll" ).AsBoolean();

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ).ToList() )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ).ToList() )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected || loadAll ).ToList() )
                        {
                            var kioskGroupType = checkInState.Kiosk.FilteredGroupTypes( checkInState.ConfiguredGroupTypes ).Where( g => g.GroupType.Id == groupType.GroupType.Id ).FirstOrDefault();
                            if ( kioskGroupType != null )
                            {
                                foreach ( var group in groupType.Groups.Where( g => g.Selected || loadAll ).ToList() )
                                {
                                    foreach ( var location in group.Locations.Where( l => l.Selected || loadAll ).ToList() )
                                    {
                                        var kioskGroup = kioskGroupType.KioskGroups.Where( g => g.Group.Id == group.Group.Id ).FirstOrDefault();
                                        if ( kioskGroup != null )
                                        {
                                            var kioskLocation = kioskGroup.KioskLocations.Where( l => l.Location.Id == location.Location.Id ).FirstOrDefault();
                                            if ( kioskLocation != null )
                                            {
                                                foreach ( var kioskSchedule in kioskLocation.KioskSchedules )
                                                {
                                                    if ( !location.Schedules.Any( s => s.Schedule.Id == kioskSchedule.Schedule.Id ) )
                                                    {
                                                        var checkInSchedule = new CheckInSchedule();
                                                        checkInSchedule.Schedule = kioskSchedule.Schedule.Clone( false );
                                                        checkInSchedule.StartTime = kioskSchedule.StartTime;
                                                        location.Schedules.Add( checkInSchedule );
                                                    }
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