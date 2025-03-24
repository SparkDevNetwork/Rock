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

namespace Rock.IpAddress.Classes
{
    /// <summary>
    /// POCO for storing the information we need to write an Interaction Session Location
    /// </summary>
    [RockObsolete( "1.17" )]
    [Obsolete( "Use IpGeolocation instead." )]
    public class IpLocation
    {
        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets the ip location error code.
        /// </summary>
        /// <value>
        /// The ip location error code.
        /// </value>
        public IpLocationErrorCode IpLocationErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the isp.
        /// </summary>
        /// <value>
        /// The isp.
        /// </value>
        public string ISP { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the country value identifier.
        /// </summary>
        /// <value>
        /// The country value identifier.
        /// </value>
        public int? CountryValueId { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the region value identifier.
        /// </summary>
        /// <value>
        /// The region value identifier.
        /// </value>
        public int? RegionValueId { get; set; }

        /// <summary>
        /// Gets or sets the region code.
        /// </summary>
        /// <value>
        /// The region code.
        /// </value>
        public string RegionCode { get; set; }
    }

    /// <summary>
    /// Enum for describing why the address is invalid
    /// </summary>
    [RockObsolete( "1.17" )]
    [Obsolete( "Use IpGeolocationErrorCode instead." )]
    public enum IpLocationErrorCode
    {
        /// <summary>
        /// No error exists
        /// </summary>
        None,
        /// <summary>
        /// The invalid address
        /// </summary>
        InvalidAddress,
        /// <summary>
        /// The IP address represents an internal address (e.g. 127.0.0.1)
        /// </summary>
        ReservedAddress
    }
}
