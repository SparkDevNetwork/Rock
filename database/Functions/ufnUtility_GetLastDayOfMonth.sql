/*
<doc>
	<summary>
 		This function returns the date of the last day of the month.
	</summary>

	<returns>
		Datetime of the last day of the month.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetLastDayOfMonth](getdate())
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnUtility_GetLastDayOfMonth](@InputDate datetime) 

RETURNS datetime AS

BEGIN

	RETURN DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, @InputDate) + 1, 0))
END