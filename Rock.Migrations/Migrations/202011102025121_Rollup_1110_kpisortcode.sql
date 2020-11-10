INSERT INTO [LavaShortcode] ([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid]) VALUES (N'KPI', N'Create quick key performance indicators.', N'<p>Basic Usage:</p>
<pre><code>{[kpis]}
  [<span class="hljs-string">[ kpi icon:''fa-highlighter'' value:''4'' label:''Highlighters'' color:''yellow-700''</span>]][[ endkpi ]]
  [<span class="hljs-string">[ kpi icon:''fa-pen-fancy'' value:''8'' label:''Pens'' color:''indigo-700''</span>]][[ endkpi ]]
  [<span class="hljs-string">[ kpi icon:''fa-pencil-alt'' value:''15'' label:''Pencils'' color:''green-600''</span>]][[ endkpi ]]
{[endkpis]}
</code></pre><p>To Cache Use the Lava Tag:</p>
<pre><code>{[kpis]}
  {%- cache key:<span class="hljs-string">''kpi-group-name''</span> duration:<span class="hljs-string">''3600''</span> -%}
  <span class="hljs-string">[[ kpi icon:''fa-highlighter'' value:''16'' label:''Highlighters'']]</span><span class="hljs-string">[[ endkpi ]]</span>
  <span class="hljs-string">[[ kpi icon:''fa-pen-fancy'' value:''23'' label:''Pens'']]</span><span class="hljs-string">[[ endkpi ]]</span>
  <span class="hljs-string">[[ kpi icon:''fa-pencil-alt'' value:''42'' label:''Pencils'']]</span><span class="hljs-string">[[ endkpi ]]</span>
  {%- endcache -%}
{[endkpis]}
</code></pre><h4 id="kpi-options">KPI Options</h4>
<ul>
<li><strong>icon</strong> (blank) – The class of the <a href="https://fontawesome.com/icons?d=gallery">FontAwesome</a> icon. </li>
<li><strong>value</strong> (--) – The value of the KPI.</li>
<li><strong>label</strong> (blank) – The label for the KPI.</li>
<li><strong>description</strong> (blank) – An optional description, shown as tooltip on hover.</li>
<li><strong>color</strong> (blue-500) – The color of the icon from <a href="https://tailwindcss.com/docs/background-color#app">Tailwind</a>. Using the name of the <a href="#colors">color</a> and 100-900 or a CSS color (<code>#4ba9df</code>).</li>
<li><strong>mode</strong> (default) - The mode of the KPI. Options include <code>default</code> and <code>light</code>.</li>
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
', '1', '1', N'kpis', N'{%- stylesheet id:''kpi-shortcode'' -%}
  .kpi {
  display: flex;
  flex-grow: 1;
  flex-basis: 100%;
  margin: 0 8px 16px;
  background: #fff;
  border: 1px solid #000;
  border-color: currentColor;
  border-radius: 4px;
}

@media (min-width: 768px) {
  .kpi {
    max-width: calc(33.33333333% - 16px);
  }
}

@media (min-width: 992px) {
  .kpi {
    max-width: calc(25% - 16px);
  }
}

.kpi-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 60px;
  max-width: 35%;
  overflow: hidden;
  font-size: 32px;
  width: max-content;
}

.kpi-stat {
  align-self: center;
  padding: 8px 16px;
  max-width: 65%;
}

.kpi-value {
  display: block;
  font-size: 26px;
  font-weight: 700;
  line-height: 1;
}

.kpi-label {
  display: block;
  font-size: 14px;
  line-height: 1.1;
}

.kpi-icon {
  position: relative;
  display: inline-block;
  height: 100%;
  vertical-align: top;
}

.kpi-content {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.svg-placeholder {
  display: block;
  width: auto;
  height: 100%;
  background: currentColor;
  opacity: .2;
}

.kpi-light.kpi {
    padding: 16px;
}

.kpi-light .kpi-icon {
    display: flex;
    max-width: 36px;
    width: 36px;
    font-size: 18px;
    flex-grow: 1;
}

.kpi-light .kpi-stat {
    padding-right: 0;
}

.kpi-light .svg-placeholder {
  width: 36px;
  height: auto;
  border-radius: 4px;
}
{%- endstylesheet -%}


<div class="kpi-container d-flex flex-wrap" {% if height != '''' %}style="min-height: {{ height }};"{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon -%}
        {%- assign color = item.color | Default:''blue-500'' -%}
        {%- assign colorSplit = color | Split:''-'' -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Default:''--'' -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemMode = item.mode -%}

        <div class="kpi {{ itemMode }} {% if itemMode == ''light'' %}kpi-light {% endif %}{% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}" {% if colorSplitLength != 2 %}style="color:{{ color }};border-color:{{ color | FadeOut:''50%'' }}"{% endif %} {% if itemDescription != '''' %}data-toggle="tooltip" title="{{ itemDescription }}"{% endif %}>
            <div class="kpi-icon">
                <img class="svg-placeholder"
                src="data:image/svg+xml;utf8,&lt;svg xmlns=''http://www.w3.org/2000/svg'' viewBox=''0 0 1 1''&gt;&lt;/svg&gt;">
                <div class="kpi-content"><i class="{{ icontype }} {{ itemIcon }}"></i></div>
            </div>

            <div class="kpi-stat">
                <span class="kpi-value text-color">{{ itemValue }}</span>
                {% if itemLabel != '''' %}<span class="kpi-label text-muted">{{ itemLabel }}</span>{% endif %}
            </div>
        </div>
    {% endfor %}
</div>
', '2', N'', N'icontype^fa|height^', '8A49FD01-D59E-4611-8FF4-9E226C99FB22');
