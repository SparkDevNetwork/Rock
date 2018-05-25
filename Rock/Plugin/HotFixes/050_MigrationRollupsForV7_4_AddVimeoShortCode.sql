IF ( SELECT COUNT(*) FROM [LavaShortCode] WHERE [Guid] = 'EA1335B7-158F-464F-8994-98C53D4E47FF' ) = 0
BEGIN
INSERT INTO [LavaShortCode]
	([Name], [Description], [Documentation], [IsSystem], [IsActive], [TagName], [Markup], [TagType], [EnabledLavaCommands], [Parameters], [Guid])
	VALUES
	('Vimeo','Creates a responsive Vimeo embed from just a simple video id.','<p>Embedding a Vimeo video is easy, right? Well what if you want it to be responsive (adjust with the size of the window)? Or what about 
control of what is shown in the player? The Vimeo shortcode helps to shorten (see what we did there) the time it takes to get a video 
up on your Rock site. Here’s how:<br>  </p>

<p>Basic Usage:</p>

<pre>{[ vimeo id:''180467014'' ]}</pre>


<p>This will put the video with the id provide onto your page in a responsive container. The id can be found in the address of the 
Vimeo video. There are also a couple of options for you to add:</p>

<ul>
    <li><b>id</b> (required) – The Vimeo id of the video.</li>
    <li><b>width</b> (100%) – The width you would like the video to be. By default it will be 100% but you can provide any width in percentages, pixels, or any other valid CSS unit of measure.</li>
    <li><b>color</b> (video default) – The color you would like the video controls to use. By default it will use the color set by the video author, but you can provide any hexidecimal color i.e. <code>#ffffff</code>.</li>
    <li><b>loop</b> (false) – This determines if the name of the video and other information should be shown at the top of the video.</li>
    <li><b>title</b> (false) – This determines if the Vimeo title should be shown.</li>
    <li><b>byline</b> (false) – This determines if the Vimeo byline should be shown.</li>
    <li><b>portrait</b> (false) – This determines if the Vimeo portrait icon should be shown.</li>
    <li><b>autoplay</b> (false) – Should the video start playing once the page is loaded?</li>
</ul>',1,1,'vimeo','{% assign wrapperId = uniqueid %}

{% assign parts = id | Split:''/'' %}
{% assign id = parts | Last %}
{% assign parts = id | Split:''='' %}
{% assign id = parts | Last | Trim %}

{% assign url = ''https://player.vimeo.com/video/'' | Append:id | Append:''?autoplay=0'' %}

{% assign autoplay = autoplay | AsBoolean %}
{% assign loop = loop | AsBoolean %}
{% assign title = title | AsBoolean %}
{% assign byline = byline | AsBoolean %}
{% assign portrait = portrait | AsBoolean %}

{% if autoplay %}
    {% assign url = url | Append:''&autoplay=1'' %}
{% else %}
    {% assign url = url | Append:''&autoplay=0'' %}
{% endif %}

{% if loop %}
    {% assign url = url | Append:''&loop=1'' %}
{% else %}
    {% assign url = url | Append:''&loop=0'' %}
{% endif %}

{% if color != '''' %}
    {% assign color = color | Split:''#'' | Last %}
    {% assign url = url | Append:''&color='' | Append:color %}
{% endif %}

{% if title %}
    {% assign url = url | Append:''&title=1'' %}
{% else %}
    {% assign url = url | Append:''&title=0'' %}
{% endif %}

{% if byline %}
    {% assign url = url | Append:''&byline=1'' %}
{% else %}
    {% assign url = url | Append:''&byline=0'' %}
{% endif %}

{% if portrait %}
    {% assign url = url | Append:''&portrait=1'' %}
{% else %}
    {% assign url = url | Append:''&portrait=0'' %}
{% endif %}


<style>
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

<div id=''{{ wrapperId }}'' style=''width:{{ width }};''>
    <div class=''embed-container''><iframe src=''{{ url }}'' frameborder=''0'' webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe></div>
</div>',1,'','id^|width^100%|autoplay^false|loop^false|color^|title^false|byline^false|portrait^false','EA1335B7-158F-464F-8994-98C53D4E47FF')
 END