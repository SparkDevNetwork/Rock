// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupGeofenceFunctions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
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

CREATE FUNCTION [dbo].[ufnGroup_GetGeofencingGroupNames](
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
" );

            Sql( @"

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
CREATE FUNCTION [dbo].[ufnGroup_GeofencingGroups] (
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
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DROP FUNCTION [dbo].[ufnGroup_GeofencingGroups]
" );
            Sql( @"
    DROP FUNCTION [dbo].[ufnGroup_GetGeofencingGroupNames]
" );

        }
    }
}
