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
    public partial class KpiShortCodeEdit : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE [LavaShortcode] SET [Markup]=
N'<div class=""kpi-container d-flex flex-wrap"" {% if height != '''' %}style=""min-height: {{ height }};""{% endif %}>
    {% for item in kpis %}
        {%- assign itemIcon = item.icon -%}
        {%- assign color = item.color | Default:''blue-500'' -%}
        {%- assign colorSplit = color | Split:''-'' -%}
        {%- assign colorSplitLength = colorSplit | Size -%}
        {%- assign itemValue = item.value | Default:''--'' -%}
        {%- assign itemLabel = item.label -%}
        {%- assign itemDescription = item.description | Trim | Escape -%}
        {%- assign itemMode = item.mode -%}
        <div class=""kpi {{ itemMode }} {% if itemMode == ''light'' %}kpi-light {% endif %}{% if colorSplitLength == 2 %}text-{{ color }} border-{{ colorSplit | First }}-{{ colorSplit | Last | Minus:200 | AtLeast:100 }}{% endif %}"" {% if colorSplitLength != 2 %}style=""color:{{ color }};border-color:{{ color | FadeOut:''50%'' }}""{% endif %} {% if itemDescription != '''' %}data-toggle=""tooltip"" title=""{{ itemDescription }}""{% endif %}>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder""
                src=""data:image/svg+xml;utf8,&lt;svg xmlns=''http://www.w3.org/2000/svg'' viewBox=''0 0 1 1''&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""{{ icontype }} {{ itemIcon }}""></i></div>
            </div>
            <div class=""kpi-stat"">
                <span class=""kpi-value text-color"">{{ itemValue }}</span>
                {% if itemLabel != '''' %}<span class=""kpi-label text-muted"">{{ itemLabel }}</span>{% endif %}
            </div>
        </div>
    {% endfor %}
</div>
' 
WHERE [Guid]='8A49FD01-D59E-4611-8FF4-9E226C99FB22';" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Intentionally blank
        }
    }
}
