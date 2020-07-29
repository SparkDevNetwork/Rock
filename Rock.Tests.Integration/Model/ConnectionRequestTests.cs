using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class ConnectionRequestTests
    {
        private string connectionRequestForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            connectionRequestForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( connectionRequestForeignKey );
        }

        [TestMethod]
        public void ConnectionRequestCreatedDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                ConnectionRequest connectionRequest = new ConnectionRequest();
                connectionRequest.CreatedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, connectionRequest.CreatedDateKey );
            }
        }

        [TestMethod]
        public void ConnectionRequestCreatedDateKeyWorksWithNullValue()
        {
            ConnectionRequest connectionRequest = new ConnectionRequest();
            connectionRequest.CreatedDateTime = null;
            Assert.IsNull( connectionRequest.CreatedDateKey );
        }

        [TestMethod]
        public void ConnectionRequestDateKeySavesCorrectlyWhenNull()
        {
            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var connectionRequest = BuildConnectionRequest( rockContext, null );
            connectionRequestService.Add( connectionRequest );
            rockContext.SaveChanges();

            var connectionRequestId = connectionRequest.Id;

            // We're bypassing the model because the model doesn't user the ConnectionRequestDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT CreatedDateKey FROM ConnectionRequest WHERE Id = {connectionRequestId}" ).First();

            Assert.AreEqual( Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")), result );
        }

        [TestMethod]
        public void ConnectionRequestDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var connectionRequest = BuildConnectionRequest( rockContext, Convert.ToDateTime( "3/15/2010" ) );
            connectionRequestService.Add( connectionRequest );
            rockContext.SaveChanges();

            var connectionRequestId = connectionRequest.Id;

            // We're bypassing the model because the model doesn't user the ConnectionRequestDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT CreatedDateKey FROM ConnectionRequest WHERE Id = {connectionRequestId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void ConnectionRequestDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
            var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

            for ( var i = 0; i < 15; i++ )
            {

                var connectionRequest = BuildConnectionRequest( rockContext,
                    TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                connectionRequestService.Add( connectionRequest );
            }

            rockContext.SaveChanges();

            var connectionRequests = connectionRequestService.
                                Queryable( "AnalyticsSourceDate" ).
                                Where( i => i.ForeignKey == connectionRequestForeignKey ).
                                Where( i => i.CreatedSourceDate.CalendarYear == year );

            Assert.AreEqual( expectedRecordCount, connectionRequests.Count() );
        }

        private ConnectionRequest BuildConnectionRequest( RockContext rockContext, DateTime? createdDate )
        {
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Queryable().First();
            var personAlias = new PersonAliasService( rockContext ).Queryable().First();
            var connectionStatus = new ConnectionStatusService( rockContext ).Queryable().First();

            var connectionRequest = new ConnectionRequest();

            connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
            connectionRequest.PersonAliasId = personAlias.Id;
            connectionRequest.ConnectionStatusId = connectionStatus.Id;
            connectionRequest.ForeignKey = connectionRequestForeignKey;
            connectionRequest.CreatedDateTime = createdDate;

            return connectionRequest;
        }

        private void CleanUpData( string connectionRequestForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [ConnectionRequest] WHERE [ForeignKey] = '{connectionRequestForeignKey}'" );
        }
    }
}
