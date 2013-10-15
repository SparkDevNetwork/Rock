--
-- from http://www.sql-server-helper.com/functions/comma-delimited-to-table
--
-- drop/create the function
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_csv_to_table]') and [type] = 'TF')
BEGIN
    DROP FUNCTION [dbo].[ufn_csv_to_table];
END
GO

CREATE FUNCTION [dbo].[ufn_csv_to_table] ( @input VARCHAR(max) )
RETURNS @outputTable TABLE ( [id] int )
AS
BEGIN
    DECLARE @numericString VARCHAR(10)

    WHILE LEN(@input) > 0
    BEGIN
        SET @numericString      = LEFT(@input, 
                                ISNULL(NULLIF(CHARINDEX(',', @input) - 1, -1),
                                LEN(@input)))
        SET @input = SUBSTRING(@input,
                                     ISNULL(NULLIF(CHARINDEX(',', @input), 0),
                                     LEN(@input)) + 1, LEN(@input))

        INSERT INTO @OutputTable ( [id] )
        VALUES ( CAST(@numericString AS INT) )
    END
    
    RETURN
END
GO
