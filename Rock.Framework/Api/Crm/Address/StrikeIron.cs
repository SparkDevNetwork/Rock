using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.StrikeIron.USAddressVerification;

namespace Rock.Api.Crm.Address
{
    [Description( "Address Standardization and Geocoding service from StrikeIron" )]
    [Export( typeof( IGeocodeService ) )]
    [ExportMetadata( "ServiceName", "StrikeIron" )]
    [Rock.Attribute.Property( 0, "Order", "The order that this service should be used (priority)" )]
    [Rock.Attribute.Property( 1, "Active", "Active", "Should Service be used?", "True", "Rock.Framework", "Rock.FieldTypes.Boolean" )]
    [Rock.Attribute.Property( 2, "User ID", "The Strike Iron User ID", "" )]
    [Rock.Attribute.Property( 3, "Password", "The Strike Iron Password", "" )]
    public class StrikeIron : IGeocodeService
    {
        public int Id { get { return 0; } }
        public List<Rock.Cms.Cached.Attribute> Attributes { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        public int Order 
        { 
            get 
            { 
                int order = 0;
                if (AttributeValues.ContainsKey("Order"))
                    if (!(Int32.TryParse(AttributeValues["Order"].Value, out order)))
                        order = 0;
                return order;
            } 
        }

        public StrikeIron()
        {
            // TODO: next line should be moved to Job creation UI, when it's created
            Rock.Attribute.Helper.CreateAttributes( this.GetType(), "Rock.Api.Crm.Address.StrikeIron", string.Empty, string.Empty, null );
            Rock.Attribute.Helper.LoadAttributes( this );
        }

        public bool Geocode( AddressStub address )
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