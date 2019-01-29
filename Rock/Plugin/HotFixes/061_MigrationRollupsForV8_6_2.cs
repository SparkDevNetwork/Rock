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
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 61, "1.8.5" )]
    public class MigrationRollupsForV8_6_2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //DeleteAllAuthForTaggedItemsControllerAndTagsController();
            //RemoveAllUserAuthInternalCalendar();
            //AddAuthToPeopleVCardController();
            //UpdateStatementGeneratorInstallerLocation();
            //ReAddMissingVisitorConnectionStatus();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //RestoreAllAuthForTaggedItemsControllerAndTagsController();
            //RestoreCalendarAuth();
            //UndoAddAuthToPeopleVCardController();
        }

        /// <summary>
        /// NA: Re-add Missing Visitor Connection Status (Fixes #3497)
        /// </summary>
        private void ReAddMissingVisitorConnectionStatus()
        {
            // Fixes #3497
            Sql( @"
IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'b91ba046-bc1e-400c-b85d-638c1f4e0ce2' )
BEGIN
    SET IDENTITY_INSERT [dbo].[DefinedValue] ON
    INSERT INTO [dbo].[DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid], [IsActive] ) VALUES (66, 1, 4, 2, N'Visitor', N'Used when a person first enters through your first-time visitor process. As they continue to attend they will become an attendee and possibly a member.', N'b91ba046-bc1e-400c-b85d-638c1f4e0ce2', 1)
    SET IDENTITY_INSERT [dbo].[DefinedValue] OFF
END" );
        }

        /// <summary>
        /// MP: Update StatementGenerator.exe to 1.8.0
        /// </summary>
        private void UpdateStatementGeneratorInstallerLocation()
        {
            // update new location of statementgenerator installer
            Sql( @"
 UPDATE [AttributeValue] 
 SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.8.0/statementgenerator.exe' 
 WHERE [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'
" );
        }

        /// <summary>
        /// Undoes the add authentication to people v card controller.
        /// </summary>
        private void UndoAddAuthToPeopleVCardController()
        {
            // Restore back to previous state by removing new auth rules.
            RockMigrationHelper.DeleteSecurityAuth( "0F59A758-14F5-4F3F-B3D6-E4FCB49BFD15" );
            RockMigrationHelper.DeleteSecurityAuth( "22C50349-F97A-4CAA-8776-AB2B59674A32" );
            RockMigrationHelper.DeleteSecurityAuth( "3614B557-6EAD-4716-AD60-A7DA66C840F7" );
            RockMigrationHelper.DeleteSecurityAuth( "E5BA0C07-5743-4E80-B956-82B361BA131E" );
        }

        /// <summary>
        /// SK: Add auth to the People/GetVCard/ REST controller.
        /// </summary>
        private void AddAuthToPeopleVCardController()
        {
            Sql( @"
IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PeopleController')   INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )   VALUES ( 'People', 'Rock.Rest.Controllers.PeopleController', NEWID() ) 

IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/VCard/{personGuid}')   INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )   SELECT [Id], 'GET', 'GETapi/People/VCard/{personGuid}', 'api/People/VCard/{personGuid}', NEWID()   FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PeopleController' 

INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [CreatedDateTime], [Guid] )   VALUES (   (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'),    (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/VCard/{personGuid}'),    1, 'View', 'A', 0,    (SELECT [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4'), GetDate(), '0F59A758-14F5-4F3F-B3D6-E4FCB49BFD15')  
INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [CreatedDateTime], [Guid] )   VALUES (   (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'),    (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/VCard/{personGuid}'),    2, 'View', 'A', 0,    (SELECT [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745'), GetDate(), '22C50349-F97A-4CAA-8776-AB2B59674A32')  
INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [CreatedDateTime], [Guid] )   VALUES (   (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'),    (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/VCard/{personGuid}'),    0, 'View', 'A', 0,    (SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'), GetDate(), '3614B557-6EAD-4716-AD60-A7DA66C840F7')  
INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [CreatedDateTime], [Guid] )   VALUES (   (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'),    (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/VCard/{personGuid}'),    3, 'View', 'D', 1,    NULL, GetDate(), 'E5BA0C07-5743-4E80-B956-82B361BA131E')  
" );
        }

        /// <summary>
        /// NA: Remove auth for all users from TaggedItemsController and TagsController
        /// </summary>
        public void DeleteAllAuthForTaggedItemsControllerAndTagsController()
        {
            Sql( @"
    DELETE FROM[Auth] WHERE[Guid] = '9D921716-A946-4DEF-9EF9-40A5CD3FFAA4';

    DELETE FROM[Auth] WHERE[Guid] = 'A9576D72-A98B-42AE-8A04-DEDCEEEF0671';

    DELETE FROM[Auth] WHERE[Guid] = '6212376F-B47B-46E1-B5DC-47B317BEC5F5';
" );
        }

        /// <summary>
        /// NA: Remove auth for all users from Internal Calendar by overriding global default inherited
        /// </summary>
        public void RemoveAllUserAuthInternalCalendar()
        {
            // Add View Auth for internal calendar
            RockMigrationHelper.AddSecurityAuthForCalendar( "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798", 0, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "0D6136B4-CCCC-473D-843E-BBBDEEE9BC5B" ); // RSR - Staff Workers
            RockMigrationHelper.AddSecurityAuthForCalendar( "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798", 1, "View", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "093536A0-CCCC-4B27-AE97-CF123F0ED29E" ); // RSR - Staff Like Workers
            RockMigrationHelper.AddSecurityAuthForCalendar( "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798", 2, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "067631D6-CCCC-40E1-A8BD-6AEC733E9104" ); // RSR - Rock Administration
            // All Users - View - Deny
            RockMigrationHelper.AddSecurityAuthForCalendar( "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798", 3, "View", false, null, Model.SpecialRole.AllUsers, "099A70A5-CCCC-4B02-8444-2504E6630CA1" ); // All Users
        }

        /// <summary>
        /// Restores the calendar authentication.
        /// </summary>
        public void RestoreCalendarAuth()
        {
            // Restore back to previous state by removing new auth rules.
            RockMigrationHelper.DeleteSecurityAuth( "0D6136B4-CCCC-473D-843E-BBBDEEE9BC5B" );
            RockMigrationHelper.DeleteSecurityAuth( "093536A0-CCCC-4B27-AE97-CF123F0ED29E" );
            RockMigrationHelper.DeleteSecurityAuth( "067631D6-CCCC-40E1-A8BD-6AEC733E9104" );
            RockMigrationHelper.DeleteSecurityAuth( "099A70A5-CCCC-4B02-8444-2504E6630CA1" );
        }

        /// <summary>
        /// Restores all authentication for tagged items controller and tags controller.
        /// </summary>
        public void RestoreAllAuthForTaggedItemsControllerAndTagsController()
        {
            Sql( @"
            INSERT INTO[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])

    VALUES(
        ( SELECT[Id] FROM[EntityType] WHERE[Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D' ), 
		( SELECT[Id] FROM[RestController] WHERE[ClassName] = 'Rock.Rest.Controllers.TaggedItemsController'), 
		0, 'View', 'A', 1, 
		NULL, 
		'9D921716-A946-4DEF-9EF9-40A5CD3FFAA4')

INSERT INTO[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])

    VALUES(
        ( SELECT[Id] FROM[EntityType] WHERE[Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D' ), 
		( SELECT[Id] FROM[RestController] WHERE[ClassName] = 'Rock.Rest.Controllers.TaggedItemsController'), 
		1, 'Edit', 'A', 1, 
		NULL, 
		'A9576D72-A98B-42AE-8A04-DEDCEEEF0671')

INSERT INTO[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])

    VALUES(
        ( SELECT[Id] FROM[EntityType] WHERE[Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D' ), 
		( SELECT[Id] FROM[RestController] WHERE[ClassName] = 'Rock.Rest.Controllers.TagsController'), 
		0, 'View', 'A', 1, 
		NULL, 
		'6212376F-B47B-46E1-B5DC-47B317BEC5F5')
" );
        }
    }
}
