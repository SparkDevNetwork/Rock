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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Selects a location based on the Location Selection Strategy checkin type attribute. Only does work if the TypeOfCheckin is Family since the time is known when choosing a location.
    /// </summary>
    /// <seealso cref="Rock.Workflow.Action.CheckIn.CheckInActionComponent" />
    [ActionCategory( "Check-In" )]
    [Description( "For Family Checkin this will choose a location based on the selected Location Selection Strategy. Note, this workflow action assumes the locations have already been filtered by schedule." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Locations By Location Selection Strategy" )]

    [BooleanField( "Remove",
        Key = AttributeKey.Remove,
        Description = "Select 'Yes' if locations should be be removed. Select 'No' if they should just be marked as excluded.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 0 )]

    public class FilterLocationsByLocationSelectionStrategy : CheckInActionComponent
    {
        private class AttributeKey
        {
            public const string Remove = "Remove";
        }

        /// <inheritdoc /> 
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family == null )
            {
                return false;
            }

            if ( checkInState.CheckInType.TypeOfCheckin != TypeOfCheckin.Family )
            {
                // This only works for family checkin so don't filter any locations but don't indicate an error either.
                return true;
            }

            var remove = GetAttributeValue( action, AttributeKey.Remove ).AsBoolean();

            foreach ( var person in family.People )
            {
                foreach ( var checkinGroupType in person.GroupTypes )
                {
                    var attributeLocationSelectionStrategy = ( CheckinConfigurationHelper.LocationSelectionStrategy? ) checkinGroupType.GroupType.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY ).AsIntegerOrNull() ?? null;
                    
                    if ( attributeLocationSelectionStrategy == null ||  attributeLocationSelectionStrategy == CheckinConfigurationHelper.LocationSelectionStrategy.Ask )
                    {
                        // Either this is not set for some reason or the location should not be automatically selected, so don't filter the locations.
                        continue;
                    }

                    var selectedCheckinSchedules = person.PossibleSchedules.Where( s => s.Selected == true ).ToList();

                    FilterLocationList( checkinGroupType, remove, attributeLocationSelectionStrategy.Value, selectedCheckinSchedules );
                }
            }

            return true;
        }

        private void FilterLocationList( CheckInGroupType checkInGroupType, bool remove, CheckinConfigurationHelper.LocationSelectionStrategy attributeLocationSelectionStrategy, List<CheckInSchedule> selectedSchedules )
        {
            // Order the list
            var checkinGroups = checkInGroupType.Groups.OrderBy( g => g.Group.Order ).ToList();
            
            foreach ( var checkinGroup in checkinGroups )
            {
                // Get a list of locations that have not reached their threshold.
                var locationListQuery = checkinGroup.Locations
                    .Where( l => l.Location.SoftRoomThreshold == null || KioskLocationAttendance.Get( l.Location.Id ).CurrentCount <= l.Location.SoftRoomThreshold.Value );

                List<CheckInLocation> locationList = new List<CheckInLocation>();

                if ( attributeLocationSelectionStrategy == CheckinConfigurationHelper.LocationSelectionStrategy.Balance )
                {
                    locationList = locationListQuery.OrderBy( l => KioskLocationAttendance.Get( l.Location.Id ).CurrentCount ).ToList();
                }
                else if ( attributeLocationSelectionStrategy == CheckinConfigurationHelper.LocationSelectionStrategy.FillInOrder )
                {
                    locationList = locationListQuery.OrderBy( l => l.Order ).ToList();
                }

                if ( selectedSchedules.Count() == 1 )
                {
                    // If we only have one schedule then we can just remove the other locations and not care about the schedules.
                    FilterLocations( checkinGroup, locationList, remove );
                }
                else
                {
                    // Now we have to care about schedules.
                    FilterLocationSchedules( checkinGroup, locationList, selectedSchedules, remove );
                }
            }
        }

        private void FilterLocationSchedules( CheckInGroup checkinGroup, List<CheckInLocation> locationList, List<CheckInSchedule> selectedSchedules, bool remove )
        {
            // Check the locations in the sorted or for the first one that has all the schedules available and use it if it exists.
            var locationForSchedules = locationList.Where( l => l.Schedules.Select( s => s.Schedule.Id ).Intersect( selectedSchedules.Select( ss => ss.Schedule.Id ) ).Count() == selectedSchedules.Count ).FirstOrDefault();
            if ( locationForSchedules != null )
            {
                locationList.Remove( locationForSchedules );
                foreach ( var location in locationList )
                {
                    if ( remove )
                    {
                        checkinGroup.Locations.Remove( location );
                    }
                    else
                    {
                        location.ExcludedByFilter = true;
                    }
                }

                return;
            }

            // There is no location that has all of the selected schedules for this person so we need to choose each schedule location in the sorted list order.
            // As a practical matter this will probably not be more than two. The choosing will be done by removing the schedules from the locations that are not needed.
            foreach ( var selectedSchedule in selectedSchedules )
            {
                var foundFirstMatch = false;
                foreach ( var location in locationList )
                {
                    if ( location.Schedules.Contains( selectedSchedule ) )
                    {
                        if ( foundFirstMatch )
                        {
                            foreach ( var checkinLocation in checkinGroup.Locations )
                            {
                                if ( remove )
                                {
                                    checkinLocation.Schedules.Remove( selectedSchedule );
                                }
                                else
                                {
                                    selectedSchedule.ExcludedByFilter = true;
                                }
                            }
                        }
                        else
                        {
                            foundFirstMatch = true;
                        }
                    }
                }
            }
        }

        private void FilterLocations( CheckInGroup checkinGroup, List<CheckInLocation> locationList, bool remove )
        {
            var foundFirstMatch = false;

            foreach ( var location in locationList )
            {
                if ( foundFirstMatch )
                {
                    if ( remove )
                    {
                        checkinGroup.Locations.Remove( location );
                    }
                    else
                    {
                        location.ExcludedByFilter = true;
                    }
                }
                else
                {
                    foundFirstMatch = true;
                }
            }
        }
    }
}
