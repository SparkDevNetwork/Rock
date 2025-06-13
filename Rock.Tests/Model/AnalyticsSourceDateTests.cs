using System;
using System.Web.UI.WebControls;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class AnalyticsSourceDateTests
    {
        /// <summary>
        /// This tests the Fiscal Week # calculation for an Organization that starts their FY in April.
        /// Rock adheres to the 4 day week rule, which means the first week of the fiscal year
        /// must have at least 4 days in it. This creates some interesting edge cases such as
        /// Saturday, April 1, 2023 -- that day is the last day of the 52nd week of FY2023.
        /// </summary>
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

        [DataTestMethod]
        // Fiscal Start Jan 
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
        // Fiscal Start Sept   
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
        /// <param name="date"></param>
        /// <param name="expectedFiscalYear"></param>
        [DataTestMethod]
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

        //[DataTestMethod]
        //[DataRow( "Saturday, April 1, 2023", "March" )]
        //[DataRow( "Sunday, April 2, 2023", "March" )]
        //[DataRow( "Monday, April 3, 2023", "April" )]
        //public void Date_UsingFiscalStartApril_HasValidFiscalMonth( string date, string expectedFiscalMonth )
        //{
        //    //var fiscalMonth = private static int GetFiscalWeek( DateTime date, int fiscalYearStartMonth, DayOfWeek firstDayOfWeek, int minimumDaysRequiredInFirstWeek = 4 )
        //   // Assert.AreEqual( "0667", expectedFiscalMonth );
        //}

        //[DataTestMethod]
        //[DataRow( "Saturday, April 1, 2023", "2022" )]
        //[DataRow( "Sunday, April 2, 2023", "2022" )]
        //[DataRow( "Monday, April 3, 2023", "2023" )]
        //public void Date_UsingFiscalStartApril_HasValidFiscalYear( string date, string expectedFiscalMonth )
        //{
        //    //var fiscalYear = AnalyticsSourceDate.
        //    // Assert.AreEqual( "0667", expectedFiscalMonth );
        //}

    }
}
