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

using Rock.Security;
using Rock.SystemKey;
using Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration;
using Rock.ViewModels.Utility;
using Rock.Web;

using ConnectedServicesChatBag = Rock.ViewModels.Blocks.Administration.SparkConnectedServices.ChatConfigurationBag;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// Reads and writes the church's chat settings. They live as one JSON value in
    /// system settings with the signing key encrypted inside it, so the key is at rest
    /// wherever that value is backed up, copied or read by hand. It also answers what
    /// the Chat Configuration block and the Connected Services card may show and accept,
    /// so those questions can be answered by a test while the blocks supply only the
    /// stored settings and the caller's authority.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Two things write here and they write different fields. An administrator
    ///         at the settings screen owns the church half; enabling chat owns the
    ///         credentials the platform issued and the signing key that came with them.
    ///         Neither writes the whole value: each re-reads what is stored inside the
    ///         lock and overwrites only the fields it owns, so a save made from a
    ///         reading taken minutes ago cannot carry back a stale copy of the other
    ///         half.
    ///     </para>
    ///     <para>
    ///         The lock is this process only. Rock runs on web farms and system settings
    ///         offer a whole-string write with no compare and set, so two nodes can still
    ///         read one snapshot and have the later write carry the earlier one's stale
    ///         copy. Enabling happens once per church and the settings screen has one
    ///         editor at a time, so a collision needs both in the same instant on
    ///         different nodes; closing it properly needs a version column or a row lock.
    ///     </para>
    /// </remarks>
    internal static class ChatPlatformConfigurationService
    {
        private static readonly object _saveLock = new object();

        /// <summary>
        /// The settings as stored, with the signing key decrypted. A church that has
        /// never saved any gets an empty configuration rather than null, so every caller
        /// can ask <see cref="ChatPlatformConfiguration.IsConfigured"/> without a null check first.
        /// </summary>
        public static ChatPlatformConfiguration Read()
        {
            var configuration = ReadStored();

            if ( configuration.PrivateKey.IsNotNullOrWhiteSpace() )
            {
                // A value this installation cannot decrypt, because the database came from
                // one with a different encryption key, decrypts to null. Reading it as absent
                // is right: the church genuinely cannot sign anything. Nothing is lost by
                // saying so, because no writer here takes a key from what it was handed.
                configuration.PrivateKey = Encryption.DecryptString( configuration.PrivateKey );
            }

            return configuration;
        }

        /// <summary>
        /// Stores the settings an administrator owns. Everything the platform issued,
        /// the signing key included, is left exactly as it sits in storage, so a save
        /// from a screen opened before chat was enabled cannot undo the enabling.
        /// </summary>
        /// <param name="configuration">The settings to store. Only the church-owned fields are read from it; null stores the defaults.</param>
        public static void SaveChurchSettings( ChatPlatformConfiguration configuration )
        {
            var source = configuration ?? new ChatPlatformConfiguration();

            lock ( _saveLock )
            {
                var stored = ReadStored();

                stored.AreChatProfilesVisible = source.AreChatProfilesVisible;
                stored.IsOpenDirectMessagingAllowed = source.IsOpenDirectMessagingAllowed;
                stored.MinimumAge = source.MinimumAge;
                stored.DirectMessageAccessDataViewGuid = source.DirectMessageAccessDataViewGuid;
                stored.ChatBadgeDataViewGuids = source.ChatBadgeDataViewGuids;

                Write( stored );
            }
        }

        /// <summary>
        /// Stores what the platform issued when chat was enabled, encrypting the signing
        /// key on the way. The church's own settings are left exactly as they are.
        /// </summary>
        /// <param name="entry">The credentials enabling chat returned.</param>
        public static void SavePlatformCredentials( ConnectedServicesChatEntry entry )
        {
            if ( entry == null )
            {
                return;
            }

            lock ( _saveLock )
            {
                var stored = ReadStored();

                stored.TenantId = entry.TenantId;
                stored.ProjectUrl = entry.ProjectUrl;
                stored.PublishableKey = entry.PublishableKey;
                stored.Kid = entry.Kid;
                stored.PrivateKey = Encryption.EncryptString( entry.PrivateKey );

                Write( stored );
            }
        }

        /// <summary>
        /// Stores the backoff the chat platform last advised. Everything else is left exactly as it
        /// sits in storage, so a run finishing while an administrator saves the settings screen
        /// cannot carry back the settings as they were when the run started.
        /// </summary>
        /// <param name="until">The time it would rather not hear from this church before, or null to clear it.</param>
        public static void SaveSyncBackoff( DateTimeOffset? until )
        {
            lock ( _saveLock )
            {
                var stored = ReadStored();

                // Most runs are told what is already stored, usually no backoff at all.
                if ( stored.SyncBackoffUntil == until )
                {
                    return;
                }

                stored.SyncBackoffUntil = until;

                Write( stored );
            }
        }

        /// <summary>
        /// The settings as the Chat Configuration block may see them. The signing key is
        /// reduced to the fact that there is one, because a screen that receives a key is
        /// a screen that can leak it, and the shipped chat screen this replaces put its
        /// secret in a plain text box.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <returns>A bag safe to hand to a browser.</returns>
        public static ChatConfigurationBag ToConfigurationBag( ChatPlatformConfiguration stored )
        {
            var configuration = stored ?? new ChatPlatformConfiguration();

            return new ChatConfigurationBag
            {
                AreChatProfilesVisible = configuration.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = configuration.IsOpenDirectMessagingAllowed,
                MinimumAge = configuration.MinimumAge,
                DirectMessageAccessDataView = ToListItem( configuration.DirectMessageAccessDataViewGuid ),
                ChatBadgeDataViews = ( configuration.ChatBadgeDataViewGuids ?? new List<Guid>() )
                    .Select( guid => ToListItem( guid ) )
                    .ToList(),
                ProjectUrl = configuration.ProjectUrl,
                PublishableKey = configuration.PublishableKey,
                TenantId = configuration.TenantId?.ToString(),
                Kid = configuration.Kid,
                IsChurchKeyPresent = configuration.PrivateKey.IsNotNullOrWhiteSpace()
            };
        }

        /// <summary>
        /// Reads the church-owned settings out of what the Chat Configuration block sent
        /// back. The half the platform issued when chat was enabled, and the signing key,
        /// are not here at all: an administrator does not type them, the writer that
        /// stores this reaches only the fields below, and so a bag that carries them
        /// changes nothing.
        /// </summary>
        /// <param name="bag">What the screen sent back.</param>
        /// <param name="isAuthorizedToEdit">Whether the caller may change this block's settings.</param>
        /// <param name="isPersonDataView">Whether a Data View exists and is a Data View of people.</param>
        /// <returns>The outcome, carrying the settings to store when the save is allowed.</returns>
        public static ChatConfigurationSaveResult SaveConfiguration( ChatConfigurationBag bag, bool isAuthorizedToEdit, Func<Guid, bool> isPersonDataView )
        {
            if ( !isAuthorizedToEdit )
            {
                return new ChatConfigurationSaveResult { IsSaved = false };
            }

            var sent = bag ?? new ChatConfigurationBag();

            return new ChatConfigurationSaveResult
            {
                IsSaved = true,
                Configuration = new ChatPlatformConfiguration
                {
                    AreChatProfilesVisible = sent.AreChatProfilesVisible,
                    IsOpenDirectMessagingAllowed = sent.IsOpenDirectMessagingAllowed,
                    MinimumAge = sent.MinimumAge,
                    DirectMessageAccessDataViewGuid = sent.DirectMessageAccessDataView?.Value.AsGuidOrNull(),
                    // Once each, in its first place, and people only: a badge is matched to its
                    // holders by person id, and the picker's own limits hold in the browser alone.
                    ChatBadgeDataViewGuids = ( sent.ChatBadgeDataViews ?? new List<ListItemBag>() )
                        .Select( item => item?.Value.AsGuidOrNull() )
                        .Where( guid => guid.HasValue )
                        .Select( guid => guid.Value )
                        .Distinct()
                        .Where( guid => isPersonDataView( guid ) )
                        .ToList()
                }
            };
        }

        /// <summary>
        /// The chat card's state on the Connected Services page as the browser may see it.
        /// The signing key that arrived with these settings is absent, because a bag that
        /// carries a key is a bag that can leak one; nothing on the card needs it.
        /// </summary>
        /// <remarks>
        /// This and <see cref="MayEnable"/> read whether the organization has been set up,
        /// never whether chat can run. The two disagree on the organization whose stored
        /// signing key this installation cannot decrypt, and reading the second would offer
        /// that organization an Enable that mints a new key and strands the tenant it
        /// already has.
        /// </remarks>
        /// <param name="stored">The settings as stored.</param>
        /// <returns>A bag safe to hand to a browser.</returns>
        public static ConnectedServicesChatBag ToCardBag( ChatPlatformConfiguration stored )
        {
            var configuration = stored ?? new ChatPlatformConfiguration();

            if ( !configuration.HasBeenEnabled )
            {
                return new ConnectedServicesChatBag { IsEnabled = false };
            }

            return new ConnectedServicesChatBag
            {
                IsEnabled = true,
                TenantId = configuration.TenantId?.ToString(),
                ProjectUrl = configuration.ProjectUrl,
                IsCredentialUnreadable = configuration.IsEnabledWithoutCredentials
            };
        }

        /// <summary>
        /// Whether this organization may be set up on the chat platform. False once it
        /// has been, because enabling a second time mints a second signing key and
        /// orphans the first, and nothing here can rotate or retire one.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <returns><c>true</c> when Enable may be offered and acted on.</returns>
        public static bool MayEnable( ChatPlatformConfiguration stored )
        {
            return !( stored ?? new ChatPlatformConfiguration() ).HasBeenEnabled;
        }

        /// <summary>
        /// The settings exactly as storage holds them, signing key still encrypted.
        /// </summary>
        private static ChatPlatformConfiguration ReadStored()
        {
            var json = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

            return json.FromJsonOrNull<ChatPlatformConfiguration>() ?? new ChatPlatformConfiguration();
        }

        private static void Write( ChatPlatformConfiguration stored )
        {
            SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION, stored.ToJson() );
        }

        /// <summary>
        /// A Data View reference the screen can render. The text is filled in by the
        /// block, which has the cache; the value alone is what is stored and sent back.
        /// </summary>
        private static ListItemBag ToListItem( Guid? guid )
        {
            return guid.HasValue
                ? new ListItemBag { Value = guid.Value.ToString() }
                : null;
        }
    }
}
