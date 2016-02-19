ALTER FUNCTION [dbo].[_church_ccv_WeeklyHealthMetrics_GetMetricValueByEntity] (@MetricID int, @EntityID int, @RunDate DateTime)
	RETURNS int

AS

BEGIN

	DECLARE @Value int

	DECLARE @FromDate datetime
	SET @FromDate = (
		SELECT
			CASE 
				WHEN S.iCalendarContent LIKE '%DAILY%' THEN DATEADD(dd, -4, @RunDate)
				WHEN S.iCalendarContent LIKE '%WEEKLY%'  THEN DATEADD(wk, -4, @RunDate)
				WHEN S.iCalendarContent LIKE '%MONTHLY%'  THEN DATEADD(mm, -4, @RunDate)
				WHEN S.iCalendarContent LIKE '%YEARLY%'  THEN DATEADD(yy, -4, @RunDate)
				ELSE DATEADD(mm, -4, @RunDate)
			END
		FROM Metric M
		LEFT OUTER JOIN Schedule S
			ON S.Id = M.ScheduleId
		WHERE M.Id = @MetricID )

	SET @Value = (
		SELECT TOP 1 MV.YValue
		FROM MetricValue MV
		WHERE MV.MetricId = @MetricID
		AND MV.EntityId = @EntityID
		AND MV.MetricValueDateTime <= @RunDate
		AND MV.MetricValueDateTime >= @FromDate
		ORDER BY MV.MetricValueDateTime DESC )

	IF @Value IS NULL
		SET @Value = 0

	RETURN @Value

END