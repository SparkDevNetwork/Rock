/* ====================================================== */
-- CodeGen to script workflows

-- Scripts workflows, categories, attributes, etc into sql inserts so they can be added to NewSpring 13

/* ====================================================== */

DECLARE @etid_workflowType AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.WorkflowType');
DECLARE @etid_workflow AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Workflow');
DECLARE @etid_workflowActivity AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.WorkflowActivity');
DECLARE @etid_systemEmail AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.SystemEmail');
DECLARE @etid_workflowActionType AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.WorkflowActionType');

IF OBJECT_ID('tempdb..#TempCode') IS NOT NULL DROP TABLE #TempCode;

-- STEP 1: Generate code to insert categories
SELECT 
	CAST(CONCAT(
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
	) AS NVARCHAR(MAX)) AS Code,
	1 AS Step,
	ROW_NUMBER() OVER(ORDER BY c.Id) AS SubStep
INTO #TempCode
FROM 
	Category c
	LEFT JOIN Category pc ON c.ParentCategoryId = pc.Id
WHERE 
	c.EntityTypeId = @etid_workflowType;

-- STEP 2: Generate code to copy Workflow Types
INSERT INTO #TempCode
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
	) AS Step2,
	2,
	ROW_NUMBER() OVER(ORDER BY wt.Id)
FROM 
	WorkflowType wt
	LEFT JOIN Category c ON c.Id = wt.CategoryId;

-- STEP 3: WorkflowType Attributes
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [IconCssClass], [AllowSearch]) VALUES (',
		a.IsSystem,
		', ',
		a.FieldTypeId,
		', ',
		'@etid_workflow',
		', ''',
		a.EntityTypeQualifierColumn,
		''', ',
		CASE WHEN wt.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowType WHERE [Guid] = ''',
				wt.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		'''',
		REPLACE(a.[Key], '''', ''''''),
		''', ''',
		REPLACE(a.[Name], '''', ''''''),
		''', ''',
		REPLACE(a.[Description], '''', ''''''),
		''', ',
		a.[Order],
		', ',
		a.IsGridColumn,
		', ''',
		ISNULL(REPLACE(a.[DefaultValue], '''', ''''''), 'NULL'),
		''', ',
		a.IsMultiValue,
		', ',
		a.IsRequired,
		', ''',
		a.[Guid],
		''', ''',
		a.IconCssClass,
		''', ',
		a.AllowSearch,
		');'
	) AS Step3,
	3,
	ROW_NUMBER() OVER(ORDER BY a.Id)
FROM
	Attribute a
	JOIN WorkflowType wt ON wt.Id = a.EntityTypeQualifierValue AND a.EntityTypeQualifierColumn = 'WorkflowTypeId';

-- STEP 4: Workflow Activity Types
INSERT INTO #TempCode
SELECT 
	CONCAT(
		'INSERT [WorkflowActivityType] ([IsActive], [Name], [Description], [WorkflowTypeId], [IsActivatedWithWorkflow], [Order], [Guid]) VALUES (',
		wat.IsActive,
		', ''',
		REPLACE(wat.Name, '''', ''''''),
		''', ''',
		REPLACE(wat.[Description], '''', ''''''),
		''', ',
		CASE WHEN wt.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowType WHERE [Guid] = ''',
				wt.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		wat.IsActivatedWithWorkflow,
		', ',
		wat.[Order],
		', ''',
		wat.[Guid],
		''');'
	) AS Step4,
	4,
	ROW_NUMBER() OVER(ORDER BY wat.Id)
FROM 
	WorkflowActivityType wat
	LEFT JOIN WorkflowType wt ON wat.WorkflowTypeId = wt.Id;

-- Step 5: Workflow Activity Type Attributes
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [IconCssClass], [AllowSearch]) VALUES (',
		a.IsSystem,
		', ',
		a.FieldTypeId,
		', ',
		'@etid_workflowActivity',
		', ''',
		a.EntityTypeQualifierColumn,
		''', ',
		CASE WHEN wat.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowActivityType WHERE [Guid] = ''',
				wat.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		'''',
		REPLACE(a.[Key], '''', ''''''),
		''', ''',
		REPLACE(a.[Name], '''', ''''''),
		''', ''',
		REPLACE(a.[Description], '''', ''''''),
		''', ',
		a.[Order],
		', ',
		a.IsGridColumn,
		', ''',
		ISNULL(REPLACE(a.[DefaultValue], '''', ''''''), 'NULL'),
		''', ',
		a.IsMultiValue,
		', ',
		a.IsRequired,
		', ''',
		a.[Guid],
		''', ''',
		a.IconCssClass,
		''', ',
		a.AllowSearch,
		');'
	) AS Step5,
	5,
	ROW_NUMBER() OVER(ORDER BY a.Id)
FROM
	Attribute a
	JOIN WorkflowActivityType wat ON wat.Id = a.EntityTypeQualifierValue AND a.EntityTypeQualifierColumn = 'ActivityTypeId';

-- STEP 6: System Email Categories
INSERT INTO #TempCode
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
		'@etid_systemEmail',
		', ''',
		REPLACE(c.Name, '''', ''''''),
		''', ''',
		c.IconCssClass,
		''', ''',
		c.[Guid],
		''', ',
		c.[Order],
		');'
	) AS Step6,
	6,
	ROW_NUMBER() OVER(ORDER BY c.Id)
FROM 
	Category c
	LEFT JOIN Category pc ON c.ParentCategoryId = pc.Id
WHERE 
	c.EntityTypeId = @etid_systemEmail;

-- STEP 7: System Emails
INSERT INTO #TempCode
SELECT 
	CONCAT(
		'INSERT [SystemEmail] ([IsSystem], [CategoryId], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [FromName], [Guid]) VALUES (',
		se.IsSystem,
		', ',
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
		REPLACE(se.Title, '''', ''''''),
		''', ''',
		ISNULL(REPLACE(se.[From], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[To], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[Cc], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[Bcc], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[Subject], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[Body], '''', ''''''), 'NULL'),
		''', ''',
		ISNULL(REPLACE(se.[FromName], '''', ''''''), 'NULL'),
		''', ''',
		se.[Guid],
		''');'
	) AS Step7,
	7,
	ROW_NUMBER() OVER(ORDER BY se.Id)
FROM 
	SystemEmail se
	LEFT JOIN Category c ON c.Id = se.CategoryId;

-- STEP 8: Workflow Action Forms
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [WorkflowActionForm] ([Header], [Footer], [Actions], [Guid], [NotificationSystemEmailId], [IncludeActionsInNotification], [ActionAttributeGuid], [AllowNotes]) VALUES (',
		CASE WHEN waf.Header IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(waf.Header, '''', ''''''),
				''''
			)
		END,
		', ',
		CASE WHEN waf.Footer IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(waf.Footer, '''', ''''''),
				''''
			)
		END,
		', ',
		CASE WHEN waf.Actions IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(waf.Actions, '''', ''''''),
				''''
			)
		END,
		', ''',
		waf.[Guid],
		''', ',
		CASE WHEN sa.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM SystemEmail WHERE [Guid] = ''',
				sa.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		waf.IncludeActionsInNotification,
		', ',
		CASE WHEN waf.ActionAttributeGuid IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				waf.ActionAttributeGuid,
				''''
			)
		END,
		', ',
		ISNULL(CONVERT(NVARCHAR, waf.AllowNotes), 'NULL'),
		');'
	) AS Step8,
	8,
	ROW_NUMBER() OVER(ORDER BY waf.Id)
FROM
	WorkflowActionForm waf
	LEFT JOIN SystemEmail sa ON sa.Id = waf.NotificationSystemEmailId;

-- STEP 9: Workflow Action Form Attributes
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [WorkflowActionFormAttribute] ([WorkflowActionFormId], [AttributeId], [Order], [IsVisible], [IsReadOnly], [IsRequired], [Guid], [HideLabel], [PreHtml], [PostHtml]) VALUES (',
		CASE WHEN waf.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowActionForm WHERE [Guid] = ''',
				waf.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN a.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Attribute WHERE [Guid] = ''',
				a.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		wafa.[Order],
		', ',
		wafa.IsVisible,
		', ',
		wafa.IsReadOnly,
		', ',
		wafa.IsRequired,
		', ''',
		wafa.[Guid],
		''', ',
		wafa.HideLabel,
		', ',
		CASE WHEN wafa.PreHtml IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(wafa.PreHtml, '''', ''''''),
				''''
			)
		END,
		', ',
		CASE WHEN wafa.PostHtml IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(wafa.PostHtml, '''', ''''''),
				''''
			)
		END,
		');'
	) AS Step9,
	9,
	ROW_NUMBER() OVER(ORDER BY wafa.Id)
FROM
	WorkflowActionFormAttribute wafa
	LEFT JOIN WorkflowActionForm waf ON waf.Id = wafa.WorkflowActionFormId
	LEFT JOIN Attribute a ON a.Id = wafa.AttributeId;

-- STEP 10: Workflow Action Type
INSERT INTO #TempCode
SELECT 
	CONCAT(
		'INSERT [WorkflowActionType] ([ActivityTypeId], [Name], [Order], [EntityTypeId], [IsActionCompletedOnSuccess], [IsActivityCompletedOnSuccess], [Guid], [WorkflowFormId], [CriteriaAttributeGuid], [CriteriaComparisonType], [CriteriaValue]) VALUES (',
		CASE WHEN wactt.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowActivityType WHERE [Guid] = ''',
				wactt.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		'''',
		REPLACE(wat.Name, '''', ''''''),
		''', ',
		wat.[Order],
		', ',
		CASE WHEN et.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM EntityType WHERE [Name] = ''',
				et.[Name],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		wat.IsActionCompletedOnSuccess,
		', ',
		wat.IsActivityCompletedOnSuccess,
		', ''',
		wat.[Guid],
		''', ',
		CASE WHEN waf.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowActionForm WHERE [Guid] = ''',
				waf.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN wat.CriteriaAttributeGuid IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				wat.CriteriaAttributeGuid,
				''''
			)
		END,
		', ',
		wat.CriteriaComparisonType,
		', ',
		CASE WHEN wat.CriteriaValue IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(wat.CriteriaValue, '''', ''''''),
				''''
			)
		END,
		');'
	) AS Step10,
	10,
	ROW_NUMBER() OVER(ORDER BY wat.Id)
FROM 
	WorkflowActionType wat
	LEFT JOIN WorkflowActivityType wactt ON wat.ActivityTypeId = wactt.Id
	LEFT JOIN WorkflowActionForm waf ON waf.Id = wat.WorkflowFormId
	LEFT JOIN EntityType et ON wat.EntityTypeId = et.Id;

-- STEP 11 - Workflow Action Type Attribute Values
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid]) VALUES (',
		av.IsSystem,
		', ',
		CASE WHEN a.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Attribute WHERE [Guid] = ''',
				a.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN wat.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM WorkflowActionType WHERE [Guid] = ''',
				wat.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN av.Value IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(av.Value, '''', ''''''),
				''''
			)
		END,
		', ''',
		av.[Guid],
		'''', 
		');'
	) AS Step11,
	11,
	ROW_NUMBER() OVER(ORDER BY av.Id)
FROM
	AttributeValue av
	JOIN WorkflowActionType wat ON wat.Id = av.EntityId
	JOIN Attribute a ON a.Id = av.AttributeId
WHERE
	a.EntityTypeId = @etid_workflowActionType;

SELECT Code FROM #TempCode ORDER BY Step, SubStep;