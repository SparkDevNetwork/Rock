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
using System.Linq;
using Rock.Attribute;
using Rock.Extension;
using Rock.IpAddress.Classes;

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
    [RockInternal( "1.14" )]
    public abstract class IpAddressLookupComponent : Component
    {
        /// <summary>
        /// Takes a list of IP Addresses and returns the location information associated with them.
        /// </summary>
        public virtual List<IpLocation> BulkLookup( List<string> ipAddresses, out string resultMsg )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lookups the specified IP Address and returns it's location.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual IpLocation Lookup( string ipAddress, out string resultMsg )
        {
            var location = BulkLookup( new List<string> { ipAddress }, out resultMsg )
                .FirstOrDefault();
            return location;
        }

        /// <summary>
        /// Gets a flag indicating the current availability status of the service,
        /// and an optional status message with additional information.
        /// </summary>
        /// <param name="statusMessage"></param>
        /// <returns>true if the service is available, otherwise false.</returns>
        public virtual bool VerifyCanProcess( out string statusMessage )
        {
            statusMessage = null;
            return true;
        }
    }
}