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
    public partial class Rollup_20250219 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAdaptiveMessagesUp();
            FixChartLavaShortcodeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateAdaptiveMessagesDown();
        }

        #region KH: Update Adaptive Message Adaptation Attributes

        private void UpdateAdaptiveMessagesUp()
        {
            Sql( @"
UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'The main action or next step you would like the individual to take. <span class=""tip tip-lava""></span>' WHERE [Guid] = '9E67B39B-C95C-464B-AC7B-CB191834EF85'

UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'A URL that supports the Call To Action.' WHERE [Guid] = 'FF9A62A6-3F19-458C-9527-9A3274652BCC'

UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'A brief description or highlight of the message.' WHERE [Guid] = '611C3F9D-67BA-496E-9533-2D07B0AD9733'

UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'Additional information or context about the message.' WHERE [Guid] = 'B55E8E55-1021-4A9E-8D05-EA9F52AA5C98'

UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'An image that represents or highlights the summary content.' WHERE [Guid] = 'A927602E-DC72-4CE5-B9B6-D6F77086AB6D'

UPDATE [dbo].[Attribute] SET [IsSystem] = 1, [Description] = 'An image that provides more context or enhances the detailed information.' WHERE [Guid] = '2144A2A2-6381-426F-9B5A-BCAFFF96A5D3'

UPDATE [dbo].[Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'FE12A90C-C20F-4F23-A1B1-528E0C5FDA83'" );
        }

        private void UpdateAdaptiveMessagesDown()
        {
            Sql( @"
UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = '9E67B39B-C95C-464B-AC7B-CB191834EF85'

UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = 'FF9A62A6-3F19-458C-9527-9A3274652BCC'

UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = '611C3F9D-67BA-496E-9533-2D07B0AD9733'

UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = 'B55E8E55-1021-4A9E-8D05-EA9F52AA5C98'

UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = 'A927602E-DC72-4CE5-B9B6-D6F77086AB6D'

UPDATE [dbo].[Attribute] SET [IsSystem] = 0, [Description] = '' WHERE [Guid] = '2144A2A2-6381-426F-9B5A-BCAFFF96A5D3'

UPDATE [dbo].[Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'FE12A90C-C20F-4F23-A1B1-528E0C5FDA83'" );
        }

        #endregion

        #region KA: Migration to remove y-axis configuration for Gauge chart types for Chart LavaShortcode.

        private void FixChartLavaShortcodeUp()
        {
            Sql( @"UPDATE [dbo].[LavaShortcode] 
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
