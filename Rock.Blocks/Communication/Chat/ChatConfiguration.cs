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

using Rock.Security;
using Rock.SystemKey;
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
    //[SupportedSiteTypes( Model.SiteType.Web )]

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

            SaveChatConfigurationToSystemSettings( bag );

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
            var chatConfigurationJson = Rock.Web.SystemSettings.GetValue( SystemSetting.CHAT_CONFIGURATION );
            var chatConfiguration = chatConfigurationJson.FromJsonOrNull<Rock.Communication.Chat.ChatConfiguration>() ?? new Rock.Communication.Chat.ChatConfiguration();

            ListItemBag welcomeWorkflowType = null;
            if ( chatConfiguration.WelcomeWorkflowTypeGuid.HasValue )
            {
                var welcomeWorkflowTypeCache = WorkflowTypeCache.Get( chatConfiguration.WelcomeWorkflowTypeGuid.Value );
                if ( welcomeWorkflowTypeCache != null )
                {
                    welcomeWorkflowType = welcomeWorkflowTypeCache.ToListItemBag();
                }
            }

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
                ApiSecret = Encryption.DecryptString( chatConfiguration.ApiSecret ),
                AreChatProfilesVisible = chatConfiguration.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = chatConfiguration.IsOpenDirectMessagingAllowed,
                WelcomeWorkflowType = welcomeWorkflowType,
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
            Guid? welcomeWorkflowTypeGuid = null;
            if ( bag.WelcomeWorkflowType != null )
            {
                var welcomeWorkflowTypeCache = WorkflowTypeCache.Get( bag.WelcomeWorkflowType.Value );
                if ( welcomeWorkflowTypeCache != null )
                {
                    welcomeWorkflowTypeGuid = welcomeWorkflowTypeCache.Guid;
                }
            }

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
                ApiSecret = Encryption.EncryptString( bag.ApiSecret ),
                AreChatProfilesVisible = bag.AreChatProfilesVisible,
                IsOpenDirectMessagingAllowed = bag.IsOpenDirectMessagingAllowed,
                WelcomeWorkflowTypeGuid = welcomeWorkflowTypeGuid,
                ChatBadgeDataViewGuids = chatBadgeDataViewGuids
            };

            Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_CONFIGURATION, chatConfiguration.ToJson() );
        }

        #endregion Private Methods
    }
}
