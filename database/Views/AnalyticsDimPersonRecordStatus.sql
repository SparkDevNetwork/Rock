IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonRecordStatus]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonRecordStatus
GO

CREATE VIEW AnalyticsDimPersonRecordStatus
AS
SELECT dv.Id [RecordStatusId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '8522BADD-2871-45A5-81DD-C76DA07E2E7E'
