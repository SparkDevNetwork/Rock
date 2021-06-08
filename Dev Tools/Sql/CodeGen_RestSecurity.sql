-- Display the Auth rules associated with REST Controllers
SELECT 
	A.Id,
	RC.[Name] AS [Controller],
	A.[Action] AS [Action],
	CASE A.[AllowOrDeny] 
		WHEN 'A' THEN 'Allow '
		WHEN 'D' THEN 'Deny ' END AS [Allow or Deny],
	CASE A.[SpecialRole]
		WHEN 0 THEN G.[Name]
		WHEN 1 THEN 'All Users'
		WHEN 2 THEN 'All Authenticated Users'
		WHEN 3 THEN 'All Unauthenticated Users' END AS [Who]
FROM [Auth] A
	INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Name] = 'Rock.Model.RestController'
	LEFT OUTER JOIN [RestController] RC ON RC.[Id] = A.[EntityId]
	LEFT OUTER JOIN [Group] G ON G.[Id] = A.[GroupId]
ORDER BY RC.[Name], A.[Action], A.[Order]

-- Display the Auth rules associated with REST Actions
SELECT 
	RC.[Name] AS [Controller],
	RA.[Method] AS [Method],
	RA.[Path] AS [Path],
	A.[Action] AS [Action],
	CASE A.[AllowOrDeny] 
		WHEN 'A' THEN 'Allow '
		WHEN 'D' THEN 'Deny ' END AS [Allow or Deny],
	CASE A.[SpecialRole]
		WHEN 0 THEN G.[Name]
		WHEN 1 THEN 'All Users'
		WHEN 2 THEN 'All Authenticated Users'
		WHEN 3 THEN 'All Unauthenticated Users' END AS [Who]
FROM [Auth] A
	INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Name] = 'Rock.Model.RestAction'
	LEFT OUTER JOIN [RestAction] RA ON RA.[Id] = A.[EntityId]
	LEFT OUTER JOIN [RestController] RC ON RC.[Id] = RA.[ControllerId]
	LEFT OUTER JOIN [Group] G ON G.[Id] = A.[GroupId]
ORDER BY RC.[Name], A.[Action], A.[Order]


-- SQL to create any controllers that are secured
-- The output of this goes into the migration.  Pre-Beta: Can go to earlier REST migrations and empty them out 
SELECT DISTINCT
	'IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = ''' + [ClassName] + ''') ' + CHAR(13) + CHAR(9) +
	'INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )' + CHAR(13) + CHAR(9) + CHAR(9) +
	'VALUES ( ''' + [Name] + ''', ''' + [ClassName] + ''', NEWID() )' + CHAR(13) [Add RestController Script], rcc.Id 
FROM [RestController] rcc
WHERE [Id] IN (
	SELECT DISTINCT RC.[Id]
	FROM [Auth] A
	INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'
	INNER JOIN [RestAction] RA ON RA.[Id] = A.[EntityId]
	INNER JOIN [RestController] RC ON RC.[Id] = RA.[ControllerId]
	UNION 
	SELECT DISTINCT RC.[Id]
	FROM [Auth] A
	INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'
	INNER JOIN [RestController] RC ON RC.[Id] = A.[EntityId]
)
order by rcc.Id desc


-- SQL to create any actions that are secured
-- The output of this goes into the migration.  Pre-Beta: Can go to earlier REST migrations and empty them out 
SELECT 
	'IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = ''' + RA.[ApiId] + ''') ' + CHAR(13) + CHAR(9) +
	'INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )' + CHAR(13) + CHAR(9) + CHAR(9) +
	'SELECT [Id], ''' + RA.[Method] + ''', ''' + RA.[ApiId] + ''', ''' + RA.[Path] + ''', NEWID()' + CHAR(13) + CHAR(9) + CHAR(9) +
	'FROM [RestController] WHERE [ClassName] = ''' + RC.[ClassName] + '''' + CHAR(13) [Add Rest Action Script], RC.Id 
FROM [Auth] A
INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'
INNER JOIN [RestAction] RA ON RA.[Id] = A.[EntityId]
INNER JOIN [RestController] RC ON RC.[Id] = RA.[ControllerId]
ORDER BY RC.Id desc


-- SQL to delete all auth rules associated to REST controllers or actions
-- This DELETE statement gets pasted into the migration so that following INSERTS will work
/*
DELETE A
FROM [Auth] A
	INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] 
		AND ET.[Guid] IN ('D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D', '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D')
*/

-- Generate SQL to create inserts for current db's action auth rules
-- The output of this goes into the migration.  Pre-Beta: Can go to earlier REST migrations and empty them out 
SELECT 
	'INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) ' + CHAR(13) + CHAR(9) +
	'VALUES (' + CHAR(13) + CHAR(9) + CHAR(9) +
	'(SELECT [Id] FROM [EntityType] WHERE [Guid] = ''D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D''), ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CASE WHEN RA.[ApiId] IS NOT NULL 
		THEN '(SELECT [Id] FROM [RestAction] WHERE [ApiId] = ''' + RA.[ApiId] + ''')'
		ELSE '0' END + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CAST(A.[Order] AS varchar) + ', ''' +
	A.[Action] + ''', ''' +
	A.[AllowOrDeny] + ''', ' +
	CAST(A.[SpecialRole] as varchar) + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CASE WHEN G.[Id] IS NOT NULL THEN 
		'(SELECT [Id] FROM [Group] WHERE [Guid] = ''' + 
		CAST(G.[Guid] AS varchar(64)) + ''')' ELSE 'NULL' END + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	'''' + CAST(A.[Guid] AS varchar(64))+ ''')'  + CHAR(13) + CHAR(13) [Add RestAction Auth]
FROM [Auth] A
INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'
LEFT OUTER JOIN [RestAction] RA ON RA.[Id] = A.[EntityId]
LEFT OUTER JOIN [Group] G ON G.[Id] = A.[GroupId]

-- Generate SQL to create inserts for current db's controller auth rules
-- The output of this goes into the migration.  Pre-Beta: Can go to earlier REST migrations and empty them out 
SELECT 
	'INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) ' + CHAR(13) + CHAR(9) +
	'VALUES (' + CHAR(13) + CHAR(9) + CHAR(9) +
	'(SELECT [Id] FROM [EntityType] WHERE [Guid] = ''65CDFD5B-A9AA-48FA-8D22-669612D5EA7D''), ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CASE WHEN RC.[ClassName] IS NOT NULL 
		THEN '(SELECT [Id] FROM [RestController] WHERE [ClassName] = ''' + RC.[ClassName] + ''')' 
		ELSE '0' END + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CAST(A.[Order] AS varchar) + ', ''' +
	A.[Action] + ''', ''' +
	A.[AllowOrDeny] + ''', ' +
	CAST(A.[SpecialRole] as varchar) + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	CASE WHEN G.[Id] IS NOT NULL THEN 
		'(SELECT [Id] FROM [Group] WHERE [Guid] = ''' + 
		CAST(G.[Guid] AS varchar(64)) + ''')' ELSE 'NULL' END + ', ' + CHAR(13) + CHAR(9) + CHAR(9) +
	'''' + CAST(A.[Guid] AS varchar(64))+ ''')'  + CHAR(13) + CHAR(13) [Add RestController Auth]
FROM [Auth] A
INNER JOIN [EntityType] ET ON ET.[Id] = A.[EntityTypeId] AND ET.[Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'
LEFT OUTER JOIN [RestController] RC ON RC.[Id] = A.[EntityId]
LEFT OUTER JOIN [Group] G ON G.[Id] = A.[GroupId]
ORDER BY RC.Id desc

