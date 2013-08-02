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
    public partial class UpdateBinaryFileStoredProc : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop and re-create BinaryFile_sp_getByID to work with updated schema
            // Adjusted to include BinaryFile `Url` and the Name associated `StorageEntityTypeId`
            Sql( @"
DROP PROCEDURE [BinaryFile_sp_getByID]
CREATE PROCEDURE [BinaryFile_sp_getByID]
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
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Rather than restore to previous non-working state, restore to a working version.
            Sql( @"
DROP PROCEDURE [BinaryFile_sp_getByID]
CREATE PROCEDURE [BinaryFile_sp_getByID]
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
			f.MimeType
		FROM BinaryFile f 
		LEFT JOIN BinaryFileData d
			ON f.Id = d.Id
		WHERE f.[Id] = @Id
    END
    ELSE
    BEGIN
        SELECT f.Id,
			d.Content, 
			f.[FileName], 
			f.MimeType
		FROM BinaryFile f 
		LEFT JOIN BinaryFileData d
			ON f.Id = d.Id
		WHERE f.[Guid] = @Guid
    END
END" );
        }
    }
}
