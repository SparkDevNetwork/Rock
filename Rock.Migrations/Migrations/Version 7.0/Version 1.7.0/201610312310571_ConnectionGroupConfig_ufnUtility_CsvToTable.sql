IF object_id('[dbo].[ufnUtility_CsvToTable]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnUtility_CsvToTable]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function converts a comma-delimited string of values into a table of values
        It comes from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
		Used by spFinance_ContributionStatementQuery
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/
CREATE FUNCTION [dbo].[ufnUtility_CsvToTable]
(
	@pString VARCHAR(8000)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
--Produce values from 1 up to 10,000. Enough to cover VARCHAR(8000).
WITH E1(N) AS ( --Create 10 rows
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
),
E2(N) AS ( --10E+2 for 100 rows
	SELECT 1 
	FROM E1 a, E1 b
),   
E4(N) AS ( --10E+4 for 10,000 rows
	SELECT 1 
	FROM E2 a, E2 b
),
cteTally(N) AS (--==== This provides the "base" CTE and limits the number of rows right up front
                    -- for both a performance gain and prevention of accidental "overruns"
	SELECT TOP (ISNULL(DATALENGTH(@pString),0)) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) 
	FROM E4
),
cteStart(N1) AS (--==== This returns N+1 (starting position of each "element" just once for each delimiter)
	SELECT 1 
	UNION ALL
	SELECT t.N+1 
	FROM cteTally t WHERE SUBSTRING(@pString,t.N,1) = ','
),
cteLen(N1,L1) AS(--==== Return start and length (for use in substring)
	SELECT s.N1, 
		ISNULL(NULLIF(CHARINDEX(',',@pString,s.N1),0)-s.N1,8000)
	FROM cteStart s
)
--===== Do the actual split. The ISNULL/NULLIF combo handles the length for the final element when no delimiter is found.
SELECT Item = SUBSTRING(@pString, l.N1, l.L1)
FROM cteLen l
GO