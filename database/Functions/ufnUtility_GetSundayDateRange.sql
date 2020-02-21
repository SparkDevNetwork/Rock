/*
<doc>
	<summary>
 		This function returns the First and Last Sunday Date between the @StartDate and @EndDate.
        Note: If the @StartDate and @EndDate are within the same SundayWeek, there are no SundayDates between them, so this will return an [EndSundayDate] that is a week before the [StartSundayDate]
	</summary>
	<code>
		SELECT [dbo].[ufnUtility_GetSundayDateRange]('2019-09-17 00:00:00', '2019-10-09 00:00:00')
        -- should return '2019-09-22 00:00:00', '2019-10-06 00:00:00' (assuming FirstDayOfWeek is set to Monday)
	</code>
</doc>
*/
ALTER FUNCTION [dbo].[ufnUtility_GetSundayDateRange] (
	@StartDate DATETIME
	,@EndDate DATETIME
	)
RETURNS TABLE
AS
RETURN

SELECT isnull(x.[StartSundayDate], '1900-01-01') [StartSundayDate]
	,CASE 
		WHEN x.[EndSundayDate] IS NULL
			THEN '9999-12-31'
		WHEN x.[EndSundayDate] > @EndDate
			THEN DateAdd(day, - 7, x.[EndSundayDate])
		ELSE @EndDate
		END [EndSundayDate]
FROM (
	SELECT dbo.ufnUtility_GetSundayDate(@StartDate) [StartSundayDate]
		,dbo.ufnUtility_GetSundayDate(@EndDate) [EndSundayDate]
	) x