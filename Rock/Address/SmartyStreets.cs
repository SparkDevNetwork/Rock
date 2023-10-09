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
using System.Dynamic;
using System.Linq;
using System.Net;

using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Address
{
    /// <summary>
    /// The standardization/geocoding service from SmartyStreets.
    /// </summary>
    [Description( "Address verification service from SmartyStreets" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Smarty Streets" )]

    #region Attributes

    [BooleanField( "Use Managed API Key",
        Key = AttributeKey.UseManagedAPIKey,
        Description = "Enable this to use the Auth ID, Auth Token and License Value that is managed by Spark.",
        DefaultBooleanValue = true,
        Order = 1,
        IsRequired = false )]

    [TextField( "Auth ID",
        Key = AttributeKey.AuthID,
        Description = "The Smarty Streets Authorization ID. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.",
        DefaultValue = "",
        Order = 2,
        IsRequired = false )]

    [TextField( "Auth Token",
        Key = AttributeKey.AuthToken,
        Description = "The Smarty Streets Authorization Token. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.",
        DefaultValue = "",
        Order = 3,
        IsRequired = false )]

    [TextField( "License Value",
        Key = AttributeKey.LicenseValue,
        Description = "The Smarty Streets License Value. NOTE: This can be left blank and will be ignored if 'Use Managed API Key' is enabled.",
        DefaultValue = "",
        Order = 4,
        IsRequired = false )]

    [TextField( "Acceptable DPV Codes",
        Key = AttributeKey.AcceptableDPVCodes,
        Description = "The Smarty Streets Delivery Point Validation (DPV) match code values that are considered acceptable levels of standardization (see https://www.smarty.com/docs/cloud/us-street-api#analysis for details).",
        DefaultValue = "Y,S,D",
        Order = 5,
        IsRequired = false )]

    [TextField( "Acceptable Precisions",
        Key = AttributeKey.AcceptablePrecisions,
        Description = "The Smarty Streets latitude & longitude precision values that are considered acceptable levels of geocoding (see https://www.smarty.com/docs/cloud/us-street-api#metadata for details).",
        DefaultValue = "Zip7,Zip8,Zip9",
        Order = 6,
        IsRequired = false )]

    #endregion Attributes

    public class SmartyStreets : VerificationComponent
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            public const string UseManagedAPIKey = "UseManagedAPIKey";
            public const string AuthID = "AuthID";
            public const string AuthToken = "AuthToken";
            public const string LicenseValue = "LicenseValue";
            public const string AcceptableDPVCodes = "AcceptableDPVCodes";
            public const string AcceptablePrecisions = "AcceptablePrecisions";
        }

        private static class SystemSettingsKey
        {
            public const string SmartyStreetsApiKeyLastUpdate = "core_SmartyStreetsApiKeyLastUpdate";
            public const string SmartyStreetsAuthID = "core_SmartyStreetsAuthID";
            public const string SmartyStreetsAuthToken = "core_SmartyStreetsAuthToken";
            public const string SmartyStreetsLicenseValue = "core_SmartyStreetsLicenseValue";
        }

        private static class LicenseValue
        {
            public const string USCoreCloud = "us-core-cloud";
            public const string USRooftopGeocodingCloud = "us-rooftop-geocoding-cloud";
        }

        #endregion Keys

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

            var dpvCodes = GetAttributeValue( AttributeKey.AcceptableDPVCodes ).SplitDelimitedValues();
            var precisions = GetAttributeValue( AttributeKey.AcceptablePrecisions ).SplitDelimitedValues();

            dynamic address = new ExpandoObject();
            address.addressee = location.Name;
            address.street = location.Street1;
            address.street2 = location.Street2;
            address.city = location.City;
            address.state = location.State;
            address.zipcode = location.PostalCode;
            address.candidates = 1;

            var licenseParts = apiKey.LicenseValue?.ToLower().SplitDelimitedValues( "," );
            if ( licenseParts?.Any( p => p == LicenseValue.USCoreCloud || p == LicenseValue.USRooftopGeocodingCloud ) == true )
            {
                address.match = "enhanced";
            }

            var payload = new[] { address };

            var client = new RestClient( $"https://us-street.api.smarty.com/street-address?auth-id={apiKey.AuthID}&auth-token={apiKey.AuthToken}&license={apiKey.LicenseValue}" );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( "Accept", "application/json" );
            request.AddJsonBody( payload );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var candidates = JsonConvert.DeserializeObject( response.Content, typeof( List<CandidateAddress> ) ) as List<CandidateAddress>;
                if ( candidates.Any() )
                {
                    var candidate = candidates.FirstOrDefault();
                    resultMsg = $"RecordType:{candidate.metadata.record_type}; DPV MatchCode:{candidate.analysis.dpv_match_code}; Precision:{candidate.metadata.precision}";

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
            return GetZipCodeLocation( new[] { new { zipcode = postalCode } }, out resultMsg );
        }

        /// <summary>
        /// Gets the location from city and state combination.
        /// </summary>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        public MapCoordinate GetLocationFromCityState( string city, string state, out string resultMsg )
        {
            return GetZipCodeLocation( new[] { new { city, state } }, out resultMsg );
        }

        private SmartyStreetsAPIKey GetAPIKey()
        {
            SmartyStreetsAPIKey apiKey = null;
            if ( GetAttributeValue( AttributeKey.UseManagedAPIKey ).AsBooleanOrNull() ?? true )
            {
                var lastKeyUpdate = Rock.Web.SystemSettings.GetValue( SystemSettingsKey.SmartyStreetsApiKeyLastUpdate ).AsDateTime() ?? DateTime.MinValue;
                var hoursSinceLastUpdate = ( RockDateTime.Now - lastKeyUpdate ).TotalHours;
                if ( hoursSinceLastUpdate > 24 )
                {
                    var rockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
                    var getAPIKeyClient = new RestClient( $"https://www.rockrms.com/api/SmartyStreets/GetSmartyStreetsApiKey?rockInstanceId={rockInstanceId}" );

                    // If debugging locally
                    // var getAPIKeyClient = new RestClient( $"http://localhost:57822/api/SmartyStreets/GetSmartyStreetsApiKey?rockInstanceId={rockInstanceId}" );

                    var getApiKeyRequest = new RestRequest( Method.GET );
                    var getApiKeyResponse = getAPIKeyClient.Get<SmartyStreetsAPIKey>( getApiKeyRequest );

                    if ( getApiKeyResponse.StatusCode == HttpStatusCode.OK )
                    {
                        SmartyStreetsAPIKey managedKey = getApiKeyResponse.Data;
                        if ( managedKey.AuthID.IsNotNullOrWhiteSpace() && managedKey.AuthToken.IsNotNullOrWhiteSpace() )
                        {
                            Rock.Web.SystemSettings.SetValue( SystemSettingsKey.SmartyStreetsApiKeyLastUpdate, RockDateTime.Now.ToString( "o" ) );
                            Rock.Web.SystemSettings.SetValue( SystemSettingsKey.SmartyStreetsAuthID, Rock.Security.Encryption.EncryptString( managedKey.AuthID ) );
                            Rock.Web.SystemSettings.SetValue( SystemSettingsKey.SmartyStreetsAuthToken, Rock.Security.Encryption.EncryptString( managedKey.AuthToken ) );
                            Rock.Web.SystemSettings.SetValue( SystemSettingsKey.SmartyStreetsLicenseValue, Rock.Security.Encryption.EncryptString( managedKey.LicenseValue ) );
                        }
                    }
                }

                var encryptedAuthID = Rock.Web.SystemSettings.GetValue( SystemSettingsKey.SmartyStreetsAuthID );
                var encryptedAuthToken = Rock.Web.SystemSettings.GetValue( SystemSettingsKey.SmartyStreetsAuthToken );
                var licenseValue = Rock.Web.SystemSettings.GetValue( SystemSettingsKey.SmartyStreetsLicenseValue );

                apiKey = new SmartyStreetsAPIKey();
                apiKey.AuthID = Rock.Security.Encryption.DecryptString( encryptedAuthID );
                apiKey.AuthToken = Rock.Security.Encryption.DecryptString( encryptedAuthToken );
                apiKey.LicenseValue = Rock.Security.Encryption.DecryptString( licenseValue );
            }

            if ( apiKey == null || apiKey.AuthID.IsNullOrWhiteSpace() || apiKey.AuthToken.IsNullOrWhiteSpace() )
            {
                apiKey = new SmartyStreetsAPIKey();
                apiKey.AuthID = GetAttributeValue( AttributeKey.AuthID );
                apiKey.AuthToken = GetAttributeValue( AttributeKey.AuthToken );
                apiKey.LicenseValue = GetAttributeValue( AttributeKey.LicenseValue );
            }

            if ( apiKey.LicenseValue.IsNullOrWhiteSpace() )
            {
                apiKey.LicenseValue = LicenseValue.USCoreCloud;
            }

            return apiKey;
        }

        private MapCoordinate GetZipCodeLocation( dynamic payload, out string resultMsg )
        {
            MapCoordinate result = null;
            resultMsg = string.Empty;

            if ( this.IsActive )
            {

                SmartyStreetsAPIKey apiKey = GetAPIKey();

                var client = new RestClient( string.Format( "https://us-zipcode.api.smartystreets.com/lookup?auth-id={0}&auth-token={1}", apiKey.AuthID, apiKey.AuthToken ) );
                var request = new RestRequest( Method.POST );
                request.RequestFormat = DataFormat.Json;
                request.AddHeader( "Accept", "application/json" );
                request.AddJsonBody( payload );
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
            public string enhanced_match { get; set; }
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
    /// The Smarty Streets API Key information.
    /// This is a key maintained by Spark that is available for common use.
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

        /// <summary>
        /// Gets or sets the license value.
        /// </summary>
        /// <value>
        /// The license value.
        /// </value>
        public string LicenseValue { get; set; }
    }
}