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
    /// A group type option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInGroupType 
    {
        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public GroupType GroupType { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInGroupType" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked in to any of the Locations for this group type
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        [DataMember]
        public DateTime LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the locations that are available for the current group type
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        [DataMember]
        public List<CheckInLocation> Locations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroupType" /> class.
        /// </summary>
        public CheckInGroupType()
            : base()
        {
            Locations = new List<CheckInLocation>();
        }
    }
}