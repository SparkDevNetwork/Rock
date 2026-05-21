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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v19.0 that backfills the [ConnectionRequest].[ConnectedDateTime] (and [WasCompletedOnTime])
    /// columns for connection requests that were already in the Connected state prior to v19. The save hook
    /// only stamps these columns on a state transition, so requests that were Connected before the upgrade
    /// have NULL values; this job recovers the timestamps from the [History] table.
    /// </summary>
    [DisplayName( "Rock Update Helper v19.0 - Backfill Connection Request Connected DateTime" )]
    [Description( "This job backfills the ConnectedDateTime and WasCompletedOnTime columns on Connection Requests completed before v19." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV19BackfillConnectionRequestConnectedDateTime : RockJob
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

            jobMigration.Sql( @"
DECLARE @ConnectionRequestEntityTypeId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[EntityType]
    WHERE [Guid] = '36B0D0C7-8125-48FA-9DA2-729AAA65F718'
);

DECLARE @ConnectionRequestCategoryId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[Category]
    WHERE [Guid] = 'A8542DD2-91B1-4CCA-873A-D052BCD6EE06'
);

IF @ConnectionRequestEntityTypeId IS NOT NULL AND @ConnectionRequestCategoryId IS NOT NULL
BEGIN
    -- Pick the most recent ""ConnectionState changed to Connected"" history record per request,
    -- then stamp ConnectedDateTime and WasCompletedOnTime on requests that are still Connected
    -- and have no ConnectedDateTime yet. Re-running is a no-op due to the NULL check.
    ;WITH LatestConnected AS (
        SELECT
            h.[EntityId] AS ConnectionRequestId,
            MAX( h.[CreatedDateTime] ) AS ConnectedDateTime
        FROM [dbo].[History] h
        WHERE h.[EntityTypeId] = @ConnectionRequestEntityTypeId
          AND h.[CategoryId]   = @ConnectionRequestCategoryId
          AND h.[ValueName]    = 'ConnectionState'
          AND h.[NewValue]     = 'Connected'
          AND h.[CreatedDateTime] IS NOT NULL
        GROUP BY h.[EntityId]
    )
    UPDATE cr
    SET cr.[ConnectedDateTime]  = lc.[ConnectedDateTime],
        cr.[WasCompletedOnTime] = CASE
            WHEN cr.[DueDate] IS NULL OR lc.[ConnectedDateTime] <= cr.[DueDate] THEN 1
            ELSE 0
        END
    FROM [dbo].[ConnectionRequest] cr
    INNER JOIN LatestConnected lc ON lc.[ConnectionRequestId] = cr.[Id]
    WHERE cr.[ConnectedDateTime] IS NULL
      AND cr.[ConnectionState]   = 3; -- ConnectionState.Connected
END
" );

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
