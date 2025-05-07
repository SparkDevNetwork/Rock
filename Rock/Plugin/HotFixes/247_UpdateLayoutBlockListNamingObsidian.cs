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
    [MigrationNumber( 247, "17.1" )]
    public class UpdateLayoutBlockListNamingObsidian : Migration
    {
        /// <summary>
        /// Up methods
        /// 
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //  EntityType:Rock.Blocks.Cms.LayoutBlockList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LayoutBlockList", "Layout Block List", "Rock.Blocks.Cms.LayoutBlockList, Rock.Blocks, Version=17.1, Culture=neutral, PublicKeyToken=null", false, false, "9CF1AA10-24E4-4530-A345-57DA4CFE9595" );

            // Add/Update Obsidian Block Type
            //  Name: Layout Block List
            //  Category: Obsidian > CMS
            //  EntityType:Rock.Blocks.Cms.LayoutBlockList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Layout Block List", "Displays a list of blocks.", "Rock.Blocks.Cms.LayoutBlockList", "CMS", "EA8BE085-D420-4D1B-A538-2C0D4D116E0A" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }
    }
}