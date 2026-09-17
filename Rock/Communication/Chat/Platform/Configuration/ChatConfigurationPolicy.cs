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

using Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration;
using Rock.ViewModels.Utility;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// What the chat configuration screen may be shown, and what it may change. Kept
    /// apart from the block so both questions can be answered by a test: the block
    /// itself only supplies the stored settings and the caller's authority.
    /// </summary>
    internal static class ChatConfigurationPolicy
    {
        /// <summary>
        /// The settings as the screen may see them. The signing key is reduced to the
        /// fact that there is one, because a screen that receives a key is a screen
        /// that can leak it, and the shipped chat screen this replaces put its secret
        /// in a plain text box.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <returns>A bag safe to hand to a browser.</returns>
        public static ChatConfigurationBag ToBag( ChatPlatformConfiguration stored )
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
        /// Applies the church-owned settings from a screen onto the stored ones. The
        /// half the platform issued when chat was enabled, and the signing key, are
        /// taken from what is stored and never from the bag: an administrator does not
        /// type them, so a bag that carries them is either stale or hostile, and the
        /// key is not on the bag at all so a save could otherwise erase it.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <param name="bag">What the screen sent back.</param>
        /// <param name="isAuthorizedToEdit">Whether the caller may change this block's settings.</param>
        /// <returns>The outcome, carrying the settings to store when the save is allowed.</returns>
        public static ChatConfigurationSaveResult Save( ChatPlatformConfiguration stored, ChatConfigurationBag bag, bool isAuthorizedToEdit )
        {
            if ( !isAuthorizedToEdit )
            {
                return new ChatConfigurationSaveResult { IsSaved = false };
            }

            var configuration = stored ?? new ChatPlatformConfiguration();
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
                    ChatBadgeDataViewGuids = ( sent.ChatBadgeDataViews ?? new List<ListItemBag>() )
                        .Select( item => item?.Value.AsGuidOrNull() )
                        .Where( guid => guid.HasValue )
                        .Select( guid => guid.Value )
                        .ToList(),

                    PrivateKey = configuration.PrivateKey,

                    // Carried as well as the readable key, because on an installation that
                    // cannot decrypt what is stored the readable one is null and this is all
                    // that stands between a save and the church losing its signing key.
                    StoredPrivateKey = configuration.StoredPrivateKey,

                    TenantId = configuration.TenantId,
                    ProjectUrl = configuration.ProjectUrl,
                    PublishableKey = configuration.PublishableKey,
                    Kid = configuration.Kid
                }
            };
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

    /// <summary>
    /// What came of a save the screen asked for.
    /// </summary>
    internal sealed class ChatConfigurationSaveResult
    {
        /// <summary>
        /// Whether the save was allowed.
        /// </summary>
        public bool IsSaved { get; set; }

        /// <summary>
        /// The settings to store, or null when the save was refused.
        /// </summary>
        public ChatPlatformConfiguration Configuration { get; set; }
    }
}
