DECLARE @NoteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Note' )

;WITH CTE
AS
( 
                SELECT [EntityId], COUNT(*) AS [AuthCount]
                FROM [Auth]
                WHERE [EntityTypeId] = @NoteEntityTypeId
                AND [Action] = 'View'
                GROUP BY [EntityId]
)

UPDATE [Note]
SET [IsPrivateNote] = 1
WHERE [Id] IN (
                SELECT A1.[EntityId]
                FROM [Auth] A1
                INNER JOIN [Auth] A2
                                ON A2.[EntityId] = A1.[EntityId]
                                AND A2.[EntityTypeId] = A1.[EntityTypeId]
                                AND A2.[Action] = 'View'
                                AND A2.[AllowOrDeny] = 'D'
                                AND A2.[SpecialRole] =  1
                                AND A2.[PersonAliasId] IS NULL
                                AND A2.[Order] > A1.[Order]
                WHERE A1.[EntityTypeId] = @NoteEntityTypeId
                AND A1.[Action] = 'View'
                AND A1.[AllowOrDeny] = 'A'
                AND A1.[GroupId] IS NULL
                AND A1.[SpecialRole] = 0
                AND A1.[PersonAliasId] IS NOT NULL
                AND A1.[Order] = 0
                AND A1.[EntityId] IN ( SELECT [EntityId] FROM CTE WHERE [AuthCount] = 2 )
)

DELETE [Auth]
WHERE [EntityTypeId] = @NoteEntityTypeId
AND [Action] = 'View'
AND [EntityId] IN ( SELECT [Id] FROM [Note] WHERE [IsPrivateNote] = 1 )
