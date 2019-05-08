namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 74, "1.8.8" )]
    public class MigrationRollupsForV8_8_1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixRockInstanceIDs();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// ED: Fix Rock Instance IDs F8ABA66C-C73D-441F-9A28-233D803EFB0D
        /// </summary>
        private void FixRockInstanceIDs()
        {
            Sql( @"
                -- Update to a unique ID
                UPDATE [dbo].[Attribute]
                SET [Guid] = NEWID()
                WHERE [EntityTypeQualifierColumn] = 'systemsetting'
                 AND [Key] = 'RockInstanceId'
                 AND [Guid] = 'F8ABA66C-C73D-441F-9A28-233D803EFB0D'" );
        }
    }
}
