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

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V11.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v11.0 - Populates the new DateKey fields with data." )]
    [Description( "This job will populate the new DateKey fields on the Interaction, FinancialTransaction, AttendanceOccurence, Communication, MetricValue tables which were added as part of v11.0. After all the operations are done, this job will delete itself." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV110DataMigrationsUpdateDateKeyValues : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                UpdateKeyDateColumnData( rockContext, "Interaction", "InteractionDateKey", "InteractionDateTime" );
                UpdateKeyDateColumnData( rockContext, "FinancialTransaction", "TransactionDateKey", "TransactionDateTime" );
                UpdateKeyDateColumnData( rockContext, "AttendanceOccurrence", "OccurrenceDateKey", "OccurrenceDate" );
                UpdateKeyDateColumnData( rockContext, "Communication", "SendDateKey", "SendDateTime" );
                UpdateKeyDateColumnData( rockContext, "MetricValue", "MetricValueDateKey", "MetricValueDateTime" );
                UpdateKeyDateColumnData( rockContext, "ConnectionRequest", "CreatedDateKey", "CreatedDateTime" );
                UpdateKeyDateColumnData( rockContext, "BenevolenceRequest", "RequestDateKey", "RequestDateTime" );
                UpdateKeyDateColumnData( rockContext, "FinancialPledge", "StartDateKey", "StartDate" );
                UpdateKeyDateColumnData( rockContext, "FinancialPledge", "EndDateKey", "EndDate" );
                UpdateKeyDateColumnData( rockContext, "Registration", "CreatedDateKey", "CreatedDateTime" );
                UpdateKeyDateColumnData( rockContext, "Step", "CompletedDateKey", "CompletedDateTime" );
                UpdateKeyDateColumnData( rockContext, "Step", "StartDateKey", "StartDateTime" );
                UpdateKeyDateColumnData( rockContext, "Step", "EndDateKey", "EndDateTime" );
            }

            DeleteJob( context.GetJobId() );
        }

        private void UpdateKeyDateColumnData( RockContext rockContext, string tableName, string keyColumnName, string dateColumnName )
        {
            rockContext.Database.ExecuteSqlCommand( $@"
                UPDATE [{tableName}]
                SET [{keyColumnName}] = CONVERT(INT, (CONVERT(CHAR(8), [{dateColumnName}], 112)))
            " );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
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