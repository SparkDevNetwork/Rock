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

namespace com.lcbcchurch.Utility.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    class EasyReturn : Migration
    {
        public override void Up()
        {
            
            // Add Site Block: Easy Return | To Site: Rock RMS
            AddSiteBlock( true, null, null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Easy Return", "Header", "", "", 0, "14A1257F-C050-4A62-B0AB-FA240C9EB8AE" );
            
            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Ensure the HTML Content blockType is created and all the attributes it contains are set
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );

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
            ////////////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Fill out Block Attribute Values for the Easy Return block
            // Attrib Value for Block:Easy Return, Attribute:Context Parameter Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            // Attrib Value for Block:Easy Return, Attribute:Context Name Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );
            // Attrib Value for Block:Easy Return, Attribute:Enable Versioning Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Easy Return, Attribute:Require Approval Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Easy Return, Attribute:Cache Duration Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Easy Return, Attribute:Entity Type Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"" );
            // Attrib Value for Block:Easy Return, Attribute:Enabled Lava Commands Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Easy Return, Attribute:Start in Code Editor mode Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Easy Return, Attribute:Document Root Folder Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Easy Return, Attribute:Image Root Folder Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Easy Return, Attribute:User Specific Folders Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Easy Return, Attribute:Is Secondary Block Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Easy Return, Attribute:Cache Tags Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", "522C18A9-C727-42A5-A0BA-13C673E8C4B6", @"" );
            ////////////////////////////////////////////////////////////////////////////////////////////////

            // Add/Update HtmlContent for Block: Easy Return
            RockMigrationHelper.UpdateHtmlContentBlock( "14A1257F-C050-4A62-B0AB-FA240C9EB8AE", @"<style>
    #quickreturn {
        width:140px;
        margin-right:1.5em;
        color: initial;
        background-color: transparent;
    }
    #quickreturn b {
        padding-left:0.75em;
    }
</style>
{% assign lastperson = CurrentPerson | GetUserPreference:'lastperson' | PersonById %}
{% assign lastgroup = CurrentPerson | GetUserPreference:'lastgroup' | GroupById %}
{% assign lastdv = CurrentPerson | GetUserPreference:'lastdv' %}
{% assign lastreport = CurrentPerson | GetUserPreference:'lastreport' %}


<div class=""smartsearch"" id=""quickreturn"">
<ul class=""nav pull-right smartsearch-type"">
<li class=""dropdown""><a class=""dropdown-toggle navbar-link"" data-toggle=""dropdown"">Easy Return <b class=""fa fa-caret-down""></b></a>
<ul class=""dropdown-menu"">
{% if lastperson %}
<li><b>Person</b></li>
<li><a href=""/person/{{ lastperson.Id }}"">{{ lastperson.FullName }}</a></li>
{% endif %}
{% if lastgroup %}
<li><b>Group</b></li>
<li><a href=""/page/113?GroupId={{ lastgroup.Id }}"">{{ lastgroup.Name }}</a></li>
{% endif %}
{% if lastdv %}
<li><b>Data View</b></li>
<li>{% dataview id:'{{ lastdv }}' %}<a href=""/page/145?DataViewId={{ dataview.Id }}"">{{ dataview.Name }}{% enddataview %}</a></li>
{% endif %}
{% if lastreport %}
<li><b>Report</b></li>
<li>{% report id:'{{ lastreport }}' %}<a href=""/page/149?ReportId={{ report.Id }}"">{{ report.Name }}{% endreport %}</a></li>
{% endif %}
</ul>
</li>
</ul>
</div>", "FD7F54D6-33A8-45FC-9478-A08275CD30D4" );



            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Set all the Pre-HTML Lava needed to retain the last Person, Group, DataView, and Report

            // Person Bio
            Sql( @"
                UPDATE [Block]
                SET [PreHtml] = '{{ CurrentPerson | SetUserPreference:''lastperson'',PageParameter.PersonId }}'
                WHERE [Guid] = 'B5C1FDB6-0224-43E4-8E26-6B2EAF86253A'
" );

            // Group Detail
            Sql( @"
                UPDATE [Block]
                SET [PreHtml] = '{% if PageParameter.GroupId and PageParameter.GroupId <> 0 %}{{ CurrentPerson | SetUserPreference:''lastgroup'',PageParameter.GroupId }}{% endif %}'
                WHERE [Guid] = '88344FE3-737E-4741-A38D-D2D3A1653818'
" );

            // Data View Detail
            Sql( @"
                UPDATE [Block]
                SET [PreHtml] = '{% if PageParameter.DataViewId and PageParameter.DataViewId <> 0 %}{{ CurrentPerson | SetUserPreference:''lastdv'',PageParameter.DataViewId }}{% endif %}'
                WHERE [Guid] = '7868AF5C-6512-4F33-B127-93B159E08A56'
" );

            // Report Detail
            Sql( @"
                UPDATE [Block]
                SET [PreHtml] = '{% if PageParameter.ReportId and PageParameter.ReportId <> 0 %}{{ CurrentPerson | SetUserPreference:''lastreport'',PageParameter.ReportId }}{% endif %}'
                WHERE [Guid] = '1B7D7C5C-A201-4FFD-BCEC-762D126DFC2F'
" );
        }



        public void AddSiteBlock( bool skipIfAlreadyExists, string pageGuid, string layoutGuid, string siteGuid, string blockTypeGuid, string name,
            string zone, string preHtml, string postHtml, int order, string guid )
        {
            // Build SQL to do the insert into the Block table, only set SiteId (and Header Zone) to make this a Site Block
            //RockMigrationHelper.AddBlock(
            //public void AddBlock( bool skipIfAlreadyExists, string pageGuid, string layoutGuid, string blockTypeGuid, string name, string zone, string preHtml, string postHtml, int order, string guid )
            StringBuilder sb = new StringBuilder();
            sb.Append( @"
                DECLARE @PageId int
                SET @PageId = null

                DECLARE @LayoutId int
                SET @LayoutId = null

                DECLARE @SiteId int
                SET @SiteId = null
" );

            if ( !string.IsNullOrWhiteSpace( pageGuid ) )
            {
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );
            }

            if ( !string.IsNullOrWhiteSpace( layoutGuid ) )
            {
                sb.AppendFormat( @"
                SET @LayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{0}')
", layoutGuid );
            }

            if ( !string.IsNullOrWhiteSpace( siteGuid ) )
            {
                sb.AppendFormat( @"
                SET @SiteId = (Select [Id] FROM [Site] WHERE [Guid] = '{0}')
", siteGuid );
            }

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
                

                DECLARE @BlockId int
                INSERT INTO [Block] (
                    [IsSystem],[PageId],[LayoutId],[BlockTypeId],[Zone],
                    [Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
                    [Guid],[SiteId])
                VALUES(
                    1,@PageId,@LayoutId,@BlockTypeId,'{1}',
                    {2},'{3}','{4}','{5}',0,
                    '{6}',@SiteId)
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid, //{0}
                    //"19B61D65-37E3-459F-A44F-DEF0089118A3", // HTML Content BlockType {0} blockTypeGuid
                    //"C2D29296-6A87-47A9-A753-EE4E9159C4C4", // RockRMS SiteId {1}
                    zone, // {1}
                    order, // {2}
                    name.Replace( "'", "''" ), // {3}
                    preHtml.Replace( "'", "''" ), // {4}
                    postHtml.Replace( "'", "''" ), // {5}
                    guid ); // {6}

            // If adding a layout block, give edit/configuration authorization to admin role
            //if ( string.IsNullOrWhiteSpace( pageGuid ) )
            sb.Append( @"
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,2,NEWID())
" );

            string addBlockSQL = sb.ToString();

            if ( skipIfAlreadyExists )
            {
                addBlockSQL = $"if not exists (select * from [Block] where [Guid] = '{guid}') begin\n" + addBlockSQL + "\nend";
            }

            Sql( addBlockSQL );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
