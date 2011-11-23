using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.MelissaData.Service;

namespace Rock.Address.Standardize
{
    [Description( "Address Standardization service from Melissa Data" )]
    [Export( typeof( StandardizeService ) )]
    [ExportMetadata( "ServiceName", "MelissaData" )]
    [Rock.Attribute.Property( 1, "Customer Id", "The Melissa Data Customer ID", "" )]
    public class MelissaData : StandardizeService
    {
        public override bool Standardize( Rock.Models.Crm.Address address, out string result )
        {
            if ( address != null )
            {
                var requestArray = new RequestArray();
                requestArray.CustomerID = AttributeValues["CustomerId"].Value;
                requestArray.OptAddressParsed = "True";

                RequestArrayRecord requestAddress = new RequestArrayRecord();
                requestAddress.AddressLine1 = address.Street1;
                requestAddress.AddressLine2 = address.Street2;
                requestAddress.City = address.City;
                requestAddress.State = address.State;
                requestAddress.Zip = address.Zip;
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
                        address.Street1 = responseAddress.Address1;
                        address.Street2 = responseAddress.Address2;
                        address.City = responseAddress.City.Name;
                        address.State = responseAddress.State.Abbreviation;
                        address.Zip = responseAddress.Zip + '-' + responseAddress.Plus4;
                        if ( address.Street2 == string.Empty &&
                            responseAddress.Suite != string.Empty )
                            address.Street2 = responseAddress.Suite;

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