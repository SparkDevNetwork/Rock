UPDATE [LavaShortcode] SET [Documentation]=N'<p>
The panel shortcode allows you to easily add a 
<a href="https://community.rockrms.com/styling/components/panels" target="_blank">Bootstrap panel</a> to your markup. This is a pretty simple shortcode, but it does save you some time.
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
<li><b>icon </b> – The icon to use with the title.</li>
<li><b>footer</b> – If provided the text will be placed in the panel’s footer.</li>
<li><b>type</b> (block) – Change the type of panel displayed. Options include: default, primary, success, info, warning, danger, block and widget.</li>
</ul>' WHERE ([Guid]='ADB1F75D-500D-4305-9805-99AF04A2CD88')

UPDATE [LavaShortcode] SET [Documentation]=N'<p>Basic Usage:</p>
<pre><code>{[ trendchart ]}
    <span>[[ dataitem label:''January'' value:''120'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''February'' value:''45'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''March'' value:''38'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''April'' value:''34'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''May'' value:''12'' ]]</span> <span>[[ enddataitem ]]</span>
    <span>[[ dataitem label:''June'' value:''100'' ]]</span> <span>[[ enddataitem ]]</span>
{[ endtrendchart ]}
</code></pre>

<h4 id="shortcode-options">Shortcode Options</h4>
<ul>
<li><strong>minimumitems</strong> (0) - The minimum number of dataitems to show. If the number of dataitems provided is less than the minimumitems; the shortcode will create empty dataitems.</li>
<li><strong>maximumitems</strong> (auto) - The maximum number of dataitems to show.</li>
<li><strong>color</strong> - The default color of the dataitems, if no color is set the chart will use the theme''s default color for a trend chart.</li>
<li><strong>yaxismax</strong> (auto) - The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
<li><strong>reverseorder</strong> (false) - If true, the first dataitem will appear last. This is useful to have empty dataitems added using the minimumitems parameter.</li>
<li><strong>height</strong> (70px) - The height of the trend chart.</li>
<li><strong>width</strong> (100%) - The width of the trend chart.</li>
<li><strong>labeldelay</strong> (0) - The delay in milliseconds to show and hide the label tooltips.</li>
</ul>
<h4 id="data-item-options">Data Item Options</h4>
<p>Each "bar" on the trendchart is set using a <code>dataitem</code>.</p>
<pre><code><span>[[ dataitem label:''January'' value:''120'' ]]</span> <span>[[ enddataitem ]]</span>
</code></pre>
<ul>
<li><strong>label</strong> - The label for the data item.</li>
<li><strong>value</strong> - The numeric data point for the item.</li>
<li><strong>color</strong> - The color of the dataitem, which overrides the default value.</li>
</ul>' WHERE ([Guid]='52B27805-7C36-4965-90BD-3AA42D11F2DB')

UPDATE [LavaShortcode] SET [Documentation]=N'<p>
This shortcut takes a large amount of text and converts it to a word cloud of the most popular terms. It''s 
smart enough to take out common words like ''the'', ''and'', etc. Below are the various options for this shortcode. If 
you would like to play with the settings in real-time head over <a href="https://www.jasondavies.com/wordcloud/" target="_blank">the Javascript''s homepage</a>.
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
<li><strong>anglemin</strong> (-90) – The minimum angle to use when drawing the cloud.</li>
<li><strong>anglemax</strong> (90) – The maximum angle to use when drawing the cloud.</li>
</ul>' WHERE ([Guid]='CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4')


UPDATE [LavaShortcode] SET [Documentation]=N'<p>Embedding a Vimeo video is easy, right? Well, what if you want it to be responsive (adjust with the size of the window)? Or what about 
control of what is shown in the player? The Vimeo shortcode helps to shorten (see what we did there) the time it takes to get a video 
up on your Rock site. Here’s how:<br>  </p>

<p>Basic Usage:</p>

<pre>{[ vimeo id:''180467014'' ]}</pre>

<p>This will put the video with the id provide onto your page in a responsive container. The id can be found in the address of the 
Vimeo video. There are also a couple of options for you to add:</p>

<ul>
    <li><b>id</b> (required) – The Vimeo id of the video.</li>
    <li><b>width</b> (100%) – The width you would like the video to be. By default it will be 100% but you can provide any width in percentages, pixels, or any other valid CSS unit of measure.</li>
    <li><b>color</b> (video default) – The color you would like the video controls to use. By default it will use the color set by the video author, but you can provide any hexadecimal  color i.e. <code>#ffffff</code>.</li>
    <li><b>loop</b> (false) – This determines if the name of the video and other information should be shown at the top of the video.</li>
    <li><b>title</b> (false) – This determines if the Vimeo title should be shown.</li>
    <li><b>byline</b> (false) – This determines if the Vimeo byline should be shown.</li>
    <li><b>portrait</b> (false) – This determines if the Vimeo portrait icon should be shown.</li>
    <li><b>autoplay</b> (false) – Should the video start playing once the page is loaded?</li>
</ul>' WHERE ([Guid]='EA1335B7-158F-464F-8994-98C53D4E47FF')


UPDATE [LavaShortcode] SET [Documentation]=N'<p>
    <a href="https://getbootstrap.com/docs/3.4/javascript/#collapse-example-accordion" target="_blank">Bootstrap accordions</a> are a clean way of displaying a large 
    amount of structured content on a page. While they''re not incredibly difficult to make using just HTML this shortcode simplifies the markup 
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
</ul>' WHERE ([Guid]='18F87671-A848-4509-8058-C95682E7BAD4')