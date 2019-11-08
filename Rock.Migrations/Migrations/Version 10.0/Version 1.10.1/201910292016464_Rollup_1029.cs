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
    public partial class Rollup_1029 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCommunicationEntryDefaultAsBulkAttributes();
            AddConnectionOpportunitySignupBlockAttributes();
            CodeGenMigrationsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Connection Opportunity Signup:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect'.)", 5, @"368DD475-242C-49C4-A42C-7278BE690CC2", "57BC03C9-55F7-47B1-B3BA-DEA586B917D9" );
            // Attrib for BlockType: Connection Opportunity Signup:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", @"The record status to use for new individuals (default: 'Pending'.)", 6, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "2AF9F953-816F-4946-AAAB-DBBD878D6170" );
            // Attrib for BlockType: Workflow Entry:Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCssClass", "Block Title Icon CSS Class", @"The CSS class for the icon displayed in the block title. If not specified, the icon for the Workflow Type will be shown.", 3, @"", "7570B611-CB33-4933-AEB7-86C25AFC0CF3" );
            // Attrib for BlockType: Workflow Entry:Block Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Block Title Template", "BlockTitleTemplate", "Block Title Template", @"Lava template for determining the title of the block. If not specified, the name of the Workflow Type will be shown.", 2, @"", "7C01569D-4C25-4322-9308-05FB15AF6613" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Workflow Entry:Block Title Template
            RockMigrationHelper.DeleteAttribute("7C01569D-4C25-4322-9308-05FB15AF6613");
            // Attrib for BlockType: Workflow Entry:Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute("7570B611-CB33-4933-AEB7-86C25AFC0CF3");
            // Attrib for BlockType: Connection Opportunity Signup:Record Status
            RockMigrationHelper.DeleteAttribute("2AF9F953-816F-4946-AAAB-DBBD878D6170");
            // Attrib for BlockType: Connection Opportunity Signup:Connection Status
            RockMigrationHelper.DeleteAttribute("57BC03C9-55F7-47B1-B3BA-DEA586B917D9");
        }

        /// <summary>
        /// DL: Add Communication Entry Bulk Default Attributes
        /// </summary>
        private void AddCommunicationEntryDefaultAsBulkAttributes()
        {
	        RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.COMMUNICATION_ENTRY, Rock.SystemGuid.FieldType.BOOLEAN, "Default As Bulk", "DefaultAsBulk", "", "Should new entries be flagged as bulk communication by default?", 12, @"", Rock.SystemGuid.Attribute.COMMUNICATION_ENTRY_DEFAULT_AS_BULK );
	        RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.COMMUNICATION_ENTRY_WIZARD, Rock.SystemGuid.FieldType.BOOLEAN, "Default As Bulk", "DefaultAsBulk", "", "Should new entries be flagged as bulk communication by default?", 12, @"", Rock.SystemGuid.Attribute.COMMUNICATION_ENTRY_WIZARD_DEFAULT_AS_BULK );
        }

        /// <summary>
        /// DL: Add Connection Opportunity Signup Block Attributes
        /// </summary>
        private void AddConnectionOpportunitySignupBlockAttributes()
        {
            // Attrib for BlockType: Connection Opportunity Signup:Exclude Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONNECTION_OPPORTUNITY_SIGNUP, Rock.SystemGuid.FieldType.CATEGORIES, "Exclude Attribute Categories", "ExcludeAttributeCategories", "Exclude Attribute Categories", "Attributes in these Categories will not be displayed.", 9, @"", "4617DD9E-CADE-42FB-915D-D82AB3E3C0D5" );
            // Attrib for BlockType: Connection Opportunity Signup:Include Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONNECTION_OPPORTUNITY_SIGNUP, Rock.SystemGuid.FieldType.CATEGORIES, "Include Attribute Categories", "IncludeAttributeCategories", "Include Attribute Categories", "Attributes in these Categories will be displayed.", 8, @"", "B6C64813-C7E1-480E-BF8F-8FCE4EAD9242" );
        }

    }
}
