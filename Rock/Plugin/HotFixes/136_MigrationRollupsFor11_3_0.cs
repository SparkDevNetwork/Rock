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
    [MigrationNumber( 136, "1.11.0" )]
    public class MigrationRollupsFor11_3_0 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //FixNotesBlockAttributeTypo();
            //RemoveContentChannelItemListMobileBlock();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: Fixed Typo on Notes Block Attribute
        /// </summary>
        public void FixNotesBlockAttributeTypo()
        {
            
            Sql( @"
                UPDATE 
                    [Attribute]
                SET [Description]='Should replies be automatically expanded?'
                WHERE
	                [Guid]='84E53A88-32D2-432C-8BB5-600BDBA10949'" );
        }

        /// <summary>
        /// ED: Remove the Content Channel Item List Mobile block
        /// </summary>
        public void RemoveContentChannelItemListMobileBlock()
        {
            Sql( @"DELETE FROM [dbo].[BlockType] WHERE [Guid] = '5A06FF57-DE19-423A-9E8A-CB71B69DD4FC'" );
        }
    }
}
