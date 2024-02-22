
/********************************************************************************************************************
 Sign-Ups - View slots available by opportunity.
*********************************************************************************************************************/

DECLARE @SignUpGroupTypeId [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '499B1367-06B3-4538-9D56-56D53F55DCB1');

DECLARE @Opportunity TABLE
(
    [GroupId] [int] NOT NULL
    , [LocationId] [int] NOT NULL
    , [ScheduleId] [int] NOT NULL
    , [ProjectName] [nvarchar](100) NOT NULL
    , [OpportunityName] [nvarchar](100) NULL
    , [EffectiveStartDate] [date] NULL
    , [EffectiveEndDate] [date] NULL
    , [iCalendarContent] [nvarchar](max)
    , [SlotsMin] [int] NULL
    , [SlotsDesired] [int] NULL
    , [SlotsMax] [int] NULL
    , [ParticipantCount] [int] NOT NULL
);

WITH CTE AS
(
    SELECT g.[Id] AS [GroupId]
        , gl.[LocationId] AS [LocationId]
        , glsc.[ScheduleId] AS [ScheduleId]
        , g.[Name] AS [ProjectName]
        , glsc.[ConfigurationName] AS [OpportunityName]
        , s.[EffectiveStartDate]
        , s.[EffectiveEndDate]
        , s.[iCalendarContent]
        , glsc.[MinimumCapacity] AS [SlotsMin]
        , glsc.[DesiredCapacity] AS [SlotsDesired]
        , glsc.[MaximumCapacity] AS [SlotsMax]
    FROM [GroupLocation] gl
    INNER JOIN [Group] g
        ON g.[Id] = gl.[GroupId]
    INNER JOIN [GroupType] gt
        ON gt.[Id] = g.[GroupTypeId]
    INNER JOIN [GroupLocationScheduleConfig] glsc
        ON glsc.[GroupLocationId] = gl.[Id]
    INNER JOIN [Schedule] s
        ON s.[Id] = glsc.[ScheduleId]
    WHERE gt.[Id] = @SignUpGroupTypeId OR gt.[InheritedGroupTypeId] = @SignUpGroupTypeId
)
INSERT INTO @Opportunity
SELECT CTE.*
    , COUNT(gma.[Id]) AS [ParticipantCount]
FROM CTE
LEFT OUTER JOIN [GroupMember] gm
    ON gm.[GroupId] = CTE.[GroupId]
LEFT OUTER JOIN [GroupMemberAssignment] gma
    ON gma.[GroupMemberId] = gm.[Id]
        AND gma.[LocationId] = CTE.[LocationId]
        AND gma.[ScheduleId] = CTE.[ScheduleId]
GROUP BY CTE.[GroupId]
    , CTE.[LocationId]
    , CTE.[ScheduleId]
    , CTE.[ProjectName]
    , CTE.[OpportunityName]
    , CTE.[EffectiveStartDate]
    , CTE.[EffectiveEndDate]
    , CTE.[iCalendarContent]
    , CTE.[SlotsMin]
    , CTE.[SlotsDesired]
    , CTE.[SlotsMax]
ORDER BY CTE.[EffectiveStartDate]
    , CTE.[ProjectName]
    , [ParticipantCount] DESC;

SELECT 'Opportunity' AS [Entity]
    , *
    , SUM(CASE WHEN [SlotsMax] IS NOT NULL AND [SlotsMax] > 0 THEN [SlotsMax] ELSE 0 END - [ParticipantCount]) AS [SlotsAvailable]
FROM @Opportunity
GROUP BY [GroupId]
    , [LocationId]
    , [ScheduleId]
    , [ProjectName]
    , [OpportunityName]
    , [EffectiveStartDate]
    , [EffectiveEndDate]
    , [iCalendarContent]
    , [SlotsMin]
    , [SlotsDesired]
    , [SlotsMax]
    , [ParticipantCount]
ORDER BY [EffectiveStartDate]
    , [ProjectName]
    , [ParticipantCount] DESC
