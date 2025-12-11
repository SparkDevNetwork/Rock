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
    /// <seealso cref="Migration" />
    [MigrationNumber( 272, "19.0" )]
    public class UpdateFileEditorPageLayout : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateFileEditorPage();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

        }

        private void UpdateFileEditorPage()
        {
            Sql( @"
                UPDATE dbo.[Page]
                SET [LayoutId] = (
                    SELECT [Id]
                    FROM dbo.[Layout]
                    WHERE [Guid] = 'C2467799-BB45-4251-8EE6-F0BF27201535'
                )
                WHERE [Guid] = '053C3F1D-8BF2-48B2-A8E6-55184F8A87F4';
            " );
        }
    }
}