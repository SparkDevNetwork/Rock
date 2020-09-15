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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 135, "1.11.0" )]
    public class MigrationRollupsFor11_2_0 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //AddModelMapIcons();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// MB: Add Model Map Icons
        /// </summary>
        private void AddModelMapIcons()
        {
            RockMigrationHelper.AddBlockAttributeValue( "2583DE89-F028-4ACE-9E1F-2873340726AC", 
                "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A",
                @"CMS^fa fa-code|Communication^fa fa-comment|Connection^fa fa-plug|Core^fa fa-gear|Event^fa fa-clipboard|Finance^fa fa-money|Group^fa fa-users|Prayer^fa fa-cloud-upload|Reporting^fa fa-list-alt|Workflow^fa fa-gears|Other^fa fa-question-circle|CRM^fa fa-user|Meta^fa fa-table|Engagement^fa fa-cogs" );
        }
    }
}
