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
using Rock.Security;
using Rock.SystemKey;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;

namespace Rock.Tests.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// The church's chat settings: how they are stored, what is encrypted, and
    /// when they are complete enough for chat to run.
    /// </summary>
    [TestClass]
    public class ChatPlatformConfigurationTests
    {
        private const string PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}";
        private const string Undecryptable = "not-something-this-installation-can-decrypt";

        #region Round trip

        [TestMethod]
        public void Save_ThenRead_RoundTripsEverySetting()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                var badgeOne = Guid.NewGuid();
                var badgeTwo = Guid.NewGuid();
                var dmAccess = Guid.NewGuid();
                var tenantId = Guid.NewGuid();

                ChatPlatformConfigurationService.Save( new ChatPlatformConfiguration
                {
                    AreChatProfilesVisible = true,
                    IsOpenDirectMessagingAllowed = true,
                    MinimumAge = 13,
                    DirectMessageAccessDataViewGuid = dmAccess,
                    ChatBadgeDataViewGuids = new List<Guid> { badgeOne, badgeTwo },
                    PrivateKey = PrivateKey,
                    TenantId = tenantId,
                    ProjectUrl = "https://example.supabase.co",
                    PublishableKey = "sb_publishable_test",
                    Kid = "platform-kid-1"
                } );

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
            }
        }

        [TestMethod]
        public void Save_StoresNoReadablePrivateKey_AndReadReturnsItDecrypted()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                ChatPlatformConfigurationService.Save( new ChatPlatformConfiguration
                {
                    PrivateKey = PrivateKey,
                    TenantId = Guid.NewGuid(),
                    ProjectUrl = "https://example.supabase.co",
                    PublishableKey = "sb_publishable_test"
                } );

                var stored = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

                Assert.IsFalse( stored.Contains( "secret-part" ), "the stored settings carry the key in the clear" );
                Assert.AreEqual( PrivateKey, ChatPlatformConfigurationService.Read().PrivateKey );
            }
        }

        [TestMethod]
        public void Read_WhenTheStoredKeyCannotBeDecrypted_ReportsNoKeyButKeepsTheStoredOne()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                // A database restored onto an installation with a different encryption key
                // decrypts to nothing. The church genuinely cannot sign anything, so reading
                // the key as absent is right; losing what is stored is not.
                SystemSettings.SetValue(
                    SystemSetting.CHAT_PLATFORM_CONFIGURATION,
                    new ChatPlatformConfiguration
                    {
                        PrivateKey = Undecryptable,
                        TenantId = Guid.NewGuid(),
                        ProjectUrl = "https://example.supabase.co",
                        PublishableKey = "sb_publishable_test"
                    }.ToJson() );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsNull( read.PrivateKey );
                Assert.IsFalse( read.IsConfigured );
                Assert.AreEqual( Undecryptable, read.StoredPrivateKey );
            }
        }

        [TestMethod]
        public void ToStoredForm_WithNoReadableKey_PutsTheStoredOneBackRatherThanNothing()
        {
            var configuration = Complete();
            configuration.PrivateKey = null;
            configuration.StoredPrivateKey = Undecryptable;

            var stored = ChatPlatformConfigurationService.ToStoredForm( configuration );

            Assert.AreEqual( Undecryptable, stored.PrivateKey, "a save destroyed the stored signing key" );
        }

        [TestMethod]
        public void ToStoredForm_FromAConfigurationNobodyRead_KeepsTheKeyStorageAlreadyHolds()
        {
            // The shape a later entry point takes when it records the platform half: built
            // rather than read, so it carries no key at all. Writing is whole-document, so
            // without this it would write nothing over the church's signing key.
            var built = new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = "https://example.supabase.co",
                PublishableKey = "sb_publishable_test",
                Kid = "platform-kid-1"
            };

            var stored = ChatPlatformConfigurationService.ToStoredForm( built, Undecryptable );

            Assert.AreEqual( Undecryptable, stored.PrivateKey, "a save that mentioned no key took the stored one away" );
        }

        [TestMethod]
        public void ToStoredForm_FromNothingAtAll_KeepsTheKeyStorageAlreadyHolds()
        {
            var stored = ChatPlatformConfigurationService.ToStoredForm( null, Undecryptable );

            Assert.AreEqual( Undecryptable, stored.PrivateKey );
        }

        [TestMethod]
        public void ToStoredForm_WithAReadableKey_EncryptsIt()
        {
            var configuration = Complete();
            configuration.StoredPrivateKey = Undecryptable;

            var stored = ChatPlatformConfigurationService.ToStoredForm( configuration );

            Assert.AreNotEqual( PrivateKey, stored.PrivateKey );
            Assert.AreNotEqual( Undecryptable, stored.PrivateKey );
            Assert.AreEqual( PrivateKey, Encryption.DecryptString( stored.PrivateKey ) );
        }

        [TestMethod]
        public void ToStoredForm_CarriesNothingThatIsNotStored()
        {
            var configuration = Complete();
            configuration.StoredPrivateKey = Undecryptable;

            var json = ChatPlatformConfigurationService.ToStoredForm( configuration ).ToJson();

            Assert.IsFalse( json.Contains( "StoredPrivateKey" ) );
            Assert.IsFalse( json.Contains( "IsConfigured" ) );
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

        #endregion Round trip

        #region IsConfigured

        [TestMethod]
        public void IsConfigured_WithEveryPartTheGatesNeed_IsTrue()
        {
            Assert.IsTrue( Complete().IsConfigured );
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
