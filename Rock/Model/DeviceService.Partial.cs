//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Device POCO Service class
    /// </summary>
    public partial class DeviceService 
    {
        /// <summary>
        /// Gets the by device type GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public IQueryable<Device> GetByDeviceTypeGuid( Guid guid )
        {
            return Repository.AsQueryable().Where( d => d.DeviceType.Guid == guid );
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
            Device kiosk = null;
            DbGeography aLocation = DbGeography.FromText( string.Format("POINT({0} {1})", longitude, latitude) );

            kiosk = Repository.AsQueryable()
                .Where( d => 
                    d.DeviceTypeValueId == deviceTypeValueId &&
                    d.Location != null &&
                    aLocation.Intersects( d.Location.GeoFence ) ).FirstOrDefault();

            return kiosk;
        }

        /// <summary>
        /// Gets the device by name.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        /// <returns></returns>
        public Device GetByDeviceName( string deviceName )
        {
            return Repository.AsQueryable().Where( d => d.Name == deviceName ).FirstOrDefault();
        }
    }
}
