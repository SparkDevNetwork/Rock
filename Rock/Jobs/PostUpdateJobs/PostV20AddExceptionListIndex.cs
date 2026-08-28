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
    /// Run once job for v20.0 to add the Exception Log indexes that the Exception List, Exception Occurrences and
    /// Exception Detail blocks rely on.
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

            /*
                8/27/26 - MSE

                Two indexes, one per problem.

                1. IX_Outermost_ParentId_CreatedDateTime serves the Exception List and Exception Occurrences grids,
                   which only ever read outermost exceptions inside a date window. It is filtered to those rows and
                   INCLUDEs everything the grids need, so they are answered from the index alone without touching
                   the table.

                   The first version INCLUDEd [Description], which is unbounded text, so the index grew as large as
                   the messages an install happened to log; on one install it reached roughly the size of the table.
                   It now INCLUDEs [ExceptionGroupHash] instead, a fixed-width column the Exception List block
                   groups by directly in SQL, so the schema sets the index size rather than the data. Descriptions
                   are no longer needed here because the block looks each group's description up by Id afterwards.
                   [ExceptionType] stays because the grid filters on it, shows it per group, and the chart groups
                   by it.

                2. IX_ParentId serves the Exception Detail block, which walks the exception hierarchy through
                   ExceptionLogService.GetByParentId. That lookup passes a nullable parent id, a shape the filtered
                   index above can never satisfy, so every call fell back to a full table scan of ExceptionLog.
                   IX_ParentId is deliberately left unfiltered so the lookup has an index it can actually use.

                This run-once job shipped in early v20.0 builds and deletes its own row, so it was changed in place
                and the AddExceptionLogExceptionGroupHash migration re-registers the same guid: a no-op where the
                row is still pending, a re-insert where it already ran. The first index is dropped and recreated
                rather than created only when missing, which is what replaces the old shape; the SQL is idempotent
                so an extra run is harmless.

                Reason: Bound the Exception List covering index by the schema instead of the data, and stop Exception Detail from scanning the table.
            */
            jobMigration.Sql( @"
IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Outermost_ParentId_CreatedDateTime' AND object_id = OBJECT_ID(N'[dbo].[ExceptionLog]'))
BEGIN
    DROP INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog];
END

CREATE NONCLUSTERED INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog] (
    [ParentId] ASC,
    [CreatedDateTime] ASC
)
INCLUDE ([SiteId], [PageId], [ExceptionType], [ExceptionGroupHash], [CreatedByPersonAliasId])
WHERE [ParentId] IS NULL;

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
