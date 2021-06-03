UPDATE [LavaShortcode]
SET 
	  [Markup] = N'{% javascript url:''~/Scripts/sparkline/jquery-sparkline.min.js'' id:''sparkline''%}{% endjavascript %}


{%- comment -%}{%- assign type = '''' -%}{%- endcomment -%}

{% assign id = uniqueid %}
{%- assign renderChart = ''true'' | AsBoolean -%}
{%- if cssclasstarget == '''' -%}
  {%- assign class = ''sparkline-'' | Append:id | Prepend:''.'' -%}
  {%- assign data = data | Trim | Prepend:''['' | Append:'']'' -%}
{%- else -%}
  {%- assign data = ''"html"'' -%}
  {%- assign class = cssclasstarget | Trim -%}
  {%- assign renderChart = ''false'' | AsBoolean -%}
{%- endif -%}


{%- if class != '''' -%}
<span class="sparkline sparkline-{{ id }}">Loading...</span>
{%- endif -%}

{%- if fillcolor == '''' -%}
{%- assign fillcolor = primarycolor | Lighten:''25%'' -%}
{%- endif -%}

{%- assign linewidth = linewidth | Remove:''px'' -%}
{%- assign pointradius = pointradius | Remove:''px'' -%}
{%- assign barwidth = barwidth | Remove:''px'' -%}
{%- assign barspacing = barspacing | Remove:''px'' -%}

{%- capture chartJs -%}
  {%- if type == ''line'' -%}
  $("{{class}}").sparkline({{data}}, {
      type: ''line''
      , width: ''{{chartwidth}}''
      , height: ''{{chartheight}}''
      , lineColor: ''{{primarycolor}}''
      , fillColor: ''{{fillcolor}}''
      , lineWidth: {{linewidth}}
      , spotColor: ''{{lastpointcolor}}''
      , minSpotColor: ''{{minpointcolor}}''
      , maxSpotColor: ''{{maxpointcolor}}''
      , highlightSpotColor: ''{{highlightpointcolor}}''
      , highlightLineColor: ''{{highlightlinecolor}}''
      , spotRadius: {{pointradius}}
      , chartRangeMin: {{yaxisminvalue}}
      , chartRangeMax: {{yaxismaxvalue}}
      , chartRangeMinX: {{xaxisminvalue}}
      , chartRangeMaxX: {{xaxismaxvalue}}
      , normalRangeMin: {{normalminvalue}}
      , normalRangeMax: {{normalmaxvalue}}
      , normalRangeColor: ''{{normalrangecolor}}''
    });
  {% elseif type == ''bar'' %}
    {%- if stackedbarcolor != '''' -%}
      {%- assign stackedbarcolor = stackedbarcolor | Prepend:''['' | Append:'']'' %}
    {%- endif -%}

    $("{{class}}").sparkline({{data}}, {
      type: ''bar''
      , height: ''{{chartheight}}''
      , barWidth: {{barwidth}}
      , barSpacing: {{barspacing}}
      , barColor: ''{{primarycolor}}''
      {%- if negativecolor != ''undefined'' -%}, negBarColor: ''{{negativecolor}}''{%- endif -%}
      {%- if zerocolor != ''undefined'' -%}, zeroColor: ''{{zerocolor}}''{%- endif -%}
      {%- if nullcolor != ''undefined'' -%}, nullColor: ''{{nullcolor}}''{%- endif -%}
      , zeroAxis: {{zeroaxis}}
    });
  {%- elseif type == ''tristate'' -%}
    $("{{class}}").sparkline({{data}}, {
      type: ''tristate''
      , height: ''{{chartheight}}''
      , posBarColor: ''{{primarycolor}}''
      {%- if negativecolor != ''undefined'' -%}, negBarColor: ''{{negativecolor}}''{%- endif -%}
      {%- if zerocolor != ''undefined'' -%}, zeroBarColor: ''{{zerocolor}}''{%- endif -%}
      , barWidth: {{barwidth}}
      , barSpacing: {{barspacing}}
      , zeroAxis: {{zeroaxis}}
    });
  {%- elseif type == ''discrete'' -%}
    $("{{class}}").sparkline({{data}}, {
      type: ''discrete''
      , width: ''{{chartwidth}}''
      , height: ''{{chartheight}}''
      , lineColor: ''{{primarycolor}}''
      , lineHeight: ''{{lineheight}}''
      , thresholdValue: {{thresholdvalue}}
      {%- if thresholdcolor != ''undefined'' -%}, thresholdColor: ''{{thresholdcolor}}''{%- endif -%}
    });
  {% elseif type == ''bullet'' %}
    {%- if rangecolors != '''' -%}
      {%- assign rangecolors = rangecolors | Split:'','' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {% else %}
      {%- assign color = primarycolor | Lighten:''45%'' -%}
      {% for num in (1..3) %}
        {%- assign color = color | Darken:''10%'' -%}
        {%- assign rangecolors = rangecolors | Append:color | Append:'','' %}
      {% endfor %}
      {%- assign rangecolors = rangecolors | Split:'','' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {%- endif -%}

    $("{{class}}").sparkline({{data}}, {
      type: ''bullet''
      , height: ''{{chartheight}}''
      , width: ''{{chartwidth}}''
      , targetColor: ''{{targetcolor}}''
      , performanceColor: ''{{primarycolor}}''
      , targetWidth: {{targetwidth}}
      , rangeColors: {{rangecolors}}
    });
  {%- elseif type == ''pie'' -%}
    {%- assign colorCount = fillcolor | Split:'','' | Size -%}
    {%- assign slicecolors = '''' -%}
    {%- if colorCount > 1 -%}
      {%- assign slicecolors = fillcolor | Split:'','' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {%- endif -%}

    $("{{class}}").sparkline({{data}}, {
      type: ''pie''
      , width: ''{{chartwidth}}''
      , height: ''{{chartheight}}''
      {%- if slicecolors != '''' -%}, sliceColors: {{slicecolors}}{%- endif -%}
      , offset: {{offset}}
      , borderWidth: {{borderwidth}}
      , borderColor: ''{{bordercolor}}''
    });
  {%- elseif type == ''box'' -%}
    $("{{class}}").sparkline({{data}}, {
      type: ''box''
      , width: ''{{chartwidth}}''
      , height: ''{{chartheight}}''
      {%- if target != ''undefined'' -%}, target: ''{{target}}''{%- endif -%}
      {%- if yaxisminvalue != ''undefined'' -%}, chartRangeMin: {{yaxisminvalue}}{%- endif -%}
      {%- if yaxismaxvalue != ''undefined'' -%}, chartRangeMax: {{yaxismaxvalue}}{%- endif -%}
      , boxLineColor: ''{{primarycolor}}''
      , boxFillColor: ''{{fillcolor}}''
      , whiskerColor: ''{{whiskercolor}}''
      , showOutliers: {{showoutliers}}
      , outlierLineColor: ''{{outlierlinecolor}}''
      , outlierFillColor: ''{{outlierfillcolor}}''
      , medianColor: ''{{mediancolor}}''
      , spotRadius: {{pointradius}}
      {%- if target != ''undefined'' -%}, target: ''{{target}}''{%- endif -%}
      , targetColor: ''{{targetcolor}}''
      , chartRangeMin: {{yaxisminvalue}}
      , chartRangeMax: {{yaxismaxvalue}}
    });
  {%- endif -%}
{%- endcapture -%}

{%- if renderChart -%}
  <script>
  {{ chartJs }}
  </script>
{% else %}
  {%- javascript id:class -%}
  {{ chartJs }}
  {%- endjavascript -%}
{%- endif -%}'
    , [Parameters] = N'data^|type^bar|chartwidth^auto|chartheight^auto|primarycolor^#ee7625|minspot^auto|fillcolor^|linewidth^1|lastpointcolor^#f80|maxpointcolor^#f80|minpointcolor^#f80|pointrollovercolor^#5f5|linerollovercolor^#f22|lineheight^auto|pointradius^1.5|yaxisminvalue^undefined|yaxismaxvalue^undefined|xaxisminvalue^undefined|xaxismaxvalue^undefined|normalminvalue^undefined|normalmaxvalue^undefined|normalrangecolor^#ccc|barwidth^4|barspacing^1|negativecolor^#f44|zerocolor^undefined|nullcolor^undefined|zeroaxis^true|thresholdvalue^0|thresholdcolor^undefined|targetwidth^3|targetcolor^#33f|rangecolors^|offset^0|borderwidth^0|bordercolor^#fff|target^undefined|minvalue^|maxvalue^|whiskercolor^#000|showoutliers^true|outlierlinecolor^#333|outlierfillcolor^#fff|mediancolor^#f00|cssclasstarget^'
WHERE [Guid] = 'E7AC1E9B-0200-49AF-967F-0A9D2DD0F968'