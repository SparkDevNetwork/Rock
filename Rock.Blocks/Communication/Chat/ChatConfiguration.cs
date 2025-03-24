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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Rock.Communication.Chat;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication.Chat
{
    /// <summary>
    /// Used for making configuration changes to Rock's chat system.
    /// </summary>

    [DisplayName( "Chat Configuration" )]
    [Category( "Communication > Chat" )]
    [Description( "Used for making configuration changes to Rock's chat system." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "4E1EF8E8-8984-47EA-A6FC-31125C3B6153" )]
    [Rock.SystemGuid.BlockTypeGuid( "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB" )]
    public class ChatConfiguration : RockBlockType
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region RockBlockType Impelementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ChatConfigurationInitializationBox
            {
                ChatConfigurationBag = GetCurrentChatConfigurationBag(),
                NavigationUrls = GetBoxNavigationUrls()
            };
        }

        #endregion RockBlockType Impelementation

        #region Block Actions

        /// <summary>
        /// Saves the provided chat configuration.
        /// </summary>
        /// <param name="bag">An object containing the chat configuration to save.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveChatConfiguration( ChatConfigurationBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            var oldConfig = GetCurrentChatConfigurationBag();
            var shouldReinitialize = oldConfig.ApiKey != bag.ApiKey || oldConfig.ApiSecret != bag.ApiSecret;

            SaveChatConfigurationToSystemSettings( bag );

            // Perform an app settings and group type sync to the external chat system in a background task, as it could
            // take some time to complete.
            Task.Run( async () =>
            {
                using ( var rockContext = new RockContext() )
                using ( var chatHelper = new ChatHelper( rockContext ) )
                {
                    if ( shouldReinitialize )
                    {
                        chatHelper.InitializeChatProvider();
                    }

                    var isSetUpResult = await chatHelper.EnsureChatProviderAppIsSetUpAsync();
                    if ( isSetUpResult?.IsSetUp != true )
                    {
                        // There's no point in trying to sync the group types if initial setup failed.
                        return;
                    }

                    // We'll only sync chat-enabled group types as a part of this configuration save, as there is no
                    // good way to warn the individual of any previously-synced channel types that might become deleted
                    // as a result of synching no-longer-chat-enabled group types here. We'll let the job clean those
                    // up later.
                    var chatEnabledGroupTypes = new GroupTypeService( rockContext )
                        .GetChatEnabledGroupTypes()
                        .ToList();

                    await chatHelper.SyncGroupTypesToChatProviderAsync( chatEnabledGroupTypes );
                }
            } );

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the chat configuration bag for the current chat configuration.
        /// </summary>
        /// <returns>A chat configuration bag instance that represents the current chat configuration.</returns>
        private ChatConfigurationBag GetCurrentChatConfigurationBag()
        {
            var chatConfiguration = ChatHelper.GetChatConfiguration();

            List<ListItemBag> chatBadgeDataViews = null;
            if ( chatConfiguration.ChatBadgeDataViewGuids?.Any() == true )
            {
                chatBadgeDataViews = new List<ListItemBag>();

                foreach ( var dataViewGuid in chatConfiguration.ChatBadgeDataViewGuids )
                {
                    var dataViewCache = DataViewCache.Get( dataViewGuid );
                    if ( dataViewCache != null )
                    {
                        chatBadgeDataViews.Add( dataViewCache.ToListItemBag() );
                    }
                }
            }

            return new ChatConfigurationBag
            {
                ApiKey = chatConfiguration.ApiKey,
                ApiSecret = chatConfiguration.ApiSecret,
                AreChatProfilesVisible = chatConfiguration.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = chatConfiguration.IsOpenDirectMessagingAllowed,
                ChatBadgeDataViews = chatBadgeDataViews
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Saves the provided chat configuration to system settings.
        /// </summary>
        /// <param name="bag">The chat configuration to save to system settings.</param>
        private void SaveChatConfigurationToSystemSettings( ChatConfigurationBag bag )
        {
            List<Guid> chatBadgeDataViewGuids = null;
            if ( bag.ChatBadgeDataViews?.Any() == true )
            {
                chatBadgeDataViewGuids = new List<Guid>();

                foreach ( var dataView in bag.ChatBadgeDataViews )
                {
                    var dataViewCache = DataViewCache.Get( dataView.Value );
                    if ( dataViewCache != null )
                    {
                        chatBadgeDataViewGuids.Add( dataViewCache.Guid );
                    }
                }
            }

            var chatConfiguration = new Rock.Communication.Chat.ChatConfiguration
            {
                ApiKey = bag.ApiKey,
                ApiSecret = bag.ApiSecret,
                AreChatProfilesVisible = bag.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = bag.IsOpenDirectMessagingAllowed,
                ChatBadgeDataViewGuids = chatBadgeDataViewGuids
            };

            ChatHelper.SaveChatConfiguration( chatConfiguration );
        }

        #endregion Private Methods
    }
}
