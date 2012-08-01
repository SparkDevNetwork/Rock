CREATE PROC [dbo].[cust_rock_sp_update_defined_type]
@LookupTypeId int

AS

DECLARE @Id int
DECLARE @Name nvarchar(100)
DECLARE @Category nvarchar(100)
DECLARE @Description nvarchar(max)

SELECT
	 @Id = [foreign_key]
	,@Name = [lookup_type_name]
	,@Category = [lookup_category]
	,@Description = [lookup_type_desc]
FROM [core_lookup_type] WITH (NOLOCK)
WHERE [lookup_type_id] = @LookupTypeId

IF @Id IS NULL OR 
	NOT EXISTS ( SELECT [Id] FROM [RockChMS].[dbo].[coreDefinedType] WITH (NOLOCK) WHERE [Id] = @Id)
BEGIN

	INSERT INTO [RockChMS].[dbo].[coreDefinedType] (
		 [IsSystem]
		,[Order]
		,[Category]
		,[Name]
		,[Description]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[CreatedByPersonId]
		,[ModifiedByPersonId]
		,[Guid]
	)
	VALUES (
		 0
		,999
		,@Category
		,@Name
		,@Description
		,GETDATE()
		,GETDATE()
		,1
		,1
		,NEWID()
	)
	
	SET @Id = SCOPE_IDENTITY()
	
	UPDATE [core_lookup_type]
	SET 
		[foreign_key] = @Id
	WHERE [lookup_type_id] = @LookupTypeID

END
ELSE
BEGIN

	UPDATE [RockChMS].[dbo].[coreDefinedType]
	SET
		 [Category] = @Category
		,[Name] = @Name
		,[Description] = @Description
		,[ModifiedDateTime] = GETDATE()
		,[ModifiedByPersonId] = 1
	WHERE [Id] = @Id
END
