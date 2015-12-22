DECLARE @pageName AS NVARCHAR(100) = 'Guest Services Dashboard';
DECLARE @blockEntityTypeId AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Block');

WITH cte1 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Number of Columns'
),
cte2 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Metric Display Type'
),
cte3 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Primary Metric Key'
),
cte3s AS (
	SELECT
		av.Value,
		av.EntityId,
		PARSENAME(REPLACE(av.Value, '|', '.'), 2) AS MetricPart,
		PARSENAME(REPLACE(av.Value, '|', '.'), 1) AS CategoryPart
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Primary Metric Source'
),
cte4 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Comparison Metric Key'
),
cte4s AS (
	SELECT
		av.Value,
		av.EntityId,
		PARSENAME(REPLACE(av.Value, '|', '.'), 2) AS MetricPart,
		PARSENAME(REPLACE(av.Value, '|', '.'), 1) AS CategoryPart
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Comparison Metric Source'
),
cte5 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Display Comparison As'
),
cte3names AS (
	SELECT 
		m.Title as MetricTitle,
		c.Name as CategoryName,
		s.EntityId
	FROM
		Metric m
		JOIN MetricCategory mc ON m.Id = mc.MetricId
		JOIN Category c ON mc.CategoryId = c.Id
		JOIN cte3s s ON s.CategoryPart = c.[Guid] AND s.MetricPart = m.[Guid]
),
cte4names AS (
	SELECT 
		m.Title as MetricTitle,
		c.Name as CategoryName,
		s.EntityId
	FROM
		Metric m
		JOIN MetricCategory mc ON m.Id = mc.MetricId
		JOIN Category c ON mc.CategoryId = c.Id
		JOIN cte4s s ON s.CategoryPart = c.[Guid] AND s.MetricPart = m.[Guid]
)
SELECT 
	CONCAT('EXEC insertMetricBlock @pageId, ', b.[Order], ', ''', b.Name, ''', ''', av1.Value, ''', ''', av2.Value, ''', ''', av3.Value, ''', ''', av3s.MetricTitle, ''', ''', av3s.CategoryName, ''', ''', av4.Value, ''', ''', av4s.MetricTitle, ''', ''', av4s.CategoryName, ''', ''', av5.Value, ''';') AS [Sql]
FROM
	[Page] p
	JOIN Block b ON b.PageId = p.Id
	LEFT JOIN cte1 av1 ON av1.EntityId = b.Id
	LEFT JOIN cte2 av2 ON av2.EntityId = b.Id
	LEFT JOIN cte3 av3 ON av3.EntityId = b.Id
	LEFT JOIN cte3names av3s ON av3s.EntityId = b.Id
	LEFT JOIN cte4 av4 ON av4.EntityId = b.Id
	LEFT JOIN cte4names av4s ON av4s.EntityId = b.Id
	LEFT JOIN cte5 av5 ON av5.EntityId = b.Id
WHERE
	p.InternalName = @pageName
	AND b.Zone = 'SectionB'
ORDER BY
	b.[Order];
