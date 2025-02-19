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
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Exceptions;
using Rock.Enums.Communication.Chat;
using Rock.Logging;
using Rock.Model;

using StreamChat.Clients;
using StreamChat.Exceptions;
using StreamChat.Models;

namespace Rock.Communication.Chat
{
    /// <summary>
    /// A <see cref="IChatProvider"/> implementation for interacting with the Stream chat provider.
    /// </summary>
    [RockLoggingCategory]
    internal class StreamChatProvider : IChatProvider
    {
        #region Keys

        /// <summary>
        /// Custom field keys for Stream channels.
        /// </summary>
        private static class ChannelDataKey
        {
            public const string Name = "name";
            public const string IsLeavingAllowed = "rock_leaving_allowed";
            public const string IsPublic = "rock_public";
            public const string IsAlwaysShown = "rock_always_shown";
        }

        /// <summary>
        /// Custom field keys for Stream users;
        /// </summary>
        private static class UserDataKey
        {
            public const string Name = "name";
            public const string Image = "image";
            public const string IsProfileVisible = "rock_profile_public";
            public const string IsOpenDirectMessageAllowed = "rock_open_direct_message_allowed";
            public const string Badges = "rock_badges";
        }

        /// <summary>
        /// Sort parameter field keys for Stream query responses.
        /// </summary>
        private static class SortParameterFieldKey
        {
            public const string CreatedAt = "created_at";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="Logger"/> property.
        /// </summary>
        private readonly Lazy<ILogger> _logger;

        // Per Stream: https://github.com/GetStream/stream-chat-net
        // All client instances can be used as a singleton for the lifetime of your application as they don't maintain state.

        /// <summary>
        /// Provides access to the <see cref="StreamClientFactory"/> for creating specific API clients.
        /// </summary>
        private readonly Lazy<StreamClientFactory> _clientFactory;

        /// <summary>
        /// The backing field for the <see cref="AppClient"/> property.
        /// </summary>
        private readonly Lazy<IAppClient> _appClient;

        /// <summary>
        /// The backing field for the <see cref="PermissionClient"/> property.
        /// </summary>
        private readonly Lazy<IPermissionClient> _permissionClient;

        /// <summary>
        /// The backing field for the <see cref="ChannelTypeClient"/> property.
        /// </summary>
        private readonly Lazy<IChannelTypeClient> _channelTypeClient;

        /// <summary>
        /// The backing field for the <see cref="ChannelClient"/> property.
        /// </summary>
        private readonly Lazy<IChannelClient> _channelClient;

        /// <summary>
        /// The backing field for the <see cref="UserClient"/> property.
        /// </summary>
        private readonly Lazy<IUserClient> _userClient;

        // TODO (Jason): Carefully evaluate the default grants-per-role, and decide if the below values are what we want.
        // https://getstream.io/chat/docs/rest/#/product%3Achat/GetApp
        // https://getstream.io/chat/docs/dotnet-csharp/permissions_reference/#scope-app

        /// <summary>
        /// The backing field for the <see cref="DefaultAppGrantsByRole"/> property.
        /// </summary>
        private readonly Lazy<Dictionary<string, List<string>>> _defaultAppGrantsByRole = new Lazy<Dictionary<string, List<string>>>( () =>
        {
            return new Dictionary<string, List<string>>
            {
                {
                    ChatRole.User.GetDescription(),
                    new List<string>
                    {
                        "close-poll-owner",
                        "create-poll-any-team",
                        "delete-poll-owner",
                        "flag-user",
                        "mute-user",
                        "query-polls-owner",
                        "search-user",
                        "update-poll-owner",
                        "update-user-owner"
                    }
                },
                {
                    ChatRole.Administrator.GetDescription(),
                    new List<string>
                    {
                        "close-poll-any-team",
                        "create-poll-any-team",
                        "delete-moderation-config-any-team",
                        "delete-poll-any-team",
                        "flag-user-any-team",
                        "mute-user-any-team",
                        "query-moderation-review-queue-any-team",
                        "query-polls-owner-any-team",
                        "read-flag-reports-any-team",
                        "read-moderation-config-any-team",
                        "search-user-any-team",
                        "submit-moderation-action-any-team",
                        "update-flag-report-any-team",
                        "update-poll-any-team",
                        "update-user-owner",
                        "upsert-moderation-config-any-team"
                    }
                }
            };
        } );

        // TODO (Jason): Carefully evaluate the default grants-per-role, and decide if the below values are what we
        // want (focus on the `messaging` channel type).
        // https://getstream.io/chat/docs/rest/#/product%3Achat/ListChannelTypes
        // https://getstream.io/chat/docs/dotnet-csharp/permissions_reference/#scope-messaging
        //
        // Also, should we combine some of the default permission grants sets? For example:
        //  1. Should the default `moderator` and `channel_moderator` grants be combined to form our
        //     `rock_moderator` channel type grants set?
        //  2. Should the default `channel_member` and `user` grants be combined to form our `rock_user` channel
        //     type grants set?

        /// <summary>
        /// The backing field for the <see cref="DefaultChannelTypeGrantsByRole"/> property.
        /// </summary>
        private readonly Lazy<Dictionary<string, List<string>>> _defaultChannelTypeGrantsByRole = new Lazy<Dictionary<string, List<string>>>( () =>
        {
            return new Dictionary<string, List<string>>
            {
                {
                    ChatRole.User.GetDescription(),
                    new List<string>
                    {
                        "add-links",
                        "cast-vote",
                        "create-attachment",
                        "create-call",
                        "create-mention",
                        "create-message",
                        "create-reaction",
                        "create-reply",
                        "flag-message",
                        "join-call",
                        "mute-channel",
                        "pin-message",
                        "query-votes",
                        "read-channel",
                        "read-channel-members",
                        "remove-own-channel-membership",
                        "run-message-action",
                        "send-custom-event",
                        "send-poll",
                        "upload-attachment"
                    }
                },
                {
                    ChatRole.Moderator.GetDescription(),
                    new List<string>
                    {
                        "add-links",
                        "ban-channel-member",
                        "cast-vote",
                        "create-attachment",
                        "create-call",
                        "create-mention",
                        "create-message",
                        "create-reaction",
                        "create-reply",
                        "create-system-message",
                        "delete-attachment",
                        "delete-message",
                        "delete-reaction",
                        "flag-message",
                        "join-call",
                        "mute-channel",
                        "pin-message",
                        "query-votes",
                        "read-channel",
                        "read-channel-members",
                        "read-message-flags",
                        "remove-own-channel-membership",
                        "run-message-action",
                        "send-custom-event",
                        "send-poll",
                        "skip-channel-cooldown",
                        "skip-message-moderation",
                        "unblock-message",
                        "update-channel",
                        "update-channel-cooldown",
                        "update-channel-frozen",
                        "update-channel-members",
                        "update-message",
                        "update-thread",
                        "upload-attachment"
                    }
                },
                {
                    ChatRole.Administrator.GetDescription(),
                    new List<string>
                    {
                        "add-links",
                        "ban-channel-member",
                        "ban-user",
                        "cast-vote",
                        "create-attachment",
                        "create-call",
                        "create-channel",
                        "create-mention",
                        "create-message",
                        "create-reaction",
                        "create-reply",
                        "create-system-message",
                        "delete-attachment",
                        "delete-channel",
                        "delete-message",
                        "delete-reaction",
                        "flag-message",
                        "join-call",
                        "mute-channel",
                        "pin-message",
                        "query-votes",
                        "read-channel",
                        "read-channel-members",
                        "read-message-flags",
                        "recreate-channel",
                        "remove-own-channel-membership",
                        "run-message-action",
                        "send-custom-event",
                        "send-poll",
                        "skip-channel-cooldown",
                        "skip-message-moderation",
                        "truncate-channel",
                        "unblock-message",
                        "update-channel",
                        "update-channel-cooldown",
                        "update-channel-frozen",
                        "update-channel-members",
                        "update-message",
                        "update-thread",
                        "upload-attachment"
                    }
                }
            };
        } );

        /// <summary>
        /// The backing field for the <see cref="UnrecoverableErrorCodes"/> property.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Taken from: https://getstream.io/chat/docs/dotnet-csharp/api_errors_response/
        /// </para>
        /// <para>
        /// ALL error codes are represented here; those that are considered "recoverable" have been commented out.
        /// </para>
        /// </remarks>
        private readonly Lazy<List<int>> _unrecoverableErrorCodes = new Lazy<List<int>>( () =>
        {
            return new List<int>
            {
                -1, // Internal System Error: Triggered when something goes wrong in [Stream's] system
                2, // Access Key Error: Access Key invalid
                4, // Input Error: When wrong data/parameter is sent to the API
                5, // Authentication Error: Unauthenticated, problem with authentication
                6, // Duplicate Username Error: When a duplicate username is sent while enforce_unique_usernames is enabled
                //9, // Rate Limit Error: Too many requests in a certain time frame
                16, // Does Not Exist Error: Resource not found
                17, // Not Allowed Error: Unauthorized / forbidden to make request
                18, // Event Not Supported Error: Event is not supported
                19, // Channel Feature Not Supported Error: The feature is currently disabled on the dashboard (i.e. Reactions & Replies)
                20, // Message Too Long Error: Message is too long
                21, // Multiple Nesting Level Error: Multiple Levels Reply is not supported - the API only supports 1 level deep reply threads
                22, // Payload Too Big Error: Payload too big
                23, // Request Timeout Error: Request timed out
                24, // Maximum Header Size Exceeded Error: Request headers are too large
                44, // Custom Command Endpoint Missing Error: App config does not have custom_action_handler_url
                45, // Custom Command Endpoint Call Error: Custom Command handler returned an error
                40, // Authentication Token Expired: Unauthenticated, token expired
                41, // Authentication Token Not Valid Yet: Unauthenticated, token not valid yet
                42, // Authentication Token Before Issued At: Unauthenticated, token date incorrect
                43, // Authentication Token Signature Invalid: Unauthenticated, token signature invalid
                60, // Cooldown Error: User tried to post a message during the cooldown period
                70, // No Access to Channels: No access to requested channels
                73, // Message Moderation Failed: Message did not pass moderation
                99, // App Suspended Error: App suspended
            };
        } );

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the <see cref="ILogger"/> that should be used to write log messages for this chat provider.
        /// </summary>
        private ILogger Logger => _logger.Value;

        /// <summary>
        /// Gets the log message prefix to be used for all log messages.
        /// </summary>
        private string LogMessagePrefix => $"{nameof( StreamChatProvider ).SplitCase()} >";

        /// <summary>
        /// Gets the <see cref="IAppClient"/> that should be used to manage the app within the external Stream chat
        /// system.
        /// </summary>
        private IAppClient AppClient => _appClient.Value;

        /// <summary>
        /// Gets the <see cref="IPermissionClient"/> that should be used to manage roles and permissions within the
        /// external Stream chat system.
        /// </summary>
        private IPermissionClient PermissionClient => _permissionClient.Value;

        /// <summary>
        /// Gets the <see cref="IChannelTypeClient"/> that should be used to manage channel types within the external
        /// Stream chat system.
        /// </summary>
        private IChannelTypeClient ChannelTypeClient => _channelTypeClient.Value;

        /// <summary>
        /// Gets the <see cref="IChannelClient"/> that should be used to manage channels within the external Stream chat
        /// system.
        /// </summary>
        private IChannelClient ChannelClient => _channelClient.Value;

        /// <summary>
        /// Gets the <see cref="IUserClient"/> that should be used to manage users within the external Stream chat
        /// system.
        /// </summary>
        private IUserClient UserClient => _userClient.Value;

        /// <summary>
        /// Gets the "unrecoverable" error codes that can be returned by Stream's API.
        /// </summary>
        private List<int> UnrecoverableErrorCodes => _unrecoverableErrorCodes.Value;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes an instance of the <see cref="StreamChatProvider"/> class.
        /// </summary>
        public StreamChatProvider()
        {
            _logger = new Lazy<ILogger>( () =>
                RockLogger.LoggerFactory.CreateLogger( typeof( StreamChatProvider ).FullName )
            );

            _clientFactory = new Lazy<StreamClientFactory>( () =>
            {
                var chatConfiguration = ChatHelper.GetChatConfiguration();
                return new StreamClientFactory( chatConfiguration.ApiKey, chatConfiguration.ApiSecret );
            } );

            _appClient = new Lazy<IAppClient>( () => _clientFactory.Value.GetAppClient() );
            _permissionClient = new Lazy<IPermissionClient>( () => _clientFactory.Value.GetPermissionClient() );
            _channelTypeClient = new Lazy<IChannelTypeClient>( () => _clientFactory.Value.GetChannelTypeClient() );
            _channelClient = new Lazy<IChannelClient>( () => _clientFactory.Value.GetChannelClient() );
            _userClient = new Lazy<IUserClient>( () => _clientFactory.Value.GetUserClient() );
        }

        #endregion Constructors

        #region IChatProvider Implementation

        #region Roles & Permission Grants

        /// <inheritdoc/>
        public Dictionary<string, List<string>> DefaultAppGrantsByRole => _defaultAppGrantsByRole.Value;

        /// <inheritdoc/>
        public Dictionary<string, List<string>> DefaultChannelTypeGrantsByRole => _defaultChannelTypeGrantsByRole.Value;

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any roles fail to be created.</exception>
        public async Task EnsureAppRolesExistAsync()
        {
            var operationName = nameof( EnsureAppRolesExistAsync ).SplitCase();

            var listRolesResponse = await RetryAsync(
                async () => await PermissionClient.ListRolesAsync(),
                operationName
            );

            var existingRoles = listRolesResponse?.Roles ?? new List<CustomRole>();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var requiredRole in ChatHelper.RequiredAppRoles )
            {
                // The Stream client will throw an exception if the role already exists, so we'll do our best to avoid this.
                if ( !existingRoles.Any( r => r.Name == requiredRole ) )
                {
                    try
                    {
                        await RetryAsync(
                            async () => await PermissionClient.CreateRoleAsync( requiredRole ),
                            operationName
                        );
                    }
                    catch ( Exception ex )
                    {
                        exceptions.Add( ex );
                    }
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }
        }

        /// <inheritdoc/>
        public async Task EnsureAppGrantsExistAsync()
        {
            var operationName = nameof( EnsureAppGrantsExistAsync ).SplitCase();

            var getAppResponse = await RetryAsync(
                async () => await AppClient.GetAppSettingsAsync(),
                operationName
            );

            var anyGrantsToAdd = false;
            var grants = getAppResponse?.App?.Grants;
            if ( grants?.Any() != true || ShouldEnforceDefaultGrantsPerRole )
            {
                if ( grants == null )
                {
                    // This will probably never happen, but for some reason the app doesn't have ANY grants.
                    grants = new Dictionary<string, List<string>>();
                }

                // Add or overwrite `rock_` grants.
                foreach ( var kvp in DefaultAppGrantsByRole )
                {
                    grants.AddOrReplace( kvp.Key, kvp.Value );
                }

                anyGrantsToAdd = true;
            }
            else
            {
                /*
                    2/3/2025 - JPH

                    When an app already has at least one app-scoped permission grant defined in the external chat system,
                    the only reason we would currently want to "update" the app is if we detect that ANY of the `rock_`
                    roles don't already have at least one grant defined. If ALL roles already have at least one grant
                    defined, this means [A] we've already synced this app at least once, and [B] the external chat system
                    is now the system of truth for permission grants (as admins may fine-tune grants on that side).

                    Reason: Avoid calling chat provider APIs when not needed & avoid overwriting existing App Grants.
                 */

                foreach ( var role in ChatHelper.RequiredAppRoles )
                {
                    grants.TryGetValue( role, out var existingRoleGrants );
                    if ( existingRoleGrants?.Any() == true )
                    {
                        // There's already at least one grant for this role; don't overwrite.
                        continue;
                    }

                    DefaultAppGrantsByRole.TryGetValue( role, out var defaultRoleGrants );
                    if ( defaultRoleGrants?.Any() == true )
                    {
                        // Add the default grants for this role.
                        grants.AddOrReplace( role, defaultRoleGrants );
                        anyGrantsToAdd = true;
                    }
                }
            }

            if ( anyGrantsToAdd )
            {
                await RetryAsync(
                    async () => await AppClient.UpdateAppSettingsAsync( new AppSettingsRequest
                    {
                        Grants = grants
                    } ),
                    operationName
                );
            }
        }

        #endregion Roles & Permission Grants

        #region Chat Channel Types

        /// <inheritdoc/>
        public async Task<List<ChatChannelType>> GetAllChatChannelTypesAsync()
        {
            var operationName = nameof( GetAllChatChannelTypesAsync ).SplitCase();

            var listChannelTypesResponse = await RetryAsync(
                async () => await ChannelTypeClient.ListChannelTypesAsync(),
                operationName
            );

            return listChannelTypesResponse
                ?.ChannelTypes
                ?.Values
                .Select( ct => TryConvertToChatChannelType( ct ) )
                .Where( ct => ct != null )
                .ToList();
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannelType"/>s fail to be created.</exception>
        public async Task<List<ChatChannelType>> CreateChatChannelTypesAsync( List<ChatChannelType> chatChannelTypes )
        {
            var results = new List<ChatChannelType>();

            if ( chatChannelTypes?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( CreateChatChannelTypesAsync ).SplitCase();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatChannelType in chatChannelTypes )
            {
                // Always set default property values and permission grants for newly-created channel types.
                var request = TryConvertToStreamChannelTypeWithStringCommandsRequest( chatChannelType, true, true );
                if ( request == null )
                {
                    continue;
                }

                try
                {
                    await RetryAsync(
                        async () => await ChannelTypeClient.CreateChannelTypeAsync( request ),
                        operationName
                    );

                    results.Add( chatChannelType );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannelType"/>s fail to be updated.</exception>
        public async Task<List<ChatChannelType>> UpdateChatChannelTypesAsync( List<ChatChannelType> chatChannelTypes )
        {
            var results = new List<ChatChannelType>();

            if ( chatChannelTypes?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( UpdateChatChannelTypesAsync ).SplitCase();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatChannelType in chatChannelTypes )
            {
                // Only set default property values and/or permission grants for updates if explicitly instructed to do so.
                var request = TryConvertToStreamChannelTypeWithStringCommandsRequest( chatChannelType, ShouldEnforceDefaultSettings, ShouldEnforceDefaultGrantsPerRole );
                if ( request == null )
                {
                    continue;
                }

                // Clear the `Name` to prevent an exception being thrown by the Stream API.
                request.Name = default;

                try
                {
                    await RetryAsync(
                        async () => await ChannelTypeClient.UpdateChannelTypeAsync( chatChannelType.Key, request ),
                        operationName
                    );

                    results.Add( chatChannelType );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannelType"/>s fail to be deleted.</exception>
        public async Task<List<string>> DeleteChatChannelTypesAsync( List<string> chatChannelTypeKeys )
        {
            var results = new List<string>();

            if ( chatChannelTypeKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( DeleteChatChannelTypesAsync ).SplitCase();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var key in chatChannelTypeKeys.Where( k => k.IsNotNullOrWhiteSpace() ) )
            {
                try
                {
                    // Stream channels tied to this channel type are NOT cascade-deleted, so we need to first delete
                    // those to prevent an error response when deleting the channel type.
                    await DeleteChatChannelsOfTypeAsync( key );

                    await RetryAsync(
                        async () => await ChannelTypeClient.DeleteChannelTypeAsync( key ),
                        operationName
                    );

                    results.Add( key );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        #endregion Chat Channel Types

        #region Chat Channels

        /// <inheritdoc/>
        public string GetQueryableChatChannelKey( Group group )
        {
            if ( ( group?.ChatChannelKey ).IsNotNullOrWhiteSpace() )
            {
                return group.ChatChannelKey;
            }

            if ( group?.Id < 1 || group.GroupTypeId < 1 )
            {
                return null;
            }

            // Per Stream: Querying by the channel Identifier should be done using the cid field as far as possible to
            // optimize API performance. As the full channel ID, cid is indexed everywhere in Stream database where id
            // is not (https://getstream.io/chat/docs/dotnet-csharp/query_channels/#channel-queryable-built-in-fields).
            return $"{ChatHelper.GetChatChannelTypeKey( group.GroupTypeId )}:{ChatHelper.GetChatChannelKey( group.Id )}";
        }

        /// <inheritdoc/>
        public async Task<List<ChatChannel>> GetChatChannelsAsync( List<string> chatChannelKeys )
        {
            var results = new List<ChatChannel>();

            if ( chatChannelKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( GetChatChannelsAsync ).SplitCase();

            // Max number of channels to get per query is 30.
            // https://getstream.io/chat/docs/dotnet-csharp/query_channels/#query-options
            var pageSize = 30;
            var offset = 0;
            var channelsToGetCount = chatChannelKeys.Count;

            // Keep track of channel keys already seen, to prevent the possibility of duplicates returned.
            var seenChannelKeys = new HashSet<string>();

            // With a query request, we want "all or nothing". So if any batches fail, allow an exception to be thrown.
            while ( offset < channelsToGetCount )
            {
                var batchedKeys = chatChannelKeys
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                var queryChannelsOptions = new QueryChannelsOptions() { MemberLimit = 0, MessageLimit = 0 }
                    .WithLimit( pageSize )
                    .WithFilter(
                        new Dictionary<string, object>
                        {
                            { "cid", new Dictionary<string, object> { { "$in", batchedKeys } } }
                        }
                    )
                    .WithSortBy(
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    );

                var queryChannelResponse = await RetryAsync(
                    async () => await ChannelClient.QueryChannelsAsync( queryChannelsOptions ),
                    operationName
                );

                if ( queryChannelResponse?.Channels?.Any() == true )
                {
                    results.AddRange(
                        queryChannelResponse.Channels
                            .Select( c => TryConvertToChatChannel( c ) )
                            .Where( c =>
                                c != null
                                && seenChannelKeys.Add( c.Key )
                            )
                    );
                }

                offset += pageSize;
            }

            return results;
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannel"/>s fail to be created.</exception>
        public async Task<List<ChatChannel>> CreateChatChannelsAsync( List<ChatChannel> chatChannels )
        {
            var results = new List<ChatChannel>();

            if ( chatChannels?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( CreateChatChannelsAsync ).SplitCase();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatChannel in chatChannels )
            {
                var request = TryConvertToStreamChannelGetRequest( chatChannel );
                if ( request == null )
                {
                    continue;
                }

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.GetOrCreateAsync( chatChannel.ChatChannelTypeKey, chatChannel.Key, request ),
                        operationName
                    );

                    results.Add( chatChannel );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannel"/>s fail to be updated.</exception>
        public async Task<List<ChatChannel>> UpdateChatChannelsAsync( List<ChatChannel> chatChannels )
        {
            var results = new List<ChatChannel>();

            if ( chatChannels?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( UpdateChatChannelsAsync ).SplitCase();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatChannel in chatChannels )
            {
                var request = TryConvertToStreamChannelUpdateRequest( chatChannel );
                if ( request == null )
                {
                    continue;
                }

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.UpdateAsync( chatChannel.ChatChannelTypeKey, chatChannel.Key, request ),
                        operationName
                    );

                    results.Add( chatChannel );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any <see cref="ChatChannel"/>s fail to be deleted.</exception>
        public async Task<List<string>> DeleteChatChannelsAsync( List<string> chatChannelKeys )
        {
            var results = new List<string>();

            if ( chatChannelKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( DeleteChatChannelsAsync ).SplitCase();

            // Max number of channels to delete per batch is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/channel_delete/#deleting-many-channels
            var pageSize = 100;
            var offset = 0;
            var channelsToDeleteCount = chatChannelKeys.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < channelsToDeleteCount )
            {
                var batchedKeys = chatChannelKeys
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                try
                {
                    // Stream channel members and messages tied to this channel ARE cascade-deleted when deleting this
                    // channel, so there is no need to manually delete those first.

                    await RetryAsync(
                        // The 2nd argument is `hardDelete = true`: By default, messages are soft deleted, which means
                        // they are removed from client but are still available via server-side export functions. You
                        // can also hard delete messages, which deletes them from everywhere, by setting "hard_delete":
                        // true in the request. Messages that have been soft or hard deleted cannot be recovered.
                        // https://getstream.io/chat/docs/dotnet-csharp/channel_delete/#deleting-many-channels
                        async () => await ChannelClient.DeleteChannelsAsync( batchedKeys, true ),
                        operationName
                    );

                    results.AddRange( batchedKeys );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
                finally
                {
                    offset += pageSize;
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <summary>
        /// Deletes <see cref="ChatChannel"/>s of the specified <see cref="ChatChannelType"/> in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> whose <see cref="ChatChannel"/>s
        /// should be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="AggregateException">If any <see cref="ChatChannel"/>s fail to be deleted.</exception>
        private async Task DeleteChatChannelsOfTypeAsync( string chatChannelTypeKey )
        {
            var operationName = nameof( DeleteChatChannelsOfTypeAsync ).SplitCase();

            // We'll get and delete these channels in batches of 30, as the query max of 30 is the limiting factor.
            // https://getstream.io/chat/docs/dotnet-csharp/query_channels/#query-options
            var pageSize = 30;
            var offset = 0;
            var channelsRetrievedCount = 0;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            do
            {
                var queryChannelsOptions = new QueryChannelsOptions { MemberLimit = 0, MessageLimit = 0 }
                    .WithOffset( offset )
                    .WithLimit( pageSize )
                    .WithFilter(
                        new Dictionary<string, object>
                        {
                            { "type", chatChannelTypeKey }
                        }
                    )
                    .WithSortBy(
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    );

                try
                {
                    var queryChannelResponse = await RetryAsync(
                        async () => await ChannelClient.QueryChannelsAsync( queryChannelsOptions ),
                        operationName
                    );

                    List<string> chatChannelKeys = null;
                    if ( queryChannelResponse?.Channels?.Any() == true )
                    {
                        channelsRetrievedCount = queryChannelResponse.Channels.Count;
                        chatChannelKeys = new List<string>(
                            queryChannelResponse.Channels
                                .Where( c => ( c?.Channel?.Cid ).IsNotNullOrWhiteSpace() )
                                .Select( c => c.Channel.Cid )
                                .Distinct()
                        );
                    }
                    else
                    {
                        channelsRetrievedCount = 0;
                    }

                    if ( chatChannelKeys?.Any() == true )
                    {
                        await DeleteChatChannelsAsync( chatChannelKeys );
                    }
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );

                    // Don't get stuck in an infinite loop, but try at least once more to see if we can get past this
                    // failed batch.
                    channelsRetrievedCount = pageSize;
                }
                finally
                {
                    offset += channelsRetrievedCount;
                }
            }
            while ( channelsRetrievedCount == pageSize );

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }
        }

        #endregion Chat Channels

        #region Chat Channel Members

        /// <inheritdoc/>
        public async Task<List<ChatChannelMember>> GetChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey )
        {
            var results = new List<ChatChannelMember>();

            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() )
            {
                return results;
            }

            var operationName = nameof( GetChatChannelMembersAsync ).SplitCase();

            // Max number of members to get per query is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/query_members/#query-options
            var pageSize = 100;
            var offset = 0;
            var membersRetrievedCount = 0;

            // Keep track of user keys already seen, to prevent the possibility of duplicates returned.
            var seenChatUserKeys = new HashSet<string>();

            // With a query request, we want "all or nothing". So if any batches fail, allow an exception to be thrown.
            do
            {
                var queryMembersRequest = new QueryMembersRequest
                {
                    Limit = pageSize,
                    Offset = offset,
                    Type = chatChannelTypeKey,
                    Id = chatChannelKey,
                    FilterConditions = new Dictionary<string, object>(), // Exception thrown if not defined.
                    Sorts = new List<SortParameter>
                    {
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    }
                };

                try
                {
                    var queryMembersResponse = await RetryAsync(
                        async () => await ChannelClient.QueryMembersAsync( queryMembersRequest ),
                        operationName
                    );

                    if ( queryMembersResponse?.Members?.Any() == true )
                    {
                        membersRetrievedCount = queryMembersResponse.Members.Count;
                        results.AddRange(
                            queryMembersResponse.Members
                                .Select( m => TryConvertToChatChannelMember( m ) )
                                .Where( m =>
                                    m != null
                                    && seenChatUserKeys.Add( m.ChatUserKey )
                                )
                        );
                    }
                    else
                    {
                        membersRetrievedCount = 0;
                    }

                    offset += membersRetrievedCount;
                }
                catch ( Exception ex )
                {
                    if ( ex is StreamChatException streamChatException && streamChatException.ErrorCode == 16 )
                    {
                        // Provide the caller with a predictable exception so they can decide how to proceed when the
                        // targeted channel does not exist.
                        throw new ChatChannelNotFoundException(
                            $"{LogMessagePrefix} {operationName} failed: channel does not exist in Stream (type: {chatChannelTypeKey}; id: {chatChannelKey}).",
                            ex,
                            chatChannelTypeKey,
                            chatChannelKey
                        );
                    }

                    throw;
                }
            }
            while ( membersRetrievedCount == pageSize );

            return results;
        }

        /// <inheritdoc/>
        public async Task<ChatChannelMember> GetChatChannelMemberAsync( string chatChannelTypeKey, string chatChannelKey, string chatUserKey )
        {
            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatUserKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var operationName = nameof( GetChatChannelMemberAsync ).SplitCase();

            var queryMembersRequest = new QueryMembersRequest
            {
                Type = chatChannelTypeKey,
                Id = chatChannelKey,
                FilterConditions = new Dictionary<string, object>
                {
                    { "id", chatUserKey }
                },
                Sorts = new List<SortParameter>
                    {
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    }
            };

            try
            {
                var queryMembersResponse = await RetryAsync(
                    async () => await ChannelClient.QueryMembersAsync( queryMembersRequest ),
                    operationName
                );

                if ( queryMembersResponse?.Members?.Any() == true )
                {
                    return TryConvertToChatChannelMember( queryMembersResponse.Members.First() );
                }
            }
            catch ( Exception ex )
            {
                if ( ex is StreamChatException streamChatException && streamChatException.ErrorCode == 16 )
                {
                    // Provide the caller with a predictable exception so they can decide how to proceed when the
                    // targeted channel does not exist.
                    throw new ChatChannelNotFoundException(
                        $"{LogMessagePrefix} {operationName} failed: channel does not exist in Stream (type: {chatChannelTypeKey}; id: {chatChannelKey}).",
                        ex,
                        chatChannelTypeKey,
                        chatChannelKey
                    );
                }

                throw;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<List<ChatChannelMember>> CreateChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<ChatChannelMember> chatChannelMembers )
        {
            var results = new List<ChatChannelMember>();

            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatChannelMembers?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( CreateChatChannelMembersAsync ).SplitCase();

            // Max number of members to create per request is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/channel_member/#add-channel-members
            var pageSize = 100;
            var offset = 0;
            var membersToCreateCount = chatChannelMembers.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < membersToCreateCount )
            {
                var batchedMembers = chatChannelMembers
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                var batchedMemberChatUserKeys = batchedMembers
                    .Select( m => m.ChatUserKey )
                    .ToArray();

                try
                {
                    // First, create the members.
                    await RetryAsync(
                        async () => await ChannelClient.AddMembersAsync(
                            chatChannelTypeKey,
                            chatChannelKey,
                            batchedMemberChatUserKeys
                        ),
                        operationName
                    );

                    // Then assign their channel roles.
                    var assignedRoleResults = await AssignRolesAsync(
                        chatChannelTypeKey,
                        chatChannelKey,
                        batchedMembers
                    );

                    results.AddRange( assignedRoleResults );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
                finally
                {
                    offset += pageSize;
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<ChatChannelMember>> UpdateChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<ChatChannelMember> chatChannelMembers )
        {
            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatChannelMembers?.Any() != true )
            {
                return new List<ChatChannelMember>();
            }

            // The only Stream channel member field that can currently be updated from Rock is "channel_role"; we'll use
            // the `AssignRolesAsync` API so we can do this in bulk.

            return await AssignRolesAsync( chatChannelTypeKey, chatChannelKey, chatChannelMembers );
        }

        /// <summary>
        /// Assigns <see cref="ChatChannelMember"/> roles in the external chat system.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for which roles should be assigned.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for which roles should be assigned.</param>
        /// <param name="chatChannelMembers">The list of <see cref="ChatChannelMember"/>s to whom roles should be assigned.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of <see cref="ChatChannelMember"/>s whose
        /// roles were successfully assigned.
        /// </returns>
        /// <exception cref="AggregateException">If any <see cref="ChatChannelMember"/>s fail to have a role assigned.</exception>
        private async Task<List<ChatChannelMember>> AssignRolesAsync( string chatChannelTypeKey, string chatChannelKey, List<ChatChannelMember> chatChannelMembers )
        {
            var results = new List<ChatChannelMember>();
            var operationName = nameof( AssignRolesAsync ).SplitCase();

            // Max number of roles to assign per request is 100.
            // https://getstream.io/chat/docs/rest/#/product%3Achat/UpdateChannel
            var pageSize = 100;
            var offset = 0;
            var rolesToAssignCount = chatChannelMembers.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < rolesToAssignCount )
            {
                var batchedAssignments = chatChannelMembers
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                var assignRoleRequest = new AssignRoleRequest
                {
                    AssignRoles = new List<RoleAssignment>()
                };

                batchedAssignments.ForEach( m =>
                {
                    assignRoleRequest.AssignRoles.Add(
                        new RoleAssignment
                        {
                            UserId = m.ChatUserKey,
                            ChannelRole = m.Role
                        }
                    );
                } );

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.AssignRolesAsync(
                            chatChannelTypeKey,
                            chatChannelKey,
                            assignRoleRequest
                        ),
                        operationName
                    );

                    results.AddRange( batchedAssignments );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
                finally
                {
                    offset += pageSize;
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<string>> DeleteChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys )
        {
            var results = new List<string>();

            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( DeleteChatChannelMembersAsync ).SplitCase();

            // Max number of channel members to delete per batch is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/channel_member/#channel-member-custom-data
            var pageSize = 100;
            var offset = 0;
            var channelMembersToDeleteCount = chatUserKeys.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < channelMembersToDeleteCount )
            {
                var batchedKeys = chatUserKeys
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.RemoveMembersAsync(
                            chatChannelTypeKey,
                            chatChannelKey,
                            batchedKeys
                        ),
                        operationName
                    );

                    results.AddRange( batchedKeys );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
                finally
                {
                    offset += pageSize;
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        #endregion Chat Channel Members

        #region Chat Users

        /// <inheritdoc/>
        public async Task EnsureSystemUserExistsAsync()
        {
            await UpsertChatUsersAsync( new List<ChatUser> { ChatHelper.GetChatSystemUser() } );
        }

        /// <inheritdoc/>
        public async Task<List<ChatUser>> GetChatUsersAsync( List<string> chatUserKeys )
        {
            var results = new List<ChatUser>();

            if ( chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( GetChatUsersAsync ).SplitCase();

            // Max number of users to get per query is 30.
            // https://getstream.io/chat/docs/dotnet-csharp/query_users/#supported-queries
            var pageSize = 30;
            var offset = 0;
            var usersToGetCount = chatUserKeys.Count;

            // Keep track of user keys already seen, to prevent the possibility of duplicates returned.
            var seenUserKeys = new HashSet<string>();

            // With a query request, we want "all or nothing". So if any batches fail, allow an exception to be thrown.
            while ( offset < usersToGetCount )
            {
                var batchedKeys = chatUserKeys
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                var queryUserOptions = new QueryUserOptions()
                    .WithLimit( pageSize )
                    .WithFilter(
                        new Dictionary<string, object>
                        {
                            { "id", new Dictionary<string, object> { { "$in", batchedKeys } } }
                        }
                    )
                    .WithSortBy(
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    );

                var queryUsersResponse = await RetryAsync(
                    async () => await UserClient.QueryAsync( queryUserOptions ),
                    operationName
                );

                if ( queryUsersResponse?.Users?.Any() == true )
                {
                    results.AddRange(
                        queryUsersResponse.Users
                            .Select( u => TryConvertToChatUser( u ) )
                            .Where( u =>
                                u != null
                                && seenUserKeys.Add( u.Key )
                            )
                    );
                }

                offset += pageSize;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<ChatUser>> CreateChatUsersAsync( List<ChatUser> chatUsers )
        {
            return await UpsertChatUsersAsync( chatUsers );
        }

        /// <inheritdoc/>
        public async Task<List<ChatUser>> UpdateChatUsersAsync( List<ChatUser> chatUsers )
        {
            return await UpsertChatUsersAsync( chatUsers );
        }

        /// <summary>
        /// Creates new or updates existing <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="chatUsers">The list of <see cref="ChatUser"/>s to create or update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of <see cref="ChatUser"/>s that were
        /// successfully created or updated.
        /// </returns>
        /// <exception cref="AggregateException">If any <see cref="ChatUser"/>s fail to be created or updated.</exception>
        private async Task<List<ChatUser>> UpsertChatUsersAsync( List<ChatUser> chatUsers )
        {
            var requests = chatUsers
                ?.Select( u => new
                {
                    Request = TryConvertToStreamUserRequest( u ),
                    ChatUser = u
                } )
                .Where( r => r.Request != null )
                .ToList();

            var results = new List<ChatUser>();

            if ( requests?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( UpsertChatUsersAsync ).SplitCase();

            // Max number of users to upsert per batch is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/update_users/#server-side-user-updates-batch
            var pageSize = 100;
            var offset = 0;
            var usersToCreateCount = requests.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < usersToCreateCount )
            {
                var batchedRequests = requests
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                try
                {
                    await RetryAsync(
                        async () => await UserClient.UpsertManyAsync( batchedRequests.Select( r => r.Request ) ),
                        operationName
                    );

                    results.AddRange( batchedRequests.Select( r => r.ChatUser ) );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
                finally
                {
                    offset += pageSize;
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( $"{LogMessagePrefix} {operationName} failed.", exceptions );
            }

            return results;
        }

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<List<string>> DeleteChatUsersAsync( List<string> chatUserKeys )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<string> GetChatUserTokenAsync( string chatUserKey )
        {
            if ( chatUserKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return Task.FromResult( UserClient.CreateToken( chatUserKey ) );
        }

        #endregion Chat Users

        #region Default Value Enforcers

        /// <inheritdoc/>
        [RockInternal( "17.0" )]
        public bool ShouldEnforceDefaultGrantsPerRole { get; set; }

        /// <inheritdoc/>
        [RockInternal( "17.0" )]
        public bool ShouldEnforceDefaultSettings { get; set; }

        #endregion Default Value Enforcers

        #endregion IChatProvider Implementation

        #region Private Methods

        #region Resiliency & Error Handling

        /// <summary>
        /// Executes the specified operation with retry logic, including exponential backoff and jitter to help avoid
        /// contention when multiple operations are retrying at the same time.
        /// </summary>
        /// <typeparam name="T">The type of the response returned by the operation.</typeparam>
        /// <param name="operation">The asynchronous operation to execute.</param>
        /// <param name="operationName">The name of the operation, to be used when logging failures.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response from the operation.</returns>
        /// <exception cref="Exception">Thrown if the operation fails after the specified number of retries.</exception>
        /// <remarks>
        /// <para>
        /// If the operation encounters a rate-limiting response, the retry logic respects the rate limit reset time
        /// provided in the API response and also applies a jitter to handle multiple requests being made in parallel.
        /// </para>
        /// <para>
        /// Exponential backoff with jitter is applied for any non-rate-limiting failures, and such failures will be
        /// limited to a max of 5 retries.
        /// </para>
        /// </remarks>
        private async Task<T> RetryAsync<T>( Func<Task<T>> operation, string operationName ) where T : ApiResponse
        {
            var delay = TimeSpan.FromSeconds( 1 ); // Initial delay.
            var retries = 0;
            var maxRetries = 5;
            var random = new Random();

            while ( true )
            {
                try
                {
                    // Attempt the operation.
                    var response = await operation();

                    // Check for rate limiting.
                    if ( response.TryGetRateLimit( out var rateLimits ) )
                    {
                        if ( rateLimits.Remaining == 0 && rateLimits.Reset > DateTimeOffset.UtcNow )
                        {
                            var waitTime = rateLimits.Reset - DateTimeOffset.UtcNow;

                            // Apply jitter up to +20% of waitTime
                            var jitter = TimeSpan.FromMilliseconds( random.NextDouble() * 0.2 * waitTime.TotalMilliseconds );
                            var waitTimeWithJitter = waitTime + jitter;

                            Logger.LogWarning( $"{LogMessagePrefix} Rate Limit Exceeded ({operationName}). Waiting for {waitTimeWithJitter.TotalSeconds} seconds before retrying." );

                            await Task.Delay( waitTimeWithJitter );
                            continue; // Retry after delay.
                        }
                    }

                    // If successful and no rate limit issues, return the response.
                    return response;
                }
                catch ( Exception ex )
                {
                    if ( ex is StreamChatException streamChatException && UnrecoverableErrorCodes.Contains( streamChatException.ErrorCode ) )
                    {
                        // If this exception can be identified as a known, unrecoverable Stream API Error Code, there's no need to retry.
                        // Let the caller decide how to handle the exception.
                        throw;
                    }

                    if ( ex is ChatChannelNotFoundException )
                    {
                        // Let the caller decide how to handle the exception when a Stream channel could not be found.
                        throw;
                    }

                    if ( ++retries > maxRetries )
                    {
                        // If the max retry count has been exceeded, let the caller decide how to handle the exception.
                        throw;
                    }

                    // Log the retry attempt.
                    Logger.LogWarning( ex, $"{LogMessagePrefix} {operationName} failed. Retrying in {delay.TotalSeconds} seconds (attempt {retries}/{maxRetries})." );

                    // Add jitter: +/- 20% of the delay.
                    var jitter = TimeSpan.FromMilliseconds(
                        random.NextDouble() * 0.4 * delay.TotalMilliseconds - 0.2 * delay.TotalMilliseconds
                    );
                    var delayWithJitter = delay + jitter;

                    // Apply cap to the jittered delay (e.g., max 30 seconds).
                    delayWithJitter = TimeSpan.FromTicks( Math.Min( delayWithJitter.Ticks, TimeSpan.FromSeconds( 30 ).Ticks ) );

                    // Wait before retrying.
                    await Task.Delay( delayWithJitter );

                    // Increase delay exponentially for the next retry.
                    delay = TimeSpan.FromTicks( Math.Min( delay.Ticks * 2, TimeSpan.FromSeconds( 30 ).Ticks ) );
                }
            }
        }

        #endregion Resiliency & Error Handling

        #region Converters: From Rock Chat DTOs to Stream DTOs

        /// <summary>
        /// Tries to convert a <see cref="ChatChannelType"/> to a <see cref="ChannelTypeWithStringCommandsRequest"/>.
        /// </summary>
        /// <param name="chatChannelType">The <see cref="ChatChannelType"/> to convert.</param>
        /// <param name="setDefaultPropertyValues">If <see langword="true"/>, will set property values to match Rock's
        /// preferred, default channel type settings in Stream. If <see langword="false"/>, will only set the
        /// `Name` property.</param>
        /// <param name="setDefaultGrants">If <see langword="true"/>, will set `Grants` to match Rock's preferred,
        /// default channel type permission grants (per role) in Stream. If <see langword="false"/>, will not set the
        /// `Grants` property.</param>
        /// <returns>A <see cref="ChannelTypeWithStringCommandsRequest"/> or <see langword="null"/> if unable to convert.</returns>
        private ChannelTypeWithStringCommandsRequest TryConvertToStreamChannelTypeWithStringCommandsRequest( ChatChannelType chatChannelType, bool setDefaultPropertyValues, bool setDefaultGrants )
        {
            if ( ( chatChannelType?.Key ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            /*
                1/29/2025: JPH

                The following default property values were derived from the GET /channeltypes endpoint using Stream's
                Swagger docs, focusing on the `messaging` channel type:
                https://getstream.io/chat/docs/rest/#/product%3Achat/ListChannelTypes

                Reason: Set channel type default property values to match Stream's default "Messaging" channel type.
            */

            ChannelTypeWithStringCommandsRequest request;

            if ( setDefaultPropertyValues )
            {
                request = new ChannelTypeWithStringCommandsRequest
                {
                    Name = chatChannelType.Key,
                    TypingEvents = true,
                    ReadEvents = true,
                    ConnectEvents = true,
                    Search = true,
                    Reactions = true,
                    Replies = true,
                    Reminders = false,
                    Uploads = true,
                    MarkMessagesPending = false,
                    Mutes = true,
                    MessageRetention = "infinite",
                    MaxMessageLength = 5000,
                    Automod = Automod.Disabled,
                    AutomodBehavior = ModerationBehaviour.Flag,
                    AutomodThresholds = null,
                    UrlEnrichment = true,
                    CustomEvents = true,
                    PushNotifications = true,
                    Blocklist = null,
                    BlocklistBehavior = null,
                    Commands = new List<string> { "giphy" }
                };
            }
            else
            {
                request = new ChannelTypeWithStringCommandsRequest
                {
                    Name = chatChannelType.Key
                };
            }

            if ( setDefaultGrants )
            {
                request.Grants = DefaultChannelTypeGrantsByRole;
            }
            else
            {
                request.Grants = chatChannelType.GrantsByRole;
            }

            return request;
        }

        /// <summary>
        /// Tries to convert a <see cref="ChatChannel"/> to a <see cref="ChannelGetRequest"/>.
        /// </summary>
        /// <param name="chatChannel">The <see cref="ChatChannel"/> to convert.</param>
        /// <returns>A <see cref="ChannelGetRequest"/> or <see langword="null"/> if unable to convert.</returns>
        private ChannelGetRequest TryConvertToStreamChannelGetRequest( ChatChannel chatChannel )
        {
            if ( chatChannel == null )
            {
                return null;
            }

            return new ChannelGetRequest
            {
                Data = GetStreamChannelRequest( chatChannel )
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="ChatChannel"/> to a <see cref="ChannelUpdateRequest"/>.
        /// </summary>
        /// <param name="chatChannel">The <see cref="ChatChannel"/> to convert.</param>
        /// <returns>A <see cref="ChannelUpdateRequest"/> or <see langword="null"/> if unable to convert.</returns>
        private ChannelUpdateRequest TryConvertToStreamChannelUpdateRequest( ChatChannel chatChannel )
        {
            if ( chatChannel == null )
            {
                return null;
            }

            return new ChannelUpdateRequest
            {
                Data = GetStreamChannelRequest( chatChannel )
            };
        }

        /// <summary>
        /// Gets common <see cref="ChannelRequest"/> data that is shared between multiple Stream channel request types.
        /// </summary>
        /// <param name="chatChannel">The <see cref="ChatChannel"/> for which to get the <see cref="ChannelRequest"/>
        /// data.</param>
        /// <returns>Common <see cref="ChannelRequest"/> data that is shared between multiple Stream channel request types.</returns>
        private ChannelRequest GetStreamChannelRequest( ChatChannel chatChannel )
        {
            var channelRequest = new ChannelRequest
            {
                CreatedBy = TryConvertToStreamUserRequest( ChatHelper.GetChatSystemUser() ),
            };

            channelRequest.SetData( ChannelDataKey.Name, chatChannel.Name ?? string.Empty );
            channelRequest.SetData( ChannelDataKey.IsLeavingAllowed, chatChannel.IsLeavingAllowed );
            channelRequest.SetData( ChannelDataKey.IsPublic, chatChannel.IsPublic );
            channelRequest.SetData( ChannelDataKey.IsAlwaysShown, chatChannel.IsAlwaysShown );

            return channelRequest;
        }

        /// <summary>
        /// Tries to convert a <see cref="ChatUser"/> to a <see cref="UserRequest"/>.
        /// </summary>
        /// <param name="chatUser">The <see cref="ChatUser"/> to convert.</param>
        /// <returns>A <see cref="UserRequest"/> or <see langword="null"/> if unable to convert.</returns>
        private UserRequest TryConvertToStreamUserRequest( ChatUser chatUser )
        {
            if ( ( chatUser?.Key ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            var request = new UserRequest
            {
                Id = chatUser.Key,
                Name = chatUser.Name,
                Role = chatUser.IsAdmin ? ChatRole.Administrator.GetDescription() : ChatRole.User.GetDescription()
            };

            request.SetData( UserDataKey.Image, chatUser.PhotoUrl ?? string.Empty );
            request.SetData( UserDataKey.IsProfileVisible, chatUser.IsProfileVisible );
            request.SetData( UserDataKey.IsOpenDirectMessageAllowed, chatUser.IsOpenDirectMessageAllowed );
            request.SetData( UserDataKey.Badges, chatUser.Badges ?? new List<ChatBadge>() );

            return request;
        }

        #endregion Converters: From Rock Chat DTOs to Stream DTOs

        #region Converters: From Stream DTOs to Rock Chat DTOs

        /// <summary>
        /// Tries to convert a <see cref="ChannelTypeWithCommandsResponse"/> to a <see cref="ChatChannelType"/>.
        /// </summary>
        /// <param name="response">The <see cref="ChannelTypeWithCommandsResponse"/>to convert.</param>
        /// <returns>A <see cref="ChatChannelType"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannelType TryConvertToChatChannelType( ChannelTypeWithCommandsResponse response )
        {
            if ( ( response?.Name ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ChatChannelType
            {
                Key = response.Name,
                GrantsByRole = response.Grants
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="ChannelGetResponse"/> to a <see cref="ChatChannel"/>.
        /// </summary>
        /// <param name="response">The <see cref="ChannelGetResponse"/> to convert.</param>
        /// <returns>A <see cref="ChatChannel"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannel TryConvertToChatChannel( ChannelGetResponse response )
        {
            var channel = response?.Channel;
            if ( ( channel?.Cid ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ChatChannel
            {
                Key = channel.Id,
                ChatChannelTypeKey = channel.Type,
                QueryableKey = channel.Cid,
                Name = channel.GetDataOrDefault( ChannelDataKey.Name, channel.Cid ),
                IsLeavingAllowed = channel.GetDataOrDefault( ChannelDataKey.IsLeavingAllowed, false ),
                IsPublic = channel.GetDataOrDefault( ChannelDataKey.IsPublic, false ),
                IsAlwaysShown = channel.GetDataOrDefault( ChannelDataKey.IsAlwaysShown, false )
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="ChannelMember"/> to a <see cref="ChatChannelMember"/>.
        /// </summary>
        /// <param name="channelMember">the <see cref="ChannelMember"/> to convert.</param>
        /// <returns>A <see cref="ChatChannelMember"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannelMember TryConvertToChatChannelMember( ChannelMember channelMember )
        {
            if ( ( channelMember?.UserId ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ChatChannelMember
            {
                ChatUserKey = channelMember.UserId,
                Role = channelMember.ChannelRole,
                IsChatMuted = channelMember.NotificationsMuted ?? false,
                IsChatBanned = channelMember.Banned ?? false
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="User"/> to a <see cref="ChatUser"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to convert.</param>
        /// <returns>A <see cref="ChatUser"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatUser TryConvertToChatUser( User user )
        {
            if ( ( user?.Id ).IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ChatUser
            {
                Key = user.Id,
                Name = user.GetDataOrDefault( UserDataKey.Name, user.Id ),
                PhotoUrl = user.GetDataOrDefault( UserDataKey.Image, string.Empty ),
                IsAdmin = user.Role.Equals( ChatRole.Administrator.GetDescription() ),
                IsProfileVisible = user.GetDataOrDefault( UserDataKey.IsProfileVisible, false ),
                IsOpenDirectMessageAllowed = user.GetDataOrDefault( UserDataKey.IsOpenDirectMessageAllowed, false ),
                Badges = user.GetDataOrDefault( UserDataKey.Badges, new List<ChatBadge>() )
            };
        }

        #endregion Converters: From Stream DTOs to Rock Chat DTOs

        #endregion Private Methods
    }
}
