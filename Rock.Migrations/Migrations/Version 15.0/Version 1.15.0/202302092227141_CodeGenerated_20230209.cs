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
    public partial class CodeGenerated_20230209 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update BlockType 
            //   Name: Snippet Type List
            //   Category: Communication
            //   Path: ~/Blocks/Communication/SnippetTypeList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Snippet Type List", "Shows a list of all snippet types.", "~/Blocks/Communication/SnippetTypeList.ascx", "Communication", "397583B2-0DC4-4D69-9169-C95B430AB336" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.SmsConversation
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Communication.SmsConversation", "Sms Conversation", "Rock.Blocks.Types.Mobile.Communication.SmsConversation, Rock, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "99812F83-B514-4A76-A79D-01A97369F726" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.SmsConversationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Communication.SmsConversationList", "Sms Conversation List", "Rock.Blocks.Types.Mobile.Communication.SmsConversationList, Rock, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "77701BE2-1335-45F3-93B3-F06466CA391F" );

            // Add/Update Mobile Block Type
            //   Name:SMS Conversation
            //   Category:Mobile > Communication
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.SmsConversation
            RockMigrationHelper.UpdateMobileBlockType( "SMS Conversation", "Displays a single SMS conversation between Rock and individual.", "Rock.Blocks.Types.Mobile.Communication.SmsConversation", "Mobile > Communication", "4EF4250E-2D22-426C-ADAC-571C1301D18E" );

            // Add/Update Mobile Block Type
            //   Name:SMS Conversation List
            //   Category:Mobile > Communication
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.SmsConversationList
            RockMigrationHelper.UpdateMobileBlockType( "SMS Conversation List", "Displays a list of SMS conversations that the individual can interact with.", "Rock.Blocks.Types.Mobile.Communication.SmsConversationList", "Mobile > Communication", "E16DC868-101F-4944-BE6C-29D858D9821D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SnippetTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SnippetTypeDetail", "Snippet Type Detail", "Rock.Blocks.Communication.SnippetTypeDetail, Rock.Blocks, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "D3EEAD93-29E7-4BB3-9D16-F7E81B414D49" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.AccountEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.AccountEntry", "Account Entry", "Rock.Blocks.Security.AccountEntry, Rock.Blocks, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "75704274-FDB8-4A0C-AE0E-510F1977BE0A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.Login
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.Login", "Login", "Rock.Blocks.Security.Login, Rock.Blocks, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "D9482EF9-F774-4E37-AC84-8B340CBCA364" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Utility.SmsTestTransport
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Utility.SmsTestTransport", "Sms Test Transport", "Rock.Blocks.Utility.SmsTestTransport, Rock.Blocks, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "803DB5AE-0A92-4B6D-A5BD-81845D1202AE" );

            // Add/Update Obsidian Block Type
            //   Name:Snippet Type Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SnippetTypeDetail
            RockMigrationHelper.UpdateMobileBlockType( "Snippet Type Detail", "Displays the details of a particular snippet type.", "Rock.Blocks.Communication.SnippetTypeDetail", "Communication", "96664080-04EF-4C88-BD16-4F002009DA3C" );

            // Add/Update Obsidian Block Type
            //   Name:Account Entry
            //   Category:Obsidian > Security
            //   EntityType:Rock.Blocks.Security.AccountEntry
            RockMigrationHelper.UpdateMobileBlockType( "Account Entry", "Allows the user to register.", "Rock.Blocks.Security.AccountEntry", "Obsidian > Security", "E5C34503-DDAD-4881-8463-0E1E20B1675D" );

            // Add/Update Obsidian Block Type
            //   Name:Login
            //   Category:Obsidian > Security
            //   EntityType:Rock.Blocks.Security.Login
            RockMigrationHelper.UpdateMobileBlockType( "Login", "Allows the user to authenticate.", "Rock.Blocks.Security.Login", "Obsidian > Security", "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" );

            // Add/Update Obsidian Block Type
            //   Name:SMS Test Transport
            //   Category:Utility
            //   EntityType:Rock.Blocks.Utility.SmsTestTransport
            RockMigrationHelper.UpdateMobileBlockType( "SMS Test Transport", "Allows interaction with the SMS Test transport.", "Rock.Blocks.Utility.SmsTestTransport", "Utility", "2C2D6BC3-8257-4E23-8FE7-06E744D58AC0" );

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "16A4C5D1-25AB-4EC2-B36F-C040E7B076F1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "E4534926-AC2D-4904-BA19-C47FA6A34EAB" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If enabled, prevents the use of captcha verification on the form.", 8, @"False", "D927634E-BE8D-4EAA-B760-B3BA34C90380" );

            // Attribute for BlockType
            //   BlockType: Line Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Chart Style", @"", 3, @"", "1F737AB0-C313-4FA6-90B0-3D598F4E3F02" );

            // Attribute for BlockType
            //   BlockType: Bar Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Chart Style", @"", 3, @"", "6E91771D-BB01-482B-9E96-761614448049" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Chart Style", @"", 3, @"", "C75AE677-629E-4E71-870A-7662E856555B" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Legend Position
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "Legend Position", @"Select the position of the Legend (corner)", 8, @"ne", "8E37B2E2-3690-439E-A18D-9F9CE96EDA7B" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Metric
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Metric", "Metric", "Metric", @"NOTE: Weird storage due to backwards compatible", 0, @"", "47E3B187-EAF4-49F7-AC03-5F15E8A72093" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: MetricEntityTypeEntityIds
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MetricEntityTypeEntityIds", "MetricEntityTypeEntityIds", "MetricEntityTypeEntityIds", @"", 0, @"", "FF8626BD-E772-40C5-9649-FD1CC4A9B31C" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Metric Value Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Metric Value Types", "MetricValueTypes", "Metric Value Types", @"Select which metric value types to display in the chart", 4, @"Measure", "74F9F22C-60AD-4B5B-8E73-DB9D743E29EA" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Show Legend
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "Show Legend", @"", 7, @"True", "788DFBD3-FD41-4986-9584-46861C36EC41" );

            // Attribute for BlockType
            //   BlockType: Dynamic Chart
            //   Category: Reporting
            //   Attribute: Chart Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Chart Style", @"", 3, @"", "50C750E8-D69C-4910-9527-FB06A2A7071C" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Notes Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Notes Template", "NoteTemplate", "Notes Template", @"The template to use when rendering the notes. Provided with a 'Notes' merge field, among some others (see documentation).", 4, @"C9134085-D433-444D-9803-8E5CE1B053DE", "2BD15F86-E93D-47A2-B8AF-3C4A242B8660" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Note List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Note List Page", "DetailPage", "Note List Page", @"Page to link to when user taps on the 'See All' button (in template mode). Should link to a page containing a fullscreen note block.", 5, @"", "ACD9B175-31F6-4081-AC36-CA7812A3EBD6" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Use Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Template", "NoteDisplayMode", "Use Template", @"If enabled, notes will be displayed using the 'Notes Template', allowing you full customization of the layout.", 3, @"False", "BB4F5960-5EC9-4A64-9E53-82471DFDA30A" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Page Load Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B337D89-A298-4620-A0BE-078A41BC054B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Page Load Size", "PageLoadSize", "Page Load Size", @"Determines the amount of notes to show in the initial page load. In template mode, this is the amount of notes your 'Notes' merge field will be limited to.", 6, @"6", "7F088EF0-BB9F-4932-87F2-B2DA2D157376" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Auto Focus Keyboard
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Focus Keyboard", "AutoFocusKeyboard", "Auto Focus Keyboard", @"Determines if the keyboard should auto-focus into the search field when the page is attached.", 8, @"True", "B5EF2741-7917-45EF-BDC2-8B5EB8749845" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Account Hierarchy for Additional Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Account Hierarchy for Additional Accounts", "EnableAccountHierarchy", "Enable Account Hierarchy for Additional Accounts", @"When enabled this will group accounts under their parents. This allows a person to keep the current behavior if desired. Note: This setting is not compatible with the ""Use Account Campus Mapping Logic"" setting.", 15, @"False", "BB548362-8DD2-4FE5-B118-2150A9B14C18" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Reminders
            //   Attribute: View Reminders Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC59B6D6-5CA1-4367-9109-CDDC92357D35", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Reminders Page", "ViewRemindersPage", "View Reminders Page", @"The page where a person can view their reminders.", 0, @"E1736347-1D4F-42A6-8EC4-7595286054A6", "558BEBE2-2892-4291-826A-BA13371CA49C" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Reminders
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC59B6D6-5CA1-4367-9109-CDDC92357D35", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Reminder Page", "EditReminderPage", "Edit Reminder Page", @"The page where a person can edit a reminder.", 1, @"2640FF34-DCC9-4604-9C5B-9E2DA590D5A7", "4A40C0F1-0D3E-490E-9E68-CC99A2BDBDF2" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Reminder Page", "EditReminderPage", "Edit Reminder Page", @"The page where a person can edit a reminder.", 1, @"2640FF34-DCC9-4604-9C5B-9E2DA590D5A7", "9791FCA9-DDB4-4BB6-8427-03946F63F329" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Show Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filters", "ShowFilters", "Show Filters", @"Select this option if you want the block to show filters for reminders.", 4, @"True", "310A546E-D033-4125-8E7B-57B839DFF410" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "71AA3245-AA5F-4C62-AFB4-2A5A0E7494C6" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "61737227-8138-402C-AB5F-157BACDCAEC7" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Include
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Include", "ReminderTypesInclude", "Reminder Types Include", @"Select any specific remindeder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).", 2, @"", "8FBA07B5-6056-44A3-9608-05A78BAF5AFB" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Exclude
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Exclude", "ReminderTypesExclude", "Reminder Types Exclude", @"Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.", 3, @"", "C380366F-FAF6-44E6-962A-6759E1015909" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Forgot Username
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Forgot Username", "ForgotUsernameTemplate", "Forgot Username", @"Forgot Username Email Template", 10, @"113593ff-620e-4870-86b1-7a0ec0409208", "74B1010B-C387-46C7-B98E-CE3D6836213F" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account", "ConfirmAccountTemplate", "Confirm Account", @"Confirm Account Email Template", 11, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "35377B42-22EC-458D-B521-AA0D7B6B892D" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Account Created
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Account Created", "AccountCreatedTemplate", "Account Created", @"Account Created Email Template", 12, @"84e373e9-3aaf-4a31-b3fb-a8e3f0666710", "79115CB5-D494-4505-9BE2-E76DD647E9F2" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account (Passwordless)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account (Passwordless)", "ConfirmAccountPasswordlessTemplate", "Confirm Account (Passwordless)", @"Confirm Account (Passwordless) Email Template", 28, @"543B7C09-80C0-4DAB-8487-10569474D9C7", "AE1E918A-358C-4425-B6FC-E254C3F92119" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Prospect'.)", 13, @"368DD475-242C-49C4-A42C-7278BE690CC2", "6FBB35C7-846B-4543-8161-3728102DD2FB" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 14, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "4CCA6AEA-0574-42EB-836A-DC04D3E0FA15" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Phone Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types", "PhoneTypes", "Phone Types", @"The phone numbers to display for editing.", 20, @"", "BDCE233D-284F-4476-BD9C-42C0326B2A7D" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Phone Types Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types Required", "PhoneTypesRequired", "Phone Types Required", @"The phone numbers that are required.", 21, @"", "7EB29BB0-B60C-49C8-82F0-35D082A0AFAC" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Minimum Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Age", "MinimumAge", "Minimum Age", @"The minimum age allowed to create an account. Warning = The Children's Online Privacy Protection Act disallows children under the age of 13 from giving out personal information without their parents' permission.", 19, @"13", "A9BEC7A7-D3FD-43AA-A5CF-DF571342B9AF" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "AttributeCategories", "Attribute Categories", @"The Attribute Categories to display attributes from.", 26, @"", "9B1E994E-1DBD-4B94-B8CA-7E7D3978C2C3" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Location Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Location Type", "LocationType", "Location Type", @"The type of location that address should use.", 16, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "4BA67F1C-E1DB-451D-8B6B-951048314793" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Username Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Username Field Label", "UsernameFieldLabel", "Username Field Label", @"The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.", 1, @"Username", "94F05F5E-F3EC-4F2E-98FD-04DB1DCC692B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Found Duplicate Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Found Duplicate Caption", "FoundDuplicateCaption", "Found Duplicate Caption", @"", 3, @"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?", "1DFCFD47-BC0C-4005-B751-5566F5E6FD01" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Existing Account Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Existing Account Caption", "ExistingAccountCaption", "Existing Account Caption", @"", 4, @"{0}, you already have an existing account.  Would you like us to email you the username?", "8426CA8E-7491-4A0F-810E-48E6BDDFC229" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Sent Login Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sent Login Caption", "SentLoginCaption", "Sent Login Caption", @"", 5, @"Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.", "2B8FDE04-69C4-414D-A62D-7030872CB018" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirm Caption", "ConfirmCaption", "Confirm Caption", @"", 6, @"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "C6BB3C69-3DE0-443E-AE95-5E308567EA1C" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Success Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Caption", "SuccessCaption", "Success Caption", @"", 7, @"{0}, Your account has been created", "822FC7F0-B630-47FC-987D-FB5F9CC60564" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Campus Selector Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Selector Label", "CampusSelectorLabel", "Campus Selector Label", @"The label for the campus selector (only effective when ""Show Campus Selector"" is enabled).", 23, @"Campus", "218AD2D8-129C-4EEA-9B7D-5211BAC4A863" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption (Passwordless)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirm Caption (Passwordless)", "ConfirmCaptionPasswordless", "Confirm Caption (Passwordless)", @"", 29, @"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We’ve sent you an email that contains a code for confirming.  Please enter the code from your email to continue.", "4E99A990-A9F0-4F76-807C-FB8F580A9D04" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Require Email For Username
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Email For Username", "RequireEmailForUsername", "Require Email For Username", @"When enabled the label on the Username will be changed to Email and the field will validate to ensure that the input is formatted as an email.", 0, @"False", "AF46BAB7-81DE-41D3-8A5A-023CADB01316" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Check For Duplicates
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check For Duplicates", "Duplicates", "Check For Duplicates", @"Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.", 2, @"True", "7586F7A2-D697-433C-A2AF-E549935AA598" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Address", "ShowAddress", "Show Address", @"Allows showing the address field.", 15, @"False", "F0650100-74A3-4356-9DCA-E05F74453699" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Whether the address is required.", 17, @"False", "0112FC95-B856-4C34-B693-CD9E296C93D5" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Phone Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Phone Numbers", "ShowPhoneNumbers", "Show Phone Numbers", @"Allows showing the phone numbers.", 18, @"False", "D734E889-3EDE-4E3D-BE3E-682F0A9351D6" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampusSelector", "Show Campus", @"Allows selection of primary a campus. If there is only one active campus then the campus field will not show.", 22, @"False", "9CAE407A-2669-4AA7-8110-119A3FCBB0C4" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Save Communication History
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "CreateCommunicationRecord", "Save Communication History", @"Should a record of communication from this block be saved to the recipient's profile?", 24, @"False", "830EC8A0-9690-497A-9EC1-4EBD8A21E9BE" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "Show Gender", @"Determines if the gender selection field should be shown.", 25, @"True", "442A7BF5-50E0-4DD8-9BB2-F36160DEB50B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Disable Username Availability Checking
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Username Availability Checking", "DisableUsernameAvailabilityCheck", "Disable Username Availability Checking", @"Disables username availability checking.", 27, @"False", "9A75C3B2-52C7-4089-AD2B-FF4815EFA015" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirmation Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationPage", "Confirmation Page", @"Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", 8, @"", "20D0B45D-CB1B-4037-9CEC-7088707731CA" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Login Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Login Page", "LoginPage", "Login Page", @"Page to navigate to when a user elects to log in (if blank will use 'Login' page route)", 9, @"", "507591E3-0828-4600-A9D9-AAB2F5358D0B" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: New Account Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "New Account Page", "NewAccountPage", "New Account Page", @"The page to navigate to when individual selects 'Create New Account' or if a matching email / phone number could not be found for passwordless sign in (if blank will use 'NewAccountPage' page route).", 1, @"", "D371AC88-F24B-4BF4-8ACF-BC7594161F45" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Help Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Help Page", "HelpPage", "Help Page", @"Page to navigate to when individual selects 'Forgot Account' option (if blank will use 'ForgotUserName' page route).", 2, @"", "4A82C63D-D034-4B19-B922-2E24539F2B75" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirmation Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationPage", "Confirmation Page", @"Page for individual to confirm their account (if blank will use 'ConfirmAccount' page route).", 4, @"", "1721E639-21B3-4825-8F73-B7AFCB59461D" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "Redirect Page", @"Page to navigate to when upon successful log in. The 'returnurl' query string will always override this setting for database authenticated logins. Redirect Page Setting will override third-party authentication 'returnurl'.", 15, @"", "56D7F241-E008-4061-BEA5-E94EC5D48651" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Default Login Method
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default Login Method", "DefaultLoginMethod", "Default Login Method", @"The login method that will be shown when the block is loaded.", 18, @"1", "79216C66-CA01-4F28-B9A6-67B30AA8C7B5" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Hide New Account Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide New Account Option", "HideNewAccountOption", "Hide New Account Option", @"Should 'New Account' option be hidden? For sites that require individual to be in a role (Internal Rock Site for example), individuals shouldn't be able to create their own account.", 7, @"False", "A44827E9-A32C-4AA5-8691-3FA753127F52" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Show Internal Database Login
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Internal Database Login", "ShowInternalLogin", "Show Internal Database Login", @"Show the internal database (username & password) login.", 12, @"True", "7A4520AD-1C92-43CD-9E24-DB0E2A32F898" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Redirect to Single External Auth Provider
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Redirect to Single External Auth Provider", "RedirectToSingleExternalAuthProvider", "Redirect to Single External Auth Provider", @"Redirect straight to the external authentication provider if only one is configured and internal database login is disabled.", 13, @"False", "7BC1D42F-9CA3-4E60-8A81-202962D438F3" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Username Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Username Field Label", "UsernameFieldLabel", "Username Field Label", @"The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.", 0, @"Username", "DE1525F5-05F0-4B57-8F73-8C9ED7A421EC" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: New Account Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "New Account Text", "NewAccountButtonText", "New Account Text", @"The text to show on the New Account button.", 8, @"Register", "AE9AEFA8-7F23-4065-B0C9-D36B5E996E2F" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirm Caption", "ConfirmCaption", "Confirm Caption", @"The text (HTML) to display when a individual's account needs to be confirmed.", 3, @"Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "7EA1972E-26CF-4481-A354-2392F4228DEC" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Locked Out Caption
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Locked Out Caption", "LockedOutCaption", "Locked Out Caption", @"The text (HTML) to display when a individual's account has been locked. <span class='tip tip-lava'></span>.", 6, @"{%- assign phone = Global' | Attribute:'OrganizationPhone' | Trim -%} Sorry, your account has been locked. Please {% if phone != '' %}contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email{% else %}email us at{% endif %} <a href='mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}'>{{ 'Global' | Attribute:'OrganizationEmail' }}</a> for help. Thank you.", "5177B76C-8E38-4969-9965-BF73F080307B" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: No Account Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Account Text", "NoAccountText", "No Account Text", @"The text to show when no account exists. <span class='tip tip-lava'></span>.", 9, @"We couldn't find an account with that username and password combination. Can we help you recover your <a href='{{HelpPage}}'>account information</a>?", "94DF12DB-2CFC-4297-96A2-BE122FD3C004" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Remote Authorization Prompt Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Remote Authorization Prompt Message", "RemoteAuthorizationPromptMessage", "Remote Authorization Prompt Message", @"Optional text (HTML) to display above remote authorization options.", 10, @"Log in with social account", "2DE2A188-6541-4C6F-B21E-9564E75F45FE" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Prompt Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Prompt Message", "PromptMessage", "Prompt Message", @"Optional text (HTML) to display above username and password fields.", 14, @"", "47125A4B-6AD7-4DB8-907D-B45ABC21291B" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Invalid PersonToken Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Invalid PersonToken Text", "InvalidPersonTokenText", "Invalid PersonToken Text", @"The text to show when an individual is logged out due to an invalid persontoken. <span class='tip tip-lava'></span>.", 16, @"<div class='alert alert-warning'>The login token you provided is no longer valid. Please log in below.</div>", "3D5C6940-C0DD-4CC5-80BB-E66B16FAF1F5" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Content Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Text", "ContentText", "Content Text", @"Lava template to show below the 'Log in' button. <span class='tip tip-lava'></span>.", 17, @"<div>By signing in, I agree to {{ 'Global' | Attribute:'OrganizationName' }}'s <a href='/terms'>Terms of Use</a> and <a href='/privacy'>Privacy Policy</a>.</div>", "05A21920-7091-46D8-BEBA-348E2FE85FF2" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account Template", "ConfirmAccountTemplate", "Confirm Account Template", @"Confirm Account Email Template.", 5, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "9125032A-03BE-4CC3-8175-89E1A7032A88" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Secondary Authentication Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3", "98F57599-2DC3-4022-BE33-14A22C3043E1", "Secondary Authentication Types", "SecondaryAuthenticationTypes", "Secondary Authentication Types", @"The active secondary authorization types that should be displayed as options for authentication.", 11, @"", "10B218B4-B4BD-4385-87AE-5E612410A326" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Message Count
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4EF4250E-2D22-426C-ADAC-571C1301D18E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Message Count", "MessageCount", "Message Count", @"The number of messages to be returned each time more messages are requested.", 2, @"50", "DA170CD0-00FB-4CDE-BC21-D8D8AA9B5BB5" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4EF4250E-2D22-426C-ADAC-571C1301D18E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 2, @"180", "5132C2F0-3BD1-41D7-8A8E-50F24EB672D9" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Snippet Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4EF4250E-2D22-426C-ADAC-571C1301D18E", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Snippet Type", "SnippetType", "Snippet Type", @"The type of snippets to make available when sending a message.", 0, @"", "C6A0FE3F-0B5B-4979-997A-339289EA2E58" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Conversation Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Conversation Page", "ConversationPage", "Conversation Page", @"The page that the person will be pushed to when selecting a conversation.", 6, @"", "CF128932-8976-4A83-9EAB-2D42DBE3A697" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show only personal SMS number", "ShowOnlyPersonalSmsNumber", "Show only personal SMS number", @"Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.", 1, @"False", "95341ACE-E688-4ECF-967A-0DB6339426A8" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide personal SMS numbers", "HidePersonalSmsNumbers", "Hide personal SMS numbers", @"Only SMS Numbers that are not associated with a person. The numbers without an Assigned To Person value.", 2, @"False", "39C084E7-16D0-4AEF-A6AE-2816F6776367" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Show Conversations From Months Ago
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Show Conversations From Months Ago", "ShowConversationsFromMonthsAgo", "Show Conversations From Months Ago", @"Limits the conversations shown in the left pane to those of X months ago or newer.", 3, @"6", "7E37BC9A-3032-440A-A738-AAB4DDCAF8D9" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Max Conversations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Conversations", "MaxConversations", "Max Conversations", @"Limits the number of conversations shown in the left pane.", 4, @"100", "7B8D30ED-8EFF-49DF-AC93-3ABCE8A53749" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 5, @"180", "39CDBC4B-C6BA-48EA-9E32-A74EC01476BC" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 0, @"", "8B3D1BBF-41EC-4C2C-98D2-A456E0673AC3" );

            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "397583B2-0DC4-4D69-9169-C95B430AB336", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "25ED95F8-9CD5-4839-93DE-2315A25DF2F0" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "E4534926-AC2D-4904-BA19-C47FA6A34EAB", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
            RockMigrationHelper.UpdateFieldType( "Reminder Types", "", "Rock", "Rock.Field.Types.ReminderTypesFieldType", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7" );
            RockMigrationHelper.UpdateFieldType( "Secondary Auths", "", "Rock", "Rock.Field.Types.SecondaryAuthsFieldType", "98F57599-2DC3-4022-BE33-14A22C3043E1" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Metric Value Types
            RockMigrationHelper.DeleteAttribute( "74F9F22C-60AD-4B5B-8E73-DB9D743E29EA" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: MetricEntityTypeEntityIds
            RockMigrationHelper.DeleteAttribute( "FF8626BD-E772-40C5-9649-FD1CC4A9B31C" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Metric
            RockMigrationHelper.DeleteAttribute( "47E3B187-EAF4-49F7-AC03-5F15E8A72093" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Legend Position
            RockMigrationHelper.DeleteAttribute( "8E37B2E2-3690-439E-A18D-9F9CE96EDA7B" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Show Legend
            RockMigrationHelper.DeleteAttribute( "788DFBD3-FD41-4986-9584-46861C36EC41" );

            // Attribute for BlockType
            //   BlockType: Pie Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.DeleteAttribute( "C75AE677-629E-4E71-870A-7662E856555B" );

            // Attribute for BlockType
            //   BlockType: Line Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.DeleteAttribute( "1F737AB0-C313-4FA6-90B0-3D598F4E3F02" );

            // Attribute for BlockType
            //   BlockType: Bar Chart
            //   Category: Reporting > Dashboard
            //   Attribute: Chart Style
            RockMigrationHelper.DeleteAttribute( "6E91771D-BB01-482B-9E96-761614448049" );

            // Attribute for BlockType
            //   BlockType: Dynamic Chart
            //   Category: Reporting
            //   Attribute: Chart Style
            RockMigrationHelper.DeleteAttribute( "50C750E8-D69C-4910-9527-FB06A2A7071C" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Auto Focus Keyboard
            RockMigrationHelper.DeleteAttribute( "B5EF2741-7917-45EF-BDC2-8B5EB8749845" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Page Load Size
            RockMigrationHelper.DeleteAttribute( "7F088EF0-BB9F-4932-87F2-B2DA2D157376" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Note List Page
            RockMigrationHelper.DeleteAttribute( "ACD9B175-31F6-4081-AC36-CA7812A3EBD6" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Notes Template
            RockMigrationHelper.DeleteAttribute( "2BD15F86-E93D-47A2-B8AF-3C4A242B8660" );

            // Attribute for BlockType
            //   BlockType: Notes
            //   Category: Mobile > Core
            //   Attribute: Use Template
            RockMigrationHelper.DeleteAttribute( "BB4F5960-5EC9-4A64-9E53-82471DFDA30A" );

            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "25ED95F8-9CD5-4839-93DE-2315A25DF2F0" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Conversation Page
            RockMigrationHelper.DeleteAttribute( "CF128932-8976-4A83-9EAB-2D42DBE3A697" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute( "39CDBC4B-C6BA-48EA-9E32-A74EC01476BC" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Max Conversations
            RockMigrationHelper.DeleteAttribute( "7B8D30ED-8EFF-49DF-AC93-3ABCE8A53749" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Show Conversations From Months Ago
            RockMigrationHelper.DeleteAttribute( "7E37BC9A-3032-440A-A738-AAB4DDCAF8D9" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.DeleteAttribute( "39C084E7-16D0-4AEF-A6AE-2816F6776367" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.DeleteAttribute( "95341ACE-E688-4ECF-967A-0DB6339426A8" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation List
            //   Category: Mobile > Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "8B3D1BBF-41EC-4C2C-98D2-A456E0673AC3" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute( "5132C2F0-3BD1-41D7-8A8E-50F24EB672D9" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Message Count
            RockMigrationHelper.DeleteAttribute( "DA170CD0-00FB-4CDE-BC21-D8D8AA9B5BB5" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Snippet Type
            RockMigrationHelper.DeleteAttribute( "C6A0FE3F-0B5B-4979-997A-339289EA2E58" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Default Login Method
            RockMigrationHelper.DeleteAttribute( "79216C66-CA01-4F28-B9A6-67B30AA8C7B5" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Content Text
            RockMigrationHelper.DeleteAttribute( "05A21920-7091-46D8-BEBA-348E2FE85FF2" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Invalid PersonToken Text
            RockMigrationHelper.DeleteAttribute( "3D5C6940-C0DD-4CC5-80BB-E66B16FAF1F5" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Redirect Page
            RockMigrationHelper.DeleteAttribute( "56D7F241-E008-4061-BEA5-E94EC5D48651" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Prompt Message
            RockMigrationHelper.DeleteAttribute( "47125A4B-6AD7-4DB8-907D-B45ABC21291B" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Redirect to Single External Auth Provider
            RockMigrationHelper.DeleteAttribute( "7BC1D42F-9CA3-4E60-8A81-202962D438F3" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Show Internal Database Login
            RockMigrationHelper.DeleteAttribute( "7A4520AD-1C92-43CD-9E24-DB0E2A32F898" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Secondary Authentication Types
            RockMigrationHelper.DeleteAttribute( "10B218B4-B4BD-4385-87AE-5E612410A326" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Remote Authorization Prompt Message
            RockMigrationHelper.DeleteAttribute( "2DE2A188-6541-4C6F-B21E-9564E75F45FE" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: No Account Text
            RockMigrationHelper.DeleteAttribute( "94DF12DB-2CFC-4297-96A2-BE122FD3C004" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: New Account Text
            RockMigrationHelper.DeleteAttribute( "AE9AEFA8-7F23-4065-B0C9-D36B5E996E2F" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Hide New Account Option
            RockMigrationHelper.DeleteAttribute( "A44827E9-A32C-4AA5-8691-3FA753127F52" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Locked Out Caption
            RockMigrationHelper.DeleteAttribute( "5177B76C-8E38-4969-9965-BF73F080307B" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account Template
            RockMigrationHelper.DeleteAttribute( "9125032A-03BE-4CC3-8175-89E1A7032A88" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirmation Page
            RockMigrationHelper.DeleteAttribute( "1721E639-21B3-4825-8F73-B7AFCB59461D" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption
            RockMigrationHelper.DeleteAttribute( "7EA1972E-26CF-4481-A354-2392F4228DEC" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Help Page
            RockMigrationHelper.DeleteAttribute( "4A82C63D-D034-4B19-B922-2E24539F2B75" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: New Account Page
            RockMigrationHelper.DeleteAttribute( "D371AC88-F24B-4BF4-8ACF-BC7594161F45" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Obsidian > Security
            //   Attribute: Username Field Label
            RockMigrationHelper.DeleteAttribute( "DE1525F5-05F0-4B57-8F73-8C9ED7A421EC" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption (Passwordless)
            RockMigrationHelper.DeleteAttribute( "4E99A990-A9F0-4F76-807C-FB8F580A9D04" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account (Passwordless)
            RockMigrationHelper.DeleteAttribute( "AE1E918A-358C-4425-B6FC-E254C3F92119" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Disable Username Availability Checking
            RockMigrationHelper.DeleteAttribute( "9A75C3B2-52C7-4089-AD2B-FF4815EFA015" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Attribute Categories
            RockMigrationHelper.DeleteAttribute( "9B1E994E-1DBD-4B94-B8CA-7E7D3978C2C3" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Gender
            RockMigrationHelper.DeleteAttribute( "442A7BF5-50E0-4DD8-9BB2-F36160DEB50B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Save Communication History
            RockMigrationHelper.DeleteAttribute( "830EC8A0-9690-497A-9EC1-4EBD8A21E9BE" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Campus Selector Label
            RockMigrationHelper.DeleteAttribute( "218AD2D8-129C-4EEA-9B7D-5211BAC4A863" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Campus
            RockMigrationHelper.DeleteAttribute( "9CAE407A-2669-4AA7-8110-119A3FCBB0C4" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Phone Types Required
            RockMigrationHelper.DeleteAttribute( "7EB29BB0-B60C-49C8-82F0-35D082A0AFAC" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Phone Types
            RockMigrationHelper.DeleteAttribute( "BDCE233D-284F-4476-BD9C-42C0326B2A7D" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Minimum Age
            RockMigrationHelper.DeleteAttribute( "A9BEC7A7-D3FD-43AA-A5CF-DF571342B9AF" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Phone Numbers
            RockMigrationHelper.DeleteAttribute( "D734E889-3EDE-4E3D-BE3E-682F0A9351D6" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Address Required
            RockMigrationHelper.DeleteAttribute( "0112FC95-B856-4C34-B693-CD9E296C93D5" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Location Type
            RockMigrationHelper.DeleteAttribute( "4BA67F1C-E1DB-451D-8B6B-951048314793" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Address
            RockMigrationHelper.DeleteAttribute( "F0650100-74A3-4356-9DCA-E05F74453699" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute( "4CCA6AEA-0574-42EB-836A-DC04D3E0FA15" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute( "6FBB35C7-846B-4543-8161-3728102DD2FB" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Account Created
            RockMigrationHelper.DeleteAttribute( "79115CB5-D494-4505-9BE2-E76DD647E9F2" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Account
            RockMigrationHelper.DeleteAttribute( "35377B42-22EC-458D-B521-AA0D7B6B892D" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Forgot Username
            RockMigrationHelper.DeleteAttribute( "74B1010B-C387-46C7-B98E-CE3D6836213F" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Login Page
            RockMigrationHelper.DeleteAttribute( "507591E3-0828-4600-A9D9-AAB2F5358D0B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirmation Page
            RockMigrationHelper.DeleteAttribute( "20D0B45D-CB1B-4037-9CEC-7088707731CA" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Success Caption
            RockMigrationHelper.DeleteAttribute( "822FC7F0-B630-47FC-987D-FB5F9CC60564" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Confirm Caption
            RockMigrationHelper.DeleteAttribute( "C6BB3C69-3DE0-443E-AE95-5E308567EA1C" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Sent Login Caption
            RockMigrationHelper.DeleteAttribute( "2B8FDE04-69C4-414D-A62D-7030872CB018" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Existing Account Caption
            RockMigrationHelper.DeleteAttribute( "8426CA8E-7491-4A0F-810E-48E6BDDFC229" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Found Duplicate Caption
            RockMigrationHelper.DeleteAttribute( "1DFCFD47-BC0C-4005-B751-5566F5E6FD01" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Check For Duplicates
            RockMigrationHelper.DeleteAttribute( "7586F7A2-D697-433C-A2AF-E549935AA598" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Username Field Label
            RockMigrationHelper.DeleteAttribute( "94F05F5E-F3EC-4F2E-98FD-04DB1DCC692B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Require Email For Username
            RockMigrationHelper.DeleteAttribute( "AF46BAB7-81DE-41D3-8A5A-023CADB01316" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "D927634E-BE8D-4EAA-B760-B3BA34C90380" );

            // Attribute for BlockType
            //   BlockType: Utility Payment Entry
            //   Category: Finance
            //   Attribute: Enable Account Hierarchy for Additional Accounts
            RockMigrationHelper.DeleteAttribute( "BB548362-8DD2-4FE5-B118-2150A9B14C18" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "71AA3245-AA5F-4C62-AFB4-2A5A0E7494C6" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "61737227-8138-402C-AB5F-157BACDCAEC7" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Show Filters
            RockMigrationHelper.DeleteAttribute( "310A546E-D033-4125-8E7B-57B839DFF410" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Exclude
            RockMigrationHelper.DeleteAttribute( "C380366F-FAF6-44E6-962A-6759E1015909" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Reminder Types Include
            RockMigrationHelper.DeleteAttribute( "8FBA07B5-6056-44A3-9608-05A78BAF5AFB" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Reminders
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.DeleteAttribute( "9791FCA9-DDB4-4BB6-8427-03946F63F329" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Reminders
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.DeleteAttribute( "4A40C0F1-0D3E-490E-9E68-CC99A2BDBDF2" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Reminders
            //   Attribute: View Reminders Page
            RockMigrationHelper.DeleteAttribute( "558BEBE2-2892-4291-826A-BA13371CA49C" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E4534926-AC2D-4904-BA19-C47FA6A34EAB" );

            // Delete BlockType 
            //   Name: Snippet Type List
            //   Category: Communication
            //   Path: ~/Blocks/Communication/SnippetTypeList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "397583B2-0DC4-4D69-9169-C95B430AB336" );

            // Delete BlockType 
            //   Name: SMS Test Transport
            //   Category: Utility
            //   Path: -
            //   EntityType: Sms Test Transport
            RockMigrationHelper.DeleteBlockType( "2C2D6BC3-8257-4E23-8FE7-06E744D58AC0" );

            // Delete BlockType 
            //   Name: SMS Conversation List
            //   Category: Mobile > Communication
            //   Path: -
            //   EntityType: Sms Conversation List
            RockMigrationHelper.DeleteBlockType( "E16DC868-101F-4944-BE6C-29D858D9821D" );

            // Delete BlockType 
            //   Name: SMS Conversation
            //   Category: Mobile > Communication
            //   Path: -
            //   EntityType: Sms Conversation
            RockMigrationHelper.DeleteBlockType( "4EF4250E-2D22-426C-ADAC-571C1301D18E" );

            // Delete BlockType 
            //   Name: Login
            //   Category: Obsidian > Security
            //   Path: -
            //   EntityType: Login
            RockMigrationHelper.DeleteBlockType( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" );

            // Delete BlockType 
            //   Name: Account Entry
            //   Category: Obsidian > Security
            //   Path: -
            //   EntityType: Account Entry
            RockMigrationHelper.DeleteBlockType( "E5C34503-DDAD-4881-8463-0E1E20B1675D" );

            // Delete BlockType 
            //   Name: Snippet Type Detail
            //   Category: Communication
            //   Path: -
            //   EntityType: Snippet Type Detail
            RockMigrationHelper.DeleteBlockType( "96664080-04EF-4C88-BD16-4F002009DA3C" );
        }
    }
}
