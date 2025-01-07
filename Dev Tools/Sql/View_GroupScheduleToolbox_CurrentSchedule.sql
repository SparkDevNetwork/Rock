
/********************************************************************************************************************
 Group Schedule Toolbox - View "Current Schedule" Data
*********************************************************************************************************************/

DECLARE @PersonAliasIds TABLE ([Id] [int] NOT NULL);
INSERT INTO @PersonAliasIds
VALUES
  (0) -- Leave this here for ease of commenting/uncommenting the below lines.
, (10) -- Admin Admin
, (17) -- Ted Decker
, (16) -- Cindy Decker
--, (15) -- Noah Decker
--, (14) -- Alex Decker
;

/***************************
 DON'T EDIT ANYTHING BELOW.
****************************/

---------------------------------------------------------------------------------------------------

DECLARE @Today [date] = (SELECT CAST(GETDATE() AS [date]));

SELECT 'Attendance' AS [Entity]
    , p.[Id] AS [PersonId]
    , CONCAT(p.[NickName], ' ', p.[LastName]) AS 'Name'
    , a.[Id] AS [AttendanceId]
    , g.[Name] AS [GroupName]
    , l.[Name] AS [LocationName]
    , s.[Name] AS [SchedlueName]
    , ao.[OccurrenceDate]
    , CASE
        WHEN a.[RSVP] = 0 THEN 'No'
        WHEN a.[RSVP] = 1 THEN 'Yes'
        WHEN a.[RSVP] = 2 THEN 'Maybe'
        WHEN a.[RSVP] = 3 THEN 'Unknown'
      END AS [RSVP]
    , a.[RequestedToAttend]
    , a.[ScheduledToAttend]
    , a.[DidAttend]
    , dv.[Value] AS [DeclineReason]
    , a.[Note]
    , CONCAT(pScheduledBy.[NickName], ' ', pScheduledBy.[LastName]) AS 'ScheduledByName'
FROM [Attendance] a
INNER JOIN @PersonAliasIds pai
    ON pai.[Id] = a.[PersonAliasId]
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = a.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
INNER JOIN [AttendanceOccurrence] ao
    ON ao.[Id] = a.[OccurrenceId]
LEFT OUTER JOIN [Group] g
    ON g.[Id] = ao.[GroupId]
LEFT OUTER JOIN [Location] l
    on l.[Id] = ao.[LocationId]
LEFT OUTER JOIN [Schedule] s
    ON s.[Id] = ao.[ScheduleId]
LEFT OUTER JOIN [DefinedValue] dv
    ON dv.[Id] = a.[DeclineReasonValueId]
LEFT OUTER JOIN [PersonAlias] paScheduledBy
    ON paScheduledBy.[Id] = a.[ScheduledByPersonAliasId]
LEFT OUTER JOIN [Person] pScheduledBy
    ON pScheduledBy.[Id] = paScheduledBy.[PersonId]
WHERE 1 = 1
    AND a.[RequestedToAttend] = 1
    AND ao.[OccurrenceDate] >= @Today
ORDER BY p.[PrimaryFamilyId]
    , p.[Id]
    , ao.[OccurrenceDate]
    , s.[Order]
    , s.[Name]
    , s.[Id]
    , g.[Name]
    , l.[Name];

DECLARE @PersonScheduleExclusionIds TABLE ([Id] [int]);
INSERT INTO @PersonScheduleExclusionIds
SELECT pse.[Id]
FROM [PersonScheduleExclusion] pse
INNER JOIN @PersonAliasIds pai
    ON pai.[Id] = pse.[PersonAliasId]
WHERE pse.[StartDate] >= @Today
    OR pse.[EndDate] >= @Today;

SELECT 'PersonScheduleExclusion' AS [Entity]
    , p.[Id] AS [PersonId]
    , CONCAT(p.[NickName], ' ', p.[LastName]) AS 'Name'
    , pse.[Id] AS [ExclusionId]
    , pse.[ParentPersonScheduleExclusionId] AS [ParentExclusionId]
    , CASE
        WHEN g.[Name] IS NULL THEN 'All Groups'
        ELSE g.[Name]
      END AS [GroupName]
    , pse.[Title]
    , pse.[StartDate]
    , pse.[EndDate]
    , CASE
        WHEN g.[Id] IS NOT NULL THEN g.[Name]
        ELSE 'All Groups'
      END AS [GroupName]
FROM [PersonScheduleExclusion] pse
INNER JOIN @PersonScheduleExclusionIds psei
    ON psei.[Id] = pse.[Id]-- OR psei.[Id] = pse.[ParentPersonScheduleExclusionId]
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = pse.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
LEFT OUTER JOIN [Group] g
    ON g.[Id] = pse.[GroupId]
ORDER BY pse.[StartDate]
    , pse.[EndDate]
    , g.[Name];
