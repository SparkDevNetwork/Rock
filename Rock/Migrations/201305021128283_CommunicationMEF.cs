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
            AddColumn("dbo.Communication", "FutureSendDateTime", c => c.DateTime());
            AddColumn("dbo.Communication", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "ReviewerPersonId", c => c.Int());
            AddColumn("dbo.Communication", "ReviewedDateTime", c => c.DateTime());
            AddColumn("dbo.Communication", "ReviewerNote", c => c.String());
            AddColumn("dbo.Communication", "ChannelEntityTypeId", c => c.Int());
            AddColumn("dbo.Communication", "ChannelDataJson", c => c.String());
            AddColumn("dbo.CommunicationRecipient", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationRecipient", "StatusNote", c => c.String());
            AddForeignKey("dbo.Communication", "ReviewerPersonId", "dbo.Person", "Id");
            AddForeignKey("dbo.Communication", "ChannelEntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.Communication", "ReviewerPersonId");
            CreateIndex("dbo.Communication", "ChannelEntityTypeId");
            DropColumn("dbo.Communication", "IsTemporary");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Communication", "IsTemporary", c => c.Boolean(nullable: false));
            DropIndex("dbo.Communication", new[] { "ChannelEntityTypeId" });
            DropIndex("dbo.Communication", new[] { "ReviewerPersonId" });
            DropForeignKey("dbo.Communication", "ChannelEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.Communication", "ReviewerPersonId", "dbo.Person");
            DropColumn("dbo.CommunicationRecipient", "StatusNote");
            DropColumn("dbo.CommunicationRecipient", "Status");
            DropColumn("dbo.Communication", "ChannelDataJson");
            DropColumn("dbo.Communication", "ChannelEntityTypeId");
            DropColumn("dbo.Communication", "ReviewerNote");
            DropColumn("dbo.Communication", "ReviewedDateTime");
            DropColumn("dbo.Communication", "ReviewerPersonId");
            DropColumn("dbo.Communication", "Status");
            DropColumn("dbo.Communication", "FutureSendDateTime");
        }
    }
}
