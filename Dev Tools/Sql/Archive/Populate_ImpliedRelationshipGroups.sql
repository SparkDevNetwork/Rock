--Adds an implied Relationship group for each person in DB that does not have one

DECLARE @RelationshipId int
DECLARE @GroupTypeId int
DECLARE @OwnerRoleId int
DECLARE @RelatedRoleId int

SET @GroupTypeId = (
	SELECT [Id] 
	FROM [GroupType] WITH (NOLOCK)
	WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E'
)

SET @OwnerRoleId = (
	SELECT [Id]
	FROM [GroupRole] WITH (NOLOCK)
	WHERE [Guid] = 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'
)

SET @RelatedRoleId = (
	SELECT [Id]
	FROM [GroupRole] WITH (NOLOCK)
	WHERE [Guid] = 'FEA75948-97CB-4DE9-8A0D-43FA2646F55B'
)

IF @RelatedRoleId IS NULL
BEGIN
	INSERT INTO [GroupRole] (
		 [IsSystem]
		,[GroupTypeId]
		,[Name]
		,[Description]
		,[Guid]
		,[IsLeader]
	) VALUES (
		 1
		,@GroupTypeId
		,'Related'
		,'Related person in an implied relationship'
		,'FEA75948-97CB-4DE9-8A0D-43FA2646F55B'
		,0
	)
	SET @RelatedRoleId = SCOPE_IDENTITY()
END

-- Find all the people that aren't owners of an Implied Relationship group
SELECT
	 RP.[id]
	,RP.[guid]
INTO #Persons
FROM [Person] RP WITH(NOLOCK)
LEFT OUTER JOIN [GroupMember] GM WITH (NOLOCK)
	ON GM.[PersonId] = RP.[Id]
	AND GM.[GroupRoleId] = @OwnerRoleId
WHERE GM.[Id] IS NULL

-- Create an implied Relationships group for each person
-- (Use person's guid so we know what group to add owner to in next step)
INSERT INTO [Group] (
	 [IsSystem]
	,[GroupTypeId]
	,[Name]
	,[IsSecurityRole]
	,[IsActive]
	,[Guid]
)
SELECT
	 0
	,@GroupTypeId
	,'Peer Network'
	,0
	,1
	,[guid]
FROM #Persons WITH (NOLOCK)

-- Add the owner to the group
INSERT INTO [GroupMember] (
	 [IsSystem]
	,[GroupId]
	,[PersonId]
	,[GroupRoleId]
	,[Guid]
)
SELECT
	0
	,G.[Id]
	,P.[Id]
	,@OwnerRoleId
	,NEWID()
FROM #Persons P WITH (NOLOCK)
INNER JOIN [Group] G WITH (NOLOCK)
	ON G.[GroupTypeId] = @GroupTypeId
	AND G.[Guid] = P.[Guid]

-- Reset the Group Guids
UPDATE G
SET [Guid] = NEWID()
FROM #Persons P WITH (NOLOCK)
INNER JOIN [Group] G WITH (NOLOCK)
	ON G.[Guid] = P.[Guid]

DROP TABLE #Persons
