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

using Newtonsoft.Json;

using Rock.Core.Geography.Classes;
using Rock.Core.Geography.GeographyExtensions.GoogleMaps.Classes;
using Rock.Enums.Geography;
using Rock.Web.Cache;

using Twilio.Types;

namespace Rock.Core.Geography.GeographyExtensions.GoogleMaps
{
    /// <summary>
    /// Provides methods to interact with the Google Maps Distance Matrix API for calculating driving distances and durations.
    /// </summary>
    internal class GoogleMapsLocationExtension
    {
        private string _apiKey;

        #region Constructors
        public GoogleMapsLocationExtension()
        {
            _apiKey = GlobalAttributesCache.Get().GetValue( "GoogleApiKeyServer" );

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
        /// Asynchronously geocodes the specified input string to a point and its recommended viewport using the Google Maps Geocoding API.
        /// </summary>
        /// <param name="input">The address, ZIP, city, or place to geocode.</param>
        /// <returns>The point and viewport, or <c>null</c> when the input could not be resolved.</returns>
        public async Task<GeocodeResult> GeocodeDetailed( string input )
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

                    var json = await response.Content.ReadAsStringAsync();
                    using ( var doc = JsonDocument.Parse( json ) )
                    {
                        var root = doc.RootElement;

                        if ( root.GetProperty( "status" ).GetString() != "OK" )
                        {
                            return null;
                        }

                        var geometry = root.GetProperty( "results" )[0].GetProperty( "geometry" );

                        var location = geometry.GetProperty( "location" );
                        var point = new GeographyPoint(
                            location.GetProperty( "lat" ).GetDouble(),
                            location.GetProperty( "lng" ).GetDouble() );

                        // Google always returns a viewport sized to the match (a ZIP, city, and street
                        // address each come back with a differently sized box), which is exactly the
                        // location-appropriate search boundary we want. It is optional in the schema, so
                        // fall back to just the point when it is absent.
                        GeographyBounds viewport = null;
                        if ( geometry.TryGetProperty( "viewport", out var viewportElement ) )
                        {
                            var northeast = viewportElement.GetProperty( "northeast" );
                            var southwest = viewportElement.GetProperty( "southwest" );
                            viewport = new GeographyBounds(
                                northeast.GetProperty( "lat" ).GetDouble(),
                                southwest.GetProperty( "lat" ).GetDouble(),
                                northeast.GetProperty( "lng" ).GetDouble(),
                                southwest.GetProperty( "lng" ).GetDouble() );
                        }

                        return new GeocodeResult( point, viewport );
                    }
                }
                catch
                {
                    // A transient failure (network, quota, or an unexpected response shape) yields no
                    // result; the caller treats the location as unresolved rather than erroring.
                    return null;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves address suggestions for a partial address using the Google Places API (New) Autocomplete endpoint.
        /// </summary>
        /// <param name="input">The partial address, ZIP, city, or place the visitor has typed so far.</param>
        /// <returns>The matching suggestion texts, or an empty list when the input is blank or nothing matched.</returns>
        public async Task<List<string>> GetAddressSuggestionsAsync( string input )
        {
            var suggestions = new List<string>();

            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return suggestions;
            }

            using ( var httpClient = new HttpClient() )
            {
                /*
                    07/28/26 - JMH

                    "geocode" biases predictions to addresses, ZIPs, cities, and regions (the things a
                    location search targets) rather than businesses. Autocomplete bills per request and, unlike
                    Text Search, returns the suggestion text without pushing the call into a higher billing tier.

                    Reason: Keep address suggestions on the cheapest Places (New) endpoint.
                */
                var body = new
                {
                    input,
                    includedPrimaryTypes = new[] { "geocode" }
                };

                var requestJson = body.ToJson();

                var request = new HttpRequestMessage( HttpMethod.Post, "https://places.googleapis.com/v1/places:autocomplete" )
                {
                    Content = new StringContent( requestJson, Encoding.UTF8, "application/json" )
                };

                request.Headers.Add( "X-Goog-Api-Key", _apiKey );

                // Ask only for each prediction's display text so the response stays small.
                request.Headers.Add( "X-Goog-FieldMask", "suggestions.placePrediction.text.text" );

                try
                {
                    var response = await httpClient.SendAsync( request );

                    if ( !response.IsSuccessStatusCode )
                    {
                        return suggestions;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using ( var doc = JsonDocument.Parse( json ) )
                    {
                        var root = doc.RootElement;

                        // A response with no "suggestions" array means nothing matched.
                        if ( !root.TryGetProperty( "suggestions", out var suggestionElements ) )
                        {
                            return suggestions;
                        }

                        foreach ( var suggestion in suggestionElements.EnumerateArray() )
                        {
                            if ( suggestion.TryGetProperty( "placePrediction", out var placePrediction )
                                && placePrediction.TryGetProperty( "text", out var textElement )
                                && textElement.TryGetProperty( "text", out var textValue ) )
                            {
                                var text = textValue.GetString();
                                if ( !string.IsNullOrWhiteSpace( text ) )
                                {
                                    suggestions.Add( text );
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // A transient failure (network, quota) just yields no suggestions; the visitor can still
                    // type a full address and search.
                    return suggestions;
                }
            }

            return suggestions;
        }

        /// <summary>
        /// Asynchronously retrieves a driving matrix for the specified origin and list of destinations.
        /// </summary>
        /// <param name="origin">The origin lat/long </param>
        /// <param name="destinations">A list of up to 25 destination points. Each can be an address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="mode">The travel mode to use for the calculation.</param>
        /// <returns>A list of driving distances and durations for each destination.</returns>
        public async Task<List<DistanceResult>> GetDrivingMatrixAsync( GeographyPoint origin, List<GeographyPoint> destinations, TravelMode mode, RouteMatrixDetail detail )
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

                // Request only the fields the caller needs. Distance-only and distance-plus-static-duration
                // both stay in the Essentials billing tier (the request is not traffic-aware); "*" can pull
                // in Pro/Enterprise fields that bill at higher rates.
                string fieldMask;
                switch ( detail )
                {
                    case RouteMatrixDetail.DistanceOnly:
                        fieldMask = "originIndex,destinationIndex,distanceMeters";
                        break;
                    case RouteMatrixDetail.DistanceAndDuration:
                        fieldMask = "originIndex,destinationIndex,distanceMeters,duration";
                        break;
                    default:
                        fieldMask = "*";
                        break;
                }
                request.Headers.Add( "X-Goog-FieldMask", fieldMask );

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
                            TravelTimeInMinutes = ( int ) ( e.DurationTimeSpan ?? TimeSpan.Zero ).TotalMinutes                            
                        } )
                .ToList();
            }

        }
    }
}
