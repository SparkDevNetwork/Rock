IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCrm_FamilyAnalyticsUpdateVisitDates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCrm_FamilyAnalyticsUpdateVisitDates]
GO

/*
<doc>
	<summary>
 		This stored procedure attempts to update an indivdiual's first and second visit
		attributes. Below are a few points of interest on the logic.

		+ A child's date will only be calculated looking at their check-in data.
		+ An adult's date will be calculated looking at the check-in data of all the children.

		+ When calculating a first-visit date it will only write the date if the first checkin date
		  is within 14 days of the record being created. This helps eliminate bad first-time visit dates
		  for adults who have attended for quite some time and then have a child later.
		  
	</summary>

	<returns>
	</returns>
	<remarks>
		
	</remarks>
	<code>
		EXEC [dbo].[spCrm_FamilyAnalyticsFirstVisitsAttributeUpdate] 
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCrm_FamilyAnalyticsUpdateVisitDates]
AS

DECLARE @FirstVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '655D6FBA-F8C0-4919-9E31-C1C936653555')
DECLARE @SecondVisitAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D7EA578F-0132-473E-8338-4A48DDB8BF66')

DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')
DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

-- clean up any empty attribute values
DELETE FROM [AttributeValue] WHERE [Value] = '' AND AttributeId = @FirstVisitAttributeId
DELETE FROM [AttributeValue] WHERE [Value] = '' AND AttributeId = @SecondVisitAttributeId

--
-- FIRST VISIT DATES
--
-- update children's records with their specific first visit date
IF @FirstVisitAttributeId IS NOT NULL
BEGIN
	INSERT INTO [AttributeValue]
	([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
	SELECT 1, @FirstVisitAttributeId, p.Id, a.FirstVisit, newid(), getdate()
	FROM [Person] p
	CROSS APPLY ( 
		SELECT 
			MIN([StartDateTime]) [FirstVisit]
		FROM
			[Attendance] a
			INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
		WHERE 
			[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
			AND a.[DidAttend] = 1
			AND pa.[PersonId] = p.[Id]
	) a
	WHERE
		p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId )
		AND a.FirstVisit < DATEADD(d, 14, p.CreatedDateTime)
	
	-- next process adults (they look at the first visit of anyone in the family)
	INSERT INTO [AttributeValue]
	([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
	SELECT 1, @FirstVisitAttributeId, p.Id, MIN(a.FirstVisit), newid(), getdate()
	FROM [Person] p
	INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupRoleId] = @AdultRoleId
	CROSS APPLY ( 
		SELECT 
			MIN([StartDateTime]) [FirstVisit]
		FROM
			[Attendance] a
			INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
			INNER JOIN [GroupMember] agm ON agm.[PersonId] = pa.[PersonId] AND agm.[GroupRoleId] = @ChildRoleId
		WHERE 
			a.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
			AND a.[DidAttend] = 1
			AND agm.[GroupId] = gm.[GroupId]
	) a
	WHERE
		p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId)
	GROUP BY p.[Id]
END
--
-- SECOND VISIT DATES
--
-- update children's records with their specific first visit date
IF @SecondVisitAttributeId IS NOT NULL
BEGIN
	INSERT INTO [AttributeValue]
	([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
	SELECT 1, @SecondVisitAttributeId, p.Id, a.[SecondVisit], newid(), getdate()
	FROM [Person] p
	CROSS APPLY ( 
		SELECT 
			MIN([StartDateTime]) [SecondVisit]
		FROM
			[Attendance] a
			INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
		WHERE 
			[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
			AND pa.[PersonId] = p.[Id]
			AND a.[DidAttend] = 1
			AND CONVERT(date, a.[StartDateTime]) > (SELECT MAX([ValueAsDateTime]) FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [EntityId] = pa.[PersonId] AND [IsSystem] = 1)
	) a
	WHERE
		p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @SecondVisitAttributeId )
		AND p.[Id] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [ValueAsDateTime] IS NOT NULL )
		AND a.[SecondVisit] IS NOT NULL

	
	-- next process adults (they look at the first visit of anyone in the family)
	INSERT INTO [AttributeValue]
	([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime])
	SELECT 1, @SecondVisitAttributeId, p.[Id], MIN(a.[SecondVisit]), newid(), getdate()
	FROM [Person] p
	INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupRoleId] = @AdultRoleId
	CROSS APPLY ( 
		SELECT 
			MIN([StartDateTime]) [SecondVisit]
		FROM
			[Attendance] a
			INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
			INNER JOIN [GroupMember] agm ON agm.[PersonId] = pa.[PersonId] AND agm.[GroupRoleId] = @ChildRoleId
		WHERE 
			a.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
			AND agm.[GroupId] = gm.[GroupId]
			AND a.[DidAttend] = 1
			AND CONVERT(date, a.[StartDateTime]) > (SELECT MAX([ValueAsDateTime]) FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [EntityId] = pa.[PersonId] AND [IsSystem] = 1)
	) a
	WHERE
		p.[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @SecondVisitAttributeId )
		AND p.[Id] IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @FirstVisitAttributeId AND [ValueAsDateTime] IS NOT NULL )
		AND a.[SecondVisit] IS NOT NULL
	GROUP BY p.[Id]
END