CREATE PROC [dbo].[cust_rock_sp_update_attribute]
@AttributeId int

AS

DECLARE @Id int
DECLARE @AttributeType int
DECLARE @TypeQualifier varchar(100)
DECLARE @FieldTypeId int
DECLARE @Key nvarchar(50)
DECLARE @Name nvarchar(100)
DECLARE @Category nvarchar(100)
DECLARE @Order int
DECLARE @Required bit

SELECT
	 @Id = A.[foreign_key]
	,@AttributeType = A.[attribute_type]
	,@TypeQualifier = A.[type_qualifier]
	,@FieldTypeId = CASE A.[attribute_type]
		WHEN 0 THEN 7	-- Integer
		WHEN 2 THEN 11	-- Date
		WHEN 3 THEN 16  -- Lookup\Defined Value
		WHEN 4 THEN 3	-- Bool
		WHEN 5 THEN 14	-- Decimal
		WHEN 6 THEN 13	-- Currency
		WHEN 7 THEN 19	-- Url
		--WHEN 8 THEN	-- Guid (Not used)
		WHEN 9 THEN 17	-- Document
		WHEN 10 THEN 18 -- Person
		ELSE 1			-- Text
	 END	
	,@Key = 'Arena' + '-' + CAST(G.attribute_group_id as varchar) + '-' + CAST(A.attribute_id as varchar)
	,@Name = A.[attribute_name]
	,@Category = G.[group_name]
	,@Order = A.[attribute_order]
	,@Required = A.[required]
FROM [core_attribute] A WITH (NOLOCK)
INNER JOIN [core_attribute_group] G WITH (NOLOCK)
	ON G.[attribute_group_id] = A.[attribute_group_id]
WHERE A.[attribute_id] = @AttributeId

IF @Id IS NULL OR 
	NOT EXISTS ( SELECT [Id] FROM [RockChMS].[dbo].[coreAttribute] WITH (NOLOCK) WHERE [Id] = @Id)
BEGIN

	INSERT INTO [RockChMS].[dbo].[coreAttribute] (
		 [IsSystem]
		,[FieldTypeId]
		,[Entity]
		,[EntityQualifierColumn]
		,[EntityQualifierValue]
		,[Key]
		,[Name]
		,[Category]
		,[Order]
		,[IsGridColumn]
		,[IsMultiValue]
		,[IsRequired]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[CreatedByPersonId]
		,[ModifiedByPersonId]
		,[Guid]
	)
	VALUES (
		 0
		,@FieldTypeId
		,'Rock.CRM.Person'
		,''
		,''
		,@Key
		,@Name
		,@Category
		,@Order
		,0
		,0
		,@Required
		,GETDATE()
		,GETDATE()
		,1
		,1
		,NEWID()
	)
	
	SET @Id = SCOPE_IDENTITY()
	
	UPDATE [core_attribute]
	SET 
		[foreign_key] = @Id
	WHERE [attribute_id] = @AttributeId

END
ELSE
BEGIN

	UPDATE [RockChMS].[dbo].[coreAttribute]
	SET
		 [FieldTypeId] = @FieldTypeId
		,[Key] = @Key
		,[Name] = @Name
		,[Category] = @Category
		,[Order] = @Order
		,[IsRequired] = @Required
		,[ModifiedDateTime] = GETDATE()
		,[ModifiedByPersonId] = 1
	WHERE [Id] = @Id
	
END

IF @AttributeType = 3
BEGIN

	DECLARE @DefinedTypeId int
	
	SELECT TOP 1
		@DefinedTypeId = [foreign_key]
	FROM [core_lookup_type]
	WHERE CAST([lookup_type_id] as varchar) = @TypeQualifier

	IF NOT EXISTS ( 
		SELECT [Id] 
		FROM [RockChMS].[dbo].[coreAttributeQualifier] WITH (NOLOCK) 
		WHERE [AttributeId] = @Id 
		AND [Key] = 'definedtype'
	)
	BEGIN
	
		INSERT INTO [RockChMS].[dbo].[coreAttributeQualifier] (
			 [IsSystem]
			,[AttributeId]
			,[Key]
			,[Value]
			,[CreatedDateTime]
			,[ModifiedDateTime]
			,[CreatedByPersonId]
			,[ModifiedByPersonId]
			,[Guid]
		)
		VALUES (
			0
			,@Id
			,'definedtype'
			,CAST(@DefinedTypeId AS nvarchar(max))
			,GETDATE()
			,GETDATE()
			,1
			,1
			,NEWID()
		)

	END
	ELSE
	BEGIN
	
		UPDATE [RockChMS].[dbo].[coreAttributeQualifier]
		SET 
			 [Value] = CAST(@DefinedTypeId AS nvarchar(max))
			,[ModifiedDateTime] = GETDATE()
			,[ModifiedByPersonId] = 1
		WHERE [AttributeId] = @Id 
		AND [Key] = 'definedtype'
		
	END
	
END

