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
    /// Run once job for v17.1 to prepare the new HistoryLogin table to record only true login events moving forward.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.1 - Migrate to History Login Table" )]
    [Description( "This job will prepare the new HistoryLogin table to record only true login events moving forward." )]

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

            /*
                5/6/2025 - JPH

                When we originally introduced the new [HistoryLogin] table, we migrated all login-specific [History]
                records (where [Verb] = 'LOGIN') over to the new table, and deleted them from the old table. After
                further investigation, we realized that Rock was adding 'LOGIN' records far too often, making the login
                history far too noisy and therefore, not very helpful. This was because non-login records were being
                falsely reported as logins (e.g. Whenever Rock would start and find a preexisting auth cookie / the
                mobile app would start and issue a "launch packet", a 'LOGIN' record was added, even though these aren't
                true "login" events).

                We've since decided to only create [HistoryLogin] records in the new table when an individual truly logs
                in (when providing a username + password, using passwordless login, going through an OIDC flow, Etc.).
                Furthermore, to clean up the previous migration of noisy data, we've decided to:

                    1. TRUNCATE any preexisting records from the [HistoryLogin] table, so only true logins will be
                       present moving forward. This will only affect the Spark + Triumph sites and any super early beta
                       testers (the Rock community - in general - will not be affected by this step, as Rock v17.1 has
                       not yet been released).
                    2. DELETE - without migrating - any preexisting login-specific [History] records (where [Verb] = 'LOGIN'),
                       since we have no way of differentiating between "true" and "noisy" past records. We'd rather start
                       with a clean and helpful slate than bring over unhelpful mountains of data.

                Another benefit of this precision approach is that the performance of the new Login History block will
                be greatly improved, as it will need to load and display far fewer records.

                Reason: Implement revised strategy for what it means to "migrate" login history.
            */

            // Ensure an index is in place that will significantly improve the performance of this job, as well as
            // future queries against the History table. It might take a while (several minutes) to add this index for
            // Rock instances that have millions of records in the History table.
            using ( var activity = ObservabilityHelper.StartActivity( "Task: Add 'IX_EntityTypeId_Verb' [History] Index" ) )
            {
                jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_EntityTypeId_Verb' AND object_id = OBJECT_ID('History'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_EntityTypeId_Verb] ON [dbo].[History]
    (
        [EntityTypeId] ASC
        , [Verb] ASC
        , [RelatedEntityTypeId] ASC
        , [RelatedEntityId] ASC
        , [CreatedDateTime] ASC
    )
    INCLUDE (
        [EntityId]
    );

    -- Remove any preexisting history login records (most Rock instances won't have any).
    TRUNCATE TABLE [HistoryLogin];
END" );
            }

            // Delete preexisting [History] records in batches of 1500, ensuring the script runs at least once. If any
            // records are deleted within a given batch, we'll try at least once more.
            var shouldContinueDeleting = true;
            var totalRecordsDeletedCount = 0;

            while ( shouldContinueDeleting )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "Task: Delete 'LOGIN' [History] Records" ) )
                {
                    var recordsDeletedCount = ( int ) jobMigration.SqlScalar( @"
DECLARE @PersonEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7');

-- Delete a batch of login-specific [History] records.
WITH LoginHistoryToDelete AS (
    SELECT TOP 1500 [Id]
    FROM [History]
    WHERE [Verb] = 'LOGIN'
        AND [EntityTypeId] = @PersonEntityTypeId
)
DELETE h
FROM [History] h
INNER JOIN LoginHistoryToDelete ON LoginHistoryToDelete.[Id] = h.[Id];

SELECT @@ROWCOUNT AS [RecordsDeletedCount];" );

                    totalRecordsDeletedCount += recordsDeletedCount;

                    activity?.AddTag( "rock.job.batch_deleted_record_count", recordsDeletedCount );
                    activity?.AddTag( "rock.job.total_deleted_record_count", totalRecordsDeletedCount );

                    // If any records were migrated, check once more.
                    shouldContinueDeleting = recordsDeletedCount > 0;
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
