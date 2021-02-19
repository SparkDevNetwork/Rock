UPDATE TOP(1) [LavaShortcode] SET [Description]=N'Create quick key performance indicators.', [Documentation]=N'<p>Basic Usage:</p>
<pre><code>{[kpis]}
  [<span class="hljs-string">[ kpi icon:''fa-highlighter'' value:''4'' label:''Highlighters'' color:''yellow-700''</span>]][[ endkpi ]]
  [<span class="hljs-string">[ kpi icon:''fa-pen-fancy'' value:''8'' label:''Pens'' color:''indigo-700''</span>]][[ endkpi ]]
  [<span class="hljs-string">[ kpi icon:''fa-pencil-alt'' value:''15'' label:''Pencils'' color:''green-600''</span>]][[ endkpi ]]
{[endkpis]}
</code></pre>
<h4 id="shortcode-options">Shortcode Options</h4>
<ul>
<li><strong>title</strong> (blank) – An optional main title for the group of KPIs. </li>
<li><strong>subtitle</strong> (blank) – The smaller body sized text to show under the title. Only shown if provided.</li>
<li><strong>showtitleseparator</strong> (true) – The label for the KPI.</li>
<li><strong>columncount</strong> (4) – The number of columns to display at the large responsive size. Will decrement by one for each subsequent breakpoint (lg: 4, md: 3, sm: 2, xs: 1).</li>
<li><strong>mode</strong> (default) - The mode of the KPI. Options include <code>default</code>, <code>card-light</code>, and <code>light</code>.

<ul>
<li><strong>default</strong> – Default card KPI apperance.</li>
<li><strong>card-light</strong> – Card, but the border is standard panel border color and the icon has no background color.</li>
<li><strong>light</strong> – Condensed KPI with squared icon.</li>
</ul>
</li>
</ul>
<h4 id="kpi-options">KPI Options</h4>
<ul>
<li><strong>icon</strong> (blank) – The class of the <a href="https://fontawesome.com/icons?d=gallery">FontAwesome</a> icon. </li>
<li><strong>value</strong> (--) – The value of the KPI.</li>
<li><strong>label</strong> (blank) – The label for the KPI.</li>
<li><strong>labellocation</strong> (bottom) – Options: <code>bottom</code>, <code>top</code> Determines if the label should be display above the value or below.</li>
<li><strong>secondarylabel</strong> (blank) – Smaller lighter label to show under the main label.</li>
<li><strong>description</strong> (blank) – An optional description, shown as tooltip on hover.</li>
<li><strong>color</strong> (blue-500) – The color of the icon from <a href="https://tailwindcss.com/docs/background-color#app">Tailwind</a>. Using the name of the <a href="#colors">color</a> and 100-900 or a CSS color (<code>#4ba9df</code>).</li>

</ul>
<h5 id="advanced-options">Advanced Options</h5>
<ul>
<li><strong>height</strong> (auto) – Optionally set a minimum height for the row of icons. (Example values <code>250px</code>, <code>50vh</code>)</li>
<li><strong>icontype</strong> (fa) – Optionally override the default FontAwesome icon class to allow using alternate style icons.</li>
</ul>
<h5 id="colors">Colors</h5>
<ul>
<li><code>gray</code></li>
<li><code>red</code></li>
<li><code>orange</code></li>
<li><code>yellow</code></li>
<li><code>green</code></li>
<li><code>teal</code></li>
<li><code>blue</code></li>
<li><code>indigo</code></li>
<li><code>purple</code></li>
<li><code>pink</code></li>
</ul>
', [IsSystem]='1', [IsActive]='1', [TagName]=N'kpis', [Markup]=N'{% if columncount != '''' %}
{%- assign columncountlg = columncount | AsInteger | AtLeast:1 -%}
{%- assign columncountmd = columncountlg | Minus:1 | AtLeast:1 -%}
{%- assign columncountsm = columncountlg | Minus:2 | AtLeast:1 -%}
{% endif %}
{%- assign showtitleseparator = showtitleseparator | AsBoolean -%}

{%- if title != '''' -%}<h3 id="{{ title | ToCssClass }}" class="kpi-title">{{ title }}</h3>{%- endif -%}
{%- if subtitle != '''' -%}<p class="kpi-subtitle">{{ subtitle }}</p>{%- endif -%} 
{% if title != '''' or subtitle != ''''  %}
{%- if showtitleseparator -%}<hr class="mt-3 mb-4">{%- endif -%}
{% endif %}

<div class="kpi-container" {% if columncount != '''' %}style="--kpi-col-lg: {{ columncountlg }};--kpi-col-md: {{ columncountmd }};--kpi-col-sm: {{ columncountsm }};"{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon -%}
        {%- assign color = item.color | Default:''blue-500'' -%}
        {%- assign colorSplit = color | Split:''-'' -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Default:''--'' -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemMode = item.mode -%}
        {%- assign itemSecondaryLabel = item.secondarylabel | Trim -%}
        {%- assign itemLabelBottom = true | AsBoolean -%}
        {%- assign itemLabelTop = false | AsBoolean -%}
        {% if item.labellocation == ''top'' %}
        {%- assign itemLabelBottom = false | AsBoolean -%}
        {%- assign itemLabelTop = true | AsBoolean -%}
        {% endif %}

        <div class="kpi {% if mode == ''light'' or itemMode == ''light'' %}kpi-light{% elseif mode == ''card-light'' or itemMode == ''card-light'' %}kpi-card-light{% endif %} {% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}" {% if height != '''' or colorSplitLength != 2 %}style="min-height: {{ height }};color:{{ color }};border-color:{{ color | FadeOut:''50%'' }}"{% endif %} {% if itemDescription != '''' %}data-toggle="tooltip" title="{{ itemDescription }}"{% endif %}>
            <div class="kpi-icon">
                <img class="svg-placeholder"
                src="data:image/svg+xml;utf8,&lt;svg xmlns=''http://www.w3.org/2000/svg'' viewBox=''0 0 1 1''&gt;&lt;/svg&gt;">
                <div class="kpi-content"><i class="{{ icontype }} {{ itemIcon }}"></i></div>
            </div>
            <div class="kpi-stat">
                {% if itemLabelTop and itemLabel != '''' %}<span class="kpi-label">{{ itemLabel }}</span>{% endif %}
                {% if itemLabelTop and itemSecondaryLabel != '''' %}<span class="kpi-secondary-label">{{ itemSecondaryLabel }}</span>{% endif %}
                <span class="kpi-value text-color">{{ itemValue }}</span>
                {% if itemLabelBottom and itemLabel != '''' %}<span class="kpi-label">{{ itemLabel }}</span>{% endif %}
                {% if itemLabelBottom and itemSecondaryLabel != '''' %}<span class="kpi-secondary-label">{{ itemSecondaryLabel }}</span>{% endif %}
            </div>
        </div>
    {% endfor %}
</div>
', [Parameters]=N'title^|subtitle^|columncount^|showtitleseparator^true|mode^|icontype^fa|height^' WHERE ([Guid]='8A49FD01-D59E-4611-8FF4-9E226C99FB22');
