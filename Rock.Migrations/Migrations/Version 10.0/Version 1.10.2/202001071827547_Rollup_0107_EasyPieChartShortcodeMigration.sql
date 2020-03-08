INSERT INTO [LavaShortcode] ([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
VALUES (N'Easy Pie Chart', N'Lightweight plugin to render simple, animated and retina optimized pie charts.', N'<p>Easy Pie Chart is the perfect solution when you need to display a single percentage value on a chart. In fact it''s as simple as <code>{[easypie value:''60'']}{[endeasypie]}</code></p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-main.png" alt="Easy Pie"></p>

<p>Each has the following basic settings settings:</p>
<ul>
<li><strong>value</strong> (0) - The data point for the item in percent (0 - 100)</li>
<li><strong>chartwidth</strong> (95px) - The width of the chart in pixels.</li>
</ul>
<p>Advanced options include:</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - The color of the circular bar.</li>
<li><strong>label</strong> - Optional label to display inside the chart.</li>
<li><strong>valuesize</strong> - Font size of the rendered percentage value.</li>
<li><strong>labelsize</strong> - Font size of the label.</li>
<li><strong>trackcolor</strong> (rgba(0,0,0,0.04)) - The CSS color of the track for the bar.</li>
<li><strong>scalelinelength</strong> (0px) - The length of the scale lines.</li>
<li><strong>scalelinecolor</strong> (#dfe0e0) - The CSS color of the scale lines.</li>
<li><strong>linecap</strong> (none) - Defines how the ending of the bar line looks like. (Possible values are none, round and square).</li>
<li><strong>trackwidth</strong>  - Width of the bar line in pixels. Default value is computed based on the chart width.</li>
<li><strong>animateduration</strong> (1500) - Time in milliseconds for a eased animation of the bar growing, or 0 to deactivate.</li>
</ul>
<h5>Advanced Options</h5>
<ul>
<li><strong>cssclasstarget</strong> - Set value to create many of the same type of chart.</li>
</ul>
<h3>Avatar</h3>
<p>Add a avatar to the center of the pie chart just by adding a image.</p>
<h3>Nested Charts</h3>
<p>Add an additional item named [[ easypie ]] using the same attributes</p>
<p>Example Charts and Markup is shown below:</p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-examples.png" alt="Easy Pie Examples"></p>
<pre><code>/* Small Chart (A) */
{[ easypie value:''25'' scalelinelength:''0'' chartwidth:''50'' ]} {[ endeasypie ]}

/* Standard Chart (B) */
{[ easypie value:''75'' ]} {[ endeasypie ]}

/* Chart With Scalelines and Color (C) */
{[easypie value:''90'' chartwidth:''120'' scalelinelength:''8'' primarycolor:''#16C98D'']}
{[ endeasypie]}

/* Chart Avatar (D) */
{[easypie value:''90'' chartwidth:''120'' primarycolor:''#D4442E'']}
&lt;img src="https://rock.rocksolidchurchdemo.com/GetImage.ashx?id=69" alt="Ted Decker"&gt;
{[ endeasypie]}

/* Chart with Labels (E) */
{[ easypie value:''90'' label:''Memory'' showpercent:''true'' primarycolor:''#FFC870'' chartwidth:''120'']} {[ endeasypie ]}

/* Nested Chart (F) */
{[easypie value:''90'' chartwidth:''120'' primarycolor:''#009CE3'']}
[[easypie value:''50'' primarycolor:''#16C98D'']][[endeasypie]]
{[ endeasypie]}
</code></pre>
', '1', '1', N'easypie', N'{% javascript url:''https://cdnjs.cloudflare.com/ajax/libs/easy-pie-chart/2.1.6/jquery.easypiechart.min.js'' id:''easypiechart''%}{% endjavascript %}
{% javascript id:''easypiechart-iterator'' %}
$( document ).ready(function() {
    $(".js-easy-pie-chart").each(function() {
        var e = $(this)
          , t = e.data("color") || e.css("color")
          , a = e.data("trackcolor") || "rgba(0,0,0,0.04)"
          , n = parseInt(e.data("piesize")) || 50
          , i = e.data("scalecolor")
          , r = parseInt(e.data("scalelinelength")) || 0
          , o = parseInt(e.data("trackwidth")) || parseInt(n / 8.5)
          , s = e.data("linecap") || "butt"
          , x = e.data("animateduration") || 1500;
        e.easyPieChart({
            size: n,
            barColor: t,
            trackColor: a,
            scaleColor: i,
            scaleLength: r,
            lineCap: s,
            lineWidth: o,
            animate: {
                duration: x,
                enabled: !0
            },
            onStep: function(e, t, a) {
                $(this.el).find(".js-percent").text(Math.round(a))
            }
        }),
        e = null
    })
});
{% endjavascript %}

{% stylesheet id:''easypiechart-main''%}
.easy-pie-chart {
  position: relative;
  display: -webkit-inline-box;
  display: -ms-inline-flexbox;
  display: inline-flex;
  -ms-flex-align: center;
  -ms-flex-pack: center;
  align-items: center;
  justify-content: center;
  -webkit-box-pack: center;
  -webkit-box-align: center;
  text-align: center;
}

.easy-pie-contents {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -ms-flex-align: center;
  -ms-flex-direction: column;
  flex-direction: column;
  -ms-flex-pack: center;
  align-items: center;
  justify-content: center;
  line-height: 1.2;
  -webkit-box-align: center;
  -webkit-box-pack: center;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
}

.easy-pie-contents .chart-label {
  opacity: .7;
}
{% endstylesheet %}

{%- assign id = uniqueid -%}
{%- assign showpercent = showpercent | AsBoolean %}

{%- assign value = value | Remove:''%'' %}
{%- assign chartwidth = chartwidth | Remove:''px'' %}
{%- assign trackwidth = trackwidth | Remove:''px'' %}
{%- assign scalelinelength = scalelinelength | Remove:''px'' %}
{%- assign padding = padding | Remove:''px'' | Times:2 %}
{%- if linecap == ''none'' -%}
  {%- assign linecap = ''butt'' -%}
{%- endif -%}

{%- assign comppadding = 4 %}
{%- if trackwidth == '''' -%}
  {%- assign trackwidth = chartwidth | DividedBy:8.5,0 -%}
{%- endif -%}

<div class="easy-pie-chart {{id}} {{cssclasstarget}} js-easy-pie-chart" data-percent="{{value}}" data-piesize="{{chartwidth}}" data-scalelinelength="{{scalelinelength}}" data-scalecolor={{scalelinecolor}} data-trackwidth="{{trackwidth}}" data-linecap="{{linecap}}" data-animateduration="{{animateduration}}" style="color:{{primarycolor}}">
  <div class="easy-pie-contents {{id}}-contents">
      {%- assign doubletrackwidth = trackwidth | Times:2 -%}
      {%- assign doublescalelength = scalelinelength | Times:2 -%}
      {%- assign maxwidth = chartwidth | Minus:doubletrackwidth | Minus:doublescalelength | Minus:padding -%}
      <style>
      .{{id}}-contents > img {
          border-radius: 50%;
          max-width: {{maxwidth}}px;
      }
      </style>
      {% assign childpies = easypies | Size %}
      {% if childpies > 0 %}
        {%- for item in easypies -%}
          {%- if item.chartwidth != '''' or item.chartwidth == null -%}
          {%- assign chartwidth = chartwidth | Minus:doubletrackwidth | Minus:doublescalelength | Minus:comppadding | Minus:padding -%}
          {% else %}
          {%- assign chartwidth = item.chartwidth %}
          {%- endif -%}
        
          {%- if item.primarycolor != '''' or item.primarycolor == null -%}
          {%- assign itemcolor = item.primarycolor -%}
          {% else %}
          {%- assign itemcolor = primarycolor %}
          {%- endif -%}
          
          {%- if item.trackwidth != '''' or item.trackwidth == null -%}
              {%- assign itemtrackwidth = chartwidth | DividedBy:8.5,0 -%}
          {%- else -%}
              {%- assign itemtrackwidth = item.trackwidth -%}
              {%- assign doublelinewidth = itemtrackwidth -%}
              {%- assign doublescalelength = 0 -%}
          {%- endif -%}
          
          <div class="easy-pie-chart easy-pie-contents js-easy-pie-chart pie-child-{{forloop.index}}" data-percent="{{item.value}}" data-piesize="{{chartwidth}}"  data-color="{{itemcolor}}" data-trackwidth="{{itemtrackwidth}}" data-trackcolor="{{item.trackcolor}}" data-linecap="{{linecap}}" data-animateduration="{{animateduration}}" style="color:{{itemcolor}}">
        {%- endfor -%}
      {% endif %}
      
        {%- if showpercent -%}
          <span class="js-percent value" {% if valuesize != '''' %}style="font-size:{{valuesize}}"{% endif %}></span>
        {%- endif -%}
        {%- if label != '''' %}<span class="chart-label small" {% if labelsize != '''' %}style="font-size:{{labelsize}}"{% endif %}>{{label}}</span>{% endif %}

        {{ blockContent }}

      {%- for item in easypies -%}
        </div>
      {%- endfor -%}
  </div>
</div>', '2', N'', N'value^0|showpercent^true|labelsize^|primarycolor^#ee7725|trackcolor^rgba(0%2C0%2C0%2C0.04)|linecap^none|primarycolor^#ee7625|scalelinelength^8|animateduration^1500|showpercent^false|linewidth^6|padding^6|scalelinecolor^#dfe0e0|cssclasstarget^|chartwidth^95px', '96A8284E-96A6-4E38-969C-640F0BDC8EB8');