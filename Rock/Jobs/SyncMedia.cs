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
using System.Threading.Tasks;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Media;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job synchronizes media content from configured <see cref="MediaAccount">accounts</see>.
    /// </summary>
    [DisplayName( "Sync Media" )]
    [Description( "Synchronizes media content from configured Media Accounts." )]

    [BooleanField(
        "Limit Full Sync to Once a Day",
        Key = AttributeKey.LimitFullSyncToOnceADay,
        Description = "A full sync downloads additional analytics information and can take longer to process.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "General",
        Order = 0 )]

    [DisallowConcurrentExecution]
    public class SyncMedia : IJob
    {
        /// <summary>
        /// Attribute Keys for the <see cref="SyncMedia"/> job.
        /// </summary>
        private static class AttributeKey
        {
            public const string LimitFullSyncToOnceADay = "LimitFullSyncToOnceADay";
        }

        /// <summary> 
        /// Empty constructor for job initialization.
        /// </summary>
        public SyncMedia()
        {
        }

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var limitFullSync = dataMap.GetString( AttributeKey.LimitFullSyncToOnceADay ).AsBoolean( true );

            // Start a task that will let us run the Async methods in order.
            var task = Task.Run( () => ProcessAllAccounts( limitFullSync ) );

            // Wait for our main task to complete.
            var result = task.GetAwaiter().GetResult();

            context.Result = result.Message;

            if ( result.Errors.Any() )
            {
                throw new Exception( "One or more errors occurred while syncing accounts:" + Environment.NewLine + string.Join( Environment.NewLine, result.Errors ) );
            }
        }

        /// <summary>
        /// Synchronizes the folders and media in all active accounts.
        /// </summary>
        /// <param name="limitFullSync"><c>true</c> if a full-sync should only be performed once per day.</param>
        /// <returns>A <see cref="SyncOperationResult"/> object with the result of the operation.</returns>
        private async Task<OperationResult> ProcessAllAccounts( bool limitFullSync )
        {
            using ( var rockContext = new RockContext() )
            {
                var tasks = new List<Task<OperationResult>>();
                var mediaAccounts = new MediaAccountService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.IsActive )
                    .ToList();

                if ( mediaAccounts.Count == 0 )
                {
                    return new OperationResult( "No active accounts to process.", new string[0] );
                }

                // Start a SyncMedia task for each active account.
                foreach ( var mediaAccount in mediaAccounts )
                {
                    var task = Task.Run( async () =>
                    {
                        try
                        {
                            return await ProcessOneAccount( mediaAccount, limitFullSync );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                            return new OperationResult( $"Failed to sync account.", new[] { $"{mediaAccount.Name}: {ex.Message}" } );
                        }
                    } );

                    tasks.Add( task );
                }

                // Wait for all operational tasks to complete and then
                // aggregate the results.
                var results = await Task.WhenAll( tasks );

                var message = string.Join( Environment.NewLine, results.Select( a => a.Message ) );

                return new OperationResult( message, results.SelectMany( a => a.Errors ) );
            }
        }

        /// <summary>
        /// Processes one account and return the result of the operation.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="limitFullSync"><c>true</c> if a full-sync should only be performed once per day.</param>
        /// <returns>The result of the operation.</returns>
        private async Task<OperationResult> ProcessOneAccount( MediaAccount mediaAccount, bool limitFullSync )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var errors = new List<string>();

            if ( mediaAccount.GetMediaAccountComponent() == null )
            {
                return new OperationResult( $"Skipped account {mediaAccount.Name}.", new[] { $"{mediaAccount.Name}: Media Account component was not found." } );
            }

            // Determine if this is a full sync or a partial refresh.
            var currentDateTime = RockDateTime.Now;
            var lastFullSync = mediaAccount.LastRefreshDateTime;
            var haveSyncedToday = lastFullSync.HasValue && lastFullSync.Value.Date == currentDateTime.Date;
            var refreshOnly = limitFullSync && haveSyncedToday;

            if ( refreshOnly )
            {
                // Quick refresh media and folders only.
                var result = await MediaAccountService.RefreshMediaInAccountAsync( mediaAccount.Id );
                errors.AddRange( result.Errors );
            }
            else
            {
                // First sync all the media and folders.
                var result = await MediaAccountService.SyncMediaInAccountAsync( mediaAccount.Id );
                errors.AddRange( result.Errors );

                // Next sync all the analytics.
                result = await MediaAccountService.SyncAnalyticsInAccountAsync( mediaAccount.Id );
                errors.AddRange( result.Errors );
            }

            sw.Stop();
            var seconds = ( int ) sw.Elapsed.TotalSeconds;

            var message = $"{( refreshOnly ? "Refreshed" : "Synchronized" )} account {mediaAccount.Name} in {seconds}s.";

            // Since we will be aggregating errors include the
            // account name if there were any errors.
            return new OperationResult( message, errors.Select( a => $"{mediaAccount.Name}: {a}" ) );
        }

        /// <summary>
        /// The result of one of our internal operations.
        /// </summary>
        private class OperationResult
        {
            /// <summary>
            /// Gets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; }

            /// <summary>
            /// Gets the errors.
            /// </summary>
            /// <value>
            /// The errors.
            /// </value>
            public List<string> Errors { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="OperationResult"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="errors">The errors.</param>
            public OperationResult( string message, IEnumerable<string> errors )
            {
                Message = message;
                Errors = errors.ToList();
            }
        }
    }
}
