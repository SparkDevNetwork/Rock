IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonConnectionStatus]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonConnectionStatus
GO

CREATE VIEW AnalyticsDimPersonConnectionStatus
AS
SELECT dv.Id [ConnectionStatusId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '2E6540EA-63F0-40FE-BE50-F2A84735E600' 



