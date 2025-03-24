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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Sync;
using Rock.Data;
using Rock.Enums.Communication.Chat;
using Rock.Logging;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Performs synchronization tasks between Rock and the external chat system.
    /// </summary>
    [DisplayName( "Chat Sync" )]
    [Description( "Performs synchronization tasks between Rock and the external chat system." )]

    #region Job Attributes

    [BooleanField( "Synchronize Data",
        Key = AttributeKey.SynchronizeData,
        Description = "Determines if data synchronization should be performed between Rock and the external chat system. If enabled, this will ensure that all chat-related data in Rock is in sync with the corresponding data in the external chat system.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField( "Create Interactions",
        Key = AttributeKey.CreateInteractions,
        Description = "Determines if chat interaction records should be created. If enabled, this will create a daily interaction for each person who posted one or more messages within a given chat channel, and will include how many messages that person posted within that channel for that day. Will only look back up to 5 days when determining the interactions to create.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 2 )]

    [BooleanField( "Delete Merged Chat Individuals",
        Key = AttributeKey.DeleteMergedChatUsers,
        Description = "Determines if non-prevailing, merged chat individuals should be deleted in the external chat system. If enabled, when two people in Rock have been merged, and both had an associated chat individual, the non-prevailing chat individual will be deleted from the external chat system to ensure other people can send future messages to only the prevailing chat individual.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField( "Enforce Default Grants Per Role",
        Key = AttributeKey.EnforceDefaultGrantsPerRole,
        Description = "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all permission grants (per role) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these permissions.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 4 )]

    [BooleanField( "Enforce Default Sync Settings",
        Key = AttributeKey.EnforceDefaultSyncSettings,
        Description = "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all settings (e.g. channel type and channel settings) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these settings.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 5 )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60,
        Category = "General",
        Order = 6 )]

    #endregion Job Attributes

    [RockLoggingCategory]
    public class ChatSync : RockJob
    {
        #region Keys & Constants

        /// <summary>
        /// Attribute Keys for the <see cref="ChatSync"/> job.
        /// </summary>
        private static class AttributeKey
        {
            public const string SynchronizeData = "SynchronizeData";
            public const string CreateInteractions = "CreateInteractions";
            public const string DeleteMergedChatUsers = "DeleteMergedChatUsers";
            public const string EnforceDefaultGrantsPerRole = "EnforceDefaultGrantsPerRole";
            public const string EnforceDefaultSyncSettings = "EnforceDefaultSyncSettings";
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// The friendly message strings used for CRUD operations.
        /// </summary>
        private static class CrudMessage
        {
            public const string Skipped = "Already Up-to-Date";
            public const string Created = "Created";
            public const string Updated = "Updated";
            public const string Deleted = "Deleted";

            // Alternative wordings.
            public const string Present = "Already Present";
        }

        #endregion Keys & Constants

        #region Fields

        /// <summary>
        /// The list of chat sync result sections.
        /// </summary>
        private readonly List<ChatSyncResultSection> _resultSections = new List<ChatSyncResultSection>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Empty constructor for job initialization.
        /// </summary>
        /// <remarks>
        /// Jobs require a public empty constructor so that the scheduler can instantiate the class whenever it needs.
        /// </remarks>
        public ChatSync()
        {
        }

        #endregion Constructors

        #region RockJob Implementation

        /// <inheritdoc/>
        public override void Execute()
        {
            // If chat is not enabled, exit early.
            if ( !ChatHelper.IsChatEnabled )
            {
                var section = CreateAndAddResultSection( null ); // No section title.

                var taskResult = CreateAndAddNewTaskResult( section, "Chat is not enabled.", TimeSpan.Zero );
                taskResult.IsWarning = true;

                ReportResults( showWarningInstructions: false );
                return;
            }

            // If there are Task.Runs that don't handle their exceptions, this will catch those so that we can log them.
            // Note that this event won't fire until the Task is disposed. In most cases, that'll be when GC is collected.
            // So it won't happen immediately.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var syncTask = Task.Run( async () =>
            {
                using ( var rockContext = new RockContext() )
                using ( var chatHelper = new ChatHelper( rockContext ) )
                {
                    var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
                    rockContext.Database.CommandTimeout = commandTimeout;

                    // The chat helper methods that will be called by this job have been designed to never throw an
                    // unhandled exception, but will - instead - return an object that will contain any exceptions
                    // encountered during a given operation. This allows us to continue processing other tasks, and
                    // aggregate all exceptions at the end of the job run.

                    if ( GetAttributeValue( AttributeKey.SynchronizeData ).AsBoolean() )
                    {
                        await SynchronizeData( rockContext, chatHelper );
                    }

                    if ( GetAttributeValue( AttributeKey.CreateInteractions ).AsBoolean() )
                    {
                        await CreateInteractionsAsync( rockContext, chatHelper );
                    }

                    if ( GetAttributeValue( AttributeKey.DeleteMergedChatUsers ).AsBoolean() )
                    {
                        await DeleteMergedChatUsersAsync( rockContext, chatHelper );
                    }
                }
            } );

            // We need to wait for this task to complete so we can report the results.
            syncTask.Wait();

            ReportResults();
        }

        #endregion RockJob Implementation

        #region Private Methods

        #region Synchronize Data

        /// <summary>
        /// <para>
        /// First, ensures that all chat-related data in Rock is in sync with the corresponding data in the external chat system.
        /// </para>
        /// <para>
        /// Second, ensures that all chat-related data in the external chat system is in sync with the corresponding data in Rock.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="chatHelper">The chat helper.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SynchronizeData( RockContext rockContext, ChatHelper chatHelper )
        {
            #region 1) Rock-to-Chat Sync

            chatHelper.RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole = GetAttributeValue( AttributeKey.EnforceDefaultGrantsPerRole ).AsBoolean();
            chatHelper.RockToChatSyncConfig.ShouldEnforceDefaultSettings = GetAttributeValue( AttributeKey.EnforceDefaultSyncSettings ).AsBoolean();

            var rockToChatStopwatch = new Stopwatch();

            var section = CreateAndAddResultSection( "Rock-to-Chat Sync:" );

            // ---------------------------------------------------------
            // 1a) Ensure the app is set up in the external chat system.

            rockToChatStopwatch.Restart();
            var isSetUpResult = await chatHelper.EnsureChatProviderAppIsSetUpAsync();
            rockToChatStopwatch.Stop();

            var taskResult = CreateAndAddNewTaskResult( section, "Chat Configuration", rockToChatStopwatch.Elapsed );

            // If the setup operation failed, we can't continue with the other tasks.
            if ( isSetUpResult?.HasException == true )
            {
                taskResult.Exception = isSetUpResult.Exception;
                return;
            }
            else if ( isSetUpResult?.IsSetUp != true )
            {
                Log( LogLevel.Warning, "One or more setup operations failed within the external Chat system." );

                taskResult.IsWarning = true;
                return;
            }

            // -------------------------------------------------------------------------------
            // 1b) Delete any Rock chat users who no longer exist in the external chat system.

            // Technically, this should be considered a Chat-to-Rock sync operation, but we need to perform it up front,
            // as it's important to delete these Rock chat users before attempting to perform the remaining Rock-to-Chat
            // sync operations. This should prevent exceptions that might otherwise be encountered by referencing chat
            // user keys that have already been deleted in the external chat system. We won't report these deletions in
            // the job results, as we don't want to cause alarm.

            await chatHelper.DeleteRockChatUsersMissingFromChatProviderAsync();

            // ------------------------------------------------------------------
            // 1c) Delete any deceased individuals from the external chat system.

            // We'll keep track of chat users that are synced along the way, so we ensure each person is fully-synced
            // all the way through to the external chat system ONLY ONCE throughout this job run.
            var rockToChatUsersStopwatch = new Stopwatch();
            var rockToChatUsersResult = new ChatSyncCrudResult();
            var rockToChatUsersExceptions = new List<Exception>();

            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            // This should almost never find any individuals to delete, as the chat helper already has a pretty robust
            // process for inactivating Rock group members and deleting external chat users, as soon as they're marked
            // as deceased in Rock. This is just a safeguard, in case that process fails for some reason, or in case the
            // deceased individual didn't belong to any chat-enabled groups at the time of being marked as deceased.

            rockToChatUsersStopwatch.Start();

            var deceasedChatUserPersonIds = personService.GetDeceasedChatUserPersonIdsQuery().ToList();
            if ( deceasedChatUserPersonIds.Any() )
            {
                foreach ( var personId in deceasedChatUserPersonIds )
                {
                    var deleteDeceasedIndividualsResult = await chatHelper.DeleteChatUsersAsync( personId, personAliasService );
                    if ( deleteDeceasedIndividualsResult?.Deleted.Any() == true )
                    {
                        rockToChatUsersResult.Deleted.Add( personId.ToString() );
                    }
                }
            }

            rockToChatUsersStopwatch.Stop();

            // ---------------------------------------------------------------------------
            // 1d) Add/[re]enforce global chat bans by syncing "Chat Ban List" chat users.

            var globallyBannedChatUserKeys = new HashSet<string>();

            rockToChatUsersStopwatch.Start();
            var chatBanListResult = await chatHelper.SyncGroupMembersToChatProviderAsync( ChatHelper.ChatBanListGroupId );
            rockToChatUsersStopwatch.Stop();

            if ( chatBanListResult != null )
            {
                // Add any exceptions encountered at the channel member sync stage (should never happen within this call).
                if ( chatBanListResult.HasException )
                {
                    rockToChatUsersExceptions.Add( chatBanListResult.Exception );
                }

                // Add any exceptions encountered at the global chat user sync stage.
                var chatBanListUserExceptions = chatBanListResult
                    .InnerResults
                    .OfType<ChatSyncCreateOrUpdateUsersResult>()
                    .Where( r => r.HasException )
                    .Select( r => r.Exception )
                    .ToList();

                if ( chatBanListUserExceptions.Any() )
                {
                    rockToChatUsersExceptions.AddRange( chatBanListUserExceptions );
                }

                // Aggregate the chat user CRUD results from the "Chat Ban List" sync operation.
                // There should be no deletions here.
                rockToChatUsersResult.Skipped.UnionWith(
                    chatBanListResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                        .Select( r => r.PersonId.ToString() )
                );

                rockToChatUsersResult.Created.UnionWith(
                    chatBanListResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                        .Select( r => r.PersonId.ToString() )
                );

                rockToChatUsersResult.Updated.UnionWith(
                    chatBanListResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                        .Select( r => r.PersonId.ToString() )
                );

                // Add any exceptions encountered at the global ban/unban sync stage.
                var chatBanUnbanExceptions = chatBanListResult
                    .InnerResults
                    .OfType<ChatSyncBanResult>()
                    .Where( r => r.HasException )
                    .Select( r => r.Exception )
                    .ToList();

                if ( chatBanUnbanExceptions.Any() )
                {
                    rockToChatUsersExceptions.AddRange( chatBanUnbanExceptions );
                }

                // Aggregate the chat user ban results from the "Chat Ban List" sync operation. Note that global bans
                // could only have been added/[re]enforced by this process; not lifted/removed.
                globallyBannedChatUserKeys.UnionWith(
                    chatBanListResult
                        .InnerResults
                        .OfType<ChatSyncBanResult>()
                        .SelectMany( r => r.Banned )
                );
            }

            // -------------------------------------------------------------
            // 1e) Sync "APP - Chat Administrator" security role chat users.

            // Only perform this sync if chat is NOT enabled for the chat admins group. Otherwise, these chat users will
            // be synced as part of the regular group sync process below.
            var chatAminsGroup = GroupCache.Get( ChatHelper.ChatAdministratorsGroupId );
            if ( chatAminsGroup?.GetIsChatEnabled() == false )
            {
                rockToChatUsersStopwatch.Start();
                var chatAdminsResult = await chatHelper.SyncGroupMembersToChatProviderAsync( chatAminsGroup.Id );
                rockToChatUsersStopwatch.Stop();

                if ( chatAdminsResult != null )
                {
                    // Add any exceptions encountered at the channel member sync stage (should never happen within this call).
                    if ( chatAdminsResult.HasException )
                    {
                        rockToChatUsersExceptions.Add( chatAdminsResult.Exception );
                    }

                    // Add any exceptions encountered at the global chat user sync stage.
                    var chatAdminsUserExceptions = chatAdminsResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( chatAdminsUserExceptions.Any() )
                    {
                        rockToChatUsersExceptions.AddRange( chatAdminsUserExceptions );
                    }

                    // Aggregate the chat user CRUD results from the chat admins sync operation.
                    // There should be no deletions here.
                    rockToChatUsersResult.Skipped.UnionWith(
                        chatAdminsResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Created.UnionWith(
                        chatAdminsResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Updated.UnionWith(
                        chatAdminsResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );
                }
            }

            // ---------------------------------------------------
            // 1f) Sync all Rock group types to the chat provider.
            rockToChatStopwatch.Restart();

            var groupTypeService = new GroupTypeService( rockContext );
            var allGroupTypes = groupTypeService.Queryable().ToList();

            var rockToChatChannelTypesResult = await chatHelper.SyncGroupTypesToChatProviderAsync( allGroupTypes );

            rockToChatStopwatch.Stop();

            // It's possible for chat channels to have been deleted as a part of the group type sync, so we'll go ahead
            // and create a channels CRUD result here to begin tracking those deletions.
            var rockToChatChannelsResult = new ChatSyncCrudResult();
            var rockToChatChannelsExceptions = new List<Exception>();

            if ( rockToChatChannelTypesResult != null )
            {
                taskResult = CreateAndAddNewTaskResult( section, "Rock Group Types to Chat Channel Types", rockToChatStopwatch.Elapsed );

                AddCrudDetailsToTaskResult( taskResult, rockToChatChannelTypesResult, "Channel Type" );

                // Add any exceptions encountered at the channel type sync stage.
                if ( rockToChatChannelTypesResult.HasException == true )
                {
                    taskResult.Exception = rockToChatChannelTypesResult.Exception;
                }

                // Add any exceptions encountered at the channel deletion stage.
                var channelDeletionExceptions = rockToChatChannelTypesResult
                    .InnerResults
                    .OfType<ChatSyncCrudResult>()
                    .Where( r => r.HasException )
                    .Select( r => r.Exception )
                    .ToList();

                if ( channelDeletionExceptions.Any() )
                {
                    rockToChatChannelsExceptions.AddRange( channelDeletionExceptions );
                }

                // Aggregate the channel delete results from the group type sync operation.
                rockToChatChannelsResult.Deleted.UnionWith(
                    rockToChatChannelTypesResult
                        .InnerResults
                        .OfType<ChatSyncCrudResult>()
                        .SelectMany( r => r.Deleted )
                );
            }

            // -------------------------------------------------------
            // 1g) Sync all chat-enabled groups to the chat provider.

            rockToChatStopwatch.Restart();

            var groupService = new GroupService( rockContext );
            var chatEnabledGroupSyncCommands = groupService.GetChatEnabledGroupsQuery()
                .Select( g =>
                    new SyncGroupToChatCommand
                    {
                        GroupTypeId = g.GroupTypeId,
                        GroupId = g.Id,
                        ChatChannelKey = g.ChatChannelKey
                    }
                )
                .ToList();

            if ( chatEnabledGroupSyncCommands.Any() )
            {
                var syncConfig = new RockToChatGroupSyncConfig { ShouldSyncAllGroupMembers = true };

                var chatEnabledChannelCrudResult = await chatHelper.SyncGroupsToChatProviderAsync( chatEnabledGroupSyncCommands, syncConfig );
                if ( chatEnabledChannelCrudResult != null )
                {
                    // Add any exceptions encountered at the channel sync stage.
                    if ( chatEnabledChannelCrudResult.HasException )
                    {
                        rockToChatChannelsExceptions.Add( chatEnabledChannelCrudResult.Exception );
                    }

                    // Aggregate the channel CRUD results from the chat-enabled group sync operation.
                    // There should be no deletions here.
                    rockToChatChannelsResult.Skipped.UnionWith(
                        chatEnabledChannelCrudResult.Skipped
                    );

                    rockToChatChannelsResult.Created.UnionWith(
                        chatEnabledChannelCrudResult.Created
                    );

                    rockToChatChannelsResult.Updated.UnionWith(
                        chatEnabledChannelCrudResult.Updated
                    );

                    // Keep track of channel members synced during the chat-enabled group sync operation.
                    var rockToChatChannelMembersResult = new ChatSyncCrudResult();

                    // Add any exceptions encountered at the channel member sync stage.
                    var rockToChatChannelMembersExceptions = chatEnabledChannelCrudResult
                        .InnerResults
                        .OfType<ChatSyncCrudResult>()
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( rockToChatChannelMembersExceptions.Any() )
                    {
                        rockToChatChannelsExceptions.AddRange( rockToChatChannelMembersExceptions );
                    }

                    // Aggregate the channel member CRUD results from the chat-enabled group sync operation.
                    // There might be deletions here.
                    rockToChatChannelMembersResult.Skipped.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Skipped )
                    );

                    rockToChatChannelMembersResult.Created.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Created )
                    );

                    rockToChatChannelMembersResult.Updated.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Updated )
                    );

                    rockToChatChannelMembersResult.Deleted.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Deleted )
                    );

                    // Add any exceptions encountered at the channel member user sync stage.
                    var channelMemberUserExceptions = chatEnabledChannelCrudResult
                        .InnerResults
                        .OfType<ChatSyncCrudResult>()
                        .SelectMany( r => r.InnerResults )
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( channelMemberUserExceptions.Any() )
                    {
                        rockToChatUsersExceptions.AddRange( channelMemberUserExceptions );
                    }

                    // Aggregate the channel member user CRUD results from the chat-enabled group sync operation.
                    // There should be no deletions here.
                    rockToChatUsersResult.Skipped.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Created.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Updated.UnionWith(
                        chatEnabledChannelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatStopwatch.Stop();

                    taskResult = CreateAndAddNewTaskResult( section, "Rock Groups to Chat Channels", rockToChatStopwatch.Elapsed );

                    if ( rockToChatChannelsExceptions.Any() )
                    {
                        taskResult.Exception = ChatHelper.GetFirstOrAggregateException(
                            rockToChatChannelsExceptions,
                            "One or more exceptions occurred while syncing groups to chat channels."
                        );
                    }

                    AddCrudDetailsToTaskResult( taskResult, rockToChatChannelsResult, "Channel" );
                    AddCrudDetailsToTaskResult( taskResult, rockToChatChannelMembersResult, "Channel Member" );
                }
            }

            // -----------------------------------------------------------------------
            // 1h) Sync any non-deceased people who haven't already been synced above.

            rockToChatUsersStopwatch.Start();

            var chatUserPersonIds = personService.GetNonDeceasedChatUserPersonIdsQuery().ToList();

            // Filter down to those people who haven't already been synced above.
            var alreadySyncedPersonIds = rockToChatUsersResult.Unique.Select( a => a.AsInteger() ).ToList();
            var syncCommands = chatUserPersonIds
                .Where( personId => !alreadySyncedPersonIds.Contains( personId ) )
                .Select( personId =>
                    new SyncPersonToChatCommand
                    {
                        PersonId = personId,
                        ShouldEnsureChatAliasExists = true
                    }
                )
                .ToList();

            if ( syncCommands.Any() )
            {
                var createOrUpdateUsersResult = await chatHelper.CreateOrUpdateChatUsersAsync( syncCommands );
                if ( createOrUpdateUsersResult != null )
                {
                    // Add any exceptions encountered at the global chat user sync stage.
                    if ( createOrUpdateUsersResult.HasException )
                    {
                        rockToChatUsersExceptions.Add( createOrUpdateUsersResult.Exception );
                    }

                    // Aggregate the chat user CRUD results from the global chat user sync operation.
                    // There should be no deletions here.
                    rockToChatUsersResult.Skipped.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Created.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    rockToChatUsersResult.Updated.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );
                }
            }

            rockToChatUsersStopwatch.Stop();

            taskResult = CreateAndAddNewTaskResult( section, "Rock People to Chat Individuals", rockToChatUsersStopwatch.Elapsed );

            if ( rockToChatUsersExceptions.Any() )
            {
                taskResult.Exception = ChatHelper.GetFirstOrAggregateException(
                    rockToChatUsersExceptions,
                    "Exceptions occurred while syncing Rock People to Chat Individuals."
                );
            }

            AddCrudDetailsToTaskResult( taskResult, rockToChatUsersResult, "Chat Individual" );

            if ( globallyBannedChatUserKeys.Any() )
            {
                var count = globallyBannedChatUserKeys.Count;
                taskResult.Details.Add( $"{count:N0} {"Chat Individual".PluralizeIf( count > 1 )} Globally Banned" );
            }

            #endregion 1) Rock-to-Chat Sync

            #region 2) Chat-to-Rock Sync

            /*
                3/14/2025 - JPH

                There can only be one primary system of truth, and in this case, that's Rock. Since we've already
                synced Rock-to-Chat above, this secondary Chat-to-Rock sync process is for creating Rock groups and
                members for chat channels and members that Rock doesn't already know about. We won't go so far as to
                update or delete any already-existing Rock entities during this phase.

                Reason: Treat Rock as the primary system of truth, with the external chat system as the secondary.
             */

            var chatToRockStopwatch = new Stopwatch();

            section = CreateAndAddResultSection( "Chat-to-Rock Sync:" );

            // -----------------------------------
            // 2a) Create any missing Rock groups.

            taskResult = CreateAndAddNewTaskResult( section, "Chat Channels to Rock Groups", TimeSpan.Zero );

            chatToRockStopwatch.Restart();

            // Start by querying the external chat system for a complete list of chat channels.
            var getChatChannelsResult = await chatHelper.GetAllChatChannelsAsync();
            if ( getChatChannelsResult == null || getChatChannelsResult.HasException || !getChatChannelsResult.ChatChannels.Any() )
            {
                chatToRockStopwatch.Stop();

                taskResult.Elapsed = chatToRockStopwatch.Elapsed;
                taskResult.Details.Add( "0 Channels Retrieved" );

                // Add any exceptions encountered while getting external chat channels.
                if ( getChatChannelsResult?.HasException == true )
                {
                    taskResult.Exception = getChatChannelsResult.Exception;
                }
                else
                {
                    Log( LogLevel.Warning, "Unable to perform Chat-to-Rock sync when no Chat Channels were retrieved from the external chat system." );

                    taskResult.IsWarning = true;
                }

                // We have nothing more to do.
                return;
            }

            // Continue by getting existing Rock groups.
            var existingRockChatGroups = groupService.GetRockChatGroups() ?? new List<RockChatGroup>();

            // Collect any Rock groups along the way, whose members should be synced.
            var groupsRequiringMemberSync = new List<GroupCache>();

            // Collect any Stream channels along the way, that originated in Rock, yet somehow no longer have a Rock group representation.
            var channelQueryableKeysToDelete = new List<string>();

            var chatToRockGroupsResult = new ChatSyncCrudResult();
            var chatToRockGroupsExceptions = new List<Exception>();
            var chatToRockGroupsCommands = new List<SyncChatChannelToRockCommand>();

            // Spin through the external chat channels to determine if we need to create any Rock groups.
            foreach ( var chatChannel in getChatChannelsResult.ChatChannels )
            {
                // Find the Rock group matching this external chat channel.
                var existingRockChatGroup = existingRockChatGroups.FirstOrDefault( g => g.ChatChannelKey == chatChannel.Key );
                if ( existingRockChatGroup != null )
                {
                    // This channel already has a Rock group representation; no need to create it.
                    chatToRockGroupsResult.Skipped.Add( existingRockChatGroup.GroupId.ToString() );

                    // However, we will sync the members if it's currently chat-enabled.
                    if ( existingRockChatGroup.IsChatEnabled )
                    {
                        groupsRequiringMemberSync.Add( GroupCache.Get( existingRockChatGroup.GroupId ) );
                    }

                    continue;
                }

                if ( ChatHelper.DidChatChannelOriginateInRock( chatChannel.Key ) )
                {
                    // This means something unexpected happened in the upstream sync processes, as the channel should
                    // have already been deleted from the external chat system. We're not going to re-create it here,
                    // but we will delete it externally.
                    channelQueryableKeysToDelete.Add( chatChannel.QueryableKey );
                    continue;
                }

                var groupTypeId = ChatHelper.GetGroupTypeId( chatChannel.ChatChannelTypeKey );
                if ( !groupTypeId.HasValue )
                {
                    // Ignore channels that don't have a valid Rock group ID embedded within their chat channel type key.
                    continue;
                }

                // We have everything we need to create this group; add a sync command.
                var syncCommand = new SyncChatChannelToRockCommand( ChatSyncType.Create )
                {
                    AttemptLimit = 1,
                    GroupTypeId = groupTypeId,
                    ChatChannelKey = chatChannel.Key,
                    GroupName = chatChannel.Name,
                    IsActive = chatChannel.IsActive
                };

                chatToRockGroupsCommands.Add( syncCommand );
            }

            // Try to create any missing Rock groups.
            if ( chatToRockGroupsCommands.Any() )
            {
                var channelSyncCrudResult = chatHelper.SyncChatChannelsToRock( chatToRockGroupsCommands );
                if ( channelSyncCrudResult != null )
                {
                    // Add any exceptions encountered at the group sync stage.
                    if ( channelSyncCrudResult.HasException )
                    {
                        chatToRockGroupsExceptions.Add( channelSyncCrudResult.Exception );
                    }

                    // Aggregate the group CRUD results from this sync operation.
                    chatToRockGroupsResult.Skipped.UnionWith( channelSyncCrudResult.Skipped ); // Already existed (another process beat this job?)
                    chatToRockGroupsResult.Created.UnionWith( channelSyncCrudResult.Created ); // Newly-created

                    // Sync the members for any groups that were synced.
                    groupsRequiringMemberSync.AddRange(
                        channelSyncCrudResult.Unique.Select( id => GroupCache.Get( id.AsInteger() ) )
                    );
                }
            }

            // Try to delete any external chat channels that somehow still exist when they shouldn't.
            if ( channelQueryableKeysToDelete.Any() )
            {
                await chatHelper.DeleteChatChannelsAsync( channelQueryableKeysToDelete );
            }

            // ------------------------------------------
            // 2b) Create any missing Rock group members.

            var chatToRockGroupMembersResult = new ChatSyncCrudResult();
            var chatToRockGroupMemberCommands = new List<SyncChatChannelMemberToRockCommand>();

            foreach ( var groupCache in groupsRequiringMemberSync )
            {
                var groupId = groupCache.Id;
                var chatChannelKey = ChatHelper.GetChatChannelKey( groupId, groupCache.ChatChannelKey );
                var chatChannelTypeKey = ChatHelper.GetChatChannelTypeKey( groupCache.GroupTypeId );

                // Start by querying the external chat system for a list of channel members.
                var getChatChannelMembersResult = await chatHelper.GetChatChannelMembersAsync( chatChannelTypeKey, chatChannelKey );
                if ( getChatChannelMembersResult == null )
                {
                    Log( LogLevel.Warning, $"Unable to preform Chat-to-Rock sync when no Chat Channel Members response was received from the external chat system for channel with key = '{chatChannelKey}'." );
                    continue;
                }

                // Add any exceptions encountered while getting external channel members.
                if ( getChatChannelMembersResult.HasException )
                {
                    chatToRockGroupsExceptions.Add( getChatChannelMembersResult.Exception );
                    continue;
                }

                var channelMembersWithKeys = getChatChannelMembersResult.ChatChannelMembers
                    .Where( m => m?.ChatUserKey.IsNotNullOrWhiteSpace() == true )
                    .ToList();

                if ( channelMembersWithKeys.Any() != true )
                {
                    // No group members to create.
                    continue;
                }

                foreach ( var channelMember in channelMembersWithKeys )
                {
                    var chatRole = EnumExtensions.ConvertToEnumOrNull<ChatRole>( channelMember.Role );

                    // We have everything we need to create this member; add a sync command.
                    var syncCommand = new SyncChatChannelMemberToRockCommand( ChatSyncType.Create )
                    {
                        AttemptLimit = 1,
                        GroupId = groupId,
                        ChatChannelKey = chatChannelKey,
                        ChatPersonKey = channelMember.ChatUserKey,
                        ChatRole = chatRole,
                        IsBanned = channelMember.IsChatBanned,
                        IsMuted = channelMember.IsChatMuted
                    };

                    chatToRockGroupMemberCommands.Add( syncCommand );
                }
            }

            // Try to create any missing Rock group members.
            if ( chatToRockGroupMemberCommands.Any() )
            {
                var channelMemberCrudResult = chatHelper.SyncChatChannelMembersToRock( chatToRockGroupMemberCommands );
                if ( channelMemberCrudResult != null )
                {
                    // Add any exceptions encountered at the group member sync stage.
                    if ( channelMemberCrudResult.HasException )
                    {
                        chatToRockGroupsExceptions.Add( channelMemberCrudResult.Exception );
                    }

                    // Aggregate the group member CRUD results from this sync operation.
                    chatToRockGroupMembersResult.Skipped.UnionWith( channelMemberCrudResult.Skipped ); // Already existed
                    chatToRockGroupMembersResult.Created.UnionWith( channelMemberCrudResult.Created ); // Newly-created
                }
            }

            chatToRockStopwatch.Stop();
            taskResult.Elapsed = chatToRockStopwatch.Elapsed;

            if ( chatToRockGroupsExceptions.Any() )
            {
                taskResult.Exception = ChatHelper.GetFirstOrAggregateException(
                    chatToRockGroupsExceptions,
                    "Exceptions occurred while syncing Chat data to Rock."
                );
            }

            AddCrudDetailsToTaskResult( taskResult, chatToRockGroupsResult, "Group", CrudMessage.Present );
            AddCrudDetailsToTaskResult( taskResult, chatToRockGroupMembersResult, "Group Member", CrudMessage.Present );

            #endregion 2) Chat-to-Rock Sync
        }

        #endregion Synchronize Data

        #region Create Interactions

        /// <summary>
        /// Creates chat-related interactions for people who have been active in the external chat system.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="chatHelper">The chat helper.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateInteractionsAsync( RockContext rockContext, ChatHelper chatHelper )
        {
            var section = CreateAndAddResultSection( "Chat Message Interactions:" );

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var settings = Rock.Web.SystemSettings
                .GetValue( SystemSetting.CHAT_SYNC_JOB_SETTINGS )
                .FromJsonOrNull<ChatSyncJobSettings>() ?? new ChatSyncJobSettings();

            DateTime messageDate; // The next date to process.
            var minimumMessageDate = RockDateTime.Today.AddDays( -5 );
            var lastSuccessfulMessageDate = settings.LastSuccessfulMessageInteractionsDate;

            if ( lastSuccessfulMessageDate.HasValue )
            {
                // Increment one day from the last successful run, ensuring we're always working with only dates (and not times).
                lastSuccessfulMessageDate = lastSuccessfulMessageDate.Value.Date;
                messageDate = lastSuccessfulMessageDate.Value.AddDays( 1 );

                // While ensuring we don't go further in the past than the minimum date.
                if ( messageDate < minimumMessageDate )
                {
                    messageDate = minimumMessageDate;
                }
            }
            else
            {
                messageDate = minimumMessageDate;
            }

            // A local function to get an "Interaction(s)" label.
            string GetInteractionsLabel( int recordCount )
            {
                return $"{"Interaction".PluralizeIf( recordCount != 1 )}";
            }

            // We only create these types of interactions through "yesterday".
            if ( messageDate >= RockDateTime.Today )
            {
                stopwatch.Stop();

                CreateAndAddNewTaskResult( section, CrudMessage.Skipped, stopwatch.Elapsed );
                return;
            }

            var channelTypeMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHAT.AsGuid(), rockContext )?.Id;
            var componentEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP.AsGuid(), rockContext )?.Id;

            var interactionChannelId = InteractionChannelCache.GetOrCreateChannelIdByName(
                channelTypeMediumValueId.Value,
                channelName: "Chat",
                componentEntityTypeId,
                interactionEntityTypeId: null
            );

            var interactionService = new InteractionService( rockContext );

            // -----------------------------------------------------------------------------------------
            // 2a) Delete untrusted interactions from previous, unsuccessful runs (we'll recreate them).
            if ( lastSuccessfulMessageDate.HasValue )
            {
                stopwatch.Restart();

                var deleteInteractionsQry = interactionService.Queryable()
                    .Where( i =>
                        i.InteractionComponent.InteractionChannel.Id == interactionChannelId
                        && i.InteractionDateTime > lastSuccessfulMessageDate.Value
                    );

                var deletedCount = rockContext.BulkDelete( deleteInteractionsQry );

                stopwatch.Stop();

                if ( deletedCount > 0 )
                {
                    CreateAndAddNewTaskResult( section, $"{deletedCount} {GetInteractionsLabel( deletedCount )} {CrudMessage.Deleted}", stopwatch.Elapsed );
                }
            }

            // Get the [chat channel key]-to-[Rock group ID & name] mappings.
            var rockChatGroups = new GroupService( rockContext ).GetRockChatGroups();
            if ( rockChatGroups?.Any() != true )
            {
                var taskResult = CreateAndAddNewTaskResult( section, "Missing Chat-to-Rock Mapping Data", TimeSpan.Zero );
                taskResult.IsWarning = true;

                Log( LogLevel.Warning, "No Chat Channel-to-Rock Group ID mappings found. Unable to create Interactions, as we won't be able to map a given Chat Channel back to a Rock Group." );

                // There's no reason to continue if we have no mappings.
                return;
            }

            // Get the [chat user key]-to-[Rock person alias ID] mappings.
            var personAliasIdByChatUserKeys = new PersonAliasService( rockContext ).GetChatPersonAliasIdByChatUserKeys();
            if ( personAliasIdByChatUserKeys?.Any() != true )
            {
                var taskResult = CreateAndAddNewTaskResult( section, "Missing Chat-to-Rock Mapping Data", TimeSpan.Zero );
                taskResult.IsWarning = true;

                Log( LogLevel.Warning, "No Chat Individual Key-to-Rock Person Alias ID mappings found. Unable to create Interactions, as we won't be able to map a given Chat Channel back to a Rock Group." );

                // There's no reason to continue if we have no mappings.
                return;
            }

            // A local function to get a formatted message for the interactions created.
            string GetInteractionsCreatedMessage( int count, DateTime interactionDate )
            {
                return $"{count:N0} {GetInteractionsLabel( count )} {CrudMessage.Created} for {interactionDate:d}";
            }

            // -----------------------------------------------
            // 2b) Create new interactions, one day at a time.
            while ( messageDate < RockDateTime.Today )
            {
                stopwatch.Restart();

                // A local function to save the current date to job settings and increment to the next day.
                void MarkDateAsSuccessfullyProcessed()
                {
                    settings.LastSuccessfulMessageInteractionsDate = messageDate;
                    Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_SYNC_JOB_SETTINGS, settings.ToJson() );

                    // Move on to the next day.
                    messageDate = messageDate.AddDays( 1 );
                }

                // Get message counts from the external chat system for the given date.
                var messageCountsResult = await chatHelper.GetChatUserMessageCountsByChatChannelKeyAsync( messageDate );
                if ( messageCountsResult == null )
                {
                    stopwatch.Stop();

                    Log( LogLevel.Warning, $"`ChatHelper.GetChatUserMessageCountsByChatChannelKeyAsync( {messageDate:d} )` returned `null`." );

                    var taskResult = CreateAndAddNewTaskResult( section, GetInteractionsCreatedMessage( 0, messageDate ), stopwatch.Elapsed );
                    taskResult.IsWarning = true;

                    // Return from this method altogether, so we can try again next time.
                    return;
                }
                else if ( messageCountsResult.HasException )
                {
                    stopwatch.Stop();

                    var taskResult = CreateAndAddNewTaskResult( section, GetInteractionsCreatedMessage( 0, messageDate ), stopwatch.Elapsed );
                    taskResult.Exception = messageCountsResult.Exception;

                    // Return from this method altogether, so we can try again next time.
                    return;
                }
                else if ( messageCountsResult.MessageCounts.Any() != true )
                {
                    stopwatch.Stop();

                    CreateAndAddNewTaskResult( section, GetInteractionsCreatedMessage( 0, messageDate ), stopwatch.Elapsed );
                    MarkDateAsSuccessfullyProcessed();

                    continue;
                }

                var interactionsToAdd = new List<Interaction>();
                var chatChannelKeysSkipped = new HashSet<string>();
                var chatUserKeysSkipped = new HashSet<string>();

                foreach ( var channelMessageCountsKvp in messageCountsResult.MessageCounts )
                {
                    var chatChannelKey = channelMessageCountsKvp.Key;
                    var chatUserMessageCounts = channelMessageCountsKvp.Value;

                    if ( chatChannelKey.IsNullOrWhiteSpace() || chatUserMessageCounts?.Any() != true )
                    {
                        continue;
                    }

                    // Find the rock chat group.
                    var rockChatGroup = rockChatGroups.FirstOrDefault( g => g.ChatChannelKey == chatChannelKey );
                    if ( rockChatGroup == null )
                    {
                        if ( chatChannelKeysSkipped.Add( chatChannelKey ) )
                        {
                            Log( LogLevel.Warning, $"No Rock Group found for Chat Channel Key '{chatChannelKey}'. Unable to create Interactions for this Chat Channel." );
                        }

                        continue;
                    }

                    // Ensure we have an interaction component for this group (while lumping all DMs into a single component).
                    var componentName = rockChatGroup.GroupTypeId == ChatHelper.ChatDirectMessageGroupTypeId
                        ? ChatHelper.ChatDirectMessageGroupName
                        : rockChatGroup.Name;

                    var interactionComponentId = InteractionComponentCache.GetOrCreateComponentIdByName(
                        interactionChannelId,
                        componentName
                    );

                    foreach ( var chatUserMessageCountsKvp in chatUserMessageCounts )
                    {
                        var chatUserKey = chatUserMessageCountsKvp.Key;
                        var messageCount = chatUserMessageCountsKvp.Value;

                        if ( chatUserKey.IsNullOrWhiteSpace() || messageCount <= 0 )
                        {
                            continue;
                        }

                        // Find the person alias ID.
                        if ( !personAliasIdByChatUserKeys.TryGetValue( chatUserKey, out var personAliasId ) )
                        {
                            if ( chatUserKeysSkipped.Add( chatUserKey ) )
                            {
                                Log( LogLevel.Warning, $"No Rock Person Alias ID found for Chat Individual Key '{chatUserKey}'. Unable to create Interactions for this Chat Individual." );
                            }

                            continue;
                        }

                        // Add the interaction.
                        interactionsToAdd.Add( new Interaction
                        {
                            InteractionDateTime = messageDate,
                            Operation = "Chatted",
                            InteractionComponentId = interactionComponentId,
                            PersonAliasId = personAliasId,
                            InteractionSummary = messageCount.ToString(),
                        } );
                    }
                }

                if ( interactionsToAdd.Any() )
                {
                    rockContext.BulkInsert( interactionsToAdd );
                }

                stopwatch.Stop();

                var createInteractionsTaskResult = CreateAndAddNewTaskResult( section, GetInteractionsCreatedMessage( interactionsToAdd.Count, messageDate ), stopwatch.Elapsed );

                if ( chatChannelKeysSkipped.Any() )
                {
                    createInteractionsTaskResult.Details.Add( $"{chatChannelKeysSkipped.Count:N0} Chat {"Channel".PluralizeIf( chatChannelKeysSkipped.Count > 1 )} Skipped" );
                    createInteractionsTaskResult.IsWarning = true;
                }

                if ( chatUserKeysSkipped.Any() )
                {
                    createInteractionsTaskResult.Details.Add( $"{chatUserKeysSkipped.Count:N0} Chat {"Person".PluralizeIf( chatUserKeysSkipped.Count > 1 )} Skipped" );
                    createInteractionsTaskResult.IsWarning = true;
                }

                MarkDateAsSuccessfullyProcessed();
            }
        }

        #endregion Create Interactions

        #region Delete Merged Chat Users

        /// <summary>
        /// Deletes non-prevailing chat individuals that have been merged with other Rock people.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="chatHelper">The chat helper.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DeleteMergedChatUsersAsync( RockContext rockContext, ChatHelper chatHelper )
        {
            var section = CreateAndAddResultSection( "Delete Merged Chat Individuals:" );

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var mergeResult = await chatHelper.DeleteMergedChatUsersAsync();

            stopwatch.Stop();

            if ( mergeResult?.Deleted.Any() != true )
            {
                CreateAndAddNewTaskResult( section, CrudMessage.Skipped, stopwatch.Elapsed );
                return;
            }

            var count = mergeResult.Deleted.Count;
            CreateAndAddNewTaskResult( section, $"{count:N0} Merged Chat Individual {"Record".PluralizeIf( count > 1 )} {CrudMessage.Deleted}", stopwatch.Elapsed );
        }

        #endregion Delete Merged Chat Users

        #region Task Result Reporting

        /// <summary>
        /// Creates and adds a new <see cref="ChatSyncResultSection"/> to the <see cref="_resultSections"/> list.
        /// </summary>
        /// <param name="title">The title of the result section.</param>
        /// <returns>The created <see cref="ChatSyncResultSection"/>.</returns>
        private ChatSyncResultSection CreateAndAddResultSection( string title )
        {
            var resultSection = new ChatSyncResultSection
            {
                Title = title
            };

            _resultSections.Add( resultSection );

            return resultSection;
        }

        /// <summary>
        /// Adds a new <see cref="ChatSyncTaskResult"/> to the provided <see cref="ChatSyncResultSection"/>.
        /// </summary>
        /// <param name="section">The result section.</param>
        /// <param name="title">The title of the task result.</param>
        /// <param name="elapsed">The time elapsed for the task.</param>
        /// <param name="rowsAffected">The number of rows affected by the task.</param>
        /// <returns>The created <see cref="ChatSyncTaskResult"/>.</returns>
        private ChatSyncTaskResult CreateAndAddNewTaskResult( ChatSyncResultSection section, string title, TimeSpan elapsed, int rowsAffected = 0 )
        {
            var taskResult = new ChatSyncTaskResult
            {
                Title = title,
                Elapsed = elapsed,
                RowsAffected = rowsAffected
            };

            section.Results.Add( taskResult );

            return taskResult;
        }

        /// <summary>
        /// Adds the details of a <see cref="ChatSyncCrudResult"/> to a <see cref="ChatSyncTaskResult"/>.
        /// </summary>
        /// <param name="taskResult">The task result.</param>
        /// <param name="crudResult">The CRUD result.</param>
        /// <param name="entityName">The name of the entity being synced.</param>
        /// <param name="skippedMessage">The message to display for skipped entities.</param>
        private void AddCrudDetailsToTaskResult( ChatSyncTaskResult taskResult, ChatSyncCrudResult crudResult, string entityName, string skippedMessage = CrudMessage.Skipped )
        {
            /*
                2/26/2025 - JPH

                A given entity might appear in more than one of these counts (e.g. if a chat user is created as a member
                of one channel and then skipped for a subsequent update within the same sync process, when they actually
                belong to more than one channel). Rather than worry about reporting exactly-accurate "final state" entity
                counts per CRUD action, we'll simply report the total counts of actions that were actually performed. If
                anyone cares to point out these reporting overlaps, we can try to improve the counts at that time.

                Reason: report reasonably-accurate CRUD counts.
             */

            if ( crudResult?.Skipped.Count > 0 )
            {
                var count = crudResult.Skipped.Count;
                taskResult.Details.Add( $"{count:N0} {entityName.PluralizeIf( count > 1 )} {skippedMessage}" );
            }

            if ( crudResult?.Created.Count > 0 )
            {
                var count = crudResult.Created.Count;
                taskResult.Details.Add( $"{count:N0} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Created}" );
            }

            if ( crudResult?.Updated.Count > 0 )
            {
                var count = crudResult.Updated.Count;
                taskResult.Details.Add( $"{count:N0} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Updated}" );
            }

            if ( crudResult?.Deleted.Count > 0 )
            {
                var count = crudResult.Deleted.Count;
                taskResult.Details.Add( $"{count:N0} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Deleted}" );
            }
        }

        /// <summary>
        /// Reports the results of this job run.
        /// </summary>
        /// <param name="showWarningInstructions">Whether to show further instructions in the case of warnings.</param>
        /// <exception cref="RockJobWarningException">If any <see cref="ChatSyncTaskResult"/> has an <see cref="Exception"/>.</exception>
        private void ReportResults( bool showWarningInstructions = true )
        {
            var jobSummaryBuilder = new StringBuilder();

            var index = 0;
            foreach ( var section in _resultSections )
            {
                if ( index++ > 0 )
                {
                    // Always introduce an empty line between sections.
                    jobSummaryBuilder.AppendLine( string.Empty );
                }

                if ( section.Title.IsNotNullOrWhiteSpace() )
                {
                    jobSummaryBuilder.AppendLine( section.Title );
                    jobSummaryBuilder.AppendLine( string.Empty );
                }

                foreach ( var result in section.Results )
                {
                    jobSummaryBuilder.AppendLine( GetFormattedResult( result ) );
                }
            }

            var exceptions = _resultSections
                .SelectMany( s => s.Results )
                .Where( r => r.HasException )
                .Select( r => r.Exception )
                .ToList();

            var anyWarnings = _resultSections
                .SelectMany( s => s.Results )
                .Where( r => r.IsWarning )
                .Any();

            if ( exceptions.Any() )
            {
                jobSummaryBuilder.AppendLine( string.Empty );
                jobSummaryBuilder.AppendLine( "<i class='fa fa-circle text-danger'></i> Some tasks have errors. View Rock's Exception List for more details. You can also enable 'Error' verbosity level for 'Chat' domains in Rock Logs and re-run this job to get a full list of issues." );
            }
            else if ( anyWarnings && showWarningInstructions )
            {
                jobSummaryBuilder.AppendLine( string.Empty );
                jobSummaryBuilder.AppendLine( "<i class='fa fa-circle text-warning'></i> Some tasks completed with warnings. Enable 'Warning' verbosity level for all 'Chat' domains in Rock Logs and re-run this job to get a full list of issues." );
            }

            this.Result = jobSummaryBuilder.ToString();

            if ( exceptions.Any() )
            {
                var jobName = nameof( ChatSync );
                var innerException = ChatHelper.GetFirstOrAggregateException( exceptions, $"Exceptions occurred in {jobName}." );

                throw new RockJobWarningException( $"{jobName} completed with errors.", innerException );
            }
        }

        /// <summary>
        /// Gets the formatted result for a chat sync task result.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetFormattedResult( ChatSyncTaskResult result )
        {
            var rowsAffected = result.RowsAffected > 0
                ? $"{result.RowsAffected:N0} "
                : string.Empty;

            var elapsed = result.Elapsed.TotalMilliseconds > 0
                ? $" ({result.Elapsed.TotalMilliseconds:N0}ms)"
                : string.Empty;

            var title = $"{rowsAffected}{result.Title}{elapsed}";

            var formattedResultSb = new StringBuilder();

            if ( result.HasException )
            {
                formattedResultSb.Append( $"<i class='fa fa-circle text-danger'></i> {title}" );
            }
            else if ( result.IsWarning )
            {
                formattedResultSb.Append( $"<i class='fa fa-circle text-warning'></i> {title}" );
            }
            else
            {
                formattedResultSb.Append( $"<i class='fa fa-circle text-success'></i> {title}" );
            }

            var iconSpacer = "<span style='visibility:hidden'><i class='fa fa-circle'></i></span>";
            foreach ( var detail in result.Details?.Where( d => d.IsNotNullOrWhiteSpace() ) )
            {
                formattedResultSb.AppendLine( string.Empty );
                formattedResultSb.Append( $"{iconSpacer} {detail}" );
            }

            return formattedResultSb.ToString();
        }

        /// <summary>
        /// If there are Task.Runs that don't handle their exceptions, this will catch those so that we can log them.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void TaskScheduler_UnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
        {
            ExceptionLogService.LogException( new RockJobWarningException( $"Unobserved Task Exception in {nameof( ChatSync )} Job.", e.Exception ) );
        }

        #endregion Task Result Reporting

        #endregion Private Methods

        #region Supporting Members

        /// <summary>
        /// A configuration object to store progress and drive behavior for the <see cref="ChatSync"/> job.
        /// </summary>
        private class ChatSyncJobSettings
        {
            /// <summary>
            /// Gets or sets the last date for which message interactions were successfully created.
            /// </summary>
            public DateTime? LastSuccessfulMessageInteractionsDate { get; set; }
        }

        /// <summary>
        /// The result data from a grouped set of <see cref="ChatSyncTaskResult"/>s.
        /// </summary>
        private class ChatSyncResultSection
        {
            /// <summary>
            /// Gets or sets the title for this section.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets the <see cref="ChatSyncTaskResult"/>s for this section.
            /// </summary>
            public List<ChatSyncTaskResult> Results { get; } = new List<ChatSyncTaskResult>();
        }

        /// <summary>
        /// The result data from a chat sync task.
        /// </summary>
        private class ChatSyncTaskResult
        {
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets the details.
            /// </summary>
            public List<string> Details { get; } = new List<string>();

            /// <summary>
            /// Gets or sets the number of rows affected.
            /// </summary>
            public int RowsAffected { get; set; }

            /// <summary>
            /// Gets or sets the elapsed time.
            /// </summary>
            public TimeSpan Elapsed { get; set; }

            /// <summary>
            /// Gets or sets whether this result is a warning.
            /// </summary>
            public bool IsWarning { get; set; }

            /// <summary>
            /// Gets or sets the exception.
            /// </summary>
            public Exception Exception { get; set; }

            /// <summary>
            /// Gets whether this result has an exception.
            /// </summary>
            public bool HasException => Exception != null;
        }

        #endregion Supporting Members
    }
}
