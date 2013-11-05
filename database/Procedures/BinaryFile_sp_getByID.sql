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
END