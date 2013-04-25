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
        /// Get the device by IPAddress
        /// </summary>
        /// <param name="ipAddress">an IPAddress (string)</param>
        /// <returns>a device or null if nothing was found</returns>
        public Device GetByIpAddress( string ipAddress )
        {
            return Repository.FirstOrDefault( d => d.IPAddress == ipAddress );
        }

        /// <summary>
        /// Get the device by name (ie, kiosk1.domain.com).
        /// </summary>
        /// <param name="name">the fully qualified name of the kiosk (string)</param>
        /// <returns>a device or null if nothing was found</returns>
        public Device GetByName( string name )
        {
            return Repository.FirstOrDefault( d => d.Name == name );
        }

        /// <summary>
        /// Finds a matching kiosk that's within the distanceThreshold for the given lat/long coordinates. 
        /// </summary>
        /// <param name="latitude">Latitude of the mobile phone/kiosk.</param>
        /// <param name="longitude">Longitude of the mobile phone/kiosk.</param>
        /// <param name="distanceThreshold">distance in miles (e.g. 1.0 or .25) that a matching kiosk must be in order to be considered a match.</param>
        /// <returns></returns>
        public Device GetByGeocode( double latitude, double longitude, double distanceThreshold )
        {
            Device kiosk = null;
            DbGeography aLocation = DbGeography.FromText( string.Format("POINT({0} {1})", longitude, latitude) );

            kiosk = Repository.AsQueryable().Where( d => d.GeoPoint.Distance( aLocation ) <= distanceThreshold ).FirstOrDefault();

            return kiosk;
        }
    }
}
