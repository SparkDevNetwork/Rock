DECLARE @true AS BIT = 1;
DECLARE @false AS BIT = 0;
DECLARE @etidPerson AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Person');
DECLARE @gtFamily AS INT = (SELECT Id FROM GroupType WHERE Name = 'Family');
DECLARE @gtrAdult AS INT = (SELECT Id FROM GroupTypeRole WHERE Name = 'Adult' AND GroupTypeId = @gtFamily);
DECLARE @rstActive AS INT = 3;
DECLARE @minActivityDate AS DATETIME = GETDATE() - 380;
DECLARE @gmsActive AS INT = 1;

WITH people AS (
	SELECT
		g.Id AS FamilyId,
		p.Id AS PersonId,
		pa.Id AS AliasId,
		CONVERT(DATE, p.CreatedDateTime) AS PersonCreatedDate,
		CONVERT(DATE, p.ModifiedDateTime) AS PersonUpdatedDate,
		gm.GroupRoleId AS FamilyRole,
		p.NickName,
		p.LastName,
		p.Email,
		g.Name AS FamilyName,
		p.BirthDate,
		c.Name AS Campus,
		dv.Value AS ConnectionStatus,
		m.Value AS MaritalStatus
	FROM
		GroupMember gm
		JOIN [Group] g ON g.Id = gm.GroupId
		JOIN Person p ON p.Id = gm.PersonId
		JOIN PersonAlias pa ON pa.PersonId = p.Id
		JOIN Campus c ON c.Id = g.CampusId
		LEFT JOIN DefinedValue dv ON dv.Id = p.ConnectionStatusValueId
		LEFT JOIN DefinedValue m ON m.Id = p.MaritalStatusValueId
	WHERE
		g.GroupTypeId = @gtFamily
		AND p.IsDeceased = @false
		AND p.RecordStatusValueId = @rstActive
		AND gm.GroupMemberStatus = @gmsActive
), lastAttendance AS (
	SELECT
		cp.PersonId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
	WHERE
		a.DidAttend = 1
	GROUP BY
		cp.PersonId
), volunteerActivity AS (
	SELECT
		cp.PersonId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		a.DidAttend = 1
		AND gt.Name LIKE '%Volunteer'
	GROUP BY
		cp.PersonId
), participantActivity AS (
	SELECT
		cp.PersonId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		a.DidAttend = 1
		AND gt.Name NOT LIKE '%Volunteer'
	GROUP BY
		cp.PersonId
), givingActivity AS (
	SELECT
		cp.FamilyId,
		MAX(CONVERT(DATE, ft.TransactionDateTime)) AS LastGiftDate
	FROM
		FinancialTransaction ft
		JOIN people cp ON cp.AliasId = ft.AuthorizedPersonAliasId
	GROUP BY
		cp.FamilyId
), kidspring AS (
	SELECT
		cp.FamilyId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		gt.Name IN ('Elementary Attendee', 'Nursery Attendee', 'Preschool Attendee', 'Special Needs Attendee')
	GROUP BY
		cp.FamilyId
), fuse AS (
	SELECT
		cp.FamilyId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		gt.Name IN ('Fuse Group', 'Fuse Attendee')
	GROUP BY
		cp.FamilyId
), staff AS (
	SELECT
		cp.FamilyId,
		1 AS IsStaff
	FROM
		[Group] g
		JOIN GroupMember gm ON gm.GroupId = g.Id
		JOIN people cp ON cp.PersonId = gm.PersonId
	WHERE
		Name = 'RSR - Staff Workers'
		AND IsSecurityRole = 1
		AND gm.GroupMemberStatus = @gmsActive
	GROUP BY
		cp.FamilyId
), care AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, n.CreatedDateTime)) AS CareDate
	FROM
		people p
		JOIN Note n ON n.EntityId = p.PersonId
		JOIN NoteType nt ON nt.Id = n.NoteTypeId
	WHERE
		nt.Name = 'Care Note'
		AND nt.EntityTypeId = @etidPerson
	GROUP BY
		p.PersonId
), baptism AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, av.ValueAsDateTime)) AS BaptismDate
	FROM
		people p
		JOIN AttributeValue av ON p.PersonId = av.EntityId
		JOIN Attribute a ON a.Id = av.AttributeId
	WHERE
		a.EntityTypeId = @etidPerson
		AND a.Name = 'Baptism Date'
	GROUP BY
		p.PersonId
), salavation AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, av.ValueAsDateTime)) AS SalvationDate
	FROM
		people p
		JOIN AttributeValue av ON p.PersonId = av.EntityId
		JOIN Attribute a ON a.Id = av.AttributeId
	WHERE
		a.EntityTypeId = @etidPerson
		AND a.Name = 'Salvation Date'
	GROUP BY
		p.PersonId
), vip AS (
	SELECT
		cp.PersonId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN people cp ON cp.AliasId = a.PersonAliasId
		JOIN [Group] g ON a.GroupId = g.Id
	WHERE
		g.Name = 'VIP Room Attendee'
	GROUP BY
		cp.PersonId
), activityAggregate AS (
	SELECT
		p.PersonId,
		(
			SELECT 
				CONVERT(DATE, MAX(v))
			FROM 
				(VALUES 
					(MAX(la.LastAttendanceDate)),
					(MAX(va.LastAttendanceDate)),
					(MAX(pa.LastAttendanceDate)),
					(MAX(ga.LastGiftDate)),
					(MAX(k.LastAttendanceDate)),
					(MAX(f.LastAttendanceDate)),
					(MAX(c.CareDate)),
					(MAX(b.BaptismDate)),
					(MAX(sal.SalvationDate)),
					(MAX(p.PersonCreatedDate)),
					(MAX(p.PersonUpdatedDate)),
					(MAX(v.LastAttendanceDate)),
					(MAX(CASE WHEN s.IsStaff = 1 THEN GETDATE() END))
				) AS value(v)
		) as MostRecentActivity
	FROM
		people p
		LEFT JOIN lastAttendance la ON la.PersonId = p.PersonId
		LEFT JOIN volunteerActivity va ON va.PersonId = p.PersonId
		LEFT JOIN participantActivity pa ON pa.PersonId = p.PersonId
		LEFT JOIN givingActivity ga ON ga.FamilyId = p.FamilyId
		LEFT JOIN fuse f ON f.FamilyId = p.FamilyId
		LEFT JOIN kidspring k ON k.FamilyId = p.FamilyId
		LEFT JOIN staff s ON p.FamilyId = s.FamilyId
		LEFT JOIN care c ON c.PersonId = p.PersonId
		LEFT JOIN baptism b ON b.PersonId = p.PersonId
		LEFT JOIN salavation sal ON sal.PersonId = p.PersonId
		LEFT JOIN vip v ON v.PersonId = p.PersonId
	GROUP BY
		p.PersonId
)
SELECT
	p.NickName AS [First Name],
	p.LastName AS [Last Name],
	p.Email AS [Email Address],
	p.Campus AS [Campus],
	ISNULL(CONVERT(NVARCHAR(10), p.BirthDate), '') AS [Birthday],
	ISNULL(p.ConnectionStatus, '') AS [Member Status],
	ISNULL(p.MaritalStatus, '') AS [Marital Status],
	ISNULL(CONVERT(NVARCHAR(10), la.LastAttendanceDate), '') AS [Most Recent Attendance],
	ISNULL(CONVERT(NVARCHAR(10), p.PersonCreatedDate), '') AS [First Record Date],
	ISNULL(CONVERT(NVARCHAR(10), p.PersonUpdatedDate), '') AS [Last Updated],
	ISNULL(CONVERT(NVARCHAR(10), va.LastAttendanceDate), '') AS [Last AttendedRoster],
	ISNULL(CONVERT(NVARCHAR(10), aa.MostRecentActivity), '') AS [Most Recent Activity Date],
	ISNULL(CONVERT(NVARCHAR(10), pa.LastAttendanceDate), '') AS [Most Recent Participant Attendance],
	ISNULL(CONVERT(NVARCHAR(10), va.LastAttendanceDate), '') AS [Most Recent Volunteer Attendance],
	ISNULL(CONVERT(NVARCHAR(10), MAX(ga.LastGiftDate)), '') AS [Most Recent Contribution],
	ISNULL(CONVERT(NVARCHAR(10), MAX(f.LastAttendanceDate)), '') AS [Most Recent Fuse Child Attendance Date],
	ISNULL(CONVERT(NVARCHAR(10), MAX(k.LastAttendanceDate)), '') AS [Most Recent KidSpring Child Attendance Date],
	ISNULL(CONVERT(NVARCHAR(10), c.CareDate), '') AS [Most Recent Care Date],
	ISNULL(CONVERT(NVARCHAR(10), v.LastAttendanceDate), '') AS [Most Recent VIP Date],
	CASE WHEN MAX(s.IsStaff) = 1 THEN '1' ELSE '' END AS [Is Staff]
FROM
	people p
	LEFT JOIN lastAttendance la ON la.PersonId = p.PersonId
	LEFT JOIN activityAggregate aa ON aa.PersonId = p.PersonId
	LEFT JOIN volunteerActivity va ON va.PersonId = p.PersonId
	LEFT JOIN participantActivity pa ON pa.PersonId = p.PersonId
	LEFT JOIN givingActivity ga ON ga.FamilyId = p.FamilyId
	LEFT JOIN fuse f ON f.FamilyId = p.FamilyId
	LEFT JOIN kidspring k ON k.FamilyId = p.FamilyId
	LEFT JOIN staff s ON s.FamilyId = p.FamilyId
	LEFT JOIN care c ON c.PersonId = p.PersonId
	LEFT JOIN vip v ON v.PersonId = p.PersonId
WHERE
	p.Email IS NOT NULL
	AND LEN(LTRIM(RTRIM(p.Email))) > 0
	AND (p.BirthDate IS NULL OR p.BirthDate <= DATEADD(YEAR, -18, GETDATE()))
	AND p.FamilyRole = @gtrAdult
	AND aa.MostRecentActivity > @minActivityDate
GROUP BY
	p.PersonId,
	p.NickName,
	p.LastName,
	p.Email,
	p.Campus,
	p.BirthDate,
	p.ConnectionStatus,
	p.MaritalStatus,
	la.LastAttendanceDate,
	aa.MostRecentActivity,
	p.PersonCreatedDate,
	p.PersonUpdatedDate,
	va.LastAttendanceDate,
	pa.LastAttendanceDate,
	c.CareDate,
	v.LastAttendanceDate
ORDER BY
	p.LastName,
	p.NickName,
	p.PersonId;