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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Model;
using Rock.Net;
using Rock.Net.Geolocation;
using Rock.Utility;

namespace Rock.Personalization
{
    /// <summary>
    /// Class IPAddressRequestFilter.
    /// Implements the <see cref="Rock.Personalization.GeolocationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.GeolocationRequestFilter" />
    public class GeolocationRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the location component.
        /// </summary>
        /// <value>The location component.</value>
        public LocationComponentEnum LocationComponent { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        #endregion Configuration

        /// <inheritdoc/>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var ipAddress = WebRequestHelper.GetClientIpAddress( new HttpRequestWrapper( httpRequest ) );
            if ( ipAddress.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var geolocation = IpGeoLookup.Instance.GetGeolocation( ipAddress );
            if ( geolocation == null )
            {
                return false;
            }

            return IsMatch( geolocation );
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            var geoLocation = request?.ClientInformation?.Geolocation;
            if ( geoLocation == null )
            {
                return false;
            }

            return IsMatch( geoLocation );
        }

        /// <summary>
        /// Determines whether the specified IPGeoLocation meets the criteria of this filter.
        /// </summary>
        /// <param name="geoLocation">The user agent object.</param>
        /// <returns><c>true</c> if the specified user agent is a match; otherwise, <c>false</c>.</returns>
        private bool IsMatch( Rock.Net.Geolocation.IpGeolocation geoLocation )
        {
            string compareValue;
            switch ( LocationComponent )
            {
                case LocationComponentEnum.RegionCode:
                    compareValue = geoLocation.RegionCode;
                    break;
                case LocationComponentEnum.RegionName:
                    compareValue = geoLocation.RegionName;
                    break;
                case LocationComponentEnum.City:
                    compareValue = geoLocation.City;
                    break;
                case LocationComponentEnum.PostalCode:
                    compareValue = geoLocation.PostalCode;
                    break;
                case LocationComponentEnum.CountryCode:
                default:
                    compareValue = geoLocation.CountryCode;
                    break;
            }

            return compareValue.CompareTo( Value, ComparisonType );
        }

        /// <summary>
        /// The location component options that the Configuration UI for this filter should show,
        /// </summary>
        public enum LocationComponentEnum
        {
            /// <summary>
            /// Country Code
            /// </summary>
            [Description( "Country Code" )]
            CountryCode = 0,

            /// <summary>
            /// Region Code
            /// </summary>
            [Description( "Region Code" )]
            RegionCode = 1,

            /// <summary>
            /// Region Name
            /// </summary>
            [Description( "Region Name" )]
            RegionName = 2,

            /// <summary>
            /// City
            /// </summary>
            [Description( "City" )]
            City = 3,

            /// <summary>
            /// Postal Code
            /// </summary>
            [Description( "Postal Code" )]
            PostalCode = 4
        }
    }
}
