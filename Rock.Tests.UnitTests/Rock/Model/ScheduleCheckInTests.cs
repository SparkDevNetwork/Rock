using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    [TestClass]
    public class ScheduleCheckInTests
    {
        /// <summary>
        /// Checks for valid CheckOut condition involving a schedule that has a start time at 11PM
        /// with an end time on 2AM the next day.
        /// </summary>
        [TestMethod]
        public void CheckOutTimeIsWithinWindowOnAScheduleThatSpillsToASecondDay()
        {
            // Sunday, Thursday 11AM to 2AM
            var schedule = ScheduleWithCheckOut11PMto2AM();

            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-08 11:01PM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-08 1:00AM" ) ) );
        }

        /// <summary>
        /// Checks for no CheckOut condition involving a schedule that has a start time at 11PM
        /// with an end time on 2AM the next day.
        /// </summary>
        [TestMethod]
        public void CheckOutTimeIsOutsideWindowOnAScheduleThatSpillsToASecondDay()
        {
            // Sunday, Thursday 11AM to 2AM
            var schedule = ScheduleWithCheckOut11PMto2AM();

            Assert.That.IsFalse( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-08 10:45PM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-08 3:00AM" ) ) );
        }

        /// <summary>
        /// Checks for valid CheckOut condition involving a standard 9AM to 10AM schedule with a check-in
        /// that starts 30 minutes before start of schedule and *check-in* ends 30 minutes after start of schedule.
        /// </summary>
        [TestMethod]
        public void CheckOutTimeIsWithinWindowOnASameDay()
        {
            // Sunday 9AM to 10AM (check-in starts 30 min before start time and ends 30 minutes after start time)
            var schedule = Standard9AMto10AMSchedule();

            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 8:31 AM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 9:00 AM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 10:00:00 AM" ) ) ); // hurry!!!
            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 9:55 AM" ) ) );
            Assert.That.IsTrue( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 9:59:59AM" ) ) );
        }

        /// <summary>
        /// Checks for no CheckOut condition involving a standard 9AM to 10AM schedule with a check-in
        /// that starts 30 minutes before start of schedule and *check-in* ends 30 minutes after start of schedule.
        /// </summary>
        [TestMethod]
        public void CheckOutTimeIsOutsideWindowOnASameDay()
        {
            // Sunday 9AM to 10AM (check-in starts 30 min before start time and ends 30 minutes after start time)
            var schedule = Standard9AMto10AMSchedule();

            Assert.That.IsFalse( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 8:29:59 AM" ) ) );
            Assert.That.IsFalse( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 10:00:01 AM" ) ) ); // just missed it!
            Assert.That.IsFalse( schedule.WasScheduleOrCheckInActiveForCheckOut( DateTime.Parse( "2019-08-04 9:05 PM" ) ) );
        }

        #region Helper Methods - Sample Schedules

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

        #endregion
    }
}