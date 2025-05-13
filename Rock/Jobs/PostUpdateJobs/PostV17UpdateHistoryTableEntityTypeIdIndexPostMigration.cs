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

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// This job updates the IX_EntityTypeId_EntityId index on the dbo.History table to add includes for:
    /// [RelatedEntityTypeId], [RelatedEntityId], [CategoryId], [CreatedByPersonAliasId], [CreatedDateTime].
    /// </summary>
    [DisplayName( "Rock Update Helper v17.0 - History Index Update - Add Includes" )]
    [Description( @"This job updates the IX_EntityTypeId_EntityId index on the dbo.History table to add includes for: 
                [RelatedEntityTypeId], [RelatedEntityId], [CategoryId], [CreatedByPersonAliasId], [CreatedDateTime]." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of data, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV17UpdateHistoryTableEntityTypeIdIndexPostMigration : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                /*
	                08/26/2024 - JC

	                Updates the IX_EntityTypeId_EntityId to include some commonly used int and DateTime columns.
                    These changes increased the size of the index from 2GB to 5GB in an environment with 130 million records.
                    However; they decreased the logical reads for the HistoryLog block query from ~ 1.5 million to < 5k pages
                    (about 11.5 GB to about 35 MB) and removed thousands of key lookups per query execution.
                    
	                Reason: HistoryLog block Performance
                */
                rockContext.Database.ExecuteSqlCommand( @"
CREATE NONCLUSTERED INDEX [IX_EntityTypeId_EntityId] ON [dbo].[History]
(
	[EntityTypeId] ASC,
	[EntityId] ASC
)
INCLUDE (
    [RelatedEntityTypeId],
    [RelatedEntityId],
    [CategoryId],
    [CreatedByPersonAliasId],
    [CreatedDateTime]
)
WITH ( DROP_EXISTING = ON )
" );

                UpdateLastStatusMessage( $"Updated the IX_EntityTypeId_EntityId index on dbo.History" );
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
