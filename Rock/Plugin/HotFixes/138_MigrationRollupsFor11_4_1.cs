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
    [MigrationNumber( 138, "1.11.0" )]
    public class MigrationRollupsFor11_4_1 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateForgotUserNameTemplateUpPartTwo();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: Update Forgot User Names Communication Template
        /// </summary>
        private void UpdateForgotUserNameTemplateUpPartTwo()
        {
            string oldValue = @"{% assign isChangeable =  SupportsChangePassword | Contains: User.UserName %}";
            string newValue = @"{% assign isChangeable =  Result.SupportsChangePassword | Contains: User.UserName %}";

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE 
                        {targetColumn} LIKE '%{oldValue}%'
                        AND [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'" );
        }
    }
}
