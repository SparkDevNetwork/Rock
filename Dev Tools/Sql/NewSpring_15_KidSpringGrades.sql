/* ====================================================== */
-- Calculate Grades for KidSpring Elementary Attendees

-- Assumes move up Sunday occurs after August. If today is after august, grad year is next
-- year since school years are split (start senior year 2015, graduate 2016). So if today is
-- December 2015, a fourth grader will graduate 2024. If today is January 2016, a fourth grader 
-- will still graduate 2024.  This is why we add 1 to currentGradYear when month is after
-- August. Fourth grade defined value is 8. 2016 + 8 = 2024.

/* ====================================================== */

DECLARE @august AS INT = 8;
DECLARE @minAttendanceDate AS DATE = '2015-09-01';
DECLARE @elementaryGroupId AS INT = (SELECT Id FROM [Group] WHERE Name = 'Elementary Attendee');
DECLARE @groupEntityTypeId AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Group');
DECLARE @gradeRangeAttributeId AS INT = (SELECT Id FROM Attribute WHERE Name = 'Grade Range' AND EntityTypeId = @groupEntityTypeId);
DECLARE @currentGradYear AS INT = YEAR(GETDATE()) + CASE WHEN MONTH(GETDATE()) > @august THEN 1 ELSE 0 END;

WITH LastCheckin AS (
	SELECT
		MAX(a.Id) AS AttendanceId
	FROM
		Attendance a
		JOIN [Group] g ON a.GroupId = g.Id
	WHERE
		a.StartDateTime > @minAttendanceDate
		AND g.ParentGroupId = @elementaryGroupId
	GROUP BY
		a.PersonAliasId
)
UPDATE
	p
SET
	p.GraduationYear = @currentGradYear + CONVERT(INT, dv.Value)
FROM 
	LastCheckin lci
	JOIN Attendance a ON a.Id = lci.AttendanceId
	JOIN PersonAlias pa ON a.PersonAliasId = pa.Id
	JOIN Person p ON pa.PersonId = p.Id
	JOIN [Group] g ON a.GroupId = g.Id
	JOIN AttributeValue av ON av.EntityId = g.Id
	JOIN DefinedValue dv ON dv.[Guid] LIKE SUBSTRING(av.Value, 0, PATINDEX('%,%', av.Value))
WHERE 
	ParentGroupId = @elementaryGroupId
	AND av.AttributeId = @gradeRangeAttributeId;