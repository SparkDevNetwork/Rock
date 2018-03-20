-- Add 'ReopenBatch' as a security action on Rock.Model.FinancialBatch and default it to whatever might be there for EDIT on BatchDetail, and ensure that FinanceAdmin, FinanceWorker, and Admin have 'ReopenBatch' security
DECLARE @EntityTypeIdBlock INT = (
		SELECT TOP 1 [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.Block'
		)
DECLARE @EntityTypeIdFinancialBatch INT = (
		SELECT TOP 1 [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.FinancialBatch'
		)
DECLARE @BatchDetailBlockId INT = (
		SELECT TOP 1 [Id]
		FROM [Block]
		WHERE [Guid] = 'E7C8C398-0E1D-4BCE-BC54-A02957228514'
		)
DECLARE @RockAdminGroupId INT = (
		SELECT TOP 1 [Id]
		FROM [Group]
		WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'
		)
DECLARE @FinanceAdminGroupId INT = (
		SELECT TOP 1 [Id]
		FROM [Group]
		WHERE [Guid] = '6246A7EF-B7A3-4C8C-B1E4-3FF114B84559'
		)
DECLARE @FinanceUsersGroupId INT = (
		SELECT TOP 1 [Id]
		FROM [Group]
		WHERE [Guid] = '2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9'
		)
DECLARE @Order INT = 0

IF NOT EXISTS (
		SELECT *
		FROM Auth
		WHERE Action = 'ReopenBatch'
			AND EntityTypeId = @EntityTypeIdFinancialBatch
			AND EntityId = @BatchDetailBlockId
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
	SELECT @EntityTypeIdFinancialBatch
		,0
		,[Order]
		,'ReopenBatch'
		,AllowOrDeny
		,SpecialRole
		,GroupId
		,PersonAliasId
		,newid()
	FROM [Auth]
	WHERE EntityTypeId = @EntityTypeIdBlock
		AND EntityId = @BatchDetailBlockId
		AND [Action] = 'Edit'

	IF @FinanceUsersGroupId IS NOT NULL
		AND NOT EXISTS (
			SELECT *
			FROM Auth
			WHERE Action = 'ReopenBatch'
				AND EntityTypeId = @EntityTypeIdFinancialBatch
				AND EntityId = 0
				AND GroupId = @FinanceUsersGroupId
			)
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
			@EntityTypeIdFinancialBatch
			,0
			,@Order + 1
			,'ReopenBatch'
			,'A'
			,0
			,@FinanceUsersGroupId
			,NEWID()
			)
	END

	IF @FinanceAdminGroupId IS NOT NULL
		AND NOT EXISTS (
			SELECT *
			FROM Auth
			WHERE Action = 'ReopenBatch'
				AND EntityTypeId = @EntityTypeIdFinancialBatch
				AND EntityId = 0
				AND GroupId = @FinanceAdminGroupId
			)
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
			@EntityTypeIdFinancialBatch
			,0
			,@Order + 1
			,'ReopenBatch'
			,'A'
			,0
			,@FinanceAdminGroupId
			,NEWID()
			)
	END

	IF @RockAdminGroupId IS NOT NULL
		AND NOT EXISTS (
			SELECT *
			FROM Auth
			WHERE Action = 'ReopenBatch'
				AND EntityTypeId = @EntityTypeIdFinancialBatch
				AND EntityId = 0
				AND GroupId = @RockAdminGroupId
			)
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
			@EntityTypeIdFinancialBatch
			,0
			,@Order + 1
			,'ReopenBatch'
			,'A'
			,0
			,@RockAdminGroupId
			,NEWID()
			)
	END

	IF NOT EXISTS (
			SELECT *
			FROM Auth
			WHERE Action = 'ReopenBatch'
				AND EntityTypeId = @EntityTypeIdFinancialBatch
				AND EntityId = 0
				AND [SpecialRole] = 1
			)
	BEGIN
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
			@EntityTypeIdFinancialBatch
			,0
			,@Order + 2
			,'ReopenBatch'
			,'D'
			,1
			,NEWID()
			)
	END
END