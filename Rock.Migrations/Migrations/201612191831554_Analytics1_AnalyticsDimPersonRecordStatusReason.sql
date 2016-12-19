IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonRecordStatusReason]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonRecordStatusReason
GO

CREATE VIEW AnalyticsDimPersonRecordStatusReason
AS
SELECT dv.Id [RecordStatusReasonId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = 'E17D5988-0372-4792-82CF-9E37C79F7319'
