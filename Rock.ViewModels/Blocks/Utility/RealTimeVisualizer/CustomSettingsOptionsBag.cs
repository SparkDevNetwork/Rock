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

namespace Rock.ViewModels.Blocks.Utility.RealTimeVisualizer
{
    /// <summary>
    /// The additional information required to build the custom settings UI
    /// for the Real Time Visualizer block.
    /// </summary>
    public class CustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the themes that can be picked by the individual.
        /// </summary>
        /// <value>The themes that can be picked by the individual.</value>
        public List<ThemeListItemBag> Themes { get; set; }

        /// <summary>
        /// Gets or sets the topics that can be picked by the individual.
        /// </summary>
        /// <value>The topics that can be picked by the individual.</value>
        public List<string> Topics { get; set; }
    }
}
