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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17 to update indexes in the Interaction Table.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.0 - Interaction Index Post Migration Job." )]
    [Description( "This job adds the IX_InteractionSessionId_CreatedDateTime index on the Interaction Table." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV17InteractionIndexPostMigration : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            jobMigration.Sql( @"
                IF object_id(N'[dbo].[FK_dbo.Interaction_dbo.PersonAlias_CreatedByPersonAliasId]', N'F') IS NOT NULL
                    ALTER TABLE [dbo].[Interaction] DROP CONSTRAINT [FK_dbo.Interaction_dbo.PersonAlias_CreatedByPersonAliasId]

                IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_CreatedByPersonAliasId' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    DROP INDEX [IX_CreatedByPersonAliasId] ON [dbo].[Interaction]

                IF object_id(N'[dbo].[FK_dbo.Interaction_dbo.PersonAlias_ModifiedByPersonAliasId]', N'F') IS NOT NULL
                    ALTER TABLE [dbo].[Interaction] DROP CONSTRAINT [FK_dbo.Interaction_dbo.PersonAlias_ModifiedByPersonAliasId]

                IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ModifiedByPersonAliasId' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    DROP INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[Interaction]

                IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_PersonAliasId' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    DROP INDEX [IX_PersonAliasId] ON [dbo].[Interaction]

                IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_RelatedEntityTypeId' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    DROP INDEX [IX_RelatedEntityTypeId] ON [dbo].[Interaction]

                IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_InteractionSessionId' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    DROP INDEX [IX_InteractionSessionId] ON [dbo].[Interaction]

                IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_InteractionSessionId_CreatedDateTime' AND object_id = object_id(N'[dbo].[Interaction]', N'U'))
                    CREATE INDEX [IX_InteractionSessionId_CreatedDateTime] ON [dbo].[Interaction]([InteractionSessionId], [CreatedDateTime])
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
