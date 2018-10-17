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
    public partial class Rollup_0925 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateInteractionForeignKeyIndex();
            FixShortcodeFunctionality();
            AddDefaultBackgroundCheckSystemSetting();
            ImproveFamilyAnalyticsJobPerf();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// MP: Update Interaction.ForeignKey Index
        /// </summary>
        private void UpdateInteractionForeignKeyIndex()
        {
            Sql( @"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV84DataMigrations'
   AND[Guid] = '79FBDA04-ADFD-40D4-824F-E07D660F7858'
  )
BEGIN
 INSERT INTO[ServiceJob](
  [IsSystem]
  ,[IsActive]
  ,[Name]
  ,[Description]
  ,[Class]
  ,[CronExpression]
  ,[NotificationStatus]
  ,[Guid]
  )
 VALUES(
  0
  ,1
  ,'Data Migrations for v8.4'
  ,'This job will take care of any data migrations that need to occur after updating to v8.4. After all the operations are done, this job will delete itself.'
  ,'Rock.Jobs.PostV84DataMigrations'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'79FBDA04-ADFD-40D4-824F-E07D660F7858'
  );
        END");
        }

        /// <summary>
        /// GJ: Fix Shortcode Functionality
        /// </summary>
        private void FixShortcodeFunctionality()
        {
                        // GJ: Fix Wordcloud Shortcode
            Sql( @"UPDATE [LavaShortCode] 
SET [Markup] = '{% javascript id:''d3-layout-cloud'' url:''~/Scripts/d3-cloud/d3.layout.cloud.js'' %}{% endjavascript %}
{% javascript id:''d3-min'' url:''~/Scripts/d3-cloud/d3.min.js'' %}{% endjavascript %}

<div id=""{{ uniqueid }}"" style=""width: {{ width }}; height: {{ height }};""></div>

{%- assign anglecount = anglecount | Trim -%}
{%- assign anglemin = anglemin | Trim -%}
{%- assign anglemax = anglemax | Trim -%}

{% javascript disableanonymousfunction:''true'' %}
    $( document ).ready(function() {
        Rock.controls.wordcloud.initialize({
            inputTextId: ''hf-{{ uniqueid }}'',
            visId: ''{{ uniqueid }}'',
            width: ''{{ width }}'',
            height: ''{{ height }}'',
            fontName: ''{{ fontname }}'',
            maxWords: {{ maxwords }},
            scaleName: ''{{ scalename }}'',
            spiralName: ''{{ spiralname}}'',
            colors: [ ''{{ colors | Replace:'','',""'',''"" }}''],
            {%- if anglecount != '''' %}
            anglecount: {{ anglecount }}{%- if anglemin != '''' or anglemax != '''' -%},{%- endif -%}
            {%- endif -%}
            {%- if anglemin != '''' %}
            anglemin: {{ anglemin }}{%- if anglemax != '''' -%},{%- endif -%}
            {%- endif -%}
            {%- if anglemax != '''' %}
            anglemax: {{ anglemax }}
            {%- endif -%}
        });
    });
{% endjavascript %}

<input type=""hidden"" id=""hf-{{ uniqueid }}"" value=""{{ blockContent }}"" />' WHERE [Guid] = 'CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4'" );

// GJ: Fix Parallax Shortcode
Sql( @"UPDATE [LavaShortCode] 
SET [Markup] = '{%- javascript url:''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax.min.js'' id:''jarallax-shortcode'' -%}{%- endjavascript -%}
{% if videourl != '''' -%}
    {%- javascript url:''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax-video.min.js'' id:''jarallax-video-shortcode'' -%}{%- endjavascript -%}
{% endif -%}

{% assign id = uniqueid -%} 
{% assign bodyZindex = zindex | Plus:1 -%}

{% assign speed = speed | AsInteger %}

{% if speed > 0 -%}
    {% assign speed = speed | Times:''.01'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% elseif speed == 0 -%}
    {% assign speed = 1 -%}
{% else -%}
    {% assign speed = speed | Times:''.02'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% endif -%}


 
{% if videourl != ''''- %}
    <div id=""{{ id }}"" class=""jarallax"" data-jarallax-video=""{{ videourl }}"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
{% else- %} 
    <div id=""{{ id }}"" data-jarallax class=""jarallax"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
        <img class=""jarallax-img"" src=""{{ image }}"" alt="""">
{% endif -%}

        {% if blockContent != '''' -%}
            <div class=""parallax-content"">
                {{ blockContent }}
            </div>
        {% else- %}
            {{ blockContent }}
        {% endif -%}
    </div>

{% stylesheet %}
#{{ id }} {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: {{ height }};
    background: transparent;
    position: relative;
    z-index: 0;
}

#{{ id }} .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: ''object-fit: cover;'';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#{{ id }} .parallax-content{
    display: inline-block;
    margin: {{ contentpadding }};
    color: {{ contentcolor }};
    text-align: {{ contentalign }};
	width: 100%;
}
{% endstylesheet %}' WHERE [Guid] = '4b6452ef-6fea-4a66-9fb9-1a7cce82e7a4'" );

        }

        /// <summary>
        /// NA: Add default background check provider system setting
        /// </summary>
        private void AddDefaultBackgroundCheckSystemSetting()
        {
            RockMigrationHelper.UpdateSystemSetting( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, "" );
        }

        /// <summary>
        /// MP: Improve performance of Family Analytics Job
        /// </summary>
        private void ImproveFamilyAnalyticsJobPerf()
        {
            Sql( MigrationSQL._201809252117562_Rollup_0925_spCrm_FamilyAnalyticsEraDataset );
        }

    }
}
