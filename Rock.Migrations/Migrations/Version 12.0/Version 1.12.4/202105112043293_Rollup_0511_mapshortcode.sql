UPDATE [LavaShortcode] SET [Markup]=N'{% capture singleQuote %}''{% endcapture %}
{% capture escapedQuote %}\''{% endcapture %}
{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}
{% assign url = ''key='' | Append:apiKey %}
{% assign id = uniqueid | Replace:''-'','''' %}

{% if apiKey == "" %}
    <div class="alert alert-warning">
        There is no Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% if center == "" and markerCount == 1 -%}
    {% assign centerPoint = markers | First %}
    {% assign center = centerPoint.location %}
{% endif %}

{% if zoom == "" and markerCount == 1 -%}
    {% assign zoom = ''11'' %}
{% endif %}

{% javascript id:''googlemapsapi'' url:''{{ "https://maps.googleapis.com/maps/api/js?key=" | Append:apiKey }}'' %}{% endjavascript %}

{% case markeranimation %}
{% when ''drop'' %}
    {% assign markeranimation = ''google.maps.Animation.DROP'' %}
{% when ''bounce'' %}
    {% assign markeranimation = ''google.maps.Animation.BOUNCE'' %}
{% else %}
    {% assign markeranimation = ''null'' %}
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

<div class="map-container {{ id }}">
    <div id="map-container-{{ id }}"></div>
	<div id="{{ id }}"></div>
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
        
        {% if center != "" -%}
    	var centerLatlng = new google.maps.LatLng( {{ center }} );
    	{% endif -%}
    	
    	var mapOptions = {
    		{% if zoom != "" -%}
    		    zoom: {{ zoom }},
    		{% endif -%}
    		scrollwheel: {{ scrollwheel }},
    		draggable: {{ draggable }},
    		{% if center != "" -%}
    		    center: centerLatlng,
    		{% endif -%}
    		mapTypeId: ''{{ maptype }}'',
    		zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: ''{{ gesturehandling }}'',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
    		{% if style and style.content != "" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
	    }

		var map = new google.maps.Map(document.getElementById(''{{ id }}''), mapOptions);
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
        {% if center == "" and zoom == "" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}
        

		//Resize Function
		google.maps.event.addDomListener(window, "resize", function() {
			var center = map.getCenter();
			google.maps.event.trigger(map, "resize");
			map.setCenter(center);
		});
	}


    google.maps.event.addDomListener(window, ''load'', initialize{{ id }});

</script>' WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')