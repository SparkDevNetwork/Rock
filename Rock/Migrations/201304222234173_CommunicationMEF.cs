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
    public partial class CommunicationMEF : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Communication", "CommunicationChannelEntityTypeId", c => c.Int(nullable: false));
            AddForeignKey("dbo.Communication", "CommunicationChannelEntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.Communication", "CommunicationChannelEntityTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Communication", new[] { "CommunicationChannelEntityTypeId" });
            DropForeignKey("dbo.Communication", "CommunicationChannelEntityTypeId", "dbo.EntityType");
            DropColumn("dbo.Communication", "CommunicationChannelEntityTypeId");
        }
    }
}
