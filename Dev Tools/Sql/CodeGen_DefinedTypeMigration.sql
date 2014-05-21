select 
    CONCAT('RockMigrationHelper.AddDefinedType("', 
    [dt].[Category], '","',
    [dt].[Name], '","',
    [dt].[Description], '","',
    [dt].[Guid], '",@"',
    REPLACE([dt].[HelpText], '"', '""'), '");'
    ) [Up]
FROM 
    [DefinedType] [dt]
where dt.IsSystem=0
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
    ) [Up]
FROM [Attribute] [a]
    left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    join [DefinedType] [dt] on a.EntityTypeQualifierValue = [dt].Id
where 
    e.Name = 'Rock.Model.DefinedValue' 
and 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and dt.IsSystem=0
union
SELECT 
    CONCAT('RockMigrationHelper.AddDefinedValue("', 
    [dt].[Guid], '","',
    [dv].[Name], '","',
    [dv].[Description], '","',
    [dv].[Guid], '",',
    case [dv].[IsSystem] when 0 then 'false' else 'true' end, ');'
    ) [Up]
  FROM [DefinedValue] [dv]
    join [DefinedType] [dt] on [dv].[DefinedTypeId] = [dt].[Id]
   where dt.IsSystem = 0
union
select 
    CONCAT('RockMigrationHelper.AddDefinedValueAttributeValue("', 
    [dv].[Guid], '","',
    [a].[Guid], '",@"',
    REPLACE([av].[Value], '"', '""'), '");'
    ) [Up]
FROM [AttributeValue] [av]
    join [Attribute] [a] on av.AttributeId = a.Id
    join [DefinedValue] [dv] on av.EntityId = [dv].Id
    join [DefinedType] [dt] on dv.DefinedTypeId = dt.Id
where 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and
    dt.IsSystem=0
order by [Up]

select 
    CONCAT('RockMigrationHelper.DeleteAttribute("', 
    [a].[Guid], '");'
    ) [Down]
FROM [Attribute] [a]
    left join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    join [DefinedType] [dt] on a.EntityTypeQualifierValue = [dt].Id
where 
    e.Name = 'Rock.Model.DefinedValue' 
and 
    a.EntityTypeQualifierColumn = 'DefinedTypeId'
and dt.IsSystem=0
union
SELECT 
    CONCAT('RockMigrationHelper.DeleteDefinedValue("', 
    [dv].[Guid], '");'
    ) [Down]
  FROM [DefinedValue] [dv]
    join [DefinedType] [dt] on [dv].[DefinedTypeId] = [dt].[Id]
   where dt.IsSystem = 0
union
select 
    CONCAT('RockMigrationHelper.DeleteDefinedType("', 
    [dt].[Guid], '");'
    ) [Down]
FROM 
    [DefinedType] [dt]
where dt.IsSystem=0
order by [Down]