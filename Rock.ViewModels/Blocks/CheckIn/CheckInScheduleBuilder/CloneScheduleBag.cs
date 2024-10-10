using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    /// <summary>
    /// The Clone Schedule Bag
    /// </summary>
    public class CloneScheduleBag
    {
        /// <summary>
        /// The source schedule for the clone operation
        /// </summary>
        public ListItemBag SourceSchedule { get; set; }

        /// <summary>
        /// The destination schedule for the clone operation
        /// </summary>
        public ListItemBag DestinationSchedule { get; set; }
    }
}
