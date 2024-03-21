using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class RegistrationTests : DatabaseTestsBase
    {
        private string registrationForiegnKey;

        [TestInitialize]
        public void TestInitialize()
        {
            registrationForiegnKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( registrationForiegnKey );
        }

        [TestMethod]
        public void RegistrationCreatedDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var registration = new Rock.Model.Registration();
                registration.CreatedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, registration.CreatedDateKey );
            }
        }

        [TestMethod]
        public void RegistrationCreatedDateKeyWorksWithNullValue()
        {
            var registration = new Rock.Model.Registration();
            registration.CreatedDateTime = null;
            Assert.IsNull( registration.CreatedDateKey );
        }

        [TestMethod]
        public void RegistrationDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var registrationService = new RegistrationService( rockContext );

            var registration = BuildRegistration( rockContext, Convert.ToDateTime( "2010-3-15" ) );
            registrationService.Add( registration );
            rockContext.SaveChanges();

            var registrationId = registration.Id;

            // We're bypassing the model because the model doesn't user the RegistrationDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT CreatedDateKey FROM Registration WHERE Id = {registrationId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void RegistrationDateKeySavesCorrectlyWhenNull()
        {
            var rockContext = new RockContext();
            var registrationService = new RegistrationService( rockContext );

            var registrationRequest = BuildRegistration( rockContext, null );
            registrationService.Add( registrationRequest );
            rockContext.SaveChanges();

            var connectionRequestId = registrationRequest.Id;

            // We're bypassing the model because the model doesn't user the ConnectionRequestDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT CreatedDateKey FROM Registration WHERE Id = {connectionRequestId}" ).First();

            Assert.AreEqual( Convert.ToInt32( RockDateTime.Now.ToString( "yyyyMMdd" ) ), result );
        }

        [TestMethod]
        public void RegistrationDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;

            using ( var rockContext = new RockContext() )
            {
                var registrationService = new RegistrationService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {

                    var registration = BuildRegistration( rockContext,
                        TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );

                    registrationService.Add( registration );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationService = new RegistrationService( rockContext );

                var registrations = registrationService.
                                Queryable().
                                Where( i => i.ForeignKey == registrationForiegnKey ).
                                Where( i => i.CreatedSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, registrations.Count() );
                Assert.IsNotNull( registrations.First().CreatedSourceDate );
            }
        }

        private Rock.Model.Registration BuildRegistration( RockContext rockContext, DateTime? requestDate )
        {
            var registrationInstance = new RegistrationInstanceService( rockContext ).Queryable().First();
            var registration = new Rock.Model.Registration();

            registration.RegistrationInstanceId = registrationInstance.Id;
            registration.ForeignKey = registrationForiegnKey;
            registration.CreatedDateTime = requestDate;

            return registration;
        }

        private void CleanUpData( string registrationForiegnKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE Registration WHERE [ForeignKey] = '{registrationForiegnKey}'" );
        }
    }
}
