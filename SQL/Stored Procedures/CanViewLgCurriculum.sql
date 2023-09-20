CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_CanViewLgCurriculum @PersonAliasGuid NVARCHAR(40)
AS
BEGIN
    -- Identify current user
    DECLARE @PersonId INT;
    SELECT @PersonId = p.Id
    FROM [Person] p
    JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);

    -- check if in an Override Group
    DECLARE @Override INT;
    SELECT @Override = CASE WHEN EXISTS
    (
        SELECT gm.PersonId
        FROM [GroupMember] gm
        WHERE gm.GroupId IN (1002372, 800522)
            AND gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL AND gm.PersonId = @PersonId
    ) THEN 1 ELSE 0 END;

    SELECT dv.Id, dv.Value, dv.Description, ldv.Id AS [LanguageId], ldv.Value AS [Language],
        CASE WHEN glt.Value IS NULL THEN 'True' ELSE glt.Value END AS [DisplayOnGLT] -- attribute defaults to true but may not be present
    FROM [DefinedValue] dv
    JOIN [AttributeValue] av ON TRY_CAST(av.Value AS UNIQUEIDENTIFIER) = dv.Guid
    JOIN [Attribute] a ON a.Id = av.AttributeId AND a.[Key] = 'GroupCurriculum'
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
    ) gd ON gd.Id = av.EntityId AND (@Override = 1 OR gd.PersonId = @PersonId OR gd.CoachId = @PersonId)
    JOIN [AttributeValue] l ON l.EntityId = dv.Id AND l.AttributeId = 107473
    JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER) AND ldv.DefinedTypeId = 350
    LEFT JOIN [AttributeValue] glt ON glt.EntityId = dv.Id AND glt.AttributeId = 108716
    WHERE dv.DefinedTypeId = 381 AND dv.IsActive = 1
    GROUP BY dv.Id, dv.Value, dv.Description, ldv.Id, ldv.Value, glt.Value
END;
GO