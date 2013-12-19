//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FixPrayerRequestDetailsPageContext : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib Value for Page/BlockPrayer Request Detail/Prayer Comments:Entity Type (FieldType: EntityType)
            AddBlockAttributeValue("FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7","F1BCF615-FBCA-4BC2-A912-C35C0DC04174","f13c8fd2-7702-4c79-a6a9-86440dd5de13");
        }
        
        /// <summary>
        /// There is no down because the previous value is definitely incorrect.
        /// </summary>
        public override void Down()
        {
        }
    }
}
