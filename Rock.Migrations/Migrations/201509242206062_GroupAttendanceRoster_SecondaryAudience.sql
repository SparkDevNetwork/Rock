DECLARE @Id int
DECLARE @ParentId int
DECLARE @Selection nvarchar(max)
DECLARE @FilterEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Reporting.DataFilter.PropertyFilter' )
DECLARE @AndFilterId int

DECLARE DataViewCursor INSENSITIVE CURSOR FOR
SELECT 
	F.[Id],
	F.[ParentId],
	'[' + char(13) + char(10) + 
	char(9) + '"SecondaryAudiences",' + char(13) + char(10) + 
	char(9) + '"8",' + char(13) + char(10) + 
	char(9) + '"' + SUBSTRING( F.[Selection], 38, 36 ) + '",' + char(13) + char(10) + 
	']'
FROM [DataViewFilter] F
INNER JOIN [DataViewFilter] G ON G.[Id] = F.[ParentId]
WHERE F.[Selection] LIKE '%"PrimaryAudience"%'
AND G.[ExpressionType] = 1

OPEN DataViewCursor
FETCH NEXT FROM DataViewCursor
INTO @Id, @ParentId, @Selection

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [Guid] )
		VALUES ( 2, @ParentId, newid() )

		SET @AndFilterId = SCOPE_IDENTITY()

		UPDATE [DataViewFilter] SET [ParentId] = @AndFilterId
		WHERE [Id] = @Id

		INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] )
		VALUES ( 0, @AndFilterId, @FilterEntityTypeId, @Selection, newid() )
		
		FETCH NEXT FROM DataViewCursor
		INTO  @Id, @ParentId, @Selection

	END
	
END

CLOSE DataViewCursor
DEALLOCATE DataViewCursor