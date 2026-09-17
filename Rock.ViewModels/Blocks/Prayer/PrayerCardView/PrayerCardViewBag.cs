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

namespace Rock.ViewModels.Blocks.Prayer.PrayerCardView
{
    /// <summary>
    /// The bag that contains the data for the Prayer Card View block. It is
    /// returned on initial load and again whenever the campus filter changes.
    /// </summary>
    public class PrayerCardViewBag
    {
        /// <summary>
        /// Gets or sets the prayer requests to display as cards, already
        /// ordered and limited according to the block settings. An empty list
        /// means the block should show its empty state.
        /// </summary>
        public List<PrayerRequestCardBag> PrayerRequests { get; set; }

        /// <summary>
        /// Gets or sets the campus the person has saved as their filter
        /// selection, or <c>null</c> when no campus is selected.
        /// </summary>
        public ListItemBag SelectedCampus { get; set; }
    }
}
