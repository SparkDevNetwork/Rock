CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_CanViewNextGenLeaders @PersonAliasGuid NVARCHAR(40), @GroupTypeIds NVARCHAR(255)
AS
BEGIN
    -- Next Gen Leaders
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

	SELECT CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (SELECT * FROM STRING_SPLIT(@GroupTypeIds, ','))
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE @AO END;
END;
GO