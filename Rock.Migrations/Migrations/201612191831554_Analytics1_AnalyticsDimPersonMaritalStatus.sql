IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonMaritalStatus]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonMaritalStatus
GO

CREATE VIEW AnalyticsDimPersonMaritalStatus
AS
SELECT dv.Id [MaritalStatusId]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order]
FROM DefinedValue dv
JOIN DefinedType dt ON dv.DefinedTypeId = dt.Id
WHERE dt.[Guid] = 'B4B92C3F-A935-40E1-A00B-BA484EAD613B'
