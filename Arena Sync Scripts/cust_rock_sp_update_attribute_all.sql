CREATE PROC [dbo].[cust_rock_sp_update_attribute_all]

AS

DECLARE @AttributeId int

DECLARE ArenaCursor INSENSITIVE CURSOR FOR
SELECT 
	 [attribute_id]
FROM [core_attribute] WITH (NOLOCK)

OPEN ArenaCursor
FETCH NEXT FROM ArenaCursor
INTO @AttributeId

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		EXEC [cust_rock_sp_update_attribute] @AttributeId
		
	FETCH NEXT FROM ArenaCursor
	INTO @AttributeId

	END
	
END

CLOSE ArenaCursor
DEALLOCATE ArenaCursor
