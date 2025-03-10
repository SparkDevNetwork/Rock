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

using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Communication.Chat.Sync;
using Rock.Data;
using Rock.Enums.Communication.Chat;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Performs synchronization tasks between Rock and the external chat system.
    /// </summary>
    [DisplayName( "Chat Sync" )]
    [Description( "Performs synchronization tasks between Rock and the external chat system." )]

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
        Description = "Determines if non-prevailing, merged chat person records should be deleted in the external chat system. If enabled, when two people in Rock have been merged, and both had an associated chat person record, the non-prevailing chat person record will be deleted from the external chat system to ensure other people can send future messages to only the prevailing chat person record.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField( "Enforce Default Grants Per Role",
        Key = AttributeKey.EnforceDefaultGrantsPerRole,
        Description = "This is an experimental setting that will be removed in a future version of Rock. If enabled, will overwrite all permission grants (per role) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these permissions.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField( "Enforce Default Sync Settings",
        Key = AttributeKey.EnforceDefaultSyncSettings,
        Description = "This is an experimental setting that will be removed in a future version of Rock. If enabled, will overwrite all settings (e.g. channel type and channel settings) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these settings.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 5 )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60,
        Category = "General",
        Order = 6 )]

    public class ChatSync : RockJob
    {
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

            public const string GloballyBanned = "Globally Banned";
        }

        /// <summary>
        /// The list of chat sync task results.
        /// </summary>
        private readonly List<ChatSyncTaskResult> _results = new List<ChatSyncTaskResult>();

        /// <summary>
        /// Empty constructor for job initialization.
        /// </summary>
        /// <remarks>
        /// Jobs require a public empty constructor so that the scheduler can instantiate the class whenever it needs.
        /// </remarks>
        public ChatSync()
        {
        }

        /// <inheritdoc/>
        public override void Execute()
        {
            // If chat is not enabled, exit early.
            if ( !ChatHelper.IsChatEnabled )
            {
                _results.Add( new ChatSyncTaskResult
                {
                    Title = "Chat is not enabled."
                } );

                ReportResults();
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
                        chatHelper.RockToChatSyncConfig.ShouldEnforceDefaultGrantsPerRole = GetAttributeValue( AttributeKey.EnforceDefaultGrantsPerRole ).AsBoolean();
                        chatHelper.RockToChatSyncConfig.ShouldEnforceDefaultSettings = GetAttributeValue( AttributeKey.EnforceDefaultSyncSettings ).AsBoolean();
                        chatHelper.RockToChatSyncConfig.ShouldEnsureChatUsersExist = true;

                        await SyncDataFromRockToChat( rockContext, chatHelper );
                    }
                }
            } );

            // We need to wait for this task to complete so we can report the results.
            syncTask.Wait();

            ReportResults();
        }

        /// <summary>
        /// Ensures that all chat-related data in Rock is in sync with the corresponding data in the external chat system.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="chatHelper">The chat helper.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SyncDataFromRockToChat( RockContext rockContext, ChatHelper chatHelper )
        {
            var stopwatch = new Stopwatch();

            // We'll keep track of chat users that are synced along the way, so we ensure each person is fully-synced
            // all the way through to the external chat system ONLY ONCE throughout this job run.
            var chatUserStopwatch = new Stopwatch();
            var chatUserCrudResult = new ChatSyncCrudResult();
            var chatUserExceptions = new List<Exception>();

            // --------------------------------------------------------
            // 1) Ensure the app is set up in the external chat system.
            stopwatch.Start();
            var isSetUpResult = await chatHelper.EnsureChatProviderAppIsSetUpAsync();
            stopwatch.Stop();

            var taskResult = CreateAndAddNewTaskResult( "Sync Chat System Settings", stopwatch.Elapsed );

            // If the setup operation failed, we can't continue with the other tasks.
            if ( isSetUpResult?.HasException == true )
            {
                taskResult.Exception = isSetUpResult.Exception;
                return;
            }
            else if ( isSetUpResult?.WasSuccessful != true )
            {
                taskResult.IsWarning = true;
                return;
            }

            // --------------------------------------------------------------------------
            // 2) Add/[re]enforce global chat bans by syncing "Chat Ban List" chat users.

            var globallyBannedChatUserKeys = new HashSet<string>();

            chatUserStopwatch.Start();
            var chatBanListCrudResult = await chatHelper.SyncGroupMembersToChatProviderAsync( ChatHelper.ChatBanListGroupId );
            chatUserStopwatch.Stop();

            if ( chatBanListCrudResult != null )
            {
                if ( chatBanListCrudResult.HasException )
                {
                    chatUserExceptions.Add( chatBanListCrudResult.Exception );
                }

                // Add any exceptions encountered at the user sync stage.
                var chatBanListUserExceptions = chatBanListCrudResult
                    .InnerResults
                    .OfType<ChatSyncCreateOrUpdateUsersResult>()
                    .Where( r => r.HasException )
                    .Select( r => r.Exception )
                    .ToList();

                if ( chatBanListUserExceptions.Any() )
                {
                    chatUserExceptions.AddRange( chatBanListUserExceptions );
                }

                // Aggregate the chat user CRUD results from the "Chat Ban List" sync operation.
                chatUserCrudResult.Skipped.UnionWith(
                    chatBanListCrudResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                        .Select( r => r.PersonId.ToString() )
                );

                chatUserCrudResult.Created.UnionWith(
                    chatBanListCrudResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                        .Select( r => r.PersonId.ToString() )
                );

                chatUserCrudResult.Updated.UnionWith(
                    chatBanListCrudResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .SelectMany( r => r.UserResults )
                        .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                        .Select( r => r.PersonId.ToString() )
                );

                // Aggregate the chat user ban results from the "Chat Ban List" sync operation. Note that global bans
                // could only have been added/[re]enforced by this process; not lifted/removed.
                globallyBannedChatUserKeys.UnionWith(
                    chatBanListCrudResult
                        .InnerResults
                        .OfType<ChatSyncBanResult>()
                        .SelectMany( r => r.Banned )
                );
            }

            // ------------------------------------------------------------
            // 3) Sync "APP - Chat Administrator" security role chat users.

            // Only perform this sync if chat is NOT enabled for the chat admins group. Otherwise, these chat users will
            // be synced as part of the regular group sync process below.
            var chatAminsGroup = GroupCache.Get( ChatHelper.ChatAdministratorsGroupId );
            if ( chatAminsGroup?.GetIsChatEnabled() == false )
            {
                chatUserStopwatch.Start();
                var chatAdminsCrudResult = await chatHelper.SyncGroupMembersToChatProviderAsync( chatAminsGroup.Id );
                chatUserStopwatch.Stop();

                if ( chatAdminsCrudResult != null )
                {
                    if ( chatAdminsCrudResult.HasException )
                    {
                        chatUserExceptions.Add( chatAdminsCrudResult.Exception );
                    }

                    // Add any exceptions encountered at the user sync stage.
                    var chatAdminsUserExceptions = chatAdminsCrudResult
                        .InnerResults
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( chatAdminsUserExceptions.Any() )
                    {
                        chatUserExceptions.AddRange( chatAdminsUserExceptions );
                    }

                    // Aggregate the chat user CRUD results from the chat admins sync operation.
                    chatUserCrudResult.Skipped.UnionWith(
                        chatAdminsCrudResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Created.UnionWith(
                        chatAdminsCrudResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Updated.UnionWith(
                        chatAdminsCrudResult
                            .InnerResults
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );
                }
            }

            // --------------------------------------------------
            // 4) Sync all Rock group types to the chat provider.
            stopwatch.Restart();

            var groupTypeService = new GroupTypeService( rockContext );
            var allGroupTypes = groupTypeService.Queryable().ToList();

            var channelTypeCrudResult = await chatHelper.SyncGroupTypesToChatProviderAsync( allGroupTypes );
            stopwatch.Stop();

            if ( channelTypeCrudResult != null )
            {
                taskResult = CreateAndAddNewTaskResult( "Sync Group Types to Chat Channel Types", stopwatch.Elapsed );
                AddCrudDetailsToTaskResult( taskResult, channelTypeCrudResult, "Channel Type" );

                if ( channelTypeCrudResult.HasException == true )
                {
                    taskResult.Exception = channelTypeCrudResult.Exception;
                }
            }

            // ------------------------------------------------------
            // 5) Sync all chat-enabled groups to the chat provider.
            stopwatch.Restart();

            var groupService = new GroupService( rockContext );
            var chatEnabledGroups = groupService.GetChatEnabled().ToList();

            if ( chatEnabledGroups.Any() )
            {
                var syncConfig = new RockToChatGroupSyncConfig { ShouldSyncAllGroupMembers = true };

                var channelCrudResult = await chatHelper.SyncGroupsToChatProviderAsync( chatEnabledGroups, syncConfig );

                if ( channelCrudResult != null )
                {
                    // Keep track of group members and exceptions encountered during this phase of the job run.
                    var channelMemberCrudResult = new ChatSyncCrudResult();
                    var channelExceptions = new List<Exception>();

                    if ( channelCrudResult.HasException )
                    {
                        channelExceptions.Add( channelCrudResult.Exception );
                    }

                    // Add any exceptions encountered at the channel member sync stage.
                    var channelMemberExceptions = channelCrudResult
                        .InnerResults
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( channelMemberExceptions.Any() )
                    {
                        channelExceptions.AddRange( channelMemberExceptions );
                    }

                    // Add any exceptions encountered at the channel member user sync stage.
                    var channelMemberUserExceptions = channelCrudResult
                        .InnerResults
                        .OfType<ChatSyncCrudResult>()
                        .SelectMany( r => r.InnerResults )
                        .OfType<ChatSyncCreateOrUpdateUsersResult>()
                        .Where( r => r.HasException )
                        .Select( r => r.Exception )
                        .ToList();

                    if ( channelMemberUserExceptions.Any() )
                    {
                        chatUserExceptions.AddRange( channelMemberUserExceptions );
                    }

                    // Aggregate the channel member CRUD results from the channel sync operation.
                    channelMemberCrudResult.Skipped.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Skipped )
                    );

                    channelMemberCrudResult.Created.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Created )
                    );

                    channelMemberCrudResult.Updated.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Updated )
                    );

                    channelMemberCrudResult.Deleted.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.Deleted )
                    );

                    // Aggregate the chat user CRUD results from the channel[member] sync operations.
                    chatUserCrudResult.Skipped.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Created.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Updated.UnionWith(
                        channelCrudResult
                            .InnerResults
                            .OfType<ChatSyncCrudResult>()
                            .SelectMany( r => r.InnerResults )
                            .OfType<ChatSyncCreateOrUpdateUsersResult>()
                            .SelectMany( r => r.UserResults )
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );

                    stopwatch.Stop();

                    taskResult = CreateAndAddNewTaskResult( "Sync Groups to Chat Channels", stopwatch.Elapsed );

                    if ( channelExceptions.Any() )
                    {
                        taskResult.Exception = channelExceptions.Count == 1
                            ? channelExceptions.First()
                            : new AggregateException( "One or more exceptions occurred while syncing groups to chat channels.", channelExceptions );
                    }

                    AddCrudDetailsToTaskResult( taskResult, channelCrudResult, "Channel" );
                    AddCrudDetailsToTaskResult( taskResult, channelMemberCrudResult, "Channel Member" );
                }
            }

            // ---------------------------------------------------------
            // 6) Sync any people who haven't already been synced above.

            chatUserStopwatch.Start();

            var personService = new PersonService( rockContext );
            var chatUserPersonIds = personService.GetChatUserPersonIds().ToList();

            // Filter down to those people who haven't already been synced above.
            var alreadySyncedPersonIds = chatUserCrudResult.Unique.Select( a => a.AsInteger() ).ToList();
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
                    if ( createOrUpdateUsersResult.HasException )
                    {
                        chatUserExceptions.Add( createOrUpdateUsersResult.Exception );
                    }

                    // Aggregate the chat user CRUD results from this last person sync operation.
                    chatUserCrudResult.Skipped.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Skip )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Created.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Create )
                            .Select( r => r.PersonId.ToString() )
                    );

                    chatUserCrudResult.Updated.UnionWith(
                        createOrUpdateUsersResult.UserResults
                            .Where( r => r.SyncTypePerformed == ChatSyncType.Update )
                            .Select( r => r.PersonId.ToString() )
                    );
                }
            }

            chatUserStopwatch.Stop();
            taskResult = CreateAndAddNewTaskResult( "Sync People to Chat Person Records", chatUserStopwatch.Elapsed );

            if ( chatUserExceptions.Any() )
            {
                taskResult.Exception = chatUserExceptions.Count == 1
                    ? chatUserExceptions.First()
                    : new AggregateException( "One or more exceptions occurred while syncing people to chat person records.", chatUserExceptions );
            }

            AddCrudDetailsToTaskResult( taskResult, chatUserCrudResult, "Chat User" );

            if ( globallyBannedChatUserKeys.Any() )
            {
                var count = globallyBannedChatUserKeys.Count;
                taskResult.Details.Add( $"{count} {"Chat User".PluralizeIf( count > 1 )} {CrudMessage.GloballyBanned}" );
            }
        }

        /// <summary>
        /// Adds a new <see cref="ChatSyncTaskResult"/> to the <see cref="_results"/> list.
        /// </summary>
        /// <param name="title">The title of the task result.</param>
        /// <param name="elapsed">The time elapsed for the task.</param>
        /// <param name="rowsAffected">The number of rows affected by the task.</param>
        /// <returns>The created <see cref="ChatSyncTaskResult"/>.</returns>
        private ChatSyncTaskResult CreateAndAddNewTaskResult( string title, TimeSpan elapsed, int rowsAffected = 0 )
        {
            var taskResult = new ChatSyncTaskResult
            {
                Title = title,
                Elapsed = elapsed,
                RowsAffected = rowsAffected
            };

            _results.Add( taskResult );

            return taskResult;
        }

        /// <summary>
        /// Adds the details of a <see cref="ChatSyncCrudResult"/> to a <see cref="ChatSyncTaskResult"/>.
        /// </summary>
        /// <param name="taskResult">The task result.</param>
        /// <param name="crudResult">The CRUD result.</param>
        /// <param name="entityName">The name of the entity being synced.</param>
        private void AddCrudDetailsToTaskResult( ChatSyncTaskResult taskResult, ChatSyncCrudResult crudResult, string entityName )
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
                taskResult.Details.Add( $"{count} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Skipped}" );
            }

            if ( crudResult?.Created.Count > 0 )
            {
                var count = crudResult.Created.Count;
                taskResult.Details.Add( $"{count} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Created}" );
            }

            if ( crudResult?.Updated.Count > 0 )
            {
                var count = crudResult.Updated.Count;
                taskResult.Details.Add( $"{count} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Updated}" );
            }

            if ( crudResult?.Deleted.Count > 0 )
            {
                var count = crudResult.Deleted.Count;
                taskResult.Details.Add( $"{count} {entityName.PluralizeIf( count > 1 )} {CrudMessage.Deleted}" );
            }
        }

        /// <summary>
        /// Reports the results of this job run.
        /// </summary>
        /// <exception cref="RockJobWarningException">If any <see cref="ChatSyncTaskResult"/> has an <see cref="Exception"/>.</exception>
        private void ReportResults()
        {
            var jobSummaryBuilder = new StringBuilder();

            if ( _results.Count > 1 )
            {
                jobSummaryBuilder.AppendLine( "Summary:" );
                jobSummaryBuilder.AppendLine( string.Empty );
            }

            foreach ( var result in _results )
            {
                jobSummaryBuilder.AppendLine( GetFormattedResult( result ) );
            }

            if ( _results.Any( a => a.HasException ) )
            {
                jobSummaryBuilder.AppendLine( string.Empty );
                jobSummaryBuilder.AppendLine( "<i class='fa fa-circle text-danger'></i> Some tasks have errors. View Rock's Exception List for more details. You can also enable 'Error' verbosity level for 'Chat' domains in Rock Logs and re-run this job to get a full list of issues." );
            }
            else if ( _results.Any( a => a.IsWarning ) )
            {
                jobSummaryBuilder.AppendLine( string.Empty );
                jobSummaryBuilder.AppendLine( "<i class='fa fa-circle text-warning'></i> Some tasks completed with warnings. Enable 'Warning' verbosity level for 'Chat' domains in Rock Logs and re-run this job to get a full list of issues." );
            }

            this.Result = jobSummaryBuilder.ToString();

            var chatSyncTaskExceptions = _results.Where( a => a.HasException ).Select( a => a.Exception ).ToList();
            if ( chatSyncTaskExceptions.Any() )
            {
                var jobName = nameof( ChatSync );
                var exceptionList = new AggregateException( $"One or more exceptions occurred in {jobName}.", chatSyncTaskExceptions );
                throw new RockJobWarningException( $"{jobName} completed with errors.", exceptionList );
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
    }
}
