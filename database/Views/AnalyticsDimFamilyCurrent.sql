IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyCurrent]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFamilyCurrent
GO

CREATE VIEW AnalyticsDimFamilyCurrent
AS
SELECT * FROM AnalyticsDimFamilyHistorical where CurrentRowIndicator = 1