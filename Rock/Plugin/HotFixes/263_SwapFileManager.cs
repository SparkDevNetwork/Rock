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

using System;
using System.Collections.Generic;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 263, "19.0" )]
    public class SwapFileManager : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SwapBlocksForV19Up();
            UpdateFileManagerPage();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

        }

        private void UpdateFileManagerPage()
        {
            Sql( @"
UPDATE dbo.[Page]
SET [LayoutId] = (
    SELECT [Id]
    FROM dbo.[Layout]
    WHERE [Guid] = 'C2467799-BB45-4251-8EE6-F0BF27201535'
)
WHERE [Guid] = '6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1';
" );
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the swap job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForSwap()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.FileAssetManager", "File Asset Manager", "Rock.Blocks.Cms.FileAssetManager, Rock.Blocks, Version=19.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, "E357AD54-1725-48B8-997C-23C2587800FB" );

            // Add/Update Obsidian Block Type
            //   Name:File Asset Manager
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.AddOrUpdateEntityBlockType( "File Asset Manager", "Browse and manage files on the web server or stored on a remote server or 3rd party cloud storage", "Rock.Blocks.Cms.FileAssetManager", "CMS", "535500A7-967F-4DA3-8FCA-CB844203CB3D" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Browse Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Browse Mode", "BrowseMode", "Browse Mode", @"Select 'image' to show only image files. Select 'doc' to show all files.", 5, @"doc", "C872B6A7-36F6-4771-807A-7B4A7E8BAD2C" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Storage Providers", "EnableAssetProviders", "Enable Asset Storage Providers", @"Set this to true to enable showing folders and files from your configured asset storage providers.", 0, @"False", "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable File Manager", "EnableFileManager", "Enable File Manager", @"Set this to true to enable showing folders and files your server's local file system.", 1, @"True", "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Zip Upload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Zip Upload", "ZipUploaderEnabled", "Enable Zip Upload", @"Set this to true to enable the Zip File uploader.", 7, @"False", "BD031ADA-5D23-4237-A332-468FAC7282E9" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: File Editor Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "File Editor Page", "FileEditorPage", "File Editor Page", @"Page used to edit the contents of a file.", 6, @"", "80D4544B-563A-4109-9F61-F4E019580B3A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Height Mode", "HeightMode", "Height Mode", @"Static lets you set a CSS height below to determine the height of the block. Flexible will grow with the content. Full Worksurface is designed to fill up a full worksurface page layout.", 2, @"full", "67ECB409-F5C5-4487-A60B-FD572B99D95B" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Height", "Height", "Height", @"If you've selected Yes for ""Use Static Height"", this will be the CSS length value that dictates how tall the block will be.", 3, @"400px", "48C3DFAD-2168-4F6C-8DEC-167E49C379B7" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Root Folder", "RootFolder", "Root Folder", @"The root file manager folder to browse", 4, @"~/Content", "14684245-5768-442D-9BFB-C80E1383775A" );
        }

        private void SwapBlockTypesv19_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 19.0 (19.0.1)",
                blockTypeReplacements: new Dictionary<string, string> {
                // blocks chopped in v19.0.1
{ "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "535500a7-967f-4da3-8fca-cb844203cb3d" }, // File Manager -> File Asset Manager
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_190_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        private void SwapBlocksForV19Up()
        {
            RegisterBlockAttributesForSwap();
            SwapBlockTypesv19_0();
        }
    }
}