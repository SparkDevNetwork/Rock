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
            CreatePersonBirthdatePersistedIndexed();
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
            Sql( @"
IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BirthDate' AND object_id = OBJECT_ID('Person'))
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

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BirthDate' AND object_id = OBJECT_ID('Person'))
BEGIN
    CREATE INDEX [IX_BirthDate] ON [Person] ([BirthDate])
END
" );
        }

    }
}
