-- This will create a migration for Attribute and AttributeQualifier for every attribute where the ModifiedDate is greater than 1 day ago 

DECLARE @crlf varchar(2) = char(13) + char(10)

select @crlf + '// Person Attribute "' +  a.[Name] + '"' + @crlf
	+ 'RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( '
	+ '@"' + CONVERT(nvarchar(50),f.[Guid]) + '", '
	+ '@"' + CONVERT(nvarchar(50),c.[Guid]) + '", '
	+ '@"' + a.[Name] + '", '
	+ '@"' + a.[AbbreviatedName] + '", '
	+ '@"' + a.[Key] + '", '
	+ '@"' + a.[IconCssClass] + '", '
	+ '@"' + a.[Description] + '", '
	+ CONVERT(nvarchar(50), a.[Order]) + ', '
	+ '@"' + a.[DefaultValue] + '", '
	+ '@"' + CONVERT(nvarchar(50),a.[Guid])
	+ '" );'
	+ CASE
		WHEN aq.[Id] IS NOT NULL THEN @crlf 
			+ 'RockMigrationHelper.AddAttributeQualifier( '
			+ '@"' + CONVERT(nvarchar(50),a.[Guid]) + '", '
			+ '@"' + aq.[Key] + '", '
			+ '@"' + REPLACE(aq.[Value], '"', '""') + '", '
			+ '@"' + CONVERT(NVARCHAR(50),aq.[Guid]) 
			+ '" );'
	END
FROM Attribute a
JOIN AttributeCategory ac ON a.id = ac.AttributeId
JOIN Category c ON c.id = ac.CategoryId
JOIN FieldType f ON f.id = a.FieldTypeId
JOIN EntityType et ON et.id = a.EntityTypeId
LEFT JOIN AttributeQualifier aq ON a.id = aq.AttributeId
WHERE a.[ModifiedDateTime] > GETDATE() - 1
	AND et.[Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'



