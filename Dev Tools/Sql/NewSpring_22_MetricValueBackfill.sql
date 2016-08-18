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
	AND (m.SourceSql IS NOT NULL OR RTRIM(LTRIM(LEN(m.SourceSql))) > 1);

DECLARE @sundayDate AS DATE = '01-04-2015';
DECLARE @scopeIndex AS INT = (SELECT MIN(Id) FROM #metricTypes);
DECLARE @numItems AS INT = (SELECT COUNT(1) + @scopeIndex FROM #metricTypes);
DECLARE @metricId AS INT;
DECLARE @groupType AS INT;
DECLARE @sourceSql AS NVARCHAR(MAX);

WHILE @sundayDate < GETDATE()
BEGIN
	SET @scopeIndex = (SELECT MIN(Id) FROM #metricTypes);

	WHILE @scopeIndex < @numItems
	BEGIN
		SET @metricId = (SELECT MetricId FROM #metricTypes WHERE Id = @scopeIndex);
		SET @sourceSql = (SELECT SourceSql FROM Metric WHERE Id = @metricId);
		SET @groupType = (SELECT EntityTypeQualifierValue FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group');
		
		/* Replace groupType and dateRange in sourceSql */

		/* Exec sourceSql */

		/* Store result in metricValues */

		SET @scopeIndex = @scopeIndex + 1;
	END

	SET @sundayDate = DATEADD(WEEK, 1, @sundayDate);
END