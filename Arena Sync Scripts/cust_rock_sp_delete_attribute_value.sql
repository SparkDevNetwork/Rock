CREATE PROC [dbo].[cust_rock_sp_delete_attribute_value]
@PersonId int,
@AttributeId int

AS

DECLARE @RockAttributeId int
DECLARE @EntityId int

SELECT
	 @RockAttributeId = [foreign_key]
FROM [core_attribute] WITH (NOLOCK)
WHERE [attribute_id] = @AttributeId

SELECT
	 @EntityId = [foreign_key]
FROM [core_person] WITH (NOLOCK)
WHERE [person_id] = @PersonId

IF @RockAttributeId IS NOT NULL
AND @EntityId IS NOT NULL
BEGIN

	DELETE [RockChMS].[dbo].[coreAttributeValue] 
	WHERE [AttributeId] = @AttributeId
	AND [EntityId] = @EntityId
	
END
