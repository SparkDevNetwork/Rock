DECLARE @crlf varchar(2) = char(13) + char(10)

select
    cast( concat(@crlf, '// ', et.Name, ' - ', a.[Key], @crlf, 'RockMigrationHelper.AddOrUpdateAchievementTypeAttribute("', et.[Guid], '", ', '"', ft.Guid, '", ', '"', a.[Name], '", ', '"', a.[Key], '", ', '@"', a.[Description], '", ', '', a.[Order], ', ', '@"', a.[DefaultValue], '", "', a.[Guid], '");'  ) as nvarchar(255) )[CodeGen]
from Attribute a
    join EntityType et on et.Id = a.EntityTypeQualifierValue
    join FieldType ft on a.FieldTypeId = ft.Id
where a.EntityTypeQualifierColumn = 'ComponentEntityTypeId'
order by et.Name, a.[Order], a.[Key]


