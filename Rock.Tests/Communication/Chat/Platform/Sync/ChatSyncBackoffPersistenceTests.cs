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

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The backoff the platform advises, kept between job runs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every acknowledgement carries the time the platform would rather not hear from this church
    /// before. It is advice about load, so the job has to still be honouring it after an
    /// application pool recycle, which rules out holding it in memory. It sits with the tenant id
    /// and the project URL because the same writer owns that half, and because the other candidate,
    /// a job attribute, would put the platform's advice on a screen the church's administrator
    /// edits.
    /// </para>
    /// <para>
    /// The writes are narrow on purpose, in both directions. A backoff write that carried a whole
    /// configuration would let a job run overwrite a settings change made while it was running, and
    /// a settings save that carried a whole configuration would throw away the advice of the run
    /// that just finished.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncBackoffPersistenceTests
    {
        #region Fields

        private const string PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}";

        #endregion Fields

        #region Round trip

        [TestMethod]
        public void SaveSyncBackoff_IsReadBackByTheNextRun()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                SavePlatformCredentials( Credentials() );

                var until = new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero );
                SaveSyncBackoff( until );

                Assert.AreEqual( until, ChatPlatformConfigurationService.Read().SyncBackoffUntil );
            }
        }

        /// <summary>
        /// The advice arrives as a moment with an offset on it, and the store has to keep the
        /// moment rather than the wall clock reading. A value that came back shifted would hold a
        /// church off for an extra hour, or let it through an hour early.
        /// </summary>
        [TestMethod]
        public void SaveSyncBackoff_WithANonUtcOffset_ReadsBackAsTheSameInstant()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                SavePlatformCredentials( Credentials() );

                var until = new DateTimeOffset( 2026, 9, 21, 6, 30, 0, TimeSpan.FromHours( -5 ) );
                SaveSyncBackoff( until );

                var read = ChatPlatformConfigurationService.Read().SyncBackoffUntil;

                Assert.IsNotNull( read );
                Assert.AreEqual( until.UtcDateTime, read.Value.UtcDateTime );
            }
        }

        [TestMethod]
        public void SaveSyncBackoff_WithNothing_ClearsTheAdvice()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                SavePlatformCredentials( Credentials() );
                SaveSyncBackoff( new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero ) );

                SaveSyncBackoff( null );

                Assert.IsNull( ChatPlatformConfigurationService.Read().SyncBackoffUntil );
            }
        }

        #endregion Round trip

        #region Who owns which field

        [TestMethod]
        public void SaveSyncBackoff_LeavesEveryOtherSettingExactlyAsStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();

                var tenantId = Guid.NewGuid();
                var dmAccess = Guid.NewGuid();

                SaveChurchSettings( new ChatPlatformConfiguration
                {
                    AreChatProfilesVisible = true,
                    MinimumAge = 13,
                    DirectMessageAccessDataViewGuid = dmAccess
                } );
                SavePlatformCredentials( Credentials( tenantId ) );

                SaveSyncBackoff( new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero ) );

                var read = ChatPlatformConfigurationService.Read();

                Assert.IsTrue( read.AreChatProfilesVisible );
                Assert.AreEqual( 13, read.MinimumAge );
                Assert.AreEqual( dmAccess, read.DirectMessageAccessDataViewGuid );
                Assert.AreEqual( tenantId, read.TenantId );
                Assert.AreEqual( PrivateKey, read.PrivateKey, "a backoff write took the signing key away" );
            }
        }

        /// <summary>
        /// An administrator saving the settings screen mid-run does not throw away the advice the
        /// run has just been given.
        /// </summary>
        [TestMethod]
        public void SaveChurchSettings_LeavesTheBackoffExactlyAsStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                SavePlatformCredentials( Credentials() );

                var until = new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero );
                SaveSyncBackoff( until );

                SaveChurchSettings( new ChatPlatformConfiguration { MinimumAge = 16 } );

                var read = ChatPlatformConfigurationService.Read();

                Assert.AreEqual( 16, read.MinimumAge );
                Assert.AreEqual( until, read.SyncBackoffUntil );
            }
        }

        [TestMethod]
        public void SavePlatformCredentials_LeavesTheBackoffExactlyAsStored()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                PrimeSettingKey();
                SavePlatformCredentials( Credentials() );

                var until = new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero );
                SaveSyncBackoff( until );

                SavePlatformCredentials( Credentials() );

                Assert.AreEqual( until, ChatPlatformConfigurationService.Read().SyncBackoffUntil );
            }
        }

        #endregion Who owns which field

        #region Support

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

        private static void SaveChurchSettings( ChatPlatformConfiguration configuration )
        {
            ChatPlatformConfigurationService.SaveChurchSettings( configuration );
            RockCache.ClearAllCachedItems( false );
        }

        private static void SavePlatformCredentials( ConnectedServicesChatEntry entry )
        {
            ChatPlatformConfigurationService.SavePlatformCredentials( entry );
            RockCache.ClearAllCachedItems( false );
        }

        private static void SaveSyncBackoff( DateTimeOffset? until )
        {
            ChatPlatformConfigurationService.SaveSyncBackoff( until );
            RockCache.ClearAllCachedItems( false );
        }

        /// <summary>
        /// Inserts the placeholder Attribute row the setting is stored in. Without it the first
        /// write takes the insert branch, which resolves a field type the mock context does not
        /// have.
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

        #endregion Support
    }
}
