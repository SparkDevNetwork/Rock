DECLARE @EntityTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.Block'
		)
DECLARE @PersonEditBlockId INT = (
		SELECT TOP 1 [Id]
		FROM [Block]
		WHERE [Guid] = '59C7EA79-2073-4EA9-B439-7E74F06E8F5B'
		)
DECLARE @RockAdminGroupId INT = (
		SELECT TOP 1 [Id]
		FROM [Group]
		WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'
		)
DECLARE @Order INT = (
		SELECT MAX([Order])
		FROM [Auth]
		WHERE EntityTypeId = @EntityTypeId
			AND EntityId = @PersonEditBlockId
			AND [Action] = 'Edit'
		)

--select * from [Auth] 
IF NOT EXISTS (
		SELECT *
		FROM Auth
		WHERE Action = 'EditConnectionStatus'
			AND EntityTypeId = @EntityTypeId
			AND EntityId = @PersonEditBlockId
		)
BEGIN
	INSERT INTO [Auth] (
		EntityTypeid
		,EntityId
		,[Order]
		,[Action]
		,AllowOrDeny
		,SpecialRole
		,GroupId
		,PersonAliasId
		,[Guid]
		)
	SELECT EntityTypeid
		,EntityId
		,[Order]
		,'EditConnectionStatus'
		,AllowOrDeny
		,SpecialRole
		,GroupId
		,PersonAliasId
		,newid()
	FROM [Auth]
	WHERE EntityTypeId = @EntityTypeId
		AND EntityId = @PersonEditBlockId
		AND [Action] = 'Edit'

	IF @RockAdminGroupId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Auth] (
			[EntityTypeId]
			,[EntityId]
			,[Order]
			,[Action]
			,[AllowOrDeny]
			,[SpecialRole]
			,[GroupId]
			,[Guid]
			)
		VALUES (
			@EntityTypeId
			,@PersonEditBlockId
			,@Order + 1
			,'EditConnectionStatus'
			,'A'
			,0
			,@RockAdminGroupId
			,NEWID()
			)
	END

	INSERT INTO [dbo].[Auth] (
		[EntityTypeId]
		,[EntityId]
		,[Order]
		,[Action]
		,[AllowOrDeny]
		,[SpecialRole]
		,[Guid]
		)
	VALUES (
		@EntityTypeId
		,@PersonEditBlockId
		,@Order + 2
		,'EditConnectionStatus'
		,'D'
		,1
		,NEWID()
		)
END

IF NOT EXISTS (
		SELECT *
		FROM Auth
		WHERE Action = 'EditRecordStatus'
			AND EntityTypeId = @EntityTypeId
			AND EntityId = @PersonEditBlockId
		)
BEGIN
	INSERT INTO [Auth] (
		EntityTypeid
		,EntityId
		,[Order]
		,[Action]
		,AllowOrDeny
		,SpecialRole
		,GroupId
		,PersonAliasId
		,[Guid]
		)
	SELECT EntityTypeid
		,EntityId
		,[Order]
		,'EditRecordStatus'
		,AllowOrDeny
		,SpecialRole
		,GroupId
		,PersonAliasId
		,NEWID()
	FROM [Auth]
	WHERE EntityTypeId = @EntityTypeId
		AND EntityId = @PersonEditBlockId
		AND [Action] = 'Edit'

	IF @RockAdminGroupId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Auth] (
			[EntityTypeId]
			,[EntityId]
			,[Order]
			,[Action]
			,[AllowOrDeny]
			,[SpecialRole]
			,[GroupId]
			,[Guid]
			)
		VALUES (
			@EntityTypeId
			,@PersonEditBlockId
			,@Order + 1
			,'EditRecordStatus'
			,'A'
			,0
			,@RockAdminGroupId
			,NEWID()
			)
	END

	INSERT INTO [dbo].[Auth] (
		[EntityTypeId]
		,[EntityId]
		,[Order]
		,[Action]
		,[AllowOrDeny]
		,[SpecialRole]
		,[Guid]
		)
	VALUES (
		@EntityTypeId
		,@PersonEditBlockId
		,@Order + 2
		,'EditRecordStatus'
		,'D'
		,1
		,NEWID()
		)
END