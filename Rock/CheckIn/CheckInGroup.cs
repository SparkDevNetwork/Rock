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
    /// A group option for the current check-in
    /// </summary>
    public class CheckInGroup : GroupDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInGroup" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this group for any of the schedules
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        public DateTime LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the schedules that are available for the current group
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        public Dictionary<int, CheckInSchedule> Schedules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroup" /> class.
        /// </summary>
        public CheckInGroup()
            : base()
        {
            Schedules = new Dictionary<int, CheckInSchedule>();
        }
    }
}