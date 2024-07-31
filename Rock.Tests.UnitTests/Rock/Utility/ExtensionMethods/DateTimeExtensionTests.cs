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

namespace Rock.Tests.Rock.Utility.ExtensionMethods
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
                Assert.That.AreEqual( test.Expected, test.Date.StartOfMonth() );
            }
        }

        #region To Elapsed String 

        #region Seconds

        [TestMethod]
        public void ToElapsedString_SecondsPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddSeconds( -5 ).ToElapsedString(), "Seconds Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanOneSecondPastGivesSingularNoun()
        {
            Assert.That.Equal( RockDateTime.Now.ToElapsedString(), "1 Second Ago" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneSecondGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddSeconds( -5 ).ToElapsedString(), "5 Seconds Ago" );
        }

        [TestMethod]
        public void ToElapsedString_SecondsInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddSeconds( 5 ).ToElapsedString(), "Seconds From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanOneSecondInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddSeconds( 1 ).ToElapsedString(), "Second From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneSecondInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddSeconds( 5 ).ToElapsedString(), "Seconds From Now" );
        }

        #endregion

        #region Minutes

        [TestMethod]
        public void ToElapsedString_MinutesPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddMinutes( -5 ).ToElapsedString(), "Minutes Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMinutesPastGivesSingularNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddMinutes( -1 ).ToElapsedString(), "1 Minute Ago" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMinuteGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddMinutes( -5 ).ToElapsedString(), "5 Minutes Ago" );
        }

        [TestMethod]
        public void ToElapsedString_MinutesInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddMinutes( 5 ).ToElapsedString(), "Minutes From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMinutesInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddMinutes( 1 ).AddSeconds( 2 ).ToElapsedString(), "Minute From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMinuteInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddMinutes( 5 ).ToElapsedString(), "Minutes From Now" );
        }

        #endregion

        #region Hours

        [TestMethod]
        public void ToElapsedString_HoursPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddHours( -5 ).ToElapsedString(), "Hours Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoHoursPastGivesSingularNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddHours( -1 ).ToElapsedString(), "1 Hour Ago" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneHourGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddHours( -5 ).ToElapsedString(), "5 Hours Ago" );
        }

        [TestMethod]
        public void ToElapsedString_HoursInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddHours( 5 ).ToElapsedString(), "Hours From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoHoursInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddHours( 1 ).AddMinutes( 2 ).ToElapsedString(), "Hour From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneHourInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddHours( 5 ).ToElapsedString(), "Hours From Now" );
        }

        #endregion

        #region Days

        [TestMethod]
        public void ToElapsedString_DaysPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddDays( -5 ).ToElapsedString(), "Days Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoDaysPastGivesSingularNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddDays( -1 ).ToElapsedString(), "1 Day Ago" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneDayGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddDays( -5 ).ToElapsedString(), "5 Days Ago" );
        }

        [TestMethod]
        public void ToElapsedString_DaysInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddDays( 5 ).ToElapsedString(), "Days From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoDaysInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddDays( 1 ).AddHours( 2 ).ToElapsedString(), "Day From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneDayInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddDays( 5 ).ToElapsedString(), "Days From Now" );
        }

        #endregion

        #region Months

        [TestMethod]
        public void ToElapsedString_MonthsPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddMonths( -5 ).ToElapsedString(), "Months Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMonthsPastGivesSingularNoun()
        {
            var nowDate = RockDateTime.New( 2024, 6, 30 ).Value;
            var date = RockDateTime.New( 2024, 5, 14 ).Value;
            Assert.That.Equal( "1 Month Ago", DateTimeExtensions.ToElapsedString( date, nowDate ) );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMonthGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddMonths( -5 ).ToElapsedString(), "5 Months Ago" );
        }

        [TestMethod]
        public void ToElapsedString_MonthsInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddMonths( 5 ).ToElapsedString(), "Months From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoMonthsInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddMonths( 1 ).AddDays( 2 ).ToElapsedString(), "Month From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneMonthInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddMonths( 5 ).ToElapsedString(), "Months From Now" );
        }

        #endregion

        #region Years

        [TestMethod]
        public void ToElapsedString_YearsPastDateIncludesAgo()
        {
            Assert.That.Contains( RockDateTime.Now.AddYears( -5 ).ToElapsedString(), "Years Ago" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoYearsPastGivesSingularNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddYears( -1 ).AddMonths( -7 ).ToElapsedString(), "1 Year Ago" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneYearGivesPluralNoun()
        {
            Assert.That.Equal( RockDateTime.Now.AddYears( -5 ).ToElapsedString(), "5 Years Ago" );
        }

        [TestMethod]
        public void ToElapsedString_YearsInFutureIncludesFromNow()
        {
            Assert.That.Contains( RockDateTime.Now.AddYears( 5 ).ToElapsedString(), "Years From Now" );
        }

        [TestMethod]
        public void ToElapsedString_LessThanTwoYearsInFutureGivesSingularNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddYears( 1 ).AddMonths( 7 ).ToElapsedString(), "Year From Now" );
        }

        [TestMethod]
        public void ToElapsedString_GreaterThanOneYearInFutureGivesPluralNoun()
        {
            Assert.That.Contains( RockDateTime.Now.AddYears( 5 ).ToElapsedString(), "Years From Now" );
        }

        #endregion

        #endregion
    }
}
