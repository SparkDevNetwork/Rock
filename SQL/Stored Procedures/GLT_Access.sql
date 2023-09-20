CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_GLT_Access @PersonAliasGuid NVARCHAR(40)
AS
BEGIN

	DECLARE @PersonId INT;
	SELECT @PersonId = p.Id
	FROM [Person] p
	JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);

	-- English Inductive Curriculum
	DECLARE @EIC INT;
	SELECT @EIC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (103880,97583)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5658) -- whose language is English or not specified
		AND (curriculum.Id IS NULL OR curriculum.Id = 6134) -- whose curriculum is Inductive Bible Study or not specified
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- English Sermon-Based Curriculum
	DECLARE @ESBC INT;
	SELECT @ESBC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (103880,97583)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5658) -- whose language is English or not specified
		AND (curriculum.Id IS NOT NULL AND curriculum.Id = 6135) -- whose curriculum is specified and is Sermon-Based Curriculum
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- English Transitional Curriculum
	DECLARE @ETC INT;
	SELECT @ETC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (103880,97583)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5658) -- whose language is English or not specified
		AND (curriculum.Id IS NOT NULL AND curriculum.Id = 6512) -- whose curriculum is specified and is Transitional Curriculum
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- English Leader Resources
	DECLARE @ELR INT;
	SELECT @ELR = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607, 381, 558, 383, 611, 25) -- all types of Adult Life Groups
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId 
			AND NOT gtr.ID IN (962,1369,965,1731,1732,1733,23,1043,1050,1370,1371,1372,1373,1380,1483,1770,1772,1773,1775,1777) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5658) -- whose language is English or not specified
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Spanish Inductive Curriculum
	DECLARE @SIC INT;
	SELECT @SIC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId, g.Id, g.Name, language.Id, curriculum.Id
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (106484,106485)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NOT NULL AND language.Id = 5659) -- whose language is specified and Spanish
		AND (curriculum.Id IS NULL OR curriculum.Id = 6545) -- whose curriculum is Inductive Bible Study or not specified
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Spanish Sermon-Based Curriculum
	DECLARE @SSBC INT;
	SELECT @SSBC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (106484,106485)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5659) -- whose language is specified and Spanish
		AND (curriculum.Id IS NOT NULL AND curriculum.Id = 6546) -- whose curriculum is specified and is Sermon-Based Curriculum
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Spanish Transitional Curriculum
	DECLARE @STC INT;
	SELECT @STC = CASE WHEN EXISTS
	(
		SELECT gm.PersonId, g.Id, g.Name, language.Id, curriculum.Id
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607) -- groups of on campus and off campus types
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId AND gtr.ID NOT IN (962,1369,965,1731,1732,1733) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT c.EntityId, cdv.Id
			FROM [AttributeValue] c
			JOIN [DefinedValue] cdv ON cdv.Guid = TRY_CAST(c.Value AS UNIQUEIDENTIFIER)
			WHERE c.AttributeId IN (106484,106485)
		) curriculum ON curriculum.EntityId = g.Id
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NULL OR language.Id = 5659) -- whose language is specified and Spanish
		AND (curriculum.Id IS NOT NULL AND curriculum.Id = 6547) -- whose curriculum is specified and is Transitional Curriculum
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Spanish Leader Resources
	DECLARE @SLR INT;
	SELECT @SLR = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (327, 607, 381, 558, 383, 611, 25) -- all types of Adult Life Groups
		JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId 
			AND gtr.ID NOT IN (962,1369,965,1731,1732,1733,23,1043,1050,1370,1371,1372,1373,1380,1483,1770,1772,1773,1775,1777) -- and role is not Member or Prospect or Prospective Member
		LEFT JOIN
		(
			SELECT l.EntityId, ldv.Id
			FROM [AttributeValue] l
			JOIN [DefinedValue] ldv ON ldv.Guid = TRY_CAST(l.Value AS UNIQUEIDENTIFIER)
			WHERE l.AttributeId IN (106483,106486)
		) language ON language.EntityId = g.Id
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND (language.Id IS NOT NULL AND language.Id = 5659) -- whose language is specified and Spanish
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Override Groups
	DECLARE @AO INT;
	SELECT @AO = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm
		WHERE gm.GroupId IN (1002372, 800522)
			AND gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Kids Leaders
	DECLARE @KL INT;
	SELECT @KL = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (408,412,414,422,424,425,527) -- groups of kids ministry types
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;

	-- Students Leaders
	DECLARE @SL INT;
	SELECT @SL = CASE WHEN EXISTS
	(
		SELECT gm.PersonId
		FROM [GroupMember] gm -- members of
		JOIN [Group] g ON g.Id = gm.GroupId AND g.GroupTypeId IN (431,432) -- groups of student ministry types
		WHERE gm.IsArchived = 0 AND gm.InactiveDateTime IS NULL -- and is active
		AND gm.PersonId = @PersonId
	) THEN 1 ELSE 0 END;


	SELECT @PersonId AS [PersonId], 
		CASE WHEN @AO = 1 THEN 1 ELSE @EIC END AS [EnglishInductiveCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @ESBC END AS [EnglishSermonBasedCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @ETC END AS [EnglishTransitionalCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @ELR END AS [EnglishLeaderResources],
		CASE WHEN @AO = 1 THEN 1 ELSE @SIC END AS [SpanishInductiveCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @SSBC END AS [SpanishSermonBasedCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @STC END AS [SpanishTransitionalCurriculum],
		CASE WHEN @AO = 1 THEN 1 ELSE @SLR END AS [SpanishLeaderResources],
		@KL AS [KidsLeader],
		@SL AS [StudentLeader]

END;
GO