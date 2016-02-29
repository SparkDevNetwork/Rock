/*
<doc>
	<summary>
 		This function returns the name(s) of the groups of selected type that geofence a person 
	</summary>

	<returns>
		Group Names
	</returns>
	<remarks>
		This function allows you to request the group names for those groups of a specific type
		that have one or more locations with a geofence that surrounds any of the given persons
		addresses that have the is mapped location
	</remarks>
	<code>
		SELECT [dbo].[ufnGroup_GetGeofencingGroupNames](2, 24)
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnGroup_GetGeofencingGroupNames](
	@PersonId int, 
	@GroupTypeId int) 

RETURNS nvarchar(max) AS

BEGIN

	DECLARE @Result nvarchar(max)
	DECLARE @FamilyGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' )

	;WITH CTE1 AS 
	(
		SELECT FL.[GeoPoint]
		FROM [GroupMember] M
		INNER JOIN [Group] F ON F.[Id] = M.[GroupId] AND F.[GroupTypeId] = @FamilyGroupTypeId
		INNER JOIN [GroupLocation] FGL ON FGL.[GroupId] = F.[Id] AND FGL.[IsMappedLocation] = 1
		INNER JOIN [Location] FL ON FL.[Id] = FGL.[LocationId] AND FL.[GeoPoint] IS NOT NULL
		WHERE M.[PersonId] = @PersonId
	),
	CTE2 AS
	(
		SELECT G.[Name]
		FROM [Group] G
		INNER JOIN [GroupLocation] GL ON GL.[GroupId] = G.[Id]
		INNER JOIN [Location] L ON L.[Id] = GL.[LocationId] AND L.[GeoFence] IS NOT NULL
		INNER JOIN CTE1 ON ((CTE1.[GeoPoint].STIntersects(L.[GeoFence])) = 1)
		WHERE G.[GroupTypeId] = @GroupTypeId
	)

	SELECT @Result = COALESCE( @Result + ' | ' + [Name], [Name] )
	FROM CTE2
	ORDER BY [Name]

	RETURN COALESCE ( @Result, '' )

END