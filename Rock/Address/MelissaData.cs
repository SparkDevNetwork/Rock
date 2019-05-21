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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.MelissaData.AddressCheck;

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
        /// Gets a value indicating whether Melissa Data supports geocoding.
        /// </summary>
        public override bool SupportsGeocoding
        {
            get { return false; }
        }

        /// <summary>
        /// Standardizes the address
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns>
        /// True/False value of whether the address was standardized successfully
        /// </returns>
        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            VerificationResult result = VerificationResult.None;
            resultMsg = string.Empty;

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
                resultMsg = responseArray.Record[0].Results;
                if ( string.IsNullOrWhiteSpace( resultMsg ) )
                {
                    resultMsg = responseArray.Results;
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

                    if ( location.Street2.Trim() == string.Empty && responseAddress.Suite.Trim() != string.Empty )
                    {
                        location.Street2 = responseAddress.Suite;
                    }

                    result = VerificationResult.Standardized;
                }
            }
            else
            {
                result = VerificationResult.ConnectionError;
                resultMsg = "No Records Returned";
            }

            return result;
        }
    }
}