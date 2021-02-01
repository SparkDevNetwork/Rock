UPDATE [LavaShortcode] SET [Documentation]=N'<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>
<pre>{[ parallax image:''https://source.unsplash.com/phIFdC6lA4E/1920x1080'' contentpadding:''20px'' ]}
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
<p>Note: Due to the javascript requirements of this shortcode, you will need to do a full page reload before changes to the shortcode appear on your page.</p>' WHERE ([Guid]='4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4')