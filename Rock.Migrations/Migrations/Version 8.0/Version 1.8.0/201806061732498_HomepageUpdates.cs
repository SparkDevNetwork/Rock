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
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class HomepageUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Move Staff Updates to be First on the Page
            Sql( @"UPDATE [dbo].[Block] SET [Order] = 0 WHERE [Guid] = '879bc5a7-3ce2-43fc-bedb-b93b0054f417'" );

            // Check if default "Welcome to Rock" is on the page. If it exists, remove it
            Sql(
                @"DECLARE @WelcomeBlockId INT = (SELECT [Id] FROM [dbo].[Block] WHERE [Guid] = '5F0DBB84-BFEF-43ED-9E51-E245DC85B7B5')

                -- If versioning was or is enabled then we don't want to delete. The three HtmlContent GUIDs here were inserted by migrations.
                IF ((SELECT COUNT(*)
	                FROM [dbo].[HtmlContent]
	                WHERE [BlockId] = @WelcomeBlockId 
		                AND [Guid] NOT IN ('F76B1C2C-BAF1-45E1-844C-459417FEFC69', 'B89605CF-C111-4A1B-9228-29A78EA9DA25', '7EF64ACF-A66E-4086-900B-6832EC65CC9C')) = 0)
                BEGIN
	                -- If the welcome page content has never been altered then we want to delete it
	                IF EXISTS ( SELECT [Id]
                        FROM [dbo].[HtmlContent]
                        WHERE ([Guid] = '7EF64ACF-A66E-4086-900B-6832EC65CC9C') AND ([Content] LIKE N'%Administrators, add your organization&#39;s welcome message here.</p>%'))
	                BEGIN
		                DELETE FROM [dbo].[Block] WHERE [Guid] = '5F0DBB84-BFEF-43ED-9E51-E245DC85B7B5'
		                DELETE FROM [dbo].[HtmlContent] WHERE [Guid] = '7EF64ACF-A66E-4086-900B-6832EC65CC9C'
		                DELETE FROM [dbo].[HtmlContent] WHERE [Guid] = 'F76B1C2C-BAF1-45E1-844C-459417FEFC69'
		                DELETE FROM [dbo].[HtmlContent] WHERE [Guid] = 'B89605CF-C111-4A1B-9228-29A78EA9DA25'
	                END
                END" );
        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
