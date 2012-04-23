//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.StrikeIron.USAddressVerification;

namespace Rock.Address.Geocode
{
    /// <summary>
    /// The USAddressVerification service from <a href="http://www.strikeiron.com/Home.aspx">StrikeIron</a>
    /// </summary>
    [Description( "Address Standardization and Geocoding service from StrikeIron" )]
    [Export( typeof( GeocodeComponent ) )]
    [ExportMetadata( "ComponentName", "StrikeIron" )]
    [Rock.Attribute.Property( 1, "User ID", "Security", "The Strike Iron User ID", true, "" )]
    [Rock.Attribute.Property( 2, "Password", "Security", "The Strike Iron Password", true, "" )]
    public class StrikeIron : GeocodeComponent
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        /// True/False value of whether the address was standardized was succesfully
        /// </returns>
        public override bool Geocode( Rock.CRM.Address address, out string result )
        {
            if ( address != null )
            {
                var registeredUser = new RegisteredUser();
                registeredUser.UserID = AttributeValue("UserID");
                registeredUser.Password = AttributeValue("Password");

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
                            address.Latitude = usAddress.GeoCode.Latitude;
                            address.Longitude = usAddress.GeoCode.Longitude;

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