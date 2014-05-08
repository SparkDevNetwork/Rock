using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rock.Reporting.Dashboard
{
    public class ChartTheme
    {
        public ChartTheme()
        {
            this.SeriesColors = new string[] { 
                "#8498ab",
                "#a4b4c4",
                "#b9c7d5",
                "#c6d2df",
                "#d8e1ea"
            };
        }

        public string[] SeriesColors { get; set; }

        public string GoalSeriesColor { get; set; }

        public string[] GridColorGradiant { get; set; }

        public string GridColor { get; set; }

        public string[] GridBackgroundColorGradiant { get; set; }

        public string GridBackgroundColor { get; set; }

        public AxisStyle XAxis { get; set; }
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
    }

    public class AxisStyle
    {
        public string Color { get; set; }
        public ThemeFont Font { get; set; }
    }

    public class ThemeFont
    {
        public ThemeFont( string color )
            : this( color, null, null )
        {
        }

        public ThemeFont( string color, string family, int? size )
        {
            this.Size = size;
            this.Family = family;
            this.Color = color;
        }

        public int? Size { get; set; }
        public string Family { get; set; }
        public string Color { get; set; }
    }

}
