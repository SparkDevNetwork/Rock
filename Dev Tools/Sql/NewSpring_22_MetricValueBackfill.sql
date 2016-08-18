/* ====================================================== 
-- NewSpring Script #22: 
-- Backfill metric values to the beggining of 2015

   ====================================================== */

IF object_id('tempdb..#metricTypes') IS NOT NULL
BEGIN
	DROP TABLE #metricTypes;
END

CREATE TABLE #metricTypes (
	Id INT IDENTITY(1,1),
	CategoryId INT,
	MetricId INT
);

INSERT INTO #metricTypes
SELECT
	c.Id AS CategoryId,
	m.Id AS MetricId
FROM
	Metric m
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN Category c ON c.Id = mc.CategoryId
WHERE
	m.ForeignKey = 'Metrics 2.0'
	AND m.SourceSql IS NOT NULL 
	AND RTRIM(LTRIM(LEN(m.SourceSql))) > 1;

DECLARE @sundayDate AS DATE = '01-04-2015';
DECLARE @scopeIndex AS INT = (SELECT MIN(Id) FROM #metricTypes);
DECLARE @numItems AS INT = (SELECT COUNT(1) + @scopeIndex FROM #metricTypes);
DECLARE @metricId AS INT;
DECLARE @groupType AS INT;
DECLARE @sourceSql AS NVARCHAR(MAX);
DECLARE @startPattern AS NVARCHAR(10);
DECLARE @stopPattern AS NVARCHAR(10);
DECLARE @start AS INT;
DECLARE @stop AS INT;
DECLARE @length AS INT;
DECLARE @msg AS NVARCHAR(MAX);

WHILE @sundayDate < GETDATE()
BEGIN
	SET @scopeIndex = (SELECT MIN(Id) FROM #metricTypes);

	WHILE @scopeIndex < @numItems
	BEGIN
		SET @metricId = (SELECT MetricId FROM #metricTypes WHERE Id = @scopeIndex);
		SET @sourceSql = (SELECT SourceSql FROM Metric WHERE Id = @metricId);
		SET @groupType = (SELECT EntityTypeQualifierValue FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group');

		SET @msg = 'Creating metric values for metric id ' + CONVERT(NVARCHAR(20), @metricId) + ' / ' + CONVERT(NVARCHAR(20), @sundayDate); 
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT;
		
		-- Replace grouptype lava
		SET @startPattern = '%{{ %';
		SET @stopPattern = '% }}%';
		SET @start = PATINDEX(@startPattern, @sourceSql);
		SET @stop = PATINDEX(@stopPattern, @sourceSql) + LEN(@stopPattern) - 1;
		SET @length = @stop - @start;
		SET @sourceSql = STUFF(@sourceSql, @start, @length, @groupType);

		-- Replace recentSundayDate
		SET @sourceSql = REPLACE(@sourceSql, 'DECLARE @RecentSundayDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(DW, @Today), @Today));', '');
		SET @sourceSql = REPLACE(@sourceSql, 'StartDateTime >= @RecentSundayDate', CONCAT('StartDateTime >= ''', @sundayDate, ''''));
		SET @sourceSql = REPLACE(@sourceSql, 'AND StartDateTime < DATEADD(day, 1, @Today)', CONCAT('AND StartDateTime < DATEADD(WEEK, 1, ''', @sundayDate, ''')'));
		
		EXEC(@sourceSql);
		RETURN;

		/* Exec sourceSql */

		/* Store result in metricValues */

		SET @scopeIndex = @scopeIndex + 1;
	END

	SET @sundayDate = DATEADD(WEEK, 1, @sundayDate);
END