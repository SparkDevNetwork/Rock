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
    [MigrationNumber( 106, "1.10.0" )]
    public class EnableLavaDefaultSetting : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Communication Entry:Enable Lava
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Lava", "EnableLava", "Enable Lava", @"Remove the lava syntax from the message without resolving it.", 0, @"False", "2613E297-1FF8-4444-82FD-C3F6000BFFF1" );
            //RockMigrationHelper.AddBlockAttributeValue( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", "2613E297-1FF8-4444-82FD-C3F6000BFFF1", "True" );
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
