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

using Rock.Attribute;

namespace Rock.Net.Geolocation
{
    /// <summary>
    /// A POCO to represent IP-based geolocation data.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.17.0" )]
    public class IpGeolocation
    {
        /// <summary>
        /// Gets or sets the IP address as a result of the geolocation lookup.
        /// <remarks>
        /// This will be the client's IP address if a valid address was detected,
        /// <see cref="IpGeolocationErrorCode.InvalidAddress"/> if an invalid address was detected
        /// or <see cref="IpGeolocationErrorCode.ReservedAddress"/> if a reserved address was detected.
        /// </remarks>
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the lookup date time.
        /// </summary>
        public DateTime? LookupDateTime { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the region name.
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// Gets or sets the region code.
        /// </summary>
        public string RegionCode { get; set; }

        /// <summary>
        /// Gets or sets the region value identifier.
        /// </summary>
        public int? RegionValueId { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the country value identifier.
        /// </summary>
        public int? CountryValueId { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// An enum to describe why the address is invalid.
    /// </summary>
    internal enum IpGeolocationErrorCode
    {
        /// <summary>
        /// No error exists.
        /// </summary>
        None = 0,

        /// <summary>
        /// Is an invalid IP address.
        /// </summary>
        InvalidAddress = 1,

        /// <summary>
        /// Is a reserved IP address (e.g. 127.0.0.1).
        /// </summary>
        ReservedAddress = 2
    }
}
