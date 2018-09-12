using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Rock.Lava;
using Subtext.TestLibrary;
using Xunit;

namespace Rock.Tests.Rock.Lava
{
    public class RockFiltersTest
    {
        // A fake webroot Content folder for any tests that use the HTTP Context simulator
        private static string webContentFolder = string.Empty;

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

        #region Numeric Filters

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

        #endregion

        #region "Other" category filters

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
            Assert.Equal( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidDecimal()
        {
            var output = RockFilters.AsInteger( ( decimal ) 3.0d );
            Assert.Equal( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidDouble()
        {
            var output = RockFilters.AsInteger( 3.0d );
            Assert.Equal( 3, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to an integer.
        /// </summary>
        [Fact]
        public void AsInteger_ValidString()
        {
            var output = RockFilters.AsInteger( "3" );
            Assert.Equal( 3, output );
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
        public void AsInteger_ValidDecimalString()
        {
            var output = RockFilters.AsInteger( "3.2" );
            Assert.Equal( 3, output );
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
            Assert.Equal( 3.0d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDecimal()
        {
            var output = RockFilters.AsDouble( ( decimal ) 3.2d );
            Assert.Equal( ( double ) 3.2d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidDouble()
        {
            var output = RockFilters.AsDouble( 3.141592d );
            Assert.Equal( ( double ) 3.141592d, output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a double.
        /// </summary>
        [Fact]
        public void AsDouble_ValidString()
        {
            var output = RockFilters.AsDouble( "3.14" );
            Assert.Equal( ( double ) 3.14d, output );
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
            Assert.Equal( "False", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the true boolean to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidTrueBoolean()
        {
            var output = RockFilters.AsString( true );
            Assert.Equal(  "True", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the integer to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidInteger()
        {
            var output = RockFilters.AsString( 3 );
            Assert.Equal( "3", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the decimal to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDecimal()
        {
            var output = RockFilters.AsString( ( decimal ) 3.2d );
            Assert.Equal( "3.2", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the double to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDouble()
        {
            var output = RockFilters.AsString( 3.141592d );
            Assert.Equal( "3.141592", output );
        }

        /// <summary>
        /// For use in Lava -- should cast the string to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidDoubleString()
        {
            var output = RockFilters.AsString( "3.14" );
            Assert.Equal( "3.14", output );
        }

        /// <summary>
        /// For use in Lava -- should not cast the string to a string.
        /// </summary>
        [Fact]
        public void AsString_ValidString()
        {
            var output = RockFilters.AsString( "abc" );
            Assert.Equal( "abc", output );
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
            Assert.Equal( dt, output );
        }

        #endregion

        /// <summary>
        /// For use in Lava -- should return the IP address of the Client
        /// </summary>
        [Fact]
        public void Client_IP()
        {
            InitWebContentFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var output = RockFilters.Client( "Global", "ip" );
                Assert.Equal( "127.0.0.1", output );
            }
        }

        /// <summary>
        /// For use in Lava -- should return the IP address of the Client using the HTTP_X_FORWARDED_FOR header value
        /// </summary>
        [Fact( Skip = "Need to figure out how to properly set a Server Variable with this HttpSimulator" )]
        public void Client_IP_ForwardedFor()
        {
            InitWebContentFolder();

            NameValueCollection headers = new NameValueCollection();
            headers.Add( "HTTP_X_FORWARDED_FOR", "77.7.7.77" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest( new Uri( "http://localhost/" ), new NameValueCollection(), headers ) )
            {
                var output = RockFilters.Client( "Global", "ip" );
                Assert.Equal( "77.7.7.77", output );
            }
        }

        /// <summary>
        /// For use in Lava -- should return the user agent of the client (which is setup in the fake/mock HttpSimulator)
        /// </summary>
        [Fact]
        public void Client_Browser()
        {
            InitWebContentFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                dynamic output = RockFilters.Client( "Global", "browser" );
                Assert.Equal( "Chrome", output.UserAgent.Family );
                Assert.Equal( "68", output.UserAgent.Major );
                Assert.Equal( "Windows 10", output.OS.Family );
            }
        }

        #endregion

        #region Array filters

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
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndNegativeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, -1 );
            Assert.Null( output );
        }

        /// <summary>
        /// For use in Lava -- should fail to extract a single element from the array.
        /// </summary>
        [Fact]
        public void Index_ArrayAndHugeInt()
        {
            var output = RockFilters.Index( new string[] { "value1", "value2", "value3" }, int.MaxValue );
            Assert.Null( output );
        }

        #endregion

        #endregion

        #region Date Filters

        /// <summary>
        /// For use in Lava -- adding default days to 'Now' should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddDaysDefaultViaNow()
        {
            var output = RockFilters.DateAdd( "Now", 5 );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.AddDays( 5 ) );
        }

        /// <summary>
        /// For use in Lava -- adding days (default) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddDaysDefaultToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018", 3 );
            DateTimeAssert.AreEqual( output, DateTime.Parse("5/4/2018") );
        }

        /// <summary>
        /// For use in Lava -- adding days (d param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddDaysIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018", 3, "d" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/4/2018" ) );
        }

        /// <summary>
        /// For use in Lava -- adding hours (h param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddHoursIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 1, "h" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2018 4:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding minutes (m param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddMinutesIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 120, "m" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2018 5:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding seconds (s param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddSecondsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 300, "s" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2018 3:05 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding years (y param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddYearsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 2, "y" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2020 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding years (y param) to a given leap-year date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddYearsIntervalToGivenLeapDate()
        {
            var output = RockFilters.DateAdd( "2/29/2016 3:00 PM", 1, "y" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "2/28/2017 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding months (M param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddMonthsIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 1, "M" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "6/1/2018 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding months (M param) to a given date with more days in the month
        /// should be equal to the month's last day.
        /// </summary>
        [Fact]
        public void DateAdd_AddMonthsIntervalToGivenLongerMonthDate()
        {
            var output = RockFilters.DateAdd( "5/31/2018 3:00 PM", 1, "M" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "6/30/2018 3:00 PM" ) );
        }

        /// <summary>
        /// For use in Lava -- adding weeks (w param) to a given date should be equal.
        /// </summary>
        [Fact]
        public void DateAdd_AddWeeksIntervalToGivenDate()
        {
            var output = RockFilters.DateAdd( "5/1/2018 3:00 PM", 2, "w" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/15/2018 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week using the simplest format.
        /// </summary>
        [Fact]
        public void NextDayOfTheWeek_NextWeekdate()
        {
            var output = RockFilters.NextDayOfTheWeek( "5/1/2018 3:00 PM", "Tuesday" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/8/2018 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week including the current day.
        /// </summary>
        [Fact]
        public void NextDayOfTheWeek_NextWeekdateIncludeCurrentDay()
        {
            var output = RockFilters.NextDayOfTheWeek( "5/1/2018 3:00 PM", "Tuesday", true );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2018 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week in two weeks.
        /// </summary>
        [Fact]
        public void NextDayOfTheWeek_NextWeekdateTwoWeeks()
        {
            var output = RockFilters.NextDayOfTheWeek( "5/1/2018 3:00 PM", "Tuesday", false, 2 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/15/2018 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the next day of the week in minus one week.
        /// </summary>
        [Fact]
        public void NextDayOfTheWeek_NextWeekdateBackOneWeek()
        {
            var output = RockFilters.NextDayOfTheWeek( "5/1/2018 3:00 PM", "Tuesday", false, -1 );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "4/24/2018 3:00 PM" ) );
        }

        /// <summary>
        /// Tests the To Midnight using a string.
        /// </summary>
        [Fact]
        public void ToMidnight_TextString()
        {
            var output = RockFilters.ToMidnight( "5/1/2018 3:00 PM" );
            DateTimeAssert.AreEqual( output, DateTime.Parse( "5/1/2018 12:00 AM" ) );
        }

        /// <summary>
        /// Tests the To Midnight using a string of "Now".
        /// </summary>
        [Fact]
        public void ToMidnight_Now()
        {
            var output = RockFilters.ToMidnight( "Now" );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.Date );
        }

        /// <summary>
        /// Tests the To Midnight using a datetime.
        /// </summary>
        [Fact]
        public void ToMidnight_DateTime()
        {
            var output = RockFilters.ToMidnight( RockDateTime.Now );
            DateTimeAssert.AreEqual( output, RockDateTime.Now.Date );
        }

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
            Assert.Equal( "www.rockrms.com", output );
        }

        /// <summary>
        /// Should extract the port number as an integer from the URL.
        /// </summary>
        [Fact]
        public void Url_Port()
        {
            var output = RockFilters.Url( _urlValidHttps, "port" );
            Assert.Equal( 443, output );
        }

        /// <summary>
        /// Should extract all the segments from the URL.
        /// </summary>
        [Fact]
        public void Url_Segments()
        {
            var output = RockFilters.Url( _urlValidHttps, "segments" ) as string[];
            Assert.NotNull( output );
            Assert.Equal( 3, output.Length );
            Assert.Equal( "/", output[0] );
            Assert.Equal( "WorkflowEntry/", output[1] );
            Assert.Equal( "35", output[2] );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [Fact]
        public void Url_Scheme()
        {
            var output = RockFilters.Url( _urlValidHttps, "scheme" );
            Assert.Equal( "https", output );
        }

        /// <summary>
        /// Should extract the protocol/scheme from the URL.
        /// </summary>
        [Fact]
        public void Url_Protocol()
        {
            var output = RockFilters.Url( _urlValidHttps, "protocol" );
            Assert.Equal( "https", output );
        }

        /// <summary>
        /// Should extract the request path from the URL.
        /// </summary>
        [Fact]
        public void Url_LocalPath()
        {
            var output = RockFilters.Url( _urlValidHttps, "localpath" );
            Assert.Equal( "/WorkflowEntry/35", output );
        }

        /// <summary>
        /// Should extract the request path and the query string from the URL.
        /// </summary>
        [Fact]
        public void Url_PathAndQuery()
        {
            var output = RockFilters.Url( _urlValidHttps, "pathandquery" );
            Assert.Equal( "/WorkflowEntry/35?PersonId=2", output );
        }

        /// <summary>
        /// Should extract a single query parameter from the URL.
        /// </summary>
        [Fact]
        public void Url_QueryParameter()
        {
            var output = RockFilters.Url( _urlValidHttps, "queryparameter", "PersonId" );
            Assert.Equal( "2", output );
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

        #region Helper methods to build web content folder for HttpSimulator

        /// <summary>
        /// Initializes the web content folder.
        /// </summary>
        private void InitWebContentFolder()
        {
            var codeBaseUrl = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase );
            var codeBasePath = Uri.UnescapeDataString( codeBaseUrl.AbsolutePath );
            var dirPath = System.IO.Path.GetDirectoryName( codeBasePath );
            webContentFolder = System.IO.Path.Combine( dirPath, "Content" );
        }

        #endregion
    }
    #region Helper class to deal with comparing inexact dates (that are otherwise equal).

    public static class DateTimeAssert
    {
        /// <summary>
        /// Asserts that the two dates are equal within a timespan of 500 milliseconds.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate )
        {
            AreEqual( expectedDate, actualDate, TimeSpan.FromMilliseconds( 500 ) );
        }

        /// <summary>
        /// Asserts that the two dates are equal within what ever timespan you deem is sufficient.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <param name="maximumDelta">The maximum delta.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta )
        {
            if ( expectedDate == null && actualDate == null )
            {
                return;
            }
            else if ( expectedDate == null )
            {
                throw new NullReferenceException( "The expected date was null" );
            }
            else if ( actualDate == null )
            {
                throw new NullReferenceException( "The actual date was null" );
            }

            double totalSecondsDifference = Math.Abs( ( ( DateTime ) actualDate - ( DateTime ) expectedDate ).TotalSeconds );

            if ( totalSecondsDifference > maximumDelta.TotalSeconds )
            {
                //throw new Xunit.Sdk.EqualException( string.Format( "{0}", expectedDate ), string.Format( "{0}", actualDate ) );
                throw new Exception( string.Format( "\nExpected Date: {0}\nActual Date: {1}\nExpected Delta: {2}\nActual Delta in seconds: {3}",
                                                expectedDate,
                                                actualDate,
                                                maximumDelta,
                                                totalSecondsDifference ) );
            }
        }
    }
    #endregion
}
