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
    public partial class UndoObsidianChop_20250311 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SwapBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region KH: Undo Media Folder Detail Block Chop

        private void SwapBlocksUp()
        {
            RegisterBlockTypeForSwap();
            SwapObsidianBlocks();
        }

        private void RegisterBlockTypeForSwap()
        {
            RockMigrationHelper.UpdateBlockType( "Media Folder Detail", "Edit details of a Media Folder", "~/Blocks/Cms/MediaFolderDetail.ascx", "CMS", "3C9D442B-D066-43FA-9380-98C60936992E" );
        }

        private void SwapObsidianBlocks()
        {
            // Custom swap to replace Obsidian Blocks with Webform Blocks
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Webforms Blocks",
                blockTypeReplacements: new Dictionary<string, string> {
{ "662af7bb-5b61-43c6-bda6-a6e7aab8fc00", "3C9D442B-D066-43FA-9380-98C60936992E" }, // MediaFolderDetail -> MediaFolderDetail.ascx
{ "f431f950-f007-493e-81c8-16559fe4c0f0", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" }, // DefinedValueList -> DefinedValueList.ascx
{ "73fd23b4-fa3a-49ea-b271-ffb228c6a49e", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" }, // DefinedTypeDetail -> DefinedTypeDetail.ascx
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_WEBFORMS_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        #endregion

    }
}
