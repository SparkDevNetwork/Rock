UPDATE [LavaShortcode] SET [Markup]=N'{%- if columncount != '''' -%}
{%- assign columncountlg = columncount | AsInteger | AtLeast:1 -%}
{%- if columncountmd == '''' -%}{%- assign columncountmd = columncountlg | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- if columncountsm == '''' -%}{%- assign columncountsm = columncountmd | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- endif -%}
{%- assign showtitleseparator = showtitleseparator | AsBoolean -%}

{%- if title != '''' -%}<h3 id="{{ title | ToCssClass }}" class="kpi-title">{{ title }}</h3>{%- endif -%}
{%- if subtitle != '''' -%}<p class="kpi-subtitle">{{ subtitle }}</p>{%- endif -%}
{% if title != '''' or subtitle != ''''  %}
{%- if showtitleseparator -%}<hr class="mt-3 mb-4">{%- endif -%}
{% endif %}

{%- assign iconbackground = iconbackground | AsBoolean -%}

{%- assign kpisize = '''' -%}
{%- if size == ''sm'' -%}
{%- assign kpisize = ''kpi-sm'' -%}
{%- elseif size == ''lg'' -%}
{%- assign kpisize = ''kpi-lg'' -%}
{%- elseif size == ''xl'' -%}
{%- assign kpisize = ''kpi-xl'' -%}
{%- endif -%}

<div class="kpi-container" {% if columncount != '''' %}style="--kpi-col-lg:{{ 100 | DividedBy:columncountlg,4 }}%;--kpi-col-md:{{ 100 | DividedBy:columncountmd,4 }}%;--kpi-col-sm:{{ 100 | DividedBy:columncountsm,4 }}%;{% if columnmin != '''' %}--kpi-min-width:{{ columnmin }};{% endif %} {{ cssstyle }}"{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon | Trim -%}
        {%- assign color = item.color | Trim -%}
        {%- assign colorSplit = color | Split:''-'' -%}
        {%- assign height = item.height | Trim -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Trim | Default:''--'' -%}
        {%- assign itemSubValue = item.subvalue | Trim -%}
        {%- if itemSubValue != '''' -%}
            {%- assign itemSubValueColor = item.subvaluecolor | Trim -%}
            {%- assign subvalueColorSplit = itemSubValueColor | Split:''-'' -%}
            {%- assign subvalueSplitLength = subvalueColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemSecondaryLabel = item.secondarylabel | Trim -%}
        {%- if itemSecondaryLabel != '''' -%}
            {%- assign itemSecondaryLabelColor = item.secondarylabelcolor | Default:'''' | Trim -%}
            {%- assign secondaryColorSplit = itemSecondaryLabelColor | Split:''-'' -%}
            {%- assign secondarySplitLength = secondaryColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabelBottom = true | AsBoolean -%}
        {%- assign itemLabelTop = false | AsBoolean -%}
        {%- assign itemTextRight = false | AsBoolean -%}
        {%- if item.textalign == ''right''  -%}
        {%- assign itemTextRight = true | AsBoolean -%}
        {%- endif -%}
        {%- if item.labellocation == ''top'' -%}
        {%- assign itemLabelBottom = false | AsBoolean -%}
        {%- assign itemLabelTop = true | AsBoolean -%}
        {%- endif -%}
        {%- capture kpiStat -%}
            {% if itemLabel != '''' %}<span class="kpi-label">{{ itemLabel }}</span>{% endif %}
            {% if itemSecondaryLabel != '''' %}
                <span class="kpi-secondary-label">
                {% if itemSecondaryLabelColor != '''' %}
                <span class="my-1 badge text-white{% if secondarySplitLength == 2 %} bg-{{ itemSecondaryLabelColor }}{% endif %}">{{ itemSecondaryLabel }}</span>
                {% else %}
                {{ itemSecondaryLabel }}
                {% endif %}
                </span>
            {% endif %}
        {%- endcapture -%}


        <div class="kpi {{ kpisize }} {% if style == ''card'' %}kpi-card{% endif %} {% if iconbackground %}has-icon-bg{% endif %} {% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}{{ class }}" {% if height != '''' and color != '''' or colorSplitLength != 2 %}style="{% if height != '''' %}min-height: {{ height }};{% endif %}{% if color != '''' and colorSplitLength != 2 %}color:{{ color }};border-color:{{ color | FadeOut:''50%'' }}{% endif %}"{% endif %} {% if itemDescription != '''' %}data-toggle="tooltip" title="{{ itemDescription }}"{% endif %}>
            {%- if itemIcon != '''' -%}
            <div class="kpi-icon">
                <img class="svg-placeholder" src="data:image/svg+xml;utf8,&lt;svg xmlns=''http://www.w3.org/2000/svg'' viewBox=''0 0 1 1''&gt;&lt;/svg&gt;">
                <div class="kpi-content"><i class="{{ icontype }} fa-fw {{ itemIcon }}"></i></div>
            </div>
            {%- endif -%}
            <div class="kpi-stat {% if itemTextRight %}text-right{% endif %}">
                {% if itemLabelTop %}{{ kpiStat }}{% endif %}
                <span class="kpi-value text-color">{{ itemValue }}{% if itemSubValue != '''' %}<span class="kpi-subvalue {% if subvalueSplitLength == 2 %}text-{{ colitemSubValueColoror }}{% endif %}">{{ itemSubValue }}</span>{% endif %}</span>
                {% if itemLabelBottom %}{{ kpiStat }}{% endif %}

            </div>
        </div>
    {% endfor %}
</div>' WHERE ([Guid]='8A49FD01-D59E-4611-8FF4-9E226C99FB22')