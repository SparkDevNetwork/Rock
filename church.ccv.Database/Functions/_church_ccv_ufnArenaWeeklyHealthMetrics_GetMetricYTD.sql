ALTER FUNCTION [dbo].[_church_ccv_GetMetricYTD] 
(@MetricID int, @RunDate DateTime)
	RETURNS int

AS

BEGIN

	DECLARE @YTD int

	DECLARE @FromDate datetime
	SET @FromDate = CAST('01/01/' + CAST(YEAR(@RunDate) AS varchar(4)) AS datetime)

	SET @YTD = ISNULL((
		SELECT
			CASE M.IsCumulative
				WHEN 0 THEN AVG(MV.YValue)
				WHEN 1 THEN SUM(MV.YValue)
				ELSE AVG(MV.YValue)
			END
		FROM MetricValue MV
		INNER JOIN Metric M ON M.Id = MV.MetricId
		WHERE MV.MetricValueDateTime > @FromDate
		AND MV.MetricValueDateTime <= @RunDate
		AND MV.MetricId = @MetricID
		GROUP BY M.Id, M.IsCumulative), 0)

	RETURN @YTD

END