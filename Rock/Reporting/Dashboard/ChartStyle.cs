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
using Newtonsoft.Json;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// Chart Style class.  All values are optional.  The chart will decide the style if a value is not specified
    /// Color values can be any html color specification
    /// Gradients can be 2 or more html colors that will result in a vertical gradient (limited to vertical because that's what works in IE)
    /// </summary>
    public class ChartStyle
    {
        /// <summary>
        /// Creates from json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static ChartStyle CreateFromJson(string json)
        {
            return JsonConvert.DeserializeObject( json, typeof( ChartStyle ) ) as ChartStyle;
        }

        /// <summary>
        /// Gets or sets the series colors. SeriesColors[0] will be used for the first series, SeriesColors[1] for the second, etc
        /// </summary>
        /// <value>
        /// The series colors.
        /// </value>
        public string[] SeriesColors { get; set; }

        /// <summary>
        /// Gets or sets the color of the goal series.
        /// </summary>
        /// <value>
        /// The color of the goal series.
        /// </value>
        public string GoalSeriesColor { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        /// <value>
        /// The grid.
        /// </value>
        public GridStyle Grid { get; set; }
        
        /// <summary>
        /// Gets or sets the x axis color and font 
        /// </summary>
        /// <value>
        /// The x axis.
        /// </value>
        public AxisStyle XAxis { get; set; }

        /// <summary>
        /// Gets or sets the y axis color and font
        /// </summary>
        /// <value>
        /// The y axis.
        /// </value>
        public AxisStyle YAxis { get; set; }

        /// <summary>
        /// Gets or sets the fill opacity.
        /// If FillColor is NULL, this will determine the FillOpacity of the a fillcolor determined by the chart
        /// </summary>
        /// <value>
        /// The fill opacity.
        /// </value>
        public double? FillOpacity { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// Leave null and set FillOpacity to have the chart automatically choose the color
        /// </summary>
        /// <value>
        /// The color of the fill.
        /// </value>
        public string FillColor { get; set; }

        /// <summary>
        /// Gets or sets the legend style
        /// </summary>
        /// <value>
        /// The legend.
        /// </value>
        public LegendStyle Legend { get; set; }

    }

    public class GridStyle
    {
        /// <summary>
        /// Gets or sets the grid color gradient.
        /// </summary>
        /// <value>
        /// The grid color gradient.
        /// </value>
        public string[] ColorGradient { get; set; }

        /// <summary>
        /// Gets or sets the color of the grid.
        /// </summary>
        /// <value>
        /// The color of the grid.
        /// </value>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the grid background color gradient.
        /// </summary>
        /// <value>
        /// The grid background color gradient.
        /// </value>
        public string[] BackgroundColorGradient { get; set; }

        /// <summary>
        /// Gets or sets the color of the grid background.
        /// </summary>
        /// <value>
        /// The color of the grid background.
        /// </value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the width of the border.
        /// Can be set to Null, a number, or a { top, right, bottom, left } object;
        /// </summary>
        /// <value>
        /// The width of the border.
        /// </value>
        public dynamic BorderWidth { get; set; }

        /// <summary>
        /// Gets or sets the color of the border.
        /// Can be set to Null, a htmlColor string, or a { top, right, bottom, left } object;
        /// </summary>
        /// <value>
        /// The color of the border.
        /// </value>
        public dynamic BorderColor { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class LegendStyle
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background opacity.
        /// </summary>
        /// <value>
        /// The background opacity.
        /// </value>
        public double? BackgroundOpacity { get; set; }

        /// <summary>
        /// Gets or sets the color of the label box border.
        /// </summary>
        /// <value>
        /// The color of the label box border.
        /// </value>
        public string LabelBoxBorderColor { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AxisStyle
    {
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        /// <value>
        /// The font.
        /// </value>
        public FontStyle Font { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FontStyle
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int? Size { get; set; }

        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string Family { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string Color { get; set; }
    }
}
