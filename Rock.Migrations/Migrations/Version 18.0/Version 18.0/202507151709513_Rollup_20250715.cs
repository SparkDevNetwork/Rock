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
    public partial class Rollup_20250715 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddFallbackChatNotificationSystemCommunication_20250714_Up();
            NA_DeprecateEditLabelBlockUp();
            UpdateMarkupUp();
            UpdateDocumentationUp();
            UpdateParametersUp();
            ObsoleteFunction_ufnUtility_CsvToTable();
            UpdateGroupTypeScheduleConfirmationEmailUp();
            UpdateLearningActivityAvailableSystemCommunicationUp();
            UpdateContentChannelInteractionComponentEntityTypeIdUp();
            WindowsCheckinClientDownloadLinkUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            NA_DeprecateEditLabelBlockDown();
            UpdateMarkupDown();
            UpdateDocumentationDown();
            UpdateParametersDown();
            UpdateLearningActivityAvailableSystemCommunicationDown();
            UpdateContentChannelInteractionComponentEntityTypeIdDown();
            WindowsCheckinClientDownloadLinkDown();
        }

        #region JPH: Adds the Fallback Chat Notification System Communication.

        private void JPH_AddFallbackChatNotificationSystemCommunication_20250714_Up()
        {
            RockMigrationHelper.UpdateSystemCommunication(
                category: "System",
                title: "Fallback Chat Notification",
                from: "",
                fromName: "",
                to: "",
                cc: "",
                bcc: "",
                subject: "Secured Chat Message from {{ SenderPerson.FullName }} at {{ 'Global' | Attribute:'OrganizationName'}}",
                body: @"{% capture chatUrl %}
  {{ 'Global' | Attribute:'PublicApplicationRoot' }}chat?SelectedChannelId={{ Channel.IdKey }}
{% endcapture %}

{{ 'Global' | Attribute:'EmailHeader' }}

<h1>New Chat Message</h1>

<p>Hi {{ RecipientPerson.NickName }},</p>

<p>You have a secured chat message from {{ SenderPerson.FullName }}:</p>

<p><i>{{ Message }}</i></p>

<p>Join the conversation: <a href=""{{ chatUrl }}"">{{ chatUrl }}</a></p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                guid: "007C9AA2-057C-412F-A1F1-04805300D523",
                smsMessage: @"You have a secured chat message from {{ SenderPerson.FullName }} at {{ 'Global' | Attribute:'OrganizationName'}}.

Join the conversation: {{ 'Global' | Attribute:'PublicApplicationRoot' }}chat?SelectedChannelId={{ Channel.IdKey }}"
            );
        }

        #endregion

        #region NA: Deprecate Edit Label Block

        private void NA_DeprecateEditLabelBlockUp()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Edit Label (Legacy)'
WHERE GUID = '5ACB281A-CE85-426F-92A6-771F3B8AEF8A'
" );
        }

        private void NA_DeprecateEditLabelBlockDown()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Edit Label'
WHERE GUID = '5ACB281A-CE85-426F-92A6-771F3B8AEF8A'
" );
        }

        #endregion

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
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?loading=async&key=''%}
{% else %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key=''%}
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
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?loading=async&key=''%}
{% else %}
    {% assign googleMapsUrl =''https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key=''%}
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

        #region NA: 255_MigrationRollupsForV17_2_1

        #region Up Methods

        #region NA: Re-obsolete the ufnUtility_CsvToTable function
        private void ObsoleteFunction_ufnUtility_CsvToTable()
        {
            Sql( @"
/*
<doc>
	<summary>

        *** THIS FUNCTION IS OBSOLETE.  PLEASE USE STRING_SPLIT(@YourString, ',') INSTEAD. ***

 		This function converts a comma-delimited string of values into a table of values
        The original version came from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
        (Previously) Used by:
            * spFinance_ContributionStatementQuery
            * spFinance_GivingAnalyticsQuery_AccountTotals
            * spFinance_GivingAnalyticsQuery_PersonSummary
            * spFinance_GivingAnalyticsQuery_TransactionData
            * spCheckin_AttendanceAnalyticsQuery_AttendeeDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance
            * spCheckin_AttendanceAnalyticsQuery_Attendees
            * spCheckin_AttendanceAnalyticsQuery_NonAttendees
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/

/* #Obsolete# - Use STRING_SPLIT() instead */

ALTER FUNCTION [dbo].[ufnUtility_CsvToTable] 
(
 @pString VARCHAR(MAX)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT Item = TRY_CONVERT(INT, LTRIM(RTRIM(value)))
    FROM STRING_SPLIT(@pString, ',');
" );
        }

        #endregion

        #region SC: New SystemCommunication - Scheduling Confirmation Email (One Button)
        private void UpdateGroupTypeScheduleConfirmationEmailUp()
        {
            Sql( @"DECLARE @EntityTypeId_SystemCommunication INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D0CAD7C0-10FE-41EF-B89D-E6F0D22456C4');
DECLARE @CategoryId INT = (SELECT TOP 1 [Id] FROM [Category] WHERE [EntityTypeId] = @EntityTypeId_SystemCommunication AND [Name] = 'Groups');
DECLARE @Guid UNIQUEIDENTIFIER = 'BA1716E0-6B31-4E93-ABA1-42B3C81FDBDC';
DECLARE @Title NVARCHAR(100) = N'Scheduling Confirmation Email (One Button)';
DECLARE @Subject NVARCHAR(1000) = N'Scheduling Confirmation';
DECLARE @Body NVARCHAR(MAX) = N'{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you''ll be attending as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><h5>{{attendance.Occurrence.OccurrenceDate | Date:''dddd, MMMM d, yyyy''}}</h5></td></tr>
    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
    {% if forloop.first  %}
    <tr><td>
        <!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}"" style=""height:38px;v-text-anchor:middle;width:275px;"" arcsize=""5%"" strokecolor=""#009ce3"" fillcolor=""#33cfe3"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">Required: Confirm or Decline</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#009ce3;border:1px solid ##33cfe3;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:275px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}"">Required: Confirm or Decline</a>&nbsp;
    </td></tr>
    <tr><td>&nbsp;</td></tr>
	{% endif %}
{% endfor %}
</table>

<br/>

{{ ''Global'' | Attribute:''EmailFooter'' }}';

IF NOT EXISTS (
    SELECT 1
    FROM [SystemCommunication]
    WHERE [Guid] = @Guid
)
BEGIN
	INSERT INTO [SystemCommunication] ([IsSystem], [IsActive], [CategoryId], [Guid], [CssInliningEnabled], [Title], [Subject], [Body])
	VALUES (1, 1, @CategoryId, @Guid, 0, @Title, @Subject, @Body);
END;

DECLARE @SystemCommunicationId INT = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = @Guid);

UPDATE [GroupType] SET [ScheduleConfirmationSystemCommunicationId] = @SystemCommunicationId WHERE [ScheduleConfirmationSystemCommunicationId] IS NOT NULL;
    " );
        }

        #endregion

        #endregion

        #endregion

        #region KH: Update the Learning Activity Available System Communication

        private void UpdateLearningActivityAvailableSystemCommunicationUp()
        {
            // d40a9c32-f179-4e5e-9b0d-ce208c5d1870 is Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
UPDATE [SystemCommunication]
    SET Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your available activities as of {{ currentDate }}.
</p>

<h2 style=""margin:0; margin-bottom: 16px"">
	{{ Program.ProgramName }}
</h2>
{% for course in Courses %}
	{% assign availableActivities = course.Classes | SelectMany:''Activities'' %}
	{% assign availableActivitiesCount = availableActivities | Size %}
	{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}
	<h3 style=""margin: 0 0 0 16px;"">{{ course.CourseName }} {% if course.CourseCode and course.CourseCode != empty %}- {{ course.CourseCode }}{% endif %} </h3>

	{% for class in course.Classes %}
		{% assign orderedActivities = class.Activities | OrderBy:''Order'' %}
		{% for activity in orderedActivities %}
			<p style=""margin: 16px 0 16px 32px;"">
				<strong>Activity:</strong>
				<a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}learn/{{ Program.ProgramIdKey }}/courses/{{ course.CourseIdKey }}/{{ class.ClassIdKey }}?activity={{ activity.LearningClassActivityIdKey }}"">{{ activity.ActivityName }}</a>
					{% if activity.AvailableDate and activity.AvailableDate != empty %}
						(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
					{% endif %}
				<br />
				{% if activity.DueDate and activity.DueDate != empty %}
					<strong>Due:</strong>
					{{ activity.DueDate | HumanizeDateTime }}
				{% endif %}
			</p>
		{% endfor %}
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
',
    SMSMessage = '
New {{ Program.ProgramName }} {%if ActivityCount > 1 %}Activities{%else%}Activity{%endif%} Available:{% for course in Courses %}{% assign availableActivities = course.Classes | SelectMany:''Activities'' %}{% assign availableActivitiesCount = availableActivities | Size %}{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}

{{ course.CourseName }}{% if course.CourseCode and course.CourseCode != empty %} - {{ course.CourseCode }}{% endif %}:{% for class in course.Classes %}{% assign orderedActivities = class.Activities | OrderBy:''Order'' %}{% for activity in orderedActivities %}

Activity: {{ activity.ActivityName }}
{% if activity.AvailableDate and activity.AvailableDate != empty %}Available: {{ activity.AvailableDate | Date: ''MMM d'' }}{% endif %}{% if activity.DueDate and activity.DueDate != empty %}{% if activity.AvailableDate and activity.AvailableDate != empty %} | {% endif %}Due: {{ activity.DueDate | HumanizeDateTime }} {% endif %}Link: {{ ''Global'' | Attribute:''PublicApplicationRoot'' }}learn/{{ Program.ProgramIdKey }}/courses/{{ course.CourseIdKey }}/{{ class.ClassIdKey }}?activity={{ activity.LearningClassActivityIdKey }}{% endfor %}{% endfor %}{% endfor %}
'
WHERE [Guid] = @activityCommunicationGuid;" );
        }

        private void UpdateLearningActivityAvailableSystemCommunicationDown()
        {
            // d40a9c32-f179-4e5e-9b0d-ce208c5d1870 is Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
UPDATE [SystemCommunication]
    SET Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	{% assign availableActivitiesCount = course.Activities | Size %}
	{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}
	<h2> {{ course.ProgramName }}: {{ course.CourseName }} {% if course.CourseCode and course.CourseCode != empty %}- {{ course.CourseCode }}{% endif %} </h2>

	{% assign orderedActivities = course.Activities | OrderBy:''Order'' %}
	{% for activity in orderedActivities %}
		<p class=""mb-4"">
			<strong>Activity:</strong>
			{{ activity.ActivityName }}
				{% if activity.AvailableDate and activity.AvailableDate != empty %}
					(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
				{% endif %}
			<br />
			{% if activity.DueDate and activity.DueDate != empty %}
			    <strong>Due:</strong>
				{{ activity.DueDate | HumanizeDateTime }}
			{% endif %}
		</p>	
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
',
    SMSMessage = NULL
WHERE [Guid] = @activityCommunicationGuid;" );
        }

        #endregion

        #region MSE - Fix InteractionChannel ComponentEntityTypeId from ContentChannel (209) to ContentChannelItem (208) for ContentChannelItem interactions

        private void UpdateContentChannelInteractionComponentEntityTypeIdUp()
        {
            Sql( @"
UPDATE [InteractionChannel]
SET [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = 'BF12AE64-21FB-433B-A8A4-E40E8C426DDA'
)
WHERE [ChannelTypeMediumValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
)
AND [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = '44484685-477E-4668-89A6-84F29739EB68'
);" );
        }

        private void UpdateContentChannelInteractionComponentEntityTypeIdDown()
        {
            // Revert InteractionChannel.ComponentEntityTypeId back to ContentChannel (209) from ContentChannelItem (208)
            // for all ContentChannel interaction channels
            Sql( @"
UPDATE [InteractionChannel]
SET [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = '44484685-477E-4668-89A6-84F29739EB68'
)
WHERE [ChannelTypeMediumValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
)
AND [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = 'BF12AE64-21FB-433B-A8A4-E40E8C426DDA'
);" );
        }

        #endregion

        #region DH: Update Windows Check-in Download Link

        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
        DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

        UPDATE [AttributeValue]
        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/17.3/checkinclient.msi'
        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
        DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

        UPDATE [AttributeValue]
        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.16.7/checkinclient.msi'
        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        #endregion
    }
}
