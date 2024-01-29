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
using System.Linq;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about an additional time sign-up occurrence for the group schedule toolbox block.
    /// </summary>
    public class SignUpOccurrenceBag
    {
        /// <summary>
        /// Gets or sets the occurrence schedule unique identifier.
        /// </summary>
        public Guid ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the occurrence schedule name.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the formatted occurrence schedule name.
        /// </summary>
        public string FormattedScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date time.
        /// </summary>
        public DateTimeOffset OccurrenceDateTime { get; set; }

        /// <summary>
        /// Gets or sets the count of people scheduled for this occurrence without a location specified.
        /// </summary>
        public int PeopleScheduledWithoutLocationCount { get; set; }

        /// <summary>
        /// Gets or sets whether this occurrence represents an immediate need.
        /// </summary>
        public bool IsImmediateNeed { get; set; }

        /// <summary>
        /// Gets or sets the locations that may be selected for this occurrence.
        /// </summary>
        public List<SignUpOccurrenceLocationBag> Locations { get; set; }

        /// <summary>
        /// Gets the count of people needed for this sign-up occurrence, across all locations.
        /// </summary>
        public int PeopleNeededCount
        {
            get
            {
                var locationPeopleNeededCount = this.Locations
                    ?.Sum( l => l.PeopleNeededCount )
                    ?? 0;

                var peopleNeededCount = locationPeopleNeededCount - this.PeopleScheduledWithoutLocationCount;
                if ( peopleNeededCount < 0 )
                {
                    return 0;
                }

                return peopleNeededCount;
            }
        }

        /// <summary>
        /// Gets whether all of this sign-up occurrence's locations are at maximum capacity.
        /// </summary>
        public bool AreAllLocationsAtMaximumCapacity
        {
            get
            {
                // If this schedule doesn't have any locations, we have nowhere to schedule people.
                if ( this.Locations?.Any() != true )
                {
                    return true;
                }

                // If any locations are not capped, we can always schedule more.
                if ( this.Locations.Any( l => l.MaximumCapacity == 0 ) )
                {
                    return false;
                }

                // Since all locations have a maximum capacity setting, check each location for available capacity
                // and subtract any scheduled group members who have not specified a location.
                int totalAvailableCapacity = 0;
                foreach ( var location in this.Locations )
                {
                    if ( location.IsAtMaximumCapacity )
                    {
                        // If this location is overbooked, we will ignore it. This could potentially result in
                        // over-booking this schedule, but only because this location is already overbooked and
                        // we can't assume that people can be rescheduled to another open location.

                        // Note that locations at max capacity should never be added to this collection, based
                        // on how we're aggregating data in the `GroupScheduleToolbox.GetSignUpOccurrences()`
                        // method, but we'll leave this check here as a failsafe.
                        continue;
                    }

                    // Add this location's available capacity to the total.
                    totalAvailableCapacity += location.MaximumCapacity - location.PeopleScheduledCount;
                }

                if ( this.PeopleScheduledWithoutLocationCount >= totalAvailableCapacity )
                {
                    // We have enough people; they just need to be assigned to a location.
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets this sign-up occurrence's location sort order.
        /// </summary>
        public int LocationSortOrder => this.Locations?.Min( l => l.LocationOrder ) ?? int.MaxValue;

        /// <summary>
        /// Gets this sign-up occurrence's location sort name.
        /// </summary>
        public string LocationSortName
        {
            get
            {
                var location = this.Locations
                    ?.OrderBy( l => l.LocationOrder )
                    ?.FirstOrDefault();

                return location?.LocationName ?? string.Empty;
            }
        }
    }
}
