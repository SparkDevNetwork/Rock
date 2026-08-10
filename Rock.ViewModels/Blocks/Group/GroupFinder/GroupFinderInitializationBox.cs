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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// The configuration sent to the Group Finder Obsidian block when it first loads.
    /// </summary>
    public class GroupFinderInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the card image area is shown (the "Show Image" setting).
        /// </summary>
        public bool IsImageShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the map pane is shown.
        /// </summary>
        public bool IsMapShown { get; set; }

        /// <summary>
        /// Gets or sets the color of the map approximation circles. One color drives every state via opacity (solid border + light fill when highlighted, lighter otherwise).
        /// </summary>
        public string MapCircleColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the "you are here" proximity marker (current location or entered address) on the map.
        /// </summary>
        public string CurrentLocationMarkerColor { get; set; }

        /// <summary>
        /// Gets or sets the guid of the Map Styles defined value applied to the results map, or empty to use the default style.
        /// </summary>
        public string MapStyleValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the best-guess latitude of the visitor, used to default the proximity Where filter when device geolocation is unavailable, or null when no guess could be made.
        /// </summary>
        /// <remarks>
        /// Provided only when proximity features are enabled. The client prefers the visitor's device
        /// location (with their permission) and falls back to this server-side guess (their profile
        /// address, then IP geolocation).
        /// </remarks>
        public double? VisitorLatitude { get; set; }

        /// <summary>
        /// Gets or sets the best-guess longitude of the visitor, used to default the proximity Where filter when device geolocation is unavailable, or null when no guess could be made.
        /// </summary>
        public double? VisitorLongitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether proximity features (address input, current location, distance) are enabled.
        /// </summary>
        public bool IsProximityEnabled { get; set; }

        /// <summary>
        /// Gets or sets an administrator-only warning shown when a required Google key is missing, or null when the block is configured or the visitor cannot administrate it.
        /// </summary>
        public string ConfigurationWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a typed address or ZIP can be searched, which requires a configured Google server geocoding key.
        /// </summary>
        /// <remarks>
        /// When false, the client hides the address input and offers only current-location search, which
        /// resolves from device coordinates and needs no geocoding.
        /// </remarks>
        public bool IsLocationSearchAvailable { get; set; }

        /// <summary>
        /// Gets or sets the number of group cards shown per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Campus filter section is shown.
        /// </summary>
        public bool IsCampusFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Where filter section is shown.
        /// </summary>
        public bool IsWhereFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the When filter section is shown.
        /// </summary>
        public bool IsWhenFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the What filter section is shown.
        /// </summary>
        public bool IsWhatFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Meeting Style filter is shown (Where section).
        /// </summary>
        public bool IsMeetingStyleFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Day of Week filter is shown (When section).
        /// </summary>
        public bool IsDayOfWeekFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Time of Day filter is shown (When section).
        /// </summary>
        public bool IsTimeOfDayFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the live text search is enabled (What section).
        /// </summary>
        public bool IsLiveSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the campuses offered by the Campus filter, each with its name and avatar badge.
        /// </summary>
        public List<GroupFinderCampusBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the meeting styles offered by the Where filter (value is the MeetingStyle enum value).
        /// </summary>
        public List<ListItemBag> MeetingStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the More Filters action is shown (true when any modal attribute filter is configured).
        /// </summary>
        public bool IsMoreFiltersShown { get; set; }

        /// <summary>
        /// Gets or sets the attribute filters promoted into the What section as pills.
        /// </summary>
        public List<GroupFinderAttributeFilterBag> FeaturedAttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the attribute filters rendered in the More Filters modal via the field-type filter control.
        /// </summary>
        public List<GroupFinderModalFilterBag> ModalAttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL of the Register Page for the card's Sign Up action, or null when not configured.
        /// </summary>
        public string RegisterPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the first, unfiltered page of results so the block renders groups immediately without an initial round trip.
        /// </summary>
        public GroupFinderResultsBag Results { get; set; }
    }
}
