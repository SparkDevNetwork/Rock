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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20220823 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaFolderDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.MediaFolderDetail", "Media Folder Detail", "Rock.Blocks.Cms.MediaFolderDetail, Rock.Blocks, Version=1.14.0.17, Culture=neutral, PublicKeyToken=null", false, false, "29CF7521-2DCD-467A-98FA-1C28C16C8B69");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.PersistedDatasetDetail", "Persisted Dataset Detail", "Rock.Blocks.Cms.PersistedDatasetDetail, Rock.Blocks, Version=1.14.0.17, Culture=neutral, PublicKeyToken=null", false, false, "B189040B-2914-437F-900D-A54705B22D2E");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.PersonalLinkSectionDetail", "Personal Link Section Detail", "Rock.Blocks.Cms.PersonalLinkSectionDetail, Rock.Blocks, Version=1.14.0.17, Culture=neutral, PublicKeyToken=null", false, false, "E76598F7-F686-41EE-848C-58E10758027F");

            // Add/Update Obsidian Block Type
            //   Name:Media Folder Detail
            //   Category:Cms
            //   EntityType:Rock.Blocks.Cms.MediaFolderDetail
            RockMigrationHelper.UpdateMobileBlockType("Media Folder Detail", "Displays the details of a particular media folder.", "Rock.Blocks.Cms.MediaFolderDetail", "Cms", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00");

            // Add/Update Obsidian Block Type
            //   Name:Persisted Dataset Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetDetail
            RockMigrationHelper.UpdateMobileBlockType("Persisted Dataset Detail", "Displays the details of a particular persisted dataset.", "Rock.Blocks.Cms.PersistedDatasetDetail", "CMS", "6035AC10-07A5-4EDD-A1E9-10862FC41494");

            // Add/Update Obsidian Block Type
            //   Name:Personal Link Section Detail
            //   Category:Cms
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionDetail
            RockMigrationHelper.UpdateMobileBlockType("Personal Link Section Detail", "Displays the details of a particular personal link section.", "Rock.Blocks.Cms.PersonalLinkSectionDetail", "Cms", "1ABC8DE5-A64D-4E69-875A-4407D9A7B425");

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "85C4768C-A0A5-4476-9671-4F21E6C7ADAB".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"1F45ABF9-1E70-4633-AFCC-0CE77EACE149"); 

            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Personalization
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Personalization", "Personalization", "Personalization", @"The setting determines how personalization effect the results shown. Ignore will not consider segments or request filters, Prioritize will add items with matching items to the top of the list (in order by the sort order) and Filter will only show items that match the current individuals segments and request filters.", 0, @"", "0A17F3F7-8018-4422-9937-08E364EDB203" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section Detail
            //   Category: Cms
            //   Attribute: Shared Section
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1ABC8DE5-A64D-4E69-875A-4407D9A7B425", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Shared Section", "SharedSection", "Shared Section", @"When enabled, only shared sections will be displayed.", 0, @"False", "6A696EED-A3B2-4FD9-AC99-6846571A98C7" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C4FAFAE-41D1-4FF2-B6DC-FF99CD4DABBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "404B38F3-CB27-4E61-B1AC-1638D94DBD04" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("1F45ABF9-1E70-4633-AFCC-0CE77EACE149","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("404B38F3-CB27-4E61-B1AC-1638D94DBD04");

            // Attribute for BlockType
            //   BlockType: Personal Link Section Detail
            //   Category: Cms
            //   Attribute: Shared Section
            RockMigrationHelper.DeleteAttribute("6A696EED-A3B2-4FD9-AC99-6846571A98C7");

            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Personalization
            RockMigrationHelper.DeleteAttribute("0A17F3F7-8018-4422-9937-08E364EDB203");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("1F45ABF9-1E70-4633-AFCC-0CE77EACE149");

            // Delete BlockType 
            //   Name: Personal Link Section Detail
            //   Category: Cms
            //   Path: -
            //   EntityType: Personal Link Section Detail
            RockMigrationHelper.DeleteBlockType("1ABC8DE5-A64D-4E69-875A-4407D9A7B425");

            // Delete BlockType 
            //   Name: Persisted Dataset Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Persisted Dataset Detail
            RockMigrationHelper.DeleteBlockType("6035AC10-07A5-4EDD-A1E9-10862FC41494");

            // Delete BlockType 
            //   Name: Media Folder Detail
            //   Category: Cms
            //   Path: -
            //   EntityType: Media Folder Detail
            RockMigrationHelper.DeleteBlockType("662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00");
        }
    }
}
