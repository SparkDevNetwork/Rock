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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class DateFilterTests : LavaUnitTestBase
    {
        // Defines the UTC offset for the comparison dates used in unit tests in this class.
        // Modifying this value simulates executing the test set for a different timezone, but should have no impact on the result.
        private static TimeSpan _utcBaseOffset = new TimeSpan( 4, 0, 0 );

        #region Filter Tests: Date

        /*
         * The Date filter is implemented using the standard .NET Format() function, so we only need to exhaustively test the custom format parameters.
        */

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        public void Date_NullInput_ProducesEmptyString()
        {
            var template = "{{ '' | Date:'yyyy-MM-dd HH:mm:ss' }}";

            TestHelper.AssertTemplateOutput( string.Empty, template );
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
        /// Using the Date filter to format a DateTimeOffset type should correctly account for the offset in the output.
        /// </summary>
        [TestMethod]
        public void AsDateTime_WithDateTimeOffsetObjectAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var dateTimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // Convert the input time to local server time, which most likely has a different UTC offset to the input date.
            var serverLocalTimeString = dateTimeInput.ToLocalTime().ToString( "HH:mm" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInput } };

            // Verify that the Lava Date filter parses the DateTimeOffset value correctly to include the offset, and the result matches the local server time.
            TestHelper.AssertTemplateOutput( serverLocalTimeString, "{{ dateTimeInput | AsDateTime | Date:'HH:mm' }}", mergeValues );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should correctly account for the offset in the output.
        /// </summary>
        [TestMethod]
        public void AsDateTime_WithDateTimeOffsetStringAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var dateTimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            var dateTimeInputString = dateTimeInput.ToString();

            // Convert the input time to local server time, which most likely has a different UTC offset to the input date.
            var serverLocalTimeString = dateTimeInput.ToLocalTime().ToString( "HH:mm" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", dateTimeInputString } };

            // Verify that the Lava Date filter parses the DateTimeOffset value correctly to include the offset, and the result matches the local server time.
            TestHelper.AssertTemplateOutput( serverLocalTimeString, "{{ dateTimeInput | AsDateTime | Date:'HH:mm' }}", mergeValues );
        }

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow( "d/MMM/yy", "1/May/18" )]
        [DataRow( "MMMM dd, yyyy H:mm:ss", "May 01, 2018 18:30:00" )]
        [DataRow( "yyyy-MM-dd HH:mm:ss", "2018-05-01 18:30:00" )]
        public void Date_UsingValidDotNetFormatString_ProducesValidDate( string formatString, string result )
        {
            var template = "{{ '1-May-2018 6:30 PM' | Date:'<formatString>' }}"
                .Replace( "<formatString>", formatString );

            TestHelper.AssertTemplateOutput( result, template );
        }

        /// <summary>
        /// The Date filter should accept the keyword 'Now' as input and resolve it to the current date using the general datetime output format.
        /// </summary>
        [TestMethod]
        public void Date_NowWithNoFormatStringAsInput_ResolvesToCurrentDateTimeWithGeneralFormat()
        {
            TestHelper.AssertTemplateOutput( RockDateTime.Now.ToString( "g" ), "{{ 'Now' | Date }}" );
        }

        /// <summary>
        /// The Date filter should accept the keyword 'Now' as input and resolve it to the current date with the specified format.
        /// </summary>
        [TestMethod]
        public void Date_NowWithFormatStringAsInput_ResolvesToCurrentDateTimeWithSpecifiedFormat()
        {
            TestHelper.AssertTemplateOutputDate( RockDateTime.Now.ToString( "yyyy-MM-dd" ), "{{ 'Now' | Date:'yyyy-MM-dd' }}" );
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
            var shortTime = new DateTime( 2018, 5, 1, 18, 30, 0 );

            TestHelper.AssertTemplateOutput( shortTime.ToShortTimeString(), "{{ '1-May-2018 6:30 PM' | Date:'st' }}" );
        }

        /// <summary>
        /// Adding hours to an input date with a time zone that differs from the server time zone should return the correct result for the input time zone.
        /// </summary>
        [TestMethod]
        public void DateAdd_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // Convert the input time to local server time, which may have a different UTC offset to the input date.
            var serverLocalTimeString = datetimeInput.ToLocalTime().AddHours( 1 ).ToString( "HH:mm" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset value as a local server time.
            TestHelper.AssertTemplateOutput( serverLocalTimeString, "{{ dateTimeInput | DateAdd:1,'h' | Date:'HH:mm' }}", mergeValues );
        }

        /// <summary>
        /// Using the Date filter to format a DateTimeOffset type should correctly account for the offset in the output.
        /// </summary>
        [TestMethod]
        public void Date_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // Convert the input time to local server time, which may have a different UTC offset to the input date.
            var serverLocalTimeString = datetimeInput.ToLocalTime().ToString( "HH:mm" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            // Verify that the Lava Date filter formats the DateTimeOffset value as a local server time.
            TestHelper.AssertTemplateOutput( serverLocalTimeString, "{{ dateTimeInput | Date:'HH:mm' }}", mergeValues );
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
        public void DateAdd_Now_ResolvesToCurrentDate()
        {
            TestHelper.AssertTemplateOutputDate( DateTime.Now.AddDays( 5 ), "{{ 'Now' | DateAdd:5,'d' }}", TimeSpan.FromSeconds( 300 ) );
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
        /// Requesting the difference between a target date and a DateTimeOffset should return a result that accounts for the input time zone.
        /// </summary>
        [TestMethod]
        public void DateDiff_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get an input time of 10:00 AM in the test timezone.
            var datetimeInput = new DateTimeOffset( 2018, 5, 1, 10, 0, 0, _utcBaseOffset );

            // Convert the input time to local server time, which may have a different UTC offset to the input date, then add 7 hours.
            var serverLocalTimeString = datetimeInput.ToLocalTime().AddHours( 7 ).ToString( "yyyy-MM-dd HH:mm" );

            // Add the input DateTimeOffset object to the Lava context.
            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput }, { "serverLocalTime", serverLocalTimeString } };

            // Verify that the difference between the input date and the adjusted local time is 7 hours.
            TestHelper.AssertTemplateOutput( "7", "{{ dateTimeInput | DateDiff:serverLocalTime,'h' }}", mergeValues );
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

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -14 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "14 days ago", template );
        }

        /// <summary>
        /// Using the previous day as a parameter should return "yesterday".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_ComparePreviousDay_YieldsYesterday()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -1 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "yesterday", template );
        }

        /// <summary>
        /// Using the currentr date as a parameter should return "today".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareCurrentDate_YieldsToday()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "today", template );
        }

        /// <summary>
        /// Using the previous day as a parameter should return "yesterday".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareNextDay_YieldsTomorrow()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 1 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "tomorrow", template );
        }

        /// <summary>
        /// Using a future date as a parameter should return "in X days".
        /// </summary>
        [TestMethod]
        public void DaysFromNow_CompareFutureDate_YieldsInDays()
        {
            var template = "{{ '<compareDate>' | DaysFromNow }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 14 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "in 14 days", template );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void DaysFromNow_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get a local time of 1:00am tomorrow.
            var tomorrow = RockDateTime.Now.Date.AddDays( 1 ).AddHours( 1 );
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( tomorrow );

            // First, verify that the Lava filter returns "tomorrow" when the DateTimeOffset resolves to local time 1:00am tomorrow.
            var datetimeInput = new DateTimeOffset( tomorrow );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "tomorrow", "{{ dateTimeInput | DaysFromNow }}", mergeValues );

            // Now verify that the Lava filter will return "today" if the DateTimeOffset is 11:00pm today.
            var datetimeInput2 = new DateTimeOffset( tomorrow.Year, tomorrow.Month, tomorrow.Day, tomorrow.Hour, tomorrow.Minute, tomorrow.Second, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "today", "{{ dateTimeInput | DaysFromNow }}", mergeValues2 );
        }

        #endregion

        /// <summary>
        /// Input keyword 'Now' should return number of days in current month.
        /// </summary>
        [TestMethod]
        public void DaysInMonth_InputKeywordNow_YieldsDaysInCurrentMonth()
        {
            var currentDateTime = RockDateTime.Now;

            var targetDate = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( 1 );

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

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "3", template );
        }

        /// <summary>
        /// Using today's date as a parameter should return a result of zero.
        /// </summary>
        [TestMethod]
        public void DaysSince_CompareCurrentDate_YieldsZero()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "0", template );
        }

        /// <summary>
        /// Using yesterday's date as a parameter should return a result of one.
        /// </summary>
        [TestMethod]
        [Ignore( "This test fails. A Spark internal issue has been raised to verify the correct operation of this filter." )]
        public void DaysSince_ComparePreviousDay_YieldsOne()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.Date.AddMinutes( -1 ).ToString( "dd-MMM-yyyy HH:mm:ss" ) );

            TestHelper.AssertTemplateOutput( "1", template );
        }

        /// <summary>
        /// Using a future date as a parameter should return a negative number of days.
        /// </summary>
        [TestMethod]
        public void DaysSince_CompareFutureDate_YieldsNegativeInteger()
        {
            var template = "{{ '<compareDate>' | DaysSince }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 3 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "-2", template );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        [Ignore( "This test fails. A Spark internal issue has been raised to verify the correct operation of this filter." )]
        public void DaysSince_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            // Get a local time of 1:00am two weeks from today.
            var futureDate = RockDateTime.Now.Date.AddDays( -14 ).AddHours( 1 );
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( futureDate );

            // First, verify that the Lava filter returns "14" when the DateTimeOffset resolves to local time 1:00am two weeks from today.
            var datetimeInput = new DateTimeOffset( futureDate );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "14", "{{ dateTimeInput | DaysSince }}", mergeValues );

            // Now verify that the Lava filter returns "15" if the DateTimeOffset is increased by 2 hours.
            var datetimeInput2 = new DateTimeOffset( futureDate.Year, futureDate.Month, futureDate.Day, futureDate.Hour, futureDate.Minute, futureDate.Second, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

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

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 3 ).ToString( "dd-MMM-yyyy" ) );

            // Note that only complete days are counted in the result, so the remaining portion of today is not included, nor is any portion of the target date.
            TestHelper.AssertTemplateOutput( "2", template );
        }

        /// <summary>
        /// Using today's date as a parameter should return a result of zero.
        /// </summary>
        [TestMethod]
        public void DaysUntil_CompareCurrentDate_YieldsZero()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "0", template );
        }

        /// <summary>
        /// Using tomorrow's date as a parameter should return a result of one.
        /// </summary>
        [TestMethod]
        [Ignore( "This test fails. A Spark internal issue has been raised to verify the correct operation of this filter." )]
        public void DaysUntil_CompareNextDay_YieldsOne()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.Date.AddDays( 1 ).AddMinutes( 1 ).ToString( "dd-MMM-yyyy HH:mm:ss" ) );

            TestHelper.AssertTemplateOutput( "1", template );
        }

        /// <summary>
        /// Using a past date as a parameter should return a negative number of days.
        /// </summary>
        [TestMethod]
        public void DaysUntil_ComparePastDate_YieldsNegativeInteger()
        {
            var template = "{{ '<compareDate>' | DaysUntil }}";

            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -3 ).ToString( "dd-MMM-yyyy" ) );

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
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -1 ).ToString( "dd-MMM-yyyy" ) );

            TestHelper.AssertTemplateOutput( "yesterday", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X hours earlier than the current date/time should return "X hours ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareHoursEarlier_YieldsHoursAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddHours( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss tt" ) );

            TestHelper.AssertTemplateOutput( "2 hours ago", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X days earlier than the current date/time should return "X days ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareDaysEarlier_YieldsDaysAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss" ) );

            TestHelper.AssertTemplateOutput( "2 days ago", template );
        }

        /// <summary>
        /// Comparing an input date/time that is X months earlier than the current date/time should return "X months ago".
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_CompareMonthsEarlier_YieldsMonthsAgo()
        {
            var template = "{{ '<compareDate>' | HumanizeDateTime }}";
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddMonths( -2 ).ToString( "dd-MMM-yyyy hh:mm:ss" ) );

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
        /// </summary>
        [TestMethod]
        public void HumanizeDateTime_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( RockDateTime.Now );

            // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
            var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "14 days from now", "{{ dateTimeInput | HumanizeDateTime:'1-May-2020 1:00 AM' }}", mergeValues );

            // Next, verify that the Lava filter returns the previous day if the DateTimeOffset is increased such that the datetime
            // crosses the boundary to the previous day when translated to local time.
            var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "13 days from now", "{{ dateTimeInput | HumanizeDateTime:'1-May-2020 1:00 AM' }}", mergeValues2 );
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
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours, M minutes".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_NowAsInput_YieldsResult()
        {
            var template = "{{ 'Now' | HumanizeTimeSpan:'<compareDate>' }}";
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 3 ).ToString( "yyyy-MM-dd hh:mm:ss" ) );

            TestHelper.AssertTemplateOutputRegex( "[23] days", template );
        }

        /// <summary>
        /// Comparing an input date/time to a supplied reference that is weeks/days/hours/minutes later should return "W weeks, D days, H hours, M minutes".
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_NowAsReferenceDate_YieldsResult()
        {
            var template = "{{ '<compareDate>' | HumanizeTimeSpan:'Now' }}";
            template = template.Replace( "<compareDate>", RockDateTime.Now.AddDays( 3 ).ToString( "yyyy-MM-dd hh:mm:ss" ) );

            TestHelper.AssertTemplateOutputRegex( "[23] days", template );

        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void HumanizeTimeSpan_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( RockDateTime.Now );

            // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
            var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "2 weeks", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues );

            // Next, verify that the Lava filter returns a lesser result if the DateTimeOffset is increased such that the datetime
            // crosses the boundary to the previous day when translated to local time.
            var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "1 week", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues2 );
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
        public void NextDayOfTheWeek_WithNextDow_ReturnsNextDaysDate()
        {
            // Since Wednesday has not happened, we advance to it -- which is Wed, 5/2
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
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( RockDateTime.Now );

            // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
            var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "2 weeks", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues );

            // Next, verify that the Lava filter returns a lesser result if the DateTimeOffset is increased such that the datetime
            // crosses the boundary to the previous day when translated to local time.
            var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "1 week", "{{ dateTimeInput | HumanizeTimeSpan:'1-May-2020 1:00 AM' }}", mergeValues2 );
        }

        #endregion

        #region Filter Tests: ToMidnight

        /// <summary>
        /// Applying the filter to a date/time input with a non-midnight time component returns the same date with a 12:00 AM time component.
        /// </summary>
        [TestMethod]
        public void ToMidnight_InputDateHasTimeComponent_YieldsMidnight()
        {
            TestHelper.AssertTemplateOutputDate( "1-May-2018 12:00 AM",
                                      "{{ '1-May-2018 3:00 PM' | ToMidnight }}" );
        }

        /// <summary>
        /// Tests the To Midnight using a string of "Now".
        /// </summary>
        [TestMethod]
        public void ToMidnight_Now()
        {
            TestHelper.AssertTemplateOutputDate( RockDateTime.Now.Date,
                                      "{{ 'Now' | ToMidnight }}" );
        }

        /// <summary>
        /// Using a DateTimeOffset type as the filter input should return a result that accounts for the input time offset.
        /// </summary>
        [TestMethod]
        public void ToMidnight_WithDateTimeOffsetAsInput_AdjustsResultForOffset()
        {
            var localOffset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( RockDateTime.Now );

            // First, verify that the Lava filter returns a predictable result for input expressed in local server time.
            var datetimeInput = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset );

            var mergeValues = new LavaDataDictionary() { { "dateTimeInput", datetimeInput } };

            TestHelper.AssertTemplateOutput( "2020-05-15 00:00:00", "{{ dateTimeInput | ToMidnight | Date:'yyyy-MM-dd HH:mm:ss' }}", mergeValues );

            // Next, verify that the Lava filter returns a lesser result if the DateTimeOffset is increased such that the datetime
            // crosses the boundary to the previous day when translated to local time.
            var datetimeInput2 = new DateTimeOffset( 2020, 5, 15, 1, 0, 0, localOffset.Add( new TimeSpan( 2, 0, 0 ) ) );

            var mergeValues2 = new LavaDataDictionary() { { "dateTimeInput", datetimeInput2 } };

            TestHelper.AssertTemplateOutput( "2020-05-14 00:00:00", "{{ dateTimeInput | ToMidnight | Date:'yyyy-MM-dd HH:mm:ss' }}", mergeValues2 );
        }

        #endregion

    }
}
