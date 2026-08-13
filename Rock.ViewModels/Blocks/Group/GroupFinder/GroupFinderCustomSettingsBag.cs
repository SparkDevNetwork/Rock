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

using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// The settings edited in the custom settings panel for the Group Finder block.
    /// </summary>
    public class GroupFinderCustomSettingsBag
    {
        #region Group Types and Attributes

        /// <summary>
        /// Gets or sets the group types whose groups the finder searches, and whose attributes the filter and card settings are drawn from.
        /// </summary>
        public List<ListItemBag> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the attributes surfaced as "What" filter pills.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="DisplayAttributeFilters"/>, and limited to Single-select,
        /// Multi-select, and Boolean field types.
        /// </remarks>
        public List<Guid> FeaturedAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attributes surfaced in the "More Filters" modal.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="FeaturedAttributes"/>.
        /// </remarks>
        public List<Guid> DisplayAttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the attributes displayed on each group card.
        /// </summary>
        public List<Guid> ShowAttributeOnCard { get; set; }

        #endregion Group Types and Attributes

        #region Filters

        /// <summary>
        /// Gets or sets whether the Campus filter is hidden.
        /// </summary>
        public bool IsCampusFilterHidden { get; set; }

        /// <summary>
        /// Gets or sets whether the Where (location) filter is hidden.
        /// </summary>
        public bool IsWhereFilterHidden { get; set; }

        /// <summary>
        /// Gets or sets whether the When (schedule) filter is hidden.
        /// </summary>
        public bool IsWhenFilterHidden { get; set; }

        /// <summary>
        /// Gets or sets whether the What (attributes) filter is hidden.
        /// </summary>
        public bool IsWhatFilterHidden { get; set; }

        /// <summary>
        /// Gets or sets the campus types offered by the Campus filter.
        /// </summary>
        public List<ListItemBag> CampusTypes { get; set; }

        /// <summary>
        /// Gets or sets the campus statuses offered by the Campus filter.
        /// </summary>
        public List<ListItemBag> CampusStatuses { get; set; }

        /// <summary>
        /// Gets or sets whether proximity features (an address input and a Use Current Location action) are enabled.
        /// </summary>
        public bool IsProximityEnabled { get; set; }

        /// <summary>
        /// Gets or sets the meeting styles offered by the Where filter (InPerson, Online, Hybrid). When none are selected the Meeting Style filter is hidden.
        /// </summary>
        public List<string> SupportedMeetingStyles { get; set; }

        /// <summary>
        /// Gets or sets whether the Day of Week filter is displayed.
        /// </summary>
        public bool IsDayOfWeekFilterShown { get; set; }

        /// <summary>
        /// Gets or sets whether the Time of Day filter is displayed.
        /// </summary>
        public bool IsTimeOfDayFilterShown { get; set; }

        /// <summary>
        /// Gets or sets whether live text search (filtering by group name as the visitor types) is enabled.
        /// </summary>
        public bool IsLiveSearchEnabled { get; set; }

        #endregion Filters

        #region Card and Map

        /// <summary>
        /// Gets or sets whether the group image is shown on each card.
        /// </summary>
        public bool IsImageShown { get; set; }

        /// <summary>
        /// Gets or sets whether the average member age is shown on each card.
        /// </summary>
        public bool IsAverageAgeShown { get; set; }

        /// <summary>
        /// Gets or sets whether the results map is shown alongside the cards.
        /// </summary>
        public bool IsMapShown { get; set; }

        /// <summary>
        /// Gets or sets the color of the group markers on the map.
        /// </summary>
        public string GroupMarkerColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the "you are here" proximity marker on the map.
        /// </summary>
        public string CurrentLocationMarkerColor { get; set; }

        /// <summary>
        /// Gets or sets the map style applied to the results map.
        /// </summary>
        public ListItemBag MapStyle { get; set; }

        /// <summary>
        /// Gets or sets the Lava template that renders the content of each result card. Blank falls back to the built-in default.
        /// </summary>
        public string GroupCardTemplate { get; set; }

        #endregion Card and Map

        #region Linked Pages

        /// <summary>
        /// Gets or sets the page a visitor is sent to when signing up for a group.
        /// </summary>
        public PageRouteValueBag RegisterPage { get; set; }

        #endregion Linked Pages
    }
}
