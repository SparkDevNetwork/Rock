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
using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class AnalyticsSourceDateTests
    {
        /// <summary>
        /// Tests the fiscal week number calculation for an organization that starts its fiscal year in April.
        /// Rock adheres to the "4-day rule", meaning the first week of the fiscal year must contain at least 4 days.
        /// This test validates edge cases such as the transition between fiscal years —
        /// e.g., Saturday, April 1, 2023 is the last day of the 52nd week of FY2023,
        /// and Monday, April 3, 2023 starts the 1st week of FY2024.
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="expectedFiscalWeekNumber">The expected fiscal week number based on the fiscal calendar starting in April, with weeks starting on Monday.</param>
        /*
          ───────────────────────────────────────────── 2022 ────────────────────────────────────────────
                                                                            
                                                                                      December          
                                                                                                 
                                                                            Mon Tue Wed Thu Fri Sat Sun  
                                                                            ━━━━━━━━━━━━━━━━━━━━━━━━━━━  
                                                                                          1   2   3   4 
                                                                              5   6   7   8   9  10  11 
                                                                             12  13  14  15  16  17  18 
                                                                             19  20  21  22  23  24  25 
                                                                             26  27  28  29  30  31    
         
          ───────────────────────────────────────────── 2023 ─────────────────────────────────────────────

                    January                         February                           March           
                                                                                                 
          Mon Tue Wed Thu Fri Sat Sun      Mon Tue Wed Thu Fri Sat Sun      Mon Tue Wed Thu Fri Sat Sun  
          ━━━━━━━━━━━━━━━━━━━━━━━━━━━      ━━━━━━━━━━━━━━━━━━━━━━━━━━━      ━━━━━━━━━━━━━━━━━━━━━━━━━━━  
                                    1                1   2   3   4   5                1   2   3   4   5 
            2   3   4   5   6   7   8        6   7   8   9  10  11  12        6   7   8   9  10  11  12 
            9  10  11  12  13  14  15       13  14  15  16  17  18  19       13  14  15  16  17  18  19 
           16  17  18  19  20  21  22       20  21  22  23  24  25  26       20  21  22  23  24  25  26 
           23  24  25  26  27  28  29       27  28                           27  28  29  30  31         
           30  31                                                                                       
                                     
                     April           
                                     
          Mon Tue Wed Thu Fri Sat Sun
          ━━━━━━━━━━━━━━━━━━━━━━━━━━━
                                1   2
            3   4   5   6   7   8   9
           10  11  12  13  14  15  16
           17  18  19  20  21  22  23
           24  25  26  27  28  29  30
         */
        [DataTestMethod]
        [DataRow( "Sun, Dec 25, 2022", 38 )]
        [DataRow( "Mon, Dec 26, 2022", 39 )] // should only be 7 days with the 39th week
        [DataRow( "Tue, Dec 27, 2022", 39 )] // .
        [DataRow( "Wed, Dec 28, 2022", 39 )] // .
        [DataRow( "Thu, Dec 29, 2022", 39 )] // .
        [DataRow( "Fri, Dec 30, 2022", 39 )]
        [DataRow( "Sat, Dec 31, 2022", 39 )] // last day of the 39th week
        [DataRow( "Sun, Jan 1, 2023", 39 )]
        [DataRow( "Mon, Jan 2, 2023", 40 )]
        //...
        [DataRow( "Saturday, April 1, 2023", 52 )]
        [DataRow( "Sunday, April 2, 2023", 52 )]
        [DataRow( "Monday, April 3, 2023", 1 )]
        public void Date_UsingFiscalStartApril_WithStartingDayOfWeekMonday_HasValidFiscalWeekNumber( string date, int expectedFiscalWeekNumber )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalWeekNumber = AnalyticsSourceDate.GetFiscalWeek( dateTime, 4, DayOfWeek.Monday );

            Assert.AreEqual( expectedFiscalWeekNumber, fiscalWeekNumber );
        }

        /// <summary>
        /// Unit test that verifies the fiscal week number is correctly calculated
        /// for various dates and fiscal year start months.
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="fiscalMonthStart">The month the fiscal year starts in (1 = January, 9 = September, etc.).</param>
        /// <param name="expectedFiscalWeekNumber">The expected fiscal week number for the given date and fiscal start month.</param>
        ///
        /*
            ----------- 2021 -----------
            FWK     Mo Tu We Th Fr Sa Su
            ----------------------------
            48 Dec  29 30 01 02 03 04 05 
            49      06 07 08 09 10 11 12 
            50      13 14 15 16 17 18 19 
            51      20 21 22 23 24 25 26 
            52      27 28 29 30 31 

            ----------- 2022 -----------
            FWK     Mo Tu We Th Fr Sa Su
            ----------------------------
            52 Jan                 01 02 
            01      03 04 05 06 07 08 09 <-- start FY2022
            02      10 11 12 13 14 15 16 
            03      17 18 19 20 21 22 23 
            04      24 25 26 27 28 29 30 
         */
        [DataTestMethod]
        // Fiscal Start == Jan 
        [DataRow( "Sat, Dec 25, 2021", 1, 51 )]
        [DataRow( "Sun, Dec 26, 2021", 1, 51 )]
        [DataRow( "Mon, Dec 27, 2021", 1, 52 )] // 1 - 52nd week
        [DataRow( "Tue, Dec 28, 2021", 1, 52 )] // 2 - 52nd week
        [DataRow( "Wed, Dec 29, 2021", 1, 52 )] // 3 - 52nd week
        [DataRow( "Thu, Dec 30, 2021", 1, 52 )] // 4 - 52nd week
        [DataRow( "Fri, Dec 31, 2021", 1, 52 )] // 5 - 52nd week
        [DataRow( "Sat, Jan 1, 2022", 1, 52 )]  // 6 - 52nd week
        [DataRow( "Sun, Jan 2, 2022", 1, 52 )]  // 7 - 52nd week
        [DataRow( "Mon, Jan 3, 2022", 1, 1 )]   // 1st week of fiscal year
        [DataRow( "Tue, Jan 4, 2022", 1, 1 )]
        [DataRow( "Wed, Jan 5, 2022", 1, 1 )]
        // Fiscal Start == Sept
        /*
            ----------- 2022 -----------
            FWK     Mo Tu We Th Fr Sa Su
            ----------------------------
            49 Aug  01 02 03 04 05 06 07 
            50      08 09 10 11 12 13 14 
            51      15 16 17 18 19 20 21 
            52      22 23 24 25 26 27 28
            53      29 30 31
            01 Sep           01 02 03 04 
            02      05 06 07 08 09 10 11 
            03      12 13 14 15 16 17 18 
            04      19 20 21 22 23 24 25 
         */
        [DataRow( "Sun, August 21, 2022", 9, 51 )]
        [DataRow( "Mon, August 22, 2022", 9, 52 )] // 1 - 52nd week
        [DataRow( "Tue, August 23, 2022", 9, 52 )] // 2 - 52nd week
        [DataRow( "Wed, August 24, 2022", 9, 52 )] // 3 - 52nd week
        [DataRow( "Thu, August 25, 2022", 9, 52 )] // 4 - 52nd week
        [DataRow( "Fri, August 26, 2022", 9, 52 )] // 5 - 52nd week
        [DataRow( "Sat, August 27, 2022", 9, 52 )] // 6 - 52nd week
        [DataRow( "Sun, August 28, 2022", 9, 52 )] // 7 - 52nd week
        [DataRow( "Mon, August 29, 2022", 9, 53 )] // 53rd week
        [DataRow( "Tue, August 30, 2022", 9, 53 )]
        [DataRow( "Wed, August 31, 2022", 9, 53 )]
        [DataRow( "Thu, September 1, 2022", 9, 1 )] // 1st week of fiscal year
        public void Date_WithVariousFiscalMonthStart_HasValidFiscalWeekNumber( string date, int fiscalMonthStart, int expectedFiscalWeekNumber )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalWeekNumber = AnalyticsSourceDate.GetFiscalWeek( dateTime, fiscalMonthStart, DayOfWeek.Monday );

            Assert.AreEqual( expectedFiscalWeekNumber, fiscalWeekNumber );
        }

        /// <summary>
        /// Tests that the correct fiscal month number is calculated for a given date,
        /// based on a specified fiscal year start month. This helps verify edge cases across year transitions,
        /// such as December to January and fiscal starts in months like October or April.
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="fiscalMonthStart">The month the fiscal year starts (1 = January, 10 = October, etc.)</param>
        /// <param name="expectedFiscalMonthNumber">The expected fiscal month number result for the input date</param>
        [DataTestMethod]
        [DataRow( "Thu, Oct 1, 2020", 10, 1 )]
        [DataRow( "Sat, Dec 31, 2022", 1, 12 )]
        [DataRow( "Sun, Jan 1, 2023", 1, 1 )]
        [DataRow( "Mon, Jan 2, 2023", 1, 1 )]
        [DataRow( "Sat, April 1, 2023", 1, 4 )]
        [DataRow( "Sun, April 2, 2023", 1, 4 )]
        [DataRow( "Mon, April 3, 2023", 1, 4 )]
        public void Date_WithVariousFiscalMonthStart_HasValidFiscalMonth( string date, int fiscalMonthStart, int expectedFiscalMonthNumber )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalMonthNumber = AnalyticsSourceDate.GetFiscalMonthNumber( dateTime, fiscalMonthStart );
            Assert.AreEqual( expectedFiscalMonthNumber, fiscalMonthNumber );
        }

        /// <summary>
        /// This tests the FY calculation for an Organization that starts their FY in April, and
        /// Rock adheres to the 4 day week rule, which means the first week of the fiscal year
        /// must have at least 4 days in it. This creates some interesting edge cases such as
        /// Saturday, April 1, 2023 -- that day is the last day of the 52nd week of FY2023.
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="expectedFiscalYear">The expected fiscal year result for the input date</param>
        [DataTestMethod]
        /*
            ------- FY 2022-2023 -------
            FWK     Mo Tu We Th Fr Sa Su
            ----------------------------
                         2022
            53 Mar  27 28 29 30 31
            53 Apr                 01 02 
            01      03 04 05 06 07 08 09 <-- start FY2023
            02      10 11 12 13 14 15 16 
            03      17 18 19 20 21 22 23 
            04      24 25 26 27 28 29 30 
            ...
            35 Dec              01 02 03 
            36      04 05 06 07 08 09 10 
            37      11 12 13 14 15 16 17 
            38      18 19 20 21 22 23 24 
            39      25 26 27 28 29 30 31

                         2023
            40 Jan  01 02 03 04 05 06 07
            41      08 09 10 11 12 13 14 
            42      15 16 17 18 19 20 21 
            43      22 23 24 25 26 27 28 
            ...
            52 Mar  26 27 28 29 30 31
            52 Apr                    01 
            01      02 03 04 05 06 07 08 <-- start FY2024
         */
        [DataRow( "Saturday, April 1, 2023", 2023 )] // Because of the 4 day week rule, the 3rd is the first week in FY2024
        [DataRow( "Sunday, April 2, 2023", 2023 )]
        [DataRow( "Monday, April 3, 2023", 2024 )]
        [DataRow( "Sunday, December 31, 2023", 2024 )]
        [DataRow( "Monday, January 1, 2024", 2024 )]
        [DataRow( "Monday, April 1, 2024", 2025 )] // But here the 1st is the first week in FY2025
        public void Date_UsingFiscalStartApril_WithStartingDayOfWeekMonday_HasValidFiscalYear( string date, int expectedFiscalYear )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalWeekNumber = AnalyticsSourceDate.GetFiscalWeek( dateTime, 4, DayOfWeek.Monday );
            int fiscalYear = AnalyticsSourceDate.GetFiscalYear( dateTime, fiscalWeekNumber, 4, DayOfWeek.Monday );
            Assert.AreEqual( expectedFiscalYear, fiscalYear );
        }

        /// <summary>
        /// This tests the Fiscal Quarter calculation for an Organization that starts their FY in April.
        /// Rock adheres to the 4 day week rule, which means the first week of the fiscal year
        /// must have at least 4 days in it. This creates some interesting edge cases such as
        /// Saturday, April 1, 2023 -- that day is the last day of the 52nd week of FY2023 (or
        /// Q4 of the previous year).
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="expectedFiscalQuarter">The expected fiscal quarter result for the input date</param>
        [DataTestMethod]
        [DataRow( "Saturday, April 1, 2023", 4 )]
        [DataRow( "Sunday, April 2, 2023", 4 )]
        [DataRow( "Monday, April 3, 2023", 1 )]
        [DataRow( "Friday, June 30, 2023", 1 )]
        [DataRow( "Sunday, October 1, 2023", 3 )]
        [DataRow( "Monday, Jan 1, 2024", 4 )]
        [DataRow( "Sunday, March 31, 2024", 4 )]
        public void Date_UsingFiscalStartApril_WithStartingDayOfWeekMonday_HasValidFiscalQuarter( string date, int expectedFiscalQuarter )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalQuarter = AnalyticsSourceDate.GetFiscalQuarter( dateTime, 4, DayOfWeek.Monday );

            Assert.AreEqual( expectedFiscalQuarter, fiscalQuarter );
        }

        /// <summary>
        /// This tests the Fiscal Half Year calculation for an Organization that starts their FY in April.
        /// Rock adheres to the 4 day week rule, which means the first week of the fiscal year
        /// must have at least 4 days in it. This creates some interesting edge cases such as
        /// Saturday, April 1, 2023 -- that day is the last day of the 52nd week of FY2023 (or
        /// the "Second" half of the year).
        /// </summary>
        /// <param name="date">The input date string</param>
        /// <param name="expectedFiscalHalfYear">The expected fiscal half-year result for the input date</param>
        [DataTestMethod]
        [DataRow( "Saturday, April 1, 2023", "Second" )]
        [DataRow( "Sunday, April 2, 2023", "Second" )]
        [DataRow( "Monday, April 3, 2023", "First" )]
        [DataRow( "Sunday, October 1, 2023", "Second" )]
        public void Date_UsingFiscalStartApril_WithStartingDayOfWeekMonday_HasValidFiscalHalfYear( string date, string expectedFiscalHalfYear )
        {
            var dateTime = DateTime.Parse( date );
            int fiscalQuarter = AnalyticsSourceDate.GetFiscalQuarter( dateTime, 4, DayOfWeek.Monday );
            string fiscalHalfyear = AnalyticsSourceDate.GetFiscalHalfYear( fiscalQuarter );

            Assert.AreEqual( expectedFiscalHalfYear, fiscalHalfyear );
        }
    }
}
