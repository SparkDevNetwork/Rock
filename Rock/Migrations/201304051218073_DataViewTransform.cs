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
    public partial class DataViewTransform : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DataView", "TransformEntityTypeId", c => c.Int());
            AddForeignKey("dbo.DataView", "TransformEntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.DataView", "TransformEntityTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.DataView", new[] { "TransformEntityTypeId" });
            DropForeignKey("dbo.DataView", "TransformEntityTypeId", "dbo.EntityType");
            DropColumn("dbo.DataView", "TransformEntityTypeId");
        }
    }
}
