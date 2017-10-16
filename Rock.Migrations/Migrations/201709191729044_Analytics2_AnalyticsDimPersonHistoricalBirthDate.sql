IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonHistoricalBirthDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonHistoricalBirthDate
GO

CREATE VIEW [dbo].AnalyticsDimPersonHistoricalBirthDate
AS
SELECT d.DateKey AS [BirthDateKey]
    ,*
FROM AnalyticsSourceDate d
WHERE d.DateKey >= (
        SELECT MIN(x.BirthDateKey)
        FROM AnalyticsDimPersonHistorical x
        )
    AND d.DateKey <= (
        SELECT MAX(x.BirthDateKey)
        FROM AnalyticsDimPersonHistorical x
        )
