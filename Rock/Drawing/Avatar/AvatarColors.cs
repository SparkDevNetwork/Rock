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
using Rock.Utility;

namespace Rock.Drawing.Avatar
{
    /// <summary>
    /// Color settings and utilities for creating avatars
    /// </summary>
    public class AvatarColors
    {
        /// <summary>
        /// Reusable random generator (ensures colors are truely random). 
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Gets or sets the foreground color for the avatar.
        /// </summary>
        /// <value>The color of the foreground.</value>
        public string ForegroundColor {
            get
            {
                if ( _foregroundColor == null )
                {
                    return string.Empty;
                }
                return _foregroundColor;
            }
            set
            {
                _foregroundColor = value;
            }
        }
        private string _foregroundColor = null;

        /// <summary>
        /// Gets or sets the background color for the avatar.
        /// </summary>
        /// <value>The color of the background.</value>
        public string BackgroundColor
        {
            get
            {
                if ( _backgroundColor == null )
                {
                    return string.Empty;
                }
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
            }
        }
        private string _backgroundColor = null;

        /// <summary>
        /// Generates any missing colors.
        /// </summary>
        public void GenerateMissingColors()
        {
            // If we were provided both a foreground color and background color then we don't need
            // to do anything
            if ( this.ForegroundColor.IsNotNullOrWhiteSpace() && this.BackgroundColor.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            // If no color was provided find a random color
            if ( this.ForegroundColor.IsNullOrWhiteSpace() && this.BackgroundColor.IsNullOrWhiteSpace() )
            {
                var randomIndex = _random.Next( 0, colorValues.Length );
                var randomColor = colorValues[randomIndex];

                // Get color pairing for the random color
                var colorParing = RockColor.CalculateColorPair( new RockColor( randomColor ) );
                this.ForegroundColor = colorParing.ForegroundColor.ToHex();
                this.BackgroundColor = colorParing.BackgroundColor.ToHex();
                return;
            }

            // At this point we only have one color so generate the missing one
            if ( this.BackgroundColor.IsNullOrWhiteSpace() )
            {
                this.BackgroundColor = GetContrastColor( this.ForegroundColor );
            }

            if ( this.ForegroundColor.IsNullOrWhiteSpace() )
            {
                this.ForegroundColor = GetContrastColor( this.BackgroundColor );
            }
        }

        /// <summary>
        /// Get's the contrast color from the provided color.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string GetContrastColor( string source )
        {
            var sourceColor = RockColor.FromHex( source );

            // Create dark contrast using the Practical UI darkest recipe
            var darkColor = RockColor.FromHex( source );
            darkColor.Saturation = .60;
            darkColor.Luminosity = .20;

            // Create light contrast using the Practical UI ligest recipe
            var lightColor = RockColor.FromHex( source );
            lightColor.Saturation = .88;
            lightColor.Luminosity = .87;

            // Return the color with the greatest contrast
            var darkContrast = RockColor.CalculateContrastRatio( sourceColor, darkColor );
            var lightContrast = RockColor.CalculateContrastRatio( sourceColor, lightColor );

            if ( darkContrast > lightContrast )
            {
                return darkColor.ToHex();
            }

            return lightColor.ToHex();
        }

        // List of random colors to use when no colors were provided
        // From: https://github.com/LasseRafn/ui-avatars/blob/master/Utils/Input.php
        private string[] colorValues = new string[] {
            "#ef4444",
            "#f97316",
            "#71717a",
            "#f59e0b",
            "#eab308",
            "#84cc16",
            "#22c55e",
            "#10b981",
            "#14b8a6",
            "#06b6d4",
            "#0ea5e9",
            "#3b82f6",
            "#6366f1",
            "#8b5cf6",
            "#a855f7",
            "#d946ef",
            "#ec4899",
            "#f43f5e",
            "#22c55e"
        };
    }
}
