WITH CTE
AS
(
	SELECT 
		Id,
		CASE UserOrRoleName
			WHEN '*AU' THEN 1
			WHEN '*AAU' THEN 2
			WHEN '*AUU' THEN 3
			ELSE 0 END as SpecialRole,
		CASE WHEN UserOrRoleName not like '*%' AND UserOrRole = 'U' THEN UserOrRoleName ELSE null END as PersonGuid,
		CASE WHEN UserOrRoleName not like '*%' AND UserOrRole = 'R' THEN UserOrRoleName ELSE null END as GroupGuid
	from cmsAuth
)

UPDATE A SET
	SpecialRole = CTE.SpecialRole,
	PersonId = P.Id,
	GroupId = G.Id
FROM CTE
INNER JOIN cmsAuth A
	ON A.Id = CTE.Id
LEFT OUTER JOIN crmPerson P
	ON P.Guid = CTE.PersonGuid
LEFT OUTER JOIN GroupsGroup G
	ON G.Guid = CTE.GroupGuid
	