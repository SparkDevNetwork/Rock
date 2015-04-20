// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel;
using System.ComponentModel.Composition;
//using System.Data.Spatial;

using Rock;
using Rock.Attribute;
using Rock.ServiceObjects.GeoCoder;
using Rock.Web.UI;

namespace Rock.Address
{
    /// <summary>
    /// Geocoder service from <a href="http://www.serviceobjects.com">ServiceObjects</a>
    /// </summary>
    [Description("Service Objects Geocoding service")]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "ServiceObjects" )]
    [TextField( "License Key", "The Service Objects License Key" )]
    public class ServiceObjects : VerificationComponent
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="reVerify">Should location be reverified even if it has already been succesfully verified</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the address was geocoded succesfully
        /// </returns>
        public override bool VerifyLocation( Rock.Model.Location location, bool reVerify, out string result )
        {
            bool verified = false;
            result = string.Empty;

            // Only verify if location is valid, has not been locked, and 
            // has either never been attempted or last attempt was in last 30 secs (prev active service failed) or reverifying
            if ( location != null && 
                !(location.IsGeoPointLocked ?? false) &&  
                (
                    !location.GeocodeAttemptedDateTime.HasValue || 
                    location.GeocodeAttemptedDateTime.Value.CompareTo( RockDateTime.Now.AddSeconds(-30) ) > 0 ||
                    reVerify
                ) )
            {
                string licenseKey = GetAttributeValue("LicenseKey");

                var client = new DOTSGeoCoderSoapClient();
                Location_V3 location_match = client.GetBestMatch_V3(
                    string.Format("{0} {1}",
                        location.Street1,
                        location.Street2),
                    location.City,
                    location.State,
                    location.PostalCode,
                    licenseKey );

                result = location_match.Level;

                location.GeocodeAttemptedServiceType = "ServiceObjects";
                location.GeocodeAttemptedDateTime = RockDateTime.Now;
                location.GeocodeAttemptedResult = result;

                if ( location_match.Level == "S" || location_match.Level == "P" )
                {
                    double latitude = double.Parse( location_match.Latitude );
                    double longitude = double.Parse( location_match.Longitude );
                    location.SetLocationPointFromLatLong(latitude, longitude);
                    location.GeocodedDateTime = RockDateTime.Now;
                    verified = true;
                }

            }

            return verified;
        }
    }
}