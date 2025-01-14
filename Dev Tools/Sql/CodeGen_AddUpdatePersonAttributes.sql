-- This will create a migration for Attribute and AttributeQualifier for every Person attribute where the ModifiedDate is greater than 1 day ago 

DECLARE @crlf varchar(2) = char(13) + char(10)

select @crlf + '// Person Attribute "' +  a.[Name] + '"' + @crlf
	+ 'RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( '
	+ '@"' + CONVERT(nvarchar(50),f.[Guid]) + '", '
	+ 'new List<string> {' + STUFF((SELECT '", "' + CONVERT(nvarchar(50),c.[Guid])
         FROM AttributeCategory ac
JOIN Category c ON c.id = ac.CategoryId 
         WHERE a.id = ac.AttributeId
         FOR XML PATH(''), TYPE)
        .value('.','NVARCHAR(MAX)'),1,2,' ') + '" }, '
	+ '@"' + a.[Name] + '", '
	+ '@"' + COALESCE(a.[AbbreviatedName], '') + '", '
	+ '@"' + a.[Key] + '", '
	+ '@"' + COALESCE(a.[IconCssClass], '') + '", '
	+ '@"' + a.[Description] + '", '
	+ CONVERT(nvarchar(50), a.[Order]) + ', '
	+ ( CASE WHEN [a].[DefaultValue] IS NULL THEN 'null'
			ELSE + '"' + A.[DefaultValue] + '"' END) + ', '
	+ '@"' + CONVERT(nvarchar(50),a.[Guid])
	+ '" );'
FROM Attribute a
JOIN FieldType f ON f.id = a.FieldTypeId
JOIN EntityType et ON et.id = a.EntityTypeId
WHERE  a.[ModifiedDateTime] > GETDATE() - 1
	AND et.[Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'
union
select 
    CASE WHEN aq.[Id] IS NOT NULL THEN @crlf 
			+ 'RockMigrationHelper.AddAttributeQualifier( '
			+ '@"' + CONVERT(nvarchar(50),a.[Guid]) + '", '
			+ '@"' + aq.[Key] + '", '
			+ '@"' + REPLACE(aq.[Value], '"', '""') + '", '
			+ '@"' + CONVERT(NVARCHAR(50),aq.[Guid]) 
			+ '" );'
		ELSE + '' END
FROM Attribute a
JOIN EntityType et ON et.id = a.EntityTypeId
LEFT JOIN AttributeQualifier aq ON a.id = aq.AttributeId
WHERE  a.[ModifiedDateTime] > GETDATE() - 1
	AND et.[Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'


