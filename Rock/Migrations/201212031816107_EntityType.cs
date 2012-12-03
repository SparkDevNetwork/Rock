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
    public partial class EntityType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "AssemblyName", c => c.String(maxLength: 200));
            AddColumn("dbo.EntityType", "IsEntity", c => c.Boolean(nullable: false));
            AddColumn("dbo.EntityType", "IsSecured", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.EntityType", "IsSecured");
            DropColumn("dbo.EntityType", "IsEntity");
            DropColumn("dbo.EntityType", "AssemblyName");
        }
    }
}
