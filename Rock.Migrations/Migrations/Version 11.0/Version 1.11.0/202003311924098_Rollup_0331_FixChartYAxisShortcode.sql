UPDATE [LavaShortcode] SET [Description]=N'Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The chart shortcode allows anyone to create charts with just a few lines of Lava.', [Documentation]=N'<p>
    Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The 
    chart shortcode allows anyone to create charts with just a few lines of Lava. There are two modes for 
    creating a chart. The first ‘simple’ mode creates a chart with a single series. This option will suffice 
    for most of your charting needs. The second ‘series’ option allows you to create charts with multiple 
    series. Let’s look at each option separately starting with the simple option.
</p>

<h4>Simple Mode</h4>
<p>
    Let’s start by jumping to an example. We’ll then talk about the various configuration options, deal?
</p>

<pre>{[ chart type:''bar'' ]}
    [[ dataitem label:''Small Groups'' value:''45'' ]] [[ enddataitem ]]
    [[ dataitem label:''Serving Groups'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''General Groups'' value:''34'' ]] [[ enddataitem ]]
    [[ dataitem label:''Fundraising Groups'' value:''12'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
    
<p>    
    As you can see this sample provides a nice-looking bar chart. The shortcode defines the chart type (several other 
    options are available). The [[ dataitem ]] configuration item defines settings for each bar/point on the chart. Each 
    has the following settings:
</p>

<ul>
    <li><strong>label</strong> – The label for the data item.</li>
    <li><strong>value</strong> – The data point for the item.</li>
</ul>

<p>
    The chart itself has quite a few settings for you to consider. These include:
</p>

<ul>
    <li><strong>type</strong> (bar) – The type of chart to display. The valid options include: bar, stackedbar line, radar, pie, doughnut, polarArea (think radar meets pie).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderdash</strong> – This setting defines how the lines on the chart should be displayed. No value makes them display as solid lines. You can make interesting dot/dash patterns by providing an array of numbers representing lines and spaces. For instance, the setting of ‘[5, 5]'' would say draw a line of length 5px and then a space of 5px and repeat. You can provide as many numbers as you like to make more complex patterns (but isn''t that getting a little too fancy?)</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>legendposition</strong> (bottom) – This determines where the legend should be displayed.</li>
    <li><strong>legendshow</strong> (false) – Setting determines if the legend should be shown.</li>
    <li><strong>chartheight</strong> (400px) – The height of the chart must be set in pixels.</li>
    <li><strong>chartwidth</strong> (100%) – The width of the chart (can set as either a percentage or pixel size).</li>
    <li><strong>tooltipshow</strong> (true) – Determines if tooltips should be displayed when rolling over data items.</li>
    <li><strong>tooltipbackgroundcolor</strong> (#000) – The background color of the tooltip.</li>
    <li><strong>tooltipfontcolor</strong> (#fff) – The font color of the tooltip.</li>
    <li><strong>fontcolor</strong> (#777) – The font color to use on the chart.</li>
    <li><strong>fontfamily</strong> (sans-serif) – The font to use for the chart.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be.</li>
    <li><strong>pointcolor</strong> #059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
    <li><strong>curvedlines</strong> (true) – This determines if the lines should be straight between two points or beautifully curved. Based on this description you should be able to determine the default.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for data items. You can also provide a fill color for each item independently on the [[ dataitem ]] configuration. </li>
    <li><strong>label</strong> – The label to show for the single axis (not often needed in a single axis chart, but hey it''s there.)</li>
    <li><strong>xaxistype</strong> (linear) – The x-axis type. This is primarily used for time based charts. Valid values are ''linear'', ''time'' and ''linearhorizontal0to100''. The linearhorizontal0to100 option makes the horizontal axis scale from 0 to 100.</li>
    <li><strong>yaxismin</strong> (undefined) – The minimum number value of the y-axis. If no value is provided the min value is automatically calculated. To set a chart to always start from zero, rather than using a computed minimum, set the value to 0</li>
    <li><strong>yaxismax</strong> (undefined) – The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
    <li><strong>yaxisstepsize</strong> (undefined) – If set, the y-axis scale ticks are displayed by a multiple of the defined value. So a yaxisstepsize of 10 means one tick on 10, 20, 30, 40 etc. If no value is provided the step size is automatically computed.</li>
</ul>

<h5>Time Based Charts</h5>
<p>
    If the x-axis of your chart is date/time based you’ll want to set the ''xaxistype'' to ''time'' and provide
    the date in the label field.
</p>
<pre>{[ chart type:''line'' xaxistype:''time'' ]}
    [[ dataitem label:''1/1/2017'' value:''24'']] [[ enddataitem ]]
    [[ dataitem label:''2/1/2017'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''3/1/2017'' value:''42''  ]] [[ enddataitem ]]
    [[ dataitem label:''5/1/2017'' value:''23'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>

<p>
    That should be more than enough settings to get you started on the journey to chart success. But… what about 
    multiple series? Glad you asked…
</p>

<h4>Multiple Series</h4>
<p>
    It’s simple to add multiple series to your charts using the [[ dataset ]] configuration option. Each series is defined 
    by a [[ dataset ]] configuration block. Let’s again start with an example.
</p>

<pre>{[ chart type:''bar'' labels:''2015,2016,2017'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>

<div class="text-center">
    <img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/chart-series.jpg">
</div>

<p>
    If there is a trick to using series it’s understanding the organization of the data. In our example each [[ dataset ]] 
    is type of group. The data property of the dataset determine the number of groups for each year. The configuration of dataset 
    was created to help you write Lava to dynamically create your charts.
</p>

<p>
    Each of the dataset items have the following configuration options:
</p>

<ul>
    <li><strong>label</strong> – This is the descriptor of the dataset used for the legend.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for the data set. You should change this to help differentiate the series.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart.</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be. </li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
</ul>

<h5>Time Based Multi-Series Charts</h5>
<p>
    Like their single series brothers, multi-series charts can be line based to by setting
    the xseriestype = ''line'' and providing the dates in the ''label'' setting.
</p>
<pre>{[ chart type:''line'' labels:''1/1/2017,2/1/2017,6/1/2017'' xaxistype:''time'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
', [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
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
            {%- if yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
			, ticks: {
               {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
               {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
               {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
             }
            {%- endif -%}
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

</script>' WHERE [Guid] = '43819A34-4819-4507-8fEA-2E406b5474EA'
