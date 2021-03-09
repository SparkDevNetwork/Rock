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
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of data, this could take several minutes or more.",
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

                UpdateDateKeyColumnsNotNull( rockContext );
                CreateDateKeyIndexes( rockContext );

                CreateInteractionChannelCustomIndexed1( rockContext );
            }

            DeleteJob( context.GetJobId() );
        }

        /// <summary>
        /// Creates the interaction channel custom indexed1.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private static void CreateInteractionChannelCustomIndexed1( RockContext rockContext )
        {
            rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_ChannelCustomIndexed1'
    AND object_id = OBJECT_ID('Interaction')
)
BEGIN
CREATE INDEX [IX_ChannelCustomIndexed1] ON [dbo].[Interaction]([ChannelCustomIndexed1])
END" );
        }

        /// <summary>
        /// Updates the date key columns not null.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private static void UpdateDateKeyColumnsNotNull( RockContext rockContext )
        {
            rockContext.Database.ExecuteSqlCommand( @"
ALTER TABLE [dbo].[AttendanceOccurrence]
ALTER COLUMN OccurrenceDateKey INT NOT NULL

ALTER TABLE [dbo].BenevolenceRequest
ALTER COLUMN RequestDateKey INT NOT NULL

ALTER TABLE [dbo].FinancialPledge
ALTER COLUMN StartDateKey INT NOT NULL

ALTER TABLE [dbo].FinancialPledge
ALTER COLUMN EndDateKey INT NOT NULL

ALTER TABLE [dbo].Interaction
ALTER COLUMN InteractionDateKey INT NOT NULL
" );
        }

        /// <summary>
        /// Creates the date key indexes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private static void CreateDateKeyIndexes( RockContext rockContext )
        {


            rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_CreatedDateKey' AND object_id = OBJECT_ID('Registration')
)
BEGIN
CREATE INDEX [IX_CreatedDateKey] ON [dbo].[Registration] ([CreatedDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_OccurrenceDateKey' AND object_id = OBJECT_ID('AttendanceOccurrence')
)
BEGIN
CREATE INDEX [IX_OccurrenceDateKey] ON [dbo].[AttendanceOccurrence] ([OccurrenceDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_StartDateKey' AND object_id = OBJECT_ID('Step')
)
BEGIN
CREATE INDEX [IX_StartDateKey] ON [dbo].[Step] ([StartDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_EndDateKey' AND object_id = OBJECT_ID('Step')
)
BEGIN
CREATE INDEX [IX_EndDateKey] ON [dbo].[Step] ([EndDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_CompletedDateKey' AND object_id = OBJECT_ID('Step')
)
BEGIN
CREATE INDEX [IX_CompletedDateKey] ON [dbo].[Step] ([CompletedDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_RequestDateKey' AND object_id = OBJECT_ID('BenevolenceRequest')
)
BEGIN
CREATE INDEX [IX_RequestDateKey] ON [dbo].[BenevolenceRequest] ([RequestDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_SendDateKey' AND object_id = OBJECT_ID('Communication')
)
BEGIN
CREATE INDEX [IX_SendDateKey] ON [dbo].[Communication] ([SendDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_CreatedDateKey' AND object_id = OBJECT_ID('ConnectionRequest')
)
BEGIN
CREATE INDEX [IX_CreatedDateKey] ON [dbo].[ConnectionRequest] ([CreatedDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_TransactionDateKey' AND object_id = OBJECT_ID('FinancialTransaction')
)
BEGIN
CREATE INDEX [IX_TransactionDateKey] ON [dbo].[FinancialTransaction] ([TransactionDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_SettledDateKey' AND object_id = OBJECT_ID('FinancialTransaction')
)
BEGIN
CREATE INDEX [IX_SettledDateKey] ON [dbo].[FinancialTransaction] ([SettledDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_StartDateKey' AND object_id = OBJECT_ID('FinancialPledge')
)
BEGIN
CREATE INDEX [IX_StartDateKey] ON [dbo].[FinancialPledge] ([StartDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_EndDateKey' AND object_id = OBJECT_ID('FinancialPledge')
)
BEGIN
CREATE INDEX [IX_EndDateKey] ON [dbo].[FinancialPledge] ([EndDateKey])
END

IF NOT EXISTS (
SELECT *
FROM sys.indexes
WHERE name = 'IX_InteractionDateKey' AND object_id = OBJECT_ID('Interaction')
)
BEGIN
CREATE INDEX [IX_InteractionDateKey] ON [dbo].[Interaction] ([InteractionDateKey])
END
" );
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