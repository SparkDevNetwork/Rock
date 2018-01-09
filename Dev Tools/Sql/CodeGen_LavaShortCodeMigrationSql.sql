
DECLARE @LavaTag VARCHAR(50) = 'parallax'



DECLARE @Name VARCHAR(50) 
DECLARE @Description VARCHAR(max)  
DECLARE @Documentation VARCHAR(max) 
DECLARE @IsSystem bit 
DECLARE @IsActive bit
DECLARE @TagName varchar(50)
DECLARE @Markup varchar(max)
DECLARE @TagType varchar(50)
DECLARE @EnabledLavaCommands varchar(500)
DECLARE @Parameters varchar(max)
DECLARE @Guid as varchar(100)


DECLARE db_cursor CURSOR FOR  
SELECT [Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid]
FROM  [LavaShortCode]
WHERE [TagName] = @LavaTag


OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @Name, @Description, @Documentation, @IsSystem, @IsActive, @TagName, @Markup, @TagType, @EnabledLavaCommands, @Parameters, @Guid

WHILE @@FETCH_STATUS = 0   
BEGIN   
       
	   PRINT 'INSERT INTO [LavaShortCode]
	([Name], [Description], [IsSystem], [IsActive], [TagName], [TagType], [EnabledLavaCommands], [Parameters], [Guid], [Documentation], [Markup])
	VALUES
	(''' + REPLACE(@Name,'''','''''') + ''',''' + REPLACE(@Description,'''','''''') + ''',1,1,''' + @TagName + ''',' + CONVERT(VARCHAR(4), @TagType) + ',''' + @EnabledLavaCommands + ''',''' + REPLACE(@Parameters,'''','''''') + ''',''' + CONVERT(nvarchar(50), NEWID()) + ''',''' + REPLACE(@Documentation,'''','''''') + ''',''' + REPLACE(@Markup,'''','''''') + ''')'
	   

       FETCH NEXT FROM db_cursor INTO @Name, @Description, @Documentation, @IsSystem, @IsActive, @TagName, @Markup, @TagType, @EnabledLavaCommands, @Parameters, @Guid  
END   

CLOSE db_cursor   
DEALLOCATE db_cursor