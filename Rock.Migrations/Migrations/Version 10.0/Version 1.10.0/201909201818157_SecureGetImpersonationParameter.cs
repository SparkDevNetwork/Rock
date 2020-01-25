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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SecureGetImpersonationParameter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateRestSecurity();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// ED: Restrict GetImpersonationParameter API Endpoint to Rock Administrators
        /// </summary>
        private void UpdateRestSecurity()
        {
            Sql( @"
                IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PeopleController') 
                BEGIN
                    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
                    VALUES ( 'People', 'Rock.Rest.Controllers.PeopleController', NEWID() )
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/GetImpersonationParameter?personId={personId}&expireDateTime={expireDateTime}&usageLimit={usageLimit}&pageId={pageId}')
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                    SELECT [Id], 'GET', 'GETapi/People/GetImpersonationParameter?personId={personId}&expireDateTime={expireDateTime}&usageLimit={usageLimit}&pageId={pageId}', 'api/People/GetImpersonationParameter?personId={personId}&expireDateTime={expireDateTime}&usageLimit={usageLimit}&pageId={pageId}', NEWID()
                    FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PeopleController'
                END

                DECLARE @RestActionEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D')
                DECLARE @GetImpersonationParameterRestActionId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/GetImpersonationParameter?personId={personId}&expireDateTime={expireDateTime}&usageLimit={usageLimit}&pageId={pageId}')
                DECLARE @RockAdminSecurityGroupId INT = (SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E')

                -- There is already user defined security on this don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @GetImpersonationParameterRestActionId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
                    VALUES
                        (@RestActionEntityTypeId, @GetImpersonationParameterRestActionId, 0, 'View', 'A', 0, @RockAdminSecurityGroupId, '28261071-2ACB-4C7A-AA14-CAC9F6F18105'),
                        (@RestActionEntityTypeId, @GetImpersonationParameterRestActionId, 0, 'Edit', 'A', 0, @RockAdminSecurityGroupId, '648C68AE-F470-4A61-A36F-A90F21A17EA5'),
                        (@RestActionEntityTypeId, @GetImpersonationParameterRestActionId, 1, 'View', 'D', 1, NULL, 'C379B102-55A9-4E05-AC2A-49D4120611A4'),
                        (@RestActionEntityTypeId, @GetImpersonationParameterRestActionId, 1, 'Edit', 'D', 1, NULL, 'B5FF3548-5BBA-49B8-950A-A9FCED82C019')
                END" );
        }
    }
}
