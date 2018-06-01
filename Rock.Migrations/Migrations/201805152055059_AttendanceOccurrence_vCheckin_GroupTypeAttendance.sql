IF  EXISTS ( SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vCheckin_GroupTypeAttendance]') AND type = N'V' )
DROP VIEW [dbo].[vCheckin_GroupTypeAttendance]
GO

/*
<doc>
	<summary>
 		This view returns attendance records in a pre-version 8 format 
		(It is provided as a way for backward compatability so that scripts that were referencing the Attendance table
		prior to adding AttendanceOccurrence, can be easily modified to use this view instead.
	</summary>

	<remarks>	
	</remarks>
	<code>
		SELECT * FROM [vCheckin_GroupTypeAttendance] WHERE [GroupTypeId] = 14
	</code>
</doc>
*/
CREATE VIEW [dbo].[vCheckin_GroupTypeAttendance]

AS

	SELECT DISTINCT
        A.[Id],
        O.[GroupId],
        O.[ScheduleId],
        A.[CampusId],
        O.[LocationId],
        G.[Name] AS [GroupName],
		G.[GroupTypeId],		
		PA.[PersonId],
		A.[StartDateTime],
		O.[OccurrenceDate] AS [StartDate],
		O.[SundayDate]
	FROM [Attendance] A
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN [Group] G ON G.[Id] = O.[GroupId]
	AND A.[DidAttend] = 1


