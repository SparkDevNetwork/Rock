using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    public class CheckInScheduleBuilderDataBag
    {
        public List<ListItemBag> Schedules { get; set; }

        public List<GroupLocationsBag> GroupLocations { get; set; }
    }
}
