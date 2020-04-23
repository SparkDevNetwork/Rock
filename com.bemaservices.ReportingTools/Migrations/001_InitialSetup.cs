// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Plugin;

namespace com.bemaservices.ReportingTools
{
    [MigrationNumber( 1, "1.9.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Reporting Tools
            RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reporting Tools", "", "5051557B-7EDD-4CA2-A18C-28881BA1C860", "fa fa-clipboard" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Menu", "Renders a page menu based on a root page and liquid template.", "~/Blocks/Cms/PageMenu.ascx", "CMS", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            // Add Block to Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5051557B-7EDD-4CA2-A18C-28881BA1C860", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "7A6FF48A-DC42-486E-B192-64CE4936A41B" );
            // Attrib for BlockType: Page Menu:CSS File
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS File", "CSSFile", "", "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the style sheet in the current theme's styles folder.", 0, @"", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22" );
            // Attrib for BlockType: Page Menu:Include Current Parameters
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current Parameters", "IncludeCurrentParameters", "", "Flag indicating if current page's route parameters should be used when building URL for child pages", 0, @"False", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            // Attrib for BlockType: Page Menu:Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", "The lava template to use for rendering. This template would typically be in the theme's \"Assets / Lava\" folder.", 0, @"{% include '~~/Assets/Lava/PageNav.lava' %}", "1322186A-862A-4CF1-B349-28ECB67229BA" );
            // Attrib for BlockType: Page Menu:Root Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Root Page", "RootPage", "", "The root page to use for the page collection. Defaults to the current page instance if not set.", 0, @"", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            // Attrib for BlockType: Page Menu:Number of Levels
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Number of Levels", "NumberofLevels", "", "Number of parent-child page levels to display. Default 3.", 0, @"3", "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            // Attrib for BlockType: Page Menu:Include Current QueryString
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current QueryString", "IncludeCurrentQueryString", "", "Flag indicating if current page's QueryString should be used when building URL for child pages", 0, @"False", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            // Attrib for BlockType: Page Menu:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 0, @"False", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            // Attrib for BlockType: Page Menu:Include Page List
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Include Page List", "IncludePageList", "", "List of pages to include in the Lava. Any ~/ will be resolved by Rock. Enable debug for assistance. Example 'Give Now' with '~/page/186' or 'Me' with '~/MyAccount'.", 0, @"", "0A49DABE-42EE-40E5-9E06-0E6530944865" );
            // Attrib for BlockType: Page Menu:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this content channel item block.", 1, @"", "EF10B2F9-93E5-426F-8D43-8C020224670F" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7A6FF48A-DC42-486E-B192-64CE4936A41B", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Template Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7A6FF48A-DC42-486E-B192-64CE4936A41B", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7A6FF48A-DC42-486E-B192-64CE4936A41B", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7A6FF48A-DC42-486E-B192-64CE4936A41B", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Reporting Tools, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7A6FF48A-DC42-486E-B192-64CE4936A41B", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Page: User Preferences
            RockMigrationHelper.AddPage( "5051557B-7EDD-4CA2-A18C-28881BA1C860", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "User Preferences", "", "6A520CB0-4E3E-473B-8ACA-0A717C29238D", "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "User Preference List", "Block for viewing user preferences.", "~/Plugins/com_bemaservices/ReportingTools/UserPreferenceList.ascx", "BEMA Services > Reporting Tools", "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A" );
            // Add Block to Page: User Preferences, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6A520CB0-4E3E-473B-8ACA-0A717C29238D", "", "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "User Preference List", "Main", "", "", 0, "2B74F55F-46BF-4FEF-AA69-90B4ED49E435" );
            // Attrib for BlockType: User Preference List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "90736631-A63C-4975-914E-EEE68D7120C7" );
            // Attrib for BlockType: User Preference List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "3AF8122C-DAC5-4167-9DCA-81B7312E1332" );
            // Attrib for BlockType: User Preference List:Entity
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "", "Entity Name", 0, @"", "E1528848-8A34-4618-9A2F-12285E1B1B62" );
            // Attrib for BlockType: User Preference List:Entity Qualifier Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "", "The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "1152D117-F818-4C43-B852-B68E4FD03B77" );
            // Attrib for BlockType: User Preference List:Entity Qualifier Value
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "", "The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "C6CFCAC1-780C-4B80-9049-F8401C43797A" );
            // Attrib for BlockType: User Preference List:Allow Setting of Values
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "", "Should UI be available for setting values of the specified Entity ID?", 3, @"False", "71679E74-9E43-47C6-9936-2BF6F80BAFFD" );
            // Attrib for BlockType: User Preference List:Entity Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "", "The entity id that values apply to", 4, @"0", "72E7C1DC-C83F-4FF0-A37D-980E1768E320" );
            // Attrib for BlockType: User Preference List:Enable Show In Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "", "Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"False", "312F517B-A25F-4FCB-A179-09E4AF30472E" );
            // Attrib for BlockType: User Preference List:Category Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "", "A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "A466819A-CB32-4EEC-B32E-9668F42025E3" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "72E7C1DC-C83F-4FF0-A37D-980E1768E320" );
            RockMigrationHelper.DeleteAttribute( "C6CFCAC1-780C-4B80-9049-F8401C43797A" );
            RockMigrationHelper.DeleteAttribute( "A466819A-CB32-4EEC-B32E-9668F42025E3" );
            RockMigrationHelper.DeleteAttribute( "E1528848-8A34-4618-9A2F-12285E1B1B62" );
            RockMigrationHelper.DeleteAttribute( "1152D117-F818-4C43-B852-B68E4FD03B77" );
            RockMigrationHelper.DeleteAttribute( "312F517B-A25F-4FCB-A179-09E4AF30472E" );
            RockMigrationHelper.DeleteAttribute( "71679E74-9E43-47C6-9936-2BF6F80BAFFD" );
            RockMigrationHelper.DeleteAttribute( "3AF8122C-DAC5-4167-9DCA-81B7312E1332" );
            RockMigrationHelper.DeleteAttribute( "90736631-A63C-4975-914E-EEE68D7120C7" );
            RockMigrationHelper.DeleteBlock( "2B74F55F-46BF-4FEF-AA69-90B4ED49E435" );
            RockMigrationHelper.DeleteBlockType( "6D4FE8F3-6784-4E3C-8328-E3EB73EE775A" );
            RockMigrationHelper.DeletePage( "6A520CB0-4E3E-473B-8ACA-0A717C29238D" ); //  Page: User Preferences

            RockMigrationHelper.DeleteAttribute( "EF10B2F9-93E5-426F-8D43-8C020224670F" );
            RockMigrationHelper.DeleteAttribute( "0A49DABE-42EE-40E5-9E06-0E6530944865" );
            RockMigrationHelper.DeleteAttribute( "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            RockMigrationHelper.DeleteAttribute( "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            RockMigrationHelper.DeleteAttribute( "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            RockMigrationHelper.DeleteAttribute( "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            RockMigrationHelper.DeleteAttribute( "1322186A-862A-4CF1-B349-28ECB67229BA" );
            RockMigrationHelper.DeleteAttribute( "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            RockMigrationHelper.DeleteAttribute( "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22" );
            RockMigrationHelper.DeleteBlock( "7A6FF48A-DC42-486E-B192-64CE4936A41B" );
            RockMigrationHelper.DeleteBlockType( "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            RockMigrationHelper.DeletePage( "5051557B-7EDD-4CA2-A18C-28881BA1C860" ); //  Page: Reporting Tools
        }
    }
}

