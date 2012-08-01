CREATE PROC [dbo].[cust_rock_sp_update_attribute_value_all]

AS

DECLARE @PersonId int
DECLARE @AttributeId int

DECLARE ArenaCursor INSENSITIVE CURSOR FOR
SELECT 
	 [person_id]
	,[attribute_id]
FROM [core_person_attribute] WITH (NOLOCK)

OPEN ArenaCursor
FETCH NEXT FROM ArenaCursor
INTO @PersonId, @AttributeId

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		EXEC [cust_rock_sp_update_attribute_value] @PersonId, @AttributeId
		
	FETCH NEXT FROM ArenaCursor
	INTO @PersonId, @AttributeId

	END
	
END

CLOSE ArenaCursor
DEALLOCATE ArenaCursor
