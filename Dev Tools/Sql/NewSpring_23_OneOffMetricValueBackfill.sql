/* ====================================================== 
-- NewSpring Script #23: 
-- Backfill metric values to the beggining of 2015 for one offs

   ====================================================== */

DECLARE @foreignKey AS NVARCHAR(20) = 'Metrics 2.0 - Backfi';
DECLARE @metricValueType AS INT = 0;
DECLARE @CreatedDateTime AS DATETIME = GETDATE();

IF object_id('tempdb..#metricTypes') IS NOT NULL
BEGIN
	DROP TABLE #metricTypes;
END

CREATE TABLE #metricTypes (
	Id INT IDENTITY(1,1),
	CategoryId INT,
	MetricId INT,
	MetricTitle NVARCHAR(100),
	CategoryName NVARCHAR(100),
);

IF object_id('tempdb..#currentResult') IS NOT NULL
BEGIN
	DROP TABLE #currentResult;
END

CREATE TABLE #currentResult (
	Value INT,
	MetricValueDateTime DATETIME,
	Campus INT,
	[Group] INT,
	Schedule INT,
	MetricValueGuid UNIQUEIDENTIFIER DEFAULT NEWID()
);

INSERT INTO #metricTypes
SELECT
	c.Id AS CategoryId,
	m.Id AS MetricId,
	m.Title AS MetricTitle,
	c.Name AS CategoryName
FROM
	Metric m
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN Category c ON c.Id = mc.CategoryId
WHERE
	m.ForeignKey = 'Metrics 2.0 - 1Off'
	AND m.SourceSql IS NOT NULL 
	AND RTRIM(LTRIM(LEN(m.SourceSql))) > 1;

DECLARE @sundayDate AS DATE = '01-04-2015';
DECLARE @scopeIndex AS INT = (SELECT MIN(Id) FROM #metricTypes);
DECLARE @numItems AS INT = (SELECT COUNT(1) + @scopeIndex FROM #metricTypes);
DECLARE @metricId AS INT;
DECLARE @metricName AS NVARCHAR(100);
DECLARE @groupType AS INT;
DECLARE @sourceSql AS NVARCHAR(MAX);
DECLARE @startPattern AS NVARCHAR(10);
DECLARE @stopPattern AS NVARCHAR(10);
DECLARE @start AS INT;
DECLARE @stop AS INT;
DECLARE @length AS INT;
DECLARE @msg AS NVARCHAR(MAX);
DECLARE @categoryName AS NVARCHAR(100);

WHILE @sundayDate < GETDATE()
BEGIN
	SET @scopeIndex = (SELECT MIN(Id) FROM #metricTypes);

	WHILE @scopeIndex < @numItems
	BEGIN
		SELECT @metricId = MetricId, @categoryName = CategoryName FROM #metricTypes WHERE Id = @scopeIndex;
		SET @metricName = (SELECT MetricTitle FROM #metricTypes WHERE Id = @scopeIndex);
		SET @sourceSql = (SELECT SourceSql FROM Metric WHERE Id = @metricId);
		SET @groupType = (SELECT EntityTypeQualifierValue FROM MetricPartition WHERE MetricId = @metricId AND Label = 'Group');

		SET @msg = 'Creating metric values for ' + @categoryName + '/' + @metricName + ' (Id: ' + CONVERT(NVARCHAR(20), @metricId) + ') / ' + CONVERT(NVARCHAR(20), @sundayDate); 
		RAISERROR ( @msg, 0, 0 ) WITH NOWAIT;
		
		SET @sourceSql = REPLACE(@sourcesql, 'GETDATE()', '''' + CONVERT(NVARCHAR(20), DATEADD(DAY, 6, @sundayDate)) + '''');

		TRUNCATE TABLE #currentResult;
		INSERT INTO  #currentResult (Value, MetricValueDateTime, Campus, [Group], Schedule)
		EXEC(@sourceSql);

		INSERT INTO MetricValue(
			MetricValueType,
			YValue,
			MetricId,
			MetricValueDateTime,
			CreatedDateTime,
			[Guid],
			ForeignKey
		)
		SELECT 
			@metricValueType,
			cr.Value,
			@metricId,
			cr.MetricValueDateTime,
			@CreatedDateTime,
			cr.MetricValueGuid,
			@foreignKey
		FROM 
			#currentResult cr;

		-- Group
		INSERT INTO MetricValuePartition(
			MetricPartitionId,
			MetricValueId,
			EntityId,
			CreatedDateTime,
			[Guid],
			ForeignKey
		)
		SELECT 
			(SELECT mp.Id FROM MetricPartition mp WHERE mp.MetricId = @metricId AND mp.Label = 'Group') AS MetricPartitionId,
			(SELECT Id FROM MetricValue WHERE [Guid] = cr.MetricValueGuid) AS MetricValueId,
			cr.[Group] AS EntityId,
			@CreatedDateTime AS CreatedDateTime,
			NEWID() AS [Guid],
			@foreignKey AS ForeignKey
		FROM 
			#currentResult cr
		WHERE
			cr.[Group] IS NOT NULL;

		-- Schedule
		INSERT INTO MetricValuePartition(
			MetricPartitionId,
			MetricValueId,
			EntityId,
			CreatedDateTime,
			[Guid],
			ForeignKey
		)
		SELECT 
			(SELECT mp.Id FROM MetricPartition mp WHERE mp.MetricId = @metricId AND mp.Label = 'Schedule') AS MetricPartitionId,
			(SELECT Id FROM MetricValue WHERE [Guid] = cr.MetricValueGuid) AS MetricValueId,
			cr.Schedule AS EntityId,
			@CreatedDateTime AS CreatedDateTime,
			NEWID() AS [Guid],
			@foreignKey AS ForeignKey
		FROM 
			#currentResult cr
		WHERE
			cr.Schedule IS NOT NULL;

		-- Campus
		INSERT INTO MetricValuePartition(
			MetricPartitionId,
			MetricValueId,
			EntityId,
			CreatedDateTime,
			[Guid],
			ForeignKey
		)
		SELECT 
			(SELECT mp.Id FROM MetricPartition mp WHERE mp.MetricId = @metricId AND mp.Label = 'Campus') AS MetricPartitionId,
			(SELECT Id FROM MetricValue WHERE [Guid] = cr.MetricValueGuid) AS MetricValueId,
			cr.Campus AS EntityId,
			@CreatedDateTime AS CreatedDateTime,
			NEWID() AS [Guid],
			@foreignKey AS ForeignKey
		FROM 
			#currentResult cr
		WHERE
		cr.Campus IS NOT NULL;

		SET @scopeIndex = @scopeIndex + 1;
	END

	SET @sundayDate = DATEADD(WEEK, 1, @sundayDate);
END

/*
DELETE FROM MetricValuePartition WHERE MetricValueId IN (SELECT Id FROM MetricValue WHERE ForeignKey = 'Metrics 2.0 - Backfi');
DELETE FROM MetricValue WHERE ForeignKey = 'Metrics 2.0 - Backfi';
*/