// <copyright>
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
using System;
using System.Collections.Generic;

namespace Rock.Chart
{
    /// <summary>
    /// Defines a set of constant values that can be used with the Chart.js charting component.
    /// </summary>
    public static class ChartJsConstants
    {
        /// <summary>
        /// Default color palette
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// Hex value for the color gray
            /// </summary>
            public static readonly string Gray = "#4D4D4D";
            /// <summary>
            /// Hex value for the color blue
            /// </summary>
            public static readonly string Blue = "#5DA5DA";
            /// <summary>
            /// Hex value for the color orange
            /// </summary>
            public static readonly string Orange = "#FAA43A";
            /// <summary>
            /// Hex value for the color green
            /// </summary>
            public static readonly string Green = "#60BD68";
            /// <summary>
            /// Hex value for the color pink
            /// </summary>
            public static readonly string Pink = "#F17CB0";
            /// <summary>
            /// Hex value for the color brown
            /// </summary>
            public static readonly string Brown = "#B2912F";
            /// <summary>
            /// Hex value for the color purple
            /// </summary>
            public static readonly string Purple = "#B276B2";
            /// <summary>
            /// Hex value for the color yellow
            /// </summary>
            public static readonly string Yellow = "#DECF3F";
            /// <summary>
            /// Hex value for the color red
            /// </summary>
            public static readonly string Red = "#F15854";

            /// <summary>
            /// Get the default color palette.
            /// </summary>
            /// <remarks>
            /// Color defaults are based on recommendations in this article: http://www.mulinblog.com/a-color-palette-optimized-for-data-visualization/
            /// </remarks>
            public static List<string> DefaultPalette = new List<string> { Blue, Green, Pink, Brown, Purple, Yellow, Red, Orange, Gray };
        }
    }
}