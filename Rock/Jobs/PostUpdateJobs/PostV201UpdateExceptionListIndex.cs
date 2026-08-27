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
    /// Run once job for v20.1 to update the Exception Log indexes that the Exception List, Exception Occurrences and
    /// Exception Detail blocks rely on, replacing the index shape that the v20.0 job built.
    /// </summary>
    [DisplayName( "Rock Update Helper v20.1 - Update Exception Log Indexes" )]
    [Description( "This job will update the Exception Log indexes used by the Exception List, Exception Occurrences and Exception Detail blocks." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV201UpdateExceptionListIndex : RockJob
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
                8/27/26 - MSE

                The v20.0 job (PostV20AddExceptionListIndex) built IX_Outermost_ParentId_CreatedDateTime with
                INCLUDE ([Description]), an nvarchar(max) column, which made the size of the index depend on how long
                an install's exception messages were (on one install it grew to roughly the size of the table). This
                job rebuilds it to INCLUDE the bounded [ExceptionGroupKey] computed column instead, which the
                Exception List block groups by in SQL.

                v20.0 had already shipped, so the v20.0 job was retired rather than edited (see the note in that
                class) and this job was added with its own guid, matching how earlier index fixes were shipped.

                Reason: Replace the unbounded v20.0 index shape on every install, whether or not the v20.0 job ran.
            */
            jobMigration.Sql( @"
-- Drop the index (if it exists). The v20.0 shape INCLUDEd the unbounded [Description] column.
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Outermost_ParentId_CreatedDateTime' AND object_id = OBJECT_ID(N'[dbo].[ExceptionLog]'))
BEGIN
    DROP INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog];
END

-- Recreate the index to include the bounded [ExceptionGroupKey] computed column instead of [Description].
-- Note that this index is purposefully a filtered index (WHERE [ParentId] IS NULL) while also including that same
-- column within the index proper. This is to reduce the size of the index while also giving the optimizer the index
-- shape it's most often able to use.
CREATE NONCLUSTERED INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog] (
    [ParentId] ASC,
    [CreatedDateTime] ASC
)
INCLUDE ([SiteId], [PageId], [ExceptionType], [ExceptionGroupKey], [CreatedByPersonAliasId])
WHERE [ParentId] IS NULL;

-- Add an index on [ParentId] for the Exception Detail block, which loads an exception's inner exceptions by their
-- parent. This index is purposefully NOT filtered: EF6 compiles the nullable parent identifier parameter as
-- ""[ParentId] = @parentId OR ([ParentId] IS NULL AND @parentId IS NULL)"", which a filtered index cannot satisfy.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_ParentId' AND object_id = OBJECT_ID(N'[dbo].[ExceptionLog]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParentId] ON [dbo].[ExceptionLog] (
        [ParentId] ASC
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
