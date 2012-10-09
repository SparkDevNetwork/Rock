//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.ServiceObjects.GeoCoder;

namespace Rock.Address.Geocode
{
    /// <summary>
    /// Geocoder service from <a href="http://www.serviceobjects.com">ServiceObjects</a>
    /// </summary>
    [Description("Service Objects Geocoding service")]
    [Export( typeof( GeocodeComponent ) )]
    [ExportMetadata( "ComponentName", "ServiceObjects" )]
    [Rock.Attribute.Property( 2, "License Key", "Security", "The Service Objects License Key", true, "" )]
    public class ServiceObjects : GeocodeComponent
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="result">The ServiceObjects result.</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public override bool Geocode( Rock.Crm.Location location, out string result )
        {
            if ( location != null )
            {
                string licenseKey = AttributeValue("LicenseKey");

                var client = new DOTSGeoCoderSoapClient();
                Location_V3 location_match = client.GetBestMatch_V3(
                    string.Format("{0} {1}",
                        location.Street1,
                        location.Street2),
                    location.City,
                    location.State,
                    location.Zip,
                    licenseKey );

                result = location_match.Level;

                if ( location_match.Level == "S" || location_match.Level == "P" )
                {
                    location.Latitude = double.Parse( location_match.Latitude );
                    location.Longitude = double.Parse( location_match.Longitude );

                    return true;
                }
            }
            else
                result = "Null Address";

            return false;
        }
    }
}