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
    public partial class CodeGenerated_20260318 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Cms.VoiceAgent
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Cms.VoiceAgent", "Voice Agent", "Rock.Blocks.Types.Mobile.Cms.VoiceAgent, Rock, Version=19.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "8654B230-5868-4490-8832-61DBDD1FD6D4" );

            // Add/Update Mobile Block Type
            //   Name:Voice Agent
            //   Category:Mobile > Cms
            //   EntityType:Rock.Blocks.Types.Mobile.Cms.VoiceAgent
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Voice Agent", "A voice agent that help you with you everyday task.", "Rock.Blocks.Types.Mobile.Cms.VoiceAgent", "Mobile > Cms", "64B2A7B9-0C52-4C03-80DE-A9ABDD213206" );

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13955B32-11F4-4606-8C31-4C6E5324C81A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E1E0C710-1006-419B-9EBB-861F7501D604" );

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13955B32-11F4-4606-8C31-4C6E5324C81A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "43E4932D-DE22-4A58-9C4C-58DAF400A492" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment Results
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "438432E3-22A8-43D9-9F06-179C3B65D298", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "97637874-5900-43EF-9ED6-B81F823E0584" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment Results
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "438432E3-22A8-43D9-9F06-179C3B65D298", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F289B05E-96E3-40F5-BA03-88FF79240471" );

            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72C74D98-D80F-4EEE-BD14-6308EA565D7A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8A173226-FD49-4FF2-ADE3-321931F79DAF" );

            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72C74D98-D80F-4EEE-BD14-6308EA565D7A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "19A3FBBC-BC4A-411C-86C7-EB7681F9BC29" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Enable Smart Scroll
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Smart Scroll", "EnableSmartScroll", "Enable Smart Scroll", @"Determines if the block should automatically scroll the main content section to the top whenever an activity is selected.", 5, @"True", "7A8A6930-CA4C-4136-81E3-D612F5EC63BF" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Title", "IsNotBulkOptionTitle", "Personal / Need-to-Know Title", @"The title text to display for the Personal / Need-to-Know option.", 0, @"Personal / Need-to-Know", "35F8D02F-2754-45D1-B70D-8AF67A9CD983" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Description", "IsNotBulkOptionDescription", "Personal / Need-to-Know Description", @"The description text to display for the Personal / Need-to-Know option.", 1, @"Direct messages an individual expects or considers important or timely.", "D3DA7AA3-5603-4D65-9E89-3EC5C3319DF1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Title", "IsBulkOptionTitle", "Bulk / Marketing Title", @"The title text to display for the Bulk / Marketing option.", 2, @"Bulk / Marketing", "67A77398-50CB-4FA5-B49F-2F2D48C32D5E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Description", "IsBulkOptionDescription", "Bulk / Marketing Description", @"The description text to display for the Bulk / Marketing option.", 3, @"Marketing messages sent to large groups; regulated by law, and misuse can cause fines and reputational harm.", "D1817BE8-B30E-4000-9868-C5106DF801C8" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: OpenAI API Key
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "9C204CD0-1233-41C5-818A-C5DA439445AA", "OpenAI API Key", "ApiKey", "OpenAI API Key", @"The API key obtained from the OpenAI developer portal.", 0, @"", "5B619FB4-B090-40F6-B1D6-40779EA9A3A5" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: OpenAI Model
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "9C204CD0-1233-41C5-818A-C5DA439445AA", "OpenAI Model", "Model", "OpenAI Model", @"The realtime OpenAI model used for audio interactions.", 1, @"gpt-realtime-mini", "3677DDEA-CDB6-412C-8072-0C7D20991655" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Instruction
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Instruction", "Instruction", "Instruction", @"Instructions that define how the AI assistant should behave during conversations.", 2, @"
                  You are a helpful voice assistant.
                    # Tool behavior (very important)
                    - Before ANY tool call, say ONE short natural line out loud like:
                      “I’m checking that now.”
                      “One moment while I look that up.”
                      “Let me pull that up for you.”
                      “Just a second…”
                    - Then immediately call the tool. Do not ask for confirmation first.
                    - After you get the tool result, continue the conversation naturally.
                    (Keep the rest of your normal personality/instructions here...)
                ", "F5206E64-183F-479A-B99D-690DD142E657" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Rock MCP
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Rock MCP", "RockMcp", "Rock MCP", @"Select an MCP agent configured in Rock. This agent will be available to the AI assistant.", 3, @"", "CE7CDA51-7085-435C-A727-D90E3F5A4BC8" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: External MCP
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "7BDAE237-6E49-47AC-9961-A45AFB69E240", "External MCP", "ExternalMcp", "External MCP", @"Enter one or more MCP server URLs from external systems that should be available to the AI agent.", 4, @"", "60EAD3F0-1B12-4493-8BA3-49D04BF2C7F1" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: External MCP
            RockMigrationHelper.DeleteAttribute( "60EAD3F0-1B12-4493-8BA3-49D04BF2C7F1" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Rock MCP
            RockMigrationHelper.DeleteAttribute( "CE7CDA51-7085-435C-A727-D90E3F5A4BC8" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Instruction
            RockMigrationHelper.DeleteAttribute( "F5206E64-183F-479A-B99D-690DD142E657" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: OpenAI Model
            RockMigrationHelper.DeleteAttribute( "3677DDEA-CDB6-412C-8072-0C7D20991655" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: OpenAI API Key
            RockMigrationHelper.DeleteAttribute( "5B619FB4-B090-40F6-B1D6-40779EA9A3A5" );

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "43E4932D-DE22-4A58-9C4C-58DAF400A492" );

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E1E0C710-1006-419B-9EBB-861F7501D604" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Enable Smart Scroll
            RockMigrationHelper.DeleteAttribute( "7A8A6930-CA4C-4136-81E3-D612F5EC63BF" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.DeleteAttribute( "D1817BE8-B30E-4000-9868-C5106DF801C8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.DeleteAttribute( "67A77398-50CB-4FA5-B49F-2F2D48C32D5E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.DeleteAttribute( "D3DA7AA3-5603-4D65-9E89-3EC5C3319DF1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.DeleteAttribute( "35F8D02F-2754-45D1-B70D-8AF67A9CD983" );

            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "19A3FBBC-BC4A-411C-86C7-EB7681F9BC29" );

            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "8A173226-FD49-4FF2-ADE3-321931F79DAF" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment Results
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F289B05E-96E3-40F5-BA03-88FF79240471" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment Results
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "97637874-5900-43EF-9ED6-B81F823E0584" );

            // Delete BlockType 
            //   Name: Voice Agent
            //   Category: Mobile > Cms
            //   Path: -
            //   EntityType: Voice Agent
            RockMigrationHelper.DeleteBlockType( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206" );
        }
    }
}
