using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class FinancialPledgeTests : DatabaseTestsBase
    {
        private string financialPledgeForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            financialPledgeForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( financialPledgeForeignKey );
        }

        [TestMethod]
        public void FinancialPledgeDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var financialPledge = new Rock.Model.FinancialPledge();
                financialPledge.StartDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialPledge.StartDateKey );
            }

            testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var financialPledge = new Rock.Model.FinancialPledge();
                financialPledge.EndDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialPledge.EndDateKey );
            }
        }

        [TestMethod]
        public void FinancialPledgeDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var financialPledgeService = new FinancialPledgeService( rockContext );

            var financialPledge = BuildFinancialPledge( rockContext,
                                    Convert.ToDateTime( "2010-3-15" ),
                                    Convert.ToDateTime( "2010-3-16" ) );

            financialPledgeService.Add( financialPledge );
            rockContext.SaveChanges();

            var financialPledgeId = financialPledge.Id;

            // We're bypassing the model because the model doesn't user the FinancialPledgeDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT StartDateKey FROM FinancialPledge WHERE Id = {financialPledgeId}" ).First();
            Assert.AreEqual( 20100315, result );

            result = rockContext.Database.
                            SqlQuery<int>( $"SELECT EndDateKey FROM FinancialPledge WHERE Id = {financialPledgeId}" ).First();
            Assert.AreEqual( 20100316, result );
        }

        [TestMethod]
        public void FinancialPledgeDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            using ( var rockContext = new RockContext() )
            {
                var financialPledgeService = new FinancialPledgeService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {

                    var financialPledge = BuildFinancialPledge( rockContext,
                                            TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ),
                                            TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                    financialPledgeService.Add( financialPledge );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var financialPledgeService = new FinancialPledgeService( rockContext );
                var financialPledges = financialPledgeService.
                                Queryable().
                                Where( i => i.ForeignKey == financialPledgeForeignKey ).
                                Where( i => i.StartSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, financialPledges.Count() );
                Assert.IsNotNull( financialPledges.First().StartSourceDate );

                financialPledges = financialPledgeService.
                                    Queryable().
                                    Where( i => i.ForeignKey == financialPledgeForeignKey ).
                                    Where( i => i.EndSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, financialPledges.Count() );
                Assert.IsNotNull( financialPledges.First().EndSourceDate );
            }
        }

        private Rock.Model.FinancialPledge BuildFinancialPledge( RockContext rockContext, DateTime startDate, DateTime endDate )
        {
            var financialPledge = new Rock.Model.FinancialPledge();

            financialPledge.ForeignKey = financialPledgeForeignKey;
            financialPledge.StartDate = startDate;
            financialPledge.EndDate = endDate;

            return financialPledge;
        }

        private void CleanUpData( string financialPledgeForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [FinancialPledge] WHERE [ForeignKey] = '{financialPledgeForeignKey}'" );
        }
    }
}
