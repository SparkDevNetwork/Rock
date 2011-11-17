using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.StrikeIron.USAddressVerification;

namespace Rock.Api.Crm.Address
{
    [Export( typeof( IGeocodeService ) )]
    [ExportMetadata( "ServiceName", "StrikeIron" )]
    public class StrikeIron : IGeocodeService
    {
        // TODO: Need to abstract a way to set these properties
        public int Order { get { return 0; } }

        [System.ComponentModel.Description("User ID")]
        public string UserID { get { return "CD2548164B6BC1B2530C"; } }

        [System.ComponentModel.Description( "Password" )]
        public string Password { get { return "ArenaSIService"; } }

        public bool Geocode( AddressStub address )
        {
            if ( address != null )
            {
                var registeredUser = new RegisteredUser();
                registeredUser.UserID = this.UserID;
                registeredUser.Password = this.Password;

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
                            address.Service = "StrikeIron";
                            address.ResultCode = "Exact";

                            address.Street1 = usAddress.AddressLine1;
                            address.Street2 = usAddress.AddressLine2;
                            address.City = usAddress.City;
                            address.State = usAddress.State;
                            address.Zip = usAddress.ZIPPlus4;

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