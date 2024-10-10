using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    /// <summary>
    /// The Check-in Schedule Builder Data Bag
    /// </summary>
    public class CheckInScheduleBuilderDataBag
    {
        /// <summary>
        /// The list of Schedules to display on the Grid
        /// </summary>
        public List<ListItemBag> Schedules { get; set; }

        /// <summary>
        /// The List of Group Locations to display on the Grid
        /// </summary>
        public List<GroupLocationsBag> GroupLocations { get; set; }
    }
}
