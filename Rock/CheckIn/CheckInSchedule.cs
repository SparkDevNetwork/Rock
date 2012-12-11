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
    /// A shedule options for the current check-in
    /// </summary>
    public class CheckInSchedule : ScheduleDto
    {
        /// <summary>
        /// Gets or sets the the unique code for check-in labels
        /// </summary>
        /// <value>
        /// The security code.
        /// </value>
        string SecurityCode { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this schedule for the selected group type, location and group 
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        public DateTime LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInSchedule" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected { get; set; }
    }
}