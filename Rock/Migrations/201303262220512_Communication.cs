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
    public partial class Communication : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderPersonId = c.Int(),
                        Subject = c.String(nullable: false, maxLength: 100),
                        Content = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.SenderPersonId)
                .Index(t => t.SenderPersonId);
            
            CreateIndex( "dbo.Communication", "Guid", true );
            CreateTable(
                "dbo.CommunicationRecipient",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        CommunicationId = c.Int(nullable: false),
                        MergeData = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.Communication", t => t.CommunicationId, cascadeDelete: true)
                .Index(t => t.PersonId)
                .Index(t => t.CommunicationId);
            
            CreateIndex( "dbo.CommunicationRecipient", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.CommunicationRecipient", new[] { "CommunicationId" });
            DropIndex("dbo.CommunicationRecipient", new[] { "PersonId" });
            DropIndex("dbo.Communication", new[] { "SenderPersonId" });
            DropForeignKey("dbo.CommunicationRecipient", "CommunicationId", "dbo.Communication");
            DropForeignKey("dbo.CommunicationRecipient", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Communication", "SenderPersonId", "dbo.Person");
            DropTable("dbo.CommunicationRecipient");
            DropTable("dbo.Communication");
        }
    }
}
