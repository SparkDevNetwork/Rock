DECLARE @PageEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Page' ) 
DECLARE @FinancePageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7BEB7569-C485-40A0-A609-B0678F6F7240' ) 
DECLARE @FinanceFunctionsPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '142627AE-6590-48E3-BFCA-3669260B8CF2' ) 
DECLARE @FinanceAdminPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C' ) 
DECLARE @BenevolenceGroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '02FA0881-3552-42B8-A519-D021139B800F' )

IF @PageEntityTypeId IS NOT NULL 
	AND @FinancePageId IS NOT NULL 
	AND @FinanceFunctionsPageId IS NOT NULL
	AND @FinanceAdminPageId IS NOT NULL
	AND @BenevolenceGroupId IS NOT NULL
BEGIN

	-- If Finance Functions page does not have any security...
	IF NOT EXISTS ( 
		SELECT [Id] 
		FROM [Auth] 
		WHERE [EntityTypeId] = @PageEntityTypeId 
		AND [EntityId] = @FinanceFunctionsPageId
	)
	BEGIN

		-- Copy Finance security to Finance/Functions
		INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [PersonAliasId], [Guid] )
		SELECT [EntityTypeId], @FinanceFunctionsPageId, [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [PersonAliasId], NEWID()
		FROM [Auth]
		WHERE [EntityTypeId] = @PageEntityTypeId 
		AND [Action] = 'View'
		AND [EntityId] = @FinancePageId

		-- Remove the deny benevolence group authorization from each child page where that is the only rule
		DELETE [Auth] 
		WHERE [EntityTypeId] = @PageEntityTypeId
		AND [Action] = 'View'
		AND [EntityId] IN ( 
			SELECT [Id] 
			FROM [Page] P
			WHERE P.[ParentPageId] = @FinanceFunctionsPageId
			AND P.[Id] NOT IN (
				SELECT [EntityId]
				FROM [Auth]
				WHERE [EntityTypeId] = @PageEntityTypeId
				AND [Action] = 'View'
				AND  ( [GroupId] IS NULL OR [GroupId] <> @BenevolenceGroupId )
			)
		)

		-- Copy the parent auths to any page that has no auths
		INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [PersonAliasId], [Guid] )
		SELECT A.[EntityTypeId], P.[Id], A.[Order], A.[Action], A.[AllowOrDeny], A.[SpecialRole], A.[GroupId], A.[PersonAliasId], NEWID()
		FROM [Page] P
		INNER JOIN [Auth] A 
			ON [EntityTypeId] = @PageEntityTypeId 
			AND [EntityId] = P.[ParentPageId]
			AND [Action] = 'View'
			AND ( [GroupId] IS NULL OR [GroupId] <> @BenevolenceGroupId )
		WHERE P.[ParentPageId] = @FinanceFunctionsPageId
		AND P.[Id] NOT IN (
			SELECT [EntityId]
			FROM [Auth]
			WHERE [EntityTypeId] = @PageEntityTypeId
			AND [Action] = 'View'
		)

		-- If only a benevolence rule exists for admin page
		IF NOT EXISTS (
			SELECT [Id]
			FROM [Auth]
			WHERE [EntityTypeId] = @PageEntityTypeId
			AND [EntityId] = @FinanceAdminPageId
			AND [Action] = 'View'
			AND  ( [GroupId] IS NULL OR [GroupId] <> @BenevolenceGroupId )
		)		
		BEGIN

			-- Delete the Benevolence rule
			DELETE [Auth]
			WHERE [EntityTypeId] = @PageEntityTypeId
			AND [EntityId] = @FinanceAdminPageId
			AND [Action] = 'View'
			AND  [GroupId] = @BenevolenceGroupId

			-- Copy the parent page's rules except the benevolence rule
			INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [PersonAliasId], [Guid] )
			SELECT [EntityTypeId], @FinanceAdminPageId, [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [PersonAliasId], NEWID()
			FROM [Auth]
			WHERE [EntityTypeId] = @PageEntityTypeId 
			AND [EntityId] = @FinancePageId
			AND [Action] = 'View'
			AND ( [GroupId] IS NULL OR [GroupId] <> @BenevolenceGroupId )

		END
	END
END
