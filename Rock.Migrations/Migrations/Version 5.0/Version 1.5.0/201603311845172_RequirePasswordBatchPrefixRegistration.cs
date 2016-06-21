// <copyright>
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
    public partial class RequirePasswordBatchPrefixRegistration : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationTemplate", "BatchNamePrefix", c => c.String());
            AddColumn("dbo.UserLogin", "IsPasswordChangeRequired", c => c.Boolean());
            AddColumn("dbo.Site", "ChangePasswordPageId", c => c.Int());
            AddColumn("dbo.Site", "ChangePasswordPageRouteId", c => c.Int());
            CreateIndex("dbo.Site", "ChangePasswordPageId");
            CreateIndex("dbo.Site", "ChangePasswordPageRouteId");
            AddForeignKey("dbo.Site", "ChangePasswordPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "ChangePasswordPageRouteId", "dbo.PageRoute", "Id");

            // change security on the internal password change page
            RockMigrationHelper.AddSecurityAuthForPage( "4508223C-2989-4592-B764-B3F372B6051B", 0, "View", true, null, 1, "868E013E-EE83-8FB3-4927-457601407A0E" );

            // set the change password page for the interal site
            Sql( @"  DECLARE @ChangePasswordPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '4508223C-2989-4592-B764-B3F372B6051B')
  UPDATE [Site]
	SET [ChangePasswordPageId] = @ChangePasswordPageId
WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4'" );

            // set the change password page for the external site
            Sql( @"DECLARE @ChangePasswordPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'FAD4F98A-2CBC-4C3E-B597-6C63E2177E7D')
  DECLARE @ChangePasswordRouteId int = (SELECT TOP 1 [Id] FROM [PageRoute] WHERE [Guid] = '5230092F-126D-4169-A060-3B65211DCB71')
  UPDATE [Site]
	SET [ChangePasswordPageId] = @ChangePasswordPageId
	, [ChangePasswordPageRouteId] = @ChangePasswordRouteId
WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "ChangePasswordPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "ChangePasswordPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "ChangePasswordPageRouteId" });
            DropIndex("dbo.Site", new[] { "ChangePasswordPageId" });
            DropColumn("dbo.Site", "ChangePasswordPageRouteId");
            DropColumn("dbo.Site", "ChangePasswordPageId");
            DropColumn("dbo.UserLogin", "IsPasswordChangeRequired");
            DropColumn("dbo.RegistrationTemplate", "BatchNamePrefix");
        }
    }
}
