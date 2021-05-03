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
		SELECT * FROM [vCheckin_Attendance] WHERE [PersonAliasId] = 4
	</code>
</doc>
*/
ALTER VIEW [dbo].[vCheckin_Attendance]

AS

SELECT A.[Id]
      ,O.[LocationId]
      ,O.[ScheduleId]
      ,O.[GroupId]
      ,A.[DeviceId]
      ,A.[SearchTypeValueId]
      ,A.[AttendanceCodeId]
      ,A.[QualifierValueId]
      ,A.[StartDateTime]
      ,A.[EndDateTime]
      ,A.[DidAttend]
      ,A.[Note]
      ,A.[Guid]
      ,A.[CreatedDateTime]
      ,A.[ModifiedDateTime]
      ,A.[CreatedByPersonAliasId]
      ,A.[ModifiedByPersonAliasId]
      ,A.[ForeignKey]
      ,A.[CampusId]
      ,A.[PersonAliasId]
      ,A.[RSVP]
      ,O.[DidNotOccur]
      ,A.[Processed]
      ,A.[ForeignGuid]
      ,A.[ForeignId]
      ,O.[SundayDate]
      ,A.[SearchValue]
      ,A.[SearchResultGroupId]
  FROM [Attendance] A
  INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]