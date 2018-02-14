IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyCurrent]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFamilyCurrent
GO

<<<<<<< HEAD

CREATE VIEW AnalyticsDimFamilyCurrent
=======
CREATE VIEW [dbo].[AnalyticsDimFamilyCurrent]
>>>>>>> f65ca482f7a4ce9064ffb63e8b61e970376241fa
AS
SELECT * FROM AnalyticsDimFamilyHistorical where CurrentRowIndicator = 1