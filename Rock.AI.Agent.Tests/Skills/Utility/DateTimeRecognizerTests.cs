using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities;

namespace Rock.AI.Agent.Tests.Skills.Utility
{
    [TestClass]
    public class DateTimeRecognizerTests
    {
        private readonly DateTime _referenceDate = new DateTime( 2025, 7, 30 );

        #region Range Recognition

        [DataTestMethod]
        [DataRow( "this week", "2025-07-28", "2025-08-04" )]
        [DataRow( "last week", "2025-07-21", "2025-07-28" )]
        [DataRow( "last 3 months", "2025-04-30", "2025-07-30" )]
        [DataRow( "march 14th to july 31st", "2025-03-14", "2025-07-31" )]
        [DataRow( "march 14th to july 31st of 2024", "2024-03-14", "2024-07-31" )]
        [DataRow( "last two weeks", "2025-07-16", "2025-07-30" )]
        [DataRow( "year to date", "2025-01-01", "2025-07-30" )]
        [DataRow( "last year", "2024-01-01", "2025-01-01" )]
        [DataRow( "first quarter", "2025-01-01", "2025-04-01" )]
        public void RecognizeRange_WorksAsExpected(
            string query,
            string expectedStart,
            string expectedEnd )
        {
            var result = DateTimeRecognitionHelper.RecognizeDateRange( query, _referenceDate );

            AssertDateRangeNotNull( result );
            Assert.AreEqual( DateTime.ParseExact( expectedStart, "yyyy-MM-dd", null ), result.StartDate.Value, "Incorrect start date." );
            Assert.AreEqual( DateTime.ParseExact( expectedEnd, "yyyy-MM-dd", null ), result.EndDate.Value, "Incorrect end date." );
        }

        #endregion

        #region Single Date Recognition

        [DataTestMethod]
        [DataRow( "tomorrow", 2025, 7, 31 )]
        [DataRow( "August 1st, 2025", 2025, 8, 1 )]
        public void RecognizeSingleDate_WorksAsExpected( string query, int year, int month, int day )
        {
            var result = DateTimeRecognitionHelper.RecognizeDateRange( query, _referenceDate );

            Assert.IsNotNull( result, $"Expected a result for '{query}'." );
            Assert.IsNotNull( result.StartDate, $"Expected a date for '{query}'." );
            Assert.IsNull( result.EndDate, $"Did not expect an end date for '{query}'." );
            Assert.AreEqual( new DateTime( year, month, day ), result.StartDate.Value, $"Incorrect recognized date for '{query}'." );
        }

        #endregion

        #region Edge/Negative Cases

        [DataTestMethod]
        [DataRow( "march 14th to july 31st from 2024", 2025, 3, 14, 2025, 7, 31 )]
        public void RecognizeRange_FromKeywordDoesNotWork( string query, int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay )
        {
            var result = DateTimeRecognitionHelper.RecognizeDateRange( query, _referenceDate );

            AssertDateRangeNotNull( result );
            Assert.AreEqual( new DateTime( startYear, startMonth, startDay ), result.StartDate.Value, $"Unexpected: '{query}' should not set year." );
            Assert.AreEqual( new DateTime( endYear, endMonth, endDay ), result.EndDate.Value, $"Unexpected: '{query}' should not set year." );
        }

        #endregion

        #region Utility Methods

        private static void AssertDateRangeNotNull( DateRangeResult result )
        {
            Assert.IsNotNull( result, "Failed to determine result." );
            Assert.IsNotNull( result.StartDate, "Start date should not be null." );
            Assert.IsNotNull( result.EndDate, "End date should not be null." );
        }

        #endregion
    }
}
