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

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
            rockContext.Database.CommandTimeout = _commandTimeout;
            var noteService = new NoteService( rockContext );
            var historyCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid() )?.Id;
            var entityTypeIdScheduledTransaction = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.FINANCIAL_SCHEDULED_TRANSACTION.AsGuid() );
            var noteTypeIdScheduledTransaction = NoteTypeCache.GetId( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );

            if ( !historyCategoryId.HasValue || !entityTypeIdScheduledTransaction.HasValue || !noteTypeIdScheduledTransaction.HasValue )
            {
                return;
            }

            var historyService = new HistoryService( rockContext );

            var historyQuery = historyService.Queryable().Where( a => a.EntityTypeId == entityTypeIdScheduledTransaction.Value );
            var captionsToConvert = new string[]
            {
                "Created Transaction"
                ,"Updated Transaction"
                ,"Cancelled Transaction"
                ,"Reactivated Transaction"
            };

            var notesToConvertToHistory = noteService.Queryable()
                .Where( a => a.NoteTypeId == noteTypeIdScheduledTransaction.Value && captionsToConvert.Contains( a.Caption ) && a.EntityId.HasValue )
                .Where( a => !historyQuery.Any( h => h.EntityId == a.EntityId ) );

            List<History> historyRecordsToInsert = notesToConvertToHistory.AsNoTracking()
                .ToList()
                .Select( n =>
                {
                    var historyRecord = new History
                    {
                        CategoryId = historyCategoryId.Value,
                        EntityTypeId = entityTypeIdScheduledTransaction.Value,
                        EntityId = n.EntityId.Value,
                        Guid = Guid.NewGuid(),
                        CreatedByPersonAliasId = n.CreatedByPersonAliasId,
                        ModifiedByPersonAliasId = n.ModifiedByPersonAliasId,
                        CreatedDateTime = n.CreatedDateTime,
                        ModifiedDateTime = n.ModifiedDateTime
                    };

                    if ( n.Caption == "Cancelled Transaction" )
                    {
                        historyRecord.Verb = "MODIFY";
                        historyRecord.ChangeType = "Property";
                        historyRecord.ValueName = "Is Active";
                        historyRecord.NewValue = "False";
                    }
                    else if ( n.Caption == "Reactivated Transaction" )
                    {
                        historyRecord.Verb = "MODIFY";
                        historyRecord.ChangeType = "Property";
                        historyRecord.ValueName = "Is Active";
                        historyRecord.NewValue = "True";
                    }
                    else if ( n.Caption == "Updated Transaction" )
                    {
                        historyRecord.Verb = "MODIFY";
                        historyRecord.ValueName = "Transaction";
                    }
                    else
                    {
                        historyRecord.Verb = "ADD";
                        historyRecord.ChangeType = "Record";
                        historyRecord.ValueName = "Transaction";
                    }

                    return historyRecord;


                } ).ToList();

            rockContext.BulkInsert( historyRecordsToInsert );
            var qryNotesToDelete = noteService.Queryable().Where( a => a.NoteTypeId == noteTypeIdScheduledTransaction && captionsToConvert.Contains( a.Caption ) );
            rockContext.BulkDelete( qryNotesToDelete );
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
