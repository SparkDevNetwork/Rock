namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class PageContext : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("cmsPageRoute", "PageId", "cmsPage");
            DropForeignKey("cmsSiteDomain", "SiteId", "cmsSite");
            DropIndex("cmsPageRoute", new[] { "PageId" });
            DropIndex("cmsSiteDomain", new[] { "SiteId" });
            CreateTable(
                "cmsPageContext",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(nullable: false),
                        Entity = c.String(nullable: false, maxLength: 200),
                        IdParameter = c.String(nullable: false, maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PageId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            AddForeignKey("cmsPageRoute", "PageId", "cmsPage", "Id", cascadeDelete: true);
            AddForeignKey("cmsSiteDomain", "SiteId", "cmsSite", "Id", cascadeDelete: true);
            CreateIndex("cmsPageRoute", "PageId");
            CreateIndex("cmsSiteDomain", "SiteId");

            Sql( @"
UPDATE cmsPage SET [Layout] = 'Default' WHERE [Guid] = 'F8657CB3-C97B-4F24-82C4-B93579A38B4F'
UPDATE cmsBlockInstance SET [Zone] = 'Content', [Name] = 'Person Edit' WHERE [GUID] = '6E189D68-C4EC-443F-B409-1EEC0F12D427'
INSERT INTO [cmsPageRoute] ([IsSystem],[PageId],[Route],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,9,'Person/{PersonId}','Jul 31 2012 06:00:00:000AM','Jul 31 2012 06:00:00:000AM',1,1,'7E97823A-78A8-4E8E-A337-7A20F2DA9E52')
INSERT INTO [cmsPageContext] ([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,9,'Rock.CRM.Person','PersonId','Jul 31 2012 06:00:00:000AM','Jul 31 2012 06:00:00:000AM',1,1,'09CCA802-EE6E-48FB-AD1B-AA603BA13B99')
" );
        }
        
        public override void Down()
        {
             Sql( @"
DELETE [cmsPageContext] WHERE [Guid] = '09CCA802-EE6E-48FB-AD1B-AA603BA13B99'
DELETE [cmsPageRoute] WHERE [Guid] = '7E97823A-78A8-4E8E-A337-7A20F2DA9E52'
" );

           DropIndex("cmsSiteDomain", new[] { "SiteId" });
            DropIndex("cmsPageContext", new[] { "ModifiedByPersonId" });
            DropIndex("cmsPageContext", new[] { "CreatedByPersonId" });
            DropIndex("cmsPageContext", new[] { "PageId" });
            DropIndex("cmsPageRoute", new[] { "PageId" });
            DropForeignKey("cmsSiteDomain", "SiteId", "cmsSite");
            DropForeignKey("cmsPageContext", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsPageContext", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsPageContext", "PageId", "cmsPage");
            DropForeignKey("cmsPageRoute", "PageId", "cmsPage");
            DropTable("cmsPageContext");
            CreateIndex("cmsSiteDomain", "SiteId");
            CreateIndex("cmsPageRoute", "PageId");
            AddForeignKey("cmsSiteDomain", "SiteId", "cmsSite", "Id");
            AddForeignKey("cmsPageRoute", "PageId", "cmsPage", "Id");
        }
    }
}
