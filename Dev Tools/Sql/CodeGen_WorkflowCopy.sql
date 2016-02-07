DECLARE @etid_workflowType AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.WorkflowType');

-- STEP 1: Generate code to insert categories
SELECT 
	CONCAT(
		'INSERT [Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Guid], [Order]) VALUES (',
		c.IsSystem,
		', ',
		CASE WHEN pc.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Category WHERE [Guid] = ''',
				pc.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		'@etid_workflowType',
		', ''',
		REPLACE(c.Name, '''', ''''''),
		''', ''',
		c.IconCssClass,
		''', ''',
		c.[Guid],
		''', ',
		c.[Order],
		');'
	) AS Step1
FROM 
	Category c
	LEFT JOIN Category pc ON c.ParentCategoryId = pc.Id
WHERE 
	c.EntityTypeId = @etid_workflowType;

-- STEP 2: Generate code to copy Workflow Types
SELECT 
	CONCAT(
		'INSERT [WorkflowType] ([IsSystem], [IsActive], [Name], [Description], [CategoryId], [Workterm], [ProcessingIntervalSeconds], [IsPersisted], [LoggingLevel], [IconCssClass], [Guid], [Order]) VALUES (',
		wt.IsSystem,
		', ',
		wt.IsActive,
		', ''',
		REPLACE(wt.Name, '''', ''''''),
		''', ''',
		REPLACE(wt.[Description], '''', ''''''),
		''', ',
		CASE WHEN c.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Category WHERE [Guid] = ''',
				c.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		'''',
		wt.WorkTerm,
		''', ',
		ISNULL(CAST(wt.ProcessingIntervalSeconds AS NVARCHAR), 'NULL'),
		', ',
		wt.IsPersisted,
		', ',
		wt.LoggingLevel,
		', ''',
		wt.IconCssClass,
		''', ''',
		wt.[Guid],
		''', ',
		wt.[Order],
		');'
	) AS Step2
FROM 
	WorkflowType wt
	LEFT JOIN Category c ON c.Id = wt.CategoryId;