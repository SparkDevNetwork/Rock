using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rock.Address.LocationExtensions.GoogleMaps.Classes
{
    /// <summary>
    /// Represents a waypoint in the Google Maps Distance Matrix API request, which includes a geographical location.
    /// </summary>
    public class Waypoint
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
        public Point Point { get; set; }
    }

    /// <summary>
    /// Represents an origin or destination in the Google Maps Distance Matrix API request.
    /// </summary>
    public class OriginDestination
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
    public class RouteMatrixRequest
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
