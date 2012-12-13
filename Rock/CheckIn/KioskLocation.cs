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
    /// 
    /// </summary>
    [DataContract]
    public class KioskLocation : LocationDto
    {
        /// <summary>
        /// All groups with active schedules
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroup> Groups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        public KioskLocation()
            : base()
        {
            Groups = new List<KioskGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public KioskLocation( Location location )
            : base( location )
        {
            Groups = new List<KioskGroup>();
        }
    }
}