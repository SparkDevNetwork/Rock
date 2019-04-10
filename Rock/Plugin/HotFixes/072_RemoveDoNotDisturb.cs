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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{

    /// <summary>
    /// This hotfix update is to address an issue where the blacklist value was edited and then deleted (so the defaults would be used) before the WhitelistBlacklist migration.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 72, "1.8.7" )]
    public class RemoveDoNotDisturb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //RemoveCommunicationSettings();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Down migration functionality not yet available for hotfix migrations.
        }

        /// <summary>
        /// Removes the communication settings page, block, and attributes
        /// </summary>
        private void RemoveCommunicationSettings()
        {
            // Delete the block attribute value for the category attribute on the communication setting block
            RockMigrationHelper.DeleteBlockAttributeValue( "8083E072-A5F6-4BA0-8110-7D3A9E94A05F", "25E02584-4B8F-4A8F-8558-8D2EDA2C5393" );

            // Delete the block attribute category on the systems settings block type
            RockMigrationHelper.DeleteAttribute( "25E02584-4B8F-4A8F-8558-8D2EDA2C5393" );

            // Delete the System Setting attribute for Do Not Disturb Start
            RockMigrationHelper.DeleteAttribute( "4A558666-32C7-4490-B860-0F41358E14CA" );

            // Delete the System Setting attribute for Do Not Disturb End
            RockMigrationHelper.DeleteAttribute( "661802FC-E636-4CE2-B75A-4AC05595A347" );

            // Delete the System Setting attribute for Do Not Disturb Active
            RockMigrationHelper.DeleteAttribute( "1BE30413-5C90-4B78-B324-BD31AA83C002" );

            // Delete the Communication Settings Category
            RockMigrationHelper.DeleteCategory( "1059CCF2-933F-488E-8DBF-4FEC64A12409" );

            // Delete the Communication Setting Block
            RockMigrationHelper.DeleteBlock( "8083E072-A5F6-4BA0-8110-7D3A9E94A05F" );

            // Delete the system settings block type
            RockMigrationHelper.DeleteBlockType( "41A585E0-4522-40FA-8CC6-A411C70340F7" );

            // Delete the Communication Settings page
            RockMigrationHelper.DeletePage( "5B67480F-418D-4916-9C39-A26D2F8FA95C" );
        }


    }
}
