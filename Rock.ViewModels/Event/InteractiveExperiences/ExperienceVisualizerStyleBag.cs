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

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// The styles that should be applied to the visualizer.
    /// </summary>
    public class ExperienceVisualizerStyleBag
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        public string TextColor { get; set; }

        /// <summary>
        /// Gets or sets the primary color.
        /// </summary>
        /// <value>The primary color.</value>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary color.
        /// </summary>
        /// <value>The secondary color.</value>
        public string SecondaryColor { get; set; }

        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        /// <value>The accent color.</value>
        public string AccentColor { get; set; }

        /// <summary>
        /// Gets or sets the background image URL.
        /// </summary>
        /// <value>The background image URL.</value>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets the custom CSS.
        /// </summary>
        /// <value>The custom CSS.</value>
        public string CustomCss { get; set; }
    }
}
