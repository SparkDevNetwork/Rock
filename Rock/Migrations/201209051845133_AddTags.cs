namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTags : RockMigration_0
    {
        public override void Up()
        {
            CreateTable(
                "dbo.coreTag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Entity = c.String(maxLength: 50),
                        EntityQualifierColumn = c.String(maxLength: 50),
                        EntityQualifierValue = c.String(maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        OwnerId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.crmPerson", t => t.OwnerId)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.OwnerId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreTaggedItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        TagId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.coreTag", t => t.TagId, cascadeDelete: true)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.TagId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);

			// Update General Settings page to only show one level of child pages
			Sql( @"
	UPDATE [coreAttributeValue] SET [Value] = '1' WHERE [Guid] = 'A7F79FFF-DFAB-447F-81D6-07D1BE667B1B'
");

			// Add parent page for administering all types of tags
			AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Tags", "Administer Tags", "F111791B-6A58-4388-8533-00E913F48F41" );
			AddBlockInstance( "F111791B-6A58-4388-8533-00E913F48F41", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Person Tags", "Content", "B551E9D1-304C-4E13-8CC5-318899FF2741", 0 );
			AddBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "" );
			AddBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Assets/XSLT/AdminPageList.xslt" );
			AddBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );
			
			// Tag Administration Block
			AddBlock( "Tag Administration", "Administer tags for specific entity and owner (or system)", "~/Blocks/Administration/Tags.ascx", "9BC9B09F-EDDB-40F8-9939-FF881A7874DB" );
			AddBlockAttribute( "9BC9B09F-EDDB-40F8-9939-FF881A7874DB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity", "Entity", "Entity Name", 0, "", "94D56BCC-8E62-495B-B722-6CE0B5EEDEF4" );
			AddBlockAttribute( "9BC9B09F-EDDB-40F8-9939-FF881A7874DB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "Entity", "The entity column to evaluate when determining if this attribute applies to the entity", 1, "", "B4D5A749-20D3-4A55-987B-147CF4EE2B3F" );
			AddBlockAttribute( "9BC9B09F-EDDB-40F8-9939-FF881A7874DB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "Entity", "The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, "", "B4ECFF00-2843-4ADE-94CF-443FB95C4EBA" );
			AddBlockAttribute( "9BC9B09F-EDDB-40F8-9939-FF881A7874DB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Global Tags", "Entity", "Edit global tags (vs. personal tags)?", 3, "false", "6DF40DD2-AC15-47D6-86A0-F23C333AA47C" );
	
			// Tag Administration Page
			AddPage( "F111791B-6A58-4388-8533-00E913F48F41", "Person Tags", "Tags related to a person", "9EC914AF-D726-4715-934D-49D9F41BF039" );
			AddBlockInstance( "9EC914AF-D726-4715-934D-49D9F41BF039", "9BC9B09F-EDDB-40F8-9939-FF881A7874DB", "Person Tags", "Content", "D464B931-7783-4912-98DB-E895643044B0", 0 );
			AddBlockAttributeValue( "D464B931-7783-4912-98DB-E895643044B0", "94D56BCC-8E62-495B-B722-6CE0B5EEDEF4", "Rock.Crm.Person" );
			AddBlockAttributeValue( "D464B931-7783-4912-98DB-E895643044B0", "B4D5A749-20D3-4A55-987B-147CF4EE2B3F", "" );
			AddBlockAttributeValue( "D464B931-7783-4912-98DB-E895643044B0", "B4ECFF00-2843-4ADE-94CF-443FB95C4EBA", "" );
			AddBlockAttributeValue( "D464B931-7783-4912-98DB-E895643044B0", "6DF40DD2-AC15-47D6-86A0-F23C333AA47C", "True" );
        }
        
        public override void Down()
        {
			// Tag Administration Page
			DeletePage( "9EC914AF-D726-4715-934D-49D9F41BF039" );

			// Tag Administration Block
			DeleteBlockAttribute( "6DF40DD2-AC15-47D6-86A0-F23C333AA47C" );
			DeleteBlockAttribute( "B4ECFF00-2843-4ADE-94CF-443FB95C4EBA" );
			DeleteBlockAttribute( "B4D5A749-20D3-4A55-987B-147CF4EE2B3F" );
			DeleteBlockAttribute( "94D56BCC-8E62-495B-B722-6CE0B5EEDEF4" );
			DeleteBlock( "9BC9B09F-EDDB-40F8-9939-FF881A7874DB" );

			// Parent Tag page
			DeletePage( "F111791B-6A58-4388-8533-00E913F48F41" );
			DeleteBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA");
			DeleteBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "D8A029F8-83BE-454A-99D3-94D879EBF87C");
			DeleteBlockAttributeValue( "B551E9D1-304C-4E13-8CC5-318899FF2741", "9909E07F-0E68-43B8-A151-24D03C795093");

            DropIndex("dbo.coreTaggedItem", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.coreTaggedItem", new[] { "CreatedByPersonId" });
            DropIndex("dbo.coreTaggedItem", new[] { "TagId" });
            DropIndex("dbo.coreTag", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.coreTag", new[] { "CreatedByPersonId" });
            DropIndex("dbo.coreTag", new[] { "OwnerId" });
            DropForeignKey("dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTaggedItem", "TagId", "dbo.coreTag");
            DropForeignKey("dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.coreTag", "OwnerId", "dbo.crmPerson");
            DropTable("dbo.coreTaggedItem");
            DropTable("dbo.coreTag");
        }
    }
}
