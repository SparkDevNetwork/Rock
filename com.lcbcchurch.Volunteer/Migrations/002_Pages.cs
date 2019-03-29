// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.Volunteer.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    class Pages : Migration
    {
        public override void Up()
        {
            ///////////////////////////// Connections Button ////////////////////////////////
            // Page: Connections
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: Connections, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Clearing Name Report", "Feature", "", "", 0, "141D32A4-41C8-4836-8EFD-D77EA9CBE4C0" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
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
            // HTML Contents
            RockMigrationHelper.UpdateHtmlContentBlock( "141D32A4-41C8-4836-8EFD-D77EA9CBE4C0", @"<a class=""btn btn-primary"" href=""/GenerateNCReport"" role=""button"">Name Clearing Report</a></br></br>", "13F51697-8F5E-43CB-A279-06BAA86C47B1" );

            ////////////////////// Name Clearing Report //////////////////////////

            // Page: Name Clearing Report
            RockMigrationHelper.AddPage( "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Name Clearing Report", "", "0CBFBB2E-4392-436F-990C-E63F1548E0EC", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "0CBFBB2E-4392-436F-990C-E63F1548E0EC", "GenerateNCReport" );
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Filter By Page Parameters", "Filter block that passes the filter values as query string parameters.", "~/Plugins/com_bemaservices/Cms/FilterByPageParameter.ascx", "BEMA Services > Cms", "EC464362-6645-4944-A21A-1AAAB023BA9B" );
            // Add Block to Page: Name Clearing Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CBFBB2E-4392-436F-990C-E63F1548E0EC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Report Link", "Main", "", "", 1, "C2E1EF75-8695-41F0-A50A-7BE6E545B666" );
            // Add Block to Page: Name Clearing Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CBFBB2E-4392-436F-990C-E63F1548E0EC", "", "EC464362-6645-4944-A21A-1AAAB023BA9B", "Filter By Page Parameters", "Main", "", "", 0, "5D630085-4F8F-4D9C-A033-4A0C5CE2DD9C" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", "Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Filter By Page Parameters:Heading
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", "The text to display as the heading.", 1, @"Filters", "7D2952F9-F497-49AB-8392-71396E808462" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", "The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Filter By Page Parameters:Heading Icon CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon CSS Class", "HeadingIconCSSClass", "", "The css class name to use for the heading icon. ", 2, @"fa fa-filter", "BC03021E-D749-41AF-AE39-AB7023AA87DB" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", "The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: Filter By Page Parameters:Filters Per Row
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filters Per Row", "FiltersPerRow", "", "The number of filters to have per row.  Maximum is 12.", 3, @"2", "AA23BA3A-3DB0-4496-90CB-DB53AE5A7A3B" );
            // Attrib for BlockType: Filter By Page Parameters:Show Reset Filters
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reset Filters", "ShowResetFilters", "", "Determines if the Reset Filters button should be displayed", 4, @"True", "692DF71B-DB81-4548-8D7A-D664CEFDFB31" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", "Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", "Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: Filter By Page Parameters:Filter Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filter Button Text", "FilterButtonText", "", "Sets the button text for the filter button.", 5, @"Filter", "8534E943-02F5-4D0B-A20C-B9C7A48B2CFA" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", "Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: Filter By Page Parameters:Page Redirect
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC464362-6645-4944-A21A-1AAAB023BA9B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Page Redirect", "PageRedirect", "", "If set, the filter button will redirect to the selected page.", 6, @"", "81B97BCA-58B5-4DD6-AB57-FF3A619CE052" );
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
            // HTML Contents
            RockMigrationHelper.UpdateHtmlContentBlock( "C2E1EF75-8695-41F0-A50A-7BE6E545B666", @"{% assign type = 'Global' | PageParameter:'ConnectionType' %}{% if type != empty %}<div class=""panel panel-block""><div class=""panel-body""><a class=""btn btn-primary"" href=""/NCReport?ConnectionType={{type}}"">Report PDF</a></div></div>{% endif %}", "8311932E-FF09-42C3-987D-730D0B818217" );
            // Add Page Parameter Filter
            //var blockId = SqlScalar( "Select Top 1 Id From [Block] Where Guid = 'EC464362-6645-4944-A21A-1AAAB023BA9B'" ).ToString();
            //RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Block", "50DA6F25-E81E-46E8-A773-4B479B4FB9E6", "Id", blockId, "Connection Type", "", 1015, "", "F600F667-1465-46C2-9A4E-39901BE4B7FE", "ConnectionType" );

            ///////////////////////////////////////// Clearing Name PDF////////////////////////////

            // Page: Clearing Name PDF
            RockMigrationHelper.AddPage( "0CBFBB2E-4392-436F-990C-E63F1548E0EC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Clearing Name PDF", "", "8558E07B-E881-48B2-AC39-FBE807672B2B", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "8558E07B-E881-48B2-AC39-FBE807672B2B", "NCReport" );
            RockMigrationHelper.UpdateBlockType( "Lava To PDF", "Block that renders a Lava template as PDF.", "~/Plugins/com_minecartstudio/Cms/LavaToPdf.ascx", "Mine Cart Studio > CMS", "5790A87A-D619-4D98-86F8-4C9E7E74349D" );
            // Add Block to Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8558E07B-E881-48B2-AC39-FBE807672B2B", "", "5790A87A-D619-4D98-86F8-4C9E7E74349D", "Lava To PDF", "Main", "", "", 0, "6FAA0366-B93A-4554-8425-C67830DCE4D1" );
            // Attrib for BlockType: Lava To PDF:Enable Header
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Header", "EnableHeader", "", "Displays a document header based on the template provided.", 0, @"False", "BEE6CBB0-2CBB-4C4C-BB7D-828238D69B28" );
            // Attrib for BlockType: Lava To PDF:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "0E28EC49-6F7F-4E8A-8F9E-553376C7D807" );
            // Attrib for BlockType: Lava To PDF:Document Width
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Document Width", "DocumentWidth", "", "The width of the document in inches.", 1, @"8.5", "E778DCBE-DE6A-446C-A188-AA8CE4CDD7A4" );
            // Attrib for BlockType: Lava To PDF:Header Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Header Height", "HeaderHeight", "", "The header height in inches.", 2, @"0.25", "2A5003DC-7179-4A10-B776-2C5E3A53DB8F" );
            // Attrib for BlockType: Lava To PDF:Document Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Document Height", "DocumentHeight", "", "The height of the document in inches.", 2, @"11", "04E62176-4107-4895-86B9-608B2CCF573D" );
            // Attrib for BlockType: Lava To PDF:Header Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "", "The Lava to use for the header (should be a full HTML document).", 2, @"", "BF5778D3-91B5-439F-8813-ABC88AACB053" );
            // Attrib for BlockType: Lava To PDF:Header Spacing
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Header Spacing", "HeaderSpacing", "", "The amount of space between the header and the document body in inches.", 3, @"0.25", "93507651-B019-492B-9AE6-638EC4578ACD" );
            // Attrib for BlockType: Lava To PDF:Page Top Margin
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Page Top Margin", "PageTopMargin", "", "The height of top margin of the page in inches.", 3, @"0.5", "219FCCAB-E315-4BC6-A6D4-BF2529980E47" );
            // Attrib for BlockType: Lava To PDF:Page Bottom Margin
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Page Bottom Margin", "PageBottomMargin", "", "The height of bottom margin of the page in inches.", 4, @"0.5", "69076356-1E59-42ED-BE93-CFCB68E27D16" );
            // Attrib for BlockType: Lava To PDF:Document Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Name", "DocumentName", "", "Lava template for the document name.", 5, @"document.pdf", "84189421-47DD-43F6-AC13-24602D7F951D" );
            // Attrib for BlockType: Lava To PDF:Enable Footer
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Footer", "EnableFooter", "", "Displays a document footer based on the template provided.", 5, @"False", "4FDD6E70-04D2-4949-9C5D-8B97B1A50CC4" );
            // Attrib for BlockType: Lava To PDF:Document Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Document Template", "DocumentTemplate", "", "The Lava to use for the document.", 6
                , @"{% assign type = 'Global' | PageParameter:'ConnectionType' %}
                <html>
                    <head>    
                        <style>
                            body {
                                padding-left: .25in;
                                padding-right: .1in;
                                width: 14in;
                            }
                                table {
                                font-family: arial, sans-serif;
                                border-collapse: collapse;
                                width: 100%;
                            }
            
                            td, th {
                                border: 2px solid #dddddd;
                                text-align: left;
                                padding: 8px;
                            }
            
                            tr:nth-child(even) {
                                background-color: #dddddd;
                            }
                        </style>
                        <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"" integrity=""sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u"" crossorigin=""anonymous"">
                    </ head >
                    < body >
                        {% sql %}
                            Select
                                P.Id
            	                , Cast( CR.CreatedDateTime As Date ) As[Date]
            	                , CO.Name As[Opportunity]
                                , C.Name As[Campus]
                                , Cast(C.Guid As nvarchar( MAX )) As[Guid]
                            From ConnectionOpportunity CO
                            Inner Join ConnectionRequest CR On CO.ID = CR.ConnectionOpportunityId
                            Inner Join ConnectionType CT On CO.ConnectionTypeId = CT.Id
                            Inner Join PersonAlias PA On CR.PersonAliasId = PA.Id
                            Inner Join Person P On PA.PersonId = P.Id
                            Left Outer Join Campus C On CR.CampusId = C.Id
                            Where CT.Guid = '{{type}}'
                            And CR.ConnectionState = 0
                        {% endsql %}
                        < div class=""row"" >
                            <img src = ""/Assets/Images/LCBC_Logo_Blue.png"" style=""float:left; width:20%;"">
                            <div style = ""text-align:center; padding-right:20%;"" >
                                < h1 > Name Clearing List</h1>
                                <h3>KidMinistry</h3>
                                </div>
                        </div>
                        {% assign group = 630454 | GroupById %}
                        {% for member in results %}
                        {% assign person = member.Id | PersonById %}
                        {% assign campus = person | Campus %}
                            <table>
                                <tr>
                                    <td width = ""20%"" >
                                        < img src=""{{person.Photo.Url}}"">
                                    </td>
                                    <td colspan = ""3"" >
                                        < strong >< font size=""6"">{{person.LastName}}, {{person.FirstName}}</font></strong></br>
                                        <font size = ""5"" >{% if person.MaritalStatusValue.Value != null %}
                                            {{person.MaritalStatusValue.Value}}, 
                                        {% endif %}
                                        {% if person.Age != null %}
                                            {{person.Age}}, 
                                        {% endif %}
                                        {{person.ConnectionStatusValue.Value}} > {{campus.ShortCode}}</font>
                                    </td>
                                </tr>
                                <tr style = ""font-size:20px;"" >
                                    < th > Date </ th >
                                    < th > Opportunity </ th >
                                    < th > Campus </ th >
                                    < th > Connector </ th >
                                </ tr >
                                < tr style=""font-size:20px;"">
                                    <td width = ""20%"" >
                                        {{member.Date | Date:'M/d/yyyy'}}
                                    </td>
                                    <td>
                                        {{member.Opportunity}}
                                    </td>
                                    <td width = ""15%"" >
                                        {{member.Campus}}
                                    </td>
                                    <td width = ""20%"" >
                                        {% for person in group.Members %} 
                                            {% for attribute in person.AttributeValues %}
                                            {% assign val = attribute.Value | Upcase %}
                                            {% assign guid = member.Guid | Upcase %}
                                                {% if val == guid %}
                                                    {{person.Person.FullName}}
                                                {% endif %}
                                            {% endfor %}
                                        {% endfor %}
                                    </td>
                                </tr>
                            </table></br></br>
                        {% endfor %}
                    </body>
                </html>"
                , "8B5385F1-3C9E-4ED9-B8E1-0FE370BC28A9" );
            // Attrib for BlockType: Lava To PDF:Footer Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Footer Template", "FooterTemplate", "", "The Lava to use for the footer (should be a full HTML document). Use &amp;p; for the current page and &amp;P; for the total number of pages.", 6, @"", "9A4EBDF8-C67B-4DF5-B9BD-C2D5F389F13F" );
            // Attrib for BlockType: Lava To PDF:Open Inline
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Open Inline", "OpenInline", "", "Determines if the document should be opened in the browser or downloaded as a file.", 7, @"True", "EBD3FE22-39C9-421B-B300-F6CC3636D78D" );
            // Attrib for BlockType: Lava To PDF:Footer Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Footer Height", "FooterHeight", "", "The header height in inches.", 7, @"0.25", "5A2E2853-48DA-46AE-84B5-E03E8B388B2F" );
            // Attrib for BlockType: Lava To PDF:Document Render Delay
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Document Render Delay", "DocumentRenderDelay", "", "The amount of time in seconds to wait before generating the PDF. This allows time for scripts and external resources to load.", 8, @"0", "7A4ACB9A-487C-4D7C-93BA-58DF27A8C2FF" );
            // Attrib for BlockType: Lava To PDF:Footer Spacing
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Footer Spacing", "FooterSpacing", "", "The amount of space between the header and the document body in inches.", 8, @"0.25", "64458847-D7B1-4FF5-B60E-57BEB7CA7CCA" );
            // Attrib for BlockType: Lava To PDF:Auto Create Bookmarks
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Create Bookmarks", "AutoCreateBookmarks", "", "Determines if PDF bookmarks should be generated from h1..h6.", 9, @"False", "8E2C5411-2155-4229-AE9B-4A046809BF2C" );
            // Attrib for BlockType: Lava To PDF:Enable Debug
            RockMigrationHelper.UpdateBlockTypeAttribute( "5790A87A-D619-4D98-86F8-4C9E7E74349D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show lava merge fields.", 15, @"False", "D1D6EC5E-3C01-4D80-A7AA-8A614C1063FE" );
            // Attrib Value for Block:Lava To PDF, Attribute:Enable Header Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "BEE6CBB0-2CBB-4C4C-BB7D-828238D69B28", @"False" );
            // Attrib Value for Block:Lava To PDF, Attribute:Enabled Lava Commands Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "0E28EC49-6F7F-4E8A-8F9E-553376C7D807", @"Sql" );
            // Attrib Value for Block:Lava To PDF, Attribute:Document Width Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "E778DCBE-DE6A-446C-A188-AA8CE4CDD7A4", @"8.5" );
            // Attrib Value for Block:Lava To PDF, Attribute:Document Height Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "04E62176-4107-4895-86B9-608B2CCF573D", @"11" );
            // Attrib Value for Block:Lava To PDF, Attribute:Header Template Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "BF5778D3-91B5-439F-8813-ABC88AACB053", @"" );
            // Attrib Value for Block:Lava To PDF, Attribute:Header Height Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "2A5003DC-7179-4A10-B776-2C5E3A53DB8F", @"0.25" );
            // Attrib Value for Block:Lava To PDF, Attribute:Page Top Margin Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "219FCCAB-E315-4BC6-A6D4-BF2529980E47", @"0.5" );
            // Attrib Value for Block:Lava To PDF, Attribute:Header Spacing Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "93507651-B019-492B-9AE6-638EC4578ACD", @"0.25" );
            // Attrib Value for Block:Lava To PDF, Attribute:Page Bottom Margin Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "69076356-1E59-42ED-BE93-CFCB68E27D16", @"0.5" );
            // Attrib Value for Block:Lava To PDF, Attribute:Document Name Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "84189421-47DD-43F6-AC13-24602D7F951D", @"NameClearingList.pdf" );
            // Attrib Value for Block:Lava To PDF, Attribute:Enable Footer Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "4FDD6E70-04D2-4949-9C5D-8B97B1A50CC4", @"False" );
            // Attrib Value for Block:Lava To PDF, Attribute:Document Template Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "8B5385F1-3C9E-4ED9-B8E1-0FE370BC28A9", @"{% assign type = 'Global' | PageParameter:'ConnectionType' %} <html>     <head>             <style>             body {                 padding-left: .25in;                 padding-right: .1in;                 width: 14in;             }              table {               font-family: arial, sans-serif;               border-collapse: collapse;               width: 100%;             }                          td, th {               border: 2px solid #dddddd;               text-align: left;               padding: 8px;             }                          tr:nth-child(even) {               background-color: #dddddd;             }         </style>         <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"" integrity=""sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u"" crossorigin=""anonymous"">     </head>     <body>         {% sql %}             Select                  P.Id              , Cast(CR.CreatedDateTime As Date) As [Date]              , CO.Name As [Opportunity]              , C.Name As [Campus]              , Cast(C.Guid As nvarchar(MAX)) As [Guid]             From ConnectionOpportunity CO             Inner Join ConnectionRequest CR On CO.ID = CR.ConnectionOpportunityId             Inner Join ConnectionType CT On CO.ConnectionTypeId = CT.Id             Inner Join PersonAlias PA On CR.PersonAliasId = PA.Id             Inner Join Person P On PA.PersonId = P.Id             Left Outer Join Campus C On CR.CampusId = C.Id             Where CT.Guid ='{{type}}'             And CR.ConnectionState = 0         {% endsql %}         <div class=""row"" >             <img src=""https://localhost:44321/Assets/Images/LCBC_Logo_Blue.png"" style=""float:left; width:20%;"">             <div style=""text-align:center; padding-right:20%;"">                 <h1>Name Clearing List</h1>                 <h3>KidMinistry</h3>             </div>         </div>         {% assign group = 630454 | GroupById %}         {% for member in results %}         {% assign person = member.Id | PersonById %}         {% assign campus = person | Campus %}             <table>                 <tr>                     <td width=""20%"">                         <img src=""{{person.Photo.Url}}"">                     </td>                     <td colspan=""3"">                         <strong><font size=""6"">{{person.LastName}}, {{person.FirstName}}</font></strong></br>                         <font size=""5"">{% if person.MaritalStatusValue.Value != null %}                             {{person.MaritalStatusValue.Value}},                          {% endif %}                         {% if person.Age != null %}                             {{person.Age}},                          {% endif %}                         {{person.ConnectionStatusValue.Value}} > {{campus.ShortCode}}</font>                     </td>                 </tr>                 <tr style=""font-size:20px;"">                     <th>Date</th>                     <th>Opportunity</th>                     <th>Campus</th>                     <th>Connector</th>                 </tr>                 <tr style=""font-size:20px;"">                     <td width=""20%"">                         {{member.Date | Date:'M/d/yyyy'}}                     </td>                     <td>                         {{member.Opportunity}}                     </td>                     <td width=""15%"">                         {{member.Campus}}                     </td>                     <td width=""20%"">                         {% for person in group.Members %}                              {% for attribute in person.AttributeValues %}                             {% assign val = attribute.Value | Upcase %}                             {% assign guid = member.Guid | Upcase %}                                 {% if val == guid %}                                     {{person.Person.FullName}}                                 {% endif %}                             {% endfor %}                         {% endfor %}                     </td>                 </tr>             </table></br></br>         {% endfor %}     </body> </html>" );
            // Attrib Value for Block:Lava To PDF, Attribute:Footer Template Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "9A4EBDF8-C67B-4DF5-B9BD-C2D5F389F13F", @"" );
            // Attrib Value for Block:Lava To PDF, Attribute:Open Inline Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "EBD3FE22-39C9-421B-B300-F6CC3636D78D", @"True" );
            // Attrib Value for Block:Lava To PDF, Attribute:Footer Height Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "5A2E2853-48DA-46AE-84B5-E03E8B388B2F", @"0.25" );
            // Attrib Value for Block:Lava To PDF, Attribute:Document Render Delay Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "7A4ACB9A-487C-4D7C-93BA-58DF27A8C2FF", @"0" );
            // Attrib Value for Block:Lava To PDF, Attribute:Footer Spacing Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "64458847-D7B1-4FF5-B60E-57BEB7CA7CCA", @"0.25" );
            // Attrib Value for Block:Lava To PDF, Attribute:Auto Create Bookmarks Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "8E2C5411-2155-4229-AE9B-4A046809BF2C", @"False" );
            // Attrib Value for Block:Lava To PDF, Attribute:Enable Debug Page: Clearing Name PDF, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6FAA0366-B93A-4554-8425-C67830DCE4D1", "D1D6EC5E-3C01-4D80-A7AA-8A614C1063FE", @"False" );

            /////////////////////////////Standalone Pages////////////////////////////

            // Page: Standalone Pages
            RockMigrationHelper.AddPage( "85F25819-E948-4960-9DDF-00F54D32444E", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Standalone Pages", "", "4F0FAED3-E59D-4AA2-9B6F-782B5B32CAEE", "" ); // Site:External Website

            ////////////////////////////////////// Volunteer ////////////////////////////////////////

            // Page: Volunteer
            RockMigrationHelper.AddPage( "4F0FAED3-E59D-4AA2-9B6F-782B5B32CAEE", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Volunteer", "", "05B385CE-184F-43E1-97FA-22A0F9974FD7", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "05B385CE-184F-43E1-97FA-22A0F9974FD7", "Volunteer" );
            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
            // Add Block to Page: Volunteer, Site: External Website
            RockMigrationHelper.AddBlock( true, "05B385CE-184F-43E1-97FA-22A0F9974FD7", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "4FF7B41D-B19F-41EF-8E59-C8A5F9BB36AD" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Volunteer, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4FF7B41D-B19F-41EF-8E59-C8A5F9BB36AD", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"195b07af-b80b-4bbe-ad83-9b39804b6c55" );
            // Attrib Value for Block:Workflow Entry, Attribute:Show Summary View Page: Volunteer, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4FF7B41D-B19F-41EF-8E59-C8A5F9BB36AD", "1CFB44EE-4DF7-40DD-83DC-B7801909D259", @"False" );

            //////////////////////////////////////// Volunteer Application - Adult ////////////////////////////////

            // Page: Volunteer Application - Adult
            RockMigrationHelper.AddPage( "4F0FAED3-E59D-4AA2-9B6F-782B5B32CAEE", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Volunteer Application - Adult", "", "28DF23AD-5F02-4C01-B149-4F28D499BBA1", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "28DF23AD-5F02-4C01-B149-4F28D499BBA1", "VolunteerAdultApplication" );
            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
            // Add Block to Page: Volunteer Application - Adult, Site: External Website
            RockMigrationHelper.AddBlock( true, "28DF23AD-5F02-4C01-B149-4F28D499BBA1", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "965E571A-8A50-4B91-ACE3-7962EDF52447" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            // Attrib Value for Block:Workflow Entry, Attribute:Show Summary View Page: Volunteer Application - Adult, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "965E571A-8A50-4B91-ACE3-7962EDF52447", "1CFB44EE-4DF7-40DD-83DC-B7801909D259", @"False" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Volunteer Application - Adult, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "965E571A-8A50-4B91-ACE3-7962EDF52447", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"c2534042-8e5c-49c6-9972-cd23a953113a" );

            //////////////////////////////////////// Volunteer Application - Student //////////////////////////////////////////////////////

            // Page: Volunteer Application - Student
            RockMigrationHelper.AddPage( "4F0FAED3-E59D-4AA2-9B6F-782B5B32CAEE", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Volunteer Application - Student", "", "F65BC199-9589-4641-9D6B-0B720015DFA2", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "F65BC199-9589-4641-9D6B-0B720015DFA2", "VolunteerStudentApplication" );
            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
            // Add Block to Page: Volunteer Application - Student, Site: External Website
            RockMigrationHelper.AddBlock( true, "F65BC199-9589-4641-9D6B-0B720015DFA2", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "9B208F8A-4B7F-4F23-82BE-D6FD003EA31A" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Volunteer Application - Student, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "9B208F8A-4B7F-4F23-82BE-D6FD003EA31A", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"7c874929-af96-411f-99cf-68ec829fabbc" );
            // Attrib Value for Block:Workflow Entry, Attribute:Show Summary View Page: Volunteer Application - Student, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "9B208F8A-4B7F-4F23-82BE-D6FD003EA31A", "1CFB44EE-4DF7-40DD-83DC-B7801909D259", @"False" );
        }


        public override void Down()
        {
            /////////////////////////// Name Clearing Report /////////////////////
            RockMigrationHelper.DeleteAttribute( "BC03021E-D749-41AF-AE39-AB7023AA87DB" );
            RockMigrationHelper.DeleteAttribute( "AA23BA3A-3DB0-4496-90CB-DB53AE5A7A3B" );
            RockMigrationHelper.DeleteAttribute( "692DF71B-DB81-4548-8D7A-D664CEFDFB31" );
            RockMigrationHelper.DeleteAttribute( "7D2952F9-F497-49AB-8392-71396E808462" );
            RockMigrationHelper.DeleteAttribute( "8534E943-02F5-4D0B-A20C-B9C7A48B2CFA" );
            RockMigrationHelper.DeleteAttribute( "81B97BCA-58B5-4DD6-AB57-FF3A619CE052" );
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
            RockMigrationHelper.DeleteBlock( "5D630085-4F8F-4D9C-A033-4A0C5CE2DD9C" );
            RockMigrationHelper.DeleteBlock( "C2E1EF75-8695-41F0-A50A-7BE6E545B666" );
            RockMigrationHelper.DeleteBlockType( "EC464362-6645-4944-A21A-1AAAB023BA9B" );
            RockMigrationHelper.DeleteBlockType( "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.DeletePage( "0CBFBB2E-4392-436F-990C-E63F1548E0EC" ); //  Page: Name Clearing Report

            ////////////////////// PDF///////////////////////////
            RockMigrationHelper.DeleteAttribute( "E778DCBE-DE6A-446C-A188-AA8CE4CDD7A4" );
            RockMigrationHelper.DeleteAttribute( "0E28EC49-6F7F-4E8A-8F9E-553376C7D807" );
            RockMigrationHelper.DeleteAttribute( "69076356-1E59-42ED-BE93-CFCB68E27D16" );
            RockMigrationHelper.DeleteAttribute( "2A5003DC-7179-4A10-B776-2C5E3A53DB8F" );
            RockMigrationHelper.DeleteAttribute( "64458847-D7B1-4FF5-B60E-57BEB7CA7CCA" );
            RockMigrationHelper.DeleteAttribute( "9A4EBDF8-C67B-4DF5-B9BD-C2D5F389F13F" );
            RockMigrationHelper.DeleteAttribute( "5A2E2853-48DA-46AE-84B5-E03E8B388B2F" );
            RockMigrationHelper.DeleteAttribute( "4FDD6E70-04D2-4949-9C5D-8B97B1A50CC4" );
            RockMigrationHelper.DeleteAttribute( "93507651-B019-492B-9AE6-638EC4578ACD" );
            RockMigrationHelper.DeleteAttribute( "BF5778D3-91B5-439F-8813-ABC88AACB053" );
            RockMigrationHelper.DeleteAttribute( "04E62176-4107-4895-86B9-608B2CCF573D" );
            RockMigrationHelper.DeleteAttribute( "BEE6CBB0-2CBB-4C4C-BB7D-828238D69B28" );
            RockMigrationHelper.DeleteAttribute( "D1D6EC5E-3C01-4D80-A7AA-8A614C1063FE" );
            RockMigrationHelper.DeleteAttribute( "8E2C5411-2155-4229-AE9B-4A046809BF2C" );
            RockMigrationHelper.DeleteAttribute( "7A4ACB9A-487C-4D7C-93BA-58DF27A8C2FF" );
            RockMigrationHelper.DeleteAttribute( "EBD3FE22-39C9-421B-B300-F6CC3636D78D" );
            RockMigrationHelper.DeleteAttribute( "8B5385F1-3C9E-4ED9-B8E1-0FE370BC28A9" );
            RockMigrationHelper.DeleteAttribute( "84189421-47DD-43F6-AC13-24602D7F951D" );
            RockMigrationHelper.DeleteAttribute( "219FCCAB-E315-4BC6-A6D4-BF2529980E47" );
            RockMigrationHelper.DeleteBlock( "6FAA0366-B93A-4554-8425-C67830DCE4D1" );
            RockMigrationHelper.DeleteBlockType( "5790A87A-D619-4D98-86F8-4C9E7E74349D" );
            RockMigrationHelper.DeletePage( "8558E07B-E881-48B2-AC39-FBE807672B2B" ); //  Page: Clearing Name PDF

            ////////////////////////////////////// Standalone Pages ///////////////////////////////////////
            RockMigrationHelper.DeletePage( "4F0FAED3-E59D-4AA2-9B6F-782B5B32CAEE" ); //  Page: Standalone Pages

            ///////////////////////////////////// Volunteer ///////////////////////////////////////
            RockMigrationHelper.DeleteAttribute( "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            RockMigrationHelper.DeleteAttribute( "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            RockMigrationHelper.DeleteBlock( "4FF7B41D-B19F-41EF-8E59-C8A5F9BB36AD" );
            RockMigrationHelper.DeleteBlockType( "A8BD05C8-6F89-4628-845B-059E686F089A" );
            RockMigrationHelper.DeletePage( "05B385CE-184F-43E1-97FA-22A0F9974FD7" ); //  Page: Volunteer

            ////////////////////////////////////////// Volunteer Application - Adult //////////////////////////////////////
            RockMigrationHelper.DeleteAttribute( "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            RockMigrationHelper.DeleteAttribute( "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            RockMigrationHelper.DeleteBlock( "965E571A-8A50-4B91-ACE3-7962EDF52447" );
            RockMigrationHelper.DeleteBlockType( "A8BD05C8-6F89-4628-845B-059E686F089A" );
            RockMigrationHelper.DeletePage( "28DF23AD-5F02-4C01-B149-4F28D499BBA1" ); //  Page: Volunteer Application - Adult

            ////////////////////////////////// Volunteer Application - Student //////////////////////////////////
            RockMigrationHelper.DeleteAttribute( "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            RockMigrationHelper.DeleteAttribute( "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            RockMigrationHelper.DeleteBlock( "9B208F8A-4B7F-4F23-82BE-D6FD003EA31A" );
            RockMigrationHelper.DeleteBlockType( "A8BD05C8-6F89-4628-845B-059E686F089A" );
            RockMigrationHelper.DeletePage( "F65BC199-9589-4641-9D6B-0B720015DFA2" ); //  Page: Volunteer Application - Student

            throw new NotImplementedException();
        }


    }
}
