using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
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

        public string WarningMessage { get; set; }
    }
}
