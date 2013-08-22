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
    public partial class BinaryFileCachingRenameAndProcUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.BinaryFile", "AllowCaching", c => c.Boolean( nullable: false ) );

            // Update stored proc `BinaryFile_sp_getByID` to reflect new schema changes
            Sql( @"
ALTER PROCEDURE [BinaryFile_sp_getByID]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id > 0
    BEGIN
        SELECT f.Id,
            d.Content, 
            f.[FileName], 
            f.MimeType,
            f.Url,
            e.Name as StorageTypeName
        FROM BinaryFile f 
        LEFT JOIN BinaryFileData d
            ON f.Id = d.Id
        INNER JOIN EntityType e
            ON f.StorageEntityTypeId = e.Id
        WHERE f.[Id] = @Id
    END
    ELSE
    BEGIN
        SELECT f.Id,
            d.Content, 
            f.[FileName], 
            f.MimeType,
            f.Url,
            e.Name as StorageTypeName
        FROM BinaryFile f 
        LEFT JOIN BinaryFileData d
            ON f.Id = d.Id
        INNER JOIN EntityType e
            ON f.StorageEntityTypeId = e.Id
        WHERE f.[Guid] = @Guid
    END
END" );

            // Rename `Rock.BinaryFile` namespace to `Rock.Storage`
            Sql( @"
DELETE [EntityType] WHERE [AssemblyName] LIKE 'Rock.Storage.Provider.%'
UPDATE [EntityType] SET
    [Name] = REPLACE([Name], 'Rock.BinaryFile.Storage.', 'Rock.Storage.Provider.'),
    [AssemblyName] = REPLACE([AssemblyName], 'Rock.BinaryFile.Storage.', 'Rock.Storage.Provider.')
WHERE [Name] LIKE 'Rock.BinaryFile.Storage.%'" );

            // Update General Settings > Storage Providers > Components Block setting to use the newly renamed container class
            AddBlockAttributeValue( "8966CAFE-D8FC-4703-8960-17CB5807A3B8", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Storage.ProviderContainer, Rock" );

            // Add Default BinaryFileType record to database
            Sql( @"
DECLARE @EntityTypeId int

SET @EntityTypeId = (SELECT Id
FROM EntityType
WHERE [Guid] = '0AA42802-04FD-4AEC-B011-FEB127FC85CD')

INSERT INTO BinaryFileType ( IsSystem, Name, [Description], IconSmallFileId, IconLargeFileId, IconCssClass, [Guid], StorageEntityTypeId )
VALUES ( 1, 'Default', 'The default file type', null, null, null, 'C1142570-8CD6-4A20-83B1-ACB47C1CD377', @EntityTypeId )" );

            // Set existing BinaryFile records with a `null` BinaryFileTypeId FK value to that of the new Default BinaryFileType record
            Sql( @"
DECLARE @BinaryFileTypeId int

SET @BinaryFileTypeId = ( SELECT Id
FROM BinaryFileType
WHERE [Guid] = 'C1142570-8CD6-4A20-83B1-ACB47C1CD377' )

UPDATE BinaryFile
SET BinaryFileTypeId = @BinaryFileTypeId
WHERE BinaryFileTypeId IS NULL" );

            // Remove index on BinaryFile.BinaryFileId, change it to not nullable, re-add index
            DropIndex( "dbo.BinaryFile", new[] { "BinaryFileTypeId" } );
            AlterColumn( "dbo.BinaryFile", "BinaryFileTypeId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.BinaryFile", "BinaryFileTypeId", false );

            // Move default save location to App_Data so files are not publicly addressable
            Sql( @"
UPDATE AttributeValue
SET [Value] = '~/App_Data/Uploads'
WHERE [Guid] = '04C9E6AF-F109-41DA-8353-C4E01FAF6963'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.BinaryFile", new[] { "BinaryFileTypeId" } );
            AlterColumn( "dbo.BinaryFile", "BinaryFileTypeId", c => c.Int() );
            CreateIndex( "dbo.BinaryFile", "BinaryFileTypeId" );

            DropColumn( "dbo.BinaryFile", "AllowCaching" );

            // Original SQL statement in `BinaryFile_sp_getByID` hasn't functioned properly for some time.
            // Rather than restoring it, put it into a working state.
            Sql( @"
ALTER PROCEDURE [BinaryFile_sp_getByID]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id > 0
    BEGIN
        SELECT d.Content, 
            f.[FileName], 
            f.MimeType
        FROM BinaryFile f 
        LEFT JOIN BinaryFileData d
            ON f.Id = d.Id
        WHERE f.[Id] = @Id
    END
    ELSE
    BEGIN
        SELECT d.Content, 
            f.[FileName], 
            f.MimeType
        FROM BinaryFile f 
        LEFT JOIN BinaryFileData d
            ON f.Id = d.Id
        WHERE f.[Guid] = @Guid
    END
END" );

            Sql( @"
UPDATE [EntityType] SET
    [Name] = REPLACE([Name], 'Rock.Storage.Provider.', 'Rock.BinaryFile.Storage.'),
    [AssemblyName] = REPLACE([AssemblyName], 'Rock.Storage.Provider.', 'Rock.BinaryFile.Storage.')
WHERE [Name] LIKE 'Rock.Storage.Provider.%'" );

            AddBlockAttributeValue( "8966CAFE-D8FC-4703-8960-17CB5807A3B8", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.BinaryFile.StorageContainer, Rock" );
            Sql( @"DELETE BinaryFileType WHERE [Guid] = 'C1142570-8CD6-4A20-83B1-ACB47C1CD377'" );

            Sql( @"
UPDATE AttributeValue
SET [Value] = '~/Assets/Uploads'
WHERE [Guid] = '04C9E6AF-F109-41DA-8353-C4E01FAF6963'" );
        }
    }
}
