using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rock.Address.LocationExtensions.GoogleMaps.Classes
{
    /// <summary>
    /// Represents an element in the route matrix response from the Google Maps Distance Matrix API.
    /// </summary>
    public class RouteMatrixElement
    {
        /// <summary>
        /// The index of the origin in the request that this element corresponds to.
        /// </summary>
        [JsonProperty( "originIndex" )]
        public int OriginIndex { get; set; }

        /// <summary>
        /// The index of the destination in the request that this element corresponds to.
        /// </summary>
        [JsonProperty( "destinationIndex" )]
        public int DestinationIndex { get; set; }

        /// <summary>
        /// The status of the element, indicating whether the distance and duration could be calculated successfully.
        /// </summary>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// The distance in meters between the origin and destination.
        /// </summary>
        [JsonProperty( "distanceMeters" )]
        public int DistanceMeters { get; set; }

        /// <summary>
        /// The duration in seconds between the origin and destination.
        /// </summary>
        [JsonProperty( "duration" )]
        public string Duration { get; set; }

        /// <summary>
        /// The distance in miles between the origin and destination.
        /// </summary>
        public double DistanceMiles => DistanceMeters / 1609.34;

        /// <summary>
        /// The duration as a TimeSpan object, parsed from the Duration string.
        /// </summary>
        public TimeSpan? DurationTimeSpan => TryParseDuration( Duration );

        private static TimeSpan? TryParseDuration( string s )
        {
            if ( string.IsNullOrEmpty( s ) )
                return null;
            if ( s.EndsWith( "s" ) && double.TryParse( s.TrimEnd( 's' ), out var seconds ) )
                return TimeSpan.FromSeconds( seconds );
            return null;
        }
    }
}
