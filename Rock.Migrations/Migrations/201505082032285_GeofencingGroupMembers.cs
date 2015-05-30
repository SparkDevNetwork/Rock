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
    public partial class GeofencingGroupMembers : Rock.Migrations.RockMigration
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

CREATE FUNCTION [dbo].[ufnGroup_GeofencingGroupMembers](
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
" );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, string.Empty, string.Empty, "Valid Username Caption", "A user-friendly description of a valid username.", 0, "It must only contain letters, numbers, +, -, _, or @. It must also be at least three characters and less than 129.", "BDC62C21-9427-4E20-BC28-98E9EC1E189F", "core.ValidUsernameCaption" );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, string.Empty, string.Empty, "Valid Username Regular Expression", "A regular expression used to validate good, valid usernames.", 0, @"^[A-Za-z0-9+.@_-]{3,128}$", "FE1A18AF-0F3F-4B38-8F1B-158A7E23E81F", "core.ValidUsernameRegularExpression" );

            Sql( @"
    -- Update family members to be active
    UPDATE M SET [GroupMemberStatus] = 1
    FROM [GroupMember] M
    INNER JOIN [Group] G ON G.[Id] = M.[GroupId]
    INNER JOIN [GroupType] T ON T.[Id] = G.[GroupTypeId]
    WHERE T.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'
    AND M.[GroupMemberStatus] = 0
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "BDC62C21-9427-4E20-BC28-98E9EC1E189F" );
            RockMigrationHelper.DeleteAttribute( "FE1A18AF-0F3F-4B38-8F1B-158A7E23E81F" );

            Sql( @"
DROP FUNCTION [dbo].[ufnGroup_GeofencingGroupMembers]
" );
        }
    }
}
