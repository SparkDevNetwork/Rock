SET NOCOUNT ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON

-- =============================================
-- Description: Assigns Group.MeetingStyle across the map-located Small Groups (GroupTypeId 25) so the
--              Group Finder "Meeting Style" filter (Where section) has data. Spread: ~40% In-Person,
--              ~20% Online, ~20% Hybrid, ~20% left unset (so the filter's exclude-missing case is testable).
--              MeetingStyle enum: InPerson = 1, Online = 2, Hybrid = 3. Idempotent (deterministic by row number).
-- Date: 2026-08-05
-- =============================================

DECLARE @GroupTypeId INT = 25

;WITH [geo] AS (
    SELECT g.[Id], ROW_NUMBER() OVER (ORDER BY g.[Id]) AS [rn]
    FROM [Group] g
    WHERE g.[GroupTypeId] = @GroupTypeId AND g.[IsActive] = 1
      AND EXISTS (SELECT 1 FROM [GroupLocation] gl JOIN [Location] l ON l.[Id] = gl.[LocationId] WHERE gl.[GroupId] = g.[Id] AND l.[GeoPoint] IS NOT NULL)
)
UPDATE g
SET g.[MeetingStyle] = m.[Style], g.[ModifiedDateTime] = GETDATE()
FROM [Group] g
JOIN [geo] ON [geo].[Id] = g.[Id]
CROSS APPLY (SELECT CASE [geo].[rn] % 5 WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 2 WHEN 4 THEN 3 ELSE NULL END AS [Style]) m
WHERE m.[Style] IS NOT NULL

-- Verify
SELECT ISNULL(CAST([MeetingStyle] AS NVARCHAR(10)), '(null)') AS [MeetingStyle], COUNT(*) AS [Groups]
FROM [Group] WHERE [GroupTypeId] = @GroupTypeId AND [IsActive] = 1
GROUP BY [MeetingStyle] ORDER BY [MeetingStyle]
