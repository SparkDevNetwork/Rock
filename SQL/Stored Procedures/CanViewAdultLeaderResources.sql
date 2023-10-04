CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_CanViewAdultLeaderResources @PersonAliasGuid NVARCHAR(40)
AS
BEGIN
    -- Adult LG Leader Resources
	DECLARE @PersonId INT;
	SELECT @PersonId = p.Id
	FROM [Person] p
	JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);
		
	-- Override Groups
	DECLARE @AO INT;
	SELECT @AO = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm
		WHERE gm.GroupId IN (1002372, 800522)
			AND gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;
	
	SELECT dv.Id, dv.Value, dv.Description
	FROM [DefinedValue] dv
	JOIN [AttributeValue] av ON TRY_CAST(av.Value AS UNIQUEIDENTIFIER) = dv.Guid
	JOIN [Attribute] a ON a.Id = av.AttributeId AND a.[Key] = 'Language'
	JOIN
	(
		SELECT g.Id, gm.PersonId, coach.CoachId
		FROM [Group] g 
		JOIN [GroupMember] gm ON gm.GroupId = g.Id AND gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId 
			AND gtr.ID NOT IN (962,1369,965,1731,1732,1733,23,1043,1050,1370,1371,1372,1373,1380,1483,1770,1772,1773,1775,1777) -- and role is not Member or Prospect or Prospective Member
        LEFT JOIN
        (
        SELECT coachav.EntityId AS GroupId, coach.PersonId AS CoachId
        FROM [AttributeValue] coachav --ON coachav.EntityId = g.Id
        JOIN [Attribute] coacha ON coacha.Id = coachav.AttributeId AND coacha.[Key] = 'AGCoach' AND coacha.EntityTypeId = 16
        JOIN [PersonAlias] coach ON coach.Guid = TRY_CAST(coachav.Value AS UNIQUEIDENTIFIER)
        ) coach ON coach.GroupId = g.Id
		WHERE g.GroupTypeId IN (327, 607, 381, 558, 383, 611, 25) -- all types of Adult Life Groups
	) gd ON gd.Id = av.EntityId AND (@AO = 1 OR gd.PersonId = @PersonId OR gd.CoachId = @PersonId)
	WHERE dv.DefinedTypeId = 350 AND dv.IsActive = 1
	GROUP BY dv.Id, dv.Value, dv.Description
END;
GO   