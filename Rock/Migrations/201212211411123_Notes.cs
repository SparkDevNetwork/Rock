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
    public partial class Notes : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Note",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        NoteTypeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        SourceTypeValueId = c.Int(),
                        Caption = c.String(maxLength: 200),
                        Date = c.DateTime(nullable: false),
                        IsAlert = c.Boolean(),
                        Text = c.String(),
                        Guid = c.Guid(nullable: false),
                        NoteType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NoteType", t => t.NoteType_Id)
                .ForeignKey("dbo.NoteType", t => t.NoteTypeId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.SourceTypeValueId)
                .Index(t => t.NoteType_Id)
                .Index(t => t.NoteTypeId)
                .Index(t => t.SourceTypeValueId);
            
            CreateIndex( "dbo.Note", "Guid", true );
            CreateTable(
                "dbo.NoteType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        SourcesTypeId = c.Int(),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedType", t => t.SourcesTypeId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.SourcesTypeId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.NoteType", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.NoteType", new[] { "EntityTypeId" });
            DropIndex("dbo.NoteType", new[] { "SourcesTypeId" });
            DropIndex("dbo.Note", new[] { "SourceTypeValueId" });
            DropIndex("dbo.Note", new[] { "NoteTypeId" });
            DropIndex("dbo.Note", new[] { "NoteType_Id" });
            DropForeignKey("dbo.NoteType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.NoteType", "SourcesTypeId", "dbo.DefinedType");
            DropForeignKey("dbo.Note", "SourceTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Note", "NoteTypeId", "dbo.NoteType");
            DropForeignKey("dbo.Note", "NoteType_Id", "dbo.NoteType");
            DropTable("dbo.NoteType");
            DropTable("dbo.Note");
        }
    }
}
