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
using System.Threading.Tasks;

using Rock.Geography.Classes;
using Rock.Geography.GeographyExtensions.GoogleMaps;
using Rock.Enums.Geography;

namespace Rock.Geography
{
    /// <summary>
    /// Provides helper methods for location-related operations, such as retrieving driving distances and durations.
    /// </summary>
    public static class GeographyHelpers
    {
        /// <summary>
        /// Asynchronously geocodes the specified input string to retrieve latitude and longitude coordinates.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<GeographyPoint> Geocode( string input )
        {
            // For now we'll only support Google for geocoding calculations. This static method though abstracts the implementation details,
            // so that in the future if we want to support other providers, we can do so without changing the method signature.
            var googleLocationExtension = new GoogleMapsLocationExtension();

            return await googleLocationExtension.Geocode( input );
        }

        /// <summary>
        /// Asynchronously retrieves a driving matrix for the specified origin and list of destinations.
        /// </summary>
        /// <param name="origin">The starting point. Can be a full address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="destinations">A list of up to 25 destination points. Each can be an address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="mode">The travel mode to use for the calculation</param>
        /// <returns>A list of driving distances and durations for each destination.</returns>
        public static async Task<List<DistanceResult>> GetDrivingMatrixAsync( GeographyPoint origin, List<GeographyPoint> destinations, TravelMode mode = TravelMode.Drive )
        {
            // For now we'll only support Google for driving matrix calculations. This static method though abstracts the implementation details,
            // so that in the future if we want to support other providers, we can do so without changing the method signature.
            var googleLocationExtension = new GoogleMapsLocationExtension();

            return await googleLocationExtension.GetDrivingMatrixAsync( origin, destinations, mode );
        }
    }
}
