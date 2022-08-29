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
    public partial class Rollup_20220705 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChartShortcodeUpdate();
            RegisterBenevolenceTypeFieldType();
            AppleDevicesDefinedType();
            UpdateFamilyPreRegistrationBlock();
            LavaShortcodeCategory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: Chart Shortcode Update
        /// </summary>
        private void ChartShortcodeUpdate()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- endif -%}

{%- if type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- endif -%}

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
      {% assign firstDataItem = dataitems | First  %}

      {% capture seriesData -%}
      {
          label: ''{{ label }}'',
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
                {% if valueformat != '''' and valueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% case valueformat %}
                            {% when ''currency'' %}
                                return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(tooltipItem.yLabel);
                            {% else %}
                                return Intl.NumberFormat().format(tooltipItem.yLabel);
                        {% endcase %}
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
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                }],
                yAxes: [{
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: ''value''
                    }
                    , ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
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
                        ticks: {
                            min: 0,
                            max: 100
                        }
                }],
                yAxes: [{
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
                stacked: true,
            }],
            yAxes: [{
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
                yAxes: [{
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
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
            borderWidth: {{ borderwidth }}
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
</script>'
                WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        /// <summary>
        /// DL: Register Benevolence Type Field Type.
        /// </summary>
        private void RegisterBenevolenceTypeFieldType()
        {
            // Update FieldType Guid with direct SQL to allow back-porting to v13.4 where needed.
            Sql( @"
                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = 'Rock' AND [Class] = 'Rock.Field.Types.BenevolenceTypeFieldType')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [FieldType] (
                        [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                    VALUES(
                        'Benevolence Type',
                        'Select a Benevolence Type from a list of available values.',
                        'Rock',
                        'Rock.Field.Types.BenevolenceTypeFieldType',
                        'A9997462-1CE2-4A3D-B6DE-78FDFC4B755F',
                        1)
                END
                ELSE
                BEGIN
                    UPDATE [FieldType] SET
                        [Name] = 'Benevolence Type',
                        [Description] = 'Select a Benevolence Type from a list of available values.',
                        [Guid] = 'A9997462-1CE2-4A3D-B6DE-78FDFC4B755F',
                        [IsSystem] = 1
                    WHERE [Assembly] = 'Rock'
                    AND [Class] = 'Rock.Field.Types.BenevolenceTypeFieldType'
                END" );
        }

        /// <summary>
        /// KA: Migration to add AppleDevices DefinedType
        /// </summary>
        private void AppleDevicesDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Apple Device Models", "Apple device models.", SystemGuid.DefinedType.APPLE_DEVICE_MODELS );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "i386", "iPhone Simulator", "649110C3-6664-4F06-8E55-C44782F356FA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "x86_64", "iPhone Simulator", "AFDCAC1A-5897-4A55-84D7-DD3AC59FF3F6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "arm64", "iPhone Simulator", "4BAE0554-6A6B-49F3-AF19-7A2EC5A539C0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone1,1", "iPhone", "197D349D-9428-4FBC-9F32-68E9E2D081BB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone1,2", "iPhone 3G", "44D1B39F-A238-40F9-9C66-ACF2B7300BBF", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone2,1", "iPhone 3GS", "934A984D-EC16-4347-B952-11BF48D4AF3C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone3,1", "iPhone 4", "03F3558A-7242-4A18-9564-961995F48EB5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone3,2", "iPhone 4 GSM Rev A", "B118F7BA-778C-4BDB-A645-782331450FE6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone3,3", "iPhone 4 CDMA", "BEC3A911-D136-4979-8DDB-D0038286328A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone4,1", "iPhone 4S", "CD67AB72-9BD1-4DBB-9318-A5E587727494", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone5,1", "iPhone 5 (GSM)", "1A7138D7-4179-4569-A530-0B0414574DC2", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone5,2", "iPhone 5 (GSM+CDMA)", "78BBBE42-B196-4FC8-85FA-D6983BB94457", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone5,3", "iPhone 5C (GSM)", "C84470EC-F4AD-4A8C-97A4-56702CFEF58C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone5,4", "iPhone 5C (Global)", "8C783F2E-96FF-42C2-8747-19EA63A0E8A1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone6,1", "iPhone 5S (GSM)", "5F3A3E71-F69F-4A17-A5A3-AE2EAAF93F03", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone6,2", "iPhone 5S (Global)", "2A1E014D-370C-42FC-B906-94814159823D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone7,1", "iPhone 6 Plus", "63CDA68A-E7D5-4BD4-9D2A-4756701BBE1E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone7,2", "iPhone 6", "C0E8E2DB-90B1-4D68-ADB2-5C929CBCD331", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone8,1", "iPhone 6s", "DA8FD7B9-03DF-4DF5-8A02-1744961F760E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone8,2", "iPhone 6s Plus", "21926076-3FE3-4E67-A22D-879C1BFCBC0B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone8,4", "iPhone SE (GSM)", "39DD84AC-74AD-4CDF-B23A-D6EBEE1C49E0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone9,1", "iPhone 7", "22787E41-6623-4A37-B9DB-C8D1FA92CA09", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone9,2", "iPhone 7 Plus", "0D0DFA29-6ABC-4164-A1EE-FC4AC9629B09", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone9,3", "iPhone 7", "FBEBC7A0-B80D-47BF-B85F-335668268D59", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone9,4", "iPhone 7 Plus", "733236F0-1AD8-4A3D-B847-F9C0D31A5698", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,1", "iPhone 8", "38BEFDFA-B2F3-42B9-9C53-DF6EE27E2BDE", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,2", "iPhone 8 Plus", "4ED1FC82-D786-422B-8167-AA16DFA49EFB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,3", "iPhone X Global", "884ACB1C-2216-4A03-8904-EBFAACB86067", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,4", "iPhone 8", "479EB3A2-B954-4D01-AEDC-F0FDE80F1DC1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,5", "iPhone 8 Plus", "BA887138-855A-452C-9DD1-ABAC9479360F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone10,6", "iPhone X GSM", "C51A4972-83A3-4E0F-94DF-5FF7893F0C5E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone11,2", "iPhone XS", "4B49B365-CDCB-4216-B380-C64E25FD13F5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone11,4", "iPhone XS Max", "41DA475F-736F-43DF-8EA2-3D91A6457EB6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone11,6", "iPhone XS Max Global", "21681FD8-C6B1-4E82-BB47-DA19CD8BD5B9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone11,8", "iPhone XR", "D6EA258E-3866-4376-B3F0-D0653619D152", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone12,1", "iPhone 11", "36F5EA3D-FE4B-4605-906C-AC0213AACDDC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone12,3", "iPhone 11 Pro", "0047678D-1E99-4AF7-A79D-AC28D3182119", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone12,5", "iPhone 11 Pro Max", "7491DCBE-E588-421B-9E4A-3E9988E2C432", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone12,8", "iPhone SE 2nd Gen", "0230571F-1DA2-41B0-A05A-C58B322F081E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone13,1", "iPhone 12 Mini", "C8A90BE5-825E-42EF-AC1C-BD77F231C7E8", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone13,2", "iPhone 12", "8FFE7BBB-FB44-4BE6-8945-0E5852FA1D0C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone13,3", "iPhone 12 Pro", "8D27B3E0-7CF3-4A79-BB57-315D472DFD2E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone13,4", "iPhone 12 Pro Max", "416EA719-17A6-4E6C-A7E2-4B5EE569D46B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,2", "iPhone 13 Pro", "F61EC68B-3C2A-46A1-93C8-71C5EA9CED1D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,3", "iPhone Pro Max", "50655253-B65A-4CDA-8B82-77D547F0EBEA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,4", "iPhone 13 Mini", "3B292AF3-9560-4A1D-AE88-D0A481B44594", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,5", "iPhone 13", "A21C54B3-DE05-4DCE-BFEE-7AFD4549E32F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,6", "iPhone SE 3rd Gen", "CF717CAD-F4DE-41B8-B739-306C5180F756", true );
                                
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod1,1", "1st Gen iPod", "24904541-8AF4-4BEA-8152-A4CEF35ADEFC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod2,1", "2nd Gen iPod", "2406400F-FFED-4304-9221-1FD61B694A14", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod3,1", "3rd Gen iPod", "23471C11-7835-42C2-8B2D-B67BD93EDC35", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod4,1", "4th Gen iPod", "DBE6CCA2-8849-4A9F-A111-6447DB98BB48", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod5,1", "5th Gen iPod", "7C0D0473-5597-4655-8653-1D5FA68D91BA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod6,1", "6th Gen iPod", "22C1DA23-C126-4C2C-8142-DB313453151C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod7,1", "7th Gen iPod", "AF883B41-ED81-44C7-A69E-3D4BDD9BEF71", true );
                                                                                                                                    
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad1,1", "iPad", "461F12C4-B6F1-4E1B-A0C2-9D3B3D9F6DAB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad1,2", "iPad 3G", "A499030C-4BE1-4668-91C7-EA724EBF27EA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,1", "2nd Gen iPad", "43A8617A-7997-41B6-8C6E-159CC97E8965", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,2", "2nd Gen iPad GSM", "4562BDB3-A3D9-4F64-A9E2-AF6BF9537436", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,3", "2nd Gen iPad CDMA", "0347FDBD-C707-4444-AC11-98590AF82EE9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,4", "2nd Gen iPad New Revision", "60576A7D-2E11-4C46-BEAF-734EA1EA2810", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,1", "3rd Gen iPad", "439954CB-80F1-485C-98C9-38B2BAD08D8E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,2", "3rd Gen iPad CDMA", "A62E1B87-E2DC-4B84-8FE5-06FA72C9B409", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,3", "3rd Gen iPad GSM", "FD19081C-E715-4587-8920-729595E52468", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,5", "iPad mini", "10440F4E-3F44-49E3-BCAA-BF7004905318", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,6", "iPad mini GSM+LTE", "A03B92C5-9240-4902-83B9-070C2A894A09", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad2,7", "iPad mini CDMA+LTE", "85051012-078B-4774-91AE-BCACAF1D6B8B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,4", "4th Gen iPad", "15F42C70-1FEC-44A4-BC01-599565F941A5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,5", "4th Gen iPad GSM+LTE", "77BE49D2-3F9F-4F1C-8408-174461BC6A75", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad3,6", "4th Gen iPad CDMA+LTE", "8A9700FA-E9CC-4D2B-BD6E-84C7CD4D86F6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,1", "iPad Air (WiFi)", "538D5C5B-BC67-4414-A808-73A2DCF5F9B0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,2", "iPad Air (GSM+CDMA)", "6C143C40-24DF-4991-9C99-EA0D01FF6510", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,3", "1st Gen iPad Air (China)", "C20BDE5B-E52D-489D-B700-0A7F9284DB6F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,4", "iPad mini Retina (WiFi)", "ED960D00-E4AF-4B96-8D63-0BE216CC5BF3", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,5", "iPad mini Retina (GSM+CDMA)", "8C977AF4-8CAF-4DD1-964B-B6C985817211", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,6", "iPad mini Retina (China)", "D3F0F71A-9FB4-4406-8F14-A3C34267E1A9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,7", "iPad mini 3 (WiFi)", "65C18998-A02F-43B4-A5A9-04406F9E7345", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,8", "iPad mini 3 (GSM+CDMA)", "EC1B6F9E-DB84-4245-942F-9CD5C97FA927", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad4,9", "iPad Mini 3 (China)", "7C8A351B-4A6D-40E0-919C-ABC4F862F287", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad5,1", "iPad mini 4 (WiFi)", "148077CA-448A-4392-B242-8B1C7450FCDB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad5,2", "4th Gen iPad mini (WiFi+Cellular)", "EB94B0AE-7B8C-4A14-B571-163F3F81FD82", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad5,3", "iPad Air 2 (WiFi)", "2B92A779-08E6-42F9-AC1D-E7E16E8CD02D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad5,4", "iPad Air 2 (Cellular)", "315ACD50-9419-4216-A6D6-D2E0BEB52F1D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,3", "iPad Pro (9.7 inch, WiFi)", "95D7B4FD-497B-4100-895B-571489D21104", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,4", "iPad Pro (9.7 inch, WiFi+LTE)", "CDF1936C-65D6-4080-BC7E-887584A05A73", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,7", "iPad Pro (12.9 inch, WiFi)", "C57BFBFB-3795-4140-AA98-87443EED777F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,8", "iPad Pro (12.9 inch, WiFi+LTE)", "7C4039F2-41ED-4010-BC1D-773CE08FA13D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,11", "iPad (2017)", "97643E47-F1CD-4B2B-A272-EC5D85ECB704", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad6,12", "iPad (2017)", "60B0192D-0CF4-4192-8F27-528F849D1F13", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,1", "iPad Pro 2nd Gen (WiFi)", "350868EA-24D1-4F1E-A64A-22DA74F8F67A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,2", "iPad Pro 2nd Gen (WiFi+Cellular)", "5D544CC2-EED2-4E2E-88D6-5F3F72215497", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,3", "iPad Pro 10.5-inch 2nd Gen", "42912EB6-540B-4C91-9D58-385AA3518909", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,4", "iPad Pro 10.5-inch 2nd Gen", "D359723D-E8D1-41F2-BE93-B149D195860E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,5", "iPad 6th Gen (WiFi)", "0F931D3E-4A49-49A4-A288-3DB28F93336E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,6", "iPad 6th Gen (WiFi+Cellular)", "7D4D5642-4A25-404E-8DF3-052FCC94BE76", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,11", "iPad 7th Gen 10.2-inch (WiFi)", "59C4DB32-910F-4A22-8B71-9A34B4E7DC2D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad7,12", "iPad 7th Gen 10.2-inch (WiFi+Cellular)", "8DB86BF5-B6A9-4D8D-92B7-D1C3975460E6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,1", "iPad Pro 11 inch 3rd Gen (WiFi)", "F19EB89C-2836-4A23-B77F-7B248E8FEB49", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,2", "iPad Pro 11 inch 3rd Gen (1TB, WiFi)", "74BC60A5-D0A9-439E-9842-0BBB651B2D1E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,3", "iPad Pro 11 inch 3rd Gen (WiFi+Cellular)", "8255D050-1ADE-419F-90B7-BBE6B01C4707", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,4", "iPad Pro 11 inch 3rd Gen (1TB, WiFi+Cellular)", "FBCCBDBA-CD21-4F12-8A7C-7D4E76BEEC27", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,5", "iPad Pro 12.9 inch 3rd Gen (WiFi)", "52D6C197-8C4F-4084-9559-E523D92815D8", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,6", "iPad Pro 12.9 inch 3rd Gen (1TB, WiFi)", "3165D294-529F-4867-8C8C-7965FB07112B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,7", "iPad Pro 12.9 inch 3rd Gen (WiFi+Cellular)", "51A5D945-2CB9-43D7-9FC0-EF24E898D82E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,8", "iPad Pro 12.9 inch 3rd Gen (1TB, WiFi+Cellular)", "EAB9CBD4-3084-46F0-B8C4-59D75614BC39", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,9", "iPad Pro 11 inch 4th Gen (WiFi)", "B518D93C-41C7-4E7D-8B4C-09E41658F559", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,10", "iPad Pro 11 inch 4th Gen (WiFi+Cellular)", "6DEE3F6B-9039-4CF1-8814-34D6A89B3FC7", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,11", "iPad Pro 12.9 inch 4th Gen (WiFi)", "727D077E-66A9-4306-A32B-4E5EA1C4A2A8", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad8,12", "iPad Pro 12.9 inch 4th Gen (WiFi+Cellular)", "7C0E14F5-EB08-4033-B716-5FDD8A51228C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,1", "iPad mini 5th Gen (WiFi)", "9EE55000-151E-47F8-B018-58EE0924EF09", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,2", "iPad mini 5th Gen", "43566C8E-08F6-4FA0-85AD-4C76E8F78B1A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,3", "iPad Air 3rd Gen (WiFi)", "2B59427E-516B-4430-B972-0F4555BC26FE", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,4", "iPad Air 3rd Gen", "55E29AEF-9D67-48DA-94B5-73F375327876", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,6", "iPad 8th Gen (WiFi)", "4377F614-7C1F-4ABC-A87C-5BFDC7EE92B1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad11,7", "iPad 8th Gen (WiFi+Cellular)", "50AF2B05-3738-4188-AED4-F7C5F5167BDB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad12,1", "iPad 9th Gen (WiFi)", "673DC4BE-2AB8-45E1-9EEE-70FDE0EF1290", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad12,2", "iPad 9th Gen (WiFi+Cellular)", "7A0808B4-D09D-4182-83A1-79E6D74D2AE6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,1", "iPad mini 6th Gen (WiFi)", "32725560-DD9E-4E68-999C-223BA56552EF", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,2", "iPad mini 6th Gen (WiFi+Cellular)", "74FFF099-98FE-40B6-9651-21F887F502E1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,1", "iPad Air 4th Gen (WiFi)", "497D7881-0154-489D-9E14-8A0EB6C15145", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,2", "iPad Air 4th Gen (WiFi+Cellular)", "F1386ADB-89CA-4531-97E1-7FC9B2520F64", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,4", "iPad Pro 11 inch 5th Gen", "1C4B3A97-A6B3-4A3A-BE51-C3B1069C561C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,5", "iPad Pro 11 inch 5th Gen", "4BCFE654-1398-4303-AA1F-EFEE447D2313", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,6", "iPad Pro 11 inch 5th Gen", "32E15AF2-0AF5-4E15-8406-5AE803987F90", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,7", "iPad Pro 11 inch 5th Gen", "79526B7C-9DF6-4D49-931D-72898EEC7F8E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,8", "iPad Pro 12.9 inch 5th Gen", "52379922-8451-4D6D-B37A-F58DD55A50BA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,9", "iPad Pro 12.9 inch 5th Gen", "7F57B1E1-1D2E-4F45-8E2B-F490867EEFA1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,10", "iPad Pro 12.9 inch 5th Gen", "B3D207B0-BC1D-4ADD-81A1-2E5949498481", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,11", "iPad Pro 12.9 inch 5th Gen", "874D269E-7A54-4CA2-AF97-CA0AF0BADE5D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,16", "iPad Air 5th Gen (WiFi)", "C4D409A5-35B7-44B6-B9DB-EABA10B8F6EF", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,17", "iPad Air 5th Gen (WiFi+Cellular)", "DC791F3A-6943-46F5-8502-3AB22DCD92A4", true );
                                
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch1,1", "Apple Watch 38mm case", "BDE4DA51-7CCC-42B0-871F-EDEDF3A3EA0B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch1,2", "Apple Watch 42mm  case", "B05F49EC-2026-466C-9F4C-2779A57715AB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch2,6", "Apple Watch Series 1 38mm case", "10F34AD6-75D9-4364-A7D5-61BE61D334C9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch2,7", "Apple Watch Series 1 42mm case", "4D1E0CE7-7D81-4D8E-8432-D12872331A96", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch2,3", "Apple Watch Series 2 38mm case", "9731DE22-FE13-4EDD-85F2-4FFA3837A3F3", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch2,4", "Apple Watch Series 2 42mm case", "7BF2EAA0-4628-4746-8FCE-241FF6B7868B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch3,1", "Apple Watch Series 3 38mm case (GPS+Cellular)", "5D1BC43A-829D-4697-876D-1F8F902DA415", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch3,2", "Apple Watch Series 3 42mm case (GPS+Cellular)", "701376F1-8CBF-4B93-B37F-ED0F96046F27", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch3,3", "Apple Watch Series 3 38mm case (GPS)", "5E089C56-5EB8-4755-88E3-2DF7E1A6040C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch3,4", "Apple Watch Series 3 42mm case (GPS)", "BD5782B8-2549-4C9B-BDD7-4B7452DFAA9B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch4,1", "Apple Watch Series 4 40mm case (GPS)", "C82F795B-B94C-400D-8284-250C58EDA4F4", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch4,2", "Apple Watch Series 4 44mm case (GPS)", "45FE0477-FFA2-45BF-A129-E8D9CAF606B0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch4,3", "Apple Watch Series 4 40mm case (GPS+Cellular)", "A84624BA-B612-4954-85DA-2F1653C3AC47", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch4,4", "Apple Watch Series 4 44mm case (GPS+Cellular)", "F8E40B39-57A9-4C0E-B18D-493718DDAA9D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,1", "Apple Watch Series 5 40mm case (GPS)", "3E3075EF-8A07-4377-84C2-BB6C537BADF0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,2", "Apple Watch Series 5 44mm case (GPS)", "1D7F0BB3-4457-4461-990A-063BED082B13", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,3", "Apple Watch Series 5 40mm case (GPS+Cellular)", "84C912FE-ADB1-4838-A205-A0D09DAC8AB4", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,4", "Apple Watch Series 5 44mm case (GPS+Cellular)", "CCA10BB1-64A5-4B5B-AC9F-0EA95938AF72", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,9", "Apple Watch 40mm case (GPS)", "EFFF3BAD-58D6-40FF-A8EA-B66F31087CF5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,10", "Apple Watch 44mm case (GPS)", "1EEEFC4E-3BDD-4852-84D6-552B9B512C7A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,11", "Apple Watch 40mm case (GPS+Cellular)", "9F574260-D240-408D-94A7-E72BAD1ABE74", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,12", "Apple Watch 44mm case (GPS+Cellular)", "BED75931-A35A-49E6-BF66-E8F8FFAE3864", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,1", "Apple Watch 40mm case (GPS)", "072EB602-236A-4EF8-907D-38709873E463", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,2", "Apple Watch 44mm case (GPS)", "55961288-307F-40CB-A388-09C033F71E03", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,3", "Apple Watch 40mm case (GPS+Cellular)", "854BBB1E-F2B2-4224-92BB-7AD9296F4858", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,4", "Apple Watch 44mm case (GPS+Cellular)", "C66C9680-C2AF-4841-AA69-224054E8D0EC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,6", "Apple Watch 41mm case (GPS)", "82BDAEB1-E2CE-4A2E-ADCE-98EA1148277F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,7", "Apple Watch 45mm case (GPS)", "A2F46F18-FB52-4685-9953-E84E46873DB6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,8", "Apple Watch 41mm case (GPS+Cellular)", "38C5B568-63BE-486D-BCE4-19183DB15EDA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,9", "Apple Watch 45mm case (GPS+Cellular)", "8DCF48E6-00E4-446D-A4D7-30236D95AF92", true );

            Sql( @"DECLARE @AppleDeviceModelDefinedTypeId INT = (
                            SELECT TOP 1 [Id]
                            FROM [DefinedType]
                            WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C'
                            ) 

                       MERGE INTO PersonalDevice pd
                       USING DefinedValue dv
                       ON pd.Model = dv.Value AND dv.DefinedTypeId = @AppleDeviceModelDefinedTypeId
                       WHEN MATCHED THEN 
                       UPDATE SET [model] = dv.Description;" );
        }

        /// <summary>
        /// KA:Migration to Update FamilyPreRegistration block Domain.
        /// </summary>
        private void UpdateFamilyPreRegistrationBlock()
        {
            RockMigrationHelper.UpdateBlockType( "Family Pre Registration", "Provides a way to allow people to pre-register their families for weekend check-in.", "~/Blocks/Crm/FamilyPreRegistration.ascx", "CRM", "463A454A-6370-4B4A-BCA1-415F2D9B0CB7" );
        }

        /// <summary>
        /// ED: Rename the LavaShortcode category "Website" to "Web"
        /// </summary>
        private void LavaShortcodeCategory()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY, "Web", "", "", "C3270142-E72E-4FBF-BE94-9A2505DE7D54" );
        }
    }
}
