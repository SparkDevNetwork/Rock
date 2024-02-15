using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class MetricValueTests : DatabaseTestsBase
    {
        private string metricValueForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            metricValueForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( metricValueForeignKey );
        }

        [TestMethod]
        public void MetricValueDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var metricValue = new Rock.Model.MetricValue();
                metricValue.MetricValueDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, metricValue.MetricValueDateKey );
            }
        }

        [TestMethod]
        public void MetricValueDateKeyKeyWorksWithNullValue()
        {
            var metricValue = new Rock.Model.MetricValue();
            metricValue.MetricValueDateTime = null;
            Assert.IsNull( metricValue.MetricValueDateKey );
        }

        [TestMethod]
        public void MetricValueDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var metricValueService = new MetricValueService( rockContext );

            var metricValue = BuildMetricValue( rockContext, Convert.ToDateTime( "2010-3-15" ) );
            metricValueService.Add( metricValue );
            rockContext.SaveChanges();

            var metricValueId = metricValue.Id;

            // We're bypassing the model because the model doesn't user the MetricValueDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT MetricValueDateKey FROM MetricValue WHERE Id = {metricValueId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void MetricValueDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;

            using ( var rockContext = new RockContext() )
            {
                var metricValueService = new MetricValueService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {

                    var metricValue = BuildMetricValue( rockContext,
                        TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                    metricValueService.Add( metricValue );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var metricValueService = new MetricValueService( rockContext );

                var metricValues = metricValueService.
                                Queryable().
                                Where( i => i.ForeignKey == metricValueForeignKey ).
                                Where( i => i.MetricValueSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, metricValues.Count() );
                Assert.IsNotNull( metricValues.First().MetricValueSourceDate );
            }
        }

        private Rock.Model.MetricValue BuildMetricValue( RockContext rockContext, DateTime requestDate )
        {
            var metric = new MetricService( rockContext ).Queryable().First();
            var metricValue = new Rock.Model.MetricValue();

            metricValue.ForeignKey = metricValueForeignKey;
            metricValue.MetricValueDateTime = requestDate;
            metricValue.MetricId = metric.Id;

            return metricValue;
        }

        private void CleanUpData( string metricValueForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [MetricValue] WHERE [ForeignKey] = '{metricValueForeignKey}'" );
        }
    }
}
