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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Enums.Configuration;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// Enabling chat through the connected services gateway: the call it makes, what
    /// it refuses to do without a gateway key, and what it is not allowed to leave
    /// behind in the connected services settings.
    /// </summary>
    [TestClass]
    public class ChatEnableProviderTests
    {
        #region Constants

        private const string GatewayKey = "gateway-key-for-this-organization";

        private const string EnabledPath = "/svcs/v1/chat/enabled";

        private const string ChurchPrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"church-kid-1\",\"d\":\"secret-part\"}";

        private const string ProjectUrl = "https://example.supabase.co";

        private const string PublishableKey = "sb_publishable_test";

        /// <summary>
        /// Mirrors the address the provider has always used. Written out here rather
        /// than read from the provider so that changing the shipped default fails a
        /// test instead of quietly moving every installation.
        /// </summary>
        private const string ShippedGatewayAddress = "https://apigateway.rockrms.com/";

        #endregion Constants

        #region The call Enable makes

        [TestMethod]
        public async Task EnableChat_PostsToTheSharedEnabledRoute_UnderTheGatewayKey()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKeys();
                SeedGatewayKey( GatewayKey );

                var handler = new RecordingMessageHandler();
                handler.SetResponse( HttpMethod.Post, EnabledPath, HttpStatusCode.OK, EnableResponse() );

                var result = await ProviderFor( handler ).EnableChatAsync( CancellationToken.None );

                Assert.IsTrue( result.IsSuccess, result.ErrorMessage );

                var request = handler.Requests.Single();

                Assert.AreEqual( HttpMethod.Post, request.Method );
                Assert.AreEqual( EnabledPath, request.Path );
                Assert.AreEqual( GatewayKey, request.Headers["X-Gateway-Api-Key"].First() );
                Assert.IsTrue( request.Body.Contains( "\"enabled\":true" ), "the enable request did not ask for enabled: " + request.Body );
            }
        }

        [TestMethod]
        public async Task EnableChat_CarriesTheCredentialsBackOnTheServiceEntry()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKeys();
                SeedGatewayKey( GatewayKey );
                var tenantId = Guid.NewGuid();

                var handler = new RecordingMessageHandler();
                handler.SetResponse( HttpMethod.Post, EnabledPath, HttpStatusCode.OK, EnableResponse( tenantId ) );

                var result = await ProviderFor( handler ).EnableChatAsync( CancellationToken.None );

                Assert.IsTrue( result.IsSuccess, result.ErrorMessage );
                Assert.IsNotNull( result.Data, "enable returned no service entry" );
                Assert.AreEqual( "chat", result.Data.ServiceId );

                var entry = ConnectedServicesChatEntry.FromEntry( result.Data );

                Assert.IsNotNull( entry, "the entry the gateway answered with was not usable" );
                Assert.AreEqual( tenantId, entry.TenantId );
                Assert.AreEqual( ProjectUrl, entry.ProjectUrl );
                Assert.AreEqual( PublishableKey, entry.PublishableKey );
                Assert.AreEqual( "platform-kid-1", entry.Kid );
                Assert.AreEqual( ChurchPrivateKey, entry.PrivateKey );
            }
        }

        [TestMethod]
        public async Task EnableChat_ThenStoring_LeavesTheChurchReadyToMintWithNoReadableKey()
        {
            // The whole seam in one test: what the gateway answers, read into credentials,
            // stored, and read back. Each half is covered on its own elsewhere; what this
            // adds is that they fit together, which is the only thing the block does.
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKeys();
                SeedGatewayKey( GatewayKey );
                var tenantId = Guid.NewGuid();

                var handler = new RecordingMessageHandler();
                handler.SetResponse( HttpMethod.Post, EnabledPath, HttpStatusCode.OK, EnableResponse( tenantId ) );

                var result = await ProviderFor( handler ).EnableChatAsync( CancellationToken.None );
                ChatPlatformConfigurationService.SavePlatformCredentials( ConnectedServicesChatEntry.FromEntry( result.Data ) );
                RockCache.ClearAllCachedItems( false );

                var stored = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );
                Assert.IsFalse( stored.Contains( "secret-part" ), "the stored settings carry the key in the clear" );

                var configuration = ChatPlatformConfigurationService.Read();
                Assert.IsTrue( configuration.IsConfigured, "the church is not ready to mint after enabling" );
                Assert.AreEqual( tenantId, configuration.TenantId );
                Assert.AreEqual( ChurchPrivateKey, configuration.PrivateKey );
            }
        }

        [TestMethod]
        public async Task EnableChat_WithNoGatewayKey_RefusesBeforeAnyRequest()
        {
            // Every provider method refuses outright when the organization has not been
            // linked, so a church reaches Enable Chat only through Spark. Nothing about
            // chat changes that, and nothing about chat may reach the network first.
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKeys();

                var handler = new RecordingMessageHandler();
                handler.SetResponse( HttpMethod.Post, EnabledPath, HttpStatusCode.OK, EnableResponse() );
                var provider = ProviderFor( handler );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () => provider.EnableChatAsync( CancellationToken.None ) );

                Assert.AreEqual( 0, handler.Requests.Count, "enable reached the network without a gateway key" );
            }
        }

        [TestMethod]
        public async Task EnableChat_LeavesNoChatEntryAndNoPrivateKeyInTheConnectedServicesSettings()
        {
            // Rock IQ and Knowledge Base both store their service configuration in the
            // connected services value, which is written whole and in the clear. The
            // church signing key must never be in there, so chat stores nothing at all.
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKeys();
                SeedGatewayKey( GatewayKey );

                var handler = new RecordingMessageHandler();
                handler.SetResponse( HttpMethod.Post, EnabledPath, HttpStatusCode.OK, EnableResponse() );

                await ProviderFor( handler ).EnableChatAsync( CancellationToken.None );

                RockCache.ClearAllCachedItems( false );
                var stored = SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION );

                Assert.IsFalse( stored.Contains( "secret-part" ), "the connected services settings carry the church signing key" );
                Assert.IsFalse( stored.IndexOf( "chat", StringComparison.OrdinalIgnoreCase ) >= 0, "the connected services settings carry a chat entry: " + stored );
            }
        }

        #endregion The call Enable makes

        #region Where the gateway lives

        [TestMethod]
        public void GatewayAddress_ComesFromTheInitializationSettings()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var settings = ( InitializationSettings ) RockApp.Current.GetRequiredService<IInitializationSettings>();
                settings.ConnectedServicesApiUrl = "http://localhost:7788/";

                var provider = new ConnectedServicesProvider( settings );

                Assert.AreEqual( new Uri( "http://localhost:7788/" ), provider.GatewayAddress );
            }
        }

        [TestMethod]
        public void GatewayAddress_WhenTheSettingIsBlank_IsTheShippedAddress()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var settings = ( InitializationSettings ) RockApp.Current.GetRequiredService<IInitializationSettings>();
                settings.ConnectedServicesApiUrl = string.Empty;

                var provider = new ConnectedServicesProvider( settings );

                Assert.AreEqual( new Uri( ShippedGatewayAddress ), provider.GatewayAddress );
            }
        }

        #endregion Where the gateway lives

        #region Helpers

        /// <summary>
        /// A provider whose HTTP calls land in <paramref name="handler"/> rather than on
        /// a network. The base address is irrelevant to the handler, which matches on the
        /// request path alone.
        /// </summary>
        private static ConnectedServicesProvider ProviderFor( RecordingMessageHandler handler )
        {
            var httpClient = new HttpClient( handler )
            {
                BaseAddress = new Uri( "http://test.local/" )
            };

            return new ConnectedServicesProvider( httpClient, DeploymentEnvironment.Production );
        }

        /// <summary>
        /// What the gateway answers an enable with: the service entry, carrying the
        /// church's credentials on its configuration the way Knowledge Base carries
        /// its API key.
        /// </summary>
        private static string EnableResponse( Guid? tenantId = null )
        {
            return new
            {
                enabled = true,
                newlyProvisioned = true,
                serviceEntry = new
                {
                    serviceId = "chat",
                    status = "Ok",
                    configuration = new
                    {
                        tenantId = ( tenantId ?? Guid.NewGuid() ).ToString(),
                        projectUrl = ProjectUrl,
                        publishableKey = PublishableKey,
                        kid = "platform-kid-1",
                        privateKey = ChurchPrivateKey
                    }
                }
            }.ToJson();
        }

        /// <summary>
        /// Writes the gateway key where the provider reads it from, which is the same
        /// value a completed organization link leaves behind.
        /// </summary>
        private static void SeedGatewayKey( string key )
        {
            SystemSettings.SetValue(
                SystemSetting.CONNECTED_SERVICES_CONFIGURATION,
                "{\"authToken\":\"" + key + "\"}" );

            SystemSettings.Remove();
        }

        /// <summary>
        /// Inserts the placeholder Attribute rows the settings are stored in. Without
        /// them the first write takes the insert branch, which resolves a field type the
        /// mock context does not have.
        /// </summary>
        private static void PrimeSettingKeys()
        {
            var rockContext = RockApp.Current.CreateRockContext();
            var attributes = rockContext.Set<Rock.Model.Attribute>();
            var nextId = 1;

            foreach ( var key in new[] { SystemSetting.CONNECTED_SERVICES_CONFIGURATION, SystemSetting.CONNECTED_SERVICES_MANIFEST, SystemSetting.CHAT_PLATFORM_CONFIGURATION } )
            {
                attributes.Add( new Rock.Model.Attribute
                {
                    Id = nextId++,
                    EntityTypeId = null,
                    EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER,
                    EntityTypeQualifierValue = string.Empty,
                    Key = key,
                    Name = key.SplitCase(),
                    DefaultValue = string.Empty,
                    Guid = Guid.NewGuid(),
                    Categories = new List<Category>(),
                    AttributeQualifiers = new List<AttributeQualifier>()
                } );
            }

            SystemSettings.Remove();
        }

        #endregion Helpers
    }
}
