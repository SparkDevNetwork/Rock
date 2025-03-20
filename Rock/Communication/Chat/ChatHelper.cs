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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
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
    [RockInternal( "17.1", true )]
    public class ChatHelper : IDisposable
    {
        #region Fields

        /// <summary>
        /// A lock object for thread-safe saving of chat configuration to system settings.
        /// </summary>
        private static readonly object _saveChatConfigurationLock = new object();

        /// <summary>
        /// A helper method to lazily get or create the chat system user GUID.
        /// </summary>
        /// <returns>The lazy, chat system user GUID.</returns>
        private static Lazy<Guid> GetOrCreateChatSystemUserGuid()
        {
            lock ( _saveChatConfigurationLock )
            {
                return new Lazy<Guid>( () =>
                {
                    var chatConfiguration = GetChatConfiguration();
                    if ( !chatConfiguration.SystemUserGuid.HasValue )
                    {
                        chatConfiguration.SystemUserGuid = Guid.NewGuid();
                        SaveChatConfiguration( chatConfiguration );
                    }

                    return chatConfiguration.SystemUserGuid.Value;
                } );
            }
        }

        /// <summary>
        /// The backing field for the <see cref="ChatSystemUserGuid"/> property.
        /// </summary>
        private static Lazy<Guid> _chatSystemUserGuid = GetOrCreateChatSystemUserGuid();

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
                { SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES, GroupCache.GetId( SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES.AsGuid() ) ?? 0 },
                { SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS, GroupCache.GetId( SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS.AsGuid() ) ?? 0 }
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
        /// The value to use for a chat-specific <see cref="PersonAlias.Name"/>.
        /// </summary>
        internal const string ChatPersonAliasName = "core-chat";

        /// <summary>
        /// A semaphore for thread-safe deletion of deceased individuals.
        /// </summary>
        private static readonly SemaphoreSlim _deleteDeceasedIndividualsSemaphore = new SemaphoreSlim( 1, 1 );

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
        /// Gets the chat system user GUID.
        /// </summary>
        internal static Guid ChatSystemUserGuid => _chatSystemUserGuid.Value;

        /// <summary>
        /// Gets a list of required, app-scoped roles that should exist in the external chat system.
        /// </summary>
        internal static List<string> RequiredAppRoles => _requiredAppRoles.Value;

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat administrators group.
        /// </summary>
        internal static int ChatAdministratorsGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS];

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat ban list group.
        /// </summary>
        internal static int ChatBanListGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_BAN_LIST];

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat direct messages group.
        /// </summary>
        internal static int ChatDirectMessagesGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES];

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the chat shared channels group.
        /// </summary>
        internal static int ChatSharedChannelsGroupId => _systemGroupIdsByGuid.Value[SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS];

        /// <summary>
        /// Gets the URL to which the external chat provider should send webhook requests.
        /// </summary>
        internal static string WebhookUrl
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
        internal RockToChatSyncConfig RockToChatSyncConfig { get; } = new RockToChatSyncConfig();

        /// <summary>
        /// Gets the <see cref="ILogger"/> that should be used to write log messages for this chat helper.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the log message prefix to be used for all log messages.
        /// </summary>
        private string LogMessagePrefix => $"{nameof( ChatHelper ).SplitCase()} >";

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

        #region Internal Methods

        #region Configuration & Keys

        /// <summary>
        /// Gets the current chat configuration from system settings.
        /// </summary>
        /// <returns>The current chat configuration.</returns>
        internal static ChatConfiguration GetChatConfiguration()
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
        internal static void SaveChatConfiguration( ChatConfiguration chatConfiguration )
        {
            if ( chatConfiguration == null )
            {
                chatConfiguration = new ChatConfiguration();
            }

            if ( chatConfiguration.ApiSecret.IsNotNullOrWhiteSpace() )
            {
                chatConfiguration.ApiSecret = Encryption.EncryptString( chatConfiguration.ApiSecret );
            }

            lock ( _saveChatConfigurationLock )
            {
                Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_CONFIGURATION, chatConfiguration.ToJson() );
            }
        }

        /// <summary>
        /// Gets the <see cref="ChatChannelType.Key"/> for the provided <see cref="GroupType"/> identifier.
        /// </summary>
        /// <param name="groupTypeId">The <see cref="GroupType"/> identifier for which to get the <see cref="ChatChannelType.Key"/>.</param>
        /// <returns>The <see cref="ChatChannelType.Key"/>.</returns>
        internal static string GetChatChannelTypeKey( int groupTypeId )
        {
            return $"{ChatChannelTypeKeyPrefix}{groupTypeId}";
        }

        /// <summary>
        /// Gets the <see cref="GroupType"/> identifier for the provided <see cref="ChatChannelType.Key"/>.
        /// </summary>
        /// <param name="chatChannelTypeKey">The <see cref="ChatChannelType.Key"/> for which to get the <see cref="GroupType"/> identifier.</param>
        /// <returns>The <see cref="GroupType"/> identifier or <see langword="null"/> if unable to parse.</returns>
        internal static int? GetGroupTypeId( string chatChannelTypeKey )
        {
            if ( chatChannelTypeKey.IsNotNullOrWhiteSpace() )
            {
                return chatChannelTypeKey.Replace( ChatChannelTypeKeyPrefix, string.Empty ).AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ChatChannel.Key"/> for the provided <see cref="GroupCache"/>.
        /// </summary>
        /// <param name="groupId">The <see cref="Group"/> identifier for which to get the <see cref="ChatChannel.Key"/>.</param>
        /// <param name="chatChannelKey">The <see cref="Group.ChatChannelKey"/> for which to get the <see cref="ChatChannel.Key"/>.</param>
        /// <returns>The <see cref="ChatChannel.Key"/> or <see langword="null"/>.</returns>
        internal static string GetChatChannelKey( int groupId, string chatChannelKey )
        {
            if ( chatChannelKey.IsNotNullOrWhiteSpace() )
            {
                return chatChannelKey;
            }

            return $"{ChatChannelKeyPrefix}{groupId}";
        }

        /// <summary>
        /// Gets the <see cref="Group"/> identifier for the provided <see cref="ChatChannel.Key"/>.
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> for which to get the <see cref="Group"/> identifier.</param>
        /// <returns>The <see cref="Group"/> identifier or <see langword="null"/> if unable to parse.</returns>
        internal static int? GetGroupId( string chatChannelKey )
        {
            if ( chatChannelKey.IsNotNullOrWhiteSpace() )
            {
                return chatChannelKey.Replace( ChatChannelKeyPrefix, string.Empty ).AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Determines if a given <see cref="ChatChannel"/> originated in Rock, based on its <see cref="ChatChannel.Key"/>.
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> to check.</param>
        /// <returns>Whether the <see cref="ChatChannel"/> originated in Rock.</returns>
        internal static bool DidChatChannelOriginateInRock( string chatChannelKey )
        {
            return GetGroupId( chatChannelKey ).HasValue;
        }

        /// <summary>
        /// Gets the <see cref="ChatUser.Key"/> for the provided <see cref="PersonAlias"/> unique identifier.
        /// </summary>
        /// <param name="personAliasGuid">The <see cref="PersonAlias"/> unique identifier for which to get the <see cref="ChatUser.Key"/>.</param>
        /// <returns>The <see cref="ChatUser.Key"/>.</returns>
        internal static string GetChatUserKey( Guid personAliasGuid )
        {
            return $"{ChatUserKeyPrefix}{personAliasGuid}".ToLower();
        }

        /// <summary>
        /// Gets the <see cref="PersonAlias"/> unique identifier for the provided <see cref="ChatUser.Key"/>.
        /// </summary>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/> for which to get the <see cref="PersonAlias"/> unique identifier.</param>
        /// <returns>The <see cref="PersonAlias"/> unique identifier or <see langword="null"/> if unable to parse.</returns>
        internal static Guid? GetPersonAliasGuid( string chatUserKey )
        {
            if ( chatUserKey.IsNotNullOrWhiteSpace() )
            {
                return chatUserKey.Replace( ChatUserKeyPrefix, string.Empty ).AsGuidOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the runtime key for the <see cref="ChatChannelMember"/> (not saved in Rock or the external chat system).
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/>.</param>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/>.</param>
        /// <returns>
        /// "<paramref name="chatChannelKey"/>|<paramref name="chatUserKey"/>" or <see langword="null"/> if either argument is not provided.
        /// </returns>
        internal static string GetChatChannelMemberKey( string chatChannelKey, string chatUserKey )
        {
            if ( chatChannelKey.IsNullOrWhiteSpace() || chatUserKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return $"{chatChannelKey}|{chatUserKey}";
        }

        /// <summary>
        /// Gets the chat system user responsible for performing synchronization tasks between Rock and the external
        /// chat system.
        /// </summary>
        /// <returns>A <see cref="ChatUser"/> representing the chat system user.</returns>
        internal static ChatUser GetChatSystemUser()
        {
            return new ChatUser
            {
                Key = $"rock-chat-synchronizer-{ChatSystemUserGuid}".ToLower(),
                Name = "Rock Chat Synchronizer",
                IsAdmin = true
            };
        }

        /// <summary>
        /// Releases static <see cref="ChatHelper"/> values (e.g. <see cref="ChatConfiguration"/>) when updated values
        /// should be loaded into memory.
        /// </summary>
        /// <param name="shouldReinitializeChatProvider">Whether to also reinitialize the <see cref="IChatProvider"/>.</param>"/>
        internal void Reinitialize( bool shouldReinitializeChatProvider = true )
        {
            if ( !IsChatEnabled )
            {
                return;
            }

            lock ( _saveChatConfigurationLock )
            {
                _chatSystemUserGuid = GetOrCreateChatSystemUserGuid();
            }

            if ( shouldReinitializeChatProvider )
            {

                ChatProvider.Initialize();
            }
        }

        #endregion Configuration & Keys

        #region Caching

        /// <summary>
        /// Gets the cache key for the <see cref="Group"/> identifier, for the provided <see cref="ChatChannel.Key"/>.
        /// </summary>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> for which to get the cache key.</param>
        /// <returns>The cache key or <see langword="null"/>.</returns>
        internal static string GetChatChannelGroupIdCacheKey( string chatChannelKey )
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
        internal static void TryRemoveCachedChatChannel( int groupId )
        {
            var groupCache = GroupCache.Get( groupId );
            if ( groupCache != null )
            {
                var chatChannelKey = GetChatChannelKey( groupCache.Id, groupCache.ChatChannelKey );
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
        /// <param name="shouldCreate">
        /// Whether to create a new chat-specific <see cref="PersonAlias"/> if one doesn't already exist for this <see cref="Person"/>.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the <see cref="ChatUserAuthentication"/> or
        /// <see langword="null"/> if unable to find the <see cref="ChatUser"/> or get a token.
        /// </returns>
        internal async Task<ChatUserAuthentication> GetChatUserAuthenticationAsync( int personId, bool shouldCreate )
        {
            ChatUserAuthentication auth = null;

            if ( !IsChatEnabled || personId < 1 )
            {
                return auth;
            }

            var structuredLog = "PersonId: {PersonId} (ShouldCreate: {ShouldCreate})";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( GetChatUserAuthenticationAsync ).SplitCase()} failed for {structuredLog}";

            try
            {
                var syncCommand = new SyncPersonToChatCommand
                {
                    PersonId = personId,
                    ShouldEnsureChatAliasExists = shouldCreate
                };

                // The next method call does the following:
                //  1) Ensures this person has a chat-specific person alias in Rock IF instructed by `shouldCreate`;
                //  2) Ensures this person has a chat user in the external chat system IF a chat-specific person alias existed or was created.
                var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( new List<SyncPersonToChatCommand> { syncCommand } );
                if ( createOrUpdateChatUsersResult == null || createOrUpdateChatUsersResult.HasException )
                {
                    Logger.LogError( createOrUpdateChatUsersResult?.Exception, $"{logMessagePrefix} at step '{nameof( CreateOrUpdateChatUsersAsync ).SplitCase()}'.", personId, shouldCreate );
                    return auth;
                }

                var chatUserResult = createOrUpdateChatUsersResult
                    .UserResults
                    .FirstOrDefault( r => r?.ChatUserKey.IsNotNullOrWhiteSpace() == true );

                if ( chatUserResult == null )
                {
                    if ( shouldCreate )
                    {
                        // If the caller specified that a chat-specific person alias should be created if missing, yet
                        // we didn't get one back, log it.
                        Logger.LogError( $"{logMessagePrefix} at step '{nameof( CreateOrUpdateChatUsersAsync ).SplitCase()}'; no chat-specific Person Alias record was returned.", personId, shouldCreate );
                    }

                    return auth;
                }

                var tokenResult = await ChatProvider.GetChatUserTokenAsync( chatUserResult.ChatUserKey );
                if ( tokenResult?.Value.IsNotNullOrWhiteSpace() != true || tokenResult.HasException )
                {
                    Logger.LogError( tokenResult?.Exception, $"{logMessagePrefix} at step '{nameof( ChatProvider.GetChatUserTokenAsync ).SplitCase()}'; no token was returned from the Chat provider.", personId, shouldCreate );
                    return auth;
                }

                auth = new ChatUserAuthentication
                {
                    Token = tokenResult.Value,
                    ChatUserKey = chatUserResult.ChatUserKey
                };
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, $"{logMessagePrefix}.", personId, shouldCreate );
            }

            return auth;
        }

        #endregion Authentication

        #region Synchronization: From Rock To Chat Provider

        /// <summary>
        /// Ensures app-level roles, permission grants and other settings are in place within the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncSetupResult"/>.
        /// </returns>
        internal async Task<ChatSyncSetupResult> EnsureChatProviderAppIsSetUpAsync()
        {
            var result = new ChatSyncSetupResult();

            if ( !IsChatEnabled )
            {
                return result;
            }

            var logMessagePrefix = $"{LogMessagePrefix} {nameof( EnsureChatProviderAppIsSetUpAsync ).SplitCase()} failed";

            try
            {
                // Each setup step is dependent on the previous steps, so we'll run them in order and return early if any fail.
                void LogFailure( Exception ex, string stepName )
                {
                    result.Exception = ex;
                    Logger.LogError( ex, $"{logMessagePrefix} on step '{stepName.SplitCase()}'." );
                }

                var appSettingsResult = await ChatProvider.UpdateAppSettingsAsync();
                if ( appSettingsResult?.IsSetUp != true || appSettingsResult.HasException )
                {
                    LogFailure( appSettingsResult?.Exception, nameof( ChatProvider.UpdateAppSettingsAsync ) );
                    return result;
                }

                var appRolesResult = await ChatProvider.EnsureAppRolesExistAsync();
                if ( appRolesResult?.IsSetUp != true || appRolesResult.HasException )
                {
                    LogFailure( appRolesResult?.Exception, nameof( ChatProvider.EnsureAppRolesExistAsync ) );
                    return result;
                }

                var appGrantsResult = await ChatProvider.EnsureAppGrantsExistAsync( RockToChatSyncConfig );
                if ( appGrantsResult?.IsSetUp != true || appGrantsResult.HasException )
                {
                    LogFailure( appGrantsResult?.Exception, nameof( ChatProvider.EnsureAppGrantsExistAsync ) );
                    return result;
                }

                var systemUserResult = await ChatProvider.EnsureSystemUserExistsAsync();
                if ( systemUserResult?.IsSetUp != true || systemUserResult.HasException )
                {
                    LogFailure( systemUserResult?.Exception, nameof( ChatProvider.EnsureSystemUserExistsAsync ) );
                    return result;
                }

                // If we made it this far, setup was successful.
                result.IsSetUp = true;
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}." );
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
        internal async Task<ChatSyncCrudResult> SyncGroupTypesToChatProviderAsync( List<GroupType> groupTypes )
        {
            var result = new ChatSyncCrudResult();

            if ( !IsChatEnabled || groupTypes?.Any() != true )
            {
                return result;
            }

            var structuredLog = "GroupTypeIds: {@GroupTypeIds}";
            var groupTypeIds = groupTypes.Select( gt => gt.Id ).ToList();

            var logMessagePrefix = $"{LogMessagePrefix} {nameof( SyncGroupTypesToChatProviderAsync ).SplitCase()} failed for {structuredLog}";

            try
            {
                // Get all of the existing channel types.
                var getChatChannelTypesResult = await ChatProvider.GetAllChatChannelTypesAsync();
                if ( getChatChannelTypesResult == null || getChatChannelTypesResult.HasException )
                {
                    result.Exception = getChatChannelTypesResult?.Exception;
                    Logger.LogError( result.Exception, $"{logMessagePrefix} on step '{nameof( ChatProvider.GetAllChatChannelTypesAsync ).SplitCase()}'.", groupTypeIds );

                    return result;
                }

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
                    var existingChannelType = getChatChannelTypesResult
                        .ChatChannelTypes
                        .FirstOrDefault( ct => ct.Key == channelType.Key );

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
                                    ChatProvider.GetDefaultChannelTypeGrantsByRole()?.TryGetValue( role, out defaultRoleGrants );

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

                // Don't let individual CRUD failures cause all to fail.
                var crudExceptions = new List<Exception>();

                if ( channelTypesToDelete.Any() )
                {
                    var deletedResult = await ChatProvider.DeleteChatChannelTypesAsync( channelTypesToDelete );
                    deletedResult?.Deleted.ToList().ForEach( key => AddGroupTypeToResult( key, ChatSyncType.Delete ) );

                    if ( deletedResult?.HasException == true )
                    {
                        crudExceptions.Add( deletedResult.Exception );
                    }

                    // It's possible for channels to have been deleted as a part of this channel type deletion.
                    if ( deletedResult?.InnerResults?.Any() == true )
                    {
                        // Add the channel delete results to the outgoing result (for chat sync job logging).
                        result.InnerResults.AddRange( deletedResult.InnerResults );
                    }
                }

                if ( channelTypesToCreate.Any() )
                {
                    var createdResult = await ChatProvider.CreateChatChannelTypesAsync( channelTypesToCreate );
                    createdResult?.Created.ToList().ForEach( key => AddGroupTypeToResult( key, ChatSyncType.Create ) );

                    if ( createdResult?.HasException == true )
                    {
                        crudExceptions.Add( createdResult.Exception );
                    }
                }

                if ( channelTypesToUpdate.Any() )
                {
                    var updatedResult = await ChatProvider.UpdateChatChannelTypesAsync(
                        channelTypesToUpdate,
                        RockToChatSyncConfig
                    );

                    updatedResult?.Updated.ToList().ForEach( key => AddGroupTypeToResult( key, ChatSyncType.Update ) );

                    if ( updatedResult?.HasException == true )
                    {
                        crudExceptions.Add( updatedResult.Exception );
                    }
                }

                if ( crudExceptions.Any() )
                {
                    result.Exception = GetFirstOrAggregateException( crudExceptions );
                    Logger.LogError( result.Exception, $"{logMessagePrefix}.", groupTypeIds );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}.", groupTypeIds );
            }

            return result;
        }

        /// <summary>
        /// Synchronizes <see cref="Group"/>s from Rock to <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="Group"/>s to sync.</param>
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
        internal async Task<ChatSyncCrudResult> SyncGroupsToChatProviderAsync( List<SyncGroupToChatCommand> syncCommands, RockToChatGroupSyncConfig syncConfig = null )
        {
            var result = new ChatSyncCrudResult();

            // Validate commands.
            // Allow the "Chat Administrators" system group, but EXCLUDE sync commands for the other system chat groups.
            syncCommands = syncCommands
                ?.Where( c =>
                    c?.GroupTypeId > 0
                    && c.GroupId > 0
                    && c.GroupId != ChatBanListGroupId
                    && c.GroupId != ChatDirectMessagesGroupId
                    && c.GroupId != ChatSharedChannelsGroupId
                )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return result;
            }

            if ( syncConfig == null )
            {
                syncConfig = new RockToChatGroupSyncConfig();
            }

            var structuredLog = "SyncCommands: {@SyncCommands} (SyncConfig: {@SyncConfig})";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( SyncGroupsToChatProviderAsync ).SplitCase()} failed";

            try
            {
                // Get the Rock group data needed to create, update or delete channel members in the external chat system.
                var rockChatGroups = GetRockChatGroups( syncCommands );

                // Get the existing chat channels.
                var queryableKeys = rockChatGroups
                    .Select( g => ChatProvider.GetQueryableChatChannelKey( g ) )
                    .Where( k => k.IsNotNullOrWhiteSpace() )
                    .ToList();

                if ( !queryableKeys.Any() )
                {
                    // This will probably never happen, but if we have no queryable keys, exit early.
                    Logger.LogWarning( $"{logMessagePrefix} because no queryable Chat Channel keys were found. {structuredLog}", syncCommands, syncConfig );

                    return result;
                }

                var getChatChannelsResult = await ChatProvider.GetChatChannelsAsync( queryableKeys );
                if ( getChatChannelsResult == null || getChatChannelsResult.HasException )
                {
                    result.Exception = getChatChannelsResult?.Exception;
                    Logger.LogError( result.Exception, $"{logMessagePrefix} on step '{nameof( ChatProvider.GetChatChannelsAsync ).SplitCase()}'. {structuredLog}", syncCommands, syncConfig );

                    return result;
                }

                var channelsToCreate = new List<ChatChannel>();
                var channelsToUpdate = new List<ChatChannel>();
                var channelsToDelete = new List<string>();

                // Keep track of the channels that should and should NOT trigger a group member sync.
                var channelsToTriggerGroupMemberSync = new List<ChatChannel>();
                var queryableKeysToAvoidGroupMemberSync = new HashSet<string>();

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

                /*
                    3/18/2025 - JPH

                    Regarding De/Reactivating chat channels:

                    While the majority of this `ChatHelper` code is external chat provider-agnostic, the topic of
                    deactivating/reactivating (or disabling/re-enabling, as Stream calls it) should be carefully
                    considered if we move away from Stream.

                    With Stream, it's as simple as setting a `disabled` property on the channel and running it through
                    the standard update call. With other providers, it might not be this simple, and might require
                    additional API calls, at which point, we should enhance the `IChatProvider` contract accordingly.

                    Reason: Call out risk of switching to a different external chat provider.
                 */

                // A local function to determine if a channel has changed since last synced.
                bool HasChannelChanged( ChatChannel lastSyncedChannel, ChatChannel currentChannel )
                {
                    return lastSyncedChannel.Name != currentChannel.Name
                        || lastSyncedChannel.IsLeavingAllowed != currentChannel.IsLeavingAllowed
                        || lastSyncedChannel.IsPublic != currentChannel.IsPublic
                        || lastSyncedChannel.IsAlwaysShown != currentChannel.IsAlwaysShown
                        || lastSyncedChannel.IsActive != currentChannel.IsActive;
                }

                foreach ( var rockChatGroup in rockChatGroups )
                {
                    var channel = TryConvertToChatChannel( rockChatGroup );
                    if ( channel == null )
                    {
                        continue;
                    }

                    groupIdByQueryableKeys.Add( channel.QueryableKey, rockChatGroup.GroupId );

                    // Does it already exist in the external chat system?
                    var existingChannel = getChatChannelsResult
                        .ChatChannels
                        .FirstOrDefault( c => c.QueryableKey == channel.QueryableKey );

                    // Start by deleting any channels that no longer have a corresponding Rock group.
                    if ( rockChatGroup.ShouldDelete && existingChannel != null )
                    {
                        channelsToDelete.Add( channel.QueryableKey );
                        continue;
                    }

                    // Take note of inactive channels that should never trigger group member syncs.
                    if ( !channel.IsActive )
                    {
                        queryableKeysToAvoidGroupMemberSync.Add( channel.QueryableKey );
                    }

                    // For each chat-enabled group, add or update the channel in the external chat system.
                    // Note that a channel can be simultaneously chat-enabled AND inactive.
                    if ( rockChatGroup.IsChatEnabled )
                    {
                        if ( existingChannel != null )
                        {
                            if ( HasChannelChanged( existingChannel, channel ) )
                            {
                                channelsToUpdate.Add( channel );

                                // If this channel was previously inactive and is now being reactivated, trigger a group member sync.
                                if ( !existingChannel.IsActive && channel.IsActive )
                                {
                                    channelsToTriggerGroupMemberSync.Add( channel );
                                }
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
                        else if ( channel.IsActive )
                        {
                            // This active channel doesn't exist yet in the external chat system; create it.
                            channelsToCreate.Add( channel );
                        }
                    }
                    else if ( existingChannel != null )
                    {
                        // Handle non-chat-enabled groups that already exist in the external chat system.
                        // We'll inactivate these channels, so they can potentially be reactivated later.
                        if ( HasChannelChanged( existingChannel, channel ) )
                        {
                            channelsToUpdate.Add( channel );
                        }
                        else
                        {
                            // Add it to the results as an already-up-to-date group.
                            AddGroupToResult( channel.QueryableKey, ChatSyncType.Skip );
                        }
                    }

                    // Else, if we got here, this group did not already have a corresponding chat channel and is not chat-enabled.
                    // Since it wasn't added to one of the CRUD collections, it's been filtered out from the sync operations below.
                }

                // Don't let individual CRUD failures cause all to fail.
                var crudExceptions = new List<Exception>();

                if ( channelsToDelete.Any() )
                {
                    var deletedResult = await ChatProvider.DeleteChatChannelsAsync( channelsToDelete );
                    deletedResult?.Deleted.ToList().ForEach( key => AddGroupToResult( key, ChatSyncType.Delete ) );

                    if ( deletedResult?.HasException == true )
                    {
                        crudExceptions.Add( deletedResult.Exception );
                    }
                }

                if ( channelsToCreate.Any() )
                {
                    var createdResult = await ChatProvider.CreateChatChannelsAsync( channelsToCreate );

                    var createdKeys = createdResult?.Created.ToList();
                    if ( createdKeys?.Any() == true )
                    {
                        createdKeys.ForEach( key => AddGroupToResult( key, ChatSyncType.Create ) );
                        channelsToTriggerGroupMemberSync.AddRange(
                            channelsToCreate.Where( c => createdKeys.Contains( c.QueryableKey ) )
                        );
                    }

                    if ( createdResult?.HasException == true )
                    {
                        crudExceptions.Add( createdResult.Exception );
                    }
                }

                if ( channelsToUpdate.Any() )
                {
                    var updatedResult = await ChatProvider.UpdateChatChannelsAsync( channelsToUpdate );

                    var updatedKeys = updatedResult?.Updated.ToList();
                    if ( updatedKeys?.Any() == true )
                    {
                        updatedKeys.ForEach( key => AddGroupToResult( key, ChatSyncType.Update ) );

                        if ( syncConfig.ShouldSyncAllGroupMembers )
                        {
                            channelsToTriggerGroupMemberSync.AddRange(
                                channelsToUpdate.Where( c =>
                                    !queryableKeysToAvoidGroupMemberSync.Contains( c.QueryableKey )
                                    && !channelsToTriggerGroupMemberSync.Contains( c )
                                    && updatedKeys.Contains( c.QueryableKey )
                                )
                            );
                        }
                    }

                    if ( updatedResult?.HasException == true )
                    {
                        crudExceptions.Add( updatedResult.Exception );
                    }
                }

                if ( crudExceptions.Any() )
                {
                    result.Exception = GetFirstOrAggregateException( crudExceptions );
                    Logger.LogError( result.Exception, $"{logMessagePrefix}. {structuredLog}", syncCommands, syncConfig );
                }

                foreach ( var channel in channelsToTriggerGroupMemberSync )
                {
                    // Try to sync the group members.
                    if ( groupIdByQueryableKeys.TryGetValue( channel.QueryableKey, out var groupId ) )
                    {
                        var membersResult = await SyncGroupMembersToChatProviderAsync( groupId, syncConfig );

                        // If `SyncGroupMembersToChatProviderAsync` failed, a detailed error will have already been logged.
                        if ( membersResult != null )
                        {
                            // Add the group member sync results to the outgoing result (for chat sync job logging).
                            result.InnerResults.Add( membersResult );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}. {structuredLog}", syncCommands, syncConfig );
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
        internal async Task<ChatSyncCrudResult> SyncGroupMembersToChatProviderAsync( int groupId, RockToChatGroupSyncConfig groupSyncConfig = null )
        {
            if ( !IsChatEnabled || groupId <= 0 )
            {
                return new ChatSyncCrudResult();
            }

            // Get the sync commands for this group's members.
            var syncCommands = new GroupMemberService( RockContext )
                // In this case, we DO want to get deceased individuals and archived group members, as the sync process
                // knows how to handle them.
                .Queryable( includeDeceased: true, includeArchived: true )
                .Where( gm => gm.GroupId == groupId )
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
        internal async Task<ChatSyncCrudResult> SyncGroupMembersToChatProviderAsync( List<SyncGroupMemberToChatCommand> syncCommands, RockToChatGroupSyncConfig groupSyncConfig = null )
        {
            var result = new ChatSyncCrudResult();

            // Validate commands.
            // Allow the "Chat Administrators" and "Ban List" system groups (as they're handled below) but EXCLUDE sync
            // commands for the other system chat groups.
            syncCommands = syncCommands
                ?.Where( c =>
                    c?.GroupId > 0
                    && c.PersonId > 0
                    && c.GroupId != ChatDirectMessagesGroupId
                    && c.GroupId != ChatSharedChannelsGroupId
                )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return result;
            }

            if ( groupSyncConfig == null )
            {
                groupSyncConfig = new RockToChatGroupSyncConfig();
            }

            var structuredLog = "SyncCommands: {@SyncCommands} (GroupSyncConfig: {@GroupSyncConfig})";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( SyncGroupMembersToChatProviderAsync ).SplitCase()}";

            try
            {
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

                        // The next method call does the following:
                        //  1) Ensures this person has a chat-specific person alias in Rock;
                        //  2) Ensures this person has a chat user in the external chat system.
                        var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                        // If `createOrUpdateChatUsersResult` failed, a detailed error will have already been logged.
                        if ( createOrUpdateChatUsersResult != null )
                        {
                            // Add the created or updated chat users to the result (for chat sync job logging).
                            result.InnerResults.Add( createOrUpdateChatUsersResult );

                            // Determine which commands represent "bans" and which represent "unbans", by checking if
                            // each person represented by the sync commands currently exists in the group. We'll exclude
                            // deceased individuals altogether from these sync commands, as allowing them to be banned
                            // would be disrespectful. The sync job will ensure they're properly removed them from the
                            // external chat system the next time it runs.
                            var chatBanListMembers = GetRockChatGroupMembers( chatBanListCommands )
                                .Where( m => !m.IsDeceased )
                                .ToList();

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
                                var bannedResult = await ChatProvider.BanChatUsersAsync( banChatUserKeys );
                                if ( bannedResult != null )
                                {
                                    // Add the banned users to the result (for chat sync job logging).
                                    result.InnerResults.Add( bannedResult );
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
                                var unbannedResult = await ChatProvider.UnbanChatUsersAsync( unbanChatUserKeys );
                                if ( unbannedResult != null )
                                {
                                    // Add the unbanned users to the result (for chat sync job logging).
                                    result.InnerResults.Add( unbannedResult );
                                }
                            }
                        }

                        // Do we have any more sync commands to process?
                        if ( !syncCommands.Any() )
                        {
                            return result;
                        }
                    }
                }

                #endregion Chat Ban List

                #region APP - Chat Administrators

                // Next, we'll handle any members being added to or removed from the "APP - Chat Administrators" Rock
                // security role group. These members usually don't need to be added to a chat channel, but instead only
                // need to have their chat user records created or updated accordingly, within the external chat system.
                var chatAminsGroup = GroupCache.Get( ChatAdministratorsGroupId );

                // However.. someone COULD enable chat for the "Security Role" group type, in which case we'll bypass this
                // code block, and instead  perform a full (chat channel, down) sync of these members, which will effectively
                // add or remove them to/from the global `rock_admin` role in the external chat system.
                if ( chatAminsGroup?.GetIsChatEnabled() == false )
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

                        // The next method call does the following:
                        //  1) Ensures this person has a chat-specific person alias in Rock;
                        //  2) Ensures this person has a chat user in the external chat system.
                        var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                        // If `createOrUpdateChatUsersResult` failed, a detailed error will have already been logged.
                        if ( createOrUpdateChatUsersResult != null )
                        {
                            // Add the created or updated chat users to the result (for chat sync job logging).
                            result.InnerResults.Add( createOrUpdateChatUsersResult );
                        }

                        // Do we have any more sync commands to process?
                        if ( !syncCommands.Any() )
                        {
                            return result;
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

                    var chatChannelKey = GetChatChannelKey( groupCache.Id, groupCache.ChatChannelKey );

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
                var memberByChannelMemberKeys = new Dictionary<string, RockChatGroupMember>();

                void MapMemberToChannelMemberKey( string channelMemberKey, RockChatGroupMember member )
                {
                    if ( !memberByChannelMemberKeys.ContainsKey( channelMemberKey ) )
                    {
                        memberByChannelMemberKeys.Add( channelMemberKey, member );
                    }
                }

                void AddMemberToResult( string channelMemberKey, ChatSyncType chatSyncType )
                {
                    if ( memberByChannelMemberKeys.TryGetValue( channelMemberKey, out var member ) )
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

                // Get the Rock group member data needed to create, update or delete channel members in the external chat system.
                var rockChatGroupMembers = GetRockChatGroupMembers( syncCommands );

                #region Deceased Individuals to Delete

                PersonService personService = null;
                PersonAliasService personAliasService = null;

                // Take this opportunity to delete any deceased members who are being synced.
                var deceasedMembers = rockChatGroupMembers.Where( m => m.IsDeceased ).ToList();
                if ( deceasedMembers.Any() )
                {
                    await _deleteDeceasedIndividualsSemaphore.WaitAsync();
                    try
                    {
                        foreach ( var deceasedMember in deceasedMembers )
                        {
                            if ( personService == null )
                            {
                                personService = new PersonService( RockContext );
                                personAliasService = new PersonAliasService( RockContext );
                            }

                            // Remove this member from the outer collection so they aren't processed below.
                            rockChatGroupMembers.Remove( deceasedMember );

                            // Get this member's chat user key(s).
                            var keysToDelete = personService
                                .GetAllRockChatUserKeysQuery( deceasedMember.PersonId )
                                .AsEnumerable() // Materialize the query.
                                .Where( k =>
                                    k.ChatPersonAliasId.HasValue
                                    && k.ChatUserKey.IsNotNullOrWhiteSpace()
                                )
                                .ToList();

                            if ( keysToDelete?.Any() != true )
                            {
                                // It's possible we've already deleted this individual.
                                continue;
                            }

                            // This first call will remove the person from all chat-related groups in Rock.
                            // (We only need to use one of their keys - if they have multiple - to accomplish this).
                            var deleteCommands = new List<DeleteChatPersonInRockCommand> {
                                new DeleteChatPersonInRockCommand { ChatPersonKey = keysToDelete.First().ChatUserKey }
                            };

                            var config = new DeleteChatUsersInRockConfig
                            {
                                ShouldUnban = true,
                                ShouldClearChatPersonAliasForeignKey = false
                            };

                            DeleteChatUsersInRock( deleteCommands, config );

                            // This second call will delete the person's chat user(s) from the external chat system and
                            // clear the corresponding chat person alias names and foreign keys.
                            var deleteResult = await DeleteChatUsersAsync( personAliasService, keysToDelete );

                            // If `DeleteChatUsersAsync` failed, a detailed error will have already been logged.
                            if ( deleteResult != null )
                            {
                                // Add the deleted chat users to the result (for chat sync job logging).
                                result.InnerResults.Add( deleteResult );
                            }
                        }
                    }
                    finally
                    {
                        _deleteDeceasedIndividualsSemaphore.Release();
                    }

                    // Do we have any more members to process?
                    if ( !rockChatGroupMembers.Any() )
                    {
                        return result;
                    }
                }

                #endregion Deceased Individuals to Delete

                // Before moving on, let's remove any members who should be ignored (if their group is inactive or archived).
                rockChatGroupMembers = rockChatGroupMembers.Where( m => !m.ShouldIgnore ).ToList();

                // Do we have any members left to sync?
                if ( !rockChatGroupMembers.Any() )
                {
                    return result;
                }

                #region Channel Members to Delete

                // Handle commands to delete group members.
                var deleteMembers = rockChatGroupMembers
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

                    var rockChatUserKeys = ( personService ?? new PersonService( RockContext ) )
                        .GetActiveRockChatUserKeys( deletePersonIds );

                    // Just in case any people don't have a chat user key, let's at least log them.
                    // Subbing "user" for "person" here, as this can get logged in the UI.
                    var membersWithoutChatPersonKeys = deleteMembers
                        .Where( c => !rockChatUserKeys.Any( r => r.PersonId == c.PersonId ) )
                        .ToList();

                    if ( membersWithoutChatPersonKeys.Any() )
                    {
                        var deleteMembersStructuredLog = "{@MembersWithoutChatPersonKeys}";
                        Logger.LogWarning( $"{logMessagePrefix}: Unable to delete chat channel members in the external chat system, as matching chat person keys could not be found in Rock. {structuredLog} {deleteMembersStructuredLog}", syncCommands, groupSyncConfig, membersWithoutChatPersonKeys );
                    }

                    // Filter down to members that we're sure have keys.
                    deleteMembers = deleteMembers
                        .Except( membersWithoutChatPersonKeys )
                        .ToList();

                    // Organize members by channel.
                    foreach ( var member in deleteMembers )
                    {
                        var chatUserKey = rockChatUserKeys
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
                            MapMemberToChannelMemberKey( GetChatChannelMemberKey( syncChannelCommand.ChatChannelKey, chatUserKey ), member );
                        }
                    }
                }

                #endregion Channel Members to Delete

                #region Channel Members to Create or Update

                // Handle commands to create new or update existing group members. The chat sync job will ensure deceased
                // members are properly removed from the external chat system, so don't include them here.
                var createOrUpdateMembers = rockChatGroupMembers
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
                    //  2) Ensures this person has a chat user in the external chat system.
                    var createOrUpdateChatUsersResult = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                    // If `createOrUpdateChatUsersResult` failed, a detailed error will have already been logged.
                    if ( createOrUpdateChatUsersResult != null )
                    {
                        // Add the created or updated chat users to the result (for chat sync job logging).
                        result.InnerResults.Add( createOrUpdateChatUsersResult );

                        // Just in case any people still don't have a chat user key, let's at least log them. This will
                        // probably never happen. Subbing "user" for "person" here, as this can get logged in the UI.
                        var membersWithoutChatPersonKeys = createOrUpdateMembers
                            .Where( c => !createOrUpdateChatUsersResult.UserResults.Any( r => r.PersonId == c.PersonId ) )
                            .ToList();

                        if ( membersWithoutChatPersonKeys.Any() )
                        {
                            var createOrUpdateMembersStructuredLog = "{@MembersWithoutChatPersonKeys}";
                            Logger.LogWarning( $"{logMessagePrefix}: Unable to create or update chat channel members in the external chat system, as matching chat person keys could not be found in Rock. {structuredLog} {createOrUpdateMembersStructuredLog}", syncCommands, groupSyncConfig, membersWithoutChatPersonKeys );
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
                                var chatChannelMember = TryConvertToChatChannelMember(
                                    member,
                                    syncChannelCommand.ChatChannelTypeKey,
                                    syncChannelCommand.ChatChannelKey,
                                    chatUserKey
                                );

                                if ( chatChannelMember == null )
                                {
                                    continue;
                                }

                                syncChannelCommand.MembersToCreateOrUpdate.Add( chatChannelMember );
                                MapMemberToChannelMemberKey( chatChannelMember.Key, member );
                            }
                        }
                    }
                }

                #endregion Channel Members to Create or Update

                // Don't let individual channel failures cause all to fail.
                var perChannelExceptions = new List<Exception>();

                foreach ( var command in perChannelCommands )
                {
                    // Get the existing members.
                    var getChatChannelMembersResult = await ChatProvider.GetChatChannelMembersAsync(
                        command.ChatChannelTypeKey,
                        command.ChatChannelKey,
                        command.DistinctChatUserKeys
                    );

                    if ( getChatChannelMembersResult?.Exception is MemberChatChannelNotFoundException memberChatChannelNotFoundException )
                    {
                        // If this failure was due to the channel not existing in the external chat system, we'll try to
                        // create it and then re-attempt to sync all of its members.

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

                        GroupCache groupCache = null;
                        if ( shouldSyncGroup )
                        {
                            groupCache = GroupCache.Get( groupId.Value );
                        }

                        if ( groupCache != null )
                        {
                            if ( groupSyncConfig.AlreadySyncedGroupIds == null )
                            {
                                groupSyncConfig.AlreadySyncedGroupIds = new HashSet<int> { groupCache.Id };
                            }

                            var groupSyncCommands = new List<SyncGroupToChatCommand>
                            {
                                new SyncGroupToChatCommand
                                {
                                    GroupTypeId = groupCache.GroupTypeId,
                                    GroupId = groupCache.Id,
                                    ChatChannelKey = groupCache.ChatChannelKey
                                }
                            };

                            // Enable syncing of group members for skipped & updated groups, just in case another
                            // process beats this one to creating the missing channel.
                            groupSyncConfig.ShouldSyncAllGroupMembers = true;

                            var innerGroupSyncResult = await SyncGroupsToChatProviderAsync(
                                groupSyncCommands,
                                groupSyncConfig
                            );

                            if ( innerGroupSyncResult != null )
                            {
                                // Relay any members that were synced to the outer result instance.
                                result.Created.UnionWith( innerGroupSyncResult.Created );
                                result.Updated.UnionWith( innerGroupSyncResult.Updated );
                                result.Deleted.UnionWith( innerGroupSyncResult.Deleted );

                                if ( innerGroupSyncResult.HasException )
                                {
                                    perChannelExceptions.Add( innerGroupSyncResult.Exception );
                                }
                            }
                        }

                        continue;
                    }
                    else if ( getChatChannelMembersResult == null || getChatChannelMembersResult.HasException )
                    {
                        perChannelExceptions.Add(
                            new ChatSyncException(
                                $"{logMessagePrefix} failed on step '{nameof( ChatProvider.GetChatChannelMembersAsync ).SplitCase()}' for Chat Channel with key '{command.ChatChannelKey}'.",
                                getChatChannelMembersResult?.Exception
                            )
                        );

                        continue;
                    }

                    var existingMembers = getChatChannelMembersResult.ChatChannelMembers;

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
                                AddMemberToResult( existingMember.Key, ChatSyncType.Skip );
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
                        // Do they exist in the external chat system?
                        if ( existingMembers?.Any( m => m.ChatUserKey == chatUserKey ) == true )
                        {
                            membersToDelete.Add( chatUserKey );
                        }
                    }

                    // Don't let individual CRUD failures cause all to fail.
                    var crudExceptions = new List<Exception>();

                    if ( membersToDelete.Any() )
                    {
                        var deletedResult = await ChatProvider.DeleteChatChannelMembersAsync(
                            command.ChatChannelTypeKey,
                            command.ChatChannelKey,
                            membersToDelete
                        );

                        deletedResult?.Deleted.ToList().ForEach( key => AddMemberToResult( key, ChatSyncType.Delete ) );

                        if ( deletedResult?.HasException == true )
                        {
                            crudExceptions.Add( deletedResult.Exception );
                        }
                    }

                    if ( membersToCreate.Any() )
                    {
                        var createdResult = await ChatProvider.CreateChatChannelMembersAsync(
                            command.ChatChannelTypeKey,
                            command.ChatChannelKey,
                            membersToCreate
                        );

                        createdResult?.Created.ToList().ForEach( key => AddMemberToResult( key, ChatSyncType.Create ) );

                        if ( createdResult?.HasException == true )
                        {
                            crudExceptions.Add( createdResult.Exception );
                        }
                    }

                    if ( membersToUpdate.Any() )
                    {
                        var updatedResult = await ChatProvider.UpdateChatChannelMembersAsync(
                            command.ChatChannelTypeKey,
                            command.ChatChannelKey,
                            membersToUpdate
                        );

                        updatedResult?.Updated.ToList().ForEach( key => AddMemberToResult( key, ChatSyncType.Update ) );

                        if ( updatedResult?.HasException == true )
                        {
                            crudExceptions.Add( updatedResult.Exception );
                        }
                    }

                    if ( crudExceptions.Any() )
                    {
                        perChannelExceptions.Add( GetFirstOrAggregateException( crudExceptions ) );
                    }
                }

                if ( perChannelExceptions.Any() )
                {
                    result.Exception = GetFirstOrAggregateException( perChannelExceptions );
                    Logger.LogError( result.Exception, $"{logMessagePrefix} failed. {structuredLog}", syncCommands, groupSyncConfig );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix} failed. {structuredLog}", syncCommands, groupSyncConfig );
            }

            return result;
        }

        /// <summary>
        /// Creates new or updates existing <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="ChatUser"/>s to create or update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCreateOrUpdateUsersResult"/>.
        /// </returns>
        internal async Task<ChatSyncCreateOrUpdateUsersResult> CreateOrUpdateChatUsersAsync( List<SyncPersonToChatCommand> syncCommands )
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

            var structuredLog = "SyncCommands: {@SyncCommands}";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( CreateOrUpdateChatUsersAsync ).SplitCase()} failed";

            try
            {
                var chatConfiguration = GetChatConfiguration();

                // Ensure each person has a chat-specific person alias record in Rock IF instructed by their respective sync commands.
                var rockChatUserPeople = GetOrCreateRockChatUserPeople( syncCommands, chatConfiguration );

                // Do we have any people to sync all the way to the chat provider?
                if ( !rockChatUserPeople.Any() )
                {
                    return result;
                }

                // Get the existing chat users.
                var chatUserKeys = rockChatUserPeople
                    .Where( p => p.ChatPersonAliasGuid.HasValue )
                    .Select( p => p.ChatUserKey )
                    .ToList();

                // Get the existing users.
                var getChatUsersResult = await ChatProvider.GetChatUsersAsync( chatUserKeys );
                if ( getChatUsersResult == null || getChatUsersResult.HasException )
                {
                    result.Exception = getChatUsersResult?.Exception;
                    Logger.LogError( result.Exception, $"{logMessagePrefix} on step '{nameof( ChatProvider.GetChatUsersAsync ).SplitCase()}'. {structuredLog}", syncCommands );

                    return result;
                }

                var usersToCreate = new List<ChatUser>();
                var usersToUpdate = new List<ChatUser>();

                // A local function to add users to the outgoing results collection only AFTER we know they've been
                // successfully created or updated within the external chat system.
                void AddChatUserToResults( string chatUserKey, ChatSyncType chatSyncType )
                {
                    var chatUserPerson = rockChatUserPeople.FirstOrDefault( p => p.ChatUserKey == chatUserKey );
                    if ( chatUserPerson == null )
                    {
                        // Should never happen.
                        return;
                    }

                    result.UserResults.Add(
                        new ChatSyncCreateOrUpdateUserResult
                        {
                            ChatUserKey = chatUserKey,
                            PersonId = chatUserPerson.PersonId,
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
                    var existingUser = getChatUsersResult
                        .ChatUsers
                        .FirstOrDefault( u => u.Key == chatUser.Key );

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
                            AddChatUserToResults( chatUser.Key, ChatSyncType.Skip );
                        }
                    }
                    else
                    {
                        // The user doesn't exist yet in the external chat system; create them.
                        usersToCreate.Add( chatUser );
                    }
                }

                // Don't let individual CRUD failures cause all to fail.
                var crudExceptions = new List<Exception>();

                if ( usersToCreate.Any() )
                {
                    var createdResult = await ChatProvider.CreateChatUsersAsync( usersToCreate );
                    createdResult?.Created.ToList().ForEach( key => AddChatUserToResults( key, ChatSyncType.Create ) );

                    if ( createdResult?.HasException == true )
                    {
                        crudExceptions.Add( createdResult.Exception );
                    }
                }

                if ( usersToUpdate.Any() )
                {
                    var updatedResult = await ChatProvider.UpdateChatUsersAsync( usersToUpdate );
                    updatedResult?.Updated.ToList().ForEach( key => AddChatUserToResults( key, ChatSyncType.Update ) );

                    if ( updatedResult?.HasException == true )
                    {
                        crudExceptions.Add( updatedResult.Exception );
                    }
                }

                if ( crudExceptions.Any() )
                {
                    result.Exception = GetFirstOrAggregateException( crudExceptions );
                    Logger.LogError( result.Exception, $"{logMessagePrefix}. {structuredLog}", syncCommands );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}. {structuredLog}", syncCommands );
            }

            return result;
        }

        /// <summary>
        /// Deletes non-prevailing, merged <see cref="ChatUser"/>s from the external chat system.
        /// </summary>
        /// <param name="personId">
        /// The optional <see cref="Person"/> identifier to check for merged <see cref="ChatUser"/>s to delete.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If <paramref name="personId"/> is not provided, ALL chat-enabled Rock <see cref="Person"/>s will be checked
        /// for merged <see cref="ChatUser"/>s to delete.
        /// </para>
        /// <para>
        /// For <see cref="ChatUser"/>s who are successfully deleted in the external chat system, their corresponding,
        /// chat-specific <see cref="PersonAlias"/> records will also be cleared.
        /// </para>
        /// </remarks>
        internal async Task<ChatSyncCrudResult> DeleteMergedChatUsersAsync( int? personId = null )
        {
            var result = new ChatSyncCrudResult();

            if ( !IsChatEnabled )
            {
                return result;
            }

            var structuredLog = "PersonId: {PersonId}";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( DeleteMergedChatUsersAsync ).SplitCase()} failed";

            try
            {
                var peopleWithMultipleChatUserKeys = new PersonService( RockContext )
                        .GetPeopleWithMultipleChatUserKeys( personId );

                if ( peopleWithMultipleChatUserKeys?.Any() != true )
                {
                    return result;
                }

                // Don't let individual exceptions cause all to fail.
                var perPersonExceptions = new List<Exception>();

                // This will be used to clear out the chat user[foreign] keys for the chat users who are deleted.
                var personAliasService = new PersonAliasService( RockContext );

                foreach ( var personKvp in peopleWithMultipleChatUserKeys )
                {
                    var rockChatUserKeys = personKvp.Value
                        ?.Where( k =>
                            k.ChatPersonAliasId.HasValue
                            && k.ChatUserKey.IsNotNullOrWhiteSpace()
                        )
                        .OrderBy( k => k.ChatPersonAliasId ) // Keep the earliest chat person alias.
                        .ToList();

                    var keyToKeep = rockChatUserKeys?.FirstOrDefault();
                    if ( keyToKeep == null )
                    {
                        continue;
                    }

                    var keysToDelete = rockChatUserKeys.Skip( 1 ).ToList();
                    if ( !keysToDelete.Any() )
                    {
                        continue;
                    }

                    var deleteResult = await DeleteChatUsersAsync( personAliasService, keysToDelete, keyToKeep );

                    // If `DeleteChatUsersAsync` failed, a detailed error will have already been logged.
                    // But we still want to add an exception to the outgoing result (for chat sync job logging).
                    if ( deleteResult == null || deleteResult?.HasException == true )
                    {
                        perPersonExceptions.Add(
                            new ChatSyncException(
                                $"{logMessagePrefix} on step '{nameof( DeleteChatUsersAsync ).SplitCase()}' for Person Alias ID {keyToKeep.ChatPersonAliasId}.",
                                deleteResult?.Exception
                            )
                        );

                        continue;
                    }

                    result.Deleted.UnionWith( deleteResult.Deleted );
                    result.Skipped.UnionWith( deleteResult.Skipped );
                }

                if ( perPersonExceptions.Any() )
                {
                    result.Exception = GetFirstOrAggregateException( perPersonExceptions );
                    Logger.LogError( result.Exception, $"{logMessagePrefix}. {structuredLog}", personId );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}. {structuredLog}", personId );
            }

            return result;
        }

        /// <summary>
        /// Deletes all <see cref="ChatUser"/>s for the specified <see cref="Person"/> in the external chat system.
        /// </summary>
        /// <param name="personId">The <see cref="Person"/> identifier for whom to delete all <see cref="ChatUser"/>s.</param>
        /// <param name="personAliasService">
        /// The optional <see cref="PersonAliasService"/> to use for clearing name and foreign key values.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// For <see cref="ChatUser"/>s who are successfully deleted in the external chat system, their corresponding,
        /// chat-specific <see cref="PersonAlias"/> records will also be cleared.
        /// </remarks>
        internal async Task<ChatSyncCrudResult> DeleteChatUsersAsync( int personId, PersonAliasService personAliasService = null )
        {
            var result = new ChatSyncCrudResult();

            if ( !IsChatEnabled || personId <= 0 )
            {
                return result;
            }

            var structuredLog = "PersonId: {PersonId}";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( DeleteChatUsersAsync ).SplitCase()} failed";

            try
            {
                var keysToDelete = new PersonService( RockContext )
                    .GetAllRockChatUserKeysQuery( personId )
                    .AsEnumerable() // Materialize the query.
                    .Where( k =>
                        k.ChatPersonAliasId.HasValue
                        && k.ChatUserKey.IsNotNullOrWhiteSpace()
                    )
                    .ToList();

                if ( keysToDelete?.Any() != true )
                {
                    return result;
                }

                var deleteResult = await DeleteChatUsersAsync(
                    personAliasService ?? new PersonAliasService( RockContext ),
                    keysToDelete
                );

                // If `DeleteChatUsersAsync` failed, a detailed error will have already been logged.
                // But we still want to add an exception to the outgoing result (for chat sync job logging).
                if ( deleteResult == null || deleteResult?.HasException == true )
                {
                    result.Exception = new ChatSyncException(
                        $"{logMessagePrefix} on step '{nameof( DeleteChatUsersAsync ).SplitCase()}' for Person ID {personId}.",
                        deleteResult?.Exception
                    );

                    return result;
                }

                result.Deleted.UnionWith( deleteResult.Deleted );
                result.Skipped.UnionWith( deleteResult.Skipped );
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}. {structuredLog}", personId );
            }

            return result;
        }

        #endregion Synchronization: From Rock To Chat Provider

        #region Synchronization: From Chat Provider To Rock

        /// <inheritdoc cref="IChatProvider.ValidateWebhookRequestAsync(HttpRequestMessage)"/>
        internal async Task<WebhookValidationResult> ValidateWebhookRequestAsync( HttpRequestMessage request )
        {
            if ( !IsChatEnabled )
            {
                return new WebhookValidationResult( null );
            }

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
                var structuredLog = "RequestBody: {RequestBody}";
                var logMessagePrefix = $"{LogMessagePrefix} {nameof( ValidateWebhookRequestAsync ).SplitCase()} failed.";

                var requestBody = result?.RequestBody ?? string.Empty;

                if ( result?.Exception is InvalidChatWebhookRequestException validationEx )
                {
                    // Prefer a structured log without a stack trace.
                    Logger.LogError( $"{logMessagePrefix} {validationEx.Message} {structuredLog}", requestBody );
                }
                else
                {
                    // Fall back to logging unexpected exceptions.
                    Logger.LogError( result?.Exception, $"{logMessagePrefix} Webhook request is invalid. {structuredLog}", requestBody );
                }
            }

            return result;
        }

        /// <summary>
        /// Handles chat webhook requests by synchronizing data from the external chat system to Rock.
        /// </summary>
        /// <param name="webhookRequests">The list of webhook requests to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task HandleChatWebhookRequestsAsync( List<ChatWebhookRequest> webhookRequests )
        {
            if ( !IsChatEnabled || webhookRequests?.Any() != true )
            {
                return;
            }

            var logMessage = $"{LogMessagePrefix} {nameof( HandleChatWebhookRequestsAsync ).SplitCase()} failed.";

            try
            {
                var result = ChatProvider.GetChatToRockSyncCommands( webhookRequests );
                if ( result == null || result?.HasException == true )
                {
                    Logger.LogError( result?.Exception, logMessage );
                }

                if ( result?.SyncCommands?.Any() == true )
                {
                    await SyncFromChatToRockAsync( result.SyncCommands );
                }
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, logMessage );
            }
        }

        /// <summary>
        /// Gets all chat channels from the external chat system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelsResult"/>.
        /// </returns>
        internal async Task<GetChatChannelsResult> GetAllChatChannelsAsync()
        {
            var result = new GetChatChannelsResult();

            if ( !IsChatEnabled )
            {
                return result;
            }

            var logMessage = $"{LogMessagePrefix} {nameof( GetAllChatChannelsAsync ).SplitCase()} failed.";

            try
            {
                var chatProviderResult = await ChatProvider.GetAllChatChannelsAsync();
                if ( chatProviderResult == null || chatProviderResult.HasException )
                {
                    result.Exception = chatProviderResult?.Exception;
                    Logger.LogError( result.Exception, logMessage );
                }
                else
                {
                    result = chatProviderResult;
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, logMessage );
            }

            return result;
        }

        /// <summary>
        /// Gets a list of <see cref="ChatChannelMember"/>s from the external chat system that match the provided
        /// <see cref="ChatChannelType"/> and <see cref="ChatChannel"/> key combination.
        /// </summary>
        /// <param name="chatChannelTypeKey">The key of the <see cref="ChatChannelType"/> for the members to get.</param>
        /// <param name="chatChannelKey">The key of the <see cref="ChatChannel"/> for the members to get.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="GetChatChannelMembersResult"/>.
        /// </returns>
        internal async Task<GetChatChannelMembersResult> GetChatChannelMembersAsync( string chatChannelTypeKey, string chatChannelKey )
        {
            var result = new GetChatChannelMembersResult
            {
                ChatChannelTypeKey = chatChannelTypeKey,
                ChatChannelKey = chatChannelKey
            };

            if ( !IsChatEnabled || chatChannelTypeKey.IsNullOrWhiteSpace() || chatChannelKey.IsNullOrWhiteSpace() )
            {
                return result;
            }

            var structuredLog = "ChatChannelTypeKey: {ChatChannelTypeKey}, ChatChannelKey: {ChatChannelKey}";
            var logMessage = $"{LogMessagePrefix} {nameof( GetChatChannelMembersAsync ).SplitCase()} failed for {structuredLog}.";

            try
            {
                var chatProviderResult = await ChatProvider.GetAllChatChannelMembersAsync( chatChannelTypeKey, chatChannelKey );
                if ( chatProviderResult == null || chatProviderResult.HasException )
                {
                    result.Exception = chatProviderResult?.Exception;
                    Logger.LogError( result.Exception, logMessage, chatChannelTypeKey, chatChannelKey );
                }
                else
                {
                    result = chatProviderResult;
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, logMessage, chatChannelTypeKey, chatChannelKey );
            }

            return result;
        }

        /// <summary>
        /// Deletes any Rock chat users who no longer exist in the external chat system.
        /// </summary>
        /// <returns>
        /// An asynchronous task representing the operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        internal async Task<ChatSyncCrudResult> DeleteRockChatUsersMissingFromChatProviderAsync()
        {
            var result = new ChatSyncCrudResult();

            if ( !IsChatEnabled )
            {
                return result;
            }

            var logMessagePrefix = $"{LogMessagePrefix} {nameof( DeleteRockChatUsersMissingFromChatProviderAsync ).SplitCase()} failed";

            try
            {
                // Get all of the existing external chat user keys.
                var getChatUserKeysResult = await ChatProvider.GetAllChatUserKeysAsync();
                if ( getChatUserKeysResult == null || getChatUserKeysResult.HasException )
                {
                    result.Exception = getChatUserKeysResult?.Exception;
                    Logger.LogError( result.Exception, $"{logMessagePrefix} on step '{nameof( ChatProvider.GetAllChatUserKeysAsync ).SplitCase()}'." );

                    return result;
                }

                var externalChatUserKeys = getChatUserKeysResult.Keys;

                // Get all of the existing Rock chat user keys
                var rockChatUserKeys = new PersonAliasService( RockContext )
                    .GetAllChatUserKeysQuery()
                    .ToHashSet();

                // Get those that exist in Rock but don't exist externally.
                var keysToDelete = rockChatUserKeys.Except( externalChatUserKeys ).ToList();
                if ( !keysToDelete.Any() )
                {
                    return result;
                }

                // Create a sync command for each.
                var syncCommands = keysToDelete
                    .Select( key => new DeleteChatPersonInRockCommand { ChatPersonKey = key } )
                    .ToList();

                var deleteResult = DeleteChatUsersInRock( syncCommands );
                result.Deleted.UnionWith( deleteResult.Deleted );
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}." );
            }

            return result;
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
        internal async Task SyncFromChatToRockAsync( List<ChatToRockSyncCommand> syncCommands )
        {
            syncCommands = syncCommands
                ?.Where( c => c?.ShouldRetry == true )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
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
                Logger.LogError( ex, $"{LogMessagePrefix} {nameof( SyncFromChatToRockAsync ).SplitCase()} failed." );
            }
            finally
            {
                LogChatToRockSyncCommandOutcomes( syncCommands );
                await RequeueRecoverableChatToRockSyncCommandsAsync( syncCommands );
            }
        }

        #endregion Synchronization: From Chat Provider To Rock

        #region Interactions

        /// <summary>
        /// Gets the message counts for each <see cref="ChatUser"/> within each <see cref="ChatChannel"/>, for the specified date.
        /// </summary>
        /// <param name="messageDate">The date for which to get message counts.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatUserMessageCountsByChatChannelResult"/>.
        /// </returns>
        internal async Task<ChatUserMessageCountsByChatChannelResult> GetChatUserMessageCountsByChatChannelKeyAsync( DateTime messageDate )
        {
            var result = new ChatUserMessageCountsByChatChannelResult { MessageDate = messageDate };

            if ( !IsChatEnabled )
            {
                return result;
            }

            var structuredLog = "MessageDate: {MessageDate}";
            var logMessage = $"{LogMessagePrefix} {nameof( GetChatUserMessageCountsByChatChannelKeyAsync ).SplitCase()} failed for {structuredLog}.";

            try
            {
                var chatProviderResult = await ChatProvider.GetChatUserMessageCountsByChatChannelKeyAsync( messageDate );
                if ( chatProviderResult == null || chatProviderResult.HasException )
                {
                    result.Exception = chatProviderResult?.Exception;
                    Logger.LogError( result.Exception, logMessage, messageDate );
                }
                else
                {
                    result = chatProviderResult;
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, logMessage, messageDate );
            }

            return result;
        }

        #endregion Interactions

        #endregion Internal Methods

        #region Private Methods

        #region Error Handling

        /// <summary>
        /// Gets the first exception in the list, or an <see cref="AggregateException"/> if there are multiple exceptions.
        /// </summary>
        /// <param name="exceptions">The list of exceptions.</param>
        /// <param name="aggregateMessage">The optional message to include in the <see cref="AggregateException"/>.</param>
        internal static Exception GetFirstOrAggregateException( List<Exception> exceptions, string aggregateMessage = null )
        {
            return exceptions.Count == 1
                ? exceptions.First()
                : aggregateMessage.IsNotNullOrWhiteSpace()
                    ? new AggregateException( aggregateMessage, exceptions )
                    : new AggregateException( exceptions );
        }

        #endregion Error Handling

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
            // Note that we never want to include deceased individuals here.
            var personQry = new PersonService( RockContext ).Queryable();
            var chatAliasQry = new PersonAliasService( RockContext ).GetChatPersonAliasesQuery();

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
                    ChatPersonAliasGuid = p.ChatAliases?.Any() == true
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
                if ( rockChatUserPerson.ChatPersonAliasGuid.HasValue )
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
                    Name = ChatPersonAliasName,
                    PersonId = rockChatUserPerson.PersonId,
                    Guid = guid,
                    ForeignKey = GetChatUserKey( guid )
                };

                newChatAliasesToSave.Add( chatAlias );
                rockChatUserPerson.ChatPersonAliasGuid = guid;
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

            rockChatUserPeople.ForEach( p =>
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
        /// Gets <see cref="RockChatGroup"/>s for the provided <see cref="SyncGroupToChatCommand"/>s.
        /// </summary>
        /// <param name="syncCommands">The list of <see cref="SyncGroupToChatCommand"/>s for which to get
        /// <see cref="RockChatGroup"/>s.</param>
        /// <returns>
        /// A list of <see cref="RockChatGroup"/>s with one entry for each <see cref="SyncGroupToChatCommand"/>.
        /// </returns>
        private List<RockChatGroup> GetRockChatGroups( List<SyncGroupToChatCommand> syncCommands )
        {
            // We always want to include archived groups, as they'll be considered inactive chat channels.
            var groupQry = new GroupService( RockContext ).AsNoFilter();

            // Get the distinct group IDs represented within the commands.
            var groupIds = syncCommands.Select( c => c.GroupId ).Distinct().ToList();
            if ( groupIds.Count == 1 )
            {
                // Most performant: limit queries to just this group.
                var firstGroupId = groupIds.First();
                groupQry = groupQry.Where( g => g.Id == firstGroupId );
            }
            else if ( groupIds.Count < 1000 )
            {
                // For fewer than 1k groups, allow a SQL `WHERE...IN` clause.
                groupQry = groupQry.Where( g => groupIds.Contains( g.Id ) );
            }
            else
            {
                // For 1k or more groups, create and join to an entity set.
                var entitySetOptions = new AddEntitySetActionOptions
                {
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatGroups )}",
                    EntityTypeId = EntityTypeCache.Get<Group>().Id,
                    EntityIdList = groupIds,
                    ExpiryInMinutes = 20
                };

                var entitySetService = new EntitySetService( RockContext );
                var entitySetId = entitySetService.AddEntitySet( entitySetOptions );
                var entitySetItemQry = entitySetService.GetEntityQuery( entitySetId ).Select( e => e.Id );

                groupQry = groupQry.Where( g => entitySetItemQry.Contains( g.Id ) );
            }

            // Get all groups matching the provided commands.
            var dbRockChatGroups = groupQry
                .AsEnumerable() // Materialize the query.
                .Select( g =>
                    new RockChatGroup
                    {
                        GroupTypeId = g.GroupTypeId,
                        GroupId = g.Id,
                        ChatChannelTypeKey = GetChatChannelTypeKey( g.GroupTypeId ),
                        ChatChannelKey = GetChatChannelKey( g.Id, g.ChatChannelKey ),
                        Name = g.Name,
                        IsLeavingAllowed = g.GetIsLeavingChatChannelAllowed(),
                        IsPublic = g.GetIsChatChannelPublic(),
                        IsAlwaysShown = g.GetIsChatChannelAlwaysShown(),
                        IsChatEnabled = g.GetIsChatEnabled(),
                        IsChatChannelActive = g.GetIsChatChannelActive()
                    }
                )
                .ToList();

            var rockChatGroups = new List<RockChatGroup>();

            // Loop through the commands and create a rock chat group instance for each, regardless of whether we found
            // it in the Rock database.
            foreach ( var command in syncCommands )
            {
                // Create and add the outgoing group to the collection with the data we already know from the command;
                // we'll refine it below.
                var rockChatGroup = new RockChatGroup
                {
                    GroupTypeId = command.GroupTypeId,
                    GroupId = command.GroupId,
                    ChatChannelTypeKey = GetChatChannelTypeKey( command.GroupTypeId ),
                    ChatChannelKey = GetChatChannelKey( command.GroupId, command.ChatChannelKey )
                };

                rockChatGroups.Add( rockChatGroup );

                // Did we find this group in the database?
                var dbRockChatGroup = dbRockChatGroups.FirstOrDefault( g => g.GroupId == command.GroupId );
                if ( dbRockChatGroup == null )
                {
                    // Since we didn't find this group in the database, mark it for deletion.
                    rockChatGroup.ShouldDelete = true;
                    continue;
                }

                // Copy the database values to the outgoing group.
                rockChatGroup.ChatChannelTypeKey = dbRockChatGroup.ChatChannelTypeKey;
                rockChatGroup.ChatChannelKey = dbRockChatGroup.ChatChannelKey;
                rockChatGroup.Name = dbRockChatGroup.Name;
                rockChatGroup.IsLeavingAllowed = dbRockChatGroup.IsLeavingAllowed;
                rockChatGroup.IsPublic = dbRockChatGroup.IsPublic;
                rockChatGroup.IsAlwaysShown = dbRockChatGroup.IsAlwaysShown;
                rockChatGroup.IsChatEnabled = dbRockChatGroup.IsChatEnabled;
                rockChatGroup.IsChatChannelActive = dbRockChatGroup.IsChatChannelActive;
            }

            return rockChatGroups;
        }

        /// <summary>
        /// Gets <see cref="RockChatGroupMember"/>s for the provided <see cref="SyncGroupMemberToChatCommand"/>s.
        /// </summary>
        /// <param name="syncCommands">The list of <see cref="SyncGroupMemberToChatCommand"/>s for which to get
        /// <see cref="RockChatGroupMember"/>s.</param>
        /// <returns>
        /// A list of <see cref="RockChatGroupMember"/>s with one entry for each Rock <see cref="Group"/> and
        /// <see cref="Person"/> combination.
        /// </returns>
        private List<RockChatGroupMember> GetRockChatGroupMembers( List<SyncGroupMemberToChatCommand> syncCommands )
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
                       prefer the first active, non-archived record (but still allow an inactive or archived, banned
                       record if there are no active ones), further sorted as follows:
                            `GroupRole.Order` (ascending)
                            `GroupRole.IsLeader` (descending)
                            `GroupRole.Id` (ascending)
                    3. If the selected group member record for a given person matches both of the following:
                            `GroupMemberStatus != GroupMemberStatus.Active || IsArchived == true`
                            `IsChatBanned == true`
                       Then we SHOULD NOT remove the member from the external chat system, so it can know that the
                       member has already been banned from the channel.

                Reason: Always return the "correct" group member for a given group & person combination.
             */

            // In this case, we DO want to get deceased individuals and archived group members. This method will decide
            // how to handle archived group members, and the callers of this method know how to handle deceased individuals.
            var groupMemberQry = new GroupMemberService( RockContext )
                .Queryable(
                    includeDeceased: true,
                    includeArchived: true
                );

            // Get the distinct group IDs represented within the commands.
            var groupIds = syncCommands.Select( c => c.GroupId ).Distinct().ToList();
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
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatGroupMembers )}_Groups",
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
            var personIds = syncCommands.Select( c => c.PersonId ).Distinct().ToList();
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
                    Name = $"{nameof( ChatHelper )}_{nameof( GetRockChatGroupMembers )}_People",
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
                    gm.IsChatBanned,
                    gm.Person.IsDeceased,
                    gm.IsArchived,
                    IsGroupActive = gm.Group.IsActive,
                    IsGroupArchived = gm.Group.IsArchived
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

            var rockChatGroupMembers = new List<RockChatGroupMember>();

            // Because the list of commands could represent a given group & person combination more than once, let's
            // keep track of those we've already seen to speed up this method's return.
            var alreadySeenCombinations = new HashSet<string>();

            // Loop through the commands and create a rock chat group member instance for each, regardless of whether we
            // found them in the Rock database.
            foreach ( var command in syncCommands )
            {
                var groupAndPersonKey = $"{command.GroupId}|{command.PersonId}";
                if ( !alreadySeenCombinations.Add( groupAndPersonKey ) )
                {
                    continue;
                }

                // Create and add the outgoing member to the collection with the data we already know from the command;
                // we'll refine them below.
                var rockChatChannelMember = new RockChatGroupMember
                {
                    GroupId = command.GroupId,
                    PersonId = command.PersonId
                };

                rockChatGroupMembers.Add( rockChatChannelMember );

                // Find all member records for this group & person combination.
                if ( !membersByGroupAndPerson.TryGetValue( groupAndPersonKey, out var members ) || !members.Any() )
                {
                    // This person is not currently a member of this group.
                    rockChatChannelMember.ShouldDelete = true;
                    continue;
                }

                var memberToSync = members
                    .OrderBy( gm => gm.IsArchived ) // Prefer non-archived.
                    .ThenByDescending( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ) // Prefer active.
                    .ThenBy( gm => gm.GroupRoleOrder )
                    .ThenByDescending( gm => gm.GroupRoleIsLeader )
                    .ThenBy( gm => gm.GroupRoleId )
                    .First();

                if ( !memberToSync.IsGroupActive || memberToSync.IsGroupArchived )
                {
                    // This member's group is not currently active; they'll only be synced if recently deceased.
                    rockChatChannelMember.IsDeceased = memberToSync.IsDeceased;
                    rockChatChannelMember.ShouldIgnore = true;
                    continue;
                }

                var isMemberInactive = memberToSync.IsArchived || memberToSync.GroupMemberStatus != GroupMemberStatus.Active;
                if ( isMemberInactive && !memberToSync.IsChatBanned && !memberToSync.IsDeceased )
                {
                    // Only delete non-archived, non-active, non-banned members, non-deceased members.
                    // Other processes within the chat sync code know how to properly delete deceased individuals.
                    rockChatChannelMember.ShouldDelete = true;
                }
                else
                {
                    rockChatChannelMember.ChatRole = memberToSync.ChatRole;
                    rockChatChannelMember.IsChatMuted = memberToSync.IsChatMuted;
                    rockChatChannelMember.IsChatBanned = memberToSync.IsChatBanned;
                    rockChatChannelMember.IsDeceased = memberToSync.IsDeceased;
                    rockChatChannelMember.ShouldDelete = false;
                    rockChatChannelMember.ShouldIgnore = false;
                }
            }

            return rockChatGroupMembers;
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
        /// Tries to convert a <see cref="RockChatGroup"/> to a <see cref="ChatChannel"/>.
        /// </summary>
        /// <param name="rockChatGroup">The <see cref="RockChatGroup"/> to convert.</param>
        /// <returns>A <see cref="ChatChannel"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannel TryConvertToChatChannel( RockChatGroup rockChatGroup )
        {
            if ( rockChatGroup?.GroupId < 1 || rockChatGroup.GroupTypeId < 1 )
            {
                return null;
            }

            return new ChatChannel
            {
                Key = GetChatChannelKey( rockChatGroup.GroupId, rockChatGroup.ChatChannelKey ),
                ChatChannelTypeKey = GetChatChannelTypeKey( rockChatGroup.GroupTypeId ),
                QueryableKey = ChatProvider.GetQueryableChatChannelKey( rockChatGroup ),
                Name = rockChatGroup.Name,
                IsLeavingAllowed = rockChatGroup.IsLeavingAllowed,
                IsPublic = rockChatGroup.IsPublic,
                IsAlwaysShown = rockChatGroup.IsAlwaysShown,
                IsActive = rockChatGroup.IsChatChannelActive
            };
        }

        /// <summary>
        /// Tries to convert a <see cref="RockChatGroupMember"/> and <paramref name="chatUserKey"/> to a
        /// <see cref="ChatChannelMember"/>.
        /// </summary>
        /// <param name="rockChatChannelMember">The <see cref="RockChatGroupMember"/> to convert.</param>
        /// <param name="chatChannelTypeKey">The <see cref="ChatChannelType.Key"/> that represents this <see cref="ChatChannelMember"/>.</param>
        /// <param name="chatChannelKey">The <see cref="ChatChannel.Key"/> that represents this <see cref="ChatChannelMember"/>.</param>
        /// <param name="chatUserKey">The <see cref="ChatUser.Key"/> that represents this <see cref="ChatChannelMember"/>.</param>
        /// <returns>A <see cref="ChatChannelMember"/> or <see langword="null"/> if unable to convert.</returns>
        private ChatChannelMember TryConvertToChatChannelMember( RockChatGroupMember rockChatChannelMember, string chatChannelTypeKey, string chatChannelKey, string chatUserKey )
        {
            if ( rockChatChannelMember == null
                || chatChannelTypeKey.IsNullOrWhiteSpace()
                || chatChannelKey.IsNullOrWhiteSpace()
                || chatUserKey.IsNullOrWhiteSpace()
            )
            {
                return null;
            }

            return new ChatChannelMember
            {
                ChatChannelTypeKey = chatChannelTypeKey,
                ChatChannelKey = chatChannelTypeKey,
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
            if ( rockChatUserPerson?.ChatPersonAliasGuid == null )
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
                Key = GetChatUserKey( rockChatUserPerson.ChatPersonAliasGuid.Value ),
                Name = fullName,
                PhotoUrl = photoUrl,
                IsAdmin = rockChatUserPerson.IsChatAdministrator,
                IsProfileVisible = isProfileVisible,
                IsOpenDirectMessageAllowed = isOpenDirectMessageAllowed,
                Badges = rockChatUserPerson.Badges
            };
        }

        #endregion Converters: From Rock Models To Rock Chat DTOs

        #region Synchronization: From Rock To Chat Provider

        /// <summary>
        /// Deletes <see cref="ChatUser"/>s in the external chat system and optionally transfers ownership of their
        /// resources to a new <see cref="ChatUser"/>.
        /// </summary>
        /// <param name="personAliasService">
        /// The <see cref="PersonAliasService"/> to use for clearing name and foreign key values.
        /// </param>
        /// <param name="keysToDelete">The list of <see cref="RockChatUserKey"/>s to delete.</param>
        /// <param name="newKey">The new <see cref="ChatUser.Key"/> to whom ownership of resources should be transferred.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a <see cref="ChatSyncCrudResult"/>.
        /// </returns>
        /// <remarks>
        /// For <see cref="ChatUser"/>s who are successfully deleted in the external chat system, their corresponding,
        /// chat-specific <see cref="PersonAlias"/> records will also be cleared.
        /// </remarks>
        private async Task<ChatSyncCrudResult> DeleteChatUsersAsync( PersonAliasService personAliasService, List<RockChatUserKey> keysToDelete, RockChatUserKey newKey = null )
        {
            var result = new ChatSyncCrudResult();

            var structuredLog = "KeysToDelete: {@KeysToDelete}, NewKey: {@NewKey}";
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( DeleteChatUsersAsync ).SplitCase()} failed";

            try
            {
                var chatUserKeys = keysToDelete
                    .Select( k => k.ChatUserKey )
                    .ToList();

                var deleteResult = await ChatProvider.DeleteChatUsersAsync( chatUserKeys, newKey?.ChatUserKey );
                if ( deleteResult == null || deleteResult.HasException )
                {
                    result.Exception = deleteResult?.Exception;
                    Logger.LogError( result.Exception, $"{logMessagePrefix} at step '{nameof( ChatProvider.DeleteChatUsersAsync ).SplitCase()}'. {structuredLog}", keysToDelete, newKey );
                }
                else
                {
                    result.Skipped.UnionWith( deleteResult.Skipped );
                    result.Deleted.UnionWith( deleteResult.Deleted );
                }
            }
            catch ( Exception ex )
            {
                result.Exception = ex;
                Logger.LogError( ex, $"{logMessagePrefix}. {structuredLog}", keysToDelete, newKey );
            }

            // Even if an exception occurred, if any users were actually deleted in the external chat system (or skipped),
            // let's take this opportunity to ensure their chat person alias name and foreign key values are cleared.
            if ( result.Deleted.Any() || result.Skipped.Any() )
            {
                var keysToClear = keysToDelete
                    .Where( k =>
                        result.Deleted.Contains( k.ChatUserKey )
                        || result.Skipped.Contains( k.ChatUserKey )
                    )
                    .ToList();

                // This will always be a short list of records (usually just one), so a  SQL `WHERE...IN` clause will
                // never be problematic here.
                var deletedAliasIds = keysToClear.Select( k => k.ChatPersonAliasId.Value ).ToList();
                var aliasesToUpdateQry = personAliasService
                    .Queryable()
                    .Where( a => deletedAliasIds.Contains( a.Id ) );

                RockContext.BulkUpdate( aliasesToUpdateQry, a => new PersonAlias { Name = null, ForeignKey = null } );
            }

            return result;
        }

        #endregion Synchronization: From Rock To Chat Provider

        #region Synchronization: From Chat Provider To Rock

        /// <summary>
        /// Deletes Rock chat users and inactivates their corresponding chat-enabled group members.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat users to delete.</param>
        /// <param name="config">The optional configuration for fine-tuning the deletion process.</param>
        private ChatSyncCrudResult DeleteChatUsersInRock( List<DeleteChatPersonInRockCommand> syncCommands, DeleteChatUsersInRockConfig config = null )
        {
            var result = new ChatSyncCrudResult();

            if ( config == null )
            {
                config = new DeleteChatUsersInRockConfig();
            }

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
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
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

                    // Inactivate this person's chat-related group member records.
                    var groupMembers = new GroupMemberService( rockContext )
                        // In this case, we DO want to get deceased individuals, so we can inactivate them.
                        // Their group member records SHOULD already be inactive, but we'll just make sure.
                        .Queryable( includeDeceased: true )
                        .Where( gm =>
                            gm.PersonId == chatPersonAlias.PersonId
                            && (
                                gm.GroupId == ChatAdministratorsGroupId
                                || (
                                    config.ShouldUnban
                                    && gm.GroupId == ChatBanListGroupId
                                )
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

                    var shouldSaveChanges = false;

                    foreach ( var groupMember in groupMembers )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;

                        if ( !groupMember.InactiveDateTime.HasValue )
                        {
                            groupMember.InactiveDateTime = RockDateTime.Now;
                        }

                        shouldSaveChanges = true;
                    }

                    if ( config.ShouldClearChatPersonAliasForeignKey )
                    {
                        // We can't DELETE this chat person alias (because there are probably already interactions tied
                        // to it), so we should - instead - clear out the name and foreign key field that designates it
                        // as a chat-specific record.
                        chatPersonAlias.Name = null;
                        chatPersonAlias.ForeignKey = null;

                        shouldSaveChanges = true;
                    }

                    if ( shouldSaveChanges )
                    {
                        rockContext.SaveChanges();
                    }

                    result.Deleted.Add( chatPersonAlias.Id.ToString() );
                    syncCommand.MarkAsCompleted();
                }
            }

            return result;
        }

        /// <summary>
        /// Synchronizes chat channels to Rock groups.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channels to sync.</param>
        /// <returns>A <see cref="ChatSyncCrudResult"/> containing the results of the synchronization.</returns>
        internal ChatSyncCrudResult SyncChatChannelsToRock( List<SyncChatChannelToRockCommand> syncCommands )
        {
            var result = new ChatSyncCrudResult();

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
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
                using ( var rockContext = new RockContext { IsRockToChatSyncEnabled = false } )
                {
                    var groupService = new GroupService( rockContext );

                    // Try to get the targeted group.
                    Group group;

                    if ( groupId.HasValue )
                    {
                        group = groupService
                            .GetChatChannelGroupsQuery()
                            .FirstOrDefault( g => g.Id == groupId.Value );

                        if ( group == null )
                        {
                            // Since the group's ID was provided, this means the group existed in Rock before we got this
                            // sync command. If we can't find a group matching the specified ID, we have nothing more to do.
                            if ( syncCommand.ChatSyncType == ChatSyncType.Delete )
                            {
                                // No need to report these delete attempts as failures.
                                result.Skipped.Add( groupId.ToString() );
                                syncCommand.MarkAsSkipped();
                                continue;
                            }

                            syncCommand.MarkAsUnrecoverable( $"Rock group with ID {groupId.Value} could not be found." );
                            continue;
                        }
                    }
                    else
                    {
                        group = groupService
                            .GetChatChannelGroupsQuery()
                            .FirstOrDefault( g => g.ChatChannelKey == syncCommand.ChatChannelKey );
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
                            result.Skipped.Add( groupId.ToString() );
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        var newGroup = new Group
                        {
                            ParentGroupId = ChatDirectMessagesGroupId,
                            Name = syncCommand.GroupName,
                            GroupTypeId = groupTypeId.Value,
                            ChatChannelKey = syncCommand.ChatChannelKey,
                            IsActive = syncCommand.IsActive
                        };

                        // Add the group.
                        groupService.Add( newGroup );
                        rockContext.SaveChanges();

                        result.Created.Add( newGroup.Id.ToString() );
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
                            result.Skipped.Add( groupId.ToString() );
                            syncCommand.MarkAsSkipped();
                            continue;
                        }

                        /*
                            3/18/2025 - JPH

                            While the sync command technically contains an `IsActive` value, we'll not consider this when
                            updating existing Rock groups.

                            Reason: Rock should be the system of truth for `IsActive` & `IsArchived` status, for existing groups.
                         */

                        group.Name = syncCommand.GroupName;

                        rockContext.SaveChanges();

                        result.Updated.Add( groupId.ToString() );
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

            return result;
        }

        /// <summary>
        /// Synchronizes chat channel members to Rock group members.
        /// </summary>
        /// <param name="syncCommands">The list of commands for chat channel members to sync.</param>
        /// <returns>A <see cref="ChatSyncCrudResult"/> containing the results of the synchronization.</returns>
        internal ChatSyncCrudResult SyncChatChannelMembersToRock( List<SyncChatChannelMemberToRockCommand> syncCommands )
        {
            var result = new ChatSyncCrudResult();

            foreach ( var syncCommand in syncCommands )
            {
                syncCommand.ResetForSyncAttempt();

                int? groupId = syncCommand.GroupId;
                if ( !groupId.HasValue && syncCommand.ChatChannelKey.IsNullOrWhiteSpace() )
                {
                    syncCommand.MarkAsUnrecoverable( "Rock group ID and chat channel key are both missing from sync command; unable to identify group." );
                    continue;
                }

                // We don't have a person ID yet, so we'll start with their chat user key.
                var memberId = $"{syncCommand.GroupId}|{syncCommand.ChatPersonKey}";

                // We'll use a new rock context to keep each command isolated.
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
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
                            result.Skipped.Add( memberId );
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

                    // Supplement the command and update the member ID.
                    syncCommand.PersonAliasId = chatPersonAlias.Id;
                    syncCommand.PersonId = chatPersonAlias.PersonId;
                    memberId = $"{syncCommand.GroupId}|{chatPersonAlias.PersonId}";

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
                            result.Skipped.Add( memberId );
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
                                GroupTypeId = groupTypeCache.Id,
                                IsChatBanned = syncCommand.IsBanned ?? false,
                                IsChatMuted = syncCommand.IsMuted ?? false
                            }
                        );

                        // For new member creation, we actually DO want to sync back to the external chat system, to
                        // ensure the correct channel member role is in place.
                        rockContext.IsRockToChatSyncEnabled = true;
                        rockContext.SaveChanges();

                        result.Created.Add( memberId );
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

                        result.Deleted.Add( memberId );
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

            return result;
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
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
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
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
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
                // Note that we're also disabling Rock-to-Chat post save hooks as a result of any saves performed here.
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
        /// Logs Chat-to-Rock sync command outcomes to Rock Logs.
        /// </summary>
        /// <param name="syncCommands">The list of sync commands to log.</param>
        private void LogChatToRockSyncCommandOutcomes( List<ChatToRockSyncCommand> syncCommands )
        {
            var logMessagePrefix = $"{LogMessagePrefix} {nameof( SyncFromChatToRockAsync ).SplitCase()}";
            var structuredLog = "SyncCommand: {@SyncCommand}";

            foreach ( var syncCommand in syncCommands.Where( c => c.WasCompleted ) )
            {
                Logger.LogDebug( $"{logMessagePrefix} succeeded. {structuredLog}", syncCommand );
            }

            foreach ( var syncCommand in syncCommands.Where( c => c.HasFailureReason && c.ShouldRetry ) )
            {
                Logger.LogInformation( $"{logMessagePrefix} failed. {syncCommand.FailureReason} {structuredLog}", syncCommand );
            }

            foreach ( var syncCommand in syncCommands.Where( c => c.HasFailureReason && !c.ShouldRetry ) )
            {
                Logger.LogError( $"{logMessagePrefix} failed. {syncCommand.FailureReason} {structuredLog}", syncCommand );
            }
        }

        /// <summary>
        /// Requeues recoverable Chat-to-Rock sync commands for retrying.
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

        #endregion Private Methods

        #region Supporting Members

        /// <summary>
        /// Represents a Rock <see cref="Group"/> identifier for Chat-to-Rock synchronization.
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
        /// Represents a Rock <see cref="Person"/> identifier for Chat-to-Rock synchronization.
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
