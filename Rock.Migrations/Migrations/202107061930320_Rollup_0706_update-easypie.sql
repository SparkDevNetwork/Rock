UPDATE [LavaShortcode] SET [Documentation]=N'<p>Easy Pie Chart is the perfect solution when you need to display a single percentage value on a chart. In fact it''s as simple as <code>{[easypie value:''60'']}{[endeasypie]}</code></p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-main.png" class="img-responsive" alt="Easy Pie" width="100" height="100" loading="lazy"></p>

<p>Each has the following basic settings settings:</p>
<ul>
<li><strong>value</strong> (0) - The data point for the item in percent (0 - 100)</li>
<li><strong>chartwidth</strong> (95px) - The width of the chart in pixels.</li>
</ul>
<p>Advanced options include:</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - The color of the circular bar.</li>
<li><strong>label</strong> - Optional label to display inside the chart.</li>
<li><strong>valuesize</strong> - Font size of the rendered percentage value.</li>
<li><strong>labelsize</strong> - Font size of the label.</li>
<li><strong>trackcolor</strong> (rgba(0,0,0,0.04)) - The CSS color of the track for the bar.</li>
<li><strong>scalelinelength</strong> (0px) - The length of the scale lines.</li>
<li><strong>scalelinecolor</strong> (#dfe0e0) - The CSS color of the scale lines.</li>
<li><strong>linecap</strong> (none) - Defines how the ending of the bar line looks like. (Possible values are none, round and square).</li>
<li><strong>trackwidth</strong>  - Width of the bar line in pixels. Default value is computed based on the chart width.</li>
<li><strong>animateduration</strong> (1500) - Time in milliseconds for a eased animation of the bar growing, or 0 to deactivate.</li>
</ul>
<h5>Advanced Options</h5>
<ul>
<li><strong>cssclasstarget</strong> - Set value to create many of the same type of chart.</li>
</ul>
<h3>Avatar</h3>
<p>Add a avatar to the center of the pie chart just by adding a image.</p>
<h3>Nested Charts</h3>
<p>Add an additional item named [[ easypie ]] using the same attributes</p>
<p>Example Charts and Markup is shown below:</p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-examples.png" class="img-responsive" alt="Easy Pie Examples" width="607" height="121" loading="lazy"></p>
<pre><code>/* Small Chart (A) */
{[ easypie value:''25'' scalelinelength:''0'' chartwidth:''50'' ]} {[ endeasypie ]}

/* Standard Chart (B) */
{[ easypie value:''75'' ]} {[ endeasypie ]}

/* Chart With Scalelines and Color (C) */
{[easypie value:''90'' chartwidth:''120'' scalelinelength:''8'' primarycolor:''#16C98D'']}
{[ endeasypie]}

/* Chart Avatar (D) */
{[easypie value:''90'' chartwidth:''120'' primarycolor:''#D4442E'']}
&lt;img src="https://rock.rocksolidchurchdemo.com/GetImage.ashx?id=69" alt="Ted Decker"&gt;
{[ endeasypie]}

/* Chart with Labels (E) */
{[ easypie value:''90'' label:''Memory'' showpercent:''true'' primarycolor:''#FFC870'' chartwidth:''120'']} {[ endeasypie ]}

/* Nested Chart (F) */
{[easypie value:''90'' chartwidth:''120'' primarycolor:''#009CE3'']}
[[easypie value:''50'' primarycolor:''#16C98D'']][[endeasypie]]
{[ endeasypie]}
</code></pre>
' WHERE ([Guid]='96A8284E-96A6-4E38-969C-640F0BDC8EB8')