using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.StrikeIron.USAddressVerification;

namespace Rock.Address.Standardize
{
    /// <summary>
    /// The USAddressVerification service from <a href="http://www.strikeiron.com/Home.aspx">StrikeIron</a>
    /// </summary>
    [Description( "Address Standardization and Geocoding service from StrikeIron" )]
    [Export( typeof( StandardizeService ) )]
    [ExportMetadata( "ServiceName", "StrikeIron" )]
    [Rock.Attribute.Property( 1, "User ID", "The Strike Iron User ID", "" )]
    [Rock.Attribute.Property( 2, "Password", "The Strike Iron Password", "" )]
    public class StrikeIron : StandardizeService
    {
        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <remarks>
        /// The StrikeIron address verification will also attempt to geocode the address.  If this 
        /// geocode is succesful, the Geocode information of the address will be updated also.
        /// </remarks>
        /// <param name="address">The address.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        /// True/False value of whether the address was standardized was succesfully
        /// </returns>
        public override bool Standardize( Rock.Models.Crm.Address address, out string result )
        {
            if ( address != null )
            {
                var registeredUser = new RegisteredUser();
                registeredUser.UserID = AttributeValues["UserID"].Value;
                registeredUser.Password = AttributeValues["Password"].Value;

                var licenseInfo = new LicenseInfo();
                licenseInfo.RegisteredUser = registeredUser;

                var client = new USAddressVerificationSoapClient();

                SIWsOutputOfUSAddress verifyResult;
                SubscriptionInfo info = client.VerifyAddressUSA(
                    licenseInfo,
                    address.Street1,
                    address.Street2,
                    string.Format("{0} {1} {2}", 
                        address.City,
                        address.State,
                        address.Zip),
                    string.Empty,
                    string.Empty,
                    CasingEnum.PROPER,
                    out verifyResult );

                if (verifyResult != null)
                {
                    result = verifyResult.ServiceStatus.StatusNbr.ToString();

                    if ( verifyResult.ServiceStatus.StatusNbr == 200 )
                    {
                        USAddress usAddress = verifyResult.ServiceResult;

                        if ( usAddress != null && usAddress.GeoCode != null )
                        {
                            address.Street1 = usAddress.AddressLine1;
                            address.Street2 = usAddress.AddressLine2;
                            address.City = usAddress.City;
                            address.State = usAddress.State;
                            address.Zip = usAddress.ZIPPlus4;

                            if ( usAddress.GeoCode != null )
                            {
                                address.GeocodeService = "StrikeIron";
                                address.GeocodeResult = "200";
                                address.GeocodeDate = DateTime.Now;

                                address.Latitude = usAddress.GeoCode.Latitude;
                                address.Longitude = usAddress.GeoCode.Longitude;
                            }

                            return true;
                        }
                    }
                }
                else
                    result = "Null Result";
            }
            else
                result = "Null Address";

            return false;
        }
    }
}