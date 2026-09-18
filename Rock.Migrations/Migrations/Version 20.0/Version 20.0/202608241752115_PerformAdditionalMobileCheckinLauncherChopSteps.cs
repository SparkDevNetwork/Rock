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

    /// <summary>
    ///
    /// </summary>
    public partial class PerformAdditionalMobileCheckinLauncherChopSteps : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.MobileCheckInLauncher
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.MobileCheckInLauncher", "Mobile Check In Launcher", "Rock.Blocks.CheckIn.MobileCheckInLauncher, Rock.Blocks, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "FA4A6783-BFAA-4129-AE24-5BF871518EE9" );

            // Add/Update Obsidian Block Type
            //   Name:Mobile Check-in Launcher
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.MobileCheckInLauncher
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Mobile Check-in Launcher", "Launch page for checking in from a person's mobile device.", "Rock.Blocks.CheckIn.MobileCheckInLauncher", "Check-in", "1703315B-6255-499D-9B27-76245A314640" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Enabled Devices
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Enabled Devices", "DeviceIdList", "Enabled Devices", @"The devices to consider when determining a matching device kiosk, or leave blank for all. Typically the selection should include only one device kiosk for each geo-fenced area / campus.", 0, @"", "B32914E1-8E73-4B9D-A1FE-33E0A1327B6A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Theme
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Theme", "CheckinTheme", "Theme", @"The check-in theme this page renders in, overriding the theme configured on the site. Leave blank to use the site's theme.", 1, @"", "A7B3833F-34B1-419D-9E8A-E80313787AE9" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Check-in Configuration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Configuration", "CheckinConfiguration_GroupTypeGuid", "Check-in Configuration", @"The check-in configuration that will be used for the check-in process.", 2, @"FEDD389A-616F-4A53-906C-63D8255631C5", "4D5F9705-B068-4056-AB82-12AE22B75A8D" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Check-in Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Areas", "ConfiguredAreas_GroupTypeIds", "Check-in Areas", @"The check-in areas that will be used for the check-in process.", 3, @"", "A1C95D6C-9174-4A16-99C1-53518001B40A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable Location Services
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Location Services", "DisableLocationServices", "Disable Location Services", @"If disabled, the mobile device's location services will not be used and instead a list of active campuses will be shown. The selected campus will be used to find a matching device from the Devices block setting.", 4, @"False", "30BDBC4F-A75D-4C13-AEE3-F45F4F5BAF8A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable QR Code
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable QR Code", "DisableQRCode", "Disable QR Code", @"If disabled, no QR code is shown on the mobile device after check-in. Use this for events that do not print labels.", 0, @"False", "364FE9EB-3D6C-4424-BADC-88AE1D73897C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Select All Schedules Automatically", "SelectAllSchedulesAutomatically", "Select All Schedules Automatically", @"When enabled, all available schedules are selected automatically instead of asking the individual to make a selection. This will also disable the 'skip' screen when there is nothing to check into, instead those individuals will quietly be skipped and not checked in.", 1, @"False", "752BA9C9-DA54-482E-AFB8-747585E34885" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Log In Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Log In Page", "LoginPage", "Log In Page", @"The page to use for logging in the person. If blank the log in button will not be shown.", 0, @"", "16B44B84-AA7A-46BC-843A-4399681134B1" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Phone Identification Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Phone Identification Page", "PhoneIdentificationPage", "Phone Identification Page", @"Page to use for identifying the person by phone number. If blank the button will not be shown.", 1, @"", "7F7458FD-D046-4F53-900A-070693A18460" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Mobile Check-in Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Mobile Check-in Header", "MobileCheckinHeader", "Mobile Check-in Header", @"", 0, @"Mobile Check-in", "969005AE-F9CE-4036-BFA5-4C5C0358555F" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Identify You Prompt Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Identify You Prompt Template", "IdentifyYouPromptTemplate", "Identify You Prompt Template", @"", 1, @"Before we proceed we'll need to identify you for check-in.", "F8200F73-73FC-4F4C-9FAF-290DE0B97C1A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Allow Location Prompt
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Allow Location Prompt", "AllowLocationPermissionPromptTemplate", "Allow Location Prompt", @"", 2, @"We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to your location when you're visiting this site.", "F4FF3DFC-52E1-427E-B048-4A4D39FA379C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Location Progress
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Location Progress", "LocationProgress", "Location Progress", @"", 3, @"Determining location...", "3FE94D48-24FF-46F8-8E85-8F3838949EB4" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Welcome Back
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Welcome Back", "WelcomeBackTemplate", "Welcome Back", @"", 4, @"Hi {{ CurrentPerson.NickName }}! Great to see you back. Select the Check In button to get started.", "693C90F7-5FA4-4889-8D72-1A1C167DB805" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Services
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Services", "NoScheduledDevicesAvailableTemplate", "No Services", @"", 5, @"Hi {{ CurrentPerson.NickName }}! There are currently no services ready for check-in at this time.", "9D77FB81-0878-4CA8-8D0B-5B75687E8A75" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Can't Determine Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Can't Determine Location", "UnableToDetermineMobileLocationTemplate", "Can't Determine Location", @"", 6, @"Hi {{ CurrentPerson.NickName }}! We can't determine your location. Please be sure to enable location permissions for your device.", "7FC31693-7301-4571-88F6-02E411911D01" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Devices Found
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Devices Found", "NoDevicesFoundTemplate", "No Devices Found", @"", 7, @"Hi {{ CurrentPerson.NickName }}! Currently, you're not close enough to check in. Please try again once you're closer to the campus.", "A2D44523-C747-436C-842B-D1D67F9EB093" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No People Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No People Message", "NoPeopleMessage", "No People Message", @"Text to display when there is not anyone in the family that can check in.", 8, @"Sorry, no one in your family is eligible to check in at this location.", "BEACCB5D-5F95-4800-B5F3-64DC2514701C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Campuses Found
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1703315B-6255-499D-9B27-76245A314640", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "No Campuses Found", "NoCampusesFoundTemplate", "No Campuses Found", @"", 9, @"Hi {{ CurrentPerson.NickName }}! There are currently no active campuses ready for check-in at this time.", "45186DAE-9B78-439A-870C-269E754EDF6E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Campuses Found
            RockMigrationHelper.DeleteAttribute( "45186DAE-9B78-439A-870C-269E754EDF6E" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No People Message
            RockMigrationHelper.DeleteAttribute( "BEACCB5D-5F95-4800-B5F3-64DC2514701C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Devices Found
            RockMigrationHelper.DeleteAttribute( "A2D44523-C747-436C-842B-D1D67F9EB093" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Can't Determine Location
            RockMigrationHelper.DeleteAttribute( "7FC31693-7301-4571-88F6-02E411911D01" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: No Services
            RockMigrationHelper.DeleteAttribute( "9D77FB81-0878-4CA8-8D0B-5B75687E8A75" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Welcome Back
            RockMigrationHelper.DeleteAttribute( "693C90F7-5FA4-4889-8D72-1A1C167DB805" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Location Progress
            RockMigrationHelper.DeleteAttribute( "3FE94D48-24FF-46F8-8E85-8F3838949EB4" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Allow Location Prompt
            RockMigrationHelper.DeleteAttribute( "F4FF3DFC-52E1-427E-B048-4A4D39FA379C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Identify You Prompt Template
            RockMigrationHelper.DeleteAttribute( "F8200F73-73FC-4F4C-9FAF-290DE0B97C1A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Mobile Check-in Header
            RockMigrationHelper.DeleteAttribute( "969005AE-F9CE-4036-BFA5-4C5C0358555F" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Phone Identification Page
            RockMigrationHelper.DeleteAttribute( "7F7458FD-D046-4F53-900A-070693A18460" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Log In Page
            RockMigrationHelper.DeleteAttribute( "16B44B84-AA7A-46BC-843A-4399681134B1" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.DeleteAttribute( "752BA9C9-DA54-482E-AFB8-747585E34885" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable QR Code
            RockMigrationHelper.DeleteAttribute( "364FE9EB-3D6C-4424-BADC-88AE1D73897C" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable Location Services
            RockMigrationHelper.DeleteAttribute( "30BDBC4F-A75D-4C13-AEE3-F45F4F5BAF8A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Check-in Areas
            RockMigrationHelper.DeleteAttribute( "A1C95D6C-9174-4A16-99C1-53518001B40A" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Check-in Configuration
            RockMigrationHelper.DeleteAttribute( "4D5F9705-B068-4056-AB82-12AE22B75A8D" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Theme
            RockMigrationHelper.DeleteAttribute( "A7B3833F-34B1-419D-9E8A-E80313787AE9" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Enabled Devices
            RockMigrationHelper.DeleteAttribute( "B32914E1-8E73-4B9D-A1FE-33E0A1327B6A" );

            // Delete BlockType 
            //   Name: Mobile Check-in Launcher
            //   Category: Check-in
            //   Path: -
            //   EntityType: Mobile Check In Launcher
            RockMigrationHelper.DeleteBlockType( "1703315B-6255-499D-9B27-76245A314640" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.MobileCheckInLauncher
            RockMigrationHelper.DeleteEntityType( "FA4A6783-BFAA-4129-AE24-5BF871518EE9" );
        }
    }
}
