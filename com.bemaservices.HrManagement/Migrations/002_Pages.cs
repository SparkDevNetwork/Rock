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

            // Page: PTO Tiers
            RockMigrationHelper.AddPage( "3EDD82D0-1F64-4784-875B-688B7D3BD9AD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Tiers", "", "92E597F5-5A4E-49B2-B3F1-8390CEFF705F", "fa fa-gear" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Teir List", "Block to display the Pto Teirs.", "~/Plugins/com_bemaservices/HrManagement/PtoTierList.ascx", "BEMA Services > HR Management", "2BC393D8-123A-42E3-8EB5-766EDEDEFC2A" );
            // Add Block to Page: PTO Tiers, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "92E597F5-5A4E-49B2-B3F1-8390CEFF705F", "", "2BC393D8-123A-42E3-8EB5-766EDEDEFC2A", "Pto Teir List", "Main", "", "", 0, "BAFB601A-D59D-412A-8373-91976949AEC8" );
            // Attrib for BlockType: Pto Teir List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2BC393D8-123A-42E3-8EB5-766EDEDEFC2A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of a Pto Tier.", 0, @"", "0DFF581E-451A-4191-9BDC-5C5EE1A97ED2" );
            // Attrib Value for Block:Pto Teir List, Attribute:Detail Page Page: PTO Tiers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFB601A-D59D-412A-8373-91976949AEC8", "0DFF581E-451A-4191-9BDC-5C5EE1A97ED2", @"afde7e5d-9d6d-4d0a-87df-f2128ca51681" );

            // Page: PTO Tier Detail
            RockMigrationHelper.AddPage( "92E597F5-5A4E-49B2-B3F1-8390CEFF705F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Tier Detail", "", "AFDE7E5D-9D6D-4D0A-87DF-F2128CA51681", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Bracket List", "Lists all the brackets for a given PTO tier.", "~/Plugins/com_bemaservices/HrManagement/PtoBracketList.ascx", "BEMA Services > HR Management", "786AE09B-D8E7-4EC2-8488-1B104CB2C0A0" );
            RockMigrationHelper.UpdateBlockType( "Pto Teir Detail", "Displays the details of the given Pto Tier for editing.", "~/Plugins/com_bemaservices/HrManagement/PtoTierDetail.ascx", "BEMA Services > HR Management", "4736490B-BF41-42AB-AA4B-88DEFCFB6CAE" );
            // Add Block to Page: PTO Tier Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AFDE7E5D-9D6D-4D0A-87DF-F2128CA51681", "", "4736490B-BF41-42AB-AA4B-88DEFCFB6CAE", "Pto Teir Detail", "Main", "", "", 0, "F5DF34F5-D7E7-46C9-A246-7BB67F2AE155" );
            // Add Block to Page: PTO Tier Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AFDE7E5D-9D6D-4D0A-87DF-F2128CA51681", "", "786AE09B-D8E7-4EC2-8488-1B104CB2C0A0", "Pto Bracket List", "Main", "", "", 1, "FD8D9E8E-2F57-4831-9350-B1E7D6DD041C" );
            // Attrib for BlockType: Pto Bracket List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "786AE09B-D8E7-4EC2-8488-1B104CB2C0A0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C0A4D43B-4FC9-45C8-943F-F4639B380D4F" );
            // Attrib for BlockType: Pto Bracket List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "786AE09B-D8E7-4EC2-8488-1B104CB2C0A0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "841C5D6A-D410-481A-864D-5399E13E817C" );
            // Attrib for BlockType: Pto Bracket List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "786AE09B-D8E7-4EC2-8488-1B104CB2C0A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "8B5C48DD-6E2E-498C-807E-9E98EFD24DD2" );
            // Attrib Value for Block:Pto Bracket List, Attribute:Detail Page Page: PTO Tier Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD8D9E8E-2F57-4831-9350-B1E7D6DD041C", "C0A4D43B-4FC9-45C8-943F-F4639B380D4F", @"82b4ab59-dffa-4a2f-9035-c0cf18073552" );
            // Attrib Value for Block:Pto Bracket List, Attribute:core.CustomGridEnableStickyHeaders Page: PTO Tier Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD8D9E8E-2F57-4831-9350-B1E7D6DD041C", "8B5C48DD-6E2E-498C-807E-9E98EFD24DD2", @"False" );

            // Page: PTO Bracket Detail
            RockMigrationHelper.AddPage( "AFDE7E5D-9D6D-4D0A-87DF-F2128CA51681", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Bracket Detail", "", "82B4AB59-DFFA-4A2F-9035-C0CF18073552", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Bracket Detail", "Displays the details of the given Pto Bracket for editing.", "~/Plugins/com_bemaservices/HrManagement/PtoBracketDetail.ascx", "BEMA Services > HR Management", "0801C3F9-019E-4B8F-B5D5-4D38D48D11B2" );
            // Add Block to Page: PTO Bracket Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "82B4AB59-DFFA-4A2F-9035-C0CF18073552", "", "0801C3F9-019E-4B8F-B5D5-4D38D48D11B2", "Pto Bracket Detail", "Main", "", "", 0, "D7B2C0D9-54DF-44F5-8A0F-6CD0F1F7F723" );

            // Page: PTO Allocations
            RockMigrationHelper.AddPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Allocations", "", "E9E3118F-A7FC-4EC7-99C1-530727AF3275", "fa fa-clock" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Allocation List", "Lists all the pto allocations.", "~/Plugins/com_bemaservices/HrManagement/PtoAllocationList.ascx", "BEMA Services > HR Management", "9A7FDFF1-21DE-4BF5-9598-690224E98FFC" );
            // Add Block to Page: PTO Allocations, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E9E3118F-A7FC-4EC7-99C1-530727AF3275", "", "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "Pto Allocation List", "Main", "", "", 0, "489C67D2-C537-495C-88C8-EC16E7D5ED87" );
            // Attrib for BlockType: Pto Allocation List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "784D9FD6-96ED-4E74-8A72-A9BDF30471E1" );
            // Attrib for BlockType: Pto Allocation List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "79A67E87-6415-4D9F-8DF7-9B079791EAE8" );
            // Attrib for BlockType: Pto Allocation List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "7E3FCA44-7961-45C1-886F-666F3910F56F" );
            // Attrib Value for Block:Pto Allocation List, Attribute:Detail Page Page: PTO Allocations, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "489C67D2-C537-495C-88C8-EC16E7D5ED87", "784D9FD6-96ED-4E74-8A72-A9BDF30471E1", @"a6ae71ea-8aba-40e6-b033-207aa98f88f2" );
            // Attrib Value for Block:Pto Allocation List, Attribute:core.CustomGridEnableStickyHeaders Page: PTO Allocations, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "489C67D2-C537-495C-88C8-EC16E7D5ED87", "7E3FCA44-7961-45C1-886F-666F3910F56F", @"False" );

            // Page: PTO Allocation Detail
            RockMigrationHelper.AddPage( "E9E3118F-A7FC-4EC7-99C1-530727AF3275", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Allocation Detail", "", "A6AE71EA-8ABA-40E6-B033-207AA98F88F2", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Allocation Detail", "Displays the details of the given Pto Allocation for editing.", "~/Plugins/com_bemaservices/HrManagement/PtoAllocationDetail.ascx", "BEMA Services > HR Management", "D59A0A08-0A20-406E-95CE-03882446F70C" );
            // Add Block to Page: PTO Allocation Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A6AE71EA-8ABA-40E6-B033-207AA98F88F2", "", "D59A0A08-0A20-406E-95CE-03882446F70C", "Pto Allocation Detail", "Main", "", "", 0, "597C5907-D760-4C6C-8E0D-98F67A6DEB72" );

            // Page: Human Resources
            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Human Resources", "", "34A9F1E4-8249-4F39-B861-F28210E8C70A", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "34A9F1E4-8249-4F39-B861-F28210E8C70A", "Person/{PersonId}/HR" );
            // Add/Update PageContext for Page:Human Resources, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "34A9F1E4-8249-4F39-B861-F28210E8C70A", "Rock.Model.Person", "PersonId", "ED667F0F-B9F1-4D14-8076-69F7F1C72DE2" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
