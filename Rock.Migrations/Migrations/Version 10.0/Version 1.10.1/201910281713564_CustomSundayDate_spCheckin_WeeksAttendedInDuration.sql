/*
<doc>
	<summary>
 		This function returns the number of weekends a member of a family has attended a weekend service
		in the last X weeks.
	</summary>

	<returns>
		* Number of weeks
	</returns>
	<param name=""PersonId"" datatype=""int"">The person id to use</param>
	<param name=""WeekDuration"" datatype=""int"">The number of weeks to use as the duration (default 16)</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	@PersonId int
	,@WeekDuration int = 16
AS
BEGIN
	
    DECLARE @startSunday datetime 
    DECLARE @lastSunday datetime 

    SET @lastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()
    SET @startSunday = DATEADD(WEEK, ((@WeekDuration -1) * -1), @lastSunday)

    SELECT 
	    COUNT(DISTINCT O.SundayDate )
    FROM
	    [Attendance] a
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
	    INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
    WHERE 
	    O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	    AND O.[SundayDate] BETWEEN @startSunday AND @lastSunday
	    AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
		AND a.[DidAttend] = 1

END