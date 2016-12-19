IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonTitle]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonTitle
GO

CREATE VIEW AnalyticsDimPersonTitle
AS
SELECT dv.Id [TitleId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '4784CD23-518B-43EE-9B97-225BF6E07846'
