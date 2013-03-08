//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for storing the current attendance for a given kiosk schedule
    /// </summary>
    public class KioskScheduleAttendance
    {
        /// <summary>
        /// Gets or sets the schedule id.
        /// </summary>
        /// <value>
        /// The schedule id.
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        /// <value>
        /// The name of the schedule.
        /// </value>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the person ids.
        /// </summary>
        /// <value>
        /// The person ids.
        /// </value>
        public List<int> PersonIds { get; set; }

        /// <summary>
        /// Gets the distinct person ids.
        /// </summary>
        /// <value>
        /// The distinct person ids.
        /// </value>
        public List<int> DistinctPersonIds 
        {
            get
            {
                if ( PersonIds != null )
                {
                    return PersonIds.Distinct().ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public int CurrentCount
        {
            get
            {
                var people = DistinctPersonIds;
                return people != null ? people.Count() : 0;
            }
        }
    }
}