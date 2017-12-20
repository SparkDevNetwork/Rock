IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyHeadOfHouseholdBirthDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFamilyHeadOfHouseholdBirthDate
GO

CREATE VIEW [dbo].AnalyticsDimFamilyHeadOfHouseholdBirthDate
AS
SELECT d.DateKey AS [BirthDateKey]
    ,*
FROM AnalyticsSourceDate d
WHERE d.DateKey >= (
        SELECT MIN(x.BirthDateKey)
        FROM AnalyticsDimFamilyHeadOfHousehold x
        )
    AND d.DateKey <= (
        SELECT MAX(x.BirthDateKey)
        FROM AnalyticsDimFamilyHeadOfHousehold x
        )
