-- set @definedTypeNameFilter if you need DefinedValue migrations for a specific DefinedType
declare
 @definedTypeNameFilter nvarchar(max) = '%%' --'Transaction Source%'


select 
    CONCAT('RockMigrationHelper.AddDefinedType("', 
    [c].[Name], '","',
    [dt].[Name], '","',
    [dt].[Description], '","',
    [dt].[Guid], '",@"',
    REPLACE(REPLACE([dt].[HelpText], '"', '""'), '''', ''''''), '");'
    ) [Up],
    0 [SortOrder]
FROM 
    [DefinedType] [dt]
	INNER JOIN [Category] [c] ON [c].Id = [dt].CategoryId
where (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
select 
    CONCAT('RockMigrationHelper.AddDefinedTypeAttribute("', 
    [dt].[Guid], '","',
    [ft].[Guid], '","',
    [a].[Name], '","',
    [a].[Key], '","',
    [a].[Description], '",',
    [a].[Order], ',"',
    [a].[DefaultValue], '","',
    [a].[Guid], '");'
    ) [Up],
    1 [SortOrder]
FROM [Attribute] [a]
    left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    join [DefinedType] [dt] on a.EntityTypeQualifierValue = [dt].Id
where 
    e.Name = 'Rock.Model.DefinedValue' 
and 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
select 
    CONCAT('RockMigrationHelper.AddAttributeQualifier("', 
    [a].[Guid], '","',
    [aq].[Key], '","',
    [aq].[Value], '","',
    [aq].[Guid], '");'
    ) [Up],
    2 [SortOrder]
FROM [Attribute] [a]
    left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    join [DefinedType] [dt] on a.EntityTypeQualifierValue = [dt].Id
    join [AttributeQualifier] [aq] on a.Id = aq.AttributeId
where 
    e.Name = 'Rock.Model.DefinedValue' 
and 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
SELECT 
    CONCAT('RockMigrationHelper.AddDefinedValue("', 
    [dt].[Guid], '","',
    [dv].[Value], '","',
    [dv].[Description], '","',
    [dv].[Guid], '",',
    case [dv].[IsSystem] when 0 then 'false' else 'true' end, ');'
    ) [Up],
    3 [SortOrder]
  FROM [DefinedValue] [dv]
    join [DefinedType] [dt] on [dv].[DefinedTypeId] = [dt].[Id]
   where (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
select 
    CONCAT('RockMigrationHelper.AddDefinedValueAttributeValue("', 
    [dv].[Guid], '","',
    [a].[Guid], '",@"',
    REPLACE(REPLACE([av].[Value], '"', '""'), '''', ''''''), '");'
    ) [Up],
    4 [SortOrder]
FROM [AttributeValue] [av]
    join [Attribute] [a] on av.AttributeId = a.Id
    join [DefinedValue] [dv] on av.EntityId = [dv].Id
    join [DefinedType] [dt] on dv.DefinedTypeId = dt.Id
where 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and
    (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
order by [SortOrder]

select 
    CONCAT('RockMigrationHelper.DeleteAttribute("', 
    [a].[Guid], '"); // ',
	a.[Key]
    ) [Down],
    0 [SortOrder]
FROM [Attribute] [a]
    left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    join [DefinedType] [dt] on a.EntityTypeQualifierValue = [dt].Id
where 
    e.Name = 'Rock.Model.DefinedValue' 
and 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
SELECT 
    CONCAT('RockMigrationHelper.DeleteDefinedValue("', 
    [dv].[Guid], '"); // ',
	dv.Value
    ) [Down],
    1 [SortOrder]
  FROM [DefinedValue] [dv]
    join [DefinedType] [dt] on [dv].[DefinedTypeId] = [dt].[Id]
   where (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
union
select 
    CONCAT('RockMigrationHelper.DeleteDefinedType("', 
    [dt].[Guid], '"); // ',
	dt.Name
    ) [Down],
    2 [SortOrder]
FROM 
    [DefinedType] [dt]
where (dt.IsSystem=0 or dt.Name like @definedTypeNameFilter)
order by [SortOrder]