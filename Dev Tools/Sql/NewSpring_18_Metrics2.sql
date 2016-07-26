/* ====================================================== 
-- NewSpring Script #777: 
-- Creates the metric 2.0 structure 
  
--  Assumptions:
--  Existing metrics structure exists according to script 7:

   ====================================================== */
-- Make sure you're using the right Rock database:

--USE Rock

-- Set common variables 
DECLARE @IsSystem bit = 0
DECLARE @True bit = 1
DECLARE @False bit = 0
DECLARE @Order int = 0
DECLARE @MetricSourceSQLId int = (SELECT [Id] FROM DefinedValue WHERE [Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764');
DECLARE @CreatedDateTime AS DATETIME = GETDATE();
DECLARE @foreignKey AS NVARCHAR(15) = 'Metrics 2.0';
DECLARE @metricValueType AS INT = 0;
DECLARE @dvTrue AS INT = 826;
DECLARE @dvFalse AS INT = 827;

-- Schedule ids variables
DECLARE @service0915 AS INT = 12;
DECLARE @service1115 AS INT = 13;
DECLARE @service1600 AS INT = 14;
DECLARE @service1800 AS INT = 15;

-- Structure ids
DECLARE @etidMetricCategory AS INT = 189;
DECLARE @volunteerMetricCategoryId AS INT = (SELECT Id FROM Category WHERE EntityTypeId = @etidMetricCategory AND Name = 'Volunteers');

-- Debug variables
DECLARE @metricId AS INT = 912;

/* ====================================================== */
-- create the group conversion table
/* ====================================================== */
IF object_id('tempdb..#groupConversion') IS NOT NULL
BEGIN
	drop table #groupConversion
END

select ogt.Name OldGroupType, og.id OldGroupId, og.name OldGroup, ogl.Id OldLocationId, ogl.name OldLocation, gt.Name GroupTypeName, ng.Id GroupId, ng.Name GroupName, ng.CampusId
into #groupConversion
from [group] og
	inner join grouptype ogt
	on og.grouptypeid = ogt.id
	and ogt.name not like 'NEW %'    
	inner join grouplocation gl
	on og.id = gl.groupid
	and og.isactive = 1    
	inner join location ogl
	on gl.LocationId = ogl.id
	and ogl.name is not null    
	inner join [group] ng
	on ng.name = og.name
	inner join grouptype gt
	on ng.grouptypeid = gt.id
	and gt.name like 'NEW %'
	inner join grouplocation ngl
	on ng.id = ngl.groupid
	and ogl.id = ngl.LocationId;

/* ====================================================== */
-- create the initial metric structure
/* ====================================================== */
IF object_id('tempdb..#metricTypes') IS NOT NULL
BEGIN
	drop table #metricTypes
END

SELECT 
	m.ForeignId AS GroupId,
	g.Name AS GroupName,
	mc.CategoryId,
	NEWID() AS AttendanceMetricGuid,
	NEWID() AS UniqueMetricGuid
INTO #metricTypes
FROM 
	Metric m
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN [Group] g ON g.Id = m.ForeignId
	JOIN Category c ON c.Id = mc.CategoryId
	JOIN Category pc ON pc.Id = c.ParentCategoryId
WHERE 
	Title LIKE '% Service Attendance'
	AND pc.ParentCategoryId = @volunteerMetricCategoryId;

/* ====================================================== */
-- insert the new metrics
/* ====================================================== */
-- Attendance
INSERT INTO Metric (
	IsSystem,
	Title,
	[Description],
	IsCumulative,
	SourceValueTypeId,
	SourceSql,
	CreatedDateTime,
	[Guid],
	ForeignKey,
	IconCssClass
)
SELECT
	@IsSystem AS IsSystem,
	CONCAT(mt.GroupName, ' Attendance') AS [Title],
	'Metric to track attendance' AS [Description],
	0 AS IsCumulative,
	@MetricSourceSQLId AS SourceValueTypeId,
	'' AS SourceSql,
	@CreatedDateTime AS CreatedDateTime,
	mt.AttendanceMetricGuid AS [Guid],
	@foreignKey AS ForeignKey,
	'' AS IconCssClass
FROM
	#metricTypes mt;

-- Unique
INSERT INTO Metric (
	IsSystem,
	Title,
	[Description],
	IsCumulative,
	SourceValueTypeId,
	SourceSql,
	CreatedDateTime,
	[Guid],
	ForeignKey,
	IconCssClass
)
SELECT
	@IsSystem AS IsSystem,
	CONCAT(mt.GroupName, ' Unique Volunteer') AS [Title],
	'Metric to track unique volunteers' AS [Description],
	0 AS IsCumulative,
	@MetricSourceSQLId AS SourceValueTypeId,
	'' AS SourceSql,
	@CreatedDateTime AS CreatedDateTime,
	mt.UniqueMetricGuid AS [Guid],
	@foreignKey AS ForeignKey,
	'' AS IconCssClass
FROM
	#metricTypes mt;

/* ====================================================== */
-- add the new metrics to a category
/* ====================================================== */
-- Attendance
INSERT INTO MetricCategory (
	MetricId,
	CategoryId,
	[Order],
	[Guid],
	ForeignKey
)
SELECT
	(SELECT Id FROM Metric WHERE [Guid] = mt.AttendanceMetricGuid) AS MetricId,
	mt.CategoryId,
	@Order AS [Order],
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM
	#metricTypes mt;

-- Unique
INSERT INTO MetricCategory (
	MetricId,
	CategoryId,
	[Order],
	[Guid],
	ForeignKey
)
SELECT
	(SELECT Id FROM Metric WHERE [Guid] = mt.UniqueMetricGuid) AS MetricId,
	mt.CategoryId,
	@Order AS [Order],
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM
	#metricTypes mt;

/* ====================================================== */
-- add the new metric partitions for attendance
/* ====================================================== */
-- Campus
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.AttendanceMetricGuid) AS MetricId,
	'Campus' AS Label,
	67 AS EntityTypeId,
	@True AS IsRequired,
	0 AS [Order],
	'' AS EntityTypeQualifierColumn,
	'' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

-- Group
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.AttendanceMetricGuid) AS MetricId,
	'Group' AS Label,
	16 AS EntityTypeId,
	@False AS IsRequired,
	1 AS [Order],
	'' AS EntityTypeQualifierColumn,
	'' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

-- Schedule
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.AttendanceMetricGuid) AS MetricId,
	'Schedule' AS Label,
	54 AS EntityTypeId,
	@False AS IsRequired,
	2 AS [Order],
	'' AS EntityTypeQualifierColumn,
	'' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

-- Did attend
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.AttendanceMetricGuid) AS MetricId,
	'Did Attend' AS Label,
	31 AS EntityTypeId,
	@False AS IsRequired,
	3 AS [Order],
	'DefinedTypeId' AS EntityTypeQualifierColumn,
	'72' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

/* ====================================================== */
-- add the new metric partitions for unique
/* ====================================================== */
-- Campus
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.UniqueMetricGuid) AS MetricId,
	'Campus' AS Label,
	67 AS EntityTypeId,
	@True AS IsRequired,
	0 AS [Order],
	'' AS EntityTypeQualifierColumn,
	'' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

-- Group
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.UniqueMetricGuid) AS MetricId,
	'Group' AS Label,
	16 AS EntityTypeId,
	@False AS IsRequired,
	1 AS [Order],
	'' AS EntityTypeQualifierColumn,
	'' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

-- Did attend
INSERT INTO MetricPartition(
	MetricId,
	Label,
	EntityTypeId,
	IsRequired,
	[Order],
	EntityTypeQualifierColumn,
	EntityTypeQualifierValue,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT Id FROM Metric WHERE [Guid] = mt.UniqueMetricGuid) AS MetricId,
	'Did Attend' AS Label,
	31 AS EntityTypeId,
	@False AS IsRequired,
	2 AS [Order],
	'DefinedTypeId' AS EntityTypeQualifierColumn,
	'72' AS EntityTypeQualifierValue,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#metricTypes mt;

/* ====================================================== */
-- convert the metric values for service attendance to attendance
/* ====================================================== */
IF object_id('tempdb..#attendanceMetricValues') IS NOT NULL
BEGIN
	drop table #attendanceMetricValues
END

SELECT 
	m.Id AS MetricId,
	mv.YValue,
	CONVERT(DATE, mv.MetricValueDateTime) AS MetricDate,
	CASE 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '09' THEN @service0915
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '11' THEN @service1115 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '16' THEN @service1600 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '18' THEN @service1800
	END AS ScheduleId,
	1 AS DidAttend,
	c.Id AS CampusId,
	ng.GroupId AS NewGroupId,
	NEWID() AS MetricValueGuid,
	mt.AttendanceMetricGuid
INTO #attendanceMetricValues
FROM 
	Metric m
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN #metricTypes mt ON mt.CategoryId = mc.CategoryId
	JOIN [Group] g ON g.Id = m.ForeignId
	JOIN MetricPartition mp ON mp.MetricId = m.Id
	JOIN EntityType et ON et.Id = mp.EntityTypeId
	JOIN #groupConversion ng ON ng.OldGroupId = g.Id
	JOIN MetricValue mv ON mv.MetricId = m.Id
	JOIN MetricValuePartition mvp ON mvp.MetricPartitionId = mp.Id AND mvp.MetricValueId = mv.Id AND ng.CampusId = mvp.EntityId
	JOIN Campus c ON c.Id = ng.CampusId
WHERE 
	m.Title LIKE '% Service Attendance';

-- Add metric values
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
	amv.YValue,
	(SELECT Id FROM Metric WHERE [Guid] = amv.AttendanceMetricGuid) AS MetricId,
	amv.MetricDate,
	@CreatedDateTime,
	amv.MetricValueGuid,
	@foreignKey
FROM 
	#attendanceMetricValues amv;

/* ====================================================== */
-- add partition values
/* ====================================================== */
-- Add partition values for campus
INSERT INTO MetricValuePartition(
	MetricPartitionId,
	MetricValueId,
	EntityId,
	CreatedDateTime,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT mp.Id FROM MetricPartition mp JOIN Metric m ON m.Id = mp.MetricId WHERE m.[Guid] = amv.AttendanceMetricGuid AND mp.Label = 'Campus') AS MetricPartitionId,
	(SELECT Id FROM MetricValue WHERE [Guid] = amv.MetricValueGuid) AS MetricValueId,
	amv.CampusId AS EntityId,
	@CreatedDateTime AS CreatedDateTime,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#attendanceMetricValues amv;

-- Add partition values for group
INSERT INTO MetricValuePartition(
	MetricPartitionId,
	MetricValueId,
	EntityId,
	CreatedDateTime,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT mp.Id FROM MetricPartition mp JOIN Metric m ON m.Id = mp.MetricId WHERE m.[Guid] = amv.AttendanceMetricGuid AND mp.Label = 'Group') AS MetricPartitionId,
	(SELECT Id FROM MetricValue WHERE [Guid] = amv.MetricValueGuid) AS MetricValueId,
	amv.NewGroupId AS EntityId,
	@CreatedDateTime AS CreatedDateTime,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#attendanceMetricValues amv;

-- Add partition values for service
INSERT INTO MetricValuePartition(
	MetricPartitionId,
	MetricValueId,
	EntityId,
	CreatedDateTime,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT mp.Id FROM MetricPartition mp JOIN Metric m ON m.Id = mp.MetricId WHERE m.[Guid] = amv.AttendanceMetricGuid AND mp.Label = 'Schedule') AS MetricPartitionId,
	(SELECT Id FROM MetricValue WHERE [Guid] = amv.MetricValueGuid) AS MetricValueId,
	amv.ScheduleId AS EntityId,
	@CreatedDateTime AS CreatedDateTime,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#attendanceMetricValues amv;

-- Add partition values for did attend
INSERT INTO MetricValuePartition(
	MetricPartitionId,
	MetricValueId,
	EntityId,
	CreatedDateTime,
	[Guid],
	ForeignKey
)
SELECT 
	(SELECT mp.Id FROM MetricPartition mp JOIN Metric m ON m.Id = mp.MetricId WHERE m.[Guid] = amv.AttendanceMetricGuid AND mp.Label = 'Did Attend') AS MetricPartitionId,
	(SELECT Id FROM MetricValue WHERE [Guid] = amv.MetricValueGuid) AS MetricValueId,
	@dvTrue AS EntityId,
	@CreatedDateTime AS CreatedDateTime,
	NEWID() AS [Guid],
	@foreignKey AS ForeignKey
FROM 
	#attendanceMetricValues amv;

--DELETE FROM Metric WHERE ForeignKey = 'Metrics 2.0'