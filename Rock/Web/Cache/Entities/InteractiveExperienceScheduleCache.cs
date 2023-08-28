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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an interactive experience schedule required to obtain
    /// fast access to the details.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class InteractiveExperienceScheduleCache : ModelCache<InteractiveExperienceScheduleCache, InteractiveExperienceSchedule>
    {
        #region Fields

        /// <summary>
        /// The cached calendar content that will be used to determine
        /// schedule details.
        /// </summary>
        private string _calendarContent;

        /// <summary>
        /// The number of minutes before the schedule starts to enable it.
        /// </summary>
        private int _enableMinutesBefore;

        /// <summary>
        /// The friendly schedule text used when rendering the name of this item.
        /// </summary>
        private string _friendlyScheduleText;

        #endregion

        #region Properties

        /// <inheritdoc cref="InteractiveExperienceSchedule.InteractiveExperienceId"/>
        [DataMember]
        public int InteractiveExperienceId { get; private set; }

        /// <inheritdoc cref="InteractiveExperienceSchedule.ScheduleId"/>
        [DataMember]
        public int ScheduleId { get; private set; }

        /// <inheritdoc cref="InteractiveExperienceSchedule.DataViewId"/>
        [DataMember]
        public int? DataViewId { get; private set; }

        /// <inheritdoc cref="InteractiveExperienceSchedule.GroupId"/>
        [DataMember]
        public int? GroupId { get; private set; }

        /// <summary>
        /// Gets or sets the campus identifiers associated with this schedule.
        /// </summary>
        /// <value>
        /// The campus identifiers associated with this schedule.
        /// </value>
        [DataMember]
        public IEnumerable<int> CampusIds { get; private set; }

        /// <summary>
        /// Gets the <see cref="InteractiveExperience"/> cache representation that
        /// this schedule belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="InteractiveExperience"/> cache representation that this schedule belongs to.
        /// </value>
        public InteractiveExperienceCache InteractiveExperience => InteractiveExperienceCache.Get( InteractiveExperienceId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is InteractiveExperienceSchedule interactiveExperienceSchedule ) )
            {
                return;
            }

            InteractiveExperienceId = interactiveExperienceSchedule.InteractiveExperienceId;
            ScheduleId = interactiveExperienceSchedule.ScheduleId;
            DataViewId = interactiveExperienceSchedule.DataViewId;
            GroupId = interactiveExperienceSchedule.GroupId;

            CampusIds = interactiveExperienceSchedule.InteractiveExperienceScheduleCampuses
                .Select( c => c.CampusId )
                .ToList();

            _calendarContent = interactiveExperienceSchedule.Schedule.iCalendarContent;
            _enableMinutesBefore = interactiveExperienceSchedule.Schedule.CheckInStartOffsetMinutes ?? 0;
            _friendlyScheduleText = interactiveExperienceSchedule.Schedule.ToFriendlyScheduleText( true );
        }

        /// <summary>
        /// Gets the current occurrence date time for the schedule. This looks
        /// for a schedule that started in the past and whose duration puts the
        /// end date and time past the current date and time.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> that represents the occurrence date and time or <c>null</c> if the schedule does not have an active occurrence date and time.</returns>
        internal DateTime? GetCurrentOccurrenceDateTime()
        {
            var now = RockDateTime.Now;

            var schedule = new Schedule
            {
                iCalendarContent = _calendarContent
            };

            // Look for a schedule that started sometime between midnight today
            // and the current time. The cast to DateTime? ensures we get a null
            // value back, otherwise we get a 1/1/0001 date if nothing found.
            return schedule.GetScheduledStartTimes( now.Date, now.AddMinutes( _enableMinutesBefore ) )
                .Where( dt => dt.AddMinutes( -_enableMinutesBefore ) <= now && dt.AddMinutes( schedule.DurationInMinutes ) > now )
                .Select( dt => ( DateTime? ) dt )
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{InteractiveExperience.Name} ({_friendlyScheduleText})";
        }

        #endregion
    }
}