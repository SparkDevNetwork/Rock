set nocount on
DECLARE @crlf varchar(2) = char(13) + char(10)

BEGIN

DECLARE @JobId int = 32

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

	-- RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendCreditCardExpirationNotices", "Expiring Credit Card Email", "The system email template to use for the credit card expiration notice. The attributes 'Person', 'Card' (the last four digits of the credit card), and 'Expiring' (the MM/YYYY of expiration) will be passed to the email.", 0, "C07ACD2E-7B9D-400A-810F-BC0EBB9A60DD", "074E32E2-99E3-4962-80C3-4025CC934AB1", "ExpiringCreditCardEmail" );

    -- Add the service job attributes
    INSERT INTO #codeTable
    SELECT 
        '            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "'+ 
        CONVERT(nvarchar(50), ft.[Guid])+ '", "'+
		'Class", '+
		'"'+ a.[EntityTypeQualifierValue] + '", "' +
		a.[Name] + '", "'+  
        ISNULL(a.[Description],'')+ '", '+ 
        CONVERT(varchar, a.[Order])+ ', @"'+ 
		ISNULL(a.[DefaultValue],'')+ '", "'+
        CONVERT(nvarchar(50), a.[Guid])+ '", "' +
		a.[Key]+ '" );' +
        @crlf
    FROM [Attribute] a
	JOIN [EntityType] e ON e.[Id] = a.[EntityTypeId]
    JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
    WHERE a.[EntityTypeQualifierColumn] = 'Class'
	AND a.[EntityTypeQualifierValue] in
	(
		SELECT [Class] FROM [ServiceJob] [p] WHERE [p].[Id] = @JobId
	)
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
    DROP TABLE #codeTable

END