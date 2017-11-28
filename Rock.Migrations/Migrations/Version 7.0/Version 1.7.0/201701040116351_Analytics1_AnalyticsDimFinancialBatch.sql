IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialBatch]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialBatch
GO

CREATE VIEW AnalyticsDimFinancialBatch
AS
SELECT fb.Id [BatchId]
    ,fb.[Name]
    ,fb.[BatchStartDateTime]
    ,fb.[BatchEndDateTime]
    ,CASE fb.[Status]
        WHEN 0
            THEN 'Pending'
        WHEN 1
            THEN 'Open'
        WHEN 2
            THEN 'Closed'
        ELSE NULL
        END [Status]
    ,c.NAME [Campus]
    ,fb.[AccountingSystemCode]
    ,fb.[ControlAmount]
FROM FinancialBatch fb
LEFT JOIN Campus c ON fb.CampusId = c.Id
