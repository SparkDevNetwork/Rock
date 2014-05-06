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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rock.Utility;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// TODO: Convert to FLOT chart options
    /// </summary>
    public class ChartOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartOptions"/> class.
        /// </summary>
        public ChartOptions()
        {
            titleTextStyle = new System.Dynamic.ExpandoObject();
            vAxis = new System.Dynamic.ExpandoObject();
            hAxis = new System.Dynamic.ExpandoObject();
            legend = new System.Dynamic.ExpandoObject();
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the type of the curve for Line Charts
        /// </summary>
        /// <value>
        /// The type of the curve.
        /// </value>
        [JsonConverter( typeof( EnumAsStringJsonConverter ) )]
        public LineChartCurveType curveType {get; set;}

        /// <summary>
        /// Gets or sets the title text style.
        /// </summary>
        /// <value>
        /// The title text style.
        /// </value>
        public dynamic titleTextStyle { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string backgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the colors.
        /// </summary>
        /// <value>
        /// The colors.
        /// </value>
        public string[] colors { get; set; }

        /// <summary>
        /// Gets or sets the v axis.
        /// </summary>
        /// <value>
        /// The v axis.
        /// </value>
        public dynamic vAxis { get; set; }

        /// <summary>
        /// Gets or sets the h axis.
        /// </summary>
        /// <value>
        /// The h axis.
        /// </value>
        public dynamic hAxis { get; set; }

        /// <summary>
        /// Gets or sets the legend.
        /// </summary>
        /// <value>
        /// The legend.
        /// </value>
        public dynamic legend { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int? width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int? height { get; set; }

        public static ChartOptions Default
        {
            get
            {
                var chartOptions = new ChartOptions();
                chartOptions.vAxis.title = "Chart";

                chartOptions.vAxis.minValue = 0;
                chartOptions.vAxis.titleTextStyle = new
                {
                    color = "#515151",
                    italic = false
                };

                chartOptions.colors = new string[] { "#8498ab", "#a4b4c4", "#b9c7d5", "#c6d2df", "#d8e1ea" };
                chartOptions.hAxis = new
                {
                    textStyle = new
                    {
                        color = "#515151"
                    },
                    baselineColor = "#515151"
                };

                chartOptions.width = null;
                chartOptions.height = null;
                chartOptions.legend = new
                {
                    position = "bottom",
                    textStyle = new
                    {
                        color = "#515151"
                    }
                };

                chartOptions.backgroundColor = "transparent";

                return chartOptions;
            }
        }
    }

    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// </summary>
    public class ChartTooltip : ColumnDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartTooltip"/> class.
        /// </summary>
        public ChartTooltip()
            : base( "Tooltip", ColumnDataType.@string )
        {
            role = "tooltip";
        }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        public string role { get; set; }
    }

    /// <summary>
    /// Class that can be JSON'd and used for Google Charts (properties are case sensitive)
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinition"/> class.
        /// </summary>
        /// <param name="Label">The label.</param>
        /// <param name="type">The type.</param>
        public ColumnDefinition( string label, ColumnDataType type )
        {
            this.label = label;
            this.type = type.ConvertToString();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string label { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ColumnDataType
    {
        /// <summary>
        /// The date
        /// </summary>
        date,

        /// <summary>
        /// The number
        /// </summary>
        number,

        /// <summary>
        /// The string
        /// </summary>
        @string
    }

    /// <summary>
    /// Controls the curve of the lines when the line width is not zero
    /// </summary>
    public enum LineChartCurveType
    {
        // Straight lines without curve
        none,

        // The angles of the line will be smoothed
        function
    }

    
}
