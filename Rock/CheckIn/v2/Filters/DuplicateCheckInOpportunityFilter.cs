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

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on if the person has
    /// already checked into the schedule.
    /// </summary>
    internal class DuplicateCheckInOpportunityFilter : OpportunityFilter
    {
        #region Properties

        /// <summary>
        /// Gets the schedule identifiers this person is currently
        /// checked into today.
        /// </summary>
        /// <value>The checked in schedule identifiers.</value>
        private Lazy<HashSet<string>> CheckedInScheduleIds { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateCheckInOpportunityFilter"/> class.
        /// </summary>
        public DuplicateCheckInOpportunityFilter()
        {
            CheckedInScheduleIds = new Lazy<HashSet<string>>( () =>
            {
                var today = RockDateTime.Today;
                var attendances = Person.RecentAttendances
                    .Where( a => a.StartDateTime.Date == today
                        && !a.EndDateTime.HasValue )
                    .Select( a => a.ScheduleId );

                return new HashSet<string>( attendances );
            }, true );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void FilterSchedules( OpportunityCollection opportunities )
        {
            var startCount = opportunities.Schedules.Count;

            if ( startCount == 0 )
            {
                return;
            }

            // Remove any schedules the attendee has already checked in for.
            opportunities.Schedules.RemoveAll( s => !IsScheduleValid( s ) );

            // If we removed the last schedule then mark them as unavailable
            // and set a helpful message to display in the UI.
            if ( opportunities.Schedules.Count == 0 && !Person.IsUnavailable )
            {
                Person.IsUnavailable = true;
                Person.UnavailableMessage = "Already Checked In.";
            }
        }

        /// <inheritdoc/>
        public override bool IsScheduleValid( ScheduleOpportunity schedule )
        {
            if ( !TemplateConfiguration.IsDuplicateCheckInPrevented )
            {
                return true;
            }

            // Remove any schedules the attendee has already checked in for.
            return !CheckedInScheduleIds.Value.Contains( schedule.Id );
        }

        /// <inheritdoc/>
        public override void FilterGroups( OpportunityCollection opportunities )
        {
            var startCount = opportunities.Groups.Count;

            if ( startCount == 0 )
            {
                return;
            }

            // Remove any groups that are marked as not available for
            // concurrent check-in.
            opportunities.Groups.RemoveAll( g => !IsGroupValid( g ) );

            // If we removed the last schedule then mark them as unavailable
            // and set a helpful message to display in the UI.
            if ( opportunities.Groups.Count == 0 && !Person.IsUnavailable )
            {
                Person.IsUnavailable = true;
                Person.UnavailableMessage = "Already Checked In.";
            }
        }

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            if ( !group.CheckInAreaData.IsConcurrentCheckInPrevented )
            {
                return true;
            }

            // Remove any location schedules the attendee has already checked in for.
            group.Locations.RemoveAll( l => CheckedInScheduleIds.Value.Contains( l.ScheduleId ) );
            group.OverflowLocations.RemoveAll( l => CheckedInScheduleIds.Value.Contains( l.ScheduleId ) );

            return group.Locations.Count > 0 || group.OverflowLocations.Count > 0;
        }

        #endregion
    }
}
