CREATE PROC [dbo].[cust_rock_sp_delete_defined_type]
@LookupTypeId int

AS

DECLARE @Id int

SELECT
	 @Id = [foreign_key]
FROM [core_lookup_type] WITH (NOLOCK)
WHERE [lookup_type_id] = @LookupTypeId

IF @Id IS NOT NULL
BEGIN

	DELETE [RockChMS].[dbo].[coreDefinedValue] 
	WHERE [DefinedTypeId] = @Id
	
	DELETE [RockChMS].[dbo].[coreDefinedType] 
	WHERE [Id] = @Id
	
END
