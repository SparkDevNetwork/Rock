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
            RockMigrationHelper.AddPage( "7F2581A1-941E-4D51-8A9D-5BE9B881B003", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "HR Management", "", "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "fa fa-briefcase" ); // Site:Rock RMS            RockMigrationHelper.UpdateBlockType( "Page Menu", "Renders a page menu based on a root page and liquid template.", "~/Blocks/Cms/PageMenu.ascx", "CMS", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
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

            RockMigrationHelper.AddSecurityAuthForPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", 0, "View", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, "4FCD2AF2-22C3-4C26-90E0-238C00E5DF86" );
            RockMigrationHelper.AddSecurityAuthForPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", 1, "View", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "B22BCDE6-13C8-48C0-8ACC-76B8BE624209" );
            RockMigrationHelper.AddSecurityAuthForPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", 2, "View", false, null, 1, "28F06CCB-ABEE-433D-87B6-B4CAAEC36F65" );

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
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Attribute Values", "Allows for editing the value(s) of a set of attributes for person.", "~/Blocks/Crm/PersonDetail/AttributeValues.ascx", "CRM > Person Detail", "D70A59DC-16BE-43BE-9880-59598FA7A94C" );
            RockMigrationHelper.UpdateBlockType( "Pto Allocation List", "Lists all the pto allocations.", "~/Plugins/com_bemaservices/HrManagement/PtoAllocationList.ascx", "BEMA Services > HR Management", "9A7FDFF1-21DE-4BF5-9598-690224E98FFC" );
            RockMigrationHelper.UpdateBlockType( "Pto Request List", "Lists all the pto requests.", "~/Plugins/com_bemaservices/HrManagement/PtoRequestList.ascx", "BEMA Services > HR Management", "7DBC964F-C656-4015-8E5B-E99C3A65AE26" );
            // Add Block to Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "34A9F1E4-8249-4F39-B861-F28210E8C70A", "", "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "Pto Allocation List", "SectionA1", "", "", 1, "61B8867D-07E4-4486-9858-B09C9ED90C1B" );
            // Add Block to Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "34A9F1E4-8249-4F39-B861-F28210E8C70A", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SectionA2", "", "", 0, "B4BA7334-890C-4569-B33B-555713E42E58" );
            // Add Block to Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "34A9F1E4-8249-4F39-B861-F28210E8C70A", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Attribute Values", "SectionA2", "", "", 1, "B9F7C989-3797-4905-9729-48A2B60AC083" );
            // Add Block to Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "34A9F1E4-8249-4F39-B861-F28210E8C70A", "", "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "Pto Request List", "SectionA1", "", "", 0, "060395F3-DA4C-481B-B53F-4B915C74697F" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: Pto Allocation List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "784D9FD6-96ED-4E74-8A72-A9BDF30471E1" );
            // Attrib for BlockType: Pto Allocation List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "79A67E87-6415-4D9F-8DF7-9B079791EAE8" );
            // Attrib for BlockType: Pto Allocation List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "7E3FCA44-7961-45C1-886F-666F3910F56F" );
            // Attrib for BlockType: Pto Allocation List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "9A7FDFF1-21DE-4BF5-9598-690224E98FFC", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "C8E25923-EF24-40DF-A9D7-7EFA201A6E21" );
            // Attrib for BlockType: Pto Request List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "65BCE14E-C8C9-45B1-A063-D2EBE0A753C5" );
            // Attrib for BlockType: Pto Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "06D0BBF8-EDD6-4783-9038-932A998FDCCD" );
            // Attrib for BlockType: Pto Request List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "FA8C97BF-DA0E-4EDE-8C7D-9D4FBF710D12" );
            // Attrib for BlockType: Pto Request List:PTO Request Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "PTO Request Workflow", "PTORequestWorkflow", "", "The Workflow used to add, modify, and delete pto requests.", 0, @"EBF1D986-8BBD-4888-8A7E-43AF5914751C", "0A159929-0871-4115-AB10-C4943EAA5A7C" );
            // Attrib for BlockType: Attribute Values:Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "", "The Attribute Categories to display attributes from", 0, @"", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81" );
            // Attrib for BlockType: Attribute Values:Attribute Order
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Order", "AttributeOrder", "", "The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.", 1, @"", "235C6D48-E1D1-410C-8006-1EA412BC12EF" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Attribute Values:Use Abbreviated Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Abbreviated Name", "UseAbbreviatedName", "", "Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.", 2, @"False", "51693680-B03C-468B-A771-CD8C103D0B1B" );
            // Attrib for BlockType: Attribute Values:Block Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "", "The text to display as the heading.", 3, @"", "AAF2428B-AFF7-412B-80F3-904FF04C37F8" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: Attribute Values:Block Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Icon", "BlockIcon", "", "The css class name to use for the heading icon.", 4, @"", "3F2AD439-09F8-47FE-A486-8949AAD855F3" );
            // Attrib for BlockType: Attribute Values:Show Category Names as Separators
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Names as Separators", "ShowCategoryNamesasSeparators", "", "If enabled, attributes will be grouped by category and will include the category name as a heading separator.", 5, @"False", "EF57237E-BA12-488A-9585-78466E4C3DB5" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", "Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", "Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", "If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", "Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", "Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Pto Allocation List, Attribute:Detail Page Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "61B8867D-07E4-4486-9858-B09C9ED90C1B", "784D9FD6-96ED-4E74-8A72-A9BDF30471E1", @"278de672-b691-4017-bd0a-893c50652edf" );
            // Attrib Value for Block:Pto Allocation List, Attribute:core.CustomGridEnableStickyHeaders Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "61B8867D-07E4-4486-9858-B09C9ED90C1B", "7E3FCA44-7961-45C1-886F-666F3910F56F", @"False" );
            // Attrib Value for Block:Pto Allocation List, Attribute:Entity Type Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "61B8867D-07E4-4486-9858-B09C9ED90C1B", "C8E25923-EF24-40DF-A9D7-7EFA201A6E21", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B4BA7334-890C-4569-B33B-555713E42E58", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Attribute Values, Attribute:Use Abbreviated Name Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B9F7C989-3797-4905-9729-48A2B60AC083", "51693680-B03C-468B-A771-CD8C103D0B1B", @"False" );
            // Attrib Value for Block:Attribute Values, Attribute:Show Category Names as Separators Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B9F7C989-3797-4905-9729-48A2B60AC083", "EF57237E-BA12-488A-9585-78466E4C3DB5", @"False" );
            // Attrib Value for Block:Attribute Values, Attribute:Category Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B9F7C989-3797-4905-9729-48A2B60AC083", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"4c8a3433-14e1-420b-9700-979c66c59edb" );
            // Attrib Value for Block:Pto Request List, Attribute:Entity Type Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "060395F3-DA4C-481B-B53F-4B915C74697F", "65BCE14E-C8C9-45B1-A063-D2EBE0A753C5", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:Pto Request List, Attribute:core.CustomGridEnableStickyHeaders Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "060395F3-DA4C-481B-B53F-4B915C74697F", "FA8C97BF-DA0E-4EDE-8C7D-9D4FBF710D12", @"False" );
            // Attrib Value for Block:Pto Request List, Attribute:PTO Request Workflow Page: Human Resources, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "060395F3-DA4C-481B-B53F-4B915C74697F", "0A159929-0871-4115-AB10-C4943EAA5A7C", @"ebf1d986-8bbd-4888-8a7e-43af5914751c" );

            RockMigrationHelper.UpdateHtmlContentBlock( "B4BA7334-890C-4569-B33B-555713E42E58", "{% include '~/Plugins/com_bemaservices/HrManagement/Assets/Lava/TimeOffSummary.lava' %}", "B9CD5B95-DEBA-4D8C-B9CB-CBA3F6BC9D77" );
            // Add/Update PageContext for Page:Human Resources, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "34A9F1E4-8249-4F39-B861-F28210E8C70A", "Rock.Model.Person", "PersonId", "ED667F0F-B9F1-4D14-8076-69F7F1C72DE2" );

            RockMigrationHelper.AddSecurityAuthForBlock( "B9F7C989-3797-4905-9729-48A2B60AC083", 0, "View", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, "B928B5EB-7CEF-4E6C-B628-01EC9EEF14A8" );
            RockMigrationHelper.AddSecurityAuthForBlock( "B9F7C989-3797-4905-9729-48A2B60AC083", 1, "View", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "4B8229F1-B6D9-4089-8D51-FEACF84D9C55" );
            RockMigrationHelper.AddSecurityAuthForBlock( "B9F7C989-3797-4905-9729-48A2B60AC083", 2, "View", false, null, Rock.Model.SpecialRole.AllUsers, "D84CB64F-517F-4CF6-80BD-CF522E149A6A" );

            RockMigrationHelper.AddSecurityAuthForBlock( "B4BA7334-890C-4569-B33B-555713E42E58", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "6D12252B-8621-47CF-9AF0-44376425F45E" );
            RockMigrationHelper.AddSecurityAuthForBlock( "61B8867D-07E4-4486-9858-B09C9ED90C1B", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "D8EE0902-1E92-42EB-8CF4-7F75E179F549" );
            RockMigrationHelper.AddSecurityAuthForBlock( "060395F3-DA4C-481B-B53F-4B915C74697F", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "B4E9D6A9-8AD9-460F-9E96-9DB87FC7010E" );

            // Page: Allocation Detail
            RockMigrationHelper.AddPage( "34A9F1E4-8249-4F39-B861-F28210E8C70A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Allocation Detail", "", "278DE672-B691-4017-BD0A-893C50652EDF", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Allocation Detail", "Displays the details of the given Pto Allocation for editing.", "~/Plugins/com_bemaservices/HrManagement/PtoAllocationDetail.ascx", "BEMA Services > HR Management", "D59A0A08-0A20-406E-95CE-03882446F70C" );
            // Add Block to Page: Allocation Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "278DE672-B691-4017-BD0A-893C50652EDF", "", "D59A0A08-0A20-406E-95CE-03882446F70C", "Pto Allocation Detail", "Main", "", "", 0, "EAD482BE-1E01-4889-A12F-0B64BD95BFF5" );

            RockMigrationHelper.AddSecurityAuthForBlock( "EAD482BE-1E01-4889-A12F-0B64BD95BFF5", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "C7432259-2F3E-4D97-84A3-DC318E3F23F4" );
            RockMigrationHelper.AddSecurityAuthForBlock( "597C5907-D760-4C6C-8E0D-98F67A6DEB72", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "A4561B3D-E14F-4E3F-B212-917122EC9E79" );
            RockMigrationHelper.AddSecurityAuthForBlock( "61B8867D-07E4-4486-9858-B09C9ED90C1B", 0, "Edit", true, "6F8AABA3-5BC8-468B-90DD-F0686F38E373", 0, "42697E5A-288C-4552-8DBA-F6E7694009C0" );

            // Page: PTO Requests
            RockMigrationHelper.AddPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Requests", "", "CB752D0E-BEBD-4001-92C0-63EA3B0B0B60", "fa fa-clock" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pto Request List", "Lists all the pto requests.", "~/Plugins/com_bemaservices/HrManagement/PtoRequestList.ascx", "BEMA Services > HR Management", "7DBC964F-C656-4015-8E5B-E99C3A65AE26" );
            // Add Block to Page: PTO Requests, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CB752D0E-BEBD-4001-92C0-63EA3B0B0B60", "", "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "Pto Request List", "Main", "", "", 0, "FD58CCE6-50F5-4EE8-8C79-2D304A1ACF91" );
            // Attrib for BlockType: Pto Request List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "65BCE14E-C8C9-45B1-A063-D2EBE0A753C5" );
            // Attrib for BlockType: Pto Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "06D0BBF8-EDD6-4783-9038-932A998FDCCD" );
            // Attrib for BlockType: Pto Request List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "FA8C97BF-DA0E-4EDE-8C7D-9D4FBF710D12" );
            // Attrib for BlockType: Pto Request List:PTO Request Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DBC964F-C656-4015-8E5B-E99C3A65AE26", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "PTO Request Workflow", "PTORequestWorkflow", "", "The Workflow used to add, modify, and delete pto requests.", 0, @"EBF1D986-8BBD-4888-8A7E-43AF5914751C", "0A159929-0871-4115-AB10-C4943EAA5A7C" );

            // Page: Employee List
            RockMigrationHelper.AddPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Employee List", "", "25D0F177-86CF-4C10-9A4D-695E22A357C4", "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HR Employee List", "Lists all the employees along with their PTO information.", "~/Plugins/com_bemaservices/HrManagement/HrEmployeeList.ascx", "BEMA Services > HR Management", "601C2443-C67D-4D4F-9CA2-D42AF186D5EB" );
            // Add Block to Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "25D0F177-86CF-4C10-9A4D-695E22A357C4", "", "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "HR Employee List", "Main", "", "", 0, "CA7CC1DE-1B27-4972-8D71-086278C6EEED" );
            // Attrib for BlockType: HR Employee List:Person Hired Date Attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Hired Date Attribute", "HireDate", "", "The Person Attribute that contains the Person's Hired Date.  This will be used to determine if the person is currently staff or not.", 0, @"", "D9436CCA-BD78-49F0-88F6-27262C0E6372" );
            // Attrib for BlockType: HR Employee List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "30E50E69-C0C3-4193-B096-15851854D9DC" );
            // Attrib for BlockType: HR Employee List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "52603F66-1CAE-4860-9F56-35061441B6EF" );
            // Attrib for BlockType: HR Employee List:Person Fired Date Attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Fired Date Attribute", "FireDate", "", "The Person Attribute that contains the Person's Fired Date.  This will be used to determine if the person is currently staff or not.", 1, @"", "CE9D4482-8130-45FF-BA04-5E95FA231C35" );
            // Attrib for BlockType: HR Employee List:Person Supervisor Attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Supervisor Attribute", "Supervisor", "", "The Person Attribute that contains the Person's Supervisor.", 2, @"", "B0EF7BA8-E480-4C66-9EDB-729721020CCA" );
            // Attrib for BlockType: HR Employee List:Person Ministry Area Attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Ministry Area Attribute", "MinistryArea", "", "The Person Attribute that contains the Person's Ministry Area.", 3, @"", "77BCA48F-B955-4FEE-9564-796AB28596B6" );
            // Attrib for BlockType: HR Employee List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "601C2443-C67D-4D4F-9CA2-D42AF186D5EB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 4, @"", "1096F3B7-4CF9-44B8-9E4F-CAA44CCC2BD1" );
            // Attrib Value for Block:HR Employee List, Attribute:Person Hired Date Attribute Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "D9436CCA-BD78-49F0-88F6-27262C0E6372", @"ccbf546c-41de-41af-9426-4a385d8d9663" );
            // Attrib Value for Block:HR Employee List, Attribute:core.CustomGridEnableStickyHeaders Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "52603F66-1CAE-4860-9F56-35061441B6EF", @"False" );
            // Attrib Value for Block:HR Employee List, Attribute:Person Fired Date Attribute Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "CE9D4482-8130-45FF-BA04-5E95FA231C35", @"10ce7b9c-3d65-4f11-ae7d-dd23c5e5189c" );
            // Attrib Value for Block:HR Employee List, Attribute:Person Supervisor Attribute Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "B0EF7BA8-E480-4C66-9EDB-729721020CCA", @"67afd5a3-28f3-404f-a3b8-88630061f294" );
            // Attrib Value for Block:HR Employee List, Attribute:Person Ministry Area Attribute Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "77BCA48F-B955-4FEE-9564-796AB28596B6", @"" );
            // Attrib Value for Block:HR Employee List, Attribute:Detail Page Page: Employee List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CA7CC1DE-1B27-4972-8D71-086278C6EEED", "1096F3B7-4CF9-44B8-9E4F-CAA44CCC2BD1", @"34a9f1e4-8249-4f39-b861-f28210e8c70a" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
