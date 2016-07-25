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
	NEWID() AS MetricGuid
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
-- insert the new metric
/* ====================================================== */
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
	mt.MetricGuid AS [Guid],
	'Metrics 2.0' AS ForeignKey,
	''
FROM
	#metricTypes mt;

/* ====================================================== */
-- add the new metric to a category
/* ====================================================== */
INSERT INTO MetricCategory (
	MetricId,
	CategoryId,
	[Order],
	[Guid],
	ForeignKey
)
SELECT
	(SELECT Id FROM Metric WHERE [Guid] = mt.MetricGuid) AS MetricId,
	mt.CategoryId,
	@Order AS [Order],
	NEWID() AS [Guid],
	'Metrics 2.0' AS ForeignKey
FROM
	#metricTypes mt;

--DELETE FROM Metric WHERE ForeignKey = 'Metrics 2.0'

--SELECT * FROM #groupConversion;
--SELECT * FROM #metricTypes

/*
SELECT 
	* 
FROM 
	MetricCategory 
WHERE 
	CategoryId = 451;

-- Add partitions
-- INSERT INTO MetricPartition
SELECT 
	Id AS MetricId,
	'Did Attend' AS Label,
	31 AS EntityTypeId,
	0 AS IsRequired,
	3 AS [Order],
	'DefinedTypeId' AS EntityTypeQualifierColumn,
	'72' AS EntityTypeQualifierValue,
	NEWID() AS [Guid]
FROM Metric WHERE Title LIKE '% Service Attendance';


WITH cteNewGroups AS (
	
)
SELECT 
	m.Title AS MetricTitle,
	g.Name AS GroupName,
	ng.GroupName,
	ng.CampusId,
	et.FriendlyName,
	mv.YValue,
	mv.MetricValueDateTime,
	CASE 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '09' THEN @service0915
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '11' THEN @service1115 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '16' THEN @service1600 
		WHEN SUBSTRING(CONVERT(NVARCHAR(30), CONVERT(TIME, mv.MetricValueDateTime)), 0, 3) = '18' THEN @service1800
	END AS ScheduleId,
	1 AS DidAttend,
	c.Name
FROM 
	Metric m
	JOIN [Group] g ON g.Id = m.ForeignId
	JOIN MetricPartition mp ON mp.MetricId = m.Id
	JOIN EntityType et ON et.Id = mp.EntityTypeId
	JOIN cteNewGroups ng ON ng.OldGroupId = g.Id
	JOIN MetricValue mv ON mv.MetricId = m.Id
	JOIN MetricValuePartition mvp ON mvp.MetricPartitionId = mp.Id AND mvp.MetricValueId = mv.Id AND ng.CampusId = mvp.EntityId
	JOIN Campus c ON c.Id = ng.CampusId
WHERE 
	m.Id = @metricId
	AND mv.Id = 143917
ORDER BY 
	mv.MetricValueDateTime DESC,
	ng.CampusId;*/