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
    public partial class RenameGroupLocationIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.GroupLocation", "GroupLocationTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.GroupLocation", new[] { "GroupLocationTypeValueId" });
            CreateIndex("dbo.GroupLocation", "GroupLocationTypeValueId");
            AddForeignKey("dbo.GroupLocation", "GroupLocationTypeValueId", "dbo.DefinedValue", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupLocation", "GroupLocationTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.GroupLocation", new[] { "GroupLocationTypeValueId" });
            CreateIndex("dbo.GroupLocation", "GroupLocationTypeValueId");
            AddForeignKey("dbo.GroupLocation", "GroupLocationTypeValueId", "dbo.DefinedValue", "Id");
        }
    }
}
