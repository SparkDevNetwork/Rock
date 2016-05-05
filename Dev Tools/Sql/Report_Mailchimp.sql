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
	[Is Staff] BIT
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

-- Attendance dates
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
				(CASE WHEN [Is Staff] = 1 THEN GETDATE() END)
			) AS value(v)
	);

-- Print data to screen for export
SELECT 
	* 
FROM 
	#MailChimpExport
WHERE
	[Most Recent Activity Date] >= @minActivityDate;