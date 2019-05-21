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
    /// A run once job for V8.4
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Data Migrations for v8.4" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v8.4. After all the operations are done, this job will delete itself." )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.", false, 60 * 60, "General", 7, "CommandTimeout" )]
    public class PostV84DataMigrations : IJob
    {
        private int? _commandTimeout = null;

        /// <summary>
        /// Executes the specified context. When updating large data sets SQL will burn a lot of time updating the indexes. If performing multiple inserts/updates
        /// consider dropping the related indexes first and re-creating them once the operation is complete.
        /// Put all index creation method calls at the end of this method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            CreateOrUpdateIndexInteractionsForeignKey();
            UpdateRefundTransactionSource();
            DeleteJob( context.GetJobId() );
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

        /// <summary>
        /// Creates the index on Interactions.ForeignKey.
        /// Includes were recommended by Query Analyzer
        /// </summary>
        public void CreateOrUpdateIndexInteractionsForeignKey()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                rockContext.Database.ExecuteSqlCommand( @"
IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_ForeignKey'
			AND object_id = OBJECT_ID(N'[dbo].[Interaction]')
			AND has_filter = 0
		)
BEGIN
	DROP INDEX [IX_ForeignKey] ON [dbo].[Interaction]
END

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_ForeignKey'
			AND object_id = OBJECT_ID(N'[dbo].[Interaction]')
			AND has_filter = 1
		)
BEGIN
	CREATE NONCLUSTERED INDEX [IX_ForeignKey] ON [dbo].[Interaction] ([ForeignKey]) WHERE [ForeignKey] IS NOT NULL
END
" );
            }
        }

        /// <summary>
        /// Updates the a Refund Transaction's SourceTypeValueId to match the SourceTypeValueId of the original transaction
        /// </summary>
        public void UpdateRefundTransactionSource()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                rockContext.Database.ExecuteSqlCommand( @"
UPDATE rft
SET rft.SourceTypeValueId = oft.SourceTypeValueId
FROM FinancialTransactionRefund r
JOIN FinancialTransaction rft ON rft.Id = r.Id
JOIN FinancialTransaction oft ON oft.Id = r.OriginalTransactionId
WHERE isnull(rft.SourceTypeValueId, 0) != isnull(oft.SourceTypeValueId, 0)
" );
            }
        }
    }
}
