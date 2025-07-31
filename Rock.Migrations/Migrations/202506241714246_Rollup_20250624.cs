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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250624 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopBlocksForV18Up();
            UpdateLavaChartShortcodeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateLavaChartShortcodeDown();
        }

        #region KH: Register block attributes for chop job in v18 (18.0.8)

        private void ChopBlocksForV18Up()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv18_0();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AIProviderDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AIProviderDetail", "AI Provider Detail", "Rock.Blocks.Core.AIProviderDetail, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "C2D6EC62-4076-43BC-A458-6DAA2C246B48" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AIProviderList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AIProviderList", "AI Provider List", "Rock.Blocks.Core.AIProviderList, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "17AE74AD-1234-4572-B3AD-E44742EE1C7B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AuditList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AuditList", "Audit List", "Rock.Blocks.Core.AuditList, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "8D4A9E56-30F1-4A2D-BD00-7803D7D51909" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignalTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignalTypeList", "Signal Type List", "Rock.Blocks.Core.SignalTypeList, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "2D9562D6-D28D-4515-8CA6-A2955E0ACE23" );

            // Add/Update Obsidian Block Type
            //   Name:AI Provider Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AIProviderDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Provider Detail", "Displays the details of a particular ai provider.", "Rock.Blocks.Core.AIProviderDetail", "Core", "13F49F94-D9BC-434A-BB20-A6BA87BBE81F" );

            // Add/Update Obsidian Block Type
            //   Name:AI Provider List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AIProviderList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Provider List", "Displays a list of ai providers.", "Rock.Blocks.Core.AIProviderList", "Core", "633A75A7-7186-4CFD-AB80-6F2237F0BDD8" );

            // Add/Update Obsidian Block Type
            //   Name:Audit List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AuditList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Audit List", "Displays a list of audits.", "Rock.Blocks.Core.AuditList", "Core", "120552E2-5C36-4220-9A73-FBBBD75B0964" );

            // Add/Update Obsidian Block Type
            //   Name:Signal Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignalTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signal Type List", "Displays a list of signal types.", "Rock.Blocks.Core.SignalTypeList", "Core", "770D3039-3F07-4D6F-A64E-C164ACCE93E1" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "633A75A7-7186-4CFD-AB80-6F2237F0BDD8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "41DCB2BB-C5D6-4EF1-B46E-DB579D0A5656" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "633A75A7-7186-4CFD-AB80-6F2237F0BDD8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8EEB1D18-E481-4CEE-AE9F-C5472ABA88C8" );

            // Attribute for BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "633A75A7-7186-4CFD-AB80-6F2237F0BDD8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the ai provider details.", 0, @"", "2C533C02-6B2A-4A89-A1C7-CBC9087D9E00" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "120552E2-5C36-4220-9A73-FBBBD75B0964", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9DBEF79B-1B79-4C4B-B8CA-3963ED00BABF" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "120552E2-5C36-4220-9A73-FBBBD75B0964", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7FE27268-DAA5-403C-A2EB-90B53CF2B2D4" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DD991CEC-C1C3-4FE8-86F2-E27296A76D80" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2A28086F-F2D2-49AB-93E6-0ED5CCF584AF" );

            // Attribute for BlockType
            //   BlockType: Signal Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "770D3039-3F07-4D6F-A64E-C164ACCE93E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signal type details.", 0, @"", "28E98675-BA87-4541-96F8-D58D557809C7" );
        }

        private void ChopBlockTypesv18_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 18.0 (18.0.8)",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v18.0.8
{ "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2", "770d3039-3f07-4d6f-a64e-c164acce93e1" }, // Signal Type List ( Core )
{ "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "633a75a7-7186-4cfd-ab80-6f2237f0bdd8" }, // AI Provider List ( Core )
{ "88820905-1B5A-4B82-8E56-F9A0736A0E98", "13f49f94-d9bc-434a-bb20-a6ba87bbe81f" }, // AI Provider Detail ( Core )
{ "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "120552e2-5c36-4220-9a73-fbbbd75b0964" }, // Audit List ( Core )
                    // blocks chopped in v18.0
{ "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "e44cac85-346f-41a4-884b-a6fb5fc64de1" }, // Page Short Link Click List ( CMS )
{ "4C4A46CD-1622-4642-A655-11585C5D3D31", "eddfcaff-70aa-4791-b051-6567b37518c4" }, // Achievement Type Detail ( Achievements )
{ "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "fbe75c18-7f71-4d23-a546-7a17cf944ba6" }, // Achievement Attempt Detail ( Engagement )
{ "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "b294c1b9-8368-422c-8054-9672c7f41477" }, // Achievement Attempt List ( Achievements )
{ "C26C7979-81C1-4A20-A167-35415CD7FED3", "09FD3746-48D1-4B94-AAA9-6896443AA43E" }, // Lava Shortcode List ( CMS )
{ "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "4acfbf3f-3d49-4ae3-b468-529f79da9898" }, // Achievement Type List ( Streaks )
{ "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" }, // Page Short Link List ( CMS )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> { } );
        }

        #endregion

        #region ME: Update Lava Chart Shortcode To Address Axis On Pie Charts and Tooltip Functionality

        private void UpdateLavaChartShortcodeUp()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}
{% assign tooltipvalueformat = valueformat %}
{% assign xvalueformat = ''none'' %}
{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- elseif type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- elseif type == ''horizontalBar'' %}
    {% assign xvalueformat = valueformat %}
    {% assign valueformat = ''none'' %}
{% endif %}
{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}
{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
    {
        backgroundColor: {{ backgroundColor }},
        borderWidth: {{ borderwidth }},
        gaugeData: {
            value: {{dataitems[0].value}},
            {% if dataitems[0].label != '''' %}
                label: ''{{dataitems[0].label}}'',
            {% endif %}
            valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
        },
        gaugeLimits: {{ gaugeLimits }}
    }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign tooltips = dataitems | Map:''tooltip'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' %}
      {% assign itemclickurls = dataitems | Select:''itemclickurl'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}
      {% capture seriesData -%}
      {
          fill: {{ filllinearea }},
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
          {% if firstDataItem.tooltip %}
          tooltips: [{{ tooltips }}],
          {% endif %}
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""'' -%}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign datasetPointHoverRadius = pointhoverradius %} {% endif -%}
          {%- capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ datasetFillLineArea }},
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ datasetPointHoverRadius }}'',
                  {%- if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{%- endif -%}
                  {%- if dataset.curvedlines and dataset.curvedlines == ''false'' -%} lineTension: 0,{%- endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}
          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}
<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>
<script>
    var options = {
    maintainAspectRatio: false,
    onClick: function(event, array) { 
        if (array.length > 0) {
            var index = array[0]._index;
            var redirectUrl = data.itemclickurl[index];
            // enable redirection only if a vaild itemclickurl is provided.
            if(data && data.itemclickurl && data.itemclickurl[index]) {
                window.location.href = data.itemclickurl[index];
            }
        }
    },
    hover: {
        onHover: function(event, array) {
            var target = event.target || event.srcElement;
            if (array.length > 0) {
                var index = array[0]._index;
                // enable redirection only if a vaild itemclickurl is provided.
                if(data && data.itemclickurl && data.itemclickurl[index]) {
                    var redirectUrl = data.itemclickurl[index];
                    target.style.cursor = ''pointer'';
                    return;
                }
            }
            target.style.cursor = ''default'';
        }
    },
    {%- if type != ''tsgauge'' -%}
        legend: {
            position: ''{{ legendposition }}'',
            display: {{ legendshow }}
        },
        tooltips: {
            enabled: {{ tooltipshow }}
            {% if tooltipshow %}
            , backgroundColor: ''{{ tooltipbackgroundcolor }}''
            , bodyFontColor: ''{{ tooltipfontcolor }}''
            , titleFontColor: ''{{ tooltipfontcolor }}''
                {% if tooltipvalueformat != '''' and tooltipvalueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% if type == ''pie'' %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% else %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% else %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% when ''number'' %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% endif %}
                    }
                }
                {% endif %}
            {% endif %}
        }
        {%- else -%}
        events: [],
        showMarkers: false
        {%- endif -%}
        {% if xaxistype == ''time'' %}
            ,scales: {
                xAxes: [{
                    type: ""time"",
                    display: {{ xaxisshow }},
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                    {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                    {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                    {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                    }
                }]
            }
        {% elseif xaxistype == ''linearhorizontal0to100'' %}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    ticks: {
                        min: 0,
                        max: 100
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    gridLines: {
                        display: false
                    }
                }]
            }
        {% elseif xaxistype == ''stacked'' %}
        {% if yaxislabels != '''' %}
            {%- assign yaxislabels = yaxislabels | Split:'','' -%}
            {%- assign yaxislabelcount = yaxislabels | Size -%}
        {% else %}
            {%- assign yaxislabelcount = 0 -%}
        {% endif %}
        ,scales: {
            xAxes: [{
                display: {{ xaxisshow }},
                stacked: true,
                {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                    , ticks: {
                    {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                    {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                    {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                    }
                {% endif %}
            }],
            yAxes: [{
                display: {{ yaxisshow }},
                stacked: true
                {%- if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
                , ticks: {
                {% if yaxismin != '''' %}min: {{ yaxismin }}{% endif %}
                {% if yaxismax != '''' %},max: {{ yaxismax }}{% endif %}
                {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                {% if yaxislabelcount > 0 %}
                    ,
                    callback: function(label, index, labels) {
                        switch (label) {
                            {%- for yaxislabel in yaxislabels -%}
                                {%- assign axislabel = yaxislabel | Split:''^'' -%}
                                case {{ axislabel[0] }}: return ''{{axislabel[1]}}'';
                            {%- endfor -%}
                        }
                    }
                {% endif %}
                },
        {% endif %}
            }]
    }
        {%- elseif type != ''pie'' and type != ''tsgauge'' -%}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                        ticks: {
                        {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                        {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                        {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                        {% if xvalueformat != '''' and xvalueformat != ''none'' %}
                            callback: function(label, index, labels) {
                                {% case xvalueformat %}
                                    {% when ''currency'' %}
                                        if (label % 1 === 0) {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                        } else {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                        }
                                    {% when ''percentage'' %}
                                        return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                    {% else %}
                                        return Intl.NumberFormat().format(label);
                                {% endcase %}
                            },
                        {% endif %}
                        }
                    {% endif %}
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    if (label % 1 === 0) {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                    } else {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                    }
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                        {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                        {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                        {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                        },
                }]
            }
        {% endif %}
    };
    {%- if type == ''tsgauge'' -%}
        var data = {
            datasets: [{{ seriesData }}]
        };
    {%- else -%}
        var data = {
            labels: [{{ labels }}],
            datasets: [{{ seriesData }}],
            borderWidth: {{ borderwidth }},
            {% if itemclickurls %}
                itemclickurl: {{ itemclickurls }},
            {% endif %}
        };
    {% endif %}
    Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
    Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";
    var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
    var chart = new Chart(ctx, {
        type: ''{{ type }}'',
        data: data,
        options: options
    });
</script>
'
WHERE [GUID] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        private void UpdateLavaChartShortcodeDown()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}
{% assign tooltipvalueformat = valueformat %}
{% assign xvalueformat = ''none'' %}
{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- elseif type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- elseif type == ''horizontalBar'' %}
    {% assign xvalueformat = valueformat %}
    {% assign valueformat = ''none'' %}
{% endif %}
{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}
{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
    {
        backgroundColor: {{ backgroundColor }},
        borderWidth: {{ borderwidth }},
        gaugeData: {
            value: {{dataitems[0].value}},
            {% if dataitems[0].label != '''' %}
                label: ''{{dataitems[0].label}}'',
            {% endif %}
            valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
        },
        gaugeLimits: {{ gaugeLimits }}
    }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign tooltips = dataitems | Map:''tooltip'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' %}
      {% assign itemclickurls = dataitems | Select:''itemclickurl'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}
      {% capture seriesData -%}
      {
          fill: {{ filllinearea }},
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
          {% if firstDataItem.tooltip %}
          tooltips: [{{ tooltips }}],
          {% endif %}
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""'' -%}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign datasetPointHoverRadius = pointhoverradius %} {% endif -%}
          {%- capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ datasetFillLineArea }},
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ datasetPointHoverRadius }}'',
                  {%- if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{%- endif -%}
                  {%- if dataset.curvedlines and dataset.curvedlines == ''false'' -%} lineTension: 0,{%- endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}
          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}
<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>
<script>
    var options = {
    maintainAspectRatio: false,
    onClick: function(event, array) { 
        if (array.length > 0) {
            var index = array[0]._index;
            var redirectUrl = data.itemclickurl[index];
            // enable redirection only if a vaild itemclickurl is provided.
            if(data && data.itemclickurl && data.itemclickurl[index]) {
                window.location.href = data.itemclickurl[index];
            }
        }
    },
    hover: {
        onHover: function(event, array) {
            var target = event.target || event.srcElement;
            if (array.length > 0) {
                var index = array[0]._index;
                var redirectUrl = data.itemclickurl[index];
                // enable redirection only if a vaild itemclickurl is provided.
                if(data && data.itemclickurl && data.itemclickurl[index]) {
                    target.style.cursor = ''pointer'';
                    return;
                }
            }
            target.style.cursor = ''default'';
        }
    },
    {%- if type != ''tsgauge'' -%}
        scales: {
            yAxes: [{
                ticks: {
                    beginAtZero: true
                }
            }]
        },
        legend: {
            position: ''{{ legendposition }}'',
            display: {{ legendshow }}
        },
        tooltips: {
            enabled: {{ tooltipshow }}
            {% if tooltipshow %}
            , backgroundColor: ''{{ tooltipbackgroundcolor }}''
            , bodyFontColor: ''{{ tooltipfontcolor }}''
            , titleFontColor: ''{{ tooltipfontcolor }}''
                {% if tooltipvalueformat != '''' and tooltipvalueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% if type == ''pie'' %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% else %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% else %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% when ''number'' %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% endif %}
                    }
                }
                {% endif %}
            {% endif %}
        }
        {%- else -%}
        events: [],
        showMarkers: false
        {%- endif -%}
        {% if xaxistype == ''time'' %}
            ,scales: {
                xAxes: [{
                    type: ""time"",
                    display: {{ xaxisshow }},
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                    {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                    {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                    {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                    }
                }]
            }
        {% elseif xaxistype == ''linearhorizontal0to100'' %}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    ticks: {
                        min: 0,
                        max: 100
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    gridLines: {
                        display: false
                    }
                }]
            }
        {% elseif xaxistype == ''stacked'' %}
        {% if yaxislabels != '''' %}
            {%- assign yaxislabels = yaxislabels | Split:'','' -%}
            {%- assign yaxislabelcount = yaxislabels | Size -%}
        {% else %}
            {%- assign yaxislabelcount = 0 -%}
        {% endif %}
        ,scales: {
            xAxes: [{
                display: {{ xaxisshow }},
                stacked: true,
                {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                    , ticks: {
                    {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                    {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                    {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                    }
                {% endif %}
            }],
            yAxes: [{
                display: {{ yaxisshow }},
                stacked: true
                {%- if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
                , ticks: {
                {% if yaxismin != '''' %}min: {{ yaxismin }}{% endif %}
                {% if yaxismax != '''' %},max: {{ yaxismax }}{% endif %}
                {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                {% if yaxislabelcount > 0 %}
                    ,
                    callback: function(label, index, labels) {
                        switch (label) {
                            {%- for yaxislabel in yaxislabels -%}
                                {%- assign axislabel = yaxislabel | Split:''^'' -%}
                                case {{ axislabel[0] }}: return ''{{axislabel[1]}}'';
                            {%- endfor -%}
                        }
                    }
                {% endif %}
                },
        {% endif %}
            }]
    }
        {%- elseif type != ''pie'' and type != ''tsgauge'' -%}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                        ticks: {
                        {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                        {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                        {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                        {% if xvalueformat != '''' and xvalueformat != ''none'' %}
                            callback: function(label, index, labels) {
                                {% case xvalueformat %}
                                    {% when ''currency'' %}
                                        if (label % 1 === 0) {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                        } else {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                        }
                                    {% when ''percentage'' %}
                                        return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                    {% else %}
                                        return Intl.NumberFormat().format(label);
                                {% endcase %}
                            },
                        {% endif %}
                        }
                    {% endif %}
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    if (label % 1 === 0) {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                    } else {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                    }
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                        {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                        {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                        {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                        },
                }]
            }
        {% endif %}
    };
    {%- if type == ''tsgauge'' -%}
        var data = {
            datasets: [{{ seriesData }}]
        };
    {%- else -%}
        var data = {
            labels: [{{ labels }}],
            datasets: [{{ seriesData }}],
            borderWidth: {{ borderwidth }},
            {% if itemclickurls %}
                itemclickurl: {{ itemclickurls }},
            {% endif %}
        };
    {% endif %}
    Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
    Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";
    var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
    var chart = new Chart(ctx, {
        type: ''{{ type }}'',
        data: data,
        options: options
    });
</script>
'
WHERE [GUID] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        #endregion
    }
}
