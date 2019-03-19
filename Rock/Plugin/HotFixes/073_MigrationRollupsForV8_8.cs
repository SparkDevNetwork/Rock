namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 73, "1.8.8" )]
    public class MigrationRollupsForV8_8 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //RemoveWindowsJobSchedulerServiceValue();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }


        /// <summary>
        ///NA: Remove Windows Job Scheduler Service Defined Value
        /// </summary>
        private void RemoveWindowsJobSchedulerServiceValue()
        {
            RockMigrationHelper.DeleteDefinedValue( "98E421D8-0F9D-484B-B2EA-2F6FEE8E785D" );
        }
    }
}
