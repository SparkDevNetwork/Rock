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
    public partial class Rollup_20240523 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateShortcodeKPI_Up();
            UpdateShortcodeGoogleMap_Up();
            UpdateRecurringTransactionFrequencyDefinedTypeIntervalDaysAttributeAbbreviatedName();
            UpdateVolunteerGenerosityScheduleLocation();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// DL: Update the KPI Shortcode to fix an issue with the icontype parameter.
        /// </summary>
        private void UpdateShortcodeKPI_Up()
        {
            var markup = @"
{%- if columncount != '' -%}
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
</div>
";

            var sql = @"
-- Update Shortcode: KPI
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='8A49FD01-D59E-4611-8FF4-9E226C99FB22')
";
            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        /// <summary>
        /// DL: Update the Google Map Shortcode to fix an issue with the zoom parameter.
        /// </summary>
        private void UpdateShortcodeGoogleMap_Up()
        {
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'google.maps.Animation.DROP' %}
{% when 'bounce' %}
    {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
{% else %}
    {% assign markeranimation = 'null' %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;

        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            marker = new google.maps.Marker({
                position: position,
                map: map,
                animation: {{ markeranimation }},
                title: markers{{ id }}[i][2],
                icon: markers{{ id }}[i][4]
            });

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";
            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        /// <summary>
        /// JMH: Set "Interval Days" Attribute abbreviated name to "Payment Interval Days"
        /// </summary>
        private void UpdateRecurringTransactionFrequencyDefinedTypeIntervalDaysAttributeAbbreviatedName()
        {
            Sql( @"
DECLARE @PaymentFrequencyDefinedTypeId AS INT
DECLARE @IntervalDaysAttributeId AS INT
SELECT TOP 1 @PaymentFrequencyDefinedTypeId = [Id] FROM [dbo].[DefinedType] WHERE [Guid] = '1F645CFB-5BBD-4465-B9CA-0D2104A1479B'
SELECT TOP 1 @IntervalDaysAttributeId = [Id] FROM [dbo].[Attribute] WHERE [EntityTypeQualifierColumn] = 'DefinedTypeId' and [EntityTypeQualifierValue] = @PaymentFrequencyDefinedTypeId
UPDATE [dbo].[Attribute]
   SET [AbbreviatedName] = 'Payment Interval Days'
 WHERE [Id] = @IntervalDaysAttributeId
       AND ([AbbreviatedName] IS NULL)" );
        }

        /// <summary>
        /// JDR: Update the location of the Volunteer Generosity Persisted Dataset Schedule 
        /// </summary>
        private void UpdateVolunteerGenerosityScheduleLocation()
        {
            // Create or update the category named "Persisted Dataset Schedules"
            Sql( @"
         DECLARE @EntityTypeId int;
         SET @EntityTypeId = ( SELECT[Id] FROM [dbo].[EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A'); 
         IF NOT EXISTS (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '7D152006-C47D-47E6-BF0B-09C3C2D0AE84')
         BEGIN
             INSERT INTO [dbo].[Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Guid], [Order], [Description], [CreatedDateTime], [ModifiedDateTime])
             VALUES( 1, NULL, @EntityTypeId, 'Persisted Dataset Schedules', '', '7D152006-C47D-47E6-BF0B-09C3C2D0AE84', 0, 'Schedules used for Persisted Datasets', GETDATE(), GETDATE() );
         END
         ELSE
         BEGIN
             UPDATE [dbo].[Category]
             SET [Name] = 'Persisted Dataset Schedules',
                 [IconCssClass] = 'fa fa-calendar',
                 [Description] = 'Schedules used for Persisted Datasets',
                 [ModifiedDateTime] = GETDATE()
             WHERE [Guid] = '7D152006-C47D-47E6-BF0B-09C3C2D0AE84'
         END
     " );

            // Move the target schedule under the created category
            Sql( @"
         DECLARE @CategoryId int = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = '7D152006-C47D-47E6-BF0B-09C3C2D0AE84')
         DECLARE @ScheduleId int = (SELECT [Id] FROM [dbo].[Schedule] WHERE [Guid] = 'ACE62853-0A10-4523-8BA2-CF7597F1D190')

         IF @ScheduleId IS NOT NULL
         BEGIN
             UPDATE [dbo].[Schedule]
             SET [CategoryId] = @CategoryId,
                 [IsPublic] = 0,
                 [ModifiedDateTime] = GETDATE()
             WHERE [Id] = @ScheduleId
         END
     " );
        }
    }
}
