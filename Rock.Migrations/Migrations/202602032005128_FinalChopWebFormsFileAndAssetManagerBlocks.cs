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
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FinalChopWebFormsFileAndAssetManagerBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Data migration to add the job that will replace the File and Asset Manager blocks with Obsidian blocks
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "(final) File Manager",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "5EC30776-F12F-4F03-8B79-C0C819D97CCD", "535500a7-967f-4da3-8fca-cb844203cb3d" }, // File Manager #1 before it became BA327D25
                    { "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "535500a7-967f-4da3-8fca-cb844203cb3d" }, // File Manager
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_190_CHOP_FILE_AND_ASSET_MANAGER_FINAL,
                blockAttributeKeysToIgnore: null );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
