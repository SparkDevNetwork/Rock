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
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class DateFilterTests : LavaUnitTestBase
    {
        /*
         * Lava Date filters operate on the assumption that local DateTime values are expressed in the currently configured Rock Organization timezone.
         * This may not be the same as the local server timezone, so it is important to express DateTime values in UTC wherever possible,
         * or use DateTimeOffset values that can specify a timezone offset.
         */
        // Defines the UTC offset for the comparison dates used in unit tests in this class.
        // Modifying this value simulates executing the test set for a different timezone, but should have no impact on the result.
        private static TimeSpan _utcBaseOffset = new TimeSpan( 4, 0, 0 );

        private static CalendarSerializer _serializer = new CalendarSerializer();

        private static RecurrencePattern _weeklyRecurrence = new RecurrencePattern( "RRULE:FREQ=WEEKLY;BYDAY=SA" );
        private static RecurrencePattern _monthlyRecurrence = new RecurrencePattern( "RRULE:FREQ=MONTHLY;BYDAY=1SA" );

        private static DateTime _now;

        [TestInitialize()]
        public void SetDefaultTestTimezone()
        {
            // Prior to each test, ensure that the Lava Engine is synchronized with the test timezone.
            LavaTestHelper.SetRockDateTimeToUtcPositiveTimezone();

            // Initialize the test calendar data.
            _now = RockDateTime.Now;
        }

        private static string GetCalendarWeeklySaturday1630FromDate( DateTime startDate, TimeZoneInfo tz )
        {
            var today = startDate.Date;
            var nextSaturday = today.GetNextWeekday( DayOfWeek.Saturday );

            var weeklySaturday430 = new Calendar()
            {
                Events =
                {
                    new CalendarEvent
                    {
                        DtStart = new CalDateTime( nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 16, 30, 0, tz.Id ),
                        DtEnd = new CalDateTime( nextSaturday.Year, nextSaturday.Month, nextSaturday.Day, 17, 30, 0 , tz.Id ),
                        DtStamp = new CalDateTime( today.Year, today.Month, today.Day, tz.Id ),
                        RecurrenceRules = new List<RecurrencePattern> { _weeklyRecurrence },
                        Sequence = 0,
                        Uid = @"d74561ac-c0f9-4dce-a610-c39ca14b0d6e"
                    }
                }
            };

            var iCalString = _serializer.SerializeToString( weeklySaturday430 );
            return iCalString;
        }

        private static string GetCalendarMonthlyFirstSaturday1800FromDate( DateTime startDate, TimeZoneInfo tz )
        {
            var firstSaturdayOfMonth = startDate.StartOfMonth().GetNextWeekday( DayOfWeek.Saturday );

            var monthlyFirstSaturday = new Calendar()
            {
                Events =
                {
                new CalendarEvent
                {
                    DtStart = new CalDateTime( firstSaturdayOfMonth.Year, firstSaturdayOfMonth.Month, firstSaturdayOfMonth.Day, 8, 0, 0, tz.Id ),
                    DtEnd = new CalDateTime( firstSaturdayOfMonth.Year, firstSaturdayOfMonth.Month, firstSaturdayOfMonth.Day, 10, 0, 0, tz.Id ),
                    DtStamp = new CalDateTime( firstSaturdayOfMonth.Year, firstSaturdayOfMonth.Month, firstSaturdayOfMonth.Day, tz.Id ),
                    RecurrenceRules = new List<RecurrencePattern> { _monthlyRecurrence },
                    Sequence = 0,
                    Uid = @"517d77dd-6fe8-493b-925f-f266aa2d852c"
                }
                }
            };

            var iCalString = _serializer.SerializeToString( monthlyFirstSaturday );
            return iCalString;
        }


        [ClassCleanup]
        public static void Cleanup()
        {
            // Reset the timezone to avoid problems with other tests.
            LavaTestHelper.SetRockDateTimeToLocalTimezone();
        }

        #region Filter Tests: AsDateTime

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void AsDateTime_NullInput_ReturnsEmptyString()
        {
            var template = "{{ '' | AsDateTime }}";

            TestHelper.AssertTemplateOutput( string.Empty, template );
        }

        /// <summary>
        /// The Date filter should return invalid input as literal text if it is not a recognized date.
        /// </summary>
        [TestMethod]
        public void AsDateTime_InvalidInput_ReturnsEmptyString()
        {
            var template = "{{ 'xyzzy' | AsDateTime }}";

            TestHelper.AssertTemplateOutput( string.Empty, template );
        }

        /// <summary>
        /// The default render format for a DateTime value should be the local server culture.
        /// </summary>
        [TestMethod]
        public void AsDateTime_DefaultRenderFormat_IsServerLocalCulture()
        {
            var dateTimeInput = LavaDateTime.NewUtcDateTime( 2018, 5, 1, 10, 0, 0 );

            // Convert the input time to a Rock datetime string in General format.
            var localCultureTimeString = LavaDateTime.ToString( dateTimeInput, "G" );

            // Verify that the default date output format matches the current culture General Date format.
            TestHelper.AssertTemplateOutput( localCultureTimeString, "{{ '2018-05-01 10:00:00 AM' | AsDateTime }}" );
        }

        /// <summary>
        /// Converts an input value to a date/time value.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1-May-2018 6:30 PM", "01/May/2018 06:30 PM" )]
        [DataRow( "May 1, 2018 18:30:00", "01/May/2018 06:30 PM" )]
        public void AsDateTime_UsingKnownDateFormat_ProducesValidDate( string inputString, string result )
        {
            var template = "{{ '<inputString>' | AsDateTime }}"
                .Replace( "<inputString>", inputString );

            TestHelper.AssertTemplateOutputDate( result, template );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should correctly report the offset in the output.
        /// </summary>
        [TestMethod]
        public void AsDateTime_WithDateTimeOffsetObjectAsInput_PreservesOffset()
        {
            // Get an input time of 10:00+04:00 in the test timezone.
            var dateTimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            var expectedOutput = dateTimeInput.ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInput } };

            // Verify that the Lava Date filter processes the DateTimeOffset value correctly to include the offset, and the result matches the Rock time.
            TestHelper.AssertTemplateOutput( expectedOutput, "{{ dateTimeInput | AsDateTime | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should correctly account for the offset in the output.
        /// </summary>
        [TestMethod]
        public void AsDateTime_WithDateTimeOffsetStringAsInput_PreservesOffset()
        {
            // Get an input time of 10:00+04:00 in the test timezone.
            var dateTimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            var dateTimeInputString = dateTimeInput.ToString();

            var expectedOutput = dateTimeInput.ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInputString } };

            // Verify that the Lava Date filter parses the DateTimeOffset value correctly to include the offset, and the result matches the local server time.
            TestHelper.AssertTemplateOutput( expectedOutput, "{{ dateTimeInput | AsDateTime | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        [TestMethod]
        public void AsDateTimeUtc_WithDateTimeStringAsInput_ConvertsFromRockDateTime()
        {
            LavaTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                var dateTimeInput = LavaDateTime.NewDateTimeOffset( 2018, 5, 1, 10, 0, 0 );
                var dateTimeInputString = dateTimeInput.ToString( "yyyy-MM-ddTHH:mm:ss" );
                var expectedOutput = dateTimeInput.ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:sszzz" );

                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInputString } };

                // Verify that the filter parses the DateTimeOffset value correctly to include the offset, and the result matches the UTC time.
                TestHelper.AssertTemplateOutput( expectedOutput, "{{ dateTimeInput | AsDateTimeUtc | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
            } );
        }

        [TestMethod]
        public void AsDateTimeUtc_WithSpecifiedOffsetStringAsInput_ConvertsFromOffset()
        {
            LavaTestHelper.ExecuteForTimeZones( ( tz ) =>
            {
                // Verify that an input date with an offset of UTC+04:00 is converted to the correct UTC date.
                TestHelper.AssertTemplateOutput( "2018-05-01T23:00:00+00:00",
                    "{{ '2018-05-02T03:00:00+04:00' | AsDateTimeUtc | Date:'yyyy-MM-ddTHH:mm:sszzz' }}" );

                // Verify that an input date with an offset of UTC-04:00 is converted to the correct UTC date.
                TestHelper.AssertTemplateOutput( "2018-05-02T03:00:00+00:00",
                    "{{ '2018-05-01T23:00:00-04:00' | AsDateTimeUtc | Date:'yyyy-MM-ddTHH:mm:sszzz' }}" );

            } );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should correctly report the offset in the output.
        /// </summary>
        [TestMethod]
        public void AsDateTimeUtc_WithDateTimeOffsetObjectAsInput_ConvertsToUtc()
        {
            // Add an input datetime object of 03:00+04:00 to the Lava context.
            var dateTimeInput = new DateTimeOffset( 2018, 5, 2, 3, 0, 0, new TimeSpan( 4, 0, 0 ) );
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInput } };

            // Verify that the filter translates the DateTimeOffset to the equivalent datetime with a +00:00 offset.
            TestHelper.AssertTemplateOutput( "2018-05-01T23:00:00+00:00", "{{ dateTimeInput | AsDateTimeUtc | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        #endregion

        #region Filter Tests: Date

        /*
         * The Date filter is implemented using the standard .NET Format() function, so we only need to exhaustively test the custom format parameters.
        */

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void Date_NullInput_ProducesEmptyString()
        {
            var template = "{{ '' | Date:'yyyy-MM-dd HH:mm:ss' }}";

            TestHelper.AssertTemplateOutput( string.Empty, template );
        }

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow( "d'/'MMM'/'yy", "1/May/18" )]
        [DataRow( "MMMM dd, yyyy H:mm:ss", "May 01, 2018 18:30:00" )]
        [DataRow( "yyyy-MM-dd HH:mm:ss", "2018-05-01 18:30:00" )]
        public void Date_UsingValidDotNetFormatString_ProducesValidDate( string formatString, string result )
        {
            var template = @"{% capture formatString %}<formatString>{% endcapture %}{{ '1-May-2018 6:30 PM' | Date:formatString }}"
                .Replace( "<formatString>", formatString );

            TestHelper.AssertTemplateOutput( result, template );
        }

        /// <summary>
        /// The Date filter should accept the keyword 'Now' as input and resolve it to the current date using the general datetime output format.
        /// </summary>
        [TestMethod]
        public void Date_NowWithNoFormatStringAsInput_ResolvesToCurrentDateTimeWithGeneralFormat()
        {
            // Expect the general format: short date/long time.
            TestHelper.AssertTemplateOutputDate( _now.ToString( "G" ), "{{ 'Now' | Date }}", new TimeSpan( 0, 0, 10 ) );
        }

        /// <summary>
        /// The Date filter should accept the keyword 'Now' as input and resolve it to the current date with the specified format.
        /// </summary>
        [TestMethod]
        public void Date_NowWithFormatStringAsInput_ResolvesToCurrentDateTimeWithSpecifiedFormat()
        {
            TestHelper.AssertTemplateOutputDate( _now.ToString( "yyyy-MM-dd" ), "{{ 'Now' | Date:'yyyy-MM-dd' }}" );
        }

        /// <summary>
        /// Using the parameter "sd" should return the short date format for the current culture.
        /// </summary>
        [TestMethod]
        public void Date_ShortDateParameter_ResolvesToShortDate()
        {
            var shortDate = new DateTime( 2018, 5, 1 );

            TestHelper.AssertTemplateOutput( shortDate.ToShortDateString(), "{{ '1-May-2018' | Date:'sd' }}" );
        }

        /// <summary>
        /// Using the parameter "st" should return the short time format for the current culture.
        /// </summary>
        [TestMethod]
        public void Date_ShortTimeParameter_ResolvesToShortDate()
        {
            var shortTime = LavaDateTime.NewDateTimeOffset( 2018, 5, 1, 18, 30, 0 );

            var expectedOutput = LavaDateTime.ToString( shortTime, "t" );

            TestHelper.AssertTemplateOutput( expectedOutput, "{{ '1-May-2018 6:30 PM' | Date:'st' }}" );
        }

        /// <summary>
        /// Date filter with timezone format string should return a timezone offset component.
        /// </summary>
        [TestMethod]
        public void Date_FormatStringWithTimezone_ResolvesToDateWithTimezone()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var datetimeInput = LavaDateTime.NewDateTimeOffset( 2018, 5, 1, 10, 0, 0 );

            // Convert the input time to Rock time, which has a different UTC offset.
            var rockTimeString = LavaDateTime.ConvertToRockOffset( datetimeInput ).ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( rockTimeString, "{{ dateTimeInput | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        /// <summary>
        /// Date filter executed on a Rock instance with an alternate Rock time zone should return the correct Rock time and timezone offset.
        /// </summary>
        [TestMethod]
        public void Date_NowInputWithAlternateRockTimeZone_ResolvesToCorrectRockTime()
        {
            var template = @"{{ 'Now' | Date:'yyyy-MM-ddTHH:mm:sszzz' }}";

            // Set the Rock server timezone to UTC-07:00.
            var tzMst = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

            Assert.That.IsNotNull( tzMst, "Timezone is not available in this environment." );

            var tzDefault = RockDateTime.OrgTimeZoneInfo;

            try
            {
                RockDateTime.Initialize( tzMst );

                // Get the Rock server local time, including the correct offset from UTC.
                var expectedOutput = new DateTimeOffset( RockDateTime.Now, RockDateTime.OrgTimeZoneInfo.BaseUtcOffset ).ToString( "yyyy-MM-ddTHH:mm:sszzz" );

                TestHelper.AssertTemplateOutputDate( expectedOutput, template, new TimeSpan( 0, 0, 10 ) );
            }
            finally
            {
                RockDateTime.Initialize( tzDefault );
            }
        }

        /// <summary>
        /// Using the Date filter to format a date string that contains timezone
        /// offset should preserve the original time zone. This allows the user
        /// to construct and display dates in any time zone they wish, such as a
        /// campus time zone.
        /// </summary>
        [TestMethod]
        public void Date_WithDateTimeOffsetStringAsInput_PreservesTimeZone()
        {
            // Get an input time of 2018-05-01 at 10am +04:00 offset.
            var textInput = "2018-05-01T10:00:00+04:00";

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "textInput", textInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset, including the correct offset.
            TestHelper.AssertTemplateOutput( textInput, "{{ textInput | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should preserve
        /// the original time zone. This allows the user to construct and display
        /// dates in any time zone they wish, such as a campus time zone.
        /// </summary>
        [TestMethod]
        public void Date_WithDateTimeOffsetAsInput_PreservesTimeZone()
        {
            // Get an input time of 10:00 +04:00 in the test timezone.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, new TimeSpan( 4, 0, 0 ) );

            // Convert to a string that includes time zone so we can validate the offset.
            var rockTimeString = datetimeInput.ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset value as a Rock time, including the correct offset.
            TestHelper.AssertTemplateOutput( rockTimeString, "{{ dateTimeInput | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        [TestMethod]
        public void Date_WithDateTimeUnspecifiedKindAsInput_IsProcessedAsRockTime()
        {
            // Get an input time of 10:00am in the Rock timezone.
            var dtoInput = LavaDateTime.NewDateTimeOffset( 2020, 3, 30, 10, 0, 0 );

            // Store the value as a DateTime, with Kind=Unspecified.
            // The value should be interpreted as a Rock datetime by the Lava framework.
            var datetimeInput = dtoInput.DateTime;

            // Convert to a string that includes the Rock timezone so we can validate the offset.
            var rockTimeString = dtoInput.ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTime object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            var parameters = new LavaRenderParameters
            {
                Context = LavaRenderContext.FromMergeValues( mergeValues ),
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            // Verify that the Lava Date filter formats the DateTime value as a Rock time, including the correct offset.
            TestHelper.AssertTemplateOutput( rockTimeString, "{{ dateTimeInput | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", parameters );
        }

        [TestMethod]
        public void Date_WithDateTimeLocalKindAsInput_IsProcessedAsRockTime()
        {
            LavaTestHelper.SetRockDateTimeToUtcPositiveTimezone();

            // Get a time of 10:00am in the Rock timezone.
            var dtoInput = LavaDateTime.NewDateTimeOffset( 2020, 3, 30, 10, 0, 0 );

            // Get an input time of 10:00am in the local timezone.
            // The value should be interpreted as a Rock datetime by the Lava framework.
            var datetimeInput = new DateTime( 2020, 3, 30, 10, 0, 0, DateTimeKind.Local );

            // Convert to a string that includes the Rock timezone so we can validate the offset.
            var rockTimeString = dtoInput.ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTime object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            var parameters = new LavaRenderParameters
            {
                Context = LavaRenderContext.FromMergeValues( mergeValues ),
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            // Verify that the Lava Date filter formats the DateTime value as a Rock time, including the correct offset.
            TestHelper.AssertTemplateOutput( rockTimeString, "{{ dateTimeInput | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", parameters );
        }
        /// <summary>  
        /// Create a DateTime for a specific timezone.
        /// </summary>
        public struct DateTimeWithZone
        {
            private readonly DateTime utcDateTime;
            private readonly TimeZoneInfo timeZone;

            public DateTimeWithZone( DateTime dateTime, TimeZoneInfo timeZone )
            {
                var dateTimeUnspec = DateTime.SpecifyKind( dateTime, DateTimeKind.Unspecified );
                utcDateTime = TimeZoneInfo.ConvertTimeToUtc( dateTimeUnspec, timeZone );
                this.timeZone = timeZone;
            }

            public DateTime UniversalTime { get { return utcDateTime; } }

            public TimeZoneInfo TimeZone { get { return timeZone; } }

            public DateTime TimeInOriginalZone { get { return TimeZoneInfo.ConvertTime( utcDateTime, timeZone ); } }
            public DateTime TimeInLocalZone { get { return TimeZoneInfo.ConvertTime( utcDateTime, TimeZoneInfo.Local ); } }
            public DateTime TimeInSpecificZone( TimeZoneInfo tz )
            {
                return TimeZoneInfo.ConvertTime( utcDateTime, tz );
            }
        }

        #endregion

        #region Filter Tests: DateAdd

        /// <summary>
        /// Using the keyword 'Now' as input to the DateAdd filter should resolve to the current date.
        /// </summary>
        [TestMethod]
        public void DateAdd_WithNowAsInput_ResolvesToCurrentDate()
        {
            var expectedOutputDate = LavaDateTime.NowOffset.AddDays( 5 );

            TestHelper.AssertTemplateOutputDate( expectedOutputDate, "{{ 'Now' | DateAdd:5,'d' }}", TimeSpan.FromSeconds( 300 ) );
        }

        /// <summary>
        /// Adding an integer without specifying a unit should add a number of days.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddDefaultIncrement_AddsDays()
        {
            TestHelper.AssertTemplateOutputDate( "4-May-2018", "{{ '1-May-2018' | DateAdd:'3' }}" );
        }

        /// <summary>
        /// Verify adding a number of days to the target date using the "d" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddDaysIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "4-Jan-2018", "{{ '1-Jan-2018' | DateAdd:'3','d' }}" );
        }

        /// <summary>
        /// Verify adding a number of hours to the target date using the "h" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddHoursIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2018 4:00 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'1','h' }}" );
        }

        /// <summary>
        /// Verify adding a number of minutes to the target date using the "m" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMinutesIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2018 5:00 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'120','m' }}" );
        }

        /// <summary>
        /// Verify adding a number of seconds to the target date using the "s" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddSecondsIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2018 3:05 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'300','s' }}" );
        }

        /// <summary>
        /// Verify adding a number of years to the target date using the "y" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddYearsIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2020 3:00 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'2','y' }}" );
        }

        /// <summary>
        /// Adding a year to a leap-day target date should result in a valid date in the following year.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddYearsIntervalToGivenLeapDate()
        {
            TestHelper.AssertTemplateOutputDate( "28-Feb-2017 3:00 PM", "{{ '29-Feb-2016 3:00 PM' | DateAdd:'1','y' }}" );
        }

        /// <summary>
        /// Verify adding a number of months to the target date using the "M" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMonthsIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-Jun-2018 3:00 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'1','M' }}" );
        }

        /// <summary>
        /// Adding a number of months to a target date that occurs on the 31st day of the month such that the result is a month with only 30 days
        /// should produce a date that is on the 30th day of the month.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddMonthsIntervalToGivenLongerMonthDate()
        {
            TestHelper.AssertTemplateOutputDate( "30-Jun-2018 3:00 PM", "{{ '31-May-2018 3:00 PM' | DateAdd:'1','M' }}" );
        }

        /// <summary>
        /// Verify adding a number of weeks to the target date using the "w" interval parameter.
        /// </summary>
        [TestMethod]
        public void DateAdd_AddWeeksIntervalToGivenDate()
        {
            TestHelper.AssertTemplateOutputDate( "15-May-2018 3:00 PM", "{{ '1-May-2018 3:00 PM' | DateAdd:'2','w' }}" );
        }

        /// <summary>
        /// Adding hours to an input date with a time zone that differs from the server time zone should return the correct result for the input time zone.
        /// </summary>
        [TestMethod]
        public void DateAdd_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00+04:00.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // Convert the input time to local server time, which may have a different UTC offset to the input date.
            var expectedDateString = datetimeInput
                .AddHours( 1 )
                .ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset value as a local server time.
            TestHelper.AssertTemplateOutput( expectedDateString, "{{ dateTimeInput | DateAdd:1,'h' | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        /// <summary>
        /// Adding hours to an input date with a time zone that differs from the
        /// server time zone should return the correct result for the input time
        /// zone.
        /// </summary>
        /// <remarks>
        /// The use case for this the user (or developer) may want to display a
        /// date and time in a specific time zone. For example, they may want to
        /// convert an event start time into the time zone of the campus that is
        /// hosting the event. Using DateAdd would let them also display an "end"
        /// date, but if the timezone information is lost then they are stuck.
        /// </remarks>
        [TestMethod]
        public void DateAdd_WithDateTimeOffsetAsInput_PreservesTimeZone()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // When a DateTimeOffset is used, the original timezone should be
            // preserved by the DateAdd filter, verify that indeed happens.
            var expectedDateString = datetimeInput
                .AddHours( 1 )
                .ToString( "yyyy-MM-ddTHH:mm:sszzz" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset value as a local server time.
            TestHelper.AssertTemplateOutput( expectedDateString, "{{ dateTimeInput | DateAdd:1,'h' | Date:'yyyy-MM-ddTHH:mm:sszzz' }}", mergeValues );
        }

        #endregion

        #region Filter Tests: DateDiff

        /// <summary>
        /// Requesting the difference between a target date and a later date should yield a positive number of days.
        /// </summary>
        [TestMethod]
        public void DateDiff_CompareLaterDateInDays_YieldsPositiveInteger()
        {
            TestHelper.AssertTemplateOutput( "32", "{{ '14-Feb-2011 8:00 AM' | DateDiff:'18-Mar-2011 11:30 AM','d' }}" );
        }

        /// <summary>
        /// Requesting the difference between a target date and an earlier date should yield a negative number of days.
        /// </summary>
        [TestMethod]
        public void DateDiff_CompareEarlierDateInDays_YieldsNegativeInteger()
        {
            TestHelper.AssertTemplateOutput( "-32", "{{ '18-Mar-2011 11:30 AM' | DateDiff:'14-Feb-2011 8:00 AM','d' }}" );
        }

        /// <summary>
        /// Requesting the difference between two dates in years should yield the result as multiples of 365.25 days.
        /// </summary>
        [TestMethod]
        public void DateDiff_CompareDifferenceInYears_ReturnsWholeYearDifferenceOnly()
        {
            TestHelper.AssertTemplateOutput( "0", "{{ '31-Dec-2020' | DateDiff:'01-Jan-2021','Y' }}" );
            TestHelper.AssertTemplateOutput( "10", "{{ '31-Dec-2010' | DateDiff:'31-Dec-2020','Y' }}" );
            TestHelper.AssertTemplateOutput( "-10", "{{ '31-Dec-2020' | DateDiff:'31-Dec-2010','Y' }}" );
        }

        /// <summary>
        /// Requesting the difference between two dates in years should yield the result as multiples of 365.25 days.
        /// </summary>
        [TestMethod]
        public void DateDiff_CompareDifferenceWithInterveningLeapYears_ReturnsCorrectYearDifference()
        {
            // A period spanning 365 days that occurs during a non-leap year should return a difference of 1.
            TestHelper.AssertTemplateOutput( "1", "{{ '2024-03-02' | DateDiff:'2025-03-02','Y' }}" );

            // A period spanning 365 days that occurs during a leap year should return a difference of 0.
            TestHelper.AssertTemplateOutput( "0", "{{ '2024-02-29' | DateDiff:'2025-02-27','Y' }}" );

            // A period spanning 366 days that occurs during a leap year should return a difference of 1.
            TestHelper.AssertTemplateOutput( "1", "{{ '2023-03-02' | DateDiff:'2024-03-02','Y' }}" );
        }

        /// <summary>
        /// Requesting the difference between a target date and a DateTimeOffset should return a result that accounts for the input time zone.
        /// </summary>
        [TestMethod]
        public void DateDiff_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00+04:00.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, new TimeSpan( 4, 0, 0 ) );
            var datetimeReference = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, new TimeSpan( 3, 0, 0 ) );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Get a string representing Rock time, which has a different UTC offset.
                var referenceDateTimeString = LavaDateTime.ToString( datetimeReference, "yyyy-MM-ddTHH:mm:sszzz" );

                // Add the input DateTimeOffset object to the Lava context.
                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput }, { "serverLocalTime", referenceDateTimeString } };

                // Verify that the difference between the input date and the adjusted local time is 1 hour.
                TestHelper.AssertTemplateOutput( engine, "1", "{{ dateTimeInput | DateDiff:serverLocalTime,'h' }}", mergeValues );
            } );
        }

        /// <summary>
        /// Requesting the difference between a target date affected by Daylight Saving Time (DST) and a DateTimeOffset should return a result that accounts for the input time zone.
        /// </summary>
        [TestMethod]
        public void DateDiff_WithDaylightSavingDateTimeObjectAsInput_AdjustsResultForDst()
        {
            LavaTestHelper.SetRockDateTimeToDaylightSavingTimezone();

            // Get a date that occurs within daylight saving time for this timezone.
            var testDate = new DateTime( 2022, 9, 1, 10, 0, 0 );
            try
            {
                Assert.IsTrue( RockDateTime.OrgTimeZoneInfo.IsDaylightSavingTime( testDate ), "Test date is not within a daylight saving period." );

                var testDateFormatted = testDate.ToString( "yyyy-MM-d hh:mm:ss" );
                var template = "{{ '<inputDate>' | DateDiff:inputDate,'s' }}"
                    .Replace( "<inputDate>", testDateFormatted );

                var lavaValues = new LavaDataDictionary() { { "inputDate", testDate } };

                TestHelper.AssertTemplateOutput( "0", template, lavaValues );
            }
            finally
            {
                LavaTestHelper.SetRockDateTimeToLocalTimezone();
            }
        }

        #endregion

        #region Filter Tests: DatesFromICal

        private void VerifyDatesExistInDatesFromICalResult( string iCalString, List<DateTime> dates, string filterOption = "all" )
        {
            var dateTimeOffsets = dates.Select( x => LavaDateTime.ConvertToDateTimeOffset( x ) ).ToList();

            VerifyDatesExistInDatesFromICalResult( iCalString, dateTimeOffsets, filterOption );
        }

        private void VerifyDatesExistInDatesFromICalResult( string iCalString, List<DateTimeOffset> dates, string filterOption = "all" )
        {
            var mergeValues = new LavaDataDictionary { { "iCalString", iCalString } };

            var template = @"
{% assign nextDates = iCalString | DatesFromICal:$filterOption %}
{% for nextDate in nextDates %}
    <li>{{ nextDate | Date:'yyyy-MM-dd HH:mm:ss tt' }}</li>
{% endfor %}
";

            template = template.Replace( "$filterOption", filterOption );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template, mergeValues );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the result contains the expected entries.
                foreach ( var date in dates )
                {
                    var rockDateTimeString = LavaDateTime.ToString( date, "yyyy-MM-dd HH:mm:ss tt" );

                    if ( !output.Contains( $"<li>{ rockDateTimeString }</li>" ) )
                    {
                        Assert.That.Fail( $"Lava Output '{ output }' does not contain date string '{ rockDateTimeString }'.\n[SystemDateTime = {DateTime.Now:O}, RockDateTime = {_now:O}]" );
                    }
                }
            } );
        }

        private void VerifyDailyScheduleNextOccurrenceFromStartDate( DateTime firstEventStartDateTime, TimeSpan eventDuration, DateTime asAtDateTime, DateTime expectedNextDateTime )
        {
            // Verify that all dates are expressed as Kind=Unspecified. This is to ensure that the caller has intentionally expressed these parameters as calendar dates,
            // rather than specific points in time. 
            Assert.That.IsTrue( asAtDateTime.Kind == DateTimeKind.Unspecified, "DateTime parameter must be of Kind 'Unspecified' because it is timezone independent." );
            Assert.That.IsTrue( expectedNextDateTime.Kind == DateTimeKind.Unspecified, "DateTime parameter must be of Kind 'Unspecified' because it is timezone independent." );

            // Create a schedule with the specified parameters and verify the DatesFromICal filter output.
            var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( firstEventStartDateTime,
                eventDuration: eventDuration );

            VerifyDatesExistInDatesFromICalResult( schedule.iCalendarContent,
                new List<DateTimeOffset> { expectedNextDateTime },
                $"1,'startdatetime','{asAtDateTime}'" );
        }

        /// <summary>
        /// A schedule that specifies an infinite recurrence pattern should return dates for GetOccurrences() only up to the requested end date.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_SingleDayEventWithInfiniteRecurrencePattern_ReturnsRequestedOccurrences()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                // Create a new schedule starting at 11am today Rock time.
                var startDateTime = LavaDateTime.NewDateTime( _now.Year, _now.Month, _now.Day, 22, 0, 0 );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDateTime,
                    eventDuration: new TimeSpan( 1, 0, 0 ) );

                VerifyDatesExistInDatesFromICalResult( schedule.iCalendarContent,
                    new List<DateTimeOffset> { startDateTime, startDateTime.AddDays( 1 ), startDateTime.AddDays( 2 ) },
                    $"3,'','{startDateTime.Date:u}'" );
            } );
        }

        [TestMethod]
        public void DatesFromICal_DailyEventWithPastOccurrenceOnSameDay_ReturnsNextDayAsFirstDate()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                var eventDuration = new TimeSpan( 1, 0, 0 );

            // Create a schedule starting on 2020-06-01 04:00am for the current Rock timezone.
            // The scheduled event has a duration of 1 hour.
            // If we retrieve the occurrences for the schedule at an effective date of 2020-06-01 07:00am,
            // the event scheduled for the current day has already passed so the first entry
            // in the sequence of future occurrences should be 2020-06-02 04:00am.
            // This should be true for any Rock timezone, regardless of UTC offset.
            var firstEventStartDate = LavaDateTime.NewDateTime( 2020, 6, 1, 4, 0, 0 );
                var asAtDate = firstEventStartDate.AddHours( 3 );
                var expectedNextDate = firstEventStartDate.AddDays( 1 );

                VerifyDailyScheduleNextOccurrenceFromStartDate( firstEventStartDate, eventDuration, asAtDate, expectedNextDate );
            } );
        }

        [TestMethod]
        public void DatesFromICal_DailyEventWithFutureOccurrenceOnSameDay_ReturnsSameDayAsFirstDate()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                var eventDuration = new TimeSpan( 1, 0, 0 );

                // Create a schedule starting on 2020-06-01 04:00am for the current Rock timezone.
                // The scheduled event has a duration of 1 hour.
                // If we retrieve the occurrences for the schedule at an effective date of 2020-06-01 02:00am,
                // the event scheduled for the current day is pending so the first entry
                // in the sequence of future occurrences should be 2020-06-01 04:00am.
                // This should be true for any Rock timezone, regardless of UTC offset.
                var firstEventStartDate = LavaDateTime.NewDateTime( 2020, 6, 1, 4, 0, 0 );
                var asAtDate = firstEventStartDate.AddHours( -2 );
                var expectedNextDate = firstEventStartDate;

                VerifyDailyScheduleNextOccurrenceFromStartDate( firstEventStartDate, eventDuration, asAtDate, expectedNextDate );
            } );
        }

        [TestMethod]
        public void DatesFromICal_DailyEventWithActiveOccurrenceOnSameDay_ReturnsSameDayAsFirstDate()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                var eventDuration = new TimeSpan( 2, 0, 0 );

                // Create a schedule starting on 2020-06-01 04:00am for the current Rock timezone.
                // The scheduled event has a duration of 2 hours.
                // If we retrieve the occurrences for the schedule at an effective date of 2020-06-01 05:00am,
                // the event scheduled for the current day is in progress so the first entry
                // in the sequence of future occurrences should be 2020-06-01 04:00am.
                // This should be true for any Rock timezone, regardless of UTC offset.
                var firstEventStartDate = LavaDateTime.NewDateTime( 2020, 6, 1, 4, 0, 0 );
                var asAtDate = firstEventStartDate.AddHours( 1 );
                var expectedNextDate = firstEventStartDate;

                VerifyDailyScheduleNextOccurrenceFromStartDate( firstEventStartDate, eventDuration, asAtDate, expectedNextDate );
            } );
        }

        /// <summary>
        /// Return the next occurrence for Rock's sample data Saturday 4:30PM service datetime.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_Saturday430ServiceScheduleNextDate_ReturnsNextSaturday()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                // Get the iCalendar and expected datetime for the test time zone.
                var testDateTime = new DateTime( 2021, 3, 15, 13, 0, 0 );
                var expectedDateTime = GetNextScheduledWeeklyEventDateTime( testDateTime, DayOfWeek.Saturday, new TimeSpan( 16, 30, 0 ) );
                var iCalString = GetCalendarWeeklySaturday1630FromDate( testDateTime, timeZone );

                VerifyDatesExistInDatesFromICalResult( iCalString,
                    new List<DateTime> { expectedDateTime },
                    $"1,'','{expectedDateTime.Date:u}'" );
            } );
        }

        /// <summary>
        /// Returns the end datetime for the next occurrence for Rock's sample Saturday 4:30PM service datetime (which ends at 5:30PM).
        /// </summary>
        [TestMethod]
        public void DatesFromICal_WithEndDateTimeParameter_ReturnsEndDateTimeOfEvent()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                // Get the iCalendar and expected datetime for the test time zone.
                var testDateTime = new DateTime( 2021, 3, 15, 13, 0, 0 );
                var expectedDateTime = GetNextScheduledWeeklyEventDateTime( testDateTime, DayOfWeek.Saturday, new TimeSpan(17,30,0) );
                var iCalString = GetCalendarWeeklySaturday1630FromDate( testDateTime, timeZone );

                VerifyDatesExistInDatesFromICalResult( iCalString,
                    new List<DateTime> { expectedDateTime },
                    $"1,'enddatetime','{expectedDateTime.Date:u}'" );
            } );
        }

        private DateTime GetNextScheduledWeeklyEventDateTime( DateTime currentDateTime, DayOfWeek scheduledDayOfWeek, TimeSpan scheduledTime )
        {
            var daysUntilTargetDay = ( ( int ) scheduledDayOfWeek - ( int ) currentDateTime.Date.DayOfWeek + 7 ) % 7;
            var nextTargetDate = currentDateTime.AddDays( daysUntilTargetDay );
            var expectedDateTime = LavaDateTime.NewDateTime( nextTargetDate.Year, nextTargetDate.Month, nextTargetDate.Day, scheduledTime.Hours, scheduledTime.Minutes, scheduledTime.Seconds );

            // If the current day is the same as the schedule day and the current time is greater than the schedule time,
            // get the date for the following weekday instead.
            if ( daysUntilTargetDay == 0 && currentDateTime.TimeOfDay > expectedDateTime.TimeOfDay )
            {
                expectedDateTime = expectedDateTime.AddDays( 7 );
            }

            return expectedDateTime;
        }

        /// <summary>
        /// Returns the end datetime occurrence for the fictitious, first Saturday of the month event for Saturday a year from today.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_Saturday430ServiceScheduleNextYearDate_ReturnsSaturdayNextYear()
        {
            LavaTestHelper.ExecuteForTimeZones( ( timeZone ) =>
            {
                // Next year's Saturday (from last month). iCal can only get 12 months of data starting from the current month.
                // So 12 months from now would be the previous month next year.
                // The event ends at 10:00am.

                // Get a Rock datetime for 10am on the first Saturday 11 months from now.
                // The GetDatesFromiCal filter can only retrieve 12 months of data, including the current month.
                var expectedDateTime = _now
                    .AddMonths( -1 )
                    .StartOfMonth()
                    .AddYears( 1 )
                    .GetNextWeekday( DayOfWeek.Saturday );

                expectedDateTime = LavaDateTime.NewDateTime( expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day, 10, 0, 0 );

                var iCalendar = GetCalendarMonthlyFirstSaturday1800FromDate( _now, timeZone );

                VerifyDatesExistInDatesFromICalResult( iCalendar,
                    new List<DateTime> { expectedDateTime },
                    "12,'enddatetime'" );
            } );
        }

        /// <summary>
        /// A schedule that specifies a recurring event across a Daylight Saving Time (DST) boundary should return times that remain unadjusted.
        /// </summary>
        [TestMethod]
        public void DatesFromICal_RecurringEventAcrossDstBoundary_HasUnchangedEventTime()
        {
            // Set the Rock server to a timezone that supports Daylight Saving Time (DST).
            // DST begins on 13/03/2022 02:00 in this timezone.
            var tzCurrent = RockDateTime.OrgTimeZoneInfo;
            var tzDst = TimeZoneInfo.FindSystemTimeZoneById( "Central Standard Time" );
            Assert.That.IsNotNull( tzDst, "Timezone is not available in this environment." );

            try
            {
                RockDateTime.Initialize( tzDst );

                // Create a schedule that spans the DST boundary date.
                var startDateTime = LavaDateTime.NewDateTime( 2022, 03, 12, 11, 0, 0 );

                var isNotDstDate = startDateTime;
                var isDstDate = startDateTime.AddDays( 1 );

                Assert.That.IsFalse( tzDst.IsDaylightSavingTime( isNotDstDate ), "Input date is adjusted for DST." );
                Assert.That.IsTrue( tzDst.IsDaylightSavingTime( isDstDate ), "Input date is not adjusted for DST." );

                var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDateTime,
                    eventDuration: new TimeSpan( 1, 0, 0 ) );

                // Get the first 2 dates of the schedule, which will span the DST boundary.
                // The time expressed in the schedule is nominal rather than absolute - it should not be adjusted for DST,
                // because all future event in the schedule should have the same start time.
                VerifyDatesExistInDatesFromICalResult( schedule.iCalendarContent,
                    new List<DateTimeOffset> { isNotDstDate, isDstDate },
                    $"2,'','{startDateTime.Date:u}'" );
            }
            finally
            {
                // Restore the default time zone.
                RockDateTime.Initialize( tzCurrent );
            }
        }

        #endregion

        #region Filter Tests: DaysFromNow

        /// <summary>
        /// Using a past date as a parameter should return "X days ago".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareEarlierDate_YieldsDaysAgo()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", _now.AddDays( -14 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "14 days ago", template );
        }

        /// <summary>
        /// Using the previous day as a parameter should return "yesterday".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_ComparePreviousDay_YieldsYesterday()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", _now.AddDays( -1 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "yesterday", template );
        }

        /// <summary>
        /// Using the currentr date as a parameter should return "today".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareCurrentDate_YieldsToday()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", _now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "today", template );
        }

        /// <summary>
        /// Using the previous day as a parameter should return "yesterday".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareNextDay_YieldsTomorrow()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", _now.AddDays( 1 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "tomorrow", template );
        }

        /// <summary>
        /// Using a future date as a parameter should return "in X days".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareFutureDate_YieldsInDays()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", _now.AddDays( 14 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "in 14 days", template );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void DaysFromNow_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get a Rock time of 1:00am tomorrow.
            var tomorrow = _now.Date.AddDays( 1 ).AddHours( 1 );
            var localOffset = RockDateTime.OrgTimeZoneInfo.BaseUtcOffset;

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine cannot process the DateTimeOffset variable type.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                // First, verify that the Lava filter returns "tomorrow" for a DateTimeOffset that resolves to Rock time 1:00am tomorrow.
                var datetimeInput = LavaDateTime.ConvertToRockOffset( tomorrow );

                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

                TestHelper.AssertTemplateOutput( engine, "tomorrow", "{{ dateTimeInput | DaysFromNow }}", mergeValues );

                // Now verify that the Lava filter returns "today" if the offset is increased by 2 hours, equating to a Rock time of 11:00pm today.
                var datetimeInput2 = new DateTimeOffset( tomorrow.Year, tomorrow.Month, tomorrow.Day, tomorrow.Hour, tomorrow.Minute, tomorrow.Second, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

                var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

                TestHelper.AssertTemplateOutput( engine, "today", "{{ dateTimeInput | DaysFromNow }}", mergeValues2 );
            } );
        }

        #endregion

        #region Filter Tests: IsDateBetween

        /// <summary>
        /// Verifies when no format string is provided, the start and end date ranges default to SOD and EOD respectively.
        /// </summary>
        [TestMethod]
        public void IsDateBetween_WithoutFormatString_AdjustsStartAndEndTimes()
        {
            var template = "{{ '2022-05-01 09:00' | IsDateBetween:'2022-05-01 12:00','2022-05-01 07:00' }}";

            TestHelper.AssertTemplateOutput( "true", template );
        }

        /// <summary>
        /// Verifies when a format string if provided, the start and end date ranges maintain their given times.
        /// </summary>
        [TestMethod]
        public void IsDateBetween_WithFormatString_DoesNotAdjustTimes()
        {
            var template = "{{ '2022-05-01 09:00' | IsDateBetween:'2022-05-01 12:00','2022-05-01 07:00','yyyy-MM-dd HH:mm' }}";

            TestHelper.AssertTemplateOutput( "false", template );
        }

        /// <summary>
        /// Verifies that filter works when DateTime or DateTimeOffset input is sent.
        /// </summary>
        [TestMethod]
        public void IsDateBetween_WithDateTimeOrDateTimeOffsetAsInput()
        {
            var template = "{{ targetDate | IsDateBetween:startDate,endDate }}";
            var mergeValues = new LavaDataDictionary() { { "targetDate", DateTime.Parse( "2022-05-02" ) }, { "startDate", DateTime.Parse( "2022-05-01" ) }, { "endDate", DateTime.Parse( "2022-05-03" ) } };
            TestHelper.AssertTemplateOutput( "true", template, mergeValues );
        }

        #endregion

        /// <summary>
        /// Input keyword 'Now' should return number of days in current month.
        /// </summary>
        [TestMethod]
        public void DaysInMonth_InputKeywordNow_YieldsDaysInCurrentMonth()
        {
            var targetDate = new DateTime( _now.Year, _now.Month, 1 ).AddMonths( 1 );

            targetDate = targetDate.AddDays( -1 );

            TestHelper.AssertTemplateOutput( targetDate.Day.ToString(), "{{ 'Now' | DaysInMonth }}" );
        }

        /// <summary>
        /// Empty input string with Month parameter should return day count for month in current year.
        /// </summary>
        [TestMethod]
        public void DaysInMonth_InputMonthParameter_YieldsDaysInSpecifiedMonth()
        {
            TestHelper.AssertTemplateOutput( "31", "{{ '' | DaysInMonth:'03' }}" );
        }

        /// <summary>
        /// Empty input string with Month and Year parameters should return correct day count.
        /// </summary>
        [TestMethod]
        public void DaysInMonth_InputMonthYearParameters_YieldsDaysInSpecifiedMonth()
        {
            TestHelper.AssertTemplateOutput( "29", "{{ '' | DaysInMonth:'02','2016' }}" );
        }

        /// <summary>
        /// Days in specified month should return correct day count.
        /// </summary>
        [TestMethod]
        public void DaysInMonth_InputDate_YieldsDaysInMonth()
        {
            TestHelper.AssertTemplateOutput( "28", "{{ '1-Feb-2017' | DaysInMonth }}" );
        }

        #region Filter Tests: DaysSince

        /// <summary>
        /// Using a past date as a parameter should return a positive number of days.
        /// </summary>
        [TestMethod]
        public void DaysSince_ComparePastDate_YieldsPositiveInteger()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", _now.AddDays( -3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "3", template );
        }

        /// <summary>
        /// Using today's date as a parameter should return a result of zero.
        /// </summary>
        [TestMethod]
        public void DaysSince_CompareCurrentDate_YieldsZero()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", _now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "0", template );
        }

        /// <summary>
        /// Using yesterday's date as a parameter should return a result of one.
        /// </summary>
        [TestMethod]
        public void DaysSince_ComparePreviousDay_YieldsOne()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", _now.Date.AddMinutes( -1 ).ToString( "dd-MMM-yyyy HH:mm:ss" ) );

            TestHelper.AssertTemplateOutput( "1", template );
        }

        /// <summary>
        /// Using a future date as a parameter should return a negative number of days.
        /// </summary>
        [TestMethod]
        public void DaysSince_CompareFutureDate_YieldsNegativeInteger()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", _now.AddDays( 3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "-3", template );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void DaysSince_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get a Rock time of 14 days prior to today at 1:00am.
            var priorRockDate = _now.Date.AddDays( -14 ).AddHours( 1 );
            var rockOffset = RockDateTime.OrgTimeZoneInfo.BaseUtcOffset;

            // First, verify that the Lava filter returns "14" when the DateTimeOffset resolves to Rock time 1:00am two weeks from today.
            var datetimeInput = LavaDateTime.ConvertToRockOffset( priorRockDate );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "14", "{{ dateTimeInput | DaysSince }}", mergeValues );

            // Now verify that the Lava filter returns "15" if the DateTimeOffset is increased by 2 hours.
            var datetimeInput2 = new DateTimeOffset( priorRockDate.Year, priorRockDate.Month, priorRockDate.Day, priorRockDate.Hour, priorRockDate.Minute, priorRockDate.Second, rockOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "15", "{{ dateTimeInput | DaysSince }}", mergeValues2 );
        }

        #endregion

        #region Filter Tests: DaysUntil

        /// <summary>
        /// Using a future date as a parameter should return a positive number of days.
        /// </summary>
        [TestMethod]
        public void DaysUntil_CompareFutureDate_YieldsPositiveInteger()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", _now.AddDays( 3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "3", template );
        }

        /// <summary>
        /// Using today's date as a parameter should return a result of zero.
        /// </summary>
        [TestMethod]
        public void DaysUntil_CompareCurrentDate_YieldsZero()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", _now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "0", template );
        }

        /// <summary>
        /// Using tomorrow's date as a parameter should return a result of one.
        /// </summary>
        [TestMethod]
        public void DaysUntil_CompareNextDay_YieldsOne()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", _now.Date.AddDays( 1 ).AddMinutes( 1 ).ToString( "dd-MMM-yyyy HH:mm:ss" ) );

            TestHelper.AssertTemplateOutput( "1", template );
        }

        /// <summary>
        /// Using a past date as a parameter should return a negative number of days.
        /// </summary>
        [TestMethod]
        public void DaysUntil_ComparePastDate_YieldsNegativeInteger()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", _now.AddDays( -3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "-3", template );
        }

        #endregion

        #region Filter Tests: HumanizeDateTime

        /// <summary>
        /// Using the previous day as a parameter should return "yesterday".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareDayEarlier_YieldsYesterday()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", _now.AddDays( -1 ).ToString( "dd-MMM-yyyy tt" ) );

            TestHelper.AssertTemplateOutput( "yesterday", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X hours earlier than the current date/time should return "X hours ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareHoursEarlier_YieldsHoursAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";

            template = template.Replace( "<compareDate>", _now.AddHours( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss tt" ) );

            TestHelper.AssertTemplateOutput( "2 hours ago", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X days earlier than the current date/time should return "X days ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareDaysEarlier_YieldsDaysAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", _now.AddDays( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss tt" ) );

            TestHelper.AssertTemplateOutput( "2 days ago", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X months earlier than the current date/time should return "X months ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareMonthsEarlier_YieldsMonthsAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", _now.AddMonths( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss tt" ) );

            TestHelper.AssertTemplateOutput( "2 months ago", template );
        }

        /// <summary>
        /// Comparing an input date to a supplied reference that is 1 day later should return "yesterday".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareDayEarlierWithReferenceDate_YieldsYesterday()
        {
            var template = "{{ '1-May-2020' | HumanizeDateTime:'2-May-2020' }}";

            TestHelper.AssertTemplateOutput( "yesterday", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is X hours earlier should return "X hours ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareHoursEarlierWithReferenceDate_YieldsHoursAgo()
        {
            var template = "{{ '1-May-2020 6:00 PM' | HumanizeDateTime:'1-May-2020 8:00 PM' }}";

            TestHelper.AssertTemplateOutput( "2 hours ago", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is X days earlier should return "X days ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareDaysEarlierWithReferenceDate_YieldsDaysAgo()
        {
            var template = "{{ '7-May-2020' | HumanizeDateTime:'10-May-2020' }}";

            TestHelper.AssertTemplateOutput( "3 days ago", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is X Months earlier should return "X Months ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareMonthsEarlierWithReferenceDate_YieldsMonthsAgo()
        {
            var template = "{{ '1-Jan-2020' | HumanizeDateTime:'10-May-2020' }}";

            TestHelper.AssertTemplateOutput( "4 months ago", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is X hours later should return "X hours from now".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareHoursLaterWithReferenceDate_YieldsHoursFromNow()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeDateTime:'1-May-2020 8:00 PM' }}";

            TestHelper.AssertTemplateOutput( "2 hours from now", template );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// This test only applies to the Fluid engine. The RockLiquid engine does not process DataTimeOffset values correctly.
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // First, verify that the Lava filter returns a predictable result for input expressed in Rock time.
            var datetimeInput = LavaDateTime.NewDateTimeOffset( 2020, 5, 15, 1, 0, 0 );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), "14 days from now", "{{ dateTimeInput | HumanizeDateTime:'1-May-2020 1:00 AM' }}", mergeValues );

            // Next, verify that the Lava filter returns the previous day if the DateTimeOffset is increased such that the datetime
            // translates to the previous day when translated to Rock time.
            var newOffset = RockDateTime.OrgTimeZoneInfo.BaseUtcOffset.Add( new TimeSpan( 2, 0, 0 ) );

            var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, newOffset );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), "13 days from now", "{{ dateTimeInput | HumanizeDateTime:'1-May-2020 1:00 AM' }}", mergeValues2 );
        }

        #endregion

        #region Filter Tests: HumanizeTimeSpan

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "X weeks".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareLaterDateWithDefaultPrecision_YieldsWeeks()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30 PM' }}";

            TestHelper.AssertTemplateOutput( "17 weeks", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "X weeks".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareWithLaterReferenceDateAndPrecision1_YieldsWeeks()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30 PM',1 }}";

            TestHelper.AssertTemplateOutput( "17 weeks", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "X weeks, Y days".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareWithLaterReferenceDateAndPrecision2_YieldsWeeksDays()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30 PM',2 }}";

            TestHelper.AssertTemplateOutput( "17 weeks, 6 days", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareWithLaterReferenceDateAndPrecision3_YieldsWeeksDaysHours()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30 PM',3 }}";

            TestHelper.AssertTemplateOutput( "17 weeks, 6 days, 1 hour", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours, M minutes".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareWithLaterReferenceDateAndPrecision4_YieldsWeeksDaysHoursMinutes()
        {
            var template = "{{ '1-May-2020 10:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30 PM',4 }}";

            TestHelper.AssertTemplateOutput( "17 weeks, 6 days, 1 hour, 30 minutes", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is the same, it should return "just now".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_CompareWithSame_YieldsJustNow()
        {
            var template = "{{ '3-Sep-2020 11:30:00 PM' | HumanizeTimeSpan:'3-Sep-2020 11:30:00 PM' }}";

            TestHelper.AssertTemplateOutput( "just now", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours, M minutes".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_NowAsInput_YieldsResult()
        {
            var template = "{{ 'Now' | HumanizeTimeSpan:'<compareDate>' }}";
            template = template.Replace( "<compareDate>", _now.AddDays( 3 ).ToString( "yyyy-MM-dd hh:mm:ss" ) );

            TestHelper.AssertTemplateOutputRegex( "[23] days", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours, M minutes".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_NowAsReferenceDate_YieldsResult()
        {
            var template = "{{ '<compareDate>' | HumanizeTimeSpan:'Now' }}";
            template = template.Replace( "<compareDate>", _now.AddDays( 3 ).ToString( "yyyy-MM-dd hh:mm:ss" ) );

            TestHelper.AssertTemplateOutputRegex( "[23] days", template );

        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( _now );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine cannot process the DateTimeOffset variable type.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
                var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

                TestHelper.AssertTemplateOutput( engine, "2 weeks", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues );

                // Next, verify that the Lava filter returns a lesser result if the DateTimeOffset is increased such that the datetime
                // crosses the boundary to the previous day when translated to local time.
                var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

                var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

                TestHelper.AssertTemplateOutput( engine, "1 week", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues2 );
            } );
        }

        #endregion

        #region Filter Tests: NextDayOfTheWeek

        /// <summary>
        /// Tests the next day of the week using the simplest format.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_NextWeekdayWithDefaultParameters_ReturnsNextWeekday()
        {
            TestHelper.AssertTemplateOutputDate( "8-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday' }}" );
        }

        /// <summary>
        /// Tests the next day of the week (literally) using the simplest format.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_FromPreviousDay_ReturnsNextDaysDate()
        {
            // 1-May-2018 is a Tuesday, so the expected result is 2-May-2018.
            TestHelper.AssertTemplateOutputDate( "2-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Wednesday' }}" );
        }

        /// <summary>
        /// Tests the next day of the week including the current day.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_IncludeCurrentDayWhereInputDateIsSameDay_ReturnsInputDate()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday','true' }}" );
        }

        /// <summary>
        /// Tests the next day of the week in two weeks.
        /// 
        ///        May 2018
        /// Su Mo Tu We Th Fr Sa
        ///        1  2  3  4  5
        ///  6  7  8  9 10 11 12
        /// 13 14 15 16 17 18 19
        /// 20 21 22 23 24 25 26
        /// 27 28 29 30 31
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_TwoWeeksHence_ReturnsFollowingWeekday()
        {
            TestHelper.AssertTemplateOutputDate( "15-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday','false','2' }}" );

            // Since Wednesday has not happened, we advance two Wednesdays -- which is Wed, 5/9
            TestHelper.AssertTemplateOutputDate( "9-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Wednesday','false','2' }}" );

            // Since Monday has passed, we advance to two week's out Monday, 5/14
            TestHelper.AssertTemplateOutputDate( "14-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Monday','false','2' }}" );
        }

        /// <summary>
        /// Tests the next day of the week with minus one week.
        /// 
        ///      April 2018
        /// Su Mo Tu We Th Fr Sa
        ///  1  2  3  4  5  6  7
        ///  8  9 10 11 12 13 14
        /// 15 16 17 18 19 20 21
        /// 22 23 24 25 26 27 28
        /// 29 30
        ///
        ///        May 2018
        /// Su Mo Tu We Th Fr Sa
        ///        1  2  3  4  5
        ///  6  7  8  9 10 11 12
        /// 13 14 15 16 17 18 19
        /// 20 21 22 23 24 25 26
        /// 27 28 29 30 31
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_OneWeekPrior_ReturnsPreviousWeekday()
        {
            // If we include the current day (so it counts as *this* current week), then one week ago would be
            // last Tuesday, April 24.
            TestHelper.AssertTemplateOutputDate( "24-Apr-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday',true,'-1' }}" );

            // Otherwise in this case, since it's Tuesday (and we're not including it as the current week), then
            // the same date is the *previous* week's Tuesday.
            TestHelper.AssertTemplateOutputDate( "1-May-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday',false,'-1' }}" );

            // Since Monday has just passed, we get this past Monday, 4/30
            TestHelper.AssertTemplateOutputDate( "30-Apr-2018 3:00 PM",
                                              "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Monday',false,'-1' }}" );
        }

        /// <summary>
        /// Tests the next day of the week with an invalid 0 numberOfWeeks parameter.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_InvalidNumberOfWeeks_ReturnsEmpty()
        {
            TestHelper.AssertTemplateOutput( "", "{{ '1-May-2018 3:00 PM' | NextDayOfTheWeek:'Tuesday',false,'0' }}" );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void NextDayOfTheWeek_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( _now );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine cannot process the DateTimeOffset variable type.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
                var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

                TestHelper.AssertTemplateOutput( engine, "2 weeks", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues );

                // Next, verify that the Lava filter returns a lesser result if the DateTimeOffset is increased such that the datetime
                // crosses the boundary to the previous day when translated to local time.
                var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

                var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

                TestHelper.AssertTemplateOutput( engine, "1 week", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues2 );
            } );
        }

        #endregion

        #region Filter Tests: TimeOfDay

        [TestMethod]
        public void TimeOfDay_InputInMorningRange_ReturnsMorning()
        {
            TestHelper.AssertTemplateOutput( "Morning", "{{ '2020-1-1 05:00:00 am' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Morning", "{{ '2020-1-1 06:30:00 am' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Morning", "{{ '2020-1-1 11:59:59 am' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputInAfternoonRange_ReturnsAfternoon()
        {
            TestHelper.AssertTemplateOutput( "Afternoon", "{{ '2020-1-1 12:00:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Afternoon", "{{ '2020-1-1 02:30:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Afternoon", "{{ '2020-1-1 04:59:59 pm' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputInEveningRange_ReturnsEvening()
        {
            TestHelper.AssertTemplateOutput( "Evening", "{{ '2020-1-1 05:00:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Evening", "{{ '2020-1-1 07:30:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Evening", "{{ '2020-1-1 08:59:59 pm' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputInNightRange_ReturnsNight()
        {
            TestHelper.AssertTemplateOutput( "Night", "{{ '2020-1-1 09:00:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Night", "{{ '2020-1-1 11:30:00 pm' | TimeOfDay }}" );
            TestHelper.AssertTemplateOutput( "Night", "{{ '2020-1-1 04:59:59 am' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputIsTimeOnly_ReturnsCorrectTimeOfDay()
        {
            TestHelper.AssertTemplateOutput( "Morning", "{{ '6:30 AM' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputIsDateOnly_ReturnsNight()
        {
            TestHelper.AssertTemplateOutput( "Night", "{{ '2020-1-1' | TimeOfDay }}" );
        }

        [TestMethod]
        public void TimeOfDay_InputCannotBeParsedToValidDateTime_ReturnsEmptyString()
        {
            TestHelper.AssertTemplateOutput( string.Empty, "{{ 'This-is-not-a-date-time-string' | TimeOfDay }}" );
        }

        #endregion

        #region Filter Tests: ToMidnight

        /// <summary>
        /// Applying the filter to a date/time input with a non-midnight time component returns the same date with a 12:00 AM time component.
        /// </summary>
        [TestMethod]
        public void ToMidnight_InputDateHasTimeComponent_YieldsMidnight()
        {
            LavaTestHelper.ExecuteForTimeZones( tz =>
            {
                TestHelper.AssertTemplateOutputDate( "1-May-2018 12:00 AM",
                    "{{ '1-May-2018 3:00 PM' | ToMidnight }}" );
            } );
        }

        /// <summary>
        /// Tests the To Midnight using a string of "Now".
        /// </summary>
        [TestMethod]
        public void ToMidnight_Now()
        {
            LavaTestHelper.ExecuteForTimeZones( tz =>
            {
                var now = RockDateTime.Now;
                var midnightUtc = LavaDateTime.NewDateTimeOffset( now.Year, now.Month, now.Day, 0, 0, 0 );

                TestHelper.AssertTemplateOutputDate( midnightUtc,
                    "{{ 'Now' | ToMidnight }}" );
            } );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void ToMidnight_WithDateTimeOffsetAsInput_PreservesOffset()
        {
            LavaTestHelper.ExecuteForTimeZones( tz =>
            {
                // Get an input time of 10:00+04:00.
                var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, new TimeSpan( 2, 0, 0 ) );

                // Add the input DateTimeOffset object to the Lava context.
                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

                TestHelper.AssertTemplateOutput( "2018-05-01T00:00:00+02:00",
                    "{{ dateTimeInput | ToMidnight | Date:'yyyy-MM-ddTHH:mm:sszzz' }}",
                    mergeValues );
            } );
        }

        #endregion

        #region Filter Tests: SundayDate

        /// <summary>
        /// Applying the filter to a date/time string input returns the next Sunday date.
        /// </summary>
        [TestMethod]
        public void SundayDate_WithDateTimeStringAsInput_YieldsNextSundayDate()
        {
            LavaTestHelper.ExecuteForTimeZones( tz =>
            {
                TestHelper.AssertTemplateOutput( "2021-10-17", "{{ '2021-10-11' | SundayDate | Date:'yyyy-MM-dd' }}" );
            } );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a correct result.
        /// </summary>
        [TestMethod]
        public void SundayDate_WithDateTimeOffsetAsInput_YieldsNextSundayDate()
        {
            LavaTestHelper.ExecuteForTimeZones( tz =>
            {
                var baseDate = LavaDateTime.NewDateTime( 2021, 10, 11, 10, 0, 0 );

                // Add the input DateTimeOffset object to the Lava context.
                var mergeValues = new LavaDataDictionary() { { "dateTimeInput", baseDate } };

                // Get the next Sunday date in the active Rock time zone.
                var nextSundayDate = LavaDateTime.ConvertToRockDateTime( baseDate.GetNextWeekday( DayOfWeek.Sunday ).Date );

                TestHelper.AssertTemplateOutputDate( nextSundayDate,
                "{{ dateTimeInput | SundayDate | Date:'yyyy-MM-dd' }}",
                maximumDelta: null,
                mergeValues );
            } );
        }

        #endregion

    }
}
