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
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Exceptions;
using Rock.Communication.Chat.Sync;
using Rock.Enums.Communication.Chat;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

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
        #region Keys & Constants

        /// <summary>
        /// Custom HTTP header names.
        /// </summary>
        private static class HttpHeaderName
        {
            public const string XApiKey = "X-Api-Key";
            public const string XSignature = "X-Signature";
            public const string XStreamExt = "x-stream-ext";
        }

        /// <summary>
        /// Custom HTTP header values.
        /// </summary>
        private static class HttpHeaderValue
        {
            public const string XStreamExt = "rock-stream-chat-provider";
        }

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

        /// <summary>
        /// JSON property names for Stream webhook payloads.
        /// </summary>
        private static class WebhookJsonProperty
        {
            public const string Banned = "banned";
            public const string Channel = "channel";
            public const string ChannelId = "channel_id";
            public const string ChannelRole = "channel_role";
            public const string ChannelType = "channel_type";
            public const string Ext = "ext";
            public const string Id = "id";
            public const string Member = "member";
            public const string Members = "members";
            public const string Mute = "mute";
            public const string Name = "name";
            public const string NotificationsMuted = "notifications_muted";
            public const string RequestInfo = "request_info";
            public const string Type = "type";
            public const string User = "user";
            public const string UserId = "user_id";
        }

        /// <summary>
        /// Error message prefix for required Stream webhook payload properties that are missing.
        /// </summary>
        private const string PayloadPropMissingMsgPrefix = "Stream webhook payload is missing required";

        /// <summary>
        /// Error message prefix for invalid Stream webhook payload properties.
        /// </summary>
        private const string PayloadPropInvalidMsgPrefix = "Stream webhook payload contains invalid";

        /// <summary>
        /// Stream webhook event types.
        /// </summary>
        private static class WebhookEvent
        {
            public const string ChannelCreated = "channel.created";
            public const string ChannelUpdated = "channel.updated";

            // Rock code was originally written to support channel deletion webhooks, but we've since decided to not
            // allow ANY channel deletions from the client.
            //public const string ChannelDeleted = "channel.deleted";

            public const string MemberAdded = "member.added";
            public const string MemberRemoved = "member.removed";

            // These are technically channel[ member] muted/unmuted events.
            public const string ChannelMuted = "channel.muted";
            public const string ChannelUnmuted = "channel.unmuted";

            public const string UserBanned = "user.banned";
            public const string UserUnbanned = "user.unbanned";

            public const string UserDeleted = "user.deleted";
        }

        #endregion Keys & Constants

        #region Fields

        /// <summary>
        /// A lock object for thread-safe initialization of this chat provider instance.
        /// </summary>
        private static readonly object _initializationLock = new object();

        /// <summary>
        /// The backing field for the <see cref="Logger"/> property.
        /// </summary>
        private readonly Lazy<ILogger> _logger;

        // Per Stream: https://github.com/GetStream/stream-chat-net
        // All client instances can be used as a singleton for the lifetime of your application as they don't maintain state.

        /// <summary>
        /// Provides access to the <see cref="StreamClientFactory"/> for creating specific API clients.
        /// </summary>
        private Lazy<StreamClientFactory> _clientFactory;

        /// <summary>
        /// The backing field for the <see cref="AppClient"/> property.
        /// </summary>
        private Lazy<IAppClient> _appClient;

        /// <summary>
        /// The backing field for the <see cref="PermissionClient"/> property.
        /// </summary>
        private Lazy<IPermissionClient> _permissionClient;

        /// <summary>
        /// The backing field for the <see cref="ChannelTypeClient"/> property.
        /// </summary>
        private Lazy<IChannelTypeClient> _channelTypeClient;

        /// <summary>
        /// The backing field for the <see cref="ChannelClient"/> property.
        /// </summary>
        private Lazy<IChannelClient> _channelClient;

        /// <summary>
        /// The backing field for the <see cref="UserClient"/> property.
        /// </summary>
        private Lazy<IUserClient> _userClient;

        /// <summary>
        /// The backing field for the <see cref="MessageClient"/> property.
        /// </summary>
        private Lazy<IMessageClient> _messageClient;

        // The best way to see default app-scoped permission grants is to query Stream's "GetApp" Swagger endpoint while
        // targeting a newly-created, unmodified app.
        // https://getstream.io/chat/docs/rest/#/product%3Achat/GetApp

        // And also consider Stream's app-scoped permissions reference.
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
                        // The following were taken from the default "app (global scope)" > "user" grants.
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
                        // The following were taken from the default "app (global scope)" > "global_admin" grants.
                        "close-poll-any-team",
                        "create-block-list-any-team",
                        "create-poll-any-team",
                        "delete-block-list-any-team",
                        "delete-moderation-config-any-team",
                        "delete-poll-any-team",
                        "flag-user-any-team",
                        "mute-user-any-team",
                        "query-moderation-review-queue-any-team",
                        "query-polls-owner-any-team",
                        "read-block-lists-any-team",
                        "read-flag-reports-any-team",
                        "read-moderation-config-any-team",
                        "search-user-any-team",
                        "submit-moderation-action-any-team",
                        "update-block-list-any-team",
                        "update-flag-report-any-team",
                        "update-poll-any-team",
                        "update-user-owner",
                        "upsert-moderation-config-any-team",
                        // The following have been added as needed.
                        "ban-user" // Admins SHOULD be able to ban any user.
                    }
                }
            };
        } );

        // The best way to see default "messaging"-scoped permission grants is to query Stream's "ListChannelTypes"
        // Swagger endpoint while targeting a newly-created, unmodified app.
        // https://getstream.io/chat/docs/rest/#/product%3Achat/ListChannelTypes

        // And also consider Stream's "messaging"-scoped permissions reference.
        // https://getstream.io/chat/docs/dotnet-csharp/permissions_reference/#scope-messaging

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
                        // The following were taken from the default "messaging (Channel Type scope)" > "channel_member" grants.
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
                        "upload-attachment",
                        // The following were taken from the default "messaging (Channel Type scope)" > "user" grants.
                        // (with redundant grants commented out)
                        //"add-links-owner",
                        //"create-attachment-owner",
                        "create-channel",
                        //"create-mention-owner",
                        //"create-message-owner",
                        //"create-reaction-owner",
                        //"create-reply-owner",
                        "delete-attachment-owner",
                        //"delete-channel-owner", // We're not going to allow ANY channel deletions from the client.
                        "delete-message-owner",
                        "delete-reaction-owner",
                        //"flag-message-owner",
                        //"mute-channel-owner",
                        //"pin-message-owner",
                        //"query-votes",
                        //"read-channel-members-owner",
                        //"read-channel-owner",
                        "recreate-channel-owner",
                        //"remove-own-channel-membership-owner",
                        //"run-message-action-owner",
                        //"send-custom-event-owner",
                        //"send-poll",
                        "truncate-channel-owner",
                        "update-channel-members-owner",
                        "update-channel-owner",
                        "update-message-owner",
                        "update-thread-owner",
                        //"upload-attachment-owner"
                    }
                },
                {
                    ChatRole.Moderator.GetDescription(),
                    new List<string>
                    {
                        // The following were taken from the default "messaging (Channel Type scope)" > "moderator" grants.
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
                        "create-restricted-visibility-message",
                        "create-system-message",
                        "delete-attachment",
                        //"delete-channel-owner", // We're not going to allow ANY channel deletions from the client.
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
                        "recreate-channel-owner",
                        "remove-own-channel-membership",
                        "run-message-action",
                        "send-custom-event",
                        "send-poll",
                        "skip-channel-cooldown",
                        "skip-message-moderation",
                        "truncate-channel-owner",
                        "unblock-message",
                        "update-channel",
                        "update-channel-cooldown",
                        "update-channel-frozen",
                        //"update-channel-members", // Moderators should NOT be able to manage members for channels they didn't create (this should be done within Rock).
                        "update-channel-members-owner", // But they SHOULD be able to manage members for channels they personally create within Stream.
                        "update-message",
                        "update-thread",
                        "upload-attachment"
                    }
                },
                {
                    ChatRole.Administrator.GetDescription(),
                    new List<string>
                    {
                        // The following were taken from the default "messaging (Channel Type scope)" > "admin" grants.
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
                        "create-restricted-visibility-message",
                        "create-system-message",
                        "delete-attachment",
                        //"delete-channel", // We're not going to allow ANY channel deletions from the client.
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
                /*
                    3/7/2025 - JPH

                    Some error codes are commented out - and therefore considered to be recoverable - as follows:

                    -------------
                    Error Code 4: "Input Error: When wrong data/parameter is sent to the API"
                    Error Code 16: "Does Not Exist Error: Resource not found"

                    These are tricky. It's possible that the error is due to a bug in Rock, but these are also the error
                    codes that Stream returns due to "eventual consistency" scenarios, which they explained this way:

                    "When you create a new entity (like a custom role or channel type), the creation is confirmed by the
                    API node that processed your request, but there can be a short propagation delay before that entity
                    is fully available across all our systems. This is why you're receiving success responses for the
                    creation operations but then getting 'not defined' errors when immediately trying to use these entities."

                    Example:
                        1) Rock creates a new channel type.
                        2) Rock immediately tries to create a channel of that type.
                        3) Stream returns an error because the channel type isn't fully available yet.

                    Because of this, we need to consider these error codes as recoverable in some cases, but we can't
                    easily pick and choose which ones, so we'll just consider all cases as recoverable for now. The
                    retry logic will consider this to be a non-rate-limiting failure and will therefore retry up to 5
                    times, with an exponential backoff.

                    -------------
                    Error Code 9: Rate "Limit Error: Too many requests in a certain time frame"

                    This is a temporary error, so it's always considered "recoverable". The retry logic will continue
                    to retry failures of this type until successful.

                    Reason: Determine which Stream error codes should be "recoverable" vs not.
                 */

                -1, // Internal System Error: Triggered when something goes wrong in [Stream's] system
                2, // Access Key Error: Access Key invalid
                //4, // Input Error: When wrong data/parameter is sent to the API
                5, // Authentication Error: Unauthenticated, problem with authentication
                6, // Duplicate Username Error: When a duplicate username is sent while enforce_unique_usernames is enabled
                //9, // Rate Limit Error: Too many requests in a certain time frame
                //16, // Does Not Exist Error: Resource not found
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

        /// <summary>
        /// The backing field for the <see cref="RockToChatSyncConfig"/> property.
        /// </summary>
        private RockToChatSyncConfig _rockToChatSyncConfig = new RockToChatSyncConfig();

        /// <summary>
        /// The backing field for the <see cref="ChatToRockSyncConfig"/> property;
        /// </summary>
        private ChatToRockSyncConfig _chatToRockSyncConfig = new ChatToRockSyncConfig();

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
        /// Gets the <see cref="IMessageClient"/> that should be used to manage messages within the external Stream chat
        /// system.
        /// </summary>
        private IMessageClient MessageClient => _messageClient.Value;

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

            Initialize();
        }

        #endregion Constructors

        #region IChatProvider Implementation

        #region Configuration

        /// <inheritdoc/>
        public RockToChatSyncConfig RockToChatSyncConfig
        {
            get => _rockToChatSyncConfig;
            set
            {
                if ( value == null )
                {
                    value = new RockToChatSyncConfig();
                }

                _rockToChatSyncConfig = value;
            }
        }

        /// <inheritdoc/>
        public ChatToRockSyncConfig ChatToRockSyncConfig
        {
            get => _chatToRockSyncConfig;
            set
            {
                if ( value == null )
                {
                    value = new ChatToRockSyncConfig();
                }

                _chatToRockSyncConfig = value;
            }
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            lock ( _initializationLock )
            {
                _clientFactory = new Lazy<StreamClientFactory>( () =>
                {
                    var chatConfiguration = ChatHelper.GetChatConfiguration();
                    return new StreamClientFactory(
                        chatConfiguration.ApiKey,
                        chatConfiguration.ApiSecret,
                        config =>
                        {
                            // We're adding this custom header value so Stream will send it back to us on webhook requests
                            // that are triggered by this provider code. This way, we can know to ignore such webhooks,
                            // while only paying attention to those webhooks that are truly relevant.
                            //
                            // While this double-check locking pattern is not ideal, it's the most predictable way to ensure
                            // only one instance of the header gets added to Stream's underlying HttpClient singleton.
                            if ( !config.HttpClient.DefaultRequestHeaders.Contains( HttpHeaderName.XStreamExt ) )
                            {
                                lock ( _initializationLock )
                                {
                                    if ( !config.HttpClient.DefaultRequestHeaders.Contains( HttpHeaderName.XStreamExt ) )
                                    {
                                        config.HttpClient.DefaultRequestHeaders.Add( HttpHeaderName.XStreamExt, HttpHeaderValue.XStreamExt );
                                    }
                                }
                            }
                        }
                    );
                } );

                _appClient = new Lazy<IAppClient>( () => _clientFactory.Value.GetAppClient() );
                _permissionClient = new Lazy<IPermissionClient>( () => _clientFactory.Value.GetPermissionClient() );
                _channelTypeClient = new Lazy<IChannelTypeClient>( () => _clientFactory.Value.GetChannelTypeClient() );
                _channelClient = new Lazy<IChannelClient>( () => _clientFactory.Value.GetChannelClient() );
                _userClient = new Lazy<IUserClient>( () => _clientFactory.Value.GetUserClient() );
                _messageClient = new Lazy<IMessageClient>( () => _clientFactory.Value.GetMessageClient() );
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAppSettingsAsync()
        {
            var operationName = nameof( UpdateAppSettingsAsync ).SplitCase();

            var request = new AppSettingsRequest
            {
                WebhookUrl = ChatHelper.WebhookUrl,
                WebhookEvents = new List<string>
                {
                    WebhookEvent.ChannelCreated,
                    WebhookEvent.ChannelUpdated,

                    WebhookEvent.MemberAdded,
                    WebhookEvent.MemberRemoved,

                    WebhookEvent.ChannelMuted,
                    WebhookEvent.ChannelUnmuted,

                    WebhookEvent.UserBanned,
                    WebhookEvent.UserUnbanned,

                    WebhookEvent.UserDeleted
                }
            };

            await RetryAsync(
                async () => await AppClient.UpdateAppSettingsAsync( request ),
                operationName
            );

            // Assume if we got this far - and an exception wasn't thrown - app settings were updated.
            return true;
        }

        #endregion Configuration

        #region Roles & Permission Grants

        /// <inheritdoc/>
        public Dictionary<string, List<string>> DefaultAppGrantsByRole => _defaultAppGrantsByRole.Value;

        /// <inheritdoc/>
        public Dictionary<string, List<string>> DefaultChannelTypeGrantsByRole => _defaultChannelTypeGrantsByRole.Value;

        /// <inheritdoc/>
        /// <exception cref="AggregateException">If any roles fail to be created.</exception>
        public async Task<bool> EnsureAppRolesExistAsync()
        {
            var operationName = nameof( EnsureAppRolesExistAsync ).SplitCase();

            var listRolesResponse = await RetryAsync(
                async () => await PermissionClient.ListRolesAsync(),
                operationName
            );

            var existingRoles = listRolesResponse?.Roles ?? new List<CustomRole>();
            var existingRequiredRoles = new List<string>();

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var requiredRole in ChatHelper.RequiredAppRoles )
            {
                // The Stream client will throw an exception if the role already exists, so we'll do our best to avoid this.
                var existingRole = existingRoles.FirstOrDefault( r => r.Name == requiredRole );
                if ( existingRole != null )
                {
                    existingRequiredRoles.Add( requiredRole );
                }
                else
                {
                    try
                    {
                        var roleResponse = await RetryAsync(
                            async () => await PermissionClient.CreateRoleAsync( requiredRole ),
                            operationName
                        );

                        if ( roleResponse?.Role?.Name == requiredRole )
                        {
                            existingRequiredRoles.Add( requiredRole );
                        }
                    }
                    catch ( Exception ex )
                    {
                        exceptions.Add( ex );
                    }
                }
            }

            if ( exceptions.Any() )
            {
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            // Even though no exceptions were thrown, let's verify that all required roles actually exist.
            return ChatHelper.RequiredAppRoles.All( r => existingRequiredRoles.Contains( r ) );
        }

        /// <inheritdoc/>
        public async Task<bool> EnsureAppGrantsExistAsync()
        {
            var operationName = nameof( EnsureAppGrantsExistAsync ).SplitCase();

            var getAppResponse = await RetryAsync(
                async () => await AppClient.GetAppSettingsAsync(),
                operationName
            );

            var anyGrantsToAdd = false;
            var grants = getAppResponse?.App?.Grants;
            if ( grants?.Any() != true || RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole )
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

            // Assume if we got this far - and an exception wasn't thrown - all app grants exist.
            return true;
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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
                var request = TryConvertToStreamChannelTypeWithStringCommandsRequest(
                    chatChannelType,
                    RockToChatSyncConfig.ShouldEnforceDefaultSettings,
                    RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole
                );
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        #endregion Chat Channel Types

        #region Chat Channels

        /// <inheritdoc/>
        public string GetQueryableChatChannelKey( GroupCache groupCache )
        {
            if ( groupCache == null )
            {
                return null;
            }

            var chatChannelTypeKey = ChatHelper.GetChatChannelTypeKey( groupCache.GroupTypeId );
            var chatChannelKey = ChatHelper.GetChatChannelKey( groupCache );

            return GetQueryableChatChannelKey( chatChannelTypeKey, chatChannelKey );
        }

        /// <summary>
        /// Gets the queryable chat channel key for the specified chat channel type key and chat channel key.
        /// </summary>
        /// <param name="chatChannelTypeKey">The chat channel type key.</param>
        /// <param name="chatChannelKey">The chat channel key.</param>
        /// <returns>The queryable chat channel key.</returns>
        private string GetQueryableChatChannelKey( string chatChannelTypeKey, string chatChannelKey )
        {
            // Per Stream: Querying by the channel Identifier should be done using the cid field as far as possible to
            // optimize API performance. As the full channel ID, cid is indexed everywhere in Stream database where id
            // is not (https://getstream.io/chat/docs/dotnet-csharp/query_channels/#channel-queryable-built-in-fields).
            return $"{chatChannelTypeKey}:{chatChannelKey}";
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

                /*
                    3/11/2025 - JPH

                    Stream's "Querying Channels" docs say:

                        "It is important to note that your `filter` should include, at the very least
                         `{members: {$in: [userID]}` or pagination could break."

                    However, it's not really feasible for us to provide a list of members when querying channels. If we
                    suspect we're seeing issues related to this, we'll have to reconsider how we're filtering here.

                    Reason: Mention risk called out by Stream's docs.
                    https://getstream.io/chat/docs/dotnet-csharp/query_channels/#pagination
                */
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
        public async Task<List<ChatChannel>> GetAllChatChannelsAsync()
        {
            return await GetAllChatChannelsAsync( null );
        }

        /// <summary>
        /// Gets all <see cref="ChatChannel"/>s, optionally filtered by <see cref="ChatChannelType"/>.
        /// </summary>
        /// <param name="chatChannelTypeKey">The optional <see cref="ChatChannelType.Key"/> to filter the results by.</param>
        /// <returns>
        /// If <paramref name="chatChannelTypeKey"/> is provided, all <see cref="ChatChannel"/>s of the specified
        /// <see cref="ChatChannelType"/>; If not, all <see cref="ChatChannel"/>s in Stream.
        /// </returns>
        private async Task<List<ChatChannel>> GetAllChatChannelsAsync( string chatChannelTypeKey = null )
        {
            var results = new List<ChatChannel>();

            var operationName = nameof( GetAllChatChannelsAsync ).SplitCase();

            // Max number of channels to get per query is 30.
            // https://getstream.io/chat/docs/dotnet-csharp/query_channels/#query-options
            var pageSize = 30;
            var offset = 0;
            var channelsRetrievedCount = 0;

            // Keep track of channel keys already seen, to prevent the possibility of duplicates returned.
            var seenChannelKeys = new HashSet<string>();

            // With a query request, we want "all or nothing". So if any batches fail, allow an exception to be thrown.
            do
            {
                /*
                    3/11/2025 - JPH

                    Stream's "Querying Channels" docs say:

                        "It is important to note that your `filter` should include, at the very least
                         `{members: {$in: [userID]}` or pagination could break."

                    However, it's not really feasible for us to provide a list of members when querying channels. If we
                    suspect we're seeing issues related to this, we'll have to reconsider how we're filtering here.

                    Reason: Mention risk called out by Stream's docs.
                    https://getstream.io/chat/docs/dotnet-csharp/query_channels/#pagination
                */
                var queryChannelsOptions = new QueryChannelsOptions { MemberLimit = 0, MessageLimit = 0 }
                    .WithOffset( offset )
                    .WithLimit( pageSize )
                    .WithSortBy(
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    );

                // Filter by channel type?
                if ( chatChannelTypeKey.IsNotNullOrWhiteSpace() )
                {
                    queryChannelsOptions.Filter = new Dictionary<string, object>
                    {
                        { "type", chatChannelTypeKey }
                    };
                }

                var queryChannelResponse = await RetryAsync(
                    async () => await ChannelClient.QueryChannelsAsync( queryChannelsOptions ),
                    operationName
                );

                if ( queryChannelResponse?.Channels?.Any() == true )
                {
                    channelsRetrievedCount = queryChannelResponse.Channels.Count;
                    results.AddRange(
                        queryChannelResponse.Channels
                            .Select( c => TryConvertToChatChannel( c ) )
                            .Where( c =>
                                c != null
                                && seenChannelKeys.Add( c.Key )
                            )
                    );
                }
                else
                {
                    channelsRetrievedCount = 0;
                }

                offset += channelsRetrievedCount;
            }
            while ( channelsRetrievedCount == pageSize );

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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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
                        // The 2nd argument is `hardDelete = false`: This dictates that messages should be soft deleted,
                        // meaning they are removed from the client but are still available via server-side export functions.
                        // https://getstream.io/chat/docs/dotnet-csharp/channel_delete/#deleting-many-channels
                        async () => await ChannelClient.DeleteChannelsAsync( batchedKeys, false ),
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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

            var chatChannels = await GetAllChatChannelsAsync( chatChannelTypeKey );
            var chatChannelKeys = chatChannels
                .Select( c => c.Key )
                .ToList();

            await DeleteChatChannelsAsync( chatChannelKeys );
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
                        operationName,
                        streamErrorCodesToThrow: new HashSet<int> { 16 }
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
                catch ( StreamChatException ex ) when ( ex.ErrorCode == 16 )
                {
                    // Provide the caller with a predictable exception so they can decide how to proceed when the
                    // targeted channel does not exist.
                    var queryableChatChannelKey = GetQueryableChatChannelKey( chatChannelTypeKey, chatChannelKey );
                    throw new MemberChatChannelNotFoundException(
                        $"{LogMessagePrefix} {operationName} failed: channel does not exist in Stream (type: {chatChannelTypeKey}; id: {chatChannelKey}).",
                        ex,
                        chatChannelTypeKey,
                        chatChannelKey,
                        queryableChatChannelKey
                    );
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
                    operationName,
                    streamErrorCodesToThrow: new HashSet<int> { 16 }
                );

                if ( queryMembersResponse?.Members?.Any() == true )
                {
                    return TryConvertToChatChannelMember( queryMembersResponse.Members.First() );
                }
            }
            catch ( StreamChatException ex ) when ( ex.ErrorCode == 16 )
            {
                // Provide the caller with a predictable exception so they can decide how to proceed when the
                // targeted channel does not exist.
                var queryableChatChannelKey = GetQueryableChatChannelKey( chatChannelTypeKey, chatChannelKey );
                throw new MemberChatChannelNotFoundException(
                    $"{LogMessagePrefix} {operationName} failed: channel does not exist in Stream (type: {chatChannelTypeKey}; id: {chatChannelKey}).",
                    ex,
                    chatChannelTypeKey,
                    chatChannelKey,
                    queryableChatChannelKey
                );
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

                    // Are any members being added as banned? Unlikely, but possible.
                    var bannedMemberChatUserKeys = batchedMembers
                        .Where( m => m.IsChatBanned )
                        .Select( m => m.ChatUserKey )
                        .ToList();

                    if ( bannedMemberChatUserKeys.Any() )
                    {
                        await BanChatUsersAsync( bannedMemberChatUserKeys, chatChannelTypeKey, chatChannelKey );
                    }

                    // Are any members being added as muted? Again.. unlikely, but possible.
                    var mutedMemberChatUserKeys = batchedMembers
                        .Where( m => m.IsChatMuted )
                        .Select( m => m.ChatUserKey )
                        .ToList();

                    if ( mutedMemberChatUserKeys.Any() )
                    {
                        await MuteChatChannelAsync( chatChannelTypeKey, chatChannelKey, mutedMemberChatUserKeys );
                    }
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<ChatChannelMember>> UpdateChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey, Dictionary<ChatChannelMember, ChatChannelMember> chatChannelMembers )
        {
            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatChannelMembers?.Any() != true )
            {
                return new List<ChatChannelMember>();
            }

            // Stream's API doesn't allow us to easily update all of a member's possible changes in a single call.
            // We'll complete the updates in 3 phases and return a unique list of those members who were updated.

            var results = new List<ChatChannelMember>();
            var uniqueChatUserKeys = new HashSet<string>();

            // -------------------------------------------
            // 1) Update members whose roles have changed.
            var roleChangeMembers = chatChannelMembers
                .Where( kvp => kvp.Key.Role != kvp.Value.Role )
                .Select( kvp => kvp.Key )
                .ToList();

            if ( roleChangeMembers.Any() )
            {
                results.AddRange( await AssignRolesAsync( chatChannelTypeKey, chatChannelKey, roleChangeMembers ) );
                uniqueChatUserKeys.UnionWith( results.Select( r => r.ChatUserKey ) );
            }

            // -------------------------------------------------------
            // 2) Update members whose channel ban status has changed.
            var banStatusChangeMembers = chatChannelMembers
                .Where( kvp => kvp.Key.IsChatBanned != kvp.Value.IsChatBanned )
                .Select( kvp => kvp.Key )
                .ToList();

            if ( banStatusChangeMembers.Any() )
            {
                var banChatUserKeys = banStatusChangeMembers
                    .Where( m => m.IsChatBanned )
                    .Select( m => m.ChatUserKey )
                    .ToList();

                if ( banChatUserKeys.Any() )
                {
                    var bannedKeys = await BanChatUsersAsync( banChatUserKeys, chatChannelTypeKey, chatChannelKey );
                    bannedKeys.ForEach( k =>
                    {
                        if ( uniqueChatUserKeys.Add( k ) )
                        {
                            results.Add( banStatusChangeMembers.First( m => m.ChatUserKey == k ) );
                        }
                    } );
                }

                var unbanChatUserKeys = banStatusChangeMembers
                    .Where( m => !m.IsChatBanned )
                    .Select( m => m.ChatUserKey )
                    .ToList();

                if ( unbanChatUserKeys.Any() )
                {
                    var unbannedKeys = await UnbanChatUsersAsync( unbanChatUserKeys, chatChannelTypeKey, chatChannelKey );
                    unbannedKeys.ForEach( k =>
                    {
                        if ( uniqueChatUserKeys.Add( k ) )
                        {
                            results.Add( banStatusChangeMembers.First( m => m.ChatUserKey == k ) );
                        }
                    } );
                }
            }

            // --------------------------------------------------------
            // 3) Update members whose channel mute status has changed.

            var muteStatusChangeMembers = chatChannelMembers
                .Where( kvp => kvp.Key.IsChatMuted != kvp.Value.IsChatMuted )
                .Select( kvp => kvp.Key )
                .ToList();

            if ( muteStatusChangeMembers.Any() )
            {
                var muteChatUserKeys = muteStatusChangeMembers
                    .Where( m => m.IsChatMuted )
                    .Select( m => m.ChatUserKey )
                    .ToList();

                if ( muteChatUserKeys.Any() )
                {
                    var mutedKeys = await MuteChatChannelAsync( chatChannelTypeKey, chatChannelKey, muteChatUserKeys );
                    mutedKeys.ForEach( k =>
                    {
                        if ( uniqueChatUserKeys.Add( k ) )
                        {
                            results.Add( muteStatusChangeMembers.First( m => m.ChatUserKey == k ) );
                        }
                    } );
                }

                var unmuteChatUserKeys = muteStatusChangeMembers
                    .Where( m => !m.IsChatMuted )
                    .Select( m => m.ChatUserKey )
                    .ToList();

                if ( unmuteChatUserKeys.Any() )
                {
                    var unmutedKeys = await UnmuteChatChannelAsync( chatChannelTypeKey, chatChannelKey, unmuteChatUserKeys );
                    unmutedKeys.ForEach( k =>
                    {
                        if ( uniqueChatUserKeys.Add( k ) )
                        {
                            results.Add( muteStatusChangeMembers.First( m => m.ChatUserKey == k ) );
                        }
                    } );
                }
            }

            return results;
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<string>> MuteChatChannelAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys )
        {
            // Testing shows that Stream's mute endpoints can be repeatedly called for the same user without error, so
            // we're not going to worry about checking if the channel is already muted for these users.

            var results = new List<string>();

            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( MuteChatChannelAsync ).SplitCase();
            var queryableChatChannelKey = GetQueryableChatChannelKey( chatChannelTypeKey, chatChannelKey );

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatUserKey in chatUserKeys )
            {
                var channelMuteRequest = new ChannelMuteRequest()
                {
                    ChannelCids = new List<string> { queryableChatChannelKey },
                    UserId = chatUserKey
                };

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.MuteChannelAsync( channelMuteRequest ),
                        operationName
                    );

                    // If we got here, we'll trust the mute was successful.
                    results.Add( chatUserKey );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<string>> UnmuteChatChannelAsync( string chatChannelTypeKey, string chatChannelKey, List<string> chatUserKeys )
        {
            // Testing shows that Stream's mute endpoints can be repeatedly called for the same user without error, so
            // we're not going to worry about checking if the channel is muted for these users.

            var results = new List<string>();

            if ( chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() || chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( UnmuteChatChannelAsync ).SplitCase();
            var queryableChatChannelKey = GetQueryableChatChannelKey( chatChannelTypeKey, chatChannelKey );

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatUserKey in chatUserKeys )
            {
                var channelUnmuteRequest = new ChannelUnmuteRequest()
                {
                    ChannelCids = new List<string> { queryableChatChannelKey },
                    UserId = chatUserKey
                };

                try
                {
                    await RetryAsync(
                        async () => await ChannelClient.UnmuteChannelAsync( channelUnmuteRequest ),
                        operationName
                    );

                    // If we got here, we'll trust the unmute was successful.
                    results.Add( chatUserKey );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        #endregion Chat Channel Members

        #region Chat Users

        /// <inheritdoc/>
        public async Task<bool> EnsureSystemUserExistsAsync()
        {
            var operationName = nameof( EnsureSystemUserExistsAsync ).SplitCase();

            var chatSystemUser = ChatHelper.GetChatSystemUser();
            var request = TryConvertToStreamUserRequest( chatSystemUser );

            var upsertResponse = await RetryAsync(
                async () => await UserClient.UpsertAsync( request ),
                operationName
            );

            var systemUserExists = upsertResponse
                ?.Users
                ?.Values
                ?.Any( u => u?.Id == chatSystemUser.Key ) == true;

            return systemUserExists;
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
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<ChatSyncCrudResult> DeleteChatUsersAsync( List<string> chatUserKeys, string newChatUserKey = null )
        {
            var result = new ChatSyncCrudResult();

            if ( chatUserKeys?.Any() != true )
            {
                return result;
            }

            // Prevent delete exceptions by first ensuring the chat users truly exist in Stream.
            try
            {
                var queryChatUserKeys = chatUserKeys.ToList();
                if ( newChatUserKey.IsNotNullOrWhiteSpace() )
                {
                    if ( queryChatUserKeys.Contains( newChatUserKey ) )
                    {
                        // We can't replace the users with one we're deleting.
                        newChatUserKey = null;
                    }
                    else
                    {
                        // Add this user to query to ensure they exist.
                        queryChatUserKeys.Add( newChatUserKey );
                    }
                }

                var existingChatUsers = await GetChatUsersAsync( queryChatUserKeys );
                for ( var i = chatUserKeys.Count - 1; i >= 0; i-- )
                {
                    var chatUserKey = chatUserKeys[i];
                    if ( !existingChatUsers.Any( u => u.Key == chatUserKey ) )
                    {
                        // Send this key right back out the door.
                        result.Skipped.Add( chatUserKey );
                        chatUserKeys.RemoveAt( i );
                    }
                }

                if ( newChatUserKey.IsNotNullOrWhiteSpace() && !existingChatUsers.Any( u => u.Key == newChatUserKey ) )
                {
                    // The specified "new" user doesn't exist in Stream.
                    newChatUserKey = null;
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                return result;
            }

            // Do any remain to be deleted?
            if ( !chatUserKeys.Any() )
            {
                return result;
            }

            var operationName = nameof( DeleteChatUsersAsync ).SplitCase();

            // Max number of users to delete per query is 100.
            // https://getstream.io/chat/docs/dotnet-csharp/update_users/#deleting-many-users
            var pageSize = 100;
            var offset = 0;
            var usersToDeleteCount = chatUserKeys.Count;

            // Don't let individual batch failures cause all to fail.
            var exceptions = new List<Exception>();

            while ( offset < usersToDeleteCount )
            {
                var batchedKeys = chatUserKeys
                    .Skip( offset )
                    .Take( pageSize )
                    .ToList();

                try
                {
                    var deleteUsersRequest = new DeleteUsersRequest().WithUserIds( chatUserKeys );

                    if ( newChatUserKey.IsNotNullOrWhiteSpace() )
                    {
                        deleteUsersRequest.NewChannelOwnerId = newChatUserKey;
                    }
                    else
                    {
                        deleteUsersRequest.UserDeletionStrategy = DeletionStrategy.Soft;
                        deleteUsersRequest.MessageDeletionStrategy = DeletionStrategy.Soft;
                        deleteUsersRequest.ConversationDeletionStrategy = DeletionStrategy.Soft;
                    }

                    await RetryAsync(
                        async () => await UserClient.DeleteManyAsync( deleteUsersRequest ),
                        operationName
                    );

                    // Assume if we got this far - and an exception wasn't thrown - users were marked for deletion.
                    result.Deleted.UnionWith( batchedKeys );
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
                result.Exception = ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<string>> BanChatUsersAsync( List<string> chatUserKeys, string chatChannelTypeKey = null, string chatChannelKey = null )
        {
            // Testing shows that Stream's ban endpoints can be repeatedly called for the same user without error, so
            // we're not going to worry about checking if these users are already banned.

            var results = new List<string>();

            if ( chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( BanChatUsersAsync ).SplitCase();

            var chatSystemUserKey = ChatHelper.GetChatSystemUser().Key;

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatUserKey in chatUserKeys )
            {
                var banRequest = new BanRequest()
                {
                    TargetUserId = chatUserKey,
                    BannedById = chatSystemUserKey,
                    Type = chatChannelTypeKey,
                    Id = chatChannelKey
                };

                try
                {
                    await RetryAsync(
                        async () => await UserClient.BanAsync( banRequest ),
                        operationName
                    );

                    // If we got here, we'll trust the ban was successful.
                    results.Add( chatUserKey );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<List<string>> UnbanChatUsersAsync( List<string> chatUserKeys, string chatChannelTypeKey = null, string chatChannelKey = null )
        {
            // Testing shows that Stream's ban endpoints can be repeatedly called for the same user without error, so
            // we're not going to worry about checking if these users are banned.

            var results = new List<string>();

            if ( chatUserKeys?.Any() != true )
            {
                return results;
            }

            var operationName = nameof( UnbanChatUsersAsync ).SplitCase();

            var chatSystemUserKey = ChatHelper.GetChatSystemUser().Key;

            // Don't let individual failures cause all to fail.
            var exceptions = new List<Exception>();

            foreach ( var chatUserKey in chatUserKeys )
            {
                var banRequest = new BanRequest()
                {
                    TargetUserId = chatUserKey,
                    BannedById = chatSystemUserKey,
                    Type = chatChannelTypeKey,
                    Id = chatChannelKey
                };

                try
                {
                    await RetryAsync(
                        async () => await UserClient.UnbanAsync( banRequest ),
                        operationName
                    );

                    // If we got here, we'll trust the unban was successful.
                    results.Add( chatUserKey );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw ChatHelper.GetFirstOrAggregateException( exceptions, $"{LogMessagePrefix} {operationName} failed." );
            }

            return results;
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

        #region Messages

        /// <inheritdoc/>
        public async Task<Dictionary<string, Dictionary<string, int>>> GetChatUserMessageCountsByChatChannelKeyAsync( DateTime messageDate )
        {
            // The outer dictionary key is the chat channel key and the inner dictionary key is the chat user key.
            var results = new Dictionary<string, Dictionary<string, int>>();

            var operationName = nameof( GetChatUserMessageCountsByChatChannelKeyAsync ).SplitCase();

            var messageDateStartString = messageDate.StartOfDay().ToRfc3339UtcString();

            // Start by getting channels that have messages on or after the specified date.
            // Max number of channels to get per query is 30.
            // https://getstream.io/chat/docs/dotnet-csharp/query_channels/#query-options
            var pageSize = 30;
            var offset = 0;
            var channelsRetrievedCount = 0;
            var chatChannelIdByCids = new Dictionary<string, string>();

            // With query requests, we want "all or nothing". So if any batches fail, allow an exception to be thrown.
            do
            {
                /*
                    3/11/2025 - JPH

                    Stream's "Querying Channels" docs say:

                        "It is important to note that your `filter` should include, at the very least
                         `{members: {$in: [userID]}` or pagination could break."

                    However, it's not really feasible for us to provide a list of members when querying channels. If we
                    suspect we're seeing issues related to this, we'll have to reconsider how we're filtering here.

                    Reason: Mention risk called out by Stream's docs.
                    https://getstream.io/chat/docs/dotnet-csharp/query_channels/#pagination
                */
                var queryChannelsOptions = new QueryChannelsOptions() { MemberLimit = 0, MessageLimit = 0 }
                    .WithLimit( pageSize )
                    .WithOffset( offset )
                    .WithFilter(
                        new Dictionary<string, object>
                        {
                            // Rule out stale channels.
                            { "last_message_at", new Dictionary<string, object> { { "$gte", messageDateStartString } } }
                        }
                    )
                    .WithSortBy(
                        new SortParameter
                        {
                            Field = SortParameterFieldKey.CreatedAt,
                            Direction = SortDirection.Ascending
                        }
                    );

                var queryChannelsResponse = await RetryAsync(
                    async () => await ChannelClient.QueryChannelsAsync( queryChannelsOptions ),
                    operationName
                );

                if ( queryChannelsResponse?.Channels?.Any() == true )
                {
                    channelsRetrievedCount = queryChannelsResponse.Channels.Count;

                    foreach ( var channelGetResponse in queryChannelsResponse.Channels )
                    {
                        var key = channelGetResponse?.Channel?.Cid;
                        var value = channelGetResponse?.Channel?.Id;

                        if ( key.IsNotNullOrWhiteSpace() && value.IsNotNullOrWhiteSpace() )
                        {
                            chatChannelIdByCids.TryAdd( key, value );
                        }
                    }
                }
                else
                {
                    channelsRetrievedCount = 0;
                }

                offset += pageSize;
            }
            while ( channelsRetrievedCount == pageSize );

            if ( !chatChannelIdByCids.Any() )
            {
                // There are no channels with messages on or after `messageDate`.
                return results;
            }

            // Next, get the message counts by user, for each channel; create an [exclusive] end date to compare against.
            var messageDateEndString = messageDate.AddDays( 1 ).StartOfDay().ToRfc3339UtcString();

            foreach ( var chatChannelIdsKvp in chatChannelIdByCids )
            {
                // We want to query Stream's API using this value.
                var chatChannelCid = chatChannelIdsKvp.Key;

                // We want to return this value as the outermost dictionary key.
                var chatChannelId = chatChannelIdsKvp.Value;

                // Messages pagination operates differently than other Stream queries. It appears that we can get up to
                // 1k messages at a time, and the response will include a `next` value if there are still more messages
                // to get; we'll query again WITH the `next` value as an argument, until we get a response that doesn't
                // have a `next` value.
                // https://getstream.io/chat/docs/dotnet-csharp/search/#pagination
                var messagesPageSize = 1000;
                string next = null;

                // We'll also keep a reasonable limit on the number of requests we'll make, just in case Stream has an
                // issue in their pagination logic. 1M messages per channel, per day will [probably] never happen, so
                // we'll cap at that (1000 requests X 1000 messages per request).
                var requestCount = 0;
                var maxRequestCount = 1000;

                do
                {
                    var searchOptions = new SearchOptions()
                        .WithLimit( messagesPageSize )
                        .WithNext( next )
                        .WithFilter(
                            new Dictionary<string, object>
                            {
                                { "cid", chatChannelCid }
                            }
                        )
                        .WithMessageFilterConditions(
                            new Dictionary<string, object>
                            {
                                {
                                    "$and",
                                    new List<object>
                                    {
                                        new Dictionary<string, object>
                                        {
                                            { "created_at", new Dictionary<string, object> { { "$gte", messageDateStartString } } }
                                        },
                                        new Dictionary<string, object>
                                        {
                                            { "created_at", new Dictionary<string, object> { { "$lt", messageDateEndString } } }
                                        }
                                    }
                                }
                            }
                        )
                        .WithSortBy(
                            new SortParameter
                            {
                                Field = SortParameterFieldKey.CreatedAt,
                                Direction = SortDirection.Ascending
                            }
                        );

                    var messageSearchResponse = await RetryAsync(
                        async () => await MessageClient.SearchAsync( searchOptions ),
                        operationName
                    );

                    next = messageSearchResponse?.Next;

                    var messageCountResults = messageSearchResponse
                        ?.Results
                        ?.Where( r => r?.Message?.User?.Id.IsNotNullOrWhiteSpace() == true )
                        .GroupBy( r => r.Message.User.Id )
                        .Select( g => new
                        {
                            ChatUserKey = g.Key,
                            MessageCount = g.Count()
                        } )
                        .ToDictionary( counts => counts.ChatUserKey, counts => counts.MessageCount );

                    if ( messageCountResults?.Any() == true )
                    {
                        // Get or add the "message counts by user" dictionary for this channel.
                        if ( !results.TryGetValue( chatChannelId, out var runningMessageCounts ) )
                        {
                            // We don't have any counts for this channel yet; simply add the results.
                            results.Add( chatChannelId, messageCountResults );
                        }
                        else
                        {
                            // We're paging through this channel's messages (there are more than 1k), so we need to
                            // supplement the previous results.
                            foreach ( var messageCountKvp in messageCountResults )
                            {
                                var chatUserKey = messageCountKvp.Key;
                                var messageCount = messageCountKvp.Value;

                                if ( runningMessageCounts.TryGetValue( chatUserKey, out var existingMessageCount ) )
                                {
                                    // Add to this chat user's running total for this chat channel.
                                    runningMessageCounts[chatUserKey] = existingMessageCount + messageCount;
                                }
                                else
                                {
                                    // We haven't seen this user for this channel before now; set their count.
                                    runningMessageCounts[chatUserKey] = messageCount;
                                }
                            }
                        }
                    }

                    requestCount++;
                }
                while ( next.IsNotNullOrWhiteSpace() && requestCount < maxRequestCount );
            }

            return results;
        }

        #endregion Messages

        #region Webhooks

        /// <inheritdoc/>
        public async Task<WebhookValidationResult> ValidateWebhookRequestAsync( HttpRequestMessage request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            var webhookLabel = "Stream webhook request";

            var chatConfiguration = ChatHelper.GetChatConfiguration();
            if ( chatConfiguration.ApiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidChatWebhookRequestException( $"Rock's chat configuration has an invalid API key, so the {webhookLabel} cannot be validated." );
            }

            if ( !request.Headers.TryGetValues( HttpHeaderName.XApiKey, out var apiKeyValues )
                || !string.Equals( apiKeyValues?.FirstOrDefault(), chatConfiguration.ApiKey, StringComparison.Ordinal ) )
            {
                throw new InvalidChatWebhookRequestException( $"{webhookLabel} has an invalid '{HttpHeaderName.XApiKey}' header." );
            }

            if ( !request.Headers.TryGetValues( HttpHeaderName.XSignature, out var signatureValues )
                || signatureValues?.FirstOrDefault().IsNotNullOrWhiteSpace() != true )
            {
                throw new InvalidChatWebhookRequestException( $"{webhookLabel} has an invalid '{HttpHeaderName.XSignature}' header." );
            }

            var requestBody = await request.Content.ReadAsStringAsync();
            if ( requestBody.IsNullOrWhiteSpace() )
            {
                throw new InvalidChatWebhookRequestException( $"{webhookLabel} has an empty request body; unable to validate the request." );
            }

            if ( !AppClient.VerifyWebhook( requestBody, signatureValues.First() ) )
            {
                throw new InvalidChatWebhookRequestException( $"{webhookLabel} has an invalid '{HttpHeaderName.XSignature}' header." );
            }

            return new WebhookValidationResult( requestBody );
        }

        /// <inheritdoc/>
        public List<ChatToRockSyncCommand> GetChatToRockSyncCommands( List<ChatWebhookRequest> webhookRequests )
        {
            var commands = new List<ChatToRockSyncCommand>();

            if ( webhookRequests?.Any() != true )
            {
                return commands;
            }

            var operationName = nameof( GetChatToRockSyncCommands ).SplitCase();
            var structuredLog = "{@Payload}";

            foreach ( var request in webhookRequests )
            {
                var payload = request.Payload;

                try
                {
                    var json = JObject.Parse( payload );
                    if ( json[WebhookJsonProperty.RequestInfo]?[WebhookJsonProperty.Ext]?.ToString() == HttpHeaderValue.XStreamExt )
                    {
                        // This webhook was triggered by a request to Stream from within this provider code; ignore it.
                        continue;
                    }

                    var webhookEvent = json[WebhookJsonProperty.Type]?.ToString();
                    if ( webhookEvent.IsNullOrWhiteSpace() )
                    {
                        throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{WebhookJsonProperty.Type}'." );
                    }

                    var chatSyncType = TryGetChatSyncType( webhookEvent );
                    if ( !chatSyncType.HasValue )
                    {
                        throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{WebhookJsonProperty.Type}'." );
                    }

                    var syncCommands = new List<ChatToRockSyncCommand>();

                    switch ( webhookEvent )
                    {
                        case WebhookEvent.ChannelCreated:
                        case WebhookEvent.ChannelUpdated:
                            syncCommands.AddRange( GetSyncChatChannelAndMembersToRockCommands( chatSyncType.Value, json ) );
                            break;
                        case WebhookEvent.MemberAdded:
                        case WebhookEvent.MemberRemoved:
                            syncCommands.Add( GetSyncChatChannelMemberToRockCommand( chatSyncType.Value, json ) );
                            break;
                        case WebhookEvent.ChannelMuted:
                        case WebhookEvent.ChannelUnmuted:
                            syncCommands.Add( GetSyncChatChannelMutedStatusToRockCommand( chatSyncType.Value, json ) );
                            break;
                        case WebhookEvent.UserBanned:
                        case WebhookEvent.UserUnbanned:
                            syncCommands.Add( GetSyncChatBannedStatusToRockCommand( chatSyncType.Value, json ) );
                            break;
                        case WebhookEvent.UserDeleted:
                            syncCommands.Add( GetDeleteChatUserInRockCommand( json ) );
                            break;
                        default:
                            // Don't pollute the error log with unsupported events; log them as warnings instead.
                            // Note - also - that unsupported events can and should be prevented from the Stream side.
                            Logger.LogWarning( $"{LogMessagePrefix} Unsupported Stream webhook '{WebhookJsonProperty.Type}' received. {structuredLog}", payload );
                            break;
                    }

                    commands.AddRange( syncCommands );
                }
                catch ( ChatWebhookParseException ex )
                {
                    // Prefer a structured log without a stack trace.
                    Logger.LogError( $"{LogMessagePrefix} {ex.Message} {structuredLog}", payload );
                }
                catch ( Exception ex )
                {
                    // Fall back to logging unexpected exceptions.
                    Logger.LogError( ex, $"{LogMessagePrefix} Error processing Stream webhook payload. {structuredLog}", payload );
                }
            }

            return commands;
        }

        #endregion Webhooks

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
        /// <param name="streamErrorCodesToThrow">
        /// The list of Stream error codes whose exception should be immediately thrown instead of retrying or handling otherwise.
        /// </param>
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
        private async Task<T> RetryAsync<T>( Func<Task<T>> operation, string operationName, HashSet<int> streamErrorCodesToThrow = null ) where T : ApiResponse
        {
            var delay = TimeSpan.FromSeconds( 5 ); // Initial delay.
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
                    if ( ex is StreamChatException streamChatException )
                    {
                        /*
                            3/7/2025 - JPH

                            The following isn't ideal exception handling, but it's necessary for us to work with Stream's
                            "eventual consistency" scenarios, which they explained this way:

                            "When you create a new entity (like a custom role or channel type), the creation is confirmed by the
                            API node that processed your request, but there can be a short propagation delay before that entity
                            is fully available across all our systems. This is why you're receiving success responses for the
                            creation operations but then getting 'not defined' errors when immediately trying to use these entities."

                            Sometimes we want to retry in these cases, but other times, we can more-confidently react.

                            Reason: Know when to retry for "eventual consistency" scenarios vs. handling differently.
                        */

                        if ( streamErrorCodesToThrow?.Contains( streamChatException.ErrorCode ) == true )
                        {
                            // The caller has specified that they have custom handling for this Stream error code.
                            throw;
                        }

                        if ( UnrecoverableErrorCodes.Contains( streamChatException.ErrorCode ) )
                        {
                            // If this exception can be identified as a known, unrecoverable Stream API Error Code, there's no need to retry.
                            // Let the caller decide how to handle the exception.
                            throw;
                        }
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

                    // Apply cap of 30 seconds to the jittered delay.
                    delayWithJitter = TimeSpan.FromTicks( Math.Min( delayWithJitter.Ticks, TimeSpan.FromSeconds( 30 ).Ticks ) );

                    // Wait before retrying.
                    await Task.Delay( delayWithJitter );

                    // Increase delay exponentially for the next retry (max 30 seconds).
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

            var badges = user.GetDataOrDefault( UserDataKey.Badges, new List<ChatBadge>() ) ?? new List<ChatBadge>();

            return new ChatUser
            {
                Key = user.Id,
                Name = user.GetDataOrDefault( UserDataKey.Name, user.Id ),
                PhotoUrl = user.GetDataOrDefault( UserDataKey.Image, string.Empty ),
                IsAdmin = user.Role.Equals( ChatRole.Administrator.GetDescription() ),
                IsProfileVisible = user.GetDataOrDefault( UserDataKey.IsProfileVisible, false ),
                IsOpenDirectMessageAllowed = user.GetDataOrDefault( UserDataKey.IsOpenDirectMessageAllowed, false ),
                Badges = badges
            };
        }

        #endregion Converters: From Stream DTOs to Rock Chat DTOs

        #region Converters: From Stream Webhook Payload To Sync Commands

        /// <summary>
        /// Gets a list of channel-related <see cref="ChatToRockSyncCommand"/>s from a Stream webhook payload.
        /// </summary>
        /// <param name="chatSyncType">The <see cref="ChatSyncType"/> for the Stream webhook event.</param>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <returns>A <see cref="SyncChatChannelToRockCommand"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If the Stream webhook payload is missing expected values.</exception>
        /// <remarks>
        /// It should be expected that one <see cref="SyncChatChannelToRockCommand"/> will always be added to the results
        /// list. If <paramref name="chatSyncType"/> is <see cref="ChatSyncType.Create"/>, it should also be expected that
        /// any number of <see cref="SyncChatChannelMemberToRockCommand"/>s will also be included, one for each
        /// <see cref="ChatChannelMember"/> that exists in the <see cref="ChatChannel"/> at the time of its creation.
        /// </remarks>
        private List<ChatToRockSyncCommand> GetSyncChatChannelAndMembersToRockCommands( ChatSyncType chatSyncType, JObject json )
        {
            var syncCommands = new List<ChatToRockSyncCommand>();

            // ----------------------------------------------------
            // 1) Start by adding the sync command for the channel.
            var channelSyncCommand = new SyncChatChannelToRockCommand( ChatToRockSyncConfig.SyncCommandAttemptLimit, chatSyncType );

            // Ensure the payload contains a chat group identifier.
            var chatGroupIdentifiers = TryGetChatGroupIdentifiers( json );
            channelSyncCommand.ChatChannelKey = chatGroupIdentifiers.ChannelKey;
            channelSyncCommand.GroupId = chatGroupIdentifiers.GroupId;

            // Ensure the payload contains a channel type.
            var channelType = json[WebhookJsonProperty.ChannelType]?.ToString();
            if ( channelType.IsNullOrWhiteSpace() )
            {
                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{WebhookJsonProperty.ChannelType}'." );
            }

            // Ensure the channel type references a Rock group type.
            var groupTypeId = ChatHelper.GetGroupTypeId( channelType );
            if ( !groupTypeId.HasValue )
            {
                throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{WebhookJsonProperty.ChannelType}'." );
            }

            // Assign the group type ID to the sync command.
            channelSyncCommand.GroupTypeId = groupTypeId.Value;

            syncCommands.Add( channelSyncCommand );

            if ( chatSyncType != ChatSyncType.Delete )
            {
                var groupName = json[WebhookJsonProperty.Channel]?[WebhookJsonProperty.Name]?.ToString();
                if ( groupName.IsNullOrWhiteSpace() )
                {
                    // If a channel name wasn't provided, we'll fall back to using the channel ID for the name. This
                    // isn't ideal, but we need a value to use for the required Rock group name field.
                    groupName = chatGroupIdentifiers.ChannelKey;
                }

                // Assign the group name to the sync command.
                channelSyncCommand.GroupName = groupName;
            }

            // ----------------------------------------------------------------------------------------------
            // 2) Add a sync command for each preexisting chat channel member when creating a new Rock group.
            if ( chatSyncType == ChatSyncType.Create )
            {
                if ( !( json[WebhookJsonProperty.Members] is JArray membersArray ) )
                {
                    throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{WebhookJsonProperty.Members}'." );
                }

                var index = 0;
                foreach ( var member in membersArray )
                {
                    var memberJsonPathRoot = $"{WebhookJsonProperty.Members}.[Member with Array Index {index}]";

                    if ( !( member is JObject memberJson ) )
                    {
                        throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{memberJsonPathRoot}'." );
                    }

                    syncCommands.Add( GetSyncChatChannelMemberToRockCommand( chatSyncType, memberJson, chatGroupIdentifiers, memberJsonPathRoot ) );

                    index++;
                }
            }

            return syncCommands;
        }

        /// <summary>
        /// Gets a <see cref="SyncChatChannelMemberToRockCommand"/> from a Stream webhook payload.
        /// </summary>
        /// <param name="chatSyncType">The <see cref="ChatSyncType"/> for the Stream webhook event.</param>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <returns>A <see cref="SyncChatChannelMemberToRockCommand"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If the Stream webhook payload is missing expected values.</exception>
        private SyncChatChannelMemberToRockCommand GetSyncChatChannelMemberToRockCommand( ChatSyncType chatSyncType, JObject json )
        {
            // Ensure the payload contains a chat group identifier.
            var chatGroupIdentifiers = TryGetChatGroupIdentifiers( json );

            if ( !( json[WebhookJsonProperty.Member] is JObject memberJson ) )
            {
                throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{WebhookJsonProperty.Member}'" );
            }

            return GetSyncChatChannelMemberToRockCommand( chatSyncType, memberJson, chatGroupIdentifiers );
        }

        /// <summary>
        /// Gets a <see cref="SyncChatChannelMemberToRockCommand"/> from a Stream webhook payload's member JSON.
        /// </summary>
        /// <param name="chatSyncType">The <see cref="ChatSyncType"/> for the Stream webhook event.</param>
        /// <param name="memberJson">The Stream webhook payload's member JSON as a <see cref="JObject"/>.</param>
        /// <param name="chatGroupIdentifiers">The chat group identifiers.</param>
        /// <param name="memberJsonPathRoot">The root path to the member JSON in the payload.</param>
        /// <returns>A <see cref="SyncChatChannelMemberToRockCommand"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If the member JSON is missing expected values.</exception>
        private SyncChatChannelMemberToRockCommand GetSyncChatChannelMemberToRockCommand( ChatSyncType chatSyncType, JObject memberJson, ChatGroupIdentifiers chatGroupIdentifiers, string memberJsonPathRoot = WebhookJsonProperty.Member )
        {
            var syncCommand = new SyncChatChannelMemberToRockCommand( ChatToRockSyncConfig.SyncCommandAttemptLimit, chatSyncType )
            {
                ChatChannelKey = chatGroupIdentifiers.ChannelKey,
                GroupId = chatGroupIdentifiers.GroupId
            };

            // Ensure the payload contains a chat user ID.
            var userId = memberJson[WebhookJsonProperty.UserId]?.ToString();
            if ( userId.IsNullOrWhiteSpace() )
            {
                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{memberJsonPathRoot}.{WebhookJsonProperty.UserId}'." );
            }

            // Assign the chat user ID to the sync command.
            syncCommand.ChatPersonKey = userId;

            if ( chatSyncType != ChatSyncType.Delete )
            {
                // Try to get the channel member's role.
                var channelRole = memberJson[WebhookJsonProperty.ChannelRole]?.ToString();
                if ( channelRole.IsNotNullOrWhiteSpace() )
                {
                    // Assign the role to the sync command, but only if it represents one of Rock's chat roles.
                    syncCommand.ChatRole = EnumExtensions.ConvertToEnumOrNull<ChatRole>( channelRole );
                }

                // Try to get the channel member's banned status.
                var bannedToken = memberJson[WebhookJsonProperty.Banned];
                if ( bannedToken?.Type == JTokenType.Boolean )
                {
                    syncCommand.IsBanned = bannedToken.ToObject<bool>();
                }

                // Try to get the channel member's muted status.
                var mutedToken = memberJson[WebhookJsonProperty.NotificationsMuted];
                if ( mutedToken?.Type == JTokenType.Boolean )
                {
                    syncCommand.IsMuted = mutedToken.ToObject<bool>();
                }
            }

            return syncCommand;
        }

        /// <summary>
        /// Gets a <see cref="SyncChatChannelMutedStatusToRockCommand"/> from a Stream webhook payload.
        /// </summary>
        /// <param name="chatSyncType">The <see cref="ChatSyncType"/> for the Stream webhook event.</param>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <returns>A <see cref="SyncChatChannelMutedStatusToRockCommand"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If the Stream webhook payload is missing expected values.</exception>
        private SyncChatChannelMutedStatusToRockCommand GetSyncChatChannelMutedStatusToRockCommand( ChatSyncType chatSyncType, JObject json )
        {
            var syncCommand = new SyncChatChannelMutedStatusToRockCommand( ChatToRockSyncConfig.SyncCommandAttemptLimit, chatSyncType );

            // Ensure the payload contains a chat user ID;
            var userId = json[WebhookJsonProperty.User]?[WebhookJsonProperty.Id]?.ToString();
            if ( userId.IsNullOrWhiteSpace() )
            {
                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{WebhookJsonProperty.User}.{WebhookJsonProperty.Id}'." );
            }

            // Assign the user ID to the sync command.
            syncCommand.ChatPersonKey = userId;

            // Ensure the payload contains a chat group identifier.
            if ( !( json[WebhookJsonProperty.Mute]?[WebhookJsonProperty.Channel] is JObject muteChannelJson ) )
            {
                throw new ChatWebhookParseException( $"{PayloadPropInvalidMsgPrefix} '{WebhookJsonProperty.Mute}.{WebhookJsonProperty.Channel}'" );
            }

            var config = new TryGetChatGroupIdentifiersConfig
            {
                ChannelIdProperty = WebhookJsonProperty.Id,
                ChannelIdPath = $"{WebhookJsonProperty.Mute}.{WebhookJsonProperty.Channel}.{WebhookJsonProperty.Id}",
            };

            var chatGroupIdentifiers = TryGetChatGroupIdentifiers( muteChannelJson, config );
            syncCommand.ChatChannelKey = chatGroupIdentifiers.ChannelKey;
            syncCommand.GroupId = chatGroupIdentifiers.GroupId;

            return syncCommand;
        }

        /// <summary>
        /// Gets a <see cref="SyncChatBannedStatusToRockCommand"/> from a Stream webhook payload.
        /// </summary>
        /// <param name="chatSyncType">The <see cref="ChatSyncType"/> for the Stream webhook event.</param>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <returns>A <see cref="SyncChatBannedStatusToRockCommand"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If the Stream webhook payload is missing expected values.</exception>
        private SyncChatBannedStatusToRockCommand GetSyncChatBannedStatusToRockCommand( ChatSyncType chatSyncType, JObject json )
        {
            // Is this a channel-specific event?
            var config = new TryGetChatGroupIdentifiersConfig { ShouldThrowIfMissing = false };
            var chatGroupIdentifiers = TryGetChatGroupIdentifiers( json, config );

            var isChannelSpecific = chatGroupIdentifiers.GroupId.HasValue
                || chatGroupIdentifiers.ChannelKey.IsNotNullOrWhiteSpace();

            var attemptLimit = isChannelSpecific
                ? ChatToRockSyncConfig.SyncCommandAttemptLimit
                : 1; // Only attempt once for global ban/unban events.

            var syncCommand = new SyncChatBannedStatusToRockCommand( attemptLimit, chatSyncType )
            {
                ChatChannelKey = chatGroupIdentifiers.ChannelKey,
                GroupId = chatGroupIdentifiers.GroupId
            };

            // Ensure the payload contains a chat user ID;
            var userId = json[WebhookJsonProperty.User]?[WebhookJsonProperty.Id]?.ToString();
            if ( userId.IsNullOrWhiteSpace() )
            {
                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{WebhookJsonProperty.User}.{WebhookJsonProperty.Id}'." );
            }

            // Assign the user ID to the sync command.
            syncCommand.ChatPersonKey = userId;

            return syncCommand;
        }

        /// <summary>
        /// Gets a <see cref="DeleteChatPersonInRockCommand"/> from a Stream webhook payload.
        /// </summary>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <returns>A <see cref="DeleteChatPersonInRockCommand"/> or <see langword="null"/> if unable to parse.</returns>
        /// <exception cref="ChatWebhookParseException">If the Stream webhook payload is missing expected values.</exception>
        private DeleteChatPersonInRockCommand GetDeleteChatUserInRockCommand( JObject json )
        {
            var syncCommand = new DeleteChatPersonInRockCommand();

            // Ensure the payload contains a chat user ID;
            var userId = json[WebhookJsonProperty.User]?[WebhookJsonProperty.Id]?.ToString();
            if ( userId.IsNullOrWhiteSpace() )
            {
                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{WebhookJsonProperty.User}.{WebhookJsonProperty.Id}'." );
            }

            // Assign the user ID to the sync command.
            syncCommand.ChatPersonKey = userId;

            return syncCommand;
        }

        /// <summary>
        /// Tries to get the <see cref="ChatSyncType"/> for a Stream webhook event type.
        /// </summary>
        /// <param name="webhookEvent">The Stream webhook event type.</param>
        /// <returns>The <see cref="ChatSyncType"/> or <see langword="null"/> if unable to parse.</returns>
        private ChatSyncType? TryGetChatSyncType( string webhookEvent )
        {
            var eventParts = webhookEvent.Split( '.' );
            if ( eventParts.Length != 2 )
            {
                return null;
            }

            switch ( eventParts[1] )
            {
                case "created":
                case "added":
                    return ChatSyncType.Create;
                case "updated":
                    return ChatSyncType.Update;
                case "muted":
                    return ChatSyncType.Mute;
                case "unmuted":
                    return ChatSyncType.Unmute;
                case "banned":
                    return ChatSyncType.Ban;
                case "unbanned":
                    return ChatSyncType.Unban;
                case "deleted":
                case "removed":
                    return ChatSyncType.Delete;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Tries to get the chat <see cref="Group"/> identifiers from a Stream webhook payload.
        /// </summary>
        /// <param name="json">The Stream webhook payload as a <see cref="JObject"/>.</param>
        /// <param name="config">The optional configuration object for how to parse <see cref="Group"/> identifiers.</param>
        /// <returns>A <see cref="ChatGroupIdentifiers"/>.</returns>
        /// <exception cref="ChatWebhookParseException">If unable to parse the identifiers.</exception>
        private ChatGroupIdentifiers TryGetChatGroupIdentifiers( JObject json, TryGetChatGroupIdentifiersConfig config = null )
        {
            var chatGroupIdentifiers = new ChatGroupIdentifiers();

            if ( config == null )
            {
                config = new TryGetChatGroupIdentifiersConfig();
            }

            // Ensure the payload contains a channel ID.
            var channelId = json[config.ChannelIdProperty]?.ToString();
            if ( channelId.IsNullOrWhiteSpace() )
            {
                if ( !config.ShouldThrowIfMissing )
                {
                    return chatGroupIdentifiers;
                }

                throw new ChatWebhookParseException( $"{PayloadPropMissingMsgPrefix} '{config.ChannelIdPath}'." );
            }

            // Assign the channel key.
            chatGroupIdentifiers.ChannelKey = channelId;

            // Does the channel ID contain an embedded Rock group ID?
            var groupId = ChatHelper.GetGroupId( channelId );
            if ( groupId.HasValue )
            {
                // Assign the group ID.
                chatGroupIdentifiers.GroupId = groupId.Value;
            }

            return chatGroupIdentifiers;
        }

        #endregion Converters: From Stream Webhook Payload To Sync Commands

        #endregion Private Methods

        #region Supporting Members

        /// <summary>
        /// A configuration object for how to parse <see cref="Group"/> identifiers from a Stream webhook payload.
        /// </summary>
        private class TryGetChatGroupIdentifiersConfig
        {
            /// <summary>
            /// Gets or sets the name of the channel ID property within the root of the provided <see cref="JObject"/>.
            /// </summary>
            public string ChannelIdProperty { get; set; } = WebhookJsonProperty.ChannelId;

            /// <summary>
            /// Gets or sets the full, top-down path to the channel ID property within the Stream webhook payload.
            /// </summary>
            /// <remarks>
            /// This will only be used when reporting exceptions, if a channel ID property is not found at <see cref="ChannelIdProperty"/>.
            /// </remarks>
            public string ChannelIdPath { get; set; } = WebhookJsonProperty.ChannelId;

            /// <summary>
            /// Gets or sets whether to throw an exception if the identifiers are missing.
            /// </summary>
            public bool ShouldThrowIfMissing { get; set; } = true;
        }

        /// <summary>
        /// Represents a <see cref="Group"/>'s identifiers parsed from a Stream webhook payload.
        /// </summary>
        private class ChatGroupIdentifiers
        {
            /// <summary>
            /// Gets or sets the <see cref="ChatChannel.Key"/>.
            /// </summary>
            /// <remarks>
            /// This will be defined for all channels - created within both Rock and Stream.
            /// </remarks>
            public string ChannelKey { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Group"/> identifier.
            /// </summary>
            /// <remarks>
            /// This will be defined for channels that were created within Rock.
            /// </remarks>
            public int? GroupId { get; set; }
        }

        #endregion Supporting Members
    }
}
