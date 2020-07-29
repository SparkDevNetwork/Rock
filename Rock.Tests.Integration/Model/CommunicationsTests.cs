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
    public class CommunicationTests
    {
        private string communicationForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            communicationForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( communicationForeignKey );
        }

        [TestMethod]
        public void CommunicationSendDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var communication = new Rock.Model.Communication();
                communication.SendDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, communication.SendDateKey );
            }
        }

        [TestMethod]
        public void ConnectionRequestSendDateKeyWorksWithNullValue()
        {
            var communication = new Rock.Model.Communication();
            communication.SendDateTime = null;
            Assert.IsNull( communication.SendDateKey );
        }

        [TestMethod]
        public void CommunicationDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            var communication = BuildCommunication( rockContext, Convert.ToDateTime( "3/15/2010" ) );
            communicationService.Add( communication );
            rockContext.SaveChanges();

            var communicationId = communication.Id;

            // We're bypassing the model because the model doesn't user the CommunicationDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT SendDateKey FROM Communication WHERE Id = {communicationId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void CommunicationDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
            var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

            for ( var i = 0; i < 15; i++ )
            {

                var communication = BuildCommunication( rockContext,
                    TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                communicationService.Add( communication );
            }

            rockContext.SaveChanges();

            var communications = communicationService.
                                Queryable( "AnalyticsSourceDate" ).
                                Where( i => i.ForeignKey == communicationForeignKey ).
                                Where( i => i.SendSourceDate.CalendarYear == year );

            Assert.AreEqual( expectedRecordCount, communications.Count() );
        }

        private Rock.Model.Communication BuildCommunication( RockContext rockContext, DateTime requestDate )
        {
            var communication = new Rock.Model.Communication();

            communication.ForeignKey = communicationForeignKey;
            communication.SendDateTime = requestDate;

            return communication;
        }

        private void CleanUpData( string communicationForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE Communication WHERE [ForeignKey] = '{communicationForeignKey}'" );
        }
    }
}
