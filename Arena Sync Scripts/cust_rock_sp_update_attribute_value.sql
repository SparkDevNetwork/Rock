CREATE PROC [dbo].[cust_rock_sp_update_attribute_value]
@PersonId int,
@AttributeId int

AS 

DECLARE @RockAttributeId int
DECLARE @EntityId int
DECLARE @Value nvarchar(max)

SELECT
	 @RockAttributeId = A.[foreign_key]
	,@EntityId = P.[foreign_key]
	,@Value = 
		CASE A.[attribute_type]
			WHEN 0 THEN CAST(PA.int_value as nvarchar(max))		-- Integer
			WHEN 1 THEN CAST(PA.varchar_value as nvarchar(max))	-- Text
			WHEN 2 THEN CAST(PA.datetime_value as nvarchar(max))-- Date
			WHEN 3 THEN (										-- Lookup\Defined Value
				SELECT TOP 1 
					CAST([foreign_key] as nvarchar(max)) 
				FROM [core_lookup] LU
				WHERE [lookup_id] = PA.[int_value]
			)
			WHEN 4 THEN CAST((									-- Bool
				CASE WHEN PA.int_value = 1 THEN 'True' ELSE 'False' END
			) as nvarchar(max))		
			WHEN 5 THEN CAST(PA.decimal_value as nvarchar(max))	-- Decimal
			WHEN 6 THEN CAST(PA.decimal_value as nvarchar(max))	-- Currency
			WHEN 7 THEN CAST(PA.varchar_value as nvarchar(max))	-- Url
			--WHEN 8 THEN										-- Guid
			WHEN 9 THEN CAST(PA.int_value as nvarchar(max))		-- Document
			WHEN 10 THEN (										-- Person
				SELECT TOP 1 
					CAST([foreign_key] as nvarchar(max)) 
				FROM core_person 
				WHERE person_id = PA.int_value
			) 
		 END
FROM [core_person_attribute] PA WITH (NOLOCK)
INNER JOIN [core_attribute] A WITH (NOLOCK)
	ON A.[attribute_id] = PA.[attribute_id]
INNER JOIN [core_person] P WITH (NOLOCK)
	ON P.[person_id] = PA.[person_id]
WHERE PA.[person_id] = @PersonId
AND PA.[attribute_id] = @AttributeId

IF NOT EXISTS ( 
	SELECT [Id]
	FROM [RockChMS].[dbo].[coreAttributeValue]
	WHERE [AttributeId] = @RockAttributeId
	AND [EntityId] = @EntityId
)

BEGIN

	INSERT INTO [RockChMS].[dbo].[coreAttributeValue] (
		 [IsSystem]
		,[AttributeId]
		,[EntityId]
		,[Order]
		,[Value]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[CreatedByPersonId]
		,[ModifiedByPersonId]
		,[Guid]
	)
	VALUES (
		 0
		,@RockAttributeId
		,@EntityId
		,0
		,@Value
		,GETDATE()
		,GETDATE()
		,1
		,1
		,NEWID()
	)

END
ELSE
BEGIN

	UPDATE [RockChMS].[dbo].[coreAttributeValue] 
	SET [Value] = @Value	
	WHERE [AttributeId] = @RockAttributeId
	AND [EntityId] = @EntityId

END



