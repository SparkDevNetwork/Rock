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
    public partial class AddLayout : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Layout",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsSystem = c.Boolean( nullable: false ),
                    SiteId = c.Int( nullable: false ),
                    FileName = c.String( nullable: false, maxLength: 260 ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    Guid = c.Guid( nullable: false ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Site", t => t.SiteId, cascadeDelete: true )
                .Index( t => t.SiteId );

            AddColumn( "dbo.Block", "LayoutId", c => c.Int() );
            AddColumn( "dbo.Page", "LayoutId", c => c.Int() );

            Sql( @"
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 1,'Default','Default','','2BA19878-F9B8-4ABF-91E1-75A7CF92BD8B')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 1,'Dialog','Dialog','','7CFA101B-2D20-4523-9EC5-3F30502797A5')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 1,'PersonDetail','PersonDetail','','F66758C6-3E3D-4598-AF4C-B317047B5987')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 1,'Splash','Splash','','DEC70939-E041-4C9E-A4AA-5A15C0C8391F')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 1,'TwoColumnLeft','TwoColumnLeft','','D4287374-B898-4458-BFCB-E8F494BB626D')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 2,'Checkin','Checkin','','66FA0143-F04C-4447-A67A-2A10A6BB1A2B')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 3,'FullWidth','FullWidth','','5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 3,'Homepage','Homepage','','093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
     VALUES (1, 3,'LeftSidebar','LeftSidebar','','325B7BFD-8B80-44FD-A951-4E4763DA6C0D')
    INSERT INTO [Layout] ( IsSystem, SiteId, FileName, Name, Description, Guid)
        VALUES (1, 4,'Checkin','Checkin','','3BD6CFC1-0BF2-43C8-AD38-44E711D6ACE0')

    UPDATE P SET
	    LayoutId = L.[Id]
    FROM [Page] P
    INNER JOIN [Layout] L
	    ON L.[SiteId] = P.[SiteId]
	    AND L.[FileName] = P.[Layout]

    UPDATE B SET
	    LayoutId = L.[Id]
    FROM [Block] B
    INNER JOIN [Layout] L
	    ON L.[SiteId] = B.[SiteId]
	    AND L.[FileName] = B.[Layout]
    WHERE B.[SiteId] IS NOT NULL
" );
            AlterColumn( "dbo.Page", "LayoutId", c => c.Int( nullable: false ) );

            DropForeignKey( "dbo.Page", "SiteId", "dbo.Site" );
            DropForeignKey("dbo.Block", "SiteId", "dbo.Site");
            DropIndex("dbo.Page", new[] { "SiteId" });
            DropIndex("dbo.Block", new[] { "SiteId" });
            
            CreateIndex("dbo.Page", "LayoutId");
            CreateIndex("dbo.Block", "LayoutId");
            AddForeignKey("dbo.Page", "LayoutId", "dbo.Layout", "Id");
            AddForeignKey("dbo.Block", "LayoutId", "dbo.Layout", "Id", cascadeDelete: true);
            DropColumn("dbo.Block", "SiteId");
            DropColumn("dbo.Block", "Layout");
            DropColumn("dbo.Page", "SiteId");
            DropColumn("dbo.Page", "Layout");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Page", "Layout", c => c.String(maxLength: 100));
            AddColumn("dbo.Page", "SiteId", c => c.Int());
            AddColumn("dbo.Block", "Layout", c => c.String(maxLength: 100));
            AddColumn("dbo.Block", "SiteId", c => c.Int());

            Sql( @"
    UPDATE P SET
	    SiteID = L.SiteId,
        Layout = L.FileName
    FROM [Page] P
    INNER JOIN [Layout] L
	    ON L.[Id] = P.[LayoutId]

    UPDATE B SET
	    SiteID = L.SiteId,
        Layout = L.FileName
    FROM [Block] B
    INNER JOIN [Layout] L
	    ON L.[Id] = B.[LayoutId]
    WHERE B.[LayoutId] IS NOT NULL
" );
            DropForeignKey("dbo.Block", "LayoutId", "dbo.Layout");
            DropForeignKey("dbo.Page", "LayoutId", "dbo.Layout");
            DropForeignKey("dbo.Layout", "SiteId", "dbo.Site");
            DropIndex("dbo.Block", new[] { "LayoutId" });
            DropIndex("dbo.Page", new[] { "LayoutId" });
            DropIndex("dbo.Layout", new[] { "SiteId" });
            DropColumn("dbo.Page", "LayoutId");
            DropColumn("dbo.Block", "LayoutId");
            DropTable("dbo.Layout");
            CreateIndex("dbo.Block", "SiteId");
            CreateIndex("dbo.Page", "SiteId");
            AddForeignKey("dbo.Block", "SiteId", "dbo.Site", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Page", "SiteId", "dbo.Site", "Id");
        }
    }
}
