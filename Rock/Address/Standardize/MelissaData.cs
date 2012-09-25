//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.MelissaData.AddressCheck;

namespace Rock.Address.Standardize
{
    /// <summary>
    /// The AddressCheck service from <a href="http://www.melissadata.com/">Melissa Data</a>
    /// </summary>
    [Description( "Address Standardization service from Melissa Data" )]
    [Export( typeof( StandardizeComponent ) )]
    [ExportMetadata( "ComponentName", "MelissaData" )]
    [Rock.Attribute.Property( 1, "Customer Id", "Security", "The Melissa Data Customer ID", true, "" )]
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
        public override bool Standardize( Rock.Crm.Location location, out string result )
        {
            if ( location != null )
            {
                var requestArray = new RequestArray();
                requestArray.CustomerID = AttributeValue("CustomerId");
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