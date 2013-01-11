//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Attribute;
using Rock.StrikeIron.USAddressVerification;
using Rock.Web.UI;

namespace Rock.Address.Standardize
{
    /// <summary>
    /// The USAddressVerification service from <a href="http://www.strikeiron.com/Home.aspx">StrikeIron</a>
    /// </summary>
    [Description( "Address Standardization and Geocoding service from StrikeIron" )]
    [Export( typeof( StandardizeComponent ) )]
    [ExportMetadata( "ComponentName", "StrikeIron" )]
    [TextField( 1, "User ID", "Security", "The Strike Iron User ID", true, "" )]
    [TextField( 2, "Password", "Security", "The Strike Iron Password", true, "" )]
    public class StrikeIron : StandardizeComponent
    {
        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <remarks>
        /// The StrikeIron address verification will also attempt to geocode the address.  If this 
        /// geocode is succesful, the Geocode information of the address will be updated also.
        /// </remarks>
        /// <param name="location">The location.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        /// True/False value of whether the address was standardized was succesfully
        /// </returns>
        public override bool Standardize( Rock.Model.Location location, out string result )
        {
            if ( location != null )
            {
                var registeredUser = new RegisteredUser();
                registeredUser.UserID = GetAttributeValue("UserID");
                registeredUser.Password = GetAttributeValue("Password");

                var licenseInfo = new LicenseInfo();
                licenseInfo.RegisteredUser = registeredUser;

                var client = new USAddressVerificationSoapClient();

                SIWsOutputOfUSAddress verifyResult;
                SubscriptionInfo info = client.VerifyAddressUSA(
                    licenseInfo,
                    location.Street1,
                    location.Street2,
                    string.Format("{0} {1} {2}", 
                        location.City,
                        location.State,
                        location.Zip),
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
                            location.Street1 = usAddress.AddressLine1;
                            location.Street2 = usAddress.AddressLine2;
                            location.City = usAddress.City;
                            location.State = usAddress.State;
                            location.Zip = usAddress.ZIPPlus4;

                            if ( usAddress.GeoCode != null )
                            {
                                location.GeocodeAttemptedServiceType = "StrikeIron";
                                location.GeocodeAttemptedResult = "200";
                                location.GeocodedDateTime = DateTime.Now;

                                location.SetLocationPointFromLatLong( usAddress.GeoCode.Latitude, usAddress.GeoCode.Longitude );
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