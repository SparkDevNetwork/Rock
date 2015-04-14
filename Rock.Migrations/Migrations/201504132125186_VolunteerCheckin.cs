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
    public partial class VolunteerCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Check In", "A base group type that can be inherited from to support check in without any filters.", "Group", "Member", false, false, false, "", 0, null, 0, "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34", "6E7AD783-7614-4721-ABC1-35842113EF59", false );

            Sql( @"
    DECLARE @CheckInGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '6E7AD783-7614-4721-ABC1-35842113EF59' )
    DECLARE @ServingGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4' )

    -- Update the serving group type to inherit from new 'Check-in' group type
    UPDATE [GroupType] SET 
        [InheritedGroupTypeId] = @CheckInGroupTypeId,
        [AllowMultipleLocations] = 1
    WHERE [Id] = @ServingGroupTypeId
    AND [InheritedGroupTypeId] IS NULL

    -- Add a default 'Member' role to any group type that does not have any roles
    INSERT INTO [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Description], [Order], [IsLeader], [Guid], [CanView], [CanEdit] )
    SELECT 0, [Id], 'Member', 'Member of group', 0, 0, NEWID(), 0, 0 
    FROM [GroupType] WHERE [Id] NOT IN (
        SELECT [GroupTypeId] FROM [GroupTypeRole]
    )

    -- Set the group type's default role to the newly created group type role
	UPDATE T SET [DefaultGroupRoleId] = R.[Id]
	FROM [GroupType] T
	INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id] AND R.[Name] = 'Member'
	WHERE T.[DefaultGroupRoleId] IS NULL

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
