using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 2, "1.9.4" )]
    public class Pages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: HR Management
            RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "HR Management", "", "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "fa fa-briefcase" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Menu", "Renders a page menu based on a root page and liquid template.", "~/Blocks/Cms/PageMenu.ascx", "CMS", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            // Add Block to Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F" );
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
            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Template Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: HR Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Page: PTO Configuration
            RockMigrationHelper.AddPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Configuration", "", "3EDD82D0-1F64-4784-875B-688B7D3BD9AD", "fa fa-clock" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Menu", "Renders a page menu based on a root page and liquid template.", "~/Blocks/Cms/PageMenu.ascx", "CMS", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            // Add Block to Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3EDD82D0-1F64-4784-875B-688B7D3BD9AD", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "AE8F842D-B01A-40A7-B75D-CDF45908A49C" );
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
            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AE8F842D-B01A-40A7-B75D-CDF45908A49C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Template Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AE8F842D-B01A-40A7-B75D-CDF45908A49C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AE8F842D-B01A-40A7-B75D-CDF45908A49C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AE8F842D-B01A-40A7-B75D-CDF45908A49C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: PTO Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AE8F842D-B01A-40A7-B75D-CDF45908A49C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Page: PTO Types
            RockMigrationHelper.AddPage( "3EDD82D0-1F64-4784-875B-688B7D3BD9AD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Types", "", "17089A46-B6AF-44C9-8C3F-113F5EEE7716", "fa fa-gear" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "PTO Type List", "Block for viewing PTO Types.", "~/Plugins/com_bemaservices/HrManagement/PtoTypeList.ascx", "BEMA Services > HR Management", "39D2B37D-801B-408A-B726-DA962EB5F85F" );
            // Add Block to Page: PTO Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "17089A46-B6AF-44C9-8C3F-113F5EEE7716", "", "39D2B37D-801B-408A-B726-DA962EB5F85F", "PTO Type List", "Main", "", "", 0, "E1CC3C3B-5DA8-4169-84DB-9D764442DBAF" );
            // Attrib for BlockType: PTO Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "39D2B37D-801B-408A-B726-DA962EB5F85F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "DCE1727E-33A4-49CE-A672-B498B884FA75" );
            // Attrib for BlockType: PTO Type List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "39D2B37D-801B-408A-B726-DA962EB5F85F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "0D4BC881-A3A0-429C-A6F1-5068FC806A76" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0D4BC881-A3A0-429C-A6F1-5068FC806A76" );
            RockMigrationHelper.DeleteAttribute( "DCE1727E-33A4-49CE-A672-B498B884FA75" );
            RockMigrationHelper.DeleteBlock( "E1CC3C3B-5DA8-4169-84DB-9D764442DBAF" );
            RockMigrationHelper.DeleteBlockType( "39D2B37D-801B-408A-B726-DA962EB5F85F" );
            RockMigrationHelper.DeletePage( "17089A46-B6AF-44C9-8C3F-113F5EEE7716" ); //  Page: PTO Types

            RockMigrationHelper.DeleteBlock( "AE8F842D-B01A-40A7-B75D-CDF45908A49C" );
            RockMigrationHelper.DeleteBlockType( "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            RockMigrationHelper.DeletePage( "3EDD82D0-1F64-4784-875B-688B7D3BD9AD" ); //  Page: PTO Configuration

            RockMigrationHelper.DeleteBlock( "A6DC28DF-5FC6-4D66-9AD1-78D24E6F370F" );
            RockMigrationHelper.DeleteBlockType( "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            RockMigrationHelper.DeletePage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F" ); //  Page: HR Management
        }
    }
}
