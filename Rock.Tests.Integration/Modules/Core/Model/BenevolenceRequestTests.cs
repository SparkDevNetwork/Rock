using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class BenevolenceRequestTests : DatabaseTestsBase
    {
        private string benevolenceRequestForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            benevolenceRequestForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( benevolenceRequestForeignKey );
        }

        [TestMethod]
        public void BenevolenceRequestDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                BenevolenceRequest benevolenceRequest = new BenevolenceRequest();
                benevolenceRequest.RequestDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, benevolenceRequest.RequestDateKey );
            }
        }

        [TestMethod]
        public void BenevolenceRequestDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var benevolenceRequestService = new BenevolenceRequestService( rockContext );

            var benevolenceRequest = BuildBenevolenceRequest( rockContext, Convert.ToDateTime( "2010-3-15" ) );
            benevolenceRequestService.Add( benevolenceRequest );
            rockContext.SaveChanges();

            var benevolenceRequestId = benevolenceRequest.Id;

            // We're bypassing the model because the model doesn't user the BenevolenceRequestDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT RequestDateKey FROM BenevolenceRequest WHERE Id = {benevolenceRequestId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void BenevolenceRequestDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            using (var rockContext = new RockContext() )
            {
                var benevolenceRequestService = new BenevolenceRequestService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {

                    var benevolenceRequest = BuildBenevolenceRequest( rockContext,
                        TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                    benevolenceRequestService.Add( benevolenceRequest );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var benevolenceRequestService = new BenevolenceRequestService( rockContext );
                var benevolenceRequests = benevolenceRequestService.
                                Queryable().
                                Where( i => i.ForeignKey == benevolenceRequestForeignKey ).
                                Where( i => i.RequestSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, benevolenceRequests.Count() );
                Assert.IsNotNull( benevolenceRequests.First().RequestSourceDate );
            }
        }

        private BenevolenceRequest BuildBenevolenceRequest( RockContext rockContext, DateTime requestDate )
        {
            var requestStatuses = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) );
            var requestStatusValue = requestStatuses.DefinedValues.First().Id;
            var benevolenceType = new BenevolenceTypeService( rockContext ).Queryable().First();

            var benevolenceRequest = new BenevolenceRequest();

            benevolenceRequest.LastName = "Name";
            benevolenceRequest.FirstName = "Test";
            benevolenceRequest.RequestText = "Request Text";
            benevolenceRequest.ForeignKey = benevolenceRequestForeignKey;
            benevolenceRequest.RequestDateTime = requestDate;
            benevolenceRequest.RequestStatusValueId = requestStatusValue;
            benevolenceRequest.BenevolenceTypeId = benevolenceType.Id;

            return benevolenceRequest;
        }

        private void CleanUpData( string benevolenceRequestForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [BenevolenceRequest] WHERE [ForeignKey] = '{benevolenceRequestForeignKey}'" );
        }
    }
}
