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
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on if the group
    /// requires that the person be scheduled in order to check-in.
    /// </summary>
    internal class ScheduleRequirementOpportunityFilter : OpportunityFilter
    {
        #region Properties

        /// <summary>
        /// The details about the attendances this person has been scheduled
        /// for by group scheduling.
        /// </summary>
        private Lazy<List<ScheduledAttendance>> ScheduledAttendances { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleRequirementOpportunityFilter"/> class.
        /// </summary>
        public ScheduleRequirementOpportunityFilter()
        {
            ScheduledAttendances = new Lazy<List<ScheduledAttendance>>( GetScheduledAttendanceInformation, true );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            // Scheduling is not enabled, group is valid.
            if ( !group.CheckInAreaData.IsSchedulingEnabled )
            {
                return true;
            }

            // No scheduling requirement, group is valid.
            if ( group.CheckInData.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.ScheduleNotRequired )
            {
                return true;
            }

            // No scheduling requirement, but pre-select if the group has been
            // scheduled.
            if ( group.CheckInData.AttendanceRecordRequiredForCheckIn == AttendanceRecordRequiredForCheckIn.ScheduleRequired )
            {
                // Remove any location schedules the attendee has not been scheduled for.
                group.Locations.RemoveAll( l => !HasScheduledAttendance( group.Id, l ) );
                group.OverflowLocations.RemoveAll( l => !HasScheduledAttendance( group.Id, l ) );

                // If there are no locations left, the group is not valid.
                if ( group.Locations.Count == 0 && group.OverflowLocations.Count == 0 )
                {
                    return false;
                }
            }

            // If the requirement is ScheduleRequired or PreSelect then we want
            // to preselect any opportunities they are scheduled for.
            var locations = group.Locations
                .Where( l => HasScheduledAttendance( group.Id, l ) );
            var overflowLocations = group.Locations
                .Where( l => HasScheduledAttendance( group.Id, l ) );

            AddPreSelections( locations.Union( overflowLocations ), group );

            return true;
        }

        /// <summary>
        /// Determines if the group, location and schedule were found in the
        /// scheduled attendances for this person.
        /// </summary>
        /// <param name="groupId">The identifier of the group opportunity.</param>
        /// <param name="locationAndSchedule">The bag that has the location and schedule identifiers.</param>
        /// <returns><c>true</c> if there is a scheduled attendance record; otherwise <c>false</c>.</returns>
        private bool HasScheduledAttendance( string groupId, LocationAndScheduleBag locationAndSchedule )
        {
            return ScheduledAttendances.Value.Any( a => a.GroupId == groupId
                && a.LocationId == locationAndSchedule.LocationId
                && a.ScheduleId == locationAndSchedule.ScheduleId );
        }

        /// <summary>
        /// Add pre-selections for the attendee for the given locations and
        /// schedules.
        /// </summary>
        /// <param name="locationSchedules">The set of locations and schedules to pre-select.</param>
        /// <param name="group">The group to pre-select.</param>
        private void AddPreSelections( IEnumerable<LocationAndScheduleBag> locationSchedules, GroupOpportunity group )
        {
            foreach ( var locationSchedule in locationSchedules )
            {
                var hasSelectionAlready = Person.PreSelectedOpportunities
                    .Any( o => o.Group.Id == group.Id
                        && o.Location.Id == locationSchedule.LocationId
                        && o.Schedule.Id == locationSchedule.ScheduleId );

                if ( hasSelectionAlready )
                {
                    continue;
                }

                var area = Person.Opportunities.Areas.FirstOrDefault( a => a.Id == group.AreaId );
                var location = Person.Opportunities.Locations.FirstOrDefault( l => l.Id == locationSchedule.LocationId );
                var schedule = Person.Opportunities.Schedules.FirstOrDefault( s => s.Id == locationSchedule.ScheduleId );

                if ( area == null || location == null || schedule == null )
                {
                    continue;
                }

                Person.PreSelectedOpportunities.Add( new OpportunitySelectionBag
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
                } );
            }
        }

        /// <summary>
        /// Gets the information about existing attendance records used by group
        /// scheduling to have previously scheduled this person to work.
        /// </summary>
        /// <returns>A list of objects that describe which groups, locations and schedules they were scheduled for.</returns>
        private List<ScheduledAttendance> GetScheduledAttendanceInformation()
        {
            var today = RockDateTime.Today;
            var idHasher = IdHasher.Instance;
            var personIdNumber = idHasher.GetId( Person.Person.Id ) ?? 0;

            // get attendance records where the person was scheduled (doesn't matter if they confirmed or declined)
            return new AttendanceService( RockContext ).Queryable()
                .Where( a => ( a.ScheduledToAttend == true || a.RequestedToAttend == true )
                     && a.PersonAlias.PersonId == personIdNumber
                     && a.Occurrence.OccurrenceDate == today.Date
                     && a.Occurrence.GroupId.HasValue
                     && a.Occurrence.LocationId.HasValue
                     && a.Occurrence.ScheduleId.HasValue )
                .Select( a => new
                {
                    GroupId = a.Occurrence.GroupId.Value,
                    LocationId = a.Occurrence.LocationId.Value,
                    ScheduleId = a.Occurrence.ScheduleId.Value
                } )
                .ToList()
                .Select( a => new ScheduledAttendance
                {
                    GroupId = idHasher.GetHash( a.GroupId ),
                    LocationId = idHasher.GetHash( a.LocationId ),
                    ScheduleId = idHasher.GetHash( a.ScheduleId )
                } ).ToList();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Information about an attenance record from group scheduling.
        /// </summary>
        private class ScheduledAttendance
        {
            /// <summary>
            /// The identifier of the group.
            /// </summary>
            public string GroupId { get; set; }

            /// <summary>
            /// The identifier of the location.
            /// </summary>
            public string LocationId { get; set; }

            /// <summary>
            /// The identifier of the schedule.
            /// </summary>
            public string ScheduleId { get; set; }
        }

        #endregion
    }
}
