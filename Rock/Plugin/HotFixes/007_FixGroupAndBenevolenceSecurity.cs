﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 7, "1.5.0" )]
    public class FixGroupAndBenevolenceSecurity : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//  Moved to core migration: 201612121647292_HotFixesFrom6_1
//            Sql( @"
//    DECLARE @GroupEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '9BBFDA11-0D22-40D5-902F-60ADFBC88987' )
//    DECLARE @BinaryFileEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '62AF597F-F193-412B-94EA-291CF713327D')
    
//    DECLARE @BackgroundCheckFileTypeId int = (SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = '5C701472-8A6B-4BBE-AEC6-EC833C859F2D')

//    DECLARE @AdminRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
//	DECLARE @StaffRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' )
//	DECLARE @StaffLikeRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )
//    DECLARE @SafetyRoleId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB')

//    IF NOT EXISTS ( 
//	    SELECT [Id] 
//	    FROM [Auth]
//	    WHERE [EntityTypeId] = @GroupEntityTypeId
//	    AND [EntityId] = 0
//	    AND [Action] = 'View'
//    )
//    BEGIN

//	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
//	    VALUES ( @GroupEntityTypeId, 0, 0, 'View', 'A', 0, @AdminRoleId, NEWID() )

//	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
//	    VALUES ( @GroupEntityTypeId, 0, 1, 'View', 'A', 0, @StaffRoleId, NEWID() )

//	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
//	    VALUES ( @GroupEntityTypeId, 0, 2, 'View', 'A', 0, @StaffLikeRoleId, NEWID() )

//	    INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
//	    VALUES ( @GroupEntityTypeId, 0, 3, 'View', 'D', 1, NEWID() )

//    END

//    IF NOT EXISTS ( 
//	    SELECT [Id] 
//	    FROM [Auth]
//	    WHERE [EntityTypeId] = @BinaryFileEntityTypeId
//	    AND [EntityId] = @BackgroundCheckFileTypeId
//	    AND [Action] = 'View'
//    )
//    BEGIN

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'View', 'A', 0, @SafetyRoleId, newid())

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'View', 'A', 0, @AdminRoleId, newid())

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'View', 'D', 1, null, newid())

//    END

//    IF NOT EXISTS ( 
//	    SELECT [Id] 
//	    FROM [Auth]
//	    WHERE [EntityTypeId] = @BinaryFileEntityTypeId
//	    AND [EntityId] = @BackgroundCheckFileTypeId
//	    AND [Action] = 'Edit'
//    )
//    BEGIN

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'Edit', 'A', 0, @SafetyRoleId, newid())

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'Edit', 'A', 0, @AdminRoleId, newid())

//        INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
//        VALUES (@BinaryFileEntityTypeId, @BackgroundCheckFileTypeId, 0, 'Edit', 'D', 1, null, newid())

//    END
//" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
