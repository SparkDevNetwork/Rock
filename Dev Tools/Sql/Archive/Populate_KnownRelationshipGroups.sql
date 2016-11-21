--Adds a Known Relationship group for each person in DB that does not have one

DECLARE @RelationshipId int
DECLARE @GroupTypeId int
DECLARE @OwnerRoleId int

SET @GroupTypeId = (
	SELECT [Id] 
	FROM [GroupType] WITH (NOLOCK)
	WHERE [Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
)

SET @OwnerRoleId = (
	SELECT [Id]
	FROM [GroupRole] WITH (NOLOCK)
	WHERE [Guid] = '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'
)

-- Find all the people that aren't owners of an Known Relationship group
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
	,'Relationships'
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
