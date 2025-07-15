/*
<doc>
	<summary>

        *** THIS FUNCTION IS OBSOLETE.  PLEASE USE STRING_SPLIT(@YourString, ',') INSTEAD. ***

 		This function converts a comma-delimited string of values into a table of values
        The original version came from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
        (Previously) Used by:
            * spFinance_ContributionStatementQuery
            * spFinance_GivingAnalyticsQuery_AccountTotals
            * spFinance_GivingAnalyticsQuery_PersonSummary
            * spFinance_GivingAnalyticsQuery_TransactionData
            * spCheckin_AttendanceAnalyticsQuery_AttendeeDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance
            * spCheckin_AttendanceAnalyticsQuery_Attendees
            * spCheckin_AttendanceAnalyticsQuery_NonAttendees
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/

/* #Obsolete# - Use STRING_SPLIT() instead */

ALTER FUNCTION [dbo].[ufnUtility_CsvToTable] 
(
 @pString VARCHAR(MAX)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT Item = TRY_CONVERT(INT, TRIM(value))
    FROM STRING_SPLIT(@pString, ',');