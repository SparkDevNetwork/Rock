namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlockToBlockType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("cmsBlock", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsBlock", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsBlockInstance", "BlockId", "cmsBlock");
            DropIndex("cmsBlock", new[] { "CreatedByPersonId" });
            DropIndex("cmsBlock", new[] { "ModifiedByPersonId" });
            DropIndex("cmsBlockInstance", new[] { "BlockId" });
            CreateTable(
                "dbo.cmsBlockType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Path = c.String(nullable: false, maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);

			Sql( @"
				SET IDENTITY_INSERT cmsBlockType ON
				INSERT INTO [cmsBlockType] (
					 [Id]
					,[IsSystem]
					,[Path]
					,[Name]
					,[Description]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid] )
				SELECT 
 					 [Id]
					,[IsSystem]
					,[Path]
					,[Name]
					,[Description]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
				FROM [cmsBlock]
				SET IDENTITY_INSERT cmsBlockType OFF

				UPDATE [cmsBlockType] SET 
					 [Path] = '~/Blocks/Administration/BlockTypes.ascx'
					,[Name] = 'Block Types'
					,[Description] = 'Block Type Administration'
				WHERE [Path] = '~/Blocks/Administration/Blocks.ascx'

				UPDATE [cmsPage] SET 
					 [Name] = 'Block Types'
					,[Title] = 'Block Types'
					,[Description] = 'Manage Block Types'
				WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D'
" );
			
            AddColumn("dbo.cmsBlockInstance", "BlockTypeId", c => c.Int(nullable: false));
			Sql( @"
				UPDATE [cmsBlockInstance] SET [BlockTypeId] = [BlockId]
				UPDATE [coreAttribute] SET [EntityQualifierColumn] = 'BlockTypeId' WHERE [EntityQualifierColumn] = 'BlockId'
" );
			
			AddForeignKey("dbo.cmsBlockInstance", "BlockTypeId", "dbo.cmsBlockType", "Id", cascadeDelete: true);
            CreateIndex("dbo.cmsBlockInstance", "BlockTypeId");
            DropColumn("dbo.cmsBlockInstance", "BlockId");
            DropTable("dbo.cmsBlock");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.cmsBlock",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Path = c.String(nullable: false, maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

			Sql( @"
				SET IDENTITY_INSERT cmsBlock ON
				INSERT INTO [cmsBlock] (
					 [Id]
					,[IsSystem]
					,[Path]
					,[Name]
					,[Description]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid] )
				SELECT 
 					 [Id]
					,[IsSystem]
					,[Path]
					,[Name]
					,[Description]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
				FROM [cmsBlockType]
				SET IDENTITY_INSERT cmsBlock OFF

				UPDATE [cmsBlock] SET 
					 [Path] = '~/Blocks/Administration/Blocks.ascx'
					,[Name] = 'Blocks'
					,[Description] = 'Block Administration'
				WHERE [Path] = '~/Blocks/Administration/BlockTypes.ascx'

				UPDATE [cmsPage] SET 
					 [Name] = 'Blocks'
					,[Title] = 'Blocks'
					,[Description] = 'Manage Blocks'
				WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D'
" );
			
			AddColumn( "dbo.cmsBlockInstance", "BlockId", c => c.Int( nullable: false ) );
			Sql( @"
				UPDATE [cmsBlockInstance] SET [BlockId] = [BlockTypeId]
				UPDATE [coreAttribute] SET [EntityQualifierColumn] = 'BlockId' WHERE [EntityQualifierColumn] = 'BlockTypeId'
" );

			DropIndex("dbo.cmsBlockInstance", new[] { "BlockTypeId" });
            DropIndex("dbo.cmsBlockType", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.cmsBlockType", new[] { "CreatedByPersonId" });
            DropForeignKey("dbo.cmsBlockInstance", "BlockTypeId", "dbo.cmsBlockType");
            DropForeignKey("dbo.cmsBlockType", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.cmsBlockType", "CreatedByPersonId", "dbo.crmPerson");
            DropColumn("dbo.cmsBlockInstance", "BlockTypeId");
            DropTable("dbo.cmsBlockType");
            CreateIndex("cmsBlockInstance", "BlockId");
            CreateIndex("cmsBlock", "ModifiedByPersonId");
            CreateIndex("cmsBlock", "CreatedByPersonId");
            AddForeignKey("cmsBlockInstance", "BlockId", "cmsBlock", "Id", cascadeDelete: true);
            AddForeignKey("cmsBlock", "ModifiedByPersonId", "crmPerson", "Id");
            AddForeignKey("cmsBlock", "CreatedByPersonId", "crmPerson", "Id");
        }
    }
}
