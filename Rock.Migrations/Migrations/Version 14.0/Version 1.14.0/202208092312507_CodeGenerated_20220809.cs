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
    public partial class CodeGenerated_20220809 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Crm.AssessmentTypeDetail", "Assessment Type Detail", "Rock.Blocks.Crm.AssessmentTypeDetail, Rock.Blocks, Version=1.14.0.15, Culture=neutral, PublicKeyToken=null", false, false, "83D4C6CA-A605-44D3-8BEA-99B3E881BAA0");

            // Add/Update Obsidian Block Type
            //   Name:Assessment Type Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.UpdateMobileBlockType("Assessment Type Detail", "Displays the details of a particular assessment type.", "Rock.Blocks.Crm.AssessmentTypeDetail", "CRM", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E");

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E3738FA5-58B2-40F9-9952-2C216D2319AC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"5707E617-DDBA-402F-9D3E-3F315BFE0BDA"); 

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Planned Visit Information Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Planned Visit Information Panel Title", "PlannedVisitInformationPanelTitle", "Planned Visit Information Panel Title", @"The title for the Planned Visit Information panel", 16, @"Visit Information", "6D4D296E-74A9-4BD1-98D6-BAEA6ECFE350" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Content Collection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Content Collection", "ContentCollection", "Content Collection", @"The content collection to use when searching.", 0, @"", "C5FBFBCC-506A-45A1-B999-DC213D3E8BD0" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Order", "PrayerOrder", "Order", @"The order that requests should be displayed.", 8, @"0", "9AA9B6B9-2D74-4B6E-8A01-325F313A0AA0" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FBE0419-5404-4866-85A1-135542D33725", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "669269C5-0679-433A-9652-7F7CD4F793F6" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("5707E617-DDBA-402F-9D3E-3F315BFE0BDA","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
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
            RockMigrationHelper.DeleteAttribute("669269C5-0679-433A-9652-7F7CD4F793F6");

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Order
            RockMigrationHelper.DeleteAttribute("9AA9B6B9-2D74-4B6E-8A01-325F313A0AA0");

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Content Collection
            RockMigrationHelper.DeleteAttribute("C5FBFBCC-506A-45A1-B999-DC213D3E8BD0");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Planned Visit Information Panel Title
            RockMigrationHelper.DeleteAttribute("6D4D296E-74A9-4BD1-98D6-BAEA6ECFE350");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("5707E617-DDBA-402F-9D3E-3F315BFE0BDA");

            // Delete BlockType 
            //   Name: Assessment Type Detail
            //   Category: CRM
            //   Path: -
            //   EntityType: Assessment Type Detail
            RockMigrationHelper.DeleteBlockType("3B8B5AE5-4139-44A6-8EAA-99D48E51134E");
        }
    }
}
