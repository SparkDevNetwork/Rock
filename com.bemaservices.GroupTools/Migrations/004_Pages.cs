using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 4, "1.9.4" )]
    public class Pages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Group Finder
            RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Group Finder", "", "E9352E37-E80C-474D-B815-D963C5C31ECC", "fa fa-group" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlock( true, "E9352E37-E80C-474D-B815-D963C5C31ECC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 0, "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2" );
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
            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Group Finder, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            RockMigrationHelper.UpdateHtmlContentBlock( "664F6F1E-99D7-4890-B4FC-A8DCF1B825D2", @"{% include '~/Plugins/com_bemaservices/GroupTools/Assets/Lava/GroupFinder.lava' %}", "84C9F39F-BC31-4490-9523-394A9FE8A689" );

            // Page: Group Details
            RockMigrationHelper.AddPage( "E9352E37-E80C-474D-B815-D963C5C31ECC", "BE15B7BC-6D64-4880-991D-FDE962F91196", "Group Details", "", "86D1DB87-021E-49F3-A547-1E096667DF63", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
            // Add Block to Page: Group Details, Site: External Website
            RockMigrationHelper.AddBlock( true, "86D1DB87-021E-49F3-A547-1E096667DF63", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "E388AE1A-6D8B-4513-B663-A60C15034511" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Group Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E388AE1A-6D8B-4513-B663-A60C15034511", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"75a7dcfb-af3c-492a-81eb-f48baf606bbb" );
            // Attrib Value for Block:Workflow Entry, Attribute:Show Summary View Page: Group Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E388AE1A-6D8B-4513-B663-A60C15034511", "1CFB44EE-4DF7-40DD-83DC-B7801909D259", @"False" );
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

