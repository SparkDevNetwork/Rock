BEGIN

declare 
  @routineName varchar(max),
  @routineType varchar(max)
declare
  routine_cursor CURSOR for select ROUTINE_NAME, ROUTINE_TYPE from INFORMATION_SCHEMA.ROUTINES;

open routine_cursor
fetch next from routine_cursor into @routineName,@routineType

while @@FETCH_STATUS = 0
begin
    begin try
      exec sp_refreshsqlmodule @routineName;
    end try
    begin catch
        select 'Error in ' + @routineType + ' ' + @routineName + ': ' +  ERROR_MESSAGE();    
    end catch
    fetch next from routine_cursor into @routineName,@routineType
end

close routine_cursor
deallocate routine_cursor

end