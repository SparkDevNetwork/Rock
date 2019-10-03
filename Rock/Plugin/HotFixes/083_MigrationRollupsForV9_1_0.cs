﻿// <copyright>
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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 83, "1.9.0" )]
    public class MigrationRollupsForV9_1_0 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AddNewAbsenceGroupMemberNotificationEmailTemplate();
            FixGooglestaticmapShortcode();
            FixGoogleMapTypeo();
            FixAccordianTypeo();
            FixWordCloudTypeo();
            FixParallaxTypeo();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// SK: Added new Absence Group Member Notification Email Template
        /// </summary>
        private void AddNewAbsenceGroupMemberNotificationEmailTemplate()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Absence Group Member Notification", "", "", "", "", "", "Absent Group Members | {{ 'Global'  | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}  <p>  {{ Person.NickName }}, </p>  <p>  We wanted to make you aware of additional individuals who have missed the last few times that your {{ Group.Name }} group met. The individuals' names and contact information can be found below. Our goal is to find out if there’s a situation the church should know about, or if perhaps they’ve found another group to join. </p>   <table cellpadding=""25"">  {% for absentMember in AbsentMembers %}   <tr><td>   <strong>{{ absentMember.FullName }}</strong><br />   {%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}   {%- assign homePhone = absentMember.PhoneNumbers | Where:'NumberTypeValueId', 13 | Select:'NumberFormatted' -%}   {%- assign homeAddress = absentMember | Address:'Home' -%}      {%- if mobilePhone != empty -%}   Mobile Phone: {{ mobilePhone }}<br />   {%- endif -%}      {%- if homePhone != empty -%}   Home Phone: {{ homePhone }}<br />   {%- endif -%}      {%- if absentMember.Email != empty -%}   {{ absentMember.Email }}<br />   {%- endif -%}      <p>   {%- if homeAddress != empty -%}   Home Address <br />   {{ homeAddress }}   {%- endif -%}   </p>      </td></tr>  {% endfor %}  </table>    <p>   Once you have connected with these individuals, please update their status in your group if appropriate, or let us know if there’s anything the church needs to be aware of in their lives.  </p>   <p>   Thank you for your ongoing commitment to {{ 'Global' | Attribute:'OrganizationName' }}.  </p>   {{ 'Global' | Attribute:'EmailFooter' }}", "8747131E-3EDA-4FB0-A484-C2D2BE3918BA" );
        }

        /// <summary>
        /// GJ: Fix typo on googlestaticmap shortcode
        /// </summary>
        private void FixGooglestaticmapShortcode()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
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
    <li><strong>format</strong> (png8) – The format type of the image that is returned. Valid values include png8, png32, gif, jpg, jpg-baseline.</li>
    <li><strong>style</strong> (optional) – The returned map can be styled in an infinite (or close to) number of ways. See the Google Static Map documentation for 
        details (https://developers.google.com/maps/documentation/static-maps/styling).</li>
</ul>' WHERE ([Guid]='2DD53FE6-6EB2-4EC8-A965-3F71054F7983')" );
        }

        /// <summary>
        /// GJ: Fix Google Map Typo (Sever)
        /// </summary>
        private void FixGoogleMapTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    Adding a Google map to your page always starts out sounding easy… until… you get to the details. Soon the whole day is wasted and you don''t have much to 
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
        &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;
        It''s not as grand as it sounds.&lt;br&gt;
        &lt;img src=""https://rockrms.blob.core.windows.net/misc/spark-logo.png"" width=""179"" height=""47""&gt;          
    [[ endmarker ]]
    [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
{[ endgooglemap ]}</pre>

<p></p><p>
    Note: A list of great resources for custom map markers is below:
</p>

<ul>
    <li><a href=""http://map-icons.com/"">Map Icons</a></li>
    <li><a href=""https://mapicons.mapsmarker.com/"">Map Icons Collection</a></li>
    <li><a href=""https://github.com/Concept211/Google-Maps-Markers"">Google Maps Markers</a></li>
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
    <li><strong>markeranimation</strong> (none) – The marker animation type. Options include: ''none'', ‘bounce'' (markers bounce continuously) or ''drop'' (markers drop in).</li>
    <li><strong>scrollwheel</strong> (true) – Determines if the scroll wheel should control the zoom level when the mouse is over the map.</li>
    <li><strong>draggable</strong> (true) – Determines if the mouse should be allowed to drag the center point of the map (allow the map to be moved).</li>
    <li><strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ‘cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.</li>
</ul>

<p>
    As you can see there are a lot of options in working with your map. You can also style your map by changing the colors. You do this by providing 
    the styling information in a separate [[ style ]] section. The styling settings for Google Maps is not pretty to look at or configure for that matter. Luckily, there 
    are several sites that allow you to download preconfigured map styles. Two of the best are called <a href=""https://snazzymaps.com"">SnazzyMaps</a> and 
    <a href=""https://mapstyle.withgoogle.com"">Map Style</a>. Below is an example showing how to add styling to your maps.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]] 
     [[ style ]]
        [{""featureType"":""all"",""elementType"":""all"",""stylers"":[{""visibility"":""on""}]},{""featureType"":""all"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""},{""saturation"":""-100""}]},{""featureType"":""all"",""elementType"":""labels.text.fill"",""stylers"":[{""saturation"":36},{""color"":""#000000""},{""lightness"":40},{""visibility"":""off""}]},{""featureType"":""all"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""},{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""all"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""administrative"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""administrative"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#000000""},{""lightness"":17},{""weight"":1.2}]},{""featureType"":""landscape"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""landscape"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape.natural"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry"",""stylers"":[{""lightness"":21}]},{""featureType"":""poi"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""road"",""elementType"":""geometry"",""stylers"":[{""visibility"":""on""},{""color"":""#7f8d89""}]},{""featureType"":""road"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.highway"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":17}]},{""featureType"":""road.highway"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":29},{""weight"":0.2}]},{""featureType"":""road.arterial"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":18}]},{""featureType"":""road.arterial"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.arterial"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""road.local"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""transit"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":19}]},{""featureType"":""water"",""elementType"":""all"",""stylers"":[{""color"":""#2b3638""},{""visibility"":""on""}]},{""featureType"":""water"",""elementType"":""geometry"",""stylers"":[{""color"":""#2b3638""},{""lightness"":17}]},{""featureType"":""water"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.fill"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]}]
    [[ endstyle]]
{[ endgooglemap ]}</pre>

<p>
    Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply copy and pasted straight from SnazzyMaps!
</p>'
                WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')" );
        }

        /// <summary>
        /// GJ: Fix Accordion Typo (Simplifies)
        /// </summary>
        private void FixAccordianTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    <a href=""https://getbootstrap.com/docs/3.3/javascript/#collapse-example-accordion"">Bootstrap accordions</a> are a clean way of displaying a large 
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
</ul>'
                WHERE ([Guid]='18F87671-A848-4509-8058-C95682E7BAD4')" );
        }

        /// <summary>
        /// GJ: Fix Wordcloud Typo
        /// </summary>
        private void FixWordCloudTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode] SET [Documentation]=N'<p>
    This short cut takes a large amount of text and converts it to a word cloud of the most popular terms. It''s 
    smart enough to take out common words like ''the'', ''and'', etc. Below are the various options for this shortcode. If 
    you would like to play with the settings in real-time head over <a href=""https://www.jasondavies.com/wordcloud/"">the Javascript''s homepage</a>.
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
</ul>'
                WHERE ([Guid]='CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4')" );
        }

        /// <summary>
        /// GJ: Parallax Typos
        /// </summary>
        private void FixParallaxTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>
<pre>{[ parallax image:''http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'' contentpadding:''20px'' ]}
    &lt;h1&gt;Hello World&lt;/h1&gt;
{[ endparallax ]}</pre>

<p>  
    This shortcode takes the content you provide it and places it into a div with a parallax background using the image you provide in the ''image'' 
    parameter. As always, there are several parameters.
</p>
    
<ul>
    <li><strong>image</strong> (required) – A valid URL to the image that should be used as the background.</li><li><b>height</b> (200px) – The minimum height of the content. This is useful if you want your section to not have any 
    content, but instead be just the parallax image.</li>
    <li><strong>videourl</strong> - This is the URL to use if you''d like a video background.</li>
    <li><strong>speed</strong> (50) – the speed that the background should scroll. The value of 0 means the image will be fixed in place, the value of 100 would make the background scroll quick up as the page scrolls down, while the value of -100 would scroll quickly in the opposite direction.</li>
    <li><strong>zindex</strong> (1) – The z-index of the background image. Depending on your design you may need to adjust the z-index of the parallax image. </li>
    <li><strong>position</strong> (center center) - This is analogous to the background-position css property. Specify coordinates as top, bottom, right, left, center, or pixel values (e.g. -10px 0px). The parallax image will be positioned as close to these values as possible while still covering the target element.</li>
    <li><strong>contentpadding</strong> (0) – The amount of padding you’d like to have around your content. You can provide any valid CSS padding value. For example, the value ‘200px 20px’ would give you 200px top and bottom and 20px left and right.</li>
    <li><strong>contentcolor</strong> (#fff = white) – The font color you’d like to use for your content. This simplifies the styling of your content.</li>
    <li><strong>contentalign</strong> (center) – The alignment of your content inside of the section. </li>
    <li><strong>noios</strong> (false) – Disables the effect on iOS devices.</li>
    <li><strong>noandriod</strong> (center) – Disables the effect on Android devices.</li>
</ul>
<p>Note: Due to the javascript requirements of this shortcode, you will need to do a full page reload before changes to the shortcode appear on your page.</p>'
                WHERE ([Guid]='4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4')" );
        }

    }
}
