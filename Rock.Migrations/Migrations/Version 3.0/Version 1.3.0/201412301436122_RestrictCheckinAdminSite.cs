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
