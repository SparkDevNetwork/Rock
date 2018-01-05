IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialBatch]', 'V') IS NOT NULL
    DROP VIEW [dbo].AnalyticsDimFinancialBatch
GO

CREATE VIEW [dbo].AnalyticsDimFinancialBatch
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
	,1 [Count]
FROM FinancialBatch fb
LEFT JOIN Campus c ON fb.CampusId = c.Id