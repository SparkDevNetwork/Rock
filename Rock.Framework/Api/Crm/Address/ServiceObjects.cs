using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.ServiceObjects.GeoCoder;

namespace Rock.Api.Crm.Address
{
    [Export( typeof( IGeocodeService ) )]
    [ExportMetadata( "ServiceName", "ServiceObjects" )]
    public class ServiceObjects : IGeocodeService
    {
        // TODO: Need to abstract a way to set these property 
        public int Order { get { return 1; } }

        [System.ComponentModel.Description("License Key")]
        public string LicenseKey { get { return "WS34-YEW2-KGL3"; } }

        public bool Geocode( AddressStub address )
        {
            if ( address != null )
            {
                var client = new DOTSGeoCoderSoapClient();
                Location_V3 location = client.GetBestMatch_V3(
                    string.Format("{0} {1}",
                        address.Street1,
                        address.Street2),
                    address.City,
                    address.State,
                    address.Zip,
                    LicenseKey );

                if ( location.Level == "S" || location.Level == "P" )
                {
                    address.Service = "ServiceObjects";
                    address.ResultCode = "Exact";

                    address.Latitude = double.Parse( location.Latitude );
                    address.Longitude = double.Parse( location.Longitude );
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}