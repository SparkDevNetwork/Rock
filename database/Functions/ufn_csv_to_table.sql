--
-- from http://www.sql-server-helper.com/functions/comma-delimited-to-table
--
-- drop/create the function
if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[ufn_csv_to_table]') and [type] = 'TF')
begin
    drop function [dbo].[ufn_csv_to_table];
end
go

create function [dbo].[ufn_csv_to_table] ( @input varchar(max) )
returns @outputTable table ( [id] int )
as
begin
    declare @numericstring varchar(10)

    while LEN(@input) > 0
    begin
        set @numericString      = LEFT(@input, 
                                ISNULL(NULLIF(CHARINDEX(',', @input) - 1, -1),
                                LEN(@input)))
        set @input = SUBSTRING(@input,
                                     ISNULL(NULLIF(CHARINDEX(',', @input), 0),
                                     LEN(@input)) + 1, LEN(@input))

        insert into @OutputTable ( [id] )
        values ( CAST(@numericString as int) )
    end
    
    return
end
go
