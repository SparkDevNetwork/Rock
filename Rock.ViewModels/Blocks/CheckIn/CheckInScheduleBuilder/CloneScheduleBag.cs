using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    public class CloneScheduleBag
    {
        public ListItemBag SourceSchedule { get; set; }

        public ListItemBag DestinationSchedule { get; set; }
    }
}
