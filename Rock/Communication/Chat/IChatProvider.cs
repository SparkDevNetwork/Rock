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
using System.Net.Http;
using System.Threading.Tasks;

using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Sync;
using Rock.Web.Cache;

namespace Rock.Communication.Chat
{
    /// <summary>
    /// Interface for interacting with an external chat provider.
    /// </summary>
    internal interface IChatProvider
    {
        #region Configuration

        /// <summary>
        /// Initializes the chat provider instance.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates app settings in the external chat system to match the current Rock app settings.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/> indicating
        /// whether the app settings were updated in the external chat system.
        /// </returns>
        Task<ChatSyncSetupResult> UpdateAppSettingsAsync();

        /// <summary>
        /// Updates push notification settings in the external chat system to match the current Rock mobile push settings.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/> indicating
        /// whether the push notification settings were updated in the external chat system.
        /// </returns>
        Task<ChatSyncSetupResult> UpdatePushNotificationSettingsAsync();

        #endregion Configuration

        #region Roles & Permission Grants

        /// <summary>
        /// Gets the default permission grants per app role.
        /// </summary>
        Dictionary<string, List<string>> GetDefaultAppGrantsByRole();

        /// <summary>
        /// Gets the default permission grants per channel type role.
        /// </summary>
        Dictionary<string, List<string>> GetDefaultChannelTypeGrantsByRole();

        /// <summary>
        /// Ensures that all required, app-scoped roles exist in the external chat system. Creates any missing roles if
        /// necessary.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/> indicating
        /// whether the app roles exist in the external chat system.
        /// </returns>
        Task<ChatSyncSetupResult> EnsureAppRolesExistAsync();

        /// <summary>
        /// Ensures that all required, app-scoped permission grants exist in the external chat system.
        /// </summary>
        /// <param name="config">
        /// A <see cref="RockToChatSyncConfig"/> to fine-tune how Rock-to-Chat synchronization should be completed.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/> indicating
        /// whether the app app-scoped permission grants exist in the external chat system.
        /// </returns>
        Task<ChatSyncSetupResult> EnsureAppGrantsExistAsync( RockToChatSyncConfig config );

        #endregion Roles & Permission Grants

        #region Chat Channel Types

        /// <summary>
        /// Gets a list of all <see cref="ChatChannelType"/>s from the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelTypesResult"/>.
        /// </returns>
        Task<GetChatChannelTypesResult> GetAllChatChannelTypesAsync();

        /// <summary>
        /// Creates new <see cref="ChatChannelType"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypes">The list of <see cref="ChatChannelType"/>s to create.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> CreateChatChannelTypesAsync( List<ChatChannelType> chatChannelTypes );

        /// <summary>
        /// Updates existing <see cref="ChatChannelType"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypes">The list of <see cref="ChatChannelType"/>s to update.</param>
        /// <param name="config">
        /// A <see cref="RockToChatSyncConfig"/> to fine-tune how Rock-to-Chat synchronization should be completed.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> UpdateChatChannelTypesAsync( List<ChatChannelType> chatChannelTypes, RockToChatSyncConfig config );

        /// <summary>
        /// Deletes the specified <see cref="ChatChannelType"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypeKeys">The list of keys for the <see cref="ChatChannelType"/>s to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> DeleteChatChannelTypesAsync( List<string> chatChannelTypeKeys );

        #endregion Chat Channel Types

        #region Chat Channels

        /// <summary>
        /// Gets the "queryable" chat channel key for the provided <see cref="GroupCache"/>, to be used when querying the
        /// external chat system's API for an existing <see cref="ChatChannel"/>.
        /// </summary>
        /// <param name="rockChatGroup">The <see cref="RockChatGroup"/> for which to get the "queryable" chat channel key.</param>
        /// <returns>The "queryable" chat channel key.</returns>
        /// <remarks>
        /// For some chat providers, this might be the same value as <see cref="ChatChannel.Key"/>.
        /// </remarks>
        string GetQueryableChatChannelKey( RockChatGroup rockChatGroup );

        /// <summary>
        /// Gets a list of <see cref="ChatChannel"/>s from the external chat system that match the provided keys.
        /// </summary>
        /// <param name="queryableChatChannelKeys">The list of queryable keys for the <see cref="ChatChannel"/>s to get.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelsResult"/>.
        /// </returns>
        Task<GetChatChannelsResult> GetChatChannelsAsync( List<string> queryableChatChannelKeys );

        /// <summary>
        /// Gets a list of all <see cref="ChatChannel"/>s from the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelsResult"/>.
        /// </returns>
        Task<GetChatChannelsResult> GetAllChatChannelsAsync();

        /// <summary>
        /// Creates new <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannels">The list of <see cref="ChatChannel"/>s to create.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> CreateChatChannelsAsync( List<ChatChannel> chatChannels );

        /// <summary>
        /// Updates existing <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannels">The list of <see cref="ChatChannel"/>s to update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> UpdateChatChannelsAsync( List<ChatChannel> chatChannels );

        /// <summary>
        /// Deletes the specified <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelQueryableKeys">The list of queryable keys for the <see cref="ChatChannel"/>s to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> DeleteChatChannelsAsync( List<string> chatChannelQueryableKeys );

        #endregion Chat Channels

        #region Chat Channel Members

        /// <summary>
        /// Gets a list of <see cref="ChatChannelMember"/>s from the external chat system who match the provided
        /// <see cref="ChatChannelType"/> and <see cref="ChatChannel"/> key combination.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for the members to get.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for the members to get.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelMembersResult"/>.
        /// </returns>
        Task<GetChatChannelMembersResult> GetAllChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey );

        /// <summary>
        /// Gets a list of <see cref="ChatChannelMember"/>s from the external chat system who match the provided
        /// <see cref="ChatChannelType"/>, <see cref="ChatChannel"/> and <see cref="ChatUser"/> key combinations.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for the members to get.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for the members to get.</param>
        /// <param name="chatUserKeys">The list of <see cref="ChatUser"/> keys for the members to get.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelMembersResult"/>.
        /// </returns>
        Task<GetChatChannelMembersResult> GetChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys );

        /// <summary>
        /// Creates new <see cref="ChatChannelMember"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for which members should be created.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for which members should be created.</param>
        /// <param name="chatChannelMembers">The list of <see cref="ChatChannelMember"/>s to create.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> CreateChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<ChatChannelMember> chatChannelMembers );

        /// <summary>
        /// Updates existing <see cref="ChatChannelMember"/>s in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for which members should be updated.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for which members should be updated.</param>
        /// <param name="chatChannelMembers">The dictionary of <see cref="ChatChannelMember"/>s to update, with each key
        /// being the latest instance and its corresponding value being the previous instance.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// The <paramref name="chatChannelMembers"/> are passed as a dictionary so the chat provider can examine the old
        /// vs. new instances for specific changes and decide the best way to apply the changes.
        /// </remarks>
        Task<ChatSyncCrudResult> UpdateChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, Dictionary<ChatChannelMember, ChatChannelMember> chatChannelMembers );

        /// <summary>
        /// Deletes <see cref="ChatChannelMember"/>s from the external chat system that match the provided
        /// <see cref="ChatChannelType"/>, <see cref="ChatChannel"/> and <see cref="ChatUser"/> key combinations.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for which members should be deleted.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for which members should be deleted.</param>
        /// <param name="chatUserKeys">The keys of the <see cref="ChatUser"/>s for whom members should be deleted.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> DeleteChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys );

        /// <summary>
        /// Mutes the <see cref="ChatChannel"/> that matches the provided <see cref="ChatChannelType"/> and
        /// <see cref="ChatChannel"/> key combination, for the specified <see cref="ChatUser"/>s.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> that should be muted.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> that should be muted.</param>
        /// <param name="chatUserKeys">The keys of the <see cref="ChatUser"/>s for whom the channel should be muted.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncMuteResult"/>.
        /// </returns>
        Task<ChatSyncMuteResult> MuteChatChannelAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys );

        /// <summary>
        /// Unmutes the <see cref="ChatChannel"/> that matches the provided <see cref="ChatChannelType"/> and
        /// <see cref="ChatChannel"/> key combination, for the specified <see cref="ChatUser"/>s.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> that should be unmuted.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> that should be unmuted.</param>
        /// <param name="chatUserKeys">The keys of the <see cref="ChatUser"/>s for whom the channel should be unmuted.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncMuteResult"/>.
        /// </returns>
        Task<ChatSyncMuteResult> UnmuteChatChannelAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys );

        #endregion Chat Channel Members

        #region Chat Users

        /// <summary>
        /// Ensures that the system user - responsible for performing synchronization tasks - exists in the external
        /// chat system. Creates the user if necessary.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/> indicating
        /// whether the system user exists in the external chat system.
        /// </returns>
        Task<ChatSyncSetupResult> EnsureSystemUserExistsAsync();

        /// <summary>
        /// Gets a list of all <see cref="ChatUser.Key"/>s from the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatKeysResult"/>.
        /// </returns>
        Task<GetChatKeysResult> GetAllChatUserKeysAsync();

        /// <summary>
        /// Gets a list of <see cref="ChatUser"/>s from the external chat system that match the provided keys.
        /// </summary>
        /// <param name="chatUserKeys">The list of keys for the <see cref="ChatUser"/>s to get.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatUsersResult"/>.
        /// </returns>
        Task<GetChatUsersResult> GetChatUsersAsync( List<string> chatUserKeys );

        /// <summary>
        /// Creates new <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUsers">The list of <see cref="ChatUser"/>s to create.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> CreateChatUsersAsync( List<ChatUser> chatUsers );

        /// <summary>
        /// Updates existing <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUsers">The list of <see cref="ChatUser"/>s to update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> UpdateChatUsersAsync( List<ChatUser> chatUsers );

        /// <summary>
        /// Deletes the specified <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUserKeys">The list of keys for the <see cref="ChatUser"/>s to delete.</param>
        /// <param name="newChatUserKey">The optional key of the <see cref="ChatUser"/> to whom ownership of the deleted
        /// users' external chat system resources should be transferred.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        Task<ChatSyncCrudResult> DeleteChatUsersAsync( List<string> chatUserKeys, string newChatUserKey = null );

        /// <summary>
        /// Bans the specified <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUserKeys">The list of keys for the <see cref="ChatUser"/>s to ban.</param>
        /// <param name="chatChannelTypeKey">The optional key for the <see cref="ChatChannelType"/> in which the
        /// <see cref="ChatUser"/>s should be banned.</param>
        /// <param name="chatChannelKey">The optional key for the <see cref="ChatChannel"/> in which the
        /// <see cref="ChatUser"/>s should be banned.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncBanResult"/>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="chatChannelTypeKey"/> and <paramref name="chatChannelKey"/> are not both provided, bans
        /// will be applied globally.
        /// </remarks>
        Task<ChatSyncBanResult> BanChatUsersAsync( List<string> chatUserKeys, string chatChannelTypeKey = null, string chatChannelKey = null );

        /// <summary>
        /// Unbans the specified <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUserKeys">The list of keys for the <see cref="ChatUser"/>s to unban.</param>
        /// <param name="chatChannelTypeKey">The optional key for the <see cref="ChatChannelType"/> in which the
        /// <see cref="ChatUser"/>s should be unbanned.</param>
        /// <param name="chatChannelKey">The optional key for the <see cref="ChatChannel"/> in which the
        /// <see cref="ChatUser"/>s should be unbanned.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncBanResult"/>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="chatChannelTypeKey"/> and <paramref name="chatChannelKey"/> are not both provided, bans
        /// will be lifted globally.
        /// </remarks>
        Task<ChatSyncBanResult> UnbanChatUsersAsync( List<string> chatUserKeys, string chatChannelTypeKey = null, string chatChannelKey = null );

        /// <summary>
        /// Gets a token for the <see cref="ChatUser"/> to use when authenticating with the chat provider.
        /// </summary>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/> for which to get a token.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatStringResult"/>.
        /// </returns>
        Task<GetChatStringResult> GetChatUserTokenAsync( string chatUserKey );

        #endregion Chat Users

        #region Messages

        /// <summary>
        /// Gets the message counts for each <see cref="ChatUser"/> within each <see cref="ChatChannel"/>, for the specified date.
        /// </summary>
        /// <param name="messageDate">The date for which to get message counts.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatUserMessageCountsByChatChannelResult"/>.
        /// </returns>
        Task<ChatUserMessageCountsByChatChannelResult> GetChatUserMessageCountsByChatChannelKeyAsync( DateTime messageDate );

        #endregion Messages

        #region Webhooks

        /// <summary>
        /// Gets whether the provided <see cref="HttpRequestMessage"/> is a valid webhook request from the external chat
        /// provider.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="WebhookValidationResult"/>.
        /// </returns>
        Task<WebhookValidationResult> ValidateWebhookRequestAsync( HttpRequestMessage request );

        /// <summary>
        /// Gets a list of <see cref="ChatToRockSyncCommand"/>s for the provided <see cref="ChatWebhookRequest"/>s.
        /// </summary>
        /// <param name="webhookRequests">The list of <see cref="ChatWebhookRequest"/>s for which to get
        /// <see cref="ChatToRockSyncCommand"/>s.</param>
        /// <returns>
        /// A <see cref="GetChatToRockSyncCommandsResult"/> that contains a list of <see cref="ChatToRockSyncCommand"/>s,
        /// one for each valid <see cref="ChatWebhookRequest"/>.
        /// </returns>
        /// <remarks>
        /// Invalid <see cref="ChatWebhookRequest"/>s will not have corresponding <see cref="ChatToRockSyncCommand"/>s,
        /// with failure details instead being added to Rock Logs.
        /// </remarks>
        GetChatToRockSyncCommandsResult GetChatToRockSyncCommands( List<ChatWebhookRequest> webhookRequests );

        #endregion Webhooks
    }
}
