-- list of recently modified Auth records for Page, Block and EntityType to help write a Migration
DECLARE @EntityTypeIdPage INT = 2

SELECT a.ModifiedDateTime
    ,CONCAT (
        'RockMigrationHelper.AddSecurityAuthForPage( "'
        ,p.[Guid]
        ,'", '
        ,a.[Order]
        ,', "'
        ,a.[Action]
        ,'", '
        ,CASE a.AllowOrDeny
            WHEN 'A'
                THEN 'true'
            ELSE 'false'
            END
        ,', "'
        ,g.[Guid]
        ,'", '
        ,a.SpecialRole
        ,', "'
        ,a.[Guid]
        ,'" );'
        ,' // Page:'
        ,p.InternalName
        ) [Up]
    ,CONCAT (
        'RockMigrationHelper.DeleteSecurityAuth( "'
        ,a.[Guid]
        ,'");'
        ,' // Page:'
        ,p.InternalName
        ,' Group: '
        ,CASE a.SpecialRole
            WHEN 0
                THEN CONCAT (
                        g.[Guid]
                        ,' ( '
                        ,g.NAME
                        ,' ), '
                        )
            WHEN 1
                THEN '<all users>'
            WHEN 2
                THEN '<all authenticated users>'
            WHEN 3
                THEN '<all un-authenticated users>'
            END
        ) [Down]
    ,p.[InternalName] [Page]
    ,p.[Guid] [Page.Guid]
    ,a.[Order]
    ,a.[Action]
    ,a.AllowOrDeny
    ,a.SpecialRole
    ,CASE a.SpecialRole
        WHEN 0
            THEN CONCAT (
                    g.[Guid]
                    ,' ( '
                    ,g.NAME
                    ,' ), '
                    )
        WHEN 1
            THEN '<all users>'
        WHEN 2
            THEN '<all authenticated users>'
        WHEN 3
            THEN '<all un-authenticated users>'
        END [Group]
    ,a.[Guid] [Auth.Guid]
FROM Auth a
LEFT JOIN [Group] g ON a.GroupId = g.Id
JOIN [Page] p ON a.EntityId = p.Id
WHERE EntityTypeId = @EntityTypeIdPage
ORDER BY a.ModifiedDateTime DESC

DECLARE @EntityTypeIdBlock INT = 9

SELECT b.[Name] + CHAR(9) [Block]
    ,a.[Order]
    ,a.[Action]
    ,a.AllowOrDeny
    ,a.SpecialRole
    ,CASE a.SpecialRole
        WHEN 0
            THEN CONCAT (
                    g.[Guid]
                    ,' ( '
                    ,g.NAME
                    ,' ), '
                    )
        WHEN 1
            THEN '<all users>'
        WHEN 2
            THEN '<all authenticated users>'
        WHEN 3
            THEN '<all un-authenticated users>'
        END [Group]
    ,a.[Guid] [Auth.Guid]
FROM Auth a
LEFT JOIN [Group] g ON a.GroupId = g.Id
JOIN [Block] b ON a.EntityId = b.Id
WHERE EntityTypeId = @EntityTypeIdBlock
ORDER BY a.ModifiedDateTime DESC

-- Entity Security
SELECT b.[Name] + CHAR(9) [EntityType]
    ,a.[Order]
    ,a.[Action]
    ,a.AllowOrDeny
    ,a.SpecialRole
    ,CASE a.SpecialRole
        WHEN 0
            THEN CONCAT (
                    g.[Guid]
                    ,' ( '
                    ,g.NAME
                    ,' ), '
                    )
        WHEN 1
            THEN '<all users>'
        WHEN 2
            THEN '<all authenticated users>'
        WHEN 3
            THEN '<all un-authenticated users>'
        END [Group]
    ,a.[Guid] [Auth.Guid]
FROM Auth a
LEFT JOIN [Group] g ON a.GroupId = g.Id
JOIN [EntityType] b ON a.EntityTypeId = b.Id
WHERE a.EntityId = 0
ORDER BY isnull(a.ModifiedDateTime, '1900-01-01') DESC
