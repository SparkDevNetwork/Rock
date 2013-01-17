//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskLocation 
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public Location Location { get; set; }

        /// <summary>
        /// All groups with active schedules
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroup> KioskGroups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        public KioskLocation()
            : base()
        {
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public KioskLocation( Location location )
            : base()
        {
            Location = location.Clone( false );
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Location.ToString();
        }
    }
}