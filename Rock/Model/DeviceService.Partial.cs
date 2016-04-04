﻿// <copyright>
// Copyright by the Spark Development Network
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
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for the <see cref="Rock.Model.Device"/> entity objects
    /// </summary>
    public partial class DeviceService 
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Device">Devices</see> by the Guid of their Device Type <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the unique identifier of the <see cref="Rock.Model.Device">Device's</see> Device Type <see cref="Rock.Model.DefinedValue"/></param>
        /// <returns>A queryable co</returns>
        public IQueryable<Device> GetByDeviceTypeGuid( Guid guid )
        {
            return Queryable().Where( d => d.DeviceType.Guid == guid );
        }

        /// <summary>
        /// Finds the matching device for the given lat/long coordinates. The given coordinates
        /// must intersect one of the stored GeoFence values to be a match.  Use the deviceTypeValueId
        /// to constrain matching to only certain device types.
        /// </summary>
        /// <param name="latitude">Latitude of the mobile phone/kiosk.</param>
        /// <param name="longitude">Longitude of the mobile phone/kiosk.</param>
        /// <param name="deviceTypeValueId">Longitude of the mobile phone/kiosk.</param>
        /// <returns>a single matching Device kiosk or null if nothing was matched</returns>
        public Device GetByGeocode( double latitude, double longitude, int deviceTypeValueId )
        {
            Device device = null;
            DbGeography aLocation = DbGeography.FromText( string.Format("POINT({0} {1})", longitude, latitude) );

            device = Queryable()
                .Where( d => 
                    d.DeviceTypeValueId == deviceTypeValueId &&
                    d.Location != null &&
                    aLocation.Intersects( d.Location.GeoFence ) )
                .FirstOrDefault();

            return device;
        }

        /// <summary>
        /// Gets the device by IP address.
        /// </summary>
        /// <param name="ipAddress">A <see cref="System.String" /> representing the ip address.</param>
        /// <param name="deviceTypeValueId">A <see cref="System.Int32"/> representing the DeviceType <see cref="Rock.Model.DefinedValue"/> of the device that you are searching for.</param>
        /// <param name="skipReverseLookup">A <see cref="System.Boolean"/> indicating if a reverse lookup will be skipped. If <c>true</c> a DNS reverse lookup for the name of the system
        /// that belongs to the provided IP address will not be performed, otherwise <c>false</c> and a DNS reverse lookup will be performed.</param>
        /// <returns>
        /// A <see cref="Rock.Model.Device"/> that is associated with the provided IP address.
        /// </returns>
        public Device GetByIPAddress( string ipAddress, int deviceTypeValueId, bool skipReverseLookup = true )
        {
            string hostValue = ipAddress;

            if ( !skipReverseLookup )
            {
                // Lookup the system's "name" (as seen in the DNS) using the given IP
                // address because when using DHCP the kiosk may have a different IP from time to time
                // -- however the fully qualified name should always be the same.
                try
                {
                    hostValue = System.Net.Dns.GetHostEntry( ipAddress ).HostName;
                }
                catch ( SocketException )
                {
                    // TODO: consider whether we want to log the IP address that caused this error.
                    // As per http://msdn.microsoft.com/en-us/library/ms143998.aspx it *may* mean 
                    // a stale DNS record for an IPv4 address that actually belongs to a
                    // different host was going to be returned (there is a DNS PTR record for
                    // the IPv4 address, but no DNS A record for the IPv4 address).
                    hostValue = ipAddress;
                }
            }

            Device device = null;

            // If we still have an IPv4 address then try to find it based on IP
            if ( Regex.IsMatch( hostValue, @"\d+\.\d+\.\d+\.\d+" ) )
            {
                // find by IP
                device = Queryable()
                    .Where( d =>
                        d.DeviceTypeValueId == deviceTypeValueId &&
                        d.IPAddress == hostValue)
                    .FirstOrDefault();
            }
            else
            {
                // find by name
                device = Queryable()
                    .Where( d =>
                        d.DeviceTypeValueId == deviceTypeValueId &&
                        d.Name == hostValue )
                    .FirstOrDefault();
            }

            return device;
        }

    }
}
