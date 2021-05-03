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

namespace Rock.Jobs
{
    /// <summary>
    /// This job synchronizes media content from configured <see cref="MediaAccount">accounts</see>.
    /// </summary>
    [DisplayName( "Media Sync" )]
    [Description( "Synchronizes media content from configured Media Accounts." )]

    [BooleanField( "Refresh Only",
        Description = "This will pull new data but not synchronize changes to existing data, but is often faster.",
        DefaultBooleanValue = false,
        Key = AttributeKey.RefreshOnly,
        Order = 0 )]
    [DisallowConcurrentExecution]
    public class MediaSync : IJob
    {
        /// <summary>
        /// Attribute Keys for the <see cref="MediaSync"/> job.
        /// </summary>
        private static class AttributeKey
        {
            public const string RefreshOnly = "RefreshOnly";
        }

        /// <summary> 
        /// Empty constructor for job initialization.
        /// </summary>
        public MediaSync()
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
            var refreshOnly = dataMap.GetString( AttributeKey.RefreshOnly ).AsBoolean();

            var errors = new List<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

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
                    // Quick refresh operation.
                    var results = await MediaAccountService.RefreshMediaInAllAccountsAsync();
                    errors.AddRange( results.Errors );
                }
            } );

            try
            {
                // Wait for our main task to complete.
                task.GetAwaiter().GetResult();
                sw.Stop();
            }
            catch ( Exception ex )
            {
                throw new Exception( "One or more accounts failed to process.", ex );
            }

            context.Result = $"Synchronized all accounts in {sw.ElapsedMilliseconds:n0}ms.";

            if ( errors.Any() )
            {
                throw new Exception( "One or more errors occurred while syncing accounts..." + Environment.NewLine + errors.AsDelimited( Environment.NewLine ) );
            }
        }
    }
}
