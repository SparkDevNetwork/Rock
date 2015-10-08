/*
<doc>
	<summary>
 		This function returns the Sunday date of a given date.
	</summary>

	<returns>
		The Sunday of the date given with Sunday being the last day of the week.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetSundayDate](getdate())
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnUtility_GetSundayDate](@InputDate datetime) 

RETURNS date AS

BEGIN
	DECLARE @DayOfWeek int
	DECLARE @DaysToAdd int
	DECLARE @SundayDate datetime

	-- get day of the week in a way that will work with all SQL Server settings Monday = 1
	SET @DayOfWeek = (DATEPART(weekday, @InputDate) + @@DATEFIRST + 5) % 7 + 1

	-- calculate days to add to get to Sunday
	SET @DaysToAdd = 7 - @DayOfWeek

	SET @SundayDate = DATEADD(day, @DaysToAdd, @InputDate)

	RETURN @SundayDate
END



