using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class CommunicationTests : DatabaseTestsBase
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

            var communication = BuildCommunication( rockContext, Convert.ToDateTime( "2010-3-15" ) );
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
            using ( var rockContext = new RockContext() )
            {
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
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var communications = communicationService.
                                Queryable().
                                Where( i => i.ForeignKey == communicationForeignKey ).
                                Where( i => i.SendSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, communications.Count() );
                Assert.IsNotNull( communications.First().SendSourceDate );
            }
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
