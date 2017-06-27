/* ====================================================== */
-- Calculate Grades for KidSpring Elementary and Fuse Attendees

-- Assumes grade transition date has been set in the global attributes.  If today is after 
-- grade transition date, grad year is next year since school years are split (start senior 
-- year 2015, graduate 2016). So if today is December 2015, a fourth grader will graduate 
-- 2024. If today is January 2016, a fourth grader will still graduate 2024.  This is why 
-- we add 1 to currentGradYear when month is after the grade transition date. Fourth grade
-- defined value is 8. 2016 + 8 = 2024.

/* ====================================================== */

-- Should be obvious
DECLARE @today AS DATE = GETDATE();
DECLARE @currentYear AS INT = YEAR(@today);
DECLARE @elementaryGroupId AS INT = (SELECT Id FROM [Group] WHERE Name = 'Elementary Attendee');
DECLARE @fuseGroupId AS INT = (SELECT Id FROM [Group] WHERE Name = 'Fuse Attendee');
DECLARE @groupEntityTypeId AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Group');
DECLARE @gradeRangeAttributeId AS INT = (SELECT Id FROM Attribute WHERE Name = 'Grade Range' AND EntityTypeId = @groupEntityTypeId);

-- Returns something like `09-01`
DECLARE @gradeTransition AS NVARCHAR(10) = (SELECT Value FROM Attribute a LEFT JOIN AttributeValue av ON av.AttributeId = a.Id WHERE a.EntityTypeId IS NULL AND a.[Key] = 'GradeTransitionDate');

-- Returns 9
DECLARE @gtMonth AS INT = SUBSTRING(@gradeTransition, 0, PATINDEX('%/%', @gradeTransition));

-- Returns 1
DECLARE @gtDay AS INT = SUBSTRING(@gradeTransition, PATINDEX('%/%', @gradeTransition) + 1, LEN(@gradeTransition));

-- Returns date of 2016-09-01
DECLARE @gradeTransitionDate AS DATE = CONCAT(@currentYear, '-', @gtMonth, '-', @gtDay);

-- If today is after transition (fall semester), graduation (after spring semester) is next year
DECLARE @currentGradYear AS INT = @currentYear + CASE WHEN @today > @gradeTransitionDate THEN 1 ELSE 0 END;

-- If today is before grade transition date, then the most recent transition date was last year (2015-09-01)
SELECT @gradeTransitionDate = CASE WHEN @today < @gradeTransitionDate THEN CONCAT(@currentYear - 1, '-', @gtMonth, '-', @gtDay) ELSE @gradeTransitionDate END;

DECLARE @msg nvarchar(max) = 'Updating graduation dates for attendances past ' + @gradeTransitionDate
RAISERROR ( @msg, 0, 0 ) WITH NOWAIT

WITH RecentCheckins AS (
	SELECT
		a.*
	FROM
		Attendance a
		JOIN [Group] g ON a.GroupId = g.Id
	WHERE
		a.StartDateTime > @gradeTransitionDate
		AND (g.ParentGroupId = @elementaryGroupId OR g.ParentGroupId = @fuseGroupId)
		AND g.Name <> 'Base Camp'
), LastCheckin AS (
	SELECT 
		t1.* 
	FROM 
		RecentCheckins t1
		JOIN (
			SELECT PersonAliasId, MAX(StartDateTime) AS MaxDate
			FROM RecentCheckins
			GROUP BY PersonAliasId
		) t2 ON t1.PersonAliasId = t2.PersonAliasId AND t1.StartDateTime = t2.MaxDate
)
UPDATE
	p
SET
	p.GraduationYear = @currentGradYear + CONVERT(INT, dv.Value)
FROM 
	LastCheckin lci
	JOIN Attendance a ON a.Id = lci.Id
	JOIN PersonAlias pa ON a.PersonAliasId = pa.Id
	JOIN Person p ON pa.PersonId = p.Id
	JOIN [Group] g ON a.GroupId = g.Id
	JOIN AttributeValue av ON av.EntityId = g.Id
	JOIN DefinedValue dv ON dv.[Guid] LIKE SUBSTRING(av.Value, 0, PATINDEX('%,%', av.Value))
WHERE 
	(g.ParentGroupId = @elementaryGroupId OR g.ParentGroupId = @fuseGroupId)
	AND av.AttributeId = @gradeRangeAttributeId;

SELECT @msg = 'Completed successfully'
RAISERROR ( @msg, 0, 0 ) WITH NOWAIT
