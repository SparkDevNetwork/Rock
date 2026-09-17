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
using Rock.Security;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// Reads and writes the church's chat settings. They live as one JSON value in
    /// system settings with the signing key encrypted inside it, so the key is at rest
    /// wherever that value is backed up, copied or read by hand.
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
    }
}
