DECLARE @emailAllowed AS INT = 0;
DECLARE @true AS INT = 1;
DECLARE @false AS INT = 0;
DECLARE @etidPerson AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Person');
DECLARE @gtFamily AS INT = (SELECT Id FROM GroupType WHERE Name = 'Family');
DECLARE @gtrAdult AS INT = (SELECT Id FROM GroupTypeRole WHERE Name = 'Adult' AND GroupTypeId = @gtFamily);
DECLARE @rstActive AS INT = 3;
DECLARE @minActivityDate AS DATETIME = GETDATE() - 380;
DECLARE @gmsActive AS INT = 1;

-- Create a temp table where the data will be compiled
IF OBJECT_ID('tempdb..#MailChimpExport') IS NOT NULL DROP TABLE #MailChimpExport;

CREATE TABLE #MailChimpExport (
	[PersonId] INT,
	[FamilyId] INT,
	[AdultIndex] INT,
	[First Name] NVARCHAR(30),
	[Last Name] NVARCHAR(30),
	[Email Address] NVARCHAR(100),
	[Campus] NVARCHAR(30),
	[Birthday] DATE,
	[Member Status] NVARCHAR(20),
	[Marital Status] NVARCHAR(20),
	[Most Recent Attendance] DATE,
	[First Record Date] DATE,
	[Last Updated] DATE,
	[Last AttendedRoster] DATE,
	[Most Recent Activity Date] DATE,
	[Most Recent Participant Attendance] DATE,
	[Most Recent Volunteer Attendance] DATE,
	[Most Recent Contribution] DATE,
	[Most Recent Fuse Child Attendance Date] DATE,
	[Most Recent KidSpring Child Attendance Date] DATE,
	[Most Recent Care Date] DATE,
	[Most Recent VIP Date] DATE,
	[Baptism Date] DATE,
	[Salvation Date] DATE,
	[Is Staff] BIT,
	[Keep me updated] NVARCHAR(100)
);

-- All active, living adults with emails are included in the export
WITH cteFamily AS (
	SELECT
		MAX(g.Id) AS FamilyId,
		gm.PersonId
	FROM
		GroupMember gm
		JOIN [Group] g ON g.Id = gm.GroupId
	WHERE
		g.GroupTypeId = @gtFamily
		AND gm.GroupRoleId = @gtrAdult
	GROUP BY
		gm.PersonId
)
INSERT INTO #MailChimpExport (
	[FamilyId],
	[PersonId],
	[Campus],
	[AdultIndex],
	[First Name],
	[Last Name],
	[Email Address],
	[Birthday],
	[Member Status],
	[Marital Status]
)
SELECT
	gm.FamilyId,
	gm.PersonId,
	c.Name,
	ROW_NUMBER() OVER (PARTITION BY gm.FamilyId ORDER BY p.Gender) AS AdultIndex,
	p.NickName,
	p.LastName,
	p.Email,
	p.Birthdate,
	dv.Value AS MemberStatus,
	m.Value AS MaritalStatus
FROM
	Person p
	JOIN cteFamily gm ON gm.PersonId = p.Id
	JOIN [Group] g ON g.Id = gm.FamilyId
	LEFT JOIN Campus c ON c.Id = g.CampusId
	LEFT JOIN DefinedValue dv ON dv.Id = p.ConnectionStatusValueId
	LEFT JOIN DefinedValue m ON m.Id = p.MaritalStatusValueId
WHERE
	g.GroupTypeId = @gtFamily
	AND p.IsDeceased = @false
	AND p.RecordStatusValueId = @rstActive
	AND p.IsSystem = @false
	AND p.Email IS NOT NULL
	AND p.IsEmailActive = @true
	AND p.EmailPreference = @emailAllowed
	AND LEN(RTRIM(LTRIM(p.Email))) >= 3 -- x@x
	AND (p.BirthDate IS NULL OR p.BirthDate <= DATEADD(YEAR, -18, GETDATE()));

-- Update family dates
WITH cteMaxCreated AS (
	SELECT
		mce.FamilyId,
		MAX(p.CreatedDateTime) AS MaxCreatedDate,
		MAX(p.ModifiedDateTime) AS MaxModifiedDate
	FROM
		#MailChimpExport mce
		JOIN GroupMember gm ON gm.GroupId = mce.FamilyId
		JOIN Person p ON p.Id = gm.PersonId
	GROUP BY
		mce.FamilyId
)
UPDATE mce
SET
	mce.[First Record Date] = cte.MaxCreatedDate,
	mce.[Last Updated] = cte.MaxModifiedDate
FROM 
	#MailChimpExport mce
	JOIN cteMaxCreated cte ON mce.FamilyId = cte.FamilyId;

-- Family giving
WITH cteGiving AS (
	SELECT
		mce.FamilyId,
		MAX(ft.TransactionDateTime) AS MaxDate
	FROM
		#MailChimpExport mce
		JOIN GroupMember gm ON gm.GroupId = mce.FamilyId
		JOIN PersonAlias pa ON pa.PersonId = gm.PersonId
		JOIN FinancialTransaction ft ON ft.AuthorizedPersonAliasId = pa.Id
	WHERE
		ft.TransactionDateTime >= @minActivityDate
	GROUP BY
		mce.FamilyId
)
UPDATE mce
SET
	[Most Recent Contribution] = g.MaxDate
FROM
	#MailChimpExport mce
	JOIN cteGiving g ON g.FamilyId = mce.FamilyId;

-- Personal attendance dates
WITH cteAttendances AS (
	SELECT
		a.*,
		p.PersonId,
		gt.Name AS GroupType
	FROM
		Attendance a
		JOIN PersonAlias pa ON pa.Id = a.PersonAliasId
		JOIN #MailChimpExport p ON p.PersonId = pa.PersonId
		JOIN [Group] g ON g.Id = a.GroupId
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		a.DidAttend = @true
		AND a.StartDateTime >= @minActivityDate
), cteParticipant AS (
	SELECT
		a.PersonId,
		MAX(a.StartDateTime) AS MaxDate
	FROM
		cteAttendances a
	WHERE
		a.GroupType NOT LIKE '%Volunteer'
	GROUP BY
		a.PersonId
), cteVolunteer AS (
	SELECT
		a.PersonId,
		MAX(a.StartDateTime) AS MaxDate
	FROM
		cteAttendances a
	WHERE
		a.GroupType LIKE '%Volunteer'
	GROUP BY
		a.PersonId
), cteAll AS (
	SELECT
		a.PersonId,
		MAX(a.StartDateTime) AS MaxDate
	FROM
		cteAttendances a
	GROUP BY
		a.PersonId
)
UPDATE mce
SET
	[Most Recent Volunteer Attendance] = v.MaxDate,
	[Last AttendedRoster] = v.MaxDate,
	[Most Recent Participant Attendance] = p.MaxDate,
	[Most Recent Attendance] = a.MaxDate
FROM
	#MailChimpExport mce
	LEFT JOIN cteParticipant p ON p.PersonId = mce.PersonId
	LEFT JOIN cteVolunteer v ON v.PersonId = mce.PersonId
	LEFT JOIN cteAll a ON a.PersonId = mce.PersonId;

-- Family attendance dates
WITH cteAttendances AS (
	SELECT
		a.*
	FROM
		Attendance a
	WHERE
		a.DidAttend = @true
		AND a.StartDateTime >= @minActivityDate
), cteKidSpring AS (
	SELECT
		mce.FamilyId,
		MAX(a.StartDateTime) AS MaxDate
	FROM
		#MailChimpExport mce
		JOIN GroupMember gm ON gm.GroupId = mce.FamilyId
		JOIN PersonAlias pa ON pa.PersonId = gm.PersonId
		JOIN cteAttendances a ON a.PersonAliasId = pa.Id
		JOIN [Group] g ON g.Id = a.GroupId
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		gt.Name IN ('Elementary Attendee', 'Nursery Attendee', 'Preschool Attendee', 'Special Needs Attendee')
	GROUP BY
		mce.FamilyId
), cteFuse AS (
	SELECT
		mce.FamilyId,
		MAX(a.StartDateTime) AS MaxDate
	FROM
		#MailChimpExport mce
		JOIN GroupMember gm ON gm.GroupId = mce.FamilyId
		JOIN PersonAlias pa ON pa.PersonId = gm.PersonId
		JOIN cteAttendances a ON a.PersonAliasId = pa.Id
		JOIN [Group] g ON g.Id = a.GroupId
		JOIN GroupType gt ON gt.Id = g.GroupTypeId
	WHERE
		gt.Name IN ('Fuse Attendee', 'Fuse Group')
	GROUP BY
		mce.FamilyId
)
UPDATE mce
SET
	[Most Recent KidSpring Child Attendance Date] = ks.MaxDate,
	[Most Recent Fuse Child Attendance Date] = f.MaxDate
FROM
	#MailChimpExport mce
	LEFT JOIN cteKidSpring ks ON ks.FamilyId = mce.FamilyId
	LEFT JOIN cteFuse f ON f.FamilyId = mce.FamilyId;

-- Family attribute dates
WITH cteCare AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, n.CreatedDateTime)) AS CareDate
	FROM
		#MailChimpExport p
		JOIN Note n ON n.EntityId = p.PersonId
		JOIN NoteType nt ON nt.Id = n.NoteTypeId
	WHERE
		nt.Name = 'Care Note'
		AND nt.EntityTypeId = @etidPerson
	GROUP BY
		p.PersonId
), cteBaptism AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, av.ValueAsDateTime)) AS BaptismDate
	FROM
		#MailChimpExport p
		JOIN AttributeValue av ON p.PersonId = av.EntityId
		JOIN Attribute a ON a.Id = av.AttributeId
	WHERE
		a.EntityTypeId = @etidPerson
		AND a.Name = 'Baptism Date'
	GROUP BY
		p.PersonId
), cteSalvation AS (
	SELECT
		p.PersonId,
		MAX(CONVERT(DATE, av.ValueAsDateTime)) AS SalvationDate
	FROM
		#MailChimpExport p
		JOIN AttributeValue av ON p.PersonId = av.EntityId
		JOIN Attribute a ON a.Id = av.AttributeId
	WHERE
		a.EntityTypeId = @etidPerson
		AND a.Name = 'Salvation Date'
	GROUP BY
		p.PersonId
), cteVip AS (
	SELECT
		cp.PersonId,
		MAX(CONVERT(DATE, a.StartDateTime)) AS LastAttendanceDate
	FROM
		Attendance a
		JOIN PersonAlias pa ON pa.Id = a.PersonAliasId
		JOIN #MailChimpExport cp ON pa.PersonId = cp.PersonId
		JOIN [Group] g ON a.GroupId = g.Id
	WHERE
		g.Name = 'VIP Room Attendee'
	GROUP BY
		cp.PersonId
)
UPDATE mce
SET
	[Most Recent VIP Date] = v.LastAttendanceDate,
	[Most Recent Care Date] = c.CareDate,
	[Baptism Date] = b.BaptismDate,
	[Salvation Date] = s.SalvationDate
FROM
	#MailChimpExport mce
	LEFT JOIN cteBaptism b ON b.PersonId = mce.PersonId
	LEFT JOIN cteSalvation s ON s.PersonId = mce.PersonId
	LEFT JOIN cteVip v ON v.PersonId = mce.PersonId
	LEFT JOIN cteCare c ON c.PersonId = mce.PersonId;

-- Is Staff
WITH cteStaff AS (
	SELECT
		mce.FamilyId
	FROM
		#MailChimpExport mce
		JOIN GroupMember fm ON fm.GroupId = mce.FamilyId
		JOIN GroupMember gm ON gm.PersonId = fm.PersonId
		JOIN [Group] g ON g.Id = gm.GroupId 
	WHERE
		g.Name = 'RSR - Staff Workers'
		AND g.IsSecurityRole = 1
		AND gm.GroupMemberStatus = @gmsActive
	GROUP BY
		mce.FamilyId
)
UPDATE mce
SET
	[Is Staff] = 1
FROM
	#MailChimpExport mce
	JOIN cteStaff s ON s.FamilyId = mce.FamilyId;

-- Is N2K
WITH cteN2K AS (
	SELECT
		mce.PersonId
	FROM
		#MailChimpExport mce
		JOIN GroupMember gm ON gm.PersonId = mce.PersonId
		JOIN [Group] g ON g.Id = gm.GroupId 
	WHERE
		g.Name = 'N2K'
		AND gm.GroupMemberStatus = @gmsActive
)
UPDATE mce
SET
	[Keep me updated] = 'Need to Know Newsletter'
FROM
	#MailChimpExport mce
	JOIN cteN2K s ON s.PersonId = mce.PersonId;

-- Aggregate all dates into the most recent activity date
UPDATE #MailChimpExport
SET
	[Most Recent Activity Date] = (
		SELECT 
			CONVERT(DATE, MAX(v))
		FROM 
			(VALUES 
				([Last AttendedRoster]),
				([First Record Date]),
				([Last Updated]),
				([Last AttendedRoster]),
				([Most Recent Activity Date]),
				([Most Recent Participant Attendance]),
				([Most Recent Volunteer Attendance]),
				([Most Recent Contribution]),
				([Most Recent Fuse Child Attendance Date]),
				([Most Recent KidSpring Child Attendance Date]),
				([Most Recent Care Date]),
				([Most Recent VIP Date]),
				([Baptism Date]),
				([Salvation Date]),
				(CASE WHEN [Is Staff] = 1 THEN GETDATE() END),
				(CASE WHEN [Keep me updated] IS NOT NULL THEN GETDATE() END)
			) AS value(v)
	);

-- Print data to screen for export
SELECT 
	mce.AdultIndex,
	mce.[First Name],
	mce.[Last Name],
	mce.[Email Address],
	mce.Campus,
	mce.Birthday,
	mce.[Member Status],
	mce.[Marital Status],
	mce.[Most Recent Activity Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Attendance]), '') AS [Most Recent Attendance],
	ISNULL(CONVERT(NVARCHAR(10), mce.[First Record Date]), '') AS [First Record Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Last Updated]), '') AS [Last Updated],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Last AttendedRoster]), '') AS [Last AttendedRoster],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Participant Attendance]), '') AS [Most Recent Participant Attendance],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Volunteer Attendance]), '') AS [Most Recent Volunteer Attendance],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Contribution]), '') AS [Most Recent Contribution],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Fuse Child Attendance Date]), '') AS [Most Recent Fuse Child Attendance Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent KidSpring Child Attendance Date]), '') AS [Most Recent KidSpring Child Attendance Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent Care Date]), '') AS [Most Recent Care Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Most Recent VIP Date]), '') AS [Most Recent VIP Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Baptism Date]), '') AS [Baptism Date],
	ISNULL(CONVERT(NVARCHAR(10), mce.[Salvation Date]), '') AS [Salvation Date],
	CASE WHEN mce.[Is Staff] = 1 THEN '1' ELSE '' END AS [Is Staff],
	ISNULL(mce.[Keep me updated], '') AS [Lists]
FROM 
	#MailChimpExport mce
WHERE
	mce.[Most Recent Activity Date] >= @minActivityDate
ORDER BY
	mce.[Last Name],
	mce.FamilyId,
	mce.AdultIndex;
