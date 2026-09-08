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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.SelectArea
{
    /// <summary>
    /// Initialization payload for the Select Check-In Area block.
    /// </summary>
    public class SelectAreaOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of check-in areas to display. Each item's
        /// <see cref="ListItemBag.Value"/> is the area GroupType Guid string,
        /// and <see cref="ListItemBag.Text"/> is the area name.
        /// </summary>
        public List<ListItemBag> Areas { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the Check-in Manager Page block
        /// setting. Empty when the setting is not configured, which surfaces
        /// a warning in the UI instead of a silent no-op on click.
        /// </summary>
        public string ManagerPageUrl { get; set; }
    }
}
