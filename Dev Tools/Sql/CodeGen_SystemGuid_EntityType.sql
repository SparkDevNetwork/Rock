/* Helps code generate Rock\SystemGuid\EntityType.cs */
SELECT CONCAT (
        '
/// <summary>
/// '
        ,[Name]
        ,' field type
/// </summary>
public const string '
        ,REPLACE(REPLACE(REPLACE(UPPER([Name]), ' ', '_'), '-', '_'), '.', '_')
        ,' = "'
        ,[Guid]
        ,'";'
        )
FROM [EntityType]
ORDER BY NAME