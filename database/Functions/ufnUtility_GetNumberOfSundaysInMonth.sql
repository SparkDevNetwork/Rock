/*
<doc>
	<summary>
 		This function returns the number of Sundays in a given month.
	</summary>

	<returns>
		An integer of the number of Sundays in a given month
	</returns>
	<param name="Year" datatype="int">Year to use for the date</param>
	<param name="Month" datatype="int">Month to use for the month</param>
	<param name="Exclude Future" datatype="bit">Used to determine if future Sundays should be counted</param>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetNumberOfSundaysInMonth](3,2014, 'False')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnUtility_GetNumberOfSundaysInMonth](@Month int, @Year int, @ExcludeFuture bit) 

RETURNS int AS

BEGIN
	DECLARE @FirstDayOfMonth datetime
	DECLARE @SundayCount int
	
	-- get date of first day
	SET @FirstDayOfMonth = CAST(CONVERT(varchar, @Year) + '-' + CONVERT(varchar, @Month) + '-01' AS datetime)

	-- fill a table with all dates in month
	;with cteAllDates AS
	(
		SELECT @FirstDayOfMonth AS DateOf
			UNION ALL
			SELECT DateOf+1
				FROM cteAllDates
				WHERE
				MONTH(DateOf+1)=MONTH(@FirstDayOfMonth)
	)

	-- select out Sundays in a way that works across SQL Server setups
	SELECT @SundayCount = COUNT(DateOf) 
		FROM cteAllDates 
		WHERE 
			(@ExcludeFuture = 0 AND ((DATEPART(weekday, DateOf) + @@DATEFIRST + 5) % 7 + 1) = 7 )
			OR 
			(@ExcludeFuture = 1 AND [DateOf] <= getdate() AND ((DATEPART(weekday, DateOf) + @@DATEFIRST + 5) % 7 + 1) = 7 )



	RETURN @SundayCount
END