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
    /// The response object returned by the Join Experience real-time command.
    /// </summary>
    public class JoinExperienceResponseBag
    {
        /// <summary>
        /// Gets or sets the identifier of the experience occurrence that has
        /// been joined. This should be treated as a string of unknown data
        /// as the format might change in the future.
        /// </summary>
        /// <value>The identifier of the experience occurrence that has been joined.</value>
        public string OccurrenceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the currently displayed action identifier.
        /// </summary>
        /// <value>The currently displayed action identifier.</value>
        public string CurrentActionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the current action identifier key.
        /// </summary>
        /// <value>The current action identifier key or null.</value>
        public ActionRenderConfigurationBag CurrentActionConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the action identifier for the currently displayed visualizer.
        /// </summary>
        /// <value>The action identifier for the currently displayed visualizer.</value>
        public string CurrentVisualizerActionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the current visualizer identifier key.
        /// </summary>
        /// <value>The current visualizer identifier key or null.</value>
        public VisualizerRenderConfigurationBag CurrentVisualizerConfiguration { get; set; }
    }
}
