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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// The visitor's filter selections posted to the Group Finder results action.
    /// </summary>
    public class GroupFinderQueryBag
    {
        /// <summary>
        /// Gets or sets the selected campus unique identifiers.
        /// </summary>
        public List<string> CampusGuids { get; set; }

        /// <summary>
        /// Gets or sets the selected meeting styles.
        /// </summary>
        public List<string> MeetingStyles { get; set; }

        /// <summary>
        /// Gets or sets the selected days of the week.
        /// </summary>
        public List<string> DaysOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the selected time of day (Morning, Afternoon, Evening).
        /// </summary>
        public string TimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the live text search term.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the origin (address, postal code, or "latitude,longitude") the results are searched around. Blank for the initial, unfiltered browse.
        /// </summary>
        /// <remarks>
        /// This is the search location - it selects which groups come back. It is independent of the
        /// proximity location, which is the person and drives distance. They coincide only when the
        /// visitor uses their current location.
        /// </remarks>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the person's latitude for distance calculation and sorting, when the visitor has shared their device location; null falls back to the server's best guess.
        /// </summary>
        /// <remarks>
        /// This is the proximity location - the person - and drives only distance and sort, never which
        /// groups are returned. Sent only after the visitor opts into current location; otherwise the
        /// server resolves the guess (profile address, then IP, then campus).
        /// </remarks>
        public double? ProximityLatitude { get; set; }

        /// <summary>
        /// Gets or sets the person's longitude for distance calculation and sorting, when the visitor has shared their device location; null falls back to the server's best guess.
        /// </summary>
        public double? ProximityLongitude { get; set; }

        /// <summary>
        /// Gets or sets the driving distances and times the client already holds for the current origin, keyed by group unique identifier, so the server looks up only the groups it does not yet have.
        /// </summary>
        /// <remarks>
        /// A per-session client cache echoed back on each request. The server honors these only when
        /// <see cref="KnownDistancesOriginKey"/> matches the origin it resolves for this request; a
        /// changed origin discards them. The newly looked-up pairs are returned in the results bag.
        /// </remarks>
        public Dictionary<string, GroupFinderDistanceBag> KnownDistances { get; set; }

        /// <summary>
        /// Gets or sets the origin key the <see cref="KnownDistances"/> were computed for, so the server can discard them when the resolved origin has changed.
        /// </summary>
        public string KnownDistancesOriginKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Origin"/> the client already resolved to <see cref="ResolvedOriginLatitude"/>, <see cref="ResolvedOriginLongitude"/>, and the resolved viewport, so the server reuses that result instead of geocoding the same origin again.
        /// </summary>
        /// <remarks>
        /// A per-session round trip echoed back on each request, matching the <see cref="KnownDistances"/>
        /// approach for drive times: the client holds the point and viewport it received for a typed origin
        /// and sends them back on the next page, filter, or sort of that same origin. The server trusts them
        /// only when this equals the <see cref="Origin"/> it is resolving; a changed origin discards them and
        /// geocodes afresh. Null on the first request for an origin and for coordinate origins (never geocoded).
        /// </remarks>
        public string ResolvedOriginKey { get; set; }

        /// <summary>
        /// Gets or sets the latitude the client already resolved <see cref="ResolvedOriginKey"/> to.
        /// </summary>
        public double? ResolvedOriginLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude the client already resolved <see cref="ResolvedOriginKey"/> to.
        /// </summary>
        public double? ResolvedOriginLongitude { get; set; }

        /// <summary>
        /// Gets or sets the north edge of the geocoded viewport the client already resolved <see cref="ResolvedOriginKey"/> to, sizing the search area exactly as the original geocode did.
        /// </summary>
        /// <remarks>
        /// All four edges are sent together or all null. Null means the resolved origin had no viewport (a
        /// coordinate or a precise address), which floors the search area to the default radius - the same
        /// outcome the geocode produced.
        /// </remarks>
        public double? ResolvedViewportNorth { get; set; }

        /// <summary>
        /// Gets or sets the south edge of the geocoded viewport the client already resolved <see cref="ResolvedOriginKey"/> to.
        /// </summary>
        public double? ResolvedViewportSouth { get; set; }

        /// <summary>
        /// Gets or sets the east edge of the geocoded viewport the client already resolved <see cref="ResolvedOriginKey"/> to.
        /// </summary>
        public double? ResolvedViewportEast { get; set; }

        /// <summary>
        /// Gets or sets the west edge of the geocoded viewport the client already resolved <see cref="ResolvedOriginKey"/> to.
        /// </summary>
        public double? ResolvedViewportWest { get; set; }

        /// <summary>
        /// Gets or sets the selected featured (pill) attribute values, keyed by attribute key.
        /// </summary>
        /// <remarks>
        /// Featured pills only. Each is a value list matched with the engine's "in" operator. More Filters
        /// selections travel in <see cref="AttributeFilterValues"/> instead.
        /// </remarks>
        public Dictionary<string, List<string>> AttributeSelections { get; set; }

        /// <summary>
        /// Gets or sets the raw public filter value for each More Filters attribute, keyed by attribute key.
        /// </summary>
        /// <remarks>
        /// The field type's own filter control produces this value, which can be field-type-specific (for
        /// example a JSON structure), so the server converts it to the private value through the field type
        /// rather than parsing it here. Paired with <see cref="AttributeComparisons"/> for the comparison.
        /// </remarks>
        public Dictionary<string, string> AttributeFilterValues { get; set; }

        /// <summary>
        /// Gets or sets the comparison chosen for each More Filters attribute, keyed by attribute key.
        /// </summary>
        /// <remarks>
        /// Paired with <see cref="AttributeFilterValues"/>; together they reconstruct the field type's
        /// comparison value on the server.
        /// </remarks>
        public Dictionary<string, ComparisonType> AttributeComparisons { get; set; }

        /// <summary>
        /// Gets or sets the north edge (maximum latitude) of an explicit search area supplied by the visitor's "Search this area" action.
        /// </summary>
        /// <remarks>
        /// Supplied only when the visitor searches the current map area; all four bounds are then sent
        /// and override the boundary the server would otherwise derive from the origin. Left null for a
        /// normal origin search, where the server sizes the boundary to the origin (its geocoded
        /// viewport, else a default radius).
        /// </remarks>
        public double? MapBoundsNorth { get; set; }

        /// <summary>
        /// Gets or sets the south edge (minimum latitude) of the map viewport to restrict results to.
        /// </summary>
        public double? MapBoundsSouth { get; set; }

        /// <summary>
        /// Gets or sets the east edge (maximum longitude) of the map viewport to restrict results to.
        /// </summary>
        public double? MapBoundsEast { get; set; }

        /// <summary>
        /// Gets or sets the west edge (minimum longitude) of the map viewport to restrict results to.
        /// </summary>
        public double? MapBoundsWest { get; set; }

        /// <summary>
        /// Gets or sets the zero-based page of results to return.
        /// </summary>
        /// <remarks>
        /// Each page holds up to the server's result cap, and page 0 (the default) is the first page.
        /// Used only when more groups match than the cap returns and the visitor pages through them -
        /// a list-only layout, or a map search that is already at its maximum zoom.
        /// </remarks>
        public int Page { get; set; }
    }
}
