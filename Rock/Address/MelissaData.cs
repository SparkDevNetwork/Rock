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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Attribute;
using Rock.MelissaData.AddressCheck;
using Rock.Web.UI;

namespace Rock.Address
{
    /// <summary>
    /// The AddressCheck service from <a href="http://www.melissadata.com/">Melissa Data</a>
    /// </summary>
    [Description( "Address Standardization service from Melissa Data" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "MelissaData" )]
    [TextField( "Customer Id", "The Melissa Data Customer ID", true, "", "Security" )]
    public class MelissaData : VerificationComponent
    {
        /// <summary>
        /// Standardizes the address
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="reVerify">Should location be reverified even if it has already been succesfully verified</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public override bool VerifyLocation( Rock.Model.Location location, bool reVerify, out string result )
        {
            result = string.Empty;

            // Only verify if location is valid, has not been locked, and 
            // has either never been attempted or last attempt was in last 30 secs (prev active service failed) or reverifying
            if ( location != null && 
                !(location.IsGeoPointLocked ?? false) &&  
                (
                    !location.StandardizedDateTime.HasValue || 
                    location.StandardizedDateTime.Value.CompareTo( RockDateTime.Now.AddSeconds(-30) ) > 0 ||
                    reVerify
                ) )
            {
                var requestArray = new RequestArray();
                requestArray.CustomerID = GetAttributeValue( "CustomerId" );
                requestArray.OptAddressParsed = "True";

                RequestArrayRecord requestAddress = new RequestArrayRecord();
                requestAddress.AddressLine1 = location.Street1;
                requestAddress.AddressLine2 = location.Street2;
                requestAddress.City = location.City;
                requestAddress.State = location.State;
                requestAddress.Zip = location.PostalCode;
                requestAddress.Country = location.Country;
                requestAddress.RecordID = "1";

                requestArray.Record = new RequestArrayRecord[1];
                requestArray.Record[0] = requestAddress;

                ServiceClient serviceClient = new ServiceClient();
                var responseArray = serviceClient.doAddressCheck( requestArray );

                if ( responseArray.TotalRecords == "1" && responseArray.Record[0].RecordID == "1" )
                {
                    result = responseArray.Record[0].Results;
                    if ( string.IsNullOrWhiteSpace( result ) )
                    {
                        result = responseArray.Results;
                    }

                    string[] validResultCodes = new string[] {
                        "AS01", // Address Fully Verified
                        "AS09"  // Foreign Address
                    };

                    if ( validResultCodes.Any( a => responseArray.Record[0].Results.Contains( a ) ) )
                    {
                        bool foreignAddress = responseArray.Record[0].Results.Contains("AS09");
                        ResponseArrayRecordAddress responseAddress = responseArray.Record[0].Address;
                        location.Street1 = responseAddress.Address1;
                        location.Street2 = responseAddress.Address2;
                        location.City = responseAddress.City.Name;
                        if ( !foreignAddress || !string.IsNullOrWhiteSpace(responseAddress.State.Abbreviation) )
                        {
                            // only set the State if we got a AS01 or a State back
                            location.State = responseAddress.State.Abbreviation;
                        }

                        if ( !foreignAddress || !string.IsNullOrWhiteSpace( responseAddress.Zip ) )
                        {
                            // only set the PostalCode if we got a AS01 or a Zip back
                            location.PostalCode = responseAddress.Zip;
                            if (!string.IsNullOrWhiteSpace(requestAddress.Plus4))
                            {
                                location.PostalCode = responseAddress.Zip + '-' + responseAddress.Plus4;
                            }
                            else
                            {
                                location.PostalCode = responseAddress.Zip;
                            }
                        }

                        if ( location.Street2.Trim() == string.Empty &&
                            responseAddress.Suite.Trim() != string.Empty )
                            location.Street2 = responseAddress.Suite;

                        location.StandardizedDateTime = RockDateTime.Now;
                    }
                }
                else
                {
                    result = "No Records Returned";
                }

                location.StandardizeAttemptedServiceType = "MelissaData";
                location.StandardizeAttemptedDateTime = RockDateTime.Now;
                location.StandardizeAttemptedResult = result;
            }

            // MelissaData only standardizes addresses, it does not geocode, therefore
            // always return false so that next verifcation service will run
            return false;
        }
    }
}