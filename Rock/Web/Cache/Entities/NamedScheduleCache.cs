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
    /// Information about a named <see cref="Rock.Model.Schedule"/> that is required by the rendering engine.
    /// This information will be cached by the engine.
    /// </summary>
    [Serializable]
    [DataContract]
    public class NamedScheduleCache : ModelCache<NamedScheduleCache, Rock.Model.Schedule>
    {
        #region Fields

        private Ical.Net.CalendarComponents.CalendarEvent _calendarEvent;

        #endregion

        #region Properties

        /// <inheritdoc cref="Rock.Model.Schedule.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.CategoryId" />
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.FriendlyScheduleText" />
        [DataMember]
        public string FriendlyScheduleText { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.StartTimeOfDay" />
        [DataMember]
        public TimeSpan StartTimeOfDay { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.Category" />
        public CategoryCache Category => this.CategoryId.HasValue ? CategoryCache.Get( CategoryId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Schedule.CheckInStartOffsetMinutes" />
        public int? CheckInStartOffsetMinutes { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.CheckInEndOffsetMinutes" />
        public int? CheckInEndOffsetMinutes { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.IsCheckInEnabled" />
        public bool IsCheckInEnabled { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.iCalendarContent" />
        private string CalendarContent { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                if ( Name.IsNullOrWhiteSpace() )
                {
                    // just in case this isn't a named Schedule, expire after 10 minutes
                    return new TimeSpan( 0, 10, 0 );
                }

                return base.Lifespan;
            }
        }
        
        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            Rock.Model.Schedule schedule = entity as Rock.Model.Schedule;
            if ( schedule == null )
            {
                return;
            }

            this.Name = schedule.Name;
            this.CategoryId = schedule.CategoryId;
            this.IsActive = schedule.IsActive;
            this.FriendlyScheduleText = schedule.ToFriendlyScheduleText();
            this.CalendarContent = schedule.iCalendarContent;
            this.StartTimeOfDay = schedule.StartTimeOfDay;
            this.CheckInStartOffsetMinutes = schedule.CheckInStartOffsetMinutes;
            this.CheckInEndOffsetMinutes = schedule.CheckInEndOffsetMinutes;
            this.IsCheckInEnabled = schedule.IsCheckInEnabled;
        }


        /// <summary>
        /// Gets the calendar event. This ensures we only create it one time.
        /// </summary>
        /// <returns>An instance of <see cref="Ical.Net.CalendarComponents.CalendarEvent"/>.</returns>
        private Ical.Net.CalendarComponents.CalendarEvent GetCalendarEvent()
        {
            if ( _calendarEvent == null )
            {
                _calendarEvent = InetCalendarHelper.CreateCalendarEvent( CalendarContent );
            }

            return _calendarEvent;
        }

        /// <summary>
        /// Returns value indicating if the schedule was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <returns><c>true</c> if the schedule was active; <c>false</c> otherwise.</returns>
        public bool WasScheduleActive( DateTime time )
        {
            return Schedule.WasScheduleActive( time,
                GetCalendarEvent(),
                CategoryId,
                CalendarContent );
        }

        /// <summary>
        /// Returns value indicating if check-in was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <returns><c>true</c> if the schedule was active; <c>false</c> otherwise.</returns>
        public bool WasCheckInActive( DateTime time )
        {
            if ( !CheckInStartOffsetMinutes.HasValue || !IsActive )
            {
                return false;
            }

            return Schedule.WasCheckInActive( time,
                GetCalendarEvent(),
                CheckInStartOffsetMinutes.Value,
                CheckInEndOffsetMinutes,
                CategoryId,
                CalendarContent );
        }

        /// <summary>
        /// Determines whether a schedule is active for check-out for the specified time.
        /// </summary>
        /// <example>
        /// CheckOut Window: 5/1/2013 11:00:00 PM - 5/2/2013 2:00:00 AM
        /// 
        ///  * Current time: 8/8/2019 11:01:00 PM - returns true
        ///  * Current time: 8/8/2019 10:59:00 PM - returns false
        ///  * Current time: 8/8/2019 1:00:00 AM - returns true
        ///  * Current time: 8/8/2019 2:01:00 AM - returns false
        ///
        /// Note: Add any other test cases you want to test to the "Rock.Tests.Rock.Model.ScheduleCheckInTests" project.
        /// </example>
        /// <param name="time">The time.</param>
        /// <returns>
        ///   <c>true</c> if the schedule is active for check out at the specified time; otherwise, <c>false</c>.
        /// </returns>
        private bool IsScheduleActiveForCheckOut( DateTime time )
        {
            return Schedule.IsScheduleActiveForCheckOut( time,
                GetCalendarEvent(),
                CheckInStartOffsetMinutes.Value,
                CategoryId,
                CalendarContent );
        }

        /// <summary>
        /// Returns value indicating if check-in was active at a current time for this schedule.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <returns><c>true</c> if the schedule was active; <c>false</c> otherwise.</returns>
        public bool WasScheduleOrCheckInActive( DateTime time )
        {
            return WasScheduleActive( time ) || WasCheckInActive( time );
        }

        /// <summary>
        /// Determines if the schedule or check-in is active for check out.
        /// Check out can happen while check-in is active or until the event
        /// ends (start time + duration).
        /// </summary>
        /// <param name="time">The time to check.</param>
        /// <returns></returns>
        public bool WasScheduleOrCheckInActiveForCheckOut( DateTime time )
        {
            return IsScheduleActiveForCheckOut( time ) || WasScheduleActive( time ) || WasCheckInActive( time );
        }

        /// <summary>
        /// Gets the check in times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <returns>A list of <see cref="CheckInTimes"/> objects.</returns>
        public virtual List<CheckInTimes> GetCheckInTimes( DateTime beginDateTime )
        {
            if ( IsCheckInEnabled )
            {
                return Schedule.GetCheckInTimes( beginDateTime, CheckInStartOffsetMinutes.Value, CheckInEndOffsetMinutes, CalendarContent, () => GetCalendarEvent() );
            }

            return new List<CheckInTimes>();
        }

        /// <summary>
        /// Gets the next check in start time.
        /// </summary>
        /// <param name="beginDateTime">The begindate time.</param>
        /// <returns>The next <see cref="DateTime"/> that check-in will be active today or <c>null</c> if it will not be active anymore.</returns>
        public virtual DateTime? GetNextCheckInStartTime( DateTime beginDateTime )
        {
            var checkInTimes = GetCheckInTimes( beginDateTime );
            if ( checkInTimes != null && checkInTimes.Any() )
            {
                return checkInTimes.FirstOrDefault().CheckInStart;
            }

            return null;
        }

        /// <summary>
        /// returns <see cref="FriendlyScheduleText"/>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FriendlyScheduleText;
        }

        /// <inheritdoc cref="Rock.Model.Schedule.GetICalOccurrences(DateTime, DateTime?, DateTime?)" />
        public IList<Ical.Net.DataTypes.Occurrence> GetICalOccurrences( DateTime beginDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            return InetCalendarHelper.GetOccurrences( CalendarContent, beginDateTime, endDateTime, scheduleStartDateTimeOverride );
        }

        #endregion Public Methods
    }
}
