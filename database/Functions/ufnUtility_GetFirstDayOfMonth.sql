/*
<doc>
	<summary>
 		This function returns the date of the first of the month.
	</summary>

	<returns>
		Datetime of the first of the month.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetFirstDayOfMonth](getdate())
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnUtility_GetFirstDayOfMonth](@InputDate datetime) 

RETURNS datetime AS

BEGIN

	RETURN DATEADD(month, DATEDIFF(month, 0, getdate()), 0)
END