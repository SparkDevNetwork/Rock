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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class MobileCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpCheckinPagesAndBlocks();

            UpCheckinPagesSecurity();
        }

        /// <summary>
        /// Ups the checkin pages security.
        /// </summary>
        private void UpCheckinPagesSecurity()
        {
            // Clear Security on all of the Checkin pages, except for Admin, Welcome, Search and Family Select
            Dictionary<string, string> pageGuidAuthGuids = new Dictionary<string, string>
            {
                {  Rock.SystemGuid.Page.CHECKIN_PERSON_SELECT, "496bf09b-94cb-416d-8567-e489e6079dad" },
                {  Rock.SystemGuid.Page.CHECKIN_GROUP_TYPE_SELECT, "115443d8-02d0-4e97-b38b-82fe6d55d4af" },
                {  Rock.SystemGuid.Page.CHECKIN_ABILITY_SELECT, "bd087f02-6a67-43f6-9a84-882feb355dc6" },
                {  Rock.SystemGuid.Page.CHECKIN_GROUP_SELECT, "c40cccfe-a87c-4216-901b-189775d77cab" },
                {  Rock.SystemGuid.Page.CHECKIN_LOCATION_SELECT, "b36b37f8-95ed-4eaa-b71b-ad338da4be0a" },
                {  Rock.SystemGuid.Page.CHECKIN_TIME_SELECT, "46e4f184-4989-4af7-85e9-bbe5aa54dd0f" },
                {  Rock.SystemGuid.Page.CHECKIN_SUCCESS, "fb217720-38a0-45ba-9ed0-f6909a5d8eed" },
                {  Rock.SystemGuid.Page.CHECKIN_SCHEDULED_LOCATIONS, "d689a45b-4f42-4682-b9b1-d8dcb1fded45" },
                {  Rock.SystemGuid.Page.CHECKIN_PERSON_SELECT_FAMILY_CHECK_IN, "9da3c702-2d20-4e6b-8299-4281144277a7" },
                {  Rock.SystemGuid.Page.CHECKIN_TIME_SELECT_FAMILY_CHECK_IN, "ed8bda34-2cef-4622-a98b-20f9d1c74e8c" },
                {  Rock.SystemGuid.Page.CHECKIN_SAVE_ATTENDANCE_FAMILY_CHECK_IN, "70181ff8-7fef-4c70-990b-ba4053cfd099" },
                {  Rock.SystemGuid.Page.CHECKIN_ACTION_SELECT, "4d158628-3d1b-40cc-bb6b-0d786d993073" },
                {  Rock.SystemGuid.Page.CHECKIN_CHECK_OUT_PERSON_SELECT, "ac224b13-3431-41cd-b44e-3ef38f8c8153" },
                {  Rock.SystemGuid.Page.CHECKIN_CHECK_OUT_SUCCESS, "752427b9-822b-428b-a182-6a4f8271323f" },
            };

            foreach ( var pageGuidAuthGuid in pageGuidAuthGuids )
            {
                RockMigrationHelper.DeleteSecurityAuthForPage( pageGuidAuthGuid.Key );
                RockMigrationHelper.AddSecurityAuthForPage( pageGuidAuthGuid.Key, 0, Rock.Security.Authorization.VIEW, true, null, ( int ) Rock.Model.SpecialRole.AllUsers, pageGuidAuthGuid.Value );
            }
        }

        /// <summary>
        /// Ups the new checkin pages and blocks.
        /// </summary>
        private void UpCheckinPagesAndBlocks()
        {
            // Add Page Mobile Check-in Launcher to Site:Rock Check-in
            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Mobile Check-in Launcher", "", "2D0CD3CA-E952-4A63-B968-94833F95B389", "" );

            // Update Page Titles and set RequiresEncryption to true
            Sql( "UPDATE [Page] set PageTitle = 'Mobile Check-in', InternalName = 'Mobile Check-in Launcher', BrowserTitle = 'Mobile Check-in', RequiresEncryption = 1 where [Guid] ='2D0CD3CA-E952-4A63-B968-94833F95B389'" );

            // Add Page Route for Mobile Check-in Launcher
            RockMigrationHelper.AddPageRoute( "2D0CD3CA-E952-4A63-B968-94833F95B389", "mobilecheckin", "DE20BBFD-5097-4E8A-8866-C073AB14E6F3" );

            // Add/Update BlockType Mobile Check-in Launcher
            RockMigrationHelper.UpdateBlockType( "Mobile Check-in Launcher", "Launch page for checking in from a person's mobile device.", "~/Blocks/CheckIn/MobileLauncher.ascx", "Check-in", "FA4D15E6-4C85-4247-A374-5E592E711CFD" );

            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType( "Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "4B9177E3-EBCF-4EB9-B938-2D05BD01487B" );

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType( "Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "355F33A3-7E16-422B-837F-0A437BA61DED" );

            // Add Block Mobile Check-in Launcher to Page: Mobile Check-in Launcher, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "2D0CD3CA-E952-4A63-B968-94833F95B389".AsGuid(), null, "15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A".AsGuid(), "FA4D15E6-4C85-4247-A374-5E592E711CFD".AsGuid(), "Mobile Check-in Launcher", "Main", @"", @"", 0, "41F3637D-8F1A-4B32-9A4C-A12868B0122A" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "355F33A3-7E16-422B-837F-0A437BA61DED", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "1E784A23-0EF9-413F-ACA0-42F31C2D2F13" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "355F33A3-7E16-422B-837F-0A437BA61DED", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "83E1C2BA-67F7-40A7-8019-24D93C2FAB81" );

            // Attribute for BlockType: Mobile Check-in Launcher:Devices
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Devices", "DeviceIdList", "Devices", @"The devices to consider for determining the kiosk. No value would consider all devices in the system. If none are selected, then use all devices.", 1, @"", "81C77B62-6860-483C-8F00-B2CBA061EF02" );

            // Attribute for BlockType: Mobile Check-in Launcher:Check-in Theme
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Theme", "CheckinTheme", "Check-in Theme", @"The check-in theme to pass to the check-in pages.", 2, @"", "FE66ED5B-9E18-4BB1-A7C2-743A85C106E0" );

            // Attribute for BlockType: Mobile Check-in Launcher:Check-in Configuration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Configuration", "CheckinConfiguration_GroupTypeGuid", "Check-in Configuration", @"The check-in configuration to use.", 0, @"", "145BC47F-D6B6-4FE0-9B13-8082BE6FE2CE" );

            // Attribute for BlockType: Mobile Check-in Launcher:Check-in Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Areas", "ConfiguredAreas_GroupTypeIds", "Check-in Areas", @"The check-in areas to use.", 0, @"", "78BD9BE9-77FA-4FFE-B1D7-3D4A2CB07C8C" );

            // Attribute for BlockType: Mobile Check-in Launcher:Login Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Login Page", "LoginPage", "Login Page", @"The page to use for logging in the person. If blank the login button will not be shown", 100, @"", "5F2CD7F4-2A8B-4002-9596-0351FE0F5C17" );

            // Attribute for BlockType: Mobile Check-in Launcher:Phone Identification Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Phone Identification Page", "PhoneIdentificationPage", "Phone Identification Page", @"Page to use for identifying the person by phone number. If blank the button will not be shown.", 101, @"", "D75AD1B8-9967-4610-B956-D3C7ED0EDD79" );

            // Attribute for BlockType: Mobile Check-in Launcher:Mobile check-in header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Mobile check-in header", "MobileCheckinHeader", "Mobile check-in header", @"", 1, @"Mobile Check-in", "794959B1-E8FC-4C2F-B1B2-00AB84A0420A" );

            // Attribute for BlockType: Mobile Check-in Launcher:Identify you Prompt Template <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Identify you Prompt Template <span class='tip tip-lava'></span>", "IdentifyYouPromptTemplate", "Identify you Prompt Template <span class='tip tip-lava'></span>", @"", 2, @"Before we proceed we'll need you to identify you for check-in.", "768FDEDB-0845-4356-BA81-9F331D73E7B9" );

            // Attribute for BlockType: Mobile Check-in Launcher:Allow Location Prompt <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Allow Location Prompt <span class='tip tip-lava'></span>", "AllowLocationPermissionPromptTemplate", "Allow Location Prompt <span class='tip tip-lava'></span>", @"", 3, @"We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to your location when you're visiting this site.", "02353459-D9CD-4B4E-A3EA-39015B64E33B" );

            // Attribute for BlockType: Mobile Check-in Launcher:Location Progress <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Location Progress <span class='tip tip-lava'></span>", "LocationProgress", "Location Progress <span class='tip tip-lava'></span>", @"", 4, @"Determining location...", "90D92D89-D470-4294-9E04-3EABC6E36AD6" );

            // Attribute for BlockType: Mobile Check-in Launcher:Welcome Back <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Welcome Back <span class='tip tip-lava'></span>", "WelcomeBackTemplate", "Welcome Back <span class='tip tip-lava'></span>", @"", 5, @"Hi {{ CurrentPerson.NickName }}! Great to see you back. Select the check-in button to get started.", "31522B57-67DE-45AE-9126-372E1487FD14" );

            // Attribute for BlockType: Mobile Check-in Launcher:No Services <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Services <span class='tip tip-lava'></span>", "NoScheduledDevicesAvailableTemplate", "No Services <span class='tip tip-lava'></span>", @"", 6, @"Hi {{ CurrentPerson.NickName }}! There are currently no services ready for check-in at this time.", "7851BD1B-6050-4F3A-8618-A12C0BCB9698" );

            // Attribute for BlockType: Mobile Check-in Launcher:Can't determine location. <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Can't determine location. <span class='tip tip-lava'></span>", "UnableToDetermineMobileLocationTemplate", "Can't determine location. <span class='tip tip-lava'></span>", @"", 7, @"Hi {{ CurrentPerson.NickName }}! We can't determine your location. Please be sure to enable location permissions for your device.", "A1788EF0-6E6E-4083-A5BF-70A19C16CE07" );

            // Attribute for BlockType: Mobile Check-in Launcher:No Devices Found <span class='tip tip-lava'></span>
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Devices Found <span class='tip tip-lava'></span>", "NoDevicesFoundTemplate", "No Devices Found <span class='tip tip-lava'></span>", @"", 8, @"Hi {{ CurrentPerson.NickName }}! Currently you're not close enough to check in. Please try again once you're closer to the campus.", "09CF44CA-4C85-4B43-921B-E71799189731" );

            // Attribute for BlockType: Mobile Check-in Launcher:No People Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No People Message", "NoPeopleMessage", "No People Message", @"Text to display when there is not anyone in the family that can check-in", 8, @"Sorry, no one in your family is eligible to check in at this location.", "33C8B31A-5913-46EB-B466-2B384D4A7AA1" );

            // Attribute for BlockType: Mobile Check-in Launcher:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The workflow type to activate for check-in", 0, @"", "399B4C3B-57C2-4AEC-81A5-B70C5E14DF77" );

            // Attribute for BlockType: Mobile Check-in Launcher:Workflow Activity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "Workflow Activity", @"The name of the workflow activity to run on selection.", 1, @"", "2A36E1FA-33BD-4F0E-9BB3-C5D24489D9BA" );

            // Attribute for BlockType: Mobile Check-in Launcher:Home Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "Home Page", @"", 2, @"", "087D8783-AE2A-418B-A4ED-16AC60E918AA" );

            // Attribute for BlockType: Mobile Check-in Launcher:Previous Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "Previous Page", @"", 3, @"", "22B3BB45-E7E3-4B45-A785-8F7C4CA9A058" );

            // Attribute for BlockType: Mobile Check-in Launcher:Next Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "Next Page", @"", 4, @"", "E1137AB6-01BB-4732-9FF6-4D074447CEAE" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "5F2CD7F4-2A8B-4002-9596-0351FE0F5C17", @"d025e14c-f385-43fb-a735-abd24adc1480,4257a25e-8f4b-4e6c-9e09-822804c01891" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "D75AD1B8-9967-4610-B956-D3C7ED0EDD79", @"9f8d906f-adb6-42ac-9777-d9712a5d097f,1fb5a224-9e26-47e6-9a20-5b5a59b5c7cf" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "794959B1-E8FC-4C2F-B1B2-00AB84A0420A", @"Mobile Check-in" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "768FDEDB-0845-4356-BA81-9F331D73E7B9", @"Before we proceed we'll need you to identify you for check-in." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "02353459-D9CD-4B4E-A3EA-39015B64E33B", @"We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to your location when you're visiting this site." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "90D92D89-D470-4294-9E04-3EABC6E36AD6", @"Determining location..." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "31522B57-67DE-45AE-9126-372E1487FD14", @"Hi {{ CurrentPerson.NickName }}! Great to see you back. Select the check-in button to get started." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "7851BD1B-6050-4F3A-8618-A12C0BCB9698", @"Hi {{ CurrentPerson.NickName }}! There are currently no services ready for check-in at this time." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "A1788EF0-6E6E-4083-A5BF-70A19C16CE07", @"Hi {{ CurrentPerson.NickName }}! We can't determine your location. Please be sure to enable location permissions for your device." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "09CF44CA-4C85-4B43-921B-E71799189731", @"Hi {{ CurrentPerson.NickName }}! Currently you are not close enough to check in. Please try again once you're closer to the campus." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "33C8B31A-5913-46EB-B466-2B384D4A7AA1", @"Sorry, no one in your family is eligible to check in at this location." );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "399B4C3B-57C2-4AEC-81A5-B70C5E14DF77", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "2A36E1FA-33BD-4F0E-9BB3-C5D24489D9BA", @"Person Search" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "087D8783-AE2A-418B-A4ED-16AC60E918AA", @"2d0cd3ca-e952-4a63-b968-94833f95b389,de20bbfd-5097-4e8a-8866-c073ab14e6f3" );

            // Block Attribute Value for Mobile Check-in Launcher ( Page: Mobile Check-in Launcher, Site: Rock Check-in )
            RockMigrationHelper.AddBlockAttributeValue( "41F3637D-8F1A-4B32-9A4C-A12868B0122A", "E1137AB6-01BB-4732-9FF6-4D074447CEAE", @"0586648b-9490-43c6-b18d-7f403458c080" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Next Page Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "E1137AB6-01BB-4732-9FF6-4D074447CEAE" );

            // Previous Page Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "22B3BB45-E7E3-4B45-A785-8F7C4CA9A058" );

            // Home Page Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "087D8783-AE2A-418B-A4ED-16AC60E918AA" );

            // Workflow Activity Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "2A36E1FA-33BD-4F0E-9BB3-C5D24489D9BA" );

            // Workflow Type Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "399B4C3B-57C2-4AEC-81A5-B70C5E14DF77" );

            // No People Message Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "33C8B31A-5913-46EB-B466-2B384D4A7AA1" );

            // No Devices Found <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "09CF44CA-4C85-4B43-921B-E71799189731" );

            // Can't determine location. <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "A1788EF0-6E6E-4083-A5BF-70A19C16CE07" );

            // No Services <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "7851BD1B-6050-4F3A-8618-A12C0BCB9698" );

            // Welcome Back <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "31522B57-67DE-45AE-9126-372E1487FD14" );

            // Location Progress <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "90D92D89-D470-4294-9E04-3EABC6E36AD6" );

            // Allow Location Prompt <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "02353459-D9CD-4B4E-A3EA-39015B64E33B" );

            // Identify you Prompt Template <span class='tip tip-lava'></span> Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "768FDEDB-0845-4356-BA81-9F331D73E7B9" );

            // Mobile check-in header Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "794959B1-E8FC-4C2F-B1B2-00AB84A0420A" );

            // Phone Identification Page Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "D75AD1B8-9967-4610-B956-D3C7ED0EDD79" );

            // Login Page Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "5F2CD7F4-2A8B-4002-9596-0351FE0F5C17" );

            // Check-in Areas Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "78BD9BE9-77FA-4FFE-B1D7-3D4A2CB07C8C" );

            // Check-in Configuration Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "145BC47F-D6B6-4FE0-9B13-8082BE6FE2CE" );

            // Check-in Theme Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "FE66ED5B-9E18-4BB1-A7C2-743A85C106E0" );

            // Devices Attribute for BlockType: Mobile Check-in Launcher
            RockMigrationHelper.DeleteAttribute( "81C77B62-6860-483C-8F00-B2CBA061EF02" );

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute( "83E1C2BA-67F7-40A7-8019-24D93C2FAB81" );

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute( "1E784A23-0EF9-413F-ACA0-42F31C2D2F13" );

            // Remove Block: Mobile Check-in Launcher, from Page: Mobile Check-in Launcher, Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "41F3637D-8F1A-4B32-9A4C-A12868B0122A" );
            RockMigrationHelper.DeleteBlockType( "FA4D15E6-4C85-4247-A374-5E592E711CFD" ); // Mobile Check-in Launcher
            RockMigrationHelper.DeleteBlockType( "355F33A3-7E16-422B-837F-0A437BA61DED" ); // Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType( "4B9177E3-EBCF-4EB9-B938-2D05BD01487B" ); // Structured Content View
            RockMigrationHelper.DeletePage( "2D0CD3CA-E952-4A63-B968-94833F95B389" ); //  Page: Mobile Check-in Launcher, Layout: Checkin, Site: Rock Check-in
        }
    }
}
