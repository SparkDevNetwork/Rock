// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock.Attribute;
using Rock.ServiceObjects.GeoCoder;

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
        /// Gets a value indicating whether Melissa Data supports geocoding.
        /// </summary>
        public override bool SupportsStandardization
        {
            get { return false; }
        }

        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns>
        /// True/False value of whether the address was geocoded successfully
        /// </returns>
        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            VerificationResult result = VerificationResult.None;
            resultMsg = string.Empty;

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

            resultMsg = location_match.Level;

            if ( location_match.Level == "S" || location_match.Level == "P" )
            {
                double latitude = double.Parse( location_match.Latitude );
                double longitude = double.Parse( location_match.Longitude );
                if ( location.SetLocationPointFromLatLong( latitude, longitude ) )
                {
                    result = VerificationResult.Geocoded;
                }
            }

            return result;
        }
    }
}