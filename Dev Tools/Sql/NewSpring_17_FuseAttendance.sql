/* ====================================================== */
-- Import small and fuse group attendance

-- People and groups must exist in the DB already!

/* ====================================================== */

INSERT INTO Attendance (GroupId, StartDateTime, DidAttend, [Guid], CreatedDateTime, ForeignKey, PersonAliasId, CampusId, ScheduleId)
SELECT
	g.Id,
	ga.StartDateTime,
	1,
	NEWID(),
	GETDATE(),
	'F1.GroupsAttendance',
	pa.Id,
	g.CampusId,
	g.ScheduleId
FROM 
	[CEN-SQLDEV001].[F1].dbo.[GroupsAttendance] ga
	JOIN PersonAlias pa ON pa.ForeignId = ga.IndividualID
	JOIN [Group] g ON g.ForeignId = ga.GroupId
WHERE 
	ga.IndividualIsPresent <> 0