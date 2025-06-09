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
    public partial class CodeGenerated_20250219 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalizationSegmentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersonalizationSegmentList", "Personalization Segment List", "Rock.Blocks.Cms.PersonalizationSegmentList, Rock.Blocks, Version=17.0.37.0, Culture=neutral, PublicKeyToken=null", false, false, "18CDD594-A0E4-4190-86F5-0F7FA0B0CEDC" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationEntryWizard
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationEntryWizard", "Communication Entry Wizard", "Rock.Blocks.Communication.CommunicationEntryWizard, Rock.Blocks, Version=17.0.37.0, Culture=neutral, PublicKeyToken=null", false, false, "26917C58-C8A2-4BF5-98CB-378A02761CD7" );

            // Add/Update Obsidian Block Type
            //   Name:Personalization Segment List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersonalizationSegmentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Personalization Segment List", "Displays a list of personalization segments.", "Rock.Blocks.Cms.PersonalizationSegmentList", "CMS", "4D65B168-9FBA-4DFF-9442-6754BC4AFA48" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Entry Wizard
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationEntryWizard
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Entry Wizard", "Used for creating and sending a new communication, such as email, SMS, etc. to recipients.", "Rock.Blocks.Communication.CommunicationEntryWizard", "Communication", "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D65B168-9FBA-4DFF-9442-6754BC4AFA48", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the personalization segment details.", 0, @"", "4F1B48A5-41F7-424A-A840-A556C64658CF" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D65B168-9FBA-4DFF-9442-6754BC4AFA48", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "05C8231B-B562-44B9-B13D-CCE826A62723" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D65B168-9FBA-4DFF-9442-6754BC4AFA48", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C0DCDCBF-5184-4CAC-8410-A75FC1B0E926" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Image Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Image Binary File Type", "ImageBinaryFileType", "Image Binary File Type", @"The FileType to use for images that are added to the email using the image component", 1, @"60B896C3-F00C-411C-A31C-2D5D4CCBB65F", "5254D74F-E9C4-4825-93A9-85803006F4F1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an SMS or email communication", 2, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "8279CF6E-E60F-4C0F-BB05-7975AAC5B3E8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this HTML block.", 3, @"", "DC4D61AD-C806-46BA-9D3C-FB1687FC9157" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Communication Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Communication Types", "CommunicationTypes", "Communication Types", @"The communication types that should be available to use for the communication. (If none are selected, all will be available.) Selecting 'Recipient Preference' will automatically enable Email and SMS as mediums. Push is not an option for selection as a communication preference as delivery is not as reliable as other mediums based on an individual's privacy settings.", 4, @"", "A66198F2-2118-43B5-8A11-9DF2C7AD0340" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "Maximum Recipients", @"The maximum number of recipients allowed before communication will need to be approved.", 5, @"300", "576EF0EA-63E4-4C41-B3F2-E9DAD76129A8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "Send When Approved", @"Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 6, @"True", "0D931D61-28DA-4CA4-9610-97A131598340" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Max SMS Image Width
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max SMS Image Width", "MaxSMSImageWidth", "Max SMS Image Width", @"The maximum width (in pixels) of an image attached to a mobile communication. If its width is over the max, Rock will automatically resize image to the max width.", 7, @"600", "DDD9C8F3-1024-4561-9014-71B59560E66D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 8, @"", "05CE0252-0EE1-41AC-A6DB-7EC4105E67AA" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Simple Communication Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Simple Communication Page", "SimpleCommunicationPage", "Simple Communication Page", @"The page to use if the 'Use Simple Editor' panel heading icon is clicked. Leave this blank to not show the 'Use Simple Editor' heading icon", 9, @"", "F79DCBBA-94EC-4BC2-A92C-907AC7761C6E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 10, @"False", "4F43506E-5246-40FE-B02E-78C8F5873EB7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default As Bulk", "DefaultAsBulk", "Default As Bulk", @"Should new entries be flagged as bulk communication by default?", 11, @"False", "456EB249-56B2-400F-A45B-B2A2F236CA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.", 12, @"True", "34FE0E4A-DB17-4B7B-97AB-5B501887A4A9" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Adding Individuals to Recipient Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Adding Individuals to Recipient Lists", "DisableAddingIndividualsToRecipientLists", "Disable Adding Individuals to Recipient Lists", @"When set to 'Yes' the person picker will be hidden so that additional individuals cannot be added to the recipient list.", 13, @"False", "C814862A-C1A5-448D-9C72-4EFE2E0607C3" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Adding Individuals to Recipient Lists
            RockMigrationHelper.DeleteAttribute( "C814862A-C1A5-448D-9C72-4EFE2E0607C3" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.DeleteAttribute( "34FE0E4A-DB17-4B7B-97AB-5B501887A4A9" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.DeleteAttribute( "456EB249-56B2-400F-A45B-B2A2F236CA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.DeleteAttribute( "4F43506E-5246-40FE-B02E-78C8F5873EB7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Simple Communication Page
            RockMigrationHelper.DeleteAttribute( "F79DCBBA-94EC-4BC2-A92C-907AC7761C6E" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "05CE0252-0EE1-41AC-A6DB-7EC4105E67AA" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Max SMS Image Width
            RockMigrationHelper.DeleteAttribute( "DDD9C8F3-1024-4561-9014-71B59560E66D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.DeleteAttribute( "0D931D61-28DA-4CA4-9610-97A131598340" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.DeleteAttribute( "576EF0EA-63E4-4C41-B3F2-E9DAD76129A8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Communication Types
            RockMigrationHelper.DeleteAttribute( "A66198F2-2118-43B5-8A11-9DF2C7AD0340" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "DC4D61AD-C806-46BA-9D3C-FB1687FC9157" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.DeleteAttribute( "8279CF6E-E60F-4C0F-BB05-7975AAC5B3E8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Image Binary File Type
            RockMigrationHelper.DeleteAttribute( "5254D74F-E9C4-4825-93A9-85803006F4F1" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "C0DCDCBF-5184-4CAC-8410-A75FC1B0E926" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "05C8231B-B562-44B9-B13D-CCE826A62723" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "4F1B48A5-41F7-424A-A840-A556C64658CF" );

            // Delete BlockType 
            //   Name: Communication Entry Wizard
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Entry Wizard
            RockMigrationHelper.DeleteBlockType( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27" );

            // Delete BlockType 
            //   Name: Personalization Segment List
            //   Category: CMS
            //   Path: -
            //   EntityType: Personalization Segment List
            RockMigrationHelper.DeleteBlockType( "4D65B168-9FBA-4DFF-9442-6754BC4AFA48" );
        }
    }
}
