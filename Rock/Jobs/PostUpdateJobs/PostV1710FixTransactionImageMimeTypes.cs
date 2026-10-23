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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.10 to set the mime type of check images uploaded by the
    /// Check Scanner to image/png. The scanner uploads its PNG images as
    /// application/octet-stream, which GetImage.ashx refuses to serve. Fix for issue #7083.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.10 - Fix Transaction Image Mime Types" )]
    [Description( "This job will set the mime type of check images uploaded by the Check Scanner to image/png so that they can be displayed." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    [IntegerField(
        "Start At Id",
        Key = AttributeKey.StartAtId,
        Description = "The Id of the record to start or resume the execution of the job.",
        IsRequired = false,
        DefaultIntegerValue = 0 )]
    public class PostV1710FixTransactionImageMimeTypes : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            /// <summary>
            /// This key matches the one read by <see cref="PostUpdateJobs.PostUpdateJob.BulkUpdateRecords"/>.
            /// </summary>
            public const string CommandTimeout = "SqlCommandTimeOut";
            public const string StartAtId = "StartAtId";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            int lastId;

            using ( var rockContext = new RockContext() )
            {
                lastId = new FinancialTransactionImageService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Select( i => ( int? ) i.Id )
                    .Max() ?? 0;
            }

            // Batch on the FinancialTransactionImage Id so each batch is a range seek
            // and the BinaryFile rows are then found by their primary key. The SQL is
            // idempotent, so resuming over an already processed range is safe.
            var sql = @"
UPDATE [bf]
SET [bf].[MimeType] = 'image/png'
FROM [FinancialTransactionImage] AS [fti]
INNER JOIN [BinaryFile] AS [bf] ON [bf].[Id] = [fti].[BinaryFileId]
WHERE [fti].[Id] > @StartId
    AND [fti].[Id] <= @StartId + @BatchSize
    AND [bf].[MimeType] = 'application/octet-stream'
    AND [bf].[FileName] LIKE '%.png';";

            BulkUpdateRecords( sql, AttributeKey.StartAtId, lastId );

            DeleteJob();
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
