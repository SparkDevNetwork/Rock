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
namespace Rock.Migrations
{
    using System.Collections.Generic;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240705 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ChopBlocksDown();
        }

        /// <summary>
        /// PA: Chop Block Type
        /// </summary>
        private void ChopBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - Group 2",
                blockTypeReplacements: new Dictionary<string, string> {
            { "8D6C81EB-2FFE-41CB-9A2B-FB70857E5761", "DB19D24E-B0C8-4686-8582-7B84DAE33EE8" }, // Queue Detail
            { "CFE5C556-9E46-4A51-849D-FF5F4A899930", "1ABC8DE5-A64D-4E69-875A-4407D9A7B425" }, // Personal Link Section Detail
            { "E6E7333A-C4A6-4DE7-9A37-CC2641320C98", "E4302549-0BB8-4DDE-9B9A-70F0B665E76F" }, // Route Detail
            { "E92E3C51-EB14-414D-BC68-9061FEB92A22", "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7" }, // Route List
            { "E08504ED-B84C-4115-A058-03AAB8E8A307", "2A5AE27F-F536-4ACC-B5EB-9263C4B92EF5" } // "Pledge Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_166_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        /// <summary>
        /// PA: Chop Block Types
        /// </summary>
        private void ChopBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( SystemGuid.ServiceJob.DATA_MIGRATIONS_166_CHOP_OBSIDIAN_BLOCKS, "ServiceJob" );
        }
    }
}
