/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spDatamart_NearestGroup
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spDatamart_NearestGroup]
	</code>
</doc>
*/
CREATE PROC [dbo].[_church_ccv_spDatamart_NearestGroup]
AS
TRUNCATE TABLE _church_ccv_Datamart_NearestGroup;

INSERT INTO _church_ccv_Datamart_NearestGroup (
    [FamilyLocationId]
    ,[GroupLocationId]
    ,[Distance]
    )
SELECT fl.Id [Family.LocationId]
    ,gl.Id [NHGroup.LocationId]
    ,gl.Distance
FROM (
    SELECT l.*
    FROM [Location] l
    JOIN GroupLocation gl ON gl.LocationId = l.Id
    JOIN [Group] g ON gl.GroupId = g.Id
    WHERE g.GroupTypeId = 10 -- Family Group Type
        AND l.GeoPoint IS NOT NULL
        AND gl.IsMappedLocation = 1
    ) AS fl
CROSS APPLY
    -- Group Locations
    (
    SELECT TOP (1) l.Id
        ,g.Id [GroupId]
        ,fl.GeoPoint.STDistance(l.GeoPoint) [Distance]
    FROM [Location] l
    JOIN GroupLocation gl ON gl.LocationId = l.Id
    JOIN [Group] g ON gl.GroupId = g.Id
    WHERE g.GroupTypeId = 49 -- Small Group Type
        AND l.GeoPoint IS NOT NULL
        --AND fl.GeoPoint.STDistance(l.GeoPoint) < 160000
    ORDER BY fl.GeoPoint.STDistance(l.GeoPoint)
    ) AS gl OPTION (QUERYTRACEON 4199)

--WITH CTE1
--AS (
--    -- Family Locations
--    SELECT [Id]
--        ,[GeoPoint]
--    FROM [Location]
--    WHERE [Id] IN (
--            SELECT GL.[LocationId]
--            FROM [Group] G
--            INNER JOIN [GroupLocation] GL ON GL.[GroupId] = G.[Id]
--                AND GL.[IsMappedLocation] = 1
--            WHERE G.[GroupTypeId] = 10 -- Family Group Type
--            )
--        AND [GeoPoint] IS NOT NULL
--    )
--    ,CTE2
--AS (
--    -- Group Locations
--    SELECT [Id]
--        ,[GeoPoint]
--    FROM [Location]
--    WHERE [Id] IN (
--            SELECT GL.[LocationId]
--            FROM [Group] G
--            INNER JOIN [GroupLocation] GL ON GL.[GroupId] = G.[Id]
--            WHERE G.[GroupTypeId] = 49 -- Small Group Type
--            )
--        AND [GeoPoint] IS NOT NULL
--    )
--    ,CTE3
--AS (
--    -- Xref between all family locations and all group locations with distance between each
--    SELECT CTE1.[Id] AS [FamilyLocationId]
--        ,CTE2.[Id] AS [GroupLocationId]
--        ,CTE1.[GeoPoint].STDistance(CTE2.GeoPoint) AS [Distance]
--    FROM CTE1
--    INNER JOIN CTE2 ON 1 = 1
--    )
--    ,CTE4
--AS (
--    -- Find the smallest distance for each family location
--    SELECT [FamilyLocationId]
--        ,MIN([Distance]) AS [MinDistance]
--    FROM CTE3
--    GROUP BY [FamilyLocationId]
--    )
---- Find the family/group combination that had that smallest distance and insert into datamart table
--INSERT INTO _church_ccv_Datamart_NearestGroup (
--    [FamilyLocationId]
--    ,[GroupLocationId]
--    ,[Distance]
--    )
--SELECT CTE3.[FamilyLocationId]
--    ,CTE3.[GroupLocationId]
--    ,CTE3.[Distance]
--FROM CTE3
--INNER JOIN CTE4 ON CTE4.[FamilyLocationId] = CTE3.[FamilyLocationId]
--    AND CTE4.[MinDistance] = CTE3.[Distance]