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
    public partial class CodeGenerated_20220614 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Core.Attributes", "Attributes", "Rock.Blocks.Core.Attributes, Rock.Blocks, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null", false, false, "A7D9C259-1CD0-42C2-B708-4D95F2469B18");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.CampusDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Core.CampusDetail", "Campus Detail", "Rock.Blocks.Core.CampusDetail, Rock.Blocks, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null", false, false, "A61EAF51-5DB4-451E-9F88-9D4C6ACCE73B");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CRM.PersonDetail.Badges
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.CRM.PersonDetail.Badges", "Badges", "Rock.Blocks.CRM.PersonDetail.Badges, Rock.Blocks, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null", false, false, "86E12A6C-2086-4562-B50E-3EA1E8B5B017");

            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "791DB49B-58A4-44E1-AEF5-ABFF2F37E197");

            // Add/Update Obsidian Block Type
            //   Name:Campus Detail
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.CampusDetail
            RockMigrationHelper.UpdateMobileBlockType("Campus Detail", "Displays the details of a particular campus.", "Rock.Blocks.Core.CampusDetail", "Obsidian > Core", "507F5108-FB55-48F0-A66E-CC3D5185D35D");

            // Add/Update Obsidian Block Type
            //   Name:Badges
            //   Category:Obsidian > CRM > Person Detail
            //   EntityType:Rock.Blocks.CRM.PersonDetail.Badges
            RockMigrationHelper.UpdateMobileBlockType("Badges", "Handles displaying badges for a person.", "Rock.Blocks.CRM.PersonDetail.Badges", "Obsidian > CRM > Person Detail", "2412C653-9369-4772-955E-80EE8FA051E3");

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"Determines whether the campus picker should be shown. This allows the group locations to be filtered for a specific campus.", 6, @"True", "7B1107AE-5DA0-48E7-89DA-A62CB28E3F37" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Addresses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Addresses", "ShowAddresses", "Show Addresses", @"Whether the address section is shown or not during editing.", 8, @"True", "AB0C7921-6F08-4035-9216-B4250D290534" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Email Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Preference", "ShowEmailPreference", "Show Email Preference", @"Show the email preference and allow it to be edited", 17, @"True", "BC798FE1-01B5-45FF-AB3F-CFC9B8B14E71" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "Show Gender", @"Whether gender is shown or not.", 27, @"True", "E7A02485-8402-4681-B2C3-11366703D4E6" );

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Bulk Update Requests
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Update Requests", "BulkUpdateRequestsPage", "Bulk Update Requests", @"Page used to update selected connection requests", 16, @"1F5D34CF-89C1-426C-A139-83D87905D669", "4EC85E6D-4E3D-4B75-8A47-8FFEEA420D22" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template Detail
            //   Category: Core
            //   Attribute: Show Legacy Signature Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legacy Signature Providers", "ShowLegacyExternalProviders", "Show Legacy Signature Providers", @"Enable this setting to see the configuration for legacy signature providers. Note that support for these providers will be fully removed in the next full release.", 1, @"False", "4D23ADF3-7A3C-42C6-B211-519578B9291B" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "9434F17F-F28C-4CEF-B65A-1A42CB7A17DC" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "73B89571-4DE3-4024-9428-F6943DA927E5" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "1B403129-DBB7-4534-AD11-8467479DAF65" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "B921CB38-9D1F-4717-B4F9-D370BB8B3219" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "9AE7890A-1ABD-409C-9F3F-26D8497EC8EA" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "F09CAD11-1232-4F12-8875-BC22BA2A7693" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "6A19842F-A9FE-4AC2-A91F-2C88C54FD4FF" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "89A57BFC-839E-4469-AD44-27D29EB04CCA" );

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Left Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2412C653-9369-4772-955E-80EE8FA051E3", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Top Left Badges", "TopLeftBadges", "Top Left Badges", @"The badges that displayed in the top left section of the badge bar.", 0, @"", "B22C1920-16D2-45B3-97FD-2EF00D38DD25" );

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Middle Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2412C653-9369-4772-955E-80EE8FA051E3", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Top Middle Badges", "TopMiddleBadges", "Top Middle Badges", @"The badges that displayed in the top middle section of the badge bar.", 1, @"", "61540A33-0905-4ABE-A164-F5F5BA8524DD" );

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Right Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2412C653-9369-4772-955E-80EE8FA051E3", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Top Right Badges", "TopRightBadges", "Top Right Badges", @"The badges that displayed in the top right section of the badge bar.", 2, @"", "D87184D5-50A4-47D3-BA50-06D39FCCD328" );

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Bottom Left Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2412C653-9369-4772-955E-80EE8FA051E3", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Bottom Left Badges", "BottomLeftBadges", "Bottom Left Badges", @"The badges that displayed in the bottom left section of the badge bar.", 3, @"", "B85FB9F5-208D-40FD-AD99-8A12EE76029C" );

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Bottom Right Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2412C653-9369-4772-955E-80EE8FA051E3", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Bottom Right Badges", "BottomRightBadges", "Bottom Right Badges", @"The badges that displayed in the bottom right section of the badge bar.", 4, @"", "B39D9047-7D28-42C5-A776-03D71E3A0F36" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type Detail
            //   Category: Finance
            //   Attribute: Maximum Number of Documents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C96479B6-E309-4B1A-B024-1F1276122A13", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Number of Documents", "MaximumNumberOfDocuments", "Maximum Number of Documents", @"The maximum number of documents that can be added to a request.", 2, @"6", "5F247E73-B615-4CCE-A125-68D30EB32DCD" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Width
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node Width", "NodeWidth", "Node Width", @"How many pixels wide should the nodes be?", 1, @"12", "ACF76AD4-0B84-4BD2-AA99-CBC7F9B06806" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Vertical Spacing
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node Vertical Spacing", "NodeVerticalSpacing", "Node Vertical Spacing", @"How many pixels should separate the nodes vertically?", 2, @"12", "832FD747-4914-4F6A-92E7-65A4B8B90860" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Horizontal Spacing
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node Horizontal Spacing", "NodeHorizontalSpacing", "Node Horizontal Spacing", @"How many pixels wide should the flow paths be between the nodes?", 3, @"200", "57EA2D21-6755-41B9-B467-1847DDD50B8B" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Chart Height", "ChartHeight", "Chart Height", @"How tall should the chart be (in pixels)?", 4, @"900", "D8102550-26FE-41BC-9DEB-6DC03722A0A3" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Create Person If No Match Found
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Create Person If No Match Found", "CreatePersonIfNoMatchFound", "Create Person If No Match Found", @"When person matching is enabled this setting determines if a person should be created if a matched record is not found. This setting has no impact if person matching is disabled.", 15, @"True", "B886331D-A78D-4EC4-9935-22480A92A468" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use when creating new person records.", 16, @"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061", "E417A83D-71C4-426A-9B38-801708C2341D" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use when creating new person records.", 17, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "C8455664-DEA3-4D24-ABBA-108FBC258C49" );

            // Attribute for BlockType
            //   BlockType: Metric Detail
            //   Category: Reporting
            //   Attribute: Command Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Command Timeout", "CommandTimeout", "Command Timeout", @"Maximum amount of time (in seconds) to wait for any SQL based operations to complete. Leave blank to use the default for this metric (300). Note, some metrics do not use SQL so this timeout will only apply to metrics that are SQL based.", 5, @"300", "A3260852-71C0-41FE-920A-55ED6E0B317E" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FBE0419-5404-4866-85A1-135542D33725", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "6BF384B1-095E-4A12-9E92-273C5F326DF7" );
            RockMigrationHelper.UpdateFieldType("Categorized Defined Value","","Rock","Rock.Field.Types.CategorizedDefinedValueFieldType","3217C31F-85B6-4E0D-B6BE-2ADB0D28588D");
            RockMigrationHelper.UpdateFieldType("Signature Document Template","","Rock","Rock.Field.Types.SignatureDocumentTemplateFieldType","258A4AEF-F555-4AF5-8D5D-2D581A982D1C");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("6BF384B1-095E-4A12-9E92-273C5F326DF7");

            // Attribute for BlockType
            //   BlockType: Metric Detail
            //   Category: Reporting
            //   Attribute: Command Timeout
            RockMigrationHelper.DeleteAttribute("A3260852-71C0-41FE-920A-55ED6E0B317E");

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Record Status
            RockMigrationHelper.DeleteAttribute("C8455664-DEA3-4D24-ABBA-108FBC258C49");

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Connection Status
            RockMigrationHelper.DeleteAttribute("E417A83D-71C4-426A-9B38-801708C2341D");

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Create Person If No Match Found
            RockMigrationHelper.DeleteAttribute("B886331D-A78D-4EC4-9935-22480A92A468");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Height
            RockMigrationHelper.DeleteAttribute("D8102550-26FE-41BC-9DEB-6DC03722A0A3");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Horizontal Spacing
            RockMigrationHelper.DeleteAttribute("57EA2D21-6755-41B9-B467-1847DDD50B8B");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Vertical Spacing
            RockMigrationHelper.DeleteAttribute("832FD747-4914-4F6A-92E7-65A4B8B90860");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Node Width
            RockMigrationHelper.DeleteAttribute("ACF76AD4-0B84-4BD2-AA99-CBC7F9B06806");

            // Attribute for BlockType
            //   BlockType: Benevolence Type Detail
            //   Category: Finance
            //   Attribute: Maximum Number of Documents
            RockMigrationHelper.DeleteAttribute("5F247E73-B615-4CCE-A125-68D30EB32DCD");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Bottom Right Badges
            RockMigrationHelper.DeleteAttribute("B39D9047-7D28-42C5-A776-03D71E3A0F36");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Bottom Left Badges
            RockMigrationHelper.DeleteAttribute("B85FB9F5-208D-40FD-AD99-8A12EE76029C");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Right Badges
            RockMigrationHelper.DeleteAttribute("D87184D5-50A4-47D3-BA50-06D39FCCD328");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Middle Badges
            RockMigrationHelper.DeleteAttribute("61540A33-0905-4ABE-A164-F5F5BA8524DD");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Attribute: Top Left Badges
            RockMigrationHelper.DeleteAttribute("B22C1920-16D2-45B3-97FD-2EF00D38DD25");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("89A57BFC-839E-4469-AD44-27D29EB04CCA");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("6A19842F-A9FE-4AC2-A91F-2C88C54FD4FF");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("F09CAD11-1232-4F12-8875-BC22BA2A7693");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("9AE7890A-1ABD-409C-9F3F-26D8497EC8EA");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("B921CB38-9D1F-4717-B4F9-D370BB8B3219");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("1B403129-DBB7-4534-AD11-8467479DAF65");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("73B89571-4DE3-4024-9428-F6943DA927E5");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("9434F17F-F28C-4CEF-B65A-1A42CB7A17DC");

            // Attribute for BlockType
            //   BlockType: Signature Document Template Detail
            //   Category: Core
            //   Attribute: Show Legacy Signature Providers
            RockMigrationHelper.DeleteAttribute("4D23ADF3-7A3C-42C6-B211-519578B9291B");

            // Attribute for BlockType
            //   BlockType: Connection Request Board
            //   Category: Connection
            //   Attribute: Bulk Update Requests
            RockMigrationHelper.DeleteAttribute("4EC85E6D-4E3D-4B75-8A47-8FFEEA420D22");

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Gender
            RockMigrationHelper.DeleteAttribute("E7A02485-8402-4681-B2C3-11366703D4E6");

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Email Preference
            RockMigrationHelper.DeleteAttribute("BC798FE1-01B5-45FF-AB3F-CFC9B8B14E71");

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Show Addresses
            RockMigrationHelper.DeleteAttribute("AB0C7921-6F08-4035-9216-B4250D290534");

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Show Campus
            RockMigrationHelper.DeleteAttribute("7B1107AE-5DA0-48E7-89DA-A62CB28E3F37");

            // Delete BlockType 
            //   Name: Badges
            //   Category: Obsidian > CRM > Person Detail
            //   Path: -
            //   EntityType: Badges
            RockMigrationHelper.DeleteBlockType("2412C653-9369-4772-955E-80EE8FA051E3");

            // Delete BlockType 
            //   Name: Campus Detail
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Campus Detail
            RockMigrationHelper.DeleteBlockType("507F5108-FB55-48F0-A66E-CC3D5185D35D");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("791DB49B-58A4-44E1-AEF5-ABFF2F37E197");
        }
    }
}
