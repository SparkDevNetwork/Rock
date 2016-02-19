CREATE PROC [dbo].[_church_ccv_WeeklyHealthMetrics_Metric_Summary_Chart]
@MetricID int,
@EntityID int,
@RunDate datetime

AS

IF (@EntityID != -1)
BEGIN
	SELECT 
		Id,
		DATEADD(wk, -9, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -9, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -8, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -8, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -7, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -7, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -6, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -6, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -5, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -5, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -4, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -4, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -3, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -3, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -2, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -2, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -1, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, DATEADD(wk, -1, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		@RunDate AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValueByEntity(Id, @EntityID, @RunDate) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID
END


ELSE
BEGIN
	SELECT 
		Id,
		DATEADD(wk, -9, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -9, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -8, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -8, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -7, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -7, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -6, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -6, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -5, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -5, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -4, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -4, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -3, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -3, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -2, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -2, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		DATEADD(wk, -1, @RunDate) AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, DATEADD(wk, -1, @RunDate)) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID

	UNION

	SELECT 
		Id,
		@RunDate AS collection_date,
		dbo._church_ccv_ufnArenaWeeklyHealthMetrics_GetMetricValue(Id, @RunDate) AS metric_value
	FROM Metric 
	WHERE Id = @MetricID
END