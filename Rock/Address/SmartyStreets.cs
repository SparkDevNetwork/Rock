// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Web;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using RestSharp;
using Newtonsoft.Json;

namespace Rock.Address
{
    /// <summary>
    /// The standardization/geocoding service from <a href="http://dev.virtualearth.net">Bing</a>
    /// </summary>
    [Description( "Address verification service from SmartyStreets" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Smarty Streets" )]
    [TextField( "Auth ID", "The Smarty Streets Authorization ID", true, "", "", 2 )]
    [TextField( "Auth Token", "The Smarty Streets Authorization Token", true, "", "", 3 )]
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
        /// True/False value of whether the verification was successfull or not
        /// </returns>
        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            VerificationResult result = VerificationResult.None;
            resultMsg = string.Empty;

            string authId = GetAttributeValue( "AuthID" );
            string authToken = GetAttributeValue( "AuthToken" );
            var dpvCodes = GetAttributeValue("AcceptableDPVCodes").SplitDelimitedValues();
            var precisions = GetAttributeValue("AcceptablePrecisions").SplitDelimitedValues();

            var payload = new[] { new { addressee = location.Name, street = location.Street1, street2 = location.Street2, city = location.City, state = location.State, zipcode = location.PostalCode, candidates = 1 } };

            var client = new RestClient( string.Format( "https://api.smartystreets.com/street-address?auth-id={0}&auth-token={1}", authId, authToken ) );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( "Accept", "application/json" );
            request.AddBody( payload );
            var response = client.Execute( request );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var candidates = JsonConvert.DeserializeObject( response.Content, typeof( List<CandidateAddress> ) ) as List<CandidateAddress>;
                if (candidates.Any())
                {
                    var candidate = candidates.FirstOrDefault();
                    resultMsg = string.Format( "record_type: {0}; dpv_match_code: {1}; precision {2}",
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
                        result = result | VerificationResult.Standardized;
                    }

                    location.GeocodeAttemptedResult = candidate.metadata.precision;
                    if ( precisions.Contains( candidate.metadata.precision ) )
                    {
                        location.SetLocationPointFromLatLong( candidate.metadata.latitude, candidate.metadata.longitude );
                        result = result | VerificationResult.Geocoded;
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
#pragma warning restore
    
    }
}