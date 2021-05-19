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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.4
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v12.4 - Decrypt expiration month / year and name on card fields." )]
    [Description( "This job will decrypt the expiration month / year and the name on card fields." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV124DataMigrationUnencryptPaymentDetailFields : IJob
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
            var isProcessingComplete = false;
            var batchSize = 100;
            var totalBatchSize = 0;
            var currentBatch = 1;

            var runtime = System.Diagnostics.Stopwatch.StartNew();
            while ( !isProcessingComplete )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new FinancialPaymentDetailService( rockContext );
#pragma warning disable 612, 618
                    var itemsToUpdate = service
                        .Queryable()
                        .Where( pd => ( pd.ExpirationMonth == null && pd.ExpirationMonthEncrypted != null )
                        || ( pd.NameOnCardEncrypted != null && pd.NameOnCard == null ) );
#pragma warning restore 612, 618

                    if ( currentBatch == 1 )
                    {
                        totalBatchSize = itemsToUpdate.Count();
                    }

                    itemsToUpdate = itemsToUpdate.Take( batchSize );

                    var currentBatchSize = itemsToUpdate.Count();

                    isProcessingComplete = currentBatchSize < batchSize;

                    foreach ( var item in itemsToUpdate )
                    {
                        rockContext.Entry( item ).State = EntityState.Modified;
                    }
                    rockContext.SaveChanges();

                    currentBatch++;

                    var processTime = runtime.ElapsedMilliseconds;
                    var recordsProcessed = ( double ) ( currentBatchSize * currentBatch );
                    var recordsPerMillisecond = recordsProcessed / processTime;
                    var recordsRemaining = totalBatchSize - recordsProcessed;
                    var minutesRemaining = recordsRemaining / recordsPerMillisecond / 1000 / 60;

                    context.UpdateLastStatusMessage( $"Processing {recordsProcessed} of {totalBatchSize} records. Approximately {minutesRemaining:N0} minutes remaining." );
                }
            }

            ServiceJobService.DeleteJob( context.GetJobId() );
        }
    }
}
