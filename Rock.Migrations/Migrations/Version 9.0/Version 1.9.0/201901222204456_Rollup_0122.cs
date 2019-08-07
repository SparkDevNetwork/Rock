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
    public partial class Rollup_0122 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixParallaxShortCodeUp();
            FixWordCloudShortCodeUp();
            AddColumn("dbo.FinancialBatch", "ControlItemCount", c => c.Int());
            // Attrib for BlockType: SMS Conversations:Show Conversations From Months Ago
            RockMigrationHelper.UpdateBlockTypeAttribute( "3497603B-3BE6-4262-B7E9-EC01FC7140EB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Show Conversations From Months Ago", "ShowConversationsFromMonthsAgo", "", @"Limits the conversations shown in the left pane to those of X months ago or newer. This does not affect the actual messages shown on the right.", 5, @"6", "85FD2026-67BB-452A-9A4E-0DDBAA51F30C" );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialBatch", "ControlItemCount");
            // Attrib for BlockType: SMS Conversations:Show Conversations From Months Ago
            RockMigrationHelper.DeleteAttribute( "85FD2026-67BB-452A-9A4E-0DDBAA51F30C" );
        }

        /// <summary>
        /// Fixes the word cloud short code CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4
        /// ED: Fix "Parallax Shortcode has Word Cloud Lava"
        /// </summary>
        private void FixWordCloudShortCodeUp()
        {
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
        }

        /// <summary>
        /// Fixes the parallax short code 4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4
        /// ED: Fix "Parallax Shortcode has Word Cloud Lava"
        /// </summary>
        private void FixParallaxShortCodeUp()
        {
            Sql( @"UPDATE [LavaShortCode]
SET [Markup] = '{{ ''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax.min.js'' | AddScriptLink }}
{% if videourl != '''' -%}
    {{ ''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax-video.min.js'' | AddScriptLink }}
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
{% endstylesheet %}'
WHERE [Guid] = '4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4'" );
        }
    }
}
