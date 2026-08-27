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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Adds the ExceptionGroupKey computed column to the ExceptionLog table and replaces the v20.0 Exception Log
    /// index job with the v20.1 job that rebuilds the indexes around the new column.
    /// </summary>
    public partial class AddExceptionLogExceptionGroupKey : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MSE_AddExceptionGroupKeyColumn_Up();
            MSE_UpdateExceptionListIndexJob_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MSE_UpdateExceptionListIndexJob_Down();
            MSE_AddExceptionGroupKeyColumn_Down();
        }

        #region Add ExceptionGroupKey Column

        /// <summary>
        /// MSE: Adds the ExceptionGroupKey computed column to the ExceptionLog table - up.
        /// </summary>
        /*
            8/26/26 - MSE

            The scaffolded AddColumn was replaced with the SQL below because ExceptionGroupKey is a computed column,
            which EF cannot express. The column is deliberately not PERSISTED: adding a non-persisted computed column
            is a metadata-only change (no rows are rewritten, so it is instant even on tables with millions of rows),
            and the only consumer of the value is the index built by the PostV201UpdateExceptionListIndex job, which
            stores its own copy of it.

            The definition uses the + operator rather than CONCAT() because CONCAT() yields nvarchar(max), which is
            not indexable. The inner CAST bounds LEFT(), which also yields nvarchar(max) for an nvarchar(max) input.
            The 255 must match ExceptionLogService.DescriptionGroupingPrefixLength, and nvarchar(406) is the maximum
            length of the key: 150 (ExceptionType) + 1 (separator) + 255.

            The column uses the database's default collation, so grouping by it is case-insensitive on Rock's
            default collation (the previous in-memory grouping compared ordinally). This is intentional: descriptions
            that differ only by case are the same error.

            Reason: EF cannot scaffold a computed column, and the definition has to stay indexable.
        */
        private void MSE_AddExceptionGroupKeyColumn_Up()
        {
            // AddColumn( "dbo.ExceptionLog", "ExceptionGroupKey", c => c.String( maxLength: 406 ) );
            Sql( @"
IF COL_LENGTH( 'dbo.ExceptionLog', 'ExceptionGroupKey' ) IS NULL
BEGIN
    -- {ExceptionType}|{first 255 characters of Description}, with a NULL type or description treated as empty.
    ALTER TABLE [dbo].[ExceptionLog] ADD [ExceptionGroupKey] AS CAST( ISNULL( [ExceptionType], N'' ) + N'|' + ISNULL( CAST( LEFT( [Description], 255 ) AS NVARCHAR(255) ), N'' ) AS NVARCHAR(406) );
END" );
        }

        /// <summary>
        /// MSE: Adds the ExceptionGroupKey computed column to the ExceptionLog table - down.
        /// </summary>
        private void MSE_AddExceptionGroupKeyColumn_Down()
        {
            // Once the v20.1 post update job has run, IX_Outermost_ParentId_CreatedDateTime INCLUDEs the column and
            // SQL Server will not drop a column that an index references, so the index has to go first. The Exception
            // List index is then absent until Up() runs again and the v20.1 job rebuilds it.
            Sql( @"
IF EXISTS ( SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Outermost_ParentId_CreatedDateTime' AND [object_id] = OBJECT_ID( N'[dbo].[ExceptionLog]' ) )
BEGIN
    DROP INDEX [IX_Outermost_ParentId_CreatedDateTime] ON [dbo].[ExceptionLog];
END" );

            DropColumn( "dbo.ExceptionLog", "ExceptionGroupKey" );
        }

        #endregion Add ExceptionGroupKey Column

        #region Update Exception List Index Job

        /// <summary>
        /// MSE: Retires the v20.0 Exception Log index job and registers the v20.1 job that updates the indexes - up.
        /// </summary>
        /*
            8/27/26 - MSE

            PostV20AddExceptionListIndex (registered by Rollup_20260520, and before that by the 283 plugin hotfix)
            built the Exception List index with INCLUDE ([Description]). v20.0 had already shipped, so that job was
            retired rather than edited and PostV201UpdateExceptionListIndex was added with its own guid, matching how
            earlier index fixes were shipped (for example PostV171UpdateCommunicationRecipientIndex).

            The v20.0 job's row is deleted here because it may still be pending on an install that upgraded to v20.0
            shortly before this migration runs, and the original job dropped and recreated the index unconditionally,
            which would undo the v20.1 index if it ran afterwards. Installs that already ran it have no row (the job
            deletes itself) and Rollup_20260520 no longer registers it, so after this migration only the v20.1 job
            remains. It runs on every install: those that never ran the v20.0 job get the index created, those that
            did get it rebuilt.

            Reason: Replace the shipped v20.0 index job with a v20.1 successor without the two ever running side by side.
        */
        private void MSE_UpdateExceptionListIndexJob_Up()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_200_ADD_EXCEPTION_LIST_INDEX}'" );

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v20.1 - Update Exception Log Indexes",
                description: "This job will update the Exception Log indexes used by the Exception List, Exception Occurrences and Exception Detail blocks.",
                jobType: "Rock.Jobs.PostV201UpdateExceptionListIndex",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_201_UPDATE_EXCEPTION_LIST_INDEX );
        }

        /// <summary>
        /// MSE: Removes the v20.1 Exception Log index job - down.
        /// </summary>
        private void MSE_UpdateExceptionListIndexJob_Down()
        {
            // The v20.0 job is intentionally not registered again: its index shape is what this change replaces.
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_201_UPDATE_EXCEPTION_LIST_INDEX}'" );
        }

        #endregion Update Exception List Index Job
    }
}
