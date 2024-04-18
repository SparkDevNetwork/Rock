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
    public partial class ReplaceWeformsBlocksWithObsidianBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SwapNotesBlockUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SwapNotesBlockDown();
            ChopBlocksDown();
        }

        /// <summary>
        /// PA: Chop Block Type
        /// </summary>
        private void ChopBlocksUp()
        {

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Chop Block Types - Group 1",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "59C9C862-570C-4410-99B6-DD9064B5E594", "7C10240A-7EE5-4720-AAC9-5C162E9F5AAC" }, // Schedule Detail
                    { "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC", "4B50E08A-A805-4213-A5AF-BCA570FCB528" }, // Asset Storage Provider Detail
                    { "794C564C-6395-4303-812F-3BFBD1057443", "72EDDF3D-625E-40A9-A68B-76236E77A3F3" }, // Page Short Link Detail
                    { "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "A83A1F49-10A6-4362-ACC3-8027224A2120" }, // Streak Type Detail
                    { "762BC126-1A2E-4483-A63B-3AB4939D19F1", "78F27537-C05F-44E0-AF84-2329C8B5D71D" }, // Following Event Type Detail
                    { "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "6BE58680-8795-46A0-8BFA-434A01FEB4C8" }, // Financial Batch Detail
                },
                migrationStrategy: "Chop",
                jobGuid: "54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5" );
        }

        /// <summary>
        /// PA: Chop Block Types
        /// </summary>
        private void ChopBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( "54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5", "ServiceJob" );
        }
        
        /// <summary>
        /// PA: Swap Note Block Type
        /// </summary>
        private void SwapNotesBlockUp()
        {
            // Webforms 2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3
            // Obsidian D87B84DC-7AD9-42A2-B18D-88B7E71DADA8

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Swap Note Block Type",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8" }
                },
                migrationStrategy: "Swap",
                jobGuid: "8390C1AC-88D6-474A-AC05-8FFBD358F75D" );
        }

        /// <summary>
        /// PA: Swap Note Block Type
        /// </summary>
        private void SwapNotesBlockDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( "8390C1AC-88D6-474A-AC05-8FFBD358F75D", "ServiceJob" );
        }
    }
}
