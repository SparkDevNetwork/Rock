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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rock.Utility;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// Class that can be JSON'd and used for Flot Charts (properties are case sensitive)
    /// </summary>
    public class FlotChartOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlotChartOptions"/> class.
        /// </summary>
        public FlotChartOptions()
        {
            xaxis = new AxisOptions { mode = AxisMode.time };
            yaxis = new AxisOptions();
            grid = new GridOptions();
        }

        /// <summary>
        /// Sets the theme.
        /// </summary>
        /// <param name="chartTheme">The chart theme.</param>
        public void SetTheme( ChartTheme chartTheme )
        {
            this.colors = chartTheme.SeriesColors.Select( a => System.Drawing.ColorTranslator.ToHtml( a ) ).ToArray();
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
            font.size = 11;
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
        public decimal? min { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public decimal? max { get; set; }
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
        public decimal? backgroundOpacity { get; set; }

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
        /// Gets or sets the fill.
        /// </summary>
        /// <value>
        /// The fill.
        /// </value>
        public bool? fill { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// </summary>
        /// <value>
        /// The color of the fill.
        /// </value>
        public string fillColor { get; set; }
    }

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
        public decimal? radius { get; set; }

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
        public SeriesOptions( bool showBars, bool showLines, bool showPoints)
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
        public string color { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string backgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the margin.
        /// </summary>
        /// <value>
        /// The margin.
        /// </value>
        public MarginOptions margin { get; set; }

        /// <summary>
        /// Gets or sets the label margin.
        /// </summary>
        /// <value>
        /// The label margin.
        /// </value>
        public decimal? labelMargin { get; set; }

        /// <summary>
        /// Gets or sets the axis margin.
        /// </summary>
        /// <value>
        /// The axis margin.
        /// </value>
        public decimal? axisMargin { get; set; }

        /// <summary>
        /// Gets or sets the width of the border.
        /// </summary>
        /// <value>
        /// The width of the border.
        /// </value>
        public BorderOptions borderWidth { get; set; }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        /// <value>
        /// The color of the border.
        /// </value>
        public BorderColorOptions borderColor { get; set; }

        /// <summary>
        /// Gets or sets the minimum border margin.
        /// </summary>
        /// <value>
        /// The minimum border margin.
        /// </value>
        public decimal? minBorderMargin { get; set; }

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
        public decimal? mouseActiveRadius { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarginOptions
    {
        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>
        /// The top.
        /// </value>
        public int? top { get; set; }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>
        /// The left.
        /// </value>
        public int? left { get; set; }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>
        /// The bottom.
        /// </value>
        public int? bottom { get; set; }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>
        /// The right.
        /// </value>
        public int? right { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BorderOptions : MarginOptions
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class BorderColorOptions
    {
        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>
        /// The top.
        /// </value>
        public string top { get; set; }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>
        /// The left.
        /// </value>
        public string left { get; set; }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>
        /// The bottom.
        /// </value>
        public string bottom { get; set; }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>
        /// The right.
        /// </value>
        public string right { get; set; }
    }
}


