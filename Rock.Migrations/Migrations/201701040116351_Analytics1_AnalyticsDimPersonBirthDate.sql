IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonBirthDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonBirthDate
GO

CREATE VIEW [dbo].AnalyticsDimPersonBirthDate
AS
SELECT d.DateKey AS [BirthDateKey]
    ,*
FROM AnalyticsDimDate d
WHERE d.DateKey >= (
        SELECT MIN(ph.BirthDateKey)
        FROM AnalyticsDimPersonHistorical ph
        )
    AND d.DateKey <= (
        SELECT MAX(ph.BirthDateKey)
        FROM AnalyticsDimPersonHistorical ph
        )
