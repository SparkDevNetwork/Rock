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

using System.Collections.Generic;

using Rock.Attribute;
using Rock.Core.Geography.Classes;
using Rock.Enums.Geography;

namespace Rock.Utility.GroupFinder
{
    /// <summary>
    /// The options that can be passed to the <see cref="GroupFinderHelper"/>.
    /// </summary>
    [RockInternal( "20.0" )]
    public class GroupFinderOptions
    {
        /// <summary>
        /// The group type id to use for the search.
        /// </summary>
        public List<int> GroupTypeIds { get; set; }

        /// <summary>
        /// List of properties to eager load on the initial query.
        /// </summary>
        public string Include { get; set; }

        /// <summary>
        /// The maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// The maximum distance to search for groups in meters.
        /// </summary>
        public int? MaxDistance { get; set; }

        /// <summary>
        /// If true, only the closest location for each group will be returned.
        /// Otherwise, all locations for a group will be considered.
        /// </summary>
        public bool ReturnOnlyClosestLocationPerGroup { get; set; }

        /// <summary>
        /// The origin to use for the search.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// The origin point to use for the search.
        /// </summary>
        public GeographyPoint OriginPoint { get; set; }

        /// <summary>
        /// The travel mode to use for calculating travel mode distances and
        /// times.
        /// </summary>
        public TravelMode? TravelMode { get; set; }

        /// <summary>
        /// Determines whether to hide groups that are over their capacity.
        /// </summary>
        public bool HideOvercapacityGroups { get; set; }

        /// <summary>
        /// Determines if null campus values should be returned when filtering
        /// on campuses.
        /// </summary>
        public bool EnableStrictCampusFiltering { get; set; }

        /// <summary>
        /// Determines if filtering for public groups should be enabled.
        /// </summary>
        public bool EnablePublicFilter { get; set; }
    }
}
