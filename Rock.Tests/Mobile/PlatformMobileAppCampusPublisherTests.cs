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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;
using Rock.Mobile;

namespace Rock.Tests.Mobile
{
    [TestClass]
    public class PlatformMobileAppCampusPublisherTests
    {
        #region IsPublishNeeded

        [TestMethod]
        public void IsPublishNeeded_NothingStamped_ReturnsTrue()
        {
            Assert.IsTrue( PlatformMobileAppCampusPublisher.IsPublishNeeded( null, "abc" ) );
            Assert.IsTrue( PlatformMobileAppCampusPublisher.IsPublishNeeded( "", "abc" ) );
        }

        [TestMethod]
        public void IsPublishNeeded_SameHash_ReturnsFalse()
        {
            var campuses = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe", State = "AZ" } };
            var hash = PlatformMobileAppCampusPayload.ComputeHash( campuses );

            Assert.IsFalse( PlatformMobileAppCampusPublisher.IsPublishNeeded( hash, hash ) );
            Assert.IsFalse( PlatformMobileAppCampusPublisher.IsPublishNeeded( hash.ToUpperInvariant(), hash ) );
        }

        [TestMethod]
        public void IsPublishNeeded_CoordinatesArrived_ReturnsTrue()
        {
            var before = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe", State = "AZ" } };
            var after = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe", State = "AZ", Latitude = 33.4255, Longitude = -111.94 } };

            var stamped = PlatformMobileAppCampusPayload.ComputeHash( before );
            var current = PlatformMobileAppCampusPayload.ComputeHash( after );

            Assert.IsTrue( PlatformMobileAppCampusPublisher.IsPublishNeeded( stamped, current ) );
        }

        #endregion IsPublishNeeded

        #region BuildRequest

        [TestMethod]
        public void BuildRequest_CarriesExactlyTheHashedArray()
        {
            var campuses = new List<PlatformMobileAppCampus>
            {
                new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe", State = "AZ", Latitude = 33.4255, Longitude = -111.94 },
                new PlatformMobileAppCampus { Name = "Online" }
            };

            var request = PlatformMobileAppCampusPublisher.BuildRequest( campuses );

            Assert.AreEqual( JsonValueKind.Array, request.Campuses.ValueKind );
            Assert.AreEqual( PlatformMobileAppCampusPayload.Serialize( campuses ), request.Campuses.GetRawText() );
        }

        [TestMethod]
        public void BuildRequest_NoCampuses_SendsEmptyArray()
        {
            var request = PlatformMobileAppCampusPublisher.BuildRequest( new List<PlatformMobileAppCampus>() );

            Assert.AreEqual( "[]", request.Campuses.GetRawText() );
        }

        #endregion BuildRequest

        #region GetPublishedMessage

        [TestMethod]
        public void GetPublishedMessage_UsesDirectoryCounts()
        {
            var campuses = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe" } };
            var response = new MobileAppCampusesResponse { CampusCount = 3, GeocodedCount = 2 };

            var message = PlatformMobileAppCampusPublisher.GetPublishedMessage( campuses, response );

            Assert.AreEqual( "Published 3 campuses to the church directory, 2 with coordinates.", message );
        }

        [TestMethod]
        public void GetPublishedMessage_NoResponse_CountsWhatWasSent()
        {
            var campuses = new List<PlatformMobileAppCampus>
            {
                new PlatformMobileAppCampus { Name = "Tempe", Latitude = 33.4255, Longitude = -111.94 }
            };

            var message = PlatformMobileAppCampusPublisher.GetPublishedMessage( campuses, null );

            Assert.AreEqual( "Published 1 campus to the church directory, 1 with coordinates.", message );
        }

        #endregion GetPublishedMessage

        #region FakeMobileAppGateway

        [TestMethod]
        public async Task FakeMobileAppGateway_SetCampuses_CountsCampusesAndCoordinates()
        {
            var gateway = new FakeMobileAppGateway( new Guid( "3F2C0D7E-8B1A-4C55-9E6F-1A2B3C4D5E6F" ) );
            var request = PlatformMobileAppCampusPublisher.BuildRequest( new List<PlatformMobileAppCampus>
            {
                new PlatformMobileAppCampus { Name = "Tempe", Latitude = 33.4255, Longitude = -111.94 },
                new PlatformMobileAppCampus { Name = "Chandler", City = "Chandler", State = "AZ" }
            } );

            var result = await gateway.SetCampusesAsync( request, CancellationToken.None );

            Assert.IsTrue( result.IsSuccess );
            Assert.AreEqual( 2, result.Data.CampusCount );
            Assert.AreEqual( 1, result.Data.GeocodedCount );
        }

        #endregion FakeMobileAppGateway
    }
}
