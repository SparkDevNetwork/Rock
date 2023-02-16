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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
{
    /// <summary>
    /// Identifies a single visualizer type that can be used to configure
    /// visualizers for an action by the individual.
    /// </summary>
    public class InteractiveExperienceVisualizerTypeBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the attributes that are available on this visualizer type.
        /// </summary>
        /// <value>
        /// The attributes that are available on this visualizer type.
        /// </value>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the default attribute values.
        /// </summary>
        /// <value>The default attribute values.</value>
        public Dictionary<string, string> DefaultAttributeValues { get; set; }
    }
}
