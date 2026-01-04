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
    public partial class CodeGenerated_20251112 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.RequestFilterDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.RequestFilterDetail", "Request Filter Detail", "Rock.Blocks.Cms.RequestFilterDetail, Rock.Blocks, Version=18.0.13.0, Culture=neutral, PublicKeyToken=null", false, false, "0E340E27-D6D9-4870-9835-401545C44801" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PersonDuplicateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PersonDuplicateDetail", "Person Duplicate Detail", "Rock.Blocks.Crm.PersonDuplicateDetail, Rock.Blocks, Version=18.0.13.0, Culture=neutral, PublicKeyToken=null", false, false, "B96C02DC-F624-4953-BED3-F7BA52CE854D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.AI.ChatBot
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.AI.ChatBot", "Chat Bot", "Rock.Blocks.AI.ChatBot, Rock.Blocks, Version=18.0.13.0, Culture=neutral, PublicKeyToken=null", false, false, "C08511A6-D9F5-40F4-A9CC-50CBE40A4AB8" );

            // Add/Update Obsidian Block Type
            //   Name:Chat Bot
            //   Category:AI
            //   EntityType:Rock.Blocks.AI.ChatBot
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Chat Bot", "Allows the user to try out the chat agent.", "Rock.Blocks.AI.ChatBot", "AI", "91A66C59-830E-49B5-A196-DCF93D0DDE92" );

            // Add/Update Obsidian Block Type
            //   Name:Request Filter Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.RequestFilterDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Request Filter Detail", "Displays the details of a particular request filter.", "Rock.Blocks.Cms.RequestFilterDetail", "CMS", "59E6D50E-70A8-4695-97A4-2DE33DD09ECF" );

            // Add/Update Obsidian Block Type
            //   Name:Person Duplicate Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.PersonDuplicateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Duplicate Detail", "Shows records that are possible duplicates of the selected person.", "Rock.Blocks.Crm.PersonDuplicateDetail", "CRM", "AAA53F35-1891-4236-B9CB-37805B9134DF" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Show Upgrade Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Upgrade Message", "ShowUpgradeMessage", "Show Upgrade Message", @"Displays a message that encourages switching to the newer Communication Wizard. <br>This helps guide staff toward using the updated experience unless you plan to continue using the legacy wizard long-term.", 15, @"True", "8BE08D27-8421-48F5-9E98-FC99F80440C2" );

            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE11559B-03C0-42CE-B8B5-CE9C1027E650", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Financial Gateway", "FinancialGateway", "Financial Gateway", @"Select the MyWell gateway to use for processing transactions.", 0, @"", "AB553EB6-3A30-49CD-9463-763BF5CDC9D4" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Agent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91A66C59-830E-49B5-A196-DCF93D0DDE92", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Agent", "Agent", "Agent", @"The AI agent to use for this chat bot.", 0, @"", "C9DF3A73-4218-4B90-9C66-2F366D79D58E" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Confidence Score High
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score High", "ConfidenceScoreHigh", "Confidence Score High", @"The minimum confidence score required to be considered a likely match.", 0, @"80", "C8D0908F-B22C-4939-A97A-4AA29AFA5DA5" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Confidence Score Low
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score Low", "ConfidenceScoreLow", "Confidence Score Low", @"The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", 1, @"60", "4D5077BE-9036-4320-B238-ECAE9B60DDC4" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Include Inactive
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive", "IncludeInactive", "Include Inactive", @"Set to true to also include potential matches when both records are inactive.", 2, @"False", "E3BC0EB9-B895-4599-BAF4-1BA4578ABF0C" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Include Businesses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Businesses", "IncludeBusinesses", "Include Businesses", @"Set to true to also include potential matches when either record is a Business.", 3, @"False", "6E566BCF-5C9F-4F74-B131-F4FEBE8A0B35" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "3E1D32D6-E74F-4189-AA41-13F135CD2C86" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA53F35-1891-4236-B9CB-37805B9134DF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9A98198F-09E0-4EC7-AD98-FB123D6D122E" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Giving
            //   Category: Mobile > Finance
            //   Attribute: Financial Gateway
            RockMigrationHelper.DeleteAttribute( "AB553EB6-3A30-49CD-9463-763BF5CDC9D4" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "9A98198F-09E0-4EC7-AD98-FB123D6D122E" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "3E1D32D6-E74F-4189-AA41-13F135CD2C86" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Include Businesses
            RockMigrationHelper.DeleteAttribute( "6E566BCF-5C9F-4F74-B131-F4FEBE8A0B35" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Include Inactive
            RockMigrationHelper.DeleteAttribute( "E3BC0EB9-B895-4599-BAF4-1BA4578ABF0C" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Confidence Score Low
            RockMigrationHelper.DeleteAttribute( "4D5077BE-9036-4320-B238-ECAE9B60DDC4" );

            // Attribute for BlockType
            //   BlockType: Person Duplicate Detail
            //   Category: CRM
            //   Attribute: Confidence Score High
            RockMigrationHelper.DeleteAttribute( "C8D0908F-B22C-4939-A97A-4AA29AFA5DA5" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Agent
            RockMigrationHelper.DeleteAttribute( "C9DF3A73-4218-4B90-9C66-2F366D79D58E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Show Upgrade Message
            RockMigrationHelper.DeleteAttribute( "8BE08D27-8421-48F5-9E98-FC99F80440C2" );

            // Delete BlockType 
            //   Name: Person Duplicate Detail
            //   Category: CRM
            //   Path: -
            //   EntityType: Person Duplicate Detail
            RockMigrationHelper.DeleteBlockType( "AAA53F35-1891-4236-B9CB-37805B9134DF" );

            // Delete BlockType 
            //   Name: Request Filter Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Request Filter Detail
            RockMigrationHelper.DeleteBlockType( "59E6D50E-70A8-4695-97A4-2DE33DD09ECF" );

            // Delete BlockType 
            //   Name: Chat Bot
            //   Category: AI
            //   Path: -
            //   EntityType: Chat Bot
            RockMigrationHelper.DeleteBlockType( "91A66C59-830E-49B5-A196-DCF93D0DDE92" );
        }
    }
}
