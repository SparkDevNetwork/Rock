SELECT pa.PersonId, p.NickName, p.LastName,  a.DidAttend, a.ScheduledToAttend, a.RSVP, a.RequestedToAttend, a.DidAttend, a.Id [Attendance.Id], ao.Id [Occurrence.Id], ao.OccurrenceDate, g.Name [Group.Name], ao.SundayDate, l.Name [Location.Name], s.Name [Schedule.Name]
FROM Attendance a
JOIN AttendanceOccurrence ao
	ON a.OccurrenceId = ao.Id
JOIN [Group] g
	ON ao.GroupId = g.Id
JOIN PersonAlias pa
	ON a.PersonAliasId = pa.Id
JOIN Person p
	ON pa.PersonId = p.Id
JOIN [Location] l
	ON ao.LocationId = l.Id
JOIN Schedule s
	ON ao.ScheduleId = s.Id
WHERE (
		
        a.ScheduledToAttend = 1 -- Accepted
		
        or (RequestedToAttend = 1 and RSVP in (1))  -- Pending
		or (RequestedToAttend = 1 and RSVP in (0))  -- Declined
		
        ) AND ao.SundayDate > GetDate() AND ao.SundayDate < DATEADD(day, 7, GetDate())
	
    -- set this to a specific group if you want
    -- and g.Name = 'Children''s'
	
    AND s.IsActive = 1
ORDER BY ao.SundayDate, ao.OccurrenceDate, g.Name, l.Name, s.Name

/* manually set DidAttend if needed


update Attendance set DidAttend = 1 where Id = 2151

*/