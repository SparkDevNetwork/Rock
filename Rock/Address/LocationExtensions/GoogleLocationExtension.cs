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
using System.Text.Json;
using System.Threading.Tasks;

using Rock.Address.Classes;
using Rock.Enums.Location;
using Rock.Web.Cache;

namespace Rock.Address.LocationExtensions
{
    public class GoogleLocationExtension
    {
        /// <summary>
        /// Asynchronously retrieves a driving matrix for the specified origin and list of destinations.
        /// </summary>
        /// <param name="origin">The starting point. Can be a full address, lat/lng, ZIP code, or place ID.</param>
        /// <param name="destinations">A list of up to 25 destination points. Each can be an address, lat/lng, ZIP code, or place ID.</param>
        /// <returns>A list of driving distances and durations for each destination.</returns>
        public async Task<List<DrivingDistanceResult>> GetDrivingMatrixAsync( string origin, List<string> destinations, TravelMode mode )
        {
            using ( var httpClient = new HttpClient() )
            {

                var apiKey = GlobalAttributesCache.Get().GetValue( "GoogleApiKey" );

                // Validate the API key
                if ( apiKey.IsNullOrWhiteSpace() )
                {
                    return new List<DrivingDistanceResult>();
                }
                
                var destinationsParam = string.Join( "|", destinations.Select( Uri.EscapeDataString ) );
                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                          $"?origins={Uri.EscapeDataString( origin )}" +
                          $"&destinations={destinationsParam}" +
                          $"&mode={mode.ToString().ToLower()}" +
                          $"&units=imperial&key={apiKey}";

                var response = await httpClient.GetAsync( url );
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var matrix = JsonSerializer.Deserialize<DistanceMatrixResponse>( content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                } );

                var results = new List<DrivingDistanceResult>();
                var elements = matrix?.Rows?.FirstOrDefault()?.Elements;

                if ( elements != null )
                {
                    for ( int i = 0; i < elements.Count; i++ )
                    {
                        var element = elements[i];
                        if ( element.Status == "OK" )
                        {
                            results.Add( new DrivingDistanceResult
                            {
                                Destination = matrix.Destination_Addresses[i],
                                DrivingDistanceMiles = element.Distance.Value / 1609.34, // meters to miles
                                DriveTimeMinutes = element.Duration.Value / 60 // seconds to minutes
                            } );
                        }
                    }
                }

                return results;
            }
        }
        
    }

    #region Response Classes

    /// <summary>
    /// Represents the response from the Google Distance Matrix API.
    /// </summary>
    public class DistanceMatrixResponse
    {
        /// <summary>
        /// The list of destination addresses returned by the API.
        /// </summary>
        public List<string> Destination_Addresses { get; set; }

        /// <summary>
        /// The list of origin addresses used in the request.
        /// </summary>
        public List<string> Origin_Addresses { get; set; }

        /// <summary>
        /// The rows of the distance matrix, each containing elements for each destination.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// The status of the distance matrix request.
        /// </summary>
        public string Status { get; set; }
    }


    /// <summary>
    /// Represents a row in the distance matrix response, containing elements for each destination.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// The list of elements in the row, each representing a distance and duration for a specific origin-destination pair.
        /// </summary>
        public List<Element> Elements { get; set; }
    }

    /// <summary>
    /// Represents an element in the distance matrix response, containing distance and duration information for a specific origin-destination pair.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// The distance and duration information for the origin-destination pair.
        /// </summary>
        public Distance Distance { get; set; }

        /// <summary>
        /// The duration information for the origin-destination pair.
        /// </summary>
        public Duration Duration { get; set; }

        /// <summary>
        /// The status of the element, indicating whether the distance and duration were successfully calculated.
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// Represents a driving distance and duration result for a specific destination.
    /// </summary>
    public class Distance
    {
        /// <summary>
        /// The text representation of the distance (e.g., "10 miles").
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The numeric value of the distance in meters.
        /// </summary>
        public int Value { get; set; } // in meters
    }

    /// <summary>
    /// Represents the duration of a trip in the distance matrix response.
    /// </summary>
    public class Duration
    {
        /// <summary>
        /// The text representation of the duration (e.g., "15 mins").
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The numeric value of the duration in seconds.
        /// </summary>
        public int Value { get; set; } // in seconds
    }
    #endregion
}
