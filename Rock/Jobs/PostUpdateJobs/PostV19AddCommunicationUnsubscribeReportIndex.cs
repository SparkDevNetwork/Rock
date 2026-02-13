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
    /// Run once job for v19.0 to add an index to ensure the Communication Unsubscribe Report performs efficiently.
    /// </summary>
    [DisplayName( "Rock Update Helper v19.0 - Add Index For Communication Unsubscribe Report" )]
    [Description( "This job will add an index to ensure the Communication Unsubscribe Report performs efficiently." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV19AddCommunicationUnsubscribeReportIndex : RockJob
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
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_UnsubscribeDateTime_SendDateTime' AND object_id = OBJECT_ID(N'[dbo].[CommunicationRecipient]'))
BEGIN
    DROP INDEX [IX_UnsubscribeDateTime_SendDateTime] ON [dbo].[CommunicationRecipient];
END

CREATE NONCLUSTERED INDEX [IX_UnsubscribeDateTime_SendDateTime] ON [dbo].[CommunicationRecipient] (
    [UnsubscribeDateTime] ASC,
    [SendDateTime] ASC
)
INCLUDE ([PersonAliasId], [UnsubscribeLevel], [CommunicationId])
WHERE [UnsubscribeDateTime] IS NOT NULL
    AND [SendDateTime] IS NOT NULL
    AND [UnsubscribeLevel] IS NOT NULL
    AND [PersonAliasId] IS NOT NULL;" );

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
