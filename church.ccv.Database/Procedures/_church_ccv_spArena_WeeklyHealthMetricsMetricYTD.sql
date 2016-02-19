CREATE PROC [dbo].[_church_ccv_WeeklyHealthMetrics_MetricYTD]
@ParentMetricID int,
@RunDate datetime

AS

SELECT 
	PM.CategoryId AS parent_metric_id,
	CASE
		WHEN CA.Id IS NOT NULL THEN (CAST(1000 AS VARCHAR(10)) + CAST(M.Id AS VARCHAR(10)) + CAST(CA.Id AS VARCHAR(10)))
		ELSE M.Id 
	END AS [metric_id],
	C.[Order] AS parent_metric_order,
	PM.[Order] as [metric_order],
	C.Name AS parent_title,
	CASE
		WHEN MI.EntityId IS NOT NULL THEN CA.Name + ' - ' + M.title
		ELSE M.Title
	END as [title],
	YEAR(MI.MetricValueDateTime) AS [year],
	CAST(CAST(MONTH(MI.MetricValueDateTime) AS varchar) + '/' +
		CAST(DAY(MI.MetricValueDateTime) AS varchar) + '/2000' AS datetime) AS collection_date,
	MI.YValue AS  [metric_value]
FROM MetricValue MI
INNER JOIN Metric M ON M.Id = MI.MetricId
INNER JOIN MetricCategory PM ON M.Id = PM.MetricId
INNER JOIN Category C ON C.Id = PM.CategoryId
LEFT OUTER JOIN Campus CA ON CA.Id = MI.EntityId
WHERE (C.ParentCategoryId = @ParentMetricID OR (@ParentMetricID = -1 AND C.ParentCategoryId IS NULL))
AND YEAR(MI.MetricValueDateTime) >= YEAR(@RunDate) - 1
ORDER BY C.Name, CA.Name + ' - ' + M.title, MI.MetricValueDateTime