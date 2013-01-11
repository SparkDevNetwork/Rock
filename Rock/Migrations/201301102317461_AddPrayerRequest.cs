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
    public partial class AddPrayerRequest : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PrayerRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(maxLength: 254),
                        RequestedByPersonId = c.Int(),
                        CategoryId = c.Int(),
                        Text = c.String(nullable: false),
                        Answer = c.String(),
                        EnteredDate = c.DateTime(nullable: false),
                        ExpirationDate = c.DateTime(),
                        GroupId = c.Int(),
                        AllowComments = c.Boolean(),
                        IsUrgent = c.Boolean(),
                        IsPublic = c.Boolean(),
                        IsActive = c.Boolean(),
                        IsApproved = c.Boolean(),
                        FlagCount = c.Int(),
                        PrayerCount = c.Int(),
                        ApprovedByPersonId = c.Int(),
                        ApprovedOnDate = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.RequestedByPersonId)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.ApprovedByPersonId)
                .Index(t => t.RequestedByPersonId)
                .Index(t => t.CategoryId)
                .Index(t => t.GroupId)
                .Index(t => t.ApprovedByPersonId);
            
            CreateIndex( "dbo.PrayerRequest", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.PrayerRequest", new[] { "ApprovedByPersonId" });
            DropIndex("dbo.PrayerRequest", new[] { "GroupId" });
            DropIndex("dbo.PrayerRequest", new[] { "CategoryId" });
            DropIndex("dbo.PrayerRequest", new[] { "RequestedByPersonId" });
            DropForeignKey("dbo.PrayerRequest", "ApprovedByPersonId", "dbo.Person");
            DropForeignKey("dbo.PrayerRequest", "GroupId", "dbo.Group");
            DropForeignKey("dbo.PrayerRequest", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.PrayerRequest", "RequestedByPersonId", "dbo.Person");
            DropTable("dbo.PrayerRequest");
        }
    }
}
