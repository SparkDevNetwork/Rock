DECLARE @GroupTypeId_SignUpGroup [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '499B1367-06B3-4538-9D56-56D53F55DCB1');

SELECT 'Group' AS Entity, g.*
FROM [Group] g
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'GroupLocation' AS Entity, gl.*
FROM [GroupLocation] gl
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT '[GroupLocation]Location' AS Entity, l.*
FROM [Location] l
INNER JOIN [GroupLocation] gl
     ON gl.[LocationId] = l.[Id]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'Schedule' AS Entity, s.*
FROM [Schedule] s
INNER JOIN [Group] g
     ON g.[ScheduleId] = s.[Id]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

/***************************************************************************
* THESE REPRESENT ROWS IN THE NEW GROUP DETAIL BLOCK'S OPPORTUNITIES GRID.
***************************************************************************/
SELECT 'GroupLocationSchedule (Join Table)' AS Entity, gls.*
FROM [GroupLocationSchedule] gls
INNER JOIN [GroupLocation] gl
     ON gl.[Id] = gls.[GroupLocationId]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT '[GroupLocation]Schedule' AS Entity, s.*
FROM [Schedule] s
INNER JOIN [GroupLocationSchedule] gls
     ON gls.[ScheduleId] = s.[Id]
INNER JOIN [GroupLocation] gl
     ON gl.[Id] = gls.[GroupLocationId]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'GroupLocationScheduleConfig' AS Entity, glsc.*
FROM [GroupLocationScheduleConfig] glsc
INNER JOIN [GroupLocation] gl
     ON gl.[Id] = glsc.[GroupLocationId]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'GroupMember' AS Entity, p.[FirstName] + ' ' + p.[LastName] AS 'Person', gm.*
FROM [GroupMember] gm
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
INNER JOIN [Person] p
     ON p.[Id] = gm.[PersonId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup
ORDER BY p.[LastName], p.[FirstName];

SELECT 'GroupMemberAssignment' AS Entity, p.[FirstName] + ' ' + p.[LastName] AS 'Person', gma.*
FROM [GroupMemberAssignment] gma
INNER JOIN [GroupMember] gm
     ON gm.Id = gma.GroupMemberId
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
INNER JOIN [Person] p
     ON p.[Id] = gm.[PersonId]
WHERE (gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup)
ORDER BY p.[LastName], p.[FirstName];
