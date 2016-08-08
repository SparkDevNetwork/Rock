SELECT 
	gpc.Name,
	pc.Name, 
	c.Name, 
	m.*
FROM 
	Metric m 
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN Category c ON c.Id = mc.CategoryId
	LEFT JOIN Category pc ON pc.Id = c.ParentCategoryId
	LEFT JOIN Category gpc ON gpc.Id = pc.ParentCategoryId
WHERE 
	(m.SourceSql IS NOT NULL AND LEN(RTRIM(LTRIM(m.SourceSql))) > 0)
	AND (gpc.Name IS NULL OR gpc.Name <> 'Volunteers')
	AND m.Title NOT LIKE '% Attendees'
	AND m.Title NOT LIKE '% Attendance'
	AND m.Title NOT LIKE '% Assignments';

/*************************************
 * Avg Fuse Group Size
*************************************/
DECLARE @GroupMemberStatusActive int = 1;

SELECT
	AVG(sub.NumMembers) as Value
	, sub.CampusId AS EntityId
	, DATEADD(dd, DATEDIFF(dd, 1, GETDATE()), 0) + '00:00' AS ScheduleDate
FROM 
	(
		SELECT 
			COUNT(DISTINCT gm.PersonId) AS NumMembers
			, g.CampusId
		FROM
			[GroupMember] gm
			INNER JOIN [Group] g ON gm.GroupId = g.Id
			INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.Id
		WHERE
			gt.Name = 'Fuse Group'
			AND g.IsActive = 1
			AND g.ParentGroupId IS NOT NULL
			AND gm.GroupMemberStatus = @GroupMemberStatusActive
		GROUP BY
			g.Id, g.CampusId
	) sub
GROUP BY
	sub.CampusId;
