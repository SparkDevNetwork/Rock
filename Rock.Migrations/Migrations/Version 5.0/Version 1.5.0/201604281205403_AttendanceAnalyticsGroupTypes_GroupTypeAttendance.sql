IF  EXISTS ( SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vCheckin_GroupTypeAttendance]') AND type = N'V' )
DROP VIEW [dbo].[vCheckin_GroupTypeAttendance]
GO

/*
<doc>
	<summary>
 		This view returns distinct attendance dates for a person and group type
	</summary>

	<returns>
		* GroupTypeId
        * PersonId
		* SundayDate
	</returns>
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
        A.[GroupId],
        A.[ScheduleId],
        A.[CampusId],
        A.[LocationId],
        G.[Name] AS [GroupName],
		G.[GroupTypeId],		
		PA.[PersonId],
		A.[StartDateTime],
		CONVERT( date, [StartDateTime] ) AS [StartDate],
		DATEADD( day, ( 7 - ( ( DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), [StartDateTime] ) % 7 ) + 1 ) ), CONVERT( date, [StartDateTime] ) ) AS [SundayDate]
	FROM [Attendance] A
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
	AND A.[DidAttend] = 1