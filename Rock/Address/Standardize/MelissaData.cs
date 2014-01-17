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
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Attribute;
using Rock.MelissaData.AddressCheck;
using Rock.Web.UI;

namespace Rock.Address.Standardize
{
    /// <summary>
    /// The AddressCheck service from <a href="http://www.melissadata.com/">Melissa Data</a>
    /// </summary>
    [Description( "Address Standardization service from Melissa Data" )]
    [Export( typeof( StandardizeComponent ) )]
    [ExportMetadata( "ComponentName", "MelissaData" )]
    [TextField( "Customer Id", "The Melissa Data Customer ID", true, "", "Security" )]
    public class MelissaData : StandardizeComponent
    {
        /// <summary>
        /// Standardizes the address
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="result">The AddressCheck result code</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public override bool Standardize( Rock.Model.Location location, out string result )
        {
            if ( location != null )
            {
                var requestArray = new RequestArray();
                requestArray.CustomerID = GetAttributeValue("CustomerId");
                requestArray.OptAddressParsed = "True";

                RequestArrayRecord requestAddress = new RequestArrayRecord();
                requestAddress.AddressLine1 = location.Street1;
                requestAddress.AddressLine2 = location.Street2;
                requestAddress.City = location.City;
                requestAddress.State = location.State;
                requestAddress.Zip = location.Zip;
                requestAddress.RecordID = "1";

                requestArray.Record = new RequestArrayRecord[1];
                requestArray.Record[0] = requestAddress;

                ServiceClient serviceClient = new ServiceClient();
                var responseArray = serviceClient.doAddressCheck( requestArray );

                if ( responseArray.TotalRecords == "1" && responseArray.Record[0].RecordID == "1" )
                {
                    result = responseArray.Record[0].Results;

                    if ( responseArray.Record[0].Results.Contains( "AS01" ) )
                    {
                        ResponseArrayRecordAddress responseAddress = responseArray.Record[0].Address;
                        location.Street1 = responseAddress.Address1;
                        location.Street2 = responseAddress.Address2;
                        location.City = responseAddress.City.Name;
                        location.State = responseAddress.State.Abbreviation;
                        location.Zip = responseAddress.Zip + '-' + responseAddress.Plus4;
                        if ( location.Street2.Trim() == string.Empty &&
                            responseAddress.Suite.Trim() != string.Empty )
                            location.Street2 = responseAddress.Suite;

                        return true;
                    }
                }
                else
                    result = "No Records Returned";
            }
            else
                result = "Null Address";

            return false;
        }
    }
}