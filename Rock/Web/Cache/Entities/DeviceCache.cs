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
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Device that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class DeviceCache : ModelCache<DeviceCache, Device>
    {
        #region Properties

        /// <summary>
        /// Gets the device name. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="string" /> representing the Name of the device.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a description of the device.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the description of the device.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets  the Id of the DeviceType <see cref="Rock.Model.DefinedValue"/> that identifies
        /// what type of device this is.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the Device Type <see cref="Rock.Model.DefinedValue"/>
        /// </value>
        [DataMember]
        public int DeviceTypeValueId { get; private set; }

        /// <summary>
        /// Gets  the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </value>
        [DataMember]
        public int? LocationId { get; private set; }

        /// <summary>
        /// Gets the IP address of the device.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the IP address of the device.
        /// </value>
        [DataMember]
        public string IPAddress { get; private set; }

        /// <summary>
        /// Gets the DeviceId of the printer that is associated with this device. This is mostly used if this device is a kiosk.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the DeviceId of the printer that is associated with this device. If there is not a printer 
        /// associated with this Device, this value will be null.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; private set; }

        /// <summary>
        /// Gets  where print jobs for this device originates from.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintFrom"/> to indicate how print jobs should be handled from this device. If <c>PrintFrom.Client</c> the print job will
        /// be handled from the client, otherwise <c>PrintFrom.Server</c> and the print job will be handled from the server.
        /// </value>
        [DataMember]
        public PrintFrom PrintFrom { get; private set; }

        /// <summary>
        /// Gets a flag that overrides which printer the print job is set to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintTo"/> that indicates overrides where the print job is set to.  If <c>PrintTo.Default</c> the print job will be sent to the default
        /// printer, if <c>PrintTo.Kiosk</c> the print job will be sent to the printer associated with the kiosk, if <c>PrintTo.Location</c> the print job will be sent to the 
        /// printer at the check in location.
        /// </value>
        [DataMember]
        public PrintTo PrintToOverride { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has camera.
        /// Only applies when <see cref="DeviceTypeValueId" /> is Checkin-Kiosk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has camera; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HasCamera { get; private set; }

        /// <summary>
        /// Gets the camera barcode configuration.
        /// This is currently only used for reading barcodes on iPads.
        /// </summary>
        /// <value>
        /// The type of the camera barcode configuration.
        /// </value>
        [DataMember]
        public CameraBarcodeConfiguration? CameraBarcodeConfigurationType { get; private set; }

        /// <summary>
        /// Gets the type of checkin client this Check-in Kiosk could be using.
        /// Only applies when <see cref="DeviceTypeValueId" /> is Checkin-Kiosk.
        /// </summary>
        /// <value>The type of the kiosk.</value>
        [DataMember]
        public KioskType? KioskType { get; private set; }

        /// <summary>
        /// Gets the set of location identifiers that this device uses.
        /// </summary>
        /// <value>The location identifiers.</value>
        [DataMember]
        public List<int> LocationIds { get; private set; }

        /// <summary>
        /// Gets the set of cached locations that this device uses.
        /// </summary>
        /// <value>The cached locations.</value>
        public List<NamedLocationCache> Locations => LocationIds.Select( l => NamedLocationCache.Get( l ) ).Where( l => l != null ).ToList();

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is Device device ) )
            {
                return;
            }

            Name = device.Name;
            Description = device.Description;
            DeviceTypeValueId = device.DeviceTypeValueId;
            LocationId = device.LocationId;
            IPAddress = device.IPAddress;
            PrinterDeviceId = device.PrinterDeviceId;
            PrintFrom = device.PrintFrom;
            PrintToOverride = device.PrintToOverride;
            IsActive = device.IsActive;
            HasCamera = device.HasCamera;
            CameraBarcodeConfigurationType = device.CameraBarcodeConfigurationType;
            KioskType = device.KioskType;
            LocationIds = device.Locations.Select( l => l.Id ).ToList();
        }

        /// <summary>
        /// Gets the Campus identifier this device is associated with. This
        /// is determined by checking if any ancestor of this kiosk is
        /// referenced by a Campus.
        /// </summary>
        /// <returns>The campus identifier or <c>null</c>.</returns>
        public int? GetCampusId()
        {
            // Loop through all the locations for this device and see if any
            // have a campus id.
            foreach ( var location in Locations )
            {
                var campusId = location.GetCampusIdForLocation();

                if ( campusId.HasValue )
                {
                    return campusId.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all location identifiers attached to this device, including
        /// all descendant locations from the selected locations.
        /// </summary>
        /// <returns>An enumeration of location identifiers.</returns>
        public IEnumerable<int> GetAllLocationIds()
        {
            var locationIds = new HashSet<int>();

            foreach ( var location in Locations )
            {
                if ( locationIds.Add( location.Id ) )
                {
                    location.GetAllDescendantLocationIds( locationIds );
                }
            }

            return locationIds;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets the device by IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address to look for.</param>
        /// <param name="deviceTypeValueId">The device type <see cref="DefinedValue"/> identifier of the device that you are searching for.</param>
        /// <returns>
        /// The <see cref="DeviceCache"/> that is associated with the provided IP address or <c>null</c> if no match.
        /// </returns>
        internal static DeviceCache GetByIPAddress( string ipAddress, int deviceTypeValueId )
        {
            return All()
                .Where( d => d.DeviceTypeValueId == deviceTypeValueId
                    && d.IPAddress?.Equals( ipAddress, StringComparison.OrdinalIgnoreCase ) == true )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the device by IP address. If a matching IP address is not found
        /// then also perform a DNS lookup to determine the hostname and try
        /// to find a match from that.
        /// </summary>
        /// <param name="ipAddress">The IP address to look for.</param>
        /// <param name="deviceTypeValueId">The device type <see cref="DefinedValue"/> identifier of the device that you are searching for.</param>
        /// <returns>
        /// The <see cref="DeviceCache"/> that is associated with the provided IP address or <c>null</c> if no match.
        /// </returns>
        internal static async Task<DeviceCache> GetByIPAddressOrNameAsync( string ipAddress, int deviceTypeValueId )
        {
            var device = GetByIPAddress( ipAddress, deviceTypeValueId );

            if ( device != null )
            {
                return device;
            }

            // Lookup kiosk by the DNS name of the IP address.
            try
            {
                string hostName = ( await System.Net.Dns.GetHostEntryAsync( ipAddress ) ).HostName;

                if ( hostName.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                // Find a matching device by name in either the IP Address/Hostname
                // field or the device's name. Priority is given to matches on the
                // IP Address field.
                return All()
                    .Where( d => d.DeviceTypeValueId == deviceTypeValueId
                        && ( d.IPAddress?.Equals( hostName, StringComparison.OrdinalIgnoreCase ) == true
                            || d.Name?.Equals( hostName, StringComparison.OrdinalIgnoreCase ) == true ) )
                    .OrderByDescending( d => d.IPAddress.Equals( hostName, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
            }
            catch ( SocketException )
            {
                return null;
            }
        }

        #endregion
    }
}
