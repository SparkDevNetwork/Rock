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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
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

        /// <summary>
        /// Gets all the IP Address result through IPRegistry
        /// </summary>
        public override LookupResult Lookup( Dictionary<string, List<int>> ipAddressesWithSessionIds, out string resultMsg )
        {
            var lookupResult = new LookupResult();
            resultMsg = string.Empty;
            var ipAddressCount = ipAddressesWithSessionIds.Count;
            var requestCount = 0;
            //maximum of 1024 ipaddresses can be handled in batch request
            var maxBatchRecords = 1024;
            while ( ipAddressCount > 0 )
            {
                var apiKey = GetAttributeValue( AttributeKey.APIKey );
                var client = new RestClient( string.Format( "https://api.ipregistry.co?key={0}", apiKey ) );
                var request = new RestRequest( Method.POST );
                request.RequestFormat = DataFormat.Json;
                request.AddHeader( "Accept", "application/json" );
                var takeCount = ipAddressCount > maxBatchRecords ? maxBatchRecords : ipAddressCount;
                var ipAddresses = ipAddressesWithSessionIds.Skip( requestCount * maxBatchRecords ).Take( takeCount );
                var ipAddressBody = ipAddresses.Select( a => a.Key ).ToList();
                request.AddParameter( "application/json", ipAddressBody.ToJson(), ParameterType.RequestBody );
                var response = client.Execute( request );
                var rateLimit = response.Headers.Where( a => a.Name == "X-Rate-Limit-Reset" ).FirstOrDefault();
                var batchRemaining = response.Headers.Where( a => a.Name == "X-Rate-Limit-Remaining" ).FirstOrDefault();
                if ( response.StatusCode == HttpStatusCode.OK )
                {
                    var responseContent = JsonConvert.DeserializeObject( response.Content, typeof( Root ) ) as Root;
                    var rockContext = new RockContext();
                    var interactionSessionLocationService = new InteractionSessionLocationService( rockContext );
                    var interactionSessionService = new InteractionSessionService( rockContext );
                    var countryDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES ).DefinedValues;
                    var regionDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ).DefinedValues;
                    foreach ( var result in responseContent.Results.Where( a => ipAddressesWithSessionIds.ContainsKey( a.IP ) ) )
                    {
                        var interactionSessions = interactionSessionService.GetByIds( ipAddressesWithSessionIds[result.IP] );
                        if ( result.Location != null )
                        {
                            var interactionSessionLocation = new InteractionSessionLocation
                            {
                                IpAddress = result.IP,
                                PostalCode = result.Location.PostalCode,
                                Location = $"{result.Location.City}, {result.Location.Region.Name}",
                                LookupDateTime = RockDateTime.Now
                            };

                            var regionCode = result.Location.Region.Code.Split( '-' ).LastOrDefault();
                            if ( regionCode.IsNotNullOrWhiteSpace() )
                            {
                                interactionSessionLocation.RegionCode = regionCode.Left( 2 );
                                var regionDefinedValue = regionDefinedValues.Where( a => a.Value == regionCode ).FirstOrDefault();
                                if ( regionDefinedValue != null )
                                {
                                    interactionSessionLocation.RegionValueId = regionDefinedValue.Id;
                                }
                            }

                            if ( result.Location.Country.Code.IsNotNullOrWhiteSpace() )
                            {
                                interactionSessionLocation.CountryCode = result.Location.Country.Code;
                                var countryDefinedValue = countryDefinedValues.Where( a => a.Value == result.Location.Country.Code ).FirstOrDefault();
                                if ( countryDefinedValue != null )
                                {
                                    interactionSessionLocation.CountryValueId = countryDefinedValue.Id;
                                }
                            }

                            if ( result.Company != null )
                            {
                                interactionSessionLocation.ISP = result.Company.Name;
                            }

                            interactionSessionLocation.GeoPoint = Rock.Model.Location.GetGeoPoint( result.Location.Latitude, result.Location.Longitude );
                            interactionSessionLocationService.Add( interactionSessionLocation );
                            foreach ( var interactionSession in interactionSessions )
                            {
                                lookupResult.SuccessCount += 1;
                                interactionSessionLocation.InteractionSessions.Add( interactionSession );
                            }
                            
                            rockContext.SaveChanges();
                        }
                    }

                    ipAddressCount = ipAddressCount - takeCount;
                    requestCount += takeCount;
                }
                else if((int)response.StatusCode ==  429 )
                {
                    if ( rateLimit != null && rateLimit.Value.ToString().AsInteger() > 300 )
                    {
                        resultMsg = response.StatusDescription;
                        break;
                    }
                }
                else
                {
                    resultMsg = response.StatusDescription;
                    break;
                }

                if ( batchRemaining!=null &&   maxBatchRecords > batchRemaining.Value.ToString().AsInteger() )
                {
                    maxBatchRecords = batchRemaining.Value.ToString().AsInteger();
                }

                if ( rateLimit != null && rateLimit.Value.ToString().AsInteger() < 300 )
                {
                    System.Threading.Thread.Sleep( rateLimit.Value.ToString().AsInteger() );
                }
                else
                {
                    break;
                }
            }

            lookupResult.FailedCount = ipAddressesWithSessionIds.SelectMany( a => a.Value ).Count() - lookupResult.SuccessCount;
            return lookupResult;
        }
    }



    #region Nested Classes

    /// <summary>
    /// 
    /// </summary>
    public class Result
    {
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
    /// 
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
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [JsonProperty( "longitude" )]
        public double Longitude { get; set; }

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
    /// 
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
    /// 
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
    /// 
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
    /// 
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
