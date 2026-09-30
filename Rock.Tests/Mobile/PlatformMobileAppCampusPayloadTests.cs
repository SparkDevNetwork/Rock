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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Mobile;

namespace Rock.Tests.Mobile
{
    [TestClass]
    public class PlatformMobileAppCampusPayloadTests
    {
        [TestMethod]
        public void Serialize_UsesDirectoryFieldNamesInOrder()
        {
            var campuses = new List<PlatformMobileAppCampus>
            {
                new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe", State = "AZ", Latitude = 33.4255, Longitude = -111.94 }
            };

            var json = PlatformMobileAppCampusPayload.Serialize( campuses );

            Assert.AreEqual( "[{\"name\":\"Tempe\",\"city\":\"Tempe\",\"state\":\"AZ\",\"lat\":33.4255,\"long\":-111.94}]", json );
        }

        [TestMethod]
        public void Serialize_OmitsMissingFields()
        {
            var campuses = new List<PlatformMobileAppCampus>
            {
                new PlatformMobileAppCampus { Name = "Online" }
            };

            var json = PlatformMobileAppCampusPayload.Serialize( campuses );

            Assert.AreEqual( "[{\"name\":\"Online\"}]", json );
        }

        [TestMethod]
        public void ComputeHash_SameCampuses_SameHash()
        {
            var first = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe" } };
            var second = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe" } };

            Assert.AreEqual( PlatformMobileAppCampusPayload.ComputeHash( first ), PlatformMobileAppCampusPayload.ComputeHash( second ) );
        }

        [TestMethod]
        public void ComputeHash_ChangedSentField_DifferentHash()
        {
            var before = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Tempe" } };
            var after = new List<PlatformMobileAppCampus> { new PlatformMobileAppCampus { Name = "Tempe", City = "Chandler" } };

            Assert.AreNotEqual( PlatformMobileAppCampusPayload.ComputeHash( before ), PlatformMobileAppCampusPayload.ComputeHash( after ) );
        }

        [TestMethod]
        public void ComputeHash_IsLowercaseHexSha256()
        {
            var hash = PlatformMobileAppCampusPayload.ComputeHash( new List<PlatformMobileAppCampus>() );

            Assert.AreEqual( 64, hash.Length );
            Assert.AreEqual( hash.ToLowerInvariant(), hash );
        }
    }
}
