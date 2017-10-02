INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('YouTube','Creates a responsive YouTube embe''d from just a simple video id.','<p>Embedding a YouTube video is easy, right? Well what if you want it to be responsive (adjust with the size of the window)? Or what about 
control of what is shown in the player? The YouTube shortcode helps to shorten (see what we did there) the time it takes to get a video 
up on your Rock site. Here’s how:<br>  </p>


<p>Basic Usage:<br>  {[ youtube id:’8kpHK4YIwY4’ ]}</p>


<p>This will put the video with the id provide onto your page in a responsive container. The id can be found in the address of the 
YouTube video. There are also a couple of options for you to add:</p>


<ul>
    <li><b>id</b> (required) – The YouTube id of the video.</li>
    <li><b>width</b> (100%) – The width you would like the video to be. By default it will be 100% but you can provide any width in percentages, pixels, or any other valid CSS unit of measure.</li>
    <li><b>showinfo</b> (false) – This determines if the name of the video and other information should be shown at the top of the video.</li><li><b>controls</b> (true) – This 
        determines if the standard YouTube controls should be shown.</li>
    <li><b>autoplay</b> (false) – Should the video start playing once the page is loaded?</li>
</ul>',0,1,'youtube','{% assign wrapperId = uniqueid %}


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
</div>',1,'','id^|showinfo^false|controls^true|autoplay^false|width^100%','4284B1C9-E73D-4162-8AEE-13A03CAF2938')
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Parallax','Add a scrolling background to a section of your page.','<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>
<p>  
{[ parallax image:''http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'' contentpadding:''20px'' ]}&nbsp;&nbsp;&nbsp;&nbsp;<br>&lt;h1&gt;Hello World&lt;/h1&gt;<br>{[ endparallax ]}
</p>
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


<p>Note: Do to this javascript requirements of this shortcode you will need to do a full page reload before changes to the shortcode appear on your page.</p>',0,1,'parallax','{{ ''https://cdnjs.cloudflare.com/ajax/libs/parallax.js/1.4.2/parallax.min.js'' | AddScriptLink }}
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
{% endstylesheet %}',2,'','image^|height^200px|speed^20|zindex^1|position^center center|contentpadding^0|contentcolor^#fff|contentalign^center','B579B014-735E-4D9C-9DEF-00D4FBCECE21')
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Panel','The panel shortcode allows you to easily add a Bootstrap panel to your markup.','<p>
    The panel shortcode allows you to easily add a Bootstrap panel to your markup. This is a pretty simple shortcode, but it does save you some time.
</p>


<p>Basic Usage:<br>  
{[ panel title:''Important Stuff'' icon:''fa fa-star'' ]}<br>  
This is a super simple panel.<br> 
{[ endpanel ]}
</p>


<p>
    As you can see the body of the shortcode is placed in the body of the panel. Optional parameters include:
</p>


<ul>
    <li><b>title</b> – The title to show in the heading. If no title is provided then the panel title section will not be displayed.</li>
    <li><b>icon </b>– The icon to use with the title.</li>
    <li><b>footer</b> – If provided the text will be placed in the panel’s footer.<br><br></li>
</ul>',0,1,'panel','<div class="panel panel-{{ type }}">
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
</div>',2,'','type^block|icon^|title^|footer^','D8B431AA-13EA-4F5F-9D6E-85B6D26EB72A')
