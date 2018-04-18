-- delete existing shortcodes
DELETE FROM [LavaShortcode] WHERE [Guid] IN ('D8B431AA-13EA-4F5F-9D6E-85B6D26EB72A', 'B579B014-735E-4D9C-9DEF-00D4FBCECE21', '4284B1C9-E73D-4162-8AEE-13A03CAF2938')

-- YouTube
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('YouTube','Creates a responsive YouTube embed from just a simple video id.','<p>Embedding a YouTube video is easy, right? Well what if you want it to be responsive (adjust with the size of the window)? Or what about 
control of what is shown in the player? The YouTube shortcode helps to shorten (see what we did there) the time it takes to get a video 
up on your Rock site. Here’s how:<br>  </p>

<p>Basic Usage:</p>

<pre>{[ youtube id:''8kpHK4YIwY4'' ]}</pre>


<p>This will put the video with the id provide onto your page in a responsive container. The id can be found in the address of the 
YouTube video. There are also a couple of options for you to add:</p>

<ul>
    <li><b>id</b> (required) – The YouTube id of the video.</li>
    <li><b>width</b> (100%) – The width you would like the video to be. By default it will be 100% but you can provide any width in percentages, pixels, or any other valid CSS unit of measure.</li>
    <li><b>showinfo</b> (false) – This determines if the name of the video and other information should be shown at the top of the video.</li><li><b>controls</b> (true) – This 
        determines if the standard YouTube controls should be shown.</li>
    <li><b>autoplay</b> (false) – Should the video start playing once the page is loaded?</li>
</ul>',1,1,'youtube','{% assign wrapperId = uniqueid %}

{% assign parts = id | Split:''/'' %}
{% assign id = parts | Last %}
{% assign parts = id | Split:''='' %}
{% assign id = parts | Last | Trim %}

{% assign url = ''https://www.youtube.com/embed/'' | Append:id | Append:''?rel=0'' %}

{% assign showinfo = showinfo | AsBoolean %}
{% assign controls = controls | AsBoolean %}
{% assign autoplay = autoplay | AsBoolean %}

{% if showinfo %}
    {% assign url = url | Append:''&showinfo=1'' %}
{% else %}
    {% assign url = url | Append:''&showinfo=0'' %}
{% endif %}

{% if controls %}
    {% assign url = url | Append:''&controls=1'' %}
{% else %}
    {% assign url = url | Append:''&controls=0'' %}
{% endif %}

{% if autoplay %}
    {% assign url = url | Append:''&autoplay=1'' %}
{% else %}
    {% assign url = url | Append:''&autoplay=0'' %}
{% endif %}

<style>

#{{ wrapperId }} {
    width: {{ width }};
}

.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; } 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
</style>

<div id=''{{ wrapperId }}''>
    <div class=''embed-container''><iframe src=''{{ url }}'' frameborder=''0'' allowfullscreen></iframe></div>
</div>',1,'','id^|showinfo^false|controls^true|autoplay^false|width^100%','2FA4D446-3F63-4DFD-8C6A-55DBA76AEB83')
 
 
-- Parallax
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Parallax','Add a scrolling background to a section of your page.','<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>

<pre>{[ parallax image:''http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'' contentpadding:''20px'' ]}
    &lt;h1&gt;Hello World&lt;/h1&gt
{[ endparallax ]}</pre>


<p>  
    This shotcode takes the content you provide it and places it into a div with a parallax background using the image you provide in the ''image'' 
    parameter. As always there are several parameters.
</p>
    
<ul>
    <li><b>image</b> (required) – A valid URL to the image that should be used as the background.</li><li><b>height</b> (200px) – The minimum height of the content. This is useful if you want your section to not have any 
    content, but instead be just the parallax image.</li>
    <li><b>speed</b> (20) – the speed that the background should scroll. The value of 0 means the image will be fixed in place, while the value of 100 would make the background scroll at the same speed as the page content.</li>
    <li><b>zindex</b> (1) – The z-index of the background image. Depending on your design you may need to adjust the z-index of the parallax image. </li>
    <li><b>position</b> (center center) - This is analogous to the background-position css property. Specify coordinates as top, bottom, right, left, center, or pixel values (e.g. -10px 0px). The parallax image will be positioned as close to these values as possible while still covering the target element.</li>
    <li><b>contentpadding</b> (0) – The amount of padding you’d like to have around your content. You can provide any valid CSS padding value. For example, the value ‘200px 20px’ would give you 200px top and bottom and 20px left and right.</li>
    <li><b>contentcolor</b> (#fff = white) – The font color you’d like to use for your content. This simplifies the styling of your content.</li>
    <li><b>contentalign</b> (center) – The alignment of your content inside of the section. </li>
</ul>

<p>Note: Do to this javascript requirements of this shortcode you will need to do a full page reload before changes to the shortcode appear on your page.</p>',1,1,'parallax','{{ ''https://cdnjs.cloudflare.com/ajax/libs/parallax.js/1.4.2/parallax.min.js'' | AddScriptLink }}
{% assign id = uniqueid %} 
{% assign bodyZindex = zindex | Plus:1 %}
{% assign speed = speed | AsInteger | DividedBy:100,2 %}

<div id="{{ id }}" class="parallax-window" data-parallax="scroll" data-z-index="{{ zindex }}" data-speed="{{ speed }}" data-position="{{ position }}" data-image-src="{{ image }}">
<div class="parallax-body">{{ blockContent }}</div></div>

{% stylesheet %}
#{{ id }} {
    min-height: {{ height }};
    background: transparent;
}

#{{ id }} .parallax-body {
    z-index: {{ bodyZindex }};
    position: relative;
    color: {{ contentcolor }};
    padding: {{ contentpadding }};
    text-align: {{ contentalign }};
}
{% endstylesheet %}',2,'','image^|height^200px|speed^20|zindex^1|position^center center|contentpadding^0|contentcolor^#fff|contentalign^center','C74AC163-0D90-4E9A-8BFB-A13DFA053CA2')
 
-- Google Map
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Google Map','Add interactive maps to your site with just a bit of Lava.','<p>
    Adding a Google map to you page always starts out sounding easy… until… you get to the details. Soon the whole day is wasted and you don''t have much to 
    show. This shortcode makes it easy to add responsive Google Maps to your site. Let''s start with a simple example and work our way to more complex use cases.
</p>

<p>
    Note: Do to this javascript requirements of this shortcode you will need to do a full page reload before changes to the 
    shortcode appear on your page.
</p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    In the example above we mapped a single point to our map. Pretty easy, but not very helpful. We can add additional points by providing more markers.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    Ok… we''re getting there, but what if we wanted titles and information windows for our markers, oh and custom markers 
    too? You can provide optional parameters for each marker as shown below.
</p>

<pre>[[ marker location:''latitude,longitude'' title:''My Title'' icon:''icon url'' ]] info window content [[ endmarker ]]</pre>

<p><strong>Example:</strong></p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' title:''Spark Global Headquarters'']]
        &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;&lt;/code&gt;&lt;p&gt;&lt;code&gt;
        It''s not as grand as it sounds.&lt;br&gt;
        &lt;img src="https://rockrms.blob.core.windows.net/misc/spark-logo.png" width="179" height="47"&gt;          
    [[ endmarker ]]
    [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
{[ endgooglemap ]}</pre>

<p></p><p>
    Note: A list of great resources for custom map markers is below:
</p>

<ul>
    <li><a href="http://map-icons.com/">Map Icons</a></li>
    <li><a href="https://mapicons.mapsmarker.com/">Map Icons Collection</a></li>
    <li><a href="https://github.com/Concept211/Google-Maps-Markers">Google Maps Markers</a></li>
</ul>

<p>
    There are several other parameters for you to use to control the options on your map. They include:
</p>

<ul>
    <li><strong>height</strong> (600px) – The height of the map.</li>
    <li><strong>width</strong> (100%) – The responsive width of the map.</li>
    <li><strong>zoom</strong> (optional) – The zoom level of the map. Note when two or more points are provided the map will auto zoom to place all of the points on the map. The range of the zoom scale is 1 (the furthest out, largest) to 20 (the closest, smallest). The approximate zoom levels are: 
    <ul>
        <li>1 = world</li>
        <li>5 = continent</li>
        <li>10 = city</li>
        <li>15 = streets</li>
        <li>20 = buildings</li>
    </ul>
    </li>
    <li><strong>center</strong> (optional) – The center point on the map. If you do not provide a center a default will be calculated based on the points given.</li>
    <li><strong>maptype</strong> (roadmap) – The type of map to display. The options are ‘roadmap'', ‘hybrid'', ‘satellite'' or ‘terrain''.</li>
    <li><strong>showzoom</strong> (true) – Should the zoom control be displayed.</li>
    <li><strong>showstreetview</strong> (false) – Should he StreetView control be displayed.</li>
    <li><strong>showfullscreen</strong> (true) – Should the control to show the map full screen be displayed.</li>
    <li><strong>showmapttype</strong> (false) – Should the control to change the map type be shown.</li>
    <li><strong>markerannimation</strong> (none) – The marker animation type. Options include: ''none'', ‘bounce'' (markers bounce continuously) or ''drop'' (markers drop in).</li>
    <li><strong>scrollwheel</strong> (true) – Determines if the scroll wheel should control the zoom level when the mouse is over the map.</li>
    <li><strong>draggable</strong> (true) – Determines if the mouse should be allowed to drag the center point of the map (allow the map to be moved).</li>
    <li><strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ‘cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.</li>
</ul>

<p>
    As you can see there are a lot of options in working with your map. You can also style your map by changing the colors. You do this by providing 
    the styling information in a separate [[ style ]] section. The styling settings for Google Maps is not pretty to look at or configure for that matter. Luckily, there 
    are sever sites that allow you to download preconfigured map styles. Two of the best are called <a href="https://snazzymaps.com">SnazzyMaps</a> and 
    <a href="https://mapstyle.withgoogle.com">Map Style</a>. Below is an example showing how to add styling to your maps.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]] 
     [[ style ]]
        [{"featureType":"all","elementType":"all","stylers":[{"visibility":"on"}]},{"featureType":"all","elementType":"labels","stylers":[{"visibility":"off"},{"saturation":"-100"}]},{"featureType":"all","elementType":"labels.text.fill","stylers":[{"saturation":36},{"color":"#000000"},{"lightness":40},{"visibility":"off"}]},{"featureType":"all","elementType":"labels.text.stroke","stylers":[{"visibility":"off"},{"color":"#000000"},{"lightness":16}]},{"featureType":"all","elementType":"labels.icon","stylers":[{"visibility":"off"}]},{"featureType":"administrative","elementType":"geometry.fill","stylers":[{"color":"#000000"},{"lightness":20}]},{"featureType":"administrative","elementType":"geometry.stroke","stylers":[{"color":"#000000"},{"lightness":17},{"weight":1.2}]},{"featureType":"landscape","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":20}]},{"featureType":"landscape","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"landscape","elementType":"geometry.stroke","stylers":[{"color":"#4d6059"}]},{"featureType":"landscape.natural","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"poi","elementType":"geometry","stylers":[{"lightness":21}]},{"featureType":"poi","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"poi","elementType":"geometry.stroke","stylers":[{"color":"#4d6059"}]},{"featureType":"road","elementType":"geometry","stylers":[{"visibility":"on"},{"color":"#7f8d89"}]},{"featureType":"road","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.highway","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"},{"lightness":17}]},{"featureType":"road.highway","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"},{"lightness":29},{"weight":0.2}]},{"featureType":"road.arterial","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":18}]},{"featureType":"road.arterial","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.arterial","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.local","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":16}]},{"featureType":"road.local","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.local","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"}]},{"featureType":"transit","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":19}]},{"featureType":"water","elementType":"all","stylers":[{"color":"#2b3638"},{"visibility":"on"}]},{"featureType":"water","elementType":"geometry","stylers":[{"color":"#2b3638"},{"lightness":17}]},{"featureType":"water","elementType":"geometry.fill","stylers":[{"color":"#24282b"}]},{"featureType":"water","elementType":"geometry.stroke","stylers":[{"color":"#24282b"}]},{"featureType":"water","elementType":"labels","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text.fill","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text.stroke","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.icon","stylers":[{"visibility":"off"}]}]
    [[ endstyle]]
{[ endgooglemap ]}</pre>

<p>
    Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply copy and pasted straight from SnazzyMaps!
</p>' ,1,1,'googlemap','{% capture singleQuote %}''{% endcapture %}
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

{% case markerannimation %}
{% when ''drop'' %}
    {% assign markerannimation = ''google.maps.Animation.DROP'' %}
{% when ''bounce'' %}
    {% assign markerannimation = ''google.maps.Animation.BOUNCE'' %}
{% else %}
    {% assign markerannimation = ''null'' %}
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
            {% if marker.icon && marker.icon != '''' -%}
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
                animation: {{ markerannimation }},
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

</script>',2,'','styles^|height^600px|width^100%|zoom^|center^|maptype^roadmap|showzoom^true|showstreetview^false|showfullscreen^true|showmaptype^false|markerannimation^none|scrollwheel^true|draggable^true|gesturehandling^cooperative','FE298210-1307-49DF-B28B-3735A414CCA0')
 

-- Google Static Map
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Google Static Map','Easily allow you to add Google static maps without having to remember complex settings.','<p>
    This shortcode returns a static map (think Google Map as an image) based on parameters you provide. Let''s look at a simple example.
</p>

<pre>{[ googlestaticmap center:''10451 W Palmeras Dr Sun City, AZ 85373-2000'' zoom:''12'' ]}
{[ endgooglestaticmap ]}</pre>

<p>
    Here''s we''re showing a simple map centered around the address we provided at a reasonable zoom level. Note that you could 
    also provide the center point as a lat/long if you prefer.
</p>

<p>
So, what about markers, yep you can do that too. Notice when you use makers you don''t have to provide a center point or zoom.
</p>

<pre>{[ googlestaticmap ]}
    [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
    [[ marker location:''1 Cardinals Dr, Glendale, AZ 85305'' ]] [[ endmarker ]]
{[ endgooglestaticmap ]}</pre>

<p>
    Note here we provided both a lat/long and a street address. Either is just fine. With the markers, you can also 
    provide it other options. Here are your options:
</p>
<ul>
    <li><strong>location</strong> – The location (address) of the pin.</li>
    <li><strong>color</strong> – The color of the pin, can be a basic color from the list below or a 24-bit color in the pattern of 0xFFFFFF.</li>
        (black, brown, green, purple, yellow, blue, gray, orange, red, white).
    <li><strong>size</strong> – The size of the pin (valid values include: tiny,mid,small)</li>
    <li><strong>label</strong> - The letter or number that you want on the pin.</li>
    <li><strong>icon</strong> – The url to the png file to use for the pin icon.</li>
</ul>
<p>
    Now that we have the pins taken care of let''s look at the other options you can place on the shortcut:
</p>

<ul>
    <li><strong>returnurl</strong> (false) – By default the returned map will be in an image tag with a div to wrap it. Setting returnurl to ''true'' 
        will return just the URL (say for adding it as a background image).</li>
    <li><strong>width</strong> (100%) – The image will be added to a responsive div. The setting allows you to set the width of the div to a percent or px.</li>
    <li><strong>imagesize</strong> – The size of the image to get from the Google API. If you are using the free API you are limited to 640x640, though you can actually double 
        that if you set the scale = 2. If you buy the premium plan that size goes up to 2048x2048.</li>
    <li><strong>maptype</strong> (roadmap) – This determines the type of map to return. Valid values include roadmap, satellite, hybrid, and terrain.</li>
    <li><strong>scale</strong> (1) – This effects the number of pixels that are returned (usually used for creating high DPI images (aka retina). You can use this though to 
        double the size of the 640px limit. Valid values are 1 or 2 or 4 (4 requires the premium plan).</li>
    <li><strong>zoom</strong> (optional if used with markers) – The zoom level of the map. This must be value between 1-20. Below is a rough idea of the scales:
        <ul>
            <li>1 = world</li>
            <li>5 = continent</li>
            <li>10 = city</li>
            <li>15 = streets</li>
            <li>20 = buildings</li>
        </ul>
        </li>
    <li><strong>center</strong> (optional if used with markers) – The center point of the map. Can be either a street address or lat/long.</li>
    <li><strong>format</strong> (png8) – The format type of the image that is returned. Valid values include png8, png32, gif, jpg, jpg-basline.</li>
    <li><strong>style</strong> (optional) – The returned map can be styled in an infinite (or close to) number of ways. See the Google Static Map documentation for 
        details (https://developers.google.com/maps/documentation/static-maps/styling).</li>
</ul>',1,1,'googlestaticmap','{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}

{% if apiKey == "" %}
    <div class="alert alert-warning">
        There is not Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
    </div>
{% endif %}

{% assign url = ''https://maps.googleapis.com/maps/api/staticmap?'' | Append:''size='' | Append:imagesize | Append:''&maptype='' | Append:maptype | Append:''&scale='' | Append:scale | Append:''&format='' | Append:format -%}

{% if zoom != '''' -%}
    {% assign url = url | Append:''&zoom='' | Append:zoom %}
{% endif -%}

{% if center != '''' -%}
    {% assign center = center | EscapeDataString -%}
    {% assign url = url | Append:''&center='' | Append:center %}
{% endif -%}

{% if style != '''' -%}
    {% assign url = url | Append:''&style='' | Append:style %}
{% endif -%}

{% assign markerCount = markers | Size -%}
{% assign markersContent = '''' %}

{% for marker in markers -%}
    {% assign markerContent = '''' %}

    {% if marker.color and marker.color != '''' - %}
        {% assign markerContent = markerContent | Append:''color:'' | Append:marker.color -%}
    {% endif -%}
    
    {% if marker.icon and marker.icon != '''' - %}
        {% assign markerContent = markerContent | Append:''icon:'' | Append:marker.icon -%}
    {% endif -%}
    
    {% if marker.size and marker.size != '''' - %}
        {% assign markerContent = markerContent | Append:''|size:'' | Append:marker.size -%}
    {% endif -%}
    
    {% if marker.label and marker.label != '''' - %}
        {% assign markerContent = markerContent | Append:''|label:'' | Append:marker.label -%}
    {% endif -%}
    
    {% assign markerContent = markerContent | Append:''|'' | Append:marker.location | Trim -%}
    
    {% assign markerContent = markerContent | EscapeDataString -%}
    {% assign markersContent = markersContent | Append:''&markers='' | Append:markerContent -%}
{% endfor -%}


{% assign url = url | Append:markersContent -%}

{% assign url = url | Append:''&key='' | Append:apiKey'' -%}


{% assign returnurl = returnurl | AsBoolean %}
{% if returnurl %}
    {{ url }}
{% else %}
    <div style="width: {{ width }}">
        <img src="{{ url }}" style="width: 100%" />
    </div>
{% endif %}',2,'','returnurl^false|width^100%|imagesize^640x320|maptype^roadmap|scale^2|zoom^|center^|format^png8|style^','2DD53FE6-6EB2-4EC8-A965-3F71054F7983')
 
-- Accordion
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Accordion','Allows you to easily create a Bootstrap accordion control.','<p>
    <a href="https://getbootstrap.com/docs/3.3/javascript/#collapse-example-accordion">Bootstrap accordions</a> are a clean way of displaying a large 
    amount of structured content on a page. While they''re not incredibly difficult to make using just HTML this shortcode simifies the markup 
    quite a bit. The example below shows an accordion with three different sections.
</p>

<pre>{[ accordion ]}

    [[ item title:''Lorem Ipsum'' ]]
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut pretium tortor et orci ornare 
        tincidunt. In hac habitasse platea dictumst. Aliquam blandit dictum fringilla. 
    [[ enditem ]]
    
    [[ item title:''In Commodo Dolor'' ]]
        In commodo dolor vel ante porttitor tempor. Ut ac convallis mauris. Sed viverra magna nulla, quis 
        elementum diam ullamcorper et. 
    [[ enditem ]]
    
    [[ item title:''Vivamus Sollicitudin'' ]]
        Vivamus sollicitudin, leo quis pulvinar venenatis, lorem sem aliquet nibh, sit amet condimentum
        ligula ex a risus. Curabitur condimentum enim elit, nec auctor massa interdum in.
    [[ enditem ]]

{[ endaccordion ]}</pre>

<p>
    The base control has the following options:
</p>

<ul>
    <li><strong>paneltype</strong> (default) - This is the CSS panel class type to use (e.g. ''default'', ''block'', ''primary'', ''success'', etc.)</li>
    <li><strong>firstopen</strong> (true) - Determines is the first accordion section should be open when the page loads. Setting this to false will
        cause all sections to be closed.</li>
</ul>

<p>
    The [[ item ]] block configuration has the following options:
</p>
<ul>
    <li><strong>title</strong> - The title of the section.</li>
</ul>',1,1,'accordion','{% assign firstopen = firstopen | AsBoolean -%}

<div class="panel-group" id="accordion" role="tablist" aria-multiselectable="true">
  
  {% for item in items -%}
      {% assign isopen = '''' -%}
      {% if item.isopen and item.isopen !='''' -%}
        {% assign isopen = item.isopen | AsBoolean -%}
      {% else -%}
        {% if forloop.index == 1 and firstopen -%}
            {% assign isopen = true -%}
        {% else -%}
            {% assign isopen = false -%}
        {% endif -%}
      {% endif -%}
      
      <div class="panel panel-{{ paneltype }}">
        <div class="panel-heading" role="tab" id="heading{{ forloop.index }}">
          <h4 class="panel-title">
            <a role="button" data-toggle="collapse" data-parent="#accordion" href="#collapse{{ forloop.index }}" aria-expanded="true" aria-controls="collapse{{ forloop.index }}">
              {{ item.title }}
            </a>
          </h4>
        </div>
        <div id="collapse{{ forloop.index }}" class="panel-collapse collapse {% if isopen %} in{% endif %}" role="tabpanel" aria-labelledby="heading{{ forloop.index }}">
          <div class="panel-body">
            {{ item.content }}
          </div>
        </div>
      </div>
  {% endfor -%}
  
</div>',2,'','paneltype^default|firstopen^true','18F87671-A848-4509-8058-C95682E7BAD4')
 
-- Chart
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Chart','Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The chart shortcode allows anyone to create charts with just a few lines of Lava.','<p>
    Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The 
    chart shortcode allows anyone to create charts with just a few lines of Lava. There are two modes for 
    creating a chart. The first ‘simple'' mode creates a chart with a single series. This option will suffice 
    for most of your charting needs. The second ‘series'' option allows you to create charts with multiple 
    series. Let''s look at each option separately starting with the simple option.
</p>

<h4>Simple Mode</h4>
<p>
    Let''s start by jumping to an example. We''ll then talk about the various configuration options, deal?
</p>

<pre>{[ chart type:''bar'' ]}
    [[ dataitem label:''Small Groups'' value:''45'' ]] [[ enddataitem ]]
    [[ dataitem label:''Serving Groups'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''General Groups'' value:''34'' ]] [[ enddataitem ]]
    [[ dataitem label:''Fundraising Groups'' value:''12'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
    
<p>    
    As you can see this sample provides a nice-looking bar chart. The shortcode defines the chart type (several other 
    options are available). The [[ dataitem ]] configuration item defines settings for each bar/point on the chart. Each 
    has the following settings:
</p>

<ul>
    <li><strong>label</strong> – The label for the data item.</li>
    <li><strong>value</strong> – The data point for the item.</li>
</ul>

<p>
    The chart itself has quite a few settings for you to consider. These include:
</p>

<ul>
    <li><strong>type</strong> (bar) – The type of chart to display. The valid options include: bar, line, radar, pie, doughnut, polarArea (think radar meets pie).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderdash</strong> – This setting defines how the lines on the chart should be displayed. No value makes them display as solid lines. You can make interesting dot/dash patterns by providing an array of numbers representing lines and spaces. For instance, the setting of ‘[5, 5]'' would say draw a line of length 5px and then a space of 5px and repeat. You can provide as many numbers as you like to make more complex patterns (but isn''t that getting a little too fancy?)</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>legendposition</strong> (bottom) – This determines where the legend should be displayed.</li>
    <li><strong>legendshow</strong> (false) – Setting determines if the legend should be shown.</li>
    <li><strong>chartheight</strong> (400px) – The height of the chart must be set in pixels.</li>
    <li><strong>chartwidth</strong> (100%) – The width of the chart (can set as either a percentage or pixel size).</li>
    <li><strong>tooltipshow</strong> (true) – Determines if tooltips should be displayed when rolling over data items.</li>
    <li><strong>tooltipbackgroundcolor</strong> (#000) – The background color of the tooltip.</li>
    <li><strong>tooltipfontcolor</strong> (#fff) – The font color of the tooltip.</li>
    <li><strong>fontcolor</strong> (#777) – The font color to use on the chart.</li>
    <li><strong>fontfamily</strong> (''OpenSans'',''Helvetica Neue'',Helvetica,Arial,sans-serif) – The font to use for the chart.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be.</li>
    <li><strong>pointcolor</strong> #059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
    <li><strong>curvedlines</strong> (true) – This determines if the lines should be straight between two points or beautifully curved. Based on this description you should be able to determine the default.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for data items. You can also provide a fill color for each item independently on the [[ dataitem ]] configuration. </li>
    <li><strong>label</strong> – The label to show for the single axis (not often needed in a single axis chart, but hey it''s there.)</li>
    <li><strong>xaxistype</strong> (linear) – The x-axis type. This is primarily used for time based charts. Valid values are ''linear'' and ''time''.</li>
</ul>

<h5>Time Based Charts</h5>
<p>
    If the x-axis of your chart is date/time based you''ll want to set the ''xaxistype'' to ''time'' and provide
    the date in the label field.
</p>
<pre>{[ chart type:''line'' xaxistype:''time'' ]}
    [[ dataitem label:''1/1/2017'' value:''24'']] [[ enddataitem ]]
    [[ dataitem label:''2/1/2017'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''3/1/2017'' value:''42''  ]] [[ enddataitem ]]
    [[ dataitem label:''5/1/2017'' value:''23'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>

<p>
    That should be more than enough settings to get you started on the journey to chart success. But… what about 
    multiple series? Glad you asked…
</p>

<h4>Multiple Series</h4>
<p>
    It''s simple to add multiple series to your charts using the [[ dataset ]] configuration option. Each series is defined 
    by a [[ dataset ]] configuration block. Let''s again start with an example.
</p>

<pre>{[ chart type:''bar'' labels:''2015,2016,2017'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>

<div class="text-center">
    <img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/chart-series.jpg">
</div>

<p>
    If there is a trick to using series it''s understanding the organization of the data. In our example each [[ dataset ]] 
    is type of group. The data property of the dataset determine the number of groups for each year. The configuration of dataset 
    was created to help you write Lava to dynamically create your charts.
</p>

<p>
    Each of the dataset items have the following configuration options:
</p>

<ul>
    <li><strong>label</strong> – This is the descriptor of the dataset used for the legend.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for the data set. You should change this to help differentiate the series.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart.</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be. </li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
</ul>

<h5>Time Based Multi-Series Charts</h5>
<p>
    Like their single series brothers, multi-series charts can be line based to by setting
    the xseriestype = ''line'' and providing the dates in the ''label'' setting.
</p>
<pre>{[ chart type:''line'' labels:''1/1/2017,2/1/2017,6/1/2017'' xaxistype:''time'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
',1,1,'chart','{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}

{% assign dataitemCount = dataitems | Size -%}
{% if dataitemCount > 0 -%}
    {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''", "'' | Prepend:''["'' | Append:''"]'' %}
    {% assign firstDataItem = dataitems | First  %}

    {% capture seriesData -%}
    {
        label: ''{{ label }}'',
        fill: {{ filllinearea }}, 
        backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
        borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
        borderWidth: {{ borderwidth }},
        pointRadius: {{ pointradius }},
        pointBackgroundColor: ''{{ pointcolor }}'',
        pointBorderColor: ''{{ pointbordercolor }}'',
        pointBorderWidth: {{ pointborderwidth }},
        pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
        pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
        pointHoverRadius: ''{{ pointhoverradius }}'',
        {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
        {% if curvedlines == false -%} lineTension: 0,{% endif -%}
        data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
    }
    {% endcapture -%}
    {% assign labels = dataitems | Map:''label'' | Join:''", "'' | Prepend:''"'' | Append:''"'' -%}
{% else -%}
    {% if labels == '''' -%}
        <div class="alert alert-warning">
            When using datasets you must provide labels on the shortcode to define each unit of measure.
            {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
        </div>
    {% else %}
        {% assign labelItems = labels | Split:'','' -%}
        {% assign labels = ''"''- %}
        {% for labelItem in labelItems -%}
            {% assign labelItem = labelItem | Trim %}
            {% assign labels = labels | Append:labelItem | Append:''","'' %}
        {% endfor -%}
        {% assign labels = labels | ReplaceLast:''","'',''"'' %}
    {% endif -%}
    {% assign seriesData = '''' -%}
    {% for dataset in datasets -%}
        {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
        {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
        {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
        {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
        {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
        {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
        {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
        {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
        {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
        {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
        {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
        {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign pointHoverRadius = pointhoverradius %} {% endif -%}
        
        {% capture itemData -%}
            {
                label: ''{{ datasetLabel }}'',
                fill: {{ filllinearea }}, // 1
                backgroundColor: ''{{ datasetFillColor }}'',
                borderColor: ''{{ datasetBorderColor }}'',
                borderWidth: {{ datasetBorderWidth }},
                pointRadius: {{ datasetPointRadius }},
                pointBackgroundColor: ''{{ datasetPointColor }}'',
                pointBorderColor: ''{{ datasetPointBorderColor }}'',
                pointBorderWidth: {{ datasetPointBorderWidth }},
                pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                pointHoverRadius: ''{{ pointhoverradius }}'',
                {% if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{% endif -%}
                {% if dataset.curvedlines and dataset.curvedlines == false -%} lineTension: 0,{% endif -%}
                data: [{{ dataset.data }}]
            },
        {% endcapture -%}

        {% assign seriesData = seriesData | Append:itemData -%}
    {% endfor -%}
    {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
{% endif -%}

<div class="chart-container" style="position: relative; height:{{ chartheight }}; width:{{ chartwidth }}">
    <canvas id="chart-{{ id }}"></canvas>
</div>

<script>

var options = {
    maintainAspectRatio: false,
    legend: {
        position: ''{{ legendposition }}'',
        display: {{ legendshow }}
    },
    tooltips: {
        enabled: ''{{ tooltipshow }}'',
        backgroundColor: ''{{ tooltipbackgroundcolor }}'',
        bodyFontColor: ''{{ tooltipfontcolor }}'',
        titleFontColor: ''{{ tooltipfontcolor }}''
    }
    {% if xaxistype == ''time'' %}
        ,scales: {
        xAxes: [{
            type: "time",
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''Date''
            }
        }],
        yAxes: [{
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''value''
            }
        }]
    }
    {% endif %}
};

var data = {
    labels: [{{ labels }}],
    datasets: [{{ seriesData }}],
    borderWidth: {{ borderwidth }}
};

Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
Chart.defaults.global.defaultFontFamily = "{{ fontfamily }}";

var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
var chart = new Chart(ctx, {
    type: ''{{ type }}'',
    data: data,
    options: options
});    

</script>


',2,'','fillcolor^rgba(5,155,255,.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|fontcolor^#777|fontfamily^''OpenSans'',''Helvetica Neue'',Helvetica,Arial,sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5,155,255,.6)|pointhoverbordercolor^rgba(5,155,255,.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear','43819A34-4819-4507-8FEA-2E406B5474EA')


-- Panel
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Panel','The panel shortcode allows you to easily add a Bootstrap panel to your markup.','<p>
    The panel shortcode allows you to easily add a 
    <a href="https://getbootstrap.com/docs/3.3/components/#panels">Bootstrap panel</a> to your markup. This is a pretty simple shortcode, but it does save you some time.
</p>

<p>Basic Usage:<br>  
</p><pre>{[ panel title:''Important Stuff'' icon:''fa fa-star'' ]}<br>  
This is a super simple panel.<br> 
{[ endpanel ]}</pre>

<p></p><p>
    As you can see the body of the shortcode is placed in the body of the panel. Optional parameters include:
</p>

<ul>
    <li><b>title</b> – The title to show in the heading. If no title is provided then the panel title section will not be displayed.</li>
    <li><b>icon </b>– The icon to use with the title.</li>
    <li><b>footer</b> – If provided the text will be placed in the panel’s footer.<br><br></li>
</ul>',1,1,'panel','<div class="panel panel-{{ type }}">
  {% if title != '''' %}
      <div class="panel-heading">
        <h3 class="panel-title">
            {% if icon != '''' %}
                <i class=''{{ icon }}''></i> 
            {% endif %}
            {{ title }}</h3>
      </div>
  {% endif -%}
  <div class="panel-body">
    {{ blockContent  }}
  </div>
  {% if footer != '''' %}
    <div class="panel-footer">{{ footer }}</div>
  {% endif %}
</div>',2,'','type^block|icon^|title^|footer^','ADB1F75D-500D-4305-9805-99AF04A2CD88')

-- Word Cloud
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Word Cloud','This shortcode takes a large amount of text and creates a word cloud of the most popular terms.','<p>
    This short cut takes a large amount of text and converts it to a word cloud of the most popular terms. It''s 
    smart enough to take out common words like ''the'', ''and'', etc. Below are the various options for this shortcode. If 
    you would like to pay with the settings in real-time head over <a href="https://www.jasondavies.com/wordcloud/">the Javascript''s homepage</a>.
</p>

<pre>{[ wordcloud ]}
    ... out a lot of text here ...
{[ endwordcloud ]}
</pre>

<ul>
    <li><strong>width</strong> (960) – The width in pixels of the word cloud control.</li>
    <li><strong>height</strong> (420) - The height in pixels of the control.</li>
    <li><strong>fontname</strong> (Impact) – The font to use for the rendering.</li>
    <li><strong>maxwords</strong> (255) – The maximum number of words to use in the word cloud.</li>
    <li><strong>scalename</strong> (log) – The type of scaling algorithm to use. Options are ''log'', '' sqrt'' or ''linear''.</li>
    <li><strong>spiralname</strong> (archimedean) – The type of spiral used for positioning words. Options are ''archimedean'' or ''rectangular''.</li>
    <li><strong>colors</strong> (#0193B9,#F2C852,#1DB82B,#2B515D,#ED3223) – A comma delimited list of colors to use for the words.</li>
    <li><strong>anglecount</strong> (6) – The maximum number of angles to place words on in the cloud.</li>
    <li><strong>anglemin</strong> (-90) – The mimimum angle to use when drawing the cloud.</li>
    <li><strong>anglemax</strong> (90) – The maximum angle to use when drawing the cloud.</li>
</ul>',1,1,'wordcloud','{% javascript id:''d3-layout-cloud'' url:''~/Scripts/d3-cloud/d3.layout.cloud.js'' %}{% endjavascript %}
{% javascript id:''d3-min'' url:''~/Scripts/d3-cloud/d3.min.js'' %}{% endjavascript %}



<div id="{{ uniqueid }}" style="width: {{ width }}; height: {{ height }};">
    
</div>

{% javascript disableanonymousfunction:''true'' %}
    $( document ).ready(function() {
        Rock.controls.wordcloud.initialize({
            inputTextId: ''hf-{{ uniqueid }}'',
            visId: ''{{ uniqueid }}'',
            width: ''{{ width }}'',
            height: ''{{ height }}'',
            fontName: ''{{ fontname }}'',
            maxWords: {{ maxwords }},
            scaleName: ''{{ scalename }}'',
            spiralName: ''{{ spiralname}}'',
            colors: [ ''{{ colors | Replace:'','',"'',''" }}''],
            anglecount: {{ anglecount }},
            anglemin: {{ anglemin }},
            anglemax: {{ anglemax }}
        });
    });
{% endjavascript %}


<input type="hidden" id="hf-{{ uniqueid }}" value="{{ blockContent }}" />',2,'','width^960|height^420|fontname^Impact|maxwords^255|scalename^log|spiralname^archimedean|colors^#0193B9,#F2C852,#1DB82B,#2B515D,#ED3223','CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4')
