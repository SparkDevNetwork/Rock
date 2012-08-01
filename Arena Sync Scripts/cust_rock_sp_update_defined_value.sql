CREATE PROC [dbo].[cust_rock_sp_update_defined_value]
@LookupId int

AS

DECLARE @Id int
DECLARE @DefinedTypeId int
DECLARE @Order int
DECLARE @Name nvarchar(100)

SELECT
	 @Id = L.[foreign_key]
	,@DefinedTypeId = LT.[foreign_key]
	,@Order = L.[lookup_order]
	,@Name = L.[lookup_value]
FROM [core_lookup] L WITH (NOLOCK)
INNER JOIN [core_lookup_type] LT WITH (NOLOCK)
	ON L.[lookup_type_id] = LT.[lookup_type_id]
WHERE [lookup_id] = @LookupId

IF @Id IS NULL OR 
	NOT EXISTS ( SELECT [Id] FROM [RockChMS].[dbo].[coreDefinedValue] WITH (NOLOCK) WHERE [Id] = @Id)
BEGIN

	IF EXISTS ( SELECT [Id] FROM [RockChMS].[dbo].[coreDefinedType] WITH (NOLOCK) WHERE [Id] = @DefinedTypeId )
	BEGIN
	
		INSERT INTO [RockChMS].[dbo].[coreDefinedValue] (
			 [IsSystem]
			,[DefinedTypeId]
			,[Order]
			,[Name]
			,[CreatedDateTime]
			,[ModifiedDateTime]
			,[CreatedByPersonId]
			,[ModifiedByPersonId]
			,[Guid]
		)
		VALUES (
			 0
			,@DefinedTypeId
			,@Order
			,@Name
			,GETDATE()
			,GETDATE()
			,1
			,1
			,NEWID()
		)
		
		SET @Id = SCOPE_IDENTITY()
		
		UPDATE [core_lookup]
		SET 
			[foreign_key] = @Id
		WHERE [lookup_id] = @LookupID

	END
END
ELSE
BEGIN

	UPDATE [RockChMS].[dbo].[coreDefinedValue]
	SET
		 [Order] = @Order
		,[Name] = @Name
		,[ModifiedDateTime] = GETDATE()
		,[ModifiedByPersonId] = 1
	WHERE [Id] = @Id
END
