-- Set the entity type and id of the authorization you would like to reset. 
-- NOTE: THIS CHANGE WILL NOT TAKE AFFECT UNTIL CLEARING CACHE IN ROCK
DECLARE @EntityType varchar(100) = 'Rock.Model.Page'
DECLARE @EntityId int = ??

BEGIN TRAN

DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = @EntityType )

DELETE [Auth] 
WHERE [EntityTypeId] = @EntityTypeId
AND [EntityId] = @EntityId
AND [SpecialRole] = 1

INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
VALUES
	( @EntityTypeId, @EntityId, 0, 'View', 'A', 1, NEWID() ),
	( @EntityTypeId, @EntityId, 0, 'Edit', 'A', 1, NEWID() ),
	( @EntityTypeId, @EntityId, 0, 'Administrate', 'A', 1, NEWID() )

;WITH CTE AS (
	SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Order] ASC ) AS [RowNum] 	FROM [Auth]
	WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @EntityId AND [SpecialRole] <> 1 AND [Action] = 'View'
	UNION
	SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Order] ASC ) AS [RowNum] 	FROM [Auth]
	WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @EntityId AND [SpecialRole] <> 1 AND [Action] = 'Edit'
	UNION
	SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Order] ASC ) AS [RowNum] 	FROM [Auth]
	WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @EntityId AND [SpecialRole] <> 1 AND [Action] = 'Administrate'
)

UPDATE A SET [Order] = CTE.[RowNum]
FROM CTE
INNER JOIN [Auth] A ON A.[Id] = CTE.[Id]

COMMIT TRAN

SELECT 
	A.[Action], 
	A.[Order],
	CASE A.[SpecialRole]
		WHEN 0 THEN COALESCE( G.[Name], P.[NickName] + ' ' + P.[LastName] )
		WHEN 1 THEN 'All Users'
		WHEN 2 THEN 'All Authenticated Users'
		WHEN 3 THEN 'All Un-Authenticated Users'
	END AS [Role/User],
	CASE WHEN A.[AllowOrDeny] = 'A' THEN 'Allow' ELSE 'Deny' END AS [AllowOrDeny]
FROM [Auth] A
LEFT OUTER JOIN [Group] G ON G.[Id] = A.[GroupId]
LEFT OUTER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
LEFT OUTER JOIN [Person] P ON P.[Id] = PA.[PersonId]
WHERE A.[EntityTypeId] = @EntityTypeId
AND A.[EntityId] = @EntityId
ORDER BY A.[Action] DESC, A.[Order]
