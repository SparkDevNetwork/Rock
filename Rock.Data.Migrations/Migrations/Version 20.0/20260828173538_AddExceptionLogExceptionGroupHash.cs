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
namespace Rock.Data.Migrations;

/// <summary>
/// Adds the ExceptionGroupHash computed column to the ExceptionLog table and registers the Exception Log index
/// job again so it rebuilds the indexes around the new column.
/// </summary>
[SqlMigration( 20260828173538, "20.0" )]
internal class AddExceptionLogExceptionGroupHash : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        MSE_AddExceptionGroupHashColumn_Up();
        MSE_AddExceptionListIndexJob_Up();
    }

    #region Add ExceptionGroupHash Column

    /// <summary>
    /// MSE: Adds the ExceptionGroupHash computed column to the ExceptionLog table - up.
    /// </summary>
    /*
        8/28/26 - MSE

        The scaffolded AddColumn was replaced with SQL because EF cannot express a computed column. It is
        deliberately not PERSISTED: a non-persisted computed column is a metadata-only change, instant even on
        tables with millions of rows, and its only reader is the index the PostV20AddExceptionListIndex job
        builds, which stores its own copy. Persisting it would write the value twice and rewrite the whole table
        for no reader.

        The value hashes "{ExceptionType}|{first 255 characters of Description}", the same text the Exception
        List block groups and displays, rather than storing that text directly, because BINARY(32) is bounded by
        the schema while the text grows with an install's exception messages: 142 bytes per row in the covering
        index against 449 for the text over 500,000 outermost exceptions, and unlike the text it does not grow
        as descriptions lengthen. HASHBYTES hashes UTF-16 bytes, so grouping is case and accent sensitive
        regardless of collation, matching the ordinal comparison the previous in-memory grouping used.

        Three parts of the definition are load bearing:
          - Use +, not CONCAT(), which yields the non-indexable nvarchar(max).
          - The inner CAST bounds LEFT(), which also yields nvarchar(max) for an nvarchar(max) input.
          - CONVERT( BINARY(32), ... ) pins HASHBYTES' declared varbinary(8000) to a fixed 32 bytes so the
            column sits in the fixed portion of the index row. The 32 must match the algorithm's output width:
            BINARY(n) silently right-pads when too large and truncates when too small.

        Verified after applying: COLUMNPROPERTY IsDeterministic, IsPrecise and IsIndexable are all 1, and the
        block's queries seek IX_Outermost_ParentId_CreatedDateTime with no key lookup.

        Reason: EF cannot scaffold a computed column, and the definition has to stay indexable and fixed width.
    */
    private void MSE_AddExceptionGroupHashColumn_Up()
    {
        // AddColumn( "dbo.ExceptionLog", "ExceptionGroupHash", c => c.Binary( maxLength: 32, fixedLength: true ) );
        Sql( @"
IF COL_LENGTH( 'dbo.ExceptionLog', 'ExceptionGroupHash' ) IS NULL
BEGIN
    -- SHA-256 of {ExceptionType}|{first 255 characters of Description}, with a NULL type or description treated as empty.
    ALTER TABLE [dbo].[ExceptionLog] ADD [ExceptionGroupHash] AS CONVERT( BINARY(32), HASHBYTES( 'SHA2_256', ISNULL( [ExceptionType], N'' ) + N'|' + ISNULL( CAST( LEFT( [Description], 255 ) AS NVARCHAR(255) ), N'' ) ) );
END" );
    }

    #endregion Add ExceptionGroupHash Column

    #region Add Exception List Index Job

    /// <summary>
    /// MSE: Registers the Exception Log index job again so it rebuilds the indexes around the new column - up.
    /// </summary>
    /*
        8/27/26 - MSE

        PostV20AddExceptionListIndex, registered by Rollup_20260520, originally built the Exception List index
        with INCLUDE ([Description]) and now uses INCLUDE ([ExceptionGroupHash]). It is a run-once job that
        deletes its own row, so where the original already ran nothing would rebuild the index. Registering the
        same guid again covers both cases, since AddPostUpdateServiceJob only inserts when no row exists: a
        no-op where the job is still pending, a re-insert where it already ran. The values match
        Rollup_20260520 so both paths produce the same row.

        Reason: Make the updated index job run on installs where the original version already ran and deleted itself.
    */
    private void MSE_AddExceptionListIndexJob_Up()
    {
        RockMigrationHelper.AddPostUpdateServiceJob(
            name: "Rock Update Helper v20.0 - Add Exception Log Index for the Exception List Block",
            description: "This job will add an Exception Log index to improve performance of the Exception List block.",
            jobType: "Rock.Jobs.PostV20AddExceptionListIndex",
            cronExpression: "0 0 2 1/1 * ? *",
            guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_200_ADD_EXCEPTION_LIST_INDEX );
    }

    #endregion Add Exception List Index Job
}
