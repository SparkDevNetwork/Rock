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

using System.Collections.Generic;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 206, "1.16.4" )]
    public class MigrationRollupsForV17_0_13 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateDigitalToolsPageSettingsUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateDigitalToolsPageSettingsDown();
        }

        #region KH: Update Digital Tools Page Settings

        private void UpdateDigitalToolsPageSettingsUp()
        {
            Sql( $@"
UPDATE [dbo].[Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Page].[Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047'
" );
        }

        private void UpdateDigitalToolsPageSettingsDown()
        {
            Sql( $@"
UPDATE [dbo].[Page]
SET [BreadCrumbDisplayName] = 1
WHERE [Page].[Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047'
" );
        }

        #endregion
    }
}
