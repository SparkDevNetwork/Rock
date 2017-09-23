IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonCurrentBirthDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonCurrentBirthDate
GO

CREATE VIEW [dbo].AnalyticsDimPersonCurrentBirthDate
AS
SELECT d.DateKey AS [BirthDateKey]
    ,*
FROM AnalyticsSourceDate d
WHERE d.DateKey >= (
        SELECT MIN(x.BirthDateKey)
        FROM AnalyticsDimPersonCurrent x
        )
    AND d.DateKey <= (
        SELECT MAX(x.BirthDateKey)
        FROM AnalyticsDimPersonCurrent x
        )
