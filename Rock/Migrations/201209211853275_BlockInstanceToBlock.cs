namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlockInstanceToBlock : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.cmsBlockInstance", "BlockTypeId", "dbo.cmsBlockType");
            DropForeignKey("cmsBlockInstance", "PageId", "cmsPage");
            DropForeignKey("cmsBlockInstance", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsBlockInstance", "ModifiedByPersonId", "crmPerson");
			DropIndex( "cmsBlockInstance", new[] { "BlockTypeId" } );
			DropIndex( "cmsBlockInstance", new[] { "PageId" } );
			DropIndex( "cmsBlockInstance", new[] { "CreatedByPersonId" } );
			DropIndex( "cmsBlockInstance", new[] { "ModifiedByPersonId" } );

            DropForeignKey("cmsHtmlContent", "BlockId", "cmsBlockInstance");
			DropIndex( "cmsHtmlContent", new[] { "BlockId" } );
            
            CreateTable(
                "dbo.cmsBlock",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(),
                        Layout = c.String(maxLength: 100),
                        BlockTypeId = c.Int(nullable: false),
                        Zone = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        OutputCacheDuration = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.cmsBlockType", t => t.BlockTypeId, cascadeDelete: true)
                .ForeignKey("dbo.cmsPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.BlockTypeId)
                .Index(t => t.PageId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);

			Sql( @"
				SET IDENTITY_INSERT cmsBlock ON
				INSERT INTO [cmsBlock] (
					 [Id]
					,[IsSystem]
					,[PageId]
					,[Layout]
					,[Zone]
					,[Order]
					,[Name]
					,[OutputCacheDuration]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
					,[BlockTypeId] )
				SELECT 
					 [Id]
					,[IsSystem]
					,[PageId]
					,[Layout]
					,[Zone]
					,[Order]
					,[Name]
					,[OutputCacheDuration]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
					,[BlockTypeId]
				FROM [cmsBlockInstance]
				SET IDENTITY_INSERT cmsBlock OFF

				UPDATE [coreAttribute] SET [Entity] = 'Rock.Cms.Block' WHERE [Entity] = 'Rock.Cms.BlockInstance'
				UPDATE [cmsPageRoute] SET [Route] = 'BlockProperties/{BlockId}' WHERE [Guid] = '6438C940-96F7-4A7E-9DA5-A30FD4FF143A'
" );
            
            AddForeignKey("dbo.cmsHtmlContent", "BlockId", "dbo.cmsBlock", "Id", cascadeDelete: true);
            CreateIndex("dbo.cmsHtmlContent", "BlockId");

            DropTable("dbo.cmsBlockInstance");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.cmsBlockInstance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(),
                        Layout = c.String(maxLength: 100),
                        BlockTypeId = c.Int(nullable: false),
                        Zone = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        OutputCacheDuration = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);


			Sql( @"
				SET IDENTITY_INSERT cmsBlockInstance ON
				INSERT INTO [cmsBlockInstance] (
					 [Id]
					,[IsSystem]
					,[PageId]
					,[Layout]
					,[Zone]
					,[Order]
					,[Name]
					,[OutputCacheDuration]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
					,[BlockTypeId] )
				SELECT 
					 [Id]
					,[IsSystem]
					,[PageId]
					,[Layout]
					,[Zone]
					,[Order]
					,[Name]
					,[OutputCacheDuration]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
					,[BlockTypeId]
				FROM [cmsBlock]
				SET IDENTITY_INSERT cmsBlockInstance OFF

				UPDATE [coreAttribute] SET [Entity] = 'Rock.Cms.BlockInstance' WHERE [Entity] = 'Rock.Cms.Block'
				UPDATE [cmsPageRoute] SET [Route] = 'BlockProperties/{BlockInstance}' WHERE [Guid] = '6438C940-96F7-4A7E-9DA5-A30FD4FF143A'
" );
			DropIndex( "dbo.cmsHtmlContent", new[] { "BlockId" } );
			DropForeignKey( "dbo.cmsHtmlContent", "BlockId", "dbo.cmsBlock" );
			
			DropIndex( "dbo.cmsBlock", new[] { "ModifiedByPersonId" } );
            DropIndex("dbo.cmsBlock", new[] { "CreatedByPersonId" });
            DropIndex("dbo.cmsBlock", new[] { "PageId" });
            DropIndex("dbo.cmsBlock", new[] { "BlockTypeId" });
            DropForeignKey("dbo.cmsBlock", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.cmsBlock", "CreatedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.cmsBlock", "PageId", "dbo.cmsPage");
            DropForeignKey("dbo.cmsBlock", "BlockTypeId", "dbo.cmsBlockType");
			DropTable("dbo.cmsBlock");

			CreateIndex("cmsHtmlContent", "BlockId");
            AddForeignKey("cmsHtmlContent", "BlockId", "cmsBlockInstance", "Id", cascadeDelete: true);

			CreateIndex( "cmsBlockInstance", "ModifiedByPersonId" );
			CreateIndex( "cmsBlockInstance", "CreatedByPersonId" );
			CreateIndex( "cmsBlockInstance", "PageId" );
			CreateIndex( "cmsBlockInstance", "BlockTypeId" );
			AddForeignKey( "cmsBlockInstance", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey("cmsBlockInstance", "CreatedByPersonId", "crmPerson", "Id");
            AddForeignKey("cmsBlockInstance", "PageId", "cmsPage", "Id", cascadeDelete: true);
            AddForeignKey("dbo.cmsBlockInstance", "BlockTypeId", "dbo.cmsBlockType", "Id", cascadeDelete: true);
        }
    }
}
