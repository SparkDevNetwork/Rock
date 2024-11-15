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

using System.Collections.Generic;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 218, "1.17.0" )]
    public class MigrationRollupsForV17_0_19 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateFinishLavaTemplateUp();
            UpdateGoogleMapsLavaShortcodeUp();
            UnhideVolunteerGenerosityUp();
            UpdateFollowIconLavaShortcodeUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateFinishLavaTemplateDown();
            UpdateGoogleMapsLavaShortcodeDown();
            UpdateFollowIconLavaShortcodeDown();
        }

        #region KH: Update the Finish Lava Template Block Setting Value for Transactions

        private void UpdateFinishLavaTemplateUp()
        {
            Sql( @"UPDATE [AttributeValue]
SET [Value] = CASE 
    WHEN [Value] LIKE '%starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}%'
    THEN REPLACE(
        [Value], 
        'starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}', 
        '//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}'
    )
    ELSE [Value]
END
WHERE [AttributeId] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] IN ('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', 
                     '44DDFBF9-F63E-46E3-84A3-A9FC72D9F146', 
                     '6BEE06A9-969E-4704-9DC7-6B881D7280E3')
) AND [Value] NOT LIKE '%//- Updated to include EndDate%'" );
        }

        private void UpdateFinishLavaTemplateDown()
        {
            Sql( @"UPDATE [AttributeValue]
SET [Value] = CASE 
    WHEN [Value] LIKE '%//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}%'
    THEN REPLACE(
        [Value], 
        '//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}', 
        'starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}'
    )
    ELSE [Value]
END
WHERE [AttributeId] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] IN ('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', 
                     '44DDFBF9-F63E-46E3-84A3-A9FC72D9F146', 
                     '6BEE06A9-969E-4704-9DC7-6B881D7280E3')
) AND [Value] LIKE '%//- Updated to include EndDate%'" );
        }

        #endregion

        #region KH: Migration to Update Google Maps Lavashortcode

        private void UpdateGoogleMapsLavaShortcodeUp()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
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
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&key='%}
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

        private void UpdateGoogleMapsLavaShortcodeDown()
        {
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}
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
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&key='%}
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

        #endregion

        #region JDR: Add to v17 Migration to unhide new page

        private void UnhideVolunteerGenerosityUp()
        {
            Sql( @"UPDATE [Page] SET [DisplayInNavWhen] = 1 WHERE [Guid] = '16DD0891-E3D4-4FF3-9857-0869A6CCBA39'" );
        }

        #endregion

        #region KH: Update Follow Icon Lava Shortcode Up

        private void UpdateFollowIconLavaShortcodeUp()
        {
            var markup = @"
{% if CurrentPerson %}
    {% assign entitytypeguid = entitytypeguid | Trim %}
    {% assign entityguid = entityguid | Trim %}
    {% assign entitytypeid = entitytypeid | Trim %}
    {% assign entityid = entityid | Trim %}
    {% if entitytypeguid != '' and entityguid != '' %}
    {% assign entitytype = entitytypeguid %}
    {% assign entity = entityguid %}
    {% else %}
    {% assign entitytype = entitytypeid %}
    {% assign entity = entityid %}
    {% endif %}
    {% assign purposekey = purposekey | Trim %}
    {% assign suppresswarnings = suppresswarnings | AsBoolean %}
    {% assign isfollowed = isfollowed | AsBoolean %}
    
    {% if entitytype != '' and entity != '' %}
        <div class=""followicon js-followicon {% if isfollowed %}isfollowed{% endif %}"" data-entitytype=""{{ entitytype }}"" data-entity=""{{ entity }}"" {% if purposekey != '' %}data-purpose-key=""{{ purposekey }}""{% endif %} data-followed=""{{ isfollowed }}"">
            {{ blockContent }}
        </div>
    
        {% javascript id:'followicon' disableanonymousfunction:'true'%}
            $(document).ready(function() {
                // Use event delegation to bind the click event
                $(document).on('click', '.js-followicon', function(e) {
                    e.preventDefault();

                    var icon = $(this);
                    var entityType = icon.data('entitytype');
                    var entity = icon.data('entity');
                    var purpose = icon.data('purpose-key');
        
                    if (purpose) {
                        purpose = '?purposeKey=' + purpose;
                    } else {
                        purpose = '';
                    }
        
                    icon.toggleClass('isfollowed');
        
                    var actionType = icon.hasClass('isfollowed') ? 'POST' : 'DELETE';
        
                    $.ajax({
                        url: '/api/Followings/' + entityType + '/' + entity + purpose,
                        type: actionType,
                        statusCode: {
                            201: function() {
                                icon.attr('data-followed', 'true');
                            },
                            204: function() {
                                icon.attr('data-followed', 'false');
                            },
                            500: function() {
                                {% unless suppresswarnings %}
                                alert('Error: Check your Rock security settings and try again.');
                                {% endunless %}
                            }
                        },
                        error: function() {
                            icon.toggleClass('isfollowed');
                        }
                    });
                });
            });
        {% endjavascript %}
    {% else %}
        <!-- Follow Icon Shortcode is missing entitytype and/or entity. Note: Guids or Ids must be provided  -->
    {% endif %}
{% endif %}
";

            var sql = @"
-- Update Shortcode: Follow Icon
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='1E6785C0-7D92-49A7-9E15-68E113399152')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        private void UpdateFollowIconLavaShortcodeDown()
        {
            var markup = @"
{% if CurrentPerson %}
    {% assign entitytypeguid = entitytypeguid | Trim %}
    {% assign entityguid = entityguid | Trim %}
    {% assign entitytypeid = entitytypeid | Trim %}
    {% assign entityid = entityid | Trim %}
    {% if entitytypeguid != '' and entityguid != '' %}
    {% assign entitytype = entitytypeguid %}
    {% assign entity = entityguid %}
    {% else %}
    {% assign entitytype = entitytypeid %}
    {% assign entity = entityid %}
    {% endif %}
    {% assign purposekey = purposekey | Trim %}
    {% assign suppresswarnings = suppresswarnings | AsBoolean %}
    {% assign isfollowed = isfollowed | AsBoolean %}
    
    {% if entitytype != '' and entity != '' %}
        <div class=""followicon js-followicon {% if isfollowed %}isfollowed{% endif %}"" data-entitytype=""{{ entitytype }}"" data-entity=""{{ entity }}"" {% if purposekey != '' %}data-purpose-key=""{{ purposekey }}""{% endif %} data-followed=""{{ isfollowed }}"">
            {{ blockContent }}
        </div>
    
        {% javascript id:'followicon' disableanonymousfunction:'true'%}
            $( document ).ready(function() {
                $('.js-followicon').click(function(e) {
                    e.preventDefault();
                    var icon = $(this);
                    var entityType = icon.data('entitytype');
                    var entity = icon.data('entity');
                    var purpose = icon.data('purpose-key');
                    if (purpose != undefined && purpose != '') {
                        purpose = '?purposeKey=' + purpose;
                    } else {
                        purpose = '';
                    }
                    icon.toggleClass('isfollowed');
                    if ( icon.hasClass('isfollowed') ) {
                        var actionType = 'POST';
                    } else {
                        var actionType = 'DELETE';
                    }
                    $.ajax({
                        url: '/api/Followings/' + entityType + '/' + entity + purpose,
                        type: actionType,
                        statusCode: {
                            201: function() {
                                icon.attr('data-followed', 'true');
                            },
                            204: function() {
                                icon.attr('data-followed', 'false');
                            },
                            500: function() {
                                {% unless suppresswarnings %}
                                alert('Error: Check your Rock security settings and try again.');
                                {% endunless %}
                            }
                        },
                        error: function() {
                            icon.toggleClass('isfollowing');
                        }
                    });
                });
            });
        {% endjavascript %}
    {% else %}
        <!-- Follow Icon Shortcode is missing entitytype and/or entity. Note: Guids or Ids must be provided  -->
    {% endif %}
{% endif %}
";

            var sql = @"
-- Update Shortcode: Follow Icon
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='1E6785C0-7D92-49A7-9E15-68E113399152')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        #endregion

        #region JC: Register block attributes for chop job in v1.17.0.32

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        private void RegisterBlockAttributesForChop()
        {
            // FinancialPledgeList's ShowAccountColumn key was pluralized by mistake.
            // Since this hasn't been released yet - delete the incorrectly keyed attribute
            // before recreating it below.
            RockMigrationHelper.DeleteAttribute( "d685afae-3f10-4c4c-a19e-f483075774f0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.SystemConfiguration", "System Configuration", "Rock.Blocks.Administration.SystemConfiguration, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "7ECDCE1B-D63F-42AA-88B6-7C5585E1F33A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.AttendanceHistoryList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.AttendanceHistoryList", "Attendance History List", "Rock.Blocks.CheckIn.AttendanceHistoryList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "8B678DC2-25E0-4589-BC3E-765BE9729BC8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Config.CheckinTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Config.CheckinTypeDetail", "Checkin Type Detail", "Rock.Blocks.CheckIn.Config.CheckinTypeDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "7D1DEC32-3A94-45B4-B567-48D9478041B9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationPreview
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SystemCommunicationPreview", "System Communication Preview", "Rock.Blocks.Communication.SystemCommunicationPreview, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "D61A57A2-C067-435F-99F6-7B6BB9534058" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonSignalList", "Person Signal List", "Rock.Blocks.Core.PersonSignalList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "DB2E3CE3-94BD-4D12-8ADD-598BF938E8E1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.AssessmentTypeDetail", "Assessment Type Detail", "Rock.Blocks.Crm.AssessmentTypeDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "83D4C6CA-A605-44D3-8BEA-99B3E881BAA0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialGatewayDetail", "Financial Gateway Detail", "Rock.Blocks.Finance.FinancialGatewayDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "68CC9376-8123-4749-ACA0-1E7ED8459704" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPledgeList", "Financial Pledge List", "Rock.Blocks.Finance.FinancialPledgeList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "8B1663EB-B5CB-4C78-B0C6-ED14E173E4C0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.RestKeyDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.RestKeyDetail", "Rest Key Detail", "Rock.Blocks.Security.RestKeyDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "AED330CA-40A4-407A-B2DC-A0C1310FDC39" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.SecurityChangeAuditList", "Security Change Audit List", "Rock.Blocks.Security.SecurityChangeAuditList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "5A2E4F3C-9915-4B67-8FFE-87056D2E68DF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvPageDetail", "Apple Tv Page Detail", "Rock.Blocks.Tv.AppleTvPageDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "D8419B3C-EDA1-46FC-9810-B1D81FB37CB3" );

            // Add/Update Obsidian Block Type
            //   Name:Apple TV Page Detail
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page Detail", "Displays the details of an Apple TV page.", "Rock.Blocks.Tv.AppleTvPageDetail", "TV > TV Apps", "ADBF3377-A491-4016-9375-346496A25FB4" );

            // Add/Update Obsidian Block Type
            //   Name:Assessment Type Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Assessment Type Detail", "Displays the details of a particular assessment type.", "Rock.Blocks.Crm.AssessmentTypeDetail", "CRM", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E" );

            // Add/Update Obsidian Block Type
            //   Name:Attendance History
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.AttendanceHistoryList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attendance History", "Block for displaying the attendance history of a person or a group.", "Rock.Blocks.CheckIn.AttendanceHistoryList", "Check-in", "68D2ABBC-3C43-4450-973F-071D1715C0C9" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Type Detail
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Config.CheckinTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Type Detail", "Displays the details of a particular Check-in Type.", "Rock.Blocks.CheckIn.Config.CheckinTypeDetail", "Check-in > Configuration", "7EA2E093-2F33-4213-A33E-9E9A7A760181" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Pledge List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Pledge List", "Displays a list of financial pledges.", "Rock.Blocks.Finance.FinancialPledgeList", "Finance", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" );

            // Add/Update Obsidian Block Type
            //   Name:Gateway Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Gateway Detail", "Displays the details of the given financial gateway.", "Rock.Blocks.Finance.FinancialGatewayDetail", "Finance", "C12C615C-384D-478E-892D-0F353E2EF180" );

            // Add/Update Obsidian Block Type
            //   Name:Person Signal List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Signal List", "Displays a list of person signals.", "Rock.Blocks.Core.PersonSignalList", "Core", "653052A0-CA1C-41B8-8340-4B13149C6E66" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Key Detail
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.RestKeyDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Key Detail", "Displays the details of a particular user login.", "Rock.Blocks.Security.RestKeyDetail", "Security", "28A34F1C-80F4-496F-A598-180974ADEE61" );

            // Add/Update Obsidian Block Type
            //   Name:Security Change Audit List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Security Change Audit List", "Block for Security Change Audit List.", "Rock.Blocks.Security.SecurityChangeAuditList", "Security", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" );

            // Add/Update Obsidian Block Type
            //   Name:System Communication Preview
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationPreview
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Communication Preview", "Create a preview and send a test message for the given system communication using the selected date and target person.", "Rock.Blocks.Communication.SystemCommunicationPreview", "Communication", "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" );

            // Add/Update Obsidian Block Type
            //   Name:System Configuration
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Configuration", "Used for making configuration changes to configurable items in the web.config.", "Rock.Blocks.Administration.SystemConfiguration", "Administration", "3855B15B-C903-446A-AE5B-891AB52851CB" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "71EFE820-B132-4AAB-A702-61486E6B2FD8" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E8C27B15-4F00-4516-A624-FBB5C26DF28F" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "7C624BAB-A392-43C8-96C8-59B62E171EF4" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: Filter Attendance By Default
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Attendance By Default", "FilterAttendanceByDefault", "Filter Attendance By Default", @"Sets the default display of Attended to Did Attend instead of [All]", 0, @"False", "EA5AF2D0-E197-4523-8ED5-D100F1C8E245" );

            // Attribute for BlockType
            //   BlockType: Check-in Type Detail
            //   Category: Check-in > Configuration
            //   Attribute: Schedule Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7EA2E093-2F33-4213-A33E-9E9A7A760181", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Schedule Page", "SchedulePage", "Schedule Page", @"Page used to manage schedules for the check-in type.", 0, @"", "1F11C34E-09D8-4FF5-A188-D84BF333DA03" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"Limit the results to pledges that match the selected accounts.", 5, @"", "237658C7-0DED-4BE1-8026-613E155B23B5" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "711B6CCF-A999-4582-B1A9-770BD9BAF963" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "200AFB5A-655F-4206-858B-59376CB96856" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "A7AE994B-4A18-48D9-9FBF-04CE9C00426A" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "9BA6DDD6-E511-4CEB-8E65-3201FDE2F715" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Hide Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Amount", "HideAmount", "Hide Amount", @"Allows the amount column to be hidden.", 6, @"False", "F9C562AE-EDC6-4B96-876D-933DBA58E675" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Limit Pledges To Current Person
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Pledges To Current Person", "LimitPledgesToCurrentPerson", "Limit Pledges To Current Person", @"Limit the results to pledges for the current person.", 4, @"False", "F2E7D073-ED8C-485B-8597-8F62203134F1" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Column", "ShowAccountColumn", "Show Account Column", @"Allows the account column to be hidden.", 1, @"True", "D685AFAE-3F10-4C4C-A19E-F483075774F0" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Filter", "ShowAccountFilter", "Show Account Filter", @"Allows account filter to be hidden.", 1, @"True", "CEEE570C-013F-47DB-99F4-D3D00C5200DC" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "Show Account Summary", @"Should the account summary be displayed at the bottom of the list?", 5, @"False", "8F62CD6A-B740-47E2-8B3F-83A3CF4E06B2" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Date Range Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "Show Date Range Filter", @"Allows date range filter to be hidden.", 2, @"True", "884EE556-66A1-4F65-B037-2CD6D0964315" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Group Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Column", "ShowGroupColumn", "Show Group Column", @"Allows the group column to be hidden.", 3, @"False", "189D6D31-8A92-43C9-A42D-DE44C663F1F9" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Date Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Date Column", "ShowLastModifiedDateColumn", "Show Last Modified Date Column", @"Allows the Last Modified Date column to be hidden.", 2, @"True", "E6FBE09B-437C-4281-89B4-2C323283BA64" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Filter", "ShowLastModifiedFilter", "Show Last Modified Filter", @"Allows last modified filter to be hidden.", 3, @"True", "6CE0FF15-C6E7-4667-ADA5-5F6D3AA71D90" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Person Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Person Filter", "ShowPersonFilter", "Show Person Filter", @"Allows person filter to be hidden.", 0, @"True", "A7F66BEA-9B90-40B4-9E86-03836EF9BF74" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "00E16ED2-BC73-41C4-BA16-471725A23547" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F6CBDC1B-B5E6-4611-9A3B-F8229E3C27EA" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "88B7ED40-401C-4BCB-90FA-94EEE4BBC6C4" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "56D7689D-3BDF-435B-9605-2F61BFCA07B1" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 5, @"", "9BA51754-B5A4-4853-A927-2215D6DB91B3" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Lava Template Append
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template Append", "LavaTemplateAppend", "Lava Template Append", @"This Lava will be appended to the system communication template to help setup any data that the template needs. This data would typically be passed to the template by a job or other means.", 6, @"", "2D757741-0B23-4583-9504-1648EB0B394A" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Future Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Future Weeks to Show", "FutureWeeksToShow", "Number of Future Weeks to Show", @"How many weeks ahead to show in the drop down.", 4, @"1", "BF904834-0030-4C50-A578-311E9942A596" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Previous Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Previous Weeks to Show", "PreviousWeeksToShow", "Number of Previous Weeks to Show", @"How many previous weeks to show in the drop down.", 3, @"6", "448BC527-3AFB-498E-8297-22A10F4CC77D" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Send Day of the Week
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "08943FF9-F2A8-4DB4-A72A-31938B200C8C", "Send Day of the Week", "SendDaysOfTheWeek", "Send Day of the Week", @"Used to determine which dates to list in the Message Date drop down. <i><strong>Note:</strong> If no day is selected the Message Date drop down will not be shown and the ‘SendDateTime’ Lava variable will be set to the current day.</i>", 1, @"", "AD747D02-F838-4C8C-8B14-2D4D14E8C1BE" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: System Communication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "System Communication", "SystemCommunication", "System Communication", @"The system communication to use when previewing the message. When set as a block setting, it will not allow overriding by the query string.", 0, @"", "85D89D8E-53E0-42E6-AF53-B75DF0914421" );
        }

        // JC: Chop blocks for v1.17.0.32
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.32",
                blockTypeReplacements: new Dictionary<string, string> {
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
                { "C96479B6-E309-4B1A-B024-1F1276122A13", "MaximumNumberOfDocuments" } // Benevolence Type Detail
            } );
        }

        #endregion
    }
}
