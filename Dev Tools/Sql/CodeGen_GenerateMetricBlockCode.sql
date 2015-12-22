DECLARE @pageName AS NVARCHAR(100) = 'Fuse Dashboard';
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
cte4 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Comparison Metric Key'
),
cte5 AS (
	SELECT
		av.Value,
		av.EntityId
	FROM 
		AttributeValue av
		JOIN Attribute a ON a.Id = av.AttributeId AND a.EntityTypeQualifierColumn = 'BlockTypeId' AND a.EntityTypeId = @blockEntityTypeId AND a.Name = 'Display Comparison As'
)
SELECT 
	CONCAT('EXEC insertMetricBlock @pageId, ', b.[Order], ', ''', b.Name, ''', ''', av1.Value, ''', ''', av2.Value, ''', ''', av3.Value, ''', ''', av4.Value, ''', ''', av5.Value, ''';') AS [Sql]
FROM
	[Page] p
	JOIN Block b ON b.PageId = p.Id
	LEFT JOIN cte1 av1 ON av1.EntityId = b.Id
	LEFT JOIN cte2 av2 ON av2.EntityId = b.Id
	LEFT JOIN cte3 av3 ON av3.EntityId = b.Id
	LEFT JOIN cte4 av4 ON av4.EntityId = b.Id
	LEFT JOIN cte5 av5 ON av5.EntityId = b.Id
WHERE
	p.InternalName = @pageName
	AND b.Zone = 'SectionB'
ORDER BY
	b.[Order]