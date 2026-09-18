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
    public partial class CodeGenerated_20260730 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.Cms.LavaApplicationContent
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.Cms.LavaApplicationContent", "Lava Application Content", "Rock.Blocks.Mobile.Cms.LavaApplicationContent, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "414D49DD-2A75-405E-BA4C-3D48CF7FE96B" );

            // Add/Update Mobile Block Type
            //   Name:Lava Application Content
            //   Category:Mobile > Cms
            //   EntityType:Rock.Blocks.Mobile.Cms.LavaApplicationContent
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Application Content", "Displays content from a Lava Application and hosts its Helix interactions.", "Rock.Blocks.Mobile.Cms.LavaApplicationContent", "Mobile > Cms", "8E3F8E6D-D208-4556-A2C6-7202D0DEB984" );

            // Attribute for BlockType
            //   BlockType: Group Search
            //   Category: Groups
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "53990670-E16B-4226-BCA7-4C475EFB6D2F" );

            // Attribute for BlockType
            //   BlockType: Group Search
            //   Category: Groups
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "04AF0904-0DB3-4F7E-85B0-EE2281B9DB8F" );

            // Attribute for BlockType
            //   BlockType: Group Simple Register
            //   Category: Group
            //   Attribute: Record Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Source", "RecordSource", "Record Source", @"The record source to use for new individuals (default: 'Group Registration').", 5, @"A0F69572-B5C3-4195-8FD1-6FC72BB84FC8", "5BDB4E9C-4AA3-407E-8CE9-7B914034C6A5" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Header Title", "BlockHeaderTitle", "Block Header Title", @"The title displayed at the top of the block.", 0, @"Manage Giving Profiles", "33AE993B-8967-4363-8E71-58F69076B512" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Header Description", "BlockHeaderDescription", "Block Header Description", @"The supporting text displayed below the header title.", 1, @"Your giving profiles are listed below. Edit a profile to change its frequency, start date, or amount. Delete a profile to stop automated giving, or create a new one anytime.", "A5B95783-CE6F-4B1B-BDB5-C61F5B0F1108" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Header Icon", "BlockHeaderIcon", "Block Header Icon", @"The CSS class of the icon displayed in the block header (e.g. 'ti ti-cash').", 2, @"ti ti-cash", "43B47405-9B38-4AAD-9548-EDD136082554" );

            // Attribute for BlockType
            //   BlockType: Group Member Remove From URL
            //   Category: Groups
            //   Attribute: Limit Group Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Limit Group Type", "LimitGroupType", "Limit Group Type", @"To ensure that people cannot modify the URL and try removing people from standard Rock security groups with known Id numbers you can limit which Group Types are considered valid during remove.", 5, @"", "6255766A-9EFF-4B47-B778-18C75AAD87C5" );

            // Attribute for BlockType
            //   BlockType: Communication Queue
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694EB2F6-018D-4E99-A956-202B1FAF7FB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E2FBD709-4B64-45DB-8EB7-F768F68006A0" );

            // Attribute for BlockType
            //   BlockType: Communication Queue
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694EB2F6-018D-4E99-A956-202B1FAF7FB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "ED1AB8EC-AE8B-4363-827B-847AE53DA016" );

            // Attribute for BlockType
            //   BlockType: Person Group History
            //   Category: CRM > Person Detail
            //   Attribute: Years To Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F8E351BC-607E-4897-B732-F590B5155451", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Years To Display", "YearsToDisplay", "Years To Display", @"The number of years of history to include, counting back from today.", 1, @"10", "39957996-3238-4F50-A726-C83FA259C877" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Show Section Headers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Section Headers", "ShowSectionHeaders", "Show Section Headers", @"When enabled, displays a titled header for each section of the form.", 3, @"True", "43FE6884-5C13-4756-B75B-2966DE982304" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Title", "HeaderTitle", "Header Title", @"The title displayed at the top of the block.", 6, @"Edit Giving Profile", "F594F701-22B8-46D3-90DF-F385D4707BED" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Description", "HeaderDescription", "Header Description", @"The supporting text displayed below the header title.", 7, @"Review and update your scheduled transaction details.", "59742AE7-824B-4315-8A5B-2AD0EE7D5387" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Icon", "HeaderIcon", "Header Icon", @"The icon displayed in the block header.", 8, @"ti ti-cash", "D20696AE-E914-4850-97CD-3DB5993E7B9D" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Title", "CampusSectionTitle", "Campus Information Section Title", @"The label displayed in the Campus Information section header.", 9, @"Campus Information", "3ED99123-FBF3-4341-8FBB-EE48E699F0FA" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Icon", "CampusSectionIcon", "Campus Information Section Icon", @"The icon displayed in the Campus Information section header.", 10, @"ti ti-map-pin", "E2E07F7D-85DB-4D24-A6B0-7EE959887887" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Information Section Description", "CampusSectionDescription", "Campus Information Section Description", @"The supporting text displayed below the section title to provide context.", 11, @"Review and update the campus that your gift should be associated with.", "80AA015A-FEBF-4A28-90E1-80EF846ED14F" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Information Section Title", "GiftSectionTitle", "Gift Information Section Title", @"The label displayed in the Gift Information section header.", 12, @"Gift Information", "0A50A6BC-5BB8-4919-B1B8-32F9EB9B8C5C" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Information Section Icon", "GiftSectionIcon", "Gift Information Section Icon", @"The icon displayed in the Gift Information section header.", 13, @"ti ti-gift", "860B991D-28A2-4529-8A96-CE64D1F42D66" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Gift Information Section Description", "GiftSectionDescription", "Gift Information Section Description", @"The supporting text displayed below the section title to provide context.", 14, @"Review and update the details of your scheduled gift.", "9155A984-1229-424B-A810-3D4F430E951B" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Title", "PaymentSectionTitle", "Payment Information Section Title", @"The label displayed in the Payment Information section header.", 15, @"Payment Method", "7FC4A944-FE62-4FC3-A807-998652465B06" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Icon", "PaymentSectionIcon", "Payment Information Section Icon", @"The icon displayed in the Payment Information section header.", 16, @"ti ti-wallet", "0973A255-6E70-4E01-9BF3-669342415759" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Information Section Description", "PaymentSectionDescription", "Payment Information Section Description", @"The supporting text displayed below the section title to provide context.", 17, @"Review and update the payment method your gift will be charged to.", "6ED02B00-6E52-4A76-8BC5-48E231A16283" );

            // Attribute for BlockType
            //   BlockType: Mobile Page Detail
            //   Category: Mobile
            //   Attribute: Layout Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3C4547A-E29B-4CBA-9610-6C19D939183B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Layout Detail Page", "LayoutDetailPage", "Layout Detail Page", @"The page used to view or edit layout details.", 0, @"5583A55D-7398-48E9-971F-6A1EF8158943", "A13A764F-21A7-4D79-A4B8-C650A9560C63" );

            // Attribute for BlockType
            //   BlockType: Connection Status Changes
            //   Category: Connection
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE50DDE5-3D8C-47EC-817D-21348717AD38", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "63551ABE-D7AE-40C1-8B91-5044A616C059" );

            // Attribute for BlockType
            //   BlockType: Connection Status Changes
            //   Category: Connection
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE50DDE5-3D8C-47EC-817D-21348717AD38", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6A6403EA-FFE5-4554-AAC3-94445E27F851" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Overview
            //   Category: Engagement > Sign-Up
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6A32CDBC-A8DB-4C5D-900F-8480DCE50380" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Overview
            //   Category: Engagement > Sign-Up
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "731790CE-AE5D-4582-8344-CBBEC3DCB36A" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD404374-5DA6-4F13-B997-E29494D708A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Non-Public Connection Request Attributes", "ExcludeNonPublicAttributes", "Exclude Non-Public Connection Request Attributes", @"Attributes without 'Public' checked will not be displayed.", 27, @"True", "04F0041F-51B4-49B4-84FC-B800E337300C" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Add Connection Request Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "039AB104-FDFE-4BB0-944A-2C02F4C1D73A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Connection Request Page", "AddPage", "Add Connection Request Page", @"Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameters are passed, so the Add block starts at its Type step. When empty, the floating button is not shown.", 1, @"", "B65E40E6-0574-4E35-B742-7D9811BBC5E6" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Add Connection Request Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Connection Request Page", "AddPage", "Add Connection Request Page", @"Page that hosts the Add Connection Request block, opened by the floating Add button. No page parameters are passed, so the Add block starts at its Type step. When empty, the floating button is not shown.", 1, @"", "9D3DC91F-F117-452F-AB25-2FE0CE62B1CD" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Application
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Application", "Application", "Application", @"The Lava Application this block belongs to. Descendant Helix requests can then use single-segment routes (^/endpoint-slug), and the initial template gets the application's merge fields.", 0, @"", "BEE43436-2363-45F9-8492-42B869BFED3F" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Initial Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Initial Template", "InitialTemplate", "Initial Template", @"The Lava template rendered on the server to produce the block's initial XAML. The 'LavaApplication' and 'ConfigurationRigging' merge fields are available when an application is selected. <span class='tip tip-lava'></span>", 1, @"", "D74889C2-9702-4104-9E96-C66029B20340" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Initial Endpoint
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Initial Endpoint", "InitialEndpoint", "Initial Endpoint", @"A Helix route (for example ^/endpoint-slug) the shell fetches when the block loads. When set it is used instead of the Initial Template.", 2, @"", "9CCA3A48-F122-4897-8C1F-68B8FE2CC789" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled when rendering the initial template.", 3, @"", "FF600908-5865-43AE-B989-4D69241EDB96" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Mobile > Connection
            //   Attribute: Add Connection Request Page
            RockMigrationHelper.DeleteAttribute( "9D3DC91F-F117-452F-AB25-2FE0CE62B1CD" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Add Connection Request Page
            RockMigrationHelper.DeleteAttribute( "B65E40E6-0574-4E35-B742-7D9811BBC5E6" );

            // Attribute for BlockType
            //   BlockType: Group Simple Register
            //   Category: Group
            //   Attribute: Record Source
            RockMigrationHelper.DeleteAttribute( "5BDB4E9C-4AA3-407E-8CE9-7B914034C6A5" );

            // Attribute for BlockType
            //   BlockType: Group Member Remove From URL
            //   Category: Groups
            //   Attribute: Limit Group Type
            RockMigrationHelper.DeleteAttribute( "6255766A-9EFF-4B47-B778-18C75AAD87C5" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "FF600908-5865-43AE-B989-4D69241EDB96" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Initial Endpoint
            RockMigrationHelper.DeleteAttribute( "9CCA3A48-F122-4897-8C1F-68B8FE2CC789" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Initial Template
            RockMigrationHelper.DeleteAttribute( "D74889C2-9702-4104-9E96-C66029B20340" );

            // Attribute for BlockType
            //   BlockType: Lava Application Content
            //   Category: Mobile > Cms
            //   Attribute: Application
            RockMigrationHelper.DeleteAttribute( "BEE43436-2363-45F9-8492-42B869BFED3F" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Description
            RockMigrationHelper.DeleteAttribute( "6ED02B00-6E52-4A76-8BC5-48E231A16283" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Icon
            RockMigrationHelper.DeleteAttribute( "0973A255-6E70-4E01-9BF3-669342415759" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Payment Information Section Title
            RockMigrationHelper.DeleteAttribute( "7FC4A944-FE62-4FC3-A807-998652465B06" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Description
            RockMigrationHelper.DeleteAttribute( "9155A984-1229-424B-A810-3D4F430E951B" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Icon
            RockMigrationHelper.DeleteAttribute( "860B991D-28A2-4529-8A96-CE64D1F42D66" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Gift Information Section Title
            RockMigrationHelper.DeleteAttribute( "0A50A6BC-5BB8-4919-B1B8-32F9EB9B8C5C" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Description
            RockMigrationHelper.DeleteAttribute( "80AA015A-FEBF-4A28-90E1-80EF846ED14F" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Icon
            RockMigrationHelper.DeleteAttribute( "E2E07F7D-85DB-4D24-A6B0-7EE959887887" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Campus Information Section Title
            RockMigrationHelper.DeleteAttribute( "3ED99123-FBF3-4341-8FBB-EE48E699F0FA" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Icon
            RockMigrationHelper.DeleteAttribute( "D20696AE-E914-4850-97CD-3DB5993E7B9D" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Description
            RockMigrationHelper.DeleteAttribute( "59742AE7-824B-4315-8A5B-2AD0EE7D5387" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Header Title
            RockMigrationHelper.DeleteAttribute( "F594F701-22B8-46D3-90DF-F385D4707BED" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit
            //   Category: Finance
            //   Attribute: Show Section Headers
            RockMigrationHelper.DeleteAttribute( "43FE6884-5C13-4756-B75B-2966DE982304" );

            // Attribute for BlockType
            //   BlockType: Mobile Page Detail
            //   Category: Mobile
            //   Attribute: Layout Detail Page
            RockMigrationHelper.DeleteAttribute( "A13A764F-21A7-4D79-A4B8-C650A9560C63" );

            // Attribute for BlockType
            //   BlockType: Group Search
            //   Category: Groups
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "04AF0904-0DB3-4F7E-85B0-EE2281B9DB8F" );

            // Attribute for BlockType
            //   BlockType: Group Search
            //   Category: Groups
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "53990670-E16B-4226-BCA7-4C475EFB6D2F" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Icon
            RockMigrationHelper.DeleteAttribute( "43B47405-9B38-4AAD-9548-EDD136082554" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Description
            RockMigrationHelper.DeleteAttribute( "A5B95783-CE6F-4B1B-BDB5-C61F5B0F1108" );

            // Attribute for BlockType
            //   BlockType: Public Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Block Header Title
            RockMigrationHelper.DeleteAttribute( "33AE993B-8967-4363-8E71-58F69076B512" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Overview
            //   Category: Engagement > Sign-Up
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "731790CE-AE5D-4582-8344-CBBEC3DCB36A" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Overview
            //   Category: Engagement > Sign-Up
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6A32CDBC-A8DB-4C5D-900F-8480DCE50380" );

            // Attribute for BlockType
            //   BlockType: Person Group History
            //   Category: CRM > Person Detail
            //   Attribute: Years To Display
            RockMigrationHelper.DeleteAttribute( "39957996-3238-4F50-A726-C83FA259C877" );

            // Attribute for BlockType
            //   BlockType: Connection Status Changes
            //   Category: Connection
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6A6403EA-FFE5-4554-AAC3-94445E27F851" );

            // Attribute for BlockType
            //   BlockType: Connection Status Changes
            //   Category: Connection
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "63551ABE-D7AE-40C1-8B91-5044A616C059" );

            // Attribute for BlockType
            //   BlockType: Connection Request Entry
            //   Category: Connection
            //   Attribute: Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.DeleteAttribute( "04F0041F-51B4-49B4-84FC-B800E337300C" );

            // Attribute for BlockType
            //   BlockType: Communication Queue
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "ED1AB8EC-AE8B-4363-827B-847AE53DA016" );

            // Attribute for BlockType
            //   BlockType: Communication Queue
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E2FBD709-4B64-45DB-8EB7-F768F68006A0" );

            // Delete BlockType 
            //   Name: Lava Application Content
            //   Category: Mobile > Cms
            //   Path: -
            //   EntityType: Lava Application Content
            RockMigrationHelper.DeleteBlockType( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984" );
        }
    }
}
