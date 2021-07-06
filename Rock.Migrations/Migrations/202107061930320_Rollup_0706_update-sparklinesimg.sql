UPDATE [LavaShortcode] SET [Documentation]=N'<p>Creating Sparklines with Lava is ridiculously easy with this shortcode. Basic examples for each type are shown below.</p>
<pre><code>{[ sparkline type:''line'' data:''5,6,7,9,9,5,3,2,2,4,6,7'' ]}

{[ sparkline type:''bar'' data:''5,6,7,2,0,-4,-2,4 '' ]}

{[ sparkline type:''tristate'' data:''1,1,0,1,-1,-1,1,-1,0,0,1,1'' ]}

{[ sparkline type:''discrete'' data:''4,6,7,7,4,3,2,1,4,4'' ]}

{[ sparkline type:''bullet'' data:''10,12,12,9,7'' ]}

{[ sparkline type:''pie'' data:''1,1,2'' ]}

{[ sparkline type:''box'' data:''4,27,34,52,54,59,61,68,78,82,85,87,91,93,100'' ]}
</code></pre>
<h4>Common Options</h4>
<ul>
<li><strong>type</strong> - (line) - Type of chart to display.	One of ''line'' (default), ''bar'', ''tristate'', ''discrete'', ''bullet'', ''pie'' or ''box''</li>
<li><strong>chartwidth</strong> - (auto) - Width of the chart - Defaults to ''auto'' - May be any valid css width - 1.5em, 20px, etc (using a number without a unit specifier won''t do what you want) - This option does nothing for bar and tristate chars (see barwidth)</li>
<li><strong>chartheight</strong> - (auto) - Height of the chart - Defaults to ''auto'' (line height of the containing tag)</li>
<li><strong>primarycolor</strong> (#ee7625) - The primary color of the chart. This will set the colors for all markers and lines.</li>
<li><strong>yaxisminvalue</strong>  - Specify the minimum value to use for the range of Y values of the chart - Defaults to the minimum value supplied</li>
<li><strong>yaxismaxvalue</strong> - Specify the maximum value to use for the range of Y values of the chart - Defaults to the maximum value supplied</li>
</ul>
<h4>Line Charts</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-line.png" alt="Line Chart" width="87" height="41" loading="lazy"></p>
<p>Line charts are the default chart type, but to specify the type explicitly set an option called &quot;type&quot; to &quot;line&quot;.</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - The color of the line.</li>
<li><strong>fillcolor</strong> (#f7bd97) - The fill color of the area below the line.</li>
<li><strong>linewidth</strong> (1px) -  The width, in pixels, of the line.</li>
<li><strong>lastpointcolor</strong> (#f80) - The color of the last data point. Provide ''none'' to not show on the chart.</li>
<li><strong>maxpointcolor</strong> (#f80) - The color of the largest value in the data points. Provide ''none'' to not show on the chart.</li>
<li><strong>minpointcolor</strong> (#f80) - The color of the smallest value in the data points. Provide ''none'' to not show on the chart.</li>
<li><strong>pointrollovercolor</strong> (#f7bd97) -  The color of the markers when you rollover them.</li>
<li><strong>linerollovercolor</strong> (#f7bd97) - The color of the line when you rollover data points.</li>
<li><strong>pointradius</strong> (1.5px) - Radius in pixels of all spot markers on the chart</li>
<li><strong>xaxisminvalue</strong> (undefined) - Specifies the minimum value to use for the X value of the chart</li>
<li><strong>xaxismaxvalue</strong> (undefined) - Specifies the maximum value to use for the X value of the chart</li>
<li><strong>normalminvalue</strong> - Specify minimum threshold values between which to draw a bar to denote the &quot;normal&quot; or expected range of values.</li>
<li><strong>normalmaxvalue</strong> - Specify minimum threshold values between which to draw a bar to denote the &quot;normal&quot; or expected range of values.</li>
<li><strong>normalrangecolor</strong> - CSS color used</li>
</ul>
<h4>Bar Charts</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-bar.png" alt="Bar Chart" width="87" height="41" loading="lazy"></p>
<p>Set the &quot;type&quot; option to &quot;bar&quot; to generate bar charts. Values can be omitted by using the &quot;null&quot; value instead of a number.</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - The color of the bar.</li>
<li><strong>negativecolor</strong> (#f44) - CSS color used for negative values</li>
<li><strong>zerocolor</strong> (undefined) - CSS color used for values equal to zero</li>
<li><strong>nullcolor</strong> (undefined) - CSS color used for values equal to null - By default null values are omitted entirely, but setting this adds a thin marker for the entry - This can be useful if your chart is pretty sparse; perhaps try setting it to a light grey or something equally unobtrusive</li>
<li><strong>barwidth</strong> (4px) - Width of each bar, in pixels</li>
<li><strong>barspacing</strong> (1px) - Space between each bar, in pixels</li>
<li><strong>zeroaxis</strong> (true) - Centers the y-axis at zero if true</li>
</ul>
<h4>Tristate Charts</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-tristate.png" alt="Tristate Chart" width="87" height="41" loading="lazy"></p>
<p>Tri-state charts are useful to show win-lose-draw information, such as the SF Giants recent game results at the top of the page. You can also use the colorMap option to use different colours for different values, or for arbitrary positions in the chart.</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - CSS color for positive (win) values</li>
<li><strong>negativecolor</strong> (#f44) - CSS color for negative (lose) values</li>
<li><strong>zerocolor</strong> (undefined) - CSS color for zero (draw) values</li>
<li><strong>barwidth</strong> (4px) - Width of each bar, in pixels (integer)</li>
<li><strong>barspacing</strong> (1px) - Space between each bar, in pixels (integer)</li>
</ul>
<h4>Discrete Charts</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-discrete.png" alt="Discrete Chart" width="87" height="41" loading="lazy"></p>
<p>Discrete charts provide a separated thin vertical line for each value.</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - CSS color of the line.</li>
<li><strong>lineheight</strong> (auto) - Height of each line in pixels - Defaults to 30% of the graph height</li>
<li><strong>thresholdvalue</strong> (0) - Values less than this value will be drawn using thresholdColor instead of lineColor</li>
<li><strong>thresholdcolor</strong> (undefined) - Color to use in combination with thresholdValue</li>
</ul>
<h4>Bullet Graphs</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-bullet.png" alt="Bullet Graphs" width="87" height="41" loading="lazy"></p>
<p>Bullet graphs serve as a replacement for dashboard gauges and meters. (Read more at <a href="https://en.wikipedia.org/wiki/Bullet_graph">Wikipedia</a>)</p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-bullet-labeled.png" alt="Bullet Graph Labeled" width="325" height="159" loading="lazy"></p>
<p><em>Supplied values must be in this order: target, performance, range1, range2, range3, ...</em></p>
<ul>
<li><strong>primarycolor</strong> - The CSS color of the performance measure horizontal bar</li>
<li><strong>targetcolor</strong> (#33f) - The CSS color of the vertical target marker</li>
<li><strong>targetwidth</strong> (3px) - The width of the target marker in pixels (integer)</li>
<li><strong>rangecolors</strong> - Colors to use for each qualitative range background color - This must be a comma seperated list. eg <em>red,green,#22f</em></li>
</ul>
<h4>Pie Charts</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-pie.png" alt="Pie Chart" width="87" height="41" loading="lazy"></p>
<p>What could be better than a tiny piece of pie?</p>
<ul>
<li><strong>fillcolor</strong> - An array of CSS colors to use for pie slices. (Comma seperated list. eg <em>red,green,#22f</em>)</li>
<li><strong>offset</strong> - Angle in degrees to orotate where the first slice will appear - Try -90 or +90</li>
<li><strong>borderwidth</strong> (0) - Width of the border to draw around the whole pie chart, in pixels. Defaults to 0 (no border)</li>
<li><strong>bordercolor</strong> - CSS color to use to draw the pie border. Defaults to #000</li>
</ul>
<h4>Box Plots</h4>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-box.png" alt="Box Plot" width="87" height="41" loading="lazy"></p>
<p>A box plot is a chart of statistical data based on the minimum, first quartile, median, third quartile, and maximum. (Read more at <a href="http://en.wikipedia.org/wiki/Box_plot">Wikipedia</a>)</p>
<p><img src="https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/sparkline-box-labeled.png" alt="Box Plot Labeled" width="363" height="210" loading="lazy"></p>
<ul>
<li><strong>primarycolor</strong> - CSS line color used to outline the box</li>
<li><strong>fillcolor</strong> - CSS fill color used for the box</li>
<li><strong>whiskercolor</strong> - CSS color used to draw the whiskers</li>
<li><strong>showoutliers</strong> (true) - Show or hide the outliers.</li>
<li><strong>outlierlinecolor</strong> - CSS color used to draw the outlier circles</li>
<li><strong>outlierfillcolor</strong> - CSS color used to fill the outlier circles</li>
<li><strong>pointradius</strong> - Radius in pixels to draw the outlier circles</li>
<li><strong>mediancolor</strong> - CSS color used to draw the median line</li>
<li><strong>target</strong> - If set to a value, then a small crosshair is drawn at that point to represent a target value</li>
<li><strong>targetcolor</strong> - CSS color used to draw the target crosshair, if set</li>
<li><strong>yaxisminvalue</strong> - If yaxisminvalue and yaxismaxvalue are set then the scale of the plot is fixed. By default yaxisminvalue and yaxismaxvalue are deduced from the values supplied</li>
<li><strong>yaxismaxvalue</strong> - See yaxisminvalue</li>
</ul>
<h5>Advanced Options</h5>
<ul>
<li><strong>cssclasstarget</strong> - Set value to create many of the same type of chart.</li>
</ul>
<p>Example</p>
<pre><code>{[ sparkline cssclasstarget:''sparkpie'' type:''pie'' ]}

&lt;span class=&quot;sparkpie&quot; data-values=&quot;2,5,10&quot;&gt;Loading...&lt;/span&gt;
&lt;span class=&quot;sparkpie&quot; data-values=&quot;5,10,20&quot;&gt;Loading...&lt;/span&gt;
...
</code></pre>
' WHERE ([Guid]='E7AC1E9B-0200-49AF-967F-0A9D2DD0F968')