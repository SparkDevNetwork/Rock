
DECLARE @fileId INT
DECLARE crBinaryFile CURSOR FOR
SELECT [Id] FROM [BinaryFile] WHERE [IsTemporary] = 1;

OPEN crBinaryFile
FETCH NEXT FROM crBinaryFile INTO @fileId

WHILE @@FETCH_STATUS = 0
BEGIN
	BEGIN TRY
		DELETE FROM BinaryFile WHERE [Id] = @fileId
	END TRY
	BEGIN CATCH
		PRINT 'Unable to delete ' + ERROR_MESSAGE()
	END CATCH
	FETCH NEXT FROM crBinaryFile INTO @fileId
END

CLOSE crBinaryFile
DEALLOCATE crBinaryFile
