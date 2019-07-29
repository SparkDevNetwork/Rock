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

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for ScheduledTransactionNotesToHistory in V9.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v9.0 - ScheduledTransactionNotesToHistory" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v9. After all the operations are done, this job will delete itself." )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.", false, 60 * 60, "General", 7, "CommandTimeout" )]
    public class PostV90DataMigrationsScheduledTransactionNotesToHistory : IJob
    {
        private int? _commandTimeout = null;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            MigrateScheduledTransactionNotesToHistory();

            DeleteJob( context.GetJobId() );
        }

        /// <summary>
        /// Migrates the scheduled transaction notes to history.
        /// </summary>
        public void MigrateScheduledTransactionNotesToHistory()
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( @"
-- Scheduled Transactions didn't write to history until v7.4, so convert those into History notes

DECLARE @historyCategoryId INT = (
		SELECT TOP 1 Id
		FROM Category
		WHERE Guid = '477EE3BE-C68F-48BD-B218-FAFC99AF56B3'
		)
	,@entityTypeIdScheduledTransaction INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE [Guid] = '76824E8A-CCC4-4085-84D9-8AF8C0807E20'
		)
	,@noteTypeIdScheduledTransaction INT = (
		SELECT TOP 1 Id
		FROM NoteType
		WHERE [Guid] = '360CFFE2-7FE3-4B0B-85A7-BFDACC9AF588'
		)

BEGIN
	-- convert 'Created Transaction' notes into History if they aren't aleady in History
	INSERT INTO [History] (
		IsSystem
		,CategoryId
		,EntityTypeId
		,EntityId
		,[Guid]
		,CreatedDateTime
		,ModifiedDateTime
		,CreatedByPersonAliasId
		,ModifiedByPersonAliasId
		,[Verb]
		,[ChangeType]
		,[ValueName]
		)
	SELECT n.IsSystem
		,@historyCategoryId [CategoryId]
		,@entityTypeIdScheduledTransaction [EntityTypeId]
		,n.EntityId
		,newid() [Guid]
		,n.CreatedDateTime
		,n.ModifiedDateTime
		,n.CreatedByPersonAliasId
		,n.ModifiedByPersonAliasId
		,'ADD' [Verb]
		,'Record' [ChangeType]
		,'Transaction' [ValueName]
	FROM [Note] n
	WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] = 'Created Transaction'
		AND EntityId NOT IN (
			SELECT EntityId
			FROM [History]
			WHERE EntityTypeId = @entityTypeIdScheduledTransaction
				AND [Verb] = 'ADD'
			)

	-- convert 'Updated Transaction','Cancelled Transaction','Reactivated Transaction' notes into History if they aren't aleady in History
	INSERT INTO [History] (
		IsSystem
		,CategoryId
		,EntityTypeId
		,EntityId
		,[Summary]
		,[Guid]
		,CreatedDateTime
		,ModifiedDateTime
		,CreatedByPersonAliasId
		,ModifiedByPersonAliasId
		,[Verb]
		,[ChangeType]
		,[ValueName]
		,[NewValue]
		)
	SELECT n.IsSystem
		,@historyCategoryId [CategoryId]
		,@entityTypeIdScheduledTransaction [EntityTypeId]
		,n.EntityId
		,n.Caption [Summary]
		,newid() [Guid]
		,n.CreatedDateTime
		,n.ModifiedDateTime
		,n.CreatedByPersonAliasId
		,n.ModifiedByPersonAliasId
		,'MODIFY' [Verb]
		,'Property' [ChangeType]
		,CASE n.Caption
			WHEN 'Cancelled Transaction'
				THEN 'Is Active'
			WHEN 'Reactivated Transaction'
				THEN 'Is Active'
			ELSE 'Transaction'
			END [ValueName]
		,CASE n.Caption
			WHEN 'Cancelled Transaction'
				THEN 'False'
			WHEN 'Reactivated Transaction'
				THEN 'True'
			ELSE ''
			END [NewValue]
	FROM [Note] n
	WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] IN (
			'Updated Transaction'
			,'Cancelled Transaction'
			,'Reactivated Transaction'
			)
		AND EntityId NOT IN (
			SELECT EntityId
			FROM [History]
			WHERE EntityTypeId = @entityTypeIdScheduledTransaction
				AND [Verb] = 'MODIFY'
			)
	ORDER BY [Caption]

	DELETE FROM [Note] WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] IN (
			'Created Transaction'
			,'Updated Transaction'
			,'Cancelled Transaction'
			,'Reactivated Transaction'
			)
END
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
