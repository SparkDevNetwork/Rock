declare
-- set to 1 to include any block attribute values updated in the last 60 minutes, even if their BlockType Attribute is IsSystem=1
@forceIncludeRecentlyUpdatedBlockAttributeValues bit = 0

set nocount on
declare
@crlf varchar(2) = char(13) + char(10)

begin

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

IF OBJECT_ID('tempdb..#blocksTemp') IS NOT NULL
    DROP TABLE #blocksTemp

IF OBJECT_ID('tempdb..#knownGuidsToAdd2') IS NOT NULL
    DROP TABLE #knownGuidsToAdd2

create table #knownGuidsToAdd2(
    [Guid] UniqueIdentifier, 
    CONSTRAINT [pk_knownGuidsToAdd2] PRIMARY KEY CLUSTERED  ( [Guid]) 
);

-- External site blocks
insert into #knownGuidsToAdd2 values 
('F4424C43-93DB-43AA-AC99-D4EF0C74A056'), 
('F6FB15E7-C003-41F3-BA4A-FA07CB29C3B7'),
('AEDD95E4-F3DB-48FF-B9B5-1F0D70EF99FD'),
('89DDEEA0-A115-4558-9F46-C1C3F1718C71'), 
('3204D62F-FCC8-4005-9954-9309D6183979'),
('BDB89AD6-4EF6-4B28-9963-C5167BDC8680'),
('D168A032-F5F3-49B9-A45B-76213C66D6D1'),
('3626EEF2-B5E6-4089-A6CD-0768BF0DF53C'),
('DC9FA67A-B67D-4BA8-B810-790BBDB62C6A')

create table #codeTable (
    Id int identity(1,1) not null,
    CodeText nvarchar(max),
    CONSTRAINT [pk_codeTable] PRIMARY KEY CLUSTERED  ( [Id]) );

    -- block types
    insert into #codeTable
    SELECT '            RockMigrationHelper.UpdateBlockType("'+
        [Name]+ '","'+  
        ISNULL([Description],'')+ '","'+  
        [Path]+ '","'+  
        [Category]+ '","'+  
        CONVERT( nvarchar(50),[Guid])+ '");'
    from [BlockType]
    where [IsSystem] = 0
    and [Guid] in (select [Guid] from #knownGuidsToAdd2) -- shouldn't happen
    order by [Id]

    insert into #codeTable
    SELECT @crlf

    -- attributes
    if object_id('tempdb..#attributeIds') is not null
    begin
      drop table #attributeIds
    end

    select * 
	into #attributeIds 
	from (
		select A.[Id] 
		from [dbo].[Attribute] A
		inner join [EntityType] E 
			ON E.[Id] = A.[EntityTypeId]
left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)

        Where [bt].[Guid] in (select [Guid] from #knownGuidsToAdd2)
		and E.[Name] = 'Rock.Model.Block'
		and A.EntityTypeQualifierColumn = 'BlockTypeId'
	) [newattribs]
    
    insert into #codeTable
    SELECT @crlf

    insert into #codeTable
    SELECT 
        '            // Attrib for BlockType: ' + bt.Name + ':'+ a.Name+
        @crlf +
        '            RockMigrationHelper.AddBlockTypeAttribute("'+ 
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
    and [a].[id] in (select [Id] from #attributeIds)

    insert into #codeTable
    SELECT @crlf

	    select CodeText [MigrationUp] from #codeTable 
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id

	delete from #codeTable
    -- generate MigrationDown

    insert into #codeTable SELECT         
        '            // Attrib for BlockType: ' + bt.Name + ':'+ a.Name+
        @crlf +
        '            RockMigrationHelper.DeleteAttribute("'+ 
		CONVERT(nvarchar(50), [A].[Guid]) + '");' 
		from [Attribute] [A]
		inner join [EntityType] E 
			ON E.[Id] = A.[EntityTypeId]
		left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)
		where A.[IsSystem] = 0
        and [A].[Guid] in (select [Guid] from #knownGuidsToAdd2) 
		and E.[Name] = 'Rock.Model.Block'
		and A.EntityTypeQualifierColumn = 'BlockTypeId'
       
		order by [A].[Id] desc   

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeleteBlockType("'+ CONVERT(nvarchar(50), [Guid])+ '"); // '+ 
		[Name] 
	from [BlockType] 
	where [IsSystem] = 0
    and [Guid] in (select [Guid] from #knownGuidsToAdd2) 
	order by [Id] desc

    insert into #codeTable
    SELECT @crlf

    select CodeText [MigrationDown] from #codeTable
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

IF OBJECT_ID('tempdb..#knownGuidsToAdd2') IS NOT NULL
    DROP TABLE #knownGuidsToAdd2

end