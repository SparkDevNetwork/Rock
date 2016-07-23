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
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Adds the locations for each members group types
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Adds the locations for each members group types" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Load Locations" )]
    [BooleanField( "Load All", "By default locations are only loaded for the selected person and group type.  Select this option to load locations for all the loaded people and group types." )]
    public class LoadLocations : CheckInActionComponent
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

                foreach ( var family in checkInState.CheckIn.GetFamilies( true ) )
                {
                    foreach ( var person in family.GetPeople( !loadAll ) )
                    {
                        foreach ( var groupType in person.GetGroupTypes( !loadAll ).ToList() )
                        {
                            var kioskGroupType = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes )
                                .Where( g => g.GroupType.Id == groupType.GroupType.Id )
                                .FirstOrDefault();

                            if ( kioskGroupType != null )
                            {
                                foreach ( var group in groupType.GetGroups( !loadAll ) )
                                {
                                    foreach ( var kioskGroup in kioskGroupType.KioskGroups
                                        .Where( g => g.Group.Id == group.Group.Id && g.IsCheckInActive )
                                        .ToList() )
                                    {
                                        foreach ( var kioskLocation in kioskGroup.KioskLocations.Where( l => l.IsCheckInActive && l.IsActiveAndNotFull ) )
                                        {
                                            if ( !group.Locations.Any( l => l.Location.Id == kioskLocation.Location.Id ) )
                                            {
                                                var checkInLocation = new CheckInLocation();
                                                checkInLocation.Location = kioskLocation.Location.Clone( false );
                                                checkInLocation.Location.CopyAttributesFrom( kioskLocation.Location );
                                                checkInLocation.CampusId = kioskLocation.CampusId;
                                                group.Locations.Add( checkInLocation );
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