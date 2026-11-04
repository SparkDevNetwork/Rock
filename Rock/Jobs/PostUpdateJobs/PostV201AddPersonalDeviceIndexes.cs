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
    /// Run once job for v20.1 to add the indexes that personal device lookups use.
    /// </summary>
    [DisplayName( "Rock Update Helper v20.1 - Add Personal Device Indexes" )]
    [Description( "This job will add indexes that keep personal device lookups fast." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV201AddPersonalDeviceIndexes : RockJob
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
                11/04/2026 - MSE

                Every Rock Mobile launch looks up its PersonalDevice by DeviceUniqueIdentifier, device type
                and site (MobileController.GetLaunchPacket), and the TV app does the same. No index covered
                that lookup, so each launch read the whole table.

                Reason: Keep device lookups on app launch fast on large tables.
            */
            jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_DeviceUniqueIdentifier_PersonalDeviceTypeValueId_SiteId' AND object_id = OBJECT_ID(N'[dbo].[PersonalDevice]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DeviceUniqueIdentifier_PersonalDeviceTypeValueId_SiteId] ON [dbo].[PersonalDevice] (
        [DeviceUniqueIdentifier] ASC,
        [PersonalDeviceTypeValueId] ASC,
        [SiteId] ASC
    );
END" );

            /*
                11/04/2026 - MSE

                Push tokens are looked up by DeviceRegistrationId (REST API filters and the push transport's
                cleanup of unregistered tokens), but an nvarchar(max) column cannot be an index key. So the
                column is narrowed to nvarchar(450) (900 bytes; with the device type the key is 904 bytes,
                within the 1700 byte nonclustered key limit) and then indexed. Real tokens are far shorter
                (Firebase about 163, APNs 64, Firebase Installation IDs 22). The model still says nvarchar(max)
                on this version. That is harmless because nothing compares the model to the column, and a
                longer value now fails to save instead of being stored.

                Narrowing is skipped, never forced, when any stored value is too long or when something else
                depends on the column (an index, user statistics, a computed column, a constraint or a schema
                bound view, for example one added by a DBA or by automatic tuning). The job then ends with a
                warning that names the blocking object and is not deleted, so it stays visible and tries again
                on the next run. A site that already narrowed and indexed the column itself counts as done.

                Changing the column rewrites every row and leaves the table fragmented. The Database
                Maintenance job rebuilds it on its next run.

                Reason: Index push token lookups on large tables (Fixes #7146).
            */
            var deviceRegistrationIdState = jobMigration.SqlScalar( @"
DECLARE @ObjectId INT = OBJECT_ID(N'[dbo].[PersonalDevice]');
DECLARE @ColumnId INT, @TypeName SYSNAME, @MaxLength SMALLINT, @Dependency NVARCHAR(400);

SELECT @ColumnId = [column_id]
    , @TypeName = TYPE_NAME([system_type_id])
    , @MaxLength = [max_length]
FROM sys.columns
WHERE [object_id] = @ObjectId AND [name] = N'DeviceRegistrationId';

IF @ColumnId IS NULL OR @TypeName <> N'nvarchar'
BEGIN
    SELECT N'UnexpectedColumn';
    RETURN;
END

IF EXISTS (SELECT * FROM sys.index_columns WHERE [object_id] = @ObjectId AND [column_id] = @ColumnId AND [key_ordinal] = 1)
BEGIN
    SELECT N'Done';
    RETURN;
END

IF @MaxLength <> -1 AND @MaxLength <= 900
BEGIN
    SELECT N'CreateIndex';
    RETURN;
END

IF EXISTS (SELECT * FROM [dbo].[PersonalDevice] WHERE DATALENGTH([DeviceRegistrationId]) > 900)
BEGIN
    SELECT N'ValueTooLong';
    RETURN;
END

SELECT TOP 1 @Dependency = [Name]
FROM (
    SELECT N'index ' + [i].[name] AS [Name]
    FROM sys.index_columns AS [ic]
    INNER JOIN sys.indexes AS [i] ON [i].[object_id] = [ic].[object_id] AND [i].[index_id] = [ic].[index_id]
    WHERE [ic].[object_id] = @ObjectId AND [ic].[column_id] = @ColumnId
    UNION ALL
    SELECT N'statistics ' + [s].[name]
    FROM sys.stats AS [s]
    INNER JOIN sys.stats_columns AS [sc] ON [sc].[object_id] = [s].[object_id] AND [sc].[stats_id] = [s].[stats_id]
    WHERE [s].[object_id] = @ObjectId AND [sc].[column_id] = @ColumnId AND [s].[user_created] = 1
    UNION ALL
    SELECT N'statistics ' + [name] FROM sys.stats WHERE [object_id] = @ObjectId AND [has_filter] = 1 AND [filter_definition] LIKE N'%DeviceRegistrationId%'
    UNION ALL
    SELECT N'computed column ' + [name] FROM sys.computed_columns WHERE [object_id] = @ObjectId AND [definition] LIKE N'%DeviceRegistrationId%'
    UNION ALL
    SELECT N'check constraint ' + [name] FROM sys.check_constraints WHERE [parent_object_id] = @ObjectId AND ([parent_column_id] = @ColumnId OR [definition] LIKE N'%DeviceRegistrationId%')
    UNION ALL
    SELECT N'default constraint ' + [name] FROM sys.default_constraints WHERE [parent_object_id] = @ObjectId AND [parent_column_id] = @ColumnId
    UNION ALL
    SELECT N'schema bound object ' + OBJECT_SCHEMA_NAME([referencing_id]) + N'.' + OBJECT_NAME([referencing_id])
    FROM sys.sql_expression_dependencies
    WHERE [referenced_id] = @ObjectId AND [is_schema_bound_reference] = 1 AND ([referenced_minor_id] = @ColumnId OR [referenced_minor_id] = 0)
) AS [d];

IF @Dependency IS NOT NULL
    SELECT N'HasDependency:' + @Dependency;
ELSE
    SELECT N'AlterColumn';" ).ToStringSafe();

            if ( deviceRegistrationIdState == "ValueTooLong" )
            {
                throw new RockJobWarningException( "Skipped indexing PersonalDevice.DeviceRegistrationId because at least one stored value is longer than 450 characters." );
            }

            if ( deviceRegistrationIdState.StartsWith( "HasDependency:" ) )
            {
                var dependency = deviceRegistrationIdState.Substring( "HasDependency:".Length );
                throw new RockJobWarningException( $"Skipped indexing PersonalDevice.DeviceRegistrationId because the {dependency} depends on the column. Remove it and run this job again." );
            }

            if ( deviceRegistrationIdState != "Done" && deviceRegistrationIdState != "CreateIndex" && deviceRegistrationIdState != "AlterColumn" )
            {
                throw new RockJobWarningException( "Skipped indexing PersonalDevice.DeviceRegistrationId because the column is missing or is not nvarchar." );
            }

            if ( deviceRegistrationIdState == "AlterColumn" )
            {
                // Keep the column's current collation; ALTER COLUMN would otherwise switch it to the database default.
                jobMigration.Sql( @"
DECLARE @Collation SYSNAME = (
    SELECT [collation_name]
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'[dbo].[PersonalDevice]') AND [name] = N'DeviceRegistrationId');
DECLARE @Sql NVARCHAR(MAX) = N'ALTER TABLE [dbo].[PersonalDevice] ALTER COLUMN [DeviceRegistrationId] NVARCHAR(450) COLLATE ' + @Collation + N' NULL;';

EXEC sp_executesql @Sql;" );
            }

            if ( deviceRegistrationIdState == "AlterColumn" || deviceRegistrationIdState == "CreateIndex" )
            {
                jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_DeviceRegistrationId_PersonalDeviceTypeValueId' AND object_id = OBJECT_ID(N'[dbo].[PersonalDevice]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DeviceRegistrationId_PersonalDeviceTypeValueId] ON [dbo].[PersonalDevice] (
        [DeviceRegistrationId] ASC,
        [PersonalDeviceTypeValueId] ASC
    );
END" );
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
