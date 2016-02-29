/*
<doc>
	<summary>
 		This function returns all the groups of a selected type that geofence the 
        geopoint of the given location
	</summary>

	<returns>
		LocationId, Group.*
	</returns>
    <param name='@LocationId' datatype='int'>The location id to use for the geopoint </param>
	<param name='@GroupTypeId' datatype='int'>The Group type to use for finding groups</param>
	<code>
		SELECT * FROM [dbo].[ufnGroup_GeofencingGroups](14, 24)
	</code>
</doc>
*/
ALTER FUNCTION [dbo].[ufnGroup_GeofencingGroups] (
     @LocationId INT
    ,@GroupTypeId INT
    )
RETURNS TABLE AS

RETURN 
	SELECT	
		  L2.[Id] AS [LocationId]
		, G.*
	FROM [Group] G
	INNER JOIN [GroupLocation] GL ON GL.[GroupId] = G.[Id]
	INNER JOIN [Location] L ON L.[Id] = GL.[LocationId] AND L.[GeoFence] IS NOT NULL
	INNER JOIN [Location] L2 ON L2.[Id] = @LocationId AND ((L2.[GeoPoint].STIntersects(L.[GeoFence])) = 1)
	WHERE G.[GroupTypeId] = @GroupTypeId