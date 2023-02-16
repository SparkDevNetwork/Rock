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
    /// The visual style information for an experience.
    /// </summary>
    public class ExperienceStyleBag
    {
        /// <summary>
        /// Gets or sets the placeholder content and style when an action has not
        /// yet been displayed.
        /// </summary>
        /// <value>The initial placeholder content and style.</value>
        public ExperiencePlaceholderStyleBag Welcome { get; set; }

        /// <summary>
        /// Gets or sets the placeholder content and style when no action is
        /// currently being displayed.
        /// </summary>
        /// <value>The placeholder content and style when no other content is available.</value>
        public ExperiencePlaceholderStyleBag NoAction { get; set; }

        /// <summary>
        /// Gets or sets the action styles.
        /// </summary>
        /// <value>The action styles.</value>
        public ExperienceActionStyleBag Action { get; set; }

        /// <summary>
        /// Gets or sets the visualizer styles.
        /// </summary>
        /// <value>The visualizer styles.</value>
        public ExperienceVisualizerStyleBag Visualizer { get; set; }
    }
}
