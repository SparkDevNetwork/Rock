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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// A page of Group Finder results returned to the block.
    /// </summary>
    public class GroupFinderResultsBag
    {
        /// <summary>
        /// Gets or sets the total number of groups matching the current filters, across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of groups a single page holds, so the client can size the pager from the total count.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the group cards for the requested page.
        /// </summary>
        public List<GroupFinderCardBag> Cards { get; set; }

        /// <summary>
        /// Gets or sets the map markers for the requested page, aligned to the cards by group. The coordinates are fuzzed for privacy.
        /// </summary>
        public List<GroupFinderMapMarkerBag> Markers { get; set; }

        /// <summary>
        /// Gets or sets the driving distances and times newly looked up for this page, keyed by group unique identifier, for the client to merge into its per-session cache.
        /// </summary>
        public Dictionary<string, GroupFinderDistanceBag> NewDistances { get; set; }

        /// <summary>
        /// Gets or sets the key identifying the origin the distances were computed for, or null when no origin was in use. The client caches distances under this key and echoes it back so the server can tell when the origin has changed.
        /// </summary>
        public string OriginKey { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the point distances were measured from, when it is a precise origin (a search location or the visitor's shared device location) worth marking on the map; null for a coarse server guess or no origin.
        /// </summary>
        public double? OriginLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the point distances were measured from, when it is a precise origin worth marking on the map; null for a coarse server guess or no origin.
        /// </summary>
        public double? OriginLongitude { get; set; }

        /// <summary>
        /// Gets or sets the north edge of the area the results were searched within, so the client can frame the map to it, or null when no origin was in use.
        /// </summary>
        /// <remarks>
        /// This is the boundary the server actually applied: the origin's geocoded viewport, a default
        /// radius around it, or the explicit area the visitor searched. All four edges are returned
        /// together or all null.
        /// </remarks>
        public double? SearchBoundsNorth { get; set; }

        /// <summary>
        /// Gets or sets the south edge of the area the results were searched within.
        /// </summary>
        public double? SearchBoundsSouth { get; set; }

        /// <summary>
        /// Gets or sets the east edge of the area the results were searched within.
        /// </summary>
        public double? SearchBoundsEast { get; set; }

        /// <summary>
        /// Gets or sets the west edge of the area the results were searched within.
        /// </summary>
        public double? SearchBoundsWest { get; set; }

        /// <summary>
        /// Gets or sets the north edge of the origin's raw geocoded viewport, for the client to hold and echo back so the same origin is not geocoded again while paging or refiltering.
        /// </summary>
        /// <remarks>
        /// This is the geocoder's own viewport for the origin, before it is unioned with the default radius
        /// into <see cref="SearchBoundsNorth"/>. Returned only for a geocoded typed origin, and all four
        /// edges are returned together or all null (the origin resolved to a point with no viewport). The
        /// client pairs it with <see cref="OriginLatitude"/> and <see cref="OriginLongitude"/> as the
        /// resolved origin it sends back in the query.
        /// </remarks>
        public double? ResolvedViewportNorth { get; set; }

        /// <summary>
        /// Gets or sets the south edge of the origin's raw geocoded viewport.
        /// </summary>
        public double? ResolvedViewportSouth { get; set; }

        /// <summary>
        /// Gets or sets the east edge of the origin's raw geocoded viewport.
        /// </summary>
        public double? ResolvedViewportEast { get; set; }

        /// <summary>
        /// Gets or sets the west edge of the origin's raw geocoded viewport.
        /// </summary>
        public double? ResolvedViewportWest { get; set; }
    }
}
