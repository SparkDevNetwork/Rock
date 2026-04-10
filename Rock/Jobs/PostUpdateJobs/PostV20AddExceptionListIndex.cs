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
    /// Run once job for v20.0 to add an Exception Log index to improve performance of the Exception List block.
    /// </summary>
    [DisplayName( "Rock Update Helper v20.0 - Add Exception Log Index for the Exception List Block" )]
    [Description( "This job will add an Exception Log index to improve performance of the Exception List block." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV20AddExceptionListIndex : RockJob
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
-- Drop index (if it exists).
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Outermost_ParentId_CreatedDateTime' AND object_id = OBJECT_ID(N'[dbo].[ExceptionLog]'))
BEGIN
    DROP INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog];
END

-- Add an Exception Log index to improve performance of the Exception List block.
-- Note that this index is purposefully a filtered index (WHERE [ParentId] IS NULL) while also including that same
-- column within the index proper. This is to reduce the size of the index while also giving the optimizer the index
-- shape it's most often able to use.
CREATE NONCLUSTERED INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog] (
    [ParentId] ASC,
    [CreatedDateTime] ASC
)
INCLUDE ([SiteId], [PageId], [ExceptionType], [Description], [CreatedByPersonAliasId])
WHERE [ParentId] IS NULL;" );

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
