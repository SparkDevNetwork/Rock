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

using Rock.Model;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Provides functionality for making default selections for a person. This
    /// is used when the AutoSelect feature is enabled and also configured to
    /// select the group/location/schedule.
    /// </summary>
    internal class DefaultSelectionProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the check-in template configuration in effect during filtering.
        /// </summary>
        /// <value>The check-in template configuration.</value>
        protected TemplateConfigurationData TemplateConfiguration => Session.TemplateConfiguration;

        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        protected CheckInSession Session { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSelectionProvider"/> class.
        /// </summary>
        /// <param name="session">The check-in session.</param>
        public DefaultSelectionProvider( CheckInSession session )
        {
            Session = session ?? throw new ArgumentNullException( nameof( session ) );
        }

        #endregion

        /// <summary>
        /// Gets the default selections for the person. This uses recent
        /// attendance to try and put them in the same location they were in
        /// last time but will fall back to other methods if that is not
        /// available.
        /// </summary>
        /// <param name="person">The person to get the default selection for.</param>
        /// <returns>A collection of <see cref="OpportunitySelectionBag"/> objects.</returns>
        public virtual List<OpportunitySelectionBag> GetDefaultSelectionsForPerson( Attendee person )
        {
            person.LastCheckIn = person.RecentAttendances.Max( a => ( DateTime? ) a.StartDateTime );

            var orderedRecentAttendance = person.RecentAttendances
                .Where( a => a.StartDateTime.Date == person.LastCheckIn.Value.Date )
                .OrderBy( a => NamedScheduleCache.GetByIdKey( a.ScheduleId, Session.RockContext )?.StartTimeOfDay )
                .ThenByDescending( a => a.StartDateTime );

            var previousCheckIns = new List<RecentAttendance>();

            // Sum down the previous check-ins so that we only have one per schedule.
            // This is ordered in such a way that the most recent attendance will
            // take precedence over older attendances.
            foreach ( var attendance in orderedRecentAttendance )
            {
                if ( !previousCheckIns.Any( i => i.ScheduleId == attendance.ScheduleId ) )
                {
                    previousCheckIns.Add( attendance );
                }
            }

            var selectedOpportunities = new List<OpportunitySelectionBag>();

            if ( !previousCheckIns.Any() )
            {
                // Just try to pick anything valid.
                if ( TryGetAnyValidSelection( person, out var opportunities ) )
                {
                    selectedOpportunities.Add( opportunities );
                }

                return selectedOpportunities;
            }

            foreach ( var previousCheckIn in previousCheckIns )
            {
                // First try to find a valid exact match against a previous check-in.
                if ( TryGetExactMatch( person, previousCheckIn, out var opportunities ) )
                {
                    selectedOpportunities.Add( opportunities );

                    continue;
                }

                // Next, try to find a matching group and then just take the first
                // available location and schedule.
                if ( TryGetBestMatchingGroup( person, previousCheckIn, out opportunities ) )
                {
                    selectedOpportunities.Add( opportunities );

                    continue;
                }
            }

            return selectedOpportunities;
        }

        /// <summary>
        /// Attempts to get an exact match from a previous check-in. This checks
        /// for exact matches to group, location and schedule.
        /// </summary>
        /// <param name="person">The person to be checked in.</param>
        /// <param name="previousCheckIn">The previous check-in record.</param>
        /// <param name="selectedOpportunities">On return contains an instance of <see cref="OpportunitySelectionBag"/> or <c>null</c>.</param>
        /// <returns><c>true</c> if a match was found and <paramref name="selectedOpportunities"/> is valid, <c>false</c> otherwise.</returns>
        protected virtual bool TryGetExactMatch( Attendee person, RecentAttendance previousCheckIn, out OpportunitySelectionBag selectedOpportunities )
        {
            selectedOpportunities = null;

            var group = person.Opportunities.Groups
                .FirstOrDefault( g => g.Id == previousCheckIn.GroupId );

            if ( group == null || !group.LocationIds.Contains( previousCheckIn.LocationId ) )
            {
                return false;
            }

            var area = person.Opportunities.Areas
                .FirstOrDefault( a => a.Id == group.AreaId );

            if ( area == null )
            {
                return false;
            }

            var location = person.Opportunities.Locations
                .FirstOrDefault( l => l.Id == previousCheckIn.LocationId );

            if ( location == null || !location.ScheduleIds.Contains( previousCheckIn.ScheduleId ) )
            {
                return false;
            }

            var schedule = person.Opportunities.Schedules
                .FirstOrDefault( s => s.Id == previousCheckIn.ScheduleId );

            if ( schedule == null )
            {
                return false;
            }

            selectedOpportunities = GetSelectedOpportunities( area, group, location, schedule );

            return true;
        }

        /// <summary>
        /// Attempts to get a loose match from a previous check-in. This checks
        /// for exact matches to group and will use any valid location and
        /// schedule currently supported for that group.
        /// </summary>
        /// <param name="person">The person to be checked in.</param>
        /// <param name="previousCheckIn">The previous check-in record.</param>
        /// <param name="selectedOpportunities">On return contains an instance of <see cref="OpportunitySelectionBag"/> or <c>null</c>.</param>
        /// <returns><c>true</c> if a match was found and <paramref name="selectedOpportunities"/> is valid, <c>false</c> otherwise.</returns>
        protected virtual bool TryGetBestMatchingGroup( Attendee person, RecentAttendance previousCheckIn, out OpportunitySelectionBag selectedOpportunities )
        {
            selectedOpportunities = null;

            var group = person.Opportunities.Groups
                .FirstOrDefault( g => g.Id == previousCheckIn.GroupId );

            if ( group == null )
            {
                return false;
            }

            var area = person.Opportunities.Areas
                .FirstOrDefault( a => a.Id == group.AreaId );

            if ( area == null )
            {
                return false;
            }

            if ( TryGetFirstValidSelectionForGroup( area, group, person, out selectedOpportunities ) )
            {
                return true;
            }


            return false;
        }

        /// <summary>
        /// Attempts to get any valid selection for the person. This is called
        /// as a last resort.
        /// </summary>
        /// <param name="person">The person to be checked in.</param>
        /// <param name="selectedOpportunities">On return contains an instance of <see cref="OpportunitySelectionBag"/> or <c>null</c>.</param>
        /// <returns><c>true</c> if a match was found and <paramref name="selectedOpportunities"/> is valid, <c>false</c> otherwise.</returns>
        protected virtual bool TryGetAnyValidSelection( Attendee person, out OpportunitySelectionBag selectedOpportunities )
        {
            foreach ( var group in person.Opportunities.Groups )
            {
                var area = person.Opportunities.Areas
                    .FirstOrDefault( a => a.Id == group.AreaId );

                if ( area == null )
                {
                    continue;
                }

                if ( TryGetFirstValidSelectionForGroup( area, group, person, out selectedOpportunities ) )
                {
                    return true;
                }
            }

            selectedOpportunities = null;

            return false;
        }

        /// <summary>
        /// Attempts to get the first valid location and schedule for the
        /// indicated area and group.
        /// </summary>
        /// <param name="area">The potential check-in area to be selected.</param>
        /// <param name="group">The potential check-in group to be selected.</param>
        /// <param name="person">The person to be checked in.</param>
        /// <param name="selectedOpportunities">On return contains an instance of <see cref="OpportunitySelectionBag"/> or <c>null</c>.</param>
        /// <returns><c>true</c> if a match was found and <paramref name="selectedOpportunities"/> is valid, <c>false</c> otherwise.</returns>
        protected virtual bool TryGetFirstValidSelectionForGroup( AreaOpportunity area, GroupOpportunity group, Attendee person, out OpportunitySelectionBag selectedOpportunities )
        {
            foreach ( var locationId in group.LocationIds )
            {
                var location = person.Opportunities.Locations
                    .FirstOrDefault( l => l.Id == locationId );

                if ( location == null )
                {
                    continue;
                }

                foreach ( var scheduleId in location.ScheduleIds )
                {
                    var schedule = person.Opportunities.Schedules
                        .FirstOrDefault( s => s.Id == scheduleId );

                    if ( schedule == null )
                    {
                        continue;
                    }

                    selectedOpportunities = GetSelectedOpportunities( area, group, location, schedule );

                    return true;
                }
            }

            selectedOpportunities = null;

            return false;
        }

        /// <summary>
        /// This is a convenience method to get the <see cref="OpportunitySelectionBag"/>
        /// from the given values.
        /// </summary>
        /// <param name="area">The selectedarea.</param>
        /// <param name="group">The selected group.</param>
        /// <param name="location">The selected location.</param>
        /// <param name="schedule">The selected schedule.</param>
        /// <returns>An instance of <see cref="OpportunitySelectionBag"/>.</returns>
        protected OpportunitySelectionBag GetSelectedOpportunities( AreaOpportunity area, GroupOpportunity group, LocationOpportunity location, ScheduleOpportunity schedule )
        {
            return new OpportunitySelectionBag
            {
                Area = new CheckInItemBag
                {
                    Id = area.Id,
                    Name = area.Name
                },
                Group = new CheckInItemBag
                {
                    Id = group.Id,
                    Name = group.Name
                },
                Location = new CheckInItemBag
                {
                    Id = location.Id,
                    Name = location.Name
                },
                Schedule = new CheckInItemBag
                {
                    Id = schedule.Id,
                    Name = schedule.Name
                }
            };
        }
    }
}
