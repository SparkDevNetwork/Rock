/*
<doc>
	<summary>
 		This function returns the groupmembers of selected group type that geofences a person 
	</summary>

	<returns>
		Group Names
	</returns>
	<remarks>
		This function allows you to request the groupmebmers for those groups of a specific type
		that have one or more locations with a geofence that surrounds any of the given persons
		addresses that have the is mapped location
	</remarks>
	<code>
		SELECT [dbo].[ufnGroup_GetGeofencingGroupMembers](2, 24, 26)
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnGroup_GeofencingGroupMembers](
	@PersonId int, 
	@GroupTypeId int,
	@GroupTypeRoleId int
) 
RETURNS TABLE AS

RETURN 
(
	WITH CTE1 AS 
	(
		SELECT FL.[GeoPoint] AS [GeoPoint]
		FROM [GroupMember] M
		INNER JOIN [Group] F ON F.[Id] = M.[GroupId] 
		INNER JOIN [GroupType] FT ON FT.[Id] = F.[GroupTypeId]
		INNER JOIN [GroupLocation] FGL ON FGL.[GroupId] = F.[Id] AND FGL.[IsMappedLocation] = 1
		INNER JOIN [Location] FL ON FL.[Id] = FGL.[LocationId] AND FL.[GeoPoint] IS NOT NULL
		WHERE M.[PersonId] = @PersonId
		AND FT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'	-- Family
	)

	SELECT M.*
	FROM [GroupMember] M
	INNER JOIN [Group] G ON G.[Id] = M.[GroupId]
	INNER JOIN [GroupLocation] GL ON GL.[GroupId] = G.[Id]
	INNER JOIN [Location] L ON L.[Id] = GL.[LocationId] AND L.[GeoFence] IS NOT NULL
	INNER JOIN CTE1 ON ((CTE1.[GeoPoint].STIntersects(L.[GeoFence])) = 1)
	WHERE G.[GroupTypeId] = @GroupTypeId
	AND M.[GroupRoleId] = @GroupTypeRoleId
)