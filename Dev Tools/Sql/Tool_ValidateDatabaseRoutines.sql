BEGIN

CREATE TABLE #tmpResults(RountineType VARCHAR(50), RoutineName VARCHAR(100), ResultMessage VARCHAR(MAX));

DECLARE 
	@routineName VARCHAR(MAX),
	@routineType VARCHAR(MAX)

DECLARE
  routine_cursor CURSOR FOR SELECT CONCAT(ROUTINE_SCHEMA,'.', ROUTINE_NAME), ROUTINE_TYPE FROM INFORMATION_SCHEMA.ROUTINES;

OPEN routine_cursor
FETCH NEXT FROM routine_cursor INTO @routineName, @routineType

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
		EXEC sp_refreshsqlmodule @routineName;
    END TRY
    BEGIN CATCH
        INSERT INTO #tmpResults(RountineType, RoutineName, ResultMessage)
		SELECT @routineType, @routineName, ERROR_MESSAGE();    
    END CATCH
    FETCH NEXT FROM routine_cursor INTO @routineName,@routineType
END

CLOSE routine_cursor
DEALLOCATE routine_cursor

SELECT *
FROM #tmpResults
WHERE ResultMessage NOT LIKE 'Cannot ALTER%because it is being referenced by object%'

DROP TABLE #tmpResults

END