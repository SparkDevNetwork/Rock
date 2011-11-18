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
    // TODO: Remove hardcoded attribute defaults once UI is created for setting values
    [Rock.Attribute.Property( "Order" )]
    [Rock.Attribute.Property( "License Key", "The Service Objects License Key", "" )]
    public class ServiceObjects : IGeocodeService
    {
        public int Id { get { return 0; } }
        public List<Models.Core.Attribute> Attributes { get; set; }
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

        public ServiceObjects()
        {
            // TODO: next line should be moved to Job creation UI, when it's created
            Rock.Attribute.Helper.CreateAttributes( this.GetType(), "Rock.Api.Crm.Address.ServiceObjects", string.Empty, string.Empty, null );
            Rock.Attribute.Helper.LoadAttributes( this );
        }

        public bool Geocode( AddressStub address )
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