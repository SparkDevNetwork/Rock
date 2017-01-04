IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyHeadOfHousehold]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFamilyHeadOfHousehold
GO

CREATE VIEW AnalyticsDimFamilyHeadOfHousehold
AS
SELECT * FROM AnalyticsDimPersonCurrent