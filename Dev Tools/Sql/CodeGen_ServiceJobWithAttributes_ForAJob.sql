set nocount on
DECLARE @crlf varchar(2) = char(13) + char(10)

-- specify the Id of the ServiceJob to create a migration for
DECLARE @JobId int = 7

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable


create table #codeTable (
    Id INT IDENTITY(1,1) not null,
    CodeText nvarchar(max),
    CONSTRAINT [pk_codeTable] PRIMARY KEY CLUSTERED  ( [Id]) );
    
	-- servicejob
    INSERT INTO #codeTable
    SELECT 
		'            // add ' + ISNULL('ServiceJob: ' + p.Name,'') +
        @crlf + 
		'            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql' +
		@crlf + 
		'            Sql(@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = ''' + p.[Class] + ''' AND [Guid] = ''' + ISNULL(CONVERT( nvarchar(50), [p].[Guid]),'') + ''' )' +
		@crlf + 
		'            BEGIN' + @crlf + 
		'               INSERT INTO [ServiceJob] (' + @crlf + 
		'                  [IsSystem]' + @crlf + 
		'                  ,[IsActive]' + @crlf + 
		'                  ,[Name]' + @crlf + 
		'                  ,[Description]' + @crlf + 
        '                  ,[Class]' + @crlf + 
        '                  ,[CronExpression]' + @crlf + 
        '                  ,[NotificationStatus]' + @crlf + 
        '                  ,[Guid] )' + @crlf + 
		'               VALUES ( ' + @crlf + 
		'                  ' + CONVERT(varchar, [p].[IsSystem]) + @crlf + 
		'                  ,'+ CONVERT(varchar, [p].[IsActive]) + @crlf + 
		'                  ,'''+ [p].[Name] + '''' + @crlf + 
		'                  ,'''+ ISNULL([p].[Description],'') + '''' + @crlf + 
		'                  ,'''+ [p].[Class] + '''' + @crlf + 
		'                  ,'''+ ISNULL([p].[CronExpression],'') + '''' + @crlf + 
		'                  ,'  + CONVERT(varchar, [p].[NotificationStatus]) + @crlf + 
		'                  ,'''+ ISNULL(CONVERT( nvarchar(50), [p].[Guid]),'') + '''' + @crlf + 
		'                  );' + @crlf + 
		'            END" );' + @crlf
    FROM [ServiceJob] [p]
    WHERE [p].[Id] = @JobId


	INSERT INTO #codeTable
    SELECT @crlf

    -- Add the service job attributes
	--AddOrUpdateEntityAttribute( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn
	--, string entityTypeQualifierValue, string name, string abbreviatedName, string description, int order, string defaultValue, string guid, string key )
    INSERT INTO #codeTable
    SELECT
		  '            // Attribute: ' + [a].[EntityTypeQualifierValue] + ': ' + [a].[Name] + @crlf
		+ '            RockMigrationHelper.AddOrUpdateEntityAttribute( '
		+ '"Rock.Model.ServiceJob", '										-- EntityTypeName
		+ '"' + CONVERT(NVARCHAR(50), [ft].[Guid]) + '", '					-- FieldType.Guid
		+ '"Class", '														-- Attribute.EntityTypeQualifierColumn
		+ '"' + [a].[EntityTypeQualifierValue] + '", '						-- Attribute.EntityTypeQualifierValue
		+ '"' + [a].[Name] + '", '						  					-- Attribute.Name
		+ '"' + ISNULL([a].[AbbreviatedName], '') + '", '					-- Attribute.AbbreviatedName
		+ '@"'+ ISNULL(REPLACE([a].[Description], '"', '""'),'') + '", '	-- Attribute.Description
		+ CONVERT(VARCHAR, [a].[Order])+ ', '								-- Attribute.Order
		+ '@"'+ ISNULL(REPLACE([a].[DefaultValue], '"', '""'),'') + '", '	-- Attribute.DefaultValue
		+ '"' + CONVERT(NVARCHAR(50), [a].[Guid])+ '", '					-- Attribute.Guid
		+ '"' + [a].[Key] + '" );'											-- Attribute.Key
		+ @crlf
	FROM [Attribute] a
	JOIN [EntityType] e ON e.[Id] = a.[EntityTypeId]
	JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
	WHERE a.[EntityTypeQualifierColumn] = 'Class'
		AND a.[EntityTypeQualifierValue] in (SELECT [Class] FROM [ServiceJob] [p] WHERE [p].[Id] = @JobId)
	ORDER BY a.[Order]

	-- Add the job attribute values
    INSERT INTO #codeTable
    SELECT 
        '            RockMigrationHelper.AddAttributeValue( "'+     
        CONVERT(nvarchar(50), a.Guid)+ '", ' + 
		CONVERT(varchar, av.[EntityId]) + ', @"' +
        ISNULL(av.[Value],'')+ '", "' + 
        CONVERT(nvarchar(50), a.Guid) + '" ); // ' + j.[Name] + ': ' + a.[Name] + 
        @crlf
    FROM [AttributeValue] [av]
    JOIN [ServiceJob] j ON j.Id = av.EntityId
    JOIN Attribute a ON a.id = av.AttributeId
    WHERE av.[EntityId] = @JobId
	AND a.[EntityTypeQualifierColumn] = 'Class'
    ORDER BY a.[Order]


    SELECT CodeText [MigrationUp] FROM #codeTable 
    WHERE REPLACE(CodeText, @crlf, '') != ''
    ORDER BY [Id]

    DELETE FROM #codeTable


    -- generate MigrationDown

	INSERT INTO #codeTable select '            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql' + @crlf

	-- delete attributes
    INSERT INTO #codeTable SELECT         
        '            RockMigrationHelper.DeleteAttribute("'+ 
		CONVERT(nvarchar(50), a.[Guid]) + '"); // ' + a.[EntityTypeQualifierValue] + ': ' + a.[Name] 
	FROM [Attribute] a
	JOIN [EntityType] e on e.[Id] = a.[EntityTypeId]
    JOIN [FieldType] [ft] on ft.[Id] = a.[FieldTypeId]
    WHERE a.[EntityTypeQualifierColumn] = 'Class'
	AND a.[EntityTypeQualifierValue] IN
	(
		SELECT [Class] FROM [ServiceJob] [j] WHERE [j].[Id] = @JobId
	)
	ORDER BY a.[Order]

    INSERT INTO #codeTable
    SELECT @crlf

    INSERT INTO #codeTable
    SELECT @crlf

	-- delete servicejob
    INSERT INTO #codeTable
    SELECT
		@crlf +  
		'            // remove ' + ISNULL('ServiceJob: ' + p.Name,'') +
        @crlf + 
		'            Sql(@"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = ''' + p.[Class] + ''' AND [Guid] = ''' + ISNULL(CONVERT( nvarchar(50), [p].[Guid]),'') + ''' )' +
		@crlf + 
		'            BEGIN' + @crlf + 
		'               DELETE [ServiceJob]  WHERE [Guid] = ''' + ISNULL(CONVERT( nvarchar(50), [p].[Guid]),'') + ''';' + @crlf +
		'            END" );' + @crlf
    FROM [ServiceJob] [p]
    WHERE [p].[Id] = @JobId


    SELECT CodeText [MigrationDown] FROM #codeTable
    WHERE REPLACE(CodeText, @crlf, '') != ''
    ORDER BY [Id]

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    BEGIN DROP TABLE #codeTable END
