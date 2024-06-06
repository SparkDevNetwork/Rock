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
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240606 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddHistoryLogToPledgeDetailUp();
            ReplaceNcoaBlockAndJobUp();
            AddObsidianDynamicDataBlockTypeAndAttributesUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddHistoryLogToPledgeDetailDown();
            AddObsidianDynamicDataBlockTypeAndAttributesDown();
        }

        #region JPH: Add Obsidian Dynamic Data Block Attributes

        /// <summary>
        /// JPH: Adds the Obsidian Dynamic Data block attributes - up.
        /// </summary>
        private void AddObsidianDynamicDataBlockTypeAndAttributesUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.DynamicData
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.DynamicData", "Dynamic Data", "Rock.Blocks.Reporting.DynamicData, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "4AD30955-96AE-422E-AF0A-4D25357692A1" );

            // Add/Update Obsidian Block Type
            //   Name:Dynamic Data
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.DynamicData
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Dynamic Data", "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.", "Rock.Blocks.Reporting.DynamicData", "Reporting", "E050BDD0-4B59-4E86-AF68-18B361F76650" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Quick Return
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Quick Return", "EnableQuickReturn", "Enable Quick Return", @"When enabled, viewing the block will cause it to be added to the Quick Return list in the bookmarks feature.", 0, @"False", "E35D609D-384F-4A3E-ABFD-5C0BFD5A8892" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Update Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page", "UpdatePage", "Update Page", @"If True, provides fields for updating the parent page's Name and Description.", 1, @"True", "F47D1850-E5EF-4B74-8003-5699C85DDB88" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this dynamic data block.", 2, @"", "0F459900-C6CD-4700-8F8A-660312815F5A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: SQL Query
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "SQL Query", "Query", "SQL Query", @"The SQL query or stored procedure name to execute. If you are providing SQL you can add items from the query string using Lava like this: '{{ QueryParmName }}'. If SQL parameters are included they will also need to be in the Parameters field below.<br><span class='tip tip-lava'></span>", 0, @"", "5BBF3899-13CC-4168-934C-CEFE04C81F1A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Query Implemented as Stored Procedure
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Implemented as Stored Procedure", "StoredProcedure", "Query Implemented as Stored Procedure", @"When selecting this option, provide only the name of the stored procedure in the Query field. The parameters (if any) for the stored procedure should be configured using the Parameters field.", 0, @"False", "609D3BFA-72BA-41A0-ABBF-1B2B80B72C18" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: [Query|Stored Procedure] Parameters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "[Query|Stored Procedure] Parameters", "QueryParams", "[Query|Stored Procedure] Parameters", @"Specify the parameters required by the query or stored procedure using the format 'param1=value;param2=value'. Include an equals sign for each parameter. Omitting a value will set it to default to blank. Parameters matching query string values will automatically adopt those values. The 'CurrentPersonId' parameter will be replaced with the ID of the currently logged-in person. This field supports both standard SQL and stored procedures.", 0, @"", "B6EBBBE8-2EC7-4C18-BD82-82510445D5C9" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Timeout Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timeout Length", "Timeout", "Timeout Length", @"The amount of time in seconds to allow the query to run before timing out.", 0, @"30", "A84ED0C5-1AE9-411D-8A8B-AF8A64AD2781" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Results Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Results Display Mode", "ResultsDisplayMode", "Results Display Mode", @"Determines how the results should be displayed.", 0, @"Grid", "408B78D1-DF82-4FB8-95AB-9CD62F6E080A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Page Title Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Title Lava", "PageTitleLava", "Page Title Lava", @"Optional Lava for setting the page title. If nothing is provided then the page's title will be used. Example '{{rows[0].FullName}}' or if the query returns multiple result sets '{{table1.rows[0].FullName}}'.<br><span class='tip tip-lava'></span>", 0, @"", "964D9112-9186-49D7-9488-C9B205C6F473" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Column Configurations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Column Configurations", "ColumnConfigurations", "Column Configurations", @"A JSON object describing how each column of the grid results should be displayed, as well as rules for filtering, exporting, Etc.", 0, @"", "67E166BE-B220-49A5-BD68-6A5CF42E2F99" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Show Checkbox Selection Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Checkbox Selection Column", "ShowCheckboxSelectionColumn", "Show Checkbox Selection Column", @"Determines whether to show the checkbox select column on the grid as the first column.", 0, @"True", "12D5126A-E08D-46F4-82E4-CE4B7D246798" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Disable Paging
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Paging", "DisablePaging", "Disable Paging", @"Determines whether to disable paging on the grid.", 0, @"False", "09E7C74D-2B80-494D-9FC6-B9344E6C0150" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Person Report
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Report", "PersonReport", "Person Report", @"Does this query return a list of people? If it does, then additional options will be available from the result grid. (i.e. Communicate, etc). Note: A column named 'Id' that contains the person's Id is required for a person report.", 0, @"False", "81676FB6-A679-451C-A410-78865F4CC9DC" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Sticky Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Sticky Header", "EnableStickyHeaderOnGrid", "Enable Sticky Header", @"Determines whether the header on the grid will be sticky at the top of the page.", 0, @"False", "680FBBC7-1C58-4F3C-A874-ECC42212D275" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Export
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Export", "ShowExcelExport", "Enable Export", @"Determines whether to show 'Export to Excel' and 'Export to CSV' buttons in the grid.", 0, @"True", "27DF9797-C08D-4126-83D3-06BD505FF7C7" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Merge Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Merge Template", "ShowMergeTemplate", "Merge Template", @"Determines whether to show 'Merge Template' button in the grid.", 0, @"True", "C6C89193-F395-4D25-B380-FCDBA1D96B20" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communications
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Communications", "ShowCommunicate", "Communications", @"Determines whether to show 'Communicate' button in the grid.", 0, @"True", "C8C1D568-FE45-4CD9-A3D4-46B19E194D9D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Person Merge
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Merge", "ShowMergePerson", "Person Merge", @"Determines whether to show 'Merge Person Records' button in the grid.", 0, @"True", "9014598B-7FA8-4270-BCDF-112346CBC92D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Bulk Update
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Bulk Update", "ShowBulkUpdate", "Bulk Update", @"Determines whether to show 'Bulk Update' button in the grid.", 0, @"True", "BCC600CD-E690-45A0-A977-A8ADBD2CD613" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Launch Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Launch Workflow", "ShowLaunchWorkflow", "Launch Workflow", @"Determines whether to show 'Launch Workflow' button in the grid.", 0, @"True", "FEBB8F2A-B593-44AC-9B5D-46559687F0FF" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The title of the grid's panel.", 0, @"", "7E76F4F2-EEF8-4E46-BF92-591DED942D7D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Selection URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selection URL", "UrlMask", "Selection URL", @"The URL to redirect individual to when they click on a row in the grid. Any column's value can be used in the URL by including it in braces. For example if the grid includes an 'Id' column that contains Person IDs, you can link to the Person view by specifying a value here of '~/Person/{Id}'.", 0, @"", "1969013A-8611-4868-B999-4D83F2003C9B" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communication Merge Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Merge Fields", "MergeFields", "Communication Merge Fields", @"When creating a new communication from a person report, additional fields from the report can be used as merge fields on the communication. Enter any comma-separated column name(s) that you'd like to be available for the communication. If the same recipient has multiple results in this report, each result will be included in an 'AdditionalFields' list. These can be accessed using Lava in the communication. For example: '{% for field in AdditionalFields %}{{ field.columnName }}{% endfor %}'.", 0, @"", "007473C6-9683-4C83-86F0-85B1AAA18827" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communication Recipient Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Recipient Fields", "CommunicationRecipientPersonIdColumns", "Communication Recipient Fields", @"The comma-separated column name(s) that contain a person ID field to use as the recipient for a communication. If left blank, it will assume a column named 'Id' contains the recipient's person Id.", 0, @"", "D2F85E63-199F-49CD-8BF9-76111DC1B1DE" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Encrypted Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Encrypted Fields", "EncryptedFields", "Encrypted Fields", @"The comma-separated column name(s) that need to be decrypted before displaying their value.", 0, @"", "2F22A0D9-E1B0-4C0E-9449-EBB59B29F534" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Grid Header Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Grid Header Content", "GridHeaderContent", "Grid Header Content", @"This Lava template will be rendered above the grid. It will have access to the same dataset as the grid.<br><span class='tip tip-lava'></span>", 0, @"", "D3AA186D-9FDE-4908-9576-1D92CF8C2308" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Grid Footer Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Grid Footer Content", "GridFooterContent", "Grid Footer Content", @"This Lava template will be rendered below the grid (best used for custom totaling). It will have access to the same dataset as the grid.<br><span class='tip tip-lava'></span>", 0, @"", "BE383069-0CF9-43D7-B6B4-36D15D67F433" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E050BDD0-4B59-4E86-AF68-18B361F76650", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Lava Template", "FormattedOutput", "Lava Template", @"Formatting to apply to the returned results. Example: '{% for row in rows %}{{ row.FirstName }}{% endfor %}' or if the query returns multiple result sets: '{% for row in table1.rows %}{{ row.FirstName }}{% endfor %}'. Alternatively, you may iterate over all tables within the returned results. For example: '{% for table in tables %}{% for row in table.rows %}{{ row.FirstName }}{% endfor %}{% endfor %}'.<br><span class='tip tip-lava'></span>", 0, @"", "7852BE4F-0E96-42ED-BEA0-0D7EC7E19E21" );
        }

        /// <summary>
        /// JPH: Adds the Obsidian Dynamic Data block attributes - down.
        /// </summary>
        private void AddObsidianDynamicDataBlockTypeAndAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "7852BE4F-0E96-42ED-BEA0-0D7EC7E19E21" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Grid Footer Content
            RockMigrationHelper.DeleteAttribute( "BE383069-0CF9-43D7-B6B4-36D15D67F433" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Grid Header Content
            RockMigrationHelper.DeleteAttribute( "D3AA186D-9FDE-4908-9576-1D92CF8C2308" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Encrypted Fields
            RockMigrationHelper.DeleteAttribute( "2F22A0D9-E1B0-4C0E-9449-EBB59B29F534" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communication Recipient Fields
            RockMigrationHelper.DeleteAttribute( "D2F85E63-199F-49CD-8BF9-76111DC1B1DE" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communication Merge Fields
            RockMigrationHelper.DeleteAttribute( "007473C6-9683-4C83-86F0-85B1AAA18827" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Selection URL
            RockMigrationHelper.DeleteAttribute( "1969013A-8611-4868-B999-4D83F2003C9B" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Panel Title
            RockMigrationHelper.DeleteAttribute( "7E76F4F2-EEF8-4E46-BF92-591DED942D7D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Launch Workflow
            RockMigrationHelper.DeleteAttribute( "FEBB8F2A-B593-44AC-9B5D-46559687F0FF" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Bulk Update
            RockMigrationHelper.DeleteAttribute( "BCC600CD-E690-45A0-A977-A8ADBD2CD613" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Person Merge
            RockMigrationHelper.DeleteAttribute( "9014598B-7FA8-4270-BCDF-112346CBC92D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Communications
            RockMigrationHelper.DeleteAttribute( "C8C1D568-FE45-4CD9-A3D4-46B19E194D9D" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Merge Template
            RockMigrationHelper.DeleteAttribute( "C6C89193-F395-4D25-B380-FCDBA1D96B20" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Export
            RockMigrationHelper.DeleteAttribute( "27DF9797-C08D-4126-83D3-06BD505FF7C7" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Sticky Header
            RockMigrationHelper.DeleteAttribute( "680FBBC7-1C58-4F3C-A874-ECC42212D275" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Person Report
            RockMigrationHelper.DeleteAttribute( "81676FB6-A679-451C-A410-78865F4CC9DC" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Disable Paging
            RockMigrationHelper.DeleteAttribute( "09E7C74D-2B80-494D-9FC6-B9344E6C0150" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Show Checkbox Selection Column
            RockMigrationHelper.DeleteAttribute( "12D5126A-E08D-46F4-82E4-CE4B7D246798" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Column Configurations
            RockMigrationHelper.DeleteAttribute( "67E166BE-B220-49A5-BD68-6A5CF42E2F99" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Page Title Lava
            RockMigrationHelper.DeleteAttribute( "964D9112-9186-49D7-9488-C9B205C6F473" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Results Display Mode
            RockMigrationHelper.DeleteAttribute( "408B78D1-DF82-4FB8-95AB-9CD62F6E080A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Timeout Length
            RockMigrationHelper.DeleteAttribute( "A84ED0C5-1AE9-411D-8A8B-AF8A64AD2781" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: [Query|Stored Procedure] Parameters
            RockMigrationHelper.DeleteAttribute( "B6EBBBE8-2EC7-4C18-BD82-82510445D5C9" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Query Implemented as Stored Procedure
            RockMigrationHelper.DeleteAttribute( "609D3BFA-72BA-41A0-ABBF-1B2B80B72C18" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: SQL Query
            RockMigrationHelper.DeleteAttribute( "5BBF3899-13CC-4168-934C-CEFE04C81F1A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "0F459900-C6CD-4700-8F8A-660312815F5A" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Update Page
            RockMigrationHelper.DeleteAttribute( "F47D1850-E5EF-4B74-8003-5699C85DDB88" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Quick Return
            RockMigrationHelper.DeleteAttribute( "E35D609D-384F-4A3E-ABFD-5C0BFD5A8892" );

            // Delete BlockType
            //   Name: Dynamic Data
            //   Category: Reporting
            //   Path: -
            //   EntityType: Dynamic Data
            RockMigrationHelper.DeleteBlockType( "E050BDD0-4B59-4E86-AF68-18B361F76650" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.DynamicData
            RockMigrationHelper.DeleteEntityType( "4AD30955-96AE-422E-AF0A-4D25357692A1" );
        }

        #endregion

        #region KH: Remove Old SparkDataSettings Page and Block and ServiceJob and then Add New NCOA Process Page and Block

        private const string SparkDataBlockTypeGuid = "6B6A429D-E42C-70B5-4A04-98E886C45E7A";
        private const string SparkDataPageRouteGuid = "D34B7F39-5085-6178-1218-C50147ED28D3";
        private const string SparkDataPageGuid = "0591E498-0AD6-45A5-B8CA-9BCA5C771F03";
        private const string NcoaJobGuid = "D2D6EA6C-F94A-39A0-481B-A23D08B887D6";

        private void ReplaceNcoaBlockAndJobUp()
        {
            RemoveOldSparkDataSettings();
            AddNcoaProcess();
        }

        /// <summary>
        /// KH: Remove Old SparkDataSettings Page and Block and ServiceJob and then Add New NCOA Process Page and Block
        /// </summary>
        private void RemoveOldSparkDataSettings()
        {
            string blockQry = $@"
            DELETE b FROM [Block] b INNER JOIN [BlockType] bt on b.[BlockTypeId] = bt.[Id] WHERE bt.[Guid] = '{SparkDataBlockTypeGuid}'
            ";
            Sql( blockQry );

            RockMigrationHelper.DeleteBlockType( SparkDataBlockTypeGuid );

            RockMigrationHelper.DeletePageRoute( SparkDataPageRouteGuid );

            RockMigrationHelper.DeletePage( SparkDataPageGuid );

            string jobQry = $@"
            DELETE FROM [ServiceJob] WHERE [Guid] = '{NcoaJobGuid}'
            ";
            Sql( jobQry );
        }

        private void AddNcoaProcess()
        {
            // Add Page 
            //  Internal Name: NCOA Processing
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "NCOA Processing", "", "56EDE500-CEE6-41F4-B724-E44E66A4432F", "fa fa-people-carry" );

            // Add Page Route
            //   Page:NCOA Processing
            //   Route:ncoa-process
            RockMigrationHelper.AddOrUpdatePageRoute( "56EDE500-CEE6-41F4-B724-E44E66A4432F", "reporting/data-integrity/ncoa-results/ncoa-process", "20A7BA14-BC22-48B2-AF82-063F428B66E4" );

            //Hide NCOA Processing Page in Data Integrity
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '56EDE500-CEE6-41F4-B724-E44E66A4432F';" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.NcoaProcess
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.NcoaProcess", "NCOA Process", "Rock.Blocks.Communication.NcoaProcess, Rock.Blocks, Version=1.16.5.1, Culture=neutral, PublicKeyToken=null", false, false, "AFE1B685-B24C-41A2-BFDE-5F921EE75063" );

            // Add/Update Obsidian Block Type
            //   Name:NCOA Process
            //   Description:Displays the NCOA Process Steps.
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.NcoaProcess
            RockMigrationHelper.AddOrUpdateEntityBlockType( "NCOA Process", "Displays the NCOA Process Steps.", "Rock.Blocks.Communication.NcoaProcess", "Communication", "C3B61806-9F45-4CCF-8866-07D116E629A5" );

            // Add Block 
            //  Block Name: Rock.Blocks.Communication.Ncoa
            //  Page Name: NCOA Processing
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56EDE500-CEE6-41F4-B724-E44E66A4432F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C3B61806-9F45-4CCF-8866-07D116E629A5".AsGuid(), "NCOA Process", "Main", @"", @"", 0, "7475032F-4628-48FB-A035-B9362671F6B6" );

            // Update name of NCOA Results Page
            Sql( $@"UPDATE [Page]
SET InternalName = 'NCOA',
	PageTitle = 'NCOA',
	BrowserTitle = 'NCOA'
WHERE [Guid] = 'B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11'" );

            // Add Button to NCOA Results Block

            Sql( $@"UPDATE b SET b.PreHtml = CONCAT( b.PreHtml, '<div class=""clearfix margin-b-sm""><a href=""/reporting/data-integrity/ncoa-results/ncoa-process"" class=""btn btn-default btn-xs pull-right""><i class=""fa fa-upload""></i> Process NCOA</a></div>' )
FROM [Block] AS b INNER JOIN [BlockType] AS bt ON bt.[Id] = b.[BlockTypeId] WHERE bt.[Guid] = '3997FE75-E069-4879-B8BA-C8B19C367CD3' AND b.[PreHtml] NOT LIKE '%/reporting/data-integrity/ncoa-results/ncoa-process%'" );
        }

        #endregion

        #region KA: Migration to add History Log to Pledge Detail Page

        private const string PLEDGE_DETAIL_PAGE_GUID = "EF7AA296-CA69-49BC-A28B-901A8AAA9466";
        private const string HISTORY_LOG_BLOCK_TYPE = "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0";
        private const string HISTORY_LOG_BLOCK_TYPE_CONTEXT_ATTRIBUTE = "E9BB2534-D54E-4D50-A241-FCD45EFADE32";
        private const string HISTORY_LOG_BLOCK_TYPE_HEADING_ATTRIBUTE = "EAAE646D-69CD-41AC-9B8A-7EC5A446B379";
        private const string HISTORY_LOG_BLOCK = "2008404E-6E55-46FD-803C-7A52B626AA11";
        private const string PLEDGE_CONTEXT_GUID = "BDD0561E-01A4-4779-9D05-B6C89BBF955B";

        /// <summary>
        /// KA: Migration to add History Log to Pledge Detail Page
        /// </summary>
        private void AddHistoryLogToPledgeDetailUp()
        {
            // NOTE: Migration will be added as a RollUp migration during final merge

            // Add History Log block to the Pledge Detail page.
            RockMigrationHelper.AddBlock( true, PLEDGE_DETAIL_PAGE_GUID, null, HISTORY_LOG_BLOCK_TYPE, "History Log", "Main", "", "", 1, HISTORY_LOG_BLOCK );

            // Add Context attribute to HistoryLog block so the context entity can be set via migration.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( HISTORY_LOG_BLOCK_TYPE, "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", "The type of entity that will provide context for this block", 2, "", HISTORY_LOG_BLOCK_TYPE_CONTEXT_ATTRIBUTE );

            // Add Financial Pledge as Context entity to the newly added history log block.
            RockMigrationHelper.AddBlockAttributeValue( HISTORY_LOG_BLOCK, HISTORY_LOG_BLOCK_TYPE_CONTEXT_ATTRIBUTE, SystemGuid.EntityType.FINANCIAL_PLEDGE );

            // Add Context and idParameter to Pledge Detail page.
            RockMigrationHelper.UpdatePageContext( PLEDGE_DETAIL_PAGE_GUID, "Rock.Model.FinancialPledge", "PledgeId", PLEDGE_CONTEXT_GUID );

            // Update Heading attribute guid.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( HISTORY_LOG_BLOCK_TYPE, "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Heading", "Heading", "Heading", "The Lava template to use for the heading. <span class='tip tip-lava'></span>", 0, "{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", HISTORY_LOG_BLOCK_TYPE_HEADING_ATTRIBUTE );

            // Set History Log block heading.
            RockMigrationHelper.AddBlockAttributeValue( HISTORY_LOG_BLOCK, HISTORY_LOG_BLOCK_TYPE_HEADING_ATTRIBUTE, "Pledge History" );
        }

        /// <summary>
        /// KA: Migration to add History Log to Pledge Detail Page
        /// </summary>
        private void AddHistoryLogToPledgeDetailDown()
        {
            RockMigrationHelper.DeleteBlockAttribute( HISTORY_LOG_BLOCK_TYPE_CONTEXT_ATTRIBUTE );

            RockMigrationHelper.DeleteBlock( HISTORY_LOG_BLOCK );

            RockMigrationHelper.DeletePageContext( PLEDGE_CONTEXT_GUID );
        }

        #endregion
    }
}
