UPDATE [LavaShortcode] SET [Parameters]=N'fillcolor^rgba(5,155,255,.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|fontcolor^#777|fontfamily^sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5,155,255,.6)|pointhoverbordercolor^rgba(5,155,255,.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear|yaxislabels^|yaxismin^|yaxismax^|yaxisstepsize^' WHERE ([Guid]='43819A34-4819-4507-8FEA-2E406B5474EA')

UPDATE [LavaShortcode] SET [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
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
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
  {
      backgroundColor: {{ backgroundColor }},
      borderWidth: {{ borderwidth }},
      gaugeData: {
        value: {{dataitems[0].value}},
        valueColor: "{{dataitems[0].fillcolor | Default:''#000000''}}"
      },
      gaugeLimits: {{ gaugeLimits }}
  }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
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
      {% assign labels = dataitems | Map:''label'' | Join:''", "'' | Prepend:''"'' | Append:''"'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class="alert alert-warning">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''"''- %}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''","'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''","'',''"'' %}
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

<div class="chart-container" style="position: relative; height:{{ chartheight }}; width:{{ chartwidth }}">
    <canvas id="chart-{{ id }}"></canvas>
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
        enabled: {{ tooltipshow }},
        backgroundColor: ''{{ tooltipbackgroundcolor }}'',
        bodyFontColor: ''{{ tooltipfontcolor }}'',
        titleFontColor: ''{{ tooltipfontcolor }}''
    }
    {%- else -%}
    events: [],
    showMarkers: false
    {%- endif -%}
    {% if xaxistype == ''time'' %}
        ,scales: {
        xAxes: [{
            type: "time",
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
    {% assign yaxislabels = yaxislabels | Split:'','' %}
    {% assign yaxislabelcount = yaxislabels | Size %}
    ,scales: {
		xAxes: [{
			stacked: true,
		}],
		yAxes: [{
			stacked: true
			{% if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' %}
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
    {%- elseif yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
,scales: {
		yAxes: [{
			ticks: {
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
Chart.defaults.global.defaultFontFamily = "{{ fontfamily }}";

var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
var chart = new Chart(ctx, {
    type: ''{{ type }}'',
    data: data,
    options: options
});    

</script>' WHERE ([Guid]='43819A34-4819-4507-8FEA-2E406B5474EA')