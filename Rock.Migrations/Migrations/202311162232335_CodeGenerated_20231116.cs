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
    public partial class CodeGenerated_20231116 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Acme Certificates
            //   Category: Blue Box Moon > Acme Certificate
            //   Attribute: Redirect Override
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A08FB359-E68E-46FC-85D6-53726EAE6E23", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Redirect Override", "RedirectOverride", "Redirect Override", @"If you enter a value here it will be used as the redirect URL for Acme Challenges to other sites instead of the automatically determined one.", 1, @"", "017AC04B-D61C-486B-9616-8C16723F6088" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Show Additional Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Additional Accounts", "ShowAdditionalAccounts", "Show Additional Accounts", @"When enabled, all active financial accounts marked Public will be available for selection, or you can choose 'Additional Accounts' in the setting below to show only certain accounts.", 4, @"False", "5E06E94E-77BA-458F-988F-7A1F64AD8000" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Additional Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "17033CDD-EF97-4413-A483-7B85A787A87F", "Additional Accounts", "AdditionalAccounts", "Additional Accounts", @"When 'Show Additional Accounts' is enabled, the accounts you choose here will be available for selection.", 5, @"", "EBCB14B4-700C-47AB-92A7-26DD431D5165" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Add Account Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Add Account Text", "AddAccountText", "Add Account Text", @"The button text to display for adding an additional account", 3, @"Add Another Account", "AB3102D1-F368-4099-A534-8866676B45AF" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "11F75489-3953-460A-A905-A2A233D526F7" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute("11F75489-3953-460A-A905-A2A233D526F7");

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Add Account Text
            RockMigrationHelper.DeleteAttribute("AB3102D1-F368-4099-A534-8866676B45AF");

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Additional Accounts
            RockMigrationHelper.DeleteAttribute("EBCB14B4-700C-47AB-92A7-26DD431D5165");

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Show Additional Accounts
            RockMigrationHelper.DeleteAttribute("5E06E94E-77BA-458F-988F-7A1F64AD8000");

            // Attribute for BlockType
            //   BlockType: Acme Certificates
            //   Category: Blue Box Moon > Acme Certificate
            //   Attribute: Redirect Override
            RockMigrationHelper.DeleteAttribute("017AC04B-D61C-486B-9616-8C16723F6088");
        }
    }
}
