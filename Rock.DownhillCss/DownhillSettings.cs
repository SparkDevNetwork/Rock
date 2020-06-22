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
using System.Text;

namespace Rock.DownhillCss
{
    public class DownhillSettings
    {

        /// <summary>
        /// Gets the spacing units based on the platform.
        /// </summary>
        /// <value>
        /// The spacing units.
        /// </value>
        public Dictionary<int,string> SpacingValues
        {
            get
            {
                if (this.Platform == DownhillPlatform.Mobile )
                {
                    return new Dictionary<int, string>()
                    {
                        { 0, "0" },
                        { 1, "1" },
                        { 2, "2" },
                        { 4, "4" },
                        { 8, "8" },
                        { 12, "12" },
                        { 16, "16" },
                        { 24, "24" },
                        { 32, "32" },
                        { 64, "64" },
                    };
                }
                else
                {
                    return new Dictionary<int, string>()
                    {
                        { 0, "0" },
                        { 1, ".25rem" },
                        { 2, ".5rem" },
                        { 4, ".75rem" },
                        { 8, "2rem" },
                        { 12, "3rem" },
                        { 16, "4rem" },
                        { 24, "6rem" },
                        { 32, "8rem" },
                        { 64, "16rem" },
                    };
                }
            }
        }

        /// <summary>
        /// Gets the spacing units based on the platform.
        /// </summary>
        /// <value>
        /// The spacing units.
        /// </value>
        public string SpacingUnits
        {
            get
            {
                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    return string.Empty;
                }
                else
                {
                    return "rem";
                }
            }
        }

        /// <summary>
        /// Gets the font size list.
        /// </summary>
        /// <value>
        /// The font sizes.
        /// </value>
        public Dictionary<string, decimal> FontSizes
        {
            get
            {
                return new Dictionary<string, decimal>()
                {
                    { "xs", .75m },
                    { "sm", .875m },
                    { "base", 1 },
                    { "lg", 1.125m },
                    { "xl", 1.25m },
                    { "2xl", 1.5m },
                    { "3xl", 1.875m },
                    { "4xl", 2.25m },
                    { "5xl", 3m },
                    { "6xl", 4m },
                };
            }
        }

        /// <summary>
        /// Gets the border widths.
        /// </summary>
        /// <value>
        /// The border widths.
        /// </value>
        public List<int> BorderWidths
        {
            get
            {
                return new List<int> { 0, 1, 2, 4, 8 };
            }
        }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        public DownhillPlatform Platform { get; set; } = DownhillPlatform.Web;

        /// <summary>
        /// Gets the border units.
        /// </summary>
        /// <value>
        /// The border units.
        /// </value>
        public string BorderUnits
        {
            get
            {
                if ( _borderUnits != null )
                {
                    return _borderUnits;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _borderUnits = string.Empty;
                }
                else
                {
                    _borderUnits = "px";
                }

                return _borderUnits;
            }
        }
        private string _borderUnits;

        /// <summary>
        /// Gets the border units.
        /// </summary>
        /// <value>
        /// The border units.
        /// </value>
        public string FontUnits
        {
            get
            {
                if ( _fontUnits != null )
                {
                    return _fontUnits;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _fontUnits = string.Empty;
                }
                else
                {
                    _fontUnits = "rem";
                }

                return _fontUnits;
            }
        }
        private string _fontUnits;

        /// <summary>
        /// Gets or sets the default size of the font.
        /// </summary>
        /// <value>
        /// The default size of the font.
        /// </value>
        public decimal FontSizeDefault
        {
            get
            {
                if ( _fontSizeDefault != 0 )
                {
                    return _fontSizeDefault;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _fontSizeDefault = 16;
                }
                else
                {
                    _fontSizeDefault = 1; // rem
                }

                return _fontSizeDefault;
            }
            set
            {
                _fontSizeDefault = value;
            }
        }
        private decimal _fontSizeDefault;

        /// <summary>
        /// Gets or sets the application colors.
        /// </summary>
        /// <value>
        /// The application colors.
        /// </value>
        public ApplicationColors ApplicationColors { get; set; } = new ApplicationColors();

        /// <summary>
        /// Gets or sets the base radius.
        /// </summary>
        /// <value>
        /// The base radius.
        /// </value>
        public decimal RadiusBase { get; set; } = 0;

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>
        /// The color of the text.
        /// </value>
        public string TextColor { get; set; } = "#676767";

        /// <summary>
        /// Gets or sets the color of the heading.
        /// </summary>
        /// <value>
        /// The color of the heading.
        /// </value>
        public string HeadingColor { get; set; } = "#333333";

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string BackgroundColor { get; set; } = "#ffffff";
    }

    public enum DownhillPlatform
    {
        Mobile = 0,
        Web = 1
    }
}
