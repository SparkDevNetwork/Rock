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
    public partial class TagGuid : Rock.Migrations.RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.TaggedItem", "EntityGuid", c => c.Guid(nullable: false));
            DropColumn("dbo.TaggedItem", "EntityId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.TaggedItem", "EntityId", c => c.Int());
            DropColumn("dbo.TaggedItem", "EntityGuid");
        }
    }
}
