// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using Newtonsoft.Json;
using Rock.Utility;

namespace Rock.Reporting.Dashboard.Flot
{
    /// <summary>
    /// Class that can be JSON'd and used for Flot Charts (properties are case sensitive)
    /// </summary>
    public class ChartOptions
    {
        /// <summary>
        /// Sets the chart style.
        /// </summary>
        /// <param name="chartStyle">The chart style.</param>
        public void SetChartStyle( ChartStyle chartStyle )
        {
            if ( chartStyle.SeriesColors != null && chartStyle.SeriesColors.Count() > 0 )
            {
                this.colors = chartStyle.SeriesColors.ToArray();
            }

            if ( chartStyle.Grid != null )
            {
                this.grid = this.grid ?? new GridOptions();
                this.grid.backgroundColor = chartStyle.Grid.BackgroundColorGradient != null ? ToFlotColorGradient( chartStyle.Grid.BackgroundColorGradient ) : chartStyle.Grid.BackgroundColor;
                this.grid.color = chartStyle.Grid.ColorGradient != null ? ToFlotColorGradient( chartStyle.Grid.ColorGradient ) : chartStyle.Grid.Color;
                this.grid.borderWidth = chartStyle.Grid.BorderWidth;
                this.grid.borderColor = chartStyle.Grid.BorderColor;
            }

            this.xaxis = this.xaxis ?? new AxisOptions();
            SetFlotAxisStyle( chartStyle.XAxis, this.xaxis );
            this.yaxis = this.yaxis ?? new AxisOptions();
            SetFlotAxisStyle( chartStyle.YAxis, this.yaxis );

            SetFlotLinesPointsBarsStyle( chartStyle, this.series.lines );
            SetFlotLinesPointsBarsStyle( chartStyle, this.series.bars );
            SetFlotLinesPointsBarsStyle( chartStyle, this.series.points );

            if ( chartStyle.Legend != null )
            {
                this.legend = new Legend();
                this.legend.backgroundColor = chartStyle.Legend.BackgroundColor;
                this.legend.backgroundOpacity = chartStyle.Legend.BackgroundOpacity;
                this.legend.labelBoxBorderColor = chartStyle.Legend.LabelBoxBorderColor;
            }
        }

        /// <summary>
        /// Sets the flot lines points bars style.
        /// </summary>
        /// <param name="chartStyle">The chart style.</param>
        /// <param name="linesPointsBars">The lines points bars.</param>
        private void SetFlotLinesPointsBarsStyle( ChartStyle chartStyle, LinesPointsBars linesPointsBars )
        {
            if ( linesPointsBars != null )
            {
                linesPointsBars.fill = chartStyle.FillOpacity;
                linesPointsBars.fillColor = chartStyle.FillColor;
            }
        }

        /// <summary>
        /// Sets the flot axis style.
        /// </summary>
        /// <param name="styleAxis">The style axis.</param>
        /// <param name="flotAxis">The flot axis.</param>
        private void SetFlotAxisStyle( AxisStyle styleAxis, AxisOptions flotAxis )
        {
            if ( styleAxis != null )
            {
                flotAxis.color = styleAxis.Color;
                if ( styleAxis.Font != null )
                {
                    flotAxis.font.color = styleAxis.Font.Color;
                    flotAxis.font.family = styleAxis.Font.Family;
                    flotAxis.font.size = styleAxis.Font.Size;
                }
            }
        }

        /// <summary>
        /// convert a style color gradient to flot color specification for a gradient
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        private static dynamic ToFlotColorGradient( string[] source )
        {
            dynamic dest = null;
            if ( source != null && source.Count() > 0 )
            {
                if ( source.Count() > 1 )
                {
                    dest = new
                    {
                        colors = source
                    };
                }
                else
                {
                    dest = source[0];
                }
            }

            return dest;
        }

        /// <summary>
        /// Gets or sets the xaxis.
        /// </summary>
        /// <value>
        /// The xaxis.
        /// </value>
        public AxisOptions xaxis { get; set; }

        /// <summary>
        /// Gets or sets the yaxis.
        /// </summary>
        /// <value>
        /// The yaxis.
        /// </value>
        public AxisOptions yaxis { get; set; }

        /// <summary>
        /// Gets or sets the series.
        /// </summary>
        /// <value>
        /// The series.
        /// </value>
        public SeriesOptions series { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        /// <value>
        /// The grid.
        /// </value>
        public GridOptions grid { get; set; }

        /// <summary>
        /// Gets or sets the array of colors to use for each series 
        /// Series 1 will use colors[0], Series 2 will use colors[1], etc
        /// Leave null to autogenerate
        /// </summary>
        /// <value>
        /// The colors.
        /// </value>
        public string[] colors { get; set; }

        /// <summary>
        /// Gets or sets the legend.
        /// </summary>
        /// <value>
        /// The legend.
        /// </value>
        public Legend legend { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AxisOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AxisOptions"/> class.
        /// </summary>
        public AxisOptions()
        {
            font = new ChartFont();
        }

        /// <summary>
        /// Gets or sets the show.
        /// </summary>
        /// <value>
        /// The show.
        /// </value>
        public bool? show { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public AxisPosition? position { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public AxisMode? mode { get; set; }

        /// <summary>
        /// Gets or sets the timeformat.
        /// </summary>
        /// <value>
        /// The timeformat.
        /// </value>
        public string timeformat { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string color { get; set; }

        /// <summary>
        /// Gets or sets the color of the tick.
        /// </summary>
        /// <value>
        /// The color of the tick.
        /// </value>
        public string tickColor { get; set; }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        /// <value>
        /// The font.
        /// </value>
        public ChartFont font { get; set; }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double? min { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public double? max { get; set; }

        /// <summary>
        /// Gets or sets the ticks.
        /// null or double (number of ticks) or double[] (specific ticks)
        /// </summary>
        /// <value>
        /// The ticks.
        /// </value>
        public dynamic ticks { get; set; }

        /// <summary>
        /// Gets or sets the size of the tick.
        /// null or double (number of ticks) or double[] (specific tickSize per tick)
        /// for time mode, "tickSize" and "minTickSize" are in the form "[value, unit]" where unit is one of "second", "minute", "hour", "day", "month" and "year". ex: [3, "month"]
        /// </summary>
        /// <value>
        /// The size of the tick.
        /// </value>
        public dynamic tickSize { get; set; }

        /// <summary>
        /// Gets or sets the minimum size of the tick.
        /// null or double (number of ticks) or double[] (specific minTickSize per tick).
        /// for time mode, "tickSize" and "minTickSize" are in the form "[value, unit]" where unit is one of "second", "minute", "hour", "day", "month" and "year". ex: [3, "month"]
        /// </summary>
        /// <value>
        /// The minimum size of the tick.
        /// </value>
        public dynamic minTickSize { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( EnumAsStringJsonConverter ) )]
    public enum AxisPosition
    {
        bottom,
        top,
        left,
        right
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( EnumAsStringJsonConverter ) )]
    public enum AxisMode
    {
        time
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChartFont
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int? size { get; set; }

        /// <summary>
        /// Gets or sets the height of the line.
        /// </summary>
        /// <value>
        /// The height of the line.
        /// </value>
        public int? lineHeight { get; set; }

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public string style { get; set; }

        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        /// <value>
        /// The weight.
        /// </value>
        public string weight { get; set; }

        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string family { get; set; }

        /// <summary>
        /// Gets or sets the variant.
        /// </summary>
        /// <value>
        /// The variant.
        /// </value>
        public string variant { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string color { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Legend
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show]; otherwise, <c>false</c>.
        /// </value>
        public bool? show { get; set; }

        /// <summary>
        /// Gets or sets the color of the label box border.
        /// </summary>
        /// <value>
        /// The color of the label box border.
        /// </value>
        public string labelBoxBorderColor { get; set; }

        /// <summary>
        /// Gets or sets the no columns.
        /// </summary>
        /// <value>
        /// The no columns.
        /// </value>
        public int? noColumns { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// "ne" or "nw" or "se" or "sw"
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public string position { get; set; }

        /// <summary>
        /// Gets or sets the margin.
        /// </summary>
        /// <value>
        /// The margin.
        /// </value>
        public int? margin { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string backgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background opacity.
        /// </summary>
        /// <value>
        /// The background opacity.
        /// </value>
        public double? backgroundOpacity { get; set; }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        public string container { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class LinesPointsBars
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show]; otherwise, <c>false</c>.
        /// </value>
        public bool? show { get; set; }

        /// <summary>
        /// Gets or sets the width of the line.
        /// </summary>
        /// <value>
        /// The width of the line.
        /// </value>
        public int? lineWidth { get; set; }

        /// <summary>
        /// Gets or sets the fill opacity
        /// </summary>
        /// <value>
        /// The fill.
        /// </value>
        public double? fill { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// </summary>
        /// <value>
        /// The color of the fill.
        /// </value>
        public string fillColor { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Lines : LinesPointsBars
    {
        /// <summary>
        /// Gets or sets the zero.
        /// </summary>
        /// <value>
        /// The zero.
        /// </value>
        public bool? zero { get; set; }

        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        public bool? steps { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Bars : LinesPointsBars
    {
        /// <summary>
        /// Gets or sets the zero.
        /// </summary>
        /// <value>
        /// The zero.
        /// </value>
        public bool? zero { get; set; }

        /// <summary>
        /// Gets or sets the width of the bar.
        /// </summary>
        /// <value>
        /// The width of the bar.
        /// </value>
        public double? barWidth { get; set; }

        /// <summary>
        /// Gets or sets the align.
        /// </summary>
        /// <value>
        /// The align.
        /// </value>
        public string align { get; set; }

        /// <summary>
        /// Gets or sets the horizontal.
        /// </summary>
        /// <value>
        /// The horizontal.
        /// </value>
        public bool? horizontal { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Points : LinesPointsBars
    {
        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>
        /// The radius.
        /// </value>
        public double? radius { get; set; }

        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        /// <value>
        /// The symbol.
        /// </value>
        public string symbol { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SeriesOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeriesOptions"/> class.
        /// </summary>
        public SeriesOptions( bool showBars, bool showLines, bool showPoints )
        {
            if ( showBars )
            {
                bars = new Bars();
                bars.show = true;
            }

            if ( showLines )
            {
                lines = new Lines();
                lines.show = true;
            }

            if ( showPoints )
            {
                points = new Points();
                points.show = true;
            }
        }

        /// <summary>
        /// Gets or sets the bars.
        /// </summary>
        /// <value>
        /// The bars.
        /// </value>
        public Bars bars { get; set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>
        /// The lines.
        /// </value>
        public Lines lines { get; set; }

        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        public Points points { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GridOptions
    {
        /// <summary>
        /// Gets or sets the show.
        /// </summary>
        /// <value>
        /// The show.
        /// </value>
        public bool? show { get; set; }

        /// <summary>
        /// Gets or sets the above data.
        /// </summary>
        /// <value>
        /// The above data.
        /// </value>
        public bool? aboveData { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public dynamic color { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public dynamic backgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the margin.
        /// Can be set to Null, a number, or a { top, right, bottom, left } object;
        /// </summary>
        /// <value>
        /// The margin.
        /// </value>
        public dynamic margin { get; set; }

        /// <summary>
        /// Gets or sets the label margin.
        /// </summary>
        /// <value>
        /// The label margin.
        /// </value>
        public double? labelMargin { get; set; }

        /// <summary>
        /// Gets or sets the axis margin.
        /// </summary>
        /// <value>
        /// The axis margin.
        /// </value>
        public double? axisMargin { get; set; }

        /// <summary>
        /// Gets or sets the width of the border.
        /// Can be set to Null, a number, or a { top, right, bottom, left } object;
        /// </summary>
        /// <value>
        /// The width of the border.
        /// </value>
        public dynamic borderWidth { get; set; }

        /// <summary>
        /// Gets or sets the color of the border.
        /// Can be set to Null, a htmlColor string, or a { top, right, bottom, left } object;
        /// </summary>
        /// <value>
        /// The color of the border.
        /// </value>
        public dynamic borderColor { get; set; }

        /// <summary>
        /// Gets or sets the minimum border margin.
        /// </summary>
        /// <value>
        /// The minimum border margin.
        /// </value>
        public double? minBorderMargin { get; set; }

        /// <summary>
        /// Gets or sets the clickable.
        /// </summary>
        /// <value>
        /// The clickable.
        /// </value>
        public bool? clickable { get; set; }

        /// <summary>
        /// Gets or sets the hoverable.
        /// </summary>
        /// <value>
        /// The hoverable.
        /// </value>
        public bool? hoverable { get; set; }

        /// <summary>
        /// Gets or sets the automatic highlight.
        /// </summary>
        /// <value>
        /// The automatic highlight.
        /// </value>
        public bool? autoHighlight { get; set; }

        /// <summary>
        /// Gets or sets the mouse active radius.
        /// </summary>
        /// <value>
        /// The mouse active radius.
        /// </value>
        public double? mouseActiveRadius { get; set; }
    }
}