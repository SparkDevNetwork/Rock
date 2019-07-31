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
    public partial class Rollup_0605 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdatePushNotificationEntityGuid();
            AddAllowLabelReprintingUp();
            AddAttachmentBinaryFileTypeAttribute();
            FixCurrencyFormatInContributionStatement();
            AddAttributesForSmartyStreets();
            FixGoogleShortCode();
            CodeGenMigrationsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            AddAllowLabelRepringintDown();
        }

        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Event Registration Wizard","A wizard to simplify creation of Event Registrations.","~/Blocks/Event/EventRegistrationWizard.ascx","Event","B1C7E983-5000-4CBE-84DD-6B7D428635AC");
            RockMigrationHelper.UpdateBlockType("Financial Gateway Migration Utility","Tool to assist in migrating records from NMI a Pi.","~/Blocks/Finance/GatewayMigrationUtility.ascx","Finance","4AB0A56B-C367-40ED-8C69-48D9A9260553");
            // Attrib for BlockType: Event Registration Wizard:Check-In Group Types
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","F725B854-A15E-46AE-9D4C-0608D4154F1E","Check-In Group Types","CheckInGroupTypes","",@"Select group types which should enable check-in.  If the selected registration template is one of these types, check-in options will be enabled for the group.",10,@"","B664EC4A-ACCA-4301-8173-76DD14BCFB5D");
            // Attrib for BlockType: Event Registration Wizard:Group Viewer Page
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Group Viewer Page","GroupViewerPage","",@"Determines which page the link in the final confirmation screen will take you to.",4,@"4E237286-B715-4109-A578-C1445EC02707","D03BE1B7-5033-41D1-A736-57E96C379578");
            // Attrib for BlockType: Event Registration Wizard:Require Group
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Group","RequireGroup","",@"If set to ""Yes"", you will be required to create a new group.",5,@"False","B614A491-CA7D-4615-A1A1-025E5A82B526");
            // Attrib for BlockType: Event Registration Wizard:Set Registration Instance Active
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Set Registration Instance Active","SetRegistrationInstanceActive","",@"If set to ""No"", the new registration instance will be created, but marked as ""inactive"".",5,@"True","29F3FA35-5196-4435-AA81-F3C4D195EA2C");
            // Attrib for BlockType: Event Registration Wizard:Enable Calendar Events
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Calendar Events","EnableCalendarEvents","",@"If calendar events are not enabled, registrations and groups will be created and linked, but not linked to any calendar event.",6,@"True","5B7266F0-070D-4685-9F8E-06A867BD00A2");
            // Attrib for BlockType: Event Registration Wizard:Root Group
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","F4399CEF-827B-48B2-A735-F7806FCFE8E8","Root Group","RootGroup","",@"This is the ""root"" of the group tree which will be offered for the staff person to pick the parent group from – limiting where the new group can be created.",3,@"","CE788DEB-D465-47A4-B869-FCA737676D05");
            // Attrib for BlockType: Event Registration Wizard:Require Calendar Events
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Calendar Events","RequireCalendarEvents","",@"If calendar events are enabled and required, you must either select an existing calendar event or create a new one, and you must create an event occurrence.",8,@"True","B6A920CF-02EE-400C-A17A-3EE7AE18DB18");
            // Attrib for BlockType: Event Registration Wizard:Completion Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Completion Workflow","CompletionWorkflow","",@"A workflow that will be launched when a new registration is created.",9,@"","EC4FAB68-5BF2-43DC-A7DE-90F4A41D0E4C");
            // Attrib for BlockType: Event Registration Wizard:Registration Template Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Registration Template Instructions Lava Template","LavaInstruction_InitiateWizard","",@"Instructions here will show up on the first panel of the wizard.",1,@"","F6349656-A06D-4FE8-9E63-607BF5EB4C30");
            // Attrib for BlockType: Event Registration Wizard:Registration Instance Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Registration Instance Instructions Lava Template","LavaInstruction_Registration","",@"Instructions here will show up on the second panel of the wizard.",2,@"","95CA8091-2C40-411A-992F-181434062702");
            // Attrib for BlockType: Event Registration Wizard:Group Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Group Instructions Lava Template","LavaInstruction_Group","",@"Instructions here will show up on the third panel of the wizard.",3,@"","446305AC-35B8-4014-8F0E-CBB15908262E");
            // Attrib for BlockType: Event Registration Wizard:Event Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Event Instructions Lava Template","LavaInstruction_Event","",@"Instructions here will show up on the fourth panel of the wizard.",4,@"","23CA7E8F-D994-4FB7-8448-86C7CF886DC7");
            // Attrib for BlockType: Event Registration Wizard:Event Occurrence Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Event Occurrence Instructions Lava Template","LavaInstruction_EventOccurrence","",@"Instructions here will show up on the fifth panel of the wizard.",5,@"","ECC2286E-8D17-45DF-88DE-303DACCD8649");
            // Attrib for BlockType: Event Registration Wizard:Summary Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Summary Instructions Lava Template","LavaInstruction_Summary","",@"Instructions here will show up on the sixth panel of the wizard.",6,@"","D47A11E5-5EC0-476C-A3CE-133D7E36FCBF");
            // Attrib for BlockType: Event Registration Wizard:Wizard Finished Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Wizard Finished Instructions Lava Template","LavaInstruction_Finished","",@"Instructions here will show up on the final panel of the wizard.",7,@"","5B0A0148-75FA-417E-89AE-7EF679449EFD");
            // Attrib for BlockType: Event Registration Wizard:Allow Creating New Calendar Events
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Creating New Calendar Events","AllowCreatingNewCalendarEvents","",@"If set to ""Yes"", the staff person will be offered the ""New Event"" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.",7,@"False","2CCFC3D6-4DE3-4C4F-9462-FED6EE87E96D");
            // Attrib for BlockType: Event Registration Wizard:Default Account
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A","Default Account","DefaultAccount","",@"Select the default financial account which will be pre-filled if a cost is set on the new registration instance.",0,@"","71740751-8D07-42C0-8C28-D437AD99AA97");
            // Attrib for BlockType: Event Registration Wizard:Default Calendar
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","EC0D9528-1A22-404E-A776-566404987363","Default Calendar","DefaultCalendar","",@"The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.",1,@"","5EC64760-5720-4E5A-91B7-EC02BE6084C7");
            // Attrib for BlockType: Event Registration Wizard:Available Registration Templates
            RockMigrationHelper.UpdateBlockTypeAttribute("B1C7E983-5000-4CBE-84DD-6B7D428635AC","F56DED5E-C135-42B2-A529-878CB30436B5","Available Registration Templates","AvailableRegistrationTemplates","",@"The list of templates the staff person can pick from – not all templates need to be available to all blocks.",2,@"","EC13129F-473E-4894-9353-7A80B788FF2E");
            // Attrib for BlockType: Lava Tester:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute("D7EE4A94-29BF-4617-ADF0-1493522BA7E9","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","",@"The Lava commands that should be enabled.",0,@"","80283AF4-8BEB-46A0-9B8E-C5EDE6FC8129");
            // Attrib for BlockType: Connection Request Detail:SMS Link Page
            RockMigrationHelper.UpdateBlockTypeAttribute("A7961C9C-2EF5-44DF-BEA5-C334B42A90E2","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","SMS Link Page","SmsLinkPage","",@"Page that will be linked for SMS enabled phones.",4,@"2A22D08D-73A8-4AAF-AC7E-220E8B2E7857","E1D32323-8592-43B7-BE74-16472CBF9DA7");
            // Attrib for BlockType: Add Group:Show County
            RockMigrationHelper.UpdateBlockTypeAttribute("DE156975-597A-4C55-A649-FE46712F91C3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show County","ShowCounty","",@"Should County be displayed when editing an address?.",14,@"False","EBB59583-6022-4840-ACAA-C128E2AAB69A");
            // Attrib for BlockType: Group Schedule Status Board:Number Of Weeks (Max 16)
            RockMigrationHelper.UpdateBlockTypeAttribute("1BFB72CC-A224-4A0B-B291-21733597738A","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Number Of Weeks (Max 16)","FutureWeeksToShow","",@"How many weeks into the future should be displayed.",0,@"2","E08B014C-ACE4-4632-942B-4267E7975EE3");
            // Attrib for BlockType: Group Schedule Toolbox:Enable Sign-up
            RockMigrationHelper.UpdateBlockTypeAttribute("7F9CEA6F-DCE5-4F60-A551-924965289F1D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Sign-up","EnableSignup","",@"Set to false to hide the Sign Up tab.",1,@"True","1CAFAD2C-F4CA-4EB2-A3AF-0594A97B149C");
            RockMigrationHelper.UpdateFieldType("Registration Templates","","Rock","Rock.Field.Types.RegistrationTemplatesFieldType","F56DED5E-C135-42B2-A529-878CB30436B5");
        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Group Schedule Toolbox:Enable Sign-up
            RockMigrationHelper.DeleteAttribute("1CAFAD2C-F4CA-4EB2-A3AF-0594A97B149C");
            // Attrib for BlockType: Group Schedule Status Board:Number Of Weeks (Max 16)
            RockMigrationHelper.DeleteAttribute("E08B014C-ACE4-4632-942B-4267E7975EE3");
            // Attrib for BlockType: Add Group:Show County
            RockMigrationHelper.DeleteAttribute("EBB59583-6022-4840-ACAA-C128E2AAB69A");
            // Attrib for BlockType: Connection Request Detail:SMS Link Page
            RockMigrationHelper.DeleteAttribute("E1D32323-8592-43B7-BE74-16472CBF9DA7");
            // Attrib for BlockType: Lava Tester:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("80283AF4-8BEB-46A0-9B8E-C5EDE6FC8129");
            // Attrib for BlockType: Event Registration Wizard:Available Registration Templates
            RockMigrationHelper.DeleteAttribute("EC13129F-473E-4894-9353-7A80B788FF2E");
            // Attrib for BlockType: Event Registration Wizard:Default Calendar
            RockMigrationHelper.DeleteAttribute("5EC64760-5720-4E5A-91B7-EC02BE6084C7");
            // Attrib for BlockType: Event Registration Wizard:Default Account
            RockMigrationHelper.DeleteAttribute("71740751-8D07-42C0-8C28-D437AD99AA97");
            // Attrib for BlockType: Event Registration Wizard:Allow Creating New Calendar Events
            RockMigrationHelper.DeleteAttribute("2CCFC3D6-4DE3-4C4F-9462-FED6EE87E96D");
            // Attrib for BlockType: Event Registration Wizard:Wizard Finished Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("5B0A0148-75FA-417E-89AE-7EF679449EFD");
            // Attrib for BlockType: Event Registration Wizard:Summary Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("D47A11E5-5EC0-476C-A3CE-133D7E36FCBF");
            // Attrib for BlockType: Event Registration Wizard:Event Occurrence Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("ECC2286E-8D17-45DF-88DE-303DACCD8649");
            // Attrib for BlockType: Event Registration Wizard:Event Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("23CA7E8F-D994-4FB7-8448-86C7CF886DC7");
            // Attrib for BlockType: Event Registration Wizard:Group Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("446305AC-35B8-4014-8F0E-CBB15908262E");
            // Attrib for BlockType: Event Registration Wizard:Registration Instance Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("95CA8091-2C40-411A-992F-181434062702");
            // Attrib for BlockType: Event Registration Wizard:Registration Template Instructions Lava Template
            RockMigrationHelper.DeleteAttribute("F6349656-A06D-4FE8-9E63-607BF5EB4C30");
            // Attrib for BlockType: Event Registration Wizard:Completion Workflow
            RockMigrationHelper.DeleteAttribute("EC4FAB68-5BF2-43DC-A7DE-90F4A41D0E4C");
            // Attrib for BlockType: Event Registration Wizard:Require Calendar Events
            RockMigrationHelper.DeleteAttribute("B6A920CF-02EE-400C-A17A-3EE7AE18DB18");
            // Attrib for BlockType: Event Registration Wizard:Root Group
            RockMigrationHelper.DeleteAttribute("CE788DEB-D465-47A4-B869-FCA737676D05");
            // Attrib for BlockType: Event Registration Wizard:Enable Calendar Events
            RockMigrationHelper.DeleteAttribute("5B7266F0-070D-4685-9F8E-06A867BD00A2");
            // Attrib for BlockType: Event Registration Wizard:Set Registration Instance Active
            RockMigrationHelper.DeleteAttribute("29F3FA35-5196-4435-AA81-F3C4D195EA2C");
            // Attrib for BlockType: Event Registration Wizard:Require Group
            RockMigrationHelper.DeleteAttribute("B614A491-CA7D-4615-A1A1-025E5A82B526");
            // Attrib for BlockType: Event Registration Wizard:Group Viewer Page
            RockMigrationHelper.DeleteAttribute("D03BE1B7-5033-41D1-A736-57E96C379578");
            // Attrib for BlockType: Event Registration Wizard:Check-In Group Types
            RockMigrationHelper.DeleteAttribute("B664EC4A-ACCA-4301-8173-76DD14BCFB5D");
            RockMigrationHelper.DeleteBlockType("4AB0A56B-C367-40ED-8C69-48D9A9260553"); // Financial Gateway Migration Utility
            RockMigrationHelper.DeleteBlockType("B1C7E983-5000-4CBE-84DD-6B7D428635AC"); // Event Registration Wizard
        }

        /// <summary>
        /// NA: Migration from PR #3729
        /// </summary>
        private void UpdatePushNotificationEntityGuid()
        {
            // PR #3729 Update the PushNotification communication medium entity to have correct Guid
            RockMigrationHelper.UpdateEntityType( "Rock.Communication.Medium.PushNotification", "3638C6DF-4FF3-4A52-B4B8-AFB754991597", false, true );
        }

        /// <summary>
        /// NA: Add new Check-in Manager Person Profile block setting for Allow Label Reprinting and corresponding "Unattended Check-in" Workflow Setting
        /// </summary>
        private void AddAllowLabelReprintingUp()
        {
            // Add Welcome and Person Block attributes...   
            // Attrib for BlockType: Person Profile:Allow Label Reprinting
            RockMigrationHelper.UpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Label Reprinting", "AllowLabelReprinting", "", @" Determines if reprinting labels should be allowed.", 5, @"False", "B3383AA4-02A2-47EF-8B78-E25B86AF4C6E" );

            // Unattended Workflow Type changes for label-reprint
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8F348E7B-F9FD-4600-852D-477B13B0B4EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Saving Label Data", "EnableSavingLabelData", "Select 'Yes' if the label data should be temporarily saved on the attendance record. Select 'No' to disable saving label data.", 0, @"True", "7A6BAAB1-13A7-4F23-8CBB-0181C3620A08" ); // Rock.Workflow.Action.CheckIn.CreateLabels:Enable Saving Label Data

            RockMigrationHelper.AddActionTypeAttributeValue( "E53EF6B3-3325-4CBA-A6F1-B1A9EAE62CAA", "7A6BAAB1-13A7-4F23-8CBB-0181C3620A08", @"True" ); // Unattended Check-in:Save Attendance:Create Labels:Enable Saving Label Data
        }

        /// <summary>
        /// Revert NA: Add new Check-in Manager Person Profile block setting for Allow Label Reprinting and corresponding "Unattended Check-in" Workflow Setting
        /// </summary>
        private void AddAllowLabelRepringintDown()
        {
            // Remove Welcome and Person Block attributes   
            // Attrib for BlockType: Person Profile:Allow Label Reprinting
            RockMigrationHelper.DeleteAttribute( "B3383AA4-02A2-47EF-8B78-E25B86AF4C6E" );

            // Unattended Workflow Type changes for label-reprint
            RockMigrationHelper.DeleteAttribute( "7A6BAAB1-13A7-4F23-8CBB-0181C3620A08" );
        }

        /// <summary>
        /// SK: Added Attachment Binary File Type Setting to Communication Entry and Template Detail
        /// </summary>
        private void AddAttachmentBinaryFileTypeAttribute()
        {
            // Attrib for BlockType: Template Detail:Attachment Binary File Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "", @"The FileType to use for files that are attached to an sms or email communication", 1, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "C3AAC427-F7B9-410F-B8FE-087B0679F723" );

            // Attrib for BlockType: Communication Entry:Attachment Binary File Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "", @"The FileType to use for files that are attached to an sms or email communication", 11, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "381EA0DC-1139-460B-B4FD-BCB80AD7C6A5" );
        }

        /// <summary>
        /// SK: Fixed the currency formatting in Contribution Statement 
        /// </summary>
        private void FixCurrencyFormatInContributionStatement()
        {
            Sql( @"
            DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid]='22DEED01-F70B-4AF7-AF34-887E0C18E8FD')
            
            UPDATE [Attribute] 
            SET [DefaultValue]=REPLACE([DefaultValue],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ transaction.Amount }}','{{ transaction.Amount | FormatAsCurrency }}')
            WHERE [Id]=@AttributeId

            UPDATE [AttributeValue] 
            SET [Value]=REPLACE([Value],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ transaction.Amount }}','{{ transaction.Amount | FormatAsCurrency }}')
            WHERE [AttributeId]=@AttributeId

            UPDATE [Attribute] 
            SET [DefaultValue]=REPLACE([DefaultValue],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ accountsummary.Total }}','{{ accountsummary.Total | FormatAsCurrency }}')
            WHERE [Id]=@AttributeId

            UPDATE [AttributeValue] 
            SET [Value]=REPLACE([Value],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ accountsummary.Total }}','{{ accountsummary.Total | FormatAsCurrency }}')
            WHERE [AttributeId]=@AttributeId

            UPDATE [Attribute] 
            SET [DefaultValue]=REPLACE([DefaultValue],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountPledged }}','{{ pledge.AmountPledged | FormatAsCurrency }}')
            WHERE [Id]=@AttributeId

            UPDATE [AttributeValue] 
            SET [Value]=REPLACE([Value],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountPledged }}','{{ pledge.AmountPledged | FormatAsCurrency }}')
            WHERE [AttributeId]=@AttributeId

            UPDATE [Attribute] 
            SET [DefaultValue]=REPLACE([DefaultValue],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountGiven }}','{{ pledge.AmountGiven | FormatAsCurrency }}')
            WHERE [Id]=@AttributeId

            UPDATE [AttributeValue] 
            SET [Value]=REPLACE([Value],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountGiven }}','{{ pledge.AmountGiven | FormatAsCurrency }}')
            WHERE [AttributeId]=@AttributeId

            UPDATE [Attribute] 
            SET [DefaultValue]=REPLACE([DefaultValue],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountRemaining }}','{{ pledge.AmountRemaining | FormatAsCurrency }}')
            WHERE [Id]=@AttributeId

            UPDATE [AttributeValue] 
            SET [Value]=REPLACE([Value],'{{ ''Global'' | Attribute:''CurrencySymbol'' }}{{ pledge.AmountRemaining }}','{{ pledge.AmountRemaining | FormatAsCurrency }}')
            WHERE [AttributeId]=@AttributeId" );
        }

        /// <summary>
        /// SK: Add attributes for Smarty Streets
        /// </summary>
        private void AddAttributesForSmartyStreets()
        {
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "The order that this service should be used (priority)", 0, @"", "BB38037E-399B-4211-8A87-4E908E3953A1", "Order" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "Should Service be used?", 0, @"False", "48F4598D-94B7-4131-9BAF-8F54308EE5FE", "Active" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Use Managed API Key", "Enable this to use the Auth ID and Auth Token that is managed by Spark.", 1, @"True", "63844379-3CA1-450D-8D03-56144F6B5912", "UseManagedAPIKey" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Auth ID", "The Smarty Streets Authorization ID. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.", 2, @"", "04663026-D7A8-428E-93AD-BF524E643CC5", "AuthID" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Auth Token", "The Smarty Streets Authorization Token. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.", 3, @"", "99ECB1C8-4C1A-4DFA-B11B-9F62786A8565", "AuthToken" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Acceptable DPV Codes", "The Smarty Streets Delivery Point Validation (DPV) match code values that are considered acceptable levels of standardization (see http://smartystreets.com/kb/liveaddress-api/field-definitions#dpvmatchcode for details).", 4, @"Y,S,D", "8A7048C3-16F5-4B97-82B7-549B74C0F0D4", "AcceptableDPVCodes" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Address.SmartyStreets", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Acceptable Precisions", "The Smarty Streets latitude & longitude precision values that are considered acceptable levels of geocoding (see http://smartystreets.com/kb/liveaddress-api/field-definitions#precision for details).", 5, @"Zip7,Zip8,Zip9", "EA20653B-C02A-4608-9930-79EB515D33B3", "AcceptablePrecisions" );

            RockMigrationHelper.UpdateAttributeQualifier( "04663026-D7A8-428E-93AD-BF524E643CC5", "ispassword", @"False", "3778949A-9B29-443E-B667-482191FFDAFA" );
            RockMigrationHelper.UpdateAttributeQualifier( "99ECB1C8-4C1A-4DFA-B11B-9F62786A8565", "ispassword", @"False", "1E2406FA-FDBB-4D95-BBAA-E41E2B82EDBB" );
            RockMigrationHelper.UpdateAttributeQualifier( "8A7048C3-16F5-4B97-82B7-549B74C0F0D4", "ispassword", @"False", "5A0FA7E2-17AC-4F24-9663-FAB1D1587D2D" );
            RockMigrationHelper.UpdateAttributeQualifier( "EA20653B-C02A-4608-9930-79EB515D33B3", "ispassword", @"False", "ABC59D56-2422-4BAA-84F4-3D5AE5344776" );
            RockMigrationHelper.AddAttributeValue( "63844379-3CA1-450D-8D03-56144F6B5912", 0, @"True", "63844379-3CA1-450D-8D03-56144F6B5912" ); // Use Managed API Key  
            RockMigrationHelper.AddAttributeValue( "8A7048C3-16F5-4B97-82B7-549B74C0F0D4", 0, @"Y,S,D", "8A7048C3-16F5-4B97-82B7-549B74C0F0D4" ); // Acceptable DPV Codes  
            RockMigrationHelper.AddAttributeValue( "EA20653B-C02A-4608-9930-79EB515D33B3", 0, @"Zip7,Zip8,Zip9", "EA20653B-C02A-4608-9930-79EB515D33B3" ); // Acceptable Precisions  
            Sql( @"-- First see if any location services are enabled
                DECLARE @EnabledLocationServicesCount INT =
                 (SELECT COUNT(*)
                 FROM [EntityType] t
                 JOIN [Attribute] a on t.[Id] = a.[EntityTypeId]
                 JOIN [AttributeValue] v on a.[Id] = v.[AttributeId]
                 WHERE t.[Name] LIKE 'Rock.Address.%'
                  AND a.[Key] = 'Active'
                  AND v.[Value] = 'True')
                -- If nothing is enabled then find the SmartyStreets AttributeValue and set it to True
                IF (@EnabledLocationServicesCount = 0)
                BEGIN
                 DECLARE @SmartyStreetsAttributeValueId INT =
                  (SELECT v.[Id]
                  FROM [Attribute] a 
                  JOIN [AttributeValue] v ON a.[Id] = v.[AttributeId]
                  WHERE a.[Guid] = '48F4598D-94B7-4131-9BAF-8F54308EE5FE')
                 -- If an attibute value already exists then update
                 IF (@SmartyStreetsAttributeValueId > 0)
                 BEGIN
                  UPDATE [AttributeValue]
                  SET [Value] = 'True'
                  WHERE [Id] = @SmartyStreetsAttributeValueId
                 END
                 ELSE
                 BEGIN
                 -- Otherwise insert
                  DECLARE @SmartyStreetsAttributeId INT =
                   (SELECT [Id]
                   FROM [Attribute]
                   WHERE [Guid] = '48F4598D-94B7-4131-9BAF-8F54308EE5FE')
                  IF (@SmartyStreetsAttributeId > 0)
                  BEGIN
                   INSERT INTO [AttributeValue]([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                   VALUES (
                    0 --[IsSystem]
                    , @SmartyStreetsAttributeId --[AttributeId]
                    , 0 --[EntityId]
                    , 'True' --[Value]
                    , '48F4598D-94B7-4131-9BAF-8F54308EE5FE') --[Guid]
                  END
                 END
                END" );
        }

        /// <summary>
        /// JE: Fixed issue with GoogleMaps shortcode
        /// </summary>
        private void FixGoogleShortCode()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation] = REPLACE([Documentation], '&lt;/code&gt;&lt;p&gt;&lt;code&gt;','')
                WHERE [TagName] = 'googlemap'" );
        }
    }
}
