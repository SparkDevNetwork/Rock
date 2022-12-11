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
    /// <summary>
    ///
    /// </summary>
    public partial class InteractionSession1900 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // If the InteractionSession table somehow has a default of '1900-01-01' for the DurationLastCalculatedDateTime column, this will
            // remove that so that it defaults to null instead.
            Sql( @"
/* From https://stackoverflow.com/a/10758357/1755417 */
DECLARE @tableName VARCHAR(MAX) = 'InteractionSession'
DECLARE @columnName VARCHAR(MAX) = 'DurationLastCalculatedDateTime'
DECLARE @ConstraintName NVARCHAR(200)
SELECT @ConstraintName = Name
FROM SYS.DEFAULT_CONSTRAINTS
WHERE PARENT_OBJECT_ID = OBJECT_ID(@tableName)
    AND PARENT_COLUMN_ID = (
        SELECT column_id
        FROM sys.columns
        WHERE NAME = @columnName
            AND object_id = OBJECT_ID(@tableName)
        )
IF @ConstraintName IS NOT NULL BEGIN
    EXEC('ALTER TABLE '+@tableName+' DROP CONSTRAINT ' + @ConstraintName)
END
" );

            // Add a job that will clear out the 1900-01-01's and then recalculate sessions.
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV141DataMigrationsUpdateCurrentSessions1900'
                    AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_CURRENT_SESSIONS_1900}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v14.1 - Update current sessions '
                    , 'Update current sessions that might have 1900-01-01 set as the DurationLastCalculatedDateTime.'
                    , 'Rock.Jobs.PostV141DataMigrationsUpdateCurrentSessions1900'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_CURRENT_SESSIONS_1900}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
