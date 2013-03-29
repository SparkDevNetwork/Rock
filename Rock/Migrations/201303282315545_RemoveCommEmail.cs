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
    public partial class RemoveCommEmail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.CommunicationEmail", "Id", "dbo.Communication");
            DropIndex("dbo.CommunicationEmail", new[] { "Id" });
            DropTable("dbo.CommunicationEmail");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            // TableName: CommunicationEmail
            // The given key was not present in the dictionary.
            CreateIndex("dbo.CommunicationEmail", "Id");
            AddForeignKey("dbo.CommunicationEmail", "Id", "dbo.Communication", "Id");
        }
    }
}
