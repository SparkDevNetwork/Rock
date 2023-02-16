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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.IpAddress.Classes;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.IpAddress
{
    /// <summary>
    /// IP detail service from IpRegistry
    /// </summary>
    [Description( "IP detail service from IpRegistry" )]
    [Export( typeof( IpAddressLookupComponent ) )]
    [ExportMetadata( "ComponentName", "IpRegistry" )]

    [TextField(
        "API Key",
        Description = "The IpRegistry API Key.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.APIKey )]
    [Rock.SystemGuid.EntityTypeGuid( "7AFE6DFA-5FC4-4554-98D2-5BD4C909558B" )]
    public class IpRegistry : IpAddressLookupComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The API Key
            /// </summary>
            public const string APIKey = "APIKey";
        }

        #endregion Keys

        #region Constants

        // The maximum number of requests we can send in one call
        // https://ipregistry.co/docs/endpoints#batch-ip
        private const int _maxBulkRequestSize = 1024;

        #endregion

        /// <inheritdoc/>
        public override bool VerifyCanProcess( out string statusMessage )
        {
            // Check the available credits to indicate the status of the account.
            var client = GetIpRegistryRestClient();
            var status = GetStatus( client );

            if ( !status.IsAvailable )
            {
                statusMessage = status.StatusMessage.ToStringOrDefault( "Service not available." );
                return false;
            }

            // The service is responding, so check credit and rate limits.
            if ( status.AvailableCreditTotal.GetValueOrDefault(-1) == 0 )
            {
                statusMessage = "Insufficient account credit to process the request.";
                return false;
            }
            if ( status.AvailableCreditInRateWindow.GetValueOrDefault(-1) == 0 )
            {
                statusMessage = $"Rate limited until {status.RateWindowResetTime.ToShortDateString()}.";
                return false;
            }

            statusMessage = status.StatusMessage;
            return true;

        }

        /// <summary>
        /// It takes the single IpAddress and returns the location.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        public override IpLocation Lookup( string ipAddress, out string resultMsg )
        {
            resultMsg = string.Empty;

            var result = new IpLocation();

            // Create REST client
            var client = GetIpRegistryRestClient();

            // Create and configure REST request
            var request = new RestRequest("{ipAddress}", Method.GET );
            request.AddUrlSegment( "ipAddress", ipAddress  );
            request.AddHeader( "Accept", "application/json" );

            var response = client.Execute( request );

            // Process successful response
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var responseContent = JsonConvert.DeserializeObject( response.Content, typeof( Result ) ) as Result;
                result = ConvertResultToIpLocation( responseContent );
            }
            else if ( response.StatusCode == HttpStatusCode.BadRequest || ( int ) response.StatusCode == 429 )
            {
                var responseContent = JsonConvert.DeserializeObject( response.Content, typeof( Result ) ) as Result;
                result = ConvertResultToIpLocation( responseContent );
                resultMsg = $"{response.StatusDescription}.";
                if ( responseContent.Resolution.IsNotNullOrWhiteSpace() )
                {
                    resultMsg += responseContent.Resolution;
                }
            }
            else // Some other HTTP result
            {
                resultMsg = response.StatusDescription;
            }

            return result;
        }

        /// <summary>
        /// Takes a list of IP Addresses and returns the location information associated with them.
        /// </summary>
        /// <param name="ipAddresses"></param>
        /// <param name="resultMsg"></param>
        /// <returns></returns>
        public override List<IpLocation> BulkLookup( List<string> ipAddresses, out string resultMsg )
        {
            resultMsg = string.Empty;

            var results = new List<IpLocation>();

            // Create REST client
            var client = GetIpRegistryRestClient();

            // Create a pointer index to know where we are in the batch process
            var sessionLoopIndex = 0;

            // Well run through the IP address to lookup in batches based on the services max bulk lookup size
            while ( ipAddresses.Count() > sessionLoopIndex )
            {
                // Get IP address to use for call
                var ipAddressesBatch = ipAddresses
                    .Skip( sessionLoopIndex )
                    .Take( _maxBulkRequestSize )
                    .ToList();

                // Create and configure REST request
                var request = new RestRequest( Method.POST );
                request.RequestFormat = DataFormat.Json;
                request.AddHeader( "Accept", "application/json" );
                request.AddParameter( "application/json", ipAddressesBatch.ToJson(), ParameterType.RequestBody );

                var response = client.Execute( request );
                var rateLimitResetInSeconds = response.Headers.Where( a => a.Name == "X-Rate-Limit-Reset" ).FirstOrDefault()?.Value.ToString().AsIntegerOrNull();
                var rateLimitCreditsRemaining = response.Headers.Where( a => a.Name == "X-Rate-Limit-Remaining" ).FirstOrDefault();

                // Process successful response
                if ( response.StatusCode == HttpStatusCode.OK )
                {
                    var responseContent = JsonConvert.DeserializeObject( response.Content, typeof( Root ) ) as Root;

                    foreach ( var result in responseContent.Results )
                    {
                        results.Add( ConvertResultToIpLocation( result ) );
                    }

                    // Increment the loop pointer as these IP address have been processed
                    sessionLoopIndex += _maxBulkRequestSize;
                }
                else if ( ( int ) response.StatusCode == 429 ) // HTTP: Too many requests
                {
                    // If they didn't give us rate limit instructions then bail
                    if ( !rateLimitResetInSeconds.HasValue )
                    {
                        resultMsg = response.StatusDescription;
                        break;
                    }

                    // If the reset period is a long time (5 mins) then bail
                    if ( rateLimitResetInSeconds.Value > 300 )
                    {
                        resultMsg = response.StatusDescription;
                        break;
                    }

                    // Otherwise take a break and wait
                    System.Threading.Thread.Sleep( rateLimitResetInSeconds.Value );
                }
                else if ( ( int ) response.StatusCode == 402 )
                {
                    // Failure Code: Payment Required.
                    throw new Exception( "Processing failed. Insufficient account credit." );
                }
                else // Some other HTTP result
                {
                    resultMsg = response.StatusDescription;
                    break;
                }
            }

            return results;
        }

        internal IpRegistryStatusInfo GetServiceStatus()
        {
            var client = GetIpRegistryRestClient();

            var status = GetStatus( client );
            return status;
        }

        private IpRegistryStatusInfo GetStatus( RestClient client )
        {
            // Create and configure REST request
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( "Accept", "application/json" );

            var ipList = new List<string> { "test_ip" };
            request.AddParameter( "application/json", ipList.ToJson(), ParameterType.RequestBody );

            var response = client.Execute( request );

            var status = GetStatusFromResponse( response );
            return status;
        }

        private IpRegistryStatusInfo GetStatusFromResponse( IRestResponse response )
        {
            var status = new IpRegistryStatusInfo();
            status.IsAvailable = true;
            status.CanProcess = false;
            
            if ( response == null )
            {
                status.IsAvailable = false;
            }
            else if ( response.StatusCode == HttpStatusCode.OK )
            {
                var availableCreditTotal = response.Headers.Where( a => a.Name == "ipregistry-credits-remaining" ).FirstOrDefault();
                if ( availableCreditTotal != null )
                {
                    status.AvailableCreditTotal = availableCreditTotal.Value.ToStringSafe().AsInteger();
                };

                var rateLimitResetInSeconds = response.Headers.Where( a => a.Name == "X-Rate-Limit-Reset" ).FirstOrDefault()?.Value.ToString().AsIntegerOrNull();
                if ( rateLimitResetInSeconds != null )
                {
                    status.RateWindowResetTime = RockDateTime.Now.AddSeconds( rateLimitResetInSeconds.Value );
                }

                var rateLimitCreditsRemaining = response.Headers.Where( a => a.Name == "X-Rate-Limit-Remaining" ).FirstOrDefault();
                if ( rateLimitCreditsRemaining != null )
                {
                    status.AvailableCreditInRateWindow = rateLimitCreditsRemaining.Value.ToStringSafe().AsInteger();
                }

                if ( status.AvailableCreditTotal.GetValueOrDefault(1) > 0
                    && status.AvailableCreditInRateWindow.GetValueOrDefault(1) > 0 )
                {
                    status.CanProcess = true;
                }
            }
            else if ( ( int ) response.StatusCode == 402 )
            {
                // Status Code: PaymentRequired
                status.AvailableCreditTotal = 0;
                status.AvailableCreditInRateWindow = 0;
            }
            else
            {
                status.StatusMessage = response.ErrorMessage;
            }

            return status;
        }

        /// <summary>
        /// Converts IPRegistry's format into what Rock uses for Interaction Session Locations. This normalizes the data
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private IpLocation ConvertResultToIpLocation( Result result )
        {
            var locationInformation = new IpLocation();

            locationInformation.IpAddress = result.IP;

            // Check for invalid IPs
            if ( result.Code.IsNotNullOrWhiteSpace() )
            {
                locationInformation.IsValid = false;
                switch ( result.Code )
                {
                    case "INVALID_IP_ADDRESS":
                        {
                            locationInformation.IpLocationErrorCode = IpLocationErrorCode.InvalidAddress;
                            break;
                        }
                    case "RESERVED_IP_ADDRESS":
                        {
                            locationInformation.IpLocationErrorCode = IpLocationErrorCode.ReservedAddress;
                            break;
                        }
                }

                return locationInformation;
            }

            // Get Defined Types for lookups
            var countryDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES );
            var regionDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_ADDRESS_STATE );

            // Update the region information
            if ( result.Location.Region != null && result.Location.Region.Code.IsNotNullOrWhiteSpace() )
            {
                var regionCode = result.Location.Region.Code.Split( '-' ).LastOrDefault();
                if ( regionCode.IsNotNullOrWhiteSpace() )
                {
                    locationInformation.RegionCode = regionCode.Left( 2 );
                    var regionDefinedValue = regionDefinedType.GetDefinedValueFromValue( regionCode );
                    if ( regionDefinedValue != null )
                    {
                        locationInformation.RegionValueId = regionDefinedValue.Id;
                    }
                }
            }

            // Update the country
            if ( result.Location.Country != null && result.Location.Country.Code.IsNotNullOrWhiteSpace() )
            {
                locationInformation.CountryCode = result.Location.Country.Code;
                var countryDefinedValue = countryDefinedType.GetDefinedValueFromValue( result.Location.Country.Code );
                if ( countryDefinedValue != null )
                {
                    locationInformation.CountryValueId = countryDefinedValue.Id;
                }
            }

            // Update the ISP
            if ( result.Company != null )
            {
                locationInformation.ISP = result.Company.Name.SubstringSafe( 0, 100 );
            }

            // Upate the Lat/Long
            locationInformation.Latitude = result.Location.Latitude ?? 0;
            locationInformation.Longitude = result.Location.Longitude ?? 0;

            locationInformation.PostalCode = result.Location.PostalCode;
            locationInformation.Location = FormatLocation( result ).SubstringSafe( 0, 250 );

            return locationInformation;
        }

        /// <summary>
        /// Formats the location.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private string FormatLocation( Result result )
        {
            if ( result.Location == null || ( result.Location.City.IsNullOrWhiteSpace() && result.Location.Region.Name.IsNullOrWhiteSpace() ) )
            {
                return string.Empty;
            }

            if ( result.Location.City.IsNullOrWhiteSpace() )
            {
                return result.Location.Region.Name;
            }

            if ( result.Location.Region.Name.IsNullOrWhiteSpace() )
            {
                return result.Location.City;
            }

            return $"{result.Location.City}, {result.Location.Region.Name}";
        }

        /// <summary>
        /// Gets a IPRegistry REST client with the API key attached.
        /// </summary>
        /// <returns></returns>
        private RestClient GetIpRegistryRestClient()
        {
            var apiKey = GetAttributeValue( AttributeKey.APIKey );
            var restClient = new RestClient( "https://api.ipregistry.co" );
            restClient.AddDefaultParameter( "key", apiKey, ParameterType.QueryString );
            return restClient;
        }
    }

    #region POCO Classes

    /// <summary>
    /// Information about the current status of the service.
    /// </summary>
    internal class IpRegistryStatusInfo
    {
        /// <summary>
        /// Is the service currently responding?
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Is the service ready to process requests?
        /// </summary>
        public bool CanProcess { get; set; }

        /// <summary>
        /// The total credits available in the active account for processing.
        /// </summary>
        public int? AvailableCreditTotal { get; set; }

        /// <summary>
        /// The credits available for the current rate-limited processing window.
        /// </summary>
        public int? AvailableCreditInRateWindow { get; set; }

        /// <summary>
        /// The time at which the current rate-limited processing window will reset.
        /// </summary>
        public DateTime? RateWindowResetTime { get; set; }

        /// <summary>
        /// A status message providing additional information about the service status.
        /// </summary>
        public string StatusMessage { get; set; }
    }

    /// <summary>
    /// POCO for the Result body
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [JsonProperty( "code" )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "message" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the resolution.
        /// </summary>
        /// <value>
        /// The resolution.
        /// </value>
        [JsonProperty( "resolution" )]
        public string Resolution { get; set; }

        /// <summary>
        /// Gets or sets the IP
        /// </summary>
        /// <value>
        /// The IP.
        /// </value>
        [JsonProperty( "ip" )]
        public string IP { get; set; }

        /// <summary>
        /// Gets or sets the location
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [JsonProperty( "location" )]
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the company
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [JsonProperty( "company" )]
        public Company Company { get; set; }
    }

    /// <summary>
    /// POCO for Location information
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets the postal
        /// </summary>
        /// <value>
        /// The postal.
        /// </value>
        [JsonProperty( "postal" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the latitude
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [JsonProperty( "latitude" )]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [JsonProperty( "longitude" )]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty( "country" )]
        public Country Country { get; set; }

        /// <summary>
        /// Gets or sets the region
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        [JsonProperty( "region" )]
        public Region Region { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [JsonProperty( "city" )]
        public string City { get; set; }
    }

    /// <summary>
    /// POCO for Region
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the code
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [JsonProperty( "code" )]
        public string Code { get; set; }
    }

    /// <summary>
    /// POCO for Country Info
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Gets or sets the code
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [JsonProperty( "code" )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }
    }

    /// <summary>
    /// POCO for Company Information
    /// </summary>
    public class Company
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }
    }

    /// <summary>
    /// POCO for Root Response
    /// </summary>
    public class Root
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "results" )]
        public List<Result> Results { get; set; }
    }

    #endregion
}
