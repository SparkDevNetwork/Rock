//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A shedule options for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInSchedule 
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this schedule for the selected group type, location and group 
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInSchedule" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Schedule != null ? Schedule.ToString() : string.Empty;
        }
    }
}