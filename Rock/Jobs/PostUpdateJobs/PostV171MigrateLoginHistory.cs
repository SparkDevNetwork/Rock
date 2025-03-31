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
using Rock.Observability;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.1 to migrate login history from the History table to the HistoryLogin table.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.1 - Migrate Login History" )]
    [Description( "This job will migrate login history from the History table to the HistoryLogin table." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV171MigrateLoginHistory : RockJob
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

            // Migrate records in batches of 1500, ensuring the script runs at least once. If any records are migrated
            // within a given batch, we'll try at least once more.
            var shouldContinueMigrating = true;

            while ( shouldContinueMigrating )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "Task: Migrate Login History to HistoryLogin Table" ) )
                {
                    var recordsMigratedCount = ( int ) jobMigration.SqlScalar( @"
DECLARE @PersonEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7');

DECLARE @MigrateLoginHistory TABLE
(
    [HistoryId] INT NOT NULL
    , [PersonId] INT NOT NULL
    , [LoginDateTime] DATETIME NOT NULL
    , [UserName] NVARCHAR(250) NOT NULL
    , [RelatedData] NVARCHAR(MAX) NULL
);

-- Select the next batch of records to migrate.
INSERT INTO @MigrateLoginHistory
    SELECT TOP 1500 [Id]        -- [HistoryId]
        , [EntityId]            -- [PersonId]
        , [CreatedDateTime]     -- [LoginDateTime]
        , [ValueName]           -- [UserName]
        , [RelatedData]         -- [RelatedData]
    FROM [History]
    WHERE [Verb] = 'LOGIN'
        AND [EntityTypeId] = @PersonEntityTypeId
        AND [CreatedDateTime] IS NOT NULL
        AND [ValueName] IS NOT NULL
    ORDER BY [Id];

-- Transform and insert records into the [HistoryLogin] table.
INSERT INTO [HistoryLogin]
(
    [Guid]
    , [UserName]
    , [UserLoginId]
    , [PersonAliasId]
    , [LoginAttemptDateTime]
    , [WasLoginSuccessful]
    , [ClientIpAddress]
    , [DestinationUrl]
    , [RelatedDataJson]
)
SELECT NEWID()                  -- [Guid]
    , m.[UserName]              -- [UserName]
    , ul.[Id]                   -- [UserLoginId]
    , pa.[Id]                   -- [PersonAliasId]
    , m.[LoginDateTime]         -- [LoginAttemptDateTime]
    , 1                         -- [WasLoginSuccessful]
    , CASE                      -- [ClientIpAddress]
        WHEN m.[RelatedData] IS NOT NULL
            AND CHARINDEX('from <span class=''field-value''>', m.[RelatedData]) > 0
            AND CHARINDEX('</span>', m.[RelatedData], CHARINDEX('from <span class=''field-value''>', m.[RelatedData])) > 0
        THEN SUBSTRING(
            m.[RelatedData],
            CHARINDEX('from <span class=''field-value''>', m.[RelatedData]) + 31,
            CHARINDEX('</span>', m.[RelatedData], CHARINDEX('from <span class=''field-value''>', m.[RelatedData])) 
                - CHARINDEX('from <span class=''field-value''>', m.[RelatedData]) - 31
        )
        ELSE NULL
      END
    , CASE                      -- [DestinationUrl]
        WHEN m.[RelatedData] IS NOT NULL
            AND CHARINDEX('to <span class=''field-value''>', m.[RelatedData]) > 0
            AND CHARINDEX('</span>', m.[RelatedData], CHARINDEX('to <span class=''field-value''>', m.[RelatedData])) > 0
        THEN SUBSTRING(
            m.[RelatedData],
            CHARINDEX('to <span class=''field-value''>', m.[RelatedData]) + 29,
            CHARINDEX('</span>', m.[RelatedData], CHARINDEX('to <span class=''field-value''>', m.[RelatedData]))
                - CHARINDEX('to <span class=''field-value''>', m.[RelatedData]) - 29
        )
        ELSE NULL
      END
    , CASE                      -- [RelatedDataJson]
        WHEN m.[RelatedData] IS NOT NULL
            AND CHARINDEX(' impersonated by ', m.[RelatedData]) = 1
        THEN '{""ImpersonatedByPersonFullName"":""' +
            REPLACE(
                CASE
                    WHEN CHARINDEX(' to <span', m.[RelatedData]) > 0
                    THEN SUBSTRING(m.[RelatedData], 18, CHARINDEX(' to <span', m.[RelatedData]) - 18)
                    ELSE SUBSTRING(m.[RelatedData], 18, LEN(m.[RelatedData]))
                END,
                '""', '\""'
            ) + '"",""LoginContext"":""Impersonation""}'
        ELSE NULL
      END
FROM @MigrateLoginHistory m
OUTER APPLY (
    SELECT TOP 1 ul.[Id]
    FROM [UserLogin] ul
    WHERE ul.[UserName] = m.[UserName]
    ORDER BY ul.[Id]
) ul
OUTER APPLY (
    SELECT TOP 1 pa.[Id]
    FROM [PersonAlias] pa
    WHERE pa.[PersonId] = m.[PersonId]
        AND pa.[AliasPersonId] = m.[PersonId]
    ORDER BY pa.[Id]
) pa
ORDER BY m.[HistoryId];

-- Delete the migrated records from the [History] table.
DELETE h
FROM [History] h
INNER JOIN @MigrateLoginHistory m
    ON m.[HistoryId] = h.[Id];

SELECT @@ROWCOUNT AS [RecordsMigrated];" );

                    activity?.AddTag( "rock.job.migrated_record_count", recordsMigratedCount );

                    // If any records were migrated, check once more.
                    shouldContinueMigrating = recordsMigratedCount > 0;
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
