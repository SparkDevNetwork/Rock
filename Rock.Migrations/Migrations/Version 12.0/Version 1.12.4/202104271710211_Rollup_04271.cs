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
    public partial class Rollup_04271 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixGroupViewLavaTemplateDQ();
            UpLavaEngineSystemSettings();
            MediaInteractionsUp();
            BibleInteractionsUp();
            CreateTrendChartShortCode();
            AddMediaFeaturePageUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddMediaFeaturePageDown();
            DownLavaEngineSystemSettings();
        }

        /// <summary>
        /// NA: Fix GroupViewLavaTemplate broken Lava double single quotes.
        /// </summary>
        private void FixGroupViewLavaTemplateDQ()
        {
            // Replaces the double-single-quote ''warning'' with just a single quote 'warning' in the GroupType's GroupViewLavaTemplate
            Sql( @"
                UPDATE [GroupType]
                SET [GroupViewLavaTemplate] = REPLACE(GroupViewLavaTemplate,'''''warning''''','''warning''')
                WHERE [GroupViewLavaTemplate] like '%warningLevel = ''''warning''''%'" );
        }

        // These are const values in v13, keeping the names here to make it clear what they are referring to.
        private const string GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK = "9CBDD352-A4F5-47D6-9EFE-6115774B2DFE";
        private const string LAVA_ENGINE_LIQUID_FRAMEWORK = "core_LavaEngine_LiquidFramework";

        /// <summary>
        /// DL: Add Lava Engine Framework Setting
        /// </summary>
        private void UpLavaEngineSystemSettings()
        {
            RockMigrationHelper.AddGlobalAttribute(
                Rock.SystemGuid.FieldType.SINGLE_SELECT,
                string.Empty,
                string.Empty,
                "Lava Engine Liquid Framework",
                "Lava Engine Framework",
                @"The Liquid rendering framework used by the Lava Engine to parse and render templates. 'Default' ensures that the currently recommended framework is always used. Changes to this setting will not take effect until Rock is restarted.",
                0,
                "Default",
                GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK,
                LAVA_ENGINE_LIQUID_FRAMEWORK,
                false
            );

            RockMigrationHelper.AddAttributeQualifier( GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK, "fieldtype", "ddl", "B4C0B3BC-3416-402F-8874-CD5D33CEB5AD" );
            RockMigrationHelper.AddAttributeQualifier( GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK, "values", "Default^Default,RockLiquid^RockLiquid,DotLiquid^DotLiquid,Fluid^Fluid", "E11456FF-F57B-4964-877E-85468971C238" );
        }

        public void DownLavaEngineSystemSettings()
        {
            // This is a const value in v13, keeping the name here to make it clear what it is referring to.
            RockMigrationHelper.DeleteAttribute( GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK );
        }

        /// <summary>
        /// DH: Create Media Events Interactions Channel
        /// </summary>
        private void MediaInteractionsUp()
        {
            Sql( @"
                DECLARE @ChannelGuid UNIQUEIDENTIFIER = 'D5B9BDAF-6E52-40D5-8E74-4E23973DF159'

                IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
                BEGIN
                    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5919214F-9C59-4913-BE4E-0DFB6A05F528')
                    DECLARE @ComponentEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.MediaElement')

                    INSERT INTO [InteractionChannel]
	                    ([Name], [ComponentEntityTypeId], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	                    VALUES ('Media Events', @ComponentEntityTypeId, @ChannelTypeMediumValueId, @ChannelGuid, 0, 1)
                END" );
        }

        /// <summary>
        /// DH: Create Bible Events Interactions Channel
        /// </summary>
        private void BibleInteractionsUp()
        {
            Sql( @"
                DECLARE @ChannelGuid UNIQUEIDENTIFIER = '45b6c8cd-5016-4fcf-9aed-a7eecfe00c5e'

                IF NOT EXISTS( SELECT * FROM [InteractionChannel] WHERE [Guid] = @ChannelGuid )
                BEGIN
                    DECLARE @ChannelTypeMediumValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '5919214F-9C59-4913-BE4E-0DFB6A05F528')

                    INSERT INTO [InteractionChannel]
	                    ([Name], [ChannelTypeMediumValueId], [Guid], [UsesSession], [IsActive])
	                    VALUES ('Bible Events', @ChannelTypeMediumValueId, @ChannelGuid, 0, 1)
                END" );
        }

        /// <summary>
        /// Creates the trend chart short code.
        /// </summary>
        private void CreateTrendChartShortCode()
        {
            Sql( @"
                INSERT INTO [LavaShortcode] ([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid]) 
                VALUES (N'Trend Chart', N'Generate simple CSS based bar charts.', N'<p>Basic Usage:</p>
<pre><code>{[ trendchart ]}
    <span class=""hljs-string"">[[ dataitem label:''January'' value:''120'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
    <span class=""hljs-string"">[[ dataitem label:''February'' value:''45'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
    <span class=""hljs-string"">[[ dataitem label:''March'' value:''38'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
    <span class=""hljs-string"">[[ dataitem label:''April'' value:''34'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
    <span class=""hljs-string"">[[ dataitem label:''May'' value:''12'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
    <span class=""hljs-string"">[[ dataitem label:''June'' value:''100'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
{[ endtrendchart ]}
</code></pre>

<h4 id=""shortcode-options"">Shortcode Options</h4>
<ul>
<li><strong>minimumitems</strong> (0) - The minimum number of dataitems to show. If the number of dataitems provided is less than the minimumitems; the shortcode will create empty dataitems.</li>
<li><strong>maximumitems</strong> (auto) - The maximum number of dataitems to show.</li>
<li><strong>color</strong> - The default color of the dataitems, if no color is set the chart will use the theme''s default color for a trend chart.</li>
<li><strong>yaxismax</strong> (auto) - The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
<li><strong>reverseorder</strong> (false) - If true, the first dataitem will appear last. This is useful to have empty dataitems added using the minimumitems parameter.</li>
<li><strong>height</strong> (70px) - The height of the trend chart.</li>
<li><strong>width</strong> (100%) - The width of the trend chart.</li>
</ul>
<h4 id=""data-item-options"">Data Item Options</h4>
<p>Each ""bar"" on the trendchart is set using a <code>dataitem</code>.</p>
<pre><code><span class=""hljs-string"">[[ dataitem label:''January'' value:''120'' ]]</span> <span class=""hljs-string"">[[ enddataitem ]]</span>
</code></pre>
<ul>
<li><strong>label</strong> - The label for the data item.</li>
<li><strong>value</strong> - The numeric data point for the item.</li>
<li><strong>color</strong> - The color of the dataitem, which overrides the default value.</li>
</ul>
', 
                '1', '1', N'trendchart', N'{%- assign wrapperId = uniqueid -%}
{%- assign minimumitems = minimumitems | AsInteger -%}
{%- assign maximumitems = maximumitems | AsInteger -%}
{%- assign reverseorder = reverseorder | AsBoolean -%}
{% if yaxismax == ''auto'' %}
{%- assign yaxismax = null -%}
{% endif %}
{%- assign yaxismax = yaxismax | AsDecimal -%}

{% if color != '''' %}
<style>
    #trend-{{ wrapperId }} li span {
        background: {{ color }};
    }
</style>
{% endif %}

{% comment %} Count dataitems and the total number of items {% endcomment %}
{%- assign dataItemCount = dataitems | Size -%}
{%- assign totalItemCount = dataItemCount -%}


{% if minimumitems != null %}
{%- assign totalItemCount = dataItemCount | AtLeast:minimumitems -%}
{% endif %}

{% comment %} If maximumitems is not set, use the number of items {% endcomment %}
{% if maximumitems == null %}
{%- assign maximumitems = totalItemCount -%}
{% endif %}

{% comment %} If it''s not set, define the maximum yvalue {% endcomment %}
{% if yaxismax == null %}
    {%- assign yaxismax = -99999 -%}
    {% unless reverseorder %}
    {% for item in dataitems limit:maximumitems %}
        {% if item.value != null %}
            {%- assign yaxismax = item.value | AsDecimal | AtLeast:yaxismax -%}
        {% endif %}
    {% endfor %}
    {% else %}
        {% comment %} When reversed, use the offset to get the last items. {% endcomment %}
        {%- assign offset = dataItemCount | Minus:maximumitems | AtLeast:0 -%}
        {% for item in dataitems limit:maximumitems offset:offset reversed %}
            {%- assign yaxismax = item.value | AtLeast:yaxismax -%}
        {% endfor %}
    {% endunless %}
{% endif %}

{% comment %} Create Empty Items to append to chart, and capture them to append or prepend to the trend-chart {% endcomment %}
{%- assign emptytotalItemCount = totalItemCount | Minus:dataItemCount -%}
{%- capture emptyItems -%}
{% if emptytotalItemCount > 0 %}
{%- for i in (1..emptytotalItemCount) -%}
<li><span style=""height:0%""></span></li>
{%- endfor -%}
{% endif %}
{%- endcapture -%}

{% comment %} Create trend-chart, unless reverseorder then use  {% endcomment %}
<ul id=""trend-{{ wrapperId }}"" class=""trend-chart"" {% if height != '''' or width != '''' %}style=""{% if height != '''' %}height: {{ height }};{% endif %}{% if width != '''' %}width: {{ width }};{% endif %}""{% endif %}>
{% unless reverseorder %}
    {%- for item in dataitems limit:maximumitems -%}
    <li title=""{{ item.label }}""><span style=""height:{{ item.value | Default:''0'' | AsDecimal | DividedBy:yaxismax,4 | Times:100 }}%;{% if item.color != '''' and item.color != null %}background:{{ item.color }}{% endif %}""></span></li>
    {%- endfor -%}
    {{ emptyItems }}
{% else %}
    {%- assign offset = dataItemCount | Minus:maximumitems -%}
    {{ emptyItems }}
    {%- for item in dataitems limit:maximumitems offset:offset reversed -%}
    <li title=""{{ item.label }}""><span style=""height:{{ item.value | Default:''0'' | AsDecimal | DividedBy:yaxismax,4 | Times:100 }}%;{% if item.color != '''' and item.color != null %}background:{{ item.color }}{% endif %}""></span></li>
    {%- endfor -%}
{% endunless %}
</ul>
', 
                '2', N'', N'minimumitems^|maximumitems^|color^|yaxismax^|reverseorder^false|height^|width^', '52B27805-7C36-4965-90BD-3AA42D11F2DB');" );
        }

        /// <summary>
        /// SK: Add Media Feature Pages up.
        /// </summary>
        private void AddMediaFeaturePageUp()
        {
            // Add Page Media Accounts to Site:Rock RMS              
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Media Accounts", "", "07CB7BB5-1465-4E75-8DD4-28FA6EA48222", "fa fa-play-circle" );
            // Add Page Media Account to Site:Rock RMS              
            RockMigrationHelper.AddPage( true, "07CB7BB5-1465-4E75-8DD4-28FA6EA48222", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Media Account", "", "52548B49-6D09-467E-BEA9-04DD6F51637D", "" );
            // Add Page Media Folder to Site:Rock RMS              
            RockMigrationHelper.AddPage( true, "52548B49-6D09-467E-BEA9-04DD6F51637D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Media Folder", "", "65DE6218-2850-4924-AA55-6F6FB572E9A3", "" );
            // Add Page Media Element to Site:Rock RMS              
            RockMigrationHelper.AddPage( true, "65DE6218-2850-4924-AA55-6F6FB572E9A3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Media Element", "", "F1AB34EE-941F-41D6-9BA1-22348D09724C", "" );

            // Add Block Types
            //------------------------
            // Add/Update BlockType Media Account Detail              
            RockMigrationHelper.UpdateBlockType( "Media Account Detail", "Edit details of a Media Account", "~/Blocks/Cms/MediaAccountDetail.ascx", "CMS", "0361FFC9-F32F-4C97-98BD-9DFE5F4A777E" );
            // Add/Update BlockType Media Account List              
            RockMigrationHelper.UpdateBlockType( "Media Account List", "List Media Accounts", "~/Blocks/Cms/MediaAccountList.ascx", "CMS", "7537AB61-F80B-43B1-998B-1D2B03303B36" );
            // Add/Update BlockType Media Element List              
            RockMigrationHelper.UpdateBlockType( "Media Element List", "List Media Elements", "~/Blocks/Cms/MediaElementList.ascx", "CMS", "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554" );
            // Add/Update BlockType Media Folder Detail              
            RockMigrationHelper.UpdateBlockType( "Media Folder Detail", "Edit details of a Media Folder", "~/Blocks/Cms/MediaFolderDetail.ascx", "CMS", "3C9D442B-D066-43FA-9380-98C60936992E" );
            // Add/Update BlockType Media Folder List              
            RockMigrationHelper.UpdateBlockType( "Media Folder List", "List Media Folders", "~/Blocks/Cms/MediaFolderList.ascx", "CMS", "02A91579-9355-45E7-A67A-56E998FB332A" );
            // Add/Update BlockType Media Element Detail              
            RockMigrationHelper.UpdateBlockType( "Media Element Detail", "Edit details of a Media Element", "~/Blocks/Cms/MediaElementDetail.ascx", "CMS", "881DC0D1-FF98-4A5E-827F-49DD5CD0BD32" );

            // Add Blocks
            //------------------------
            // Add Block Media Account List to Page: Media Accounts, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "07CB7BB5-1465-4E75-8DD4-28FA6EA48222".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7537AB61-F80B-43B1-998B-1D2B03303B36".AsGuid(), "Media Account List", "Main", @"", @"", 0, "C38FB340-FD4B-4BDE-A306-FE9B75D71A85" );
            // Add Block Media Account Detail to Page: Media Account, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "52548B49-6D09-467E-BEA9-04DD6F51637D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0361FFC9-F32F-4C97-98BD-9DFE5F4A777E".AsGuid(), "Media Account Detail", "Main", @"", @"", 0, "ABAD84DA-113F-40E5-9DBD-ADA72F5B95B8" );
            // Add Block Media Folder List to Page: Media Account, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "52548B49-6D09-467E-BEA9-04DD6F51637D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "02A91579-9355-45E7-A67A-56E998FB332A".AsGuid(), "Media Folder List", "Main", @"", @"", 1, "14A0B30E-8287-4791-8443-0FAAB80FB559" );
            // Add Block Media Folder Detail to Page: Media Folder, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "65DE6218-2850-4924-AA55-6F6FB572E9A3".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3C9D442B-D066-43FA-9380-98C60936992E".AsGuid(), "Media Folder Detail", "Main", @"", @"", 0, "37108EB5-2F1F-484F-BD2D-FEF8AD6DFC18" );
            // Add Block Media Element List to Page: Media Folder, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "65DE6218-2850-4924-AA55-6F6FB572E9A3".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554".AsGuid(), "Media Element List", "Main", @"", @"", 1, "AA71BC08-DB91-43E3-BBB7-A03C698D1184" );
            // Add Block Media Element Detail to Page: Media Element, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "F1AB34EE-941F-41D6-9BA1-22348D09724C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "881DC0D1-FF98-4A5E-827F-49DD5CD0BD32".AsGuid(), "Media Element Detail", "Main", @"", @"", 0, "104BF960-5167-4ADE-A26B-6F63762877E3" );

            // Update Block Order
            //------------------------
            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Media Account,  Zone: Main,  Block: Media Account Detail              
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'ABAD84DA-113F-40E5-9DBD-ADA72F5B95B8'" );
            // Update Order for Page: Media Account,  Zone: Main,  Block: Media Folder List              
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '14A0B30E-8287-4791-8443-0FAAB80FB559'" );
            // Update Order for Page: Media Folder,  Zone: Main,  Block: Media Element List              
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'AA71BC08-DB91-43E3-BBB7-A03C698D1184'" );
            // Update Order for Page: Media Folder,  Zone: Main,  Block: Media Folder Detail              
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '37108EB5-2F1F-484F-BD2D-FEF8AD6DFC18'" );


            // Add Block Attributes
            //------------------------
            // Attribute for BlockType: Media Account List:core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7537AB61-F80B-43B1-998B-1D2B03303B36", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "33BC90A5-952B-4081-A25D-0CCA87E8426B" );
            // Attribute for BlockType: Media Account List:core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7537AB61-F80B-43B1-998B-1D2B03303B36", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FA22299B-8B86-4657-9AA2-D2C9B2C28921" );
            // Attribute for BlockType: Media Account List:Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7537AB61-F80B-43B1-998B-1D2B03303B36", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "D3C8A3CD-3E16-4244-BE6A-29C23662C065" );
            // Attribute for BlockType: Media Element List:Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "908960AB-4CFD-4DFD-A9B9-F60117DF4427" );
            // Attribute for BlockType: Media Element List:core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3A13C9B6-F02C-4D72-8D41-F1BE0218C141" );
            // Attribute for BlockType: Media Element List:core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "67EFDE42-8E98-438A-85E3-89249B84912A" );
            // Attribute for BlockType: Media Folder List:core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02A91579-9355-45E7-A67A-56E998FB332A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "AE584E4E-4179-4D5C-A60A-0AACB7029024" );
            // Attribute for BlockType: Media Folder List:core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02A91579-9355-45E7-A67A-56E998FB332A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "9950DA6D-C175-4E07-8988-7FF1E05DD02D" );
            // Attribute for BlockType: Media Folder List:Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02A91579-9355-45E7-A67A-56E998FB332A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "2B01D2A0-A67B-4BD9-B196-96CD9B649BAD" );

            // Add Block Attribute Values
            //------------------------
            // Add Block Attribute Value              
            //   Block: Media Account List              
            //   BlockType: Media Account List              
            //   Block Location: Page=Media Accounts, Site=Rock RMS              
            //   Attribute: Detail Page              
            //   Attribute Value: 52548b49-6d09-467e-bea9-04dd6f51637d              
            RockMigrationHelper.AddBlockAttributeValue( "C38FB340-FD4B-4BDE-A306-FE9B75D71A85", "D3C8A3CD-3E16-4244-BE6A-29C23662C065", @"52548b49-6d09-467e-bea9-04dd6f51637d" );
            // Add Block Attribute Value              
            //   Block: Media Account List              
            //   BlockType: Media Account List              
            //   Block Location: Page=Media Accounts, Site=Rock RMS              
            //   Attribute: core.CustomGridEnableStickyHeaders              
            //   Attribute Value: False              
            RockMigrationHelper.AddBlockAttributeValue( "C38FB340-FD4B-4BDE-A306-FE9B75D71A85", "13A63AF3-9FDF-46E4-9660-260F301CEEB3", @"False" );
            // Add Block Attribute Value              
            //   Block: Media Account List              
            //   BlockType: Media Account List              
            //   Block Location: Page=Media Accounts, Site=Rock RMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            //   Attribute Value: True              
            RockMigrationHelper.AddBlockAttributeValue( "C38FB340-FD4B-4BDE-A306-FE9B75D71A85", "FA22299B-8B86-4657-9AA2-D2C9B2C28921", @"True" );
            // Add Block Attribute Value              
            //   Block: Media Folder List              
            //   BlockType: Media Folder List              
            //   Block Location: Page=Media Account, Site=Rock RMS              
            //   Attribute: Detail Page              
            //   Attribute Value: 65de6218-2850-4924-aa55-6f6fb572e9a3              
            RockMigrationHelper.AddBlockAttributeValue( "14A0B30E-8287-4791-8443-0FAAB80FB559", "2B01D2A0-A67B-4BD9-B196-96CD9B649BAD", @"65de6218-2850-4924-aa55-6f6fb572e9a3" );
            // Add Block Attribute Value              
            //   Block: Media Folder List              
            //   BlockType: Media Folder List              
            //   Block Location: Page=Media Account, Site=Rock RMS              
            //   Attribute: core.CustomGridEnableStickyHeaders              
            //   Attribute Value: False              
            RockMigrationHelper.AddBlockAttributeValue( "14A0B30E-8287-4791-8443-0FAAB80FB559", "637D9872-722A-4D67-BED2-ED039C615D9F", @"False" );
            // Add Block Attribute Value              
            //   Block: Media Folder List              
            //   BlockType: Media Folder List              
            //   Block Location: Page=Media Account, Site=Rock RMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            //   Attribute Value: True              
            RockMigrationHelper.AddBlockAttributeValue( "14A0B30E-8287-4791-8443-0FAAB80FB559", "9950DA6D-C175-4E07-8988-7FF1E05DD02D", @"True" );
            // Add Block Attribute Value              
            //   Block: Media Element List              
            //   BlockType: Media Element List              
            //   Block Location: Page=Media Folder, Site=Rock RMS              
            //   Attribute: Detail Page              
            //   Attribute Value: f1ab34ee-941f-41d6-9ba1-22348d09724c              
            RockMigrationHelper.AddBlockAttributeValue( "AA71BC08-DB91-43E3-BBB7-A03C698D1184", "908960AB-4CFD-4DFD-A9B9-F60117DF4427", @"f1ab34ee-941f-41d6-9ba1-22348d09724c" );
            // Add Block Attribute Value              
            //   Block: Media Element List              
            //   BlockType: Media Element List              
            //   Block Location: Page=Media Folder, Site=Rock RMS              
            //   Attribute: core.CustomGridEnableStickyHeaders              
            //   Attribute Value: False              
            RockMigrationHelper.AddBlockAttributeValue( "AA71BC08-DB91-43E3-BBB7-A03C698D1184", "7EB0DC76-BD62-4ADE-8158-CB30F16B510E", @"False" );
            // Add Block Attribute Value              
            //   Block: Media Element List              
            //   BlockType: Media Element List              
            //   Block Location: Page=Media Folder, Site=Rock RMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            //   Attribute Value: True              
            RockMigrationHelper.AddBlockAttributeValue( "AA71BC08-DB91-43E3-BBB7-A03C698D1184", "3A13C9B6-F02C-4D72-8D41-F1BE0218C141", @"True" );

            Sql( @"
UPDATE
	[Page]
SET 
	[DisplayInNavWhen]=2
WHERE 
	[Guid]='07CB7BB5-1465-4E75-8DD4-28FA6EA48222'
" );
        }

        /// <summary>
        /// SK: Add Media Feature Pages down.
        /// </summary>
        private void AddMediaFeaturePageDown()
        {
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Media Folder List              
            RockMigrationHelper.DeleteAttribute( "9950DA6D-C175-4E07-8988-7FF1E05DD02D" );
            // core.CustomActionsConfigs Attribute for BlockType: Media Folder List              
            RockMigrationHelper.DeleteAttribute( "AE584E4E-4179-4D5C-A60A-0AACB7029024" );
            // Detail Page Attribute for BlockType: Media Folder List              
            RockMigrationHelper.DeleteAttribute( "2B01D2A0-A67B-4BD9-B196-96CD9B649BAD" );
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Media Element List              
            RockMigrationHelper.DeleteAttribute( "3A13C9B6-F02C-4D72-8D41-F1BE0218C141" );
            // core.CustomActionsConfigs Attribute for BlockType: Media Element List              
            RockMigrationHelper.DeleteAttribute( "67EFDE42-8E98-438A-85E3-89249B84912A" );
            // Detail Page Attribute for BlockType: Media Element List              
            RockMigrationHelper.DeleteAttribute( "908960AB-4CFD-4DFD-A9B9-F60117DF4427" );
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Media Account List              
            RockMigrationHelper.DeleteAttribute( "FA22299B-8B86-4657-9AA2-D2C9B2C28921" );
            // core.CustomActionsConfigs Attribute for BlockType: Media Account List              
            RockMigrationHelper.DeleteAttribute( "33BC90A5-952B-4081-A25D-0CCA87E8426B" );
            // Detail Page Attribute for BlockType: Media Account List              
            RockMigrationHelper.DeleteAttribute( "D3C8A3CD-3E16-4244-BE6A-29C23662C065" );

            // Remove Block: Media Element Detail, from Page: Media Element, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "104BF960-5167-4ADE-A26B-6F63762877E3" );
            // Remove Block: Media Element List, from Page: Media Folder, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "AA71BC08-DB91-43E3-BBB7-A03C698D1184" );
            // Remove Block: Media Folder Detail, from Page: Media Folder, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "37108EB5-2F1F-484F-BD2D-FEF8AD6DFC18" );
            // Remove Block: Media Folder List, from Page: Media Account, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "14A0B30E-8287-4791-8443-0FAAB80FB559" );
            // Remove Block: Media Account Detail, from Page: Media Account, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "ABAD84DA-113F-40E5-9DBD-ADA72F5B95B8" );
            // Remove Block: Media Account List, from Page: Media Accounts, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "C38FB340-FD4B-4BDE-A306-FE9B75D71A85" );

            // Delete BlockType Media Element Detail              
            RockMigrationHelper.DeleteBlockType( "881DC0D1-FF98-4A5E-827F-49DD5CD0BD32" ); // Media Element Detail  
            // Delete BlockType Media Folder List              
            RockMigrationHelper.DeleteBlockType( "02A91579-9355-45E7-A67A-56E998FB332A" ); // Media Folder List  
            // Delete BlockType Media Folder Detail              
            RockMigrationHelper.DeleteBlockType( "3C9D442B-D066-43FA-9380-98C60936992E" ); // Media Folder Detail  
            // Delete BlockType Media Element List              
            RockMigrationHelper.DeleteBlockType( "28D6F57B-59D9-4DA6-A8DC-6DBD3E157554" ); // Media Element List  
            // Delete BlockType Media Account List              
            RockMigrationHelper.DeleteBlockType( "7537AB61-F80B-43B1-998B-1D2B03303B36" ); // Media Account List  
            // Delete BlockType Media Account Detail              
            RockMigrationHelper.DeleteBlockType( "0361FFC9-F32F-4C97-98BD-9DFE5F4A777E" ); // Media Account Detail  

            // Delete Page Media Element from Site:Rock RMS              
            RockMigrationHelper.DeletePage( "F1AB34EE-941F-41D6-9BA1-22348D09724C" ); //  Page: Media Element, Layout: Full Width, Site: Rock RMS  
            // Delete Page Media Folder from Site:Rock RMS              
            RockMigrationHelper.DeletePage( "65DE6218-2850-4924-AA55-6F6FB572E9A3" ); //  Page: Media Folder, Layout: Full Width, Site: Rock RMS  
            // Delete Page Media Account from Site:Rock RMS              
            RockMigrationHelper.DeletePage( "52548B49-6D09-467E-BEA9-04DD6F51637D" ); //  Page: Media Account, Layout: Full Width, Site: Rock RMS  
            // Delete Page Media Accounts from Site:Rock RMS              
            RockMigrationHelper.DeletePage( "07CB7BB5-1465-4E75-8DD4-28FA6EA48222" ); //  Page: Media Accounts, Layout: Full Width, Site: Rock RMS  
        }
    }
}
