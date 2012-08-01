CREATE PROC [dbo].[cust_rock_sp_update_defined_value_all]

AS

DECLARE @LookupId int

DECLARE ArenaCursor INSENSITIVE CURSOR FOR
SELECT 
	 [lookup_id]
FROM [core_lookup] WITH (NOLOCK)

OPEN ArenaCursor
FETCH NEXT FROM ArenaCursor
INTO @LookupId

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		EXEC [cust_rock_sp_update_defined_value] @LookupId
		
	FETCH NEXT FROM ArenaCursor
	INTO @LookupId

	END
	
END

CLOSE ArenaCursor
DEALLOCATE ArenaCursor
