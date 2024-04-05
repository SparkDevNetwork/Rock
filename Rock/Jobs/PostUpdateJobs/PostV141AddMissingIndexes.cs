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
    /// </summary>
    [DisplayName( "Rock Update Helper v14.1 - Add Missing Indexes" )]
    [Description( "Add some missing indexes. A few PersonAliasId indexes, etc." )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaults.CommandTimeout,
        Category = "General",
        Order = 1,
        Key = AttributeKey.CommandTimeout )]
    public class PostV141AddMissingIndexes : RockJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefaults
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const int CommandTimeout = 60 * 60;
        }

        #endregion Keys

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        public override void Execute()
        {
            // get the configured timeout, or default to 3600 minutes if it is blank
            var commandTimeout = this.GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );


            /* sql to find tables that have a GUID column without a Guid Index

SELECT *
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME NOT IN (
        SELECT TableName = t.name
        FROM sys.indexes ind
        INNER JOIN sys.index_columns ic
            ON ind.object_id = ic.object_id
                AND ind.index_id = ic.index_id
        INNER JOIN sys.columns col
            ON ic.object_id = col.object_id
                AND ic.column_id = col.column_id
        INNER JOIN sys.tables t
            ON ind.object_id = t.object_id
        WHERE col.Name = 'Guid'
        )
    AND table_name IN (
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE COLUMN_NAME = 'Guid'
        )
    AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME

            */

            /* 2024-03-22 DJL

            Tables that store historical data should only have a Guid index if it is necessary to improve the performance for a
            specific use case. These tables have a large number of records that are rarely accessed individually, and adding a
            unique index reduces the performance of the frequent insert operations.

            The History.Guid index has been removed because performance analysis showed that it was not accessed in normal operating conditions,
            yet it comprised approximately 3.5% of the total database size.

            */

            AddMissingUniqueGuidIndexOnTableIfNotExists( jobMigration, migrationHelper, "Attendance" );
            AddMissingUniqueGuidIndexOnTableIfNotExists( jobMigration, migrationHelper, "AuditDetail" );
            AddMissingUniqueGuidIndexOnTableIfNotExists( jobMigration, migrationHelper, "PersonDuplicate" );

            /* 10-28-2022 MDP

            If FK to PersonAlias does not have an Index it could take a long time to delete a PersonAlias record since it would have to do a full table scan on those tables.
            This query will help find those.

            Note that FinancialTransaction.AuthorizedPersonAliasId is included in
            IX_TransactionDateTime_SourceType_AuthorizedPerson_PaymentDetails and IX_TransactionDateTime_TransactionTypeValueId_Person,
            so that might be ok without an index on it

SELECT Object_Name(a.parent_object_id) AS Table_Name
    , b.NAME AS Column_Name
FROM sys.foreign_key_columns a
    , sys.all_columns b
    , sys.objects c
WHERE a.parent_column_id = b.column_id
    AND a.parent_object_id = b.object_id
    AND b.object_id = c.object_id
    AND c.is_ms_shipped = 0
    AND Object_Name(referenced_object_id) = 'PersonAlias'

EXCEPT

SELECT Object_name(a.Object_id)
    , b.NAME
FROM sys.index_columns a
    , sys.all_columns b
    , sys.objects c
WHERE a.object_id = b.object_id
    AND a.key_ordinal = 1
    AND a.column_id = b.column_id
    AND a.object_id = c.object_id
    AND c.is_ms_shipped = 0

             
Attendance	            CreatedByPersonAliasId
Attendance	            ModifiedByPersonAliasId
History	                CreatedByPersonAliasId
History	                ModifiedByPersonAliasId
PersonDuplicate	        CreatedByPersonAliasId
PersonDuplicate	        ModifiedByPersonAliasId
             */

            migrationHelper.CreateIndexIfNotExists( "Attendance", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "Attendance", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "History", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "History", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "PersonDuplicate", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "PersonDuplicate", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );

            // Add a 'Token' index to PageShortLink. This should improve the performance of
            // PageShortLink GetByToken( string token, int siteId ) quite a bit.
            migrationHelper.CreateIndexIfNotExists( "PageShortLink", new string[1] { "Token " }, new string[0] );

            ServiceJobService.DeleteJob( this.GetJobId() );
        }

        private void AddMissingUniqueGuidIndexOnTableIfNotExists( JobMigration jobMigration, MigrationHelper migrationHelper, string tableName )
        {
            // fix up any duplicate Guids in the specific table
            this.UpdateLastStatusMessage( $"Ensuring {tableName} Guids are unique..." );
            jobMigration.Sql( $@"UPDATE [{tableName}]
SET [Guid] = newid()
WHERE [Guid] IN (
        SELECT [Guid]
        FROM (
            SELECT [Guid]
                , count(*) [DuplicateCount]
            FROM [{tableName}]
            GROUP BY [Guid]
            ) x
        WHERE [DuplicateCount] > 1
        )" );

            this.UpdateLastStatusMessage( $"Creating {tableName} IX_GUID index" );
            migrationHelper.CreateIndexIfNotExists( tableName, "IX_Guid", new string[1] { "Guid" }, new string[0], true );
        }
    }
}