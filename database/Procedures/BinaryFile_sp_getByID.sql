ALTER PROCEDURE [BinaryFile_sp_getByID]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing GetFile.ashx.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.Id,
        bf.[FileName], 
        bf.MimeType,
        bf.Url,
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        COALESCE (bfse.Name,bftse.Name ) as StorageTypeName,
        d.Content
    FROM BinaryFile bf 
    LEFT JOIN BinaryFileData bfd
        ON bf.Id = bfd.Id
    LEFT JOIN EntityType bfse
        ON bf.StorageEntityTypeId = bfse.Id
    LEFT JOIN BinaryFileType bft
        on bf.BinaryFileTypeId = bft.Id
    LEFT JOIN EntityType bftse
        ON bft.StorageEntityTypeId = bftse.Id
    WHERE 
        (@Id > 0 and bf.Id = @Id)
        or
        (bf.[Guid] = @Guid)
END