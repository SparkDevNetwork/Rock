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
@"IF EXISTS ( SELECT [Id] FROM [dbo].[HtmlContent] WHERE ([Guid] = '7ef64acf-a66e-4086-900b-6832ec65cc9c') AND ([Content] LIKE N'%Administrators, add your organization&#39;s welcome message here.</p>%'))
BEGIN
    DELETE FROM [dbo].[Block] WHERE [Guid] = '5f0dbb84-bfef-43ed-9e51-e245dc85b7b5'
    DELETE FROM [dbo].[HtmlContent] WHERE [Guid] = '7ef64acf-a66e-4086-900b-6832ec65cc9c'
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
