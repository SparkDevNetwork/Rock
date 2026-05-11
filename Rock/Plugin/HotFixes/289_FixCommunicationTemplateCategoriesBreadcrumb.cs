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
    /// Turns off BreadCrumbDisplayName on the Communication Template
    /// Categories page so the page name is not rendered twice in the
    /// breadcrumb (the Category List block already contributes its own
    /// crumb for the page).
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 289, "19.1" )]
    public class FixCommunicationTemplateCategoriesBreadcrumb : Migration
    {
        private const string CommunicationTemplateCategoriesPageGuid = "4D6DEAB3-46A0-4B27-B67B-71383EFE1171";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
IF EXISTS ( SELECT 1 FROM [Page] WHERE [Guid] = '{CommunicationTemplateCategoriesPageGuid}' )
BEGIN
    UPDATE [Page]
    SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '{CommunicationTemplateCategoriesPageGuid}'
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
