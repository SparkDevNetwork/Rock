
/********************************************************************************************************************
 Sign-Ups - View Opportunities and supporting entities.
*********************************************************************************************************************/

DECLARE @GroupTypeId_SignUpGroup [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '499B1367-06B3-4538-9D56-56D53F55DCB1');

SELECT 'Group' AS [Entity], g.*
FROM [Group] g
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'GroupLocation' AS [Entity], gl.*
FROM [GroupLocation] gl
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT '[GroupLocation]Location' AS [Entity], l.*
FROM [Location] l
INNER JOIN [GroupLocation] gl
     ON gl.[LocationId] = l.[Id]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'Schedule' AS [Entity], s.*
FROM [Schedule] s
INNER JOIN [Group] g
     ON g.[ScheduleId] = s.[Id]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

/***************************************************************************
* THESE REPRESENT ROWS IN THE NEW GROUP DETAIL BLOCK'S OPPORTUNITIES GRID.
***************************************************************************/
SELECT 'GroupLocationSchedule (Join Table)' AS [Entity], gls.*
FROM [GroupLocationSchedule] gls
INNER JOIN [GroupLocation] gl
     ON gl.[Id] = gls.[GroupLocationId]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT '[GroupLocation]Schedule' AS [Entity], s.*
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

SELECT 'GroupLocationScheduleConfig' AS [Entity], glsc.*
FROM [GroupLocationScheduleConfig] glsc
INNER JOIN [GroupLocation] gl
     ON gl.[Id] = glsc.[GroupLocationId]
INNER JOIN [Group] g
    ON g.[Id] = gl.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup;

SELECT 'GroupMember' AS [Entity], p.[FirstName] + ' ' + p.[LastName] AS 'Person', gm.*
FROM [GroupMember] gm
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
INNER JOIN [Person] p
     ON p.[Id] = gm.[PersonId]
WHERE gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup
ORDER BY p.[LastName], p.[FirstName];

SELECT 'GroupMemberAssignment' AS [Entity], p.[FirstName] + ' ' + p.[LastName] AS 'Person', gma.*
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

SELECT 'Attendance' AS [Entity]
    , ao.[GroupId]
    , ao.[LocationId]
    , ao.[ScheduleId]
    , g.[Name] AS [ProjectName]
    , glsc.[ConfigurationName] AS [OpportunityName]
    , p.[FirstName] + ' ' + p.[LastName] AS 'Person'
    , gtr.[IsLeader]
    , a.*
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] ao
    ON ao.[Id] = a.[OccurrenceId]
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = a.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
INNER JOIN [GroupMember] gm
    ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = ao.[GroupId]
INNER JOIN [GroupTypeRole] gtr
    ON gtr.[Id] = gm.[GroupRoleId]
INNER JOIN [Group] g
    ON g.[Id] = ao.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
INNER JOIN [GroupLocation] gl
    ON gl.[GroupId] = ao.[GroupId] AND gl.[LocationId] = ao.[LocationId]
LEFT OUTER JOIN [GroupLocationScheduleConfig] glsc
    ON glsc.[GroupLocationId] = gl.Id AND glsc.[ScheduleId] = ao.[ScheduleId]
WHERE (gt.[Id] = @GroupTypeId_SignUpGroup OR gt.[InheritedGroupTypeId] = @GroupTypeId_SignUpGroup)
ORDER BY ao.[GroupId]
    , ao.[LocationId]
    , ao.[ScheduleId]
    , glsc.[ConfigurationName]
    , gtr.[IsLeader] DESC
    , p.[LastName]
    , p.[FirstName];
