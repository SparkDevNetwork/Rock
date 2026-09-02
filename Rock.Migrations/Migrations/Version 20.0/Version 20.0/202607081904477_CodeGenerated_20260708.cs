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
    public partial class CodeGenerated_20260708 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.AddConnectionRequestV2
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.AddConnectionRequestV2", "Add Connection Request V2", "Rock.Blocks.Mobile.Connection.AddConnectionRequestV2, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "553609B0-49E3-4E52-9D63-7F10C03D249E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2", "Connection Opportunity List V2", "Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "8DD07282-8470-426C-8F89-7390599DB37F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2", "Connection Request Detail V2", "Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "8B53B246-526F-4B3E-AF5B-4C36763E9DC9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionRequestListV2
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.ConnectionRequestListV2", "Connection Request List V2", "Rock.Blocks.Mobile.Connection.ConnectionRequestListV2, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "CC91A1ED-7FB0-43B3-A8B4-A050DBF6BA6D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionTypeListV2
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.ConnectionTypeListV2", "Connection Type List V2", "Rock.Blocks.Mobile.Connection.ConnectionTypeListV2, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "88E9C088-5CCE-41F9-B99E-C3B03E123316" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Connection.MyConnectionRequests
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Connection.MyConnectionRequests", "My Connection Requests", "Rock.Blocks.Mobile.Connection.MyConnectionRequests, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "1160B498-50D7-4E8F-9B23-BFD87B7E7F22" );

            // Add/Update Mobile Block Type
            //   Name:Add Connection Request V2
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.AddConnectionRequestV2
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Add Connection Request V2", "Creates a new connection request through a multi-step wizard.", "Rock.Blocks.Mobile.Connection.AddConnectionRequestV2", "Mobile > Connection", "5A198A75-177C-4A2A-8558-BFB5A4EFCB30" );

            // Add/Update Mobile Block Type
            //   Name:Connection Opportunity List V2
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Opportunity List V2", "Displays the opportunities of a connection type with request count summaries and type metrics.", "Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2", "Mobile > Connection", "039AB104-FDFE-4BB0-944A-2C02F4C1D73A" );

            // Add/Update Mobile Block Type
            //   Name:Connection Request Detail V2
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Request Detail V2", "Displays a single connection request for viewing and editing.", "Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2", "Mobile > Connection", "74DDC1A2-2025-4072-8F47-DF7A5A76CF83" );

            // Add/Update Mobile Block Type
            //   Name:Connection Request List V2
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionRequestListV2
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Request List V2", "Displays the connection requests of a single connection opportunity with search, filtering, sorting and infinite-scroll paging.", "Rock.Blocks.Mobile.Connection.ConnectionRequestListV2", "Mobile > Connection", "117ADAF8-8173-4A88-8C88-2C97F88985DC" );

            // Add/Update Mobile Block Type
            //   Name:Connection Type List V2
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.ConnectionTypeListV2
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Type List V2", "Displays the list of connection types with request count summaries.", "Rock.Blocks.Mobile.Connection.ConnectionTypeListV2", "Mobile > Connection", "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A" );

            // Add/Update Mobile Block Type
            //   Name:My Connection Requests
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Mobile.Connection.MyConnectionRequests
            RockMigrationHelper.AddOrUpdateEntityBlockType( "My Connection Requests", "Displays the current person's connection requests across all opportunities, grouped, searchable, sortable and filterable client-side.", "Rock.Blocks.Mobile.Connection.MyConnectionRequests", "Mobile > Connection", "C6C6A0A3-D381-4A13-A5D0-EAA4302E78F1" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DEFF313-39CF-400F-895A-82ADB9F192BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F91CD0C3-E652-4CE8-8A19-C83E4A8484DC" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DEFF313-39CF-400F-895A-82ADB9F192BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EF498CD5-B046-476D-AB1D-1681330FDFE4" );

            // Attribute for BlockType
            //   BlockType: Pages
            //   Category: Administration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEFC2DBE-37B6-4CAB-882C-B214F587BF2E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B9087081-EB4C-4A9B-B2B0-637F0E2311E8" );

            // Attribute for BlockType
            //   BlockType: Pages
            //   Category: Administration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEFC2DBE-37B6-4CAB-882C-B214F587BF2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6687C0E4-E0D8-4118-B612-73A9E509C8F6" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Connection Opportunities List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Connection Opportunities List Title", "ConnectionOpportunitiesListTitle", "Connection Opportunities List Title", @"The label displayed above the Connection Opportunities checkbox list.", 0, @"Connection Opportunities", "B05CBD0B-094B-4D72-9E72-CFEBB0CA8781" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Connection Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "50DA6F25-E81E-46E8-A773-4B479B4FB9E6", "Connection Type", "ConnectionType", "Connection Type", @"Connection opportunities from the configured type are shown as checkboxes. If no type is configured, the section is hidden.", 1, @"", "DE2A6109-C96B-42CB-8631-B6A33EDFB461" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Communication List Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Communication List Selection", "EnableCommunicationListSelection", "Enable Communication List Selection", @"Set this to true to let the sender choose a Communication List as the recipient source instead of adding recipients individually.", 17, @"False", "3D485BD2-5AFA-432A-8485-FBAD59AFC5D0" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Display Banner
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Banner", "DisplayBanner", "Display Banner", @"Controls whether to show a banner at the top of the form.", 1, @"True", "549FF975-29D7-4673-AB30-3B890902E529" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: First Time Guest
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "First Time Guest", "FirstTimeGuest", "First Time Guest", @"Controls whether the form shows the first-time guest option and whether it is required.", 2, @"Hide", "C0956F71-4B14-4D4D-BD47-367E102060F2" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: First Time Guest Opportunity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "B188B729-FE6D-498B-8871-65AB8FD1E11E", "First Time Guest Opportunity", "FirstTimeGuestOpportunity", "First Time Guest Opportunity", @"The opportunity used when a person selects ""I am a first time guest,"" adding their request to this additional opportunity.", 3, @"", "226CBA3B-5F62-4EDE-904F-F5BD8D4BBF42" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Title", "Title", "Title", @"Controls whether the form shows the person's title (such as Mr. or Mrs.) and whether it is required.", 4, @"Hide", "BB0B53EA-BA7E-413F-8F7F-F1429F905344" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Suffix
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Suffix", "Suffix", "Suffix", @"Controls whether the form shows the person's name suffix (such as Jr., Sr., or III), and whether it is required.", 5, @"Hide", "023E2B22-B60F-4C13-AA36-1DCB274B7B8D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Birthdate
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birthdate", "Birthdate", "Birthdate", @"Controls whether the form shows the person's birthdate and whether it is required.", 6, @"Show", "CCE79C05-D6B1-4DE7-8856-CAC03BFFBFC8" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "Gender", "Gender", @"Controls whether the form shows the person's gender and whether it is required.", 7, @"Show", "F578D07B-686B-4BA2-84D2-72AD668C5376" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Profile Photo
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Profile Photo", "ProfilePhoto", "Profile Photo", @"Controls whether the form shows the person's profile photo and whether it is required.", 8, @"Hide", "E29A2CD5-80AD-41DA-A253-AA04569AC0DD" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Marital Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Marital Status", "MaritalStatus", "Marital Status", @"Controls whether the form shows the person's marital status and whether it is required.", 9, @"Show", "52074B84-580E-4261-9937-F97711304FA1" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse First Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Spouse First Name", "SpouseFirstName", "Spouse First Name", @"Controls whether the form shows the spouse's first name and whether it is required. Note: this will only show if Marital Status is set as ""Married.""", 10, @"Show", "EDEAEA72-0E2C-4E9F-98D2-AF8CEC97FAED" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Spouse Last Name", "SpouseLastName", "Spouse Last Name", @"Controls whether the form shows the spouse's last name and whether it is required. Note: this will only show if Marital Status is set as ""Married.""", 11, @"Show", "18D516AA-58DE-47B6-8D22-38B636EB480D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Spouse Gender", "SpouseGender", "Spouse Gender", @"Controls whether the form shows the spouse's gender and whether it is required. Note: this will only show if Marital Status is set as ""Married.""", 12, @"Show", "52D40A41-58D0-4306-94BE-2EC6C95CB45D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "Email", "Email", @"Controls whether the form shows the person's email address and whether it is required.", 13, @"Required", "9AD53246-23C9-4399-8254-927A3E4014AC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Spouse Email", "SpouseEmail", "Spouse Email", @"Controls whether the form shows the spouse's email address and whether it is required. Note: this will only show if Marital Status is set as ""Married.""", 14, @"Hide", "E329171A-AE68-46A9-A42B-7D72D692D94F" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "MobilePhone", "Mobile Phone", @"Controls whether the form shows the person's mobile phone number and whether it is required.", 15, @"Show", "FE49C3FF-1B3B-4733-907D-DCD8BB80511D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Spouse Mobile Phone", "SpouseMobilePhone", "Spouse Mobile Phone", @"Controls whether the form shows the spouse's mobile phone number and whether it is required. Note: this will only show if Marital Status is set as ""Married.""", 16, @"Hide", "9CC2DBEF-30F7-4DAF-8871-48F34006A8A3" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: SMS Enabled
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "SMS Enabled", "SmsEnabled", "SMS Enabled", @"Controls whether the form shows consent to receive text messages and whether it is required.", 17, @"Show", "6E641941-A43B-4248-8F76-44A71ED3098A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "Address", "Address", @"Controls whether the form shows the person's address and whether it is required.", 18, @"Show", "35C31ED9-A6BF-42CD-ADCE-83039562290D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Comments
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Additional Comments", "AdditionalComments", "Additional Comments", @"Controls whether the form shows additional comments and whether it is required.", 19, @"Show", "2D9264A4-A4C0-48D5-924B-A3FAEB199D67" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Enable Captcha
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Captcha", "EnableCaptcha", "Enable Captcha", @"Determines whether CAPTCHA verification is enabled for this form.", 20, @"False", "93EF0206-6895-4FEB-BC8A-336CF9A0582B" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Person Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Person Attribute Category", "PersonAttributeCategory", "Person Attribute Category", @"The category used to determine which person attributes are available on the form.", 21, @"", "5A1427A4-2D6B-4E61-9C13-AFC612220E83" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Preferred Service Time
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Preferred Service Time", "PreferredServiceTime", "Preferred Service Time", @"The category used to determine which service times are available on the form to set as Preferred. If no category is set, the field is hidden.", 22, @"", "32B1937E-4BF8-4DA9-A6E0-C1AAB8076B47" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Optional Redirect URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Optional Redirect URL", "OptionalRedirectUrl", "Optional Redirect URL", @"The URL to redirect the person to after a request is submitted. Leaving blank will generate a default completion message.", 23, @"", "EEEE5669-FC25-4A33-AFA5-ED4EE193F099" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Prospect').", 24, @"368DD475-242C-49C4-A42C-7278BE690CC2", "0BC5D46A-12A6-46D0-8615-ED4120FBEFD1" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending').", 25, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "CEA76A46-88ED-452F-BE7C-9072034C2DEC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Record Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Source", "RecordSource", "Record Source", @"The record source to use for new individuals (default: 'Serving Connection'). If a 'RecordSource' page parameter is found, it will be used instead.", 26, @"2CF9DE9F-14D5-4036-B329-85B192A63A9B", "FD15E5A6-5E2B-4DA5-9977-E64E8AC79C0C" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Banner Icon", "BannerIcon", "Banner Icon", @"The icon used to display in the banner.", 27, @"ti ti-route-alt-left", "F8DE21F7-5235-45FA-955D-6A9DF1BF84A3" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Banner Title", "BannerTitle", "Banner Title", @"The title used to display in the banner.", 28, @"Next Steps", "8B7F54AC-2A39-4FF6-8B3D-F349022690EC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Banner Description", "BannerDescription", "Banner Description", @"The description used to display in the banner.", 29, @"We want to connect with you and help you take a next step!", "99A84E60-0A42-4972-AA34-C57D31FB0E69" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Personal Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Information Section Title", "PersonalInformationTitle", "Personal Information Section Title", @"The title displayed for the personal information section.", 30, @"Personal Information", "F70B810C-D619-4C13-A735-0C82834DD642" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Personal Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Information Section Description", "PersonalInformationDescription", "Personal Information Section Description", @"The supporting text displayed below the section title to provide context.", 31, @"Help us get to know you and support you more personally.", "3950F111-2683-4F3A-A3B3-D0F67BC28A5A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Contact Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Section Title", "ContactInformationTitle", "Contact Information Section Title", @"The title displayed for the contact information section.", 32, @"Contact Information", "96A4474F-9C87-411B-8CCA-02271C75DF52" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Contact Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Information Section Description", "ContactInformationDescription", "Contact Information Section Description", @"The supporting text displayed below the section title to provide context.", 33, @"Provide the best ways for us to stay in touch with you.", "BC68A8ED-BD08-464D-B902-2937D15D9211" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Comments Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Comments Label", "AdditionalCommentsLabel", "Additional Comments Label", @"The text field label for capturing additional comments.", 34, @"Additional Comments", "A7DCD4C9-823D-42A3-85BC-41BA1D594291" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Opportunities Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Connection Opportunities Section Title", "ConnectionOpportunitiesTitle", "Connection Opportunities Section Title", @"The title displayed for the connection opportunities section.", 35, @"Connection Opportunities", "792B235C-126D-44CD-9613-DB4E711DB7E0" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Opportunities Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Connection Opportunities Section Description", "ConnectionOpportunitiesDescription", "Connection Opportunities Section Description", @"The supporting text displayed below the section title to provide context.", 36, @"Select the areas where you'd like to get involved.", "914C08BE-E549-4021-B1DB-0B2DE502D2D7" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Information Section Title", "AdditionalInformationTitle", "Additional Information Section Title", @"The title displayed for the additional information section.", 37, @"Additional Information", "F20EDADD-42C8-4721-88B1-7EE5A5828A37" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Information Section Description", "AdditionalInformationDescription", "Additional Information Section Description", @"The supporting text displayed below the section title to provide context.", 38, @"Provide any additional details to help us better understand your request to get connected.", "BF1F7414-CE06-4D47-AE66-4E39D8571DCE" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Submission Success Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submission Success Section Title", "SubmissionSuccessTitle", "Submission Success Section Title", @"The headline displayed after a connection request is successfully submitted.", 39, @"Submitted Connection Request Successfully", "F60D7209-8FE5-46D3-AC36-80B0861DA487" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Submission Success Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submission Success Section Description", "SubmissionSuccessDescription", "Submission Success Section Description", @"The message displayed after submission to confirm the request was received.", 40, @"Thanks for taking a step to get more connected! We'll be in contact soon.", "DDB7F8A0-43FE-4536-9600-F7863D5219E9" );

            // Attribute for BlockType
            //   BlockType: Documents
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "710C19AD-1D9A-4939-89E3-F619C4E02E40" );

            // Attribute for BlockType
            //   BlockType: Documents
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6835CDB9-CFE8-4980-B9F0-35942F3D9206" );

            // Attribute for BlockType
            //   BlockType: Person Search
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5B738B35-F5C1-4D22-9D86-0D1E381C72A5" );

            // Attribute for BlockType
            //   BlockType: Person Search
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F2DDDC62-54EE-441B-9EBA-FF918BFD0D8D" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Opportunity Attendee List
            //   Category: Engagement > Sign-Up
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE652767-5070-4EAB-8BB7-BB254DD01B46", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FCBDA08E-7C65-4FAD-8C26-24F1AC93DA29" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Opportunity Attendee List
            //   Category: Engagement > Sign-Up
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE652767-5070-4EAB-8BB7-BB254DD01B46", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0B488391-4E4E-4E64-8E72-3B06F83AE3F1" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Show Images Toggle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Images Toggle", "ShowImagesToggle", "Show Images Toggle", @"Determines whether the 'Show Images' option is available in the grid options menu.", 3, @"False", "C21C72CF-9E91-4260-9D19-5FD5ACC5F834" );

            // Attribute for BlockType
            //   BlockType: Giving Automation Alerts
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C59A0D86-CE14-451A-8E09-82D58429BACC" );

            // Attribute for BlockType
            //   BlockType: Giving Automation Alerts
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C47A558C-DEA8-4456-A2A1-FA619487130D" );

            // Attribute for BlockType
            //   BlockType: Fundraising Leader Toolbox
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A3FF6700-FCBA-49B8-9E7F-BE7C978CB6DE" );

            // Attribute for BlockType
            //   BlockType: Fundraising Leader Toolbox
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6685832D-95B6-44AC-A9A7-B455DD2B1ED1" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C770A7B2-56BB-42C8-9E93-3A189A84351F" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "64BD0F0B-9CB3-453B-808E-3E8E3963F328" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request V2
            //   Category: Mobile > Connection
            //   Attribute: Post Save Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A198A75-177C-4A2A-8558-BFB5A4EFCB30", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Save Action", "PostSaveAction", "Post Save Action", @"The navigation action to perform after the request is saved. 'ConnectionRequest' is passed as a route parameter with the new request's IdKey.", 0, @"{""Type"": 1, ""PopCount"": 1}", "A6F42D55-02D4-4AB8-9982-87B75F307CB1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "039AB104-FDFE-4BB0-944A-2C02F4C1D73A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when the individual taps an opportunity. The connection opportunity IdKey is passed as the ConnectionOpportunity page parameter.", 0, @"", "0E73C8F8-30E2-49D3-B8F6-D124CB8D478A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page to link to when the requester is tapped. The requester's PersonGuid is passed.", 0, @"", "B3D828B7-3394-4CBD-881F-5902A8E50761" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page to link to when the placement group is tapped. The group's Guid is passed.", 1, @"", "4D8CE56C-3CDA-41B5-81B0-B894C2237AF6" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Workflow Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Page", "WorkflowPage", "Workflow Page", @"Page to link to when a launched manual workflow needs an interactive entry form. The workflow Guid is passed.", 2, @"", "05E98347-9158-40C4-AFB4-F2FF912AB246" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Reminder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Page", "ReminderPage", "Reminder Page", @"Page that hosts the Reminder block, opened in a cover sheet by the Reminder quick-action. When empty, the Reminder card is hidden.", 3, @"", "AD5EB4CD-CDF6-4BA4-B988-A576FBC64CAA" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "117ADAF8-8173-4A88-8C88-2C97F88985DC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when the individual taps a request. The connection request IdKey is passed as the ConnectionRequest page parameter.", 0, @"", "3AF7FC79-DB3A-4430-97ED-89DA5B0686B4" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "117ADAF8-8173-4A88-8C88-2C97F88985DC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Page", "AddPage", "Add Page", @"Page that hosts the Add Connection Request block, opened by the floating Add button. The current ConnectionOpportunity IdKey is passed as a page parameter so the Add block prefills and locks the Type and Opportunity. When empty, the floating button is not shown.", 1, @"", "AA0D7BE9-99A0-4D15-8006-7A9E4C06ED0B" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "117ADAF8-8173-4A88-8C88-2C97F88985DC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Page Size", "PageSize", "Page Size", @"The number of requests fetched per load (the infinite-scroll page size).", 2, @"15", "23DD39F2-5331-414D-BA72-A7355A7184C8" );

            // Attribute for BlockType
            //   BlockType: Connection Type List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when the individual taps a connection type. The connection type IdKey is passed as the ConnectionType page parameter.", 0, @"", "A2B584A4-213F-4038-BD1B-76AB554D3F2A" );

            // Attribute for BlockType
            //   BlockType: My Connection Requests
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C6A0A3-D381-4A13-A5D0-EAA4302E78F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when the individual taps a request. The connection request IdKey is passed as the ConnectionRequest page parameter.", 0, @"", "9C2D810D-892F-4E19-ABB6-DF0B638A9A49" );

            // Attribute for BlockType
            //   BlockType: My Connection Requests
            //   Category: Mobile > Connection
            //   Attribute: Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C6A0A3-D381-4A13-A5D0-EAA4302E78F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Page", "AddPage", "Add Page", @"Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameter is passed (this is a cross-opportunity worklist), so the Add screen opens with its Type and Opportunity pickers unlocked. When empty, the floating button is not shown.", 1, @"", "E4FB1C6D-DE6D-42A3-A643-FAAE9266AB95" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Request Filter Boost Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Request Filter Boost Amount", "RequestFilterBoostAmount", "Request Filter Boost Amount", @"The amount of boost to apply to matches on personalization request filters.", 0, @"", "DB4660F5-0AF8-4084-BC59-3F0278AD0CCF" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Stop Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Stop Action", "StopAction", "Stop Action", @"The navigation action to perform when the stop button is pressed.", 3, @"{""Type"": 1, ""PopCount"": 1}", "027DE1B1-63D6-44EE-B530-EFCE275DB577" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Contact Page", "AddContact", "Add Contact Page", @"The page to open when someone taps on add contact button.", 3, @"", "618B731A-8F44-4518-BDA6-0BD33B8F509E" );

            // Attribute for BlockType
            //   BlockType: Outreach Recent Activity
            //   Category: Engagement
            //   Attribute: Contact Profile
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "469B0581-C7F6-4F8B-913F-E20F5B49E39D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Contact Profile", "ContactProfile", "Contact Profile", @"The page to open when someone taps on the person recent activity", 0, @"", "61A982F2-D108-43CA-9F73-4CBD27B0618A" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Outreach Recent Activity
            //   Category: Engagement
            //   Attribute: Contact Profile
            RockMigrationHelper.DeleteAttribute( "61A982F2-D108-43CA-9F73-4CBD27B0618A" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.DeleteAttribute( "618B731A-8F44-4518-BDA6-0BD33B8F509E" );

            // Attribute for BlockType
            //   BlockType: Voice Agent
            //   Category: Mobile > Cms
            //   Attribute: Stop Action
            RockMigrationHelper.DeleteAttribute( "027DE1B1-63D6-44EE-B530-EFCE275DB577" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Request Filter Boost Amount
            RockMigrationHelper.DeleteAttribute( "DB4660F5-0AF8-4084-BC59-3F0278AD0CCF" );

            // Attribute for BlockType
            //   BlockType: My Connection Requests
            //   Category: Mobile > Connection
            //   Attribute: Add Page
            RockMigrationHelper.DeleteAttribute( "E4FB1C6D-DE6D-42A3-A643-FAAE9266AB95" );

            // Attribute for BlockType
            //   BlockType: My Connection Requests
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "9C2D810D-892F-4E19-ABB6-DF0B638A9A49" );

            // Attribute for BlockType
            //   BlockType: Connection Type List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "A2B584A4-213F-4038-BD1B-76AB554D3F2A" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Page Size
            RockMigrationHelper.DeleteAttribute( "23DD39F2-5331-414D-BA72-A7355A7184C8" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Add Page
            RockMigrationHelper.DeleteAttribute( "AA0D7BE9-99A0-4D15-8006-7A9E4C06ED0B" );

            // Attribute for BlockType
            //   BlockType: Connection Request List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "3AF7FC79-DB3A-4430-97ED-89DA5B0686B4" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Reminder Page
            RockMigrationHelper.DeleteAttribute( "AD5EB4CD-CDF6-4BA4-B988-A576FBC64CAA" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Workflow Page
            RockMigrationHelper.DeleteAttribute( "05E98347-9158-40C4-AFB4-F2FF912AB246" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "4D8CE56C-3CDA-41B5-81B0-B894C2237AF6" );

            // Attribute for BlockType
            //   BlockType: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute( "B3D828B7-3394-4CBD-881F-5902A8E50761" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List V2
            //   Category: Mobile > Connection
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0E73C8F8-30E2-49D3-B8F6-D124CB8D478A" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request V2
            //   Category: Mobile > Connection
            //   Attribute: Post Save Action
            RockMigrationHelper.DeleteAttribute( "A6F42D55-02D4-4AB8-9982-87B75F307CB1" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "64BD0F0B-9CB3-453B-808E-3E8E3963F328" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C770A7B2-56BB-42C8-9E93-3A189A84351F" );

            // Attribute for BlockType
            //   BlockType: Fundraising Leader Toolbox
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6685832D-95B6-44AC-A9A7-B455DD2B1ED1" );

            // Attribute for BlockType
            //   BlockType: Fundraising Leader Toolbox
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "A3FF6700-FCBA-49B8-9E7F-BE7C978CB6DE" );

            // Attribute for BlockType
            //   BlockType: Giving Automation Alerts
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "C47A558C-DEA8-4456-A2A1-FA619487130D" );

            // Attribute for BlockType
            //   BlockType: Giving Automation Alerts
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C59A0D86-CE14-451A-8E09-82D58429BACC" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Show Images Toggle
            RockMigrationHelper.DeleteAttribute( "C21C72CF-9E91-4260-9D19-5FD5ACC5F834" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Opportunity Attendee List
            //   Category: Engagement > Sign-Up
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "0B488391-4E4E-4E64-8E72-3B06F83AE3F1" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Opportunity Attendee List
            //   Category: Engagement > Sign-Up
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FCBDA08E-7C65-4FAD-8C26-24F1AC93DA29" );

            // Attribute for BlockType
            //   BlockType: Person Search
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F2DDDC62-54EE-441B-9EBA-FF918BFD0D8D" );

            // Attribute for BlockType
            //   BlockType: Person Search
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5B738B35-F5C1-4D22-9D86-0D1E381C72A5" );

            // Attribute for BlockType
            //   BlockType: Documents
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6835CDB9-CFE8-4980-B9F0-35942F3D9206" );

            // Attribute for BlockType
            //   BlockType: Documents
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "710C19AD-1D9A-4939-89E3-F619C4E02E40" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Submission Success Section Description
            RockMigrationHelper.DeleteAttribute( "DDB7F8A0-43FE-4536-9600-F7863D5219E9" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Submission Success Section Title
            RockMigrationHelper.DeleteAttribute( "F60D7209-8FE5-46D3-AC36-80B0861DA487" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Information Section Description
            RockMigrationHelper.DeleteAttribute( "BF1F7414-CE06-4D47-AE66-4E39D8571DCE" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Information Section Title
            RockMigrationHelper.DeleteAttribute( "F20EDADD-42C8-4721-88B1-7EE5A5828A37" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Opportunities Section Description
            RockMigrationHelper.DeleteAttribute( "914C08BE-E549-4021-B1DB-0B2DE502D2D7" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Opportunities Section Title
            RockMigrationHelper.DeleteAttribute( "792B235C-126D-44CD-9613-DB4E711DB7E0" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Comments Label
            RockMigrationHelper.DeleteAttribute( "A7DCD4C9-823D-42A3-85BC-41BA1D594291" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Contact Information Section Description
            RockMigrationHelper.DeleteAttribute( "BC68A8ED-BD08-464D-B902-2937D15D9211" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Contact Information Section Title
            RockMigrationHelper.DeleteAttribute( "96A4474F-9C87-411B-8CCA-02271C75DF52" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Personal Information Section Description
            RockMigrationHelper.DeleteAttribute( "3950F111-2683-4F3A-A3B3-D0F67BC28A5A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Personal Information Section Title
            RockMigrationHelper.DeleteAttribute( "F70B810C-D619-4C13-A735-0C82834DD642" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Description
            RockMigrationHelper.DeleteAttribute( "99A84E60-0A42-4972-AA34-C57D31FB0E69" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Title
            RockMigrationHelper.DeleteAttribute( "8B7F54AC-2A39-4FF6-8B3D-F349022690EC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Banner Icon
            RockMigrationHelper.DeleteAttribute( "F8DE21F7-5235-45FA-955D-6A9DF1BF84A3" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Record Source
            RockMigrationHelper.DeleteAttribute( "FD15E5A6-5E2B-4DA5-9977-E64E8AC79C0C" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute( "CEA76A46-88ED-452F-BE7C-9072034C2DEC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute( "0BC5D46A-12A6-46D0-8615-ED4120FBEFD1" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Optional Redirect URL
            RockMigrationHelper.DeleteAttribute( "EEEE5669-FC25-4A33-AFA5-ED4EE193F099" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Preferred Service Time
            RockMigrationHelper.DeleteAttribute( "32B1937E-4BF8-4DA9-A6E0-C1AAB8076B47" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Person Attribute Category
            RockMigrationHelper.DeleteAttribute( "5A1427A4-2D6B-4E61-9C13-AFC612220E83" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Enable Captcha
            RockMigrationHelper.DeleteAttribute( "93EF0206-6895-4FEB-BC8A-336CF9A0582B" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Additional Comments
            RockMigrationHelper.DeleteAttribute( "2D9264A4-A4C0-48D5-924B-A3FAEB199D67" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Address
            RockMigrationHelper.DeleteAttribute( "35C31ED9-A6BF-42CD-ADCE-83039562290D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: SMS Enabled
            RockMigrationHelper.DeleteAttribute( "6E641941-A43B-4248-8F76-44A71ED3098A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Mobile Phone
            RockMigrationHelper.DeleteAttribute( "9CC2DBEF-30F7-4DAF-8871-48F34006A8A3" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Mobile Phone
            RockMigrationHelper.DeleteAttribute( "FE49C3FF-1B3B-4733-907D-DCD8BB80511D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Email
            RockMigrationHelper.DeleteAttribute( "E329171A-AE68-46A9-A42B-7D72D692D94F" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Email
            RockMigrationHelper.DeleteAttribute( "9AD53246-23C9-4399-8254-927A3E4014AC" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Gender
            RockMigrationHelper.DeleteAttribute( "52D40A41-58D0-4306-94BE-2EC6C95CB45D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse Last Name
            RockMigrationHelper.DeleteAttribute( "18D516AA-58DE-47B6-8D22-38B636EB480D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Spouse First Name
            RockMigrationHelper.DeleteAttribute( "EDEAEA72-0E2C-4E9F-98D2-AF8CEC97FAED" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Marital Status
            RockMigrationHelper.DeleteAttribute( "52074B84-580E-4261-9937-F97711304FA1" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Profile Photo
            RockMigrationHelper.DeleteAttribute( "E29A2CD5-80AD-41DA-A253-AA04569AC0DD" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Gender
            RockMigrationHelper.DeleteAttribute( "F578D07B-686B-4BA2-84D2-72AD668C5376" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Birthdate
            RockMigrationHelper.DeleteAttribute( "CCE79C05-D6B1-4DE7-8856-CAC03BFFBFC8" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Suffix
            RockMigrationHelper.DeleteAttribute( "023E2B22-B60F-4C13-AA36-1DCB274B7B8D" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Title
            RockMigrationHelper.DeleteAttribute( "BB0B53EA-BA7E-413F-8F7F-F1429F905344" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: First Time Guest Opportunity
            RockMigrationHelper.DeleteAttribute( "226CBA3B-5F62-4EDE-904F-F5BD8D4BBF42" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: First Time Guest
            RockMigrationHelper.DeleteAttribute( "C0956F71-4B14-4D4D-BD47-367E102060F2" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Display Banner
            RockMigrationHelper.DeleteAttribute( "549FF975-29D7-4673-AB30-3B890902E529" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Communication List Selection
            RockMigrationHelper.DeleteAttribute( "3D485BD2-5AFA-432A-8485-FBAD59AFC5D0" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Connection Type
            RockMigrationHelper.DeleteAttribute( "DE2A6109-C96B-42CB-8631-B6A33EDFB461" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Connection Opportunities List Title
            RockMigrationHelper.DeleteAttribute( "B05CBD0B-094B-4D72-9E72-CFEBB0CA8781" );

            // Attribute for BlockType
            //   BlockType: Pages
            //   Category: Administration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6687C0E4-E0D8-4118-B612-73A9E509C8F6" );

            // Attribute for BlockType
            //   BlockType: Pages
            //   Category: Administration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B9087081-EB4C-4A9B-B2B0-637F0E2311E8" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "EF498CD5-B046-476D-AB1D-1681330FDFE4" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F91CD0C3-E652-4CE8-8A19-C83E4A8484DC" );

            // Delete BlockType 
            //   Name: My Connection Requests
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: My Connection Requests
            RockMigrationHelper.DeleteBlockType( "C6C6A0A3-D381-4A13-A5D0-EAA4302E78F1" );

            // Delete BlockType 
            //   Name: Connection Type List V2
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Connection Type List V2
            RockMigrationHelper.DeleteBlockType( "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A" );

            // Delete BlockType 
            //   Name: Connection Request List V2
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Connection Request List V2
            RockMigrationHelper.DeleteBlockType( "117ADAF8-8173-4A88-8C88-2C97F88985DC" );

            // Delete BlockType 
            //   Name: Connection Request Detail V2
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Connection Request Detail V2
            RockMigrationHelper.DeleteBlockType( "74DDC1A2-2025-4072-8F47-DF7A5A76CF83" );

            // Delete BlockType 
            //   Name: Connection Opportunity List V2
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Connection Opportunity List V2
            RockMigrationHelper.DeleteBlockType( "039AB104-FDFE-4BB0-944A-2C02F4C1D73A" );

            // Delete BlockType 
            //   Name: Add Connection Request V2
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Add Connection Request V2
            RockMigrationHelper.DeleteBlockType( "5A198A75-177C-4A2A-8558-BFB5A4EFCB30" );
        }
    }
}
