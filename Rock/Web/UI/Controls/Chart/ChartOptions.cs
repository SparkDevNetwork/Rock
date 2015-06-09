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
using System;
using System.Linq;

using Newtonsoft.Json;

using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Class that can be JSON'd and used for Flot Charts (properties are case sensitive)
    /// </summary>
    public class ChartOptions
    {
        /// <summary>
        /// Sets the chart style.
        /// </summary>
        /// <param name="chartStyleDefinedValueGuid">The chart style defined value unique identifier.</param>
        public void SetChartStyle( Guid? chartStyleDefinedValueGuid )
        {
            ChartStyle chartStyle = null;
            if ( chartStyleDefinedValueGuid.HasValue )
            {
                var definedValue = DefinedValueCache.Read( chartStyleDefinedValueGuid.Value );
                if ( definedValue != null )
                {
                    chartStyle = ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( "ChartStyle" ) );
                }
            }

            SetChartStyle( chartStyle ?? new ChartStyle() );
        }

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

            this.yaxis.tickFormatter = @"
function (val, axis) {
  // show commas 
  // from http://stackoverflow.com/questions/2901102/how-to-print-a-number-with-commas-as-thousands-separators-in-javascript
  return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}".Trim();

            SetFlotLinesPointsBarsStyle( chartStyle, this.series.lines );
            SetFlotLinesPointsBarsStyle( chartStyle, this.series.bars );
            SetFlotLinesPointsBarsStyle( chartStyle, this.series.points );

            if ( chartStyle.Legend != null )
            {
                this.legend = new Legend();
                this.legend.show = chartStyle.Legend.Show ?? true;
                this.legend.labelBoxBorderColor = chartStyle.Legend.LabelBoxBorderColor;
                this.legend.noColumns = chartStyle.Legend.NoColumns;
                this.legend.position = chartStyle.Legend.Position;
                this.legend.backgroundColor = chartStyle.Legend.BackgroundColor;
                this.legend.backgroundOpacity = chartStyle.Legend.BackgroundOpacity;
                this.legend.container = chartStyle.Legend.Container;
            }

            this.customSettings = this.customSettings ?? new CustomSettings();

            // copy Title styles to Flot custom settings
            if ( chartStyle.Title != null )
            {
                if ( chartStyle.Title.Font != null )
                {
                    this.customSettings.titleFont = new ChartFont();
                    this.customSettings.titleFont.color = chartStyle.Title.Font.Color;
                    this.customSettings.titleFont.family = chartStyle.Title.Font.Family;
                    this.customSettings.titleFont.size = chartStyle.Title.Font.Size;
                }

                if ( chartStyle.Title.Align != null )
                {
                    this.customSettings.titleAlign = chartStyle.Title.Align;
                }
            }

            // copy SubTitle styles to Flot custom settings
            if ( chartStyle.Subtitle != null )
            {
                if ( chartStyle.Subtitle.Font != null )
                {
                    this.customSettings.subtitleFont = new ChartFont();
                    this.customSettings.subtitleFont.color = chartStyle.Subtitle.Font.Color;
                    this.customSettings.subtitleFont.family = chartStyle.Subtitle.Font.Family;
                    this.customSettings.subtitleFont.size = chartStyle.Subtitle.Font.Size;
                }

                if ( chartStyle.Subtitle.Align != null )
                {
                    this.customSettings.subtitleAlign = chartStyle.Subtitle.Align;
                }
            }

            // copy Goal Series Color to Flot custom settings
            this.customSettings.goalSeriesColor = chartStyle.GoalSeriesColor;

            if ( chartStyle.PieLabels != null )
            {
                this.series.pie = this.series.pie ?? new Pie();
                this.series.pie.label = new PieLabel();
                this.series.pie.label.background = new
                {
                    color = chartStyle.PieLabels.BackgroundColor,
                    opacity = chartStyle.PieLabels.BackgroundOpacity
                };
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
                flotAxis.timeformat = styleAxis.DateTimeFormat;
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

        /// <summary>
        /// Gets or sets the custom settings.
        /// </summary>
        /// <value>
        /// The custom settings.
        /// </value>
        public CustomSettings customSettings { get; set; }
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
        /// Gets or sets the tick formatter which can be null or a custom javascipt function.
        /// Example: tickFormatter = function (val, axis) {
        ///    if (val > 1000000)
        ///        return (val / 1000000).toFixed(axis.tickDecimals) + " MB";
        ///    else if (val > 1000)
        ///        return (val / 1000).toFixed(axis.tickDecimals) + " kB";
        ///    else
        ///        return val.toFixed(axis.tickDecimals) + " B";
        /// }
        /// </summary>
        /// <value>
        /// The tick formatter.
        /// </value>
        [JsonConverter( typeof( StringAsLiteralJavascriptJsonConverter ) )]
        public string tickFormatter { get; set; }

        /// <summary>
        /// Gets or sets the tick decimals.
        /// </summary>
        /// <value>
        /// The tick decimals.
        /// </value>
        public int? tickDecimals { get; set; }

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
        /// Gets or sets the length of the tick.
        /// </summary>
        /// <value>
        /// The length of the tick.
        /// </value>
        public dynamic tickLength { get; set; }

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
        /// <summary>
        /// The bottom
        /// </summary>
        bottom,

        /// <summary>
        /// The top
        /// </summary>
        top,

        /// <summary>
        /// The left
        /// </summary>
        left,

        /// <summary>
        /// The right
        /// </summary>
        right
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( EnumAsStringJsonConverter ) )]
    public enum AxisMode
    {
        /// <summary>
        /// The time
        /// </summary>
        time,

        /// <summary>
        /// The categories (text)
        /// </summary>
        categories
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
    public class Pie
    {
        /// <summary>
        /// Gets or sets the show.
        /// </summary>
        /// <value>
        /// The show.
        /// </value>
        public bool show
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the radius as a percentage of available space
        /// Defaults to .75 if there is a legend, or 1 if there isn't
        /// </summary>
        /// <value>
        /// The radius.
        /// </value>
        public double? radius { get; set; }

        /// <summary>
        /// Factor of PI used for the starting angle (in radians) It can range between 0 and 2 (where 0 and 2 have the same result).
        /// Default is 1.5
        /// </summary>
        /// <value>
        /// The start angle.
        /// </value>
        public double? startAngle { get; set; }

        /// <summary>
        /// Percentage of tilt ranging from 0 and 1, where 1 has no change (fully vertical) and 0 is completely flat (fully horizontal -- in which case nothing actually gets drawn).
        /// Default is 1.0
        /// </summary>
        /// <value>
        /// The tilt.
        /// </value>
        public double? tilt { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public PieLabel label { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PieLabel
    {
        /// <summary>
        /// Gets or sets the show.
        /// </summary>
        /// <value>
        /// The show.
        /// </value>
        public bool? show { get; set; }

        /// <summary>
        /// Sets the radius at which to place the labels. If value is between 0 and 1 (inclusive) then it will use that as a percentage of the available space (size of the container), otherwise it will use the value as a direct pixel length
        /// </summary>
        /// <value>
        /// The radius.
        /// </value>
        public double? radius { get; set; }

        /// <summary>
        /// Gets or sets the threshold.
        /// Hides the labels of any pie slice that is smaller than the specified percentage (ranging from 0 to 1) i.e. a value of '0.03' will hide all slices 3% or less of the total.
        /// </summary>
        /// <value>
        /// The threshold.
        /// </value>
        public double? threshold { get; set; }

        /// <summary>
        /// Gets or sets the background.
        /// color is color of the positioned labels. If null, the plugin will automatically use the color of the slice.
        /// specify background as object 
        /// {
        ///     color: "#515151",
        ///     opacity: .5
        /// }
        /// </summary>
        /// <value>
        /// The background.
        /// </value>
        public dynamic background { get; set; }

        /// <summary>
        /// Gets or sets the formatter.
        /// </summary>
        /// <value>
        /// The formatter.
        /// </value>
        [JsonConverter( typeof( StringAsLiteralJavascriptJsonConverter ) )]
        public string formatter { get; set; }
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

        /// <summary>
        /// Gets or sets the pie.
        /// </summary>
        /// <value>
        /// The pie.
        /// </value>
        public Pie pie { get; set; }
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

    /// <summary>
    /// Settings that aren't part of the Font spec
    /// </summary>
    public class CustomSettings
    {
        /// <summary>
        /// Gets or sets the title font.
        /// </summary>
        /// <value>
        /// The title font.
        /// </value>
        public ChartFont titleFont { get; set; }

        /// <summary>
        /// Gets or sets the title align.
        /// </summary>
        /// <value>
        /// The title align.
        /// </value>
        public string titleAlign { get; set; }

        /// <summary>
        /// Gets or sets the subtitle font.
        /// </summary>
        /// <value>
        /// The subtitle font.
        /// </value>
        public ChartFont subtitleFont { get; set; }

        /// <summary>
        /// Gets or sets the subtitle align.
        /// </summary>
        /// <value>
        /// The subtitle align.
        /// </value>
        public string subtitleAlign { get; set; }

        /// <summary>
        /// Gets or sets the color of the goal series.
        /// </summary>
        /// <value>
        /// The color of the goal series.
        /// </value>
        public string goalSeriesColor { get; set; }
    }
}