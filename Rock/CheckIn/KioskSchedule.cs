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
    /// 
    /// </summary>
    public class KioskSchedule : ScheduleDto
    {
        /// <summary>
        /// The number of people who have already arrived, and not yet departed 
        /// with an attendance record associated with the same group, location, 
        /// and schedule
        /// </summary>
        /// <value>
        /// The current attendance.
        /// </value>
        public int currentAttendance { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskSchedule" /> class.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        public KioskSchedule( Schedule schedule )
            : base( schedule )
        {
        }
    }
}