CREATE PROC [dbo].[_church_ccv_WeeklyHealthMetrics_MetricSource]
@ParentMetricID int

AS

WITH CTE AS 
(
	SELECT 
		M.Id,
		MV.EntityId
	FROM Metric M
	INNER JOIN MetricValue MV ON MV.MetricId = M.Id
	INNER JOIN MetricCategory PM ON PM.MetricId = M.Id
	INNER JOIN Category C ON C.Id = PM.CategoryId
	LEFT OUTER JOIN Campus CA ON CA.Id = MV.EntityId
	WHERE (C.ParentCategoryId = @ParentMetricID OR (@ParentMetricID = -1 AND C.ParentCategoryId IS NULL))
	GROUP BY M.Id, MV.EntityId
)

SELECT 
	PM.CategoryId AS parent_metric_id,
	CT.Id AS [metric_id],
	C.[Order] AS parent_metric_order,
	PM.[Order] AS [metric_order],
	C.Name AS parent_title,
	CASE
		WHEN CT.EntityId IS NOT NULL THEN CA.Name + ' - ' + M.title
		ELSE M.Title
	END as [title],
	CASE --Please forgive me for what I have done.
		WHEN M.Title LIKE '%Children%' THEN 'Automatically calculated using Rock attendance'
		WHEN M.Title LIKE '%High School%' THEN 'Manually Entered'
		WHEN M.Title LIKE '%Junior High%' THEN 'Manually Entered'
		WHEN M.Title LIKE '%Adult%' THEN 'Manually Entered'
		WHEN M.Title LIKE '%Total Attendance%' THEN 'Automatically calculated in Rock'
		WHEN M.Title LIKE '%General Fund%' THEN 'Hand Entered'
		WHEN M.Title LIKE '%Kiosk Giving%' THEN ' Automatically calculated using Rock contribution data'
		WHEN M.Title LIKE '%Online Giving (One Time)%' THEN 'Health Metrics Rock job'
		WHEN M.Title LIKE '%Online Giving (Recurring)%' THEN 'Health Metrics Rock job' 
		WHEN M.Title LIKE '%Giving Particpiants%' THEN 'Automatically calculated using Rock contribution data'
	END AS [source_summary],
	M.[description]
FROM CTE CT
INNER JOIN Metric M ON M.Id = CT.Id
INNER JOIN MetricCategory PM ON PM.MetricId = CT.Id
INNER JOIN Category C ON C.Id = PM.CategoryId
LEFT OUTER JOIN Campus CA ON CA.Id = CT.EntityId
WHERE (C.ParentCategoryId = @ParentMetricID OR (@ParentMetricID = -1 AND C.ParentCategoryId IS NULL))
ORDER BY C.Name, CA.Name + ' - ' + M.title