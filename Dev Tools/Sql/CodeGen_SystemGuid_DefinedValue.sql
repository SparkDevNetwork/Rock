/* Filter by defined type name*/
DECLARE
  @definedTypeName nvarchar(100) = '%button%';

SELECT CONCAT (
        '
/// <summary>
/// ' 
        , dt.Name
        , ' - '
        , dv.[Value]
        ,' 
/// </summary>
public const string '
        ,upper(REPLACE(REPLACE(REPLACE(dt.[Name]+ ' ' + dv.[Value], ' ', '_'), '-', '_'), '.', '_'))
        ,' = "'
        ,dv.[Guid]
        ,'";'
        )
FROM [DefinedValue] dv
join [DefinedType] dt
on dv.DefinedTypeId = dt.Id
where dt.Name like @definedTypeName