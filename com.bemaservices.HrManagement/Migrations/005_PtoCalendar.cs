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
    [MigrationNumber( 5, "1.10.3" )]
    public class PtoCalendar : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: PTO Calendar
            RockMigrationHelper.AddPage( "F159FB38-FEF9-4F08-8A5C-5E112B1DD88F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "PTO Calendar", "", "0A39E59F-D733-47D8-9953-BB30B5C33A27", "fa fa-calendar" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0A39E59F-D733-47D8-9953-BB30B5C33A27", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 0, "F7F8E371-C859-401A-8123-7A7F468D024A" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            RockMigrationHelper.UpdateHtmlContentBlock( "F7F8E371-C859-401A-8123-7A7F468D024A", "{% include '~/Plugins/com_bemaservices/HrManagement/Assets/Lava/PTOCalendarIframe.Lava' %}", "9DE93FC6-E0E8-408B-9DE3-30B2EF55DE02" );

            // Page: Vue PTO Calendar
            RockMigrationHelper.AddPage( "0A39E59F-D733-47D8-9953-BB30B5C33A27", "2E169330-D7D7-4ECA-B417-72C64BE150F0", "Vue PTO Calendar", "", "D7984E66-C3B7-41E9-9AFC-9F7D5124612F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "D7984E66-C3B7-41E9-9AFC-9F7D5124612F", "ptovuecalendar" );
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D7984E66-C3B7-41E9-9AFC-9F7D5124612F", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 0, "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity,Sql" );
            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Vue PTO Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            RockMigrationHelper.UpdateHtmlContentBlock( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0", "{% include '~/Plugins/com_bemaservices/HrManagement/Assets/Lava/PTOCalendar.Lava' %}", "C9A454F6-984F-4915-9E14-14C10039465D" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "FBE2780E-5E33-4C3D-809A-0CE4884B9FC0" );
            RockMigrationHelper.DeletePage( "D7984E66-C3B7-41E9-9AFC-9F7D5124612F" ); //  Page: Vue PTO Calendar

            RockMigrationHelper.DeleteBlock( "F7F8E371-C859-401A-8123-7A7F468D024A" );
            RockMigrationHelper.DeletePage( "0A39E59F-D733-47D8-9953-BB30B5C33A27" ); //  Page: PTO Calendar
        }
    }
}
