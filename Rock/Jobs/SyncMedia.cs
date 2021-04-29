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

using Quartz;

using Rock.Attribute;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;

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
            var limitFullSync = dataMap.GetString( AttributeKey.LimitFullSyncToOnceADay ).AsBoolean();

            // Determine if we are doing a full sync or just a refresh.
            var currentDateTime = RockDateTime.Now;
            var lastFullSync = SystemSettings.GetValue( SystemSetting.MEDIA_SYNC_LAST_FULL_SYNC_DATETIME ).AsDateTime();
            var haveStatsSyncedToday = lastFullSync.HasValue && lastFullSync.Value.Date == currentDateTime.Date;
            var refreshOnly = limitFullSync && haveStatsSyncedToday;

            var errors = new List<string>();

            // Start a task that will let us run the Async methods in order.
            var task = Task.Run( async () =>
            {
                if ( !refreshOnly )
                {
                    // First sync all the media and folders.
                    var results = await MediaAccountService.SyncMediaInAllAccountsAsync();
                    errors.AddRange( results.Errors );

                    // Next sync all the analytics.
                    results = await MediaAccountService.SyncAnalyticsInAllAccountsAsync();
                    errors.AddRange( results.Errors );
                }
                else
                {
                    // Quick refresh media and folders only.
                    var results = await MediaAccountService.RefreshMediaInAllAccountsAsync();
                    errors.AddRange( results.Errors );
                }
            } );

            try
            {
                // Wait for our main task to complete.
                task.GetAwaiter().GetResult();

                SystemSettings.SetValue( SystemSetting.MEDIA_SYNC_LAST_FULL_SYNC_DATETIME, currentDateTime.ToString() );

                context.Result = $"{( refreshOnly ? "Refreshed" : "Synchronized" )} all accounts successfully.";

                if ( errors.Any() )
                {
                    throw new Exception( "One or more errors occurred while syncing accounts..." + Environment.NewLine + errors.AsDelimited( Environment.NewLine ) );
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( "One or more accounts failed to process.", ex );
            }
        }
    }
}
