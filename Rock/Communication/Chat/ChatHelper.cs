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
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Exceptions;
using Rock.Communication.Chat.Sync;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Communication.Chat;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.Transactions;
using Rock.Utilities;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication.Chat
{
    /// <summary>
    /// A helper class to assist with common chat tasks.
    /// </summary>
    [RockLoggingCategory]
    internal class ChatHelper : IDisposable
    {
        #region Fields

        /// <summary>
        /// The backing field for the <see cref="RequiredAppRoles"/> property.
        /// </summary>
        private static readonly Lazy<List<string>> _requiredAppRoles = new Lazy<List<string>>( () =>
        {
            return Enum.GetValues( typeof( ChatRole ) )
                .Cast<ChatRole>()
                .Select( role =>
                {
                    var fieldInfo = typeof( ChatRole ).GetField( role.ToString() );
                    var descriptionAttribute = fieldInfo?.GetCustomAttributes( typeof( DescriptionAttribute ), false )
                        .Cast<DescriptionAttribute>()
                        .FirstOrDefault();

                    return descriptionAttribute?.Description ?? role.ToString();
                } )
                .ToList();
        } );

        /// <summary>
        /// A lazy-loaded dictionary of system group identifiers by their GUID strings.
        /// </summary>
        private static readonly Lazy<Dictionary<string, int>> _systemGroupIdsByGuid = new Lazy<Dictionary<string, int>>( () =>
        {
            return new Dictionary<string, int>
            {
                { SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS, GroupCache.GetId( SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid() ) ?? 0 },
                { SystemGuid.Group.GROUP_CHAT_BAN_LIST, GroupCache.GetId( SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid() ) ?? 0 },
            };
        } );

        /// <summary>
        /// The prefix to use for cache keys specific to the Rock chat system.
        /// </summary>
        private const string CacheKeyPrefix = "core-chat:";

        /// <summary>
        /// The prefix to use for a <see cref="GroupType"/>'s <see cref="ChatChannelType.Key"/>.
        /// </summary>
        private const string ChatChannelTypeKeyPrefix = "rock-grouptype-";

        /// <summary>
        /// The prefix to use for a <see cref="Group"/>'s <see cref="ChatChannel.Key"/>.
        /// </summary>
        private const string ChatChannelKeyPrefix = "rock-group-";

        /// <summary>
        /// The prefix to use for a <see cref="PersonAlias"/>'s <see cref="ChatUser.Key"/>.
        /// </summary>
        private const string ChatUserKeyPrefix = "rock-user-";

        /// <summary>
        /// The prefix to use for a chat-specific <see cref="PersonAlias"/> foreign key.
        /// </summary>
        public const string ChatPersonAliasForeignKeyPrefix = "core-chat:";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets whether chat is enabled, based on the current chat configuration from system settings.
        /// </summary>
        public static bool IsChatEnabled
        {
            get
            {
                var chatConfiguration = GetChatConfiguration();

                return chatConfiguration.ApiKey.IsNotNullOrWhiteSpace()
                    && chatConfiguration.ApiSecret.IsNotNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Gets a list of required, app-scoped roles that should exist in the external chat system.
        /// </summary>
        public static List<string> RequiredAppRoles => _requiredAppRoles.Value;

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat administrators group.
        /// </summary>
        public static int ChatAdministratorsGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS];

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat ban list group.
        /// </summary>
        public static int ChatBanListGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_BAN_LIST];

        /// <summary>
        /// Gets the URL to which the external chat provider should send webhook requests.
        /// </summary>
        public static string WebhookUrl
        {
            get
            {
                var publicApplicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
                return $"{publicApplicationRoot}api/v2/chat/Webhook";
            }
        }

        /// <summary>
        /// Gets the Rock-to-Chat sync configuration for this chat helper.
        /// </summary>
        public RockToChatSyncConfig RockToChatSyncConfig { get; } = new RockToChatSyncConfig();

        /// <summary>
        /// Gets the Chat-to-Rock sync configuration for this chat helper.
        /// </summary>
        public ChatToRockSyncConfig ChatToRockSyncConfig { get; } = new ChatToRockSyncConfig();

        /// <summary>
        /// Gets the <see cref="ILogger"/> that should be used to write log messages for this chat helper.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="Rock.Data.RockContext"/> that should be used to initialize Rock services and save any
        /// database changes made within this chat helper.
        /// </summary>
        private RockContext RockContext { get; }

        /// <summary>
        /// Gets whether this chat helper is responsible for disposing of the <see cref="RockContext"/>.
        /// </summary>
        private bool ShouldDisposeRockContext { get; }

        /// <summary>
        /// Gets the <see cref="IChatProvider"/> implementation used within this helper instance.
        /// </summary>
        private IChatProvider ChatProvider { get; }

        #endregion Properties

        #region Con/Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHelper"/> class.
        /// </summary>
        /// <param name="rockContext">The optional <see cref="Rock.Data.RockContext"/> that should be used to initialize
        /// services and save any database changes made within this chat helper. If a context is not provided, a new
        /// one will be created and disposed of within this chat helper. If a context IS provided, it will NOT be
        /// disposed of within this chat helper, but its underlying <see cref="System.Data.Entity.DbContext.SaveChanges()"/>
        /// will be called as needed.</param>
        public ChatHelper( RockContext rockContext = null )
        {
            Logger = RockLogger.LoggerFactory.CreateLogger( typeof( ChatHelper ).FullName );

            if ( rockContext != null )
            {
                RockContext = rockContext;
            }
            else
            {
                RockContext = new RockContext();
                ShouldDisposeRockContext = true;
            }

            ChatProvider = RockApp.Current.GetChatProvider();
            ChatProvider.RockToChatSyncConfig = RockToChatSyncConfig;
            ChatProvider.ChatToRockSyncConfig = ChatToRockSyncConfig;

            InitializeChatProvider();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if ( !ShouldDisposeRockContext )
            {
                return;
            }

            RockContext.Dispose();
        }

        #endregion Con/Destructors

        #region Public Methods

        #region Configuration & Keys

        /// <summary>
        /// Gets the current chat configuration from system settings.
        /// </summary>
        /// <returns>The current chat configuration.</returns>
        public static ChatConfiguration GetChatConfiguration()
        {
            var chatConfigurationJson = Rock.Web.SystemSettings.GetValue( SystemSetting.CHAT_CONFIGURATION );
            var chatConfiguration = chatConfigurationJson.FromJsonOrNull<ChatConfiguration>() ?? new ChatConfiguration();

            if ( chatConfiguration.ApiSecret.IsNotNullOrWhiteSpace() )
            {
                chatConfiguration.ApiSecret = Encryption.DecryptString( chatConfiguration.ApiSecret );
            }

            return chatConfiguration;
        }

        /// <summary>
        /// Saves the provided chat configuration to system settings.
        /// </summary>
        /// <param name="chatConfiguration">The chat configuration to save.</param>
        public static void SaveChatConfiguration( ChatConfiguration chatConfiguration )
        {
            if ( chatConfiguration == null )
            {
                chatConfiguration = new ChatConfiguration();
            }

            if ( chatConfiguration.ApiSecret.IsNotNullOrWhiteSpace() )
            {
                chatConfiguration.ApiSecret = Encryption.EncryptString( chatConfiguration.ApiSecret );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_CONFIGURATION, chatConfiguration.ToJson() );
        }

        /// <summary>
        /// Gets the <see cref="ChatChannelType.Key"/> for the provided <see cref="GroupType"/> identifier.
        /// </summary>
        /// <param name="groupTypeId">The <see cref="GroupType"/> identifier for which to get the <see cref="ChatChannelType.Key"/>.</param>
        /// <returns>The <see cref="ChatChannelType.Key"/>.</returns>
        public static string GetChatChannelTypeKey( int groupTypeId )
        {
            return $"{ChatChannelTypeKeyPrefix}{groupTypeId}";
        }

        /// <summary>
        /// Gets the <see cref="GroupType"/> identifier for the provided <see cref="ChatChannelType.Key"/>.
        /// </summary>
        /// <param name="chatChannelTypeKey">The <see cref="ChatChannelType.Key"/> for which to get the <see cref="GroupType"/> identifier.</param>
        /// <returns>The <see cref="GroupType"/> identifier or <see langword="null"/> if unable to parse.</returns>
        public static int? GetGroupTypeId( string chatChannelTypeKey )
        {
            if ( chatChannelTypeKey.IsNotNullOrWhiteSpace() )
            {
                return chatChannelTypeKey.Replace( ChatChannelTypeKeyPrefix, string.Empty ).AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ChatChannel.Key"/> for the provided <see cref="Group"/> identifier.
        /// </summary>
        /// <param name="groupCache">The <see cref="GroupCache"/> for which to get the <see cref="ChatChannel.Key"/>.</param>
        /// <returns>The <see cref="ChatChannel.Key"/> or <see langword="null"/>.</returns>
        public static string GetChatChannelKey( GroupCache groupCache )
        {
            if ( groupCache == null )
            {
                return null;
            }

            if ( groupCache.ChatChannelKey.IsNotNullOrWhiteSpace() )
            {
                return groupCache.ChatChannelKey;
            }

            return $"{ChatChannelKeyPrefix}{groupCache.Id}";
        }

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the provided <see cref="ChatChannel.Key"/>.
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> for which to get the <see cref="Group"/> identifier.</param>
        /// <returns>The <see cref="Group"/> identifier or <see langword="null"/> if unable to parse.</returns>
        public static int? GetGroupId( string chatChannelKey )
        {
            if ( chatChannelKey.IsNotNullOrWhiteSpace() )
            {
                return chatChannelKey.Replace( ChatChannelKeyPrefix, string.Empty ).AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ChatUser.Key"/> for the provided <see cref="PersonAlias"/> unique identifier.
        /// </summary>
        /// <param name="personAliasGuid">The <see cref="PersonAlias"/> unique identifier for which to get the <see cref="ChatUser.Key"/>.</param>
        /// <returns>The <see cref="ChatUser.Key"/>.</returns>
        public static string GetChatUserKey( Guid personAliasGuid )
        {
            return $"{ChatUserKeyPrefix}{personAliasGuid}".ToLower();
        }

        /// <summary>
        /// Gets the <see cref="PersonAlias"/> unique identifier for the provided <see cref="ChatUser.Key"/>.
        /// </summary>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/> for which to get the <see cref="PersonAlias"/> unique identifier.</param>
        /// <returns>The <see cref="PersonAlias"/> unique identifier or <see langword="null"/> if unable to parse.</returns>
        public static Guid? GetPersonAliasGuid( string chatUserKey )
        {
            if ( chatUserKey.IsNotNullOrWhiteSpace() )
            {
                return chatUserKey.Replace( ChatUserKeyPrefix, string.Empty ).AsGuidOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the foreign key for the provided <see cref="PersonAlias"/> unique identifier.
        /// </summary>
        /// <param name="personAliasGuid"></param>
        /// <returns></returns>
        public static string GetChatPersonAliasForeignKey( Guid personAliasGuid )
        {
            return $"{ChatPersonAliasForeignKeyPrefix}{GetChatUserKey( personAliasGuid )}";
        }

        /// <summary>
        /// Gets the chat system user responsible for performing synchronization tasks between Rock and the external
        /// chat system.
        /// </summary>
        /// <returns>A <see cref="ChatUser"/> representing the chat system user.</returns>
        public static ChatUser GetChatSystemUser()
        {
            return new ChatUser
            {
                Key = "rock-chat-synchronizer",
                Name = "Rock Chat Synchronizer",
                IsAdmin = true
            };
        }

        /// <summary>
        /// Initializes (or reinitializes) the <see cref="IChatProvider"/> instance managed by this chat helper.
        /// </summary>
        public void InitializeChatProvider()
        {
            ChatProvider.Initialize();
        }

        #endregion Configuration & Keys

        #region Caching

        /// <summary>
        /// Gets the cache key for the <see cref="Group"/> identifier, for the provided <see cref="ChatChannel.Key"/>.
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> for which to get the cache key.</param>
        /// <returns>The cache key or <see langword="null"/>.</returns>
        public static string GetChatChannelGroupIdCacheKey( string chatChannelKey )
        {
            if ( chatChannelKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return $"{CacheKeyPrefix}{chatChannelKey}_Id";
        }

        /// <summary>
        /// Tries to remove any cached chat channel resources.
        /// </summary>
        /// <param name="groupId">The <see cref="Group"/> identifier for which to remove cached chat channel resources.</param>
        public static void TryRemoveCachedChatChannel( int groupId )
        {
            var groupCache = GroupCache.Get( groupId );
            if ( groupCache != null )
            {
                var chatChannelKey = GetChatChannelKey( groupCache );
                if ( chatChannelKey.IsNotNullOrWhiteSpace() )
                {
                    RockCache.Remove( GetChatChannelGroupIdCacheKey( chatChannelKey ) );
                }
            }
        }

        #endregion Caching

        #region Authentication

        /// <summary>
        /// Gets a <see cref="ChatUserAuthentication"/> for the <see cref="Person"/> to use when authenticating with the
        /// chat provider.
        /// </summary>
        /// <param name="personId">The identifier of the <see cref="Person"/> for whom to get a <see cref="ChatUserAuthentication"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the <see cref="ChatUserAuthentication"/> or
        /// <see langword="null"/> if unable to find the <see cref="ChatUser"/> or get a token.
        /// </returns>
        public async Task<ChatUserAuthentication> GetChatUserAuthenticationAsync( int personId )
        {
            ChatUserAuthentication auth = null;

            if ( personId < 1 )
            {
                return auth;
            }

            try
            {
                var syncCommand = new SyncPersonToChatCommand
                {
                    PersonId = personId,
                    ShouldEnsureChatAliasExists = true
                };

                // Enforce a "full" sync of the chat user when authenticating.
                RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;

                var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( new List<SyncPersonToChatCommand> { syncCommand } );

                var chatUserResult = createOrUpdateChatUsersResult
                    ?.UserResults
                    .FirstOrDefault( r => r?.ChatUserKey.IsNotNullOrWhiteSpace() == true );

                if ( chatUserResult == null )
                {
                    return auth;
                }

                var token = await ChatProvider.GetChatUserTokenAsync( chatUserResult.ChatUserKey );
                if ( token.IsNullOrWhiteSpace() )
                {
                    return auth;
                }

                auth = new ChatUserAuthentication
                {
                    Token = token,
                    ChatUserKey = chatUserResult.ChatUserKey
                };
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( GetChatUserAuthenticationAsync ) );
            }

            return auth;
        }

        #endregion Authentication

        #region Synchronization: From Rock To Chat Provider

        /// <summary>
        /// Ensures app-level roles, permission grants and other settings are in place within the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncBooleanResult"/>.
        /// </returns>
        public async Task<ChatSyncBooleanResult> EnsureChatProviderAppIsSetUpAsync()
        {
            var result = new ChatSyncBooleanResult();

            if ( !IsChatEnabled )
            {
                return result;
            }

            try
            {
                var appSettingsUpdated = await ChatProvider.UpdateAppSettingsAsync();
                var appRolesExist = await ChatProvider.EnsureAppRolesExistAsync();
                var appGrantsExist = await ChatProvider.EnsureAppGrantsExistAsync();
                var systemUserExists = await ChatProvider.EnsureSystemUserExistsAsync();

                result.WasSuccessful =
                    appSettingsUpdated
                    && appRolesExist
                    && appGrantsExist
                    && systemUserExists;
            }
            catch ( Exception ex )
            {
                result.Exception = ex;

                LogError( ex, nameof( EnsureChatProviderAppIsSetUpAsync ) );
            }

            return result;
        }

        /// <summary>
        /// Synchronizes <see cref="GroupType"/>s from Rock to <see cref="ChatChannelType"/>s in the external chat system.
        /// </summary>
        /// <param name="groupTypes">The <see cref="GroupType"/>s to synchronize.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This one-way synchronization will result in <see cref="ChatChannelType"/>s being added, updated or deleted
        /// within the external chat system, but will NOT result in any changes being made to Rock <see cref="GroupType"/>s.
        /// </para>
        /// <para>
        /// WARNING! If a previously-synced channel type no longer has chat enabled (<see cref="GroupType.IsChatAllowed"/>),
        /// this synchronization will result in IMMEDIATE DELETION of the <see cref="ChatChannelType"/>, as well as ALL
        /// related <see cref="ChatChannel"/>s, <see cref="ChatChannelMember"/>s and messages in the external chat system!
        /// </para>
        /// </remarks>
        public async Task<ChatSyncCrudResult> SyncGroupTypesToChatProviderAsync( List<GroupType> groupTypes )
        {
            var result = new ChatSyncCrudResult();

            if ( !IsChatEnabled || groupTypes?.Any() != true )
            {
                return result;
            }

            try
            {
                // Get all of the existing channel types.
                var existingChannelTypes = await ChatProvider.GetAllChatChannelTypesAsync();

                var channelTypesToCreate = new List<ChatChannelType>();
                var channelTypesToUpdate = new List<ChatChannelType>();
                var channelTypesToDelete = new List<string>();

                // A mapping dictionary and local function to add a group type to the outgoing results collection only
                // AFTER we know it's been successfully synced with the external chat system.
                var groupTypeIdByChannelTypeKeys = new Dictionary<string, int>();
                void AddGroupTypeToResult( string channelTypeKey, ChatSyncType chatSyncType )
                {
                    if ( groupTypeIdByChannelTypeKeys.TryGetValue( channelTypeKey, out var groupTypeId ) )
                    {
                        var groupTypeIdString = groupTypeId.ToString();
                        switch ( chatSyncType )
                        {
                            case ChatSyncType.Skip:
                                result.Skipped.Add( groupTypeIdString );
                                break;
                            case ChatSyncType.Create:
                                result.Created.Add( groupTypeIdString );
                                break;
                            case ChatSyncType.Update:
                                result.Updated.Add( groupTypeIdString );
                                break;
                            case ChatSyncType.Delete:
                                result.Deleted.Add( groupTypeIdString );
                                break;
                        }
                    }
                }

                foreach ( var groupType in groupTypes )
                {
                    var channelType = TryConvertToChatChannelType( groupType );
                    if ( channelType == null )
                    {
                        continue;
                    }

                    groupTypeIdByChannelTypeKeys.Add( channelType.Key, groupType.Id );

                    // Does it already exist in the external chat system?
                    var existingChannelType = existingChannelTypes?.FirstOrDefault( ct => ct.Key == channelType.Key );

                    // For each chat-enabled group type, add or update the channel type in the external chat system.
                    if ( groupType.IsChatAllowed )
                    {
                        if ( existingChannelType != null )
                        {
                            /*
                                1/29/2025: JPH

                                When a channel type already exists in the external chat system, the only reason we would
                                currently want to "update" it is if one or more of the following are true:

                                    1. We want to enforce default permission grants per role (in which case we'll always
                                       overwrite whatever grants might already be in place).
                                    2. We detect that ANY of the roles don't already have at least one grant defined. If
                                       ALL roles already have at least one grant defined, this means [A] we've already
                                       synced this channel type at least once, and [B] the external chat system is now
                                       the system of truth for permission grants (as admins may fine-tune grants on that
                                       side).
                                    3. We want to enforce default settings/property values (excluding grants), in which
                                       case we'll always overwrite whatever property values might already be in place.

                                Reason: Avoid calling chat provider APIs when not needed & avoid overwriting existing
                                        channel type settings/permission grants.
                             */

                            if ( RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole && RockToChatSyncConfig.ShouldEnforceDefaultSettings )
                            {
                                // This is the simplest update scenario, as we're going to overwrite the entire channel
                                // type in the external chat system. The `IChatProvider` will already know to enforce
                                // these defaults because of the `SetChatProviderDefaultValueEnforcers()` call above.
                                channelTypesToUpdate.Add( channelType );
                                continue;
                            }

                            // The next update scenario is a bit more complex; if any (#'s 1-3 in the engineering note
                            // above) are true, update the channel type in the external chat system, but be sure to
                            // retain any existing grants when needed (as an admin might have already fine-tuned them)!

                            var anyGrantsToAdd = false;
                            Dictionary<string, List<string>> updatedGrants = null;
                            if ( !RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole )
                            {
                                updatedGrants = new Dictionary<string, List<string>>();

                                // Check if this channel type already has at least one grant defined for each of the
                                // required roles.
                                foreach ( var role in RequiredAppRoles )
                                {
                                    List<string> existingRoleGrants = null;
                                    existingChannelType.GrantsByRole?.TryGetValue( role, out existingRoleGrants );

                                    if ( existingRoleGrants?.Any() == true )
                                    {
                                        // Re-add these same grants so they're retained in the external chat system.
                                        updatedGrants.AddOrReplace( role, existingRoleGrants );
                                        continue;
                                    }

                                    List<string> defaultRoleGrants = null;
                                    ChatProvider.DefaultChannelTypeGrantsByRole?.TryGetValue( role, out defaultRoleGrants );

                                    if ( defaultRoleGrants?.Any() == true )
                                    {
                                        // Add the default grants for this role.
                                        updatedGrants.AddOrReplace( role, defaultRoleGrants );
                                        anyGrantsToAdd = true;
                                    }
                                }
                            }

                            if ( RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole
                                || RockToChatSyncConfig.ShouldEnforceDefaultSettings
                                || anyGrantsToAdd )
                            {
                                // If enforcing default permission grants or settings, the `IChatProvider` will already
                                // know to do so because of the `SetChatProviderDefaultValueEnforcers()` call above.
                                channelType.GrantsByRole = updatedGrants;
                                channelTypesToUpdate.Add( channelType );
                            }
                            else
                            {
                                // Add it to the results as an already-up-to-date group type.
                                AddGroupTypeToResult( channelType.Key, ChatSyncType.Skip );
                            }
                        }
                        else
                        {
                            // The channel type doesn't yet exist in the external chat system; create it.
                            channelTypesToCreate.Add( channelType );
                        }
                    }
                    else if ( existingChannelType != null )
                    {
                        // For each non-chat-enabled group type, delete any corresponding channel type in the external
                        // chat system.
                        channelTypesToDelete.Add( channelType.Key );
                    }
                }

                if ( channelTypesToDelete.Any() )
                {
                    var deletedKeys = await ChatProvider.DeleteChatChannelTypesAsync( channelTypesToDelete );
                    deletedKeys?.ForEach( k => AddGroupTypeToResult( k, ChatSyncType.Delete ) );
                }

                if ( channelTypesToCreate.Any() )
                {
                    var createdChannelTypes = await ChatProvider.CreateChatChannelTypesAsync( channelTypesToCreate );
                    createdChannelTypes?.ForEach( ct => AddGroupTypeToResult( ct?.Key, ChatSyncType.Create ) );
                }

                if ( channelTypesToUpdate.Any() )
                {
                    var updatedChannelTypes = await ChatProvider.UpdateChatChannelTypesAsync( channelTypesToUpdate );
                    updatedChannelTypes?.ForEach( ct => AddGroupTypeToResult( ct?.Key, ChatSyncType.Update ) );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;

                LogError( ex, nameof( SyncGroupTypesToChatProviderAsync ) );
            }

            return result;
        }

        /// <summary>
        /// Synchronizes <see cref="Group"/>s from Rock to <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="groups">The <see cref="Group"/>s to synchronize.</param>
        /// <param name="syncConfig">The optional configuration to use when syncing groups to the external chat system.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This one-way synchronization will result in <see cref="ChatChannel"/>s being added, updated or deleted
        /// within the external chat system, but will NOT result in any changes being made to Rock <see cref="Group"/>s.
        /// </para>
        /// <para>
        /// WARNING! If a previously-synced channel no longer has chat enabled (<see cref="Group.GetIsChatEnabled"/>),
        /// this synchronization will result in IMMEDIATE DELETION of the <see cref="ChatChannel"/> and ALL related
        /// <see cref="ChatChannelMember"/>s and messages in the external chat system!
        /// </para>
        /// </remarks>
        public async Task<ChatSyncCrudResult> SyncGroupsToChatProviderAsync( List<Group> groups, RockToChatGroupSyncConfig syncConfig = null )
        {
            var result = new ChatSyncCrudResult();

            // Only sync valid, already-saved groups.
            groups = groups?.Where( g => g?.Id > 0 && g.GroupTypeId > 0 ).ToList();

            if ( !IsChatEnabled || groups?.Any() != true )
            {
                return result;
            }

            if ( syncConfig == null )
            {
                syncConfig = new RockToChatGroupSyncConfig();
            }

            try
            {
                // Get the existing chat channels.
                var queryableChatChannelKeys = groups
                    .Select( g => ChatProvider.GetQueryableChatChannelKey( GroupCache.Get( g.Id ) ) )
                    .ToList();

                var existingChannels = await ChatProvider.GetChatChannelsAsync( queryableChatChannelKeys );

                var channelsToCreate = new List<ChatChannel>();
                var channelsToUpdate = new List<ChatChannel>();
                var channelsToDelete = new List<string>();

                var channelsToTriggerGroupMemberSync = new List<ChatChannel>();

                // A mapping dictionary and local function to add a group to the outgoing results collection only AFTER
                // we know it's been successfully synced with the external chat system.
                var groupIdByQueryableKeys = new Dictionary<string, int>();
                void AddGroupToResult( string queryableKey, ChatSyncType chatSyncType )
                {
                    if ( groupIdByQueryableKeys.TryGetValue( queryableKey, out var groupId ) )
                    {
                        var groupIdString = groupId.ToString();
                        switch ( chatSyncType )
                        {
                            case ChatSyncType.Skip:
                                result.Skipped.Add( groupIdString );
                                break;
                            case ChatSyncType.Create:
                                result.Created.Add( groupIdString );
                                break;
                            case ChatSyncType.Update:
                                result.Updated.Add( groupIdString );
                                break;
                            case ChatSyncType.Delete:
                                result.Deleted.Add( groupIdString );
                                break;
                        }
                    }
                }

                foreach ( var group in groups )
                {
                    var channel = TryConvertToChatChannel( group );
                    if ( channel == null )
                    {
                        continue;
                    }

                    groupIdByQueryableKeys.Add( channel.QueryableKey, group.Id );

                    // Does it already exist in the external chat system?
                    var existingChannel = existingChannels?.FirstOrDefault( c => c.QueryableKey == channel.QueryableKey );

                    // For each chat-enabled group, add or update the channel in the external chat system.
                    if ( group.GetIsChatEnabled() )
                    {
                        if ( existingChannel != null )
                        {
                            // Examine the group to see if anything has changed.
                            if ( existingChannel.Name != channel.Name
                                || existingChannel.IsLeavingAllowed != channel.IsLeavingAllowed
                                || existingChannel.IsPublic != channel.IsPublic
                                || existingChannel.IsAlwaysShown != channel.IsAlwaysShown )
                            {
                                channelsToUpdate.Add( channel );
                            }
                            else
                            {
                                // Add it to the results as an already-up-to-date group.
                                AddGroupToResult( channel.QueryableKey, ChatSyncType.Skip );

                                if ( syncConfig.ShouldSyncAllGroupMembers )
                                {
                                    channelsToTriggerGroupMemberSync.Add( channel );
                                }
                            }
                        }
                        else
                        {
                            // The channel doesn't exist yet in the external chat system; create it.
                            channelsToCreate.Add( channel );
                        }
                    }
                    else if ( existingChannel != null )
                    {
                        // For each non-chat-enabled group, delete any corresponding channel in the external chat system.
                        channelsToDelete.Add( channel.QueryableKey );
                    }
                }

                if ( channelsToDelete.Any() )
                {
                    var deletedKeys = await ChatProvider.DeleteChatChannelsAsync( channelsToDelete );
                    deletedKeys?.ForEach( queryableKey => AddGroupToResult( queryableKey, ChatSyncType.Delete ) );
                }

                if ( channelsToCreate.Any() )
                {
                    var createdChannels = await ChatProvider.CreateChatChannelsAsync( channelsToCreate );
                    createdChannels?.ForEach( c => AddGroupToResult( c.QueryableKey, ChatSyncType.Create ) );

                    if ( createdChannels?.Any() == true )
                    {
                        channelsToTriggerGroupMemberSync.AddRange( createdChannels );
                    }
                }

                if ( channelsToUpdate.Any() )
                {
                    var updatedChannels = await ChatProvider.UpdateChatChannelsAsync( channelsToUpdate );
                    updatedChannels?.ForEach( c => AddGroupToResult( c?.QueryableKey, ChatSyncType.Update ) );

                    if ( syncConfig.ShouldSyncAllGroupMembers && updatedChannels?.Any() == true )
                    {
                        channelsToTriggerGroupMemberSync.AddRange( updatedChannels );
                    }
                }

                foreach ( var channel in channelsToTriggerGroupMemberSync )
                {
                    // Try to sync the group members.
                    if ( groupIdByQueryableKeys.TryGetValue( channel.QueryableKey, out var groupId ) )
                    {
                        var membersResult = await SyncGroupMembersToChatProviderAsync( groupId, syncConfig );
                        result.InnerResults.Add( membersResult );
                    }
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;

                LogError( ex, nameof( SyncGroupsToChatProviderAsync ) );
            }

            return result;
        }

        /// <summary>
        /// Synchronizes <see cref="GroupMember"/>s from Rock to <see cref="ChatUser"/>s and <see cref="ChatChannelMember"/>s
        /// in the external chat system.
        /// </summary>
        /// <param name="groupId">The identifier of the <see cref="Group"/> whose <see cref="GroupMember"/>s should be synced.</param>
        /// <param name="groupSyncConfig">
        /// <para>
        /// The configuration to use when it's determined that the parent <see cref="Group"/> also needs to be synced.
        /// </para>
        /// <para>
        /// DO NOT manually provide this argument; it will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.
        /// </para>
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// This one-way synchronization will result in <see cref="ChatUser"/>s being added or updated, as well as
        /// <see cref="ChatChannelMember"/>s being added, updated or deleted within the external chat system, but will
        /// NOT result in any changes being made to Rock <see cref="GroupMember"/>s, <see cref="Person"/>s or
        /// <see cref="PersonAlias"/>es.
        /// </remarks>
        public async Task<ChatSyncCrudResult> SyncGroupMembersToChatProviderAsync( int groupId, RockToChatGroupSyncConfig groupSyncConfig = null )
        {
            if ( groupId <= 0 )
            {
                return new ChatSyncCrudResult();
            }

            // Get the sync commands for this group's members.
            var syncCommands = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.GroupId == groupId
                    && !gm.Person.IsDeceased
                )
                .Select( gm => new SyncGroupMemberToChatCommand
                {
                    GroupId = gm.GroupId,
                    PersonId = gm.PersonId
                } )
                .ToList();

            return await SyncGroupMembersToChatProviderAsync( syncCommands, groupSyncConfig );
        }

        /// <summary>
        /// Synchronizes <see cref="GroupMember"/>s from Rock to <see cref="ChatUser"/>s and <see cref="ChatChannelMember"/>s
        /// in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="GroupMember"/>s to sync.</param>
        /// <param name="groupSyncConfig">
        /// <para>
        /// The configuration to use when it's determined that the parent <see cref="Group"/> also needs to be synced.
        /// </para>
        /// <para>
        /// DO NOT manually provide this argument; it will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.
        /// </para>
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// This one-way synchronization will result in <see cref="ChatUser"/>s being added or updated, as well as
        /// <see cref="ChatChannelMember"/>s being added, updated or deleted within the external chat system, but will
        /// NOT result in any changes being made to Rock <see cref="GroupMember"/>s, <see cref="Person"/>s or
        /// <see cref="PersonAlias"/>es.
        /// </remarks>
        public async Task<ChatSyncCrudResult> SyncGroupMembersToChatProviderAsync( List<SyncGroupMemberToChatCommand> syncCommands, RockToChatGroupSyncConfig groupSyncConfig = null )
        {
            var result = new ChatSyncCrudResult();

            // Validate commands.
            syncCommands = syncCommands
                ?.Where( c => c?.GroupId > 0 && c.PersonId > 0 )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return result;
            }

            if ( groupSyncConfig == null )
            {
                groupSyncConfig = new RockToChatGroupSyncConfig();
            }

            // Keep track of this setting in case we override and need to set it back.
            var shouldEnsureChatUsersExist = RockToChatSyncConfig.ShouldEnsureChatUsersExist;

            try
            {
                // Don't let individual channel failures cause all to fail.
                var perChannelExceptions = new List<Exception>();

                #region Chat Ban List

                // First, we'll handle any members being added to or removed from the "Chat Ban List" Rock group.
                // They should be globally banned or unbanned in the external chat system.

                var chatBanListGroup = GroupCache.Get( ChatBanListGroupId );
                if ( chatBanListGroup != null )
                {
                    // Transfer these commands from the provided collection into a new one, so we don't process them again below.
                    var chatBanListCommands = new List<SyncGroupMemberToChatCommand>();
                    for ( var i = syncCommands.Count - 1; i >= 0; i-- )
                    {
                        var syncCommand = syncCommands[i];
                        if ( syncCommand.GroupId == chatBanListGroup.Id )
                        {
                            chatBanListCommands.Add( syncCommand );
                            syncCommands.RemoveAt( i );
                        }
                    }

                    if ( chatBanListCommands.Any() )
                    {
                        var syncPersonToChatCommands = chatBanListCommands
                            .GroupBy( c => c.PersonId )
                            .Select( g => g.Key )
                            .Distinct()
                            .Select( personId =>
                                new SyncPersonToChatCommand
                                {
                                    PersonId = personId,
                                    ShouldEnsureChatAliasExists = true
                                }
                            )
                            .ToList();

                        try
                        {
                            // Enforce a "full" sync of chat users when working with this particular group.
                            RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;

                            // The next method call does the following:
                            //  1) Ensures this person has a chat-specific person alias in Rock;
                            //  2) Ensures this person has a chat user in the external chat system.
                            var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                            // Add the created or updated chat users to the results (for logging).
                            result.InnerResults.Add( createOrUpdateChatUsersResult );

                            // Determine which commands represent "bans" and which represent "unbans", by checking if
                            // each person represented by the sync commands currently exits in the group.
                            var chatBanListMembers = GetRockChatChannelMembers( chatBanListCommands );

                            // Add the banned and unbanned chat users to the results (for logging).
                            var globalChatUserBanResult = new ChatSyncBanResult();
                            result.InnerResults.Add( globalChatUserBanResult );

                            // Get the chat user keys for those who should be banned.
                            var banChatUserKeys = createOrUpdateChatUsersResult
                                .UserResults
                                .Where( r =>
                                    chatBanListMembers.Any( m =>
                                        m.PersonId == r.PersonId
                                        && !m.ShouldDelete // This means they're in the "Chat Ban List" group.
                                    )
                                )
                                .Select( r => r.ChatUserKey )
                                .ToList();

                            if ( banChatUserKeys.Any() )
                            {
                                var bannedChatUserKeys = await ChatProvider.BanChatUsersAsync( banChatUserKeys );
                                if ( bannedChatUserKeys?.Any() == true )
                                {
                                    globalChatUserBanResult.Banned.UnionWith( bannedChatUserKeys );
                                }
                            }

                            // Get the chat user keys for those who should be unbanned.
                            var unbanChatUserKeys = createOrUpdateChatUsersResult
                                .UserResults
                                .Where( r =>
                                    chatBanListMembers.Any( m =>
                                        m.PersonId == r.PersonId
                                        && m.ShouldDelete // This means they were removed from the "Chat Ban List" group.
                                    )
                                )
                                .Select( r => r.ChatUserKey )
                                .ToList();

                            if ( unbanChatUserKeys.Any() )
                            {
                                var unbannedChatUserKeys = await ChatProvider.UnbanChatUsersAsync( unbanChatUserKeys );
                                if ( unbannedChatUserKeys?.Any() == true )
                                {
                                    globalChatUserBanResult.Unbanned.UnionWith( unbannedChatUserKeys );
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            perChannelExceptions.Add( ex );
                        }
                    }
                }

                #endregion Chat Ban List

                #region APP - Chat Administrators

                // Next, we'll handle any members being added to or removed from the "APP - Chat Administrators" Rock
                // security role group. These members usually don't need to be added to a chat channel, but instead only
                // need to have their chat user records created or updated accordingly, within the external chat system.
                var chatAminsGroup = GroupCache.Get( ChatAdministratorsGroupId );

                // However.. someone COULD enable chat for the "Security Role" group type, in which case we'll simply
                // perform a full (chat channel, down) sync of these members, which will effectively add or remove them
                // to/from the global `rock_admin` role in the external chat system.
                if ( chatAminsGroup?.GetIsChatEnabled() == true )
                {
                    if ( syncCommands.Any( c => c.GroupId == chatAminsGroup.Id ) )
                    {
                        // Since at least one sync command is for the chat administrators group, we'll enforce a "full"
                        // sync for this entire operation, as we want to ensure global `rock_admin` roles are toggled
                        // immediately in the external chat system.
                        RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;
                    }
                }
                else if ( chatAminsGroup != null )
                {
                    // Transfer these commands from the provided collection into a new one, so we don't process them again below.
                    var chatAdminsCommands = new List<SyncGroupMemberToChatCommand>();
                    for ( var i = syncCommands.Count - 1; i >= 0; i-- )
                    {
                        var syncCommand = syncCommands[i];
                        if ( syncCommand.GroupId == chatAminsGroup.Id )
                        {
                            chatAdminsCommands.Add( syncCommand );
                            syncCommands.RemoveAt( i );
                        }
                    }

                    if ( chatAdminsCommands.Any() )
                    {
                        var syncPersonToChatCommands = chatAdminsCommands
                            .GroupBy( c => c.PersonId )
                            .Select( g => g.Key )
                            .Distinct()
                            .Select( personId =>
                                new SyncPersonToChatCommand
                                {
                                    PersonId = personId,
                                    ShouldEnsureChatAliasExists = true
                                }
                            )
                            .ToList();

                        try
                        {
                            // Enforce a "full" sync of chat users when working with this particular group.
                            RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;

                            // The next method call does the following:
                            //  1) Ensures this person has a chat-specific person alias in Rock;
                            //  2) Ensures this person has a chat user in the external chat system.
                            var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                            // Add the created or updated chat users to the results (for logging).
                            result.InnerResults.Add( createOrUpdateChatUsersResult );
                        }
                        catch ( Exception ex )
                        {
                            perChannelExceptions.Add( ex );
                        }
                    }
                }

                #endregion APP - Chat Administrators

                // We'll aggregate the individual group member commands into per-channel commands, so we can interact
                // with the external chat provider in bulk, one channel at a time.
                var perChannelCommands = new List<SyncChannelMembersToChatCommand>();

                // A local function get or add a sync command for a given channel.
                SyncChannelMembersToChatCommand GetOrAddCommandForChannel( int groupId )
                {
                    var groupCache = GroupCache.Get( groupId );
                    if ( groupCache == null )
                    {
                        return null;
                    }

                    var chatChannelKey = GetChatChannelKey( groupCache );

                    var command = perChannelCommands.FirstOrDefault( c => c.ChatChannelKey == chatChannelKey );
                    if ( command == null )
                    {
                        command = new SyncChannelMembersToChatCommand
                        {
                            ChatChannelKey = chatChannelKey,
                            ChatChannelTypeKey = GetChatChannelTypeKey( groupCache.GroupTypeId )
                        };

                        perChannelCommands.Add( command );
                    }

                    return command;
                }

                // A mapping dictionary and local functions to add a rock chat channel member to the outgoing results
                // collection only AFTER we know they've been successfully synced with the external chat system.
                var memberByChannelMemberKeys = new Dictionary<string, RockChatChannelMember>();

                void MapMemberToChannelMemberKey( string chatChannelKey, string chatUserKey, RockChatChannelMember member )
                {
                    var channelMemberKey = $"{chatChannelKey}|{chatUserKey}";
                    if ( !memberByChannelMemberKeys.ContainsKey( channelMemberKey ) )
                    {
                        memberByChannelMemberKeys.Add( channelMemberKey, member );
                    }
                }

                void AddMemberToResult( string chatChannelKey, string chatUserKey, ChatSyncType chatSyncType )
                {
                    if ( memberByChannelMemberKeys.TryGetValue( $"{chatChannelKey}|{chatUserKey}", out var member ) )
                    {
                        var memberId = $"{member.GroupId}|{member.PersonId}";
                        switch ( chatSyncType )
                        {
                            case ChatSyncType.Skip:
                                result.Skipped.Add( memberId );
                                break;
                            case ChatSyncType.Create:
                                result.Created.Add( memberId );
                                break;
                            case ChatSyncType.Update:
                                result.Updated.Add( memberId );
                                break;
                            case ChatSyncType.Delete:
                                result.Deleted.Add( memberId );
                                break;
                        }
                    }
                }

                // Get the bulk Rock group member data needed to create, update or delete channel members in the
                // external chat system.
                var rockChatChannelMembers = GetRockChatChannelMembers( syncCommands );

                #region Channel Members to Delete

                // Handle commands to delete group members.
                var deleteMembers = rockChatChannelMembers
                    .Where( c =>
                        c.ShouldDelete
                        // Ensure the parent group currently has chat enabled. If the group doesn't have chat enabled,
                        // existing channel members should have already been deleted by other processes, or worst case:
                        // will get deleted by the chat sync job.
                        && GroupCache.Get( c.GroupId )?.GetIsChatEnabled() == true
                    )
                    .ToList();

                if ( deleteMembers.Any() )
                {
                    // For members we plan to delete, try to find their existing chat user key in Rock.
                    var deletePersonIds = deleteMembers
                        .GroupBy( c => c.PersonId )
                        .Select( g => g.Key )
                        .Distinct()
                        .ToList();

                    var rockChatUserPersonKeys = GetRockChatUserPersonKeys( deletePersonIds );

                    // Just in case any people don't have a chat user key, let's at least log them.
                    // Subbing "user" for "person" here, as this can get logged in the UI.
                    var membersWithoutChatPersonKeys = deleteMembers
                        .Where( c => !rockChatUserPersonKeys.Any( r => r.PersonId == c.PersonId ) )
                        .ToList();

                    if ( membersWithoutChatPersonKeys.Any() )
                    {
                        LogWarning(
                            nameof( SyncGroupMembersToChatProviderAsync ),
                            "Unable to delete chat channel members in the external chat system, as matching chat person keys could not be found in Rock. Members without chat person keys: {@MembersWithoutChatPersonKeys}",
                            membersWithoutChatPersonKeys
                        );
                    }

                    // Filter down to members that we're sure have keys.
                    deleteMembers = deleteMembers
                        .Except( membersWithoutChatPersonKeys )
                        .ToList();

                    // Organize members by channel.
                    foreach ( var member in deleteMembers )
                    {
                        var chatUserKey = rockChatUserPersonKeys
                            .First( k => k.PersonId == member.PersonId )
                            .ChatUserKey;

                        var syncChannelCommand = GetOrAddCommandForChannel( member.GroupId );
                        if ( syncChannelCommand == null )
                        {
                            // Could happen if the Rock group or group type were deleted since this command was issued.
                            continue;
                        }

                        if ( !syncChannelCommand.UserKeysToDelete.Contains( chatUserKey ) )
                        {
                            syncChannelCommand.UserKeysToDelete.Add( chatUserKey );
                            MapMemberToChannelMemberKey( syncChannelCommand.ChatChannelKey, chatUserKey, member );
                        }
                    }
                }

                #endregion Channel Members to Delete

                #region Channel Members to Create or Update

                // Handle commands to create new or update existing group members.
                var createOrUpdateMembers = rockChatChannelMembers
                    .Where( c =>
                        !c.ShouldDelete
                        && GroupCache.Get( c.GroupId )?.GetIsChatEnabled() == true
                    )
                    .ToList();

                if ( createOrUpdateMembers.Any() )
                {
                    // Chat channel members require chat users. We'll start by ensuring a user exists in the external
                    // chat system for each member.
                    var syncPersonToChatCommands = createOrUpdateMembers
                        .GroupBy( c => c.PersonId )
                        .Select( g => g.Key )
                        .Distinct()
                        .Select( personId =>
                            new SyncPersonToChatCommand
                            {
                                PersonId = personId,
                                ShouldEnsureChatAliasExists = true
                            }
                        )
                        .ToList();

                    // The next method call does the following:
                    //  1) Ensures this person has a chat-specific person alias in Rock;
                    //  2) Ensures this person has a chat user in the external chat system IF dictated by `RockToChatSyncConfig.ShouldEnsureChatUsersExist`.
                    var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                    // Add the created or updated chat users to the results (for logging).
                    result.InnerResults.Add( createOrUpdateChatUsersResult );

                    // Just in case any people still don't have a chat user key, let's at least log them. This will
                    // probably never happen. Subbing "user" for "person" here, as this can get logged in the UI.
                    var membersWithoutChatPersonKeys = createOrUpdateMembers
                        .Where( c => !createOrUpdateChatUsersResult.UserResults.Any( r => r.PersonId == c.PersonId ) )
                        .ToList();

                    if ( membersWithoutChatPersonKeys.Any() )
                    {
                        LogWarning(
                            nameof( SyncGroupMembersToChatProviderAsync ),
                            "Unable to create or update chat channel members in the external chat system, as matching chat person keys could not be found in Rock. Members without chat person keys: {@MembersWithoutChatPersonKeys}",
                            membersWithoutChatPersonKeys
                        );
                    }

                    // Filter down to members that we're sure have chat user keys.
                    createOrUpdateMembers = createOrUpdateMembers
                        .Except( membersWithoutChatPersonKeys )
                        .ToList();

                    // Organize members by channel.
                    foreach ( var member in createOrUpdateMembers )
                    {
                        var chatUserKey = createOrUpdateChatUsersResult
                            .UserResults
                            .First( r => r.PersonId == member.PersonId )
                            .ChatUserKey;

                        var syncChannelCommand = GetOrAddCommandForChannel( member.GroupId );
                        if ( syncChannelCommand == null )
                        {
                            // Could happen if the Rock group or group type were deleted since this command was issued.
                            continue;
                        }

                        if ( !syncChannelCommand.MembersToCreateOrUpdate.Any( m => m.ChatUserKey == chatUserKey ) )
                        {
                            var chatChannelMember = TryConvertToChatChannelMember( member, chatUserKey );
                            if ( chatChannelMember == null )
                            {
                                continue;
                            }

                            syncChannelCommand.MembersToCreateOrUpdate.Add( chatChannelMember );
                            MapMemberToChannelMemberKey( syncChannelCommand.ChatChannelKey, chatUserKey, member );
                        }
                    }
                }

                #endregion Channel Members to Create or Update

                foreach ( var command in perChannelCommands )
                {
                    try
                    {
                        // Get the existing members.
                        List<ChatChannelMember> existingMembers;
                        if ( command.DistinctChatUserKeys.Count == 1 )
                        {
                            existingMembers = new List<ChatChannelMember>();

                            // If this command only represents a single member, try to get just that one member from the
                            // external chat system.
                            var existingMember = await ChatProvider.GetChatChannelMemberAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                command.DistinctChatUserKeys.First()
                            );

                            if ( existingMember != null )
                            {
                                existingMembers.Add( existingMember );
                            }
                        }
                        else
                        {
                            // Otherwise, go ahead and get all members for this channel.
                            existingMembers = await ChatProvider.GetChatChannelMembersAsync( command.ChatChannelTypeKey, command.ChatChannelKey );
                        }

                        var membersToCreate = new List<ChatChannelMember>();
                        var membersToUpdate = new Dictionary<ChatChannelMember, ChatChannelMember>();
                        var membersToDelete = new List<string>();

                        foreach ( var chatChannelMember in command.MembersToCreateOrUpdate )
                        {
                            // Do they already exist in the external chat system?
                            var existingMember = existingMembers?.FirstOrDefault( m => m.ChatUserKey == chatChannelMember.ChatUserKey );

                            if ( existingMember != null )
                            {
                                // Examine the member to see if anything has changed.
                                if ( existingMember.Role != chatChannelMember.Role
                                    || existingMember.IsChatBanned != chatChannelMember.IsChatBanned
                                    || existingMember.IsChatMuted != chatChannelMember.IsChatMuted )
                                {
                                    // Add both the new and old instances so the chat provider can decide how to apply updates.
                                    membersToUpdate.Add( chatChannelMember, existingMember );
                                }
                                else
                                {
                                    // Add them to the results as an already up-to-date member.
                                    AddMemberToResult( command.ChatChannelKey, chatChannelMember.ChatUserKey, ChatSyncType.Skip );
                                }
                            }
                            else
                            {
                                // The member doesn't exist yet in the external chat system; create them.
                                membersToCreate.Add( chatChannelMember );
                            }
                        }

                        foreach ( var chatUserKey in command.UserKeysToDelete )
                        {
                            // Does it exist in the external chat system?
                            if ( existingMembers?.Any( m => m.ChatUserKey == chatUserKey ) == true )
                            {
                                membersToDelete.Add( chatUserKey );
                            }
                        }

                        if ( membersToDelete.Any() )
                        {
                            var deletedKeys = await ChatProvider.DeleteChatChannelMembersAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                membersToDelete
                            );

                            deletedKeys?.ForEach( k => AddMemberToResult( command.ChatChannelKey, k, ChatSyncType.Delete ) );
                        }

                        if ( membersToCreate.Any() )
                        {
                            var createdMembers = await ChatProvider.CreateChatChannelMembersAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                membersToCreate
                            );

                            createdMembers?.ForEach( m => AddMemberToResult( command.ChatChannelKey, m.ChatUserKey, ChatSyncType.Create ) );
                        }

                        if ( membersToUpdate.Any() )
                        {
                            var updatedMembers = await ChatProvider.UpdateChatChannelMembersAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                membersToUpdate
                            );

                            updatedMembers?.ForEach( m => AddMemberToResult( command.ChatChannelKey, m.ChatUserKey, ChatSyncType.Update ) );
                        }
                    }
                    catch ( Exception channelEx )
                    {
                        // If this failure was due to the channel not existing in the external chat system, we'll try to
                        // create it and then re-attempt to sync all of its members.
                        Group group = null;
                        if ( channelEx is MemberChatChannelNotFoundException memberChatChannelNotFoundException )
                        {
                            // Note that we only want to handle this exception if the channel was originally created
                            // within Rock; we don't want to accidentally recreate a Stream channel that's been deleted.
                            // Looking up the group ID using this method will accomplish this, as it will only return
                            // the ID if it's embedded within the chat channel key.
                            var groupId = GetGroupId( memberChatChannelNotFoundException.ChatChannelKey );

                            // We'll manage a hash set of group IDs we've already tried to sync using this approach, to
                            // prevent a recursive loop that could lead to a stack overflow exception.
                            var shouldSyncGroup = groupId.GetValueOrDefault() > 0
                                && (
                                    groupSyncConfig.AlreadySyncedGroupIds == null
                                    || groupSyncConfig.AlreadySyncedGroupIds.Add( groupId.Value )
                                );

                            if ( shouldSyncGroup )
                            {
                                group = new GroupService( RockContext ).GetNoTracking( groupId.Value );
                            }
                        }

                        if ( group != null )
                        {
                            // This inner try/catch allows us to ensure that even if this last attempt to sync this
                            // group fails, it still won't cause other groups within this sync attempt to fail.
                            try
                            {
                                if ( groupSyncConfig.AlreadySyncedGroupIds == null )
                                {
                                    groupSyncConfig.AlreadySyncedGroupIds = new HashSet<int> { group.Id };
                                }

                                // Enable syncing of group members for skipped & updated groups, just in case another
                                // process beats this one to creating the missing channel.
                                groupSyncConfig.ShouldSyncAllGroupMembers = true;

                                await SyncGroupsToChatProviderAsync(
                                    new List<Group> { group },
                                    groupSyncConfig
                                );
                            }
                            catch ( Exception syncGroupException )
                            {
                                perChannelExceptions.Add( syncGroupException );
                            }
                        }
                        else
                        {
                            perChannelExceptions.Add( channelEx );
                        }
                    }
                }

                if ( perChannelExceptions.Any() )
                {
                    throw new AggregateException( perChannelExceptions );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;

                LogError( ex, nameof( SyncGroupMembersToChatProviderAsync ) );
            }

            // Set this back to whatever it was, in case we overrode it above.
            RockToChatSyncConfig.ShouldEnsureChatUsersExist = shouldEnsureChatUsersExist;

            return result;
        }

        /// <summary>
        /// Creates new or updates existing <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="ChatUser"/>s to create or update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCreateOrUpdateUsersResult"/>.
        /// </returns>
        public async Task<ChatSyncCreateOrUpdateUsersResult> CreateOrUpdateChatUsersAsync( List<SyncPersonToChatCommand> syncCommands )
        {
            var result = new ChatSyncCreateOrUpdateUsersResult();

            // Validate commands.
            syncCommands = syncCommands
                ?.Where( c => c?.PersonId > 0 )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return result;
            }

            try
            {
                var chatConfiguration = GetChatConfiguration();

                // Ensure each person has a chat-specific person alias record in Rock IF instructed by their respective sync commands.
                var rockChatUserPeople = GetOrCreateRockChatUserPeople( syncCommands, chatConfiguration );

                if ( !RockToChatSyncConfig.ShouldEnsureChatUsersExist )
                {
                    result.UserResults.AddRange(
                        rockChatUserPeople
                            .Where( p => p.AlreadyExistedInRock )
                            .Select( p =>
                                new ChatSyncCreateOrUpdateUserResult
                                {
                                    ChatUserKey = p.ChatUserKey,
                                    PersonId = p.PersonId,
                                    IsAdmin = p.IsChatAdministrator
                                }
                            )
                    );

                    // Filter down to only those people who didn't already have a chat-specific person alias in Rock.
                    rockChatUserPeople = rockChatUserPeople
                        .Where( p => !p.AlreadyExistedInRock )
                        .ToList();
                }

                // Do we have any people to sync all the way to the chat provider?
                if ( !rockChatUserPeople.Any() )
                {
                    return result;
                }

                // Get the existing chat users.
                var chatUserKeys = rockChatUserPeople
                    .Where( p => p.ChatAliasGuid.HasValue )
                    .Select( p => p.ChatUserKey )
                    .ToList();

                // Get the existing users.
                var existingUsers = await ChatProvider.GetChatUsersAsync( chatUserKeys );

                var usersToCreate = new List<ChatUser>();
                var usersToUpdate = new List<ChatUser>();

                // A local function to add users to the outgoing results collection only AFTER we know they've been
                // successfully created or updated within the external chat system.
                void AddChatUserToResults( ChatUser chatUser, ChatSyncType chatSyncType )
                {
                    var chatUserPerson = rockChatUserPeople.FirstOrDefault( p => p.ChatUserKey == chatUser.Key );
                    if ( chatUserPerson == null )
                    {
                        // Should never happen.
                        return;
                    }

                    result.UserResults.Add(
                        new ChatSyncCreateOrUpdateUserResult
                        {
                            ChatUserKey = chatUser.Key,
                            PersonId = chatUserPerson.PersonId,
                            IsAdmin = chatUser.IsAdmin,
                            SyncTypePerformed = chatSyncType
                        }
                    );
                }

                // A local function for quick badge list comparison.
                bool AreBadgeListsEqual( List<ChatBadge> badges1, List<ChatBadge> badges2 )
                {
                    if ( badges1?.Count != badges2?.Count )
                    {
                        // Quick exit if counts differ.
                        return false;
                    }

                    var badgeSet1 = new HashSet<string>( badges1.Select( b => $"{b?.Key}|{b?.Name}|{b?.IconCssClass}" ) );
                    var badgeSet2 = new HashSet<string>( badges2.Select( b => $"{b?.Key}|{b?.Name}|{b?.IconCssClass}" ) );

                    return badgeSet1.SetEquals( badgeSet2 );
                }

                foreach ( var rockChatUserPerson in rockChatUserPeople )
                {
                    var chatUser = TryConvertToChatUser( rockChatUserPerson, chatConfiguration );
                    if ( chatUser == null )
                    {
                        continue;
                    }

                    // Do they already exist in the external chat system?
                    var existingUser = existingUsers?.FirstOrDefault( u => u.Key == chatUser.Key );

                    if ( existingUser != null )
                    {
                        // Examine the user to see if anything has changed.
                        var shouldUpdate = existingUser.Name != chatUser.Name
                            || existingUser.PhotoUrl != chatUser.PhotoUrl
                            || existingUser.IsAdmin != chatUser.IsAdmin
                            || existingUser.IsProfileVisible != chatUser.IsProfileVisible
                            || existingUser.IsOpenDirectMessageAllowed != chatUser.IsOpenDirectMessageAllowed;

                        if ( !shouldUpdate )
                        {
                            // Only compare badges as a last resort, if no other props already forced an update.
                            if ( !AreBadgeListsEqual( existingUser.Badges, chatUser.Badges ) )
                            {
                                shouldUpdate = true;
                            }
                        }

                        if ( shouldUpdate )
                        {
                            usersToUpdate.Add( chatUser );
                        }
                        else
                        {
                            // Add them to the results as an already-up-to-date user.
                            AddChatUserToResults( chatUser, ChatSyncType.Skip );
                        }
                    }
                    else
                    {
                        // The user doesn't exist yet in the external chat system; create them.
                        usersToCreate.Add( chatUser );
                    }
                }

                if ( usersToCreate.Any() )
                {
                    var createdUsers = await ChatProvider.CreateChatUsersAsync( usersToCreate );
                    createdUsers?.ForEach( u => AddChatUserToResults( u, ChatSyncType.Create ) );
                }

                if ( usersToUpdate.Any() )
                {
                    var updatedUsers = await ChatProvider.UpdateChatUsersAsync( usersToUpdate );
                    updatedUsers?.ForEach( u => AddChatUserToResults( u, ChatSyncType.Update ) );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;

                LogError( ex, nameof( CreateOrUpdateChatUsersAsync ) );
            }

            return result;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<List<Guid>> DeleteChatUsersAsync( List<Guid> personAliasGuids )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            /*
                --------------------------
                To delete users (IN BULK):

                    1) DeleteManyAsync(DeleteUsersRequest request)

                        public class DeleteUsersRequest
                        {
                            public IEnumerable<string> UserIds { get; set; }
                            public DeletionStrategy? UserDeletionStrategy { get; set; }
                            public DeletionStrategy? MessageDeletionStrategy { get; set; }
                            public DeletionStrategy? ConversationDeletionStrategy { get; set; }
                        }

                            public enum DeletionStrategy
                            {
                                None,
                                Soft,
                                Pruning,
                                Hard,
                            }
            */

            throw new NotImplementedException();
        }

        #endregion Synchronization: From Rock To Chat Provider

        #region Synchronization: From Chat Provider To Rock

        /// <inheritdoc cref="IChatProvider.ValidateWebhookRequestAsync(HttpRequestMessage)"/>
        public async Task<WebhookValidationResult> ValidateWebhookRequestAsync( HttpRequestMessage request )
        {
            WebhookValidationResult result;

            try
            {
                result = await ChatProvider.ValidateWebhookRequestAsync( request );
            }
            catch ( Exception ex )
            {
                result = new WebhookValidationResult( null, ex );
            }

            if ( result?.IsValid != true )
            {
                var operationName = nameof( ValidateWebhookRequestAsync );
                var requestBody = result?.RequestBody ?? string.Empty;
                var structuredLog = "{RequestBody}";

                if ( result?.Exception is InvalidChatWebhookRequestException validationEx )
                {
                    // Prefer a structured log without a stack trace.
                    LogError( operationName, $"{validationEx.Message} {structuredLog}", requestBody );
                }
                else
                {
                    // Fall back to logging unexpected exceptions.
                    LogError( result?.Exception, operationName, $"Webhook request is invalid. {structuredLog}", requestBody );
                }
            }

            return result;
        }

        /// <summary>
        /// Handles chat webhook requests by synchronizing data from the external chat system to Rock.
        /// </summary>
        /// <param name="webhookRequests">The list of webhook requests to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleChatWebhookRequestsAsync( List<ChatWebhookRequest> webhookRequests )
        {
            if ( webhookRequests?.Any() != true )
            {
                return;
            }

            try
            {
                var syncCommands = ChatProvider.GetChatToRockSyncCommands( webhookRequests );

                await SyncFromChatToRockAsync( syncCommands );
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( HandleChatWebhookRequestsAsync ) );
            }
        }

        /// <summary>
        /// Synchronizes data from the external chat system to Rock.
        /// </summary>
        /// <param name="syncCommands">The list of commands for data to sync.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// This one-way synchronization will result in <see cref="Group"/>s, <see cref="GroupMember"/>s and
        /// <see cref="PersonAlias"/>es being added, updated or deleted within Rock, but will NOT result in any changes
        /// being made to the external chat system.
        /// </para>
        /// </remarks>
        public async Task SyncFromChatToRockAsync( List<ChatToRockSyncCommand> syncCommands )
        {
            syncCommands = syncCommands
                ?.Where( c => c?.ShouldRetry == true )
                .ToList();

            if ( syncCommands?.Any() != true )
            {
                return;
            }

            try
            {
                // ---------------------------------------------------------
                // 1) Delete group members and chat-specific person aliases.
                var deleteChatPersonCommands = syncCommands.OfType<DeleteChatPersonInRockCommand>().ToList();

                DeleteChatUsersInRock( deleteChatPersonCommands );

                // Keep track of deleted chat users, so we can avoid syncing them in subsequent commands.
                var deletedChatUserKeys = new HashSet<string>(
                    deleteChatPersonCommands
                        .Where( c => c.WasCompleted )
                        .Select( c => c.ChatPersonKey )
                );

                // -------------------------------------
                // 2) Sync chat channels to Rock groups.
                var syncChatChannelCommands = syncCommands.OfType<SyncChatChannelToRockCommand>().ToList();

                SyncChatChannelsToRock( syncChatChannelCommands );

                // ---------------------------------------------------
                // 3) Sync chat channel members to Rock group members.
                var syncChatChannelMemberCommands = syncCommands.OfType<SyncChatChannelMemberToRockCommand>().ToList();

                // Avoid syncing chat channel members for chat users that were already deleted above.
                syncChatChannelMemberCommands.Where( c => deletedChatUserKeys.Contains( c.ChatPersonKey ) )
                    .ToList()
                    .ForEach( c => c.MarkAsSkipped() );

                SyncChatChannelMembersToRock( syncChatChannelMemberCommands.Where( c => c.ShouldRetry ).ToList() );

                // ---------------------------------------
                // 4) Sync global chat user ban statuses.
                var syncGlobalBannedStatusCommands = syncCommands.OfType<SyncChatBannedStatusToRockCommand>()
                    .Where( c => !c.IsChatChannelBan )
                    .ToList();

                // Avoid syncing global chat user bans for chat users that were already deleted above.
                syncGlobalBannedStatusCommands.Where( c => deletedChatUserKeys.Contains( c.ChatPersonKey ) )
                    .ToList()
                    .ForEach( c => c.MarkAsSkipped() );

                SyncGlobalBannedStatusesToRock( syncGlobalBannedStatusCommands.Where( c => c.ShouldRetry ).ToList() );

                // -----------------------------------------
                // 5) Sync chat channel member ban statuses.
                var syncChatChannelMemberBannedStatusCommands = syncCommands.OfType<SyncChatBannedStatusToRockCommand>()
                    .Where( c => c.IsChatChannelBan )
                    .ToList();

                // Avoid syncing chat channel member bans for chat users that were already deleted above.
                syncChatChannelMemberBannedStatusCommands.Where( c => deletedChatUserKeys.Contains( c.ChatPersonKey ) )
                    .ToList()
                    .ForEach( c => c.MarkAsSkipped() );

                SyncChatChannelMemberBannedStatusesToRock( syncChatChannelMemberBannedStatusCommands.Where( c => c.ShouldRetry ).ToList() );

                // -------------------------------------------
                // 6) Sync chat channel member muted statuses.
                var syncChatChannelMutedStatusCommands = syncCommands.OfType<SyncChatChannelMutedStatusToRockCommand>().ToList();

                // Avoid syncing chat channel member mutes for chat users that were already deleted above.
                syncChatChannelMutedStatusCommands.Where( c => deletedChatUserKeys.Contains( c.ChatPersonKey ) )
                    .ToList()
                    .ForEach( c => c.MarkAsSkipped() );

                SyncChatChannelMemberMutedStatusesToRock( syncChatChannelMutedStatusCommands.Where( c => c.ShouldRetry ).ToList() );
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( SyncFromChatToRockAsync ) );
            }
            finally
            {
                LogChatToRockSyncCommandOutcomes( syncCommands );
                await RequeueRecoverableChatToRockSyncCommandsAsync( syncCommands );
            }
        }

        /// <summary>
        /// Deletes chat users and their corresponding chat-enabled group members in Rock.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat users to delete.</param>
        private void DeleteChatUsersInRock( List<DeleteChatPersonInRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                if ( syncCommand.ChatSyncType != ChatSyncType.Delete )
                {
                    // Only delete commands should be processed here.
                    syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for chat person deletion commands." );
                    continue;
                }

                syncCommand.ResetForSyncAttempt();

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    var personAliasService = new PersonAliasService( rockContext );

                    var personIdentifier = GetChatToRockPersonIdentifier(
                        syncCommand.ChatPersonKey,
                        personAliasService,
                        isRecoverableIfPersonNotFound: true
                    );

                    if ( personIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted person.
                        syncCommand.MarkAsUnrecoverable( personIdentifier.UnrecoverableReason );
                        continue;
                    }

                    var chatPersonAlias = personIdentifier.PersonAlias;
                    if ( chatPersonAlias == null )
                    {
                        // No matching person alias found; it must have already been deleted.
                        syncCommand.MarkAsSkipped();
                        continue;
                    }

                    // Supplement the command.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;

                    // Delete this person's chat-related group member records. We'll load each full entity + eager-load the
                    // parent group, as the group member service knows how to delete vs. archive members when needed.
                    var groupMemberService = new GroupMemberService( rockContext );

                    // IMPORTANT: We're purposefully NOT removing them from the "Chat Ban List" group here.
                    //            If they're banned, they can only be unbanned manually.

                    var groupMembers = groupMemberService
                        .Queryable()
                        .Include( gm => gm.Group )
                        .Where( gm =>
                            gm.PersonId == chatPersonAlias.PersonId
                            && (
                                gm.GroupId == ChatAdministratorsGroupId
                                || (
                                    gm.Group.GroupType.IsChatAllowed
                                    && (
                                        gm.Group.GroupType.IsChatEnabledForAllGroups
                                        || gm.Group.IsChatEnabledOverride == true
                                    )
                                )
                            )
                        )
                        .ToList();

                    foreach ( var groupMember in groupMembers )
                    {
                        groupMemberService.Delete( groupMember );
                    }

                    personAliasService.Delete( chatPersonAlias );

                    rockContext.SaveChanges();

                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Synchronizes chat channels to Rock groups.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channels to sync.</param>
        private void SyncChatChannelsToRock( List<SyncChatChannelToRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                int? groupId = syncCommand.GroupId;
                int? groupTypeId = syncCommand.GroupTypeId;

                if ( !groupId.HasValue && syncCommand.ChatChannelKey.IsNullOrWhiteSpace() )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group ID and chat channel key are both missing from sync command; unable to identify group." );
                    continue;
                }

                if ( !groupTypeId.HasValue )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group type ID is missing from sync command." );
                    continue;
                }
                else
                {
                    // Only allow sync operations to be performed against chat-enabled Rock group types.
                    var groupTypeCache = GroupTypeCache.Get( groupTypeId.Value );
                    if ( groupTypeCache == null )
                    {
                        syncCommand.MarkAsUnrecoverable( $"Rock group type with ID {groupTypeId} could not be found." );
                        continue;
                    }
                    else if ( !groupTypeCache.IsChatAllowed )
                    {
                        syncCommand.MarkAsUnrecoverable( $"Rock group type with ID {groupTypeId} is not chat-enabled." );
                        continue;
                    }
                }

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    var groupService = new GroupService( rockContext );

                    // Try to get the targeted group.
                    Group group;

                    if ( groupId.HasValue )
                    {
                        group = groupService.Get( groupId.Value );

                        if ( group == null )
                        {
                            // Since the group's ID was provided, this means the group existed in Rock before we got this
                            // sync command. If we can't find a group matching the specified ID, we have nothing more to do.
                            if ( syncCommand.ChatSyncType == ChatSyncType.Delete )
                            {
                                // No need to report these delete attempts as failures.
                                syncCommand.MarkAsSkipped();
                                continue;
                            }

                            syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupId.Value} could not be found." );
                            continue;
                        }
                    }
                    else
                    {
                        group = groupService.Queryable().FirstOrDefault( g => g.ChatChannelKey == syncCommand.ChatChannelKey );
                    }

                    if ( group?.GetIsChatEnabled() == false )
                    {
                        // Only allow sync operations to be performed against chat-enabled Rock groups.
                        syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupId.Value} is not chat-enabled." );
                        continue;
                    }

                    var groupNotFoundMsg = $"Rock group with chat channel key '{syncCommand.ChatChannelKey}' could not be found.";

                    if ( syncCommand.ChatSyncType == ChatSyncType.Create )
                    {
                        if ( group != null )
                        {
                            // There's already a matching group.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        // Add the group.
                        groupService.Add(
                            new Group
                            {
                                Name = syncCommand.GroupName,
                                GroupTypeId = groupTypeId.Value,
                                ChatChannelKey = syncCommand.ChatChannelKey
                            }
                        );

                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Update )
                    {
                        if ( group == null )
                        {
                            // We don't have a matching group record [yet]; it's possible we got an update command
                            // before the corresponding create command completed. Send it back through the queue.
                            syncCommand.MarkAsRecoverable( groupNotFoundMsg );
                            continue;
                        }

                        // Update the group (but only if needed).
                        if ( group.Name == syncCommand.GroupName )
                        {
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        group.Name = syncCommand.GroupName;

                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Delete )
                    {
                        if ( group == null )
                        {
                            // We don't have a matching group record [yet]; it's possible we got a delete command
                            // before the corresponding create command completed. Send it back through the queue.
                            syncCommand.MarkAsRecoverable( groupNotFoundMsg );
                            continue;
                        }

                        // Note that Rock cascade-deletes group members; no need to delete them first.
                        var wasDeleted = groupService.Delete( group );
                        if ( !wasDeleted )
                        {
                            // Try to archive it instead.
                            groupService.Archive( group, null, false );
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for channel synchronizations." );
                        continue;
                    }

                    // If we got this far, the command was successfully completed.
                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Synchronizes chat channel members to Rock group members.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channel members to sync.</param>
        private void SyncChatChannelMembersToRock( List<SyncChatChannelMemberToRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                int? groupId = syncCommand.GroupId;
                if ( !groupId.HasValue && syncCommand.ChatChannelKey.IsNullOrWhiteSpace() )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group ID and chat channel key are both missing from sync command; unable to identify group." );
                    continue;
                }

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    // Try to get the targeted group.
                    var groupIdentifier = GetChatToRockGroupIdentifier(
                        groupId,
                        syncCommand.ChatChannelKey,
                        syncCommand.ChatSyncType,
                        rockContext
                    );

                    if ( groupIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted group.
                        syncCommand.MarkAsUnrecoverable( groupIdentifier.UnrecoverableReason );
                        continue;
                    }

                    if ( groupIdentifier.GroupCache == null )
                    {
                        if ( groupId.HasValue && syncCommand.ChatSyncType == ChatSyncType.Delete )
                        {
                            // No need to report these delete attempts as failures. Since the group couldn't be found,
                            // this means the child members have also been deleted (or archived).
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        // We don't have a matching group record [yet]; it's possible we got this group member command
                        // before the corresponding create channel command completed. Send it back through the queue.
                        syncCommand.MarkAsRecoverable( $"Rock group with chat channel key '{syncCommand.ChatChannelKey}' could not be found." );
                        continue;
                    }

                    var groupCache = groupIdentifier.GroupCache;
                    if ( !groupCache.GetIsChatEnabled() )
                    {
                        // Only allow sync operations to be performed against chat-enabled Rock groups.
                        syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupCache.Id} is not chat-enabled." );
                        continue;
                    }

                    // Supplement the command.
                    syncCommand.GroupId = groupCache.Id;

                    // Try to get the targeted person.
                    var personIdentifier = GetChatToRockPersonIdentifier(
                        syncCommand.ChatPersonKey,
                        new PersonAliasService( rockContext )
                    );

                    if ( personIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted person.
                        syncCommand.MarkAsUnrecoverable( personIdentifier.UnrecoverableReason );
                        continue;
                    }

                    // We now have the targeted person.
                    var chatPersonAlias = personIdentifier.PersonAlias;

                    // Supplement the command.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;

                    // Try to get the targeted group members (they might have more than one role).
                    var groupMemberService = new GroupMemberService( rockContext );

                    // Eager-load the parent group, as the group member service knows how to delete vs. archive members when needed.
                    var groupMembers = groupMemberService
                        .Queryable()
                        .Include( gm => gm.Group )
                        .Where( gm =>
                            gm.GroupId == groupCache.Id
                            && gm.PersonId == chatPersonAlias.PersonId
                        )
                        .ToList();

                    if ( syncCommand.ChatSyncType == ChatSyncType.Create )
                    {
                        if ( groupMembers.Any() )
                        {
                            // There's already a matching group member.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        // Add the group member.
                        int? groupRoleId;
                        var groupTypeCache = groupCache.GroupType;

                        if ( syncCommand.ChatRole.HasValue )
                        {
                            groupRoleId = groupTypeCache.Roles
                                .FirstOrDefault( r => r.ChatRole == syncCommand.ChatRole.Value )
                                ?.Id;
                        }
                        else
                        {
                            groupRoleId = groupTypeCache.DefaultGroupRoleId;
                        }

                        if ( !groupRoleId.HasValue )
                        {
                            syncCommand.MarkAsUnrecoverable( $"Rock group type with ID {groupTypeCache.Id} doesn't have a default group role; unable to assign person (ID {chatPersonAlias.PersonId})." );
                            continue;
                        }

                        groupMemberService.Add(
                            new GroupMember
                            {
                                GroupId = groupCache.Id,
                                PersonId = chatPersonAlias.PersonId,
                                GroupRoleId = groupRoleId.Value,
                                GroupMemberStatus = GroupMemberStatus.Active,
                                GroupTypeId = groupTypeCache.Id
                            }
                        );

                        // For new member creation, we actually DO want to sync back to the external chat system, to
                        // ensure the correct channel member role is in place.
                        rockContext.IsRockToChatSyncEnabled = true;
                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Delete )
                    {
                        if ( !groupMembers.Any() )
                        {
                            // We don't have a matching group member record [yet]; it's possible we got a delete command
                            // before the corresponding create command completed. Send it back through the queue.
                            syncCommand.MarkAsRecoverable( $"Rock group member could not be found (group ID {groupCache.Id}, person ID {chatPersonAlias.PersonId})." );
                            continue;
                        }

                        foreach ( var groupMember in groupMembers )
                        {
                            groupMemberService.Delete( groupMember );
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for channel member synchronizations." );
                        continue;
                    }

                    // If we got this far, the command was successfully completed.
                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Synchronizes global chat user banned statuses to Rock.
        /// </summary>
        /// <param name="syncCommands">The list of commands for global chat user bans to sync.</param>
        private void SyncGlobalBannedStatusesToRock( List<SyncChatBannedStatusToRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    // Try to get the targeted person.
                    var personIdentifier = GetChatToRockPersonIdentifier(
                        syncCommand.ChatPersonKey,
                        new PersonAliasService( rockContext )
                    );

                    if ( personIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted person.
                        syncCommand.MarkAsUnrecoverable( personIdentifier.UnrecoverableReason );
                        continue;
                    }

                    // We now have the targeted person.
                    var chatPersonAlias = personIdentifier.PersonAlias;

                    // Supplement the command.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;

                    // Try to get the targeted group members (they might have more than one role).
                    var groupMemberService = new GroupMemberService( rockContext );

                    // Eager-load the parent group, as the group member service knows how to delete vs. archive members when needed.
                    var groupMembers = groupMemberService
                        .Queryable()
                        .Include( gm => gm.Group )
                        .Where( gm =>
                            gm.GroupId == ChatBanListGroupId
                            && gm.PersonId == chatPersonAlias.PersonId
                        )
                        .ToList();

                    if ( syncCommand.ChatSyncType == ChatSyncType.Ban )
                    {
                        if ( groupMembers.Any() )
                        {
                            // This person is already banned.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        // Ban them.
                        var groupCache = GroupCache.Get( ChatBanListGroupId );
                        var groupRoleId = groupCache.GroupType.DefaultGroupRoleId;

                        if ( !groupRoleId.HasValue )
                        {
                            var action = syncCommand.ChatSyncType == ChatSyncType.Ban ? "ban" : "unban";
                            syncCommand.MarkAsUnrecoverable( $"Rock group type with ID {groupCache.GroupTypeId} doesn't have a default group role; unable to {action} person (ID {chatPersonAlias.PersonId})." );
                            continue;
                        }

                        groupMemberService.Add(
                            new GroupMember
                            {
                                GroupId = ChatBanListGroupId,
                                PersonId = chatPersonAlias.PersonId,
                                GroupRoleId = groupRoleId.Value,
                                GroupMemberStatus = GroupMemberStatus.Active,
                                GroupTypeId = groupCache.GroupTypeId
                            }
                        );

                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Unban )
                    {
                        if ( !groupMembers.Any() )
                        {
                            // This person isn't banned.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        // Unban them.
                        foreach ( var groupMember in groupMembers )
                        {
                            groupMemberService.Delete( groupMember );
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for global chat ban synchronizations." );
                        continue;
                    }

                    // If we got this far, the command was successfully completed.
                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Synchronizes chat channel member banned statuses to Rock.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channel member bans to sync.</param>
        private void SyncChatChannelMemberBannedStatusesToRock( List<SyncChatBannedStatusToRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                int? groupId = syncCommand.GroupId;
                if ( !groupId.HasValue && syncCommand.ChatChannelKey.IsNullOrWhiteSpace() )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group ID and chat channel key are both missing from sync command; unable to identify group." );
                    continue;
                }

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    // Try to get the targeted group.
                    var groupIdentifier = GetChatToRockGroupIdentifier(
                        groupId,
                        syncCommand.ChatChannelKey,
                        syncCommand.ChatSyncType,
                        rockContext
                    );

                    if ( groupIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted group.
                        syncCommand.MarkAsUnrecoverable( groupIdentifier.UnrecoverableReason );
                        continue;
                    }

                    if ( groupIdentifier.GroupCache == null )
                    {
                        // We don't have a matching group record [yet]; it's possible we got this group member command
                        // before the corresponding create channel command completed. Send it back through the queue.
                        syncCommand.MarkAsRecoverable( $"Rock group with chat channel key '{syncCommand.ChatChannelKey}' could not be found." );
                        continue;
                    }

                    var groupCache = groupIdentifier.GroupCache;
                    if ( !groupCache.GetIsChatEnabled() )
                    {
                        // Only allow sync operations to be performed against chat-enabled Rock groups.
                        syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupCache.Id} is not chat-enabled." );
                        continue;
                    }

                    // Supplement the command.
                    syncCommand.GroupId = groupCache.Id;

                    // Try to get the targeted person.
                    var personIdentifier = GetChatToRockPersonIdentifier(
                        syncCommand.ChatPersonKey,
                        new PersonAliasService( rockContext )
                    );

                    if ( personIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted person.
                        syncCommand.MarkAsUnrecoverable( personIdentifier.UnrecoverableReason );
                        continue;
                    }

                    // We now have the targeted person.
                    var chatPersonAlias = personIdentifier.PersonAlias;

                    // Supplement the command.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;

                    // Try to get the targeted group members (they might have more than one role).
                    var groupMembers = new GroupMemberService( rockContext )
                        .Queryable()
                        .Where( gm =>
                            gm.GroupId == groupCache.Id
                            && gm.PersonId == chatPersonAlias.PersonId
                        )
                        .ToList();

                    if ( !groupMembers.Any() )
                    {
                        // We don't have a matching group member record [yet]; it's possible we got a ban/unban command
                        // before the corresponding create command completed. Send it back through the queue.
                        syncCommand.MarkAsRecoverable( $"Rock group member could not be found (group ID {groupCache.Id}, person ID {chatPersonAlias.PersonId})." );
                        continue;
                    }

                    if ( syncCommand.ChatSyncType == ChatSyncType.Ban )
                    {
                        var groupMembersToBan = groupMembers
                            .Where( gm => !gm.IsChatBanned )
                            .ToList();

                        if ( !groupMembersToBan.Any() )
                        {
                            // This person is already banned.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        foreach ( var groupMember in groupMembersToBan )
                        {
                            groupMember.IsChatBanned = true;
                        }

                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Unban )
                    {
                        var groupMembersToUnban = groupMembers
                            .Where( gm => gm.IsChatBanned )
                            .ToList();

                        if ( !groupMembersToUnban.Any() )
                        {
                            // This person isn't banned.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        foreach ( var groupMember in groupMembersToUnban )
                        {
                            groupMember.IsChatBanned = false;
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for channel member ban synchronizations." );
                        continue;
                    }

                    // If we got this far, the command was successfully completed.
                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Synchronizes chat channel member muted statuses to Rock.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channel member mutes to sync.</param>
        private void SyncChatChannelMemberMutedStatusesToRock( List<SyncChatChannelMutedStatusToRockCommand> syncCommands )
        {
            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                int? groupId = syncCommand.GroupId;
                if ( !groupId.HasValue && syncCommand.ChatChannelKey.IsNullOrWhiteSpace() )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group ID and chat channel key are both missing from sync command; unable to identify group." );
                    continue;
                }

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    // Try to get the targeted group.
                    var groupIdentifier = GetChatToRockGroupIdentifier(
                        groupId,
                        syncCommand.ChatChannelKey,
                        syncCommand.ChatSyncType,
                        rockContext
                    );

                    if ( groupIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted group.
                        syncCommand.MarkAsUnrecoverable( groupIdentifier.UnrecoverableReason );
                        continue;
                    }

                    if ( groupIdentifier.GroupCache == null )
                    {
                        // We don't have a matching group record [yet]; it's possible we got this group member command
                        // before the corresponding create channel command completed. Send it back through the queue.
                        syncCommand.MarkAsRecoverable( $"Rock group with chat channel key '{syncCommand.ChatChannelKey}' could not be found." );
                        continue;
                    }

                    var groupCache = groupIdentifier.GroupCache;
                    if ( !groupCache.GetIsChatEnabled() )
                    {
                        // Only allow sync operations to be performed against chat-enabled Rock groups.
                        syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupCache.Id} is not chat-enabled." );
                        continue;
                    }

                    // Supplement the command.
                    syncCommand.GroupId = groupCache.Id;

                    // Try to get the targeted person.
                    var personIdentifier = GetChatToRockPersonIdentifier(
                        syncCommand.ChatPersonKey,
                        new PersonAliasService( rockContext )
                    );

                    if ( personIdentifier.UnrecoverableReason.IsNotNullOrWhiteSpace() )
                    {
                        // Unable to identify the targeted person.
                        syncCommand.MarkAsUnrecoverable( personIdentifier.UnrecoverableReason );
                        continue;
                    }

                    // We now have the targeted person.
                    var chatPersonAlias = personIdentifier.PersonAlias;

                    // Supplement the command.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;

                    // Try to get the targeted group members (they might have more than one role).
                    var groupMembers = new GroupMemberService( rockContext )
                        .Queryable()
                        .Where( gm =>
                            gm.GroupId == groupCache.Id
                            && gm.PersonId == chatPersonAlias.PersonId
                        )
                        .ToList();

                    if ( !groupMembers.Any() )
                    {
                        // We don't have a matching group member record [yet]; it's possible we got a mute/unmute command
                        // before the corresponding create command completed. Send it back through the queue.
                        syncCommand.MarkAsRecoverable( $"Rock group member could not be found (group ID {groupCache.Id}, person ID {chatPersonAlias.PersonId})." );
                        continue;
                    }

                    if ( syncCommand.ChatSyncType == ChatSyncType.Mute )
                    {
                        var groupMembersToMute = groupMembers
                            .Where( gm => !gm.IsChatMuted )
                            .ToList();

                        if ( !groupMembersToMute.Any() )
                        {
                            // This person is already muted.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        foreach ( var groupMember in groupMembersToMute )
                        {
                            groupMember.IsChatMuted = true;
                        }

                        rockContext.SaveChanges();
                    }
                    else if ( syncCommand.ChatSyncType == ChatSyncType.Unmute )
                    {
                        var groupMembersToUnmute = groupMembers
                            .Where( gm => gm.IsChatMuted )
                            .ToList();

                        if ( !groupMembersToUnmute.Any() )
                        {
                            // This person isn't muted.
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        foreach ( var groupMember in groupMembersToUnmute )
                        {
                            groupMember.IsChatMuted = false;
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        syncCommand.MarkAsUnrecoverable( $"ChatSyncType '{syncCommand.ChatSyncType.ConvertToString()}' is not supported for channel member mute synchronizations." );
                        continue;
                    }

                    // If we got this far, the command was successfully completed.
                    syncCommand.MarkAsCompleted();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ChatToRockGroupIdentifier"/> for the specified <see cref="Group"/> ID or chat channel key.
        /// </summary>
        /// <param name="groupId">The identifier to use for locating the <see cref="Group"/>.</param>
        /// <param name="chatChannelKey">The chat channel key to use for locating the <see cref="Group"/>.</param>
        /// <param name="chatSyncType">The type of synchronization operation being performed.</param>
        /// <param name="rockContext">The Rock context to use for database operations.</param>
        /// <returns>A <see cref="ChatToRockGroupIdentifier"/> object.</returns>
        private ChatToRockGroupIdentifier GetChatToRockGroupIdentifier( int? groupId, string chatChannelKey, ChatSyncType chatSyncType, RockContext rockContext )
        {
            var identifier = new ChatToRockGroupIdentifier();

            if ( groupId.HasValue )
            {
                identifier.GroupCache = GroupCache.Get( groupId.Value );

                if ( identifier.GroupCache == null )
                {
                    // Since the group's ID was provided, this means the group existed in Rock before we got this sync
                    // command. If we can't find a group matching the specified ID, we have nothing more to do.
                    if ( chatSyncType == ChatSyncType.Delete )
                    {
                        // No need to report these delete attempts as failures.
                        return identifier;
                    }

                    // But other commands that expect the group to exist should be marked as unrecoverable.
                    identifier.UnrecoverableReason = $"Rock group with ID {groupId.Value} could not be found.";
                    return identifier;
                }
            }
            else
            {
                // Try to get the group ID for the chat channel key.
                groupId = new GroupService( RockContext ).GetChatChannelGroupId( chatChannelKey );

                if ( groupId.HasValue )
                {
                    identifier.GroupCache = GroupCache.Get( groupId.Value );
                }
            }

            return identifier;
        }

        /// <summary>
        /// Gets the <see cref="ChatToRockPersonIdentifier"/> for the specified chat user key.
        /// </summary>
        /// <param name="chatUserKey">The chat user key to use for locating the <see cref="Person"/>.</param>
        /// <param name="personAliasService">The <see cref="PersonAliasService"/> to use for database operations.</param>
        /// <param name="isRecoverableIfPersonNotFound">Whether to allow the operation to be recoverable if the person is not found.</param>
        /// <returns>A <see cref="ChatToRockPersonIdentifier"/> object.</returns>
        private ChatToRockPersonIdentifier GetChatToRockPersonIdentifier( string chatUserKey, PersonAliasService personAliasService, bool isRecoverableIfPersonNotFound = false )
        {
            var identifier = new ChatToRockPersonIdentifier();

            if ( chatUserKey.IsNullOrWhiteSpace() )
            {
                identifier.UnrecoverableReason = "Chat person key is missing from sync command.";
                return identifier;
            }

            var personAliasGuid = GetPersonAliasGuid( chatUserKey );
            if ( !personAliasGuid.HasValue )
            {
                identifier.UnrecoverableReason = $"Chat person key is invalid ('{chatUserKey}').";
                return identifier;
            }

            // Assign the person alias GUID.
            identifier.PersonAliasGuid = personAliasGuid;

            // Try to find a matching person alias.
            identifier.PersonAlias = personAliasService.Get( personAliasGuid.Value );
            if ( identifier.PersonAlias == null && !isRecoverableIfPersonNotFound )
            {
                identifier.UnrecoverableReason = $"Rock person alias with GUID '{personAliasGuid}' could not be found.";
            }

            return identifier;
        }

        /// <summary>
        /// Logs chat-to-Rock sync command outcomes to Rock Logs.
        /// </summary>
        /// <param name="syncCommands">The list of sync commands to log.</param>
        private void LogChatToRockSyncCommandOutcomes( List<ChatToRockSyncCommand> syncCommands )
        {
            var operationName = nameof( SyncFromChatToRockAsync );
            var structuredLog = "{@SyncCommand}";

            foreach ( var syncCommand in syncCommands.Where( c => c.WasCompleted ) )
            {
                LogDebug( $"{operationName} succeeded", structuredLog, syncCommand );
            }

            foreach ( var syncCommand in syncCommands.Where( c => c.HasFailureReason && c.ShouldRetry ) )
            {
                LogWarning( $"{operationName} failed", $"{syncCommand.FailureReason} {structuredLog}", syncCommand );
            }

            foreach ( var syncCommand in syncCommands.Where( c => c.HasFailureReason && !c.ShouldRetry ) )
            {
                LogError( operationName, $"{syncCommand.FailureReason} {structuredLog}", syncCommand );
            }
        }

        /// <summary>
        /// Requeues recoverable chat-to-Rock sync commands for retrying.
        /// </summary>
        /// <param name="syncCommands">The list of sync commands that might need to be requeued.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RequeueRecoverableChatToRockSyncCommandsAsync( List<ChatToRockSyncCommand> syncCommands )
        {
            var recoverableCommands = syncCommands
                .Where( c => c.ShouldRetry )
                .ToList();

            // We'll wait a few seconds before requeuing these commands, to give Rock and the chat provider time to catch up.
            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            foreach ( var recoverableCommand in recoverableCommands )
            {
                new RequeueChatToRockSyncCommandTransaction( recoverableCommand ).Enqueue();
            }
        }

        #endregion Synchronization: From Chat Provider To Rock

        #endregion Public Methods

        #region Private Methods

        #region Logging

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        /// <param name="operationName">The name of the operation that was taking place.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        private void LogDebug( string operationName, string message, params object[] args )
        {
            var logMessage = $"{nameof( ChatHelper ).SplitCase()} > {operationName.SplitCase()}: {message}";
            Logger.LogDebug( logMessage, args );
        }

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>
        /// <param name="operationName">The name of the operation that was taking place.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        private void LogWarning( string operationName, string message, params object[] args )
        {
            var logMessage = $"{nameof( ChatHelper ).SplitCase()} > {operationName.SplitCase()}: {message}";
            Logger.LogWarning( logMessage, args );
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="operationName">The name of the operation that was taking place.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        private void LogError( string operationName, string message, params object[] args )
        {
            LogError( null, operationName, message, args );
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="operationName">The name of the operation that was taking place.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        private void LogError( Exception exception, string operationName, string message = null, params object[] args )
        {
            var logMessage = $"{nameof( ChatHelper ).SplitCase()} > {operationName.SplitCase()} failed.{( message.IsNotNullOrWhiteSpace() ? $" {message}" : string.Empty )}";

            if ( exception != null )
            {
                Logger.LogError( exception, logMessage, args );
            }
            else
            {
                Logger.LogError( logMessage, args );
            }
        }

        #endregion Logging

        #region Rock Model CRUD

        /// <summary>
        /// Gets existing or creates new chat-specific <see cref="PersonAlias"/> records within Rock, and returns the
        /// alias's unique identifier along with additional supporting data needed to add or update <see cref="ChatUser"/>s
        /// in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="RockChatUserPerson"/>s to get or create.</param>
        /// <param name="chatConfiguration">The cached <see cref="ChatConfiguration"/> to prevent repeated reads from
        /// system settings.</param>
        /// <returns>A list of <see cref="RockChatUserPerson"/>s.</returns>
        /// <remarks>
        /// For each provided <see cref="SyncPersonToChatCommand"/>, if a chat-specific <see cref="PersonAlias"/> record
        /// doesn't already exist for the <see cref="Person"/> AND <see cref="SyncPersonToChatCommand.ShouldEnsureChatAliasExists"/>
        /// is <see langword="false"/>, a <see cref="RockChatUserPerson"/> will NOT be returned for that <see cref="Person"/>.
        /// </remarks>
        private List<RockChatUserPerson> GetOrCreateRockChatUserPeople( List<SyncPersonToChatCommand> syncCommands, ChatConfiguration chatConfiguration )
        {
            // Start by getting the minimum data set needed to add or update chat users in the external chat system.
            var personQry = new PersonService( RockContext )
                .Queryable()
                .Where( p => !p.IsDeceased );

            var chatAliasQry = new PersonAliasService( RockContext )
                .Queryable()
                .Where( pa => pa.ForeignKey.StartsWith( ChatPersonAliasForeignKeyPrefix ) );

            var personIds = syncCommands.Select( c => c.PersonId ).Distinct().ToList();

            if ( personIds.Count == 1 )
            {
                // Most performant: limit queries to just this person.
                var firstPersonId = personIds.First();
                personQry = personQry.Where( p => p.Id == firstPersonId );
                chatAliasQry = chatAliasQry.Where( pa => pa.PersonId == firstPersonId );
            }
            else if ( personIds.Count < 1000 )
            {
                // For fewer than 1k people, allow a SQL `WHERE...IN` clause.
                personQry = personQry.Where( p => personIds.Contains( p.Id ) );
                chatAliasQry = chatAliasQry.Where( pa => personIds.Contains( pa.PersonId ) );
            }
            else
            {
                // For 1k or more people, create and join to an entity set.
                var entitySetOptions = new AddEntitySetActionOptions
                {
                    Name = $"{nameof( ChatHelper )}_{nameof( GetOrCreateRockChatUserPeople )}",
                    EntityTypeId = EntityTypeCache.Get<Person>().Id,
                    EntityIdList = personIds,
                    ExpiryInMinutes = 20
                };

                var entitySetService = new EntitySetService( RockContext );
                var entitySetId = entitySetService.AddEntitySet( entitySetOptions );
                var entitySetItemQry = entitySetService.GetEntityQuery( entitySetId ).Select( e => e.Id );

                personQry = personQry.Where( p => entitySetItemQry.Contains( p.Id ) );
                chatAliasQry = chatAliasQry.Where( pa => entitySetItemQry.Contains( pa.PersonId ) );
            }

            var chatAdministratorPersonIdQry = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.GroupId == ChatAdministratorsGroupId
                    && gm.GroupMemberStatus == GroupMemberStatus.Active
                    && !gm.Person.IsDeceased
                )
                .Select( gm => gm.PersonId );

            var rockChatUserPeople = personQry
                .GroupJoin(
                    chatAliasQry,
                    p => p.Id,
                    pa => pa.PersonId,
                    ( p, chatAliases ) => new
                    {
                        Person = p,
                        ChatAliases = chatAliases.Select( ca => new { ca.Id, ca.Guid } )
                    }
                )
                .GroupJoin(
                    chatAdministratorPersonIdQry,
                    p => p.Person.Id,
                    adminId => adminId,
                    ( p, admins ) => new { p.Person, p.ChatAliases, IsChatAdmin = admins.Any() }
                )
                .ToList() // Materialize everything in a single query; we'll perform in-memory sorting of chat aliases below.
                .Select( p => new RockChatUserPerson
                {
                    PersonId = p.Person.Id,
                    NickName = p.Person.NickName,
                    LastName = p.Person.LastName,
                    SuffixValueId = p.Person.SuffixValueId,
                    RecordTypeValueId = p.Person.RecordTypeValueId,
                    PhotoId = p.Person.PhotoId,
                    BirthYear = p.Person.BirthYear,
                    Gender = p.Person.Gender,
                    AgeClassification = p.Person.AgeClassification,
                    IsChatProfilePublic = p.Person.IsChatProfilePublic,
                    IsChatOpenDirectMessageAllowed = p.Person.IsChatOpenDirectMessageAllowed,
                    ChatAliasGuid = p.ChatAliases?.Any() == true
                        ? p.ChatAliases.OrderBy( a => a.Id ).First().Guid // Get the earliest chat alias in the case of multiple.
                        : ( Guid? ) null,
                    IsChatAdministrator = p.IsChatAdmin,
                    Badges = new List<ChatBadge>()
                } )
                .ToList();

            // If any people don't already have a chat alias, add them in bulk IF instructed by their respective sync commands.
            var newChatAliasesToSave = new List<PersonAlias>();

            // Iterate backwards for ease-of-removal for those people who will not be synced further.
            for ( var i = rockChatUserPeople.Count - 1; i >= 0; i-- )
            {
                var rockChatUserPerson = rockChatUserPeople[i];
                if ( rockChatUserPerson.ChatAliasGuid.HasValue )
                {
                    // This person already has a chat alias; move on.
                    continue;
                }

                // Find this person's sync command.
                var syncCommand = syncCommands.FirstOrDefault( c => c.PersonId == rockChatUserPerson.PersonId );
                if ( syncCommand?.ShouldEnsureChatAliasExists != true )
                {
                    // This person didn't already have a chat alias, and we're not going to add one.
                    // Remove them from the results and move on.
                    rockChatUserPeople.RemoveAt( i );
                    continue;
                }

                var guid = Guid.NewGuid();
                var chatAlias = new PersonAlias
                {
                    PersonId = rockChatUserPerson.PersonId,
                    Guid = guid,
                    ForeignKey = GetChatPersonAliasForeignKey( guid )
                };

                newChatAliasesToSave.Add( chatAlias );
                rockChatUserPerson.ChatAliasGuid = guid;
                rockChatUserPerson.AlreadyExistedInRock = false;
            }

            // If we don't have any people to sync further, return early.
            if ( !rockChatUserPeople.Any() )
            {
                return rockChatUserPeople;
            }

            if ( newChatAliasesToSave.Any() )
            {
                RockContext.BulkInsert( newChatAliasesToSave );
            }

            var peopleNeedingBadges = RockToChatSyncConfig.ShouldEnsureChatUsersExist
                ? rockChatUserPeople                                                    // Sync badges for ALL people.
                : rockChatUserPeople.Where( p => !p.AlreadyExistedInRock ).ToList();    // Sync badges for NEW people.

            if ( !peopleNeedingBadges.Any() )
            {
                return rockChatUserPeople;
            }

            // Build the badge rosters.
            var anyBadges = false;
            var badgeRosters = new List<RockChatBadgeRoster>();
            if ( chatConfiguration.ChatBadgeDataViewGuids?.Any() == true )
            {
                foreach ( var dataViewGuid in chatConfiguration.ChatBadgeDataViewGuids )
                {
                    var dataViewCache = DataViewCache.Get( dataViewGuid );
                    var dataViewIdKey = dataViewCache?.IdKey;

                    // Note that we're strictly enforcing ONLY persisted data views for this purpose.
                    if ( dataViewCache?.IsPersisted() != true || badgeRosters.Any( r => r.ChatBadge.Key == dataViewIdKey ) )
                    {
                        continue;
                    }

                    var badgePersonIds = dataViewCache.GetVolatileEntityIds().ToHashSet();
                    if ( !badgePersonIds.Any() )
                    {
                        continue;
                    }

                    ColorPair colorPair = null;
                    if ( dataViewCache.HighlightColor.IsNotNullOrWhiteSpace() )
                    {
                        colorPair = RockColor.CalculateColorPair( new RockColor( dataViewCache.HighlightColor ) );
                    }

                    badgeRosters.Add(
                        new RockChatBadgeRoster
                        {
                            ChatBadge = new ChatBadge
                            {
                                Key = dataViewCache.IdKey,
                                Name = dataViewCache.Name,
                                IconCssClass = dataViewCache.IconCssClass,
                                BackgroundColor = colorPair?.BackgroundColor?.ToHex(),
                                ForegroundColor = colorPair?.ForegroundColor?.ToHex()
                            },
                            PersonIds = badgePersonIds
                        }
                    );

                    anyBadges = true;
                }
            }

            peopleNeedingBadges.ForEach( p =>
            {
                if ( !anyBadges )
                {
                    p.Badges = new List<ChatBadge>();
                }
                else
                {
                    p.Badges = badgeRosters
                        .Where( r => r.PersonIds.Contains( p.PersonId ) )
                        .Select( r => r.ChatBadge )
                        .ToList();
                }
            } );

            return rockChatUserPeople;
        }

        /// <summary>
        /// Gets existing <see cref="ChatUser.Key"/> mappings for the provided <see cref="Person"/> identifiers.
        /// </summary>
        /// <param name="personIds">The list of <see cref="Person"/> identifiers for which to get
        /// <see cref="RockChatUserPersonKey"/>s.</param>
        /// <returns>A list of <see cref="RockChatUserPersonKey"/>s with one entry for each <see cref="Person"/> who
        /// already has a chat-specific <see cref="PersonAlias"/> record.</returns>
        ///
        /// BC TODO: Talk to Jason about this method, needed to make it public.
        public List<RockChatUserPersonKey> GetRockChatUserPersonKeys( List<int> personIds )
        {
            var personQry = new PersonService( RockContext )
                .Queryable()
                .Where( p => !p.IsDeceased );

            var chatAliasQry = new PersonAliasService( RockContext )
                .Queryable()
                .Where( pa => pa.ForeignKey.StartsWith( ChatPersonAliasForeignKeyPrefix ) );

            if ( personIds.Count == 1 )
            {
                // Most performant: limit queries to just this person.
                var firstPersonId = personIds.First();
                personQry = personQry.Where( p => p.Id == firstPersonId );
                chatAliasQry = chatAliasQry.Where( pa => pa.PersonId == firstPersonId );
            }
            else if ( personIds.Count < 1000 )
            {
                // For fewer than 1k people, allow a SQL `WHERE...IN` clause.
                personQry = personQry.Where( p => personIds.Contains( p.Id ) );
                chatAliasQry = chatAliasQry.Where( pa => personIds.Contains( pa.PersonId ) );
            }
            else
            {
                // For 1k or more people, create and join to an entity set.
                var entitySetOptions = new AddEntitySetActionOptions
                {
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatUserPersonKeys )}",
                    EntityTypeId = EntityTypeCache.Get<Person>().Id,
                    EntityIdList = personIds,
                    ExpiryInMinutes = 20
                };

                var entitySetService = new EntitySetService( RockContext );
                var entitySetId = entitySetService.AddEntitySet( entitySetOptions );
                var entitySetItemQry = entitySetService.GetEntityQuery( entitySetId ).Select( e => e.Id );

                personQry = personQry.Where( p => entitySetItemQry.Contains( p.Id ) );
                chatAliasQry = chatAliasQry.Where( pa => entitySetItemQry.Contains( pa.PersonId ) );
            }

            return personQry
                .GroupJoin(
                    chatAliasQry,
                    p => p.Id,
                    pa => pa.PersonId,
                    ( p, chatAliases ) => new
                    {
                        Person = p,
                        ChatAliases = chatAliases.Select( ca => new { ca.Id, ca.Guid } )
                    }
                )
                .ToList() // Materialize everything in a single query; we'll perform in-memory sorting of chat aliases below.
                .Select( p => new RockChatUserPersonKey
                {
                    PersonId = p.Person.Id,
                    ChatAliasGuid = p.ChatAliases?.Any() == true
                        ? p.ChatAliases.OrderBy( a => a.Id ).First().Guid // Get the earliest chat alias in the case of multiple.
                        : ( Guid? ) null,
                } )
                .Where( k => k.ChatAliasGuid.HasValue ) // Only include results that actually have a chat alias guid.
                .ToList();
        }

        /// <summary>
        /// Gets <see cref="RockChatChannelMember"/>s for the provided <see cref="SyncGroupMemberToChatCommand"/>s.
        /// </summary>
        /// <param name="commands">The list of <see cref="SyncGroupMemberToChatCommand"/>s for which to get
        /// <see cref="RockChatChannelMember"/>s.</param>
        /// <returns>
        /// A list of <see cref="RockChatChannelMember"/>s with one entry for each Rock <see cref="Group"/> and
        /// <see cref="Person"/> combination.
        /// </returns>
        private List<RockChatChannelMember> GetRockChatChannelMembers( List<SyncGroupMemberToChatCommand> commands )
        {
            /*
                2/12/2025: JPH

                When looking for group member records to sync to the external chat system, we need to follow a
                less-than-obvious plan of attack:

                    1. A group member record that is deleted, inactive (`GroupMemberStatus != GroupMemberStatus.Active`)
                       or archived (`IsArchived == true`) should sometimes result in the member being removed from the
                       external chat channel. BUT there might be another group member record for this person, that
                       should now be used as the source of truth instead.
                    2. If there are multiple group member records for a given group and person combination, we should
                       prefer the first active, non-archived record (but still allow an inactive, banned record if there
                       are no active ones), further sorted as follows:
                            `GroupRole.Order` (ascending)
                            `GroupRole.IsLeader` (descending)
                            `GroupRole.Id` (ascending)
                    3. If the selected group member record for a given person matches the following:
                            `GroupMemberStatus != GroupMemberStatus.Active`
                            `IsChatBanned == true`
                       Then we SHOULD NOT remove the member from the external chat system, so it can know that the
                       member has already been banned from the channel.

                Reason: Always return the "correct" group member for a given group & person combination.
             */

            var groupMemberService = new GroupMemberService( RockContext );
            var groupMemberQry = groupMemberService
                .Queryable() // Note that we're EXCLUDING archived members.
                .Where( gm => !gm.Person.IsDeceased );

            // Get the distinct group IDs represented within the commands.
            var groupIds = commands.Select( c => c.GroupId ).Distinct().ToList();
            if ( groupIds.Count == 1 )
            {
                // Most performant: limit queries to just this group.
                var firstGroupId = groupIds.First();
                groupMemberQry = groupMemberQry.Where( gm => gm.GroupId == firstGroupId );
            }
            else if ( groupIds.Count < 1000 )
            {
                // For fewer than 1k groups, allow a SQL `WHERE...IN` clause.
                groupMemberQry = groupMemberQry.Where( gm => groupIds.Contains( gm.GroupId ) );
            }
            else
            {
                // For 1k or more groups, create and join to an entity set.
                var entitySetOptions = new AddEntitySetActionOptions
                {
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatChannelMembers )}_Groups",
                    EntityTypeId = EntityTypeCache.Get<Group>().Id,
                    EntityIdList = groupIds,
                    ExpiryInMinutes = 20
                };

                var entitySetService = new EntitySetService( RockContext );
                var entitySetId = entitySetService.AddEntitySet( entitySetOptions );
                var entitySetItemQry = entitySetService.GetEntityQuery( entitySetId ).Select( e => e.Id );

                groupMemberQry = groupMemberQry.Where( gm => entitySetItemQry.Contains( gm.GroupId ) );
            }

            // Get the distinct person IDs represented within the commands.
            var personIds = commands.Select( c => c.PersonId ).Distinct().ToList();
            if ( personIds.Count == 1 )
            {
                // Most performant: limit queries to just this person.
                var firstPersonId = personIds.First();
                groupMemberQry = groupMemberQry.Where( gm => gm.PersonId == firstPersonId );
            }
            else if ( personIds.Count < 1000 )
            {
                // For fewer than 1k people, allow a SQL `WHERE...IN` clause.
                groupMemberQry = groupMemberQry.Where( gm => personIds.Contains( gm.PersonId ) );
            }
            else
            {
                // For 1k or more people, create and join to an entity set.
                var entitySetOptions = new AddEntitySetActionOptions
                {
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatChannelMembers )}_People",
                    EntityTypeId = EntityTypeCache.Get<Person>().Id,
                    EntityIdList = personIds,
                    ExpiryInMinutes = 20
                };

                var entitySetService = new EntitySetService( RockContext );
                var entitySetId = entitySetService.AddEntitySet( entitySetOptions );
                var entitySetItemQry = entitySetService.GetEntityQuery( entitySetId ).Select( e => e.Id );

                groupMemberQry = groupMemberQry.Where( gm => entitySetItemQry.Contains( gm.PersonId ) );
            }

            // Get all group members for each group & person combination.
            var membersByGroupAndPerson = groupMemberQry
                .Select( gm => new
                {
                    GroupMemberId = gm.Id,
                    gm.GroupId,
                    gm.PersonId,
                    gm.GroupMemberStatus,
                    GroupRoleId = gm.GroupRole.Id,
                    GroupRoleOrder = gm.GroupRole.Order,
                    GroupRoleIsLeader = gm.GroupRole.IsLeader,
                    gm.GroupRole.ChatRole,
                    gm.IsChatMuted,
                    gm.IsChatBanned
                } )
                .GroupBy( gm => new
                {
                    gm.GroupId,
                    gm.PersonId
                } )
                .AsEnumerable() // Materialize the query so inner `ToList()` calls don't result in multiple queries.
                .Select( g => new
                {
                    g.Key.GroupId,
                    g.Key.PersonId,
                    GroupMembers = g.ToList()
                } )
                .ToDictionary( g => $"{g.GroupId}|{g.PersonId}", g => g.GroupMembers );

            var rockChatChannelMembers = new List<RockChatChannelMember>();

            // Because the list of commands could represent a given group & person combination more than once, let's
            // keep track of those we've already seen to speed up this method's return.
            var alreadySeenCombinations = new HashSet<string>();

            foreach ( var command in commands )
            {
                var groupAndPersonKey = $"{command.GroupId}|{command.PersonId}";
                if ( !alreadySeenCombinations.Add( groupAndPersonKey ) )
                {
                    continue;
                }

                // Create and add the outgoing member to the collection; we'll refine them below.
                var rockChatChannelMember = new RockChatChannelMember
                {
                    GroupId = command.GroupId,
                    PersonId = command.PersonId
                };

                rockChatChannelMembers.Add( rockChatChannelMember );

                // Find all member records for this group & person combination.
                if ( !membersByGroupAndPerson.TryGetValue( groupAndPersonKey, out var members ) || !members.Any() )
                {
                    // This person is not currently a member of this group.
                    rockChatChannelMember.ShouldDelete = true;
                    continue;
                }

                var memberToSync = members
                    .OrderByDescending( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ) // Prefer active.
                    .ThenBy( gm => gm.GroupRoleOrder )
                    .ThenByDescending( gm => gm.GroupRoleIsLeader )
                    .ThenBy( gm => gm.GroupRoleId )
                    .First();

                if ( memberToSync.GroupMemberStatus != GroupMemberStatus.Active && !memberToSync.IsChatBanned )
                {
                    // Only delete non-active, non-banned members.
                    rockChatChannelMember.ShouldDelete = true;
                }
                else
                {
                    rockChatChannelMember.ChatRole = memberToSync.ChatRole;
                    rockChatChannelMember.IsChatMuted = memberToSync.IsChatMuted;
                    rockChatChannelMember.IsChatBanned = memberToSync.IsChatBanned;
                    rockChatChannelMember.ShouldDelete = false;
                }
            }

            return rockChatChannelMembers;
        }

        #endregion Rock Model CRUD

        #region Converters: From Rock Models To Rock Chat DTOs

        /// <summary>
        /// Tries to convert a <see cref="GroupType"/> to a <see cref="ChatChannelType"/>.
        /// </summary>
        /// <param name="groupType">The <see cref="GroupType"/> to convert.</param>
        /// <returns>A <see cref="ChatChannelType"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannelType TryConvertToChatChannelType( GroupType groupType )
        {
            if ( groupType?.Id < 1 )
            {
                return null;
            }

            return new ChatChannelType
            {
                Key = GetChatChannelTypeKey( groupType.Id )
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="Group"/> to a <see cref="ChatChannel"/>.
        /// </summary>
        /// <param name="group">The <see cref="Group"/> to convert.</param>
        /// <returns>A <see cref="ChatChannel"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannel TryConvertToChatChannel( Group group )
        {
            if ( group?.Id < 1 || group.GroupTypeId < 1 )
            {
                return null;
            }

            var groupCache = GroupCache.Get( group.Id );
            if ( groupCache == null )
            {
                return null;
            }

            return new ChatChannel
            {
                Key = GetChatChannelKey( groupCache ),
                ChatChannelTypeKey = GetChatChannelTypeKey( group.GroupTypeId ),
                QueryableKey = ChatProvider.GetQueryableChatChannelKey( groupCache ),
                Name = group.Name,
                IsLeavingAllowed = group.GetIsLeavingChatChannelAllowed(),
                IsPublic = group.GetIsChatChannelPublic(),
                IsAlwaysShown = group.GetIsChatChannelAlwaysShown()
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="RockChatChannelMember"/> and <paramref name="chatUserKey"/> to a
        /// <see cref="ChatChannelMember"/>.
        /// </summary>
        /// <param name="rockChatChannelMember">The <see cref="RockChatChannelMember"/> to convert.</param>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/> that represents this <see cref="ChatChannelMember"/>.</param>
        /// <returns>A <see cref="ChatChannelMember"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannelMember TryConvertToChatChannelMember( RockChatChannelMember rockChatChannelMember, string chatUserKey )
        {
            if ( rockChatChannelMember == null || chatUserKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ChatChannelMember
            {
                ChatUserKey = chatUserKey,
                Role = rockChatChannelMember.ChatRole.GetDescription(),
                IsChatMuted = rockChatChannelMember.IsChatMuted,
                IsChatBanned = rockChatChannelMember.IsChatBanned
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="RockChatUserPerson"/> to a <see cref="ChatUser"/>.
        /// </summary>
        /// <param name="rockChatUserPerson">The <see cref="RockChatUserPerson"/> to convert.</param>
        /// <param name="chatConfiguration">The cached <see cref="ChatConfiguration"/> to prevent repeated reads from
        /// system settings.</param>
        /// <returns>A <see cref="ChatUser"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatUser TryConvertToChatUser( RockChatUserPerson rockChatUserPerson, ChatConfiguration chatConfiguration )
        {
            if ( rockChatUserPerson?.ChatAliasGuid == null )
            {
                return null;
            }

            var fullName = Person.FormatFullName(
                rockChatUserPerson.NickName,
                rockChatUserPerson.LastName,
                rockChatUserPerson.SuffixValueId,
                rockChatUserPerson.RecordTypeValueId
            );

            var photoUrlPerson = new Person
            {
                NickName = rockChatUserPerson.NickName,
                LastName = rockChatUserPerson.LastName,
                PhotoId = rockChatUserPerson.PhotoId,
                BirthYear = rockChatUserPerson.BirthYear,
                Gender = rockChatUserPerson.Gender,
                RecordTypeValueId = rockChatUserPerson.RecordTypeValueId,
                AgeClassification = rockChatUserPerson.AgeClassification
            };

            var publicApplicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var photoUrl = $"{publicApplicationRoot}{Person.GetPersonPhotoUrl( photoUrlPerson ).TrimStart( '~', '/' )}";

            var isProfileVisible = rockChatUserPerson.IsChatProfilePublic.HasValue
                ? rockChatUserPerson.IsChatProfilePublic.Value
                : chatConfiguration.AreChatProfilesVisible;

            var isOpenDirectMessageAllowed = rockChatUserPerson.IsChatOpenDirectMessageAllowed.HasValue
                ? rockChatUserPerson.IsChatOpenDirectMessageAllowed.Value
                : chatConfiguration.IsOpenDirectMessagingAllowed;

            return new ChatUser
            {
                Key = GetChatUserKey( rockChatUserPerson.ChatAliasGuid.Value ),
                Name = fullName,
                PhotoUrl = photoUrl,
                IsAdmin = rockChatUserPerson.IsChatAdministrator,
                IsProfileVisible = isProfileVisible,
                IsOpenDirectMessageAllowed = isOpenDirectMessageAllowed,
                Badges = rockChatUserPerson.Badges
            };
        }

        #endregion Converters: From Rock Models To Rock Chat DTOs

        #endregion Private Methods

        #region Supporting Members

        /// <summary>
        /// Represents a Rock <see cref="Group"/> identifier for chat-to-Rock synchronization.
        /// </summary>
        private class ChatToRockGroupIdentifier
        {
            /// <summary>
            /// Gets or sets the <see cref="GroupCache"/> for the <see cref="Group"/>.
            /// </summary>
            public GroupCache GroupCache { get; set; }

            /// <summary>
            /// Gets or sets the unrecoverable reason why the <see cref="Group"/> could not be found.
            /// </summary>
            public string UnrecoverableReason { get; set; }
        }

        /// <summary>
        /// Represents a Rock <see cref="Person"/> identifier for chat-to-Rock synchronization.
        /// </summary>
        private class ChatToRockPersonIdentifier
        {
            /// <summary>
            /// Gets or sets the GUID of the <see cref="PersonAlias"/> for the person.
            /// </summary>
            public Guid? PersonAliasGuid { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="PersonAlias"/> for the person.
            /// </summary>
            public PersonAlias PersonAlias { get; set; }

            /// <summary>
            /// Gets or sets the unrecoverable reason why the <see cref="Person"/> could not be found.
            /// </summary>
            public string UnrecoverableReason { get; set; }
        }

        #endregion Supporting Members
    }
}
