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
    public partial class CodeGenerated_20250305 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.Chat.GeneralChat
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.Chat.GeneralChat", "General Chat", "Rock.Blocks.Communication.Chat.GeneralChat, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null", false, false, "B3D6F875-1589-4543-9E76-5C41201B465B" );

            // Add/Update Obsidian Block Type
            //   Name:Chat
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.Chat.GeneralChat
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Chat", "Integrate StreamChat into your Rock Mobile application.", "Rock.Blocks.Communication.Chat.GeneralChat", "Communication", "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" );

            // Attribute for BlockType
            //   BlockType: Dynamic Report
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Obsidian Components", "UseObsidianComponents", "Use Obsidian Components", @"Switches the filter components to use Obsidian if supported.", 0, @"True", "BDE9F174-C102-4007-9CEA-CD10B66765ED" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable AI Disclaimer", "EnableAIDisclaimer", "Enable AI Disclaimer", @"If enabled and the PrayerRequest Text was sent to an AI automation the configured AI Disclaimer will be shown.", 7, @"True", "5AE2ACC3-B0F5-49D5-B594-CF426ACB0DFF" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "9C204CD0-1233-41C5-818A-C5DA439445AA", "AI Disclaimer", "AIDisclaimer", "AI Disclaimer", @"The message to display indicating the Prayer Request text may have been modified by an AI automation.", 8, @"This request may have been modified by an AI for formatting and privacy. Please be aware that errors may be present.", "831EE48A-229E-4430-A9BC-90899D081975" );

            // Add Block Attribute Value
            //   Block: Personalization Segment List
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Block Location: Page=Personalization Segments, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "F4B62B4C-3FC6-4452-AD9A-1E1B1F67B2F3", "B6B6AD6D-50F5-48A7-B271-21B93EDA48EB", @"" );

            // Add Block Attribute Value
            //   Block: Personalization Segment List
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Block Location: Page=Personalization Segments, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F4B62B4C-3FC6-4452-AD9A-1E1B1F67B2F3", "C3127E25-BE9B-422E-B5D2-8523227FAEF4", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Dynamic Report
            //   Category: Reporting
            //   Attribute: Use Obsidian Components
            RockMigrationHelper.DeleteAttribute( "BDE9F174-C102-4007-9CEA-CD10B66765ED" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "831EE48A-229E-4430-A9BC-90899D081975" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Enable AI Disclaimer
            RockMigrationHelper.DeleteAttribute( "5AE2ACC3-B0F5-49D5-B594-CF426ACB0DFF" );

            // Delete BlockType 
            //   Name: Chat
            //   Category: Communication
            //   Path: -
            //   EntityType: General Chat
            RockMigrationHelper.DeleteBlockType( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0" );
        }
    }
}
