/* ====================================================== */
-- KidSpring Tag and Breakout Report

-- This query reports all checked in people the day of the 
-- most recent checkins to KidSpring groups. Usually this
-- would mean that kids have checked in on the most recent 
-- Sunday, X.  Therefore all kids checked in on X are 
-- included.  The report shows names, tag code, legal notes,
-- allergy notes, breakout group, etc.

/* ====================================================== */

WITH KidSpringAttendeeGroups AS (
	SELECT
		g.Id
	FROM
		[Group] g
	WHERE
		g.GroupTypeId in (756, 757, 758, 759) 
), LatestSunday AS (
	SELECT
		MAX(CONVERT(DATE, StartDateTime)) AS LatestSunday 
	FROM 
		Attendance a
		JOIN KidSpringAttendeeGroups g ON a.GroupID = g.Id
)
SELECT 
    a.StartDateTime AS CheckinTime,
	g.Name AS [Group], 
	l.Name AS Location,
	c.Name AS Campus,
	s.Name AS Schedule, 
	p.LastName, 
	p.FirstName,
	ac.Code, 
	ISNULL(legal.Value, '') AS LegalNotes,
	ISNULL(allergy.Value, '') AS AllergyNotes,
	ISNULL(bo.Value, '') AS Breakout
FROM 
	[Person] p 
	JOIN [PersonAlias] pa ON p.Id = pa.PersonId
	JOIN [Attendance] a ON pa.ID = a.PersonAliasId
	JOIN LatestSunday ls ON ls.LatestSunday = CONVERT(DATE, a.StartDateTime)
	JOIN [Schedule] s ON a.ScheduleId = s.Id
	JOIN [AttendanceCode] ac ON a.AttendanceCodeId = ac.Id
	JOIN [Group] g ON a.GroupID = g.Id
	JOIN Campus c ON c.Id = a.CampusId
	JOIN Location l ON l.Id = a.LocationId
	LEFT JOIN [AttributeValue] legal ON p.Id = legal.EntityId AND legal.AttributeId = 715
	LEFT JOIN [AttributeValue] allergy ON p.Id = allergy.EntityId AND allergy.AttributeId = 676
	LEFT JOIN [AttributeValue] bo ON p.Id = bo.EntityId AND bo.AttributeId = 2321
WHERE 
	g.GroupTypeId in (756, 757, 758, 759) 
	AND a.DidAttend = 1
ORDER BY 
	c.Name,
	l.Name, 
	Schedule, 
	p.LastName,
	p.FirstName;