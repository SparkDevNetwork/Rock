IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonSuffix]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonSuffix
GO

CREATE VIEW AnalyticsDimPersonSuffix
AS
SELECT dv.Id [SuffixId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '16F85B3C-B3E8-434C-9094-F3D41F87A740'
