using System;
using System.Collections.Generic;
using System.Linq;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Rock.Lava;
using Xunit;

namespace Rock.Tests.Rock.Lava
{
    public class RockFiltersTest
    {
        private static readonly Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
        private static iCalendarSerializer serializer = new iCalendarSerializer();
        private static RecurrencePattern weeklyRecurrence = new RecurrencePattern( "RRULE:FREQ=WEEKLY;BYDAY=SA" );
        private static RecurrencePattern monthlyRecurrence = new RecurrencePattern( "RRULE:FREQ=MONTHLY;BYDAY=1SA" );

        private static readonly DateTime today = RockDateTime.Today;

        private static readonly iCalendar weeklySaturday430 = new iCalendar()
        {
            Events =
            {
                new Event
                    {
                        DTStart = new iCalDateTime( today.Year, today.Month, today.Day + DayOfWeek.Saturday - today.DayOfWeek, 16, 30, 0 ),
                        DTEnd = new iCalDateTime( today.Year, today.Month, today.Day + DayOfWeek.Saturday - today.DayOfWeek, 17, 30, 0 ),
                        DTStamp = new iCalDateTime( today.Year, today.Month, today.Day ),
                        RecurrenceRules = new List<IRecurrencePattern> { weeklyRecurrence },
                        Sequence = 0,
                        UID = @"d74561ac-c0f9-4dce-a610-c39ca14b0d6e"
                    }
                }
        };

        private static readonly iCalendar monthlyFirstSaturday = new iCalendar()
        {
            Events =
            {
                new Event
                    {
                        DTStart = new iCalDateTime( today.Year, today.Month, today.Day, 8, 0, 0 ),
                        DTEnd = new iCalDateTime( today.Year, today.Month, today.Day, 10, 0, 0 ),
                        DTStamp = new iCalDateTime( today.Year, today.Month, today.Day ),
                        RecurrenceRules = new List<IRecurrencePattern> { monthlyRecurrence },
                        Sequence = 0,
                        UID = @"517d77dd-6fe8-493b-925f-f266aa2d852c"
                    }
                }
        };

        private static readonly string iCalStringSaturday430 = serializer.SerializeToString( weeklySaturday430 );
        private static readonly string iCalStringFirstSaturdayOfMonth = serializer.SerializeToString( monthlyFirstSaturday );

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

        #region AsInteger

        /// <summary>
        /// For use in Lava -- should not cast the null to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_Null()
        {
            var output = RockFilters.AsInteger( null );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_InvalidBoolean()
        {
            var output = RockFilters.AsInteger( true );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidInteger()
        {
            var output = RockFilters.AsInteger( 3 );
            Assert.Equal( output, 3 );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidDecimal()
        {
            var output = RockFilters.AsInteger( ( decimal ) 3.0d );
            Assert.Equal( output, 3 );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_InvalidDecimal()
        {
            var output = RockFilters.AsInteger( ( decimal ) 3.2d );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidDouble()
        {
            var output = RockFilters.AsInteger( 3.0d );
            Assert.Equal( output, 3 );
        }

        /// <summary>
        /// For use in Lava -- should not cast the double to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_InvalidDouble()
        {
            var output = RockFilters.AsInteger( 3.2d );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidString()
        {
            var output = RockFilters.AsInteger( "3" );
            Assert.Equal( output, 3 );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_InvalidString()
        {
            var output = RockFilters.AsInteger( "a" );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_InvalidDecimalString()
        {
            var output = RockFilters.AsInteger( "3.0" );
            Assert.Null( output );
        }

        #endregion

        #region AsDecimal

        /// <summary>
        /// For use in Lava -- should not cast the null to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_Null()
        {
            var output = RockFilters.AsDecimal( null );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_InvalidBoolean()
        {
            var output = RockFilters.AsDecimal( true );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_ValidInteger()
        {
            var output = RockFilters.AsDecimal( 3 );
            Assert.Equal( output, ( decimal ) 3.0d );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_ValidDecimal()
        {
            var output = RockFilters.AsDecimal( ( decimal ) 3.2d );
            Assert.Equal( output, ( decimal ) 3.2d );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_ValidDouble()
        {
            var output = RockFilters.AsDecimal( 3.141592d );
            Assert.Equal( output, ( decimal ) 3.141592d );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_ValidString()
        {
            var output = RockFilters.AsDecimal( "3.14" );
            Assert.Equal( output, ( decimal ) 3.14d );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_InvalidString()
        {
            var output = RockFilters.AsDecimal( "a" );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to a decimal.
        /// </summary>
        [Fact]
        public void AsDecimal_InvalidDecimalString()
        {
            var output = RockFilters.AsInteger( "3.0.2" );
            Assert.Null( output );
        }

        #endregion

        #region AsDouble

        /// <summary>
        /// For use in Lava -- should not cast the null to a double.
        /// </summary>
        [Fact]
        public void AsDouble_Null()
        {
            var output = RockFilters.AsDouble( null );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the true boolean to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidBoolean()
        {
            var output = RockFilters.AsDouble( true );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidInteger()
        {
            var output = RockFilters.AsDouble( 3 );
            Assert.Equal( output, 3.0d );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDecimal()
        {
            var output = RockFilters.AsDouble( ( decimal ) 3.2d );
            Assert.Equal( output, ( double ) 3.2d );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDouble()
        {
            var output = RockFilters.AsDouble( 3.141592d );
            Assert.Equal( output, ( double ) 3.141592d );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidString()
        {
            var output = RockFilters.AsDouble( "3.14" );
            Assert.Equal( output, ( double ) 3.14d );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidString()
        {
            var output = RockFilters.AsDouble( "a" );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the decimal string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_InvalidDecimalString()
        {
            var output = RockFilters.AsDouble( "3.0.2" );
            Assert.Null( output );
        }

        #endregion

        #region AsString

        /// <summary>
        /// For use in Lava -- should not cast the null to a string.
        /// </summary>
        [Fact]
        public void AsString_Null()
        {
            var output = RockFilters.AsString( null );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the false boolean to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidFalseBoolean()
        {
            var output = RockFilters.AsString( false );
            Assert.Equal( output, "False" );
        }

        /// <summary>
        /// For use in Lava -- should cast the true boolean to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidTrueBoolean()
        {
            var output = RockFilters.AsString( true );
            Assert.Equal( output, "True" );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidInteger()
        {
            var output = RockFilters.AsString( 3 );
            Assert.Equal( output, "3" );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDecimal()
        {
            var output = RockFilters.AsString( ( decimal ) 3.2d );
            Assert.Equal( output, "3.2" );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDouble()
        {
            var output = RockFilters.AsString( 3.141592d );
            Assert.Equal( output, "3.141592" );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDoubleString()
        {
            var output = RockFilters.AsString( "3.14" );
            Assert.Equal( output, "3.14" );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidString()
        {
            var output = RockFilters.AsString( "abc" );
            Assert.Equal( output, "abc" );
        }

        /// <summary>
        /// For use in Lava -- should not cast the datetime to a string.
        /// </summary>
        [Fact]
        public void AsString_DateTime()
        {
            DateTime dt = new DateTime( 2017, 3, 7, 15, 4, 33 );
            var output = RockFilters.AsString( dt );
            Assert.Equal( output, dt.ToString() );
        }

        #endregion

        #region AsDateTime

        /// <summary>
        /// For use in Lava -- should not cast the null to an datetime.
        /// </summary>
        [Fact]
        public void AsDateTime_Null()
        {
            var output = RockFilters.AsDateTime( null );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to an datetime.
        /// </summary>
        [Fact]
        public void AsDateTime_InvalidString()
        {
            var output = RockFilters.AsDateTime( "1/1/1 50:00" );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to an datetime.
        /// </summary>
        [Fact]
        public void AsDateTime_ValidString()
        {
            DateTime dt = new DateTime( 2017, 3, 7, 15, 4, 33 );
            var output = RockFilters.AsDateTime( dt.ToString() );
            Assert.Equal( output, dt );
        }

        #endregion

        #region Index

        /// <summary>
        /// For use in Lava -- should extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, 1 );
            Assert.Equal( "value2", output );
        }

        /// <summary>
        /// For use in Lava -- should extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndString()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, "1" );
            Assert.Equal( "value2", output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndInvalidString()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, "a" );
            Assert.Equal( null, output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndNegativeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, -1 );
            Assert.Equal( null, output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndHugeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, int.MaxValue );
            Assert.Equal( null, output );
        }

        #endregion

        #region DatesFromICal

        /// <summary>
        /// For use in Lava -- should return next occurrence for Rock's standard Saturday 4:30PM service datetime.
        /// </summary>
        [Fact( Skip = "Not including the right timestamp" )]
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
        [Fact( Skip = "Not including the right timestamp" )]
        public void DatesFromICal_NextYearSaturday()
        {
            // Next year's Saturday (from right now)
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime nextSaturday = today.AddDays( daysUntilSaturday );
            DateTime nextYearSaturday = nextSaturday.AddDays( 7 * 52 );

            DateTime expected = DateTime.Parse( nextYearSaturday.ToShortDateString() + " 4:30:00 PM" );

            var output = RockFilters.DatesFromICal( iCalStringSaturday430, 53 ).LastOrDefault();
            Assert.Equal( expected, output );
        }

        /// <summary>
        /// For use in Lava -- should return the end datetime for the next occurrence for Rock's standard Saturday 4:30PM service datetime (which ends at 5:30PM).
        /// </summary>
        [Fact( Skip = "Not including the right timestamp" )]
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
        [Fact( Skip = "Not including the right timestamp" )]
        public void DatesFromICal_NextYearsEndOccurrenceSaturday()
        {
            // Next year's Saturday (from right now)
            DateTime today = RockDateTime.Today;
            int daysUntilSaturday = ( ( int ) DayOfWeek.Saturday - ( int ) today.DayOfWeek + 7 ) % 7;
            DateTime firstSaturdayThisMonth = today.AddDays( daysUntilSaturday - ( ( today.Day / 7 ) * 7 ) );
            DateTime nextYearSaturday = firstSaturdayThisMonth.AddDays( 7 * 52 );

            DateTime expected = DateTime.Parse( nextYearSaturday.ToShortDateString() + " 10:00:00 AM" );

            var output = RockFilters.DatesFromICal( iCalStringFirstSaturdayOfMonth, 13, "enddatetime" ).LastOrDefault();
            Assert.Equal( expected, output );
        }

        #endregion

        #region Url

        private string _urlValidHttps = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";
        private string _urlValidHttpsPort = "https://www.rockrms.com:443/WorkflowEntry/35?PersonId=2";
        private string _urlValidHttpNonStdPort = "http://www.rockrms.com:8000/WorkflowEntry/35?PersonId=2";
        private string _urlInvalid = "thequickbrownfoxjumpsoverthelazydog";

        /// <summary>
        /// Should extract the host name from the URL.
        /// </summary>
        [Fact]
        public void Url_Host()
        {
            var output = RockFilters.Url( _urlValidHttps, "host" );
            Assert.Equal( output, "www.rockrms.com" );
        }

        /// <summary>
        /// Should extract the port number as an integer from the URL.
        /// </summary>
        [Fact]
        public void Url_Port()
        {
            var output = RockFilters.Url( _urlValidHttps, "port" );
            Assert.Equal( output, 443 );
        }

        /// <summary>
        /// Should extract all the segments from the URL.
        /// </summary>
        [Fact]
        public void Url_Segments()
        {
            var output = RockFilters.Url( _urlValidHttps, "segments" ) as string[];
            Assert.NotNull( output );
            Assert.Equal( output.Length, 3 );
            Assert.Equal( output[0], "/" );
            Assert.Equal( output[1], "WorkflowEntry/" );
            Assert.Equal( output[2], "35" );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [Fact]
        public void Url_Scheme()
        {
            var output = RockFilters.Url( _urlValidHttps, "scheme" );
            Assert.Equal( output, "https" );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [Fact]
        public void Url_Protocol()
        {
            var output = RockFilters.Url( _urlValidHttps, "protocol" );
            Assert.Equal( output, "https" );
        }

        /// <summary>
        /// Should extract the request path from the URL.
        /// </summary>
        [Fact]
        public void Url_LocalPath()
        {
            var output = RockFilters.Url( _urlValidHttps, "localpath" );
            Assert.Equal( output, "/WorkflowEntry/35" );
        }

        /// <summary>
        /// Should extract the request path and the query string from the URL.
        /// </summary>
        [Fact]
        public void Url_PathAndQuery()
        {
            var output = RockFilters.Url( _urlValidHttps, "pathandquery" );
            Assert.Equal( output, "/WorkflowEntry/35?PersonId=2" );
        }

        /// <summary>
        /// Should extract a single query parameter from the URL.
        /// </summary>
        [Fact]
        public void Url_QueryParameter()
        {
            var output = RockFilters.Url( _urlValidHttps, "queryparameter", "PersonId" );
            Assert.Equal( output, "2" );
        }

        /// <summary>
        /// Should extract the full URL from the URL.
        /// </summary>
        [Fact]
        public void Url_Url()
        {
            var output = RockFilters.Url( _urlValidHttps, "url" );
            Assert.Equal( output, _urlValidHttps );
        }

        /// <summary>
        /// Should extract the full URL, trimming standard port numbers, from the URL.
        /// </summary>
        [Fact]
        public void Url_UrlStdPort()
        {
            var output = RockFilters.Url( _urlValidHttpsPort, "url" );
            Assert.Equal( output, _urlValidHttpsPort.Replace( ":443", string.Empty ) );
        }

        /// <summary>
        /// Should extract the full URL, including the non-standard port number, from the URL.
        /// </summary>
        [Fact]
        public void Url_UrlNonStdPort()
        {
            var output = RockFilters.Url( _urlValidHttpNonStdPort, "url" );
            Assert.Equal( output, _urlValidHttpNonStdPort );
        }

        /// <summary>
        /// Should fail to extract the host from an invalid URL.
        /// </summary>
        [Fact]
        public void Url_InvalidUrl()
        {
            var output = RockFilters.Url( _urlInvalid, "host" );
            Assert.Equal( output, string.Empty );
        }

        #endregion
    }
}
