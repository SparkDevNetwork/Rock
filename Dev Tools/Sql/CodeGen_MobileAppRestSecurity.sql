DECLARE @ActionEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.RestAction' )
DECLARE @ControllerEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.RestController' )

SELECT DISTINCT
'            RockMigrationHelper.AddRestController( "' + c.Name + '", "' + c.ClassName + '" );'
FROM [Auth] A
INNER JOIN [Group] G ON G.[id] = A.[GroupId]
INNER JOIN [RestController] C ON C.[Id] = A.[EntityId]	AND A.[EntityTypeId] = @ControllerEntityTypeId
WHERE A.[EntityTypeId] = @ControllerEntityTypeId
AND G.[Guid] = 'EDD336D5-1429-41D9-8D41-2581A05F0E16'

UNION ALL

SELECT DISTINCT
'            RockMigrationHelper.AddRestAction( "' + c.Name + '", "' + c.ClassName + '", "' + r.Method + '", "' + r.Path + '" );' 
FROM [Auth] A
INNER JOIN [Group] G ON G.[id] = A.[GroupId]
INNER JOIN [RestAction] R ON R.[Id] = A.[EntityId] AND A.[EntityTypeId] = @ActionEntityTypeId
INNER JOIN [RestController] C ON C.[Id] = R.[ControllerId]
WHERE A.[EntityTypeId] = @ActionEntityTypeId
AND G.[Guid] = 'EDD336D5-1429-41D9-8D41-2581A05F0E16'

UNION ALL

SELECT
'            RockMigrationHelper.AddSecurityAuthForRestController( "' + c.ClassName + '", ' + CAST(a.[order] as varchar) + ', "' + a.action + '", ' + 
CASE WHEN a.AllowOrDeny = 'A' THEN 'true' ELSE 'false' END + ', "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "' + CAST(a.guid as varchar(60)) + '" );'
FROM [Auth] A
INNER JOIN [Group] G ON G.[id] = A.[GroupId]
INNER JOIN [RestController] C ON C.[Id] = A.[EntityId]	AND A.[EntityTypeId] = @ControllerEntityTypeId
WHERE A.[EntityTypeId] = @ControllerEntityTypeId
AND G.[Guid] = 'EDD336D5-1429-41D9-8D41-2581A05F0E16'

UNION ALL

SELECT
'            RockMigrationHelper.AddSecurityAuthForRestAction( "' + r.Method + '", "' + r.Path + '", ' + CAST(a.[order] as varchar) + ', "' + a.action + '", ' + 
CASE WHEN a.AllowOrDeny = 'A' THEN 'true' ELSE 'false' END + ', "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "' + CAST(a.guid as varchar(60)) + '" );'
FROM [Auth] A
INNER JOIN [Group] G ON G.[id] = A.[GroupId]
INNER JOIN [RestAction] R ON R.[Id] = A.[EntityId] AND A.[EntityTypeId] = @ActionEntityTypeId
INNER JOIN [RestController] C ON C.[Id] = R.[ControllerId]
WHERE A.[EntityTypeId] = @ActionEntityTypeId
AND G.[Guid] = 'EDD336D5-1429-41D9-8D41-2581A05F0E16'
