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
    public partial class RestrictCheckinAdminSite : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Create the Group, Person, PersonAlias, UserLogin, and GroupMember records for the Rock Mobile App to use
            RockMigrationHelper.AddSecurityRoleGroup( "Rock Mobile App", "Security group used by Rock's mobile app to access REST API.", "EDD336D5-1429-41D9-8D41-2581A05F0E16" );

            Sql( @"
    -- Get the 'Rock Mobile App' person ( create if neccessary )
    DECLARE @PersonID int = ( 
	    SELECT TOP 1 P.[Id] 
	    FROM [PersonAlias] PA
	    INNER JOIN [Person] P ON P.[Id] = PA.[PersonId]
	    WHERE PA.[AliasPersonGuid] = '3AD37FBE-0E71-4CF7-B814-DE0F00D4794B' 
    )
    IF @PersonId IS NULL
    BEGIN

	    DECLARE @RecordTypeValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'E2261A84-831D-4234-9BE0-4D628BBE751E' )
	    DECLARE @RecordStatusValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E' )

	    SET @PersonId = ( SELECT [Id] FROM [Person] WHERE [Guid] = '3AD37FBE-0E71-4CF7-B814-DE0F00D4794B' )
	    IF @PersonID IS NULL
	    BEGIN
		    INSERT INTO [Person] ( [IsSystem], [RecordTypeValueId], [RecordStatusValueId], [IsDeceased], [LastName], [Gender], [EmailPreference], [Guid] )
		    VALUES ( 1, @RecordTypeValueId, @RecordStatusValueId, 0, 'Rock Mobile App', 0, 0, '3AD37FBE-0E71-4CF7-B814-DE0F00D4794B' )
		    SET @PersonId = SCOPE_IDENTITY()
	    END

	    INSERT INTO [PersonAlias] ( [PersonId], [AliasPersonId], [AliasPersonGuid], [Guid] )
	    VALUES ( @PersonId, @PersonID, '3AD37FBE-0E71-4CF7-B814-DE0F00D4794B', NEWID() )

    END

    -- Get the 'Rock Mobile App' login ( create if neccessary )
    DECLARE @LoginID int = ( SELECT TOP 1 [Id] FROM [UserLogin] WHERE [Guid] = '39A02D96-553B-40EB-8D0F-008F01E8C963' )
    IF @LoginId IS NULL
    BEGIN

	    DECLARE @DatabaseAuthEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4E9B798F-BB68-4C0E-9707-0928D15AB020' )

	    INSERT INTO [UserLogin] ( [UserName], [IsConfirmed], [ApiKey], [PersonId], [Guid], [EntityTypeId] )
	    VALUES ( NEWID(), 1, 'rnzOT59qMAQTPYtyS9NOcPF0', @PersonId, '39A02D96-553B-40EB-8D0F-008F01E8C963', @DatabaseAuthEntityTypeId )
	    SET @LoginID = SCOPE_IDENTITY()

    END

    -- Get the 'Rock Mobile App' security role ( Should have been created in previous migration step )
    DECLARE @GroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = 'EDD336D5-1429-41D9-8D41-2581A05F0E16' )

    -- Add the person to the role if not already added
    IF NOT EXISTS ( SELECT [Id] FROM [GroupMember] WHERE [GroupId] = @GroupId AND [PersonId] = @PersonID )
    BEGIN

	    DECLARE @GroupRoleId int = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '00F3AC1C-71B9-4EE5-A30E-4C48C8A0BF1F' )

	    INSERT INTO [GroupMember] ( [IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid] )
	    VALUES ( 1, @GroupId, @PersonID, @GroupRoleId, 1, NEWID() )

    END
" );
            // Add security for Mobile App group
            RockMigrationHelper.AddRestController( "PrayerRequests", "Rock.Rest.Controllers.PrayerRequestsController" );
            RockMigrationHelper.AddRestAction( "Groups", "Rock.Rest.Controllers.GroupsController", "GET", "api/Groups/GetFamilies/{personId}" );
            RockMigrationHelper.AddRestAction( "People", "Rock.Rest.Controllers.PeopleController", "GET", "api/People/GetByUserName/{username}" );
            RockMigrationHelper.AddRestAction( "PrayerRequests", "Rock.Rest.Controllers.PrayerRequestsController", "GET", "api/PrayerRequests" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.PrayerRequestsController", 0, "View", true, "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "C87D747A-4921-410B-B12B-6F1D2FE00362" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.PrayerRequestsController", 0, "Edit", true, "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "5544DDEF-B4C6-45FA-9DA1-3CA6A0449229" );
            RockMigrationHelper.AddSecurityAuthForRestAction( "GET", "api/People/GetByUserName/{username}", 0, "View", true, "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "A691B084-F38D-4250-A7BD-C538982FADAB" );
            RockMigrationHelper.AddSecurityAuthForRestAction( "GET", "api/Groups/GetFamilies/{personId}", 0, "View", true, "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "6C171174-E7FC-447C-A415-F1E400F83B72" );
            RockMigrationHelper.AddSecurityAuthForRestAction( "GET", "api/PrayerRequests", 0, "View", false, "EDD336D5-1429-41D9-8D41-2581A05F0E16", Model.SpecialRole.None, "AA6E1E7C-D2D7-4DF0-9197-9385012C55EB" );

            // Secure the Checkin Admin site so that only Rock Admins, Staff, and Staff-Like roles can access the site
            Sql( @"
    DECLARE @SiteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '7244C10B-5D87-467B-A7F5-12DC29910CA8' )
    DECLARE @CheckinAdminSiteId int = ( SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC' )

    IF NOT EXISTS ( 
	    SELECT [Id] 
	    FROM [Auth]
	    WHERE [EntityTypeId] = @SiteEntityTypeId
	    AND [EntityId] = @CheckinAdminSiteId
	    AND [Action] = 'View'
    )
    BEGIN

	    DECLARE @AdminRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    VALUES ( @SiteEntityTypeId, @CheckinAdminSiteId, 0, 'View', 'A', 0, @AdminRoleId, NEWID() )

	    DECLARE @StaffRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' )
	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    VALUES ( @SiteEntityTypeId, @CheckinAdminSiteId, 1, 'View', 'A', 0, @StaffRoleId, NEWID() )

	    DECLARE @StaffLikeRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )
	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    VALUES ( @SiteEntityTypeId, @CheckinAdminSiteId, 2, 'View', 'A', 0, @StaffLikeRoleId, NEWID() )

	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
	    VALUES ( @SiteEntityTypeId, @CheckinAdminSiteId, 3, 'View', 'D', 1, NEWID() )

    END
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
