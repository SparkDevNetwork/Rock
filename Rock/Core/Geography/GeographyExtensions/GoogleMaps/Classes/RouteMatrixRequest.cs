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
using Newtonsoft.Json;

using Rock.Core.Geography.Classes;

namespace Rock.Core.Geography.GeographyExtensions.GoogleMaps.Classes
{
    /// <summary>
    /// Represents a waypoint in the Google Maps Distance Matrix API request, which includes a geographical location.
    /// </summary>
    internal class Waypoint
    {
        /// <summary>
        /// The geographical location of the waypoint, represented by latitude and longitude coordinates.
        /// </summary>
        [JsonProperty( "location" )]
        public Location Location { get; set; }
    }

    /// <summary>
    /// Represents a geographical location with latitude and longitude coordinates, used in the Google Maps Distance Matrix API request.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The latitude and longitude coordinates of the geographical location.
        /// </summary>
        [JsonProperty( "latLng" )]
        public GeographyPoint Point { get; set; }
    }

    /// <summary>
    /// Represents an origin or destination in the Google Maps Distance Matrix API request.
    /// </summary>
    internal class OriginDestination
    {
        /// <summary>
        /// The name of the origin or destination, which can be a full address, place ID, or other identifier.
        /// </summary>
        [JsonProperty( "waypoint" )]
        public Waypoint Waypoint { get; set; }
    }

    /// <summary>
    /// Represents a request to the Google Maps Distance Matrix API for calculating travel distances and durations between multiple origins and destinations.
    /// </summary>
    internal class RouteMatrixRequest
    {
        /// <summary>
        /// A list of origins and destinations for the distance matrix request.
        /// </summary>
        [JsonProperty( "origins" )]
        public List<OriginDestination> Origins { get; set; }

        /// <summary>
        /// A list of origins and destinations for the distance matrix request.
        /// </summary>
        [JsonProperty( "destinations" )]
        public List<OriginDestination> Destinations { get; set; }

        /// <summary>
        /// The travel mode to use for the distance matrix request, such as driving, walking, bicycling, or transit.
        /// </summary>
        [JsonProperty( "travelMode" )]
        public string TravelMode { get; set; }
    }
}
