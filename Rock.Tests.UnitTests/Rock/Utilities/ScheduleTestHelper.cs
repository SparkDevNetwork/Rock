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
using Rock.Model;

using Ical.Net;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Rock.Tests
{
    /// <summary>
    /// Provides useful functions for testing Schedules.
    /// </summary>
    public static class ScheduleTestHelper
    {
        public static Schedule GetScheduleWithDailyRecurrence( DateTimeOffset? startDateTime = null, DateTimeOffset? endDate = null, TimeSpan? eventDuration = null, int? occurrenceCount = null )
        {
            var calendarEvent = GetCalendarEvent( startDateTime ?? RockDateTime.Today, eventDuration );

            var recurrence = GetDailyRecurrencePattern( endDate, occurrenceCount );

            var calendar = GetCalendar( calendarEvent, recurrence );

            var schedule = GetSchedule( calendar );

            return schedule;
        }

        public static Schedule GetSchedule( Calendar calendar )
        {
            var schedule = new Schedule();

            var serializer = new CalendarSerializer( calendar );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            return schedule;
        }

        public static Calendar GetCalendar( Event calendarEvent, RecurrencePattern recurrencePattern = null )
        {
            if ( recurrencePattern != null )
            {
                calendarEvent.RecurrenceRules = new List<IRecurrencePattern> { recurrencePattern };
            }

            var calendar = new Calendar();

            calendar.Events.Add( calendarEvent );

            return calendar;
        }

        public static Event GetCalendarEvent( DateTimeOffset eventStartDate, TimeSpan? eventDuration )
        {
            // Convert the start date to Rock time.
            var startDate = TimeZoneInfo.ConvertTime( eventStartDate, RockDateTime.OrgTimeZoneInfo );

            var calendarEvent = new Event
            {
                DtStart = new CalDateTime( startDate.DateTime ),
                Duration = eventDuration ?? new TimeSpan( 1, 0, 0 ),
                DtStamp = new CalDateTime( eventStartDate.Year, eventStartDate.Month, eventStartDate.Day ),
            };

            return calendarEvent;
        }

        public static RecurrencePattern GetDailyRecurrencePattern( DateTimeOffset? recurrenceEndDate = null, int? occurrenceCount = null, int? interval = 1 )
        {
            // Repeat daily from the start date until the specified end date or a set number of recurrences, at the specified interval.
            var pattern = $"RRULE:FREQ=DAILY;INTERVAL={interval}";

            if ( recurrenceEndDate != null )
            {
                pattern += $";UNTIL={recurrenceEndDate:yyyyMMdd}";
            }

            if ( occurrenceCount != null )
            {
                pattern += $";COUNT={occurrenceCount}";
            }

            var recurrencePattern = new RecurrencePattern( pattern );

            return recurrencePattern;
        }
    }
}
