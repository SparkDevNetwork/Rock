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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 74, "1.8.8" )]
    public class MigrationRollupsForV8_8_1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixRockInstanceIDs();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// ED: Fix Rock Instance IDs F8ABA66C-C73D-441F-9A28-233D803EFB0D
        /// </summary>
        private void FixRockInstanceIDs()
        {
            Sql( @"
                -- Update to a unique ID
                UPDATE [dbo].[Attribute]
                SET [Guid] = NEWID()
                WHERE [EntityTypeQualifierColumn] = 'systemsetting'
                 AND [Key] = 'RockInstanceId'
                 AND [Guid] = 'F8ABA66C-C73D-441F-9A28-233D803EFB0D'" );
        }
    }
}
