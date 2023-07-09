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

using Rock.Utility;

namespace Rock.Utilities
{
    /// <summary>
    /// Pair of colors representing the foreground and background colors.
    /// </summary>
    public class ColorPair
    {
        /// <summary>
        /// Foreground color
        /// </summary>
        public RockColor ForegroundColor { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public RockColor BackgroundColor { get; set; }

        /// <summary>
        /// Returns the contrast ratio between the foreground and the background colors.
        /// </summary>
        public double ContrastRatio
        {
            get
            {
                return RockColor.CalculateContrastRatio( ForegroundColor, BackgroundColor );
            }
        }

        /// <summary>
        /// Flips the foreground and background colors.
        /// </summary>
        public void Flip()
        {
            var tempColor = ForegroundColor;

            ForegroundColor = BackgroundColor;
            BackgroundColor = tempColor;
        }
    }
}
