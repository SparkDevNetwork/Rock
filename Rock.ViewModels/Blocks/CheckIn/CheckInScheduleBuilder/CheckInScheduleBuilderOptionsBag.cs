using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    public class CheckInScheduleBuilderOptionsBag
    {
        public Dictionary<int, string> Schedules { get; set; }

        public string WarningMessage { get; set; }
    }
}
