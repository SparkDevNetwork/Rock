CREATE PROC [dbo].[cust_rock_sp_update_person_all]

AS

DECLARE @PersonId int

DECLARE ArenaCursor INSENSITIVE CURSOR FOR
SELECT 
	 [person_id]
FROM [core_person] WITH (NOLOCK)

OPEN ArenaCursor
FETCH NEXT FROM ArenaCursor
INTO @PersonId

WHILE (@@FETCH_STATUS <> -1)
BEGIN

	IF (@@FETCH_STATUS = 0)
	BEGIN

		EXEC [cust_rock_sp_update_person] @PersonId
		
	FETCH NEXT FROM ArenaCursor
	INTO @PersonId

	END
	
END

CLOSE ArenaCursor
DEALLOCATE ArenaCursor
