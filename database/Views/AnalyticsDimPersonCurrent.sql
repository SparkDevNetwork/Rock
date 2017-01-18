IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonCurrent]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonCurrent
GO

CREATE VIEW AnalyticsDimPersonCurrent
AS
SELECT * FROM AnalyticsDimPersonHistorical where CurrentRowIndicator = 1