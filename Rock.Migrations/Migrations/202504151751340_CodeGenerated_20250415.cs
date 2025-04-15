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
    public partial class CodeGenerated_20250415 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationTemplateDetail", "Communication Template Detail", "Rock.Blocks.Communication.CommunicationTemplateDetail, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "017EEC30-BDDA-4159-8249-2852AF4ADCF2" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Template Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Template Detail", "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people.", "Rock.Blocks.Communication.CommunicationTemplateDetail", "Communication", "FBAB4EB2-B180-4A76-9B5B-C75E2255F691" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Navigation Shortcuts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Navigation Shortcuts", "DisableNavigationShortcuts", "Disable Navigation Shortcuts", @"When enabled, the block will turn off the keyboard shortcuts (arrow keys) used to navigate the steps.", 100, @"False", "F4084314-8F75-44E7-9E4A-A751EA2CA60B" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Show Summary View
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "Show Summary View", @"If workflow has been completed, should the summary view be displayed?", 1, @"False", "F4261537-4A96-4061-963F-36C5827A7770" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Block Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Block Title Template", "BlockTitleTemplate", "Block Title Template", @"Lava template for determining the title of the block. If not specified, the name of the Workflow Type will be shown.", 2, @"", "1C9F3F68-1B33-4C6B-A377-3C568044077A" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCssClass", "Block Title Icon CSS Class", @"The CSS class for the icon displayed in the block title. If not specified, the icon for the Workflow Type will be shown.", 3, @"", "5295E9EB-E8C7-45E4-BD62-FD11B40BE0A5" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Passing WorkflowId
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Passing WorkflowId", "DisablePassingWorkflowId", "Disable Passing WorkflowId", @"If disabled, prevents the use of a Workflow Id (WorkflowId=) from being passed in and only accepts a WorkflowGuid.", 4, @"False", "78431245-136F-4C6E-A652-D3EB4E9CB136" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Passing WorkflowTypeId
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Passing WorkflowTypeId", "DisablePassingWorkflowTypeId", "Disable Passing WorkflowTypeId", @"If set, it prevents the use of a Workflow Type Id (WorkflowTypeId=) from being passed in and only accepts a WorkflowTypeGuid. To use this block setting on your external site, you will need to create a new page and add the Workflow Entry block to it. You may also add a new route so that URLs are in the pattern .../{PageRoute}/{WorkflowTypeGuid}. If your workflow uses a form, you will also need to adjust email content to ensure that your URLs are correct.", 5, @"False", "814997C4-0BCA-406D-98D9-719FAD2EB1BB" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Log Interaction when Form is Viewed
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interaction when Form is Viewed", "LogInteractionOnView", "Log Interaction when Form is Viewed", @"", 6, @"True", "B1295D90-6D26-4216-84A4-3E6E40BB6C95" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Log Interaction when Form is Completed
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interaction when Form is Completed", "LogInteractionOnCompletion", "Log Interaction when Form is Completed", @"", 7, @"True", "CF184A23-7854-4997-9F8B-AC64FB83324D" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 8, @"False", "A60CAEAB-368B-4F7F-A0F0-5B2C7DBB0D66" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable ACH
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable ACH", "EnableACH", "Enable ACH", @"Enabling this will also control which type of Saved Accounts can be used during the payment process.  The payment gateway must still be configured to support ACH payments.", 12, @"True", "1B62AA70-64C9-4253-B16B-13C626C5F0D5" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable Credit Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Credit Card", "EnableCreditCard", "Enable Credit Card", @"Enabling this will also control which type of Saved Accounts can be used during the payment process.  The payment gateway must still be configured to support Credit Card payments.", 13, @"True", "C112D2E6-7A1B-47ED-8E91-38D70534202A" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system communication used to send emails to the scheduler for each confirmation or decline. If a Group's ""Schedule Coordinator"" is defined, this will also be used when sending emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule).", 2, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "196E7B91-9DBD-4B5A-A108-346BC5D59318" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"When enabled, the scheduler will receive an email for each confirmation or decline. Note that if a Group's ""Schedule Coordinator"" is defined, that person will automatically receive emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule), regardless of this setting.", 2, @"False", "AB7D5E44-C04E-4AB4-B754-33E8357BD389" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system communication used to send emails to the scheduler for each confirmation or decline. If a Group's ""Schedule Coordinator"" is defined, this will also be used when sending emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule).", 2, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "A106C594-0557-4715-AAC8-FEB0144057EC" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"When enabled, the scheduler will receive an email for each confirmation or decline. Note that if a Group's ""Schedule Coordinator"" is defined, that person will automatically receive emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule), regardless of this setting.", 2, @"False", "91FF8EA6-E80B-4537-B82B-A3E9A0047D0F" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Minimum Short Link Token Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Short Link Token Length", "MinimumShortLinkTokenLength", "Minimum Short Link Token Length", @"The minimum number of characters for short link tokens.", 15, @"7", "B814AAE9-168C-43A0-B59E-B7320A6FE965" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Navigation Shortcuts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Navigation Shortcuts", "DisableNavigationShortcuts", "Disable Navigation Shortcuts", @"When enabled, the block will turn off the keyboard shortcuts (arrow keys) used to navigate the steps.", 100, @"False", "2A167571-2B6F-420C-BC81-4C59DFAB2858" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Filter Shared Channels by Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "723A3F70-87DC-4BA0-A6FB-0AC15B1865B0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Shared Channels by Campus", "FilterSharedChannelsByCampus", "Filter Shared Channels by Campus", @"Only show channels that match the individual's campus or have no campus set.", 0, @"False", "0E9134E1-F2A1-4BA5-9A08-21B14D1BE057" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Personal Templates View
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Personal Templates View", "PersonalTemplatesView", "Personal Templates View", @"Is this block being used to display personal templates (only templates that current user is allowed to edit)?", 0, @"False", "655D88DE-04B8-49F6-B994-7F16E4B88E4E" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an sms or email communication", 1, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "78CC588D-D5DA-4FAD-AEDF-E37719454320" );

            // Add Block Attribute Value
            //   Block: Attribute Matrix Template List
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Block Location: Page=Attribute Matrix Templates, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "A8649FAC-254C-4B9A-A5C4-DE10EE8C3A51", "CED937FF-D736-4B50-8A73-A66FD113C19D", @"" );

            // Add Block Attribute Value
            //   Block: Attribute Matrix Template List
            //   BlockType: Attribute Matrix Template List
            //   Category: Core
            //   Block Location: Page=Attribute Matrix Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "A8649FAC-254C-4B9A-A5C4-DE10EE8C3A51", "A955DE7F-A809-4830-92DA-5010DEF74317", @"False" );

            // Add Block Attribute Value
            //   Block: Device List
            //   BlockType: Device List
            //   Category: Core
            //   Block Location: Page=Devices, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EFCFAEF9-1178-4AE0-93CD-F8D0B01770F7", "B17AD9CA-075D-49C9-94E5-977806C881FD", @"" );

            // Add Block Attribute Value
            //   Block: Device List
            //   BlockType: Device List
            //   Category: Core
            //   Block Location: Page=Devices, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EFCFAEF9-1178-4AE0-93CD-F8D0B01770F7", "B7A694A0-7E39-4CD7-9623-F1B6939A2A08", @"False" );

            // Add Block Attribute Value
            //   Block: Device List
            //   BlockType: Device List
            //   Category: Core
            //   Block Location: Page=Devices, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "61A16DC4-BF6A-46E9-8EEC-EB90AA62B0C2", "B7A694A0-7E39-4CD7-9623-F1B6939A2A08", @"False" );

            // Add Block Attribute Value
            //   Block: Device List
            //   BlockType: Device List
            //   Category: Core
            //   Block Location: Page=Devices, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "61A16DC4-BF6A-46E9-8EEC-EB90AA62B0C2", "B17AD9CA-075D-49C9-94E5-977806C881FD", @"" );

            // Add Block Attribute Value
            //   Block: Gateway List
            //   BlockType: Gateway List
            //   Category: Finance
            //   Block Location: Page=Financial Gateways, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C3A13BFA-676A-4FA9-BDF0-17B3E8382EB7", "2F98AE3F-5C72-4221-9AF9-1E52F482F957", @"" );

            // Add Block Attribute Value
            //   Block: Gateway List
            //   BlockType: Gateway List
            //   Category: Finance
            //   Block Location: Page=Financial Gateways, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C3A13BFA-676A-4FA9-BDF0-17B3E8382EB7", "DA317290-5666-4D98-928A-2BBB99541022", @"False" );

            // Add Block Attribute Value
            //   Block: Persisted Dataset List
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Block Location: Page=Persisted Datasets, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C8D3E2C2-536B-490B-8CB4-6FFCC5ADBF58", "BF65CDE2-A430-4EA1-94A7-9D829ACC0174", @"" );

            // Add Block Attribute Value
            //   Block: Persisted Dataset List
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Block Location: Page=Persisted Datasets, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C8D3E2C2-536B-490B-8CB4-6FFCC5ADBF58", "0C35A18C-AE08-438F-B00A-1D6A8EFBD397", @"False" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Block Location: Page=Contribution Templates, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "94429A13-AA5A-49BC-B0C4-043648298D00", "E43FC825-346B-49ED-9696-20116E13127A", @"" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Category: Finance
            //   Block Location: Page=Contribution Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "94429A13-AA5A-49BC-B0C4-043648298D00", "EDA3EB4C-9F38-44CB-9BE7-A884B643B96E", @"False" );

            // Add Block Attribute Value
            //   Block: Content Type List
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Block Location: Page=Content Channel Types, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DED9CA32-94F1-4123-ABB5-4A222F100F70", "B293B94F-5814-4142-B4CC-D8A1E53E5A86", @"" );

            // Add Block Attribute Value
            //   Block: Content Type List
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Block Location: Page=Content Channel Types, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DED9CA32-94F1-4123-ABB5-4A222F100F70", "897E5F60-46E8-446E-9CE5-0844FCE010E3", @"False" );

            // Add Block Attribute Value
            //   Block: Rest Key List
            //   BlockType: Rest Key List
            //   Category: Security
            //   Block Location: Page=REST Keys, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D47EF521-011A-499F-A874-345F83F3DDE9", "C35FDCB0-4C15-43FA-872C-6A1B69622F23", @"" );

            // Add Block Attribute Value
            //   Block: Rest Key List
            //   BlockType: Rest Key List
            //   Category: Security
            //   Block Location: Page=REST Keys, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D47EF521-011A-499F-A874-345F83F3DDE9", "85E254DD-2802-4A29-A63C-11804819598A", @"False" );

            // Add Block Attribute Value
            //   Block: Tag List
            //   BlockType: Tag List
            //   Category: Core
            //   Block Location: Page=Tags, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "452F9226-6996-4705-81D4-5ED035ECE0A3", "56D955B1-3255-4682-A6A1-152EEC621768", @"" );

            // Add Block Attribute Value
            //   Block: Tag List
            //   BlockType: Tag List
            //   Category: Core
            //   Block Location: Page=Tags, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "452F9226-6996-4705-81D4-5ED035ECE0A3", "4B1EE06C-BFCA-40A0-B940-446B5D8B225C", @"False" );

            // Add Block Attribute Value
            //   Block: Person Badge List
            //   BlockType: Badge List
            //   Category: CRM
            //   Block Location: Page=Badges, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DAAFF644-B6B2-4D41-962A-023E1B117022", "BD8F7469-DAE5-4DFF-ACA8-32652F552FA3", @"" );

            // Add Block Attribute Value
            //   Block: Person Badge List
            //   BlockType: Badge List
            //   Category: CRM
            //   Block Location: Page=Badges, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "DAAFF644-B6B2-4D41-962A-023E1B117022", "286C19D0-D450-4D94-B13F-FEAF7476F248", @"False" );
            RockMigrationHelper.UpdateFieldType( "Financial Transaction", "", "Rock", "Rock.Field.Types.FinancialTransactionFieldType", "04C66B8E-2DBD-4799-875E-FFFD818EDD91" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "A60CAEAB-368B-4F7F-A0F0-5B2C7DBB0D66" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Log Interaction when Form is Completed
            RockMigrationHelper.DeleteAttribute( "CF184A23-7854-4997-9F8B-AC64FB83324D" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Log Interaction when Form is Viewed
            RockMigrationHelper.DeleteAttribute( "B1295D90-6D26-4216-84A4-3E6E40BB6C95" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Passing WorkflowTypeId
            RockMigrationHelper.DeleteAttribute( "814997C4-0BCA-406D-98D9-719FAD2EB1BB" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Disable Passing WorkflowId
            RockMigrationHelper.DeleteAttribute( "78431245-136F-4C6E-A652-D3EB4E9CB136" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "5295E9EB-E8C7-45E4-BD62-FD11B40BE0A5" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Block Title Template
            RockMigrationHelper.DeleteAttribute( "1C9F3F68-1B33-4C6B-A377-3C568044077A" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Worfklow
            //   Attribute: Show Summary View
            RockMigrationHelper.DeleteAttribute( "F4261537-4A96-4061-963F-36C5827A7770" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.DeleteAttribute( "AB7D5E44-C04E-4AB4-B754-33E8357BD389" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.DeleteAttribute( "196E7B91-9DBD-4B5A-A108-346BC5D59318" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.DeleteAttribute( "91FF8EA6-E80B-4537-B82B-A3E9A0047D0F" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.DeleteAttribute( "A106C594-0557-4715-AAC8-FEB0144057EC" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable Credit Card
            RockMigrationHelper.DeleteAttribute( "C112D2E6-7A1B-47ED-8E91-38D70534202A" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable ACH
            RockMigrationHelper.DeleteAttribute( "1B62AA70-64C9-4253-B16B-13C626C5F0D5" );

            // Attribute for BlockType
            //   BlockType: Chat View
            //   Category: Communication
            //   Attribute: Filter Shared Channels by Campus
            RockMigrationHelper.DeleteAttribute( "0E9134E1-F2A1-4BA5-9A08-21B14D1BE057" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.DeleteAttribute( "78CC588D-D5DA-4FAD-AEDF-E37719454320" );

            // Attribute for BlockType
            //   BlockType: Communication Template Detail
            //   Category: Communication
            //   Attribute: Personal Templates View
            RockMigrationHelper.DeleteAttribute( "655D88DE-04B8-49F6-B994-7F16E4B88E4E" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.DeleteAttribute( "12AAF6D2-BCA6-480B-AA44-9C77A3CEC4A1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Navigation Shortcuts
            RockMigrationHelper.DeleteAttribute( "F4084314-8F75-44E7-9E4A-A751EA2CA60B" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Navigation Shortcuts
            RockMigrationHelper.DeleteAttribute( "2A167571-2B6F-420C-BC81-4C59DFAB2858" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Minimum Short Link Token Length
            RockMigrationHelper.DeleteAttribute( "B814AAE9-168C-43A0-B59E-B7320A6FE965" );

            // Delete BlockType 
            //   Name: Communication Template Detail
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Template Detail
            RockMigrationHelper.DeleteBlockType( "FBAB4EB2-B180-4A76-9B5B-C75E2255F691" );
        }
    }
}
