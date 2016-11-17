IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonReviewReason]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonReviewReason
GO

CREATE VIEW AnalyticsDimPersonReviewReason
AS
SELECT dv.Id [ReviewReasonId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = '7680C445-AD69-4E5D-94F0-CBAA96DB0FF8'
