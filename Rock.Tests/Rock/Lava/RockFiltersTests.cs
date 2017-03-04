using System.Collections.Generic;
using System.Linq;
using System;

using Rock;
using Rock.Lava;
using Rock.Model;
using Xunit;


namespace Rock.Tests.Rock.Lava
{
    public class RockFiltersTest
    {
        static readonly Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
        static readonly string iCalStringSaturday430 = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20130501T173000
DTSTAMP:20170303T203737Z
DTSTART:20130501T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:d74561ac-c0f9-4dce-a610-c39ca14b0d6e
END:VEVENT
END:VCALENDAR";

        // First Saturday of month until 2018; ends on 12/7/2019 - 8:00 AM to 10:00 AM
        static readonly string iCalStringFirstSaturdayOfMonthTil2020 = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20170101T100000
DTSTAMP:20170303T215639Z
DTSTART:20170101T080000
RRULE:FREQ=MONTHLY;UNTIL=20200101T000000;BYDAY=1SA
SEQUENCE:0
UID:517d77dd-6fe8-493b-925f-f266aa2d852c
END:VEVENT
END:VCALENDAR";

        #region Minus
        /// <summary>
        /// For use in Lava -- should subtract two integers and return an integer.
        /// </summary>
        [Fact]
        public void MinusTwoInts()
        {
            // I'd like to test via Lava Resolve/MergeFields but can't get that to work.
            //string lava = "{{ 3 | Minus: 2 | ToJSON }}";
            //var person = new Person();
            //var o = lava.ResolveMergeFields( mergeObjects, person, "" );
            //Assert.Equal( "1", o);
            var output = RockFilters.Minus( 3, 2 );
            Assert.Equal( 1, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void MinusTwoDecimals()
        {
            var output = RockFilters.Minus( 3.0M, 2.0M );
            Assert.Equal( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void MinusTwoStringInts()
        {
            var output = RockFilters.Minus( "3", "2" );
            Assert.Equal( 1, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void MinusTwoStringDecimals()
        {
            var output = RockFilters.Minus( "3.0", "2.0" );
            Assert.Equal( 1.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should subtract an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void MinusIntAndDecimal()
        {
            var output = RockFilters.Minus( 3, "2.0" );
            Assert.Equal( 1.0M, output );
        }
        #endregion

        #region Plus
        /// <summary>
        /// For use in Lava -- should add two integers and return an integer.
        /// </summary>
        [Fact]
        public void PlusTwoInts()
        {
            var output = RockFilters.Plus( 3, 2 );
            Assert.Equal( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void PlusTwoDecimals()
        {
            var output = RockFilters.Plus( 3.0M, 2.0M );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void PlusTwoStringInts()
        {
            var output = RockFilters.Plus( "3", "2" );
            Assert.Equal( 5, output );
        }

        /// <summary>
        /// For use in Lava -- should add two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void PlusTwoStringDecimals()
        {
            var output = RockFilters.Plus( "3.0", "2.0" );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should add an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void PlusIntAndDecimal()
        {
            var output = RockFilters.Plus( 3, "2.0" );
            Assert.Equal( 5.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should concat two strings.
        /// </summary>
        [Fact]
        public void PlusStrings()
        {
            var output = RockFilters.Plus( "Foo", "Bar" );
            Assert.Equal( "FooBar", output );
        }

        #endregion

        #region Times
        /// <summary>
        /// For use in Lava -- should multiply two integers and return an integer.
        /// </summary>
        [Fact]
        public void TimesTwoInts()
        {
            var output = RockFilters.Times( 3, 2 );
            Assert.Equal( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two decimals and return a decimal.
        /// </summary>
        [Fact]
        public void TimesTwoDecimals()
        {
            var output = RockFilters.Times( 3.0M, 2.0M );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing integers) and return an int.
        /// </summary>
        [Fact]
        public void TimesTwoStringInts()
        {
            var output = RockFilters.Times( "3", "2" );
            Assert.Equal( 6, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply two strings (containing decimals) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesTwoStringDecimals()
        {
            var output = RockFilters.Times( "3.0", "2.0" );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should multiply an integer and a string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesIntAndDecimal()
        {
            var output = RockFilters.Times( 3, "2.0" );
            Assert.Equal( 6.0M, output );
        }

        /// <summary>
        /// For use in Lava -- should repeat the string (containing a decimal) and return a decimal.
        /// </summary>
        [Fact]
        public void TimesStringAndInt()
        {
            var expectedOutput = Enumerable.Repeat( "Foo", 2 );
            var output = RockFilters.Times( "Foo", 2 );
            Assert.Equal( expectedOutput, output );
        }

        #endregion

        /// <summary>
        /// For use in Lava -- should return next occurrence for Rock's standard Saturday 4:30PM service datetime.
        /// </summary>
        [Fact]
        public void DatesFromICal_OneNextSaturday()
        {
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime nextSaturday = today.AddDays( daysUntilSaturday );

            List<DateTime> expected = new List<DateTime>() { DateTime.Parse( nextSaturday.ToShortDateString() + " 4:30:00 PM" ) };

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, 1 );
            Assert.Equal( expected, output );
        }


        /// <summary>
        /// For use in Lava -- should return the current Saturday for next year's occurrence for Rock's standard Saturday 4:30PM service datetime.
        /// </summary>
        [Fact]
        public void DatesFromICal_NextYearSaturday()
        {
            // Next year's Saturday (from right now)
            DateTime nextYear = RockDateTime.Now.AddYears( 1 );
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) nextYear.DayOfWeek + 7 ) % 7;
            DateTime nextYearSaturday = nextYear.AddDays( daysUntilSaturday );

            DateTime expected = DateTime.Parse( nextYearSaturday.ToShortDateString() + " 4:30:00 PM" );

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, 100 ).LastOrDefault();
            Assert.Equal( expected, output );
        }

        /// <summary>
        /// For use in Lava -- should return the end datetime for the next occurrence for Rock's standard Saturday 4:30PM service datetime (which ends at 5:30PM).
        /// </summary>
        [Fact]
        public void DatesFromICal_NextEndOccurrenceSaturday()
        {
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime nextSaturday = today.AddDays( daysUntilSaturday );

            List<DateTime> expected = new List<DateTime>() { DateTime.Parse( nextSaturday.ToShortDateString() + " 5:30:00 PM" ) };

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, null, "enddatetime" );
            Assert.Equal( expected, output );
        }

        /// <summary>
        /// For use in Lava -- should find the end datetime (10 AM) occurrence for the fictitious, first Saturday of the month event for Saturday a year from today.
        /// </summary>
        [Fact]
        public void DatesFromICal_NextYearsEndOccurrenceSaturday()
        {
            DateTime todayNextYear = RockDateTime.Today.AddYears( 1 );
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) todayNextYear.DayOfWeek + 7 ) % 7;
            DateTime nextYearSaturday = todayNextYear.AddDays( daysUntilSaturday );

            DateTime expected = DateTime.Parse( nextYearSaturday.ToShortDateString() + " 10:00:00 AM" );

            var output = RockFilters.DatesFromICal( iCalStringFirstSaturdayOfMonthTil2020, 13, "enddatetime" ).LastOrDefault();
            Assert.Equal( expected, output );
        }
    }
}