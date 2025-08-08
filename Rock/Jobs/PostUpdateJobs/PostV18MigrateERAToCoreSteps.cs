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
    /// Run once job for v18.0 to migrate eRA records from the History table to the Steps table.
    /// </summary>
    [DisplayName( "Rock Update Helper v18.0 - Migrate eRA Records To Step Records" )]
    [Description( "This job will migrate existing eRA records from the History table to the Steps table." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV18MigrateERAToCoreSteps : RockJob
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
-- Get required IDs
DECLARE @EntityTypeId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[EntityType]
    WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'
);

DECLARE @RelatedEntityTypeId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[EntityType]
    WHERE [Guid] = '5997C8D3-8840-4591-99A5-552919F90CBD'
);

DECLARE @EraAttributeId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[Attribute]
    WHERE [Guid] = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
);

DECLARE @StepTypeId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[StepType]
    WHERE [Guid] = 'E57468BE-15BF-48B6-AAB2-F8E2B02720F3'
);

DECLARE @InProgressStepStatusId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[StepStatus]
    WHERE [Guid] = '8013C752-31AA-46C6-9B55-BCFBE57C0577'
);

DECLARE @CompleteStepStatusId INT = (
    SELECT TOP 1 [Id]
    FROM [dbo].[StepStatus]
    WHERE [Guid] = '359D3CE0-E144-491E-8C3B-2A2BCE55C04B'
);

-- Detect and flag invalid eRA event pairs
WITH OrderedHistory AS (
    SELECT
        h.[Id],
        h.[EntityId] AS [PersonId],
        h.[Verb],
        h.[CreatedDateTime],
        ROW_NUMBER() OVER (
            PARTITION BY h.[EntityId]
            ORDER BY h.[CreatedDateTime]
        ) AS [RowNum]
    FROM [dbo].[History] h
    WHERE h.[EntityTypeId] = @EntityTypeId
      AND h.[RelatedEntityTypeId] = @RelatedEntityTypeId
      AND h.[RelatedEntityId] = @EraAttributeId
      AND h.[Verb] IN ('ENTERED', 'EXITED')
),

ExitSequenced AS (
    SELECT *,
        SUM(CASE WHEN [Verb] = 'ENTERED' THEN 1 ELSE 0 END)
            OVER (
                PARTITION BY [PersonId]
                ORDER BY [RowNum]
                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
            ) AS [GroupNum]
    FROM OrderedHistory
),

InvalidExits AS (
    SELECT [Id]
    FROM (
        SELECT *,
            ROW_NUMBER() OVER (
                PARTITION BY [PersonId], [GroupNum]
                ORDER BY [CreatedDateTime]
            ) AS [GroupExitNum]
        FROM ExitSequenced
        WHERE [Verb] = 'EXITED'
    ) AS x
    WHERE [GroupExitNum] > 1
),

EnterSequenced AS (
    SELECT *,
        SUM(CASE WHEN [Verb] = 'EXITED' THEN 1 ELSE 0 END)
            OVER (
                PARTITION BY [PersonId]
                ORDER BY [RowNum]
                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
            ) AS [GroupNum]
    FROM OrderedHistory
),

InvalidEnters AS (
    SELECT [Id]
    FROM (
        SELECT *,
            ROW_NUMBER() OVER (
                PARTITION BY [PersonId], [GroupNum]
                ORDER BY [CreatedDateTime]
            ) AS [GroupEnteredNum]
        FROM EnterSequenced
        WHERE [Verb] = 'ENTERED'
    ) AS x
    WHERE [GroupEnteredNum] > 1
),

InvalidIds AS (
    SELECT [Id] FROM InvalidExits
    UNION
    SELECT [Id] FROM InvalidEnters
)

-- Delete invalid eRA history entries
DELETE FROM [dbo].[History]
WHERE [Id] IN (
    SELECT [Id]
    FROM InvalidIds
);

-- Get ENTERED and EXITED events, ordered by person and timestamp
WITH HistoryFiltered AS (
    SELECT
        h.[Id],
        h.[EntityId] AS [PersonId],
        h.[CreatedDateTime],
        h.[Verb],
        ROW_NUMBER() OVER (
            PARTITION BY h.[EntityId], h.[Verb]
            ORDER BY h.[CreatedDateTime]
        ) AS [RowNum]
    FROM [dbo].[History] h
    WHERE h.[EntityTypeId] = @EntityTypeId
      AND h.[RelatedEntityTypeId] = @RelatedEntityTypeId
      AND h.[RelatedEntityId] = @EraAttributeId
      AND h.[Verb] IN ('ENTERED', 'EXITED')
),

Entered AS (
    SELECT
        [PersonId],
        [CreatedDateTime] AS [StartDate],
        [RowNum]
    FROM HistoryFiltered
    WHERE [Verb] = 'ENTERED'
),

Exited AS (
    SELECT
        [PersonId],
        [CreatedDateTime] AS [EndDate],
        [RowNum]
    FROM HistoryFiltered
    WHERE [Verb] = 'EXITED'
)

-- Insert eRA sessions into Step table
INSERT INTO [dbo].[Step] (
    [PersonAliasId],
    [StepTypeId],
    [StepStatusId],
    [Order],
    [StartDateTime],
    [EndDateTime],
    [CompletedDateTime],
    [StartDateKey],
    [EndDateKey],
    [CompletedDateKey],
    [CreatedDateTime],
    [ModifiedDateTime],
    [Guid]
)
SELECT
    p.[PrimaryAliasId],
    @StepTypeId,
    CASE
        WHEN x.[EndDate] IS NULL THEN @InProgressStepStatusId
        ELSE @CompleteStepStatusId
    END,
    0,
    e.[StartDate],
    x.[EndDate],
    x.[EndDate],
    CAST(FORMAT(e.[StartDate], 'yyyyMMdd') AS INT),
    CASE
        WHEN x.[EndDate] IS NULL THEN NULL
        ELSE CAST(FORMAT(x.[EndDate], 'yyyyMMdd') AS INT)
    END,
    CASE
        WHEN x.[EndDate] IS NULL THEN NULL
        ELSE CAST(FORMAT(x.[EndDate], 'yyyyMMdd') AS INT)
    END,
    GETDATE(),
    GETDATE(),
    NEWID()
FROM Entered e
LEFT JOIN Exited x
    ON e.[PersonId] = x.[PersonId]
   AND e.[RowNum] = x.[RowNum]
INNER JOIN [dbo].[Person] p
    ON p.[Id] = e.[PersonId];
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
