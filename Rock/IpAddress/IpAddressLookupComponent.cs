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
using System.Data.Entity.Spatial;
using Rock.Attribute;
using Rock.Extension;

namespace Rock.IpAddress
{
    /// <summary>
    /// Internal IP Address Lookup Component
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public abstract class IpAddressLookupComponent : Component
    {
        /// <summary>
        /// Gets all the IP Address result
        /// </summary>
        public virtual List<LookupResult> Lookup( List<string> ipAddresses, out string resultMsg )
        {
            throw new NotImplementedException();
        }

    }

    #region Nested Classes

    /// <summary>
    /// The Lookup result
    /// </summary>
    public class LookupResult
    {
        /// <summary>
        /// Gets or sets the IP address of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the IP address of the request.
        /// </value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the ISP.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the ISP.
        /// </value>
        public string ISP { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the region code.
        /// </summary>
        /// <value>
        /// The region code.
        /// </value>
        public string RegionCode { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (GeoLocation) for the IP
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the GeoLocation of the IP.
        /// </value>
        public DbGeography GeoPoint { get; set; }
    }

    #endregion
}