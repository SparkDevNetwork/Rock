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
    public partial class Rollup_PreAlphaRelease_0719 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "ViewState Viewer", "Block allows you to see what's in the View State of a page.", "~/Blocks/Utility/ViewStateViewer.ascx", "Utility", "268F9F11-BC74-4E86-A72D-6AA668BF470D" );
            // Attrib for BlockType: Connection Opportunity Search:Campus Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Label", "CampusLabel", "", @"", 10, @"Campuses", "31791AAD-7D25-495E-918C-2149F75E080A" );
            // Attrib for BlockType: Bulk Update:Task Count
            RockMigrationHelper.UpdateBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Task Count", "TaskCount", "", @"The number of concurrent tasks to use when performing updates. If left blank then it will be determined automatically.", 3, @"0", "B5FDFD1A-D638-4CFC-B8ED-20664B6AE3F1" );
            // Attrib for BlockType: Add Group:Enable Alternate Identifier
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Alternate Identifier", "EnableAlternateIdentifier", "", @"If enabled, an additional step will be shown for supplying a custom alternate identifier for each person.", 29, @"False", "B87F6D4A-8A44-486E-BE14-A81FF60DA551" );
            // Attrib for BlockType: Registration Entry:Show Field Descriptions
            RockMigrationHelper.UpdateBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Field Descriptions", "ShowFieldDescriptions", "", @"Show the field description as help text", 10, @"False", "1C917D6F-9569-4D0C-B138-8220BCC812F5" );
            // Attrib for BlockType: Group Registration:Require Mobile Phone
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Mobile Phone", "IsRequiredMobile", "", @"Should mobile phone numbers be required for registration?", 0, @"False", "7CE78567-4438-47E7-B0D0-25D5B6498515" );
            // Attrib for BlockType: Group Registration:Require Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Email", "IsRequireEmail", "", @"Should email be required for registration?", 0, @"True", "37E22E5F-19C9-4F17-8E1D-8C0E5F52DE1D" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Registration:Require Email
            RockMigrationHelper.DeleteAttribute( "37E22E5F-19C9-4F17-8E1D-8C0E5F52DE1D" );
            // Attrib for BlockType: Group Registration:Require Mobile Phone
            RockMigrationHelper.DeleteAttribute( "7CE78567-4438-47E7-B0D0-25D5B6498515" );
            // Attrib for BlockType: Registration Entry:Show Field Descriptions
            RockMigrationHelper.DeleteAttribute( "1C917D6F-9569-4D0C-B138-8220BCC812F5" );
            // Attrib for BlockType: Add Group:Enable Alternate Identifier
            RockMigrationHelper.DeleteAttribute( "B87F6D4A-8A44-486E-BE14-A81FF60DA551" );
            // Attrib for BlockType: Bulk Update:Task Count
            RockMigrationHelper.DeleteAttribute( "B5FDFD1A-D638-4CFC-B8ED-20664B6AE3F1" );
            // Attrib for BlockType: Connection Opportunity Search:Campus Label
            RockMigrationHelper.DeleteAttribute( "31791AAD-7D25-495E-918C-2149F75E080A" );
            RockMigrationHelper.DeleteBlockType( "268F9F11-BC74-4E86-A72D-6AA668BF470D" ); // ViewState Viewer
        }
    }
}
