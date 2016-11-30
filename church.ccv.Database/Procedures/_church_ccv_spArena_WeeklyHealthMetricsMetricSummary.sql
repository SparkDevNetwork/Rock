CREATE PROC [dbo].[_church_ccv_WeeklyHealthMetrics_Metric_Summary]
@ParentMetricID int,
@RunDate datetime

AS

WITH metric_summary
AS
(
	SELECT 
		MC.CategoryId AS parent_metric_id,
		PM.Id,
		NULL AS entity,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(PM.Id, @RunDate) AS metric_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricMedian(PM.Id, @RunDate) AS run_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricMedian(PM.Id, DATEADD(yy, -1, @RunDate)) AS prev_year_run_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricYTD(PM.Id, @RunDate) AS ytd,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricYTD(PM.Id, DATEADD(yy, -1, @RunDate)) AS prev_year_ytd
	FROM Metric PM
	INNER JOIN MetricCategory MC ON MC.MetricId = PM.ID
	INNER JOIN Category C ON C.Id = MC.CategoryId
	WHERE (C.ParentCategoryId = @ParentMetricID OR (@ParentMetricID = -1 AND C.ParentCategoryId IS NULL))
	AND PM.EntityTypeId IS NULL

	UNION

	SELECT 
		MC.CategoryId AS parent_metric_id,
		PM.Id,
		MV.EntityId AS entity,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(PM.Id, MV.EntityId, @RunDate) AS metric_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricMedianByEntity(PM.Id, MV.EntityId, @RunDate) AS run_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricMedianByEntity(PM.Id, MV.EntityId, DATEADD(yy, -1, @RunDate)) AS prev_year_run_value,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricYTDByEntity(PM.Id, MV.EntityId, @RunDate) AS ytd,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricYTDByEntity(PM.Id, MV.EntityId, DATEADD(yy, -1, @RunDate)) AS prev_year_ytd
	FROM Metric PM
	INNER JOIN MetricValue MV ON MV.MetricId = PM.Id
	INNER JOIN MetricCategory MC ON MC.MetricId = PM.ID
	INNER JOIN Category C ON C.Id = MC.CategoryId
	WHERE (C.ParentCategoryId = @ParentMetricID OR (@ParentMetricID = -1 AND C.ParentCategoryId IS NULL))
	AND PM.EntityTypeId = 67
	GROUP BY MC.CategoryId, PM.Id, MV.EntityId

)

SELECT
	S.parent_metric_id,
	C.Name AS parent_title,
	CASE C.Name
		WHEN 'Ministries' THEN 'SUM'
		WHEN 'Attendance' THEN 'AVG'
		WHEN 'Giving' THEN 'AVG'
	END AS [parent_aggregate_type],--PM.aggregate_type AS parent_aggregate_type,
	S.Id,
	CASE
		WHEN S.entity IS NOT NULL THEN CA.Name + ' - ' + M.title
		ELSE M.Title
	END as [title],
	CASE
		WHEN S.entity IS NOT NULL THEN S.entity
		ELSE -1
	END as [entity],
	S.metric_value,
	S.run_value,
	CASE WHEN S.run_value <> 0 THEN (S.metric_value - S.run_value) / CAST(S.run_value AS decimal(9,2)) ELSE 0 END as run_change,
	S.prev_year_run_value,
	CASE WHEN S.prev_year_run_value <> 0 THEN (S.run_value - S.prev_year_run_value) / CAST(S.prev_year_run_value AS decimal(9,2)) ELSE 0 END as run_year_change,
	S.ytd,
	S.prev_year_ytd,
	CASE WHEN S.prev_year_ytd <> 0 THEN (S.ytd - S.prev_year_ytd) / CAST(S.prev_year_ytd AS decimal(9,2)) ELSE 0 END as ytd_change
FROM metric_summary S
INNER JOIN Category C ON C.Id = S.parent_metric_id
INNER JOIN Metric M ON M.Id = S.Id
LEFT OUTER JOIN Campus CA ON CA.Id = S.entity
ORDER BY C.[Order], CA.Name + ' - ' + M.title