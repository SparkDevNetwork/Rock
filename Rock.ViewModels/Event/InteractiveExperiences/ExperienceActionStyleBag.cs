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
    /// The styles that should be applied to experience actions.
    /// </summary>
    public class ExperienceActionStyleBag
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
        /// Gets or sets the color of the primary button background.
        /// </summary>
        /// <value>The color of the primary button background.</value>
        public string PrimaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the primary button text.
        /// </summary>
        /// <value>The color of the primary button text.</value>
        public string PrimaryButtonTextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the secondary button background.
        /// </summary>
        /// <value>The color of the secondary button background.</value>
        public string SecondaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the secondary button text.
        /// </summary>
        /// <value>The color of the secondary button text.</value>
        public string SecondaryButtonTextColor { get; set; }

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
