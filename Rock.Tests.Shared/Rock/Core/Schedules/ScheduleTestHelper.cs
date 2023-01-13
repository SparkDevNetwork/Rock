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
using Rock.Model;

using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;
using System.Linq;

namespace Rock.Tests
{
    /// <summary>
    /// Provides useful functions for testing Schedules.
    /// </summary>
    public static class ScheduleTestHelper
    {
        public static Schedule GetScheduleWithDailyRecurrence( DateTime? startDateTime = null, DateTime? endDateTime = null, TimeSpan? eventDuration = null, int? occurrenceCount = null )
        {
            startDateTime = startDateTime ?? new DateTime( RockDateTime.Today.Ticks, DateTimeKind.Unspecified );
            var calendarEvent = GetCalendarEvent( startDateTime.Value, eventDuration );

            var recurrence = GetDailyRecurrencePattern( endDateTime, occurrenceCount );

            var calendar = GetCalendar( calendarEvent, recurrence );

            var schedule = GetSchedule( calendar );

            return schedule;
        }

        public static Schedule GetScheduleWithSpecificDates( List<DateTime> dates, TimeSpan? startTime = null, TimeSpan? eventDuration = null )
        {
            if ( dates == null || !dates.Any() )
            {
                throw new ArgumentException( nameof( dates ) );
            }

            // Get the template calendar event.
            var firstDate = dates.First().Date.Add( startTime.GetValueOrDefault() );
            firstDate = DateTime.SpecifyKind( firstDate, DateTimeKind.Unspecified );

            //var startDateTime = dates.First().Add( startTime.GetValueOrDefault() );

            var calendarEvent = GetCalendarEvent( firstDate, eventDuration );

            var recurrenceDates = new PeriodList();
            foreach ( var datetime in dates )
            {
                recurrenceDates.Add( new CalDateTime( datetime ) );
            }

            calendarEvent.RecurrenceDates.Add( recurrenceDates );

            var calendar = GetCalendar( calendarEvent );
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

        public static Calendar GetCalendar( CalendarEvent calendarEvent, RecurrencePattern recurrencePattern = null )
        {
            if ( recurrencePattern != null )
            {
                calendarEvent.RecurrenceRules = new List<RecurrencePattern> { recurrencePattern };
            }

            var calendar = new Calendar();

            calendar.Events.Add( calendarEvent );

            return calendar;
        }

        public static CalendarEvent GetCalendarEvent( DateTime eventStartDate, TimeSpan? eventDuration )
        {
            if ( eventStartDate.Kind != DateTimeKind.Unspecified )
            {
                throw new Exception( "The Event Start Date must have a Kind of Unspecified. Calendar Events do not store timezone information." );
            }

            //eventDuration = eventDuration ?? new TimeSpan( 1, 0, 0 );
            var calendarEvent = new CalendarEvent
            {
                DtStamp = new CalDateTime( eventStartDate.Year, eventStartDate.Month, eventStartDate.Day )
            };

            var dtStart = new CalDateTime( eventStartDate );
            dtStart.HasTime = true;
            calendarEvent.DtStart = dtStart;

            if ( eventDuration != null )
            {
                var dtEnd = dtStart.Add( eventDuration.Value );
                dtEnd.HasTime = true;
                calendarEvent.DtEnd = dtEnd;
            }

            return calendarEvent;
        }

        public static CalendarEvent GetCalendarEvent( DateTimeOffset eventStartDate, TimeSpan? eventDuration )
        {
            // Convert the start date to Rock time.
            var startDate = TimeZoneInfo.ConvertTime( eventStartDate, RockDateTime.OrgTimeZoneInfo );
            eventDuration = eventDuration ?? new TimeSpan( 1, 0, 0 );

            var dtStart = new CalDateTime( startDate.DateTime );
            dtStart.HasTime = true;

            var dtEnd = dtStart.Add( eventDuration.Value );
            dtEnd.HasTime = true;

            var calendarEvent = new CalendarEvent
            {
                DtStart = dtStart,
                DtEnd = dtEnd,
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
