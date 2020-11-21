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

namespace Rock.DownhillCss
{
    /// <summary>
    /// Static class to hold all of the colors in the palette
    /// </summary>
    public static class ColorPalette
    {
        /// <summary>
        /// Gets or sets the color maps.
        /// </summary>
        /// <value>
        /// The color maps.
        /// </value>
        public static List<ColorMap> ColorMaps { get; set; } = new List<ColorMap>();

        /// <summary>
        /// Initializes the <see cref="ColorPalette"/> class by loading the default color maps.
        /// </summary>
        static ColorPalette()
        {
            // Colors come from Tailwind.css 
            // https://tailwindcss.com/docs/customizing-colors

            // Gray
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Gray",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#F8F9FA"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#E9ECEF"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#DEE2E6"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#CED4DA"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#ADB5BD"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#6C757D"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#495057"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#343A40"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#212529"}
                    }

                });

            // Red
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Red",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#FFF5F5"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#FED7D7"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#FEB2B2"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#FC8181"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#F56565"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#E53E3E"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#C53030"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#9B2C2C"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#742A2A"}
                    }

                } );

            // Orange
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Orange",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#FFFAF0"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#FEEBC8"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#FBD38D"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#F6AD55"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#ED8936"},
                        new ColorSaturation(){ Intensity = 550, ColorValue = "#EE7625"}, // Add extra option to hit Rock brand color. Our color was smack in the middle of 500 and 600
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#DD6B20"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#C05621"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#9C4221"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#7B341E"}
                    }

                } );

            // Yellow
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Yellow",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#FFFFF0"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#FEFCBF"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#FAF089"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#F6E05E"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#ECC94B"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#D69E2E"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#B7791F"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#975A16"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#744210"}
                    }

                } );

            // Green
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Green",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#F0FFF4"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#C6F6D5"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#9AE6B4"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#68D391"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#48BB78"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#38A169"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#2F855A"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#276749"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#22543D"}
                    }

                } );

            // Teal
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Teal",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#E6FFFA"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#B2F5EA"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#81E6D9"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#4FD1C5"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#38B2AC"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#319795"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#2C7A7B"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#285E61"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#234E52"}
                    }

                } );

            // Blue
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Blue",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#EBF8FF"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#BEE3F8"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#90CDF4"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#63B3ED"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#4299E1"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#3182CE"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#2B6CB0"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#2C5282"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#2A4365"}
                    }

                } );

            // Indigo
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Indigo",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#EBF4FF"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#C3DAFE"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#A3BFFA"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#7F9CF5"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#667EEA"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#5A67D8"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#4C51BF"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#434190"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#3C366B"}
                    }

                } );

            // Purple
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Purple",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#FAF5FF"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#E9D8FD"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#D6BCFA"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#B794F4"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#9F7AEA"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#805AD5"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#6B46C1"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#553C9A"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#44337A"}
                    }

                } );

            // Pink
            ColorMaps.Add(
                new ColorMap
                {
                    Color = "Pink",
                    ColorSaturations = new List<ColorSaturation>()
                    {
                        new ColorSaturation(){ Intensity = 100, ColorValue = "#FFF5F7"},
                        new ColorSaturation(){ Intensity = 200, ColorValue = "#FED7E2"},
                        new ColorSaturation(){ Intensity = 300, ColorValue = "#FBB6CE"},
                        new ColorSaturation(){ Intensity = 400, ColorValue = "#F687B3"},
                        new ColorSaturation(){ Intensity = 500, ColorValue = "#ED64A6"},
                        new ColorSaturation(){ Intensity = 600, ColorValue = "#D53F8C"},
                        new ColorSaturation(){ Intensity = 700, ColorValue = "#B83280"},
                        new ColorSaturation(){ Intensity = 800, ColorValue = "#97266D"},
                        new ColorSaturation(){ Intensity = 900, ColorValue = "#702459"}
                    }

                } );
        }
    }

    /// <summary>
    /// Specific color on the pallet with various saturation levels
    /// </summary>
    public class ColorMap
    {
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the color saturations.
        /// </summary>
        /// <value>
        /// The color saturations.
        /// </value>
        public List<ColorSaturation> ColorSaturations { get; set; } = new List<ColorSaturation>();
    }

    /// <summary>
    /// Spefic color saturation
    /// </summary>
    public class ColorSaturation
    {
        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>
        /// The intensity.
        /// </value>
        public int Intensity { get; set; } = 0;

        /// <summary>
        /// Gets or sets the color value.
        /// </summary>
        /// <value>
        /// The color value.
        /// </value>
        public string ColorValue { get; set; } = string.Empty;
    }
}
