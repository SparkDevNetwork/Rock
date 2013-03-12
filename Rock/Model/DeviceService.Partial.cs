//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
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
        
    }
}
