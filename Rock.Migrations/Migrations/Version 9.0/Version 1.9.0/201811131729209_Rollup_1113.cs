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
    public partial class Rollup_1113 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            DisplayLavaAttributeOfContentComponentUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            DisplayLavaAttributeOfContentComponentDown();
        }

        /// <summary>
        /// Up migrations created by CodeGen_PagesBlocksAttributesMigration.sql
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Location Select:Sort By
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Sort By", "SortBy", "", @"", 13, @"0", "B07314FC-473B-4693-B36F-0250EAA06026" );
            // Attrib for BlockType: Success:Success Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Template", "SuccessTemplate", "", @"The Lava Template to use when rendering the Success Html", 8, @"
{% if RegistrationModeEnabled == true %}
    {{ DetailMessage }}
{% else %}
    {{ DetailMessage }}
{% endif %}
", "E947D760-8F0A-4395-A79F-92C20EAB07CC" );
            // Attrib for BlockType: Group Members:Show County
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show County", "ShowCounty", "", @"Should County be displayed when editing an address?.", 4, @"False", "93B16E1C-1E23-490F-8EBD-72F4CB8EC760" );
            // Attrib for BlockType: Edit Group:Show County
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show County", "ShowCounty", "", @"Should County be displayed when editing an address?", 7, @"False", "7B2B073E-97E3-44E7-B9B5-A8E97564195B" );
            // Attrib for BlockType: Captive Portal:New Person Record Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Person Record Type", "NewPersonRecordType", "", @"The person type to assign to new persons created by Captive Portal.", 9, @"36CF10D6-C695-413D-8E7C-4546EFEF385E", "CA71D3FB-BE5B-4235-AF28-351E8A7A5937" );
            // Attrib for BlockType: Captive Portal:New Person Record Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Person Record Status", "NewPersonRecordStatus", "", @"The record status to assign to new persons created by Captive Portal.", 10, @"618F906C-C33D-4FA3-8AEF-E58CB7B63F1E", "EFDAE2D7-F8EA-4778-A41A-E3DDE1815076" );
            // Attrib for BlockType: Captive Portal:New Person Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Person Connection Status", "NewPersonConnectionStatus", "", @"The connection status to assign to new persons created by Captive Portal", 11, @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "0660CB21-67A2-4A37-923B-365A471A3CE4" );
            // Attrib for BlockType: Content Channel Item View:Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Status", "Status", "", @"Include items with the following status.", 0, @"2", "2815CC17-5637-45E9-B1A6-86ED0A246DC8" );

        }

        /// <summary>
        /// Down migrations created by CodeGen_PagesBlocksAttributesMigration.sql
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Captive Portal:New Person Connection Status
            RockMigrationHelper.DeleteAttribute( "0660CB21-67A2-4A37-923B-365A471A3CE4" );
            // Attrib for BlockType: Captive Portal:New Person Record Status
            RockMigrationHelper.DeleteAttribute( "EFDAE2D7-F8EA-4778-A41A-E3DDE1815076" );
            // Attrib for BlockType: Captive Portal:New Person Record Type
            RockMigrationHelper.DeleteAttribute( "CA71D3FB-BE5B-4235-AF28-351E8A7A5937" );
            // Attrib for BlockType: Group Members:Show County
            RockMigrationHelper.DeleteAttribute( "93B16E1C-1E23-490F-8EBD-72F4CB8EC760" );
            // Attrib for BlockType: Edit Group:Show County
            RockMigrationHelper.DeleteAttribute( "7B2B073E-97E3-44E7-B9B5-A8E97564195B" );
            // Attrib for BlockType: Content Channel Item View:Status
            RockMigrationHelper.DeleteAttribute( "2815CC17-5637-45E9-B1A6-86ED0A246DC8" );
            // Attrib for BlockType: Success:Success Template
            RockMigrationHelper.DeleteAttribute( "E947D760-8F0A-4395-A79F-92C20EAB07CC" );
            // Attrib for BlockType: Location Select:Sort By
            RockMigrationHelper.DeleteAttribute( "B07314FC-473B-4693-B36F-0250EAA06026" );
        }

        /// <summary>
        /// SK: Update Display Lava Attribute of Content Component
        /// </summary>
        private void DisplayLavaAttributeOfContentComponentUp()
        {
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{%- assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' -%}
{%- assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment' -%}
{%- if channelContentAlignment == 'Right' -%}
{%- assign channelContentAlignment = 'Left' -%}
{%- endif -%}
{%- assign textAlignment = channelContentAlignment | Downcase | Prepend:'text-' -%}
{%- assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' -%}
{%- assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' -%}
{%- assign contentItemStyle = '' -%}

{% stylesheet id:'contentcomponent-hero' %}
.contentComponent-hero {
  margin-left: -15px;
  margin-right: -15px;
  padding: 120px 30px;
}

.bg-cover {
  background-repeat: no-repeat;
  background-size: cover;
}
{% endstylesheet %}

{%- for item in Items -%}
  {%- if channelBackgroundColor != '' -%}
      {%- capture contentItemStyle -%}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{%- endcapture -%}
  {%- endif -%}
  {%- if channelForegroundColor != '' -%}
      {%- capture contentItemStyle -%}{{ contentItemStyle }}color:{{ channelForegroundColor }};{%- endcapture -%}
  {%- endif -%}
  {%- assign imageGuid = item | Attribute:'Image','RawValue' -%}
  <section class=""contentComponent contentComponent-hero bg-cover"" style=""{%- if imageGuid != '' -%}background-image: url('/GetImage.ashx?Guid={{ imageGuid }}');{%- endif -%}{{ contentItemStyle }}"">
    <div class=""row"">
      <div class='col-md-6 {% if channelContentAlignment == 'Right' %}col-md-offset-6{% elseif channelContentAlignment == 'Center' %}col-md-offset-3{% endif %}'>
      
        <{{ channelTitleSize }} class=""{{ textAlignment }}"">{{ item.Title }}</{{ channelTitleSize }}>
      
        <div class=""{{ textAlignment }}"">{{ item.Content }}</div>
      </div>
    </div>
  </section>
{%- endfor -%}" );


            RockMigrationHelper.UpdateDefinedValueAttributeValue( "54A6FE8C-B38F-46DB-81F7-A7648886B592", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{%- assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' -%}
{%- assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment ' -%}
{%- assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' -%}
{%- assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' -%}
{%- assign channelBackgroundImage =  ContentChannel | Attribute:'BackgroundImage','RawValue' -%}
{%- assign contentItemStyle = '' -%}

{% stylesheet id:'contentcomponent-card' %}
.contentComponent-card .row {
  padding: 120px 0;
}

.bg-cover {
  background-repeat: no-repeat;
  background-size: cover;
}
{% endstylesheet %}

<section class=""contentComponent contentComponent-card"">
<div class=""row bg-cover"" style=""background-image:url('/GetImage.ashx?Guid={{ channelBackgroundImage }}');"">

    {%- for item in Items -%}
        {%- assign length = forloop.length -%}
        {%- case length -%}
          {%- when 1 -%}
          {%- assign cardWidth = 'col-md-6 col-md-offset-3 col-sm-10 col-sm-offset-1' -%}
          {% when 2 %}
          {%- assign cardWidth = 'col-md-6 col-sm-6' -%}
          {% when 3 %}
          {%- assign cardWidth = 'col-md-4 col-sm-6' -%}
          {%- else -%}
          {%- assign cardWidth = 'col-md-3 col-sm-6' -%}
        {%- endcase -%}  
        {%- if channelBackgroundColor != '' -%}
            {%- capture contentItemStyle -%}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{%- endcapture -%}
        {%- endif -%}
        {%- if channelForegroundColor != '' -%}
            {%- capture contentItemStyle -%}{{ contentItemStyle }}color:{{ channelForegroundColor }};{%- endcapture -%}
        {%- endif -%}
        {%- assign imageGuid = item | Attribute:'Image','RawValue' -%}
    
    
        <div class=""{{ cardWidth }}""><div class=""card panel"" {% if contentItemStyle != '' %}style='{{ contentItemStyle }}'{% endif %}>
            {%- if imageGuid != '' -%}
                <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}&w=400"" class=""card-img-top img-responsive"">
            {%- endif -%}
          <div class=""card-body panel-body"">
            <{{ channelTitleSize }} class=""card-title margin-all-none"">{{ item.Title }}</{{ channelTitleSize }}>
            {{ item.Content }}
          </div>
        </div></div>
    {%- endfor -%}
</div>
</section>" );



            RockMigrationHelper.UpdateDefinedValueAttributeValue( "EC429625-767E-4F69-BB48-F55DA3C836A3", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{%- assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' -%}
{%- assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment ' -%}
{%- assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' -%}
{%- assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' -%}
{%- assign contentItemStyle = '' -%}

{% stylesheet id:'contentcomponent-split' %}
.contentComponent-split .row {
    min-height: 450px;
    position: relative;
}

.contentComponent-split .cover-half {
    position: absolute;
    top: 0;
    left: 0;
    width: 50%;
    height: 100%;
    overflow: hidden;
}

.contentComponent-split .row:nth-of-type(even) .cover-half {
    left: auto;
    right: 0;
}

.contentComponent-split .cover-half img {
    min-width: 100%;
    width: auto;
    height: 100%;
    object-fit: cover;
    object-position: 50% 50%;
}
{% endstylesheet %}
<section class=""contentComponent contentComponent-split"">
{%- for item in Items -%}
    {%- if channelBackgroundColor != '' -%}
        {%- capture contentItemStyle -%}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{%- endcapture -%}
    {%- endif -%}
    {%- if channelForegroundColor != '' -%}
        {%- capture contentItemStyle -%}{{ contentItemStyle }}color:{{ channelForegroundColor }};{%- endcapture -%}
    {%- endif -%}
    {%- assign imageGuid = item | Attribute:'Image','RawValue' -%}

    <div class=""row"">
    
        {%- if imageGuid != '' -%}
        <div class=""cover-half visible-md-block visible-lg-block"">
          <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}"">
        </div>
        {%- endif -%}

        <div class=""col-md-6 {% cycle 'firstcol': '', 'col-md-push-6' %}"">
            <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}"" class=""img-responsive hidden-md hidden-lg"">
        </div>
        <div class=""col-md-6 {% cycle 'secondcol': '', 'col-md-pull-6' %}"">
            <div class='content-item' {% if contentItemStyle != '' %}style='{{ contentItemStyle }}'{% endif %}>
            
              <{{ channelTitleSize }}>{{ item.Title }}</{{ channelTitleSize }}>
            
              {{ item.Content }}
            </div>
        </div>
    
    </div>
{%- endfor -%}
</section>" );

            RockMigrationHelper.DeleteDefinedValue( "902D960C-0B7B-425E-9CEA-94CF215AABE4" ); // Ad Unit
        }

        /// <summary>
        /// Down migration for SK: Update Display Lava Attribute of Content Component
        /// </summary>
        private void DisplayLavaAttributeOfContentComponentDown()
        {
            RockMigrationHelper.UpdateDefinedValueAttributeValue( "3E7D4D0C-8238-4A5F-9E5F-34E4DFBF7725", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{% assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' %}
{% assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment ' %}
{% assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' %}
{% assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' %}
{% assign contentItemStyle = '' %}

{% for item in Items %}
    {% if channelBackgroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{% endcapture %}
    {% endif %}
    {% if channelForegroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}color:{{ channelForegroundColor }};{% endcapture %}
    {% endif %}

    <div class='content-item' style='{{ contentItemStyle }}'>
    
    <{{ channelTitleSize }}>{{ item.Title }}</{{ channelTitleSize }}>
    
    {{ item.Content }}
    
    {% assign imageGuid = item | Attribute:'Image','RawValue' %}
        {% if imageGuid != '' %}
            <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}"" class=""title-image img-responsive"">
        {% endif %}
    </div>
{% endfor %}" );

            RockMigrationHelper.UpdateDefinedValueAttributeValue( "54A6FE8C-B38F-46DB-81F7-A7648886B592", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{% assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' %}
{% assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment ' %}
{% assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' %}
{% assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' %}
{% assign contentItemStyle = '' %}

{% for item in Items %}
    {% if channelBackgroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{% endcapture %}
    {% endif %}
    {% if channelForegroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}color:{{ channelForegroundColor }};{% endcapture %}
    {% endif %}

    <div class='content-item' style='{{ contentItemStyle }}'>
    
    <{{ channelTitleSize }}>{{ item.Title }}</{{ channelTitleSize }}>
    
    {{ item.Content }}
    
    {% assign imageGuid = item | Attribute:'Image','RawValue' %}
        {% if imageGuid != '' %}
            <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}"" class=""title-image img-responsive"">
        {% endif %}
    </div>
{% endfor %}" );

            RockMigrationHelper.UpdateDefinedValueAttributeValue( "EC429625-767E-4F69-BB48-F55DA3C836A3", "FF5C0A7E-F3CD-46F0-934D-7C73B7CC35EE", @"{% assign channelTitleSize  =  ContentChannel | Attribute:'TitleSize' | Default:'h1' %}
{% assign channelContentAlignment  =  ContentChannel | Attribute:'ContentAlignment ' %}
{% assign channelForegroundColor  =  ContentChannel | Attribute:'ForegroundColor' %}
{% assign channelBackgroundColor =  ContentChannel | Attribute:'BackgroundColor' %}
{% assign contentItemStyle = '' %}

{% for item in Items %}
    {% if channelBackgroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}background-color:{{ channelBackgroundColor }};{% endcapture %}
    {% endif %}
    {% if channelForegroundColor != '' %}
        {% capture contentItemStyle %}{{ contentItemStyle }}color:{{ channelForegroundColor }};{% endcapture %}
    {% endif %}

    <div class='content-item' style='{{ contentItemStyle }}'>
    
    <{{ channelTitleSize }}>{{ item.Title }}</{{ channelTitleSize }}>
    
    {{ item.Content }}
    
    {% assign imageGuid = item | Attribute:'Image','RawValue' %}
        {% if imageGuid != '' %}
            <img alt=""{{ item.Title }}"" src=""/GetImage.ashx?Guid={{ imageGuid }}"" class=""title-image img-responsive"">
        {% endif %}
    </div>
{% endfor %}" );

            RockMigrationHelper.AddDefinedValue( "313B579F-F442-4247-ADBB-BBD25E255003", "Ad Unit", string.Empty, "902D960C-0B7B-425E-9CEA-94CF215AABE4", true );
        }

    }
}
