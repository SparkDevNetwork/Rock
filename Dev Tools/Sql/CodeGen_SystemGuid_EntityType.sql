/*
CREATE FUNCTION dbo.SplitCamelCase(@X VARCHAR(8000))
RETURNS VARCHAR(8000) AS
BEGIN
 WHILE PATINDEX('%[^ ][ABCDEFGHIJKLMNOPQRSTUVWXYZ]%'
        COLLATE SQL_Latin1_General_CP1_CS_AS,
        @X COLLATE SQL_Latin1_General_CP1_CS_AS) > 0
    SET @X = STUFF(@X,
        PATINDEX('%[^ ][ABCDEFGHIJKLMNOPQRSTUVWXYZ]%'
            COLLATE SQL_Latin1_General_CP1_CS_AS,
            @X COLLATE SQL_Latin1_General_CP1_CS_AS) + 1, 0, ' ')
 RETURN @X
END
GO
*/

/* Helps code generate Rock\SystemGuid\EntityType.cs */
SELECT CONCAT (
        '
/// <summary>
/// The guid for the ' 
        ,[Name]
        ,' entity
/// </summary>
public const string '
        ,upper(REPLACE(REPLACE(REPLACE(dbo.SplitCamelCase(Replace([Name], 'Rock.Model.', '')), ' ', '_'), '-', '_'), '.', '_'))
        ,' = "'
        ,[Guid]
        ,'";'
        )
FROM [EntityType]
WHERE IsEntity = 1
and Name like 'Rock.Model.%'
ORDER BY NAME

