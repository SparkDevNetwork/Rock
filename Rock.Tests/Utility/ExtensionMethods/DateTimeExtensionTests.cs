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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        /// <summary>
        /// Should return the correct date.
        /// </summary>
        [TestMethod]
        public void StartOfMonth_GivesCorrectDate()
        {
            var tests = new[] {
                new { Date = new DateTime(2020, 10, 25, 5, 6, 50 ), Expected = new DateTime( 2020, 10, 1 ) },
                new { Date = new DateTime(2010, 6, 25, 23, 6, 50 ), Expected = new DateTime( 2010, 6, 1 ) },
            };

            foreach ( var test in tests )
            {
                Assert.AreEqual( test.Expected, test.Date.StartOfMonth() );
            }
        }

        #region To Elapsed String 

        #region Seconds

        [TestMethod]
        public void ToElapsedString_SecondsPastDateIncludesAgo()
        {
            Assert.Contains( "Seconds Ago", RockDateTime.Now.AddSeconds( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanOneSecondPastGivesSingularNoun()
        {
            Assert.AreEqual( "1 Second Ago", RockDateTime.Now.ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneSecondGivesPluralNoun()
        {
            Assert.AreEqual( "5 Seconds Ago", RockDateTime.Now.AddSeconds( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_SecondsInFutureIncludesFromNow()
        {
            Assert.Contains( "Seconds From Now", RockDateTime.Now.AddSeconds( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanOneSecondInFutureGivesSingularNoun()
        {
            Assert.Contains( "Second From Now", RockDateTime.Now.AddSeconds( 1 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneSecondInFutureGivesPluralNoun()
        {
            Assert.Contains( "Seconds From Now", RockDateTime.Now.AddSeconds( 5 ).ToElapsedString() );
        }

        #endregion

        #region Minutes

        [TestMethod]
        public void ToElapsedString_MinutesPastDateIncludesAgo()
        {
            Assert.Contains( "Minutes Ago", RockDateTime.Now.AddMinutes( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMinutesPastGivesSingularNoun()
        {
            Assert.AreEqual( "1 Minute Ago", RockDateTime.Now.AddMinutes( -1 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMinuteGivesPluralNoun()
        {
            Assert.AreEqual( "5 Minutes Ago", RockDateTime.Now.AddMinutes( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_MinutesInFutureIncludesFromNow()
        {
            Assert.Contains( "Minutes From Now", RockDateTime.Now.AddMinutes( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMinutesInFutureGivesSingularNoun()
        {
            Assert.Contains( "Minute From Now", RockDateTime.Now.AddMinutes( 1 ).AddSeconds( 2 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMinuteInFutureGivesPluralNoun()
        {
            Assert.Contains( "Minutes From Now", RockDateTime.Now.AddMinutes( 5 ).ToElapsedString() );
        }

        #endregion

        #region Hours

        [TestMethod]
        public void ToElapsedString_HoursPastDateIncludesAgo()
        {
            Assert.Contains( "Hours Ago", RockDateTime.Now.AddHours( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoHoursPastGivesSingularNoun()
        {
            Assert.AreEqual( "1 Hour Ago", RockDateTime.Now.AddHours( -1 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneHourGivesPluralNoun()
        {
            Assert.AreEqual( "5 Hours Ago", RockDateTime.Now.AddHours( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_HoursInFutureIncludesFromNow()
        {
            Assert.Contains( "Hours From Now", RockDateTime.Now.AddHours( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoHoursInFutureGivesSingularNoun()
        {
            Assert.Contains( "Hour From Now", RockDateTime.Now.AddHours( 1 ).AddMinutes( 2 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneHourInFutureGivesPluralNoun()
        {
            Assert.Contains( "Hours From Now", RockDateTime.Now.AddHours( 5 ).ToElapsedString() );
        }

        #endregion

        #region Days

        [TestMethod]
        public void ToElapsedString_DaysPastDateIncludesAgo()
        {
            Assert.Contains( "Days Ago", RockDateTime.Now.AddDays( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoDaysPastGivesSingularNoun()
        {
            Assert.AreEqual( "1 Day Ago", RockDateTime.Now.AddDays( -1 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneDayGivesPluralNoun()
        {
            Assert.AreEqual( "5 Days Ago", RockDateTime.Now.AddDays( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_DaysInFutureIncludesFromNow()
        {
            Assert.Contains( "Days From Now", RockDateTime.Now.AddDays( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoDaysInFutureGivesSingularNoun()
        {
            Assert.Contains( "Day From Now", RockDateTime.Now.AddDays( 1 ).AddHours( 2 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneDayInFutureGivesPluralNoun()
        {
            Assert.Contains( "Days From Now", RockDateTime.Now.AddDays( 5 ).ToElapsedString() );
        }

        #endregion

        #region Months

        [TestMethod]
        public void ToElapsedString_MonthsPastDateIncludesAgo()
        {
            Assert.Contains( "Months Ago", RockDateTime.Now.AddMonths( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMonthsPastGivesSingularNoun()
        {
            var nowDate = RockDateTime.New( 2024, 6, 30 ).Value;
            var date = RockDateTime.New( 2024, 5, 14 ).Value;
            Assert.AreEqual( "1 Month Ago", DateTimeExtensions.ToElapsedString( date, nowDate ) );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMonthGivesPluralNoun()
        {
            Assert.AreEqual( "5 Months Ago", RockDateTime.Now.AddMonths( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_MonthsInFutureIncludesFromNow()
        {
            Assert.Contains( "Months From Now", RockDateTime.Now.AddMonths( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMonthsInFutureGivesSingularNoun()
        {
            var nowDate = RockDateTime.New( 2024, 5, 1 ).Value;
            var date = RockDateTime.New( 2024, 6, 30 ).Value;
            Assert.AreEqual( "1 Month From Now", DateTimeExtensions.ToElapsedString( date, nowDate ) );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMonthInFutureGivesPluralNoun()
        {
            Assert.Contains( "Months From Now", RockDateTime.Now.AddMonths( 5 ).ToElapsedString() );
        }

        #endregion

        #region Years

        [TestMethod]
        public void ToElapsedString_YearsPastDateIncludesAgo()
        {
            Assert.Contains( "Years Ago", RockDateTime.Now.AddYears( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoYearsPastGivesSingularNoun()
        {
            Assert.AreEqual( "1 Year Ago", RockDateTime.Now.AddYears( -1 ).AddMonths( -7 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneYearGivesPluralNoun()
        {
            Assert.AreEqual( "5 Years Ago", RockDateTime.Now.AddYears( -5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_YearsInFutureIncludesFromNow()
        {
            Assert.Contains( "Years From Now", RockDateTime.Now.AddYears( 5 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoYearsInFutureGivesSingularNoun()
        {
            Assert.Contains( "Year From Now", RockDateTime.Now.AddYears( 1 ).AddMonths( 7 ).ToElapsedString() );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneYearInFutureGivesPluralNoun()
        {
            Assert.Contains( "Years From Now", RockDateTime.Now.AddYears( 5 ).ToElapsedString() );
        }

        #endregion

        #endregion
    }
}
