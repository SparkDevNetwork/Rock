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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// One Meeting Location card on the right rail of the Group Detail view panel. The Vue
    /// layer renders Address / Point / Polygon / GroupMember variants from <see cref="Mode"/>.
    /// </summary>
    public class GroupMeetingLocationBag
    {
        /// <summary>
        /// Gets or sets the <c>GroupLocation.Guid</c> for the row backing this card.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the multi-line formatted address text for the card. <c>null</c> for
        /// polygon-style locations and when the <c>ShowLocationAddresses</c> block attribute
        /// is false.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the friendly schedule text from the first associated schedule, or
        /// <c>null</c> when no schedule is attached.
        /// </summary>
        public string ScheduleText { get; set; }

        /// <summary>
        /// Gets or sets the location-picker mode classification used to drive the per-card
        /// render variant.
        /// </summary>
        public GroupLocationPickerMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the raw Well-Known Text (WKT) geometry consumed by the Vue map
        /// renderer (e.g., <c>POINT(-112.130946 33.600114)</c> or
        /// <c>POLYGON((-112.157058 33.598563, ...))</c>). Empty when the location has no
        /// geo data.
        /// </summary>
        public string MapData { get; set; }

        /// <summary>
        /// Gets or sets the URL the hover-expand button navigates to.
        /// </summary>
        public string MapUrl { get; set; }
    }
}
