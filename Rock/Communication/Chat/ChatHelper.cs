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
        /// The backing field for the <see cref="RockToChatSyncConfig"/> property.
        /// </summary>
        private readonly RockToChatSyncConfig _rockToChatSyncConfig = new RockToChatSyncConfig();

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
        private const string ChatPersonAliasForeignKeyPrefix = "core-chat:";

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
        /// Gets the Rock-to-Chat sync configuration for this chat helper.
        /// </summary>
        public RockToChatSyncConfig RockToChatSyncConfig => _rockToChatSyncConfig;

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
        /// <param name="groupId">The <see cref="Group"/> identifier for which to get the <see cref="ChatChannel.Key"/>.</param>
        /// <returns>The <see cref="ChatChannel.Key"/>.</returns>
        public static string GetChatChannelKey( int groupId )
        {
            return $"{ChatChannelKeyPrefix}{groupId}";
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
        public static int? GetPersonAliasId( string chatUserKey )
        {
            if ( chatUserKey.IsNotNullOrWhiteSpace() )
            {
                return chatUserKey.Replace( ChatUserKeyPrefix, string.Empty ).AsIntegerOrNull();
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

                var results = await CreateOrUpdateChatUsersAsync( new List<SyncPersonToChatCommand> { syncCommand } );

                var chatUserResult = results?.FirstOrDefault( r => r?.ChatUserKey.IsNotNullOrWhiteSpace() == true );
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
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EnsureChatProviderAppIsSetupAsync()
        {
            if ( !IsChatEnabled )
            {
                return;
            }

            SetChatProviderDefaultValueEnforcers();

            try
            {
                await ChatProvider.EnsureAppRolesExistAsync();
                await ChatProvider.EnsureAppGrantsExistAsync();
                await ChatProvider.EnsureSystemUserExistsAsync();
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( EnsureChatProviderAppIsSetupAsync ) );
            }
        }

        /// <summary>
        /// Synchronizes <see cref="GroupType"/>s from Rock to <see cref="ChatChannelType"/>s in the external chat system.
        /// </summary>
        /// <param name="groupTypes">The <see cref="GroupType"/>s to synchronize.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of identifiers for <see cref="GroupType"/>s
        /// that were successfully synchronized.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This one-way synchronization will result in <see cref="ChatChannelType"/>s being added, updated or deleted
        /// within the external chat system, but will NOT result in any changes being made to Rock <see cref="GroupType"/>s.
        /// </para>
        /// <para>
        /// WARNING! if a previously-synced channel type no longer has chat enabled (<see cref="GroupType.IsChatAllowed"/>),
        /// this synchronization will result in IMMEDIATE DELETION of the <see cref="ChatChannelType"/>, as well as ALL
        /// related <see cref="ChatChannel"/>s, <see cref="ChatChannelMember"/>s and messages in the external chat system!
        /// </para>
        /// </remarks>
        public async Task<List<int>> SyncGroupTypesToChatProviderAsync( List<GroupType> groupTypes )
        {
            var results = new List<int>();

            if ( !IsChatEnabled || groupTypes?.Any() != true )
            {
                return results;
            }

            SetChatProviderDefaultValueEnforcers();

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
                void AddGroupTypeToResults( string channelTypeKey )
                {
                    if ( groupTypeIdByChannelTypeKeys.TryGetValue( channelTypeKey, out var groupTypeId ) )
                    {
                        results.Add( groupTypeId );
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
                                AddGroupTypeToResults( channelType.Key );
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
                    deletedKeys?.ForEach( k => AddGroupTypeToResults( k ) );
                }

                if ( channelTypesToCreate.Any() )
                {
                    var createdChannelTypes = await ChatProvider.CreateChatChannelTypesAsync( channelTypesToCreate );
                    createdChannelTypes?.ForEach( ct => AddGroupTypeToResults( ct?.Key ) );
                }

                if ( channelTypesToUpdate.Any() )
                {
                    var updatedChannelTypes = await ChatProvider.UpdateChatChannelTypesAsync( channelTypesToUpdate );
                    updatedChannelTypes?.ForEach( ct => AddGroupTypeToResults( ct?.Key ) );
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( SyncGroupTypesToChatProviderAsync ) );
            }

            return results;
        }

        /// <summary>
        /// Synchronizes <see cref="Group"/>s from Rock to <see cref="ChatChannel"/>s in the external chat system.
        /// </summary>
        /// <param name="groups">The <see cref="Group"/>s to synchronize.</param>
        /// <param name="alreadySyncedGroupIds">DO NOT manually provide an argument for this parameter; a hash set of
        /// <see cref="Group"/> identifiers already synced will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of identifiers for <see cref="Group"/>s
        /// that were successfully synchronized.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This one-way synchronization will result in <see cref="ChatChannel"/>s being added, updated or deleted
        /// within the external chat system, but will NOT result in any changes being made to Rock <see cref="Group"/>s.
        /// </para>
        /// <para>
        /// WARNING! if a previously-synced channel no longer has chat enabled (<see cref="Group.GetIsChatEnabled"/>),
        /// this synchronization will result in IMMEDIATE DELETION of the <see cref="ChatChannel"/> and ALL related
        /// <see cref="ChatChannelMember"/>s and messages in the external chat system!
        /// </para>
        /// </remarks>
        public async Task<List<int>> SyncGroupsToChatProviderAsync( List<Group> groups, HashSet<int> alreadySyncedGroupIds = null )
        {
            var results = new List<int>();

            // Only sync valid, already-saved groups.
            groups = groups?.Where( g => g?.Id > 0 && g.GroupTypeId > 0 ).ToList();

            if ( !IsChatEnabled || groups?.Any() != true )
            {
                return results;
            }

            SetChatProviderDefaultValueEnforcers();

            try
            {
                // Get the existing chat channels.
                var queryableChatChannelKeys = groups
                    .Select( g => ChatProvider.GetQueryableChatChannelKey( g ) )
                    .ToList();

                var existingChannels = await ChatProvider.GetChatChannelsAsync( queryableChatChannelKeys );

                var channelsToCreate = new List<ChatChannel>();
                var channelsToUpdate = new List<ChatChannel>();
                var channelsToDelete = new List<string>();

                // A mapping dictionary and local function to add a group to the outgoing results collection only AFTER
                // we know it's been successfully synced with the external chat system.
                var groupIdByChannelKeys = new Dictionary<string, int>();
                void AddGroupToResults( string channelKey )
                {
                    if ( groupIdByChannelKeys.TryGetValue( channelKey, out var groupId ) )
                    {
                        results.Add( groupId );
                    }
                }

                foreach ( var group in groups )
                {
                    var channel = TryConvertToChatChannel( group );
                    if ( channel == null )
                    {
                        continue;
                    }

                    groupIdByChannelKeys.Add( channel.Key, group.Id );

                    // Does it already exist in the external chat system?
                    var existingChannel = existingChannels?.FirstOrDefault( c => c.QueryableKey == channel.QueryableKey );

                    // For each chat-enabled group, add or update the channel in the external chat system.
                    if ( group.GetIsChatEnabled() )
                    {
                        if ( existingChannel != null )
                        {
                            // Only update if anything has actually changed.
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
                                AddGroupToResults( channel.Key );
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
                    deletedKeys?.ForEach( k => AddGroupToResults( k ) );
                }

                if ( channelsToCreate.Any() )
                {
                    var createdChannels = await ChatProvider.CreateChatChannelsAsync( channelsToCreate );
                    createdChannels?.ForEach( async c =>
                    {
                        AddGroupToResults( c?.Key );

                        // For each newly-added channel, try to sync its members.
                        var groupId = GetGroupId( c?.Key );
                        if ( groupId.HasValue )
                        {
                            await SyncGroupMembersToChatProviderAsync( groupId.Value, alreadySyncedGroupIds );
                        }
                    } );
                }

                if ( channelsToUpdate.Any() )
                {
                    var updatedChannels = await ChatProvider.UpdateChatChannelsAsync( channelsToUpdate );
                    updatedChannels?.ForEach( c => AddGroupToResults( c?.Key ) );
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( SyncGroupsToChatProviderAsync ) );
            }

            return results;
        }

        /// <summary>
        /// Synchronizes <see cref="GroupMember"/>s from Rock to <see cref="ChatUser"/>s and <see cref="ChatChannelMember"/>s
        /// in the external chat system.
        /// </summary>
        /// <param name="groupId">The identifier of the <see cref="Group"/> whose <see cref="GroupMember"/>s should be synced.</param>
        /// <param name="alreadySyncedGroupIds">DO NOT manually provide an argument for this parameter; a hash set of
        /// <see cref="Group"/> identifiers already synced will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of <see cref="RockChatChannelMember"/>s,
        /// one for each <see cref="Group"/> and <see cref="Person"/> combination that was synced.
        /// </returns>
        /// <remarks>
        /// This one-way synchronization will result in <see cref="ChatUser"/>s being added or updated, as well as
        /// <see cref="ChatChannelMember"/>s being added, updated or deleted within the external chat system, but will
        /// NOT result in any changes being made to Rock <see cref="GroupMember"/>s, <see cref="Person"/>s or
        /// <see cref="PersonAlias"/>es.
        /// </remarks>
        public async Task<List<RockChatChannelMember>> SyncGroupMembersToChatProviderAsync( int groupId, HashSet<int> alreadySyncedGroupIds = null )
        {
            if ( groupId <= 0 )
            {
                return new List<RockChatChannelMember>();
            }

            // Get the sync commands for this group's members.
            var syncCommands = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm => gm.GroupId == groupId )
                .Select( gm => new SyncGroupMemberToChatCommand
                {
                    GroupId = gm.GroupId,
                    PersonId = gm.PersonId
                } )
                .ToList();

            return await SyncGroupMembersToChatProviderAsync( syncCommands, alreadySyncedGroupIds );
        }

        /// <summary>
        /// Synchronizes <see cref="GroupMember"/>s from Rock to <see cref="ChatUser"/>s and <see cref="ChatChannelMember"/>s
        /// in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="GroupMember"/>s to sync.</param>
        /// <param name="alreadySyncedGroupIds">DO NOT manually provide an argument for this parameter; a hash set of
        /// <see cref="Group"/> identifiers already synced will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of <see cref="RockChatChannelMember"/>s,
        /// one for each <see cref="Group"/> and <see cref="Person"/> combination that was synced.
        /// </returns>
        /// <remarks>
        /// This one-way synchronization will result in <see cref="ChatUser"/>s being added or updated, as well as
        /// <see cref="ChatChannelMember"/>s being added, updated or deleted within the external chat system, but will
        /// NOT result in any changes being made to Rock <see cref="GroupMember"/>s, <see cref="Person"/>s or
        /// <see cref="PersonAlias"/>es.
        /// </remarks>
        public async Task<List<RockChatChannelMember>> SyncGroupMembersToChatProviderAsync( List<SyncGroupMemberToChatCommand> syncCommands, HashSet<int> alreadySyncedGroupIds = null )
        {
            var results = new List<RockChatChannelMember>();

            // Validate commands.
            syncCommands = syncCommands
                ?.Where( c => c?.GroupId > 0 && c.PersonId > 0 )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return results;
            }

            SetChatProviderDefaultValueEnforcers();

            try
            {
                // Don't let individual channel failures cause all to fail.
                var perChannelExceptions = new List<Exception>();

                #region APP - Chat Administrators

                // First, we'll handle any members being added to or removed from the "APP - Chat Administrators" Rock
                // group. These members don't need to be added to a chat channel, but instead only need to have their
                // chat user records created or updated accordingly, within the external chat system.
                var chatAdminsGroupGuid = Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid();
                var chatAdminsCommands = new List<SyncGroupMemberToChatCommand>();

                // Iterate backwards for ease-of-removal from the original collection; this way, we can prevent repeated
                // comparison when filtering for "delete" and "create or update" commands below.
                for ( var i = syncCommands.Count - 1; i >= 0; i-- )
                {
                    var syncCommand = syncCommands[i];
                    var groupGuid = GroupCache.Get( syncCommand.GroupId )?.Guid;

                    if ( groupGuid.HasValue && groupGuid.Value.Equals( chatAdminsGroupGuid ) )
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
                        // We'll ALWAYS enforce a "full" sync of chat users when working with this particular group, as
                        // we want to ensure global `rock_admin` roles are toggled immediately in the external chat system.
                        var shouldEnsureChatUsersExist = RockToChatSyncConfig.ShouldEnsureChatUsersExist;
                        RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;

                        // The next method call does the following:
                        //  1) Ensures this person has a chat-specific person alias in Rock;
                        //  2) Ensures this person has a chat user in the external chat system IF dictated by `RockToChatSyncConfig.ShouldEnsureChatUsersExist`.
                        var createOrUpdateChatUserResults = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                        // Be sure to set this value back to whatever it was.
                        RockToChatSyncConfig.ShouldEnsureChatUsersExist = shouldEnsureChatUsersExist;

                        chatAdminsCommands.ForEach( c =>
                        {
                            var chatUserResult = createOrUpdateChatUserResults
                                .FirstOrDefault( r => r.PersonId == c.PersonId );

                            if ( chatUserResult != null && !results.Any( r => r.GroupId == c.GroupId && r.PersonId == r.PersonId ) )
                            {
                                var wasDemoted = !chatUserResult.IsAdmin;
                                results.Add(
                                    // We're kind of abusing this DTO for this purpose, but it will provide a way to
                                    // convey to the caller how many people we added and removed to the `rock_admin`
                                    // global role within the external chat system.
                                    new RockChatChannelMember
                                    {
                                        GroupId = c.GroupId,
                                        PersonId = c.PersonId,
                                        ChatRole = wasDemoted ? ChatRole.User : ChatRole.Administrator,
                                        ShouldDelete = wasDemoted
                                    }
                                );
                            }
                        } );
                    }
                    catch ( Exception ex )
                    {
                        perChannelExceptions.Add( ex );
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

                    var chatChannelKey = GetChatChannelKey( groupCache.Id );

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

                void AddMemberToResults( string chatChannelKey, string chatUserKey )
                {
                    if ( memberByChannelMemberKeys.TryGetValue( $"{chatChannelKey}|{chatUserKey}", out var member ) )
                    {
                        results.Add( member );
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
                    var membersWithoutChatUserKeys = deleteMembers
                        .Where( c => !rockChatUserPersonKeys.Any( r => r.PersonId == c.PersonId ) )
                        .ToList();

                    if ( membersWithoutChatUserKeys.Any() )
                    {
                        LogWarning(
                            nameof( SyncGroupMembersToChatProviderAsync ),
                            "Unable to delete chat channel members in the external chat system, as matching chat user keys could not be found in Rock. Members without chat user keys: {@MembersWithoutChatUserKeys}",
                            membersWithoutChatUserKeys
                        );
                    }

                    // Filter down to members that we're sure have keys.
                    deleteMembers = deleteMembers
                        .Except( membersWithoutChatUserKeys )
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
                    var createOrUpdateChatUserResults = await CreateOrUpdateChatUsersAsync( syncPersonToChatCommands );

                    // Just in case any people still don't have a chat user key, let's at least log them. This will
                    // probably never happen.
                    var membersWithoutChatUserKeys = createOrUpdateMembers
                        .Where( c => !createOrUpdateChatUserResults.Any( r => r.PersonId == c.PersonId ) )
                        .ToList();

                    if ( membersWithoutChatUserKeys.Any() )
                    {
                        LogWarning(
                            nameof( SyncGroupMembersToChatProviderAsync ),
                            "Unable to create or update chat channel members in the external chat system, as matching chat user keys could not be found in Rock. Members without chat user keys: {@MembersWithoutChatUserKeys}",
                            membersWithoutChatUserKeys
                        );
                    }

                    // Filter down to members that we're sure have chat user keys.
                    createOrUpdateMembers = createOrUpdateMembers
                        .Except( membersWithoutChatUserKeys )
                        .ToList();

                    // Organize members by channel.
                    foreach ( var member in createOrUpdateMembers )
                    {
                        var chatUserKey = createOrUpdateChatUserResults
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
                        var membersToUpdate = new List<ChatChannelMember>();
                        var membersToDelete = new List<string>();

                        foreach ( var chatChannelMember in command.MembersToCreateOrUpdate )
                        {
                            // Does it already exist in the external chat system?
                            var existingMember = existingMembers?.FirstOrDefault( m => m.ChatUserKey == chatChannelMember.ChatUserKey );

                            if ( existingMember != null )
                            {
                                // Only update if anything has actually changed.
                                if ( existingMember.Role != chatChannelMember.Role

                                // The following properties may only be set in the external chat system.
                                //|| existingMember.IsChatMuted != member.IsChatMuted
                                //|| existingMember.IsChatBanned != member.IsChatBanned

                                )
                                {
                                    membersToUpdate.Add( chatChannelMember );
                                }
                                else
                                {
                                    // Add them to the results as an already up-to-date member.
                                    AddMemberToResults( command.ChatChannelKey, chatChannelMember.ChatUserKey );
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
                            // Does it already exist in the external chat system?
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

                            deletedKeys?.ForEach( k => AddMemberToResults( command.ChatChannelKey, k ) );
                        }

                        if ( membersToCreate.Any() )
                        {
                            var createdMembers = await ChatProvider.CreateChatChannelMembersAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                membersToCreate
                            );

                            createdMembers?.ForEach( m => AddMemberToResults( command.ChatChannelKey, m.ChatUserKey ) );
                        }

                        if ( membersToUpdate.Any() )
                        {
                            var updatedMembers = await ChatProvider.UpdateChatChannelMembersAsync(
                                command.ChatChannelTypeKey,
                                command.ChatChannelKey,
                                membersToUpdate
                            );

                            updatedMembers?.ForEach( m => AddMemberToResults( command.ChatChannelKey, m.ChatUserKey ) );
                        }
                    }
                    catch ( Exception channelEx )
                    {
                        Group group = null;
                        if ( channelEx is ChatChannelNotFoundException chatChannelNotFoundException )
                        {
                            var groupId = GetGroupId( chatChannelNotFoundException.ChatChannelKey );

                            // We'll manage a hash set of group IDs we've already tried to sync using this approach, to
                            // prevent a recursive loop that could lead to a stack overflow exception.
                            var shouldSyncGroup = groupId.GetValueOrDefault() > 0
                                && (
                                    alreadySyncedGroupIds == null
                                    || alreadySyncedGroupIds.Add( groupId.Value )
                                );

                            if ( shouldSyncGroup )
                            {
                                group = new GroupService( RockContext ).GetNoTracking( groupId.Value );
                            }
                        }

                        if ( group != null )
                        {
                            try
                            {
                                await SyncGroupsToChatProviderAsync(
                                    new List<Group> { group },
                                    alreadySyncedGroupIds ?? new HashSet<int> { group.Id }
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
                LogError( ex, nameof( SyncGroupMembersToChatProviderAsync ) );
            }

            return results;
        }

        /// <summary>
        /// Creates new or updates existing <see cref="ChatUser"/>s in the external chat system.
        /// </summary>
        /// <param name="syncCommands">The list of commands for <see cref="ChatUser"/>s to create or update.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the list of <see cref="CreateOrUpdateChatUserResult"/>s.
        /// </returns>
        public async Task<List<CreateOrUpdateChatUserResult>> CreateOrUpdateChatUsersAsync( List<SyncPersonToChatCommand> syncCommands )
        {
            var results = new List<CreateOrUpdateChatUserResult>();

            // Validate commands.
            syncCommands = syncCommands
                ?.Where( c => c?.PersonId > 0 )
                .ToList();

            if ( !IsChatEnabled || syncCommands?.Any() != true )
            {
                return results;
            }

            SetChatProviderDefaultValueEnforcers();

            try
            {
                var chatConfiguration = GetChatConfiguration();

                // Ensure each person has a chat-specific person alias record in Rock IF instructed by their respective sync commands.
                var rockChatUserPeople = GetOrCreateRockChatUserPeople( syncCommands, chatConfiguration );

                if ( !RockToChatSyncConfig.ShouldEnsureChatUsersExist )
                {
                    results.AddRange(
                        rockChatUserPeople
                            .Where( p => p.AlreadyExistedInRock )
                            .Select( p =>
                                new CreateOrUpdateChatUserResult
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
                    return results;
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
                void AddChatUserToResults( ChatUser chatUser )
                {
                    var chatUserPerson = rockChatUserPeople.FirstOrDefault( p => p.ChatUserKey == chatUser.Key );
                    if ( chatUserPerson == null )
                    {
                        // Should never happen.
                        return;
                    }

                    results.Add(
                        new CreateOrUpdateChatUserResult
                        {
                            ChatUserKey = chatUser.Key,
                            PersonId = chatUserPerson.PersonId,
                            IsAdmin = chatUser.IsAdmin
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
                        // Only update if anything has actually changed.
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
                            AddChatUserToResults( chatUser );
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
                    createdUsers?.ForEach( u => AddChatUserToResults( u ) );
                }

                if ( usersToUpdate.Any() )
                {
                    var updatedUsers = await ChatProvider.UpdateChatUsersAsync( usersToUpdate );
                    updatedUsers?.ForEach( u => AddChatUserToResults( u ) );
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, nameof( CreateOrUpdateChatUsersAsync ) );
            }

            return results;
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

        // Coming soon...

        #endregion Synchronization: From Chat Provider To Rock

        #endregion Public Methods

        #region Private Methods

        #region Logging

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
        /// <param name="exception">The exception to log.</param>
        /// <param name="operationName">The name of the operation that was taking place.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        private void LogError( Exception exception, string operationName, string message = null, params object[] args )
        {
            var logMessage = $"{nameof( ChatHelper ).SplitCase()} > {operationName.SplitCase()} failed.{( message.IsNotNullOrWhiteSpace() ? $" {message}" : string.Empty )}";
            Logger.LogError( exception, logMessage, args );
        }

        #endregion Logging

        #region Default Value Enforcers

        /// <summary>
        /// Sets the <see cref="IChatProvider"/>'s default value enforcer property values to match those of this chat
        /// helper.
        /// </summary>
        private void SetChatProviderDefaultValueEnforcers()
        {
            ChatProvider.ShouldEnforceDefaultGrantsPerRole = RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole;
            ChatProvider.ShouldEnforceDefaultSettings = RockToChatSyncConfig.ShouldEnforceDefaultSettings;
        }

        #endregion Default Value Enforcers

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
            var personQry = new PersonService( RockContext ).Queryable();
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

            var chatAdminGroupGuid = Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid();
            var chatAdministratorPersonIdQry = new GroupMemberService( RockContext )
                .Queryable()
                .Where( gm =>
                    gm.Group.Guid.Equals( chatAdminGroupGuid )
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
                    ChatAliasGuid = p.ChatAliases?.Any() == true
                        ? p.ChatAliases.OrderBy( a => a.Id ).First().Guid // Get the earliest chat alias in the case of multiple.
                        : ( Guid? ) null,
                    IsChatAdministrator = p.IsChatAdmin
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
            var badgeRosters = new List<ChatBadgeRoster>();
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

                    badgeRosters.Add(
                        new ChatBadgeRoster
                        {
                            ChatBadge = new ChatBadge
                            {
                                Key = dataViewCache.IdKey,
                                Name = dataViewCache.Name,
                                IconCssClass = dataViewCache.IconCssClass
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
                    p.Badges = null;
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
        private List<RockChatUserPersonKey> GetRockChatUserPersonKeys( List<int> personIds )
        {
            var personQry = new PersonService( RockContext ).Queryable();
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
            var groupMemberQry = groupMemberService.Queryable(); // Note that we're EXCLUDING archived members.

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

            return new ChatChannel
            {
                Key = GetChatChannelKey( group.Id ),
                ChatChannelTypeKey = GetChatChannelTypeKey( group.GroupTypeId ),
                QueryableKey = ChatProvider.GetQueryableChatChannelKey( group ),
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

        #region Converters: From Rock Chat DTOs To Rock Models

        // Coming soon...

        #endregion Converters: From Rock Chat DTOs To Rock Models

        #endregion Private Methods
    }
}
