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
    /// Helper class for storing the current attendance for a given kiosk group
    /// </summary>
    public class KioskGroupAttendance
    {
        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        public List<KioskScheduleAttendance> Schedules { get; set; }

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
                if ( Schedules != null )
                {
                    return Schedules.SelectMany( s => s.PersonIds ).Distinct().ToList();
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