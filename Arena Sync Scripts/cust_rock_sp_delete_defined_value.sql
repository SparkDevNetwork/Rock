CREATE PROC [dbo].[cust_rock_sp_delete_defined_value]
@LookupId int

AS

DECLARE @Id int

SELECT
	 @Id = [foreign_key]
FROM [core_lookup] WITH (NOLOCK)
WHERE [lookup_id] = @LookupId

IF @Id IS NOT NULL
BEGIN

	DELETE [RockChMS].[dbo].[coreDefinedValue] 
	WHERE [Id] = @Id
	
END
