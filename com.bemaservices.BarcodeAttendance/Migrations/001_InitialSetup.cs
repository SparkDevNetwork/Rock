using Rock.Plugin;

namespace com.bemaservices.Support
{
    [MigrationNumber( 1, "1.7.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Barcode Attendance
            RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Barcode Attendance", "", "865D2EC5-7446-4677-8572-156D7FC74297", "fa fa-barcode" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Menu", "Renders a page menu based on a root page and liquid template.", "~/Blocks/Cms/PageMenu.ascx", "CMS", "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            // Add Block to Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "865D2EC5-7446-4677-8572-156D7FC74297", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "4049A54D-E679-4FC0-A8A1-F1764362A1C1" );
            // Attrib for BlockType: Page Menu:CSS File
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS File", "CSSFile", "", "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the stylesheet in the current theme's styles folder.", 0, @"", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22" );
            // Attrib for BlockType: Page Menu:Include Current Parameters
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current Parameters", "IncludeCurrentParameters", "", "Flag indicating if current page's parameters should be used when building url for child pages", 0, @"False", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            // Attrib for BlockType: Page Menu:Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", @"The lava template to use for rendering. This template would typically be in the theme's ""Assets / Lava"" folder.", 0, @"{% include '~~/Assets/Lava/PageNav.lava' %}", "1322186A-862A-4CF1-B349-28ECB67229BA" );
            // Attrib for BlockType: Page Menu:Root Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Root Page", "RootPage", "", "The root page to use for the page collection. Defaults to the current page instance if not set.", 0, @"", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            // Attrib for BlockType: Page Menu:Number of Levels
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Number of Levels", "NumberofLevels", "", "Number of parent-child page levels to display. Default 3.", 0, @"3", "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            // Attrib for BlockType: Page Menu:Include Current QueryString
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current QueryString", "IncludeCurrentQueryString", "", "Flag indicating if current page's QueryString should be used when building url for child pages", 0, @"False", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            // Attrib for BlockType: Page Menu:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 0, @"False", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            // Attrib for BlockType: Page Menu:Include Page List
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Include Page List", "IncludePageList", "", "List of pages to include in the Lava. Any ~/ will be resolved by Rock. Enable debug for assistance. Example 'Give Now' with '~/page/186' or 'Me' with '~/MyAccount'.", 0, @"", "0A49DABE-42EE-40E5-9E06-0E6530944865" );
            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Template Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"" );
            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            // Attrib Value for Block:Page Menu, Attribute:Include Page List Page: Barcode Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4049A54D-E679-4FC0-A8A1-F1764362A1C1", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );


            // Page: Barcode Rosters
            RockMigrationHelper.AddPage( "865D2EC5-7446-4677-8572-156D7FC74297", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Barcode Rosters", "", "379F1B06-F748-472A-98EF-840A716AA409", "fa fa-file-text" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Barcode Group Location/Schedule Filter", "Displays groups, using pre-defined filters, that allows groups to be aggregated and printed together using Lava to PDF.  This only selects groups that have group types that actually takes attendance.", "~/Plugins/com_bemaservices/BarcodeAttendance/CampusServiceTimeRoomFilter.ascx", "com_bemaservices > Barcode Attendance", "DA473FA6-75A9-4F46-A9F4-721F093862D7" );
            // Add Block to Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "379F1B06-F748-472A-98EF-840A716AA409", "", "DA473FA6-75A9-4F46-A9F4-721F093862D7", "Barcode Group Location/Schedule Filter", "Main", "", "", 0, "E2F1D644-8224-4848-A2F7-6A87BA0B751E" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Group Types Include
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Include", "GroupTypes", "", "Select any specific group types to show in this block. If multiple items selected, dropdown list will appear in block", 1, @"", "91432092-34F2-4F2D-8E0C-2DE4FF13A731" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Display the Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display the Grid", "DisplayGrid", "", "Check this box to show/hide the groups display grid", 3, @"True", "D2271712-5164-4927-BBC0-DE7E072D4366" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Page Redirect
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Page Redirect", "PageId", "", "If set, the filter button will redirect to the selected page.", 5, @"", "F6AF3629-A085-44FA-A0E9-DF9BD4D9B1B5" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Show Parent Group Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Parent Group Filter", "ShowParentGroupFilter", "", "If set, a Parent Group filter will show.", 6, @"True", "1F770886-5F25-4E17-AFE8-E4CCF3A315B1" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Show Schedule Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Schedule Filter", "ShowScheduleFilter", "", "If Set, a Group Location Schedule filter will show.", 7, @"True", "26174DC3-9E2D-4FEC-ACAE-8F3B38E99565" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Show Location Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Location Filter", "ShowLocationFilter", "", "If Set, a Group Location filter will show.", 8, @"True", "46BC5650-7952-4DFD-9A30-413C808E2A8C" );
            // Attrib for BlockType: Barcode Group Location/Schedule Filter:Show Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA473FA6-75A9-4F46-A9F4-721F093862D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "", "If Set, the Campus filter selector will show.", 9, @"True", "1B9F72DB-334B-41A5-9F42-3EA0DAFB534E" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Group Types Include Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "91432092-34F2-4F2D-8E0C-2DE4FF13A731", @"50fcfb30-f51a-49df-86f4-2b176ea1820b" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Display the Grid Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "D2271712-5164-4927-BBC0-DE7E072D4366", @"True" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Page Redirect Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "F6AF3629-A085-44FA-A0E9-DF9BD4D9B1B5", @"103de6e5-3b12-4980-a284-7e54de4abdf4" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Show Campus Filter Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "1B9F72DB-334B-41A5-9F42-3EA0DAFB534E", @"True" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Show Location Filter Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "46BC5650-7952-4DFD-9A30-413C808E2A8C", @"True" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Show Parent Group Filter Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "1F770886-5F25-4E17-AFE8-E4CCF3A315B1", @"True" );
            // Attrib Value for Block:Barcode Group Location/Schedule Filter, Attribute:Show Schedule Filter Page: Barcode Rosters, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E2F1D644-8224-4848-A2F7-6A87BA0B751E", "26174DC3-9E2D-4FEC-ACAE-8F3B38E99565", @"True" );


            // Page: Barcode Report Print
            RockMigrationHelper.AddPage( "379F1B06-F748-472A-98EF-840A716AA409", "2E169330-D7D7-4ECA-B417-72C64BE150F0", "Barcode Report Print", "", "103DE6E5-3B12-4980-A284-7E54DE4ABDF4", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "103DE6E5-3B12-4980-A284-7E54DE4ABDF4", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Barcode Report", "Main", "", @"<script>


// Run Loadscript on Document Read
$(document).ready(loadscript);

            function loadscript()
            {
                // Opens Print Dialog.
                window.print();
            }


</script> ",0,"37F96F39-E945-4995-9015-E64164724860");
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
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
            // Attrib Value for Block:Barcode Report, Attribute:Cache Duration Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Barcode Report, Attribute:Require Approval Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Barcode Report, Attribute:Enable Versioning Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Barcode Report, Attribute:Context Parameter Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            // Attrib Value for Block:Barcode Report, Attribute:Context Name Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );
            // Attrib Value for Block:Barcode Report, Attribute:Start in Code Editor mode Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Barcode Report, Attribute:Image Root Folder Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Barcode Report, Attribute:User Specific Folders Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Barcode Report, Attribute:Document Root Folder Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Barcode Report, Attribute:Entity Type Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );
            // Attrib Value for Block:Barcode Report, Attribute:Enabled Lava Commands Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"Cache,RockEntity,Sql" );
            // Attrib Value for Block:Barcode Report, Attribute:Is Secondary Block Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Barcode Report, Attribute:Cache Tags Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "37F96F39-E945-4995-9015-E64164724860", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );
            // Attrib Value for Block:Barcode Report, Attribute:Content Page: Barcode Report Print, Site: Rock RMS
            RockMigrationHelper.UpdateHtmlContentBlock ( "37F96F39-E945-4995-9015-E64164724860", "{% include '/Plugins/com_bemaservices/BarcodeAttendance/Lava/CheckInScan.lava' %}", "91FD41B1-F817-41AE-8507-D66B363BFAA9" );



            // Page: Barcode Scanner
            RockMigrationHelper.AddPage( "865D2EC5-7446-4677-8572-156D7FC74297", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Barcode Scanner", "", "F2B90EDD-ACFA-4211-8F36-6745A195E123", "fa fa-barcode" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Barcode Scan Attendance", "Scans or Type PersonAliasID to mark Attendance, given Campus, Groups", "~/Plugins/com_bemaservices/BarcodeAttendance/BarcodeAttendance.ascx", "com_bemaservices > Barcode Attendance", "F556683A-DCC1-45C2-AC04-7210D4652714" );
            // Add Block to Page: Barcode Scanner, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F2B90EDD-ACFA-4211-8F36-6745A195E123", "", "F556683A-DCC1-45C2-AC04-7210D4652714", "Barcode Scan Attendance", "Main", "", "", 0, "D4185017-8AE0-4393-A7A3-DFAD454148B1" );
            // Attrib for BlockType: Barcode Scan Attendance:Include Group Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "F556683A-DCC1-45C2-AC04-7210D4652714", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "", "The group types to display in the list.  If none are selected, all group types will be included.", 1, @"", "1378C8B9-F1B9-4FD9-B714-4B5520401EC2" );
            // Attrib for BlockType: Barcode Scan Attendance:Limit Groups By Group Schedule
            RockMigrationHelper.UpdateBlockTypeAttribute( "F556683A-DCC1-45C2-AC04-7210D4652714", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Groups By Group Schedule", "LimitGroupsByGroupSchedule", "", "Limit returned groups by those with a location/schedule that matches the schedule selector", 2, @"False", "108418E0-4994-432E-B5D4-A733BE2A0E14" );
            // Attrib Value for Block:Barcode Scan Attendance, Attribute:Limit Groups By Group Schedule Page: Barcode Scanner, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D4185017-8AE0-4393-A7A3-DFAD454148B1", "108418E0-4994-432E-B5D4-A733BE2A0E14", @"False" );
            // Attrib Value for Block:Barcode Scan Attendance, Attribute:Include Group Types Page: Barcode Scanner, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D4185017-8AE0-4393-A7A3-DFAD454148B1", "1378C8B9-F1B9-4FD9-B714-4B5520401EC2", @"50fcfb30-f51a-49df-86f4-2b176ea1820b" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0A49DABE-42EE-40E5-9E06-0E6530944865" );
            RockMigrationHelper.DeleteAttribute( "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            RockMigrationHelper.DeleteAttribute( "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            RockMigrationHelper.DeleteAttribute( "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            RockMigrationHelper.DeleteAttribute( "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            RockMigrationHelper.DeleteAttribute( "1322186A-862A-4CF1-B349-28ECB67229BA" );
            RockMigrationHelper.DeleteAttribute( "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            RockMigrationHelper.DeleteAttribute( "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22" );
            RockMigrationHelper.DeleteBlock( "4049A54D-E679-4FC0-A8A1-F1764362A1C1" );
            RockMigrationHelper.DeleteBlockType( "CACB9D1A-A820-4587-986A-D66A69EE9948" );
            RockMigrationHelper.DeletePage( "865D2EC5-7446-4677-8572-156D7FC74297" ); //  Page: Barcode Attendance


            RockMigrationHelper.DeleteAttribute( "26174DC3-9E2D-4FEC-ACAE-8F3B38E99565" );
            RockMigrationHelper.DeleteAttribute( "1F770886-5F25-4E17-AFE8-E4CCF3A315B1" );
            RockMigrationHelper.DeleteAttribute( "46BC5650-7952-4DFD-9A30-413C808E2A8C" );
            RockMigrationHelper.DeleteAttribute( "1B9F72DB-334B-41A5-9F42-3EA0DAFB534E" );
            RockMigrationHelper.DeleteAttribute( "F6AF3629-A085-44FA-A0E9-DF9BD4D9B1B5" );
            RockMigrationHelper.DeleteAttribute( "D2271712-5164-4927-BBC0-DE7E072D4366" );
            RockMigrationHelper.DeleteAttribute( "91432092-34F2-4F2D-8E0C-2DE4FF13A731" );
            RockMigrationHelper.DeleteBlock( "E2F1D644-8224-4848-A2F7-6A87BA0B751E" );
            RockMigrationHelper.DeleteBlockType( "DA473FA6-75A9-4F46-A9F4-721F093862D7" );
            RockMigrationHelper.DeletePage( "379F1B06-F748-472A-98EF-840A716AA409" ); //  Page: Barcode Rosters

            RockMigrationHelper.DeleteAttribute( "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            RockMigrationHelper.DeleteAttribute( "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            RockMigrationHelper.DeleteAttribute( "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            RockMigrationHelper.DeleteAttribute( "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            RockMigrationHelper.DeleteAttribute( "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            RockMigrationHelper.DeleteAttribute( "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            RockMigrationHelper.DeleteAttribute( "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            RockMigrationHelper.DeleteAttribute( "0673E015-F8DD-4A52-B380-C758011331B2" );
            RockMigrationHelper.DeleteAttribute( "466993F7-D838-447A-97E7-8BBDA6A57289" );
            RockMigrationHelper.DeleteAttribute( "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            RockMigrationHelper.DeleteAttribute( "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            RockMigrationHelper.DeleteAttribute( "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            RockMigrationHelper.DeleteAttribute( "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            RockMigrationHelper.DeleteBlock( "37F96F39-E945-4995-9015-E64164724860" );
            RockMigrationHelper.DeleteBlockType( "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.DeletePage( "103DE6E5-3B12-4980-A284-7E54DE4ABDF4" ); //  Page: Barcode Report Print

            RockMigrationHelper.DeleteAttribute( "1378C8B9-F1B9-4FD9-B714-4B5520401EC2" );
            RockMigrationHelper.DeleteAttribute( "108418E0-4994-432E-B5D4-A733BE2A0E14" );
            RockMigrationHelper.DeleteBlock( "D4185017-8AE0-4393-A7A3-DFAD454148B1" );
            RockMigrationHelper.DeleteBlockType( "F556683A-DCC1-45C2-AC04-7210D4652714" );
            RockMigrationHelper.DeletePage( "F2B90EDD-ACFA-4211-8F36-6745A195E123" ); //  Page: Barcode Scanner
        }
    }
}

