//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of a check-in kiosk.  
    /// </summary>
    [DataContract]
    public class KioskStatus
    {
        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        [DataMember]
        public Device Device { get; set; }

        /// <summary>
        /// The group types associated with this kiosk
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        [DataMember]
        public List<KioskGroupType> KioskGroupTypes { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocations
        {
            get
            {
                return KioskGroupTypes.Any( g => g.KioskLocations.Any() );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveLocations
        {
            get
            {
                return KioskGroupTypes.Any( g => g.KioskLocations.Any( l => l.Location.IsActive ) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskStatus" /> class.
        /// </summary>
	    public KioskStatus() : base()
	    {
            KioskGroupTypes = new List<KioskGroupType>();
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskStatus" /> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public KioskStatus( Device device )
            : base()
        {
            Device = device.Clone( false );
            KioskGroupTypes = new List<KioskGroupType>();
        }
    }
}

