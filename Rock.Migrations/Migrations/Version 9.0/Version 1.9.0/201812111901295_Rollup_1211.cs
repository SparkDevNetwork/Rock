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
    public partial class Rollup_1211 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixChartShortcodeUp();
            FixContentComponentBadMigration();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "", @"If selected, all non-public prayer request will be excluded.", 7, @"False", "8108929A-18B5-4ACC-8E28-4F9DADC21B53" );
        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.DeleteAttribute( "8108929A-18B5-4ACC-8E28-4F9DADC21B53" );
        }

        /// <summary>
        /// GJ: Fix Chart Shortcode
        /// </summary>
        private void FixChartShortcodeUp()
        {
            Sql( MigrationSQL._201812111901295_Rollup_1211_FixChartShortcode );
        }

        /// <summary>
        /// GJ: Fix Content Component Bad Migration
        /// </summary>
        private void FixContentComponentBadMigration()
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
        }
    }
}
