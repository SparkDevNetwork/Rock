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
    /// Run once job for v17.1 to update an existing index on the CommunicationRecipient table.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.1 - Update CommunicationRecipient Index" )]
    [Description( "This job will update an existing index on the CommunicationRecipient table." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV171UpdateCommunicationRecipientIndex : RockJob
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

            // Drops and recreates the IX_CommunicationId index on CommunicationRecipient to include PersonAliasId.
            // This was determined to be the fix for issue #5651.
            // https://github.com/SparkDevNetwork/Rock/issues/5651
            jobMigration.Sql( @"
-- Drop the existing index.
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_CommunicationId' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
    DROP INDEX [IX_CommunicationId] ON [dbo].[CommunicationRecipient];
END

-- Recreate the index to include PersonAliasId.
CREATE NONCLUSTERED INDEX [IX_CommunicationId] ON [dbo].[CommunicationRecipient] (
    [CommunicationId] ASC
)
INCLUDE ([PersonAliasId]);

-- Drops a temporary index used for testing this fix in a partner environment.
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_CommunicationId_PersonAliasId' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
    DROP INDEX [IX_CommunicationId_PersonAliasId] ON [dbo].[CommunicationRecipient];
END" );

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
