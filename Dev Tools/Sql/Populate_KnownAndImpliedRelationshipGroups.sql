DECLARE @fKey AS NVARCHAR(MAX) = 'INDIVIDUAL ID FOR CREATING MISSING RELATIONSHIPS';
DELETE FROM [Group] WHERE ForeignKey = @fKey;

INSERT INTO [Group] (Name, ForeignId, ForeignKey, IsSystem, GroupTypeId, IsSecurityRole, IsActive, [Order], [Guid])
SELECT 
	'Known Relationship',
	p.Id,
	@fKey,
	0,
	11,
	0,
	1,
	0,
	NEWID()
FROM 
	Person p
WHERE 
	Id NOT IN (
		SELECT 
			DISTINCT p.Id
		FROM
			Person p
			JOIN GroupMember gm ON p.Id = gm.PersonId
			JOIN [Group] g ON gm.GroupId = g.Id
			JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId
		WHERE
			g.Name = 'Known Relationship'
			AND gtr.Name = 'Allow check in by'
			AND p.IsSystem = 0
	) AND p.IsSystem = 0;

INSERT INTO [Group] (Name, ForeignId, ForeignKey, IsSystem, GroupTypeId, IsSecurityRole, IsActive, [Order], [Guid])
SELECT 
	'Implied Relationship',
	p.Id,
	@fKey,
	0,
	12,
	0,
	1,
	0,
	NEWID()
FROM 
	Person p
WHERE 
	Id NOT IN (
		SELECT 
			DISTINCT p.Id
		FROM
			Person p
			JOIN GroupMember gm ON p.Id = gm.PersonId
			JOIN [Group] g ON gm.GroupId = g.Id
			JOIN [GroupTypeRole] gtr ON gtr.Id = gm.GroupRoleId
		WHERE
			g.Name = 'Implied Relationship'
			AND gtr.Name = 'Owner'
			AND p.IsSystem = 0
	) AND p.IsSystem = 0;

INSERT INTO GroupMember (PersonId, GroupId, GroupRoleId, GroupMemberStatus, IsSystem, [Guid])
SELECT
	ForeignId as PersonId,
	g.Id,
	8,
	2,
	0,
	NEWID()
FROM 
	[Group] g
WHERE
	g.Name = 'Known Relationship'
	AND ForeignKey = @fKey;

INSERT INTO GroupMember (PersonId, GroupId, GroupRoleId, GroupMemberStatus, IsSystem, [Guid])
SELECT
	ForeignId as PersonId,
	g.Id,
	6,
	2,
	0,
	NEWID()
FROM 
	[Group] g
WHERE
	g.Name = 'Implied Relationship'
	AND ForeignKey = @fKey;
