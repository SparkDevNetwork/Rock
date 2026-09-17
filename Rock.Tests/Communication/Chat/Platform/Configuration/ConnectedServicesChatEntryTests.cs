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
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Configuration.ConnectedServices;

namespace Rock.Tests.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// Reading the church's credentials off the connected services entry the gateway
    /// answers an enable with.
    /// </summary>
    [TestClass]
    public class ConnectedServicesChatEntryTests
    {
        private const string ChurchPrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"church-kid-1\",\"d\":\"secret-part\"}";

        [TestMethod]
        public void FromEntry_WithEveryField_ReadsThePlatformHalfAndTheKey()
        {
            var tenantId = Guid.NewGuid();

            var entry = ConnectedServicesChatEntry.FromEntry( ServiceEntryFor( new
            {
                tenantId = tenantId.ToString(),
                projectUrl = "https://example.supabase.co",
                publishableKey = "sb_publishable_test",
                kid = "platform-kid-1",
                privateKey = ChurchPrivateKey
            } ) );

            Assert.IsNotNull( entry );
            Assert.AreEqual( tenantId, entry.TenantId );
            Assert.AreEqual( "https://example.supabase.co", entry.ProjectUrl );
            Assert.AreEqual( "sb_publishable_test", entry.PublishableKey );
            Assert.AreEqual( "platform-kid-1", entry.Kid );
            Assert.AreEqual( ChurchPrivateKey, entry.PrivateKey );
        }

        [TestMethod]
        public void FromEntry_MissingTheTenantId_IsRefusedWhole()
        {
            AssertRefused( Complete( tenantId: null ), "tenant id" );
        }

        [TestMethod]
        public void FromEntry_MissingTheProjectUrl_IsRefusedWhole()
        {
            AssertRefused( Complete( projectUrl: null ), "project url" );
        }

        [TestMethod]
        public void FromEntry_MissingThePublishableKey_IsRefusedWhole()
        {
            AssertRefused( Complete( publishableKey: null ), "publishable key" );
        }

        [TestMethod]
        public void FromEntry_MissingThePrivateKey_IsRefusedWhole()
        {
            AssertRefused( Complete( privateKey: null ), "private key" );
        }

        [TestMethod]
        public void FromEntry_WithAKeyThatIsNotAKey_IsRefusedWhole()
        {
            // A present but unusable key is worse than a missing one: the church reads as
            // set up, the card offers no second Enable, and every token fails.
            AssertRefused( Complete( privateKey: "not-a-jwk-at-all" ), "usable private key" );
        }

        [TestMethod]
        public void FromEntry_WithAPublicKeyWhereThePrivateOneShouldBe_IsRefusedWhole()
        {
            AssertRefused(
                Complete( privateKey: "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"church-kid-1\"}" ),
                "private half on the key" );
        }

        [TestMethod]
        public void FromEntry_WithNothingAtAll_IsRefusedRatherThanThrowing()
        {
            Assert.IsNull( ConnectedServicesChatEntry.FromEntry( null ) );
        }

        #region Helpers

        /// <summary>
        /// A church that is missing one of the four things it needs is no church at all:
        /// a partial write leaves a church that reads as enabled and cannot mint.
        /// </summary>
        private static void AssertRefused( object configuration, string missing )
        {
            Assert.IsNull(
                ConnectedServicesChatEntry.FromEntry( ServiceEntryFor( configuration ) ),
                "an entry with no " + missing + " was read as usable" );
        }

        private static object Complete( string tenantId = "unset", string projectUrl = "unset", string publishableKey = "unset", string privateKey = "unset" )
        {
            return new
            {
                tenantId = tenantId == "unset" ? Guid.NewGuid().ToString() : tenantId,
                projectUrl = projectUrl == "unset" ? "https://example.supabase.co" : projectUrl,
                publishableKey = publishableKey == "unset" ? "sb_publishable_test" : publishableKey,
                kid = "platform-kid-1",
                privateKey = privateKey == "unset" ? ChurchPrivateKey : privateKey
            };
        }

        /// <summary>
        /// The entry as it arrives: parsed from the gateway's own JSON with the
        /// provider's own options, so this test reads what production reads.
        /// </summary>
        private static ServiceEntry ServiceEntryFor( object configuration )
        {
            var json = new
            {
                serviceId = "chat",
                status = "Ok",
                configuration
            }.ToJson();

            return JsonSerializer.Deserialize<ServiceEntry>( json, ConnectedServicesProvider.JsonOptions );
        }

        #endregion Helpers
    }
}
