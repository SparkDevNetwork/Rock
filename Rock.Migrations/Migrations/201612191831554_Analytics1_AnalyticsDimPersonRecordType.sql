IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonRecordType]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonRecordType
GO

CREATE VIEW AnalyticsDimPersonRecordType
AS
SELECT dv.Id [RecordTypeId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275'
