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
    [MigrationNumber( 109, "1.10.0" )]
    public class DisableCommunicationPersonParameter : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Communication Entry:Enable Person Parameter
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'person' querystring parameter with a person Id to the block to create a communication for that person.", 2, @"False", "B9C0511F-9C95-4DD1-93F0-EDCCF7CD0471" );
            // Attrib Value for Block:Communication, Attribute:Enable Person Parameter Page: Simple Communication, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue("BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC","B9C0511F-9C95-4DD1-93F0-EDCCF7CD0471",@"True");
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
