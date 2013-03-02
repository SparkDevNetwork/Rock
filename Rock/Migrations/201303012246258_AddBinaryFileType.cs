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
    public partial class AddBinaryFileType : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BinaryFileType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IconSmallFileId = c.Int(),
                        IconLargeFileId = c.Int(),
                        IconCssClass = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.IconSmallFileId)
                .ForeignKey("dbo.BinaryFile", t => t.IconLargeFileId)
                .Index(t => t.IconSmallFileId)
                .Index(t => t.IconLargeFileId);
            
            CreateIndex( "dbo.BinaryFileType", "Name", true );
            CreateIndex( "dbo.BinaryFileType", "Guid", true );
            AddColumn("dbo.BinaryFile", "BinaryFileTypeId", c => c.Int());
            AddForeignKey("dbo.BinaryFile", "BinaryFileTypeId", "dbo.BinaryFileType", "Id");
            CreateIndex("dbo.BinaryFile", "BinaryFileTypeId");

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "File Types", "", "Default", "66031C31-B397-4F78-8AB2-389B7D8731AA" );
            AddPage( "66031C31-B397-4F78-8AB2-389B7D8731AA", "File Type", "", "Default", "19CAC4D5-FE82-4AE0-BFD3-3C12E3024574" );
            AddBlockType( "Administration - Binary File Type Detail", "", "~/Blocks/Administration/BinaryFileTypeDetail.ascx", "02D0A037-446B-403B-9719-5EF7D98239EF" );
            AddBlockType( "Administration - Binary File Type List", "", "~/Blocks/Administration/BinaryFileTypeList.ascx", "0926B82C-CBA2-4943-962E-F788C8A80037" );
            AddBlock( "66031C31-B397-4F78-8AB2-389B7D8731AA", "0926B82C-CBA2-4943-962E-F788C8A80037", "File Type List", "", "Content", 0, "3C3FF5B1-342A-4240-8264-D815FFF7BD9C" );
            AddBlock( "19CAC4D5-FE82-4AE0-BFD3-3C12E3024574", "02D0A037-446B-403B-9719-5EF7D98239EF", "File Type", "", "Content", 0, "E7073D89-23B7-44A5-BD74-90FD80930889" );
            AddBlockTypeAttribute( "0926B82C-CBA2-4943-962E-F788C8A80037", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "7019CC73-589F-43CC-8A8C-97A5E06AC0BA" );
            
            /*
            // Attrib Value for l:Active
            AddBlockAttributeValue( "A5E0DD78-BB67-41E5-BDDA-73E2277482DA", "16C3A9B7-F2ED-4440-AA3F-DC3BB011FA0A", "False" );
            // Attrib Value for l:Order
            AddBlockAttributeValue( "A5E0DD78-BB67-41E5-BDDA-73E2277482DA", "AB53E541-C2AC-4530-A73C-ACE8055E1D93", "" );
            // Attrib Value for Menu:Active
            AddBlockAttributeValue( "CC8F4186-870D-4CF3-8226-D49F1A0D0DDF", "78885845-A802-4E96-B642-A40648E954B3", "False" );
            // Attrib Value for Menu:Order
            AddBlockAttributeValue( "CC8F4186-870D-4CF3-8226-D49F1A0D0DDF", "B49E3022-EA33-48C3-82DA-4F1FF79AB54E", "" );
            // Attrib Value for Person Edit:Active
            AddBlockAttributeValue( "6E189D68-C4EC-443F-B409-1EEC0F12D427", "F5026F18-4279-4AC2-8F6F-B2BA35E9AD7A", "False" );
            // Attrib Value for Person Edit:Order
            AddBlockAttributeValue( "6E189D68-C4EC-443F-B409-1EEC0F12D427", "4C4C9EDC-8414-4B0D-BC68-901CD8835788", "" );
            */

            // Attrib Value for File Type List:Detail Page Guid
            AddBlockAttributeValue( "3C3FF5B1-342A-4240-8264-D815FFF7BD9C", "7019CC73-589F-43CC-8A8C-97A5E06AC0BA", "19cac4d5-fe82-4ae0-bfd3-3c12e3024574" );

            Sql( @"
UPDATE [dbo].[Page] SET
    MenuDisplayDescription = 0,
    IconCssClass = 'icon-file-alt'
WHERE [Guid] = '66031C31-B397-4F78-8AB2-389B7D8731AA'

DECLARE @BinaryFileTypeId int
INSERT INTO [BinaryFileType] (IsSystem, Name, Description, IconCssClass, Guid)
    VALUES (1, 'Check-In Label', 'Label used for checkin', 'icon-print', 'DE0E5C50-234B-474C-940C-C571F385E65F')
SET @BinaryFileTypeId = SCOPE_IDENTITY()

-- Get the entitytype for binary file
DECLARE @BinaryFileEntityTypeId int
SELECT @BinaryFileEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.BinaryFile'
IF @BinaryFileEntityTypeId IS NULL
BEGIN
    INSERT INTO [EntityType] ([Name], [Guid])
    VALUES ('Rock.Model.BinaryFile', NEWID())
    SET @BinaryFileEntityTypeId = SCOPE_IDENTITY()
END

DECLARE @TextFieldTypeId int
SELECT @TextFieldTypeId = [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'
DECLARE @AttributeId int
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
	VALUES (1, @TextFieldTypeId, @BinaryFileEntityTypeId, 'BinaryFileTypeId', CAST(@BinaryFileTypeId AS varchar), 'MergeCodes', 'Merge Codes', '', 'The merge codes used for the label', 0, 0, '', 0, 0, 'CE57450F-634A-420A-BF5A-B43E9B20ABF2')
SET @AttributeId = SCOPE_IDENTITY()
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "CE57450F-634A-420A-BF5A-B43E9B20ABF2" ); // Merge Code Attribute
            Sql( @"
DELETE [BinaryFileType] WHERE [Guid] = 'DE0E5C50-234B-474C-940C-C571F385E65F'
" );
            DeleteAttribute( "7019CC73-589F-43CC-8A8C-97A5E06AC0BA" ); // Detail Page Guid
            DeleteBlock( "3C3FF5B1-342A-4240-8264-D815FFF7BD9C" ); // File Type List
            DeleteBlock( "E7073D89-23B7-44A5-BD74-90FD80930889" ); // File Type
            DeleteBlockType( "02D0A037-446B-403B-9719-5EF7D98239EF" ); // Administration - Binary File Type Detail
            DeleteBlockType( "0926B82C-CBA2-4943-962E-F788C8A80037" ); // Administration - Binary File Type List
            DeletePage( "19CAC4D5-FE82-4AE0-BFD3-3C12E3024574" ); // File Type
            DeletePage( "66031C31-B397-4F78-8AB2-389B7D8731AA" ); // File Types

            DropIndex( "dbo.BinaryFileType", new[] { "IconLargeFileId" } );
            DropIndex("dbo.BinaryFileType", new[] { "IconSmallFileId" });
            DropIndex("dbo.BinaryFile", new[] { "BinaryFileTypeId" });
            DropForeignKey("dbo.BinaryFileType", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.BinaryFileType", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.BinaryFile", "BinaryFileTypeId", "dbo.BinaryFileType");
            DropColumn("dbo.BinaryFile", "BinaryFileTypeId");
            DropTable("dbo.BinaryFileType");
        }
    }
}
