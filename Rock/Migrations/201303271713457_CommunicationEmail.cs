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
    public partial class CommunicationEmail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommunicationEmail",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SenderName = c.String(maxLength: 100),
                        SenderEmail = c.String(maxLength: 100),
                        ReplyToEmail = c.String(maxLength: 100),
                        Cc = c.String(),
                        Bcc = c.String(),
                        HtmlContent = c.String(),
                        TextContent = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Communication", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.Communication", "IsTemporary", c => c.Boolean(nullable: false));
            AddColumn("dbo.Communication", "AdditionalMergeFieldsJson", c => c.String());
            AddColumn("dbo.CommunicationRecipient", "AdditionalMergeValuesJson", c => c.String());
            AlterColumn("dbo.Communication", "Subject", c => c.String(maxLength: 100));
            DropColumn("dbo.Communication", "Content");
            DropColumn("dbo.CommunicationRecipient", "MergeData");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.CommunicationRecipient", "MergeData", c => c.String());
            AddColumn("dbo.Communication", "Content", c => c.String());
            DropIndex("dbo.CommunicationEmail", new[] { "Id" });
            DropForeignKey("dbo.CommunicationEmail", "Id", "dbo.Communication");
            AlterColumn("dbo.Communication", "Subject", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.CommunicationRecipient", "AdditionalMergeValuesJson");
            DropColumn("dbo.Communication", "AdditionalMergeFieldsJson");
            DropColumn("dbo.Communication", "IsTemporary");
            DropTable("dbo.CommunicationEmail");
        }
    }
}
