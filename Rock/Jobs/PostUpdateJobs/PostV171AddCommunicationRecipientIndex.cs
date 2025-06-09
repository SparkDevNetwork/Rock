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
using System.ComponentModel;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.1 to add an index in the CommunicationRecipient Table.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.1 - Communication Recipient Index Post Migration Job." )]
    [Description( "This job adds the IX_UniqueMessageId index on the CommunicationRecipient table." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV171AddCommunicationRecipientIndex : PostUpdateJobs.PostUpdateJob
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

            // A simple index was added to the UniqueMessageId field due to a known usage in core for Twilio.  
            // It was also reported by Bill at Bema that a similar issue with ClearStream was causing poor performance. 
            // While we usually prefer to create crafted covering indexes with composite and include columns—this case justified a single-column index.
            jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_UniqueMessageId' AND object_id = OBJECT_ID('CommunicationRecipient'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UniqueMessageId] ON [dbo].[CommunicationRecipient]
    (
        [UniqueMessageId]
    );
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
