CREATE PROC [dbo].[cust_rock_sp_update_defined_type_all]

AS

DECLARE @LookupTypeId int

DECLARE ArenaCursor INSENSITIVE CURSOR FOR
SELECT 
	 [lookup_type_id]
FROM [core_lookup_type] WITH (NOLOCK)

OPEN ArenaCursor
FETCH NEXT FROM ArenaCursor
INTO @LookupTypeId

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		EXEC [cust_rock_sp_update_defined_type] @LookupTypeId
		
	FETCH NEXT FROM ArenaCursor
	INTO @LookupTypeId

	END
	
END

CLOSE ArenaCursor
DEALLOCATE ArenaCursor


