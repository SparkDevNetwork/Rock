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

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// The configuration required to display a visualizer.
    /// </summary>
    public class VisualizerRenderConfigurationBag
    {
        /// <summary>
        /// Gets or sets the title of the action this visualizer is
        /// associated with.
        /// </summary>
        /// <value>
        /// The title of the action.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the action identifier.
        /// </summary>
        /// <remarks>
        /// This should only be used when setting the CSS class name.
        /// </remarks>
        /// <value>The action identifier.</value>
        public int ActionId { get; set; }

        /// <summary>
        /// Gets or sets the visualizer type unique identifier.
        /// </summary>
        /// <value>The visualizer type unique identifier.</value>
        public Guid VisualizerTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the configuration values for the visualizer type.
        /// </summary>
        /// <value>The configuration values for the visualizer type.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
