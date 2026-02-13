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
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateKpiShortcodeForTabler : Rock.Migrations.RockMigration
    {
        private const string UpShortcodeDocumentation = @"<p>Basic Usage:</p>
<pre><code>{[kpis]}
  [<span class=""hljs-string"">[ kpi icon:'ti-highlight' value:'4' label:'Highlighters' color:'yellow-700'</span>]][[ endkpi ]]
  [<span class=""hljs-string"">[ kpi icon:'ti-ballpen' value:'8' label:'Pens' color:'indigo-700'</span>]][[ endkpi ]]
  [<span class=""hljs-string"">[ kpi icon:'ti-pencil' value:'15' label:'Pencils' color:'green-600'</span>]][[ endkpi ]]
{[endkpis]}
</code></pre>
<h4 id=""shortcode-options"">Shortcode Options</h4>
<ul>
<li><strong>title</strong> (blank) &ndash; An optional main title for the group of KPIs.</li>
<li><strong>subtitle</strong> (blank) &ndash; The smaller body sized text to show under the title. Only shown if provided.</li>
<li><strong>showtitleseparator</strong> (true) &ndash; The label for the KPI.</li>
<li><strong>columncount</strong> (4) &ndash; The number of columns to display at the large responsive size. Will decrement by one for each subsequent breakpoint (lg: 4, md: 3, sm: 2, xs: 1).</li>
<li><strong>columnmin</strong> (blank) &ndash; The minimum width for a column to display, used to force columns to wrap onto the next line when out of space.</li>
<li><strong>size</strong> (md) &ndash; The size of the KPI, default is medium. Options include <code>sm, lg, xl</code>.</li>
<li><strong>style</strong>(card) - The display style of the KPI. Options include <code>card</code> and <code>edgeless</code>.
<ul>
<li><strong>default</strong> &ndash; Default KPI with icon, and no border.</li>
<li><strong>card</strong> &ndash; KPI with card like appearance and border.</li>
</ul>
</li>
<li><strong>iconbackground</strong> (true) - Set the value to true to display a background behind the FontAwesome icon.</li>
<li><strong>tooltipdelay</strong> (0) - Milliseconds to delay showing and hiding the KPI description tooltip (if one is provided).</li>
</ul>
<h4 id=""kpi-options"">KPI Options</h4>
<ul>
<li><strong>icon</strong> (blank) &ndash; The class of the <a href=""https://tabler.io/icons"">Tabler</a>&nbsp;icon, no icon will display if left blank.</li>
<li><strong>label</strong> (blank) &ndash; The label for the KPI.</li>
<li><strong>labellocation</strong> (bottom) &ndash; Options: <code>bottom</code>, <code>top</code> Determines if the label should be display above the value or below.</li>
<li><strong>value</strong> (--) &ndash; The value of the KPI.</li>
<li><strong>description</strong> (blank) &ndash; An optional description, shown as tooltip on hover.</li>
<li><strong>color</strong> (blank) &ndash; The color of the icon from <a href=""https://tailwindcss.com/docs/background-color#app"">Tailwind</a>. Using the name of the <a href=""#colors"">color</a> and 100-900 or a CSS color (<code>#4ba9df</code>).</li>
<li><strong>textalign</strong> (left) &ndash; Alignment of KPI label and value.</li>
<li><strong>height</strong> (auto) &ndash; Optionally set a minimum height for the row of icons. (Example values <code>250px</code>, <code>50vh</code>)</li>
<li><strong>url</strong> (blank) &ndash; An optional link for the entire KPI (Note: Using this parameter will make any other links in the KPI inaccessible).</li>
</ul>
<div class=""my-2""><img class=""img-responsive"" src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/kpi-default-v125.png"" width=""961"" height=""182"" loading=""lazy""></div>
<h5 id=""advanced-options"">Advanced Options</h5>
<ul>
<li><strong>subvalue</strong> (blank) &ndash; Secondary value of the KPI, useful for adding trend information i.e. ""+6%"".</li>
<li><strong>secondarylabel</strong> (blank) &ndash; Smaller lighter label to show under the main label.</li>
<li><strong>icontype</strong> (ti) &ndash; Optionally override the default Tabler icon class to allow using alternate style icons. If you specify an icon name that begins with 'fa-' then icontype will be assumed to be 'fa'.</li>
</ul>
<div class=""my-2""><img class=""img-responsive"" src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/kpi-advanced.jpg"" width=""479"" height=""145"" loading=""lazy""></div>
<h5 id=""colors"">Colors</h5>
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
</ul>";

        private const string UpShortcodeMarkup = @"{%- if columncount != '' -%}
{%- assign columncountlg = columncount | AsInteger | AtLeast:1 -%}
{%- if columncountmd == '' -%}{%- assign columncountmd = columncountlg | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- if columncountsm == '' -%}{%- assign columncountsm = columncountmd | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- endif -%}
{%- assign showtitleseparator = showtitleseparator | AsBoolean -%}

{%- if title != '' -%}<h3 id=""{{ title | ToCssClass }}"" class=""kpi-title"">{{ title }}</h3>{%- endif -%}
{%- if subtitle != '' -%}<p class=""kpi-subtitle"">{{ subtitle }}</p>{%- endif -%}
{% if title != '' or subtitle != ''  %}
{%- if showtitleseparator -%}<hr class=""mt-3 mb-4"">{%- endif -%}
{% endif %}

{%- assign iconbackground = iconbackground | AsBoolean -%}
{%- assign iconTypeDefault = icontype | Trim -%}

{%- assign kpisize = '' -%}
{%- if size == 'sm' -%}
{%- assign kpisize = 'kpi-sm' -%}
{%- elseif size == 'lg' -%}
{%- assign kpisize = 'kpi-lg' -%}
{%- elseif size == 'xl' -%}
{%- assign kpisize = 'kpi-xl' -%}
{%- endif -%}

<div class=""kpi-container"" {% if columncount != '' %}style=""--kpi-col-lg:{{ 100 | DividedBy:columncountlg,4 }}%;--kpi-col-md:{{ 100 | DividedBy:columncountmd,4 }}%;--kpi-col-sm:{{ 100 | DividedBy:columncountsm,4 }}%;{% if columnmin != '' %}--kpi-min-width:{{ columnmin }};{% endif %} {{ cssstyle }}""{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon | Trim -%}
        {%- assign itemIconType = item.icontype | Trim -%}
        {%- if itemIconType == '' -%}{%- assign itemIconType = icontype -%}{%- endif -%}

        {%- assign itemIconWidth = ""ti-fw"" -%}
        {%- assign itemIconPrefix = itemIcon | Slice:0,3 -%}
        {%- if itemIconType == ""ti"" -%}
            {%- if itemIconPrefix == ""fa "" or itemIconPrefix == ""fa-"" -%}
                {%- assign itemIconType = ""fa"" -%}
                {%- assign itemIconWidth = ""fa-fw"" -%}
            {%- endif -%}
        {%- endif -%}

        {%- assign color = item.color | Trim -%}
        {%- assign colorSplit = color | Split:'-' -%}
        {%- assign height = item.height | Trim -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Trim | Default:'--' -%}
        {%- assign itemSubValue = item.subvalue | Trim -%}
        {%- if itemSubValue != '' -%}
            {%- assign itemSubValueColor = item.subvaluecolor | Trim -%}
            {%- assign subvalueColorSplit = itemSubValueColor | Split:'-' -%}
            {%- assign subvalueSplitLength = subvalueColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemSecondaryLabel = item.secondarylabel | Trim -%}
        {%- if itemSecondaryLabel != '' -%}
            {%- assign itemSecondaryLabelColor = item.secondarylabelcolor | Default:'' | Trim -%}
            {%- assign secondaryColorSplit = itemSecondaryLabelColor | Split:'-' -%}
            {%- assign secondarySplitLength = secondaryColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabelBottom = true | AsBoolean -%}
        {%- assign itemLabelTop = false | AsBoolean -%}
        {%- assign itemTextRight = false | AsBoolean -%}
        {%- if item.textalign == 'right'  -%}
        {%- assign itemTextRight = true | AsBoolean -%}
        {%- endif -%}
        {%- if item.labellocation == 'top' -%}
        {%- assign itemLabelBottom = false | AsBoolean -%}
        {%- assign itemLabelTop = true | AsBoolean -%}
        {%- endif -%}
                {%- assign itemUrl = item.url | Trim -%}
        {%- capture kpiStat -%}
            {% if itemLabel != '' %}<span class=""kpi-label"">{{ itemLabel }}</span>{% endif %}
            {% if itemSecondaryLabel != '' %}
                <span class=""kpi-secondary-label"">
                {% if itemSecondaryLabelColor != '' %}
                <span class=""my-1 badge text-white{% if secondarySplitLength == 2 %} bg-{{ itemSecondaryLabelColor }}{% endif %}"">{{ itemSecondaryLabel }}</span>
                {% else %}
                {{ itemSecondaryLabel }}
                {% endif %}
                </span>
            {% endif %}
        {%- endcapture -%}

        <div class=""kpi {{ kpisize }} {% if style == 'card' %}kpi-card{% endif %} {% if iconbackground %}has-icon-bg{% endif %} {% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}{{ class }}"" {% if colorSplitLength != 2 and color != '' or height != '' %}style=""{% if height != '' %}min-height: {{ height }};{% endif %}{% if color != '' and colorSplitLength != 2 %}color:{{ color }};border-color:{{ color | FadeOut:'50%' }}{% endif %}""{% endif %} {% if itemDescription != '' %}data-toggle=""tooltip"" title=""{{ itemDescription }}"" {% if tooltipdelay != '' %}data-delay='{{ tooltipdelay }}'{% endif %}{% endif %}>
            {% if itemUrl != '' %}<a href=""{{ itemUrl }}"" class=""stretched-link""></a>{% endif %}
            {%- if itemIcon != '' -%}
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""{{ itemIconType }} {{ itemIconWidth }} {{ itemIcon }}""></i></div>
            </div>
            {%- endif -%}
            <div class=""kpi-stat {% if itemTextRight %}text-right{% endif %}"">
                {% if itemLabelTop %}{{ kpiStat }}{% endif %}
                <span class=""kpi-value text-color"">{{ itemValue }}{% if itemSubValue != '' %}<span class=""kpi-subvalue {% if subvalueSplitLength == 2 %}text-{{ colitemSubValueColoror }}{% endif %}"">{{ itemSubValue }}</span>{% endif %}</span>
                {% if itemLabelBottom %}{{ kpiStat }}{% endif %}

            </div>
        </div>
    {% endfor %}
</div>";

        private const string UpShortcodeParameters = "title^|subtitle^|showtitleseparator^false|icontype^ti|columncount^|columncountmd^|columncountsm^|size^|style^card|tooltipdelay^|iconbackground^true|columnmin^|columnminmd^|columnminsm^";

        private const string DownShortcodeDocumentation = @"<p>Basic Usage:</p>
<pre><code>{[kpis]}
  [<span class=""hljs-string"">[ kpi icon:'fa-highlighter' value:'4' label:'Highlighters' color:'yellow-700'</span>]][[ endkpi ]]
  [<span class=""hljs-string"">[ kpi icon:'fa-pen-fancy' value:'8' label:'Pens' color:'indigo-700'</span>]][[ endkpi ]]
  [<span class=""hljs-string"">[ kpi icon:'fa-pencil-alt' value:'15' label:'Pencils' color:'green-600'</span>]][[ endkpi ]]
{[endkpis]}
</code></pre>
<h4 id=""shortcode-options"">Shortcode Options</h4>
<ul>
<li><strong>title</strong> (blank) &ndash; An optional main title for the group of KPIs.</li>
<li><strong>subtitle</strong> (blank) &ndash; The smaller body sized text to show under the title. Only shown if provided.</li>
<li><strong>showtitleseparator</strong> (true) &ndash; The label for the KPI.</li>
<li><strong>columncount</strong> (4) &ndash; The number of columns to display at the large responsive size. Will decrement by one for each subsequent breakpoint (lg: 4, md: 3, sm: 2, xs: 1).</li>
<li><strong>columnmin</strong> (blank) &ndash; The minimum width for a column to display, used to force columns to wrap onto the next line when out of space.</li>
<li><strong>size</strong> (md) &ndash; The size of the KPI, default is medium. Options include <code>sm, lg, xl</code>.</li>
<li><strong>style</strong>(card) - The display style of the KPI. Options include <code>card</code> and <code>edgeless</code>.
<ul>
<li><strong>default</strong> &ndash; Default KPI with icon, and no border.</li>
<li><strong>card</strong> &ndash; KPI with card like appearance and border.</li>
</ul>
</li>
<li><strong>iconbackground</strong> (true) - Set the value to true to display a background behind the FontAwesome icon.</li>
<li><strong>tooltipdelay</strong> (0) - Milliseconds to delay showing and hiding the KPI description tooltip (if one is provided).</li>
</ul>
<h4 id=""kpi-options"">KPI Options</h4>
<ul>
<li><strong>icon</strong> (blank) &ndash; The class of the <a href=""https://fontawesome.com/icons?d=gallery"">FontAwesome</a> icon, no icon will display if left blank.</li>
<li><strong>label</strong> (blank) &ndash; The label for the KPI.</li>
<li><strong>labellocation</strong> (bottom) &ndash; Options: <code>bottom</code>, <code>top</code> Determines if the label should be display above the value or below.</li>
<li><strong>value</strong> (--) &ndash; The value of the KPI.</li>
<li><strong>description</strong> (blank) &ndash; An optional description, shown as tooltip on hover.</li>
<li><strong>color</strong> (blank) &ndash; The color of the icon from <a href=""https://tailwindcss.com/docs/background-color#app"">Tailwind</a>. Using the name of the <a href=""#colors"">color</a> and 100-900 or a CSS color (<code>#4ba9df</code>).</li>
<li><strong>textalign</strong> (left) &ndash; Alignment of KPI label and value.</li>
<li><strong>height</strong> (auto) &ndash; Optionally set a minimum height for the row of icons. (Example values <code>250px</code>, <code>50vh</code>)</li>
<li><strong>url</strong> (blank) &ndash; An optional link for the entire KPI (Note: Using this parameter will make any other links in the KPI inaccessible).</li>
</ul>
<div class=""my-2""><img class=""img-responsive"" src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/kpi-default-v125.png"" width=""961"" height=""182"" loading=""lazy""></div>
<h5 id=""advanced-options"">Advanced Options</h5>
<ul>
<li><strong>subvalue</strong> (blank) &ndash; Secondary value of the KPI, useful for adding trend information i.e. ""+6%"".</li>
<li><strong>secondarylabel</strong> (blank) &ndash; Smaller lighter label to show under the main label.</li>
<li><strong>icontype</strong> (fa) &ndash; Optionally override the default FontAwesome icon class to allow using alternate style icons.</li>
</ul>
<div class=""my-2""><img class=""img-responsive"" src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/kpi-advanced.jpg"" width=""479"" height=""145"" loading=""lazy""></div>
<h5 id=""colors"">Colors</h5>
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
</ul>";

        private const string DownShortcodeMarkup = @"{%- if columncount != '' -%}
{%- assign columncountlg = columncount | AsInteger | AtLeast:1 -%}
{%- if columncountmd == '' -%}{%- assign columncountmd = columncountlg | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- if columncountsm == '' -%}{%- assign columncountsm = columncountmd | Minus:1 | AtLeast:1 -%}{%- endif -%}
{%- endif -%}
{%- assign showtitleseparator = showtitleseparator | AsBoolean -%}

{%- if title != '' -%}<h3 id=""{{ title | ToCssClass }}"" class=""kpi-title"">{{ title }}</h3>{%- endif -%}
{%- if subtitle != '' -%}<p class=""kpi-subtitle"">{{ subtitle }}</p>{%- endif -%}
{% if title != '' or subtitle != ''  %}
{%- if showtitleseparator -%}<hr class=""mt-3 mb-4"">{%- endif -%}
{% endif %}

{%- assign iconbackground = iconbackground | AsBoolean -%}
{%- assign iconTypeDefault = icontype | Trim -%}

{%- assign kpisize = '' -%}
{%- if size == 'sm' -%}
{%- assign kpisize = 'kpi-sm' -%}
{%- elseif size == 'lg' -%}
{%- assign kpisize = 'kpi-lg' -%}
{%- elseif size == 'xl' -%}
{%- assign kpisize = 'kpi-xl' -%}
{%- endif -%}

<div class=""kpi-container"" {% if columncount != '' %}style=""--kpi-col-lg:{{ 100 | DividedBy:columncountlg,4 }}%;--kpi-col-md:{{ 100 | DividedBy:columncountmd,4 }}%;--kpi-col-sm:{{ 100 | DividedBy:columncountsm,4 }}%;{% if columnmin != '' %}--kpi-min-width:{{ columnmin }};{% endif %} {{ cssstyle }}""{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon | Trim -%}
        {%- assign itemIconType = item.icontype | Trim -%}
        {%- if itemIconType == '' -%}{%- assign itemIconType = icontype -%}{%- endif -%}

        {%- assign color = item.color | Trim -%}
        {%- assign colorSplit = color | Split:'-' -%}
        {%- assign height = item.height | Trim -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Trim | Default:'--' -%}
        {%- assign itemSubValue = item.subvalue | Trim -%}
        {%- if itemSubValue != '' -%}
            {%- assign itemSubValueColor = item.subvaluecolor | Trim -%}
            {%- assign subvalueColorSplit = itemSubValueColor | Split:'-' -%}
            {%- assign subvalueSplitLength = subvalueColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemSecondaryLabel = item.secondarylabel | Trim -%}
        {%- if itemSecondaryLabel != '' -%}
            {%- assign itemSecondaryLabelColor = item.secondarylabelcolor | Default:'' | Trim -%}
            {%- assign secondaryColorSplit = itemSecondaryLabelColor | Split:'-' -%}
            {%- assign secondarySplitLength = secondaryColorSplit | Size -%}
        {%- endif -%}
        {%- assign itemLabelBottom = true | AsBoolean -%}
        {%- assign itemLabelTop = false | AsBoolean -%}
        {%- assign itemTextRight = false | AsBoolean -%}
        {%- if item.textalign == 'right'  -%}
        {%- assign itemTextRight = true | AsBoolean -%}
        {%- endif -%}
        {%- if item.labellocation == 'top' -%}
        {%- assign itemLabelBottom = false | AsBoolean -%}
        {%- assign itemLabelTop = true | AsBoolean -%}
        {%- endif -%}
                {%- assign itemUrl = item.url | Trim -%}
        {%- capture kpiStat -%}
            {% if itemLabel != '' %}<span class=""kpi-label"">{{ itemLabel }}</span>{% endif %}
            {% if itemSecondaryLabel != '' %}
                <span class=""kpi-secondary-label"">
                {% if itemSecondaryLabelColor != '' %}
                <span class=""my-1 badge text-white{% if secondarySplitLength == 2 %} bg-{{ itemSecondaryLabelColor }}{% endif %}"">{{ itemSecondaryLabel }}</span>
                {% else %}
                {{ itemSecondaryLabel }}
                {% endif %}
                </span>
            {% endif %}
        {%- endcapture -%}

        <div class=""kpi {{ kpisize }} {% if style == 'card' %}kpi-card{% endif %} {% if iconbackground %}has-icon-bg{% endif %} {% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}{{ class }}"" {% if colorSplitLength != 2 and color != '' or height != '' %}style=""{% if height != '' %}min-height: {{ height }};{% endif %}{% if color != '' and colorSplitLength != 2 %}color:{{ color }};border-color:{{ color | FadeOut:'50%' }}{% endif %}""{% endif %} {% if itemDescription != '' %}data-toggle=""tooltip"" title=""{{ itemDescription }}"" {% if tooltipdelay != '' %}data-delay='{{ tooltipdelay }}'{% endif %}{% endif %}>
            {% if itemUrl != '' %}<a href=""{{ itemUrl }}"" class=""stretched-link""></a>{% endif %}
            {%- if itemIcon != '' -%}
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""{{ itemIconType }} fa-fw {{ itemIcon }}""></i></div>
            </div>
            {%- endif -%}
            <div class=""kpi-stat {% if itemTextRight %}text-right{% endif %}"">
                {% if itemLabelTop %}{{ kpiStat }}{% endif %}
                <span class=""kpi-value text-color"">{{ itemValue }}{% if itemSubValue != '' %}<span class=""kpi-subvalue {% if subvalueSplitLength == 2 %}text-{{ colitemSubValueColoror }}{% endif %}"">{{ itemSubValue }}</span>{% endif %}</span>
                {% if itemLabelBottom %}{{ kpiStat }}{% endif %}

            </div>
        </div>
    {% endfor %}
</div>";

        private const string DownShortcodeParameters = "title^|subtitle^|showtitleseparator^false|icontype^fa|columncount^|columncountmd^|columncountsm^|size^|style^card|tooltipdelay^|iconbackground^true|columnmin^|columnminmd^|columnminsm^";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
UPDATE [LavaShortcode] SET
    [Documentation] = '{UpShortcodeDocumentation.Replace( "'", "''" )}',
    [Markup] = '{UpShortcodeMarkup.Replace( "'", "''" )}',
    [Parameters] = '{UpShortcodeParameters.Replace( "'", "''" )}'
WHERE [Guid] = '8A49FD01-D59E-4611-8FF4-9E226C99FB22' " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $@"
UPDATE [LavaShortcode] SET
    [Documentation] = '{DownShortcodeDocumentation.Replace( "'", "''" )}',
    [Markup] = '{DownShortcodeMarkup.Replace( "'", "''" )}',
    [Parameters] = '{DownShortcodeParameters.Replace( "'", "''" )}'
WHERE [Guid] = '8A49FD01-D59E-4611-8FF4-9E226C99FB22' " );
        }
    }
}
