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

namespace Rock.ViewModels.Blocks.Utility.RealTimeVisualizer
{
    /// <summary>
    /// Identifies a single theme item for the visualizer.
    /// </summary>
    public class ThemeListItemBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the HTML that describes how this theme works.
        /// </summary>
        /// <value>The HTML that describes how this theme works.</value>
        public string HelpContent { get; set; }

        /// <summary>
        /// Gets or sets the setting keys for this theme.
        /// </summary>
        /// <value>The setting keys for this theme.</value>
        public List<string> Settings { get; set; }
    }
}
