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
using System.Threading;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Observability;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v18.0 to populate the newly-added [CommunicationRecipient].[DeliveredDateTime] field.
    /// </summary>
    [DisplayName( "Rock Update Helper v18.0 - Populate [CommunicationRecipient].[DeliveredDateTime]" )]
    [Description( "This job will populate the newly-added [CommunicationRecipient].[DeliveredDateTime] field by attempting to parse the DateTime value from the [StatusNote] field for email records and by copying the value directly from the [SendDateTime] field for non-email records." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV18PopulateCommunicationRecipientDeliveredDateTime : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // Get the configured timeout, or default to 240 minutes if it is blank.
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            int? emailRecordsMigratedInLastBatch = null;
            int? nonEmailRecordsMigratedInLastBatch = null;
            var totalRecordsMigratedCount = 0;

            var shouldContinueMigrating = true;

            while ( shouldContinueMigrating )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "Task: Populate [CommunicationRecipient].[DeliveredDateTime]" ) )
                {
                    // Attempt to migrate a batch of 1500 email records on first run or if the previous run resulted in any email records being migrated.
                    if ( !emailRecordsMigratedInLastBatch.HasValue || emailRecordsMigratedInLastBatch.Value > 0 )
                    {
                        emailRecordsMigratedInLastBatch = ( int ) jobMigration.SqlScalar( $@"
DECLARE @EmailMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL}');

;WITH TopRows AS (
    SELECT TOP (1500) [Id]
        , TRY_PARSE(SUBSTRING([StatusNote], CHARINDEX('at ', [StatusNote]) + 3, 100) AS DATETIME) AS [ParsedDeliveredDateTime]
    FROM [CommunicationRecipient]
    WHERE [DeliveredDateTime] IS NULL
        AND [MediumEntityTypeId] = @EmailMediumEntityTypeId
        AND [Status] IN (1, 4) -- Delivered, Opened
        AND [StatusNote] LIKE 'Confirmed delivered by %'
        AND ISDATE(SUBSTRING([StatusNote], CHARINDEX('at ', [StatusNote]) + 3, 100)) = 1
    ORDER BY [Id] DESC
)
UPDATE cr
SET [DeliveredDateTime] = TopRows.[ParsedDeliveredDateTime]
FROM [CommunicationRecipient] cr
INNER JOIN TopRows ON cr.[Id] = TopRows.[Id];

SELECT @@ROWCOUNT AS [RecordsMigratedCount];" );
                    }
                    else
                    {
                        emailRecordsMigratedInLastBatch = 0;
                    }

                    // Attempt to migrate a batch of 1500 non-email records on first run or if the previous run resulted in any non-email records being migrated.
                    if ( !nonEmailRecordsMigratedInLastBatch.HasValue || nonEmailRecordsMigratedInLastBatch.Value > 0 )
                    {
                        nonEmailRecordsMigratedInLastBatch = ( int ) jobMigration.SqlScalar( $@"
DECLARE @EmailMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL}');

;WITH TopRows AS (
    SELECT TOP (1500) [Id]
        , [SendDateTime]
    FROM [CommunicationRecipient]
    WHERE [DeliveredDateTime] IS NULL
        AND [MediumEntityTypeId] <> @EmailMediumEntityTypeId
        AND [Status] IN (1, 4) -- Delivered, Opened
        AND [SendDateTime] IS NOT NULL
    ORDER BY [Id] DESC
)
UPDATE cr
SET [DeliveredDateTime] = TopRows.[SendDateTime]
FROM [CommunicationRecipient] cr
INNER JOIN TopRows ON cr.[Id] = TopRows.[Id];

SELECT @@ROWCOUNT AS [RecordsMigratedCount];" );
                    }
                    else
                    {
                        nonEmailRecordsMigratedInLastBatch = 0;
                    }

                    totalRecordsMigratedCount += emailRecordsMigratedInLastBatch.Value;
                    totalRecordsMigratedCount += nonEmailRecordsMigratedInLastBatch.Value;

                    activity?.AddTag( "rock.job.batch_migrated_email_record_count", emailRecordsMigratedInLastBatch.Value );
                    activity?.AddTag( "rock.job.batch_migrated_non_email_record_count", nonEmailRecordsMigratedInLastBatch.Value );
                    activity?.AddTag( "rock.job.total_migrated_record_count", totalRecordsMigratedCount );

                    // Try once more if this batch resulted in any records being migrated.
                    shouldContinueMigrating = emailRecordsMigratedInLastBatch.Value > 0 || nonEmailRecordsMigratedInLastBatch.Value > 0;
                }

                if ( shouldContinueMigrating )
                {
                    UpdateLastStatusMessage( $"{totalRecordsMigratedCount:N0} records have been updated." );

                    // Reduce server spike during sustained batches.
                    Thread.Sleep( 10 );
                }
            }

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
