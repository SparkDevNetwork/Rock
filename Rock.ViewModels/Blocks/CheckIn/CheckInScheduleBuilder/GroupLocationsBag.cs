using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    /// <summary>
    /// The Group Locations Bag
    /// </summary>
    public class GroupLocationsBag
    {
        /// <summary>
        /// The encrypted identifier of the group location to be modified.
        /// </summary>
        public string GroupLocationId { get; set; }

        /// <summary>
        /// The path to the group that should be scheduled. This includes
        /// any parent groups in the text.
        /// </summary>
        public string GroupPath { get; set; }

        /// <summary>
        /// The path to the area that contains the group.
        /// </summary>
        public string AreaPath { get; set; }

        /// <summary>
        /// The name of the location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// The path to the location which includes all ancestor locations.
        /// </summary>
        public string LocationPath { get; set; }

        /// <summary>
        /// The encrypted schedule identifiers of all schedules that are currently
        /// active for this group location.
        /// </summary>
        public List<string> ScheduleIds { get; set; }
    }
}
