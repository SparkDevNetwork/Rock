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
ALTER VIEW [vCheckin_GroupTypeAttendance]

AS

	SELECT DISTINCT
        A.[Id],
        A.[GroupId],
        A.[ScheduleId],
        A.[CampusId],
        A.[LocationId],
        G.[Name] AS [GroupName],
		GTA.[GroupTypeId],		
		PA.[PersonId],
		A.[StartDateTime],
		CONVERT( date, [StartDateTime] ) AS [StartDate],
		DATEADD( day, ( 7 - ( ( DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), [StartDateTime] ) % 7 ) + 1 ) ), CONVERT( date, [StartDateTime] ) ) AS [SundayDate]
	FROM [Attendance] A
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
	INNER JOIN [GroupTypeAssociation] GTA ON GTA.[ChildGroupTypeId] = G.[GroupTypeId]
	AND A.[DidAttend] = 1