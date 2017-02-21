set nocount on
declare
@crlf varchar(2) = char(13) + char(10)

begin

DECLARE @PageId int = 226

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable


create table #codeTable (
    Id int identity(1,1) not null,
    CodeText nvarchar(max),
    CONSTRAINT [pk_codeTable] PRIMARY KEY CLUSTERED  ( [Id]) );
    
	-- pages
    insert into #codeTable
    SELECT 
		'            // ' + ISNULL('Page: ' + p.InternalName,'') +
        @crlf + 
		'            RockMigrationHelper.AddPage("' +
        ISNULL(CONVERT( nvarchar(50), [pp].[Guid]),'') + '","'+ 
        CONVERT( nvarchar(50), [l].[Guid]) + '","'+ 
        [p].[InternalName]+  '","'+  
        ISNULL([p].[Description],'')+  '","'+
        CONVERT( nvarchar(50), [p].[Guid])+  '","'+  
        ISNULL([p].[IconCssClass],'')+ '");' + ' // Site:' + s.Name 
    FROM [Page] [p]
    left outer join [Page] [pp] on [pp].[Id] = [p].[ParentPageId]
	join [Layout] [l] on [l].[Id] = [p].[layoutId]
	join [site] [s] on [s].[Id] = [l].[siteId]
    where [p].[Id] = @PageId

    insert into #codeTable
    SELECT @crlf

	-- pages
    insert into #codeTable
    SELECT 
		'            RockMigrationHelper.AddPageRoute("' +
        ISNULL(CONVERT( nvarchar(50), [p].[Guid]),'') + '","'+ 
		[r].[Route] + '");' 
    FROM [PageRoute] [r]
    inner join [Page] [p] on [p].[Id] = [r].[PageId]
    where [p].[Id] = @PageId

    insert into #codeTable
    SELECT @crlf

    -- block types
    insert into #codeTable
    SELECT '            RockMigrationHelper.UpdateBlockType("'+
        [Name]+ '","'+  
        ISNULL([Description],'')+ '","'+  
        [Path]+ '","'+  
        [Category]+ '","'+  
        CONVERT( nvarchar(50),[Guid])+ '");'
    from [BlockType]
    where [Id] in (
		SELECT bt.[Id]
		FROM [block] b
		INNER JOIN [blocktype] bt on bt.[Id] = b.[BlockTypeId]
		WHERE b.[PageId] = @PageId
	)

    insert into #codeTable
    SELECT @crlf

    -- blocks
    insert into #codeTable
    SELECT 
		'            RockMigrationHelper.AddBlock("'+
        ISNULL(CONVERT(nvarchar(50), [p].[Guid]),'') + '","'+ 
        ISNULL(CONVERT(nvarchar(50), [l].[Guid]),'') + '","'+
        CONVERT(nvarchar(50), [bt].[Guid])+ '","'+
        [b].[Name]+ '","'+
        [b].[Zone]+ '","'+
		ISNULL([b].[PreHtml], '')+ '","'+
		ISNULL([b].[PostHtml], '')+ '",'+
        CONVERT(varchar, [b].[Order])+ ',"'+
        CONVERT(nvarchar(50), [b].[Guid])+ '"); '+
        @crlf
    from [Block] [b]
    join [BlockType] [bt] on [bt].[Id] = [b].[BlockTypeId]
    left outer join [Page] [p] on [p].[Id] = [b].[PageId]
	left outer join [Layout] [l] on [l].[Id] = [b].[LayoutId]
	left outer join [Layout] [pl] on [pl].[Id] = [p].[LayoutId]
	join [site] [s] on [s].[Id] = [l].[siteId] or [s].[Id] = [pl].[siteId]
    where b.[Id] in (
		SELECT b.[Id]
		FROM [block] b
		WHERE b.[PageId] = @PageId
	)

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable
    SELECT 
        '            RockMigrationHelper.UpdateBlockTypeAttribute("'+ 
        CONVERT(nvarchar(50), bt.Guid)+ '","'+   
        CONVERT(nvarchar(50), ft.Guid)+ '","'+     
        a.Name+ '","'+  
        a.[Key]+ '","'+ 
        ''+ '","'+ 
        --ISNULL(a.Category,'')+ '","'+ 
        ISNULL(a.Description,'')+ '",'+ 
        CONVERT(varchar, a.[Order])+ ',@"'+ 
        ISNULL(a.DefaultValue,'')+ '","'+
        CONVERT(nvarchar(50), a.Guid)+ '");' +
        @crlf
    from [Attribute] [a]
    left outer join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)
    where EntityTypeQualifierColumn = 'BlockTypeId'
    and bt.[Id] in (
		SELECT bt.[Id]
		FROM [block] b
		INNER JOIN [blocktype] bt on bt.[Id] = b.[BlockTypeId]
		WHERE b.[PageId] = @PageId
	)
	order by a.[Order]

    insert into #codeTable
    SELECT @crlf

    -- attributes values (just Block Attributes)    
    insert into #codeTable
    SELECT 
        '            RockMigrationHelper.AddBlockAttributeValue("'+     
        CONVERT(nvarchar(50), b.Guid)+ '","'+ 
        CONVERT(nvarchar(50), a.Guid)+ '",@"'+ 
        ISNULL(av.Value,'')+ '"); // '+ a.[Name] + 
        @crlf
    from [AttributeValue] [av]
    join Block b on b.Id = av.EntityId
    join Attribute a on a.id = av.AttributeId
    left outer join [Page] [p] on [p].[Id] = [b].[PageId]
	left outer join [Layout] [l] on [l].[Id] = [b].[LayoutId]
	left outer join [Layout] [pl] on [pl].[Id] = [p].[LayoutId]
	join [site] [s] on [s].[Id] = [l].[siteId] or [s].[Id] = [pl].[siteId]
    where b.[Id] in (
		SELECT b.[Id]
		FROM [block] b
		WHERE b.[PageId] = @PageId
	)
    order by b.Id

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable
    SELECT @crlf
    
    -- Page Contexts
    insert into #codeTable
      SELECT '            // Add/Update PageContext for Page:' + p.InternalName + ', Entity: ' + pc.Entity + ', Parameter: ' + pc.IdParameter  
      + @crlf +
      + '            RockMigrationHelper.UpdatePageContext( "' + convert(nvarchar(max), p.Guid) + '", "' + pc.Entity +  '", "' + pc.IdParameter +  '", "' + convert(nvarchar(max), pc.Guid) + '");'
      + @crlf
    FROM [dbo].[PageContext] [pc]
    join [Page] [p]
    on [p].[Id] = [pc].[PageId]
    where [p].[Id] = @PageId

    select CodeText [MigrationUp] from #codeTable 
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id

    delete from #codeTable

    -- generate MigrationDown

    insert into #codeTable SELECT         
        '            RockMigrationHelper.DeleteAttribute("'+ 
		CONVERT(nvarchar(50), [A].[Guid]) + '");' 
		from [Attribute] [A]
		inner join [EntityType] E 
			ON E.[Id] = A.[EntityTypeId]
		left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)
		where E.[Name] = 'Rock.Model.Block'
		and A.EntityTypeQualifierColumn = 'BlockTypeId'
		and bt.[Id] in (
			SELECT bt.[Id]
			FROM [block] b
			INNER JOIN [blocktype] bt on bt.[Id] = b.[BlockTypeId]
			WHERE b.[PageId] = @PageId
		)
       
		order by [A].[Id] desc   

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeleteBlock("'+ CONVERT(nvarchar(50), [b].[Guid])+ '");'
        from [Block] [b]
		left outer join [Page] [p] on [p].[Id] = [b].[PageId]
		left outer join [Layout] [l] on [l].[Id] = [b].[LayoutId]
		left outer join [Layout] [pl] on [pl].[Id] = [p].[LayoutId]
		join [site] [s] on [s].[Id] = [l].[siteId] or [s].[Id] = [pl].[siteId]
		where b.[Id] in (
			SELECT b.[Id]
			FROM [block] b
			WHERE b.[PageId] = @PageId
		)
		order by [b].[Id] desc

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeleteBlockType("'+ CONVERT(nvarchar(50), [Guid])+ '");'
	from [BlockType] 
	where [Id] in (
		SELECT bt.[Id]
		FROM [block] b
		INNER JOIN [blocktype] bt on bt.[Id] = b.[BlockTypeId]
		WHERE b.[PageId] = @PageId
	)
	order by [Id] desc

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeletePage("'+ CONVERT(nvarchar(50), [p].[Guid])+ '"); // ' + ISNULL(' Page: ' + p.InternalName,'') 
	from [Page] [p]
	join [Layout] [l] on [l].[Id] = [p].[layoutId]
	join [site] [s] on [s].[Id] = [l].[siteId]
    where [p].[Id] = @PageId

    insert into #codeTable
    SELECT '            // Delete PageContext for Page:' + p.InternalName + ', Entity: ' + pc.Entity + ', Parameter: ' + pc.IdParameter  + @crlf +
    + '            DeletePageContext( "' + convert(nvarchar(max), pc.Guid) + '");'
    + @crlf  
    FROM [dbo].[PageContext] [pc]
    join [Page] [p]
    on [p].[Id] = [pc].[PageId]
    where [p].[Id] = @PageId

    select CodeText [MigrationDown] from #codeTable
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

end