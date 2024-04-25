
/********************************************************************************************************************
 Group Schedule Toolbox - View "Update Schedule Preferences" Data
*********************************************************************************************************************/

DECLARE @PersonIds TABLE ([Id] [int] NOT NULL);
INSERT INTO @PersonIds
VALUES
  (0) -- Leave this here for ease of commenting/uncommenting the below lines.
, (1) -- Admin Admin
--, (5) -- Ted Decker
--, (6) -- Cindy Decker
--, (7) -- Noah Decker
--, (8) -- Alex Decker
;

DECLARE @OverrideHideFromToolbox [bit] = 0;

/***************************
 DON'T EDIT ANYTHING BELOW.
****************************/

---------------------------------------------------------------------------------------------------

DECLARE @GroupMemberIds TABLE ([Id] [int] NOT NULL);

INSERT INTO @GroupMemberIds
SELECT gm.[Id]
FROM [GroupMember] gm
INNER JOIN @PersonIds pid
    ON pid.[Id] = gm.[PersonId]
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
INNER JOIN [GroupType] gt
    ON gt.[Id] = g.[GroupTypeId]
WHERE g.[IsActive] = 1
    AND g.[IsArchived] = 0
    AND g.[DisableScheduling] = 0
    AND (@OverrideHideFromToolbox = 1 OR g.[DisableScheduleToolboxAccess] = 0)
    AND gt.[IsSchedulingEnabled] = 1
    AND gm.[IsArchived] = 0
    AND gm.[GroupMemberStatus] = 1;

SELECT 'GroupMember' AS [Entity]
    , gm.[Id] AS [GroupMemberId]
    , p.[Id] AS [PersonId]
    , CONCAT(p.[NickName], ' ', p.[LastName]) AS 'Name'
    , g.[Id] AS [GroupId]
    , g.[Name] AS [GroupName]
    , gtr.[Name] AS [GroupTypeName]
    , gm.[ScheduleReminderEmailOffsetDays] AS [SendRemindersDaysBefore]
    , gm.[ScheduleTemplateId] AS [ScheduleTemplateId]
    , gmst.[Name] AS [ScheduleTemplateName]
    , gm.[ScheduleStartDate]
FROM [GroupMember] gm
INNER JOIN @GroupMemberIds gmi
    ON gmi.[Id] = gm.[Id]
INNER JOIN [Person] p
    ON p.[Id] = gm.[PersonId]
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
INNER JOIN [GroupTypeRole] gtr
    ON gtr.[Id] = gm.[GroupRoleId]
LEFT OUTER JOIN [GroupMemberScheduleTemplate] gmst
    ON gmst.[Id] = gm.[ScheduleTemplateId]
ORDER BY p.[PrimaryFamilyId]
    , p.[AgeClassification]
    , p.[Gender]
    , p.[Id]
    , g.[Order]
    , g.[Name]

SELECT 'GroupMemberAssignment' AS [Entity]
    , gma.[Id] AS [GroupMemberAssignmentId]
    , gm.[Id] AS [GroupMemberId]
    , p.[Id] AS [PersonId]
    , CONCAT(p.[NickName], ' ', p.[LastName]) AS 'Name'
    , g.[Id] AS [GroupId]
    , g.[Name] AS [GroupName]
    , s.[Id] AS [ScheduleName]
    , s.[Name] AS [ScheduleName]
    , l.[Id] AS [LocationId]
    , l.[Name] AS [LocationName]
FROM [GroupMemberAssignment] gma
INNER JOIN [GroupMember] gm
    ON gm.[Id] = gma.[GroupMemberId]
INNER JOIN @GroupMemberIds gmi
    ON gmi.[Id] = gm.[Id]
INNER JOIN [Person] p
    ON p.[Id] = gm.[PersonId]
INNER JOIN [Group] g
    ON g.[Id] = gm.[GroupId]
LEFT OUTER JOIN [Location] l
    ON l.[Id] = gma.[LocationId]
LEFT OUTER JOIN [Schedule] s
    ON s.[Id] = gma.[ScheduleId]
ORDER BY p.[PrimaryFamilyId]
    , p.[AgeClassification]
    , p.[Gender]
    , p.[Id]
    , g.[Order]
    , g.[Name]
    , s.[Order]
    , s.[Name]
    , s.[Id]
    , l.[Name]
    , l.[Id];
