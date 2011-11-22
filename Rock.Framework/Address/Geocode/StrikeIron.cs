using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.StrikeIron.USAddressVerification;

namespace Rock.Address.Geocode
{
    [Description( "Address Standardization and Geocoding service from StrikeIron" )]
    [Export( typeof( GeocodeService ) )]
    [ExportMetadata( "ServiceName", "StrikeIron" )]
    [Rock.Attribute.Property( 1, "User ID", "The Strike Iron User ID", "" )]
    [Rock.Attribute.Property( 2, "Password", "The Strike Iron Password", "" )]
    public class StrikeIron : GeocodeService
    {
        public override bool Geocode( Rock.Models.Crm.Address address )
        {
            if ( address != null )
            {
                var registeredUser = new RegisteredUser();
                registeredUser.UserID = AttributeValues["UserID"].Value;
                registeredUser.Password = AttributeValues["Password"].Value;

                var licenseInfo = new LicenseInfo();
                licenseInfo.RegisteredUser = registeredUser;

                var client = new USAddressVerificationSoapClient();

                SIWsOutputOfUSAddress result;
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
                    out result );

                if (result != null)
                {
                    if ( result.ServiceStatus.StatusNbr == 200 )
                    {
                        USAddress usAddress = result.ServiceResult;

                        if ( usAddress != null && usAddress.GeoCode != null )
                        {
                            address.GeocodeService = "StrikeIron";
                            address.GeocodeResult = Models.Crm.GeocodeResult.Exact;

                            address.Latitude = usAddress.GeoCode.Latitude;
                            address.Longitude = usAddress.GeoCode.Longitude;

                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }
    }
}