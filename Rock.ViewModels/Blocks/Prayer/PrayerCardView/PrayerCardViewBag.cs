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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Prayer.PrayerCardView
{
    /// <summary>
    /// The bag that contains the data for the Prayer Card View block. It is
    /// returned on initial load and again whenever the rendered cards change.
    /// </summary>
    public class PrayerCardViewBag
    {
        /// <summary>
        /// Gets or sets the rendered Lava HTML content that displays the
        /// prayer request cards.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the campus the person has saved as their filter
        /// selection, or <c>null</c> when no campus is selected.
        /// </summary>
        public ListItemBag SelectedCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any prayer requests matched
        /// the current filters. When <c>false</c> the block shows an empty
        /// state instead of the rendered content.
        /// </summary>
        public bool HasPrayerRequests { get; set; }
    }
}
