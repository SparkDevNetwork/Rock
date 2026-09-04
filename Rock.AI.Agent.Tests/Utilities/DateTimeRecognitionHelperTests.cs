using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities;

namespace Rock.AI.Agent.Tests.Utilities;

[TestClass]
public class DateTimeRecognitionHelperTests
{
    private readonly DateTime _referenceDate = new( 2025, 7, 30, 14, 41, 27 );

    #region Range Recognition

    [TestMethod]
    [DataRow( "this week", "2025-07-28", "2025-08-03T23:59:59.999" )]
    [DataRow( "last week", "2025-07-21", "2025-07-27T23:59:59.999" )]
    [DataRow( "last 3 months", "2025-04-30", "2025-07-30T23:59:59.999" )]
    [DataRow( "march 14th to july 31st", "2025-03-14", "2025-07-31T23:59:59.999" )]
    [DataRow( "march 14th to july 31st of 2024", "2024-03-14", "2024-07-31T23:59:59.999" )]
    [DataRow( "last two weeks", "2025-07-16", "2025-07-30T23:59:59.999" )]
    [DataRow( "year to date", "2025-01-01", "2025-07-29T23:59:59.999" )] // 2025-07-30T23:59:59.999 would be nice, but don't see how to do it.
    [DataRow( "last year", "2024-01-01", "2024-12-31T23:59:59.999" )]
    [DataRow( "first quarter", "2025-01-01", "2025-03-31T23:59:59.999" )]
    public void RecognizeRange_WorksAsExpected( string query, string expectedStart, string expectedEnd )
    {
        var result = DateTimeRecognitionHelper.RecognizeDateRange( query, _referenceDate );

        AssertDateRangeNotNull( result );
        Assert.AreEqual( DateTime.Parse( expectedStart ), result.StartDate.Value, "Incorrect start date." );
        Assert.AreEqual( DateTime.Parse( expectedEnd ), result.EndDate.Value, "Incorrect end date." );
    }

    #endregion

    #region Single Date Recognition

    [TestMethod]
    [DataRow( "tomorrow", 2025, 7, 31 )]
    [DataRow( "August 1st, 2025", 2025, 8, 1 )]
    public void RecognizeSingleDate_WorksAsExpected( string query, int year, int month, int day )
    {
        var result = DateTimeRecognitionHelper.RecognizeDateRange( query, _referenceDate );

        AssertDateRangeNotNull( result );
        Assert.AreEqual( new DateTime( year, month, day ), result.StartDate.Value, $"Incorrect start date for '{query}'." );
        Assert.AreEqual( new DateTime( year, month, day ).AddDays( 1 ).AddMilliseconds( -1 ), result.EndDate.Value, $"Incorrect end date for '{query}'." );
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
