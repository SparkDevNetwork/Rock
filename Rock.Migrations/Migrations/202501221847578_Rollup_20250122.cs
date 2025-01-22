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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250122 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateGoogleMapIdLogicUp();
            ChopBlocksUp();
            UpdateContentFileTypeBlacklistUp();
            MobileScheduledTransactionListTemplateUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateGoogleMapIdLogicDown();
            UpdateContentFileTypeBlacklistDown();
            MobileScheduledTransactionListTemplateDown();
        }

        #region KH: Update Google Map Id Logic

        private void UpdateGoogleMapIdLogicUp()
        {
            // Delete the old Global Attribute
            RockMigrationHelper.DeleteAttribute( "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8" );

            // Add the new Google Map Id Attribute to the Map Styles Defined Type
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.TEXT,
                "Google Map Id",
                "core_GoogleMapId",
                "The map identifier that's associated with a specific map style or feature on your google console.  If you supply a Map ID value here, the other 'style' attribute values will be ignored.",
                0,
                string.Empty,
                "32AFC54A-D923-4ED1-B1E4-56873FB244A6" );

            // Update the order, description, and required properties of the other values
            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.VALUE_LIST,
                "Colors",
                "Colors",
                "(Obsoleted by Google Map Id) - The colors to use for markers and/or shapes on the map.",
                1,
                false,
                "#fe7569|#a7fe68|#68fe74|#68f2fe|#7468fe|#bf68fe",
                false,
                false,
                "215AC212-DEA8-412D-BEA9-06A777D20DFD" );

            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.CODE_EDITOR,
                "Dynamic Map Style",
                "DynamicMapStyle",
                "(Obsoleted by Google Map Id)",
                2,
                false,
                string.Empty,
                false,
                false,
                "33AA992E-F631-48CF-9055-8B06D6EDCA66" );

            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.TEXT,
                "Static Map Style",
                "StaticMapStyle",
                "(Obsoleted by Google Map Id) - Be sure to include the following (customizing color and fillcolor) to support adding markers and polygons:\r\nmarkers=color:0x779cb1|{MarkerPoints}&path=fillcolor:0x779cb155|{PolygonPoints}.",
                3,
                false,
                "http://maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|{PolygonPoints}",
                false,
                false,
                "68BA76A5-A013-4273-AEC6-E928F9FC6E04" );

            // Update the Google Maps Lava Shortcode to exclude the global attribute for Google Map Id
            var markup = @"{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = mapid | Trim %}
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
{% if mapId == """" %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?loading=async&key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key='%}
{% endif %}
{% javascript id:'googlemapsapi' url:'{{ googleMapsUrl  | Append:apiKey }}' %}{% endjavascript %}
{% if mapId == """" %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'google.maps.Animation.DROP' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
    {% else %}
        {% assign markeranimation = 'null' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'drop' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'bounce' %}
    {% else %}
        {% assign markeranimation = null %}
    {% endcase %}
{% endif %}
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
@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}
.drop {
  animation: drop 0.3s linear forwards .5s;
}
@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}
.bounce {
  animation: bounce 2s infinite;
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
            {% if mapId != """" %}
	            ,mapId: '{{ mapId }}'
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            {% if mapId == """" %}
                marker = new google.maps.Marker({
                     position: position,
                     map: map,
                     animation: {{ markeranimation }},
                     title: markers{{ id }}[i][2],
                     icon: markers{{ id }}[i][4]
                 });
            {% else %}
                if (markers{{ id }}[i][4] != ''){
                    const glyph = document.createElement('img');
                	glyph.src = markers{{ id }}[i][4];
                    
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: glyph
                    });
                }
                else {
                    var pin = new google.maps.marker.PinElement({
                        background: '#FE7569',
                        borderColor: '#000',
                        scale: 1,
                        glyph: null
                    });
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: pin.element
                    });
                }
	            const content = marker.content;
    	        {% if markeranimation -%}
                // Drop animation should be onetime so remove class once animation ends.
		            {% if markeranimation == 'drop' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener('animationend', (event) => {
                            content.classList.remove('{{ markeranimation }}');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add('{{ markeranimation }}');
                {% endif -%}
            {% endif %}
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
        google.maps.event.addListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }
    window.addEventListener('load', initialize{{ id }});
</script>";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        private void UpdateGoogleMapIdLogicDown()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                null,
                null,
                "Google Maps Id",
                "The map identifier that's associated with a specific map style or feature on your google console, when you reference a map ID, its associated map style is displayed in your map.",
                0,
                "DEFAULT_MAP_ID",
                "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8",
                "core_GoogleMapId" );

            RockMigrationHelper.DeleteAttribute( "32AFC54A-D923-4ED1-B1E4-56873FB244A6" );

            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.VALUE_LIST,
                "Colors",
                "Colors",
                "The colors to use for markers and/or shapes on the map.",
                0,
                false,
                "#fe7569|#a7fe68|#68fe74|#68f2fe|#7468fe|#bf68fe",
                false,
                false,
                "215AC212-DEA8-412D-BEA9-06A777D20DFD" );

            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.CODE_EDITOR,
                "Dynamic Map Style",
                "DynamicMapStyle",
                string.Empty,
                1,
                false,
                string.Empty,
                false,
                true,
                "33AA992E-F631-48CF-9055-8B06D6EDCA66" );

            RockMigrationHelper.UpdateDefinedTypeAttribute( SystemGuid.DefinedType.MAP_STYLES,
                SystemGuid.FieldType.TEXT,
                "Static Map Style",
                "StaticMapStyle",
                "Be sure to include the following (customizing color and fillcolor) to support adding markers and polygons:\r\nmarkers=color:0x779cb1|{MarkerPoints}&path=fillcolor:0x779cb155|{PolygonPoints}.",
                2,
                false,
                "http://maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|{PolygonPoints}",
                false,
                false,
                "68BA76A5-A013-4273-AEC6-E928F9FC6E04" );

            var markup = @"{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = mapid | Trim %}
{% if mapId == """" %}
    {% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}
{% endif %}
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
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?loading=async&key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key='%}
{% endif %}
{% javascript id:'googlemapsapi' url:'{{ googleMapsUrl  | Append:apiKey }}' %}{% endjavascript %}
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'google.maps.Animation.DROP' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
    {% else %}
        {% assign markeranimation = 'null' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'drop' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'bounce' %}
    {% else %}
        {% assign markeranimation = null %}
    {% endcase %}
{% endif %}
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
@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}
.drop {
  animation: drop 0.3s linear forwards .5s;
}
@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}
.bounce {
  animation: bounce 2s infinite;
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
            {% if mapId != 'DEFAULT_MAP_ID' %}
	            ,mapId: '{{ mapId }}'
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            {% if mapId == 'DEFAULT_MAP_ID' %}
                marker = new google.maps.Marker({
                     position: position,
                     map: map,
                     animation: {{ markeranimation }},
                     title: markers{{ id }}[i][2],
                     icon: markers{{ id }}[i][4]
                 });
            {% else %}
                if (markers{{ id }}[i][4] != ''){
                    const glyph = document.createElement('img');
                	glyph.src = markers{{ id }}[i][4];
                    
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: glyph
                    });
                }
                else {
                    var pin = new google.maps.marker.PinElement({
                        background: '#FE7569',
                        borderColor: '#000',
                        scale: 1,
                        glyph: null
                    });
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: pin.element
                    });
                }
	            const content = marker.content;
    	        {% if markeranimation -%}
                // Drop animation should be onetime so remove class once animation ends.
		            {% if markeranimation == 'drop' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener('animationend', (event) => {
                            content.classList.remove('{{ markeranimation }}');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add('{{ markeranimation }}');
                {% endif -%}
            {% endif %}
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
        google.maps.event.addListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }
    window.addEventListener('load', initialize{{ id }});
</script>";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        #endregion

        #region KH: Register block attributes for chop job in v17.0.36

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.CheckInScheduleBuilder
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.CheckInScheduleBuilder", "Check In Schedule Builder", "Rock.Blocks.CheckIn.CheckInScheduleBuilder, Rock.Blocks, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "28B9DAB2-C58A-4459-9EE7-8D1895C09592" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MergeTemplateDetail", "Merge Template Detail", "Rock.Blocks.Reporting.MergeTemplateDetail, Rock.Blocks, Version=17.0.35.0, Culture=neutral, PublicKeyToken=null", false, false, "3338D32F-20E0-4F6F-9ABC-DD21558649C8" );

            // Add/Update Obsidian Block Type
            //   Name:Merge Template Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Merge Template Detail", "Displays the details of a particular merge template.", "Rock.Blocks.Reporting.MergeTemplateDetail", "Core", "B852DB84-0CDF-4862-9EC7-CDBBBD5BB77A" );

            // Add/Update Obsidian Block Type
            //   Name:Schedule Builder
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.CheckInScheduleBuilder
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule Builder", "Helps to build schedules used for check-in.", "Rock.Blocks.CheckIn.CheckInScheduleBuilder", "Check-in", "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" );

            // Attribute for BlockType
            //   BlockType: Merge Template Detail
            //   Category: Core
            //   Attribute: Merge Templates Ownership
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B852DB84-0CDF-4862-9EC7-CDBBBD5BB77A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "Merge Templates Ownership", @"Set this to restrict if the merge template must be a Personal or Global merge template. Note: If the user has EDIT authorization to this block, both Global and Personal templates can be edited regardless of this setting.", 0, @"Global", "F7A6F6E7-9A65-42EF-B144-9D5D5BCBDFA4" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B5B5BDF6-E715-40E1-835E-49CD5A163FF9" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5F3527FD-1611-460D-8E71-87DF9E33BC22" );
        }

        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 17.0.36",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v17.0.36
{ "820DE5F9-8391-4A2A-AA87-24156882BD5F", "b852db84-0cdf-4862-9ec7-cdbbbd5bb77a" }, // Merge Template Detail ( Core )
{ "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" }, // Schedule Builder ( Check-in )
                    // blocks chopped in v17.0.35
{ "01D23E86-51DC-496D-BB3E-0CEF5094F304", "b80e8563-41f2-4528-81e5-c62cf1ece9de" }, // Signature Document Detail ( Core )
{ "02D0A037-446B-403B-9719-5EF7D98239EF", "dabf690b-be17-4821-a13e-44c7c8d587cd" }, // Binary File Type Detail ( Core )
{ "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "6076609b-d4d2-4825-8bb2-8681e99c59f2" }, // Signature Document List ( Core )
{ "2E413152-B790-4EC2-84A9-9B48D2717D63", "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7" }, // Signature Document Template List ( Core )
{ "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5" }, // Signature Document Template Detail ( Core )
{ "A79336CD-2265-4E36-B915-CF49956FD689", "5bd4cd27-c1c1-4e12-8756-9c93e4edb28e" }, // Badge Detail ( CRM )
{ "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "2306068d-3551-4c10-8db8-133c030fa4fa" }, // Scheduled Job History ( Core )
{ "D9834641-7F39-4CFA-8CB2-E64068127565", "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" }, // Communication Entry ( Communication )
{ "F791046A-333F-4B2A-9815-73B60326162D", "e120f06f-6db7-464a-a797-c3c90b92ef40" }, // Prayer Request Detail ( Prayer )
                    // blocks chopped in v1.17.0.32
{ "21FFA70E-18B3-4148-8FC4-F941100B49B8", "68D2ABBC-3C43-4450-973F-071D1715C0C9" }, // Attendance History ( Check-in )
{ "23CA8858-6D02-48A8-92C4-CE415DAB41B6", "ADBF3377-A491-4016-9375-346496A25FB4" }, // Apple TV Page Detail ( TV > TV Apps )
// { "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "7EA2E093-2F33-4213-A33E-9E9A7A760181" }, // Check-in Type Detail ( Check-in > Configuration )
{ "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" }, // Financial Pledge List ( Finance )
{ "813CFCCF-30BF-4A2F-BB55-F240A3B7809F", "653052A0-CA1C-41B8-8340-4B13149C6E66" }, // Person Signal List ( Core )
{ "95366DA1-D878-4A9A-A26F-83160DBE784F", "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" }, // System Communication Preview ( Communication )
{ "9F577C39-19FB-4C33-804B-35023284B856", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" }, // Security Change Audit List ( Security )
{ "A2C41730-BF79-4F8C-8368-2C4D5F76129D", "28A34F1C-80F4-496F-A598-180974ADEE61" }, // Rest Key Detail( Security )
{ "A81AB554-B438-4C7F-9C45-1A9AE2F889C5", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E" }, // Assessment Type Detail ( CRM )
{ "B4D8CBCA-00F6-4D81-B8B6-170373D28128", "C12C615C-384D-478E-892D-0F353E2EF180" }, // Gateway Detail ( Finance )
{ "E2D423B8-10F0-49E2-B2A6-D62892379429", "3855B15B-C903-446A-AE5B-891AB52851CB" }, // System Configuration ( Administration )
                    // blocks chopped in v1.17.0.31
{ "41CD9629-9327-40D4-846A-1BB8135D130C", "dbcfb477-0553-4bae-bac9-2aec38e1da37" }, // Registration Instance - Fee List
{ "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "5ecca4fb-f8fb-49db-96b7-082bb4e4c170" }, // Assessment List
{ "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" }, // Note Watch List
{ "361F15FC-4C08-4A26-B482-CC260E708F7C", "b1f65833-ceca-4054-bcc3-2de5692741ed" }, // Note Watch Detail
// { "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "f431f950-f007-493e-81c8-16559fe4c0f0" }, // Defined Value List
// { "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "73fd23b4-fa3a-49ea-b271-ffb228c6a49e" }, // Defined Type Detail
{ "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "a6d8bfd9-0c3d-4f1e-ae0d-325a9c70b4c8" }, // REST Controller List
{ "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "2eafa987-79c6-4477-a181-63392aa24d20" }, // Rest Action List
{ "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "57babd60-2a45-43ac-8ed3-b09af79c54ab" }, // Account List
{ "DCD63280-B661-48AA-8DEB-F5ED63C7AB77", "c0c464c0-2c72-449f-b46f-8e31c1daf29b" }, // Account Detail (Finance)
{ "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65", "507F5108-FB55-48F0-A66E-CC3D5185D35D" }, // Campus Detail
{ "B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF", "b150e767-e964-460c-9ed1-b293474c5f5d" }, // Tag Detail
{ "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "972ad143-8294-4462-b2a7-1b36ea127374" }, // Group Archived List
{ "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "b6a17e77-e53d-4c96-bcb2-643123b8160c" }, // Schedule List
{ "C679A2C6-8126-4EF5-8C28-269A51EC4407", "5f3151bf-577d-485b-9ee3-90f3f86f5739" }, // Document Type List
{ "85E9AA73-7C96-4731-8DD6-AA604C35E536", "fd3eb724-1afa-4507-8850-c3aee170c83b" }, // Document Type Detail
{ "4280625A-C69A-4B47-A4D3-89B61F43C967", "d9510038-0547-45f3-9eca-c2ca85e64416" }, // Web Farm Settings
{ "B6AD2D98-0DF3-4DFB-AE2B-A8CF6E21E5C0", "011aede7-b036-4f4a-bf3e-4c284dc45de8" }, // Interaction Detail
{ "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100", "054a8469-a838-4708-b18f-9f2819346298" }, // Fundraising Donation List
{ "8CD3C212-B9EE-4258-904C-91BA3570EE11", "e3b5db5c-280f-461c-a6e3-64462c9b329d" }, // Device Detail
{ "678ED4B6-D76F-4D43-B069-659E352C9BD8", "e07607c6-5428-4ccf-a826-060f48cacd32" }, // Attendance List
{ "451E9690-D851-4641-8BA0-317B65819918", "2ad9e6bc-f764-4374-a714-53e365d77a36" }, // Content Channel Type Detail
{ "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "699ed6d1-e23a-4757-a0a2-83c5406b658a" }, // Fundraising List
                    // blocks chopped in v1.17.0.30
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload
                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "ShowAccountFilter,ShowDateRangeFilter,ShowLastModifiedFilter,ShowPersonFilter" }, // Pledge List ( Finance )
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "EnableDebug,LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
                { "361F15FC-4C08-4A26-B482-CC260E708F7C", "NoteType,EntityType" }, // Note Watch Detail
                { "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "EnableDebug" }, // Prayer Request Entry
                { "C96479B6-E309-4B1A-B024-1F1276122A13", "MaximumNumberOfDocuments" }, // Benevolence Type Detail
                { "D9834641-7F39-4CFA-8CB2-E64068127565", "DisplayCount" }, // Communication Entry ( Communication )
                { "F791046A-333F-4B2A-9815-73B60326162D", "EnableAIDisclaimer,AIDisclaimer" }, // Prayer Request Detail ( Prayer )
            } );
        }

        #endregion

        #region KA: Migration To Update Content File Type Blacklist

        private void UpdateContentFileTypeBlacklistUp()
        {
            Sql( $@"
                IF EXISTS (
                    SELECT [Id]
                    FROM dbo.[Attribute]
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5' )
                BEGIN
                    UPDATE [Attribute] SET
                        [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config, asmx'
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5'
                END
                ELSE
                BEGIN
                    INSERT [dbo].[Attribute] (
                        [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid]
                        , [CreatedDateTime]
                        , [ModifiedDateTime]
                        , [CreatedByPersonAliasId]
                        , [ModifiedByPersonAliasId]
                        , [ForeignKey]
                        , [IconCssClass]
                        , [AllowSearch]
                        , [ForeignGuid]
                        , [ForeignId]
                        , [IsIndexEnabled]
                        , [IsAnalytic]
                        , [IsAnalyticHistory]
                        , [IsActive]
                        , [EnableHistory]
                        , [PreHtml]
                        , [PostHtml]
                        , [AbbreviatedName]
                        , [ShowOnBulk]
                        , [IsPublic])
                    VALUES (
                        1
                        , 1
                        , NULL
                        , ''
                        , ''
                        , 'ContentFiletypeBlacklist'
                        , 'Content Filetype Blacklist'
                        , 'List of file types are not allowed to be uploaded in the HTML Editor.'
                        , 0
                        , 0
                        , 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config, asmx'
                        , 0
                        , 0
                        , '9ffb15c1-aa53-4fba-a480-64c9b348c5e5'
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , NULL
                        , 0
                        , NULL
                        , NULL
                        , 0
                        , 0
                        , 0
                        , 1
                        , 0
                        , NULL
                        , NULL
                        , NULL
                        , 0
                        , 0)
                END" );
        }

        private void UpdateContentFileTypeBlacklistDown()
        {
            Sql( $@"
                IF EXISTS (
                    SELECT [Id]
                    FROM dbo.[Attribute]
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5' )
                BEGIN
                    UPDATE [Attribute] SET
                        [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll, config'
                    WHERE [Guid] = '9ffb15c1-aa53-4fba-a480-64c9b348c5e5'
                END" );
        }

        #endregion

        #region PS: Scheduled Transaction List Template

        private void MobileScheduledTransactionListTemplateUp()
        {
            string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.AddOrUpdateTemplateBlock(
    SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST,
    "Mobile > Finance > Scheduled Transaction List",
    string.Empty );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0",
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST,
                "Default",
                @"<Rock:StyledBorder StyleClass=""border, border -interface-soft, rounded, bg-interface-softest, p-16"">
    <Grid ColumnDefinitions = ""Auto, Auto, *, Auto""
        RowDefinitions=""Auto, Auto, Auto""
        StyleClass=""gap-column-8"" >
        <Label StyleClass = ""body, bold, text-interface-strongest, mb-8""
            Grid.Row=""0""
            Grid.Column=""0""
            Text=""${{ ScheduledTransactionInfo.TotalAmount }}"" />
            
        {% if ScheduledTransactionInfo.IsActive == false %}
            <Label StyleClass = ""footnote, pt-2""
                Text=""Inactive""
                Grid.Column=""1""
                TextColor=""{Rock:PaletteColor App-Warning-strong}"" />                
        {% endif %}

{% if ScheduledTransactionInfo.NextPaymentDate and ScheduledTransactionInfo.NextPaymentDate != null %}
            < Label StyleClass = ""footnote""
                Grid.Row = ""1""
                Grid.ColumnSpan = ""3""
                Text = ""Next Gift: {{ ScheduledTransactionInfo.NextPaymentDate | Date:'MMM dd, yyyy' }}"" />
        {% endif %}


        < Label StyleClass = ""footnote""
            Grid.Row = ""2""
            Text = ""{{ ScheduledTransactionInfo.FrequencyText }}"" />
        < Rock:Icon
            Grid.Column = ""3""
            Grid.RowSpan = ""3""
            VerticalOptions = ""Center""
            StyleClass = ""footnote""
            IconFamily = ""FontAwesomeSolid""
            IconClass = ""chevron-right"" />
    </ Grid >


    < Rock:StyledBorder.Behaviors >
        < Rock:TouchBehavior
            PressedOpacity = ""0.6"" 
            DefaultOpacity=""1"" 
            HoveredOpacity=""0.6"" 
            Command=""{Binding PushPage}"" 
            CommandParameter=""{{ DetailPage }}"" />
    </Rock:StyledBorder.Behaviors >
</ Rock:StyledBorder >
",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }

        private void MobileScheduledTransactionListTemplateDown()
        {
            RockMigrationHelper.DeleteTemplateBlock( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST );
            RockMigrationHelper.DeleteTemplateBlockTemplate( "AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0" );
        }

        #endregion
    }
}
