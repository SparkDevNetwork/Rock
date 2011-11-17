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
    // TODO: Remove hardcoded attribute defaults once UI is created for setting values
    [Rock.Attribute.Property( "User ID", "The Strike Iron User ID", "" )]
    [Rock.Attribute.Property( "Password", "The Strike Iron Password", "" )]
    public class StrikeIron : IGeocodeService, Rock.Attribute.IHasAttributes
    {
        public int Id { get { return 0; } }
        public List<Models.Core.Attribute> Attributes { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        // TODO: Need to abstract a way to set these properties
        public int Order { get { return 1; } }

        public bool Geocode( AddressStub address )
        {
            if ( address != null )
            {
                // TODO: next line should be moved to Job creation UI, when it's created
                Rock.Attribute.Helper.CreateAttributes( this.GetType(), "Rock.Api.Crm.Address.StrikeIron", string.Empty, string.Empty, null );

                Rock.Attribute.Helper.LoadAttributes( this );

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