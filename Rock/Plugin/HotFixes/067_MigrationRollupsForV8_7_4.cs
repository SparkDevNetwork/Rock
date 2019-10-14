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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///MigrationRollupsForV8_7_4
    /// </summary>
    [MigrationNumber( 67, "1.8.6" )]
    public class MigrationRollupsForV8_7_4 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //CreatePersonBirthdatePersistedIndexed();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// Re-creates the person BirthDate column as a computed-persisted column and adds an index on it
        /// (based on discussions on https://github.com/SparkDevNetwork/Rock/pull/3507)
        /// </summary>
        private void CreatePersonBirthdatePersistedIndexed()
        {
            // NOTE: Sql Server 2012 (prior to SP4) has a bug where TRY_CONVERT thinks that style 126 is non-deterministic
            Sql( @"
if (charindex( 'SQL Server 2012', @@VERSION) = 0 or charindex( 'SQL Server 2012 (SP4', @@VERSION) > 0)
BEGIN
	IF EXISTS (
			SELECT *
			FROM sys.indexes
			WHERE name = 'IX_BirthDate'
				AND object_id = OBJECT_ID('Person')
			)
	BEGIN
		DROP INDEX [IX_BirthDate] ON [Person]
	END

	ALTER TABLE [Person]

	DROP COLUMN BirthDate

	-- Calculate Birthdate using TRY_CONVERT to guard against bad dates, and use 126 style so that it'll parse as style 126 (ISO-8601) which is deterministic which will let it be persisted
	ALTER TABLE [Person] ADD [BirthDate] AS CASE 
			WHEN BirthYear IS NOT NULL
				THEN TRY_CONVERT(DATE, CONVERT(VARCHAR, [BirthYear]) + '-' + CONVERT(VARCHAR, [BirthMonth]) + '-' + CONVERT(VARCHAR, [BirthDay]), 126)
			ELSE NULL
			END PERSISTED

	IF NOT EXISTS (
			SELECT *
			FROM sys.indexes
			WHERE name = 'IX_BirthDate'
				AND object_id = OBJECT_ID('Person')
			)
	BEGIN
		CREATE INDEX [IX_BirthDate] ON [Person] ([BirthDate])
	END
END
" );
        }

    }
}
