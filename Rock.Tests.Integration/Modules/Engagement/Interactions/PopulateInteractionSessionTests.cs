// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.IpAddress;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Mocks;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Engagement.Interactions
{
    [TestClass]
    public class PopulateInteractionSessionTests : DatabaseTestsBase
    {
        #region Lookup IP Addresses

        [TestMethod]
        public void IpRegistryComponent_IpAddressHavingNoLocation_IsResolvedCorrectly()
        {
            var registryComponent = new IpRegistryMock();

            // Attempt to resolve a loopback address that has no location.
            var ipAddresses = new List<string> { "192.168.0.1" };

            try
            {
                var results = registryComponent.BulkLookup( ipAddresses, out var message );
            }
            catch ( Exception ex )
            {
                // Deserialization of Latitude/Longitude fails.
                Assert.Fail( ex.Message );
            }
        }

        /// <summary>
        /// Verifies that a request to check the account credit balance can be made without incurring any charges.
        /// </summary>
        [TestMethod]
        [Ignore("This test should only be enabled for development purposes, as it requires access to a third-party service.")]
        public void IpRegistryComponent_ServiceStatusRequest_DoesNotExpendCredit()
        {
            IpRegistryMock registryComponent;
            IpRegistryStatusInfo status1;
            IpRegistryStatusInfo status2;

            registryComponent = new IpRegistryMock()
            {
                ApiKey = IpRegistryMock.IpRegistryApiKeyHasPositiveCredit
            };

            // Request the service status twice, to verify that the available credits remain the same.
            // The request has been framed in a way that it does not itself consume any account credit,
            // but this could be impacted by future changes by the third-party service provider.
            status1 = registryComponent.GetServiceStatus();
            status2 = registryComponent.GetServiceStatus();

            Assert.IsTrue( status1.IsAvailable, "Service Status Request is invalid." );
            Assert.IsTrue( status1.AvailableCreditTotal == status2.AvailableCreditTotal, "Service Status Request incurred an unexpected charge." );

            // Verify that the status is returned correctly for an account with a zero balance.
            registryComponent = new IpRegistryMock()
            {
                ApiKey = IpRegistryMock.IpRegistryApiKeyHasZeroCredit
            };

            status1 = registryComponent.GetServiceStatus();
            Assert.IsTrue( status1.AvailableCreditTotal == 0, "Service Status Request returned an unexpected value." );
        }

        [TestMethod]
        public void IpRegistryComponent_InvalidIpAddress_ReturnsError()
        {
            var registryComponent = new IpRegistryMock();

            // Attempt to resolve a loopback address that has no location.
            var ipAddresses = new List<string> { "a.b.c.d" };

            try
            {
                var results = registryComponent.BulkLookup( ipAddresses, out var message );
            }
            catch ( Exception ex )
            {
                // Deserialization of Latitude/Longitude fails.
                Assert.Fail( ex.Message );
            }
        }

        /// <summary>
        /// If there is an issue with the IP Registry Provider, batch processing should terminate early.
        /// </summary>
        [TestMethod]
        public void IpRegistryComponent_WithValidAccount_ResolvesAddressesCorrectly()
        {
            var registryComponent = new IpRegistryMock();

            // Get a randomized collection of IP Addresses.
            var ipAddresses = new List<string>();
            var random = new Random();
            for ( int i = 0; i < 10; i++ )
            {
                ipAddresses.Add( $"{random.Next( 0, 255 )}.{random.Next( 0, 255 )}.{random.Next( 0, 255 )}.{random.Next( 0, 255 )}" );
            }

            var results = registryComponent.BulkLookup( ipAddresses, out var message );

            Assert.IsTrue( results.Count > 0 );
        }

        [TestMethod]
        public void IpRegistryComponent_ValidateSampleAddresses_IsValid()
        {
            var registryComponent = new IpRegistryMock();

            // Attempt to resolve a loopback address that has no location.
            var ipAddresses = "180.76.102.66,20.125.101.231,107.77.198.194,105.235.134.225,3.252.129.228"
                .Split( ',' )
                .ToList();

            try
            {
                var results = registryComponent.BulkLookup( ipAddresses, out var message );
            }
            catch ( Exception ex )
            {
                // Deserialization of Latitude/Longitude fails.
                Assert.Fail( ex.Message );
            }
        }

        #endregion

        #region Populate Interaction Sessions Job.

        private void AddInteractionSessionTestData( DateTime firstInteractionSessionDate )
        {
            var rockContext = new RockContext();
            var rnd = new Random();

            var agentList = TestDataHelper.Web.GetHttpUserAgentList();

            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            // Add interactions for a specific browser session.
            var internalSite = TestDataHelper.Crm.GetInternalSite( rockContext );
            var internalPages = TestDataHelper.Crm.GetInternalSitePages( rockContext );
            var interactionService = new InteractionService( rockContext );

            var interactionDateTime = firstInteractionSessionDate;

            var ipAddresses = "180.76.102.66,20.125.101.231,107.77.198.194,105.235.134.225,3.252.129.228"
                .Split( ',' )
                .ToList();

            foreach ( var ipAddress in ipAddresses )
            {
                var browserSessionGuid = Guid.NewGuid();
                var interactionCount = rnd.Next( 1, 10 );
                var interactionPages = internalPages.GetRandomizedList( interactionCount );

                foreach ( var testPage in interactionPages )
                {
                    var args = new TestDataHelper.Interactions.CreatePageViewInteractionActionArgs
                    {
                        ViewDateTime = interactionDateTime,
                        SiteIdentifier = internalSite.Id.ToString(),
                        PageIdentifier = testPage.Id.ToString(),
                        UserAgentString = agentList.GetRandomElement(),
                        BrowserIpAddress = ipAddress,
                        BrowserSessionGuid = browserSessionGuid,
                        RequestUrl = $"http://localhost:12345/page/{testPage.Id}",
                        UserPersonAliasId = personTedDecker.PrimaryAliasId
                    };

                    var interaction = TestDataHelper.Interactions.CreatePageViewInteraction( args, rockContext );

                    interaction.ForeignKey = "IntegrationTestData";

                    interactionService.Add( interaction );

                    rockContext.SaveChanges();

                    // Get the date/time for the next interaction.
                    interactionDateTime = interactionDateTime.AddMinutes( rnd.Next( 1, 10 ) );
                }
            }
        }

        /// <summary>
        /// If the IP Registry Provider is correctly configured, batch processing should complete successfully.
        /// </summary>
        [TestMethod]
        public void InteractionSessionPopulateLocation_WithConfiguredIpRegistryProvider_CompletesSuccessfully()
        {
            var registryComponent = new IpRegistryMock()
            {
                ApiKey = IpRegistryMock.IpRegistryApiKeyHasPositiveCredit
            };
            var job = new PopulateInteractionSessionDataMock()
            {
                LookupComponent = registryComponent
            };

            var settings = new PopulateInteractionSessionData.PopulateInteractionSessionDataJobSettings();
            settings.MaxRecordsToProcessPerRun = 10;

            var jobOutput = job.ProcessInteractionSessionForIP( settings );

            // Check for success badge.
            Assert.That.Contains( jobOutput, "<i class='fa fa-circle text-success'></i>" );
        }

        /// <summary>
        /// An interaction session recorded since the last successful run date shouild be processed.
        /// </summary>
        [TestMethod]
        [IsolatedTestDatabase]
        public void InteractionSessionPopulateLocation_HavingSessionsWithUnknownDurationPriorToStartDate_ProcessesThoseSessions()
        {
            var interactionSessionDate = RockDateTime.New( 2023, 3, 1, 10, 0, 0, 0 );
            AddInteractionSessionTestData( interactionSessionDate.Value );

            var registryComponent = new IpRegistryMock()
            {
                ApiKey = IpRegistryMock.IpRegistryApiKeyHasPositiveCredit
            };
            var job = new PopulateInteractionSessionDataMock()
            {
                LookupComponent = registryComponent
            };

            var settings = new PopulateInteractionSessionData.PopulateInteractionSessionDataJobSettings();
            settings.MaxRecordsToProcessPerRun = 10;
            settings.LastSuccessfulJobRunDateTime = interactionSessionDate.Value.AddHours( -1 );

            var jobResult = job.Execute( settings );

            Assert.That.IsNull( jobResult.Exception );
        }

        /// <summary>
        /// If there is a fatal issue with the IP Registry Provider, batch processing should terminate early.
        /// </summary>
        [TestMethod]
        public void InteractionSessionPopulateLocation_WithIpRegistryProviderAccountHavingNoCredit_TerminatesWithError()
        {
            var registryComponent = new IpRegistryMock()
            {
                ApiKey = IpRegistryMock.IpRegistryApiKeyHasZeroCredit
            };
            var job = new PopulateInteractionSessionDataMock()
            {
                LookupComponent = registryComponent
            };

            var settings = new PopulateInteractionSessionData.PopulateInteractionSessionDataJobSettings();
            settings.MaxRecordsToProcessPerRun = 10;

            var jobOutput = job.ProcessInteractionSessionForIP( settings );

            Assert.That.Contains( jobOutput, "Insufficient account credit to process the request" );
        }

        #endregion

        #region Support classes

        public class PopulateInteractionSessionDataMock : PopulateInteractionSessionData
        {
            public IpAddressLookupComponent LookupComponent { get; set; }

            internal override IpAddressLookupComponent GetLookupComponent( string configuredProvider )
            {
                return LookupComponent;
            }
        }

        #endregion
    }
}
