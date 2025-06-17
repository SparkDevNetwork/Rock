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

using Nest;

using Newtonsoft.Json;

using Rock.Address.Classes;
using Rock.Address.LocationExtensions.GoogleMaps.Classes;
using Rock.Enums.Location;
using Rock.Web.Cache;

using Twilio.Types;

namespace Rock.Address.LocationExtensions.GoogleMaps
{
    /// <summary>
    /// Provides methods to interact with the Google Maps Distance Matrix API for calculating driving distances and durations.
    /// </summary>
    public class GoogleMapsLocationExtension
    {
        private string _apiKey;

        #region Constructors
        public GoogleMapsLocationExtension()
        {
            _apiKey = GlobalAttributesCache.Get().GetValue( "GoogleApiKey" );

            if ( _apiKey.IsNullOrWhiteSpace() )
            {
                throw new Exception( $"Google Maps API key required." );
            }
        }

        public GoogleMapsLocationExtension( string apiKey )
        {
            _apiKey = apiKey;
        }
        #endregion

        /// <summary>
        /// Asynchronously geocodes the specified input string to a GeographyPoint using the Google Maps Geocoding API.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<GeographyPoint> Geocode( string input )
        {
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return null;
            }

            using ( var httpClient = new HttpClient() )
            {
                string requestUri = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString( input )}&key={_apiKey}";

                try
                {
                    var response = await httpClient.GetAsync( requestUri );

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }
                        
                    // Parse the results
                    var json = await response.Content.ReadAsStringAsync();
                    using ( var doc = JsonDocument.Parse( json ) )
                    {
                        var root = doc.RootElement;
                        var status = root.GetProperty( "status" ).GetString();

                        if ( status != "OK" )
                            return null;

                        var location = root
                            .GetProperty( "results" )[0]
                            .GetProperty( "geometry" )
                            .GetProperty( "location" );

                        double lat = location.GetProperty( "lat" ).GetDouble();
                        double lng = location.GetProperty( "lng" ).GetDouble();

                        return new GeographyPoint( lat, lng );
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

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
                // Create message body
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

                var url = $"https://routes.googleapis.com/distanceMatrix/v2:computeRouteMatrix?key={_apiKey}";
                var request = new HttpRequestMessage( HttpMethod.Post, url )
                {
                    Content = new StringContent( requestJson, Encoding.UTF8, "application/json" )
                };

                request.Headers.Add( "X-Goog-FieldMask", "*" );

                var response = await httpClient.SendAsync( request );

                // Ensure API call was a success
                if ( response.StatusCode != System.Net.HttpStatusCode.OK )
                {
                    throw new Exception( $"Google Maps API Route API request failed with status code {response.StatusCode}. Check that API key is correct and has access to Route API." );
                }

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<RouteMatrixElement>>( json );

                var routeElements = new List<RouteMatrixElement>();

                return results
                    .Where( e => e.DestinationIndex < destinations.Count )
                    .Select( e => new DistanceResult
                        {
                            DestinationPoint = destinations[e.DestinationIndex],
                            DistanceInMeters = e.DistanceMeters,
                            TravelTimeInMinutes = ( e.DurationTimeSpan ?? TimeSpan.Zero ).Minutes                            
                        } )
                .ToList();
            }

        }
    }
}
