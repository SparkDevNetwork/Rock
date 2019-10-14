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
using System.Linq;
using System.Net;

using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Address
{
    /// <summary>
    /// The standardization/geocoding service from SmartyStreets
    /// </summary>
    [Description( "Address verification service from SmartyStreets" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Smarty Streets" )]

    [BooleanField( "Use Managed API Key", "Enable this to use the Auth ID and Auth Token that is managed by Spark.", true, "", 1 )]
    [TextField( "Auth ID", "The Smarty Streets Authorization ID. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.", false, "", "", 2 )]
    [TextField( "Auth Token", "The Smarty Streets Authorization Token. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.", false, "", "", 3 )]
    [TextField( "Acceptable DPV Codes", "The Smarty Streets Delivery Point Validation (DPV) match code values that are considered acceptable levels of standardization (see http://smartystreets.com/kb/liveaddress-api/field-definitions#dpvmatchcode for details).", false, "Y,S,D", "", 4 )]
    [TextField( "Acceptable Precisions", "The Smarty Streets latitude & longitude precision values that are considered acceptable levels of geocoding (see http://smartystreets.com/kb/liveaddress-api/field-definitions#precision for details).", false, "Zip7,Zip8,Zip9", "", 5 )]
    public class SmartyStreets : VerificationComponent
    {
        /// <summary>
        /// Standardizes and Geocodes an address using Smarty Streets service
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns>
        /// True/False value of whether the verification was successful or not
        /// </returns>
        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            VerificationResult result = VerificationResult.None;
            resultMsg = string.Empty;

            SmartyStreetsAPIKey apiKey = GetAPIKey();

            var dpvCodes = GetAttributeValue( "AcceptableDPVCodes" ).SplitDelimitedValues();
            var precisions = GetAttributeValue( "AcceptablePrecisions" ).SplitDelimitedValues();

            var payload = new[] { new { addressee = location.Name, street = location.Street1, street2 = location.Street2, city = location.City, state = location.State, zipcode = location.PostalCode, candidates = 1 } };

            var client = new RestClient( string.Format( "https://api.smartystreets.com/street-address?auth-id={0}&auth-token={1}", apiKey.AuthID, apiKey.AuthToken ) );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( "Accept", "application/json" );
            request.AddBody( payload );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var candidates = JsonConvert.DeserializeObject( response.Content, typeof( List<CandidateAddress> ) ) as List<CandidateAddress>;
                if ( candidates.Any() )
                {
                    var candidate = candidates.FirstOrDefault();
                    resultMsg = string.Format( "RecordType:{0}; DPV MatchCode:{1}; Precision:{2}",
                        candidate.metadata.record_type, candidate.analysis.dpv_match_code, candidate.metadata.precision );

                    location.StandardizeAttemptedResult = candidate.analysis.dpv_match_code;
                    if ( dpvCodes.Contains( candidate.analysis.dpv_match_code ) )
                    {
                        location.Street1 = candidate.delivery_line_1;
                        location.Street2 = candidate.delivery_line_2;
                        location.City = candidate.components.city_name;
                        location.County = candidate.metadata.county_name;
                        location.State = candidate.components.state_abbreviation;
                        location.PostalCode = candidate.components.zipcode + "-" + candidate.components.plus4_code;
                        location.Barcode = candidate.delivery_point_barcode;
                        result = result | VerificationResult.Standardized;
                    }

                    location.GeocodeAttemptedResult = candidate.metadata.precision;
                    if ( precisions.Contains( candidate.metadata.precision ) )
                    {
                        if ( location.SetLocationPointFromLatLong( candidate.metadata.latitude, candidate.metadata.longitude ) )
                        {
                            result = result | VerificationResult.Geocoded;
                        }
                    }

                }
                else
                {
                    resultMsg = "No Match";
                }
            }
            else
            {
                result = VerificationResult.ConnectionError;
                resultMsg = response.StatusDescription;
            }

            return result;
        }

        /// <summary>
        /// Gets the location from postal code.
        /// </summary>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        public MapCoordinate GetLocationFromPostalCode( string postalCode, out string resultMsg )
        {
            MapCoordinate result = null;
            resultMsg = string.Empty;

            if ( this.IsActive )
            {

                SmartyStreetsAPIKey apiKey = GetAPIKey();

                var payload = new[] { new { zipcode = postalCode } };

                var client = new RestClient( string.Format( "https://us-zipcode.api.smartystreets.com/lookup?auth-id={0}&auth-token={1}", apiKey.AuthID, apiKey.AuthToken ) );
                var request = new RestRequest( Method.POST );
                request.RequestFormat = DataFormat.Json;
                request.AddHeader( "Accept", "application/json" );
                request.AddBody( payload );
                var response = client.Execute( request );

                if ( response.StatusCode == HttpStatusCode.OK )
                {
                    var lookupResponse = JsonConvert.DeserializeObject( response.Content, typeof( List<LookupResponse> ) ) as List<LookupResponse>;
                    if ( lookupResponse != null && lookupResponse.Any()  && lookupResponse.First().zipcodes.Any() )
                    {
                        var zipcode = lookupResponse.First().zipcodes.FirstOrDefault();
                        result = new MapCoordinate();
                        result.Latitude = zipcode.latitude;
                        result.Longitude = zipcode.longitude;
                        resultMsg = JsonConvert.SerializeObject( zipcode );
                    }
                    else
                    {
                        resultMsg = "No Match";
                    }
                }
                else
                {
                    resultMsg = response.StatusDescription;
                }
            }
            else
            {
                resultMsg = "Smarty Steets is not active.";
            }

            return result;
        }

        private SmartyStreetsAPIKey GetAPIKey()
        {
            SmartyStreetsAPIKey apiKey = null;
            if ( GetAttributeValue( "UseManagedAPIKey" ).AsBooleanOrNull() ?? true )
            {
                var lastKeyUpdate = Rock.Web.SystemSettings.GetValue( "core_SmartyStreetsApiKeyLastUpdate" ).AsDateTime() ?? DateTime.MinValue;
                var hoursSinceLastUpdate = ( RockDateTime.Now - lastKeyUpdate ).TotalHours;
                if ( hoursSinceLastUpdate > 24 || true )
                {
                    var rockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
                    var getAPIKeyClient = new RestClient( "https://www.rockrms.com/api/SmartyStreets/GetSmartyStreetsApiKey?rockInstanceId={rockInstanceId}" );

                    // If debugging locally
                    // var getAPIKeyClient = new RestClient( $"http://localhost:57822/api/SmartyStreets/GetSmartyStreetsApiKey?rockInstanceId={rockInstanceId}" );

                    var getApiKeyRequest = new RestRequest( Method.GET );
                    var getApiKeyResponse = getAPIKeyClient.Get<SmartyStreetsAPIKey>( getApiKeyRequest );

                    if ( getApiKeyResponse.StatusCode == HttpStatusCode.OK )
                    {
                        SmartyStreetsAPIKey managedKey = getApiKeyResponse.Data;
                        if ( managedKey.AuthID != null && managedKey.AuthToken != null )
                        {
                            Rock.Web.SystemSettings.SetValue( "core_SmartyStreetsApiKeyLastUpdate", RockDateTime.Now.ToString( "o" ) );
                            Rock.Web.SystemSettings.SetValue( "core_SmartyStreetsAuthID", Rock.Security.Encryption.EncryptString( managedKey.AuthID ) );
                            Rock.Web.SystemSettings.SetValue( "core_SmartyStreetsAuthToken", Rock.Security.Encryption.EncryptString( managedKey.AuthToken ) );
                        }
                    }
                }

                string encryptedAuthID = Rock.Web.SystemSettings.GetValue( "core_SmartyStreetsAuthID" );
                string encryptedAuthToken = Rock.Web.SystemSettings.GetValue( "core_SmartyStreetsAuthToken" );

                apiKey = new SmartyStreetsAPIKey();
                apiKey.AuthID = Rock.Security.Encryption.DecryptString( encryptedAuthID );
                apiKey.AuthToken = Rock.Security.Encryption.DecryptString( encryptedAuthToken );
            }

            if ( apiKey == null || apiKey.AuthID == null || apiKey.AuthToken == null )
            {
                apiKey = new SmartyStreetsAPIKey();
                apiKey.AuthID = GetAttributeValue( "AuthID" );
                apiKey.AuthToken = GetAttributeValue( "AuthToken" );
            }

            return apiKey;
        }

#pragma warning disable

        public class CandidateAddress
        {
            public int input_index { get; set; }
            public int candidate_index { get; set; }
            public string delivery_line_1 { get; set; }
            public string delivery_line_2 { get; set; }
            public string last_line { get; set; }
            public string delivery_point_barcode { get; set; }
            public Components components { get; set; }
            public Metadata metadata { get; set; }
            public Analysis analysis { get; set; }
        }

        public class Components
        {
            public string primary_number { get; set; }
            public string street_name { get; set; }
            public string street_suffix { get; set; }
            public string city_name { get; set; }
            public string state_abbreviation { get; set; }
            public string zipcode { get; set; }
            public string plus4_code { get; set; }
            public string delivery_point { get; set; }
            public string delivery_point_check_digit { get; set; }
        }

        public class Metadata
        {
            public string record_type { get; set; }
            public string county_fips { get; set; }
            public string county_name { get; set; }
            public string carrier_route { get; set; }
            public string congressional_district { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string precision { get; set; }
        }

        public class Analysis
        {
            public string dpv_match_code { get; set; }
            public string dpv_footnotes { get; set; }
            public string dpv_cmra { get; set; }
            public string dpv_vacant { get; set; }
            public bool ews_match { get; set; }
            public string footnotes { get; set; }
        }

        public class Zipcode
        {
            public string zipcode { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class LookupResponse
        {
            public List<Zipcode> zipcodes { get; set; }
        }
#pragma warning restore

    }

    /// <summary>
    /// The Smarty Streets API Key information 
    /// This is a key maintained by Spark that is available for common use
    /// </summary>
    public class SmartyStreetsAPIKey
    {
        /// <summary>
        /// Gets or sets the authentication identifier.
        /// </summary>
        /// <value>
        /// The authentication identifier.
        /// </value>
        public string AuthID { get; set; }

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        public string AuthToken { get; set; }
    }
}