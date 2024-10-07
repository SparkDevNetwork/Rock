using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    public class CheckInGroupBag
    {
        public int GroupLocationId { get; set; }

        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public string GroupPath { get; set; }

        public string LocationName { get; set; }

        public string LocationPath { get; set; }

        public List<ListItemBag> IsScheduleSelected { get; set; }
    }
}
