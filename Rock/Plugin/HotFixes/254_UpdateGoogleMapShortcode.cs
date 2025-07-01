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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 254, "17.2" )]
    public class UpdateGoogleMapShortcode : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateMarkupUp();
            UpdateDocumentationUp();
            UpdateParametersUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateMarkupDown();
            UpdateDocumentationDown();
            UpdateParametersDown();
        }

        #region ME: Update Google Maps Shortcode To Address New Options From Google Maps API

        #region Up Methods

        private void UpdateMarkupUp()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Markup]=N'{% capture singleQuote %}''{% endcapture %}
{% capture escapedQuote %}\''{% endcapture %}
{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}
{% assign url = ''key='' | Append:apiKey %}
{% assign id = uniqueid | Replace:''-'','''' %}
{% assign mapId = mapid | Trim %}
{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
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
        {% assign mapZoom = ''11'' %}
    {% else %}
        {% assign mapZoom = ''10'' %}
    {% endif %}
{% endif %}
{% if mapId == """" %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?loading=async&key='' %}
{% else %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key='' %}
{% endif %}
{% javascript id:''googlemapsapi'' url:''{{ googleMapsUrl  | Append:apiKey }}'' %}{% endjavascript %}
{% if mapId == """" %}
    {% case markeranimation %}
    {% when ''drop'' %}
        {% assign markeranimation = ''google.maps.Animation.DROP'' %}
    {% when ''bounce'' %}
        {% assign markeranimation = ''google.maps.Animation.BOUNCE'' %}
    {% else %}
        {% assign markeranimation = ''null'' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when ''drop'' %}
        {% assign markeranimation = ''drop'' %}
    {% when ''bounce'' %}
        {% assign markeranimation = ''bounce'' %}
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
            {% assign title = '''' -%}
            {% assign content = '''' -%}
            {% assign icon = '''' -%}
            {% assign location = marker.location | Split:'','' -%}
            {% if marker.title and marker.title != '''' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '''' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '''' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},''{{ title }}'',''{{ content }}'',''{{ icon }}''],
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
            mapTypeId: ''{{ maptype }}'',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            cameraControl: {{ cameracontrol }},
            gestureHandling: ''{{ gesturehandling }}'',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
            {% if mapId != """" %}
	            ,mapId: ''{{ mapId }}''
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById(''{{ id }}''), mapOptions);
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
                if (markers{{ id }}[i][4] != ''''){
                    const glyph = document.createElement(''img'');
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
                        background: ''#FE7569'',
                        borderColor: ''#000'',
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
		            {% if markeranimation == ''drop'' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener(''animationend'', (event) => {
                            content.classList.remove(''{{ markeranimation }}'');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add(''{{ markeranimation }}'');
                {% endif -%}
            {% endif %}
            // Add info window to marker
            google.maps.event.addListener(marker, ''click'', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''''){
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
    window.addEventListener(''load'', initialize{{ id }});
</script>
'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        private void UpdateDocumentationUp()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation]=N'<p>
  Adding a Google map to your page always starts out sounding easy… until… you
  get to the details. Soon the whole day is wasted and you don''t have much to
  show. This shortcode makes it easy to add responsive Google Maps to your site.
  Let''s start with a simple example and work our way to more complex use cases.
</p>
<p>
  Note: Due to the javascript requirements of this shortcode you will need to do
  a full page reload before changes to the shortcode appear on your page.
</p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
  {[ endgooglemap ]}
</pre>
<p>
  In the example above we mapped a single point to our map. Pretty easy, but not
  very helpful. We can add additional points by providing more markers.
</p>
<pre>
{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
{[ endgooglemap ]}
</pre>
<p>
  Ok… we''re getting there, but what if we wanted titles and information windows
  for our markers, oh and custom markers too? You can provide optional
  parameters for each marker as shown below.
</p>
<pre>
[[ marker location:''latitude,longitude'' title:''My Title'' icon:''icon url'' ]] info window content [[ endmarker ]]</pre>
<p><strong>Example:</strong></p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' title:''Spark Global Headquarters'']]
          &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;
          It''s not as grand as it sounds.&lt;br&gt;
          &lt;img src=""https://rockrms.blob.core.windows.net/misc/spark-logo.png"" width=""179"" height=""47""&gt;
      [[ endmarker ]]
      [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
  {[ endgooglemap ]}
</pre>
<p></p>
<p>Note: A list of great resources for custom map markers is below:</p>
<ul>
  <li><a href=""http://map-icons.com/"">Map Icons</a></li>
  <li><a href=""https://mapicons.mapsmarker.com/"">Map Icons Collection</a></li>
  <li>
    <a href=""https://github.com/Concept211/Google-Maps-Markers""
      >Google Maps Markers</a
    >
  </li>
</ul>
<p>
  There are several other parameters for you to use to control the options on
  your map. They include:
</p>
<ul>
  <li><strong>height</strong> (600px) – The height of the map.</li>
  <li><strong>width</strong> (100%) – The responsive width of the map.</li>
  <li>
    <strong>zoom</strong> (optional) – The zoom level of the map. Note when two
    or more points are provided the map will auto zoom to place all of the
    points on the map. The range of the zoom scale is 1 (the furthest out,
    largest) to 20 (the closest, smallest). The approximate zoom levels are:
    <ul>
      <li>1 = world</li>
      <li>5 = continent</li>
      <li>10 = city</li>
      <li>15 = streets</li>
      <li>20 = buildings</li>
    </ul>
  </li>
  <li>
    <strong>center</strong> (optional) – The center point on the map. If you do
    not provide a center a default will be calculated based on the points given.
  </li>
  <li>
    <strong>maptype</strong> (roadmap) – The type of map to display. The options
    are ''roadmap'', ''hybrid'', ''satellite'' or ''terrain''.
  </li>
  <li>
    <strong>showzoom</strong> (true) – Should the zoom control be displayed.
  </li>
  <li>
    <strong>showstreetview</strong> (false) – Should he StreetView control be
    displayed.
  </li>
  <li>
    <strong>showfullscreen</strong> (true) – Should the control to show the map
    full screen be displayed.
  </li>
  <li>
    <strong>showmapttype</strong> (false) – Should the control to change the map
    type be shown.
  </li>
  <li>
    <strong>markeranimation</strong> (none) – The marker animation type. Options
    include: ''none'', ''bounce'' (markers bounce continuously) or ''drop'' (markers
    drop in).
  </li>
  <li>
    <strong>scrollwheel</strong> (true) – Determines if the scroll wheel should
    control the zoom level when the mouse is over the map.
  </li>
  <li>
    <strong>draggable</strong> (true) – Determines if the mouse should be
    allowed to drag the center point of the map (allow the map to be moved).
  </li>
  <li>
    <strong>gesturehandling</strong> (cooperative) – Configures how the map responds to user gestures. In ''cooperative'' mode, the map will not zoom when a user scrolls over it unless they hold down [Ctrl] (Windows/Linux) or [⌘ Command] (Mac), or use a two-finger gesture on touch devices. This prevents unintentional zooming when users are scrolling the page. For more aggressive behavior, use ''greedy'' to allow immediate zooming and panning, or ''none'' to disable all gesture interactions.
  </li>
  <li>
    <strong>cameracontrol</strong> (false) – Enables or disables user interaction with the map camera, such as changing zoom, tilt, heading, or center. When set to false, all camera movements initiated by user gestures (e.g. dragging, zooming, rotating) are disabled, effectively locking the map''s view. This is useful for static or presentation-style maps where you want to prevent user navigation. When set to true (default), users can freely control the camera using gestures and controls.
  </li>
</ul>
<p>
  As you can see there are a lot of options in working with your map. You can
  also style your map by changing the colors. You do this by providing the
  styling information in a separate [[ style ]] section. The styling settings
  for Google Maps is not pretty to look at or configure for that matter.
  Luckily, there are several sites that allow you to download preconfigured map
  styles. Two of the best are called
  <a href=""https://snazzymaps.com"">SnazzyMaps</a> and
  <a href=""https://mapstyle.withgoogle.com"">Map Style</a>. Below is an example
  showing how to add styling to your maps.
</p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
      [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
      [[ style ]]
          [{""featureType"":""all"",""elementType"":""all"",""stylers"":[{""visibility"":""on""}]},{""featureType"":""all"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""},{""saturation"":""-100""}]},{""featureType"":""all"",""elementType"":""labels.text.fill"",""stylers"":[{""saturation"":36},{""color"":""#000000""},{""lightness"":40},{""visibility"":""off""}]},{""featureType"":""all"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""},{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""all"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""administrative"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""administrative"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#000000""},{""lightness"":17},{""weight"":1.2}]},{""featureType"":""landscape"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""landscape"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape.natural"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry"",""stylers"":[{""lightness"":21}]},{""featureType"":""poi"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""road"",""elementType"":""geometry"",""stylers"":[{""visibility"":""on""},{""color"":""#7f8d89""}]},{""featureType"":""road"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.highway"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":17}]},{""featureType"":""road.highway"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":29},{""weight"":0.2}]},{""featureType"":""road.arterial"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":18}]},{""featureType"":""road.arterial"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.arterial"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""road.local"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""transit"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":19}]},{""featureType"":""water"",""elementType"":""all"",""stylers"":[{""color"":""#2b3638""},{""visibility"":""on""}]},{""featureType"":""water"",""elementType"":""geometry"",""stylers"":[{""color"":""#2b3638""},{""lightness"":17}]},{""featureType"":""water"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.fill"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]}]
      [[ endstyle]]
  {[ endgooglemap ]}
</pre>
<p>
  Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply
  copy and pasted straight from SnazzyMaps!
</p>
'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        private void UpdateParametersUp()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Parameters]=N'styles^|height^600px|width^100%|zoom^|center^|maptype^roadmap|showzoom^true|showstreetview^false|showfullscreen^true|showmaptype^false|markeranimation^none|gesturehandling^cooperative|cameracontrol^false'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        #endregion

        #region Down Methods

        private void UpdateMarkupDown()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Markup]=N'{% capture singleQuote %}''{% endcapture %}
{% capture escapedQuote %}\''{% endcapture %}
{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}
{% assign url = ''key='' | Append:apiKey %}
{% assign id = uniqueid | Replace:''-'','''' %}
{% assign mapId = mapid | Trim %}
{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
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
        {% assign mapZoom = ''11'' %}
    {% else %}
        {% assign mapZoom = ''10'' %}
    {% endif %}
{% endif %}
{% if mapId == """" %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?loading=async&key='' %}
{% else %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key='' %}
{% endif %}
{% javascript id:''googlemapsapi'' url:''{{ googleMapsUrl  | Append:apiKey }}'' %}{% endjavascript %}
{% if mapId == """" %}
    {% case markeranimation %}
    {% when ''drop'' %}
        {% assign markeranimation = ''google.maps.Animation.DROP'' %}
    {% when ''bounce'' %}
        {% assign markeranimation = ''google.maps.Animation.BOUNCE'' %}
    {% else %}
        {% assign markeranimation = ''null'' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when ''drop'' %}
        {% assign markeranimation = ''drop'' %}
    {% when ''bounce'' %}
        {% assign markeranimation = ''bounce'' %}
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
            {% assign title = '''' -%}
            {% assign content = '''' -%}
            {% assign icon = '''' -%}
            {% assign location = marker.location | Split:'','' -%}
            {% if marker.title and marker.title != '''' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '''' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '''' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},''{{ title }}'',''{{ content }}'',''{{ icon }}''],
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
            mapTypeId: ''{{ maptype }}'',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: ''{{ gesturehandling }}'',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
            {% if mapId != """" %}
	            ,mapId: ''{{ mapId }}''
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById(''{{ id }}''), mapOptions);
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
                if (markers{{ id }}[i][4] != ''''){
                    const glyph = document.createElement(''img'');
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
                        background: ''#FE7569'',
                        borderColor: ''#000'',
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
		            {% if markeranimation == ''drop'' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener(''animationend'', (event) => {
                            content.classList.remove(''{{ markeranimation }}'');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add(''{{ markeranimation }}'');
                {% endif -%}
            {% endif %}
            // Add info window to marker
            google.maps.event.addListener(marker, ''click'', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''''){
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
    window.addEventListener(''load'', initialize{{ id }});
</script>
'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        private void UpdateDocumentationDown()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation]=N'<p>
  Adding a Google map to your page always starts out sounding easy… until… you
  get to the details. Soon the whole day is wasted and you don''t have much to
  show. This shortcode makes it easy to add responsive Google Maps to your site.
  Let''s start with a simple example and work our way to more complex use cases.
</p>
<p>
  Note: Due to the javascript requirements of this shortcode you will need to do
  a full page reload before changes to the shortcode appear on your page.
</p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
  {[ endgooglemap ]}
</pre>
<p>
  In the example above we mapped a single point to our map. Pretty easy, but not
  very helpful. We can add additional points by providing more markers.
</p>
<pre>
{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
{[ endgooglemap ]}
</pre>
<p>
  Ok… we''re getting there, but what if we wanted titles and information windows
  for our markers, oh and custom markers too? You can provide optional
  parameters for each marker as shown below.
</p>
<pre>
[[ marker location:''latitude,longitude'' title:''My Title'' icon:''icon url'' ]] info window content [[ endmarker ]]</pre>
<p><strong>Example:</strong></p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' title:''Spark Global Headquarters'']]
          &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;
          It''s not as grand as it sounds.&lt;br&gt;
          &lt;img src=""https://rockrms.blob.core.windows.net/misc/spark-logo.png"" width=""179"" height=""47""&gt;
      [[ endmarker ]]
      [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
  {[ endgooglemap ]}
</pre>
<p></p>
<p>Note: A list of great resources for custom map markers is below:</p>
<ul>
  <li><a href=""http://map-icons.com/"">Map Icons</a></li>
  <li><a href=""https://mapicons.mapsmarker.com/"">Map Icons Collection</a></li>
  <li>
    <a href=""https://github.com/Concept211/Google-Maps-Markers""
      >Google Maps Markers</a
    >
  </li>
</ul>
<p>
  There are several other parameters for you to use to control the options on
  your map. They include:
</p>
<ul>
  <li><strong>height</strong> (600px) – The height of the map.</li>
  <li><strong>width</strong> (100%) – The responsive width of the map.</li>
  <li>
    <strong>zoom</strong> (optional) – The zoom level of the map. Note when two
    or more points are provided the map will auto zoom to place all of the
    points on the map. The range of the zoom scale is 1 (the furthest out,
    largest) to 20 (the closest, smallest). The approximate zoom levels are:
    <ul>
      <li>1 = world</li>
      <li>5 = continent</li>
      <li>10 = city</li>
      <li>15 = streets</li>
      <li>20 = buildings</li>
    </ul>
  </li>
  <li>
    <strong>center</strong> (optional) – The center point on the map. If you do
    not provide a center a default will be calculated based on the points given.
  </li>
  <li>
    <strong>maptype</strong> (roadmap) – The type of map to display. The options
    are ''roadmap'', ''hybrid'', ''satellite'' or ''terrain''.
  </li>
  <li>
    <strong>showzoom</strong> (true) – Should the zoom control be displayed.
  </li>
  <li>
    <strong>showstreetview</strong> (false) – Should he StreetView control be
    displayed.
  </li>
  <li>
    <strong>showfullscreen</strong> (true) – Should the control to show the map
    full screen be displayed.
  </li>
  <li>
    <strong>showmapttype</strong> (false) – Should the control to change the map
    type be shown.
  </li>
  <li>
    <strong>markeranimation</strong> (none) – The marker animation type. Options
    include: ''none'', ''bounce'' (markers bounce continuously) or ''drop'' (markers
    drop in).
  </li>
  <li>
    <strong>scrollwheel</strong> (true) – Determines if the scroll wheel should
    control the zoom level when the mouse is over the map.
  </li>
  <li>
    <strong>draggable</strong> (true) – Determines if the mouse should be
    allowed to drag the center point of the map (allow the map to be moved).
  </li>
  <li>
    <strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ''cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.
  </li>
</ul>
<p>
  As you can see there are a lot of options in working with your map. You can
  also style your map by changing the colors. You do this by providing the
  styling information in a separate [[ style ]] section. The styling settings
  for Google Maps is not pretty to look at or configure for that matter.
  Luckily, there are several sites that allow you to download preconfigured map
  styles. Two of the best are called
  <a href=""https://snazzymaps.com"">SnazzyMaps</a> and
  <a href=""https://mapstyle.withgoogle.com"">Map Style</a>. Below is an example
  showing how to add styling to your maps.
</p>
<pre>
  {[ googlemap ]}
      [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
      [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
      [[ style ]]
          [{""featureType"":""all"",""elementType"":""all"",""stylers"":[{""visibility"":""on""}]},{""featureType"":""all"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""},{""saturation"":""-100""}]},{""featureType"":""all"",""elementType"":""labels.text.fill"",""stylers"":[{""saturation"":36},{""color"":""#000000""},{""lightness"":40},{""visibility"":""off""}]},{""featureType"":""all"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""},{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""all"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""administrative"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""administrative"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#000000""},{""lightness"":17},{""weight"":1.2}]},{""featureType"":""landscape"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""landscape"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape.natural"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry"",""stylers"":[{""lightness"":21}]},{""featureType"":""poi"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""road"",""elementType"":""geometry"",""stylers"":[{""visibility"":""on""},{""color"":""#7f8d89""}]},{""featureType"":""road"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.highway"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":17}]},{""featureType"":""road.highway"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":29},{""weight"":0.2}]},{""featureType"":""road.arterial"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":18}]},{""featureType"":""road.arterial"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.arterial"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""road.local"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""transit"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":19}]},{""featureType"":""water"",""elementType"":""all"",""stylers"":[{""color"":""#2b3638""},{""visibility"":""on""}]},{""featureType"":""water"",""elementType"":""geometry"",""stylers"":[{""color"":""#2b3638""},{""lightness"":17}]},{""featureType"":""water"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.fill"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]}]
      [[ endstyle]]
  {[ endgooglemap ]}
</pre>
<p>
  Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply
  copy and pasted straight from SnazzyMaps!
</p>
'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        private void UpdateParametersDown()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Parameters]=N'styles^|height^600px|width^100%|zoom^|center^|maptype^roadmap|showzoom^true|showstreetview^false|showfullscreen^true|showmaptype^false|markeranimation^none|gesturehandling^cooperative'
WHERE [GUID] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        #endregion

        #endregion
    }
}