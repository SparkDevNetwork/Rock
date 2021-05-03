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
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class LandingPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Blocks
            RockMigrationHelper.AddSite( "Landing Page", "Site for Rock Landing pages", "LandingPage", "11E449C7-CCC8-4C24-B280-FA5363C8F554" );
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "Blank", "Blank", "", "57FCEB5D-54FC-48D0-92D1-A83F8CD99DAC" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "Dialog", "Dialog", "", "580EDE4F-249C-4D43-870C-0A3E7F9477A0" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "Error", "Error", "", "9D371240-5221-4562-AE0E-3BBCE1A95DDF" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullWidth", "Full Width", "", "31C79AAC-5A71-4E47-B62C-9C707F2F08B3" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullCentered", "Full Centered", "", "D92341BE-E482-4453-BA69-A999D26F51DA" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "SimpleFiftyFifty", "Simple Fifty Fifty", "", "706B872D-B56D-42A7-A868-1386D4E5D95F" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullClassic", "Full Classic", "", "B9126B80-146A-466F-B087-F461CE4E64A5" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullBottom", "Full Bottom", "", "C329ADC7-BA18-4532-AF88-C64ED2E137ED" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullFiftyFiftyOverlay", "Full Fifty Fifty Overlay", "", "8E1C6723-8B32-4D03-BAA6-62F158BD1071" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "SimpleFiftyFiftyOverlay", "Simple Fifty Fifty Overlay", "", "5013BAE9-CBB9-4A02-B26E-F63C91F4CA84" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullCard", "Full Card", "", "003D88BB-5DD8-46D3-BE19-C9ECAF5B665D" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "SimpleCard", "Simple Card", "", "BC335552-C9C3-4F2A-8ED7-C3384CD60301" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullCenteredInline", "Full Centered Inline", "", "44EDF6E6-2267-4806-B505-1D54960CF99C" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "SimpleCentered", "Simple Centered", "", "1B29ECAC-A1A3-4829-9464-5438400D8CD2" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "SimpleCard-Inline", "Simple Card - Inline", "", "5077891B-ABCC-480A-B2BA-E4D7A1246FF5" ); // Site:Landing Pages
            RockMigrationHelper.AddLayout( "11E449C7-CCC8-4C24-B280-FA5363C8F554", "FullFiftyFifty", "Full Fifty Fifty", "", "9DD35CBC-1496-456B-B01F-CED30C682D65" ); // Site:Landing Pages

            Sql( MigrationSQL._201806081810282_LandingPage );

            Sql( @"DECLARE @SiteId int = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '11E449C7-CCC8-4C24-B280-FA5363C8F554')
                   INSERT INTO [dbo].[Attribute]([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [ForeignId], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory]) VALUES ('0', 10, 2, N'SiteId', @SiteId, N'SecondaryImage', N'Secondary Image', N'', 1, '0', N'', '0', '0', 'D520FAD6-585D-4F59-84F8-5918715320A9', NULL, N'', '0', NULL, NULL, '0', '0', '0', '1', '0')" );

            RockMigrationHelper.AddAttributeQualifier( "D520FAD6-585D-4F59-84F8-5918715320A9", "binaryFileType", "", "499F245D-FC96-19B3-4354-FB7C766096B0" );
            RockMigrationHelper.AddAttributeQualifier( "D520FAD6-585D-4F59-84F8-5918715320A9", "formatAsLink", "False", "22BBE22B-C4BF-EFB0-4437-05791D5F31A7" );
            RockMigrationHelper.AddAttributeQualifier( "D520FAD6-585D-4F59-84F8-5918715320A9", "img_tag_template", "", "33863AE7-FF86-95BF-4745-3B6C8A260F38" );

            Sql( @"DECLARE @SiteId int = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '11E449C7-CCC8-4C24-B280-FA5363C8F554')
                   INSERT INTO [dbo].[Attribute]([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [ForeignId], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory]) VALUES ('0', 10, 2, N'SiteId', @SiteId, N'HeaderImage', N'Header Image', N'', 0, '0', N'', '0', '0', '24FE0736-D09E-48A3-AD42-7F002B9974AB', NULL, N'', '0', NULL, NULL, '0', '0', '0', '1', '0')" );

            RockMigrationHelper.AddAttributeQualifier( "24FE0736-D09E-48A3-AD42-7F002B9974AB", "binaryFileType", "", "EA5DB7C4-2992-E6B3-47A5-CFE57CB23A75" );
            RockMigrationHelper.AddAttributeQualifier( "24FE0736-D09E-48A3-AD42-7F002B9974AB", "formatAsLink", "False", "A6BBF6B9-1544-0C9E-43DD-9EB2EFA9F6BF" );
            RockMigrationHelper.AddAttributeQualifier( "24FE0736-D09E-48A3-AD42-7F002B9974AB", "img_tag_template", "", "1C7F3114-7500-C98D-4948-D6E708C45528" );

            // Page: Landing Pages Home Page
            RockMigrationHelper.AddPage( true, "", "31C79AAC-5A71-4E47-B62C-9C707F2F08B3", "Landing Pages Home Page", "", "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "" );

            Sql( @"DECLARE @PageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490')
                   UPDATE [dbo].[Site] SET [IsSystem] = 0, [DefaultPageId]=@PageId WHERE [Guid]='11E449C7-CCC8-4C24-B280-FA5363C8F554'" );

            // Site:Landing Pages
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Workflow", "", "", 0, "ED74AB11-6CBA-46C7-8BC0-CEF9CC0A3BB9" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Headline", "", "", 0, "C90FCC3A-1C80-4D70-8A52-2DA27C30EF7A" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SectionB", "", "", 0, "B86F9EAB-1778-4947-8166-720C890C0391" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SectionC", "", "", 0, "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SectionD", "", "", 0, "61EA9BD2-57E4-4897-A914-A158DE755641" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", "", "", 0, "42105CB7-E593-4102-A259-9676F1DC899C" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "CTAButtons", "", "", 0, "2A6C7BED-26E6-4A00-A7CF-4E27095ADAE4" );

            // Add Block to Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlock( true, "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "SecondaryHero", "", "", 0, "D8472640-94A7-4763-B0CD-BF6691699D2B" );

            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", "The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );

            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );

            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );

            // Attrib for BlockType: Workflow Entry:Show Summary View
            RockMigrationHelper.UpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Summary View", "ShowSummaryView", "", "If workflow has been completed, should the summary view be displayed?", 1, @"False", "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );

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

            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "ED74AB11-6CBA-46C7-8BC0-CEF9CC0A3BB9", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"5279e73a-248d-4cff-b562-3f8e27b8ce23" );

            // Attrib Value for Block:Workflow Entry, Attribute:Show Summary View Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "ED74AB11-6CBA-46C7-8BC0-CEF9CC0A3BB9", "1CFB44EE-4DF7-40DD-83DC-B7801909D259", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Context Parameter Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Context Name Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "0673E015-F8DD-4A52-B380-C758011331B2", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:Entity Type Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Tags Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "B86F9EAB-1778-4947-8166-720C890C0391", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Context Parameter Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Context Name Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "0673E015-F8DD-4A52-B380-C758011331B2", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:Entity Type Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Tags Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Context Parameter Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Context Name Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "0673E015-F8DD-4A52-B380-C758011331B2", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:Entity Type Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Tags Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "61EA9BD2-57E4-4897-A914-A158DE755641", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Duration Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Attrib Value for Block:HTML Content, Attribute:Require Approval Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Enable Versioning Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Context Parameter Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Context Name Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Start in Code Editor mode Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "0673E015-F8DD-4A52-B380-C758011331B2", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Image Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:User Specific Folders Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Document Root Folder Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:HTML Content, Attribute:Entity Type Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Enabled Lava Commands Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"" );

            // Attrib Value for Block:HTML Content, Attribute:Is Secondary Block Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Attrib Value for Block:HTML Content, Attribute:Cache Tags Page: Landing Pages Home Page, Site: Landing Pages
            RockMigrationHelper.AddBlockAttributeValue( "42105CB7-E593-4102-A259-9676F1DC899C", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );

            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.GetPersonFromFields", "E5E7CA24-7030-4D48-9C39-04B5809E71A8", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersistWorkflow", "F1A39347-6FE0-43D4-89FB-544195088ECF", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersonNoteAdd", "B2C8B951-C41E-4DFB-9F92-F183223448AA", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" );
            // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" );
            // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" );
            // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" );
            // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" );
            // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" );
            // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" );
            // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "65E69B78-37D8-4A88-B8AC-71893D2F75EF" );
            // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" );
            // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" );
            // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" );
            // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F" );
            // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" );
            // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" );
            // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" );
            // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" );
            // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "2778BEDA-ECB0-4057-8475-D624495BAEE4" );
            // Rock.Workflow.Action.PersonNoteAdd:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Alert", "Alert", "Determines if the note should be flagged as an alert.", 5, @"False", "49C560A6-4347-4907-B7E5-38CAC697C86B" );
            // Rock.Workflow.Action.PersonNoteAdd:Alert
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Author", "Author", "Workflow attribute that contains the person to use as the author of the note. While not required it is recommended.", 4, @"", "756DBFE4-4B36-49C3-A047-DB52274D5CF8" );
            // Rock.Workflow.Action.PersonNoteAdd:Author
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person", "Person", "Workflow attribute that contains the person to add the note to.", 0, @"", "EE030DB7-2FB7-482B-98AF-7BD61035CAD1" );
            // Rock.Workflow.Action.PersonNoteAdd:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "The title/caption of the note. If none is provided then the author's name will be displayed. <span class='tip tip-lava'></span>", 2, @"", "90416422-D244-4D73-A878-7C60350AB154" );
            // Rock.Workflow.Action.PersonNoteAdd:Caption
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D5CD976E-E641-465A-AD08-A13F635353F4" );
            // Rock.Workflow.Action.PersonNoteAdd:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Text", "Text", "The body of the note. <span class='tip tip-lava'></span>", 3, @"", "D45B1A38-94F0-4B4D-A895-D4E6E14F82C5" );
            // Rock.Workflow.Action.PersonNoteAdd:Text
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "The type of note to add.", 1, @"66A1B9D7-7EFA-40F3-9415-E54437977D60", "4A120959-FB2D-49D2-A9A4-6D04B9DB53D2" );
            // Rock.Workflow.Action.PersonNoteAdd:Note Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" );
            // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" );
            // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" );
            // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" );
            // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" );
            // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FE0EA0F6-9612-4E7C-A1EF-ADF0724F00BF" );
            // Rock.Workflow.Action.GetPersonFromFields:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Default Campus", "DefaultCampus", "The attribute value to use as the default campus when creating a new person.", 6, @"", "CB3D18DB-E19C-48C4-B9ED-0764373E2598" );
            // Rock.Workflow.Action.GetPersonFromFields:Default Campus
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The person attribute to set the value to the person found or created.", 3, @"", "16307EEA-9646-42F7-9A31-0B5933B3C53C" );
            // Rock.Workflow.Action.GetPersonFromFields:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Email Address|Attribute Value", "Email", "The email address or an attribute that contains the email address of the person. <span class='tip tip-lava'></span>", 2, @"", "42B3DAD7-307A-4453-A7EA-674945DA72B4" );
            // Rock.Workflow.Action.GetPersonFromFields:Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "First Name|Attribute Value", "FirstName", "The first name or an attribute that contains the first name of the person. <span class='tip tip-lava'></span>", 0, @"", "02A1EA9F-AB3F-4D1A-91C0-173FBE974BDC" );
            // Rock.Workflow.Action.GetPersonFromFields:First Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Last Name|Attribute Value", "LastName", "The last name or an attribute that contains the last name of the person. <span class='tip tip-lava'></span>", 1, @"", "94A570D3-EC23-4EBA-A412-F43F91D91E3F" );
            // Rock.Workflow.Action.GetPersonFromFields:Last Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "The connection status to use when creating a new person", 5, @"368DD475-242C-49C4-A42C-7278BE690CC2", "203FA19A-50E0-449D-BB45-F12FC4ADB600" );
            // Rock.Workflow.Action.GetPersonFromFields:Default Connection Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Record Status", "DefaultRecordStatus", "The record status to use when creating a new person", 4, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "35B6603A-20B3-4BC5-8320-52DF3D527754" );
            // Rock.Workflow.Action.GetPersonFromFields:Default Record Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E5E7CA24-7030-4D48-9C39-04B5809E71A8", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1CFE3B8B-7F1E-4498-8345-50133E4FDFDF" );
            // Rock.Workflow.Action.GetPersonFromFields:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" );
            // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "07CB7DBC-236D-4D38-92A4-47EE448BA89A" );
            // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" );
            // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" );
            // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Persist Immediately", "PersistImmediately", "This action will normally cause the workflow to be persisted (saved) once all the current activites/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.", 0, @"False", "E22BE348-18B1-4420-83A8-6319B35416D2" );
            // Rock.Workflow.Action.PersistWorkflow:Persist Immediately
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" );
            // Rock.Workflow.Action.PersistWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Samples", "fa fa-star", "", "CB99421E-9ADC-488E-8C71-94BB14F27F56", 0 );
            // Samples

            #endregion

            #region Landing Page

            RockMigrationHelper.UpdateWorkflowType( false, true, "Landing Page", "", "CB99421E-9ADC-488E-8C71-94BB14F27F56", "Request", "fa fa-space-shuttle", 28800, false, 0, "5279E73A-248D-4CFF-B562-3F8E27B8CE23", 0 );
            // Landing Page
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "8A9C3D41-5AB0-4EDC-A214-141C10C1105A", false );
            // Landing Page:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "", 1, @"", "1D77A98A-0375-4F25-821C-61B31B06FC83", false );
            // Landing Page:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", "9C204CD0-1233-41C5-818A-C5DA439445AA", "First Name", "FirstName", "", 2, @"", "2B9B570A-5A79-4E59-A29C-9126F3CEC3B9", true );
            // Landing Page:First Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Last Name", "LastName", "", 3, @"", "A3BD11DF-4413-4E27-A682-682A4A26D639", true );
            // Landing Page:Last Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "", 4, @"", "5EA2B656-3EA8-4368-A604-4648779C0268", true );
            // Landing Page:Email
            RockMigrationHelper.AddAttributeQualifier( "8A9C3D41-5AB0-4EDC-A214-141C10C1105A", "EnableSelfSelection", @"False", "D585454A-9DB9-4F85-9856-8722A9F3BE92" );
            // Landing Page:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "1D77A98A-0375-4F25-821C-61B31B06FC83", "includeInactive", @"False", "B00B4A55-4156-4832-8E34-C119CF7F94F7" );
            // Landing Page:Campus:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "2B9B570A-5A79-4E59-A29C-9126F3CEC3B9", "ispassword", @"False", "7064FAC9-10EA-4332-8C09-74FA503E81E0" );
            // Landing Page:First Name:ispassword
            RockMigrationHelper.AddAttributeQualifier( "2B9B570A-5A79-4E59-A29C-9126F3CEC3B9", "maxcharacters", @"", "B706F247-5A1C-4A9D-BD7A-D06AE6DF1B21" );
            // Landing Page:First Name:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "2B9B570A-5A79-4E59-A29C-9126F3CEC3B9", "showcountdown", @"False", "DE023D30-D9EA-4668-8C20-C014D7D0AFC8" );
            // Landing Page:First Name:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "A3BD11DF-4413-4E27-A682-682A4A26D639", "ispassword", @"False", "9F62692C-094B-400F-BAB2-F36488684647" );
            // Landing Page:Last Name:ispassword
            RockMigrationHelper.AddAttributeQualifier( "A3BD11DF-4413-4E27-A682-682A4A26D639", "maxcharacters", @"", "461295D9-8E13-4CA9-AEFB-BF18031E2564" );
            // Landing Page:Last Name:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "A3BD11DF-4413-4E27-A682-682A4A26D639", "showcountdown", @"False", "4CC69D7C-B9D7-4BC7-A52D-346099D21B02" );
            // Landing Page:Last Name:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", true, "Start", "", true, 0, "318D162F-E528-4224-A9A5-9EB231D722F0" );
            // Landing Page:Start
            RockMigrationHelper.UpdateWorkflowActivityType( "5279E73A-248D-4CFF-B562-3F8E27B8CE23", true, "Continue", "", false, 1, "D2937C40-3AAB-4AB9-9599-CC09B94A988C" );
            // Landing Page:Continue
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h2>Join now</h2> <p>Enter your info and we&#8217;ll help you find the perfect group.</p>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^D2937C40-3AAB-4AB9-9599-CC09B94A988C^Your information has been submitted successfully.|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "921F5BAC-AC70-4122-A21E-E31658355D29" );
            // Landing Page:Start:Prompt User
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "921F5BAC-AC70-4122-A21E-E31658355D29", "8A9C3D41-5AB0-4EDC-A214-141C10C1105A", 0, false, true, false, false, @"", @"", "4BB74F3F-53AD-4022-8286-6AD27F215F3D" );
            // Landing Page:Start:Prompt User:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "921F5BAC-AC70-4122-A21E-E31658355D29", "1D77A98A-0375-4F25-821C-61B31B06FC83", 4, false, true, false, false, @"", @"", "73EAA05B-E31D-49EE-924B-0FE2DA43EAD7" );
            // Landing Page:Start:Prompt User:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "921F5BAC-AC70-4122-A21E-E31658355D29", "2B9B570A-5A79-4E59-A29C-9126F3CEC3B9", 1, true, false, true, false, @"", @"", "AE541C8A-B422-47BF-9D32-04476A583230" );
            // Landing Page:Start:Prompt User:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "921F5BAC-AC70-4122-A21E-E31658355D29", "A3BD11DF-4413-4E27-A682-682A4A26D639", 2, true, false, true, false, @"", @"", "293E9D24-6AB5-40DD-9015-152F47937FF1" );
            // Landing Page:Start:Prompt User:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "921F5BAC-AC70-4122-A21E-E31658355D29", "5EA2B656-3EA8-4368-A604-4648779C0268", 3, true, false, true, false, @"", @"", "BF5DC496-6A62-4639-8BC8-2B6DC3DFAA20" );
            // Landing Page:Start:Prompt User:Email
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Current Person", 0, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "692138D6-C7FD-45F3-ACE1-71BAA07ECB44" );
            // Landing Page:Start:Current Person
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Set Email", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD" );
            // Landing Page:Start:Set Email
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Set Nickname", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156" );
            // Landing Page:Start:Set Nickname
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Set Last Name", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "BAC01E98-3DF1-48F8-9727-7E9276FB84B2" );
            // Landing Page:Start:Set Last Name
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Prompt User", 4, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "921F5BAC-AC70-4122-A21E-E31658355D29", "", 1, "", "D4984E98-48E5-4057-AE6A-D6896DCDDF76" );
            // Landing Page:Start:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType( "318D162F-E528-4224-A9A5-9EB231D722F0", "Match Person", 5, "E5E7CA24-7030-4D48-9C39-04B5809E71A8", true, false, "", "", 1, "", "C1EAD748-C39B-43BF-A504-6C9C7BC7462E" );
            // Landing Page:Start:Match Person
            RockMigrationHelper.UpdateWorkflowActionType( "D2937C40-3AAB-4AB9-9599-CC09B94A988C", "Persist", 0, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "D16D3FD9-245F-477A-B970-82F9B5F2D2A5" );
            // Landing Page:Continue:Persist
            RockMigrationHelper.UpdateWorkflowActionType( "D2937C40-3AAB-4AB9-9599-CC09B94A988C", "Send Email to Requestor", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "AEB0BCD1-995D-49C3-95CD-974A646F5DE6" );
            // Landing Page:Continue:Send Email to Requestor
            RockMigrationHelper.UpdateWorkflowActionType( "D2937C40-3AAB-4AB9-9599-CC09B94A988C", "Add Note to Person", 2, "B2C8B951-C41E-4DFB-9F92-F183223448AA", true, false, "", "", 1, "", "8FA45914-C71C-42A0-9092-CB191F2D5E28" );
            // Landing Page:Continue:Add Note to Person
            RockMigrationHelper.UpdateWorkflowActionType( "D2937C40-3AAB-4AB9-9599-CC09B94A988C", "Close Workflow", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "08A69B46-EF7F-4EDF-A46D-F860BB3AADCA" );
            // Landing Page:Continue:Close Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "692138D6-C7FD-45F3-ACE1-71BAA07ECB44", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" );
            // Landing Page:Start:Current Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "692138D6-C7FD-45F3-ACE1-71BAA07ECB44", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"8a9c3d41-5ab0-4edc-a214-141c10c1105a" );
            // Landing Page:Start:Current Person:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "692138D6-C7FD-45F3-ACE1-71BAA07ECB44", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" );
            // Landing Page:Start:Current Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD", "1B833F48-EFC2-4537-B1E3-7793F6863EAA", @"" );
            // Landing Page:Start:Set Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" );
            // Landing Page:Start:Set Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ CurrentPerson.Email }}" );
            // Landing Page:Start:Set Email:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"5ea2b656-3ea8-4368-a604-4648779c0268" );
            // Landing Page:Start:Set Email:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "EEC8076D-60F0-485B-A6AE-5BBA9A6BBADD", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"" );
            // Landing Page:Start:Set Email:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ CurrentPerson.NickName }}" );
            // Landing Page:Start:Set Nickname:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156", "1B833F48-EFC2-4537-B1E3-7793F6863EAA", @"" );
            // Landing Page:Start:Set Nickname:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" );
            // Landing Page:Start:Set Nickname:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2b9b570a-5a79-4e59-a29c-9126f3cec3b9" );
            // Landing Page:Start:Set Nickname:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F889C7E1-EEEC-4D65-BCD2-DE2E3017B156", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"" );
            // Landing Page:Start:Set Nickname:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "BAC01E98-3DF1-48F8-9727-7E9276FB84B2", "1B833F48-EFC2-4537-B1E3-7793F6863EAA", @"" );
            // Landing Page:Start:Set Last Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "BAC01E98-3DF1-48F8-9727-7E9276FB84B2", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" );
            // Landing Page:Start:Set Last Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BAC01E98-3DF1-48F8-9727-7E9276FB84B2", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ CurrentPerson.LastName }}" );
            // Landing Page:Start:Set Last Name:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "BAC01E98-3DF1-48F8-9727-7E9276FB84B2", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"a3bd11df-4413-4e27-a682-682a4a26d639" );
            // Landing Page:Start:Set Last Name:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BAC01E98-3DF1-48F8-9727-7E9276FB84B2", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"" );
            // Landing Page:Start:Set Last Name:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "D4984E98-48E5-4057-AE6A-D6896DCDDF76", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" );
            // Landing Page:Start:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D4984E98-48E5-4057-AE6A-D6896DCDDF76", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" );
            // Landing Page:Start:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "02A1EA9F-AB3F-4D1A-91C0-173FBE974BDC", @"2b9b570a-5a79-4e59-a29c-9126f3cec3b9" );
            // Landing Page:Start:Match Person:First Name|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "1CFE3B8B-7F1E-4498-8345-50133E4FDFDF", @"" );
            // Landing Page:Start:Match Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "FE0EA0F6-9612-4E7C-A1EF-ADF0724F00BF", @"False" );
            // Landing Page:Start:Match Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "94A570D3-EC23-4EBA-A412-F43F91D91E3F", @"a3bd11df-4413-4e27-a682-682a4a26d639" );
            // Landing Page:Start:Match Person:Last Name|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "42B3DAD7-307A-4453-A7EA-674945DA72B4", @"5ea2b656-3ea8-4368-a604-4648779c0268" );
            // Landing Page:Start:Match Person:Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "16307EEA-9646-42F7-9A31-0B5933B3C53C", @"8a9c3d41-5ab0-4edc-a214-141c10c1105a" );
            // Landing Page:Start:Match Person:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "35B6603A-20B3-4BC5-8320-52DF3D527754", @"283999ec-7346-42e3-b807-bce9b2babb49" );
            // Landing Page:Start:Match Person:Default Record Status
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "203FA19A-50E0-449D-BB45-F12FC4ADB600", @"368dd475-242c-49c4-a42c-7278be690cc2" );
            // Landing Page:Start:Match Person:Default Connection Status
            RockMigrationHelper.AddActionTypeAttributeValue( "C1EAD748-C39B-43BF-A504-6C9C7BC7462E", "CB3D18DB-E19C-48C4-B9ED-0764373E2598", @"1d77a98a-0375-4f25-821c-61b31b06fc83" );
            // Landing Page:Start:Match Person:Default Campus
            RockMigrationHelper.AddActionTypeAttributeValue( "D16D3FD9-245F-477A-B970-82F9B5F2D2A5", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" );
            // Landing Page:Continue:Persist:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D16D3FD9-245F-477A-B970-82F9B5F2D2A5", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" );
            // Landing Page:Continue:Persist:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D16D3FD9-245F-477A-B970-82F9B5F2D2A5", "E22BE348-18B1-4420-83A8-6319B35416D2", @"False" );
            // Landing Page:Continue:Persist:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" );
            // Landing Page:Continue:Send Email to Requestor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" );
            // Landing Page:Continue:Send Email to Requestor:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" );
            // Landing Page:Continue:Send Email to Requestor:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "0C4C13B8-7076-4872-925A-F950886B5E16", @"5ea2b656-3ea8-4368-a604-4648779c0268" );
            // Landing Page:Continue:Send Email to Requestor:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F", @"" );
            // Landing Page:Continue:Send Email to Requestor:Send to Group Role
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"" );
            // Landing Page:Continue:Send Email to Requestor:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"" );
            // Landing Page:Continue:Send Email to Requestor:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C", @"" );
            // Landing Page:Continue:Send Email to Requestor:Attachment One
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "FFD9193A-451F-40E6-9776-74D5DCAC1450", @"" );
            // Landing Page:Continue:Send Email to Requestor:Attachment Two
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "A059767A-5592-4926-948A-1065AF4E9748", @"" );
            // Landing Page:Continue:Send Email to Requestor:Attachment Three
            RockMigrationHelper.AddActionTypeAttributeValue( "AEB0BCD1-995D-49C3-95CD-974A646F5DE6", "65E69B78-37D8-4A88-B8AC-71893D2F75EF", @"False" );
            // Landing Page:Continue:Send Email to Requestor:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "EE030DB7-2FB7-482B-98AF-7BD61035CAD1", @"8a9c3d41-5ab0-4edc-a214-141c10c1105a" );
            // Landing Page:Continue:Add Note to Person:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "D5CD976E-E641-465A-AD08-A13F635353F4", @"" );
            // Landing Page:Continue:Add Note to Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "2778BEDA-ECB0-4057-8475-D624495BAEE4", @"False" );
            // Landing Page:Continue:Add Note to Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "4A120959-FB2D-49D2-A9A4-6D04B9DB53D2", @"87BACB34-DB87-45E0-AB60-BFABF7CEECDB" );
            // Landing Page:Continue:Add Note to Person:Note Type
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "90416422-D244-4D73-A878-7C60350AB154", @"" );
            // Landing Page:Continue:Add Note to Person:Caption
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "D45B1A38-94F0-4B4D-A895-D4E6E14F82C5", @"Signed up for {{ Workflow.WorkflowType.Name }}" );
            // Landing Page:Continue:Add Note to Person:Text
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "756DBFE4-4B36-49C3-A047-DB52274D5CF8", @"" );
            // Landing Page:Continue:Add Note to Person:Author
            RockMigrationHelper.AddActionTypeAttributeValue( "8FA45914-C71C-42A0-9092-CB191F2D5E28", "49C560A6-4347-4907-B7E5-38CAC697C86B", @"False" );
            // Landing Page:Continue:Add Note to Person:Alert
            RockMigrationHelper.AddActionTypeAttributeValue( "08A69B46-EF7F-4EDF-A46D-F860BB3AADCA", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" );
            // Landing Page:Continue:Close Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "08A69B46-EF7F-4EDF-A46D-F860BB3AADCA", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" );
            // Landing Page:Continue:Close Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "08A69B46-EF7F-4EDF-A46D-F860BB3AADCA", "07CB7DBC-236D-4D38-92A4-47EE448BA89A", @"Completed" );
            // Landing Page:Continue:Close Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) ) 
FROM [AttributeQualifier] [aq]
INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
AND [aq].[key] = 'definedtypeguid'" );

            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            RockMigrationHelper.DeleteAttribute( "1CFB44EE-4DF7-40DD-83DC-B7801909D259" );
            RockMigrationHelper.DeleteAttribute( "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            RockMigrationHelper.DeleteAttribute( "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            RockMigrationHelper.DeleteAttribute( "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            RockMigrationHelper.DeleteAttribute( "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            RockMigrationHelper.DeleteAttribute( "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            RockMigrationHelper.DeleteAttribute( "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            RockMigrationHelper.DeleteAttribute( "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            RockMigrationHelper.DeleteAttribute( "0673E015-F8DD-4A52-B380-C758011331B2" );
            RockMigrationHelper.DeleteAttribute( "466993F7-D838-447A-97E7-8BBDA6A57289" );
            RockMigrationHelper.DeleteAttribute( "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            RockMigrationHelper.DeleteAttribute( "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            RockMigrationHelper.DeleteAttribute( "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            RockMigrationHelper.DeleteAttribute( "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            RockMigrationHelper.DeleteBlock( "D8472640-94A7-4763-B0CD-BF6691699D2B" );
            RockMigrationHelper.DeleteBlock( "2A6C7BED-26E6-4A00-A7CF-4E27095ADAE4" );
            RockMigrationHelper.DeleteBlock( "42105CB7-E593-4102-A259-9676F1DC899C" );
            RockMigrationHelper.DeleteBlock( "61EA9BD2-57E4-4897-A914-A158DE755641" );
            RockMigrationHelper.DeleteBlock( "FE043AE3-40D1-4AE4-A9BD-9D170FC067D4" );
            RockMigrationHelper.DeleteBlock( "B86F9EAB-1778-4947-8166-720C890C0391" );
            RockMigrationHelper.DeleteBlock( "C90FCC3A-1C80-4D70-8A52-2DA27C30EF7A" );
            RockMigrationHelper.DeleteBlock( "ED74AB11-6CBA-46C7-8BC0-CEF9CC0A3BB9" );
            RockMigrationHelper.DeleteBlockType( "A8BD05C8-6F89-4628-845B-059E686F089A" );
            RockMigrationHelper.DeleteBlockType( "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.DeletePage( "0C33AEC1-D0CC-4C7E-A0BF-1646CB83B490" );
            //  Page: Landing Pages Home Page

            // Attrib for Site: Header Image
            RockMigrationHelper.DeleteAttribute( "D520FAD6-585D-4F59-84F8-5918715320A9" );
            // Attrib for Site: Secondary Image
            RockMigrationHelper.DeleteAttribute( "24FE0736-D09E-48A3-AD42-7F002B9974AB" );

            Sql( @"DELETE FROM [Site] WHERE [Guid] = '11E449C7-CCC8-4C24-B280-FA5363C8F554'" );
        }
    }
}