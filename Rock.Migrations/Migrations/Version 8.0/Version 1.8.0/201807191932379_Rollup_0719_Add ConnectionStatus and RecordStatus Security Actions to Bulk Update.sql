-- We have secured Person.ConnectionStatus and Person.RecordStatus on the bulk upate block to follow conventions on the person edit block.
-- Now we need to add those that currently have access so they don't lose anything.

DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
DECLARE @BulkUpdateBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid] = 'A610AB9D-7397-4D27-8614-F6A282B78B2C')
DECLARE @RockAdminGroupId INT = (SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E')
DECLARE @Order INT = (SELECT MAX([Order]) FROM [Auth] WHERE EntityTypeId = @BlockEntityTypeId AND EntityId = @BulkUpdateBlockId AND [Action] = 'Edit')

SET @Order = ISNULL(@Order, 0)

-- If the block already has users with the new security settings then this has already been done and we don't want/need to do it again.
IF NOT EXISTS (
		SELECT TOP 1 [Id]
		FROM [Auth]
		WHERE [Action] IN ('EditConnectionStatus', 'EditRecordStatus')
			AND [EntityTypeId] = @BlockEntityTypeId
			AND [EntityId] = @BulkUpdateBlockId
		)
BEGIN
	-- Assign EditConnectionStatus to users with Edit for the block
	INSERT INTO [Auth] (
		 [EntityTypeId]
		,[EntityId]
		,[Order]
		,[Action]
		,[AllowOrDeny]
		,[SpecialRole]
		,[GroupId]
		,[PersonAliasId]
		,[Guid]
		)
	SELECT
		 [EntityTypeId]
		,[EntityId]
		,[Order]
		,'EditConnectionStatus'
		,[AllowOrDeny]
		,[SpecialRole]
		,[GroupId]
		,[PersonAliasId]
		,newid()
	FROM [Auth]
	WHERE EntityTypeId = @BlockEntityTypeId
		AND EntityId = @BulkUpdateBlockId
		AND [Action] = 'Edit'

	-- Assign EditRecordStatus to users with Edit for the block
	INSERT INTO [Auth] (
		 [EntityTypeId]
		,[EntityId]
		,[Order]
		,[Action]
		,[AllowOrDeny]
		,[SpecialRole]
		,[GroupId]
		,[PersonAliasId]
		,[Guid]
		)
	SELECT
		 [EntityTypeId]
		,[EntityId]
		,[Order]
		,'EditRecordStatus'
		,[AllowOrDeny]
		,[SpecialRole]
		,[GroupId]
		,[PersonAliasId]
		,newid()
	FROM [Auth]
	WHERE EntityTypeId = @BlockEntityTypeId
		AND EntityId = @BulkUpdateBlockId
		AND [Action] = 'Edit'

	-- Assign Security settings to the RockAdminGroup if it exists
	IF @RockAdminGroupId IS NOT NULL
	BEGIN
		-- Assign EditConnectionStatus to the RockAdminGroup if it exists
		INSERT INTO [dbo].[Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
		VALUES (@BlockEntityTypeId, @BulkUpdateBlockId, @Order + 1, 'EditConnectionStatus', 'A', 0, @RockAdminGroupId, NEWID())

		-- Assign EditRecordStatus to the RockAdminGroup if it exists
		INSERT INTO [dbo].[Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
		VALUES (@BlockEntityTypeId, @BulkUpdateBlockId, @Order + 1, 'EditRecordStatus', 'A', 0, @RockAdminGroupId, NEWID())
	END

	-- Deny everyone else EditConnectionStatus access
	INSERT INTO [dbo].[Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid])
	VALUES (@BlockEntityTypeId, @BulkUpdateBlockId, @Order + 2, 'EditConnectionStatus', 'D', 1, NEWID())

	-- Deny everyone else EditRecordStatus access
	INSERT INTO [dbo].[Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid])
	VALUES (@BlockEntityTypeId, @BulkUpdateBlockId, @Order + 2, 'EditRecordStatus', 'D', 1, NEWID())
	
END