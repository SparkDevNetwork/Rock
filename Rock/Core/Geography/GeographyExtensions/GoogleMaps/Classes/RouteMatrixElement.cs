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
using Newtonsoft.Json;

namespace Rock.Core.Geography.GeographyExtensions.GoogleMaps.Classes
{
    /// <summary>
    /// Represents an element in the route matrix response from the Google Maps Distance Matrix API.
    /// </summary>
    internal class RouteMatrixElement
    {
        /// <summary>
        /// The index of the origin in the request that this element corresponds to.
        /// </summary>
        public int OriginIndex { get; set; }

        /// <summary>
        /// The index of the destination in the request that this element corresponds to.
        /// </summary>
        public int DestinationIndex { get; set; }

        /// <summary>
        /// The distance in meters between the origin and destination.
        /// </summary>
        public int DistanceMeters { get; set; }

        /// <summary>
        /// The duration in seconds between the origin and destination.
        /// </summary>
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
