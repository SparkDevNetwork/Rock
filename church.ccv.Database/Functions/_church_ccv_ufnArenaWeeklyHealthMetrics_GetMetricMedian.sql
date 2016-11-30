ALTER FUNCTION [dbo].[_church_ccv_WeeklyHealthMetrics_GetMetricMedian] 
(@MetricID int, @RunDate DateTime)
	RETURNS int

AS

BEGIN

	DECLARE @RunMedian int

	DECLARE @FromDate datetime
	DECLARE @ToDate datetime
	DECLARE @CollectionFrequency int

	SELECT
		@FromDate = (SELECT
						CASE
							WHEN S.iCalendarContent LIKE '%DAILY%' THEN DATEADD(dd, -4, @RunDate)
							WHEN S.iCalendarContent LIKE '%WEEKLY%' THEN DATEADD(wk, -4, @RunDate)
							WHEN S.iCalendarContent LIKE '%MONTHLY%' THEN DATEADD(mm, -4, @RunDate)
							WHEN @MetricID IN (16,17,23) THEN DATEADD(mm, -4, @RunDate) --monthly metrics
							ELSE DATEADD(wk, -4, @RunDate)
						END),
		@CollectionFrequency = (SELECT
									CASE
										WHEN S.iCalendarContent LIKE '%DAILY%' THEN 1
										WHEN S.iCalendarContent LIKE '%WEEKLY%' THEN 2
										WHEN S.iCalendarContent LIKE '%MONTHLY%' THEN 3
										ELSE 2
									END)
	FROM Metric M
	LEFT OUTER JOIN Schedule S 
		ON S.Id = M.ScheduleId
	WHERE M.ID = @MetricID 

	IF @CollectionFrequency = 1
		SET @RunDate = DATEADD(dd, -1, @RunDate)
	IF @CollectionFrequency = 2
		SET @RunDate = DATEADD(wk, -1, @RunDate)
	IF @CollectionFrequency = 3
		SET @RunDate = DATEADD(mm, -1, @RunDate)
	IF @CollectionFrequency = 4
		SET @RunDate = DATEADD(yy, -1, @RunDate)

	SET @ToDate = ISNULL((
		SELECT MAX(MV.MetricValueDateTime)
		FROM MetricValue MV
		WHERE MV.MetricValueDateTime > @FromDate
		AND MV.MetricValueDateTime <= @RunDate
		AND MV.MetricId = @MetricID ),@RunDate);

	SET @FromDate = DATEADD(dd, DATEDIFF(dd, @RunDate, @ToDate), @FromDate);

	WITH metricValues
	AS
	(
		SELECT MV.YValue
		FROM MetricValue MV
		WHERE MV.MetricValueDateTime > @FromDate
		AND MV.MetricValueDateTime <= @RunDate
		AND MV.MetricId = @MetricID
	),
	positions
	AS
	(
		SELECT 
			(1 + COUNT(*))/2 AS mid,
			1 - (COUNT(*) % 2) AS even
		FROM metricValues
	),
	rows
	AS
	(
		SELECT 
			YValue,
			ROW_NUMBER() over (ORDER BY YValue) as m
		FROM metricValues
	)
	SELECT @RunMedian = (
		SELECT AVG(YValue) 
		FROM [rows]
		JOIN positions ON m IN (positions.mid, positions.mid + positions.even))

	IF @RunMedian IS NULL
		SET @RunMedian = 0

	RETURN @RunMedian

END