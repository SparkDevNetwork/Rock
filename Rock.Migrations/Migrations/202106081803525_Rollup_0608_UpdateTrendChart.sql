UPDATE [LavaShortcode]
SET 
[Documentation]=N'<p>Basic Usage:</p>
<pre><code>{[ trendchart ]}
    <span>[[ dataitem label:''January'' value:''120'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''February'' value:''45'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''March'' value:''38'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''April'' value:''34'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''May'' value:''12'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''June'' value:''100'' ]]</span> <span>[[ enddataitem ]]</span>
{[ endtrendchart ]}
</code></pre>

<h4 id="shortcode-options">Shortcode Options</h4>
<ul>
<li><strong>minimumitems</strong> (0) - The minimum number of dataitems to show. If the number of dataitems provided is less than the minimumitems; the shortcode will create empty dataitems.</li>
<li><strong>maximumitems</strong> (auto) - The maximum number of dataitems to show.</li>
<li><strong>color</strong> - The default color of the dataitems, if no color is set the chart will use the theme''s default color for a trend chart.</li>
<li><strong>yaxismax</strong> (auto) - The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
<li><strong>reverseorder</strong> (false) - If true, the first dataitem will appear last. This is useful to have empty dataitems added using the minimumitems parameter.</li>
<li><strong>height</strong> (70px) - The height of the trend chart.</li>
<li><strong>width</strong> (100%) - The width of the trend chart.</li>
<li><strong>labeldelay</strong> (0) - The delay in milleseconds to show and hide the label tooltips.</li>
</ul>
<h4 id="data-item-options">Data Item Options</h4>
<p>Each "bar" on the trendchart is set using a <code>dataitem</code>.</p>
<pre><code><span>[[ dataitem label:''January'' value:''120'' ]]</span> <span>[[ enddataitem ]]</span>
</code></pre>
<ul>
<li><strong>label</strong> - The label for the data item.</li>
<li><strong>value</strong> - The numeric data point for the item.</li>
<li><strong>color</strong> - The color of the dataitem, which overrides the default value.</li>
</ul>',
	  [Markup] = N'{%- assign wrapperId = uniqueid -%}
{%- assign minimumitems = minimumitems | AsInteger -%}
{%- assign maximumitems = maximumitems | AsInteger -%}
{%- assign reverseorder = reverseorder | AsBoolean -%}
{% if yaxismax == ''auto'' %}
{%- assign yaxismax = null -%}
{% endif %}
{%- assign yaxismax = yaxismax | AsDecimal -%}

{% if color != '''' %}
<style>
    #trend-{{ wrapperId }} li span {
        background: {{ color }};
    }
</style>
{% endif %}

{% comment %} Count dataitems and the total number of items {% endcomment %}
{%- assign dataItemCount = dataitems | Size -%}
{%- assign totalItemCount = dataItemCount -%}


{% if minimumitems != null %}
{%- assign totalItemCount = dataItemCount | AtLeast:minimumitems -%}
{% endif %}

{% comment %} If maximumitems is not set, use the number of items {% endcomment %}
{% if maximumitems == null %}
{%- assign maximumitems = totalItemCount -%}
{% endif %}

{% comment %} If it''s not set, define the maximum yvalue {% endcomment %}
{% if yaxismax == null %}
    {%- assign yaxismax = -99999 -%}
    {% unless reverseorder %}
    {% for item in dataitems limit:maximumitems %}
        {% if item.value != null %}
            {%- assign yaxismax = item.value | AsDecimal | AtLeast:yaxismax -%}
        {% endif %}
    {% endfor %}
    {% else %}
        {% comment %} When reversed, use the offset to get the last items. {% endcomment %}
        {%- assign offset = dataItemCount | Minus:maximumitems | AtLeast:0 -%}
        {% for item in dataitems limit:maximumitems offset:offset reversed %}
            {%- assign yaxismax = item.value | AtLeast:yaxismax -%}
        {% endfor %}
    {% endunless %}
{% endif %}

{% comment %} Create Empty Items to append to chart, and capture them to append or prepend to the trend-chart {% endcomment %}
{%- assign emptytotalItemCount = totalItemCount | Minus:dataItemCount -%}
{%- capture emptyItems -%}
{% if emptytotalItemCount > 0 %}
{%- for i in (1..emptytotalItemCount) -%}
<li><span style="height:0%"></span></li>
{%- endfor -%}
{% endif %}
{%- endcapture -%}
 
{% comment %} Create trend-chart, unless reverseorder then use  {% endcomment %}
<ul id="trend-{{ wrapperId }}" class="trend-chart" {% if height != '''' or width != '''' %}style="{% if height != '''' %}height: {{ height }};{% endif %}{% if width != '''' %}width: {{ width }};{% endif %}"{% endif %}>
{% unless reverseorder %}
    {%- for item in dataitems limit:maximumitems -%}
    <li {% if item.label != '''' %} data-toggle="tooltip" title="{{ item.label }}"{% if labeldelay != ''0'' %} data-delay="{{ labeldelay }}"{% endif %}{% endif %}><span style="height:{{ item.value | Default:''0'' | AsDecimal | DividedBy:yaxismax,4 | Times:100 }}%;{% if item.color != '''' and item.color != null %}background:{{ item.color }}{% endif %}"></span></li>
    {%- endfor -%}
    {{ emptyItems }}
{% else %}
    {%- assign offset = dataItemCount | Minus:maximumitems -%}
    {{ emptyItems }}
    {%- for item in dataitems limit:maximumitems offset:offset reversed -%}
    <li {% if item.label != '''' %} data-toggle="tooltip" title="{{ item.label }}"{% if labeldelay != ''0'' %} data-delay="{{ labeldelay }}"{% endif %}{% endif %}><span style="height:{{ item.value | Default:''0'' | AsDecimal | DividedBy:yaxismax,4 | Times:100 }}%;{% if item.color != '''' and item.color != null %}background:{{ item.color }}{% endif %}"></span></li>
    {%- endfor -%}
{% endunless %}
</ul>'
    , [Parameters] = N'description^|minimumitems^|maximumitems^|color^|yaxismax^|reverseorder^false|height^|width^|labeldelay^0'
WHERE [Guid] = '52B27805-7C36-4965-90BD-3AA42D11F2DB'