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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// The church's chat settings: how they are stored, what is encrypted, when they
    /// are complete enough for chat to run, and which writer owns which field.
    /// </summary>
    [TestClass]
    public class ChatPlatformConfigurationTests
    {
        private const string PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}";
        private const string Undecryptable = "not-something-this-installation-can-decrypt";

        #region Round trip

        [TestMethod]
        public void SaveBothHalves_ThenRead_RoundTripsEverySetting()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                var badgeOne = Guid.NewGuid();
                var badgeTwo = Guid.NewGuid();
                var dmAccess = Guid.NewGuid();
                var tenantId = Guid.NewGuid();

                SaveChurchSettings( new ChatPlatformConfiguration
                {
                    AreChatProfilesVisible = true,
                    IsOpenDirectMessagingAllowed = true,
                    MinimumAge = 13,
                    DirectMessageAccessDataViewGuid = dmAccess,
                    ChatBadgeDataViewGuids = new List<Guid> { badgeOne, badgeTwo }
                } );

                SavePlatformCredentials( Credentials( tenantId ) );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsTrue( read.AreChatProfilesVisible );
                Assert.IsTrue( read.IsOpenDirectMessagingAllowed );
                Assert.AreEqual( 13, read.MinimumAge );
                Assert.AreEqual( dmAccess, read.DirectMessageAccessDataViewGuid );
                CollectionAssert.AreEqual( new List<Guid> { badgeOne, badgeTwo }, read.ChatBadgeDataViewGuids );
                Assert.AreEqual( tenantId, read.TenantId );
                Assert.AreEqual( "https://example.supabase.co", read.ProjectUrl );
                Assert.AreEqual( "sb_publishable_test", read.PublishableKey );
                Assert.AreEqual( "platform-kid-1", read.Kid );
                Assert.AreEqual( PrivateKey, read.PrivateKey );
            }
        }

        [TestMethod]
        public void SavePlatformCredentials_StoresNoReadablePrivateKey_AndReadReturnsItDecrypted()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                SavePlatformCredentials( Credentials() );

                var stored = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

                Assert.IsFalse( stored.Contains( "secret-part" ), "the stored settings carry the key in the clear" );
                Assert.AreEqual( PrivateKey, ChatPlatformConfigurationService.Read().PrivateKey );
            }
        }

        [TestMethod]
        public void Read_WhenTheStoredKeyCannotBeDecrypted_ReportsNoKey()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                // A database restored onto an installation with a different encryption key
                // decrypts to nothing. The church genuinely cannot sign anything, so reading
                // the key as absent is right.
                SeedStored( new ChatPlatformConfiguration
                {
                    PrivateKey = Undecryptable,
                    TenantId = Guid.NewGuid(),
                    ProjectUrl = "https://example.supabase.co",
                    PublishableKey = "sb_publishable_test"
                } );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsNull( read.PrivateKey );
                Assert.IsFalse( read.IsConfigured );
            }
        }

        [TestMethod]
        public void Read_WhenNothingWasEverSaved_IsNotConfiguredAndDoesNotThrow()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsNotNull( read );
                Assert.IsFalse( read.IsConfigured );
            }
        }

        [TestMethod]
        public void Stored_CarriesNothingThatIsNotStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                SavePlatformCredentials( Credentials() );

                var json = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

                Assert.IsFalse( json.Contains( "IsConfigured" ) );
                Assert.IsFalse( json.Contains( "HasBeenEnabled" ) );
            }
        }

        #endregion Round trip

        #region Who owns which field

        [TestMethod]
        public void SaveChurchSettings_LeavesThePlatformHalfAndTheKeyExactlyAsStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                var tenantId = Guid.NewGuid();
                SavePlatformCredentials( Credentials( tenantId ) );
                var storedKey = StoredForm().PrivateKey;

                // The screen has no business writing any of this, so it is given a
                // configuration that names all of it wrongly and must ignore every field.
                SaveChurchSettings( new ChatPlatformConfiguration
                {
                    MinimumAge = 16,
                    TenantId = Guid.NewGuid(),
                    ProjectUrl = "https://attacker.example",
                    PublishableKey = "sb_publishable_attacker",
                    Kid = "attacker-kid",
                    PrivateKey = "{\"kty\":\"EC\",\"d\":\"attacker-part\"}"
                } );

                var read = ChatPlatformConfigurationService.Read();

                Assert.AreEqual( 16, read.MinimumAge );
                Assert.AreEqual( tenantId, read.TenantId );
                Assert.AreEqual( "https://example.supabase.co", read.ProjectUrl );
                Assert.AreEqual( "sb_publishable_test", read.PublishableKey );
                Assert.AreEqual( "platform-kid-1", read.Kid );
                Assert.AreEqual( PrivateKey, read.PrivateKey );
                Assert.AreEqual( storedKey, StoredForm().PrivateKey, "the church half's writer re-encrypted the signing key" );
            }
        }

        [TestMethod]
        public void SaveChurchSettings_WhenTheStoredKeyCannotBeDecrypted_LeavesItByteIdentical()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                // On a database restored onto an installation with a different encryption
                // key the settings screen reads no key at all. It must still not be the
                // thing that takes the church's signing key away.
                SeedStored( new ChatPlatformConfiguration
                {
                    PrivateKey = Undecryptable,
                    TenantId = Guid.NewGuid(),
                    ProjectUrl = "https://example.supabase.co",
                    PublishableKey = "sb_publishable_test"
                } );

                SaveChurchSettings( ChatPlatformConfigurationService.Read() );

                Assert.AreEqual( Undecryptable, StoredForm().PrivateKey, "saving from the configuration screen destroyed the stored signing key" );
            }
        }

        [TestMethod]
        public void SavePlatformCredentials_LeavesTheChurchHalfExactlyAsStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                var dmAccess = Guid.NewGuid();
                var badge = Guid.NewGuid();

                SaveChurchSettings( new ChatPlatformConfiguration
                {
                    AreChatProfilesVisible = true,
                    IsOpenDirectMessagingAllowed = true,
                    MinimumAge = 13,
                    DirectMessageAccessDataViewGuid = dmAccess,
                    ChatBadgeDataViewGuids = new List<Guid> { badge }
                } );

                SavePlatformCredentials( Credentials() );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsTrue( read.AreChatProfilesVisible );
                Assert.IsTrue( read.IsOpenDirectMessagingAllowed );
                Assert.AreEqual( 13, read.MinimumAge );
                Assert.AreEqual( dmAccess, read.DirectMessageAccessDataViewGuid );
                CollectionAssert.AreEqual( new List<Guid> { badge }, read.ChatBadgeDataViewGuids );
            }
        }

        [TestMethod]
        public void SaveChurchSettings_FromAReadingTakenBeforeEnable_DoesNotRevertThePlatformHalf()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                // An administrator opens the settings screen on a church that has not
                // enabled chat, Enable Chat lands while the form is open, and the save
                // carries a reading that predates it.
                SaveChurchSettings( new ChatPlatformConfiguration { MinimumAge = 13 } );
                var beforeEnable = ChatPlatformConfigurationService.Read();

                var tenantId = Guid.NewGuid();
                SavePlatformCredentials( Credentials( tenantId ) );

                beforeEnable.MinimumAge = 16;
                SaveChurchSettings( beforeEnable );

                var read = ChatPlatformConfigurationService.Read();

                Assert.AreEqual( 16, read.MinimumAge );
                Assert.AreEqual( tenantId, read.TenantId, "a save from a stale reading reverted the platform half" );
                Assert.AreEqual( PrivateKey, read.PrivateKey, "a save from a stale reading took the signing key away" );
            }
        }

        #endregion Who owns which field

        #region IsConfigured

        [TestMethod]
        public void IsConfigured_WithEveryPartTheGatesNeed_IsTrue()
        {
            Assert.IsTrue( Complete().IsConfigured );
        }

        [TestMethod]
        public void HasBeenEnabled_WhenTheStoredKeyCannotBeDecrypted_IsStillTrue()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                // A database restored onto an installation with a different encryption key.
                // The church cannot chat, but it is live on the chat platform: asking it to
                // enable again would set it up a second time and strand the first.
                SeedStored( new ChatPlatformConfiguration
                {
                    PrivateKey = Undecryptable,
                    TenantId = Guid.NewGuid(),
                    ProjectUrl = "https://example.supabase.co",
                    PublishableKey = "sb_publishable_test"
                } );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsTrue( read.HasBeenEnabled, "a church that is live on the platform would be offered Enable again" );
                Assert.IsFalse( read.IsConfigured );
            }
        }

        [TestMethod]
        public void HasBeenEnabled_WhenNothingWasEverSaved_IsFalse()
        {
            Assert.IsFalse( new ChatPlatformConfiguration().HasBeenEnabled );
        }

        [TestMethod]
        public void IsConfigured_MissingTenant_IsFalse()
        {
            var configuration = Complete();
            configuration.TenantId = null;

            Assert.IsFalse( configuration.IsConfigured );
        }

        [TestMethod]
        public void IsConfigured_MissingProjectUrl_IsFalse()
        {
            var configuration = Complete();
            configuration.ProjectUrl = null;

            Assert.IsFalse( configuration.IsConfigured );
        }

        [TestMethod]
        public void IsConfigured_MissingPublishableKey_IsFalse()
        {
            var configuration = Complete();
            configuration.PublishableKey = null;

            Assert.IsFalse( configuration.IsConfigured );
        }

        [TestMethod]
        public void IsConfigured_MissingPrivateKey_IsFalse()
        {
            var configuration = Complete();
            configuration.PrivateKey = null;

            Assert.IsFalse( configuration.IsConfigured );
        }

        #endregion IsConfigured

        #region Helpers

        private static ChatPlatformConfiguration Complete()
        {
            return new ChatPlatformConfiguration
            {
                PrivateKey = PrivateKey,
                TenantId = Guid.NewGuid(),
                ProjectUrl = "https://example.supabase.co",
                PublishableKey = "sb_publishable_test",
                Kid = "platform-kid-1"
            };
        }

        private static ConnectedServicesChatEntry Credentials( Guid? tenantId = null )
        {
            return new ConnectedServicesChatEntry
            {
                TenantId = tenantId ?? Guid.NewGuid(),
                ProjectUrl = "https://example.supabase.co",
                PublishableKey = "sb_publishable_test",
                Kid = "platform-kid-1",
                PrivateKey = PrivateKey
            };
        }

        /// <summary>
        /// Stores the church half, then flushes the settings cache. On a live
        /// installation the attribute save hook does that flush, which is what lets
        /// the next write read what the last one left. The mock context these tests
        /// run against never fires the hook, so without this a writer's own re-read
        /// sees the settings as they were before the previous write.
        /// </summary>
        private static void SaveChurchSettings( ChatPlatformConfiguration configuration )
        {
            ChatPlatformConfigurationService.SaveChurchSettings( configuration );
            RockCache.ClearAllCachedItems( false );
        }

        /// <inheritdoc cref="SaveChurchSettings" />
        private static void SavePlatformCredentials( ConnectedServicesChatEntry entry )
        {
            ChatPlatformConfigurationService.SavePlatformCredentials( entry );
            RockCache.ClearAllCachedItems( false );
        }

        /// <summary>
        /// The settings exactly as storage holds them, signing key still encrypted.
        /// </summary>
        private static ChatPlatformConfiguration StoredForm()
        {
            return SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION )
                .FromJsonOrNull<ChatPlatformConfiguration>();
        }

        private static void SeedStored( ChatPlatformConfiguration stored )
        {
            SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION, stored.ToJson() );
            RockCache.ClearAllCachedItems( false );
        }

        /// <summary>
        /// Inserts the placeholder Attribute row the setting is stored in. Without it
        /// the first write takes the insert branch, which resolves a field type the mock
        /// context does not have.
        /// </summary>
        private static void PrimeSettingKey()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
            {
                Id = 1,
                EntityTypeId = null,
                EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER,
                EntityTypeQualifierValue = string.Empty,
                Key = SystemSetting.CHAT_PLATFORM_CONFIGURATION,
                Name = SystemSetting.CHAT_PLATFORM_CONFIGURATION.SplitCase(),
                DefaultValue = string.Empty,
                Guid = Guid.NewGuid(),
                Categories = new List<Category>(),
                AttributeQualifiers = new List<AttributeQualifier>()
            } );

            SystemSettings.Remove();
        }

        #endregion Helpers
    }
}
