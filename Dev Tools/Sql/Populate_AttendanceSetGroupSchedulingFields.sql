DECLARE @attendanceCount INT = (
		SELECT count(*)
		FROM Attendance a
		INNER JOIN AttendanceOccurrence ao ON a.OccurrenceId = ao.Id
		INNER JOIN [Group] g ON ao.GroupId = g.Id
		INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
		WHERE gt.IsSchedulingEnabled = 1
		)
DECLARE @scheduledCount INT = (
		SELECT cast(RAND() * @attendanceCount * 0.60 AS INT)
		)
	,@pendingCount INT = (
		SELECT cast(RAND() * @attendanceCount * 0.20 AS INT)
		)
	,@declineCount INT = (
		SELECT cast(RAND() * @attendanceCount * 0.10 AS INT)
		)

BEGIN
	-- Set all to Unknown first
	UPDATE a
	SET RSVP = 3 /* Unknown */
	FROM Attendance a
	INNER JOIN AttendanceOccurrence ao ON a.OccurrenceId = ao.Id
	INNER JOIN [Group] g ON ao.GroupId = g.Id
	INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
	WHERE gt.IsSchedulingEnabled = 1

	UPDATE Attendance
	SET ScheduledToAttend = 1
		,RequestedToAttend = 1
		,RSVP = 1 /* YES */
	WHERE Id IN (
			SELECT TOP (@scheduledCount) a.Id
			FROM Attendance a
			INNER JOIN AttendanceOccurrence ao ON a.OccurrenceId = ao.Id
			INNER JOIN [Group] g ON ao.GroupId = g.Id
			INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
			WHERE gt.IsSchedulingEnabled = 1
				AND RSVP = 3
			ORDER BY NEWID()
			)

	-- Set a another random 10% to declined
	UPDATE Attendance
	SET ScheduledToAttend = 0
		,RequestedToAttend = 1
		,RSVP = 0 /* Unknown */ 
		where Id IN (
			SELECT TOP (@declineCount) a.Id
			FROM Attendance a
			INNER JOIN AttendanceOccurrence ao ON a.OccurrenceId = ao.Id
			INNER JOIN [Group] g ON ao.GroupId = g.Id
			INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
			WHERE gt.IsSchedulingEnabled = 1
				AND RSVP = 3
			ORDER BY NEWID()
			)

	-- Set the remaining RSVP.Unknown (3) records (30%) 
	UPDATE Attendance
	SET ScheduledToAttend = 0
		,RequestedToAttend = 1
		,RSVP = 3 /* NO */ 
		where Id IN (
			SELECT TOP (@pendingCount) a.Id
			FROM Attendance a
			INNER JOIN AttendanceOccurrence ao ON a.OccurrenceId = ao.Id
			INNER JOIN [Group] g ON ao.GroupId = g.Id
			INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
			WHERE gt.IsSchedulingEnabled = 1
				AND RSVP = 3
			ORDER BY NEWID()
			)
END


