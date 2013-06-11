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
    public partial class BinaryFileData : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BinaryFileData",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Content = c.Binary(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);

            Sql( @"
    INSERT INTO BinaryFileData (Id, Content, Guid)
    SELECT Id, Data, NewID() FROM BinaryFile
" );

            CreateIndex( "dbo.BinaryFileData", "Guid", true );
            DropColumn("dbo.BinaryFile", "Data");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.BinaryFile", "Data", c => c.Binary());

            Sql( @"
    UPDATE F SET Data = D.Content
    FROM BinaryFile F 
    INNER JOIN BinaryFileData D ON D.Id = F.Id
" );

            DropIndex( "dbo.BinaryFileData", new[] { "Id" } );
            DropForeignKey("dbo.BinaryFileData", "Id", "dbo.BinaryFile");
            DropTable("dbo.BinaryFileData");
        }
    }
}
