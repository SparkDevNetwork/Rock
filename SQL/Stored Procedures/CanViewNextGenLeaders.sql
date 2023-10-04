/*
	09/20/2022 Steve Swaringen - Created to use for the Leader Tools sidebar menu (https://my.lakepointe.church/page/1878)
	09/29/2023 Jon Corey - Updated to exclude all inactive groups
	10/02/2023 Jon Corey - Added column name to output so that Lava can reference it
    10/02/2023 Steve Swaringen - changed API to use group type tag instead of list of group type ids

	Test:

	DECLARE
		@pag NVARCHAR(40) = 'd75fb334-233c-42ee-be89-770a5d7a56e5',
		@gti NVARCHAR(255) = '460';
	EXEC _org_lakepointe_sp_CanViewNextGenLeaders @PersonAliasGuid = @pag, @GroupTypeIds = @gti
*/
CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_CanViewNextGenLeaders @PersonAliasGuid NVARCHAR(40), @GroupTypeTag NVARCHAR(255)
AS
BEGIN
    -- Next Gen Leaders
	DECLARE @PersonId INT;
	SELECT @PersonId = p.Id
	FROM [Person] p
	JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);

    DECLARE @GroupTypeTagId INT = CAST(@GroupTypeTag AS INT);
		
	-- Override Groups
	DECLARE @AO INT;
	SELECT @AO = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm
		JOIN [Group] g ON g.Id = gm.GroupId
		WHERE gm.GroupId IN (1002372, 800522)
			AND gm.IsArchived = 0 AND gm.GroupMemberStatus != 0 AND gm.PersonId = @PersonId
			AND g.IsActive = 1 AND g.IsArchived = 0
	) THEN 1 ELSE 0 END;

	SELECT CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm
		JOIN [Group] g ON g.Id = gm.GroupId
        JOIN [GroupType] gt ON gt.Id = g.GroupTypeId
        JOIN [TaggedItem] ti ON ti.EntityGuid = gt.Guid AND ti.TagId = @GroupTypeTagId -- 460 kids, 461 students
		WHERE gm.IsArchived = 0 AND gm.GroupMemberStatus != 0 AND g.IsActive = 1 AND g.IsArchived = 0
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE @AO END AS [Leader];
END;
GO