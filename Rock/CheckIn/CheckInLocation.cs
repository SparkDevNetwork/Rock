//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A location option for the current check-in
    /// </summary>
    public class CheckInLocation : LocationDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInLocation" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into any of the groups for this location and group type
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        public DateTime LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the groups that are available for the current location
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public Dictionary<int, CheckInGroup> Groups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInLocation" /> class.
        /// </summary>
        public CheckInLocation()
            : base()
        {
            Groups = new Dictionary<int, CheckInGroup>();
        }
    }
}