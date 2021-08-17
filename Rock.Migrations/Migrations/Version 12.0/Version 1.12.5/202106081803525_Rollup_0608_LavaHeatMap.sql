INSERT INTO [LavaShortcode] ([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid]) VALUES (N'Google Heatmap', N'Add an interactive visualization to depict the intensity of data at geographical points.', N'<p>
This shortcode makes it easy to add responsive Heatmaps from Google Maps to your site. And uses similar options to the Google Map shortcode to make things as easy as possible. Let''s start with a simple example and work our way to more complex use cases.
</p>

<p>
    Note: Due to the javascript requirements of this shortcode you will need to do a full page reload before changes to the 
    shortcode appear on your page.
</p>

<pre>{[ googleheatmap ]}
    [[ datapoint location:''33.640705,-112.280198'' ]] [[ enddatapoint ]]
{[ endgoogleheatmap ]}</pre>

<p>
    In the example above we mapped a single point to our map. Pretty easy, but not very helpful. We can add additional points by providing more datapoints.
</p>

<pre>{[ googleheatmap ]}
     [[ datapoint location:''33.640705,-112.280198'' ]] [[ enddatapoint ]]
     [[ datapoint location:''33.52764,-112.262571'' ]] [[ enddatapoint ]]
{[ endgoogleheatmap ]}</pre>

<p>
    Additionally a heatmap can accept a weight for a datapoint. Applying a weight will cause the location to be rendered with a greater intensity than a simple datapoint on it''s own.
</p>
<pre>[[ datapoint location:''latitude,longitude'' weight:''5'' ]][[ enddatapoint ]]</pre>
<p>Using the weight can be useful when:</p>
<ul>
<li>Adding large amounts of data at a single location. Rendering a single data point with a weight of 5 would be faster than creating 5 separate datapoints.</li>
<li>Applying an emphasis to your data based upon arbitrary values. For example, you would use the weight value to represent giving amounts on a map.</li>
</ul>
<p>
    There are several other parameters for you to use to control the options on your map. They include:
</p>

<ul>
    <li><strong>showrangeslider</strong> (false) – Show a range slider to control the radius of the datapoints on the map.</li>
    <li><strong>radius</strong> (10px) – The radius of influence for each data point, in pixels.</li>
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
    <li><strong>center</strong> (optional) – The center point on the map. If you do not provide a center a default will be your organization address.</li>
    <li><strong>maptype</strong> (roadmap) – The type of map to display. The options are ‘roadmap'', ‘hybrid'', ‘satellite'' or ‘terrain''.</li>
    <li><strong>showzoom</strong> (true) – Should the zoom control be displayed.</li>
    <li><strong>showstreetview</strong> (false) – Should he StreetView control be displayed.</li>
    <li><strong>showfullscreen</strong> (true) – Should the control to show the map full screen be displayed.</li>
    <li><strong>showmapttype</strong> (false) – Should the control to change the map type be shown.</li>
    <li><strong>scrollwheel</strong> (true) – Determines if the scroll wheel should control the zoom level when the mouse is over the map.</li>
    <li><strong>draggable</strong> (true) – Determines if the mouse should be allowed to drag the center point of the map (allow the map to be moved).</li>
    <li><strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ‘cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.</li>
</ul>

<p>
    As you can see there are a lot of options in working with your map. You can also style your map by changing the colors. You do this by providing 
    the styling information in a separate [[ style ]] section. The styling settings for Google Maps is not pretty to look at or configure for that matter. Luckily, there 
    are several sites that allow you to download preconfigured map styles. Two of the best are called <a href="https://snazzymaps.com">SnazzyMaps</a> and 
    <a href="https://mapstyle.withgoogle.com">Map Style</a>. Below is an example showing how to add styling to your maps.
</p>

<pre>{[ googleheatmap ]}
     [[ datapoint location:''33.640705,-112.280198'' ]] [[ enddatapoint ]]
     [[ datapoint location:''33.52764,-112.262571'' ]] [[ enddatapoint ]] 
     [[ style ]]
        [{"featureType":"all","elementType":"all","stylers":[{"visibility":"on"}]},{"featureType":"all","elementType":"labels","stylers":[{"visibility":"off"},{"saturation":"-100"}]},{"featureType":"all","elementType":"labels.text.fill","stylers":[{"saturation":36},{"color":"#000000"},{"lightness":40},{"visibility":"off"}]},{"featureType":"all","elementType":"labels.text.stroke","stylers":[{"visibility":"off"},{"color":"#000000"},{"lightness":16}]},{"featureType":"all","elementType":"labels.icon","stylers":[{"visibility":"off"}]},{"featureType":"administrative","elementType":"geometry.fill","stylers":[{"color":"#000000"},{"lightness":20}]},{"featureType":"administrative","elementType":"geometry.stroke","stylers":[{"color":"#000000"},{"lightness":17},{"weight":1.2}]},{"featureType":"landscape","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":20}]},{"featureType":"landscape","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"landscape","elementType":"geometry.stroke","stylers":[{"color":"#4d6059"}]},{"featureType":"landscape.natural","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"poi","elementType":"geometry","stylers":[{"lightness":21}]},{"featureType":"poi","elementType":"geometry.fill","stylers":[{"color":"#4d6059"}]},{"featureType":"poi","elementType":"geometry.stroke","stylers":[{"color":"#4d6059"}]},{"featureType":"road","elementType":"geometry","stylers":[{"visibility":"on"},{"color":"#7f8d89"}]},{"featureType":"road","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.highway","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"},{"lightness":17}]},{"featureType":"road.highway","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"},{"lightness":29},{"weight":0.2}]},{"featureType":"road.arterial","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":18}]},{"featureType":"road.arterial","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.arterial","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.local","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":16}]},{"featureType":"road.local","elementType":"geometry.fill","stylers":[{"color":"#7f8d89"}]},{"featureType":"road.local","elementType":"geometry.stroke","stylers":[{"color":"#7f8d89"}]},{"featureType":"transit","elementType":"geometry","stylers":[{"color":"#000000"},{"lightness":19}]},{"featureType":"water","elementType":"all","stylers":[{"color":"#2b3638"},{"visibility":"on"}]},{"featureType":"water","elementType":"geometry","stylers":[{"color":"#2b3638"},{"lightness":17}]},{"featureType":"water","elementType":"geometry.fill","stylers":[{"color":"#24282b"}]},{"featureType":"water","elementType":"geometry.stroke","stylers":[{"color":"#24282b"}]},{"featureType":"water","elementType":"labels","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text.fill","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.text.stroke","stylers":[{"visibility":"off"}]},{"featureType":"water","elementType":"labels.icon","stylers":[{"visibility":"off"}]}]
    [[ endstyle]]
{[ endgoogleheatmap ]}</pre>

<p>
    Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply copy and pasted straight from SnazzyMaps!
</p>', '1', '1', N'googleheatmap', N'{% capture singleQuote %}''{% endcapture %}
{% capture escapedQuote %}\''{% endcapture %}
{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}
{%- assign showrangeslider = showrangeslider | AsBoolean -%}
{% assign id = uniqueid | Replace:''-'','''' %}

{% if apiKey == '''' %}
    <div class="alert alert-warning">
        There is no Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
    </div>
{% endif %}

{% assign pointCount = datapoints | Size -%}

{% if center == '''' and pointCount >= 1 -%}
    {% assign centerPoint = datapoints | First %}
    {% assign center = centerPoint.location %}
{% endif %}

{% if zoom == '''' and pointCount == 1 -%}
    {% assign zoom = ''11'' %}
{% endif %}

{% javascript id:''googlemapsvisualizationapi'' url:''{{ "https://maps.googleapis.com/maps/api/js?libraries=visualization&key=" | Append:apiKey }}'' %}{% endjavascript %}

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

{% if showrangeslider %}
{{ ''/Scripts/ion.rangeSlider/css/ion.rangeSlider.Rock.css'' | AddCssLink }}
<div class="form-group range-slider">
    <label class="control-label" for="{{ id }}_slider">Radius<a class="help" href="#" tabindex="-1" data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true" title="The radius of influence for each data point, in pixels"><i class="fa fa-info-circle"></i></a></label> 
    <div class="control-wrapper">
        <input type="text" value="{{ radius | AsInteger }}" id="{{ id }}_slider" class="form-control" />
    </div>
</div>
{% endif %}


<div class="map-container {{ id }}">
    <div id="map-container-{{ id }}"></div>
	<div id="{{ id }}"></div>
</div>	


<script>
    // create javascript array of datapoints info
    var heatMapData = [
    {% for point in datapoints -%}
        {%- assign location = point.location | Split:'','' -%}
        {location: new google.maps.LatLng({{ location[0] }}, {{ location[1] }}), weight:{{ point.weight | Default:''1'' }}}{% unless forloop.last %},{% endunless %}
    {% endfor -%}
    ];

	//Set Map
	function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();

        {% if center != '''' -%}
            {%- assign organizationAddress = ''Global'' | Attribute:''OrganizationAddress'',''Object'' -%}
            {%- assign center = organizationAddress.Latitude | Append:'','' | Append:organizationAddress.Longitude -%}
        {% endif -%}
    	var centerLatlng = new google.maps.LatLng( {{ center }} );
    	
    	var mapOptions = {
    		{% if zoom != '''' -%}
    		    zoom: {{ zoom }},
    		{% endif -%}
    		scrollwheel: {{ scrollwheel }},
    		draggable: {{ draggable }},
    		{% if center != '''' -%}
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
		var heatmap = new google.maps.visualization.HeatmapLayer({
            data: heatMapData
        });
        heatmap.setMap(map);

        {% if showrangeslider %}
        // create & hook into rangeslider
        Rock.controls.rangeSlider.initialize({ controlId: ''{{ id }}_slider'', min: ''1'', max: ''128'', step:''1'', from: ''{{ radius | AsInteger }}'', disable: false });
        var rangeSlider = $(''#{{ id }}_slider'');
        rangeSlider.on("change", function(obj) {
            var newRadius = parseInt($(this).val());
            if (heatmap) {
                heatmap.set(''radius'', newRadius);
            }
        });
        {% endif %}
        
		//Resize Function
		google.maps.event.addDomListener(window, "resize", function() {
			var center = map.getCenter();
			google.maps.event.trigger(map, "resize");
			map.setCenter(center);
		});
	}

    google.maps.event.addDomListener(window, ''load'', initialize{{ id }});
</script>', '2', N'', N'styles^|height^600px|width^100%|zoom^10|center^|maptype^roadmap|showzoom^true|showstreetview^false|showfullscreen^true|showmaptype^false|scrollwheel^true|draggable^true|gesturehandling^cooperative|showrangeslider^false|radius^10', '9969A52B-01F8-4597-8C5A-1842BFA1E482');
