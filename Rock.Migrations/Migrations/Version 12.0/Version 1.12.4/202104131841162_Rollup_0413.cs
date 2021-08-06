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
    public partial class Rollup_0413 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateMobileCalendarEventItemOccurrenceViewDefaultTemplate();
            RockMobilePrayerBlocksUp();
            UpdatePersonAttributeFrequencyLabel();
            GivingOverviewKPIUpdate();
            CodeGenMigrationsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            RockMobilePrayerBlocksDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add/Update BlockType Giving Overview
            RockMigrationHelper.UpdateBlockType("Giving Overview","Block used to view the giving.","~/Blocks/Crm/PersonDetail/GivingOverview.ascx","CRM > Person Detail","896D807D-2110-4007-AFD1-4D953B83375B");

            // Attribute for BlockType: Data View Detail:Add Administrate Security to Item Creator
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Administrate Security to Item Creator", "AddAdministrateSecurityToItemCreator", "Add Administrate Security to Item Creator", @"If enabled, the person who creates a new item will be granted 'Administrate' security rights to the item.  This was the behavior in previous versions of Rock.  If disabled, the item creator will not be able to edit the Data View, its security settings, or possibly perform other functions without the Rock administrator settings up a role that is allowed to perform such functions.", 0, @"False", "C8F4F1F6-481E-4829-8415-8A7B862AEDDB" );

            // Attribute for BlockType: Report Detail:Add Administrate Security to Item Creator
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Administrate Security to Item Creator", "AddAdministrateSecurityToItemCreator", "Add Administrate Security to Item Creator", @"If enabled, the person who creates a new item will be granted 'Administrate' and 'Edit' security rights to the item.  This was the behavior in previous versions of Rock.  If disabled, the item creator will not be able to edit the report, its security settings, or possibly perform other functions without the Rock administrator settings up a role that is allowed to perform such functions.", 0, @"False", "3E7BBC50-BE47-43FD-B0B0-47E549AC3336" );

            // Attribute for BlockType: Prayer Session:Create Interactions for Prayers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Create Interactions for Prayers", "CreateInteractionsForPrayers", "Create Interactions for Prayers", @"If enabled then this block will record an Interaction whenever somebody prays for a prayer request.", 8, @"True", "7BB6BF5E-2E9C-4D53-B0D0-A412E748E46C" );

            // Attribute for BlockType: Public Profile Edit:Show Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Title", "ShowTitle", "Show Title", @"Whether to show the person's title (e.g. Mr., Mrs. etc...)", 2, @"True", "4EACC79E-9CAE-468A-9025-5D7CBB1C4ECE" );

            // Attribute for BlockType: Public Profile Edit:Show Suffix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Suffix", "ShowSuffix", "Show Suffix", @"Whether to show the person's suffix (e.g. Roman Numerals, Jr., Ph.D., etc...)", 3, @"True", "5D4FDB97-3D16-4FFF-88A8-3C4D8923C3D7" );

            // Attribute for BlockType: Public Profile Edit:Show Nick Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Nick Name", "ShowNickName", "Show Nick Name", @"Whether to show the person's Nickname in addition to the First Name.", 4, @"True", "690798B9-CA9C-4C75-A4F2-E57B6D2A12BB" );

            // Attribute for BlockType: Public Profile Edit:Highlight Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Highlight Mobile Phone", "HighlightMobilePhone", "Highlight Mobile Phone", @"Determines if the emphasis box should be placed around the mobile number.", 10, @"True", "3535C1D9-1CB1-4791-99D3-487EE4AEBE34" );

            // Attribute for BlockType: Public Profile Edit:Mobile Highlight Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Mobile Highlight Title", "MobileHighlightTitle", "Mobile Highlight Title", @"The text to use for the mobile highlight title (only displayed if Highlight Mobile Phone is selected).", 11, @"Help Us Keep You Informed", "BBFBE75E-E360-4B39-874D-43E7E8EC1FBA" );

            // Attribute for BlockType: Public Profile Edit:Mobile Highlight Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Mobile Highlight Text", "MobileHighlightText", "Mobile Highlight Text", @"The text to use for the mobile highlight text (only displayed if Highlight Mobile Phone is selected).", 12, @"Help us keep you in the loop by providing your mobile phone number and opting in for text messages. We'll only send you the most important information at this number.", "D27F47F7-20A4-4D47-979F-A49372BFFBDB" );

            // Attribute for BlockType: Prayer Session:Create Interactions for Prayers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Create Interactions for Prayers", "CreateInteractionsForPrayers", "Create Interactions for Prayers", @"If enabled then this block will record an Interaction whenever somebody prays for a prayer request.", 6, @"True", "3382895E-1422-417E-84F2-A4750AF2AD70" );

            // Attribute for BlockType: Giving Analytics Alerts:Config Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Config Page", "ConfigPage", "Config Page", @"The page to configure what criteria should be used to generate alerts.", 1, @"", "477A3801-CFB7-4BEA-A5C1-ED9C2B2E1E9D" );

            // Attribute for BlockType: Giving Configuration:Scheduled Transaction Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Detail Page", "ScheduledTransactionDetailPage", "Scheduled Transaction Detail Page", @"", 7, @"996F5541-D2E1-47E4-8078-80A388203CEC", "B6861FC8-3741-46BA-9AC6-936C29511065" );

            // Attribute for BlockType: Onboard Person:Completed Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Completed Page", "CompletedPage", "Completed Page", @"The page to redirect the user to after the onboarding process has finished.", 0, @"", "D09D1B5F-A208-48AB-9E80-64E032C071B4" );

            // Attribute for BlockType: Onboard Person:Login Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Login Page", "LoginPage", "Login Page", @"The page that will be used if allowing login by existing account credentials.", 1, @"", "41B67475-F8D4-42EB-991D-71A327A08077" );

            // Attribute for BlockType: Onboard Person:Default Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "Default Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 1, @"368DD475-242C-49C4-A42C-7278BE690CC2", "487C6E9E-BF4F-4111-9C17-DEF87EADB213" );

            // Attribute for BlockType: Onboard Person:Default Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Record Status", "DefaultRecordStatus", "Default Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 2, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "EFF020AA-E9FD-4FB0-9B1E-0446E751844F" );

            // Attribute for BlockType: Onboard Person:Display Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Types", "DisplayCampusTypes", "Display Campus Types", @"The campus types that will be included in the list of campuses for the user to choose from.", 0, @"5A61507B-79CB-4DA2-AF43-6F82260203B3", "5F1F626E-65F1-44AC-BF49-40FD21E3EF64" );

            // Attribute for BlockType: Onboard Person:Display Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Statuses", "DisplayCampusStatuses", "Display Campus Statuses", @"The campus types that will be included in the list of campuses for the user to choose from.", 1, @"10696FD8-D0C7-486F-B736-5FB3F5D69F1A", "42B6A2C0-37FF-4BA8-8378-791937AECA86" );

            // Attribute for BlockType: Onboard Person:Online Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Online Campus", "OnlineCampus", "Online Campus", @"The campus to pick for the user if they press the 'Online Campus' button.", 2, @"", "A2CA2A14-272A-4CE8-9861-3744975E999F" );

            // Attribute for BlockType: Onboard Person:Do Not Attend Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Do Not Attend Campus", "DoNotAttendCampus", "Do Not Attend Campus", @"The campus to pick for the user if they press the 'Do Not Attend' button.", 3, @"", "0512A384-717C-455B-9F18-A4BEAF865D40" );

            // Attribute for BlockType: Onboard Person:Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategory", "Communication List Categories", @"The category of communication lists that will be made available to the user as topics of interest.", 4, @"", "A27FADBA-B1AD-4D5B-A11A-B63AF67BD2D5" );

            // Attribute for BlockType: Onboard Person:System Communication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "System Communication", "SystemCommunication", "System Communication", @"The communication that will be used to send the SMS or email to the user.", 3, @"", "CA1F541C-3C37-438B-8D7A-3841E31E05F8" );

            // Attribute for BlockType: Onboard Person:Verification Time Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Verification Time Limit", "VerificationTimeLimit", "Verification Time Limit", @"The number of minutes that the user has to enter the verification code.", 5, @"5", "4FE016DB-C77C-4A09-933B-1AAC649CAC95" );

            // Attribute for BlockType: Onboard Person:IP Throttle Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "IP Throttle Limit", "IpThrottleLimit", "IP Throttle Limit", @"The number of times a single IP address can submit phone numbers for verification per day.", 6, @"5000", "293A5476-C910-407A-8EAA-CA7AB12A1F55" );

            // Attribute for BlockType: Onboard Person:Validation Code Attempts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Validation Code Attempts", "ValidationCodeAttempts", "Validation Code Attempts", @"The number of times a validation code verification can be re-tried before failing permanently.", 7, @"10", "9E421C90-F505-47DC-BC93-CEE75076D653" );

            // Attribute for BlockType: Onboard Person:Allow Skip of Onboarding
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Skip of Onboarding", "AllowSkipOfOnboarding", "Allow Skip of Onboarding", @"Allows the user to skip the onboarding process and go straight to the homepage.", 0, @"True", "ED3671C9-D962-4584-8FD9-D7C2FEBFACE0" );

            // Attribute for BlockType: Onboard Person:Hide Gender if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Gender if Known", "HideGenderIfKnown", "Hide Gender if Known", @"Hides the Gender field if a value is already known.", 1, @"False", "D55F9A7C-CEB0-468A-86B2-A1B34B09B302" );

            // Attribute for BlockType: Onboard Person:Hide Birth Date if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Birth Date if Known", "HideBirthDateIfKnown", "Hide Birth Date if Known", @"Hides the Birth Date field if a value is already known.", 3, @"False", "FD94209A-76D6-4D41-B398-96745F546D5B" );

            // Attribute for BlockType: Onboard Person:Hide Mobile Phone if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Mobile Phone if Known", "HideMobilePhoneIfKnown", "Hide Mobile Phone if Known", @"Hides the Mobile Phone field if a value is already known.", 5, @"False", "4AA3A746-C705-418A-8FB5-FB5690D1429D" );

            // Attribute for BlockType: Onboard Person:Hide Email if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Email if Known", "HideEmailIfKnown", "Hide Email if Known", @"Hides the Email field if a value is already known.", 7, @"False", "1A4321DF-3AAD-4A2C-8070-A068820BE827" );

            // Attribute for BlockType: Onboard Person:Show Notifications Request
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Notifications Request", "ShowNotificationsRequest", "Show Notifications Request", @"Shows the screen that will request the user to grant permission to send notifications to them.", 8, @"True", "8458FD9D-C6C1-405B-9A93-CB713CD9A953" );

            // Attribute for BlockType: Onboard Person:Hide Campus if Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Campus if Known", "HideCampusIfKnown", "Hide Campus if Known", @"Hides the Campus field if a value is already known.", 9, @"False", "D00B6A18-D302-4A8C-ADF7-CD46ABF884C7" );

            // Attribute for BlockType: Onboard Person:Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "GenderVisibility", "Gender", @"Determines if the Gender field should be hidden, optional or required.", 0, @"1", "2D218849-F96F-4B0B-9B27-5236E8A6F8CB" );

            // Attribute for BlockType: Onboard Person:Birth Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birth Date", "BirthDateVisibility", "Birth Date", @"Determines if the Birth Date field should be hidden, optional or required.", 2, @"1", "3E362C81-EB06-444A-8E8D-92A763177B8E" );

            // Attribute for BlockType: Onboard Person:Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "MobilePhoneVisibility", "Mobile Phone", @"Determines if the Mobile Phone field should be hidden, optional or required.", 4, @"1", "E88A11E9-2118-4D70-B8CD-9E7B58AD07D3" );

            // Attribute for BlockType: Onboard Person:Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "EmailVisibility", "Email", @"Determines if the Email field should be hidden, optional or required.", 6, @"1", "386C39D8-1D77-47F4-94F1-129F03AD7B88" );

            // Attribute for BlockType: Onboard Person:Create Login
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Create Login", "CreateLoginVisibility", "Create Login", @"Determines if the Create Login screen should be hidden, optional or required.", 10, @"1", "9106D3D4-20B0-47D4-81F3-A4FFF4D01D17" );

            // Attribute for BlockType: Onboard Person:Hello Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Hello Screen Title", "HelloScreenTitle", "Hello Screen Title", @"The title to display at the top of the Hello screen. <span class='tip tip-lava'></span>", 0, @"Hello!", "03152B49-B22B-478E-8EB2-BAD959A6E39A" );

            // Attribute for BlockType: Onboard Person:Hello Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Hello Screen Subtitle", "HelloScreenSubtitle", "Hello Screen Subtitle", @"The text to display at the top of the Hello screen underneath the title. <span class='tip tip-lava'></span>", 1, @"Welcome to the {{ 'Global' | Attribute:'OrganizationName' }} mobile app. Please sign-in so we can personalize your experience.", "C93E9926-0010-4B0E-A4D7-AA30B41F0BBF" );

            // Attribute for BlockType: Onboard Person:Code Sent Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Code Sent Screen Title", "CodeSentScreenTitle", "Code Sent Screen Title", @"The title to display at the top of the Code Sent screen. <span class='tip tip-lava'></span>", 2, @"Code Sent...", "18D9FB2D-1898-4957-95F0-2A5D2A55526A" );

            // Attribute for BlockType: Onboard Person:Code Sent Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Code Sent Screen Subtitle", "CodeSentScreenSubtitle", "Code Sent Screen Subtitle", @"The text to display at the top of the Code Sent screen underneath the title. <span class='tip tip-lava'></span>", 3, @"You should be recieving a verification code from us shortly. When it arrives type or paste it below.", "A89FC144-DB98-4129-BAB7-EADA64F8FFCA" );

            // Attribute for BlockType: Onboard Person:Name Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Name Screen Title", "NameScreenTitle", "Name Screen Title", @"The title to display at the top of the Name screen. <span class='tip tip-lava'></span>", 4, @"Let’s Get to Know You", "FEC42D9A-A95C-4FEC-95DB-400B738BB250" );

            // Attribute for BlockType: Onboard Person:Name Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Name Screen Subtitle", "NameScreenSubtitle", "Name Screen Subtitle", @"The text to display at the top of the Name screen underneath the title. <span class='tip tip-lava'></span>", 5, @"To maximize your experience we’d like to know a little about you.", "6B463687-EDCF-4C44-9BE6-96B00B6C23B0" );

            // Attribute for BlockType: Onboard Person:Personal Information Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Information Screen Title", "PersonalInformationScreenTitle", "Personal Information Screen Title", @"The title to display at the top of the Personal Information screen. <span class='tip tip-lava'></span>", 6, @"Tell Us More", "BBF73E74-45DF-4E41-812D-A704E6409B14" );

            // Attribute for BlockType: Onboard Person:Personal Information Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Information Screen Subtitle", "PersonalInformationScreenSubtitle", "Personal Information Screen Subtitle", @"The text to display at the top of the Personal Information screen underneath the title. <span class='tip tip-lava'></span>", 7, @"The more we know the more we can taylor our ministry to you.", "29F7B8FD-1DAF-4062-9720-A735F60843AB" );

            // Attribute for BlockType: Onboard Person:Contact Information Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Screen Title", "ContactInformationScreenTitle", "Contact Information Screen Title", @"The title to display at the top of the Contact Information screen. <span class='tip tip-lava'></span>", 8, @"Stay Connected", "77D080E2-1CCD-4A1D-A641-20D9F0734ABF" );

            // Attribute for BlockType: Onboard Person:Contact Information Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Screen Subtitle", "ContactInformationScreenSubtitle", "Contact Information Screen Subtitle", @"The text to display at the top of the Contact Information screen underneath the title. <span class='tip tip-lava'></span>", 9, @"Help us keep you in the loop by providing your contact information.", "2DF996A3-002D-4C89-8AE4-9FC49F846107" );

            // Attribute for BlockType: Onboard Person:Interests Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Interests Screen Title", "InterestsScreenTitle", "Interests Screen Title", @"The title to display at the top of the Interests screen. <span class='tip tip-lava'></span>", 10, @"Topics Of Interest", "7C3F512A-7092-41C8-A6AA-F9427EFB313B" );

            // Attribute for BlockType: Onboard Person:Interests Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Interests Screen Subtitle", "InterestsScreenSubtitle", "Interests Screen Subtitle", @"The text to display at the top of the Interests screen underneath the title. <span class='tip tip-lava'></span>", 11, @"What topics are you most interested in.", "7F13BCFE-A16A-4688-AC74-30848B3DD1B2" );

            // Attribute for BlockType: Onboard Person:Notifications Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Notifications Screen Title", "NotificationsScreenTitle", "Notifications Screen Title", @"The title to display at the top of the Notifications screen. <span class='tip tip-lava'></span>", 12, @"Enable Notifications", "3C3F5F53-D303-4465-A6BF-58C5F1365134" );

            // Attribute for BlockType: Onboard Person:Notifications Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Notifications Screen Subtitle", "NotificationsScreenSubtitle", "Notifications Screen Subtitle", @"The text to display at the top of the Notifications screen underneath the title. <span class='tip tip-lava'></span>", 13, @"We’d like to keep you in the loop with important alerts and notifications.", "98D17BDF-C2F9-4F78-BFD8-7BE160787A24" );

            // Attribute for BlockType: Onboard Person:Campus Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Screen Title", "CampusScreenTitle", "Campus Screen Title", @"The title to display at the top of the Campus screen. <span class='tip tip-lava'></span>", 14, @"Find Your Campus", "6E725307-72A3-4D2E-AA7E-B9DDE5187A67" );

            // Attribute for BlockType: Onboard Person:Campus Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Screen Subtitle", "CampusScreenSubtitle", "Campus Screen Subtitle", @"The text to display at the top of the Campus screen underneath the title. <span class='tip tip-lava'></span>", 15, @"Select the campus you attend to get targets news and information about events.", "025306AB-40BA-490F-A9C5-BD1FC21E801D" );

            // Attribute for BlockType: Onboard Person:Create Login Screen Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Login Screen Title", "CreateLoginScreenTitle", "Create Login Screen Title", @"The title to display at the top of the Create Login screen. <span class='tip tip-lava'></span>", 16, @"Create Login", "DF658448-7F79-473B-AE1E-F3B2DA7EBBF8" );

            // Attribute for BlockType: Onboard Person:Create Login Screen Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Create Login Screen Subtitle", "CreateLoginScreenSubtitle", "Create Login Screen Subtitle", @"The text to display at the top of the Create Login screen underneath the title. <span class='tip tip-lava'></span>", 17, @"Create a login to help signing in quicker in the future.", "EAED4835-A87E-4EEE-8345-D01DF6FF1DD7" );

            // Attribute for BlockType: Answer To Prayer:Enforce Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "324D5295-72E6-42DF-B111-E428E811B786", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enforce Security", "EnforceSecurity", "Enforce Security", @"Ensures that the person editing the request is the owner of the request.", 1, @"True", "2239490F-D4CF-401A-ACC9-7C2233DE9806" );

            // Attribute for BlockType: Answer To Prayer:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "324D5295-72E6-42DF-B111-E428E811B786", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template for how to display the prayer request.", 2, @"91C29610-1D77-49A8-A46B-5A35EC67C551", "7478E7E0-0654-4665-A618-1975FF379B0E" );

            // Attribute for BlockType: Answer To Prayer:Return Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "324D5295-72E6-42DF-B111-E428E811B786", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Return Page", "ReturnPage", "Return Page", @"If set then the current page will be replaced with the Return Page on Save. If not set then a Pop Page is performed instead.", 0, @"", "440BB98F-76E9-456B-979E-ABBA9F5272C4" );

            // Attribute for BlockType: My Prayer Requests:Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Page", "EditPage", "Edit Page", @"The page that will be used for editing a prayer request.", 0, @"", "A3B29126-4A56-4D81-80D1-BE6550C38747" );

            // Attribute for BlockType: My Prayer Requests:Answer Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Answer Page", "AnswerPage", "Answer Page", @"The page that will be used for allowing the user to enter an answer to prayer.", 1, @"", "861D21C9-34DF-4B77-8127-301F7E87CE70" );

            // Attribute for BlockType: My Prayer Requests:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template for how to display the prayer request.", 2, @"DED26289-4746-4233-A5BD-D4095248023D", "C0F5912F-3A6B-402E-932C-F45CCF12C211" );

            // Attribute for BlockType: My Prayer Requests:Days Back to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Days Back to Show", "DaysBackToShow", "Days Back to Show", @"The number of days back to search for prayer requests. Leave blank for no limit.", 4, @"", "31EE1DF8-7767-496C-95EB-756FC555A1BA" );

            // Attribute for BlockType: My Prayer Requests:Max Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "Max Results", @"The maximum number of results to display. Leave blank for no limit.", 5, @"", "AC0CF5A9-B70E-4B13-BC09-6D74CAB6BBCC" );

            // Attribute for BlockType: My Prayer Requests:Show Expired
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Expired", "ShowExpired", "Show Expired", @"Include expired prayer requests in the list.", 3, @"True", "3E87DE17-B962-42C9-9691-B3B27C504034" );

            // Attribute for BlockType: Giving Overview:Inactive Giver Cutoff (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "896D807D-2110-4007-AFD1-4D953B83375B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Inactive Giver Cutoff (Days)", "InactiveGiverCutoff", "Inactive Giver Cutoff (Days)", @"The number of days after which a person is considered an inactive giver.", 0, @"365", "FA5D4E19-5840-4EF1-A861-99AEC3F0BAD3" );

            // Attribute for BlockType: Giving Overview:Alert List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "896D807D-2110-4007-AFD1-4D953B83375B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Alert List Page", "AlertListPage", "Alert List Page", @"The page to see a list of alerts for the person.", 1, @"", "3B85794D-E382-4F65-B931-AE44A789EFF6" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Config Page Attribute for BlockType: Giving Analytics Alerts
            RockMigrationHelper.DeleteAttribute("477A3801-CFB7-4BEA-A5C1-ED9C2B2E1E9D");

            // Scheduled Transaction Detail Page Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("B6861FC8-3741-46BA-9AC6-936C29511065");

            // Create Login Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("9106D3D4-20B0-47D4-81F3-A4FFF4D01D17");

            // Hide Campus if Known Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("D00B6A18-D302-4A8C-ADF7-CD46ABF884C7");

            // Show Notifications Request Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("8458FD9D-C6C1-405B-9A93-CB713CD9A953");

            // Hide Email if Known Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("1A4321DF-3AAD-4A2C-8070-A068820BE827");

            // Email Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("386C39D8-1D77-47F4-94F1-129F03AD7B88");

            // Hide Mobile Phone if Known Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("4AA3A746-C705-418A-8FB5-FB5690D1429D");

            // Mobile Phone Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("E88A11E9-2118-4D70-B8CD-9E7B58AD07D3");

            // Hide Birth Date if Known Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("FD94209A-76D6-4D41-B398-96745F546D5B");

            // Birth Date Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("3E362C81-EB06-444A-8E8D-92A763177B8E");

            // Hide Gender if Known Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("D55F9A7C-CEB0-468A-86B2-A1B34B09B302");

            // Gender Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("2D218849-F96F-4B0B-9B27-5236E8A6F8CB");

            // Create Login Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("EAED4835-A87E-4EEE-8345-D01DF6FF1DD7");

            // Create Login Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("DF658448-7F79-473B-AE1E-F3B2DA7EBBF8");

            // Campus Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("025306AB-40BA-490F-A9C5-BD1FC21E801D");

            // Campus Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("6E725307-72A3-4D2E-AA7E-B9DDE5187A67");

            // Notifications Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("98D17BDF-C2F9-4F78-BFD8-7BE160787A24");

            // Notifications Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("3C3F5F53-D303-4465-A6BF-58C5F1365134");

            // Interests Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("7F13BCFE-A16A-4688-AC74-30848B3DD1B2");

            // Interests Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("7C3F512A-7092-41C8-A6AA-F9427EFB313B");

            // Contact Information Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("2DF996A3-002D-4C89-8AE4-9FC49F846107");

            // Contact Information Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("77D080E2-1CCD-4A1D-A641-20D9F0734ABF");

            // Personal Information Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("29F7B8FD-1DAF-4062-9720-A735F60843AB");

            // Personal Information Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("BBF73E74-45DF-4E41-812D-A704E6409B14");

            // Name Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("6B463687-EDCF-4C44-9BE6-96B00B6C23B0");

            // Name Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("FEC42D9A-A95C-4FEC-95DB-400B738BB250");

            // Code Sent Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("A89FC144-DB98-4129-BAB7-EADA64F8FFCA");

            // Code Sent Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("18D9FB2D-1898-4957-95F0-2A5D2A55526A");

            // Hello Screen Subtitle Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("C93E9926-0010-4B0E-A4D7-AA30B41F0BBF");

            // Hello Screen Title Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("03152B49-B22B-478E-8EB2-BAD959A6E39A");

            // Login Page Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("41B67475-F8D4-42EB-991D-71A327A08077");

            // Completed Page Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("D09D1B5F-A208-48AB-9E80-64E032C071B4");

            // Do Not Attend Campus Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("0512A384-717C-455B-9F18-A4BEAF865D40");

            // Online Campus Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("A2CA2A14-272A-4CE8-9861-3744975E999F");

            // Display Campus Statuses Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("42B6A2C0-37FF-4BA8-8378-791937AECA86");

            // Display Campus Types Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("5F1F626E-65F1-44AC-BF49-40FD21E3EF64");

            // Validation Code Attempts Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("9E421C90-F505-47DC-BC93-CEE75076D653");

            // IP Throttle Limit Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("293A5476-C910-407A-8EAA-CA7AB12A1F55");

            // Verification Time Limit Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("4FE016DB-C77C-4A09-933B-1AAC649CAC95");

            // Communication List Categories Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("A27FADBA-B1AD-4D5B-A11A-B63AF67BD2D5");

            // System Communication Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("CA1F541C-3C37-438B-8D7A-3841E31E05F8");

            // Default Record Status Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("EFF020AA-E9FD-4FB0-9B1E-0446E751844F");

            // Default Connection Status Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("487C6E9E-BF4F-4111-9C17-DEF87EADB213");

            // Allow Skip of Onboarding Attribute for BlockType: Onboard Person
            RockMigrationHelper.DeleteAttribute("ED3671C9-D962-4584-8FD9-D7C2FEBFACE0");

            // Max Results Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("AC0CF5A9-B70E-4B13-BC09-6D74CAB6BBCC");

            // Days Back to Show Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("31EE1DF8-7767-496C-95EB-756FC555A1BA");

            // Show Expired Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("3E87DE17-B962-42C9-9691-B3B27C504034");

            // Template Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("C0F5912F-3A6B-402E-932C-F45CCF12C211");

            // Answer Page Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("861D21C9-34DF-4B77-8127-301F7E87CE70");

            // Edit Page Attribute for BlockType: My Prayer Requests
            RockMigrationHelper.DeleteAttribute("A3B29126-4A56-4D81-80D1-BE6550C38747");

            // Template Attribute for BlockType: Answer To Prayer
            RockMigrationHelper.DeleteAttribute("7478E7E0-0654-4665-A618-1975FF379B0E");

            // Enforce Security Attribute for BlockType: Answer To Prayer
            RockMigrationHelper.DeleteAttribute("2239490F-D4CF-401A-ACC9-7C2233DE9806");

            // Return Page Attribute for BlockType: Answer To Prayer
            RockMigrationHelper.DeleteAttribute("440BB98F-76E9-456B-979E-ABBA9F5272C4");

            // Create Interactions for Prayers Attribute for BlockType: Prayer Session
            RockMigrationHelper.DeleteAttribute("3382895E-1422-417E-84F2-A4750AF2AD70");

            // Add Administrate Security to Item Creator Attribute for BlockType: Data View Detail
            RockMigrationHelper.DeleteAttribute("C8F4F1F6-481E-4829-8415-8A7B862AEDDB");

            // Add Administrate Security to Item Creator Attribute for BlockType: Report Detail
            RockMigrationHelper.DeleteAttribute("3E7BBC50-BE47-43FD-B0B0-47E549AC3336");

            // Create Interactions for Prayers Attribute for BlockType: Prayer Session
            RockMigrationHelper.DeleteAttribute("7BB6BF5E-2E9C-4D53-B0D0-A412E748E46C");

            // Alert List Page Attribute for BlockType: Giving Overview
            RockMigrationHelper.DeleteAttribute("3B85794D-E382-4F65-B931-AE44A789EFF6");

            // Inactive Giver Cutoff (Days) Attribute for BlockType: Giving Overview
            RockMigrationHelper.DeleteAttribute("FA5D4E19-5840-4EF1-A861-99AEC3F0BAD3");

            // Mobile Highlight Text Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("D27F47F7-20A4-4D47-979F-A49372BFFBDB");

            // Mobile Highlight Title Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("BBFBE75E-E360-4B39-874D-43E7E8EC1FBA");

            // Highlight Mobile Phone Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("3535C1D9-1CB1-4791-99D3-487EE4AEBE34");

            // Show Nick Name Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("690798B9-CA9C-4C75-A4F2-E57B6D2A12BB");

            // Show Suffix Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("5D4FDB97-3D16-4FFF-88A8-3C4D8923C3D7");

            // Show Title Attribute for BlockType: Public Profile Edit
            RockMigrationHelper.DeleteAttribute("4EACC79E-9CAE-468A-9025-5D7CBB1C4ECE");

            // Delete BlockType Giving Overview
            RockMigrationHelper.DeleteBlockType("896D807D-2110-4007-AFD1-4D953B83375B"); // Giving Overview
        }

        /// <summary>
        /// DH: Add Mobile Answer To Prayer and Mobile My Prayer Requests blocks
        /// </summary>
        private void RockMobilePrayerBlocksUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Mobile Answer To Prayer Block.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Prayer.AnswerToPrayer",
                "Answer To Prayer",
                "Rock.Blocks.Types.Mobile.Prayer.AnswerToPrayer, Rock, Version=1.12.3.1, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_ANSWER_TO_PRAYER_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Answer To Prayer",
                "Displays an existing prayer request and allows the user to enter the answer to prayer.",
                "Rock.Blocks.Types.Mobile.Prayer.AnswerToPrayer",
                "Mobile > Prayer",
                "324D5295-72E6-42DF-B111-E428E811B786" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Answer To Prayer",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "91C29610-1D77-49A8-A46B-5A35EC67C551",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER,
                "Default",
                @"<StackLayout StyleClass=""prayer-request"">
    <StackLayout StyleClass=""prayer-header""
        Orientation=""Horizontal"">
        <Label StyleClass=""prayer-category,text-gray-500""
            Text=""{{ PrayerRequest.Category.Name | Escape }}""
            HorizontalOptions=""StartAndExpand"" />
        <Label StyleClass=""prayer-date,text-gray-500""
            Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}"" />
    </StackLayout>

    <Label StyleClass=""prayer-text"">
        <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
    </Label>
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            // Mobile My Prayer Requests Block.
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Prayer.MyPrayerRequests",
                "My Prayer Requests",
                "Rock.Blocks.Types.Mobile.Prayer.MyPrayerRequests, Rock, Version=1.12.3.1, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_MY_PRAYER_REQUESTS_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "My Prayer Requests",
                "Shows a list of prayer requests that the user has previously entered.",
                "Rock.Blocks.Types.Mobile.Prayer.MyPrayerRequests",
                "Mobile > Prayer",
                "C095B269-36E2-446A-B73E-2C8CC4B7BF37" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile My Prayer Requests",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "DED26289-4746-4233-A5BD-D4095248023D",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS,
                "Default",
                @"{% if PrayerRequestItems == empty %}
    <Rock:NotificationBox NotificationType=""Information"" Text=""Looks like you don't have any prayer requests."" />
{% else %}
    <StackLayout StyleClass=""prayer-request-list"">
        {% for PrayerRequest in PrayerRequestItems %}
            {% if forloop.index > 1 %}<Rock:Divider />{% endif %}
            <StackLayout StyleClass=""prayer-request"">
                <StackLayout StyleClass=""prayer-header""
                    Orientation=""Horizontal"">
                    <Label StyleClass=""prayer-category,text-gray-500""
                        Text=""{{ PrayerRequest.Category.Name | Escape }}""
                        HorizontalOptions=""StartAndExpand"" />
                    <Label StyleClass=""prayer-date,text-gray-500""
                        Text=""{{ PrayerRequest.EnteredDateTime | Date:'MMM d, yyyy' }}"" />
                </StackLayout>

                <Label StyleClass=""prayer-text"">
                    <Label.Text><![CDATA[{{ PrayerRequest.Text }}]]></Label.Text>
                </Label>
                
                {% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}
                    <Label Text=""Answer:"" StyleClass=""answer-header,text-gray-500"" />
                    <Label StyleClass=""answer-text,text-gray-500"">
                        <Label.Text><![CDATA[{{ PrayerRequest.Answer }}]]></Label.Text>
                    </Label>
                {% endif %}

                <StackLayout Orientation=""Horizontal"" StyleClass=""actions"">
                    {% if AnswerPage != null %}
                        <Button StyleClass=""btn,btn-primary,btn-sm,add-answer-button""
                            Text=""{% if PrayerRequest.Answer != null and PrayerRequest.Answer != '' %}Edit Answer{% else %}Add an Answer{% endif %}""
                            Command=""{Binding PushPage}""
                            CommandParameter=""{{ AnswerPage }}?requestGuid={{ PrayerRequest.Guid}}"" />
                    {% endif %}
                    
                    <ContentView HorizontalOptions=""FillAndExpand"" />

                    {% if EditPage != null %}
                        <Button StyleClass=""btn,btn-link,btn-sm,edit-button""
                            Text=""Edit Request""
                            Command=""{Binding PushPage}""
                            CommandParameter=""{{ EditPage }}?requestGuid={{ PrayerRequest.Guid}}"" />
                    {% endif %}
                    
                    <Button StyleClass=""btn,btn-link,btn-sm,delete-button""
                        Text=""Delete""
                        Command=""{Binding ShowActionPanel}"">
                        <Button.CommandParameter>
                            <Rock:ShowActionPanelParameters Title=""Delete Prayer?"" CancelTitle=""Cancel"">
                                <Rock:ShowActionPanelParameters.DestructiveButton>
                                    <Rock:ActionPanelButton Title=""Delete""
                                        Command=""{Binding Callback}""
                                        CommandParameter="":Delete?requestGuid={{ PrayerRequest.Guid }}"" />
                                </Rock:ShowActionPanelParameters.DestructiveButton>
                            </Rock:ShowActionPanelParameters>
                        </Button.CommandParameter>
                    </Button>
                </StackLayout>
            </StackLayout>
        {% endfor %}
    </StackLayout>
{% endif %}",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// DH: Add Mobile Answer To Prayer and Mobile My Prayer Requests blocks
        /// </summary>
        private void RockMobilePrayerBlocksDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "DED26289-4746-4233-A5BD-D4095248023D" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS );
            RockMigrationHelper.DeleteBlockType( "C095B269-36E2-446A-B73E-2C8CC4B7BF37" );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_MY_PRAYER_REQUESTS_BLOCK_TYPE );

            RockMigrationHelper.DeleteTemplateBlockTemplate( "91C29610-1D77-49A8-A46B-5A35EC67C551" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER );
            RockMigrationHelper.DeleteBlockType( "324D5295-72E6-42DF-B111-E428E811B786" );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_ANSWER_TO_PRAYER_BLOCK_TYPE );
        }

        /// <summary>
        /// GJ: Update Person Attribute Frequency Label
        /// </summary>
        private void UpdatePersonAttributeFrequencyLabel()
        {
            // Person Attribute "Frequency Label" Update Undetermined to "Variable"
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, @"values", @"1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Variable", @"DEDE252F-E8FF-4858-A616-BBE6A6FB95FF" );
        }

        /// <summary>
        /// GJ: KPI Update for Giving Overview
        /// </summary>
        private void GivingOverviewKPIUpdate()
        {
            Sql( MigrationSQL._202104131841162_Rollup_0413_kpiupdate );
        }

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// DH: Update Mobile Calendar Event Item Occurrence View Default Template
        /// </summary>
        private void UpdateMobileCalendarEventItemOccurrenceViewDefaultTemplate()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6593D4EB-2B7A-4C24-8D30-A02991D26BC0",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    
    {% if Event.Photo.Guid %}
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}/GetImage.ashx?Guid={{ Event.Photo.Guid }}"" 
            Aspect=""AspectFill"" 
            Ratio=""4:2""
            Margin=""0,0,0,16"">
            <Rock:RoundedTransformation CornerRadius=""12""  />
        </Rock:Image>
    {% endif %}
    
    <Label StyleClass=""h1"" 
        Text=""{{ Event.Name | Escape }}""  />
    
    <Rock:FieldContainer FieldLayout=""Individual"">
        {% assign scheduledDates = EventItemOccurrence.Schedule.iCalendarContent | DatesFromICal:'all' %}
        {% assign scheduleListing = '' %}
        {% for scheduledDate in scheduledDates %}
            {% if forloop.index <= 5 %}
                {% assign scheduleDateTime = scheduledDate | Date:'dddd, MMMM d, yyyy @ h:mm tt' %}
                {% assign scheduleListing = scheduleListing | Append:scheduleDateTime | Append:'&#xa;'  %}
            {% endif %}

        {% endfor %}
        
        <Rock:Literal Label=""Date / Time"" Text=""{{ scheduleListing | ReplaceLast:'&#xa;', '' }}"" />
    
        {% if EventItemOccurrence.Location != '' %}
            <Rock:Literal Label=""Location"" Text=""{{ EventItemOccurrence.Location }}"" />
        {% endif %}
    </Rock:FieldContainer>
    
    <Rock:Html StyleClass=""text"">
        {{ Event.Description | Escape }}
    </Rock:Html>
    
    {% if EventItemOccurrence.Note != '' %}
        <Label Text=""Note"" StyleClass=""text, font-weight-bold"" />
        <Rock:Html StyleClass=""text"">{{ EventItemOccurrence.Note | Escape }}</Rock:Html>
    {% endif %}

    
    {% if EventItemOccurrence.ContactPersonAliasId != null or EventItemOccurrence.ContactEmail != '' or EventItemOccurrence.ContactPhone != '' %}
        {% if EventItemOccurrence.ContactPersonAliasId != null %}
            <Label Text=""Contact"" StyleClass=""title"" />
            <Label Text=""{{ EventItemOccurrence.ContactPersonAlias.Person.FullName }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactEmail != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactEmail }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactPhone != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactPhone }}"" />
        {% endif %}
    {% endif %}
    
    
    {% assign showRegistration = false %}
    {% assign eventItemOccurrenceLinkages = EventItemOccurrence.Linkages %}
    
    {% assign eventItemOccurrenceLinkagesCount = eventItemOccurrenceLinkages | Size %}
    {% if eventItemOccurrenceLinkagesCount > 0 %}
        {% for eventItemOccurrenceLinkage in eventItemOccurrenceLinkages %}
            {% assign daysTillStartDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime,'m' %}
            {% assign daysTillEndDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime,'m' %}
            {% assign showRegistration = true %}
            {% assign registrationMessage = '' %}
    
            {% if daysTillStartDate and daysTillStartDate > 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if daysTillEndDate and daysTillEndDate < 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if showRegistration == true %}
                {% assign statusLabel = RegistrationStatusLabels[eventItemOccurrenceLinkage.RegistrationInstanceId] %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% assign registrationButtonText = statusLabel %}
                {% else %}
                  {% assign registrationButtonText = statusLabel | Plus:' for ' | Plus:eventItemOccurrenceLinkage.PublicName %}
                {% endif %}
    
                {% if statusLabel == 'Full' %}
                    {% if eventItemOccurrenceLinkagesCount == 1 %}
                      {% assign registrationButtonText = 'Registration Full' %}
                    {% else %}
                      {% assign registrationButtonText = eventItemOccurrenceLinkage.PublicName | Plus: ' (Registration Full) ' %}
                    {% endif %}
                    <Label StyleClass=""text"">{{ registrationButtonText }}</Label>
                {% else %}
                    {% if eventItemOccurrenceLinkage.UrlSlug != '' %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""Slug"" Value=""{{eventItemOccurrenceLinkage.UrlSlug}}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% else %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""EventOccurrenceId"" Value=""{{ eventItemOccurrenceLinkage.EventItemOccurrenceId }}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% endif %}
                {% endif %}
            {% else %}
              <Label StyleClass=""font-weight-bold"" Text=""Registration Information"" />
              <Label StyleClass=""text"" Text=""{{ registrationMessage | Escape }}"" />
            {% endif %}
        {% endfor %}
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }
    }
}
