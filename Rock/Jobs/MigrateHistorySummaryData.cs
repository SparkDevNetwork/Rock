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
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job is used to convert a history record's Summary to the actual fields that were added in v8. Once all the values have been converted, this job will delete itself.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Migrate History Summary Data" )]
    [Description( "This job is used to convert a history record's Summary to the actual fields that were added in v8. Once all the values have been converted, this job will delete itself." )]

    [DisallowConcurrentExecution]
    [IntegerField( "How Many Records", "The number of history records to process on each run of this job.", false, 500000, "", 0, "HowMany" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    [Obsolete( "No longer supported", true )]
    [RockObsolete( "1.8" )]
    public class MigrateHistorySummaryData : IJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int howMany = dataMap.GetString( "HowMany" ).AsIntegerOrNull() ?? 500000;
            var commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            bool anyRemaining = false;

            if ( !anyRemaining )
            {
                // Verify that there are not any history records that haven't been migrated
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;

                    if ( !new HistoryService( rockContext )
                        .Queryable()
                        .Where( c => c.ChangeType == null )
                        .Any() )
                    {

                        // delete job if there are no un-migrated history rows  left
                        var jobId = context.GetJobId();
                        var jobService = new ServiceJobService( rockContext );
                        var job = jobService.Get( jobId );
                        if ( job != null )
                        {
                            jobService.Delete( job );
                            rockContext.SaveChanges();
                            return;
                        }
                    }
                }
            }
        }
    }
}
