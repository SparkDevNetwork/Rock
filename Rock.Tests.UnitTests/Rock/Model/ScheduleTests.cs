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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Tests for the Schedule class that do not require a database connection.
    /// </summary>
    [TestClass]
    public class ScheduleTests
    {
        private static Calendar _calendarSpecificDates;
        private static List<DateTime> _specificDates;
        private static CalendarSerializer _calendarSerializer = new CalendarSerializer();

        #region Test Initialize and Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            var today = RockDateTime.Now;

            var nextMonth = RockDateTime.Now.AddMonths( 1 ).Month;

            // Add specific dates that exist for every month of the year.
            _specificDates = new List<DateTime>();
            _specificDates.Add( new DateTime( today.Year, nextMonth, 1 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 3 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 5 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 7 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 10 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 20 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 28 ) );

            var recurrenceDates = new PeriodList();

            _specificDates.ForEach( x => recurrenceDates.Add( new CalDateTime( x ) ) );

            var firstDate = _specificDates.First();

            // Create a calendar for an event that recurs on specific dates.
            _calendarSpecificDates = new Calendar()
            {
                // Create an event for the first scheduled date (1am-2am), and set the recurring dates.
                Events = { new CalendarEvent
                    {
                        DtStart = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day, 1, 0, 0 ),
                        DtEnd = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day, 2, 0, 0 ),
                        DtStamp = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day ),
                        RecurrenceDates = new List<PeriodList> { recurrenceDates },
                        Sequence = 0,
                    }
                }
            };
        }

        #endregion

        /// <summary>
        /// A schedule that specifies discrete dates rather than a recurrence pattern should return all dates for GetOccurrences() if no end date is provided.
        /// </summary>
        [TestMethod]
        public void Schedule_WithSpecificDates_GetOccurrencesReturnsAllDates()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var schedule = new Schedule();

                schedule.iCalendarContent = _calendarSerializer.SerializeToString( _calendarSpecificDates );

                schedule.EnsureEffectiveStartEndDates();

                var endDateSpecified = _specificDates.LastOrDefault();

                var scheduleDates = schedule.GetICalOccurrences( _specificDates.First(), _specificDates.Last(), _specificDates.First() );

                var endDateReturned = scheduleDates.LastOrDefault();

                Assert.That.IsNotNull( endDateReturned );
                Assert.That.AreEqualDate( endDateSpecified, endDateReturned.Period.StartTime.Date, "Unexpected value for Last Occurrence Date." );
                Assert.That.AreEqual( _specificDates.Count, scheduleDates.Count, "Incorrect number of Occurrences returned from Schedule." );
            } );
        }

        /// <summary>
        /// A schedule that specifies a single date with no recurrence pattern should have an effective end date matching the start date.
        /// </summary>
        [TestMethod]
        public void Schedule_WithOneTimeSingleDayEvent_HasEffectiveEndDateEqualToStartDate()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var singleDayEvent = ScheduleTestHelper.GetCalendarEvent( GetFirstTestScheduleDate(), new TimeSpan( 1, 0, 0 ) );

                var schedule = ScheduleTestHelper.GetSchedule( ScheduleTestHelper.GetCalendar( singleDayEvent ) );

                var endDateSpecified = _specificDates.FirstOrDefault();

                var endDateReturned = schedule.EffectiveEndDate;

                Assert.That.IsNotNull( endDateReturned );
                Assert.That.AreEqualDate( endDateSpecified, endDateReturned.Value.Date, "Unexpected value for Last Occurrence Date." );
            } );
        }

        /// <summary>
        /// A schedule that specifies a single date with no recurrence pattern should have an effective end date matching the start date.
        /// </summary>
        [TestMethod]
        public void Schedule_WithOneTimeMultiDayEvent_HasEffectiveEndDateOnLastDayOfEvent()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                // Create an event that has a duration of more than 1 day.
                var eventDate = GetFirstTestScheduleDate();
                var singleDayEvent = ScheduleTestHelper.GetCalendarEvent( eventDate, new TimeSpan( 25, 0, 0 ) );

                var schedule = ScheduleTestHelper.GetSchedule( ScheduleTestHelper.GetCalendar( singleDayEvent ) );

                var endDateExpected = eventDate.AddDays( 1 );
                var endDateReturned = schedule.EffectiveEndDate;

                Assert.That.IsNotNull( endDateReturned );
                Assert.That.AreEqualDate( endDateExpected, endDateReturned.Value.Date, "Unexpected value for EffectiveEndDate." );
            } );
        }

        private DateTime GetFirstTestScheduleDate()
        {
            return _specificDates.First();
        }

        /// <summary>
        /// A schedule that specifies an infinite recurrence pattern should return dates for GetOccurrences() only up to the requested end date.
        /// </summary>
        [TestMethod]
        public void Schedule_SingleDayEventWithInfiniteRecurrencePattern_GetOccurrencesObservesRequestedEndDate()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence();

                var endDate = RockDateTime.Now.Date.AddMonths( 3 );

                var scheduleDates = schedule.GetICalOccurrences( RockDateTime.Now, endDate );

                Assert.That.IsNotNull( scheduleDates.LastOrDefault() );

                // End date is at 12am, so the last occurrence of the event will land on the preceding day.
                Assert.That.AreEqualDate( endDate, scheduleDates.LastOrDefault().Period.StartTime.Date.AddDays( 1 ) );
            } );
        }

        /// <summary>
        /// Schedule occurences should include an event spanning multiple days if that event is currently in progress.
        /// </summary>
        [TestMethod]
        public void Schedule_GetOccurrencesForPeriodStartingDuringMultiDayEvent_IncludesInProgressEvent()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var nowDate = DateTimeTestHelper.GetRockNowDateTimeAsUnspecifiedKind();

                var inProgressEventStartDate = nowDate.AddDays( -1 );

                // Get a calendar that includes a multi-day event that started yesterday and repeats every 7 days.
                var multiDayEvent = ScheduleTestHelper.GetCalendarEvent( inProgressEventStartDate, new TimeSpan( 25, 0, 0 ) );
                var weeklyPattern = ScheduleTestHelper.GetDailyRecurrencePattern( null, null, 7 );
                var calendar = ScheduleTestHelper.GetCalendar( multiDayEvent, weeklyPattern );
                var schedule = ScheduleTestHelper.GetSchedule( calendar );

                // Get events for the next 3 months, starting from today.
                var endRequestDate = nowDate.Date.AddMonths( 3 );

                var scheduleDates = schedule.GetICalOccurrences( nowDate, endRequestDate );

                Assert.That.IsNotNull( scheduleDates.FirstOrDefault() );

                // Verify that the result does not include the event that started yesterday and is in progress today.
                var firstEvent = scheduleDates.FirstOrDefault();

                Assert.That.AreEqualDate( inProgressEventStartDate, firstEvent.Period.StartTime.Date );
            } );
        }

        /// <summary>
        /// Schedule occurences should include an event spanning multiple days if that event begins before the end of the schedule period.
        /// </summary>
        [TestMethod]
        public void Schedule_GetOccurrencesForPeriodEndingDuringMultiDayEvent_IncludesInProgressEvent()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var eventStartDate = GetRockNowDateTimeAsUnspecifiedKind().AddDays( -1 );

                // Get a calendar that includes a multi-day event that started yesterday and repeats daily.
                var calendar = ScheduleTestHelper.GetCalendar( ScheduleTestHelper.GetCalendarEvent( eventStartDate, new TimeSpan( 25, 0, 0 ) ),
                    ScheduleTestHelper.GetDailyRecurrencePattern( null, null, 1 ) );

                var schedule = ScheduleTestHelper.GetSchedule( calendar );

                // Get events for the next 7 days (inclusive), starting from today.
                var lastRequestDate = RockDateTime.Now.Date.AddDays( 8 ).AddMilliseconds( -1 );

                // Get occurrences for the schedule from today.
                var scheduleDates = schedule.GetICalOccurrences( RockDateTime.Now, lastRequestDate );

                Assert.That.IsNotNull( scheduleDates.FirstOrDefault() );

                var lastEvent = scheduleDates.LastOrDefault();

                // Verify that the result includes the event that starts on the last day of the request period but ends on the following day.
                Assert.That.AreEqualDate( lastRequestDate, lastEvent.Period.StartTime.Date );
            } );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern should have an EffectiveEndDate that matches the end of the recurrence.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithFiniteDateRecurrence_EffectiveEndDateMatchesRecurrenceEndDate()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {

                // Create a daily recurring calendar that has an end date of today +3 months.
                var startDate = GetRockNowDateTimeAsUnspecifiedKind();
                var endDate = startDate.AddMonths( 3 );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDate, endDateTime: endDate );

                Assert.That.AreEqualDate( schedule.EffectiveEndDate, endDate.Date );
            } );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern should have an EffectiveEndDate that matches the end of the recurrence.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithFiniteCountRecurrence_EffectiveEndDateMatchesNthOccurrence()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var occurrences = 10;

                // Create a daily recurring calendar that has X occurrences, including today.
                var endDate = RockDateTime.Now.AddDays( occurrences - 1 );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDateTime: null, occurrenceCount: occurrences );

                Assert.That.AreEqualDate( schedule.EffectiveEndDate, endDate.Date );
            } );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern with more than 999 occurrences should have no EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithExcessiveFiniteCountRecurrence_EffectiveEndDateIsUndefined()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var occurrences = 1000;

                // Create a daily recurring calendar that has X occurrences, including today.
                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDateTime: null, occurrenceCount: occurrences );

                Assert.That.AreEqualDate( null, schedule.EffectiveEndDate );
            } );
        }

        /// <summary>
        /// A schedule that specifies an infinite recurrence pattern should have an empty EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithInfiniteRecurrence_EffectiveEndDateIsNull()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                // Create a daily recurring calendar that has no end date.
                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( null, null, new TimeSpan( 1, 0, 0 ), null );

                schedule.EnsureEffectiveStartEndDates();

                Assert.That.AreEqualDate( null, schedule.EffectiveEndDate );
            } );
        }

        /// <summary>
        /// A schedule that specifies discrete dates should have an EffectiveEndDate matching the last discrete date.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithSpecificDates_EffectiveEndDateIsLastSpecifiedDate()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var schedule = new Schedule();

                var serializer = new CalendarSerializer( _calendarSpecificDates );

                schedule.iCalendarContent = serializer.SerializeToString();

                schedule.EnsureEffectiveStartEndDates();

                var endDate = _specificDates.Last();

                Assert.That.AreEqualDate( endDate, schedule.EffectiveEndDate );
            } );
        }

        /// <summary>
        /// A schedule that has a recurrence pattern and is modified to have discrete dates should have a correctly adjusted EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_UpdatedWithSpecificDates_EffectiveEndDateIsLastSpecifiedDate()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                // Create a daily recurring calendar that has an end date of today +3 months.
                var scheduleEndDate = RockDateTime.Now.AddMonths( 3 );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( GetRockNowDateTimeAsUnspecifiedKind(),
                    endDateTime: scheduleEndDate );

                Assert.That.AreEqualDate( scheduleEndDate, schedule.EffectiveEndDate );

                // Modify the Schedule to use a set of discrete dates and verify that the EffectiveEndDate is adjusted correctly.
                var serializer = new CalendarSerializer( _calendarSpecificDates );

                schedule.iCalendarContent = serializer.SerializeToString();

                schedule.EnsureEffectiveStartEndDates();

                var specificEndDate = _specificDates.Last();

                Assert.That.AreEqualDate( specificEndDate, schedule.EffectiveEndDate );
            } );
        }

        [TestMethod]
        public void Schedule_UpdatedFromCustomToWeekly_EffectiveEndDateAndEffectiveStartDateIsNull()
        {
            DateTimeTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                // Create a daily recurring calendar that has an end date of today +3 months.
                var scheduleEndDate = RockDateTime.Now.AddMonths( 3 );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( GetRockNowDateTimeAsUnspecifiedKind(),
                    endDateTime: scheduleEndDate );

                Assert.That.IsNotNull( schedule.EffectiveStartDate );
                Assert.That.IsNotNull( schedule.EffectiveEndDate );

                // Modify schedule to a weekly scheduleType
                schedule.iCalendarContent = null;
                schedule.WeeklyDayOfWeek = DayOfWeek.Friday;
                schedule.WeeklyTimeOfDay = new TimeSpan( 19, 0, 0 );

                schedule.EnsureEffectiveStartEndDates();

                Assert.That.IsNull( schedule.EffectiveStartDate );
                Assert.That.IsNull( schedule.EffectiveEndDate );
            } );
        }

        [TestMethod]
        public void Schedule_WasScheduleActive_ScheduleThatSpillsToASecondDay()
        {
            // Sunday, Thursday 11AM to 2AM
            var schedule = ScheduleWithCheckOut11PMto2AM();

            Assert.That.IsTrue( schedule.WasScheduleActive( DateTime.Parse( "2019-08-08 11:01PM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleActive( DateTime.Parse( "2019-08-08 1:00AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-08 3:00AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-08 2:00PM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-08 10:00PM" ) ) );
        }

        [TestMethod]
        public void Schedule_WasScheduleActive_ScheduleOnSameDay()
        {
            // Sunday, 9AM to 10AM
            var schedule = Standard9AMto10AMSchedule();

            Assert.That.IsTrue( schedule.WasScheduleActive( DateTime.Parse( "2019-08-04 9:00AM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleActive( DateTime.Parse( "2019-08-04 9:01AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-04 10:01AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-04 8:00AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleActive( DateTime.Parse( "2019-08-03 9:00AM" ) ) );
        }

        /// <summary>
        /// Get the current Rock date and time as an Unspecified DateTime type.
        /// </summary>
        /// <returns></returns>
        private DateTime GetRockNowDateTimeAsUnspecifiedKind()
        {
            var now = DateTime.SpecifyKind( RockDateTime.Now, DateTimeKind.Unspecified );
            return now;
        }

        private static Schedule ScheduleWithCheckOut11PMto2AM()
        {
            var iCalendarContent = @"
BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130502T020000
DTSTAMP:20190808T202903Z
DTSTART:20130501T230000
RRULE:FREQ=WEEKLY;BYDAY=SU,TH
SEQUENCE:0
UID:37e42e3e-017c-4317-a79e-e738ce6504ec
END:VEVENT
END:VCALENDAR
";
            var schedule = new Schedule();
            schedule.iCalendarContent = iCalendarContent;
            schedule.CheckInStartOffsetMinutes = 0;
            schedule.CheckInEndOffsetMinutes = 0;
            return schedule;
        }

        private static Schedule Standard9AMto10AMSchedule()
        {
            var iCalendarContent = @"
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T100000
DTSTART:20130501T090000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR
";
            var schedule = new Schedule();
            schedule.iCalendarContent = iCalendarContent;
            schedule.CheckInStartOffsetMinutes = 30;
            schedule.CheckInEndOffsetMinutes = 30;
            return schedule;
        }
    }
}