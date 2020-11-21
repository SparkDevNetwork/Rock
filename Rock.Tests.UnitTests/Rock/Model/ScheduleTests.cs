using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

using Ical.Net;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

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

            _specificDates = new List<DateTime>();
            _specificDates.Add( new DateTime( today.Year, nextMonth, 1 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 3 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 5 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 7 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 10 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 20 ) );
            _specificDates.Add( new DateTime( today.Year, nextMonth, 30 ) );

            var recurrenceDates = new PeriodList();

            _specificDates.ForEach( x => recurrenceDates.Add( new CalDateTime( x ) ) );

            var firstDate = _specificDates.First();

            _calendarSpecificDates = new Calendar()
            {
                // Create an event for the first scheduled date (1am-2am), and set the recurring dates.
                Events = { new Event
                    {
                        DtStart = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day, 1, 0, 0 ),
                        DtEnd = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day, 2, 0, 0 ),
                        DtStamp = new CalDateTime( firstDate.Year, firstDate.Month, firstDate.Day ),
                        RecurrenceDates = new List<IPeriodList> { recurrenceDates },
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
            var schedule = new Schedule();

            schedule.iCalendarContent = _calendarSerializer.SerializeToString( _calendarSpecificDates );

            schedule.EnsureEffectiveStartEndDates();

            var endDateSpecified = _specificDates.LastOrDefault();

            var scheduleDates = schedule.GetICalOccurrences( _specificDates.First(), _specificDates.Last(), _specificDates.First() );

            var endDateReturned = scheduleDates.LastOrDefault();

            Assert.That.IsNotNull( endDateReturned );
            Assert.That.AreEqualDate( endDateSpecified, endDateReturned.Period.StartTime.Date, "Unexpected value for Last Occurrence Date." );
            Assert.That.AreEqual( _specificDates.Count, scheduleDates.Count, "Incorrect number of Occurrences returned from Schedule." );
        }

        /// <summary>
        /// A schedule that specifies an infinite recurrence pattern should return dates for GetOccurrences() only up to the requested end date.
        /// </summary>
        [TestMethod]
        public void Schedule_WithInfiniteRecurrencePattern_GetOccurrencesObservesRequestedEndDate()
        {
            var schedule = GetScheduleWithDailyRecurrence( null );

            var endDate = RockDateTime.Now.Date.AddMonths( 3 );

            var scheduleDates = schedule.GetICalOccurrences( RockDateTime.Now, endDate );

            Assert.That.IsNotNull( scheduleDates.LastOrDefault() );

            // End date is at 12am, so the last occurrence of the event will land on the preceding day.
            Assert.That.AreEqualDate( endDate, scheduleDates.LastOrDefault().Period.StartTime.Date.AddDays( 1 ) );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern should have an EffectiveEndDate that matches the end of the recurrence.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithFiniteDateRecurrence_EffectiveEndDateMatchesRecurrenceEndDate()
        {
            // Create a daily recurring calendar that has an end date of today +3 months.
            var endDate = RockDateTime.Now.AddMonths( 3 );

            var schedule = GetScheduleWithDailyRecurrence( endDate );

            Assert.That.AreEqualDate( schedule.EffectiveEndDate, endDate.Date );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern should have an EffectiveEndDate that matches the end of the recurrence.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithFiniteCountRecurrence_EffectiveEndDateMatchesNthOccurrence()
        {
            var occurrences = 10;

            // Create a daily recurring calendar that has X occurrences, including today.
            var endDate = RockDateTime.Now.AddDays( occurrences - 1 );

            var schedule = GetScheduleWithDailyRecurrence( occurrenceCount: occurrences );

            Assert.That.AreEqualDate( schedule.EffectiveEndDate, endDate.Date );
        }

        /// <summary>
        /// A schedule that specifies a finite recurrence pattern with more than 999 occurrences should have no EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithExcessiveFiniteCountRecurrence_EffectiveEndDateIsUndefined()
        {
            var occurrences = 1000;

            // Create a daily recurring calendar that has X occurrences, including today.
            var schedule = GetScheduleWithDailyRecurrence( occurrenceCount: occurrences );

            Assert.That.AreEqualDate( DateTime.MaxValue, schedule.EffectiveEndDate );
        }

        /// <summary>
        /// A schedule that specifies an infinite recurrence pattern should have an empty EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithInfiniteRecurrence_EffectiveEndDateIsNull()
        {
            // Create a daily recurring calendar that has no end date.
            var calendar = GetCalendarWithDailyRecurrence( null );

            var schedule = new Schedule();

            var serializer = new CalendarSerializer( calendar );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            Assert.That.AreEqualDate( DateTime.MaxValue, schedule.EffectiveEndDate );
        }

        /// <summary>
        /// A schedule that specifies discrete dates should have an EffectiveEndDate matching the last discrete date.
        /// </summary>
        [TestMethod]
        public void Schedule_CreatedWithSpecificDates_EffectiveEndDateIsLastSpecifiedDate()
        {
            var schedule = new Schedule();

            var serializer = new CalendarSerializer( _calendarSpecificDates );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            var endDate = _specificDates.Last();

            Assert.That.AreEqualDate( endDate, schedule.EffectiveEndDate );
        }

        /// <summary>
        /// A schedule that has a recurrence pattern and is modified to have discrete dates should have a correctly adjusted EffectiveEndDate.
        /// </summary>
        [TestMethod]
        public void Schedule_UpdatedWithSpecificDates_EffectiveEndDateIsLastSpecifiedDate()
        {
            // Create a daily recurring calendar that has an end date of today +3 months.
            var scheduleEndDate = RockDateTime.Now.AddMonths( 3 );

            var schedule = GetScheduleWithDailyRecurrence( scheduleEndDate );

            Assert.That.AreEqualDate( scheduleEndDate, schedule.EffectiveEndDate );

            // Modify the Schedule to use a set of discrete dates and verify that the EffectiveEndDate is adjusted correctly.
            var serializer = new CalendarSerializer( _calendarSpecificDates );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            var specificEndDate = _specificDates.Last();

            Assert.That.AreEqualDate( specificEndDate, schedule.EffectiveEndDate );
        }

        #region Helper methods

        private Schedule GetScheduleWithDailyRecurrence( DateTime? endDate = null, int? occurrenceCount = null )
        {
            var calendar = GetCalendarWithDailyRecurrence( endDate, occurrenceCount );

            var schedule = new Schedule();

            var serializer = new CalendarSerializer( calendar );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            return schedule;
        }

        private Calendar GetCalendarWithDailyRecurrence( DateTime? endDate, int? occurrenceCount = null )
        {
            var today = RockDateTime.Now;

            // Repeat daily from today until the specified end date.
            var pattern = "RRULE:FREQ=DAILY";

            if ( endDate != null )
            {
                pattern += $";UNTIL={endDate:yyyyMMdd}";
            }

            if ( occurrenceCount != null )
            {
                pattern += $";COUNT={occurrenceCount}";
            }

            var recurrencePattern = new RecurrencePattern( pattern );

            var calendar = new Calendar()
            {
                Events = { new Event
                    {
                        DtStart = new CalDateTime( today.Year, today.Month, today.Day, 13, 0, 0 ),
                        DtEnd = new CalDateTime( today.Year, today.Month, today.Day, 14, 0, 0 ),
                        DtStamp = new CalDateTime( today.Year, today.Month, today.Day ),
                        RecurrenceRules = new List<IRecurrencePattern> { recurrencePattern }
                    }
                }
            };

            return calendar;
        }

        #endregion
    }
}
