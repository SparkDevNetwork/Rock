using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    /// <summary>
    /// The Check-in Schedule Builder Options Bag
    /// </summary>
    public class CheckInScheduleBuilderOptionsBag
    {
        /// <summary>
        /// The list of GroupTypes that can be chosen from
        /// </summary>
        public List<Guid> GroupTypes { get; set; }

        /// <summary>
        /// The list of Areas that can be chosen from
        /// </summary>
        public List<Guid> Areas { get; set; }

        /// <summary>
        /// The Default Schedule Category
        /// </summary>
        public ListItemBag DefaultScheduleCategory { get; set; }
    }
}
