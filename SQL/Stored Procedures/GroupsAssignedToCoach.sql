/****** Object:  StoredProcedure [dbo].[_org_lakepointe_sp_GroupsAssignedToCoach]    Script Date: 1/24/2024 8:17:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/************************************************************************************************************************************************
09/23/2022 Steve Swaringen - Created to use for the Leader Tools page
11/16/2022 Steve Swaringen - Updated to exclude inactive/archived groups
01/05/2024 Steve Swaringen - Updated to use new serve team structure instead of group attributes
06/18/2024 Jon Corey - Updated to use new life group structure instead of group attributes
************************************************************************************************************************************************/
ALTER   PROCEDURE [dbo].[_org_lakepointe_sp_GroupsAssignedToCoach] @PersonAliasGuid NVARCHAR(40)
AS
BEGIN
	-- Identify current user
	DECLARE @PersonId INT;
	SELECT @PersonId = p.Id
	FROM [Person] p
	JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);

	WITH leaders AS
	(
		SELECT p.Id, p.NickName, p.LastName, gm.GroupId
		FROM [GroupMember] gm
		JOIN [Person] p ON p.Id = gm.PersonId
		WHERE gm.GroupMemberStatus = 1 and gm.IsArchived = 0
	)

	-- old-style with coach attributes
	SELECT g.Id, g.Name, 'Coach' AS [Role]
	FROM [Group] g
	JOIN [AttributeValue] coachav ON coachav.EntityId = g.Id
	JOIN [Attribute] coacha ON coacha.Id = coachav.AttributeId AND coacha.[Key] = 'AGCoach' AND coacha.EntityTypeId = 16
	JOIN [PersonAlias] coach ON coach.Guid = TRY_CAST(coachav.Value AS UNIQUEIDENTIFIER) AND coach.PersonId = @PersonId
	WHERE g.IsActive = 1 AND g.IsArchived = 0 -- Added SNS 20221116 per https://helpdesk.lakepointe.org/app/itdesk/ui/requests/63677000019976129/details

	UNION ALL

	-- Serve Teams: New style from coach groups
	SELECT g.Id, g.Name, 'Coach' AS [Role]
	FROM [Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId AND gt.IsSchedulingEnabled = 1
	JOIN [Group] coachGroup ON coachGroup.Id = g.ParentGroupId
	JOIN leaders coach ON coach.GroupId = coachGroup.Id AND coach.Id = @PersonId
	WHERE gt.GroupTypePurposeValueId = 184 AND g.IsActive = 1 AND g.IsArchived = 0 AND g.DisableScheduling = 0

	UNION ALL

	-- Serve Teams: New style from captain groups
	SELECT g.Id, g.Name, 'Captain' AS [Role]
	FROM [Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId AND gt.IsSchedulingEnabled = 1
	JOIN [Group] coachGroup ON coachGroup.Id = g.ParentGroupId
	JOIN [Group] captainGroup ON captainGroup.Id = coachGroup.ParentGroupId
	JOIN leaders captain ON captain.GroupId = captainGroup.Id AND captain.Id = @PersonId
	WHERE gt.GroupTypePurposeValueId = 184 AND g.IsActive = 1 AND g.IsArchived = 0 AND g.DisableScheduling = 0

	UNION ALL

	-- Life Groups: New style from coach groups
	SELECT g.Id, g.Name, 'Coach' AS [Role]
	FROM [Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
	JOIN [Group] coachGroup ON coachGroup.Id = g.ParentGroupId
	JOIN leaders coach ON coach.GroupId = coachGroup.Id AND coach.Id = @PersonId
	WHERE gt.GroupTypePurposeValueId = 5856 AND g.IsActive = 1 AND g.IsArchived = 0

	UNION ALL

	-- Life Groups: New style from captain groups
	SELECT g.Id, g.Name, 'Captain' AS [Role]
	FROM [Group] g
	JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
	JOIN [Group] coachGroup ON coachGroup.Id = g.ParentGroupId
	JOIN [Group] captainGroup ON captainGroup.Id = coachGroup.ParentGroupId
	JOIN leaders captain ON captain.GroupId = captainGroup.Id AND captain.Id = @PersonId
	WHERE gt.GroupTypePurposeValueId = 5856 AND g.IsActive = 1 AND g.IsArchived = 0
END;