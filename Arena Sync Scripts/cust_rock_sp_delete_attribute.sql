CREATE PROC [dbo].[cust_rock_sp_delete_attribute]
@AttributeId int

AS

DECLARE @Id int

SELECT
	 @Id = [foreign_key]
FROM [core_attribute] WITH (NOLOCK)
WHERE [attribute_id] = @AttributeId

IF @Id IS NOT NULL
BEGIN

	DELETE [RockChMS].[dbo].[coreAttributeValue] 
	WHERE [AttributeId] = @Id
	
	DELETE [RockChMS].[dbo].[coreAttribute] 
	WHERE [Id] = @Id
	
END
