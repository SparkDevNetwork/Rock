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
    public partial class CodeGenerated_20240509 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.AttendanceList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.AttendanceList", "Attendance List", "Rock.Blocks.CheckIn.AttendanceList, Rock.Blocks, Version=1.16.5.3, Culture=neutral, PublicKeyToken=null", false, false, "73FD78DF-5322-4716-A478-3CD0EA07A942" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersistedDatasetList", "Persisted Dataset List", "Rock.Blocks.Cms.PersistedDatasetList, Rock.Blocks, Version=1.16.5.3, Culture=neutral, PublicKeyToken=null", false, false, "DC11E26E-7E4A-4550-AF2D-2C9B94BEED4E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceLinkageList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationInstanceLinkageList", "Registration Instance Linkage List", "Rock.Blocks.Event.RegistrationInstanceLinkageList, Rock.Blocks, Version=1.16.5.3, Culture=neutral, PublicKeyToken=null", false, false, "7C5B1E75-0571-4D62-90A5-0B2431EBB9E8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BenevolenceRequestList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BenevolenceRequestList", "Benevolence Request List", "Rock.Blocks.Finance.BenevolenceRequestList, Rock.Blocks, Version=1.16.5.3, Culture=neutral, PublicKeyToken=null", false, false, "D1245F63-A9BA-4289-BD82-44A489F9DA9A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.RestKeyList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.RestKeyList", "Rest Key List", "Rock.Blocks.Security.RestKeyList, Rock.Blocks, Version=1.16.5.3, Culture=neutral, PublicKeyToken=null", false, false, "55E010D1-152A-4745-8E1D-2DB2195F2B36" );

            // Add/Update Obsidian Block Type
            //   Name:Attendance List
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.AttendanceList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attendance List", "Block for displaying the attendance history of a person or a group.", "Rock.Blocks.CheckIn.AttendanceList", "Check-in", "E07607C6-5428-4CCF-A826-060F48CACD32" );

            // Add/Update Obsidian Block Type
            //   Name:Persisted Dataset List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersistedDatasetList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Persisted Dataset List", "Displays a list of persisted datasets.", "Rock.Blocks.Cms.PersistedDatasetList", "CMS", "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E" );

            // Add/Update Obsidian Block Type
            //   Name:Registration Instance - Linkage List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.RegistrationInstanceLinkageList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration Instance - Linkage List", "Displays the linkages associated with an event registration instance.", "Rock.Blocks.Event.RegistrationInstanceLinkageList", "Event", "AAA65861-B711-4659-8E80-975C72A2AA52" );

            // Add/Update Obsidian Block Type
            //   Name:Benevolence Request List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BenevolenceRequestList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Benevolence Request List", "Block used to list Benevolence Requests.", "Rock.Blocks.Finance.BenevolenceRequestList", "Finance", "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Key List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.RestKeyList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Key List", "Lists all the REST API Keys", "Rock.Blocks.Security.RestKeyList", "Security", "40B6AF94-5FFC-4EE3-ADD9-C76818992274" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Show Transaction Type Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transaction Type Column", "ShowTransactionTypeColumn", "Show Transaction Type Column", @"Show the Transaction Type column.", 5, @"False", "F9F3F330-F21A-4D19-90B4-B925BF121878" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List Lava
            //   Category: Finance
            //   Attribute: Transaction Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Types", "TransactionTypes", "Transaction Types", @"This setting filters the list of transactions by transaction type.", 11, @"2D607262-52D6-4724-910D-5C6E8FB89ACC", "5F8550CE-6767-4199-9A07-C1F1C843161C" );

            // Attribute for BlockType
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E07607C6-5428-4CCF-A826-060F48CACD32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "09CD883D-8D1D-45DA-BCAF-FD4093A41CF3" );

            // Attribute for BlockType
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E07607C6-5428-4CCF-A826-060F48CACD32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "13D85EBF-ED1A-48A6-A9B3-0C001D7E5DC3" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the persisted dataset details.", 0, @"", "616EAA22-3C6B-46E6-81D7-8FDB471CBCA9" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Max Preview Size (MB)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Max Preview Size (MB)", "MaxPreviewSizeMB", "Max Preview Size (MB)", @"If the JSON data is large, it could cause the browser to timeout.", 2, @"1", "18BAE77E-2A11-44F8-B275-81C001784332" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "23E8236F-7BDD-4F08-ACF1-4BC534B907B5" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "42A40F23-C51E-4311-88F3-3C7E524E6713" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "LinkagePage", "Detail Page", @"The page that will show the event item occurrence group map details.", 1, @"DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "D9DEBA8E-6580-4132-BA03-AE110AE7AF38" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The page for viewing details about a group", 2, @"4E237286-B715-4109-A578-C1445EC02707", "7BE07639-B6F5-4366-AF05-4984AB4CD484" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Calendar Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemDetailPage", "Calendar Item Page", @"The page to view calendar item details", 3, @"7FB33834-F40A-4221-8849-BB8C06903B04", "2F5A73E8-D6C3-40D0-A4EC-38B5A958D113" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Content Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemDetailPage", "Content Item Page", @"The page for viewing details about a content channel item", 4, @"D18E837C-9E65-4A38-8647-DFF04A595D97", "42EAEC8E-67A7-44C3-B1FD-5876F0E228FF" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "46002A2F-67C4-41B9-9C44-11953370AF6B" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAA65861-B711-4659-8E80-975C72A2AA52", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6A06AD74-91C8-4526-9556-FAE856140067" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "BenevolenceRequestDetail", "Detail Page", @"The page that will show the benevolence request details.", 0, @"", "676009F4-E109-4B46-AF26-907D903254A5" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Case Worker Role
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Case Worker Role", "CaseWorkerRole", "Case Worker Role", @"The security role to draw case workers from", 0, @"02FA0881-3552-42B8-A519-D021139B800F", "FF6578B9-EB29-4A8D-AB86-30C250D70D2A" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create benevolence type.", 2, @"C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", "A0743A61-F13A-400D-A1C7-933BC966DAC2" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsAttributeKey", "Hide Columns on Grid", @"The grid columns that should be hidden.", 3, @"", "712381CF-08E8-4841-94F6-4E66815C3F10" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Include Benevolence Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Include Benevolence Types", "FilterBenevolenceTypesAttributeKey", "Include Benevolence Types", @"The benevolence types to display in the list.<br/><i>If none are selected, all types will be included.<i>", 4, @"", "DEA0086A-8B29-4A0E-9B67-72A40ACA0CD4" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F29430D2-547B-4854-B030-E9701C04A1FC" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B2D4CCF1-D81C-448F-A8D5-5F8CF70EDB8A" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the person details.", 0, @"", "66785D25-9671-40BE-AC37-B9440376C117" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "865EF130-C08A-452C-8DDB-590474C1CEF9" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "40B6AF94-5FFC-4EE3-ADD9-C76818992274", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0CE31EED-E496-4E4C-8953-FD3E81E9F7BB" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "0CE31EED-E496-4E4C-8953-FD3E81E9F7BB" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "865EF130-C08A-452C-8DDB-590474C1CEF9" );

            // Attribute for BlockType
            //   BlockType: Rest Key List
            //   Category: Security
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "66785D25-9671-40BE-AC37-B9440376C117" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B2D4CCF1-D81C-448F-A8D5-5F8CF70EDB8A" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F29430D2-547B-4854-B030-E9701C04A1FC" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Include Benevolence Types
            RockMigrationHelper.DeleteAttribute( "DEA0086A-8B29-4A0E-9B67-72A40ACA0CD4" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute( "712381CF-08E8-4841-94F6-4E66815C3F10" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Configuration Page
            RockMigrationHelper.DeleteAttribute( "A0743A61-F13A-400D-A1C7-933BC966DAC2" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Case Worker Role
            RockMigrationHelper.DeleteAttribute( "FF6578B9-EB29-4A8D-AB86-30C250D70D2A" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "676009F4-E109-4B46-AF26-907D903254A5" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6A06AD74-91C8-4526-9556-FAE856140067" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "46002A2F-67C4-41B9-9C44-11953370AF6B" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Content Item Page
            RockMigrationHelper.DeleteAttribute( "42EAEC8E-67A7-44C3-B1FD-5876F0E228FF" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Calendar Item Page
            RockMigrationHelper.DeleteAttribute( "2F5A73E8-D6C3-40D0-A4EC-38B5A958D113" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "7BE07639-B6F5-4366-AF05-4984AB4CD484" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "D9DEBA8E-6580-4132-BA03-AE110AE7AF38" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "42A40F23-C51E-4311-88F3-3C7E524E6713" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "23E8236F-7BDD-4F08-ACF1-4BC534B907B5" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Max Preview Size (MB)
            RockMigrationHelper.DeleteAttribute( "18BAE77E-2A11-44F8-B275-81C001784332" );

            // Attribute for BlockType
            //   BlockType: Persisted Dataset List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "616EAA22-3C6B-46E6-81D7-8FDB471CBCA9" );

            // Attribute for BlockType
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "13D85EBF-ED1A-48A6-A9B3-0C001D7E5DC3" );

            // Attribute for BlockType
            //   BlockType: Attendance List
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "09CD883D-8D1D-45DA-BCAF-FD4093A41CF3" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List Lava
            //   Category: Finance
            //   Attribute: Transaction Types
            RockMigrationHelper.DeleteAttribute( "5F8550CE-6767-4199-9A07-C1F1C843161C" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction List
            //   Category: Finance
            //   Attribute: Show Transaction Type Column
            RockMigrationHelper.DeleteAttribute( "F9F3F330-F21A-4D19-90B4-B925BF121878" );

            // Delete BlockType 
            //   Name: Rest Key List
            //   Category: Security
            //   Path: -
            //   EntityType: Rest Key List
            RockMigrationHelper.DeleteBlockType( "40B6AF94-5FFC-4EE3-ADD9-C76818992274" );

            // Delete BlockType 
            //   Name: Benevolence Request List
            //   Category: Finance
            //   Path: -
            //   EntityType: Benevolence Request List
            RockMigrationHelper.DeleteBlockType( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1" );

            // Delete BlockType 
            //   Name: Registration Instance - Linkage List
            //   Category: Event
            //   Path: -
            //   EntityType: Registration Instance Linkage List
            RockMigrationHelper.DeleteBlockType( "AAA65861-B711-4659-8E80-975C72A2AA52" );

            // Delete BlockType 
            //   Name: Persisted Dataset List
            //   Category: CMS
            //   Path: -
            //   EntityType: Persisted Dataset List
            RockMigrationHelper.DeleteBlockType( "CFBB4DAF-1AEB-4095-8098-E3A82E30FA7E" );

            // Delete BlockType 
            //   Name: Attendance List
            //   Category: Check-in
            //   Path: -
            //   EntityType: Attendance List
            RockMigrationHelper.DeleteBlockType( "E07607C6-5428-4CCF-A826-060F48CACD32" );

        }
    }
}
