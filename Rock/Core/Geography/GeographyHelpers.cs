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

using Rock.Core.Geography.Classes;
using Rock.Core.Geography.GeographyExtensions.GoogleMaps;
using Rock.Enums.Geography;

namespace Rock.Core.Geography
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
        /// Asynchronously geocodes the specified input string to a point and its recommended viewport.
        /// </summary>
        /// <param name="input">The address, ZIP, city, or place to geocode.</param>
        /// <returns>The point and viewport, or <c>null</c> when the input could not be resolved.</returns>
        /// <remarks>
        /// Internal by design: this richer result backs an in-flight feature and may change or be
        /// removed, so it is not exposed to plugins. The viewport is sized to the match, making it a
        /// natural location-appropriate search boundary.
        /// </remarks>
        internal static async Task<GeocodeResult> GeocodeDetailed( string input )
        {
            // Only Google is supported for now (see Geocode); this abstracts the provider so it can
            // change without touching callers.
            var googleLocationExtension = new GoogleMapsLocationExtension();

            return await googleLocationExtension.GeocodeDetailed( input );
        }

        /// <summary>
        /// Asynchronously retrieves address autocomplete suggestions for a partial address.
        /// </summary>
        /// <param name="input">The partial address, ZIP, city, or place the visitor has typed so far.</param>
        /// <returns>The matching suggestion descriptions, or an empty list when the input is blank or nothing matched.</returns>
        /// <remarks>
        /// Internal by design: this backs an in-flight feature (an address type-ahead) and may change, so
        /// it is not exposed to plugins. Only Google is supported for now; this abstracts the provider.
        /// </remarks>
        internal static async Task<List<string>> GetAddressSuggestionsAsync( string input )
        {
            var googleLocationExtension = new GoogleMapsLocationExtension();

            return await googleLocationExtension.GetAddressSuggestionsAsync( input );
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
            return await GetDrivingMatrixAsync( origin, destinations, mode, RouteMatrixDetail.Full );
        }

        /// <summary>
        /// Asynchronously retrieves a driving matrix for the specified origin and list of destinations, returning only the requested detail.
        /// </summary>
        /// <param name="origin">The starting point. Can be a full address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="destinations">A list of up to 25 destination points. Each can be an address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="mode">The travel mode to use for the calculation.</param>
        /// <param name="detail">How much data each element returns, which also sets the billing tier. Request <see cref="RouteMatrixDetail.DistanceOnly"/> to stay within the Essentials tier when only the distance is needed.</param>
        /// <returns>A list of driving distances and durations for each destination.</returns>
        /// <remarks>
        /// Internal by design: the billing-tier control is an implementation concern that may change, so it
        /// is not exposed to plugins. The existing public overload is unchanged for backward compatibility.
        /// </remarks>
        internal static async Task<List<DistanceResult>> GetDrivingMatrixAsync( GeographyPoint origin, List<GeographyPoint> destinations, TravelMode mode, RouteMatrixDetail detail )
        {
            if ( destinations == null || destinations.Count == 0 )
            {
                return new List<DistanceResult>();
            }

            // For now we'll only support Google for driving matrix calculations. This static method though abstracts the implementation details,
            // so that in the future if we want to support other providers, we can do so without changing the method signature.
            var googleLocationExtension = new GoogleMapsLocationExtension();

            return await googleLocationExtension.GetDrivingMatrixAsync( origin, destinations, mode, detail );
        }
    }
}
