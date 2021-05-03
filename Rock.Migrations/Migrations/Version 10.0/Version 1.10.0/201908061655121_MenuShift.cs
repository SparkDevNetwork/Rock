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
    public partial class MenuShift : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Move Connections to Engagement
            RockMigrationHelper.MovePage( SystemGuid.Page.CONNECTIONS, SystemGuid.Page.ENGAGEMENT );

            // Make Engagement to a menu category
            RockMigrationHelper.MovePage( SystemGuid.Page.ENGAGEMENT, SystemGuid.Page.PEOPLE );

            // Order the "People" page menu categories
            Sql( $@"
                UPDATE [dbo].[Page] SET [order] = 3 WHERE [Guid] = '{SystemGuid.Page.PERSON_PAGES}'
                UPDATE [dbo].[Page] SET [order] = 0 WHERE [Guid] = '{SystemGuid.Page.MANAGE}'
                UPDATE [dbo].[Page] SET [order] = 2 WHERE [Guid] = '{SystemGuid.Page.COMMUNICATIONS_PEOPLE}'
                UPDATE [dbo].[Page] SET [order] = 1 WHERE [Guid] = '{SystemGuid.Page.ENGAGEMENT}'" );

            // Remove block from Engagement page
            RockMigrationHelper.DeleteBlock( "7EF5C2D7-E20B-4955-B09B-E31F3CC20B42" );

            // Update Engagement page properties
            Sql( $@"UPDATE [Page] SET [BreadCrumbDisplayName] = 0, [AllowIndexing] = 0 WHERE [Guid] = '{SystemGuid.Page.ENGAGEMENT}'" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
