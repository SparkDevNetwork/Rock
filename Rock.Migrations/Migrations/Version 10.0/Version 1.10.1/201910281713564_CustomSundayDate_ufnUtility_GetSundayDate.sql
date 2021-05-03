/*
<doc>
	<summary>
 		This function returns the Sunday date of a given date.
	</summary>

	<returns>
		The Sunday of the date given with Sunday being the SundayDate that is associated with the specified date, depending on the FirstDayOfWeek that is configured in Rock
	</returns>
	<code>
		SELECT [dbo].[ufnUtility_GetSundayDate](getdate())
	</code>
</doc>
*/
/* This is code generated */
ALTER FUNCTION [dbo].[ufnUtility_GetSundayDate] (@InputDate DATETIME)
RETURNS DATE
AS
BEGIN
	DECLARE @InputDOW INT
	DECLARE @sundayDiff INT
	DECLARE @lastDayOfWeek INT
	DECLARE @sundayDate DATE

	-- Configured StartDOW where (using .NET DOW standard)  0 = Sunday and 6 = Saturday. Default is 1 (Monday)
    -- NOTE: This function ([ufnUtility_GetSundayDate]) is code generated so that @FirstDayOfWeek matches what is configured in Rock
	DECLARE @FirstDayOfWeek INT = 1

	-- Sql DOW is 1 based (1 = Sunday, 7 = Saturday )
	-- So, lets convert it to the .NET standard of 0 = Sunday 
	-- from http://stackoverflow.com/a/5109557/1755417 to get the day of week deterministically, but convert to .NET DOW
	SET @InputDOW = ((datediff(day, convert(DATETIME, '18991231', 112), @inputDate) % 7))
	
    -- Get the number of days until the next Sunday date
	SET @sundayDiff = 7 - @InputDOW;
	
    -- Figure out which DayOfWeek would be the lastDayOfWeek
	-- which would be the DayOfWeek before the startDayOfWeek
	-- If the First DOW is Sunday, then the Last DOW is Saturday. Otherwise, the Last DOW is the First DOW - 1
	SET @lastDayOfWeek = CASE 
			WHEN @FirstDayOfWeek = 0
				THEN 6
			ELSE @FirstDayOfWeek - 1
			END
	
    -- There are 3 cases to deal with, and it can get confusing if Sunday isn't the last day of the week
	-- 1) The input date's DOW is Sunday. Today is the Sunday, so the Sunday Date is today
	-- 2) The input date's DOW is after the Last DOW (Today is Thursday, and the week ends next week on Tuesday).
	-- 3) The input date's DOW is before the Last DOW (Today is Monday, but the week ends this week on Tuesday)
	SET @sundayDate = CASE 
			WHEN (@InputDOW = 0)
				THEN @InputDate
			WHEN (@lastDayOfWeek < @InputDOW)
				THEN dateadd(day, @sundayDiff, @InputDate)
			ELSE dateadd(day, @sundayDiff - 7, @InputDate)
			END

	RETURN @sundayDate
END