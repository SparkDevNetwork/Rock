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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;

using Rock.Address.Classes;
using Rock.Address.LocationExtensions.GoogleMaps.Classes;
using Rock.Enums.Location;
using Rock.Web.Cache;

namespace Rock.Address.LocationExtensions.GoogleMaps
{
    /// <summary>
    /// Provides methods to interact with the Google Maps Distance Matrix API for calculating driving distances and durations.
    /// </summary>
    public class GoogleMapsLocationExtension
    {
        /// <summary>
        /// Asynchronously retrieves a driving matrix for the specified origin and list of destinations.
        /// </summary>
        /// <param name="origin">The origin lat/long </param>
        /// <param name="destinations">A list of up to 25 destination points. Each can be an address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="mode">The travel mode to use for the calculation.</param>
        /// <returns>A list of driving distances and durations for each destination.</returns>
        public async Task<List<DistanceResult>> GetDrivingMatrixAsync( GeographyPoint origin, List<GeographyPoint> destinations, TravelMode mode )
        {
            using ( var httpClient = new HttpClient() )
            {

                var apiKey = GlobalAttributesCache.Get().GetValue( "GoogleApiKey" );

                var url = $"https://routes.googleapis.com/distanceMatrix/v2:computeRouteMatrix?key={apiKey}";

                var body = new
                {
                    origins = new[]
                    {
                        new
                        {
                            waypoint = new
                            {
                                location = new
                                {
                                    latLng = new
                                    {
                                        latitude = origin.Latitude,
                                        longitude = origin.Longitude
                                    }
                                }
                            }
                        }
                    },
                    destinations = destinations.Select( dest => new
                    {
                        waypoint = new
                        {
                            location = new
                            {
                                latLng = new
                                {
                                    latitude = dest.Latitude,
                                    longitude = dest.Longitude
                                }
                            }
                        }
                    } ),
                    travelMode = mode.ToString().ToUpper()
                };

                var requestJson = JsonConvert.SerializeObject( body );
                var request = new HttpRequestMessage( HttpMethod.Post, url )
                {
                    Content = new StringContent( requestJson, Encoding.UTF8, "application/json" )
                };

                var response = await httpClient.SendAsync( request );
                response.EnsureSuccessStatusCode();

                var ndjson = await response.Content.ReadAsStringAsync();

                // Parse NDJSON
                var routeElements = new List<RouteMatrixElement>();

                var lines = ndjson.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var line in lines )
                {
                    var element = JsonConvert.DeserializeObject<RouteMatrixElement>( line );
                    if ( element?.Status == "OK" )
                    {
                        routeElements.Add( element );
                    }
                }

                var results = routeElements
                    .Where( e => e.DestinationIndex < destinations.Count )
                    .Select( e => new DistanceResult
                    {
                        DestinationPoint = destinations[e.DestinationIndex],
                        DistanceMiles = e.DistanceMiles,
                        TimeMinutes = (e.DurationTimeSpan ?? TimeSpan.Zero).Minutes
                    } )
                    .ToList();

                return results;
            }

        }
    }
}
