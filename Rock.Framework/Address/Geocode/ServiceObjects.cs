using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Framework.ServiceObjects.GeoCoder;

namespace Rock.Address.Geocode
{
    [Description("Service Objects Geocoding Service")]
    [Export( typeof( GeocodeService ) )]
    [ExportMetadata( "ServiceName", "ServiceObjects" )]
    [Rock.Attribute.Property( 2, "License Key", "The Service Objects License Key" )]
    public class ServiceObjects : GeocodeService
    {
        public override bool Geocode( Rock.Models.Crm.Address address, out string result )
        {
            if ( address != null )
            {
                string licenseKey = AttributeValues["LicenseKey"].Value;

                var client = new DOTSGeoCoderSoapClient();
                Location_V3 location = client.GetBestMatch_V3(
                    string.Format("{0} {1}",
                        address.Street1,
                        address.Street2),
                    address.City,
                    address.State,
                    address.Zip,
                    licenseKey );

                result = location.Level;

                if ( location.Level == "S" || location.Level == "P" )
                {
                    address.Latitude = double.Parse( location.Latitude );
                    address.Longitude = double.Parse( location.Longitude );

                    return true;
                }
            }
            else
                result = "Null Address";

            return false;
        }
    }
}