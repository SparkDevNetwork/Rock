UPDATE [LavaShortcode] SET [Markup]=N'{% assign apiKey = ''Global'' | Attribute:''GoogleApiKey'' %}

{% if apiKey == "" %}
    <div class="alert alert-warning">
        There is no Google API key defined. Please add your key under: ''Admin Tools > General Settings > Global Attributes > Google API Key''.
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
    {% assign styleItems = style | Split:'','' -%}
        {% for styleItem in styleItems -%}
            {% assign url = url | Append:''&style='' | Append:styleItem %}
            {% endfor -%}
{% endif -%}

{% assign markerCount = markers | Size -%}
{% assign markersContent = '''' %}

{% for marker in markers -%}
    {% assign markerContent = '''' %}

    {% if marker.color and marker.color != '''' -%}
        {% assign markerContent = markerContent | Append:''color:'' | Append:marker.color -%}
    {% endif -%}
    
    {% if marker.icon and marker.icon != '''' -%}
        {% assign markerContent = markerContent | Append:''icon:'' | Append:marker.icon -%}
    {% endif -%}
    
    {% if marker.size and marker.size != '''' -%}
        {% assign markerContent = markerContent | Append:''|size:'' | Append:marker.size -%}
    {% endif -%}
    
    {% if marker.label and marker.label != '''' -%}
        {% assign markerContent = markerContent | Append:''|label:'' | Append:marker.label -%}
    {% endif -%}
    
    {% comment %}
    // If given, handle adjusting the precision on a given lat and long 
    {% endcomment %}
    {% assign mLocation = marker.location %}
    {% if marker.precision and marker.precision != '''' %}
        {% capture precision %}0.{% for i in (1..marker.precision) %}0{% endfor %}{% endcapture %}

        {% assign latLong = marker.location | Split:'','' %}
        {% assign latLongSize = latLong | Size %}
        {% if latLongSize == 2 %}
            {% capture mLocation %}{{ latLong[0] | AsDecimal | Format:precision }},{{ latLong[1] | AsDecimal | Format:precision }}{% endcapture %}
        {% endif %}
    {% endif %}
    {% assign markerContent = markerContent | Append:''|'' | Append:mLocation | Trim -%}
    
    {% assign markerContent = markerContent | EscapeDataString -%}
    {% assign markersContent = markersContent | Append:''&markers='' | Append:markerContent -%}
{% endfor -%}

{% assign url = url | Append:markersContent -%}
{% assign url = url | Append:''&key='' | Append:apiKey -%}


{% assign returnurl = returnurl | AsBoolean %}
{% if returnurl %}
    {{ url }}
{% else %}
    <div style="width: {{ width }}">
        <img src="{{ url }}" style="width: 100%" />
    </div>
{% endif %}' WHERE ([Guid]='2DD53FE6-6EB2-4EC8-A965-3F71054F7983')