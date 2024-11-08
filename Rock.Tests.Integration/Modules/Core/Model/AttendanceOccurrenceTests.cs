using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class AttendanceOccurrenceTests : DatabaseTestsBase
    {
        private string attendanceOccurrenceForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            attendanceOccurrenceForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( attendanceOccurrenceForeignKey );
        }

        [TestMethod]
        public void AttendanceOccurrenceDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                AttendanceOccurrence attendanceOccurrence = new AttendanceOccurrence();
                attendanceOccurrence.OccurrenceDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, attendanceOccurrence.OccurrenceDateKey );
            }
        }

        [TestMethod]
        public void AttendanceOccurrenceDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            var attendanceOccurrence = BuildAttendanceOccurrence( rockContext, Convert.ToDateTime( "2010-3-15" ) );
            attendanceOccurrenceService.Add( attendanceOccurrence );
            rockContext.SaveChanges();

            var attendanceOccurrenceId = attendanceOccurrence.Id;

            // We're bypassing the model because the model doesn't user the AttendanceOccurrenceDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT OccurrenceDateKey FROM AttendanceOccurrence WHERE Id = {attendanceOccurrenceId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void AttendanceOccurrenceDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2016;
            using ( var rockContext = new RockContext() )
            {
                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                var dates = new List<DateTime>();
                DateTime GetUniqueDate()
                {
                    var date = TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue );
                    while ( dates.Contains( date ) )
                    {
                        date = TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue );
                    }
                    dates.Add( date );
                    return date;
                }

                for ( var i = 0; i < 15; i++ )
                {
                    var attendanceOccurrence = BuildAttendanceOccurrence( rockContext, GetUniqueDate() );
                    attendanceOccurrenceService.Add( attendanceOccurrence );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                var attendanceOccurrences = attendanceOccurrenceService.
                                    Queryable().
                                    Where( i => i.ForeignKey == attendanceOccurrenceForeignKey ).
                                    Where( i => i.OccurrenceSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, attendanceOccurrences.Count() );
                var actualAttendanceOccurrences = attendanceOccurrences.First();
                var actualOccurrenceSourceDate = actualAttendanceOccurrences.OccurrenceSourceDate;
                Assert.IsNotNull( actualOccurrenceSourceDate );
            }
        }

        private void CleanUpData( string foreignKey )
        {
            var rockContext = new RockContext();

            rockContext.Database.ExecuteSqlCommand( $"DELETE [AttendanceOccurrence] WHERE [ForeignKey] = '{foreignKey}'" );
        }

        private AttendanceOccurrence BuildAttendanceOccurrence( RockContext rockContext, DateTime occurrenceDate )
        {
            var AttendanceOccurrence = new AttendanceOccurrence();

            AttendanceOccurrence attendanceOccurrence = new AttendanceOccurrence();
            attendanceOccurrence.ForeignKey = attendanceOccurrenceForeignKey;
            attendanceOccurrence.OccurrenceDate = occurrenceDate;

            return attendanceOccurrence;
        }
    }
}
