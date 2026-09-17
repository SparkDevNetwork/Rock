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
            var json = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );
            var configuration = json.FromJsonOrNull<ChatPlatformConfiguration>() ?? new ChatPlatformConfiguration();

            if ( configuration.PrivateKey.IsNotNullOrWhiteSpace() )
            {
                // A value this installation cannot decrypt, because the database came from
                // one with a different encryption key, decrypts to null. Reading it as absent
                // is right, the church genuinely cannot sign anything, but the stored value is
                // kept so a save made from this reading puts it back rather than over it.
                configuration.StoredPrivateKey = configuration.PrivateKey;
                configuration.PrivateKey = Encryption.DecryptString( configuration.PrivateKey );
            }

            return configuration;
        }

        /// <summary>
        /// Stores the settings, encrypting the signing key on the way. The caller passes
        /// the configuration it means to end up with; this writes it whole.
        /// </summary>
        /// <param name="configuration">The settings to store. Null stores an empty configuration.</param>
        public static void Save( ChatPlatformConfiguration configuration )
        {
            lock ( _saveLock )
            {
                var stored = ToStoredForm( configuration );

                if ( stored.PrivateKey.IsNullOrWhiteSpace() )
                {
                    // Only asked for when the save carries no key, so an ordinary save costs
                    // no extra read: a caller that built its configuration rather than reading
                    // one has nothing to put back, and this is where it comes from.
                    stored.PrivateKey = ReadStoredKey();
                }

                SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION, stored.ToJson() );
            }
        }

        /// <summary>
        /// The signing key exactly as storage holds it, still encrypted.
        /// </summary>
        private static string ReadStoredKey()
        {
            var json = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

            return json.FromJsonOrNull<ChatPlatformConfiguration>()?.PrivateKey;
        }

        /// <summary>
        /// The configuration as it is written down: the signing key encrypted, or the
        /// value already in storage when there is no readable key to encrypt.
        /// </summary>
        /// <param name="configuration">The settings to store. Null gives an empty configuration.</param>
        /// <returns>What gets serialized.</returns>
        public static ChatPlatformConfiguration ToStoredForm( ChatPlatformConfiguration configuration )
        {
            return ToStoredForm( configuration, null );
        }

        /// <summary>
        /// The configuration as it is written down, given what storage already holds.
        /// Writing is whole-document, so a caller that builds a configuration rather than
        /// reading one carries no key at all and would otherwise write nothing over the
        /// church's. Nothing is designed to take a key away, so a save that does not
        /// mention one leaves the one that is there.
        /// </summary>
        /// <param name="configuration">The settings to store. Null gives an empty configuration.</param>
        /// <param name="existingStoredKey">The encrypted key storage already holds, if any.</param>
        /// <returns>What gets serialized.</returns>
        public static ChatPlatformConfiguration ToStoredForm( ChatPlatformConfiguration configuration, string existingStoredKey )
        {
            var toStore = configuration ?? new ChatPlatformConfiguration();

            return new ChatPlatformConfiguration
            {
                AreChatProfilesVisible = toStore.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = toStore.IsOpenDirectMessagingAllowed,
                MinimumAge = toStore.MinimumAge,
                DirectMessageAccessDataViewGuid = toStore.DirectMessageAccessDataViewGuid,
                ChatBadgeDataViewGuids = toStore.ChatBadgeDataViewGuids,
                TenantId = toStore.TenantId,
                ProjectUrl = toStore.ProjectUrl,
                PublishableKey = toStore.PublishableKey,
                Kid = toStore.Kid,
                PrivateKey = toStore.PrivateKey.IsNotNullOrWhiteSpace()
                    ? Encryption.EncryptString( toStore.PrivateKey )
                    : toStore.StoredPrivateKey ?? existingStoredKey
            };
        }
    }
}
