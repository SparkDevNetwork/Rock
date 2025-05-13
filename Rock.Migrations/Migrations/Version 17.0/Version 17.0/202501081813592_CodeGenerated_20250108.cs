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
    public partial class CodeGenerated_20250108 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SmsConversations
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SmsConversations", "Sms Conversations", "Rock.Blocks.Communication.SmsConversations, Rock.Blocks, Version=17.0.34.0, Culture=neutral, PublicKeyToken=null", false, false, "71944E38-A578-40B7-882F-A25CCBE9D408" );

            // Add/Update Obsidian Block Type
            //   Name:SMS Conversations
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SmsConversations
            RockMigrationHelper.AddOrUpdateEntityBlockType( "SMS Conversations", "Block for having SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false.", "Rock.Blocks.Communication.SmsConversations", "Communication", "3B052AC5-60DB-4490-BC47-C3471A2CA515" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 1, @"", "88EF8C95-BEC0-471B-B677-F9EF4875A378" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show only personal SMS number", "ShowOnlyPersonalSmsNumber", "Show only personal SMS number", @"Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.", 2, @"False", "4A5364F3-C471-406A-9E95-A9BA5E3533B5" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide personal SMS numbers", "HidePersonalSmsNumbers", "Hide personal SMS numbers", @"When enabled, only SMS Numbers that are not 'Assigned to a person' will be shown.", 3, @"False", "10CE5A95-EC95-4B81-94D3-01CA53D0AE8E" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Enable SMS Send
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable SMS Send", "EnableSmsSend", "Enable SMS Send", @"Allow SMS messages to be sent from the block.", 4, @"True", "E90CB432-25E6-47FA-83F5-F9342FDC1ABB" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Show Conversations From Months Ago
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Show Conversations From Months Ago", "ShowConversationsFromMonthsAgo", "Show Conversations From Months Ago", @"Limits the conversations shown in the left pane to those of X months ago or newer. This does not affect the actual messages shown on the right.", 5, @"6", "917BC8DF-6959-47AB-8967-4C2D2C4BF208" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Max Conversations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Conversations", "MaxConversations", "Max Conversations", @"Limits the number of conversations shown in the left pane. This does not affect the actual messages shown on the right.", 6, @"100", "BE44C7C5-85AA-4FD2-9D1A-858F82BA2721" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Person Info Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Person Info Lava Template", "PersonInfoLavaTemplate", "Person Info Lava Template", @"A Lava template to display person information about the selected Communication Recipient.", 7, @"{{ Person.FullName }}", "3297924A-C3FB-48A7-8696-FF4FD4C72CB2" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"Optional list of note types to limit the note editor to.", 8, @"", "9D3E1BEE-88F0-4370-BE85-AAF46533E69D" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3B052AC5-60DB-4490-BC47-C3471A2CA515", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 9, @"180", "8C434323-7809-487C-A189-08C1A663929E" );

            // Add Block Attribute Value
            //   Block: Signature Document List
            //   BlockType: Signature Document List
            //   Category: Core
            //   Block Location: Page=Document Templates, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "09493544-B05D-4FEC-9CAE-CE11EF47A0F0", "CD18809C-C4AC-489C-AB64-B27C875CB17E", @"" );

            // Add Block Attribute Value
            //   Block: Signature Document List
            //   BlockType: Signature Document List
            //   Category: Core
            //   Block Location: Page=Document Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "09493544-B05D-4FEC-9CAE-CE11EF47A0F0", "8AC3C4B3-2AB6-4E54-84DD-3BE4BDA3FA93", @"False" );

            // Add Block Attribute Value
            //   Block: Electronic Signature Documents
            //   BlockType: Signature Document List
            //   Category: Core
            //   Block Location: Page=Documents, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "EB90E536-B3E3-4767-95CB-B3671361F452", "CD18809C-C4AC-489C-AB64-B27C875CB17E", @"" );

            // Add Block Attribute Value
            //   Block: Electronic Signature Documents
            //   BlockType: Signature Document List
            //   Category: Core
            //   Block Location: Page=Documents, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "EB90E536-B3E3-4767-95CB-B3671361F452", "8AC3C4B3-2AB6-4E54-84DD-3BE4BDA3FA93", @"False" );

            // Add Block Attribute Value
            //   Block: Signature Document Type List
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Block Location: Page=Signature Documents, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "3470C25A-9115-481C-80B1-7A9B6F81556B", "41CF4B04-E1E4-4FC3-8AB6-41F5A4618BEF", @"" );

            // Add Block Attribute Value
            //   Block: Signature Document Type List
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Block Location: Page=Signature Documents, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "3470C25A-9115-481C-80B1-7A9B6F81556B", "39C34B23-3494-4A72-BE29-36677FB0BF6E", @"False" );

            // Add Block Attribute Value
            //   Block: Scheduled Job History
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Block Location: Page=Scheduled Job History, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "E59FC7AC-E91D-4D79-9D2D-C2A0A4449CB9", "32C7CDD8-A618-4A39-AA10-672F449925C3", @"" );

            // Add Block Attribute Value
            //   Block: Scheduled Job History
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Block Location: Page=Scheduled Job History, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E59FC7AC-E91D-4D79-9D2D-C2A0A4449CB9", "513B73E1-40A8-4AF4-85C3-7C78A493CCFE", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute( "8C434323-7809-487C-A189-08C1A663929E" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Note Types
            RockMigrationHelper.DeleteAttribute( "9D3E1BEE-88F0-4370-BE85-AAF46533E69D" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Person Info Lava Template
            RockMigrationHelper.DeleteAttribute( "3297924A-C3FB-48A7-8696-FF4FD4C72CB2" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Max Conversations
            RockMigrationHelper.DeleteAttribute( "BE44C7C5-85AA-4FD2-9D1A-858F82BA2721" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Show Conversations From Months Ago
            RockMigrationHelper.DeleteAttribute( "917BC8DF-6959-47AB-8967-4C2D2C4BF208" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Enable SMS Send
            RockMigrationHelper.DeleteAttribute( "E90CB432-25E6-47FA-83F5-F9342FDC1ABB" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.DeleteAttribute( "10CE5A95-EC95-4B81-94D3-01CA53D0AE8E" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.DeleteAttribute( "4A5364F3-C471-406A-9E95-A9BA5E3533B5" );

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "88EF8C95-BEC0-471B-B677-F9EF4875A378" );

            // Delete BlockType 
            //   Name: SMS Conversations
            //   Category: Communication
            //   Path: -
            //   EntityType: Sms Conversations
            RockMigrationHelper.DeleteBlockType( "3B052AC5-60DB-4490-BC47-C3471A2CA515" );
        }
    }
}
